using UltimaIII.Core.Enums;

namespace UltimaIII.Core.Models;

/// <summary>
/// Defines the properties of a character class.
/// </summary>
public class ClassDefinition
{
    public CharacterClass Class { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public StatRequirements Requirements { get; init; } = new();
    public int BaseHitPoints { get; init; } = 100;
    public int BaseMagicPoints { get; init; } = 0;
    public int HitPointsPerLevel { get; init; } = 10;
    public int MagicPointsPerLevel { get; init; } = 0;
    public bool CanUseWizardSpells { get; init; } = false;
    public bool CanUseClericSpells { get; init; } = false;
    public int MaxWizardSpellLevel { get; init; } = 0;
    public int MaxClericSpellLevel { get; init; } = 0;
    public HashSet<ArmorType> AllowedArmor { get; init; } = new();
    public HashSet<WeaponType> AllowedWeapons { get; init; } = new();
    public HashSet<ShieldType> AllowedShields { get; init; } = new();

    private static readonly HashSet<ArmorType> AllArmor = new()
        { ArmorType.Cloth, ArmorType.Leather, ArmorType.Chain, ArmorType.Plate, ArmorType.ExoticArmor };
    private static readonly HashSet<ArmorType> LightArmor = new()
        { ArmorType.Cloth, ArmorType.Leather };
    private static readonly HashSet<ArmorType> MediumArmor = new()
        { ArmorType.Cloth, ArmorType.Leather, ArmorType.Chain };
    private static readonly HashSet<WeaponType> AllWeapons = new()
        { WeaponType.Dagger, WeaponType.Sling, WeaponType.Mace, WeaponType.Axe, WeaponType.Sword,
          WeaponType.GreatSword, WeaponType.Bow, WeaponType.Crossbow, WeaponType.Exotic, WeaponType.Staff };
    private static readonly HashSet<WeaponType> SimpleWeapons = new()
        { WeaponType.Dagger, WeaponType.Sling, WeaponType.Staff };
    private static readonly HashSet<WeaponType> ThiefWeapons = new()
        { WeaponType.Dagger, WeaponType.Sling, WeaponType.Sword, WeaponType.Bow };
    private static readonly HashSet<ShieldType> AllShields = new()
        { ShieldType.SmallShield, ShieldType.LargeShield, ShieldType.MagicShield };

    public static readonly Dictionary<CharacterClass, ClassDefinition> AllClasses = new()
    {
        [CharacterClass.Fighter] = new ClassDefinition
        {
            Class = CharacterClass.Fighter,
            Name = "Fighter",
            Description = "Master of weapons and armor. No magical abilities.",
            Requirements = new StatRequirements(MinStrength: 15),
            BaseHitPoints = 150,
            HitPointsPerLevel = 15,
            AllowedArmor = AllArmor,
            AllowedWeapons = AllWeapons,
            AllowedShields = AllShields
        },
        [CharacterClass.Cleric] = new ClassDefinition
        {
            Class = CharacterClass.Cleric,
            Name = "Cleric",
            Description = "Holy warrior with healing and protection spells.",
            Requirements = new StatRequirements(MinWisdom: 15),
            BaseHitPoints = 125,
            BaseMagicPoints = 50,
            HitPointsPerLevel = 10,
            MagicPointsPerLevel = 5,
            CanUseClericSpells = true,
            MaxClericSpellLevel = 16,
            AllowedArmor = MediumArmor,
            AllowedWeapons = new HashSet<WeaponType> { WeaponType.Mace, WeaponType.Staff },
            AllowedShields = AllShields
        },
        [CharacterClass.Wizard] = new ClassDefinition
        {
            Class = CharacterClass.Wizard,
            Name = "Wizard",
            Description = "Master of arcane magic. Devastating offensive spells.",
            Requirements = new StatRequirements(MinIntelligence: 15),
            BaseHitPoints = 75,
            BaseMagicPoints = 75,
            HitPointsPerLevel = 5,
            MagicPointsPerLevel = 8,
            CanUseWizardSpells = true,
            MaxWizardSpellLevel = 16,
            AllowedArmor = new HashSet<ArmorType> { ArmorType.Cloth },
            AllowedWeapons = SimpleWeapons,
            AllowedShields = new HashSet<ShieldType>()
        },
        [CharacterClass.Thief] = new ClassDefinition
        {
            Class = CharacterClass.Thief,
            Name = "Thief",
            Description = "Stealthy rogue skilled at disarming traps and lockpicking.",
            Requirements = new StatRequirements(MinDexterity: 15),
            BaseHitPoints = 100,
            HitPointsPerLevel = 8,
            AllowedArmor = LightArmor,
            AllowedWeapons = ThiefWeapons,
            AllowedShields = new HashSet<ShieldType> { ShieldType.SmallShield }
        },
        [CharacterClass.Paladin] = new ClassDefinition
        {
            Class = CharacterClass.Paladin,
            Name = "Paladin",
            Description = "Holy knight combining combat prowess with cleric magic.",
            Requirements = new StatRequirements(MinStrength: 15, MinWisdom: 15),
            BaseHitPoints = 125,
            BaseMagicPoints = 25,
            HitPointsPerLevel = 12,
            MagicPointsPerLevel = 3,
            CanUseClericSpells = true,
            MaxClericSpellLevel = 8,
            AllowedArmor = AllArmor,
            AllowedWeapons = AllWeapons,
            AllowedShields = AllShields
        },
        [CharacterClass.Barbarian] = new ClassDefinition
        {
            Class = CharacterClass.Barbarian,
            Name = "Barbarian",
            Description = "Fierce warrior with extra hit points but no magic.",
            Requirements = new StatRequirements(MinStrength: 15, MinDexterity: 10),
            BaseHitPoints = 175,
            HitPointsPerLevel = 18,
            AllowedArmor = new HashSet<ArmorType> { ArmorType.Cloth, ArmorType.Leather, ArmorType.Chain },
            AllowedWeapons = AllWeapons,
            AllowedShields = AllShields
        },
        [CharacterClass.Lark] = new ClassDefinition
        {
            Class = CharacterClass.Lark,
            Name = "Lark",
            Description = "Thief with wizard spell abilities.",
            Requirements = new StatRequirements(MinDexterity: 15, MinIntelligence: 15),
            BaseHitPoints = 90,
            BaseMagicPoints = 35,
            HitPointsPerLevel = 7,
            MagicPointsPerLevel = 4,
            CanUseWizardSpells = true,
            MaxWizardSpellLevel = 8,
            AllowedArmor = LightArmor,
            AllowedWeapons = ThiefWeapons,
            AllowedShields = new HashSet<ShieldType> { ShieldType.SmallShield }
        },
        [CharacterClass.Illusionist] = new ClassDefinition
        {
            Class = CharacterClass.Illusionist,
            Name = "Illusionist",
            Description = "Wizard with thief abilities.",
            Requirements = new StatRequirements(MinDexterity: 15, MinIntelligence: 15),
            BaseHitPoints = 80,
            BaseMagicPoints = 60,
            HitPointsPerLevel = 6,
            MagicPointsPerLevel = 6,
            CanUseWizardSpells = true,
            MaxWizardSpellLevel = 12,
            AllowedArmor = LightArmor,
            AllowedWeapons = ThiefWeapons,
            AllowedShields = new HashSet<ShieldType>()
        },
        [CharacterClass.Druid] = new ClassDefinition
        {
            Class = CharacterClass.Druid,
            Name = "Druid",
            Description = "Nature priest with both wizard and cleric spells.",
            Requirements = new StatRequirements(MinIntelligence: 15, MinWisdom: 15),
            BaseHitPoints = 90,
            BaseMagicPoints = 60,
            HitPointsPerLevel = 8,
            MagicPointsPerLevel = 6,
            CanUseWizardSpells = true,
            CanUseClericSpells = true,
            MaxWizardSpellLevel = 8,
            MaxClericSpellLevel = 8,
            AllowedArmor = LightArmor,
            AllowedWeapons = new HashSet<WeaponType> { WeaponType.Dagger, WeaponType.Staff, WeaponType.Mace },
            AllowedShields = new HashSet<ShieldType> { ShieldType.SmallShield }
        },
        [CharacterClass.Alchemist] = new ClassDefinition
        {
            Class = CharacterClass.Alchemist,
            Name = "Alchemist",
            Description = "Scholar with limited access to both spell types.",
            Requirements = new StatRequirements(MinIntelligence: 10, MinWisdom: 10),
            BaseHitPoints = 85,
            BaseMagicPoints = 40,
            HitPointsPerLevel = 7,
            MagicPointsPerLevel = 4,
            CanUseWizardSpells = true,
            CanUseClericSpells = true,
            MaxWizardSpellLevel = 4,
            MaxClericSpellLevel = 4,
            AllowedArmor = LightArmor,
            AllowedWeapons = SimpleWeapons,
            AllowedShields = new HashSet<ShieldType>()
        },
        [CharacterClass.Ranger] = new ClassDefinition
        {
            Class = CharacterClass.Ranger,
            Name = "Ranger",
            Description = "Wilderness warrior with limited wizard magic.",
            Requirements = new StatRequirements(MinStrength: 10, MinDexterity: 10, MinIntelligence: 10),
            BaseHitPoints = 120,
            BaseMagicPoints = 20,
            HitPointsPerLevel = 10,
            MagicPointsPerLevel = 2,
            CanUseWizardSpells = true,
            MaxWizardSpellLevel = 4,
            AllowedArmor = MediumArmor,
            AllowedWeapons = AllWeapons,
            AllowedShields = AllShields
        }
    };

    public static ClassDefinition Get(CharacterClass charClass) => AllClasses[charClass];

    public bool CanUseArmor(ArmorType armor) =>
        armor == ArmorType.None || AllowedArmor.Contains(armor);

    public bool CanUseWeapon(WeaponType weapon) =>
        weapon == WeaponType.Hands || AllowedWeapons.Contains(weapon);

    public bool CanUseShield(ShieldType shield) =>
        shield == ShieldType.None || AllowedShields.Contains(shield);
}
