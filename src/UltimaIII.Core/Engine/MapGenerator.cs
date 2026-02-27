using UltimaIII.Core.Enums;
using UltimaIII.Core.Models;

namespace UltimaIII.Core.Engine;

/// <summary>
/// Procedural map generation for Ultima III.
/// </summary>
public static class MapGenerator
{
    private const int OverworldSize = 64;
    private const int TownSize = 32;
    private const int DungeonSize = 32;

    public static GameMap GenerateOverworld(Random rng)
    {
        var map = new GameMap(OverworldSize, OverworldSize)
        {
            Id = "overworld",
            Name = "Sosaria",
            MapType = MapType.Overworld
        };

        // Generate base terrain using simple noise
        float[,] heightMap = GenerateNoise(OverworldSize, OverworldSize, rng, 4);
        float[,] moistureMap = GenerateNoise(OverworldSize, OverworldSize, rng, 3);

        for (int y = 0; y < OverworldSize; y++)
        {
            for (int x = 0; x < OverworldSize; x++)
            {
                float height = heightMap[x, y];
                float moisture = moistureMap[x, y];

                TileType type;
                if (height < 0.3f)
                    type = height < 0.2f ? TileType.DeepWater : TileType.Water;
                else if (height < 0.4f)
                    type = TileType.Grass;
                else if (height < 0.55f)
                    type = moisture > 0.5f ? TileType.Forest : TileType.Grass;
                else if (height < 0.7f)
                    type = moisture < 0.3f ? TileType.Desert : TileType.Forest;
                else if (height < 0.85f)
                    type = TileType.Mountain;
                else
                    type = TileType.Mountain;

                // Add some swamps near water
                if (type == TileType.Grass && height < 0.45f && moisture > 0.6f && rng.Next(100) < 30)
                    type = TileType.Swamp;

                map.SetTile(x, y, new MapTile { Type = type });
            }
        }

        // Ensure starting area is accessible (create a clearing around center)
        int centerX = OverworldSize / 2;
        int centerY = OverworldSize / 2;
        for (int dy = -5; dy <= 5; dy++)
        {
            for (int dx = -5; dx <= 5; dx++)
            {
                int x = centerX + dx;
                int y = centerY + dy;
                if (x >= 0 && x < OverworldSize && y >= 0 && y < OverworldSize)
                {
                    map.SetTile(x, y, new MapTile { Type = TileType.Grass });
                }
            }
        }

        // Add town locations
        AddOverworldLocations(map, rng);

        return map;
    }

    private static void AddOverworldLocations(GameMap map, Random rng)
    {
        var towns = new[]
        {
            ("britain", "Britain", 32, 32),
            ("yew", "Yew", 15, 20),
            ("montor", "Montor", 50, 15),
            ("moon", "Moon", 48, 45),
            ("grey", "Grey", 20, 50),
            ("death_gulch", "Death Gulch", 55, 55),
            ("devil_guard", "Devil Guard", 10, 45),
            ("fawn", "Fawn", 40, 25)
        };

        foreach (var (id, name, x, y) in towns)
        {
            // Clear area for town
            for (int dy = -1; dy <= 1; dy++)
            {
                for (int dx = -1; dx <= 1; dx++)
                {
                    map.SetTile(x + dx, y + dy, new MapTile { Type = TileType.Grass });
                }
            }
            map.SetTile(x, y, new MapTile { Type = TileType.Town });

            map.Locations.Add(new MapLocation
            {
                Id = id,
                Name = name,
                X = x,
                Y = y,
                TargetMapId = id,
                TargetEntryPoint = "default",
                Type = LocationType.Town
            });
        }

        // Add dungeon entrances
        var dungeons = new[]
        {
            ("dungeon_doom_l1", "Dungeon of Doom", 8, 8),
            ("dungeon_fire_l1", "Dungeon of Fire", 55, 8),
            ("dungeon_time_l1", "Dungeon of Time", 8, 55),
            ("dungeon_snake_l1", "Dungeon of the Snake", 55, 50)
        };

        foreach (var (id, name, x, y) in dungeons)
        {
            // Clear area and add dungeon entrance
            map.SetTile(x, y, new MapTile { Type = TileType.DungeonEntrance });
            map.SetTile(x - 1, y, new MapTile { Type = TileType.Mountain });
            map.SetTile(x + 1, y, new MapTile { Type = TileType.Mountain });

            map.Locations.Add(new MapLocation
            {
                Id = id,
                Name = name,
                X = x,
                Y = y,
                TargetMapId = id,
                TargetEntryPoint = "default",
                Type = LocationType.Dungeon
            });
        }
    }

