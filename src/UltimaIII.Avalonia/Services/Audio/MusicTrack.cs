namespace UltimaIII.Avalonia.Services.Audio;

/// <summary>
/// Background music tracks for different game states.
/// </summary>
public enum MusicTrack
{
    None,
    MainMenu,
    Overworld,
    OverworldNight,
    Town,
    Dungeon,
    Combat,
    Victory,
    Defeat,

    // Per-dungeon exploration themes
    DungeonDoom,
    DungeonDoomDeep,
    DungeonFire,
    DungeonFireDeep,
    DungeonTime,
    DungeonTimeDeep,
    DungeonSnake,
    DungeonSnakeDeep,

    // Per-dungeon combat themes
    CombatDoom,
    CombatFire,
    CombatTime,
    CombatSnake
}
