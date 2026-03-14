using RimworldTestHarness.Core.Models;
using RimworldTestHarness.Core.Serialization;

namespace RimworldTestHarness.Core.Runtime;

public sealed class ProfileManager
{
    public PreparedProfile Prepare(HarnessManifest manifest)
    {
        Directory.CreateDirectory(manifest.ProfileDirectory);
        Directory.CreateDirectory(Path.Combine(manifest.ProfileDirectory, "Config"));
        Directory.CreateDirectory(Path.Combine(manifest.ProfileDirectory, "Mods"));
        Directory.CreateDirectory(Path.Combine(manifest.ProfileDirectory, "Saves"));
        Directory.CreateDirectory(Path.Combine(manifest.ProfileDirectory, "Scenarios"));
        Directory.CreateDirectory(Path.Combine(manifest.ProfileDirectory, "Harness"));
        Directory.CreateDirectory(manifest.ResultsDirectory);
        Directory.CreateDirectory(manifest.StateDirectory);

        var modsConfigPath = Path.Combine(manifest.ProfileDirectory, "Config", "ModsConfig.xml");
        ModsConfigWriter.Write(modsConfigPath, manifest.EnabledMods);
        PrefsWriter.Ensure(Path.Combine(manifest.ProfileDirectory, "Config", "Prefs.xml"), manifest);

        var runManifest = new TestRunManifest
        {
            Name = manifest.Name,
            Suite = manifest.GameSuite,
            ResultsDirectory = manifest.ResultsDirectory,
            StateDirectory = manifest.StateDirectory,
            ProfileDirectory = manifest.ProfileDirectory,
            ModUnderTestDirectory = manifest.ModUnderTestDirectory,
        };

        var manifestCopyPath = Path.Combine(manifest.ProfileDirectory, "Harness", "current-run.json");
        JsonFile.Save(manifestCopyPath, runManifest);

        return new PreparedProfile
        {
            ProfileDirectory = manifest.ProfileDirectory,
            ManifestCopyPath = manifestCopyPath,
            PlayerLogPath = Path.Combine(manifest.ProfileDirectory, "Player.log"),
        };
    }
}
