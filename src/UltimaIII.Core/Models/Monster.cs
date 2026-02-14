using UltimaIII.Core.Enums;

namespace UltimaIII.Core.Models;

/// <summary>
/// A potential loot drop from a monster.
/// </summary>
public record LootDrop(string ItemId, int DropChancePercent);

/// <summary>
/// Monster template definition.
/// </summary>
public class MonsterDefinition
{
    public string Id { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public int BaseHP { get; init; }
    public int MinDamage { get; init; }
    public int MaxDamage { get; init; }
    public int Defense { get; init; }
    public int Speed { get; init; } = 5;
    public int ExperienceValue { get; init; }
    public int GoldDrop { get; init; }
    public int Range { get; init; } = 1; // 1 = melee only
    public bool IsUndead { get; init; }
    public bool IsDemon { get; init; }
    public bool CanFly { get; init; }
    public bool CanSwim { get; init; }
    public SpellType? SpecialAbility { get; init; }
    public StatusEffect InflictsStatus { get; init; }
    public int TileIndex { get; init; } // For sprite rendering
    public int DungeonLevel { get; init; } = 1; // Minimum dungeon level to appear
    public List<LootDrop> LootTable { get; init; } = new();
}

/// <summary>
/// A monster instance in combat.
/// </summary>
public class Monster
{
    public MonsterDefinition Definition { get; }
    public int CurrentHP { get; set; }
    public int MaxHP { get; }
    public StatusEffect Status { get; set; } = StatusEffect.None;

    // Combat position
    public int X { get; set; }
    public int Y { get; set; }

    public bool IsAlive => CurrentHP > 0 && !Status.HasFlag(StatusEffect.Dead);
    public bool CanAct => IsAlive &&
                          !Status.HasFlag(StatusEffect.Asleep) &&
                          !Status.HasFlag(StatusEffect.Paralyzed) &&
                          !Status.HasFlag(StatusEffect.Petrified);

    public Monster(MonsterDefinition definition, Random rng)
    {
        Definition = definition;
        // Randomize HP a bit
        MaxHP = definition.BaseHP + rng.Next(-definition.BaseHP / 5, definition.BaseHP / 5 + 1);
        CurrentHP = MaxHP;
    }

    public int RollDamage(Random rng) =>
        rng.Next(Definition.MinDamage, Definition.MaxDamage + 1);

    public void TakeDamage(int damage)
    {
        CurrentHP -= damage;
        if (CurrentHP <= 0)
        {
            CurrentHP = 0;
            Status |= StatusEffect.Dead;
        }
    }

