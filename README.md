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
- **Classes**: Each class has unique combat bonuses and special abilities:

| Class | Combat Role | Attack | Defense | Damage | Special |
|-------|------------|--------|---------|--------|---------|
| Fighter | Pure warrior | +2, +1/4 lvls | +1 | +2, +1/5 lvls | Best consistent melee |
| Cleric | Healer | — | +1, +1/5 lvls | — | +2 atk/dmg vs Undead |
| Wizard | Offensive caster | -2 | — | -1 | Relies on spells |
| Thief | Agile rogue | +3, +1/3 lvls | +2 | — | Critical hits (15+Level %) |
| Paladin | Holy knight | +1, +1/5 lvls | +2 | +1 | +3 atk/dmg vs Undead/Demon |
| Barbarian | Brute | +1 | — | +3, +1/4 lvls | Rage: +1 dmg per 25% HP missing |
| Lark | Thief-mage | +1, +1/5 lvls | +1 | — | Critical hits (10+Level/2 %) |
| Illusionist | Wizard-thief | -1 | +1 | -1 | Spell-focused |
| Druid | Nature priest | — | +1 | — | +2 atk/dmg vs Demon |
| Alchemist | Scholar | -1 | — | — | Jack-of-all-trades caster |
| Ranger | Wilderness warrior | +1, +1/5 lvls | +1 | +1 | +1 weapon range |

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
- **Field Casting**: Press C outside of combat to cast support spells — heal injured allies, cure poison or paralysis, resurrect fallen party members, and more without needing a Healer shop
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

### Quest System
- **18 Quests**: Spanning all 8 towns with early, mid, and late game progression
- **3 Quest Types**:
  - **Kill Quests**: Slay a target number of specific monsters (e.g. 5 Giant Rats, 12 Undead)
  - **Fetch Quests**: Recover quest items dropped by specific monsters in combat (e.g. Lich Phylactery from Liches)
  - **Explore Quests**: Visit a specific dungeon level to scout and report back
- **Quest NPCs**: Green-robed NPCs with "!" markers placed in each town — they greet you as you approach
- **Tavern Quests**: Some quests are offered by tavern barkeeps (accessible via the Tavern shop)
- **Quest Chains**: Late-game quests require completing prerequisites (e.g. "Destroy the Lich" unlocks after "Vampire Hunt")
- **Quest Log**: Press J to view active quests with progress tracking (kill counts, exploration status)
- **Talk to NPCs**: Press T when adjacent to a quest NPC to view available quests, accept new ones, or turn in completed quests
- **Quest Item Drops**: Fetch quest items drop from specific monsters at high rates (50-80%) when the quest is active
- **Rewards**: Gold and experience for each completed quest
- **Persistent**: Quest progress saves and loads with the game

### Map System
- **Overworld**: 64x64 procedurally generated world with 8 towns and 4 dungeons
- **Towns**: 32x32 with shops, paths, signs, and decorative features
- **Dungeons**: 8 levels deep with stairs, traps, secret doors, and treasure

### Tavern Recruitment
- **Recruitable NPCs**: Each town's tavern has a randomly generated companion available
- **Recruit Tab**: Press 3 in the tavern to browse the available NPC's stats, race, and class
- **Direct Recruit**: If the party has an open slot (fewer than 4 members), the NPC joins immediately
- **Swap System**: If the party is full, choose which member to leave behind at the tavern
- **Parked Characters**: Left-behind characters remain at the tavern and can be recruited later
- **Persistent**: Tavern NPCs and parked characters save and load with the game

### Audio System
- **OGG Music Support**: Place `.ogg` files in `Assets/Music/` to replace any synthesized track (e.g., `Overworld.ogg`, `Combat.ogg`, `Town.ogg`)
- **Automatic Fallback**: Tracks without a matching OGG file use procedural chiptune synthesis
- **Smart Track Matching**: Dungeon-specific combat tracks (e.g., `CombatDoom`) fall back to `Combat.ogg` if no specific file exists; same for dungeon exploration tracks
- **Procedural Chiptune Audio**: Built-in synthesized music and sound effects as default
- **Waveform Synthesis**: Square, triangle, sawtooth waves, and white noise
- **Cross-Platform**: Uses Silk.NET.OpenAL for Windows (x64/ARM64), macOS (Intel/Apple Silicon), and Linux support
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
| T | Talk to quest NPC (when adjacent) |
| C | Cast spell (field magic — heal, cure, resurrect) |
| I | Open Party Inventory |
| J | Open Quest Log |
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
| Enter / Space | Confirm purchase/sale/equip/recruit |
| 1-4 | Switch tab (Buy/Sell/Recruit/Equip/Services) |
| Tab | Cycle tabs |
| Q | Toggle sell source (Character/Party inventory) |
| Esc | Leave shop |

### Field Spell Casting (C key)
| Key | Action |
|-----|--------|
| W/S / Up/Down | Browse casters, spells, or targets |
| Enter / Space | Select caster / cast spell / confirm target |
| Esc / C | Go back / close |

### Quest Log (J key)
| Key | Action |
|-----|--------|
| W/S / Up/Down | Browse quests |
| Esc / J | Close quest log |

### Quest Dialog (T key, when adjacent to NPC)
| Key | Action |
|-----|--------|
| W/S / Up/Down | Browse quests |
| Enter / Space | View details / accept / turn in |
| Esc | Go back / close |

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
- [x] `wall.png`
- [x] `floor.png`
- [x] `door.png`
- [ ] `lockeddoor.png`
- [ ] `secretdoor.png`
- [ ] `castlewall.png`
- [ ] `castlefloor.png`
- [ ] `counter.png`
- [ ] `sign.png`

**Dungeon Features:**
- [x] `stairsup.png`
- [x] `stairsdown.png`
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
- [x] `orc.png`
- [x] `goblin.png`
- [x] `skeleton.png`
- [x] `zombie.png`
- [x] `ghoul.png`
- [x] `giant_rat.png`
- [x] `giant_spider.png`
- [x] `gelatinous_cube.png`
- [x] `troll.png`
- [x] `ogre.png`
- [x] `wraith.png`
- [x] `vampire.png`
- [x] `lich.png`
- [x] `imp.png`
- [x] `daemon.png`
- [x] `balron.png`
- [x] `dragon.png`
- [x] `pirate.png`
- [x] `sea_serpent.png`
- [x] `guard.png`

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
- **Audio**: Silk.NET.OpenAL for cross-platform audio, NVorbis for OGG Vorbis decoding

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Acknowledgments

A recreation of Ultima III: Exodus, originally created by Richard Garriott (Lord British) and published by Origin Systems in 1983.
