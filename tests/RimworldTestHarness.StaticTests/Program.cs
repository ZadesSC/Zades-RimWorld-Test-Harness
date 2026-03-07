using RimworldTestHarness.Core.Runtime;
using RimworldTestHarness.Core.StaticTests;

if (args.Length != 1)
{
    Console.Error.WriteLine("Usage: RimworldTestHarness.StaticTests <manifest.json>");
    return 1;
}

var workspaceRoot = ResolveWorkspaceRoot();
var manifest = ManifestLoader.LoadAndNormalize(Path.GetFullPath(args[0], Environment.CurrentDirectory), workspaceRoot);
var result = new StaticSuiteRunner().Run(manifest);

foreach (var item in result.Results)
{
    Console.WriteLine($"{(item.Passed ? "PASS" : "FAIL")} {item.Name}: {item.Details}");
}

return result.Succeeded ? 0 : 1;

static string ResolveWorkspaceRoot()
{
    var markers = new[]
    {
        "Zades.RimWorldTestHarness.sln",
        "RimworldTestHarness.sln",
    };

    foreach (var start in new[] { Environment.CurrentDirectory, AppContext.BaseDirectory })
    {
        var directory = new DirectoryInfo(Path.GetFullPath(start));
        while (directory is not null)
        {
            if (markers.Any(marker => File.Exists(Path.Combine(directory.FullName, marker))))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }
    }

    throw new InvalidOperationException("Could not locate workspace root.");
}
