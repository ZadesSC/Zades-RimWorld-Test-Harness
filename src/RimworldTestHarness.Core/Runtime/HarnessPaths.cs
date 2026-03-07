namespace RimworldTestHarness.Core.Runtime;

public sealed class HarnessPaths
{
    public HarnessPaths(string workspaceRoot)
    {
        WorkspaceRoot = workspaceRoot;
        ArtifactsRoot = Path.Combine(workspaceRoot, "artifacts");
        BuildRoot = Path.Combine(ArtifactsRoot, "build");
        ResultsRoot = Path.Combine(ArtifactsRoot, "results");
        StateRoot = Path.Combine(ArtifactsRoot, "state");
    }

    public string WorkspaceRoot { get; }

    public string ArtifactsRoot { get; }

    public string BuildRoot { get; }

    public string ResultsRoot { get; }

    public string StateRoot { get; }
}
