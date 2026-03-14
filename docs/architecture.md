# Architecture

## Host Side

The host-side harness consists of:

- `RimworldTestHarness.Core`
  - manifest and profile models
  - mod metadata/XML inspection
  - generated build pipeline for legacy RimWorld mods
  - isolated profile/materialization logic
  - mod sync and `ModsConfig.xml` generation
  - RimWorld launch and artifact collection
- `RimworldTestHarness.Cli`
  - `run-static`
  - `run-game`
  - `run-all`
  - `prepare-profile`
  - `build-mod`
  - `watch`
- `RimworldTestHarness.StaticTests`
  - console-based static suite runner used by the CLI and directly

## In-Game Side

`mods/RimworldTestHarness.DevTools` contains the support mod that runs inside RimWorld.

It reads a JSON manifest path from a custom command-line argument:

- `-rimworldtestmanifest=<absolute path>`

The support mod:

- starts automatically in a quicktest game
- discovers a named suite from the manifest
- executes deterministic tests against the active map
- writes structured result JSON and state snapshots
- forces `Application.runInBackground = true` so focus loss does not pause the suite
- exits RimWorld after completion

## Artifact Flow

Artifacts are written under `artifacts/`:

- `artifacts/profiles/<profile>/`
  - isolated save/config/mods/log layout
- `artifacts/build/`
  - generated build projects and build logs
- `artifacts/results/`
  - structured game test results
- `artifacts/state/`
  - JSON state snapshots for failures or save/load checkpoints

## Mod Sync Reality

`-savedatafolder` isolates saves and config, but RimWorld does not use that folder as a local mod discovery root.

To ensure deterministic local mod loading, the harness syncs the target mod and support mod into:

- `G:\Games\Steam\steamapps\common\RimWorld\Mods`

## Hotload Model

- XML/def/assets changes: sync local mod content and optionally trigger `PlayDataLoader.HotReloadDefs()`
- assembly changes: rebuild target mod/support mod, relaunch RimWorld, rerun selected suite
- live arbitrary DLL replacement is not treated as reliable and is not the default path
