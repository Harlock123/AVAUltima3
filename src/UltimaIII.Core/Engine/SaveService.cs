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
    public string ArmorId { get; set; } = "none";
    public string ShieldId { get; set; } = "none";
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
}

public class GameSave
{
    public PartySaveData Party { get; set; } = new();
    public GameState State { get; set; }
    public int MapSeed { get; set; }
    public DateTime SavedAt { get; set; }
}

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
                CompletedQuests = party.CompletedQuests.ToList()
            }
        };

        foreach (var member in party.Members)
        {
            save.Party.Members.Add(new CharacterSaveData
            {
                Name = member.Name,
                Race = member.Race,
                Class = member.Class,
                Strength = member.Stats.Strength,
                Dexterity = member.Stats.Dexterity,
                Intelligence = member.Stats.Intelligence,
                Wisdom = member.Stats.Wisdom,
                MaxHP = member.MaxHP,
                CurrentHP = member.CurrentHP,
                MaxMP = member.MaxMP,
                CurrentMP = member.CurrentMP,
                Level = member.Level,
                Experience = member.Experience,
                Status = member.Status,
                WeaponId = member.EquippedWeapon?.Id ?? "hands",
                ArmorId = member.EquippedArmor?.Id ?? "none",
                ShieldId = member.EquippedShield?.Id ?? "none"
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

    public static GameSave? LoadSaveFile(int slot = 0)
    {
        var path = GetSavePath(slot);
        if (!File.Exists(path)) return null;

        var json = File.ReadAllText(path);
        return JsonSerializer.Deserialize<GameSave>(json, JsonOptions);
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
            var character = new Character
            {
                Name = memberData.Name,
                Race = memberData.Race,
                Class = memberData.Class,
                Stats = new Stats(memberData.Strength, memberData.Dexterity,
                    memberData.Intelligence, memberData.Wisdom),
                Level = memberData.Level,
                Experience = memberData.Experience,
                Status = memberData.Status,
                EquippedWeapon = Weapon.Hands,
                EquippedArmor = Armor.None,
                EquippedShield = Shield.None
            };

            // Set HP/MP (MaxHP first so CurrentHP clamp works)
            character.MaxHP = memberData.MaxHP;
            character.CurrentHP = memberData.CurrentHP;
            character.MaxMP = memberData.MaxMP;
            character.CurrentMP = memberData.CurrentMP;

            party.AddMember(character);
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
    }
}