    public static GameMap GenerateTown(Random rng, string id, string name)
    {
        var map = new GameMap(TownSize, TownSize)
        {
            Id = id,
            Name = name,
            MapType = MapType.Town
        };

        // Fill with grass
        for (int y = 0; y < TownSize; y++)
        {
            for (int x = 0; x < TownSize; x++)
            {
                map.SetTile(x, y, new MapTile { Type = TileType.Grass });
            }
        }

        // Create town walls
        for (int x = 0; x < TownSize; x++)
        {
            map.SetTile(x, 0, new MapTile { Type = TileType.CastleWall });
            map.SetTile(x, TownSize - 1, new MapTile { Type = TileType.CastleWall });
        }
        for (int y = 0; y < TownSize; y++)
        {
            map.SetTile(0, y, new MapTile { Type = TileType.CastleWall });
            map.SetTile(TownSize - 1, y, new MapTile { Type = TileType.CastleWall });
        }

        // Create entrance
        int entranceX = TownSize / 2;
        map.SetTile(entranceX, TownSize - 1, new MapTile { Type = TileType.Door });
        map.EntryPoints["default"] = (entranceX, TownSize - 2);

        // Generate buildings
        GenerateTownBuildings(map, rng);

        // Add some trees in remaining grass areas
        for (int i = 0; i < 10; i++)
        {
            int x = rng.Next(2, TownSize - 2);
            int y = rng.Next(2, TownSize - 2);
            var tile = map.GetTile(x, y);
            if (tile.Type == TileType.Grass)
            {
                map.SetTile(x, y, new MapTile { Type = TileType.Forest });
            }
        }

        // Place quest NPC for this town
        var townNpcs = QuestRegistry.GetNpcsForTown(id);
        var townNpc = townNpcs.FirstOrDefault(n => n.Type == QuestGiverType.TownNpc);
        if (townNpc.NpcName != null)
        {
            int npcX = 13;
            int npcY = 17;
            if (map.IsInBounds(npcX, npcY))
            {
                map.SetTile(npcX, npcY, new MapTile
                {
                    Type = TileType.QuestNpc,
                    EntityId = townNpc.NpcName
                });
            }
        }

        // Final safety pass: ensure no decoration blocks a door approach
        ClearDoorApproaches(map);

        return map;
    }

