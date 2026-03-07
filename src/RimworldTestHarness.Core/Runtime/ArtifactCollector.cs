using RimworldTestHarness.Core.Models;
using RimworldTestHarness.Core.Serialization;

namespace RimworldTestHarness.Core.Runtime;

public sealed class ArtifactCollector
{
    public GameTestRunResult LoadGameResult(HarnessManifest manifest, PreparedProfile profile)
    {
        var resultPath = Path.Combine(manifest.ResultsDirectory, "game-result.json");
        if (!File.Exists(resultPath))
        {
            throw new FileNotFoundException($"Game result file not found: {resultPath}");
        }

        var result = JsonFile.Load<GameTestRunResult>(resultPath);
        if (string.IsNullOrWhiteSpace(result.PlayerLogPath) || !File.Exists(result.PlayerLogPath))
        {
            result.PlayerLogPath = ResolvePlayerLog(profile);
        }

        return result;
    }

    public string ResolvePlayerLog(PreparedProfile profile)
    {
        if (File.Exists(profile.PlayerLogPath))
        {
            return profile.PlayerLogPath;
        }

        var fallback = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            "AppData",
            "LocalLow",
            "Ludeon Studios",
            "RimWorld by Ludeon Studios",
            "Player.log");

        return File.Exists(fallback) ? fallback : profile.PlayerLogPath;
    }
}
