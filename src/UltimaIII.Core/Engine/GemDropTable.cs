using UltimaIII.Core.Enums;

namespace UltimaIII.Core.Engine;

/// <summary>
/// Determines gem drops from monster kills based on dungeon level.
/// Higher dungeon levels yield higher tier gems.
/// </summary>
public static class GemDropTable
{
    // Drop rates: [dungeonLevel] = (chippedChance, flawedChance, perfectChance) — percentages
    private static readonly (int Chipped, int Flawed, int Perfect)[] DropRates =
    {
        (0, 0, 0),    // Level 0 (overworld) — no gems
        (8, 0, 0),    // Level 1
        (12, 4, 0),   // Level 2
        (10, 8, 0),   // Level 3
        (5, 12, 3),   // Level 4
        (0, 15, 5),   // Level 5
        (0, 12, 8),   // Level 6
        (0, 10, 12),  // Level 7
        (0, 8, 15),   // Level 8
    };

    private static readonly GemType[] AllGemTypes = Enum.GetValues<GemType>();

    /// <summary>
    /// Roll for a gem drop from a killed monster.
    /// </summary>
    /// <param name="dungeonLevel">The monster's dungeon level (1-8).</param>
    /// <param name="rng">Random number generator.</param>
    /// <returns>Gem item ID if a gem drops, null otherwise.</returns>
    public static string? RollGemDrop(int dungeonLevel, Random rng)
    {
        if (dungeonLevel < 1 || dungeonLevel > 8)
            return null;

        var (chipped, flawed, perfect) = DropRates[dungeonLevel];

        // Roll once against total drop chance, then pick tier
        int roll = rng.Next(100);

        GemTier? tier = null;
        if (roll < perfect)
            tier = GemTier.Perfect;
        else if (roll < perfect + flawed)
            tier = GemTier.Flawed;
        else if (roll < perfect + flawed + chipped)
            tier = GemTier.Chipped;

        if (tier == null)
            return null;

        // Pick a random gem type
        var gemType = AllGemTypes[rng.Next(AllGemTypes.Length)];
        string tierName = tier.Value.ToString().ToLowerInvariant();
        string typeName = gemType.ToString().ToLowerInvariant();

        return $"gem_{typeName}_{tierName}";
    }
}
