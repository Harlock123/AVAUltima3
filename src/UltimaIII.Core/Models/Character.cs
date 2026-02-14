using UltimaIII.Core.Enums;

namespace UltimaIII.Core.Models;

/// <summary>
/// A player character in the party.
/// </summary>
public class Character
{
    public string Name { get; set; } = string.Empty;
    public Race Race { get; set; }
    public CharacterClass Class { get; set; }

    public Stats Stats { get; set; } = new();

    private int _currentHp;
    private int _currentMp;
    private int _maxHp;
    private int _maxMp;

    public int MaxHP
    {
        get => _maxHp;
        set => _maxHp = Math.Max(1, value);
    }

    public int CurrentHP
    {
        get => _currentHp;
        set => _currentHp = Math.Clamp(value, 0, MaxHP);
    }

    public int MaxMP
    {
        get => _maxMp;
        set => _maxMp = Math.Max(0, value);
    }

    public int CurrentMP
    {
        get => _currentMp;
        set => _currentMp = Math.Clamp(value, 0, MaxMP);
    }

    public int Level { get; set; } = 1;
    public int Experience { get; set; } = 0;
    public StatusEffect Status { get; set; } = StatusEffect.None;

    // Equipment slots
    public Weapon? EquippedWeapon { get; set; }
    public Armor? EquippedArmor { get; set; }
    public Shield? EquippedShield { get; set; }

    // Inventory (shared with party in original, but tracked per character)
    public List<Item> Inventory { get; } = new();

    // Combat position
    public int CombatX { get; set; }
    public int CombatY { get; set; }

    public bool IsAlive => !Status.HasFlag(StatusEffect.Dead) && CurrentHP > 0;
    public bool CanAct => IsAlive &&
                          !Status.HasFlag(StatusEffect.Asleep) &&
                          !Status.HasFlag(StatusEffect.Paralyzed) &&
                          !Status.HasFlag(StatusEffect.Petrified);

    public ClassDefinition ClassDef => ClassDefinition.Get(Class);
    public RaceDefinition RaceDef => RaceDefinition.Get(Race);

    public void Initialize()
    {
        var classDef = ClassDef;

        // Calculate initial HP and MP
        MaxHP = classDef.BaseHitPoints + (Stats.Strength / 2);
        CurrentHP = MaxHP;
        MaxMP = classDef.BaseMagicPoints + (Stats.Intelligence + Stats.Wisdom) / 4;
        CurrentMP = MaxMP;

        // Set default equipment
        EquippedWeapon = Weapon.Hands;
        EquippedArmor = Armor.None;
        EquippedShield = Shield.None;
    }

    public int GetAttackBonus()
    {
        int bonus = Stats.Strength / 3;
        if (EquippedWeapon != null)
            bonus += EquippedWeapon.HitBonus;

        // Class-specific attack bonus
        bonus += Class switch
        {
            CharacterClass.Fighter => 2 + Level / 4,
            CharacterClass.Thief => 3 + Level / 3,
            CharacterClass.Paladin => 1 + Level / 5,
            CharacterClass.Lark => 1 + Level / 5,
            CharacterClass.Ranger => 1 + Level / 5,
            CharacterClass.Barbarian => 1,
            CharacterClass.Wizard => -2,
            CharacterClass.Illusionist => -1,
            CharacterClass.Alchemist => -1,
            _ => 0
        };

        return bonus;
    }

    public int GetAttackBonusVsTarget(MonsterDefinition? target)
    {
        if (target == null) return 0;

        return Class switch
        {
            CharacterClass.Cleric when target.IsUndead => 2,
            CharacterClass.Paladin when target.IsUndead || target.IsDemon => 3,
            CharacterClass.Druid when target.IsDemon => 2,
            _ => 0
        };
    }

    public int GetDefense()
    {
        int defense = Stats.Dexterity / 4;
        if (EquippedArmor != null)
            defense += EquippedArmor.Defense;
        if (EquippedShield != null)
            defense += EquippedShield.Defense;

        // Class-specific defense bonus
        defense += Class switch
        {
            CharacterClass.Thief => 2,
            CharacterClass.Paladin => 2,
            CharacterClass.Fighter => 1,
            CharacterClass.Lark => 1,
            CharacterClass.Illusionist => 1,
            CharacterClass.Druid => 1,
            CharacterClass.Ranger => 1,
            CharacterClass.Cleric => 1 + Level / 5,
            _ => 0
        };

        return defense;
    }

