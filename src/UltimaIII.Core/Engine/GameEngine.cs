using UltimaIII.Core.Enums;
using UltimaIII.Core.Models;

namespace UltimaIII.Core.Engine;

/// <summary>
/// Main game engine that coordinates all game systems.
/// </summary>
public class GameEngine
{
    private readonly Random _rng;
    private readonly Dictionary<string, GameMap> _maps = new();

    public Party Party { get; private set; } = new();
    public GameMap? CurrentMap { get; private set; }
    public GameState State { get; private set; } = GameState.MainMenu;
    public CombatSystem Combat { get; }
    public int MapSeed { get; private set; }
    public ShopType? CurrentShopType { get; private set; }

    // Message log
    public List<string> MessageLog { get; } = new();
    private const int MaxLogMessages = 50;

    // Events
    public event Action<string>? OnMessage;
    public event Action<GameState>? OnStateChanged;
    public event Action? OnMapChanged;
    public event Action? OnPartyMoved;

    public GameEngine(Random? rng = null)
    {
        _rng = rng ?? new Random();
        Combat = new CombatSystem(_rng);
        Combat.OnCombatMessage += msg => AddMessage(msg);
        Combat.OnCombatEnd += HandleCombatEnd;
    }

    public void NewGame()
    {
        Party = new Party();
        State = GameState.CharacterCreation;
        OnStateChanged?.Invoke(State);
    }

    public void StartGame()
    {
        if (Party.IsEmpty)
        {
            AddMessage("You must create at least one character!");
            return;
        }

        // Generate a seed for deterministic map generation
        MapSeed = _rng.Next();
        GenerateMaps();

        // Set starting position (Lord British's Castle area)
        Party.X = 40;
        Party.Y = 40;
        Party.CurrentMapId = "overworld";

        State = GameState.Overworld;
        OnStateChanged?.Invoke(State);
        OnMapChanged?.Invoke();

        AddMessage("Thou hast entered the land of Sosaria!");
        AddMessage("Seek Lord British in his castle.");

        UpdateVisibility();
    }

    public void LoadGame(GameSave save)
    {
        // Regenerate maps with the saved seed
        MapSeed = save.MapSeed;
        GenerateMaps();

        // Restore party and game state from save data
        SaveService.ApplySaveData(this, save);

        // Set the current map
        if (_maps.TryGetValue(Party.CurrentMapId, out var map))
            CurrentMap = map;

        State = save.State;
        UpdateVisibility();
        OnStateChanged?.Invoke(State);
        OnMapChanged?.Invoke();

        AddMessage("Game loaded.");
    }

    private void GenerateMaps()
    {
        _maps.Clear();
        var mapRng = new Random(MapSeed);

        var overworld = MapGenerator.GenerateOverworld(mapRng);
        _maps["overworld"] = overworld;
        CurrentMap = overworld;

        GenerateTowns(mapRng);
    }

    private void GenerateTowns(Random rng)
    {
        // Generate several towns
        var townNames = new[] { "Britain", "Yew", "Montor", "Moon", "Grey", "Death Gulch", "Devil Guard", "Fawn" };

        foreach (var name in townNames)
        {
            string id = name.ToLower().Replace(" ", "_");
            var town = MapGenerator.GenerateTown(rng, id, name);
            _maps[id] = town;
        }

        // Generate dungeons
        var dungeonNames = new[] { "Doom", "Fire", "Time", "Snake" };
        foreach (var name in dungeonNames)
        {
            string id = $"dungeon_{name.ToLower()}";
            for (int level = 1; level <= 8; level++)
            {
                var dungeon = MapGenerator.GenerateDungeonLevel(rng, $"{id}_l{level}", name, level);
                _maps[$"{id}_l{level}"] = dungeon;
            }
        }
    }

    public void AddMessage(string message)
    {
        MessageLog.Add(message);
        while (MessageLog.Count > MaxLogMessages)
            MessageLog.RemoveAt(0);
        OnMessage?.Invoke(message);
    }

