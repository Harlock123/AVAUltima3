using System;
using System.Collections.Generic;
using Avalonia.Media.Imaging;

namespace UltimaIII.Avalonia.Controls;

/// <summary>
/// Loads and caches PNG tile sprites from Assets/tiles/sprites/.
/// Place a PNG file named after a tile key (e.g. "party.png", "grass.png")
/// and it will be used automatically by TileMapControl.
/// </summary>
public static class TileSpriteCache
{
    private static readonly Dictionary<string, Bitmap?> _cache = new();

    private const string AssetBasePath = "avares://UltimaIII.Avalonia/Assets/sprites/";

    /// <summary>
    /// Gets a cached bitmap by sprite key (e.g. "party", "grass").
    /// Returns null if no PNG exists for this key.
    /// </summary>
    public static Bitmap? Get(string key)
    {
        if (_cache.TryGetValue(key, out var cached))
            return cached;

        // Try to load on first access
        var bitmap = TryLoad(key);
        _cache[key] = bitmap;
        return bitmap;
    }

    private static Bitmap? TryLoad(string key)
    {
        try
        {
            var uri = new Uri($"{AssetBasePath}{key}.png");
            using var stream = global::Avalonia.Platform.AssetLoader.Open(uri);
            return new Bitmap(stream);
        }
        catch
        {
            // No sprite file for this key — fall back to code drawing
            return null;
        }
    }
}
