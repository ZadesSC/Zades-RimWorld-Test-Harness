using System.Diagnostics;
using RimworldTestHarness.Core.Runtime;

var workspaceRoot = ResolveWorkspaceRoot();
var argsList = args.ToList();

if (argsList.Count < 2)
{
    PrintUsage();
    return 1;
}

var command = argsList[0];
var manifestPath = Path.GetFullPath(argsList[1], Environment.CurrentDirectory);
var manifest = ManifestLoader.LoadAndNormalize(manifestPath, workspaceRoot);
var coordinator = new RunCoordinator();

try
{
    switch (command)
    {
        case "run-static":
        {
            var result = coordinator.RunStatic(manifest);
            PrintStatic(result);
            return result.Succeeded ? 0 : 1;
        }
        case "build-mod":
        {
            var result = coordinator.BuildTargetMod(manifest, workspaceRoot);
            Console.WriteLine($"Built {result.AssemblyName} -> {result.OutputAssemblyPath}");
            return 0;
        }
        case "prepare-profile":
        {
            var profile = new ProfileManager().Prepare(manifest);
            Console.WriteLine($"Prepared profile: {profile.ProfileDirectory}");
            Console.WriteLine($"Manifest copy: {profile.ManifestCopyPath}");
            return 0;
        }
        case "start-game":
        {
            var state = coordinator.StartGameDetached(manifest, workspaceRoot);
            Console.WriteLine($"Started RimWorld in background. PID: {state.ProcessId}");
            Console.WriteLine($"Run state: {Path.Combine(manifest.ResultsDirectory, "run-state.json")}");
            return 0;
        }
        case "monitor-game":
        {
            var state = coordinator.LoadRunState(manifest);
            var alive = Process.GetProcesses().Any(process => process.Id == state.ProcessId);
            Console.WriteLine($"PID: {state.ProcessId}");
            Console.WriteLine($"Started: {state.StartedAtUtc:u}");
            Console.WriteLine($"Alive: {alive}");
            Console.WriteLine($"Result path: {state.ResultPath}");
            if (File.Exists(state.ResultPath))
            {
                var result = new RimworldTestHarness.Core.Runtime.ArtifactCollector().LoadGameResult(manifest, new RimworldTestHarness.Core.Models.PreparedProfile
                {
                    ProfileDirectory = manifest.ProfileDirectory,
                    ManifestCopyPath = Path.Combine(manifest.ProfileDirectory, "Harness", "current-run.json"),
                    PlayerLogPath = state.PlayerLogPath,
                });
                PrintGame(result);
            }
            else if (File.Exists(state.PlayerLogPath))
            {
                Console.WriteLine("Last RTH log lines:");
                foreach (var line in File.ReadLines(state.PlayerLogPath).Where(static line => line.Contains("[RTH]", StringComparison.OrdinalIgnoreCase)).TakeLast(20))
                {
                    Console.WriteLine(line);
                }
            }

            return 0;
        }
        case "stop-game":
        {
            coordinator.StopGame(manifest);
            Console.WriteLine("Stopped background RimWorld process if it was still running.");
            return 0;
        }
        case "run-game":
        {
            var result = await coordinator.RunGameAsync(manifest, workspaceRoot, CancellationToken.None);
            PrintGame(result);
            return result.Succeeded ? 0 : 1;
        }
        case "run-all":
        {
            var (staticResult, gameResult) = await coordinator.RunAllAsync(manifest, workspaceRoot, CancellationToken.None);
            PrintStatic(staticResult);
            PrintGame(gameResult);
            return staticResult.Succeeded && gameResult.Succeeded ? 0 : 1;
        }
        case "watch":
        {
            using var cts = new CancellationTokenSource();
            Console.CancelKeyPress += (_, eventArgs) =>
            {
                eventArgs.Cancel = true;
                cts.Cancel();
            };

            Console.WriteLine("Watching for changes. Press Ctrl+C to stop.");
            await coordinator.WatchAsync(manifest, workspaceRoot, cts.Token);
            return 0;
        }
        default:
            PrintUsage();
            return 1;
    }
}
catch (Exception ex)
{
    Console.Error.WriteLine(ex);
    return 1;
}

static void PrintUsage()
{
    Console.WriteLine("Usage:");
    Console.WriteLine("  RimworldTestHarness.Cli run-static <manifest.json>");
    Console.WriteLine("  RimworldTestHarness.Cli build-mod <manifest.json>");
    Console.WriteLine("  RimworldTestHarness.Cli prepare-profile <manifest.json>");
    Console.WriteLine("  RimworldTestHarness.Cli start-game <manifest.json>");
    Console.WriteLine("  RimworldTestHarness.Cli monitor-game <manifest.json>");
    Console.WriteLine("  RimworldTestHarness.Cli stop-game <manifest.json>");
    Console.WriteLine("  RimworldTestHarness.Cli run-game <manifest.json>");
    Console.WriteLine("  RimworldTestHarness.Cli run-all <manifest.json>");
    Console.WriteLine("  RimworldTestHarness.Cli watch <manifest.json>");
}

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

static void PrintStatic(RimworldTestHarness.Core.Models.StaticTestRunResult result)
{
    Console.WriteLine($"Static suite: {result.Suite}");
    foreach (var item in result.Results)
    {
        Console.WriteLine($"{(item.Passed ? "PASS" : "FAIL")} {item.Name}: {item.Details}");
    }
}

static void PrintGame(RimworldTestHarness.Core.Models.GameTestRunResult result)
{
    Console.WriteLine($"Game suite: {result.Suite}");
    foreach (var item in result.Results)
    {
        Console.WriteLine($"{(item.Passed ? "PASS" : "FAIL")} {item.Name}: {item.Details}");
        if (!string.IsNullOrWhiteSpace(item.SnapshotPath))
        {
            Console.WriteLine($"  snapshot: {item.SnapshotPath}");
        }
    }

    if (!string.IsNullOrWhiteSpace(result.PlayerLogPath))
    {
        Console.WriteLine($"Player.log: {result.PlayerLogPath}");
    }
}