    public bool MoveParty(Direction direction)
    {
        if (State != GameState.Overworld && State != GameState.Town && State != GameState.Dungeon)
            return false;

        if (CurrentMap == null) return false;

        var (dx, dy) = direction.ToOffset();
        int newX = Party.X + dx;
        int newY = Party.Y + dy;

        // Handle map wrapping for overworld
        if (CurrentMap.MapType == MapType.Overworld)
        {
            (newX, newY) = CurrentMap.WrapCoordinates(newX, newY);
        }

        // Walking off the map edge: only exit towns/dungeons through a doorway
        if (!CurrentMap.IsInBounds(newX, newY))
        {
            if (State == GameState.Town || State == GameState.Dungeon)
            {
                // Only allow exit if stepping out from a door tile
                var currentTile = CurrentMap.GetTile(Party.X, Party.Y);
                if (currentTile.Type == TileType.Door)
                {
                    ExitLocation();
                    return true;
                }
            }
            AddMessage("Blocked!");
            return false;
        }

        // Check if tile is passable
        if (!CurrentMap.IsPassable(newX, newY, Party.OnShip))
        {
            var tile = CurrentMap.GetTile(newX, newY);
            if (tile.Type == TileType.LockedDoor)
            {
                AddMessage("The door is locked.");
            }
            else if (tile.Type.IsWater() && !Party.OnShip)
            {
                AddMessage("You need a ship to cross water.");
            }
            else
            {
                AddMessage("Blocked!");
            }
            return false;
        }

        Party.X = newX;
        Party.Y = newY;
        Party.Facing = direction;

        // Advance game time
        Party.AdvanceTime();

        UpdateVisibility();
        OnPartyMoved?.Invoke();

        // Check for special tiles
        CheckSpecialTile();

        // Random encounter check
        CheckForEncounter();

        return true;
    }

    private void UpdateVisibility()
    {
        if (CurrentMap == null) return;

        CurrentMap.ClearVisibility();
        int radius = Party.IsNight ? 3 : 5;

        if (State == GameState.Dungeon)
            radius = 2;

        CurrentMap.RevealArea(Party.X, Party.Y, radius);
    }

    private void CheckSpecialTile()
    {
        if (CurrentMap == null) return;

        var tile = CurrentMap.GetTile(Party.X, Party.Y);

        // Check for location entrances
        foreach (var location in CurrentMap.Locations)
        {
            if (location.X == Party.X && location.Y == Party.Y)
            {
                HandleLocationEntry(location);
                return;
            }
        }

        // Handle dangerous terrain
        if (tile.Type == TileType.Swamp)
        {
            AddMessage("The swamp poisons the air...");
            foreach (var member in Party.GetLivingMembers())
            {
                if (_rng.Next(100) < 15)
                {
                    member.Status |= StatusEffect.Poisoned;
                    AddMessage($"{member.Name} is poisoned!");
                }
            }
        }
        else if (tile.Type == TileType.Lava)
        {
            AddMessage("The lava burns!");
            foreach (var member in Party.GetLivingMembers())
            {
                member.TakeDamage(5 + _rng.Next(10));
            }
        }

        // Check for stairs in dungeons
        if (tile.Type == TileType.StairsDown && State == GameState.Dungeon)
        {
            GoDownstairs();
        }
        else if (tile.Type == TileType.StairsUp && State == GameState.Dungeon)
        {
            GoUpstairs();
        }
    }

    private void HandleLocationEntry(MapLocation location)
    {
        if (string.IsNullOrEmpty(location.TargetMapId)) return;

        if (!_maps.TryGetValue(location.TargetMapId, out var targetMap))
        {
            AddMessage($"Cannot find {location.Name}");
            return;
        }

        AddMessage($"Entering {location.Name}...");

        CurrentMap = targetMap;
        Party.CurrentMapId = location.TargetMapId;

        // Set position at entry point
        if (!string.IsNullOrEmpty(location.TargetEntryPoint) &&
            targetMap.EntryPoints.TryGetValue(location.TargetEntryPoint, out var entryPoint))
        {
            Party.X = entryPoint.x;
            Party.Y = entryPoint.y;
        }
        else if (targetMap.EntryPoints.TryGetValue("default", out var defaultEntry))
        {
            Party.X = defaultEntry.x;
            Party.Y = defaultEntry.y;
        }
        else
        {
            Party.X = targetMap.Width / 2;
            Party.Y = targetMap.Height / 2;
        }

        // Update state based on map type
        State = targetMap.MapType switch
        {
            MapType.Town => GameState.Town,
            MapType.Castle => GameState.Town,
            MapType.Dungeon => GameState.Dungeon,
            _ => GameState.Overworld
        };

        Party.DungeonLevel = targetMap.DungeonLevel;

        UpdateVisibility();
        OnStateChanged?.Invoke(State);
        OnMapChanged?.Invoke();
    }

