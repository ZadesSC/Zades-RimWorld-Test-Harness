using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using RimWorld;
using Verse;

namespace RimworldTestHarness.Mod;

[DataContract]
public sealed class TestRunManifest
{
    [DataMember(Name = "name")]
    public string Name { get; set; }

    [DataMember(Name = "suite")]
    public string Suite { get; set; }

    [DataMember(Name = "resultsDirectory")]
    public string ResultsDirectory { get; set; }

    [DataMember(Name = "stateDirectory")]
    public string StateDirectory { get; set; }

    [DataMember(Name = "profileDirectory")]
    public string ProfileDirectory { get; set; }

    [DataMember(Name = "modUnderTestDirectory")]
    public string ModUnderTestDirectory { get; set; }
}

[DataContract]
public sealed class StateSnapshot
{
    [DataMember(Name = "name")]
    public string Name { get; set; }

    [DataMember(Name = "capturedAtUtc")]
    public DateTime CapturedAtUtc { get; set; }

    [DataMember(Name = "values")]
    public Dictionary<string, string> Values { get; set; }
}

[DataContract]
public sealed class GameTestCaseResult
{
    [DataMember(Name = "name")]
    public string Name { get; set; }

    [DataMember(Name = "passed")]
    public bool Passed { get; set; }

    [DataMember(Name = "details")]
    public string Details { get; set; }

    [DataMember(Name = "snapshotPath")]
    public string SnapshotPath { get; set; }
}

[DataContract]
public sealed class GameTestRunResult
{
    [DataMember(Name = "suite")]
    public string Suite { get; set; }

    [DataMember(Name = "completedAtUtc")]
    public DateTime CompletedAtUtc { get; set; }

    [DataMember(Name = "results")]
    public List<GameTestCaseResult> Results { get; set; }

    [DataMember(Name = "playerLogPath")]
    public string PlayerLogPath { get; set; }
}

public interface IHarnessTestCase
{
    string Name { get; }

    void Start(HarnessTestContext context);

    HarnessTestStatus Tick(HarnessTestContext context, out string details, out string snapshotPath);
}

public interface IHarnessSuiteProvider
{
    string SuiteName { get; }

    IEnumerable<IHarnessTestCase> Create();
}

public enum HarnessTestStatus
{
    Running,
    Passed,
    Failed,
}

public sealed class HarnessTestContext
{
    internal HarnessRunnerComponent Runner { get; set; }

    public TestRunManifest Manifest { get; internal set; }

    public Map Map
    {
        get
        {
            if (Find.CurrentMap != null)
            {
                return Find.CurrentMap;
            }

            if (Current.Game != null && Current.Game.CurrentMap != null)
            {
                return Current.Game.CurrentMap;
            }

            return Current.Game.Maps.First();
        }
    }

    public string WriteSnapshot(string name, Dictionary<string, string> values)
    {
        var snapshot = new StateSnapshot
        {
            Name = name,
            CapturedAtUtc = DateTime.UtcNow,
            Values = values,
        };

        var path = Path.Combine(Manifest.StateDirectory, name + ".json");
        JsonFile.Save(path, snapshot);
        return path;
    }
}
