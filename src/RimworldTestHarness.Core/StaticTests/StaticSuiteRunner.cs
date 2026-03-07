using RimworldTestHarness.Core.Build;
using RimworldTestHarness.Core.Models;
using RimworldTestHarness.Core.Serialization;

namespace RimworldTestHarness.Core.StaticTests;

public sealed class StaticSuiteRunner
{
    private readonly ModMetadataReader metadataReader = new();
    private readonly DefReader defReader = new();

    public StaticTestRunResult Run(HarnessManifest manifest)
    {
        var result = new StaticTestRunResult
        {
            Suite = string.Join(',', manifest.StaticSuites),
            CompletedAtUtc = DateTime.UtcNow,
        };

        AddManifestChecks(manifest, result.Results);
        AddMetadataChecks(manifest, result.Results);
        AddDefChecks(manifest, result.Results);

        Directory.CreateDirectory(manifest.ResultsDirectory);
        JsonFile.Save(Path.Combine(manifest.ResultsDirectory, "static-results.json"), result);
        return result;
    }

    private void AddManifestChecks(HarnessManifest manifest, List<StaticTestCaseResult> results)
    {
        results.Add(Check("manifest.game-directory", Directory.Exists(manifest.GameDirectory), manifest.GameDirectory));
        results.Add(Check("manifest.mod-under-test", Directory.Exists(manifest.ModUnderTestDirectory), manifest.ModUnderTestDirectory));
        results.Add(Check("manifest.support-mod", Directory.Exists(manifest.SupportModDirectory), manifest.SupportModDirectory));
        results.Add(Check("manifest.harmony", !string.IsNullOrWhiteSpace(manifest.HarmonyPath) && File.Exists(manifest.HarmonyPath), manifest.HarmonyPath ?? "<missing>"));
    }

    private void AddMetadataChecks(HarnessManifest manifest, List<StaticTestCaseResult> results)
    {
        var metadata = metadataReader.Read(manifest.ModUnderTestDirectory);
        results.Add(Check(
            "metadata.package-id",
            string.IsNullOrWhiteSpace(manifest.ExpectedPackageId)
                ? !string.IsNullOrWhiteSpace(metadata.PackageId)
                : string.Equals(metadata.PackageId, manifest.ExpectedPackageId, StringComparison.OrdinalIgnoreCase),
            metadata.PackageId));

        foreach (var version in manifest.RequiredSupportedVersions.DefaultIfEmpty("1.6"))
        {
            results.Add(Check(
                $"metadata.supports-{version}",
                metadata.SupportedVersions.Contains(version),
                string.Join(", ", metadata.SupportedVersions)));
        }

        foreach (var dependency in manifest.RequiredDependencies.DefaultIfEmpty("brrainz.harmony"))
        {
            results.Add(Check(
                $"metadata.depends-on-{dependency}",
                metadata.Dependencies.Contains(dependency),
                string.Join(", ", metadata.Dependencies)));
        }
    }

    private void AddDefChecks(HarnessManifest manifest, List<StaticTestCaseResult> results)
    {
        var defs = defReader.ReadThingDefs(manifest.ModUnderTestDirectory);
        results.Add(Check("defs.present", defs.Count > 0, $"{defs.Count} defs"));
        foreach (var defName in manifest.RequiredThingDefs)
        {
            results.Add(Check($"defs.{defName}", defs.Contains(defName), string.Join(", ", defs)));
        }

        var assemblyDirectory = Path.Combine(manifest.ModUnderTestDirectory, "Assemblies");
        var compiledAssembly = Directory.Exists(assemblyDirectory)
            ? Directory.EnumerateFiles(assemblyDirectory, "*.dll", SearchOption.TopDirectoryOnly).FirstOrDefault()
            : null;
        results.Add(Check("assemblies.exists", compiledAssembly is not null, compiledAssembly ?? "<missing>"));
    }

    private static StaticTestCaseResult Check(string name, bool passed, string details)
    {
        return new StaticTestCaseResult
        {
            Name = name,
            Passed = passed,
            Details = details,
        };
    }
}
