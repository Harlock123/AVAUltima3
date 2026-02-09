using UltimaIII.Core.Enums;
using UltimaIII.Core.Models;

namespace UltimaIII.Core.Engine;

/// <summary>
/// Generates random valid NPCs for tavern recruitment.
/// </summary>
public static class NpcGenerator
{
    private static readonly string[] Names =
    {
        "Aldric", "Brenna", "Calder", "Dahlia", "Eldric",
        "Fiona", "Gareth", "Helena", "Ivor", "Jocelyn",
        "Kael", "Lyra", "Magnus", "Nadia", "Osric",
        "Petra", "Quillan", "Rowan", "Sybil", "Torin",
        "Ulara", "Valen", "Wren", "Xara", "Yoren", "Zelda"
    };

    public static Character GenerateNpc(Random rng)
    {
        var name = Names[rng.Next(Names.Length)];

        var races = Enum.GetValues<Race>();
        var classes = Enum.GetValues<CharacterClass>();

        var race = races[rng.Next(races.Length)];
        var charClass = classes[rng.Next(classes.Length)];

        var raceDef = RaceDefinition.Get(race);
        var classDef = ClassDefinition.Get(charClass);
        var reqs = classDef.Requirements;
        var mods = raceDef.StatModifiers;

        // Compute minimum base stats needed so that after racial modifiers, class requirements are met
        int minStr = Math.Clamp(reqs.MinStrength - mods.StrengthMod, Stats.MinStat, Stats.MaxStat);
        int minDex = Math.Clamp(reqs.MinDexterity - mods.DexterityMod, Stats.MinStat, Stats.MaxStat);
        int minInt = Math.Clamp(reqs.MinIntelligence - mods.IntelligenceMod, Stats.MinStat, Stats.MaxStat);
        int minWis = Math.Clamp(reqs.MinWisdom - mods.WisdomMod, Stats.MinStat, Stats.MaxStat);

        int usedPoints = minStr + minDex + minInt + minWis;
        int remaining = Stats.StartingStatPoints - usedPoints;

        // Distribute remaining points randomly across 4 stats
        int[] bonus = new int[4];
        for (int i = 0; i < remaining; i++)
        {
            bonus[rng.Next(4)]++;
        }

        var baseStats = new Stats(
            Math.Min(minStr + bonus[0], Stats.MaxStat),
            Math.Min(minDex + bonus[1], Stats.MaxStat),
            Math.Min(minInt + bonus[2], Stats.MaxStat),
            Math.Min(minWis + bonus[3], Stats.MaxStat)
        );

        return Character.Create(name, race, charClass, baseStats);
    }
}
