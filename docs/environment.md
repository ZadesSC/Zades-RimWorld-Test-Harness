# Environment

## Why Windows

The harness runs RimWorld on Windows because:

- the installed game is `RimWorldWin64.exe`
- RimWorld modding commonly targets .NET Framework 4.8 style assemblies
- process launch, UI lifetime, and log paths are simpler on Windows
- WSL adds friction for GUI automation without helping the game runtime itself

WSL can still be used for auxiliary scripting, but not as the primary test-execution environment.

## Known Paths

- RimWorld install: `G:\Games\Steam\steamapps\common\RimWorld`
- RimWorld managed assemblies: `G:\Games\Steam\steamapps\common\RimWorld\RimWorldWin64_Data\Managed`
- Steam workshop mods: `G:\Games\Steam\steamapps\workshop\content\294100`
- Example mod under test: `D:\Projects\rimworld_mods\HighDensityHydroponics`

## Isolation

The harness launches RimWorld with:

- `-quicktest`
- `-savedatafolder=<repo>\artifacts\profiles\<name>`
- `-screen-fullscreen 0`
- explicit `-screen-width` / `-screen-height`

That profile owns:

- `Config\ModsConfig.xml`
- `Saves\`
- `Scenarios\`
- `Harness\` for manifests, hotload requests, and results

The harness-managed local mods are synced into `G:\Games\Steam\steamapps\common\RimWorld\Mods` because RimWorld does not discover local mods from the overridden save-data folder.

## Decompilation

The shipped `Source` directory inside the RimWorld install is partial reference only.

For full code investigation, use the managed assemblies in `RimWorldWin64_Data\Managed`, especially:

- `Assembly-CSharp.dll`
- Unity module assemblies as needed

This workspace does not silently install Windows decompiler executables. The intended default is to use non-installer tooling when needed and add that step explicitly later.