    public int RollDamage(Random rng, MonsterDefinition? target = null)
    {
        var weapon = EquippedWeapon ?? Weapon.Hands;
        int baseDamage = weapon.RollDamage(rng);
        int strBonus = Stats.Strength / 4;
        int total = baseDamage + strBonus;

        // Class-specific damage bonus
        total += Class switch
        {
            CharacterClass.Fighter => 2 + Level / 5,
            CharacterClass.Barbarian => 3 + Level / 4,
            CharacterClass.Paladin => 1,
            CharacterClass.Ranger => 1,
            CharacterClass.Wizard => -1,
            CharacterClass.Illusionist => -1,
            _ => 0
        };

        // Barbarian Rage: +1 damage per 25% HP missing
        if (Class == CharacterClass.Barbarian && MaxHP > 0)
        {
            int missingPercent = ((MaxHP - CurrentHP) * 100) / MaxHP;
            total += missingPercent / 25;
        }

        // Creature-type damage bonuses
        if (target != null)
        {
            if (Class == CharacterClass.Cleric && target.IsUndead)
                total += 2;
            if (Class == CharacterClass.Paladin && (target.IsUndead || target.IsDemon))
                total += 3;
            if (Class == CharacterClass.Druid && target.IsDemon)
                total += 2;
        }

        return Math.Max(1, total);
    }

    public int GetCriticalHitChance()
    {
        return Class switch
        {
            CharacterClass.Thief => 15 + Level,
            CharacterClass.Lark => 10 + Level / 2,
            _ => 0
        };
    }

    public int GetWeaponRange()
    {
        int range = EquippedWeapon?.Range ?? 1;

        // Ranger bonus: +1 range
        if (Class == CharacterClass.Ranger)
            range += 1;

        return range;
    }

    public void TakeDamage(int damage)
    {
        CurrentHP -= damage;
        if (CurrentHP <= 0)
        {
            CurrentHP = 0;
            Status |= StatusEffect.Dead;
        }
    }

    public void Heal(int amount)
    {
        if (!IsAlive) return;
        CurrentHP = Math.Min(CurrentHP + amount, MaxHP);
    }

    public void RestoreMana(int amount)
    {
        if (!IsAlive) return;
        CurrentMP = Math.Min(CurrentMP + amount, MaxMP);
    }

    public bool SpendMana(int cost)
    {
        if (CurrentMP < cost) return false;
        CurrentMP -= cost;
        return true;
    }

    public int ExperienceForNextLevel => Level * 100;

    public bool CanLevelUp => Experience >= ExperienceForNextLevel;

    public void LevelUp()
    {
        if (!CanLevelUp) return;

        Experience -= ExperienceForNextLevel;
        Level++;

        var classDef = ClassDef;
        MaxHP += classDef.HitPointsPerLevel;
        MaxMP += classDef.MagicPointsPerLevel;
        CurrentHP = MaxHP;
        CurrentMP = MaxMP;

        // Stat growth based on class archetype (+1 to primary stats every level)
        switch (Class)
        {
            case CharacterClass.Fighter:
                Stats.Strength += 1;
                break;
            case CharacterClass.Barbarian:
                Stats.Strength += 1;
                break;
            case CharacterClass.Wizard:
                Stats.Intelligence += 1;
                break;
            case CharacterClass.Cleric:
                Stats.Wisdom += 1;
                break;
            case CharacterClass.Thief:
                Stats.Dexterity += 1;
                break;
            case CharacterClass.Paladin:
                Stats.Strength += 1;
                if (Level % 2 == 0) Stats.Wisdom += 1;
                break;
            case CharacterClass.Ranger:
                Stats.Dexterity += 1;
                if (Level % 2 == 0) Stats.Strength += 1;
                break;
            case CharacterClass.Lark:
                Stats.Dexterity += 1;
                if (Level % 2 == 0) Stats.Intelligence += 1;
                break;
            case CharacterClass.Illusionist:
                Stats.Intelligence += 1;
                if (Level % 2 == 0) Stats.Dexterity += 1;
                break;
            case CharacterClass.Druid:
                Stats.Wisdom += 1;
                if (Level % 2 == 0) Stats.Intelligence += 1;
                break;
            case CharacterClass.Alchemist:
                Stats.Intelligence += 1;
                if (Level % 2 == 0) Stats.Wisdom += 1;
                break;
        }
    }

    public void GainExperience(int exp)
    {
        Experience += exp;
        while (CanLevelUp)
        {
            LevelUp();
        }
    }

    public static Character Create(string name, Race race, CharacterClass charClass, Stats baseStats)
    {
        var character = new Character
        {
            Name = name,
            Race = race,
            Class = charClass,
            Stats = baseStats.Clone()
        };

        // Apply racial modifiers
        character.Stats.ApplyModifiers(RaceDefinition.Get(race).StatModifiers);

        character.Initialize();
        return character;
    }
}
