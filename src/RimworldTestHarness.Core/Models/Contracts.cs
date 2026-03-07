using System.Runtime.Serialization;

namespace RimworldTestHarness.Core.Models;

[DataContract]
public sealed class HarnessManifest
{
    [DataMember(Name = "name")]
    public string Name { get; set; } = string.Empty;

    [DataMember(Name = "gameDirectory")]
    public string GameDirectory { get; set; } = string.Empty;

    [DataMember(Name = "modUnderTestDirectory")]
    public string ModUnderTestDirectory { get; set; } = string.Empty;

    [DataMember(Name = "supportModDirectory")]
    public string SupportModDirectory { get; set; } = string.Empty;

    [DataMember(Name = "suiteModDirectory")]
    public string? SuiteModDirectory { get; set; }

    [DataMember(Name = "suiteProjectPath")]
    public string? SuiteProjectPath { get; set; }

    [DataMember(Name = "profileDirectory")]
    public string ProfileDirectory { get; set; } = string.Empty;

    [DataMember(Name = "resultsDirectory")]
    public string ResultsDirectory { get; set; } = string.Empty;

    [DataMember(Name = "stateDirectory")]
    public string StateDirectory { get; set; } = string.Empty;

    [DataMember(Name = "harmonyPath")]
    public string? HarmonyPath { get; set; }

    [DataMember(Name = "buildConfiguration")]
    public string BuildConfiguration { get; set; } = "Release";

    [DataMember(Name = "windowWidth")]
    public int WindowWidth { get; set; } = 1600;

    [DataMember(Name = "windowHeight")]
    public int WindowHeight { get; set; } = 900;

    [DataMember(Name = "enabledMods")]
    public List<string> EnabledMods { get; set; } = [];

    [DataMember(Name = "staticSuites")]
    public List<string> StaticSuites { get; set; } = [];

    [DataMember(Name = "gameSuite")]
    public string GameSuite { get; set; } = string.Empty;

    [DataMember(Name = "expectedPackageId")]
    public string? ExpectedPackageId { get; set; }

    [DataMember(Name = "requiredSupportedVersions")]
    public List<string> RequiredSupportedVersions { get; set; } = [];

    [DataMember(Name = "requiredDependencies")]
    public List<string> RequiredDependencies { get; set; } = [];

    [DataMember(Name = "requiredThingDefs")]
    public List<string> RequiredThingDefs { get; set; } = [];
}

[DataContract]
public sealed class RimWorldLaunchProfile
{
    [DataMember(Name = "gameExePath")]
    public string GameExePath { get; set; } = string.Empty;

    [DataMember(Name = "saveDataFolder")]
    public string SaveDataFolder { get; set; } = string.Empty;

    [DataMember(Name = "manifestPath")]
    public string ManifestPath { get; set; } = string.Empty;

    [DataMember(Name = "enabledMods")]
    public List<string> EnabledMods { get; set; } = [];

    [DataMember(Name = "windowWidth")]
    public int WindowWidth { get; set; }

    [DataMember(Name = "windowHeight")]
    public int WindowHeight { get; set; }
}

[DataContract]
public sealed class TestRunManifest
{
    [DataMember(Name = "name")]
    public string Name { get; set; } = string.Empty;

    [DataMember(Name = "suite")]
    public string Suite { get; set; } = string.Empty;

    [DataMember(Name = "resultsDirectory")]
    public string ResultsDirectory { get; set; } = string.Empty;

    [DataMember(Name = "stateDirectory")]
    public string StateDirectory { get; set; } = string.Empty;

    [DataMember(Name = "profileDirectory")]
    public string ProfileDirectory { get; set; } = string.Empty;

    [DataMember(Name = "modUnderTestDirectory")]
    public string ModUnderTestDirectory { get; set; } = string.Empty;
}

[DataContract]
public sealed class StateSnapshot
{
    [DataMember(Name = "name")]
    public string Name { get; set; } = string.Empty;

    [DataMember(Name = "capturedAtUtc")]
    public DateTime CapturedAtUtc { get; set; }

