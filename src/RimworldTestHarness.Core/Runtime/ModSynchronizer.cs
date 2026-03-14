namespace RimworldTestHarness.Core.Runtime;

public sealed class ModSynchronizer
{
    public string SyncModDirectory(string sourceModDirectory, string profileModsDirectory)
    {
        var destination = Path.Combine(profileModsDirectory, Path.GetFileName(sourceModDirectory.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)));
        CopyDirectory(sourceModDirectory, destination);
        return destination;
    }

    public void OverlayAssembly(string builtAssemblyPath, string destinationModDirectory)
    {
        var assembliesDirectory = Path.Combine(destinationModDirectory, "Assemblies");
        Directory.CreateDirectory(assembliesDirectory);
        File.Copy(builtAssemblyPath, Path.Combine(assembliesDirectory, Path.GetFileName(builtAssemblyPath)), true);

        var pdbPath = Path.ChangeExtension(builtAssemblyPath, ".pdb");
        if (File.Exists(pdbPath))
        {
            File.Copy(pdbPath, Path.Combine(assembliesDirectory, Path.GetFileName(pdbPath)), true);
        }
    }

    private static void CopyDirectory(string sourceDir, string destinationDir)
    {
        if (Directory.Exists(destinationDir))
        {
            Directory.Delete(destinationDir, true);
        }

        Directory.CreateDirectory(destinationDir);

        foreach (var directory in Directory.EnumerateDirectories(sourceDir, "*", SearchOption.AllDirectories))
        {
            var relative = Path.GetRelativePath(sourceDir, directory);
            if (ShouldSkip(relative))
            {
                continue;
            }

            Directory.CreateDirectory(Path.Combine(destinationDir, relative));
        }

        foreach (var file in Directory.EnumerateFiles(sourceDir, "*", SearchOption.AllDirectories))
        {
            var relative = Path.GetRelativePath(sourceDir, file);
            if (ShouldSkip(relative))
            {
                continue;
            }

            var destinationFile = Path.Combine(destinationDir, relative);
            Directory.CreateDirectory(Path.GetDirectoryName(destinationFile)!);
            File.Copy(file, destinationFile, true);
        }
    }

    private static bool ShouldSkip(string relativePath)
    {
        var parts = relativePath.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        return parts.Any(static part =>
            part.Equals(".git", StringComparison.OrdinalIgnoreCase) ||
            part.Equals("Source", StringComparison.OrdinalIgnoreCase) ||
            part.Equals("obj", StringComparison.OrdinalIgnoreCase) ||
            part.Equals("bin", StringComparison.OrdinalIgnoreCase));
    }
}
