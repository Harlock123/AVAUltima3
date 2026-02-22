using System.Text.Json;
using System.Text.Json.Serialization;
using UltimaIII.Core.Enums;
using UltimaIII.Core.Models;

namespace UltimaIII.Core.Engine;

public class CharacterSaveData
{
    public string Name { get; set; } = string.Empty;
    public Race Race { get; set; }
    public CharacterClass Class { get; set; }
    public int Strength { get; set; }
    public int Dexterity { get; set; }
    public int Intelligence { get; set; }
    public int Wisdom { get; set; }
    public int MaxHP { get; set; }
    public int CurrentHP { get; set; }
    public int MaxMP { get; set; }
    public int CurrentMP { get; set; }
    public int Level { get; set; }
    public int Experience { get; set; }
    public StatusEffect Status { get; set; }
    public string WeaponId { get; set; } = "hands";
    public string ArmorId { get; set; } = "armor_none";
    public string ShieldId { get; set; } = "shield_none";
    public List<string> InventoryIds { get; set; } = new();
    // Gem socket data (null = empty socket, string = gem item ID)
    public List<string?> WeaponSocketGems { get; set; } = new();
    public List<string?> ArmorSocketGems { get; set; } = new();
    public List<string?> ShieldSocketGems { get; set; } = new();
}

public class InventoryEntrySave
{
    public string ItemId { get; set; } = string.Empty;
    public int Quantity { get; set; } = 1;
}

public class QuestProgressSave
{
    public string QuestId { get; set; } = string.Empty;
    public int KillCount { get; set; }
    public bool LocationVisited { get; set; }
}

public class PartySaveData
{
    public List<CharacterSaveData> Members { get; set; } = new();
    public int Gold { get; set; }
    public int Food { get; set; }
    public int X { get; set; }
    public int Y { get; set; }
    public Direction Facing { get; set; }
    public string CurrentMapId { get; set; } = "overworld";
    public int DungeonLevel { get; set; }
    public bool OnShip { get; set; }
    public bool OnHorse { get; set; }
    public int MoonPhase1 { get; set; }
    public int MoonPhase2 { get; set; }
    public int TurnCount { get; set; }
    public int DayCount { get; set; }
    public List<string> Marks { get; set; } = new();
    public List<string> CompletedQuests { get; set; } = new();
    public List<InventoryEntrySave> SharedInventory { get; set; } = new();
    public List<QuestProgressSave> ActiveQuests { get; set; } = new();
}

public class TavernNpcSaveData
{
    public string TownId { get; set; } = string.Empty;
    public CharacterSaveData Character { get; set; } = new();
}

public class GameSave
{
    public PartySaveData Party { get; set; } = new();
    public GameState State { get; set; }
    public int MapSeed { get; set; }
    public DateTime SavedAt { get; set; }
    public string SaveName { get; set; } = string.Empty;
    public List<TavernNpcSaveData> TavernNpcs { get; set; } = new();
}

public record SaveFileInfo(string FilePath, string SaveName, DateTime SavedAt, string Summary);