    [DataMember(Name = "values")]
    public Dictionary<string, string> Values { get; set; } = [];
}

[DataContract]
public sealed class StaticTestCaseResult
{
    [DataMember(Name = "name")]
    public string Name { get; set; } = string.Empty;

    [DataMember(Name = "passed")]
    public bool Passed { get; set; }

    [DataMember(Name = "details")]
    public string Details { get; set; } = string.Empty;
}

[DataContract]
public sealed class StaticTestRunResult
{
    [DataMember(Name = "suite")]
    public string Suite { get; set; } = string.Empty;

    [DataMember(Name = "completedAtUtc")]
    public DateTime CompletedAtUtc { get; set; }

    [DataMember(Name = "results")]
    public List<StaticTestCaseResult> Results { get; set; } = [];

    public bool Succeeded => Results.TrueForAll(static result => result.Passed);
}

[DataContract]
public sealed class GameTestCaseResult
{
    [DataMember(Name = "name")]
    public string Name { get; set; } = string.Empty;

    [DataMember(Name = "passed")]
    public bool Passed { get; set; }

    [DataMember(Name = "details")]
    public string Details { get; set; } = string.Empty;

    [DataMember(Name = "snapshotPath")]
    public string? SnapshotPath { get; set; }
}

[DataContract]
public sealed class GameTestRunResult
{
    [DataMember(Name = "suite")]
    public string Suite { get; set; } = string.Empty;

    [DataMember(Name = "completedAtUtc")]
    public DateTime CompletedAtUtc { get; set; }

    [DataMember(Name = "results")]
    public List<GameTestCaseResult> Results { get; set; } = [];

    [DataMember(Name = "playerLogPath")]
    public string? PlayerLogPath { get; set; }

    public bool Succeeded => Results.TrueForAll(static result => result.Passed);
}

[DataContract]
public sealed class ModMetadata
{
    [DataMember(Name = "name")]
    public string Name { get; set; } = string.Empty;

    [DataMember(Name = "packageId")]
    public string PackageId { get; set; } = string.Empty;

    [DataMember(Name = "supportedVersions")]
    public List<string> SupportedVersions { get; set; } = [];

    [DataMember(Name = "dependencies")]
    public List<string> Dependencies { get; set; } = [];
}

[DataContract]
public sealed class GeneratedModBuildResult
{
    [DataMember(Name = "assemblyName")]
    public string AssemblyName { get; set; } = string.Empty;

    [DataMember(Name = "projectPath")]
    public string ProjectPath { get; set; } = string.Empty;

    [DataMember(Name = "outputAssemblyPath")]
    public string OutputAssemblyPath { get; set; } = string.Empty;

    [DataMember(Name = "outputPdbPath")]
    public string? OutputPdbPath { get; set; }
}

[DataContract]
public sealed class PreparedProfile
{
    [DataMember(Name = "profileDirectory")]
    public string ProfileDirectory { get; set; } = string.Empty;

    [DataMember(Name = "manifestCopyPath")]
    public string ManifestCopyPath { get; set; } = string.Empty;

    [DataMember(Name = "playerLogPath")]
    public string PlayerLogPath { get; set; } = string.Empty;
}

[DataContract]
public sealed class HotloadRequest
{
    [DataMember(Name = "kind")]
    public string Kind { get; set; } = string.Empty;

    [DataMember(Name = "createdAtUtc")]
    public DateTime CreatedAtUtc { get; set; }
}

[DataContract]
public sealed class BackgroundRunState
{
    [DataMember(Name = "processId")]
    public int ProcessId { get; set; }

    [DataMember(Name = "manifestName")]
    public string ManifestName { get; set; } = string.Empty;

    [DataMember(Name = "startedAtUtc")]
    public DateTime StartedAtUtc { get; set; }

    [DataMember(Name = "resultPath")]
    public string ResultPath { get; set; } = string.Empty;

    [DataMember(Name = "playerLogPath")]
    public string PlayerLogPath { get; set; } = string.Empty;
}
