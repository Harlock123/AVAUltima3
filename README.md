# Ultima III: Exodus - .NET 9 Avalonia Recreation

A faithful recreation of the classic 1983 RPG "Ultima III: Exodus" using .NET 9 and Avalonia UI for cross-platform compatibility.

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
│   │   ├── Services/Audio/      # Procedural chiptune audio system
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
- **Safe Zones**: Towns have no random encounters
- **Save/Load**: Persistent game saves with full party, inventory, and quest state

### Combat System
- **Turn-Based Tactical Combat**: 11x11 grid
- **Initiative**: Based on Dexterity/Speed
- **Player Movement**: Move characters tactically during combat
- **Actions**: Move, Attack, Cast Spell, Pass, Flee
- **Target Selection**: Keyboard (WASD) or mouse (click to select, double-click to confirm)
- **Monster AI**: Simple pathfinding and attack behavior
- **20+ Monster Types**: Orcs, Skeletons, Dragons, Demons, etc.
- **Loot Drops**: Defeated monsters can drop weapons, armor, shields, and consumables based on per-monster loot tables

### Magic System
- **Wizard Spells**: 16 spells (offensive, utility, teleportation)
- **Cleric Spells**: 16 spells (healing, protection, cure, resurrection)
- **Status Cures**: Sanctu cures poison; Lib Rec removes all ailments (poison, confusion, sleep, paralysis)
- **Combat Casting**: CuresStatus spells work in combat to heal afflicted party members
- **Class Restrictions**: Each class has access to different spell levels

### Shop System
- **6 Shop Types**: Weapon Shop, Armor Shop, Tavern, Healer, Guild, Inn
- **Unique Shop Names**: Each town has procedurally named shops (e.g. "The Rusty Blade", "The Silver Stag")
- **Buy/Sell/Equip Tabs**: Purchase equipment, sell items, and equip gear
- **Party Inventory Selling**: Press Q on the Sell tab to toggle between selling from character inventory or party inventory
- **Services**: Healing, cure, resurrection (Healer); rest to restore HP/MP (Inn); food (Tavern)
- **Class Restrictions**: Equipment restricted by character class

### Party Inventory System
- **Shared Inventory**: Party-wide item storage fed by monster loot drops
- **Inventory Browser**: Press I to open the inventory screen from anywhere (overworld, town, dungeon)
- **5 Filter Tabs**: All, Weapons, Armor, Shields, Items
- **Equip from Party**: Select a character and equip items directly from party inventory
- **Equipment Swap**: Currently equipped items return to party inventory when replaced
- **Stackable Items**: Consumables stack with quantity display (e.g. "Healing Potion x3")
- **Persistent**: Party inventory saves and loads with the game

### Town System
- **Procedurally Generated Towns**: 32x32 maps with walls, buildings, and a central fountain square
- **Cobblestone Paths**: Connecting main avenue, east-west roads, and the town square
- **Shop Signs**: Readable signs next to each shop door displaying the shop name
- **Decorations**: Flower beds, lampposts along roads, and scattered trees
- **Door Clearance**: Automatic validation ensures decorations never block shop entrances

### Map System
- **Overworld**: 64x64 procedurally generated world with 8 towns and 4 dungeons
- **Towns**: 32x32 with shops, paths, signs, and decorative features
- **Dungeons**: 8 levels deep with stairs, traps, secret doors, and treasure

### Audio System
- **Procedural Chiptune Audio**: All music and sound effects generated programmatically
- **Waveform Synthesis**: Square, triangle, sawtooth waves, and white noise
- **Cross-Platform**: Uses Silk.NET.OpenAL for Windows, macOS, and Linux support
- **Music Tracks**:
  - Main Menu - Epic/mysterious arpeggios
  - Overworld - Adventurous march (day/night variations)
  - Town - Peaceful medieval melody
  - Dungeon - Tense, foreboding atmosphere
  - Combat - Fast, urgent battle music
  - Victory/Defeat - Triumphant fanfare or somber dirge
- **Sound Effects**: Footsteps, sword swings, hits, misses, spell casting, monster deaths, UI feedback, and more

## Building and Running

### Prerequisites
- .NET 9 SDK

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
| R | Rest |
| B | Enter Shop (when adjacent to counter) |
| I | Open Party Inventory |
| X | Exit current location |
| F5 | Save game |
| F12 | Quit game |

### Combat Mode
| Key | Action |
|-----|--------|
| W / Up Arrow | Move character north |
| S / Down Arrow | Move character south |
| A / Left Arrow | Move character west |
| D / Right Arrow | Move character east |
| T | Attack (select target) |
| C | Cast spell |
| P | Pass turn |
| F | Flee from combat |

