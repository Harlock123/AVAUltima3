namespace UltimaIII.Avalonia.Services.Audio;

/// <summary>
/// Sound effects that can be played as fire-and-forget audio.
/// </summary>
public enum SoundEffect
{
    // Movement
    Footstep,
    Blocked,

    // Doors
    DoorOpen,
    DoorLocked,

    // Combat - Melee
    SwordSwing,
    SwordHit,
    SwordMiss,

    // Combat - Results
    MonsterDeath,
    PlayerHit,

    // Magic
    SpellCast,
    SpellHeal,
    SpellDamage,

    // UI
    MenuSelect,
    MenuConfirm,
    MenuCancel,

    // Pickups & Events
    GoldPickup,
    ItemPickup,
    LevelUp,

    // Environment
    Stairs,
    Teleport
}
