# Zades RimWorld Test Harness

Windows-native automation harness for RimWorld mod testing.

This repo contains the generic harness only:

- host-side CLI/orchestration
- generated builds for legacy RimWorld mod projects
- static validation driven by manifest expectations
- a generic in-game runner mod that discovers external suite providers

Mod-specific runtime suites now live in the mod repos that own them. The current HDH suite moved to the High Density Hydroponics repo and runs from there through this harness.

## Layout

- solution: [Zades.RimWorldTestHarness.sln](/D:/Projects/git/rimworld_test_harness/Zades.RimWorldTestHarness.sln)
- core runtime/build logic: [src/RimworldTestHarness.Core](/D:/Projects/git/rimworld_test_harness/src/RimworldTestHarness.Core)
- CLI entrypoint: [src/RimworldTestHarness.Cli](/D:/Projects/git/rimworld_test_harness/src/RimworldTestHarness.Cli)
- static checks entrypoint: [tests/RimworldTestHarness.StaticTests](/D:/Projects/git/rimworld_test_harness/tests/RimworldTestHarness.StaticTests)
- generic in-game support mod: [mods/ZadesRimWorldTestHarness](/D:/Projects/git/rimworld_test_harness/mods/ZadesRimWorldTestHarness)

Reference docs:

- [architecture.md](/D:/Projects/git/rimworld_test_harness/docs/architecture.md)
- [environment.md](/D:/Projects/git/rimworld_test_harness/docs/environment.md)

## How It Works

The harness builds the target mod, syncs the target/support/suite mods into RimWorld's `Mods` directory, prepares an isolated `-savedatafolder`, and launches RimWorld in `-quicktest` with a manifest path. The in-game runner mod executes one suite at a time and writes partial/final JSON results.

Relative manifest paths now resolve from the manifest file location, not from the harness repo root. That lets a mod repo own its own manifest and still invoke this harness cleanly.

## Build

```powershell
dotnet build .\Zades.RimWorldTestHarness.sln -c Release
```

## Commands

```powershell
dotnet run --project .\src\RimworldTestHarness.Cli\RimworldTestHarness.Cli.csproj --configuration Release -- run-static <manifest.json>
dotnet run --project .\src\RimworldTestHarness.Cli\RimworldTestHarness.Cli.csproj --configuration Release -- build-mod <manifest.json>
dotnet run --project .\src\RimworldTestHarness.Cli\RimworldTestHarness.Cli.csproj --configuration Release -- prepare-profile <manifest.json>
dotnet run --project .\src\RimworldTestHarness.Cli\RimworldTestHarness.Cli.csproj --configuration Release -- start-game <manifest.json>
dotnet run --project .\src\RimworldTestHarness.Cli\RimworldTestHarness.Cli.csproj --configuration Release -- monitor-game <manifest.json>
dotnet run --project .\src\RimworldTestHarness.Cli\RimworldTestHarness.Cli.csproj --configuration Release -- stop-game <manifest.json>
dotnet run --project .\src\RimworldTestHarness.Cli\RimworldTestHarness.Cli.csproj --configuration Release -- run-game <manifest.json>
dotnet run --project .\src\RimworldTestHarness.Cli\RimworldTestHarness.Cli.csproj --configuration Release -- run-all <manifest.json>
dotnet run --project .\src\RimworldTestHarness.Cli\RimworldTestHarness.Cli.csproj --configuration Release -- watch <manifest.json>
```

## HDH Entry Point

Run HDH from the HDH repo:

```powershell
.\Tests\run-harness-tests.ps1 run-all
```

That repo owns:

- the HDH manifest
- the HDH runtime suite mod
- HDH-specific test strategy docs

## Notes

- The harness still syncs local mods into the live RimWorld `Mods` folder before launch.
- `watch` is polling-based.
- The current support mod package id is `zades.rimworld.testharness`.
