using System.Diagnostics;
using RimworldTestHarness.Core.Build;
using RimworldTestHarness.Core.Models;
using RimworldTestHarness.Core.Serialization;
using RimworldTestHarness.Core.StaticTests;

namespace RimworldTestHarness.Core.Runtime;

public sealed class RunCoordinator
{
    private readonly GeneratedModBuilder modBuilder = new();
    private readonly ProfileManager profileManager = new();
    private readonly ModSynchronizer modSynchronizer = new();
    private readonly ArtifactCollector artifactCollector = new();
    private readonly RimWorldLauncher launcher = new();
    private readonly StaticSuiteRunner staticSuiteRunner = new();

    public StaticTestRunResult RunStatic(HarnessManifest manifest)
    {
        return staticSuiteRunner.Run(manifest);
    }

    public GeneratedModBuildResult BuildTargetMod(HarnessManifest manifest, string workspaceRoot)
    {
        return modBuilder.BuildTargetMod(manifest, workspaceRoot);
    }

    public async Task<GameTestRunResult> RunGameAsync(HarnessManifest manifest, string workspaceRoot, CancellationToken cancellationToken)
    {
        var prepared = PrepareLaunch(manifest, workspaceRoot);
        await launcher.LaunchAsync(prepared.LaunchProfile, TimeSpan.FromMinutes(5), cancellationToken);
        return artifactCollector.LoadGameResult(manifest, prepared.Profile);
    }

    public BackgroundRunState StartGameDetached(HarnessManifest manifest, string workspaceRoot)
    {
        var prepared = PrepareLaunch(manifest, workspaceRoot);
        var process = launcher.StartDetached(prepared.LaunchProfile);
        var runState = new BackgroundRunState
        {
            ProcessId = process.Id,
            ManifestName = manifest.Name,
            StartedAtUtc = DateTime.UtcNow,
            ResultPath = Path.Combine(manifest.ResultsDirectory, "game-result.json"),
            PlayerLogPath = artifactCollector.ResolvePlayerLog(prepared.Profile),
        };

        JsonFile.Save(Path.Combine(manifest.ResultsDirectory, "run-state.json"), runState);
        return runState;
    }

    public BackgroundRunState LoadRunState(HarnessManifest manifest)
    {
        var path = Path.Combine(manifest.ResultsDirectory, "run-state.json");
        return JsonFile.Load<BackgroundRunState>(path);
    }

    public void StopGame(HarnessManifest manifest)
    {
        var state = LoadRunState(manifest);
        try
        {
            var process = Process.GetProcessById(state.ProcessId);
            if (!process.HasExited)
            {
                process.Kill(entireProcessTree: true);
            }
        }
        catch (ArgumentException)
        {
        }
    }

    public async Task<(StaticTestRunResult StaticResult, GameTestRunResult GameResult)> RunAllAsync(HarnessManifest manifest, string workspaceRoot, CancellationToken cancellationToken)
    {
        var staticResult = RunStatic(manifest);
        var gameResult = await RunGameAsync(manifest, workspaceRoot, cancellationToken);
        return (staticResult, gameResult);
    }

    public async Task WatchAsync(HarnessManifest manifest, string workspaceRoot, CancellationToken cancellationToken)
    {
        var watchedRoots = new List<string>
        {
            manifest.ModUnderTestDirectory,
            Path.Combine(workspaceRoot, "src", "RimworldTestHarness.Mod"),
            manifest.SupportModDirectory,
        };

        if (!string.IsNullOrWhiteSpace(manifest.SuiteModDirectory))
        {
            watchedRoots.Add(manifest.SuiteModDirectory);
        }

        if (!string.IsNullOrWhiteSpace(manifest.SuiteProjectPath))
        {
            watchedRoots.Add(manifest.SuiteProjectPath);
        }

        var previousSnapshot = Snapshot(watchedRoots);
        while (!cancellationToken.IsCancellationRequested)
        {
            await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken);
            var currentSnapshot = Snapshot(watchedRoots);
            var changed = previousSnapshot.Count != currentSnapshot.Count ||
                          previousSnapshot.Any(entry => !currentSnapshot.TryGetValue(entry.Key, out var timestamp) || timestamp != entry.Value);
            if (!changed)
            {
                continue;
            }

            previousSnapshot = currentSnapshot;
            await RunGameAsync(manifest, workspaceRoot, cancellationToken);
        }
    }

    private static Dictionary<string, DateTime> Snapshot(IEnumerable<string> roots)
    {
        var result = new Dictionary<string, DateTime>(StringComparer.OrdinalIgnoreCase);
        foreach (var root in roots)
        {
            if (!Directory.Exists(root))
            {
                continue;
            }

            foreach (var file in Directory.EnumerateFiles(root, "*", SearchOption.AllDirectories))
            {
                result[file] = File.GetLastWriteTimeUtc(file);
            }
        }

        return result;
    }

    private static void BuildProject(string projectPath, string configuration)
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

        using var process = Process.Start(startInfo) ?? throw new InvalidOperationException($"Failed to start dotnet build for {projectPath}");
        var stdout = process.StandardOutput.ReadToEnd();
        var stderr = process.StandardError.ReadToEnd();
        process.WaitForExit();

        if (process.ExitCode != 0)
        {
            throw new InvalidOperationException($"Build failed for {projectPath}.{Environment.NewLine}{stdout}{Environment.NewLine}{stderr}");
        }
    }

    private PreparedLaunch PrepareLaunch(HarnessManifest manifest, string workspaceRoot)
    {
        var supportModProject = Path.Combine(workspaceRoot, "src", "RimworldTestHarness.Mod", "RimworldTestHarness.Mod.csproj");
        BuildProject(supportModProject, manifest.BuildConfiguration);

        if (!string.IsNullOrWhiteSpace(manifest.SuiteProjectPath))
        {
            BuildProject(manifest.SuiteProjectPath, manifest.BuildConfiguration);
        }

        var builtTarget = modBuilder.BuildTargetMod(manifest, workspaceRoot);
        var profile = profileManager.Prepare(manifest);
        var gameModsDirectory = Path.Combine(manifest.GameDirectory, "Mods");

        var syncedTarget = modSynchronizer.SyncModDirectory(manifest.ModUnderTestDirectory, gameModsDirectory);
        modSynchronizer.SyncModDirectory(manifest.SupportModDirectory, gameModsDirectory);

        if (!string.IsNullOrWhiteSpace(manifest.SuiteModDirectory))
        {
            modSynchronizer.SyncModDirectory(manifest.SuiteModDirectory, gameModsDirectory);
        }

        modSynchronizer.OverlayAssembly(builtTarget.OutputAssemblyPath, syncedTarget);

        return new PreparedLaunch
        {
            Profile = profile,
            LaunchProfile = new RimWorldLaunchProfile
            {
                GameExePath = Path.Combine(manifest.GameDirectory, "RimWorldWin64.exe"),
                SaveDataFolder = profile.ProfileDirectory,
                ManifestPath = profile.ManifestCopyPath,
                EnabledMods = manifest.EnabledMods,
                WindowWidth = manifest.WindowWidth,
                WindowHeight = manifest.WindowHeight,
            }
        };
    }

    private sealed class PreparedLaunch
    {
        public PreparedProfile Profile { get; set; } = new();

        public RimWorldLaunchProfile LaunchProfile { get; set; } = new();
    }
}