    private static void GenerateTownBuildings(GameMap map, Random rng)
    {
        var buildings = new List<(int x, int y, int w, int h, string type)>
        {
            (3, 3, 8, 6, "weapon_shop"),
            (14, 3, 8, 6, "armor_shop"),
            (3, 12, 8, 6, "tavern"),
            (14, 12, 8, 6, "healer"),
            (24, 3, 6, 6, "guild"),
            (24, 12, 6, 6, "inn"),
            (24, 20, 6, 6, "temple")
        };

        // Generate unique shop names for this town
        var shopNames = new Dictionary<string, string>();
        foreach (var (_, _, _, _, btype) in buildings)
        {
            shopNames[btype] = TownNames.GetRandomName(btype, rng);
        }

        // Build all structures
        foreach (var (bx, by, bw, bh, btype) in buildings)
        {
            // Create building walls
            for (int x = bx; x < bx + bw; x++)
            {
                map.SetTile(x, by, new MapTile { Type = TileType.Wall });
                map.SetTile(x, by + bh - 1, new MapTile { Type = TileType.Wall });
            }
            for (int y = by; y < by + bh; y++)
            {
                map.SetTile(bx, y, new MapTile { Type = TileType.Wall });
                map.SetTile(bx + bw - 1, y, new MapTile { Type = TileType.Wall });
            }

            // Fill interior with floor
            for (int y = by + 1; y < by + bh - 1; y++)
            {
                for (int x = bx + 1; x < bx + bw - 1; x++)
                {
                    map.SetTile(x, y, new MapTile { Type = TileType.Floor });
                }
            }

            // Add door
            int doorX = bx + bw / 2;
            map.SetTile(doorX, by + bh - 1, new MapTile { Type = TileType.Door });

            // Add counter with "type|name" EntityId format
            string entityId = $"{btype}|{shopNames[btype]}";
            for (int x = bx + 2; x < bx + bw - 2; x++)
            {
                map.SetTile(x, by + 2, new MapTile { Type = TileType.Counter, EntityId = entityId });
            }

            // Place sign one tile right of door
            int signX = doorX + 1;
            int signY = by + bh - 1;
            if (map.GetTile(signX, signY).Type == TileType.Wall)
            {
                // Sign on the wall row, just outside
                signY = by + bh;
            }
            if (map.IsInBounds(signX, signY) && map.GetTile(signX, signY).Type == TileType.Grass)
            {
                map.SetTile(signX, signY, new MapTile { Type = TileType.Sign, EntityId = shopNames[btype] });
            }

            // Place flowers one tile left of door
            int flowerX = doorX - 1;
            int flowerY = by + bh;
            if (map.IsInBounds(flowerX, flowerY) && map.GetTile(flowerX, flowerY).Type == TileType.Grass)
            {
                map.SetTile(flowerX, flowerY, new MapTile { Type = TileType.Flowers });
            }
        }

        // Central town square (5x5 area around 16, 14)
        int sqCenterX = 16;
        int sqCenterY = 14;
        for (int dy = -2; dy <= 2; dy++)
        {
            for (int dx = -2; dx <= 2; dx++)
            {
                int sx = sqCenterX + dx;
                int sy = sqCenterY + dy;
                if (map.IsInBounds(sx, sy) && map.GetTile(sx, sy).Type == TileType.Grass)
                {
                    map.SetTile(sx, sy, new MapTile { Type = TileType.Path });
                }
            }
        }
        // Fountain at center
        map.SetTile(sqCenterX, sqCenterY, new MapTile { Type = TileType.Fountain });
        // Flowers on corners of the square (only on grass or path, never over walls)
        (int fx, int fy)[] cornerFlowers =
        {
            (sqCenterX - 2, sqCenterY - 2), (sqCenterX + 2, sqCenterY - 2),
            (sqCenterX - 2, sqCenterY + 2), (sqCenterX + 2, sqCenterY + 2)
        };
        foreach (var (cfx, cfy) in cornerFlowers)
        {
            var existing = map.GetTile(cfx, cfy).Type;
            if (existing is TileType.Grass or TileType.Path)
                map.SetTile(cfx, cfy, new MapTile { Type = TileType.Flowers });
        }

        // Main avenue: path from entrance (16, 30) north to town square (16, 14)
        int entranceX = TownSize / 2; // 16
        for (int y = TownSize - 2; y >= sqCenterY + 3; y--)
        {
            if (map.GetTile(entranceX, y).Type == TileType.Grass)
            {
                map.SetTile(entranceX, y, new MapTile { Type = TileType.Path });
            }
        }

        // East-west roads connecting building doors
        // Row 9: connects weapon_shop(door 7,8), armor_shop(door 18,8), guild(door 27,8)
        int roadY1 = 9;
        for (int x = 3; x <= 28; x++)
        {
            if (map.GetTile(x, roadY1).Type == TileType.Grass)
            {
                map.SetTile(x, roadY1, new MapTile { Type = TileType.Path });
            }
        }

        // Row 18: connects tavern(door 7,17), healer(door 18,17), inn(door 27,17)
        int roadY2 = 18;
        for (int x = 3; x <= 28; x++)
        {
            if (map.GetTile(x, roadY2).Type == TileType.Grass)
            {
                map.SetTile(x, roadY2, new MapTile { Type = TileType.Path });
            }
        }

        // North-south connector from top road through square to bottom road
        for (int y = roadY1; y <= roadY2; y++)
        {
            if (map.GetTile(entranceX, y).Type == TileType.Grass)
            {
                map.SetTile(entranceX, y, new MapTile { Type = TileType.Path });
            }
        }

        // Lampposts along main avenue (every ~5 tiles)
        int[] lampY = { 22, 27 };
        foreach (int ly in lampY)
        {
            // Place lamppost one tile to the side of the path
            int lx = entranceX + 1;
            if (map.IsInBounds(lx, ly) && map.GetTile(lx, ly).Type == TileType.Grass)
            {
                map.SetTile(lx, ly, new MapTile { Type = TileType.Lamppost });
            }
        }

        // Lampposts along east-west roads
        int[] lampX = { 12, 22 };
        foreach (int lx in lampX)
        {
            if (map.IsInBounds(lx, roadY1 - 1) && map.GetTile(lx, roadY1 - 1).Type == TileType.Grass)
                map.SetTile(lx, roadY1 - 1, new MapTile { Type = TileType.Lamppost });
            if (map.IsInBounds(lx, roadY2 + 1) && map.GetTile(lx, roadY2 + 1).Type == TileType.Grass)
                map.SetTile(lx, roadY2 + 1, new MapTile { Type = TileType.Lamppost });
        }

        // A few extra flower patches in open grass areas
        int[] flowerSpots = { 2, 10, 20, 29 };
        foreach (int fx in flowerSpots)
        {
            int fy = 22 + rng.Next(0, 5);
            if (map.IsInBounds(fx, fy) && map.GetTile(fx, fy).Type == TileType.Grass)
            {
                map.SetTile(fx, fy, new MapTile { Type = TileType.Flowers });
            }
        }

        // Door clearance pass: ensure the tile in front of every door is passable
        ClearDoorApproaches(map);
    }

