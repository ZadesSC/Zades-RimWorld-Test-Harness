using RimworldTestHarness.Core.Models;
using RimworldTestHarness.Core.Serialization;

namespace RimworldTestHarness.Core.Runtime;

public static class ManifestLoader
{
    public static HarnessManifest LoadAndNormalize(string manifestPath, string workspaceRoot)
    {
        var manifestDirectory = Path.GetDirectoryName(Path.GetFullPath(manifestPath)) ?? workspaceRoot;
        var manifest = JsonFile.Load<HarnessManifest>(manifestPath);
        manifest.Name = Ensure(manifest.Name, "Manifest name is required.");
        manifest.GameDirectory = NormalizePath(manifest.GameDirectory, manifestDirectory);
        manifest.ModUnderTestDirectory = NormalizePath(manifest.ModUnderTestDirectory, manifestDirectory);
        manifest.SupportModDirectory = NormalizePath(manifest.SupportModDirectory, manifestDirectory);
        manifest.ProfileDirectory = NormalizePath(manifest.ProfileDirectory, manifestDirectory);
        manifest.ResultsDirectory = NormalizePath(manifest.ResultsDirectory, manifestDirectory);
        manifest.StateDirectory = NormalizePath(manifest.StateDirectory, manifestDirectory);
        manifest.SuiteModDirectory = NormalizeOptionalPath(manifest.SuiteModDirectory, manifestDirectory);
        manifest.SuiteProjectPath = NormalizeOptionalPath(manifest.SuiteProjectPath, manifestDirectory);
        manifest.HarmonyPath = string.IsNullOrWhiteSpace(manifest.HarmonyPath)
            ? null
            : NormalizePath(manifest.HarmonyPath, manifestDirectory);

        return manifest;
    }

    private static string NormalizePath(string value, string workspaceRoot)
    {
        var trimmed = Ensure(value, "Manifest path field is required.");
        return Path.IsPathRooted(trimmed)
            ? Path.GetFullPath(trimmed)
            : Path.GetFullPath(Path.Combine(workspaceRoot, trimmed));
    }

    private static string? NormalizeOptionalPath(string? value, string baseDirectory)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return NormalizePath(value, baseDirectory);
    }

    private static string Ensure(string? value, string message)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidOperationException(message);
        }

        return value.Trim();
    }
}
