using UltimaIII.Core.Enums;

namespace UltimaIII.Core.Models;

/// <summary>
/// Defines the properties of a playable race.
/// </summary>
public class RaceDefinition
{
    public Race Race { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public StatModifiers StatModifiers { get; init; } = new();
    public int MaxAgility { get; init; } = 25;

    public static readonly Dictionary<Race, RaceDefinition> AllRaces = new()
    {
        [Race.Human] = new RaceDefinition
        {
            Race = Race.Human,
            Name = "Human",
            Description = "Balanced race with no modifiers. Versatile and adaptable.",
            StatModifiers = new StatModifiers(0, 0, 0, 0),
            MaxAgility = 25
        },
        [Race.Elf] = new RaceDefinition
        {
            Race = Race.Elf,
            Name = "Elf",
            Description = "Graceful and intelligent, but physically weaker.",
            StatModifiers = new StatModifiers(-2, 2, 2, 0),
            MaxAgility = 25
        },
        [Race.Dwarf] = new RaceDefinition
        {
            Race = Race.Dwarf,
            Name = "Dwarf",
            Description = "Strong and wise, but slower and less nimble.",
            StatModifiers = new StatModifiers(2, -2, 0, 2),
            MaxAgility = 20
        },
        [Race.Bobbit] = new RaceDefinition
        {
            Race = Race.Bobbit,
            Name = "Bobbit",
            Description = "Small folk with great dexterity and wisdom.",
            StatModifiers = new StatModifiers(-3, 2, 0, 3),
            MaxAgility = 25
        },
        [Race.Fuzzy] = new RaceDefinition
        {
            Race = Race.Fuzzy,
            Name = "Fuzzy",
            Description = "Mysterious race with high intelligence.",
            StatModifiers = new StatModifiers(-2, 0, 4, 0),
            MaxAgility = 25
        }
    };

    public static RaceDefinition Get(Race race) => AllRaces[race];
}