    private static void ClearDoorApproaches(GameMap map)
    {
        for (int y = 0; y < map.Height; y++)
        {
            for (int x = 0; x < map.Width; x++)
            {
                if (map.GetTile(x, y).Type != TileType.Door) continue;

                // Check all 4 adjacent tiles; clear any non-passable, non-wall tiles
                // that block the approach to this door
                (int dx, int dy)[] offsets = { (0, -1), (0, 1), (-1, 0), (1, 0) };
                foreach (var (dx, dy) in offsets)
                {
                    int nx = x + dx;
                    int ny = y + dy;
                    if (!map.IsInBounds(nx, ny)) continue;

                    var tile = map.GetTile(nx, ny);
                    // Only clear tiles that should be walkable approaches
                    // (skip walls and floors — those are intentional building structure)
                    if (tile.Type is TileType.Wall or TileType.Floor or TileType.Counter
                        or TileType.Door or TileType.CastleWall) continue;

                    if (!tile.Type.IsPassable())
                    {
                        map.SetTile(nx, ny, new MapTile { Type = TileType.Path });
                    }
                }
            }
        }
    }

    public static GameMap GenerateDungeonLevel(Random rng, string id, string baseName, int level)
    {
        var map = new GameMap(DungeonSize, DungeonSize)
        {
            Id = id,
            Name = $"{baseName} Level {level}",
            MapType = MapType.Dungeon,
            DungeonLevel = level
        };

        // Fill with walls
        for (int y = 0; y < DungeonSize; y++)
        {
            for (int x = 0; x < DungeonSize; x++)
            {
                map.SetTile(x, y, new MapTile { Type = TileType.Wall });
            }
        }

        // Generate rooms with size variety
        var rooms = GenerateRooms(DungeonSize, DungeonSize, rng, 8 + level);

        // Carve rooms
        foreach (var room in rooms)
        {
            for (int y = room.y; y < room.y + room.h; y++)
            {
                for (int x = room.x; x < room.x + room.w; x++)
                {
                    map.SetTile(x, y, new MapTile { Type = TileType.Floor });
                }
            }
        }

        // Connect rooms with MST corridors + extra loops
        ConnectRoomsWithMST(map, rooms, rng, level);

        // Place doors at corridor chokepoints
        PlaceDungeonDoors(map, rng, level);

        // Add dungeon features (before stairs/portal so they can't overwrite them)
        AddDungeonFeatures(map, rooms, rng, level);

        // --- Place stairs and portal LAST so nothing can overwrite them ---
        var firstRoom = rooms[0];
        var lastRoom = rooms[^1];

        // Stairs up (entrance) — always in first room
        int stairsUpX = firstRoom.x + firstRoom.w / 2;
        int stairsUpY = firstRoom.y + firstRoom.h / 2;
        map.SetTile(stairsUpX, stairsUpY, new MapTile { Type = TileType.StairsUp });
        map.EntryPoints["default"] = (stairsUpX, stairsUpY);
        map.EntryPoints["stairs_up"] = (stairsUpX, stairsUpY);

        // Stairs down — always present on levels 1-7, placed in last room
        if (level < 8)
        {
            int stairsDownX = lastRoom.x + lastRoom.w / 2;
            int stairsDownY = lastRoom.y + lastRoom.h / 2;
            map.SetTile(stairsDownX, stairsDownY, new MapTile { Type = TileType.StairsDown });
            map.EntryPoints["stairs_down"] = (stairsDownX, stairsDownY);
        }

        // Exit portal on levels 5-8 — try every middle room until one works
        if (level >= 5 && rooms.Count >= 3)
        {
            var middleRooms = rooms.Skip(1).Take(rooms.Count - 2).ToList();
            bool placed = false;

            // First pass: look for a Floor tile at room centers
            foreach (var portalRoom in middleRooms)
            {
                int portalX = portalRoom.x + portalRoom.w / 2;
                int portalY = portalRoom.y + portalRoom.h / 2;
                if (map.GetTile(portalX, portalY).Type == TileType.Floor)
                {
                    map.SetTile(portalX, portalY, new MapTile { Type = TileType.ExitPortal });
                    map.EntryPoints["exit_portal"] = (portalX, portalY);
                    placed = true;
                    break;
                }
            }

            // Fallback: scan any Floor tile inside a middle room
            if (!placed)
            {
                foreach (var portalRoom in middleRooms)
                {
                    for (int ry = portalRoom.y; ry < portalRoom.y + portalRoom.h && !placed; ry++)
                    {
                        for (int rx = portalRoom.x; rx < portalRoom.x + portalRoom.w && !placed; rx++)
                        {
                            if (map.GetTile(rx, ry).Type == TileType.Floor)
                            {
                                map.SetTile(rx, ry, new MapTile { Type = TileType.ExitPortal });
                                map.EntryPoints["exit_portal"] = (rx, ry);
                                placed = true;
                            }
                        }
                    }
                }
            }

            // Last resort: force-place in the second room's center
            if (!placed && rooms.Count >= 2)
            {
                var fallback = rooms[1];
                int px = fallback.x + fallback.w / 2;
                int py = fallback.y + fallback.h / 2;
                map.SetTile(px, py, new MapTile { Type = TileType.ExitPortal });
                map.EntryPoints["exit_portal"] = (px, py);
            }
        }

        return map;
    }

