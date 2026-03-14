namespace RimworldTestHarness.Mod;

public sealed class HarnessMod : Verse.Mod
{
    public HarnessMod(Verse.ModContentPack content) : base(content)
    {
    }
}

[Verse.StaticConstructorOnStartup]
public static class HarnessBootstrap
{
    static HarnessBootstrap()
    {
        UnityEngine.Application.runInBackground = true;
        Verse.Prefs.RunInBackground = true;
        Verse.Prefs.Save();
        Verse.Prefs.Apply();
        Verse.Log.Message("[ZRTH] Zades RimWorld Test Harness loaded.");
    }
}
