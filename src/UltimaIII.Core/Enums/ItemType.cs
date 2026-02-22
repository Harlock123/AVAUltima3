namespace UltimaIII.Core.Enums;

/// <summary>
/// Categories of items.
/// </summary>
public enum ItemCategory
{
    Weapon,
    Armor,
    Shield,
    Consumable,
    QuestItem,
    Gold,
    Tool,
    Gem
}

public enum GemType
{
    Ruby,       // Weapon: bonus damage
    Sapphire,   // Weapon: bonus hit chance
    Emerald,    // Armor/Shield: bonus max HP
    Diamond,    // Weapon: crit chance / Armor: defense
    Topaz,      // Weapon: undead damage / Armor: magic defense
    Amethyst,   // Armor/Shield: status resistance
    Onyx,       // Weapon: lifesteal
    Opal        // Shield: damage reflect
}

public enum GemTier
{
    Chipped,
    Flawed,
    Perfect
}

[Flags]
public enum GemSlotTarget
{
    Weapon = 1,
    Armor = 2,
    Shield = 4,
    Any = Weapon | Armor | Shield
}

/// <summary>
/// Weapon types available in the game.
/// </summary>
public enum WeaponType
{
    Hands,      // No weapon
    Dagger,
    Sling,
    Mace,
    Axe,
    Sword,
    GreatSword,
    Bow,
    Crossbow,
    Exotic,     // Magic weapons
    Staff
}

/// <summary>
/// Armor types available in the game.
/// </summary>
public enum ArmorType
{
    None,
    Cloth,
    Leather,
    Chain,
    Plate,
    ExoticArmor
}

/// <summary>
/// Shield types available.
/// </summary>
public enum ShieldType
{
    None,
    SmallShield,
    LargeShield,
    MagicShield
}