    public void ExitLocation()
    {
        if (State != GameState.Town && State != GameState.Dungeon) return;
        if (CurrentMap?.MapType == MapType.Overworld) return;

        // Return to overworld
        if (_maps.TryGetValue("overworld", out var overworld))
        {
            // Find the location on the overworld that matches current map
            foreach (var loc in overworld.Locations)
            {
                if (loc.TargetMapId == Party.CurrentMapId)
                {
                    Party.X = loc.X;
                    Party.Y = loc.Y;
                    break;
                }
            }

            CurrentMap = overworld;
            Party.CurrentMapId = "overworld";
            Party.DungeonLevel = 0;
            State = GameState.Overworld;

            UpdateVisibility();
            OnStateChanged?.Invoke(State);
            OnMapChanged?.Invoke();

            AddMessage("You return to the surface.");
        }
    }

    private void GoDownstairs()
    {
        int nextLevel = Party.DungeonLevel + 1;
        string baseDungeonId = Party.CurrentMapId[..Party.CurrentMapId.LastIndexOf("_l")];
        string nextMapId = $"{baseDungeonId}_l{nextLevel}";

        if (_maps.TryGetValue(nextMapId, out var nextMap))
        {
            CurrentMap = nextMap;
            Party.CurrentMapId = nextMapId;
            Party.DungeonLevel = nextLevel;

            if (nextMap.EntryPoints.TryGetValue("stairs_up", out var entry))
            {
                Party.X = entry.x;
                Party.Y = entry.y;
            }

            UpdateVisibility();
            OnMapChanged?.Invoke();
            AddMessage($"You descend to level {nextLevel}.");
        }
    }

    private void GoUpstairs()
    {
        if (Party.DungeonLevel <= 1)
        {
            ExitLocation();
            return;
        }

        int prevLevel = Party.DungeonLevel - 1;
        string baseDungeonId = Party.CurrentMapId[..Party.CurrentMapId.LastIndexOf("_l")];
        string prevMapId = $"{baseDungeonId}_l{prevLevel}";

        if (_maps.TryGetValue(prevMapId, out var prevMap))
        {
            CurrentMap = prevMap;
            Party.CurrentMapId = prevMapId;
            Party.DungeonLevel = prevLevel;

            if (prevMap.EntryPoints.TryGetValue("stairs_down", out var entry))
            {
                Party.X = entry.x;
                Party.Y = entry.y;
            }

            UpdateVisibility();
            OnMapChanged?.Invoke();
            AddMessage($"You ascend to level {prevLevel}.");
        }
    }

    private void CheckForEncounter()
    {
        if (State == GameState.Combat) return;

        // Towns are safe zones - no random encounters
        if (State == GameState.Town) return;

        // Base encounter chance varies by location
        int encounterChance = State switch
        {
            GameState.Dungeon => 15 + Party.DungeonLevel * 2,
            GameState.Overworld => Party.IsNight ? 8 : 5,
            _ => 0 // No encounters in other states
        };

        if (encounterChance == 0 || _rng.Next(100) >= encounterChance) return;

        // Generate monsters
        var monsters = GenerateEncounter();
        if (monsters.Count == 0) return;

        var terrain = CurrentMap?.GetTile(Party.X, Party.Y).Type ?? TileType.Grass;
        StartCombat(monsters, terrain);
    }

    private List<Monster> GenerateEncounter()
    {
        var monsters = new List<Monster>();
        var availableMonsters = Monster.AllMonsters.Values
            .Where(m => m.DungeonLevel <= Math.Max(1, Party.DungeonLevel + 2))
            .ToList();

        if (availableMonsters.Count == 0) return monsters;

        // Weighted selection favoring appropriate level
        var weightedMonsters = availableMonsters
            .Where(m => m.DungeonLevel <= Party.DungeonLevel + 1)
            .ToList();

        if (weightedMonsters.Count == 0)
            weightedMonsters = availableMonsters;

        var selectedDef = weightedMonsters[_rng.Next(weightedMonsters.Count)];

        // Scale enemy count: 1 to 2x party size
        int maxEnemies = Math.Max(1, Party.Count * 2);
        int count = _rng.Next(1, maxEnemies + 1);
        for (int i = 0; i < count; i++)
        {
            monsters.Add(new Monster(selectedDef, _rng));
        }

        return monsters;
    }

    public void StartCombat(List<Monster> monsters, TileType terrain = TileType.Grass)
    {
        AddMessage($"Ambushed by {monsters[0].Definition.Name}!");

        State = GameState.Combat;
        OnStateChanged?.Invoke(State);

        Combat.StartCombat(Party, monsters, terrain);
    }

