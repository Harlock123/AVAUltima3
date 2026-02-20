using UltimaIII.Core.Enums;

namespace UltimaIII.Core.Models;

/// <summary>
/// Defines a spell and its effects.
/// </summary>
public class Spell
{
    public SpellType Type { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public SpellSchool School { get; init; }
    public int Level { get; init; } // 1-16 (corresponds to A-P)
    public int ManaCost { get; init; }
    public int MinDamage { get; init; }
    public int MaxDamage { get; init; }
    public int HealAmount { get; init; }
    public int Range { get; init; } = 1;
    public int AreaOfEffect { get; init; } = 0; // 0 = single target
    public bool TargetsSelf { get; init; }
    public bool TargetsParty { get; init; }
    public bool TargetsEnemy { get; init; }
    public StatusEffect AppliesStatus { get; init; }
    public StatusEffect CuresStatus { get; init; }
    public bool IsCombatOnly { get; init; }
    public bool IsFieldOnly { get; init; }

    public static readonly Dictionary<SpellType, Spell> AllSpells = new()
    {
        // Wizard Spells
        [SpellType.Repond] = new Spell
        {
            Type = SpellType.Repond,
            Name = "Repond",
            Description = "Repels undead creatures",
            School = SpellSchool.Wizard,
            Level = 1,
            ManaCost = 5,
            TargetsEnemy = true,
            IsCombatOnly = true
        },
        [SpellType.Mittar] = new Spell
        {
            Type = SpellType.Mittar,
            Name = "Mittar",
            Description = "Magic missile attack",
            School = SpellSchool.Wizard,
            Level = 2,
            ManaCost = 5,
            MinDamage = 10,
            MaxDamage = 20,
            Range = 5,
            TargetsEnemy = true,
            IsCombatOnly = true
        },
        [SpellType.Lorum] = new Spell
        {
            Type = SpellType.Lorum,
            Name = "Lorum",
            Description = "Creates magical light",
            School = SpellSchool.Wizard,
            Level = 3,
            ManaCost = 3,
            TargetsSelf = true
        },
        [SpellType.Dum_Dum] = new Spell
        {
            Type = SpellType.Dum_Dum,
            Name = "Dum Dum",
            Description = "Weakens enemy defenses",
            School = SpellSchool.Wizard,
            Level = 4,
            ManaCost = 8,
            TargetsEnemy = true,
            IsCombatOnly = true
        },
        [SpellType.Fulgar] = new Spell
        {
            Type = SpellType.Fulgar,
            Name = "Fulgar",
            Description = "Lightning bolt attack",
            School = SpellSchool.Wizard,
            Level = 5,
            ManaCost = 10,
            MinDamage = 20,
            MaxDamage = 40,
            Range = 6,
            TargetsEnemy = true,
            IsCombatOnly = true
        },
        [SpellType.Dag_Acron] = new Spell
        {
            Type = SpellType.Dag_Acron,
            Name = "Dag Acron",
            Description = "Creates a magical trap",
            School = SpellSchool.Wizard,
            Level = 6,
            ManaCost = 12,
            MinDamage = 15,
            MaxDamage = 30,
            IsCombatOnly = true
        },
        [SpellType.Mentar] = new Spell
        {
            Type = SpellType.Mentar,
            Name = "Mentar",
            Description = "Mental attack",
            School = SpellSchool.Wizard,
            Level = 7,
            ManaCost = 15,
            MinDamage = 25,
            MaxDamage = 45,
            Range = 4,
            TargetsEnemy = true,
            IsCombatOnly = true
        },
        [SpellType.Dag_Lull] = new Spell
        {
            Type = SpellType.Dag_Lull,
            Name = "Dag Lull",
            Description = "Dispels magical effects",
            School = SpellSchool.Wizard,
            Level = 8,
            ManaCost = 10,
            TargetsEnemy = true
        },
        [SpellType.Fal_Divi] = new Spell
        {
            Type = SpellType.Fal_Divi,
            Name = "Fal Divi",
            Description = "Locates nearby enemies",
            School = SpellSchool.Wizard,
            Level = 9,
            ManaCost = 8,
            TargetsSelf = true
        },
        [SpellType.Noxum] = new Spell
        {
            Type = SpellType.Noxum,
            Name = "Noxum",
            Description = "Creates poison cloud",
            School = SpellSchool.Wizard,
            Level = 10,
            ManaCost = 18,
            MinDamage = 15,
            MaxDamage = 25,
            AreaOfEffect = 2,
            AppliesStatus = StatusEffect.Poisoned,
            TargetsEnemy = true,
            IsCombatOnly = true
        },
        [SpellType.Decorp] = new Spell
        {
            Type = SpellType.Decorp,
            Name = "Decorp",
            Description = "Destroys undead",
            School = SpellSchool.Wizard,
            Level = 11,
            ManaCost = 20,
            MinDamage = 50,
            MaxDamage = 100,
            TargetsEnemy = true,
            IsCombatOnly = true
        },
        [SpellType.Altair] = new Spell
        {
            Type = SpellType.Altair,
            Name = "Altair",
            Description = "Teleport short distance",
            School = SpellSchool.Wizard,
            Level = 12,
            ManaCost = 15,
            Range = 3,
            TargetsSelf = true
        },
        [SpellType.Dag_Mentar] = new Spell
        {
            Type = SpellType.Dag_Mentar,
            Name = "Dag Mentar",
            Description = "Confuses enemies",
            School = SpellSchool.Wizard,
            Level = 13,
            ManaCost = 22,
            AppliesStatus = StatusEffect.Confused,
            AreaOfEffect = 2,
            TargetsEnemy = true,
            IsCombatOnly = true
        },
        [SpellType.Necorp] = new Spell
        {
            Type = SpellType.Necorp,
            Name = "Necorp",
            Description = "Stops time briefly",
            School = SpellSchool.Wizard,
            Level = 14,
            ManaCost = 25,
            IsCombatOnly = true
        },
        [SpellType.Malor] = new Spell
        {
            Type = SpellType.Malor,
            Name = "Malor",
            Description = "Great teleportation",
            School = SpellSchool.Wizard,
            Level = 15,
            ManaCost = 30,
            Range = 10,
            TargetsSelf = true
        },
        [SpellType.Anju_Sermani] = new Spell
        {
            Type = SpellType.Anju_Sermani,
            Name = "Anju Sermani",
            Description = "Instant death spell",
            School = SpellSchool.Wizard,
            Level = 16,
            ManaCost = 40,
            MinDamage = 999,
            MaxDamage = 999,
            TargetsEnemy = true,
            IsCombatOnly = true
        },

        // Wizard AOE Spells (expanded)
        [SpellType.Vas_Flam] = new Spell
        {
            Type = SpellType.Vas_Flam,
            Name = "Vas Flam",
            Description = "Hurls an exploding fireball",
            School = SpellSchool.Wizard,
            Level = 6,
            ManaCost = 14,
            MinDamage = 20,
            MaxDamage = 35,
            Range = 5,
            AreaOfEffect = 2,
            TargetsEnemy = true,
            IsCombatOnly = true
        },
        [SpellType.Grav_Por] = new Spell
        {
            Type = SpellType.Grav_Por,
            Name = "Grav Por",
            Description = "Unleashes a storm of ice shards",
            School = SpellSchool.Wizard,
            Level = 9,
            ManaCost = 20,
            MinDamage = 25,
            MaxDamage = 40,
            Range = 5,
            AreaOfEffect = 2,
            TargetsEnemy = true,
            IsCombatOnly = true
        },
        [SpellType.Xen_Ang] = new Spell
        {
            Type = SpellType.Xen_Ang,
            Name = "Xen Ang",
            Description = "Arcs lightning across a wide area",
            School = SpellSchool.Wizard,
            Level = 14,
            ManaCost = 30,
            MinDamage = 35,
            MaxDamage = 60,
            Range = 6,
            AreaOfEffect = 3,
            TargetsEnemy = true,
            IsCombatOnly = true
        },

        // Cleric Spells
        [SpellType.Pontori] = new Spell
        {
            Type = SpellType.Pontori,
            Name = "Pontori",
            Description = "Light healing",
            School = SpellSchool.Cleric,
            Level = 1,
            ManaCost = 5,
            HealAmount = 25,
            TargetsSelf = true,
            TargetsParty = true
        },
        [SpellType.Appar_Unem] = new Spell
        {
            Type = SpellType.Appar_Unem,
            Name = "Appar Unem",
            Description = "Protection from undead",
            School = SpellSchool.Cleric,
            Level = 2,
            ManaCost = 6,
            TargetsSelf = true
        },
        [SpellType.Sanctu] = new Spell
        {
            Type = SpellType.Sanctu,
            Name = "Sanctu",
            Description = "Cures poison",
            School = SpellSchool.Cleric,
            Level = 3,
            ManaCost = 8,
            CuresStatus = StatusEffect.Poisoned,
            TargetsSelf = true,
            TargetsParty = true
        },
        [SpellType.Luminae] = new Spell
        {
            Type = SpellType.Luminae,
            Name = "Luminae",
            Description = "Holy light",
            School = SpellSchool.Cleric,
            Level = 4,
            ManaCost = 4,
            TargetsSelf = true
        },
        [SpellType.Rec_Su] = new Spell
        {
            Type = SpellType.Rec_Su,
            Name = "Rec Su",
            Description = "Resurrect fallen ally",
            School = SpellSchool.Cleric,
            Level = 5,
            ManaCost = 50,
            CuresStatus = StatusEffect.Dead,
            TargetsParty = true
        },
        [SpellType.Lib_Rec] = new Spell
        {
            Type = SpellType.Lib_Rec,
            Name = "Lib Rec",
            Description = "Removes all ailments",
            School = SpellSchool.Cleric,
            Level = 6,
            ManaCost = 15,
            CuresStatus = StatusEffect.Poisoned | StatusEffect.Confused | StatusEffect.Asleep | StatusEffect.Paralyzed,
            TargetsParty = true
        },
        [SpellType.Alcort] = new Spell
        {
            Type = SpellType.Alcort,
            Name = "Alcort",
            Description = "Holy lance attack",
            School = SpellSchool.Cleric,
            Level = 7,
            ManaCost = 12,
            MinDamage = 20,
            MaxDamage = 35,
            Range = 4,
            TargetsEnemy = true,
            IsCombatOnly = true
        },
        [SpellType.Sequitu] = new Spell
        {
            Type = SpellType.Sequitu,
            Name = "Sequitu",
            Description = "Paralyzes enemy",
            School = SpellSchool.Cleric,
            Level = 8,
            ManaCost = 15,
            AppliesStatus = StatusEffect.Paralyzed,
            TargetsEnemy = true,
            IsCombatOnly = true
        },
        [SpellType.Sominae] = new Spell
        {
            Type = SpellType.Sominae,
            Name = "Sominae",
            Description = "Puts enemies to sleep",
            School = SpellSchool.Cleric,
            Level = 9,
            ManaCost = 12,
            AppliesStatus = StatusEffect.Asleep,
            AreaOfEffect = 2,
            TargetsEnemy = true,
            IsCombatOnly = true
        },
        [SpellType.Sanctu_Mani] = new Spell
        {
            Type = SpellType.Sanctu_Mani,
            Name = "Sanctu Mani",
            Description = "Great protection",
            School = SpellSchool.Cleric,
            Level = 10,
            ManaCost = 20,
            TargetsParty = true
        },
        [SpellType.Vieda] = new Spell
        {
            Type = SpellType.Vieda,
            Name = "Vieda",
            Description = "Reveals hidden things",
            School = SpellSchool.Cleric,
            Level = 11,
            ManaCost = 10,
            TargetsSelf = true
        },
        [SpellType.Exorcism] = new Spell
        {
            Type = SpellType.Exorcism,
            Name = "Exorcism",
            Description = "Banishes demons",
            School = SpellSchool.Cleric,
            Level = 12,
            ManaCost = 25,
            MinDamage = 75,
            MaxDamage = 150,
            TargetsEnemy = true,
            IsCombatOnly = true
        },
        [SpellType.Zxkuqyb] = new Spell
        {
            Type = SpellType.Zxkuqyb,
            Name = "Zxkuqyb",
            Description = "Death touch",
            School = SpellSchool.Cleric,
            Level = 13,
            ManaCost = 35,
            MinDamage = 999,
            MaxDamage = 999,
            TargetsEnemy = true,
            IsCombatOnly = true
        },
        [SpellType.Xen_Corp] = new Spell
        {
            Type = SpellType.Xen_Corp,
            Name = "Xen Corp",
            Description = "Mass healing",
            School = SpellSchool.Cleric,
            Level = 14,
            ManaCost = 30,
            HealAmount = 50,
            TargetsParty = true
        },
        [SpellType.Yaulp] = new Spell
        {
            Type = SpellType.Yaulp,
            Name = "Yaulp",
            Description = "Divine power boost",
            School = SpellSchool.Cleric,
            Level = 15,
            ManaCost = 25,
            TargetsSelf = true
        },
        [SpellType.Vas_Mani] = new Spell
        {
            Type = SpellType.Vas_Mani,
            Name = "Vas Mani",
            Description = "Full healing",
            School = SpellSchool.Cleric,
            Level = 16,
            ManaCost = 45,
            HealAmount = 999,
            TargetsParty = true
        },

        // Cleric AOE/Healing Spells (expanded)
        [SpellType.Mani] = new Spell
        {
            Type = SpellType.Mani,
            Name = "Mani",
            Description = "Moderate healing",
            School = SpellSchool.Cleric,
            Level = 4,
            ManaCost = 10,
            HealAmount = 40,
            TargetsSelf = true,
            TargetsParty = true
        },
        [SpellType.Vas_Sanct] = new Spell
        {
            Type = SpellType.Vas_Sanct,
            Name = "Vas Sanct",
            Description = "Erupts in a burst of holy light",
            School = SpellSchool.Cleric,
            Level = 7,
            ManaCost = 16,
            MinDamage = 20,
            MaxDamage = 35,
            Range = 4,
            AreaOfEffect = 2,
            TargetsEnemy = true,
            IsCombatOnly = true
        },
        [SpellType.An_Corp] = new Spell
        {
            Type = SpellType.An_Corp,
            Name = "An Corp",
            Description = "Calls divine judgment upon foes",
            School = SpellSchool.Cleric,
            Level = 12,
            ManaCost = 28,
            MinDamage = 30,
            MaxDamage = 50,
            Range = 5,
            AreaOfEffect = 2,
            TargetsEnemy = true,
            IsCombatOnly = true
        }
    };

    public static Spell Get(SpellType type) => AllSpells[type];

    public static IEnumerable<Spell> GetSpellsForClass(ClassDefinition classDef)
    {
        foreach (var spell in AllSpells.Values)
        {
            if (spell.School == SpellSchool.Wizard && classDef.CanUseWizardSpells &&
                spell.Level <= classDef.MaxWizardSpellLevel)
            {
                yield return spell;
            }
            else if (spell.School == SpellSchool.Cleric && classDef.CanUseClericSpells &&
                     spell.Level <= classDef.MaxClericSpellLevel)
            {
                yield return spell;
            }
        }
    }

    public int RollDamage(Random rng)
    {
        if (MinDamage == 0 && MaxDamage == 0) return 0;
        return rng.Next(MinDamage, MaxDamage + 1);
    }
}
