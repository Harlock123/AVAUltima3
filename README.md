# Ultima III: Exodus - .NET 8 Avalonia Recreation

A faithful recreation of the classic 1983 RPG "Ultima III: Exodus" using .NET 8 and Avalonia UI for cross-platform compatibility.

## Author

**Lonnie Watson**

## Project Structure

```
UltimaIII.sln
├── src/
│   ├── UltimaIII.Core/          # Game logic, models, and engine (platform-agnostic)
│   │   ├── Enums/               # Game enumerations (Race, Class, TileType, etc.)
│   │   ├── Models/              # Core models (Character, Party, Item, Spell, Monster, Map)
│   │   └── Engine/              # Game engine, combat system, map generator
│   │
│   ├── UltimaIII.Avalonia/      # Avalonia UI project for rendering and input
│   │   ├── Controls/            # Custom controls (TileMapControl, CombatMapControl)
│   │   ├── ViewModels/          # MVVM ViewModels
│   │   └── Views/               # AXAML views
│   │
│   └── UltimaIII.Data/          # Game data (maps, monsters, items, spells)
```

## Features

### Character Creation System
- **Races**: Human, Elf, Dwarf, Bobbit, Fuzzy (each with stat modifiers)
- **Classes**: Fighter, Cleric, Wizard, Thief, Paladin, Barbarian, Lark, Illusionist, Druid, Alchemist, Ranger
- **Stats**: Strength, Dexterity, Intelligence, Wisdom (range: 3-25)
- **Party System**: Create up to 4 characters

### Core Game Systems
- **Tile-Based World**: Procedurally generated overworld, towns, and dungeons
- **Movement**: 4-directional movement with collision detection
- **Terrain Types**: Grass, Forest, Mountain, Water, Swamp, Lava, etc.
- **Time System**: Day/night cycle, moon phases
- **Fog of War**: Exploration reveals the map

### Combat System
- **Turn-Based Tactical Combat**: 11x11 grid
- **Initiative**: Based on Dexterity/Speed
- **Actions**: Attack, Cast Spell, Pass, Flee
- **Monster AI**: Simple pathfinding and attack behavior
- **20+ Monster Types**: Orcs, Skeletons, Dragons, Demons, etc.

### Magic System
- **Wizard Spells**: 16 spells (offensive, utility)
- **Cleric Spells**: 16 spells (healing, protection)
- **Class Restrictions**: Each class has access to different spell levels

### Map System
- **Overworld**: 64x64 procedurally generated world
- **Towns**: 32x32 with shops and buildings
- **Dungeons**: 8 levels deep with stairs, traps, and treasure

## Building and Running

### Prerequisites
- .NET 8 SDK

### Build
```bash
dotnet build
```

### Run
```bash
dotnet run --project src/UltimaIII.Avalonia
```

## Controls

### Exploration Mode
| Key | Action |
|-----|--------|
| W / Up Arrow | Move North |
| S / Down Arrow | Move South |
| A / Left Arrow | Move West |
| D / Right Arrow | Move East |
| Space | Search |
| R | Rest (in town only) |
| X | Exit current location |

### Combat Mode
| Key | Action |
|-----|--------|
| A | Attack |
| C | Cast spell |
| P | Pass turn |
| F | Flee |
| WASD / Arrows | Move target cursor |
| Enter / Space | Confirm target |
| Escape | Cancel |

## Technical Details

- **Framework**: .NET 8
- **UI**: Avalonia UI 11.2
- **Pattern**: MVVM with CommunityToolkit.Mvvm
- **Graphics**: Custom tile rendering with retro CGA/EGA color palette

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Acknowledgments

A recreation of Ultima III: Exodus, originally created by Richard Garriott (Lord British) and published by Origin Systems in 1983.