    private void HandleCombatEnd()
    {
        if (Party.AllDead)
        {
            State = GameState.GameOver;
            AddMessage("Thy party hath been vanquished...");
        }
        else
        {
            // Award rewards
            var (exp, gold) = Combat.GetCombatRewards();
            if (gold > 0)
            {
                Party.AddGold(gold);
                AddMessage($"Found {gold} gold!");
            }

            // Return to previous state
            State = CurrentMap?.MapType switch
            {
                MapType.Overworld => GameState.Overworld,
                MapType.Town => GameState.Town,
                MapType.Castle => GameState.Town,
                MapType.Dungeon => GameState.Dungeon,
                _ => GameState.Overworld
            };
        }

        OnStateChanged?.Invoke(State);
    }

    public void Search()
    {
        if (CurrentMap == null) return;

        AddMessage("Searching...");
        Party.AdvanceTime();

        // Check for secret doors in adjacent tiles
        for (int dy = -1; dy <= 1; dy++)
        {
            for (int dx = -1; dx <= 1; dx++)
            {
                int x = Party.X + dx;
                int y = Party.Y + dy;
                var tile = CurrentMap.GetTile(x, y);

                if (tile.Type == TileType.SecretDoor)
                {
                    CurrentMap.SetTileType(x, y, TileType.Door);
                    AddMessage("You found a secret door!");
                    OnMapChanged?.Invoke();
                    return;
                }
            }
        }

        // Check current tile for treasure
        var currentTile = CurrentMap.GetTile(Party.X, Party.Y);
        if (currentTile.Type == TileType.Chest)
        {
            int gold = _rng.Next(10, 100) * (Party.DungeonLevel + 1);
            Party.AddGold(gold);
            CurrentMap.SetTileType(Party.X, Party.Y, TileType.Floor);
            AddMessage($"Found {gold} gold in the chest!");
            OnMapChanged?.Invoke();

            // Small chance of trap
            if (_rng.Next(100) < 20)
            {
                AddMessage("The chest was trapped!");
                var victim = Party.GetLivingMembers().FirstOrDefault();
                if (victim != null)
                {
                    victim.TakeDamage(_rng.Next(5, 15));
                    AddMessage($"{victim.Name} takes damage from the trap!");
                }
            }
            return;
        }

        AddMessage("Nothing found.");
    }

    public void OpenDoor(Direction direction)
    {
        if (CurrentMap == null) return;

        var (dx, dy) = direction.ToOffset();
        int x = Party.X + dx;
        int y = Party.Y + dy;

        var tile = CurrentMap.GetTile(x, y);

        if (tile.Type == TileType.Door)
        {
            CurrentMap.SetTileType(x, y, TileType.Floor);
            AddMessage("Door opened.");
            OnMapChanged?.Invoke();
        }
        else if (tile.Type == TileType.LockedDoor)
        {
            // Check for key or thief
            var thief = Party.GetLivingMembers()
                .FirstOrDefault(m => m.Class == CharacterClass.Thief || m.Class == CharacterClass.Lark);

            if (thief != null && _rng.Next(100) < 50 + thief.Stats.Dexterity)
            {
                CurrentMap.SetTileType(x, y, TileType.Floor);
                AddMessage($"{thief.Name} picks the lock!");
                OnMapChanged?.Invoke();
            }
            else
            {
                AddMessage("The door is locked.");
            }
        }
        else
        {
            AddMessage("No door there.");
        }
    }

    public bool TryEnterShop()
    {
        if (State != GameState.Town || CurrentMap == null) return false;

        // Scan adjacent tiles for a Counter with EntityId
        int[] dx = { 0, 0, -1, 1 };
        int[] dy = { -1, 1, 0, 0 };

        for (int i = 0; i < 4; i++)
        {
            int nx = Party.X + dx[i];
            int ny = Party.Y + dy[i];

            if (!CurrentMap.IsInBounds(nx, ny)) continue;

            var tile = CurrentMap.GetTile(nx, ny);
            if (tile.Type == TileType.Counter && !string.IsNullOrEmpty(tile.EntityId))
            {
                if (ShopDefinition.EntityIdToShopType.TryGetValue(tile.EntityId, out var shopType))
                {
                    CurrentShopType = shopType;
                    State = GameState.Shop;
                    OnStateChanged?.Invoke(State);
                    return true;
                }
            }
        }

        AddMessage("No shop here.");
        return false;
    }

    public void ExitShop()
    {
        if (State != GameState.Shop) return;

        CurrentShopType = null;
        State = GameState.Town;
        OnStateChanged?.Invoke(State);
    }

    public void Rest()
    {
        if (State != GameState.Town)
        {
            AddMessage("You can only rest safely in town.");
            return;
        }

        Party.Rest();
        AddMessage("The party rests...");

        foreach (var member in Party.GetLivingMembers())
        {
            if (member.CurrentHP < member.MaxHP)
            {
                AddMessage($"{member.Name} recovers some health.");
            }
        }
    }

}