    private static List<(int x, int y, int w, int h)> GenerateRooms(int mapW, int mapH, Random rng, int numRooms)
    {
        var rooms = new List<(int x, int y, int w, int h)>();

        for (int i = 0; i < numRooms * 10; i++)
        {
            // Pick size category: 30% closet, 50% standard, 20% chamber
            int roll = rng.Next(100);
            int w, h;
            if (roll < 30)
            {
                // Closet: 2-3 tiles
                w = rng.Next(2, 4);
                h = rng.Next(2, 4);
            }
            else if (roll < 80)
            {
                // Standard: 4-6 tiles
                w = rng.Next(4, 7);
                h = rng.Next(4, 7);
            }
            else
            {
                // Chamber: 7-9 tiles
                w = rng.Next(7, 10);
                h = rng.Next(7, 10);
            }

            if (w >= mapW - 2 || h >= mapH - 2) continue;

            int x = rng.Next(1, mapW - w - 1);
            int y = rng.Next(1, mapH - h - 1);

            // Check for overlap (1-tile buffer between rooms)
            bool overlaps = false;
            foreach (var room in rooms)
            {
                if (x < room.x + room.w + 1 && x + w + 1 > room.x &&
                    y < room.y + room.h + 1 && y + h + 1 > room.y)
                {
                    overlaps = true;
                    break;
                }
            }

            if (!overlaps)
            {
                rooms.Add((x, y, w, h));
                if (rooms.Count >= numRooms) break;
            }
        }

        return rooms;
    }

