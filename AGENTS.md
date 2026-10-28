# AGENTS.md

Notes for anyone, human or AI agent, changing this code. `README.md` covers the game's systems, controls, the
sprite checklist and downloads; this file covers how the code is put together and the rules that keep it working.

## What this is

A turn-based recreation of Ultima III: Exodus (Origin Systems, 1983) in C# on .NET 9 and Avalonia 11 (11.3.22), using
MVVM with CommunityToolkit.Mvvm. The world, towns and dungeons are generated from a seed. Music and sound effects
are synthesised chiptunes, with optional OGG music tracks decoded by NVorbis, played through OpenAL Soft
(Silk.NET).

## Commands

```bash
dotnet build UltimaIII.sln                          # needs the .NET 9 SDK
dotnet run --project src/UltimaIII.Avalonia         # play it; there are no command-line arguments
./build.sh                                          # (zsh) or ./build.ps1: self-contained single-file builds into BUILDS/
```

- `build.sh` and `build.ps1` publish win-x64/arm64, osx-x64/arm64 and linux-x64/arm64 into `BUILDS/<folder>`.
  `BUILDS/` is ignored by git.
- `AVAULTIMA3_AUDIO_BUFFER` (`small`, `medium` or `large`) picks the OpenAL mixing period.
- There are no tests yet.

## Layout

- `src/UltimaIII.Core/`: the game, with no Avalonia in it.
  - `Engine/GameEngine.cs`: the game state machine (`GameState`), party movement, locations, shops, talking,
    resting and combat start. Raises `OnMessage`, `OnStateChanged`, `OnMapChanged` and `OnPartyMoved`.
  - `Engine/CombatSystem.cs`: tactical combat. `MapGenerator.cs`: the overworld, towns and dungeons.
  - `Engine/SaveService.cs`: JSON saves. `QuestEngine`, `ShopEngine`, `SocketingService`, `GemDropTable`,
    `FieldSpellService` and `NpcGenerator` cover the rest.
  - `Models/`: party, characters, items (`ItemRegistry`), spells, monsters, maps, quests (`QuestRegistry`), shops,
    races and classes. `Enums/`: the shared enumerations, including `TileType`.
- `src/UltimaIII.Avalonia/`: the UI.
  - `MainWindow.axaml.cs`: catches every key on the tunnelling route and passes `Key.ToString()` to the current
    view model's `HandleKeyPress`. Also takes Ctrl+Shift+S screenshots.
  - `ViewModels/MainViewModel.cs`: owns the `GameEngine` and swaps `CurrentView` between the fortune teller,
    character creation, load game and game views. `GameViewModel` maps keys to engine calls and runs the dialogs.
  - `Views/`: one AXAML view per view model. `GameView` and `CombatView` redraw on a `DispatcherTimer`.
  - `Controls/`: `TileMapControl` and `CombatMapControl` draw the maps; `TileSpriteCache` loads sprite overrides.
  - `Services/Audio/`: `AudioService`, the chiptune generator and patterns, the OGG decoder, sound effects and
    `OpenAlConfig`.
  - `Assets/`: `sprites/` (tile sprite overrides) and `Music/` (OGG tracks). `sounds/` (MP3 files) and `tiles/` (a
    large tile-sheet collection) are committed but not loaded by the code.
- `src/UltimaIII.Data/`: an empty project, referenced by the UI but holding no code yet.
- `Screenshots/`: the README's pictures.

## Rules that keep it working

- **Engine first, drawing second.** Game rules belong in `UltimaIII.Core`, which has no UI dependency. View models
  translate keys into engine calls and react to the engine's events.
- **The world comes from `MapSeed`.** A new game picks `MapSeed` from the engine's `Random`, and a save stores only
  the seed, the party, the state and the tavern. Loading regenerates every map from the seed. Any change to
  `MapGenerator` or to the order of its random draws changes the world for existing saves.
- **Inject the random source.** `GameEngine` and `CombatSystem` take an optional `Random`. Use that instance rather
  than new `Random()` calls in the engine.
- **Keys arrive as names.** `HandleKeyPress` receives `Key.ToString()` and upper-cases it, so movement matches
  `"W"`/`"UP"`, `"S"`/`"DOWN"` and so on. A new key needs its case there. When a text box is open (save or
  screenshot), `MainWindow` leaves keys alone.
- **Sprites are optional.** A 32x32 PNG in `Assets/sprites/` named after the lowercased `TileType` replaces the
  code-drawn tile; anything missing falls back to code. Sprites are `AvaloniaResource`s.
- **Music files ship as content.** `AudioService` reads `Assets/Music` from `AppContext.BaseDirectory`; each OGG is
  listed as `Content` in the csproj. A missing track falls back to the synthesised one.
- **Audio.** In a virtual machine `OpenAlConfig` writes a larger mixing period to `ALSOFT_CONF`, unless the player
  set `ALSOFT_CONF` already. The game runs silent if no audio device opens.
- **Saves and screenshots.** Saves go to the local app-data folder under `AVAUltima/saves/` (`save<slot>.json`).
  Screenshots go to a `Screenshots/` folder next to the executable.
- **Line endings.** The sources use LF.

## Making a change

- Check changes by building and playing; there are no automated tests.
- Keep `README.md` (systems, controls, the sprite checklist) in step with the code.

## CI and releases

`.github/workflows/build.yml` builds every push and pull request with .NET 9. It then publishes self-contained
single-file builds for win-x64/x86/arm64, linux-x64/arm64/arm and osx-x64/arm64, renamed `AVAUltima3`, kept as
workflow artifacts. Pushes to `main` replace the assets of the `latest` pre-release, and a `v*` tag makes a proper
release. After a release it asks the portfolio site to rebuild, if the `SITE_REFRESH_TOKEN` secret is set.
