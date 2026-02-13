namespace UltimaIII.Core.Enums;

/// <summary>
/// Types of terrain tiles in the game world.
/// </summary>
public enum TileType
{
    // Terrain
    Grass,
    Forest,
    Mountain,
    Water,
    DeepWater,
    Swamp,
    Desert,
    Lava,

    // Structures
    Wall,
    Floor,
    Door,
    LockedDoor,
    SecretDoor,
    StairsUp,
    StairsDown,
    Altar,
    Fountain,
    Sign,

    // Town features
    Counter,
    Chest,
    Path,
    Flowers,
    Lamppost,

    // Special
    CastleWall,
    CastleFloor,
    Bridge,
    Ladder,
    Portal,

    // Dungeon specific
    Pit,
    CeilingHole,
    Trap,

    // Overworld locations
    Town,

    // Impassable
    Void
}

public static class TileTypeExtensions
{
    public static bool IsPassable(this TileType tile) => tile switch
    {
        TileType.Grass => true,
        TileType.Forest => true,
        TileType.Swamp => true,
        TileType.Desert => true,
        TileType.Floor => true,
        TileType.Door => true,
        TileType.StairsUp => true,
        TileType.StairsDown => true,
        TileType.Altar => true,
        TileType.Fountain => true,
        TileType.Sign => true,
        TileType.Chest => true,
        TileType.Path => true,
        TileType.CastleFloor => true,
        TileType.Bridge => true,
        TileType.Ladder => true,
        TileType.Portal => true,
        TileType.Town => true,
        _ => false
    };

    public static bool IsWater(this TileType tile) =>
        tile == TileType.Water || tile == TileType.DeepWater;

    public static bool RequiresShip(this TileType tile) => tile.IsWater();

    public static bool BlocksVision(this TileType tile) => tile switch
    {
        TileType.Wall => true,
        TileType.Mountain => true,
        TileType.Forest => true,
        TileType.CastleWall => true,
        TileType.SecretDoor => true,
        _ => false
    };

    public static bool IsDangerous(this TileType tile) => tile switch
    {
        TileType.Swamp => true,
        TileType.Lava => true,
        TileType.Trap => true,
        TileType.Pit => true,
        _ => false
    };
}