    private static void ConnectRoomsWithMST(GameMap map, List<(int x, int y, int w, int h)> rooms, Random rng, int level)
    {
        if (rooms.Count < 2) return;

        // Prim's MST: start with room 0, greedily add closest room
        var inMST = new HashSet<int> { 0 };
        var edges = new List<(int from, int to)>();

        while (inMST.Count < rooms.Count)
        {
            int bestFrom = -1, bestTo = -1;
            double bestDist = double.MaxValue;

            foreach (int from in inMST)
            {
                for (int to = 0; to < rooms.Count; to++)
                {
                    if (inMST.Contains(to)) continue;

                    double dist = RoomDistance(rooms[from], rooms[to]);
                    if (dist < bestDist)
                    {
                        bestDist = dist;
                        bestFrom = from;
                        bestTo = to;
                    }
                }
            }

            if (bestTo == -1) break;

            inMST.Add(bestTo);
            edges.Add((bestFrom, bestTo));
        }

        // Carve MST corridors
        foreach (var (from, to) in edges)
        {
            CarveCorridor(map, rooms[from], rooms[to], rng, level);
        }

        // Extra random connections (~25% of room count) create loops and alternate paths
        int extraCount = Math.Max(1, rooms.Count / 4);
        for (int i = 0; i < extraCount; i++)
        {
            int a = rng.Next(rooms.Count);
            int b = rng.Next(rooms.Count);
            if (a != b)
            {
                CarveCorridor(map, rooms[a], rooms[b], rng, level);
            }
        }
    }

    private static double RoomDistance((int x, int y, int w, int h) a, (int x, int y, int w, int h) b)
    {
        int ax = a.x + a.w / 2, ay = a.y + a.h / 2;
        int bx = b.x + b.w / 2, by = b.y + b.h / 2;
        return Math.Sqrt((ax - bx) * (ax - bx) + (ay - by) * (ay - by));
    }

    private static void CarveCorridor(GameMap map, (int x, int y, int w, int h) from, (int x, int y, int w, int h) to, Random rng, int level)
    {
        int x1 = from.x + from.w / 2;
        int y1 = from.y + from.h / 2;
        int x2 = to.x + to.w / 2;
        int y2 = to.y + to.h / 2;

        bool wide = level >= 5 && rng.Next(100) < 25;
        bool horizontalFirst = rng.Next(2) == 0;

        if (horizontalFirst)
        {
            CarveHorizontal(map, x1, x2, y1, wide);
            CarveVertical(map, y1, y2, x2, wide);
        }
        else
        {
            CarveVertical(map, y1, y2, x1, wide);
            CarveHorizontal(map, x1, x2, y2, wide);
        }
    }

    private static void CarveHorizontal(GameMap map, int x1, int x2, int y, bool wide)
    {
        int step = Math.Sign(x2 - x1);
        if (step == 0) return;
        for (int x = x1; x != x2; x += step)
        {
            if (map.IsInBounds(x, y) && map.GetTile(x, y).Type == TileType.Wall)
                map.SetTile(x, y, new MapTile { Type = TileType.Floor });
            if (wide && map.IsInBounds(x, y + 1) && map.GetTile(x, y + 1).Type == TileType.Wall)
                map.SetTile(x, y + 1, new MapTile { Type = TileType.Floor });
        }
    }

    private static void CarveVertical(GameMap map, int y1, int y2, int x, bool wide)
    {
        int step = Math.Sign(y2 - y1);
        if (step == 0) return;
        for (int y = y1; y != y2; y += step)
        {
            if (map.IsInBounds(x, y) && map.GetTile(x, y).Type == TileType.Wall)
                map.SetTile(x, y, new MapTile { Type = TileType.Floor });
            if (wide && map.IsInBounds(x + 1, y) && map.GetTile(x + 1, y).Type == TileType.Wall)
                map.SetTile(x + 1, y, new MapTile { Type = TileType.Floor });
        }
    }