    public static readonly Dictionary<string, MonsterDefinition> AllMonsters = new()
    {
        ["orc"] = new MonsterDefinition
        {
            Id = "orc",
            Name = "Orc",
            BaseHP = 30,
            MinDamage = 5,
            MaxDamage = 12,
            Defense = 2,
            Speed = 5,
            ExperienceValue = 15,
            GoldDrop = 10,
            TileIndex = 0,
            DungeonLevel = 1,
            LootTable = { new("short_sword", 10), new("leather", 8) }
        },
        ["goblin"] = new MonsterDefinition
        {
            Id = "goblin",
            Name = "Goblin",
            BaseHP = 20,
            MinDamage = 3,
            MaxDamage = 8,
            Defense = 1,
            Speed = 6,
            ExperienceValue = 10,
            GoldDrop = 5,
            TileIndex = 1,
            DungeonLevel = 1,
            LootTable = { new("dagger", 15) }
        },
        ["skeleton"] = new MonsterDefinition
        {
            Id = "skeleton",
            Name = "Skeleton",
            BaseHP = 25,
            MinDamage = 4,
            MaxDamage = 10,
            Defense = 1,
            Speed = 4,
            ExperienceValue = 12,
            GoldDrop = 8,
            IsUndead = true,
            TileIndex = 2,
            DungeonLevel = 1,
            LootTable = { new("mace", 10), new("small_shield", 8) }
        },
        ["zombie"] = new MonsterDefinition
        {
            Id = "zombie",
            Name = "Zombie",
            BaseHP = 35,
            MinDamage = 6,
            MaxDamage = 14,
            Defense = 0,
            Speed = 2,
            ExperienceValue = 18,
            GoldDrop = 5,
            IsUndead = true,
            InflictsStatus = StatusEffect.Poisoned,
            TileIndex = 3,
            DungeonLevel = 1
        },
        ["ghoul"] = new MonsterDefinition
        {
            Id = "ghoul",
            Name = "Ghoul",
            BaseHP = 40,
            MinDamage = 8,
            MaxDamage = 16,
            Defense = 2,
            Speed = 4,
            ExperienceValue = 25,
            GoldDrop = 15,
            IsUndead = true,
            InflictsStatus = StatusEffect.Paralyzed,
            TileIndex = 4,
            DungeonLevel = 2
        },
        ["giant_rat"] = new MonsterDefinition
        {
            Id = "giant_rat",
            Name = "Giant Rat",
            BaseHP = 15,
            MinDamage = 2,
            MaxDamage = 6,
            Defense = 0,
            Speed = 7,
            ExperienceValue = 8,
            GoldDrop = 3,
            InflictsStatus = StatusEffect.Poisoned,
            TileIndex = 5,
            DungeonLevel = 1,
            LootTable = { new("healing_potion", 5) }
        },
        ["giant_spider"] = new MonsterDefinition
        {
            Id = "giant_spider",
            Name = "Giant Spider",
            BaseHP = 25,
            MinDamage = 4,
            MaxDamage = 10,
            Defense = 1,
            Speed = 6,
            ExperienceValue = 15,
            GoldDrop = 8,
            InflictsStatus = StatusEffect.Poisoned,
            TileIndex = 6,
            DungeonLevel = 1,
            LootTable = { new("healing_potion", 10) }
        },
        ["gelatinous_cube"] = new MonsterDefinition
        {
            Id = "gelatinous_cube",
            Name = "Gelatinous Cube",
            BaseHP = 50,
            MinDamage = 5,
            MaxDamage = 12,
            Defense = 5,
            Speed = 2,
            ExperienceValue = 30,
            GoldDrop = 20,
            InflictsStatus = StatusEffect.Paralyzed,
            TileIndex = 7,
            DungeonLevel = 2
        },
        ["troll"] = new MonsterDefinition
        {
            Id = "troll",
            Name = "Troll",
            BaseHP = 60,
            MinDamage = 10,
            MaxDamage = 20,
            Defense = 4,
            Speed = 4,
            ExperienceValue = 40,
            GoldDrop = 25,
            TileIndex = 8,
            DungeonLevel = 2,
            LootTable = { new("axe", 12), new("chain_mail", 6) }
        },
        ["ogre"] = new MonsterDefinition
        {
            Id = "ogre",
            Name = "Ogre",
            BaseHP = 55,
            MinDamage = 8,
            MaxDamage = 18,
            Defense = 3,
            Speed = 3,
            ExperienceValue = 35,
            GoldDrop = 30,
            TileIndex = 9,
            DungeonLevel = 2
        },
        ["wraith"] = new MonsterDefinition
        {
            Id = "wraith",
            Name = "Wraith",
            BaseHP = 45,
            MinDamage = 10,
            MaxDamage = 20,
            Defense = 6,
            Speed = 5,
            ExperienceValue = 50,
            GoldDrop = 40,
            IsUndead = true,
            CanFly = true,
            TileIndex = 10,
            DungeonLevel = 3
        },
        ["vampire"] = new MonsterDefinition
        {
            Id = "vampire",
            Name = "Vampire",
            BaseHP = 70,
            MinDamage = 12,
            MaxDamage = 25,
            Defense = 5,
            Speed = 6,
            ExperienceValue = 75,
            GoldDrop = 50,
            IsUndead = true,
            CanFly = true,
            TileIndex = 11,
            DungeonLevel = 4,
            LootTable = { new("long_sword", 15), new("healing_potion", 25) }
        },
        ["lich"] = new MonsterDefinition
        {
            Id = "lich",
            Name = "Lich",
            BaseHP = 80,
            MinDamage = 15,
            MaxDamage = 30,
            Defense = 7,
            Speed = 4,
            ExperienceValue = 100,
            GoldDrop = 75,
            IsUndead = true,
            Range = 5,
            SpecialAbility = SpellType.Anju_Sermani,
            TileIndex = 12,
            DungeonLevel = 5
        },
        ["imp"] = new MonsterDefinition
        {
            Id = "imp",
            Name = "Imp",
            BaseHP = 30,
            MinDamage = 6,
            MaxDamage = 12,
            Defense = 3,
            Speed = 7,
            ExperienceValue = 25,
            GoldDrop = 15,
            IsDemon = true,
            CanFly = true,
            Range = 3,
            TileIndex = 13,
            DungeonLevel = 3
        },
        ["daemon"] = new MonsterDefinition
        {
            Id = "daemon",
            Name = "Daemon",
            BaseHP = 100,
            MinDamage = 15,
            MaxDamage = 35,
            Defense = 8,
            Speed = 5,
            ExperienceValue = 125,
            GoldDrop = 100,
            IsDemon = true,
            CanFly = true,
            Range = 4,
            SpecialAbility = SpellType.Fulgar,
            TileIndex = 14,
            DungeonLevel = 6,
            LootTable = { new("great_sword", 10), new("plate_mail", 8) }
        },
        ["balron"] = new MonsterDefinition
        {
            Id = "balron",
            Name = "Balron",
            BaseHP = 150,
            MinDamage = 20,
            MaxDamage = 45,
            Defense = 10,
            Speed = 5,
            ExperienceValue = 200,
            GoldDrop = 150,
            IsDemon = true,
            CanFly = true,
            Range = 5,
            SpecialAbility = SpellType.Noxum,
            TileIndex = 15,
            DungeonLevel = 7
        },
        ["dragon"] = new MonsterDefinition
        {
            Id = "dragon",
            Name = "Dragon",
            BaseHP = 200,
            MinDamage = 25,
            MaxDamage = 50,
            Defense = 12,
            Speed = 4,
            ExperienceValue = 300,
            GoldDrop = 250,
            CanFly = true,
            Range = 6,
            TileIndex = 16,
            DungeonLevel = 8,
            LootTable = { new("great_sword", 20), new("plate_mail", 15), new("healing_potion", 40) }
        },
        ["pirate"] = new MonsterDefinition
        {
            Id = "pirate",
            Name = "Pirate",
            BaseHP = 40,
            MinDamage = 8,
            MaxDamage = 16,
            Defense = 3,
            Speed = 5,
            ExperienceValue = 30,
            GoldDrop = 50,
            CanSwim = true,
            TileIndex = 17,
            DungeonLevel = 1,
            LootTable = { new("short_sword", 15), new("small_shield", 10) }
        },
        ["sea_serpent"] = new MonsterDefinition
        {
            Id = "sea_serpent",
            Name = "Sea Serpent",
            BaseHP = 80,
            MinDamage = 15,
            MaxDamage = 30,
            Defense = 6,
            Speed = 5,
            ExperienceValue = 80,
            GoldDrop = 60,
            CanSwim = true,
            TileIndex = 18,
            DungeonLevel = 4
        },
        ["guard"] = new MonsterDefinition
        {
            Id = "guard",
            Name = "Guard",
            BaseHP = 100,
            MinDamage = 15,
            MaxDamage = 30,
            Defense = 8,
            Speed = 5,
            ExperienceValue = 50,
            GoldDrop = 25,
            TileIndex = 19,
            DungeonLevel = 1
        }
    };

    public static MonsterDefinition? Get(string id) =>
        AllMonsters.TryGetValue(id, out var def) ? def : null;
}
