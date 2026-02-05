using UltimaIII.Core.Enums;

namespace UltimaIII.Core.Models;

/// <summary>
/// Represents a single tile on the map.
/// </summary>
public struct MapTile
{
    public TileType Type { get; set; }
    public bool IsExplored { get; set; }
    public bool IsVisible { get; set; }
    public string? EntityId { get; set; } // NPC, monster, item, etc.
}

/// <summary>
/// A game map (overworld, town, or dungeon level).
/// </summary>
public class GameMap
{
    public string Id { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public MapType MapType { get; init; }
    public int Width { get; init; }
    public int Height { get; init; }
    public int DungeonLevel { get; init; } // For dungeon maps

    private MapTile[,] _tiles;

    public GameMap(int width, int height)
    {
        Width = width;
        Height = height;
        _tiles = new MapTile[width, height];
    }

    public MapTile GetTile(int x, int y)
    {
        if (x < 0 || x >= Width || y < 0 || y >= Height)
            return new MapTile { Type = TileType.Void };
        return _tiles[x, y];
    }

    public void SetTile(int x, int y, MapTile tile)
    {
        if (x < 0 || x >= Width || y < 0 || y >= Height) return;
        _tiles[x, y] = tile;
    }

    public void SetTileType(int x, int y, TileType type)
    {
        if (x < 0 || x >= Width || y < 0 || y >= Height) return;
        _tiles[x, y].Type = type;
    }

    public bool IsPassable(int x, int y, bool hasShip = false)
    {
        var tile = GetTile(x, y);
        if (tile.Type.IsWater())
            return hasShip;
        return tile.Type.IsPassable();
    }

    public bool IsInBounds(int x, int y) =>
        x >= 0 && x < Width && y >= 0 && y < Height;

    // For wrapping maps (like overworld)
    public (int x, int y) WrapCoordinates(int x, int y)
    {
        while (x < 0) x += Width;
        while (y < 0) y += Height;
        return (x % Width, y % Height);
    }

    public void RevealArea(int centerX, int centerY, int radius)
    {
        for (int dy = -radius; dy <= radius; dy++)
        {
            for (int dx = -radius; dx <= radius; dx++)
            {
                int x = centerX + dx;
                int y = centerY + dy;
                if (IsInBounds(x, y))
                {
                    _tiles[x, y].IsExplored = true;
                    _tiles[x, y].IsVisible = true;
                }
            }
        }
    }

    public void ClearVisibility()
    {
        for (int y = 0; y < Height; y++)
        {
            for (int x = 0; x < Width; x++)
            {
                _tiles[x, y].IsVisible = false;
            }
        }
    }

    // List of special locations on this map
    public List<MapLocation> Locations { get; } = new();

    // Entry points for this map
    public Dictionary<string, (int x, int y)> EntryPoints { get; } = new();
}

public enum MapType
{
    Overworld,
    Town,
    Castle,
    Dungeon
}

/// <summary>
/// A special location on a map (town entrance, dungeon, etc.)
/// </summary>
public class MapLocation
{
    public string Id { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public int X { get; init; }
    public int Y { get; init; }
    public string? TargetMapId { get; init; } // Map to load when entering
    public string? TargetEntryPoint { get; init; } // Entry point on target map
    public LocationType Type { get; init; }
}

public enum LocationType
{
    Town,
    Castle,
    Dungeon,
    Shrine,
    StairsUp,
    StairsDown,
    Portal,
    SignPost
}
