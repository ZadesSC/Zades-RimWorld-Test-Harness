using System;
using System.Collections.Generic;
using System.IO;
using Verse;

namespace RimworldTestHarness.Mod;

public sealed class HarnessRunnerComponent : GameComponent
{
    private bool initialized;
    private bool startedCurrentTest;
    private bool suiteCompleted;
    private int currentTestIndex;
    private int startupDelayTicks = 120;
    private TestRunManifest manifest;
    private List<IHarnessTestCase> tests;
    private GameTestRunResult result;

    public HarnessRunnerComponent(Game game)
    {
    }

    public override void StartedNewGame()
    {
        ResetForFreshRun();
    }

    public override void LoadedGame()
    {
        initialized = false;
        startedCurrentTest = false;
    }

    public override void ExposeData()
    {
        Scribe_Values.Look(ref initialized, "rth_initialized", false);
        Scribe_Values.Look(ref startedCurrentTest, "rth_startedCurrentTest", false);
        Scribe_Values.Look(ref suiteCompleted, "rth_suiteCompleted", false);
        Scribe_Values.Look(ref currentTestIndex, "rth_currentTestIndex", 0);
        Scribe_Values.Look(ref startupDelayTicks, "rth_startupDelayTicks", 120);
    }

    public override void GameComponentTick()
    {
        if (suiteCompleted)
        {
            return;
        }

        if (Current.Game == null || (Find.CurrentMap == null && (Current.Game.Maps == null || Current.Game.Maps.Count == 0)))
        {
            return;
        }

        if (!TryInitialize())
        {
            return;
        }

        if (startupDelayTicks > 0)
        {
            startupDelayTicks--;
            return;
        }

        if (currentTestIndex >= tests.Count)
        {
            FinalizeSuite();
            return;
        }

        var context = new HarnessTestContext
        {
            Runner = this,
            Manifest = manifest,
        };

        var test = tests[currentTestIndex];
        if (!startedCurrentTest)
        {
            test.Start(context);
            startedCurrentTest = true;
        }

        string details;
        string snapshotPath;
        var status = test.Tick(context, out details, out snapshotPath);
        if (status == HarnessTestStatus.Running)
        {
            return;
        }

        result.Results.Add(new GameTestCaseResult
        {
            Name = test.Name,
            Passed = status == HarnessTestStatus.Passed,
            Details = details,
            SnapshotPath = snapshotPath,
        });
        PersistPartialResult();

        currentTestIndex++;
        startedCurrentTest = false;
    }

    private bool TryInitialize()
    {
        if (initialized)
        {
            return true;
        }

        string manifestPath;
        if (!GenCommandLine.TryGetCommandLineArg("rimworldtestmanifest", out manifestPath) || string.IsNullOrEmpty(manifestPath) || !File.Exists(manifestPath))
        {
            return false;
        }

        manifest = JsonFile.Load<TestRunManifest>(manifestPath);
        tests = HarnessSuiteLoader.Create(manifest.Suite);
        if (currentTestIndex == 0)
        {
            DeleteFreshRunArtifacts();
        }
        result = LoadExistingPartialResult();
        initialized = true;
        return true;
    }

    private void ResetForFreshRun()
    {
        initialized = false;
        startedCurrentTest = false;
        suiteCompleted = false;
        currentTestIndex = 0;
        startupDelayTicks = 120;
    }

    private GameTestRunResult LoadExistingPartialResult()
    {
        Directory.CreateDirectory(manifest.ResultsDirectory);
        Directory.CreateDirectory(manifest.StateDirectory);

        var partialPath = GetPartialResultPath();
        if (File.Exists(partialPath))
        {
            return JsonFile.Load<GameTestRunResult>(partialPath);
        }

        return new GameTestRunResult
        {
            Suite = manifest.Suite,
            CompletedAtUtc = DateTime.UtcNow,
            Results = new List<GameTestCaseResult>(),
            PlayerLogPath = Path.Combine(manifest.ProfileDirectory, "Player.log"),
        };
    }

    private void PersistPartialResult()
    {
        result.CompletedAtUtc = DateTime.UtcNow;
        JsonFile.Save(GetPartialResultPath(), result);
    }

    private void FinalizeSuite()
    {
        suiteCompleted = true;
        result.CompletedAtUtc = DateTime.UtcNow;
        JsonFile.Save(Path.Combine(manifest.ResultsDirectory, "game-result.json"), result);
        Log.Message("[RTH] Test suite completed. Shutting down RimWorld.");
        Root.Shutdown();
    }

    private string GetPartialResultPath()
    {
        return Path.Combine(manifest.ResultsDirectory, "partial-game-result.json");
    }

    private void DeleteFreshRunArtifacts()
    {
        var partialPath = Path.Combine(manifest.ResultsDirectory, "partial-game-result.json");
        var finalPath = Path.Combine(manifest.ResultsDirectory, "game-result.json");

        if (File.Exists(partialPath))
        {
            File.Delete(partialPath);
        }

        if (File.Exists(finalPath))
        {
            File.Delete(finalPath);
        }

        if (!Directory.Exists(manifest.StateDirectory))
        {
            return;
        }

        foreach (var file in Directory.EnumerateFiles(manifest.StateDirectory, "*", SearchOption.TopDirectoryOnly))
        {
            File.Delete(file);
        }
    }
}
