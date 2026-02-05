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
    private const int DungeonSize = 16;

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
            map.SetTile(x, y, new MapTile { Type = TileType.CastleFloor });

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
            // Clear area and add mountain entrance
            map.SetTile(x, y, new MapTile { Type = TileType.StairsDown });
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

        // Add some decorative elements
        for (int i = 0; i < 10; i++)
        {
            int x = rng.Next(2, TownSize - 2);
            int y = rng.Next(2, TownSize - 2);
            if (map.GetTile(x, y).Type == TileType.Grass)
            {
                map.SetTile(x, y, new MapTile { Type = TileType.Forest });
            }
        }

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
            (24, 12, 6, 6, "inn")
        };

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

            // Add counter in shops
            if (btype.Contains("shop") || btype == "healer" || btype == "guild")
            {
                for (int x = bx + 2; x < bx + bw - 2; x++)
                {
                    map.SetTile(x, by + 2, new MapTile { Type = TileType.Counter });
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

        // Generate rooms using BSP-like approach
        var rooms = GenerateRooms(DungeonSize, DungeonSize, rng, 4 + level / 2);

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

        // Connect rooms with corridors
        for (int i = 1; i < rooms.Count; i++)
        {
            var from = rooms[i - 1];
            var to = rooms[i];
            ConnectRooms(map, from, to);
        }

        // Place stairs
        var firstRoom = rooms[0];
        var lastRoom = rooms[^1];

        // Stairs up (entrance)
        int stairsUpX = firstRoom.x + firstRoom.w / 2;
        int stairsUpY = firstRoom.y + firstRoom.h / 2;
        map.SetTile(stairsUpX, stairsUpY, new MapTile { Type = TileType.StairsUp });
        map.EntryPoints["default"] = (stairsUpX, stairsUpY);
        map.EntryPoints["stairs_up"] = (stairsUpX, stairsUpY);

        // Stairs down (if not bottom level)
        if (level < 8)
        {
            int stairsDownX = lastRoom.x + lastRoom.w / 2;
            int stairsDownY = lastRoom.y + lastRoom.h / 2;
            map.SetTile(stairsDownX, stairsDownY, new MapTile { Type = TileType.StairsDown });
            map.EntryPoints["stairs_down"] = (stairsDownX, stairsDownY);
        }

        // Add dungeon features
        AddDungeonFeatures(map, rooms, rng, level);

        return map;
    }

    private static List<(int x, int y, int w, int h)> GenerateRooms(int mapW, int mapH, Random rng, int numRooms)
    {
        var rooms = new List<(int x, int y, int w, int h)>();

        for (int i = 0; i < numRooms * 3; i++) // Try multiple times
        {
            int w = rng.Next(3, 6);
            int h = rng.Next(3, 6);
            int x = rng.Next(1, mapW - w - 1);
            int y = rng.Next(1, mapH - h - 1);

            // Check for overlap
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

    private static void ConnectRooms(GameMap map,
        (int x, int y, int w, int h) from,
        (int x, int y, int w, int h) to)
    {
        int x1 = from.x + from.w / 2;
        int y1 = from.y + from.h / 2;
        int x2 = to.x + to.w / 2;
        int y2 = to.y + to.h / 2;

        // Horizontal then vertical
        int x = x1;
        while (x != x2)
        {
            if (map.GetTile(x, y1).Type == TileType.Wall)
                map.SetTile(x, y1, new MapTile { Type = TileType.Floor });
            x += Math.Sign(x2 - x1);
        }

        int y = y1;
        while (y != y2)
        {
            if (map.GetTile(x2, y).Type == TileType.Wall)
                map.SetTile(x2, y, new MapTile { Type = TileType.Floor });
            y += Math.Sign(y2 - y1);
        }
    }

    private static void AddDungeonFeatures(GameMap map, List<(int x, int y, int w, int h)> rooms, Random rng, int level)
    {
        foreach (var room in rooms.Skip(1).Take(rooms.Count - 2))
        {
            // Add some features to each room
            int featureX = room.x + rng.Next(1, room.w - 1);
            int featureY = room.y + rng.Next(1, room.h - 1);

            int roll = rng.Next(100);
            TileType feature;

            if (roll < 15)
                feature = TileType.Chest;
            else if (roll < 25)
                feature = TileType.Fountain;
            else if (roll < 35 && level > 3)
                feature = TileType.Trap;
            else if (roll < 40)
                feature = TileType.SecretDoor;
            else
                continue;

            if (feature == TileType.SecretDoor)
            {
                // Place secret door in a wall
                int wallX = room.x + (rng.Next(2) == 0 ? 0 : room.w - 1);
                int wallY = room.y + rng.Next(1, room.h - 1);
                map.SetTile(wallX, wallY, new MapTile { Type = feature });
            }
            else
            {
                map.SetTile(featureX, featureY, new MapTile { Type = feature });
            }
        }

        // Add some lava at deeper levels
        if (level >= 5)
        {
            int lavaCount = level - 4;
            for (int i = 0; i < lavaCount; i++)
            {
                var room = rooms[rng.Next(rooms.Count)];
                int x = room.x + rng.Next(room.w);
                int y = room.y + rng.Next(room.h);
                if (map.GetTile(x, y).Type == TileType.Floor)
                {
                    map.SetTile(x, y, new MapTile { Type = TileType.Lava });
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

    private static float Lerp(float a, float b, float t) => a + (b - a) * t;
}
