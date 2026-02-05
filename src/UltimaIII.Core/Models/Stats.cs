namespace UltimaIII.Core.Models;

/// <summary>
/// Character statistics.
/// </summary>
public class Stats
{
    public const int MinStat = 3;
    public const int MaxStat = 25;
    public const int StartingStatPoints = 50;

    private int _strength;
    private int _dexterity;
    private int _intelligence;
    private int _wisdom;

    public int Strength
    {
        get => _strength;
        set => _strength = Math.Clamp(value, MinStat, MaxStat);
    }

    public int Dexterity
    {
        get => _dexterity;
        set => _dexterity = Math.Clamp(value, MinStat, MaxStat);
    }

    public int Intelligence
    {
        get => _intelligence;
        set => _intelligence = Math.Clamp(value, MinStat, MaxStat);
    }

    public int Wisdom
    {
        get => _wisdom;
        set => _wisdom = Math.Clamp(value, MinStat, MaxStat);
    }

    public int Total => Strength + Dexterity + Intelligence + Wisdom;

    public Stats()
    {
        _strength = MinStat;
        _dexterity = MinStat;
        _intelligence = MinStat;
        _wisdom = MinStat;
    }

    public Stats(int str, int dex, int intel, int wis)
    {
        Strength = str;
        Dexterity = dex;
        Intelligence = intel;
        Wisdom = wis;
    }

    public Stats Clone() => new(Strength, Dexterity, Intelligence, Wisdom);

    public void ApplyModifiers(StatModifiers modifiers)
    {
        Strength += modifiers.StrengthMod;
        Dexterity += modifiers.DexterityMod;
        Intelligence += modifiers.IntelligenceMod;
        Wisdom += modifiers.WisdomMod;
    }
}

/// <summary>
/// Stat modifiers applied by race.
/// </summary>
public record StatModifiers(
    int StrengthMod = 0,
    int DexterityMod = 0,
    int IntelligenceMod = 0,
    int WisdomMod = 0
);

/// <summary>
/// Stat requirements for a class.
/// </summary>
public record StatRequirements(
    int MinStrength = 0,
    int MinDexterity = 0,
    int MinIntelligence = 0,
    int MinWisdom = 0
)
{
    public bool MeetsRequirements(Stats stats) =>
        stats.Strength >= MinStrength &&
        stats.Dexterity >= MinDexterity &&
        stats.Intelligence >= MinIntelligence &&
        stats.Wisdom >= MinWisdom;
}