    private static void PlaceDungeonDoors(GameMap map, Random rng, int level)
    {
        int doorChance = 15 + level * 5;

        for (int y = 1; y < map.Height - 1; y++)
        {
            for (int x = 1; x < map.Width - 1; x++)
            {
                if (map.GetTile(x, y).Type != TileType.Floor) continue;

                // Check for 1-wide chokepoint: wall on opposite sides, floor on other two
                bool wallAboveAndBelow = map.GetTile(x, y - 1).Type == TileType.Wall
                                      && map.GetTile(x, y + 1).Type == TileType.Wall;
                bool wallLeftAndRight = map.GetTile(x - 1, y).Type == TileType.Wall
                                     && map.GetTile(x + 1, y).Type == TileType.Wall;

                if ((wallAboveAndBelow || wallLeftAndRight) && rng.Next(100) < doorChance)
                {
                    map.SetTile(x, y, new MapTile { Type = TileType.Door });
                }
            }
        }
    }

    private static void AddDungeonFeatures(GameMap map, List<(int x, int y, int w, int h)> rooms, Random rng, int level)
    {
        var protectedTypes = new HashSet<TileType>
        {
            TileType.StairsUp, TileType.StairsDown, TileType.ExitPortal, TileType.Door
        };

        foreach (var room in rooms)
        {
            int area = room.w * room.h;

            // Max features based on room size category
            int maxFeatures;
            if (area <= 9)
                maxFeatures = rng.Next(0, 2); // closet: 0-1
            else if (area <= 36)
                maxFeatures = rng.Next(1, 3); // standard: 1-2
            else
                maxFeatures = rng.Next(2, 5); // chamber: 2-4

            int placed = 0;
            int attempts = maxFeatures * 10;

            while (placed < maxFeatures && attempts-- > 0)
            {
                int fx = room.x + rng.Next(room.w);
                int fy = room.y + rng.Next(room.h);

                if (!map.IsInBounds(fx, fy)) continue;
                var existing = map.GetTile(fx, fy);
                if (existing.Type != TileType.Floor || protectedTypes.Contains(existing.Type)) continue;

                int roll = rng.Next(100);
                int trapThreshold = 35 + 5 + level * 2; // 35 base + trap chance
                int secretThreshold = trapThreshold + 10;
                int lavaThreshold = secretThreshold + level * 3;

                TileType feature;
                if (roll < 20)
                    feature = TileType.Chest;
                else if (roll < 35)
                    feature = TileType.Fountain;
                else if (roll < trapThreshold)
                    feature = TileType.Trap;
                else if (roll < secretThreshold)
                    feature = TileType.SecretDoor;
                else if (roll < lavaThreshold)
                    feature = TileType.Lava;
                else
                    continue;

                if (feature == TileType.SecretDoor)
                {
                    // Place on room walls (all 4 sides), but skip walls
                    // adjacent to corridors so we don't block room entrances
                    var wallCandidates = new List<(int wx, int wy)>();
                    for (int x = room.x; x < room.x + room.w; x++)
                    {
                        // Top wall — skip if tile above it (outside room) is floor/corridor
                        if (map.IsInBounds(x, room.y - 1) && map.GetTile(x, room.y - 1).Type == TileType.Wall
                            && (!map.IsInBounds(x, room.y - 2) || map.GetTile(x, room.y - 2).Type == TileType.Wall))
                            wallCandidates.Add((x, room.y - 1));
                        // Bottom wall
                        if (map.IsInBounds(x, room.y + room.h) && map.GetTile(x, room.y + room.h).Type == TileType.Wall
                            && (!map.IsInBounds(x, room.y + room.h + 1) || map.GetTile(x, room.y + room.h + 1).Type == TileType.Wall))
                            wallCandidates.Add((x, room.y + room.h));
                    }
                    for (int y = room.y; y < room.y + room.h; y++)
                    {
                        // Left wall
                        if (map.IsInBounds(room.x - 1, y) && map.GetTile(room.x - 1, y).Type == TileType.Wall
                            && (!map.IsInBounds(room.x - 2, y) || map.GetTile(room.x - 2, y).Type == TileType.Wall))
                            wallCandidates.Add((room.x - 1, y));
                        // Right wall
                        if (map.IsInBounds(room.x + room.w, y) && map.GetTile(room.x + room.w, y).Type == TileType.Wall
                            && (!map.IsInBounds(room.x + room.w + 1, y) || map.GetTile(room.x + room.w + 1, y).Type == TileType.Wall))
                            wallCandidates.Add((room.x + room.w, y));
                    }

                    if (wallCandidates.Count > 0)
                    {
                        var (wx, wy) = wallCandidates[rng.Next(wallCandidates.Count)];
                        map.SetTile(wx, wy, new MapTile { Type = TileType.SecretDoor });
                        placed++;
                    }
                }
                else if (feature == TileType.Lava)
                {
                    // Lava cluster (1-3 tiles)
                    if (!IsNearCorridor(map, fx, fy))
                    {
                        map.SetTile(fx, fy, new MapTile { Type = TileType.Lava });
                        placed++;
                        int clusterSize = rng.Next(1, 4);
                        for (int c = 1; c < clusterSize; c++)
                        {
                            int lx = fx + rng.Next(-1, 2);
                            int ly = fy + rng.Next(-1, 2);
                            if (map.IsInBounds(lx, ly)
                                && map.GetTile(lx, ly).Type == TileType.Floor
                                && !protectedTypes.Contains(map.GetTile(lx, ly).Type)
                                && !IsNearCorridor(map, lx, ly))
                            {
                                map.SetTile(lx, ly, new MapTile { Type = TileType.Lava });
                            }
                        }
                    }
                }
                else
                {
                    map.SetTile(fx, fy, new MapTile { Type = feature });
                    placed++;
                }
            }
        }
    }