### Target Selection (after Attack/Cast)
| Input | Action |
|-------|--------|
| WASD / Arrows | Move target cursor |
| Enter / Space | Confirm target |
| Escape | Cancel |
| Mouse Click | Select target tile |
| Mouse Double-Click | Select and confirm target |

### Inventory Browser
| Key | Action |
|-----|--------|
| W/S / Up/Down | Browse items |
| A/D / Left/Right | Switch character |
| Enter / Space | Equip selected item |
| 1-5 | Switch tab (All/Weapons/Armor/Shields/Items) |
| Tab | Cycle tabs |
| Esc / I | Close inventory |

### Shop Mode
| Key | Action |
|-----|--------|
| W/S / Up/Down | Browse items |
| A/D / Left/Right | Switch character |
| Enter / Space | Confirm purchase/sale/equip |
| 1-4 | Switch tab (Buy/Sell/Equip/Services) |
| Tab | Cycle tabs |
| Q | Toggle sell source (Character/Party inventory) |
| Esc | Leave shop |

## Custom Tile Sprites

The game supports PNG sprite overrides for any tile type. Place a 32x32 PNG in `src/UltimaIII.Avalonia/Assets/sprites/` named after the tile type (lowercased). Any tile without a matching sprite falls back to code-drawn rendering.

### Sprite Checklist

**Overworld / Town Terrain:**
- [x] `grass.png`
- [x] `forest.png`
- [x] `mountain.png`
- [x] `water.png`
- [x] `deepwater.png`
- [ ] `swamp.png`
- [x] `desert.png`
- [x] `lava.png`
- [ ] `bridge.png`
- [ ] `path.png`
- [ ] `flowers.png`
- [ ] `lamppost.png`
- [ ] `town.png`
- [ ] `dungeonentrance.png`

**Structures:**
- [ ] `wall.png`
- [ ] `floor.png`
- [ ] `door.png`
- [ ] `lockeddoor.png`
- [ ] `secretdoor.png`
- [ ] `castlewall.png`
- [ ] `castlefloor.png`
- [ ] `counter.png`
- [ ] `sign.png`

**Dungeon Features:**
- [ ] `stairsup.png`
- [ ] `stairsdown.png`
- [ ] `ladder.png`
- [ ] `pit.png`
- [ ] `ceilinghole.png`
- [ ] `trap.png`
- [ ] `void.png`

**Special:**
- [ ] `altar.png`
- [ ] `fountain.png`
- [ ] `chest.png`
- [ ] `portal.png`
- [x] `party.png`

### Combat Sprites

Combat sprites also go in `Assets/sprites/`. Player characters use their class name, monsters use their definition ID. Combat terrain checks for `combat_<terrain>.png` first, then falls back to the shared terrain sprite above.

**Player Classes (by class):**
- [x] `fighter.png`
- [x] `cleric.png`
- [x] `wizard.png`
- [x] `thief.png`
- [x] `paladin.png`
- [x] `barbarian.png`
- [x] `lark.png`
- [x] `illusionist.png`
- [x] `druid.png`
- [x] `alchemist.png`
- [x] `ranger.png`

**Monsters (by ID):**
- [ ] `orc.png`
- [ ] `goblin.png`
- [ ] `skeleton.png`
- [ ] `zombie.png`
- [ ] `ghoul.png`
- [ ] `giant_rat.png`
- [ ] `giant_spider.png`
- [ ] `gelatinous_cube.png`
- [ ] `troll.png`
- [ ] `ogre.png`
- [ ] `wraith.png`
- [ ] `vampire.png`
- [ ] `lich.png`
- [ ] `imp.png`
- [ ] `daemon.png`
- [ ] `balron.png`
- [ ] `dragon.png`
- [ ] `pirate.png`
- [ ] `sea_serpent.png`
- [ ] `guard.png`

**Combat Terrain (optional overrides):**
- [ ] `combat_grass.png`
- [ ] `combat_forest.png`
- [ ] `combat_mountain.png`
- [ ] `combat_floor.png`

## Technical Details

- **Framework**: .NET 9
- **UI**: Avalonia UI 11.2
- **Pattern**: MVVM with CommunityToolkit.Mvvm
- **Graphics**: Custom tile rendering with retro CGA/EGA color palette
- **Audio**: Silk.NET.OpenAL for cross-platform procedural audio synthesis

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Acknowledgments

A recreation of Ultima III: Exodus, originally created by Richard Garriott (Lord British) and published by Origin Systems in 1983.
