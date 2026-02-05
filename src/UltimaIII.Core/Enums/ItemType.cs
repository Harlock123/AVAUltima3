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
    Tool
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