    private static float[,] GenerateNoise(int width, int height, Random rng, int octaves)
    {
        var noise = new float[width, height];

        // Generate base random values
        var baseNoise = new float[width, height];
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                baseNoise[x, y] = (float)rng.NextDouble();
            }
        }

        // Smooth with multiple octaves
        float amplitude = 1f;
        float totalAmplitude = 0f;

        for (int octave = 0; octave < octaves; octave++)
        {
            int samplePeriod = 1 << (octaves - octave);
            float sampleFrequency = 1f / samplePeriod;

            for (int y = 0; y < height; y++)
            {
                int y0 = (y / samplePeriod) * samplePeriod;
                int y1 = (y0 + samplePeriod) % height;
                float vertBlend = (y - y0) * sampleFrequency;

                for (int x = 0; x < width; x++)
                {
                    int x0 = (x / samplePeriod) * samplePeriod;
                    int x1 = (x0 + samplePeriod) % width;
                    float horzBlend = (x - x0) * sampleFrequency;

                    float top = Lerp(baseNoise[x0, y0], baseNoise[x1, y0], horzBlend);
                    float bottom = Lerp(baseNoise[x0, y1], baseNoise[x1, y1], horzBlend);

                    noise[x, y] += amplitude * Lerp(top, bottom, vertBlend);
                }
            }

            totalAmplitude += amplitude;
            amplitude *= 0.5f;
        }

        // Normalize
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                noise[x, y] /= totalAmplitude;
            }
        }

        return noise;
    }

    /// <summary>
    /// Returns true if the tile — or any of its 4 neighbors — is a 1-wide corridor
    /// (walls on two opposite sides). Placing impassable tiles here or next to a
    /// corridor entrance would block passage entirely.
    /// </summary>
    private static bool IsNearCorridor(GameMap map, int x, int y)
    {
        // Check the tile itself and all 4 adjacent tiles
        int[] dx = { 0, 0, 0, -1, 1 };
        int[] dy = { 0, -1, 1, 0, 0 };
        for (int i = 0; i < 5; i++)
        {
            int cx = x + dx[i];
            int cy = y + dy[i];
            if (!map.IsInBounds(cx, cy)) continue;
            if (map.GetTile(cx, cy).Type != TileType.Floor) continue;

            bool wallAboveAndBelow = map.GetTile(cx, cy - 1).Type == TileType.Wall
                                  && map.GetTile(cx, cy + 1).Type == TileType.Wall;
            bool wallLeftAndRight  = map.GetTile(cx - 1, cy).Type == TileType.Wall
                                  && map.GetTile(cx + 1, cy).Type == TileType.Wall;
            if (wallAboveAndBelow || wallLeftAndRight)
                return true;
        }
        return false;
    }

    private static float Lerp(float a, float b, float t) => a + (b - a) * t;
}
