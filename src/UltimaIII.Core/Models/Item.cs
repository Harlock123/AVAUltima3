using UltimaIII.Core.Enums;

namespace UltimaIII.Core.Models;

/// <summary>
/// Base class for all items.
/// </summary>
public abstract class Item
{
    public string Id { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public ItemCategory Category { get; init; }
    public int Value { get; init; }
    public int Weight { get; init; } = 1;
    public bool IsStackable { get; init; } = false;
    public int Quantity { get; set; } = 1;
}

/// <summary>
/// Weapon item.
/// </summary>
public class Weapon : Item
{
    public WeaponType WeaponType { get; init; }
    public int MinDamage { get; init; }
    public int MaxDamage { get; init; }
    public int Range { get; init; } = 1; // 1 = melee, >1 = ranged
    public int HitBonus { get; init; }
    public bool IsTwoHanded { get; init; }
    public int MaxSockets { get; init; }
    public List<Gem?> Sockets { get; } = new();

    public Weapon()
    {
        Category = ItemCategory.Weapon;
    }

    public int RollDamage(Random rng) =>
        rng.Next(MinDamage, MaxDamage + 1);

    public static readonly Weapon Hands = new()
    {
        Id = "hands",
        Name = "Hands",
        Description = "Bare fists",
        WeaponType = WeaponType.Hands,
        MinDamage = 1,
        MaxDamage = 2,
        Range = 1,
        Value = 0
    };
}

/// <summary>
/// Armor item.
/// </summary>
public class Armor : Item
{
    public ArmorType ArmorType { get; init; }
    public int Defense { get; init; }
    public int MagicDefense { get; init; }
    public int MaxSockets { get; init; }
    public List<Gem?> Sockets { get; } = new();

    public Armor()
    {
        Category = ItemCategory.Armor;
    }

    public static readonly Armor None = new()
    {
        Id = "armor_none",
        Name = "None",
        Description = "No armor",
        ArmorType = ArmorType.None,
        Defense = 0,
        Value = 0
    };
}

/// <summary>
/// Shield item.
/// </summary>
public class Shield : Item
{
    public ShieldType ShieldType { get; init; }
    public int Defense { get; init; }
    public int MaxSockets { get; init; }
    public List<Gem?> Sockets { get; } = new();

    public Shield()
    {
        Category = ItemCategory.Shield;
    }

    public static readonly Shield None = new()
    {
        Id = "shield_none",
        Name = "None",
        Description = "No shield",
        ShieldType = ShieldType.None,
        Defense = 0,
        Value = 0
    };
}

/// <summary>
/// Consumable item (food, potions, etc.)
/// </summary>
public class Consumable : Item
{
    public string Effect { get; init; } = string.Empty;
    public int EffectStrength { get; init; }

    public Consumable()
    {
        Category = ItemCategory.Consumable;
        IsStackable = true;
    }
}

/// <summary>
/// Quest item (marks, keys, etc.)
/// </summary>
public class QuestItem : Item
{
    public string QuestId { get; init; } = string.Empty;

    public QuestItem()
    {
        Category = ItemCategory.QuestItem;
    }
}

public class Gem : Item
{
    public GemType GemType { get; init; }
    public GemTier Tier { get; init; }
    public GemSlotTarget SlotTarget { get; init; }
    public int BonusValue { get; init; }
    public int BonusPercent { get; init; }

    public Gem()
    {
        Category = ItemCategory.Gem;
        IsStackable = true;
    }
}
