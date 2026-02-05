namespace UltimaIII.Core.Enums;

/// <summary>
/// Status effects that can affect characters.
/// </summary>
[Flags]
public enum StatusEffect
{
    None = 0,
    Poisoned = 1 << 0,
    Asleep = 1 << 1,
    Paralyzed = 1 << 2,
    Dead = 1 << 3,
    Petrified = 1 << 4,
    Confused = 1 << 5
}
