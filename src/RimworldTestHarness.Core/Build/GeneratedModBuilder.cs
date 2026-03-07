using System.Diagnostics;
using System.Security;
using System.Text;
using System.Xml.Linq;
using RimworldTestHarness.Core.Models;

namespace RimworldTestHarness.Core.Build;

public sealed class GeneratedModBuilder
{
    public GeneratedModBuildResult BuildTargetMod(HarnessManifest manifest, string workspaceRoot)
    {
        var descriptor = AnalyzeProject(manifest.ModUnderTestDirectory);
        var projectRoot = Path.Combine(workspaceRoot, "artifacts", "build", descriptor.AssemblyName);
        Directory.CreateDirectory(projectRoot);

        var projectPath = Path.Combine(projectRoot, $"{descriptor.AssemblyName}.generated.csproj");
        var outputDirectory = Path.Combine(projectRoot, "out");
        Directory.CreateDirectory(outputDirectory);

        var projectXml = BuildGeneratedProject(descriptor, manifest, outputDirectory);
        File.WriteAllText(projectPath, projectXml, new UTF8Encoding(false));

        RunBuild(projectPath, manifest.BuildConfiguration);

        return new GeneratedModBuildResult
        {
            AssemblyName = descriptor.AssemblyName,
            ProjectPath = projectPath,
            OutputAssemblyPath = Path.Combine(outputDirectory, $"{descriptor.AssemblyName}.dll"),
            OutputPdbPath = Path.Combine(outputDirectory, $"{descriptor.AssemblyName}.pdb"),
        };
    }

    private static void RunBuild(string projectPath, string configuration)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = "dotnet",
            Arguments = $"build \"{projectPath}\" -c {configuration}",
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            WorkingDirectory = Path.GetDirectoryName(projectPath)!,
        };

        using var process = Process.Start(startInfo) ?? throw new InvalidOperationException("Failed to start dotnet build.");
        var stdout = process.StandardOutput.ReadToEnd();
        var stderr = process.StandardError.ReadToEnd();
        process.WaitForExit();

        if (process.ExitCode != 0)
        {
            throw new InvalidOperationException($"Generated mod build failed.{Environment.NewLine}{stdout}{Environment.NewLine}{stderr}");
        }
    }

    private static string BuildGeneratedProject(ModProjectDescriptor descriptor, HarnessManifest manifest, string outputDirectory)
    {
        var references = new List<(string Name, string HintPath)>
        {
            ("Assembly-CSharp", Path.Combine(manifest.GameDirectory, "RimWorldWin64_Data", "Managed", "Assembly-CSharp.dll")),
            ("UnityEngine", Path.Combine(manifest.GameDirectory, "RimWorldWin64_Data", "Managed", "UnityEngine.dll")),
            ("UnityEngine.CoreModule", Path.Combine(manifest.GameDirectory, "RimWorldWin64_Data", "Managed", "UnityEngine.CoreModule.dll")),
            ("UnityEngine.IMGUIModule", Path.Combine(manifest.GameDirectory, "RimWorldWin64_Data", "Managed", "UnityEngine.IMGUIModule.dll")),
            ("UnityEngine.TextRenderingModule", Path.Combine(manifest.GameDirectory, "RimWorldWin64_Data", "Managed", "UnityEngine.TextRenderingModule.dll")),
        };

        if (!string.IsNullOrWhiteSpace(manifest.HarmonyPath) && File.Exists(manifest.HarmonyPath))
        {
            references.Add(("0Harmony", manifest.HarmonyPath));
        }

        var builder = new StringBuilder();
        builder.AppendLine("<Project Sdk=\"Microsoft.NET.Sdk\">");
        builder.AppendLine("  <PropertyGroup>");
        builder.AppendLine("    <TargetFramework>net48</TargetFramework>");
        builder.AppendLine($"    <AssemblyName>{descriptor.AssemblyName}</AssemblyName>");
        builder.AppendLine("    <Nullable>disable</Nullable>");
        builder.AppendLine("    <ImplicitUsings>disable</ImplicitUsings>");
        builder.AppendLine("    <LangVersion>latest</LangVersion>");
        builder.AppendLine("    <GenerateAssemblyInfo>false</GenerateAssemblyInfo>");
        builder.AppendLine("    <AppendTargetFrameworkToOutputPath>false</AppendTargetFrameworkToOutputPath>");
        builder.AppendLine($"    <OutputPath>{Escape(outputDirectory)}</OutputPath>");
        builder.AppendLine("    <DebugType>portable</DebugType>");
        builder.AppendLine("  </PropertyGroup>");
        builder.AppendLine("  <ItemGroup>");

        foreach (var sourceFile in descriptor.SourceFiles)
        {
            builder.AppendLine($"    <Compile Include=\"{Escape(sourceFile)}\" />");
        }

        builder.AppendLine("  </ItemGroup>");
        builder.AppendLine("  <ItemGroup>");

        foreach (var reference in references)
        {
            builder.AppendLine($"    <Reference Include=\"{reference.Name}\">");
            builder.AppendLine($"      <HintPath>{Escape(reference.HintPath)}</HintPath>");
            builder.AppendLine("      <Private>false</Private>");
            builder.AppendLine("    </Reference>");
        }

        builder.AppendLine("  </ItemGroup>");
        builder.AppendLine("</Project>");
        return builder.ToString();
    }

    private static string Escape(string path) => SecurityElement.Escape(path) ?? path;

    private static ModProjectDescriptor AnalyzeProject(string modDirectory)
    {
        var sourceRoot = Path.Combine(modDirectory, "Source");
        var csproj = Directory.EnumerateFiles(sourceRoot, "*.csproj", SearchOption.AllDirectories).FirstOrDefault();

        if (csproj is null)
        {
            throw new InvalidOperationException($"No .csproj found under {sourceRoot}");
        }

        var document = XDocument.Load(csproj);
        var assemblyName = document.Descendants().FirstOrDefault(static element => element.Name.LocalName == "AssemblyName")?.Value?.Trim();
        if (string.IsNullOrWhiteSpace(assemblyName))
        {
            assemblyName = Path.GetFileNameWithoutExtension(csproj);
        }

        var projectDirectory = Path.GetDirectoryName(csproj)!;
        var compileIncludes = document.Descendants()
            .Where(static element => element.Name.LocalName == "Compile")
            .Select(static element => element.Attribute("Include")?.Value)
            .Where(static include => !string.IsNullOrWhiteSpace(include))
            .Select(include => Path.GetFullPath(Path.Combine(projectDirectory, include!)))
            .Where(File.Exists)
            .ToList();

        if (compileIncludes.Count == 0)
        {
            compileIncludes = Directory.EnumerateFiles(projectDirectory, "*.cs", SearchOption.AllDirectories)
                .Where(static file => !file.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase))
                .Where(static file => !file.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        return new ModProjectDescriptor(assemblyName!, compileIncludes);
    }

    private sealed record ModProjectDescriptor(string AssemblyName, List<string> SourceFiles);
}