public static class SaveService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() }
    };

    public static string GetSaveDirectory()
    {
        var dir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "AVAUltima", "saves");
        Directory.CreateDirectory(dir);
        return dir;
    }

    public static string GetSavePath(int slot = 0)
    {
        return Path.Combine(GetSaveDirectory(), $"save{slot}.json");
    }

    public static bool SaveExists(int slot = 0)
    {
        return File.Exists(GetSavePath(slot));
    }

    public static GameSave CreateSaveData(GameEngine engine)
    {
        var party = engine.Party;
        var save = new GameSave
        {
            State = engine.State,
            MapSeed = engine.MapSeed,
            SavedAt = DateTime.Now,
            Party = new PartySaveData
            {
                Gold = party.Gold,
                Food = party.Food,
                X = party.X,
                Y = party.Y,
                Facing = party.Facing,
                CurrentMapId = party.CurrentMapId,
                DungeonLevel = party.DungeonLevel,
                OnShip = party.OnShip,
                OnHorse = party.OnHorse,
                MoonPhase1 = party.MoonPhase1,
                MoonPhase2 = party.MoonPhase2,
                TurnCount = party.TurnCount,
                DayCount = party.DayCount,
                Marks = party.Marks.ToList(),
                CompletedQuests = party.CompletedQuests.ToList(),
                SharedInventory = party.SharedInventory
                    .Select(i => new InventoryEntrySave { ItemId = i.Id, Quantity = i.Quantity })
                    .ToList(),
                ActiveQuests = party.QuestLog.GetAllProgress()
                    .Select(p => new QuestProgressSave
                    {
                        QuestId = p.QuestId,
                        KillCount = p.KillCount,
                        LocationVisited = p.LocationVisited
                    })
                    .ToList()
            }
        };

        foreach (var member in party.Members)
        {
            save.Party.Members.Add(SerializeCharacter(member));
        }

        // Save tavern NPCs
        foreach (var (townId, npc) in engine.TavernRoster.AllNpcs)
        {
            save.TavernNpcs.Add(new TavernNpcSaveData
            {
                TownId = townId,
                Character = SerializeCharacter(npc)
            });
        }

        return save;
    }

    public static void SaveGame(GameEngine engine, int slot = 0)
    {
        var save = CreateSaveData(engine);
        var json = JsonSerializer.Serialize(save, JsonOptions);
        File.WriteAllText(GetSavePath(slot), json);
    }

    public static void SaveGame(GameEngine engine, string saveName)
    {
        var save = CreateSaveData(engine);
        save.SaveName = saveName;
        var sanitized = SanitizeSaveName(saveName);
        var path = Path.Combine(GetSaveDirectory(), $"{sanitized}.json");
        var json = JsonSerializer.Serialize(save, JsonOptions);
        File.WriteAllText(path, json);
    }

    public static GameSave? LoadSaveFile(int slot = 0)
    {
        var path = GetSavePath(slot);
        if (!File.Exists(path)) return null;

        var json = File.ReadAllText(path);
        return JsonSerializer.Deserialize<GameSave>(json, JsonOptions);
    }

    public static GameSave? LoadSaveFile(string path)
    {
        if (!File.Exists(path)) return null;
        var json = File.ReadAllText(path);
        return JsonSerializer.Deserialize<GameSave>(json, JsonOptions);
    }

    public static List<SaveFileInfo> GetAllSaves()
    {
        var dir = GetSaveDirectory();
        var results = new List<SaveFileInfo>();

        foreach (var file in Directory.GetFiles(dir, "*.json"))
        {
            try
            {
                var json = File.ReadAllText(file);
                var save = JsonSerializer.Deserialize<GameSave>(json, JsonOptions);
                if (save == null) continue;

                var name = !string.IsNullOrEmpty(save.SaveName)
                    ? save.SaveName
                    : Path.GetFileNameWithoutExtension(file);

                var summary = BuildSummary(save);
                results.Add(new SaveFileInfo(file, name, save.SavedAt, summary));
            }
            catch
            {
                // Skip corrupted save files
            }
        }

        return results.OrderByDescending(s => s.SavedAt).ToList();
    }

    public static bool HasAnySaves()
    {
        var dir = GetSaveDirectory();
        return Directory.GetFiles(dir, "*.json").Length > 0;
    }

    public static void DeleteSave(string path)
    {
        if (File.Exists(path))
            File.Delete(path);
    }

    private static string BuildSummary(GameSave save)
    {
        var leader = save.Party.Members.FirstOrDefault();
        var leaderInfo = leader != null
            ? $"Lv{leader.Level} {leader.Class}"
            : "Empty party";
        var location = save.Party.CurrentMapId.Replace("_", " ");
        location = string.Concat(location[0].ToString().ToUpper(), location.AsSpan(1));
        return $"{leaderInfo} - Day {save.Party.DayCount} - {location}";
    }

    public static string SanitizeSaveName(string name)
    {
        // Replace non-alphanumeric/space/dash/underscore with underscore
        var sanitized = System.Text.RegularExpressions.Regex.Replace(name.Trim(), @"[^a-zA-Z0-9 _\-]", "_");
        // Collapse multiple underscores
        sanitized = System.Text.RegularExpressions.Regex.Replace(sanitized, @"_{2,}", "_");
        // Limit length
        if (sanitized.Length > 50) sanitized = sanitized[..50];
        return string.IsNullOrWhiteSpace(sanitized) ? "save" : sanitized;
    }

    public static void ApplySaveData(GameEngine engine, GameSave save)
    {
        var party = engine.Party;

        // Clear existing party
        while (party.Members.Count > 0)
            party.RemoveMember(party.Members[0]);

        // Restore members
        foreach (var memberData in save.Party.Members)
        {
            party.AddMember(DeserializeCharacter(memberData));
        }

        // Restore party state
        party.Gold = save.Party.Gold;
        party.Food = save.Party.Food;
        party.X = save.Party.X;
        party.Y = save.Party.Y;
        party.Facing = save.Party.Facing;
        party.CurrentMapId = save.Party.CurrentMapId;
        party.DungeonLevel = save.Party.DungeonLevel;
        party.OnShip = save.Party.OnShip;
        party.OnHorse = save.Party.OnHorse;
        party.MoonPhase1 = save.Party.MoonPhase1;
        party.MoonPhase2 = save.Party.MoonPhase2;
        party.TurnCount = save.Party.TurnCount;
        party.DayCount = save.Party.DayCount;

        party.Marks.Clear();
        foreach (var mark in save.Party.Marks)
            party.Marks.Add(mark);

        party.CompletedQuests.Clear();
        foreach (var quest in save.Party.CompletedQuests)
            party.CompletedQuests.Add(quest);

        // Restore shared inventory
        party.ClearInventory();
        foreach (var entry in save.Party.SharedInventory)
        {
            var template = ItemRegistry.FindById(entry.ItemId);
            if (template != null)
            {
                var item = ItemRegistry.CloneItem(template);
                item.Quantity = entry.Quantity;
                party.AddToInventory(item);
            }
        }

        // Restore quest log
        party.QuestLog.Clear();
        foreach (var questSave in save.Party.ActiveQuests)
        {
            party.QuestLog.AcceptQuest(questSave.QuestId);
            var progress = party.QuestLog.GetProgress(questSave.QuestId);
            if (progress != null)
            {
                progress.KillCount = questSave.KillCount;
                progress.LocationVisited = questSave.LocationVisited;
            }
        }

        // Restore tavern roster
        engine.TavernRoster.Clear();
        foreach (var tavernNpc in save.TavernNpcs)
        {
            engine.TavernRoster.SetNpc(tavernNpc.TownId, DeserializeCharacter(tavernNpc.Character));
        }
        // Fill any towns missing NPCs (backward compat with old saves)
        engine.InitializeTavernNpcs();
    }

    private static CharacterSaveData SerializeCharacter(Character c)
    {
        return new CharacterSaveData
        {
            Name = c.Name,
            Race = c.Race,
            Class = c.Class,
            Strength = c.Stats.Strength,
            Dexterity = c.Stats.Dexterity,
            Intelligence = c.Stats.Intelligence,
            Wisdom = c.Stats.Wisdom,
            MaxHP = c.BaseMaxHP,
            CurrentHP = c.CurrentHP,
            MaxMP = c.MaxMP,
            CurrentMP = c.CurrentMP,
            Level = c.Level,
            Experience = c.Experience,
            Status = c.Status,
            WeaponId = c.EquippedWeapon?.Id ?? "hands",
            ArmorId = c.EquippedArmor?.Id ?? "armor_none",
            ShieldId = c.EquippedShield?.Id ?? "shield_none",
            InventoryIds = c.Inventory.Select(i => i.Id).ToList(),
            WeaponSocketGems = SerializeSockets(c.EquippedWeapon?.Sockets),
            ArmorSocketGems = SerializeSockets(c.EquippedArmor?.Sockets),
            ShieldSocketGems = SerializeSockets(c.EquippedShield?.Sockets)
        };
    }

    private static List<string?> SerializeSockets(List<Gem?>? sockets)
    {
        if (sockets == null || sockets.Count == 0) return new List<string?>();
        return sockets.Select(g => g?.Id).ToList();
    }

    private static Character DeserializeCharacter(CharacterSaveData data)
    {
        var weapon = ResolveWeapon(data.WeaponId);
        var armor = ResolveArmor(data.ArmorId);
        var shield = ResolveShield(data.ShieldId);

        // Restore socketed gems
        RestoreSockets(weapon.Sockets, data.WeaponSocketGems);
        RestoreSockets(armor.Sockets, data.ArmorSocketGems);
        RestoreSockets(shield.Sockets, data.ShieldSocketGems);

        var character = new Character
        {
            Name = data.Name,
            Race = data.Race,
            Class = data.Class,
            Stats = new Stats(data.Strength, data.Dexterity, data.Intelligence, data.Wisdom),
            Level = data.Level,
            Experience = data.Experience,
            Status = data.Status,
            EquippedWeapon = weapon,
            EquippedArmor = armor,
            EquippedShield = shield
        };

        character.MaxHP = data.MaxHP;
        character.CurrentHP = data.CurrentHP;
        character.MaxMP = data.MaxMP;
        character.CurrentMP = data.CurrentMP;

        foreach (var itemId in data.InventoryIds)
        {
            var item = ItemRegistry.FindById(itemId);
            if (item != null)
                character.Inventory.Add(ItemRegistry.CloneItem(item));
        }

        return character;
    }

    private static void RestoreSockets(List<Gem?> sockets, List<string?> savedGemIds)
    {
        if (savedGemIds == null || savedGemIds.Count == 0) return;
        for (int i = 0; i < sockets.Count && i < savedGemIds.Count; i++)
        {
            if (savedGemIds[i] != null)
            {
                var gemTemplate = ItemRegistry.FindById(savedGemIds[i]!);
                sockets[i] = gemTemplate as Gem;
            }
        }
    }

    private static Weapon ResolveWeapon(string id)
    {
        if (string.IsNullOrEmpty(id) || id == "hands") return Weapon.Hands;
        var item = ItemRegistry.FindById(id);
        if (item is Weapon)
            return (Weapon)ItemRegistry.CloneItem(item);
        return Weapon.Hands;
    }

    private static Armor ResolveArmor(string id)
    {
        if (string.IsNullOrEmpty(id) || id == "armor_none" || id == "none") return Armor.None;
        var item = ItemRegistry.FindById(id);
        if (item is Armor)
            return (Armor)ItemRegistry.CloneItem(item);
        return Armor.None;
    }

    private static Shield ResolveShield(string id)
    {
        if (string.IsNullOrEmpty(id) || id == "shield_none" || id == "none") return Shield.None;
        var item = ItemRegistry.FindById(id);
        if (item is Shield)
            return (Shield)ItemRegistry.CloneItem(item);
        return Shield.None;
    }
}
