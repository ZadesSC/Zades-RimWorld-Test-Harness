using System.Diagnostics;
using RimworldTestHarness.Core.Models;

namespace RimworldTestHarness.Core.Runtime;

public sealed class RimWorldLauncher
{
    public async Task<int> LaunchAsync(RimWorldLaunchProfile profile, TimeSpan timeout, CancellationToken cancellationToken)
    {
        using var process = StartDetached(profile);
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        linkedCts.CancelAfter(timeout);

        try
        {
            await process.WaitForExitAsync(linkedCts.Token);
            return process.ExitCode;
        }
        catch (OperationCanceledException)
        {
            if (!process.HasExited)
            {
                process.Kill(entireProcessTree: true);
            }

            throw;
        }
    }

    public Process StartDetached(RimWorldLaunchProfile profile)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = profile.GameExePath,
            WorkingDirectory = Path.GetDirectoryName(profile.GameExePath)!,
            UseShellExecute = false,
            Arguments = BuildArguments(profile),
        };

        return Process.Start(startInfo) ?? throw new InvalidOperationException("Failed to start RimWorld.");
    }

    private static string BuildArguments(RimWorldLaunchProfile profile)
    {
        return string.Join(
            ' ',
            Quote("-quicktest"),
            Quote("-screen-fullscreen"),
            Quote("0"),
            Quote($"-savedatafolder={profile.SaveDataFolder}"),
            Quote($"-rimworldtestmanifest={profile.ManifestPath}"),
            Quote($"-screen-width={profile.WindowWidth}"),
            Quote($"-screen-height={profile.WindowHeight}"));
    }

    private static string Quote(string value) => value.Contains(' ') ? $"\"{value}\"" : value;
}
