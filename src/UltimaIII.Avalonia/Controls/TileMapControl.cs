using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Threading;
using UltimaIII.Core.Engine;
using UltimaIII.Core.Enums;
using UltimaIII.Core.Models;

namespace UltimaIII.Avalonia.Controls;

public class TileMapControl : Control
{
    public static readonly StyledProperty<GameEngine?> GameEngineProperty =
        AvaloniaProperty.Register<TileMapControl, GameEngine?>(nameof(GameEngine));

    public static readonly StyledProperty<int> TileSizeProperty =
        AvaloniaProperty.Register<TileMapControl, int>(nameof(TileSize), 32);

    public static readonly StyledProperty<int> ViewportWidthProperty =
        AvaloniaProperty.Register<TileMapControl, int>(nameof(ViewportWidth), 15);

    public static readonly StyledProperty<int> ViewportHeightProperty =
        AvaloniaProperty.Register<TileMapControl, int>(nameof(ViewportHeight), 15);

    public GameEngine? GameEngine
    {
        get => GetValue(GameEngineProperty);
        set => SetValue(GameEngineProperty, value);
    }

    public int TileSize
    {
        get => GetValue(TileSizeProperty);
        set => SetValue(TileSizeProperty, value);
    }

    public int ViewportWidth
    {
        get => GetValue(ViewportWidthProperty);
        set => SetValue(ViewportWidthProperty, value);
    }

    public int ViewportHeight
    {
        get => GetValue(ViewportHeightProperty);
        set => SetValue(ViewportHeightProperty, value);
    }

    // Retro color palette (CGA/EGA style)
    private static readonly Dictionary<TileType, IBrush> TileBrushes = new()
    {
        [TileType.Grass] = new SolidColorBrush(Color.FromRgb(0, 170, 0)),
        [TileType.Forest] = new SolidColorBrush(Color.FromRgb(0, 100, 0)),
        [TileType.Mountain] = new SolidColorBrush(Color.FromRgb(128, 128, 128)),
        [TileType.Water] = new SolidColorBrush(Color.FromRgb(0, 0, 170)),
        [TileType.DeepWater] = new SolidColorBrush(Color.FromRgb(0, 0, 100)),
        [TileType.Swamp] = new SolidColorBrush(Color.FromRgb(85, 85, 0)),
        [TileType.Desert] = new SolidColorBrush(Color.FromRgb(255, 255, 85)),
        [TileType.Lava] = new SolidColorBrush(Color.FromRgb(255, 85, 0)),
        [TileType.Wall] = new SolidColorBrush(Color.FromRgb(85, 85, 85)),
        [TileType.Floor] = new SolidColorBrush(Color.FromRgb(170, 170, 170)),
        [TileType.Door] = new SolidColorBrush(Color.FromRgb(139, 90, 43)),
        [TileType.LockedDoor] = new SolidColorBrush(Color.FromRgb(100, 60, 30)),
        [TileType.SecretDoor] = new SolidColorBrush(Color.FromRgb(85, 85, 85)),
        [TileType.StairsUp] = new SolidColorBrush(Color.FromRgb(255, 255, 255)),
        [TileType.StairsDown] = new SolidColorBrush(Color.FromRgb(50, 50, 50)),
        [TileType.Altar] = new SolidColorBrush(Color.FromRgb(255, 255, 0)),
        [TileType.Fountain] = new SolidColorBrush(Color.FromRgb(85, 255, 255)),
        [TileType.Sign] = new SolidColorBrush(Color.FromRgb(139, 90, 43)),
        [TileType.Counter] = new SolidColorBrush(Color.FromRgb(139, 69, 19)),
        [TileType.Chest] = new SolidColorBrush(Color.FromRgb(255, 215, 0)),
        [TileType.CastleWall] = new SolidColorBrush(Color.FromRgb(100, 100, 100)),
        [TileType.CastleFloor] = new SolidColorBrush(Color.FromRgb(200, 200, 200)),
        [TileType.Bridge] = new SolidColorBrush(Color.FromRgb(139, 90, 43)),
        [TileType.Ladder] = new SolidColorBrush(Color.FromRgb(139, 90, 43)),
        [TileType.Portal] = new SolidColorBrush(Color.FromRgb(255, 0, 255)),
        [TileType.Pit] = new SolidColorBrush(Color.FromRgb(30, 30, 30)),
        [TileType.CeilingHole] = new SolidColorBrush(Color.FromRgb(200, 200, 200)),
        [TileType.Trap] = new SolidColorBrush(Color.FromRgb(170, 170, 170)),
        [TileType.Path] = new SolidColorBrush(Color.FromRgb(160, 140, 120)),
        [TileType.Flowers] = new SolidColorBrush(Color.FromRgb(0, 170, 0)),
        [TileType.Lamppost] = new SolidColorBrush(Color.FromRgb(0, 170, 0)),
        [TileType.QuestNpc] = new SolidColorBrush(Color.FromRgb(0, 170, 0)),
        [TileType.Town] = new SolidColorBrush(Color.FromRgb(200, 200, 200)),
        [TileType.DungeonEntrance] = new SolidColorBrush(Color.FromRgb(60, 60, 60)),
        [TileType.Void] = new SolidColorBrush(Color.FromRgb(0, 0, 0))
    };

    private static readonly IBrush PlayerBrush = new SolidColorBrush(Color.FromRgb(255, 255, 255));
    private static readonly IBrush FogBrush = new SolidColorBrush(Color.FromArgb(180, 0, 0, 0));
    private static readonly IBrush UnexploredBrush = new SolidColorBrush(Color.FromRgb(20, 20, 20));

    public TileMapControl()
    {
        ClipToBounds = true;
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        return new Size(ViewportWidth * TileSize, ViewportHeight * TileSize);
    }

    public override void Render(DrawingContext context)
    {
        base.Render(context);

        var engine = GameEngine;
        if (engine?.CurrentMap == null)
        {
            context.FillRectangle(Brushes.Black, new Rect(0, 0, Bounds.Width, Bounds.Height));
            return;
        }

        var map = engine.CurrentMap;
        var party = engine.Party;

        // Calculate viewport offset (center on party)
        int halfW = ViewportWidth / 2;
        int halfH = ViewportHeight / 2;
        int startX = party.X - halfW;
        int startY = party.Y - halfH;

        // Draw tiles
        for (int vy = 0; vy < ViewportHeight; vy++)
        {
            for (int vx = 0; vx < ViewportWidth; vx++)
            {
                int worldX = startX + vx;
                int worldY = startY + vy;

                // Handle wrapping for overworld
                if (map.MapType == MapType.Overworld)
                {
                    (worldX, worldY) = map.WrapCoordinates(worldX, worldY);
                }

                var tile = map.GetTile(worldX, worldY);
                var rect = new Rect(vx * TileSize, vy * TileSize, TileSize, TileSize);

                // Try sprite first, fall back to code drawing
                var tileSprite = TileSpriteCache.Get(tile.Type.ToString().ToLowerInvariant());
                if (tileSprite != null)
                {
                    context.DrawImage(tileSprite, rect);
                }
                else
                {
                    var brush = GetTileBrush(tile.Type);
                    context.FillRectangle(brush, rect);

                    // Draw tile border for dungeon floors
                    if (engine.State == GameState.Dungeon && tile.Type == TileType.Floor)
                    {
                        var pen = new Pen(Brushes.DarkGray, 1);
                        context.DrawRectangle(pen, rect);
                    }

                    // Draw special symbols
                    DrawTileSymbol(context, tile.Type, rect);
                }

                // Draw fog of war
                if (!tile.IsVisible && tile.IsExplored)
                {
                    context.FillRectangle(FogBrush, rect);
                }
                else if (!tile.IsExplored)
                {
                    context.FillRectangle(UnexploredBrush, rect);
                }
            }
        }

        // Draw party (always at center)
        int partyScreenX = halfW * TileSize;
        int partyScreenY = halfH * TileSize;
        DrawPlayer(context, partyScreenX, partyScreenY, party.Facing);
    }

    private IBrush GetTileBrush(TileType type)
    {
        return TileBrushes.TryGetValue(type, out var brush) ? brush : Brushes.Magenta;
    }

    private void DrawTileSymbol(DrawingContext context, TileType type, Rect rect)
    {
        var center = rect.Center;
        var pen = new Pen(Brushes.Black, 2);

        switch (type)
        {
            case TileType.StairsDown:
                // Draw down arrow
                context.DrawLine(pen, new Point(center.X, center.Y - 8), new Point(center.X, center.Y + 8));
                context.DrawLine(pen, new Point(center.X - 5, center.Y + 3), new Point(center.X, center.Y + 8));
                context.DrawLine(pen, new Point(center.X + 5, center.Y + 3), new Point(center.X, center.Y + 8));
                break;

            case TileType.StairsUp:
                // Draw up arrow
                pen = new Pen(Brushes.Black, 2);
                context.DrawLine(pen, new Point(center.X, center.Y + 8), new Point(center.X, center.Y - 8));
                context.DrawLine(pen, new Point(center.X - 5, center.Y - 3), new Point(center.X, center.Y - 8));
                context.DrawLine(pen, new Point(center.X + 5, center.Y - 3), new Point(center.X, center.Y - 8));
                break;

            case TileType.Chest:
                // Draw chest shape
                var chestRect = new Rect(center.X - 8, center.Y - 5, 16, 10);
                context.FillRectangle(new SolidColorBrush(Color.FromRgb(139, 90, 43)), chestRect);
                context.DrawRectangle(new Pen(Brushes.Black, 1), chestRect);
                break;

            case TileType.Fountain:
                // Draw fountain circle
                var geometry = new EllipseGeometry(new Rect(center.X - 8, center.Y - 8, 16, 16));
                context.DrawGeometry(Brushes.Blue, new Pen(Brushes.White, 2), geometry);
                break;

            case TileType.Door:
            case TileType.LockedDoor:
                // Draw door symbol
                var doorRect = new Rect(center.X - 4, center.Y - 10, 8, 20);
                context.FillRectangle(new SolidColorBrush(Color.FromRgb(100, 60, 30)), doorRect);
                // Door handle
                context.FillRectangle(Brushes.Yellow, new Rect(center.X + 1, center.Y, 2, 2));
                break;

            case TileType.Forest:
                // Draw simple tree
                pen = new Pen(new SolidColorBrush(Color.FromRgb(0, 80, 0)), 2);
                context.DrawLine(pen, new Point(center.X, center.Y + 10), new Point(center.X, center.Y - 5));
                var treeTop = new EllipseGeometry(new Rect(center.X - 8, center.Y - 12, 16, 14));
                context.DrawGeometry(new SolidColorBrush(Color.FromRgb(0, 120, 0)), null, treeTop);
                break;

            case TileType.Mountain:
                // Draw mountain peak
                var mountainPen = new Pen(Brushes.DarkGray, 2);
                context.DrawLine(mountainPen, new Point(center.X - 10, center.Y + 10),
                    new Point(center.X, center.Y - 8));
                context.DrawLine(mountainPen, new Point(center.X + 10, center.Y + 10),
                    new Point(center.X, center.Y - 8));
                // Snow cap
                context.DrawLine(new Pen(Brushes.White, 2),
                    new Point(center.X - 3, center.Y - 4), new Point(center.X + 3, center.Y - 4));
                break;

            case TileType.Sign:
                // Draw sign post with plaque
                var postPen = new Pen(new SolidColorBrush(Color.FromRgb(100, 60, 30)), 3);
                context.DrawLine(postPen, new Point(center.X, center.Y + 12), new Point(center.X, center.Y - 4));
                // Plaque
                var plaqueRect = new Rect(center.X - 8, center.Y - 10, 16, 8);
                context.FillRectangle(new SolidColorBrush(Color.FromRgb(180, 140, 80)), plaqueRect);
                context.DrawRectangle(new Pen(new SolidColorBrush(Color.FromRgb(100, 60, 30)), 1), plaqueRect);
                break;

            case TileType.Flowers:
                // Draw colored flower dots on green
                var red = new SolidColorBrush(Color.FromRgb(255, 80, 80));
                var yellow = new SolidColorBrush(Color.FromRgb(255, 255, 100));
                var pink = new SolidColorBrush(Color.FromRgb(255, 150, 200));
                context.FillRectangle(red, new Rect(center.X - 6, center.Y - 5, 4, 4));
                context.FillRectangle(yellow, new Rect(center.X + 3, center.Y - 7, 4, 4));
                context.FillRectangle(pink, new Rect(center.X - 2, center.Y + 3, 4, 4));
                context.FillRectangle(red, new Rect(center.X + 5, center.Y + 2, 4, 4));
                break;

            case TileType.Lamppost:
                // Draw lamppost: thin brown post with yellow glow on top
                var lampPen = new Pen(new SolidColorBrush(Color.FromRgb(80, 60, 40)), 3);
                context.DrawLine(lampPen, new Point(center.X, center.Y + 12), new Point(center.X, center.Y - 6));
                // Lantern glow
                var glowGeom = new EllipseGeometry(new Rect(center.X - 5, center.Y - 12, 10, 8));
                context.DrawGeometry(new SolidColorBrush(Color.FromRgb(255, 230, 100)), null, glowGeom);
                break;

            case TileType.Path:
                // Subtle cobblestone grid lines
                var stonePen = new Pen(new SolidColorBrush(Color.FromRgb(140, 120, 100)), 1);
                context.DrawLine(stonePen, new Point(rect.X + 8, rect.Y), new Point(rect.X + 8, rect.Y + TileSize));
                context.DrawLine(stonePen, new Point(rect.X + 24, rect.Y), new Point(rect.X + 24, rect.Y + TileSize));
                context.DrawLine(stonePen, new Point(rect.X, rect.Y + 10), new Point(rect.X + TileSize, rect.Y + 10));
                context.DrawLine(stonePen, new Point(rect.X, rect.Y + 22), new Point(rect.X + TileSize, rect.Y + 22));
                break;

            case TileType.QuestNpc:
                // Draw NPC figure: circle head + rectangle body in green/gold
                var npcHeadBrush = new SolidColorBrush(Color.FromRgb(255, 220, 180));
                var npcBodyBrush = new SolidColorBrush(Color.FromRgb(60, 160, 60));
                var npcHead = new EllipseGeometry(new Rect(center.X - 5, center.Y - 12, 10, 10));
                context.DrawGeometry(npcHeadBrush, new Pen(Brushes.Black, 1), npcHead);
                var npcBody = new Rect(center.X - 6, center.Y - 2, 12, 14);
                context.FillRectangle(npcBodyBrush, npcBody);
                context.DrawRectangle(new Pen(Brushes.Black, 1), npcBody);
                // Exclamation mark above head for quest availability
                var questMark = new FormattedText("!", System.Globalization.CultureInfo.CurrentCulture,
                    FlowDirection.LeftToRight, new Typeface("Consolas", FontStyle.Normal, FontWeight.Bold),
                    10, new SolidColorBrush(Color.FromRgb(255, 255, 0)));
                context.DrawText(questMark, new Point(center.X - questMark.Width / 2, center.Y - 22));
                break;

            case TileType.DungeonEntrance:
                // Draw a dark cave mouth
                var caveBrush = new SolidColorBrush(Color.FromRgb(40, 40, 40));
                var caveFrameBrush = new SolidColorBrush(Color.FromRgb(100, 90, 80));
                var cavePen = new Pen(Brushes.Black, 1);
                // Stone arch
                var archPoints = new Point[]
                {
                    new(center.X - 10, center.Y + 12),
                    new(center.X - 10, center.Y - 4),
                    new(center.X - 6, center.Y - 10),
                    new(center.X, center.Y - 12),
                    new(center.X + 6, center.Y - 10),
                    new(center.X + 10, center.Y - 4),
                    new(center.X + 10, center.Y + 12)
                };
                var archGeom = new PolylineGeometry(archPoints, true);
                context.DrawGeometry(caveFrameBrush, cavePen, archGeom);
                // Dark interior
                var innerPoints = new Point[]
                {
                    new(center.X - 7, center.Y + 12),
                    new(center.X - 7, center.Y - 2),
                    new(center.X - 4, center.Y - 7),
                    new(center.X, center.Y - 9),
                    new(center.X + 4, center.Y - 7),
                    new(center.X + 7, center.Y - 2),
                    new(center.X + 7, center.Y + 12)
                };
                var innerGeom = new PolylineGeometry(innerPoints, true);
                context.DrawGeometry(caveBrush, null, innerGeom);
                break;

            case TileType.Town:
                // Draw a small castle/town icon
                var wallBrush = new SolidColorBrush(Color.FromRgb(160, 140, 120));
                var roofBrush = new SolidColorBrush(Color.FromRgb(180, 60, 60));
                var townPen = new Pen(Brushes.Black, 1);
                // Main building
                var buildingRect = new Rect(center.X - 8, center.Y - 2, 16, 14);
                context.FillRectangle(wallBrush, buildingRect);
                context.DrawRectangle(townPen, buildingRect);
                // Roof (triangle)
                var roofPoints = new Point[]
                {
                    new(center.X - 10, center.Y - 2),
                    new(center.X, center.Y - 12),
                    new(center.X + 10, center.Y - 2)
                };
                var roofGeom = new PolylineGeometry(roofPoints, true);
                context.DrawGeometry(roofBrush, townPen, roofGeom);
                // Door
                var townDoorRect = new Rect(center.X - 3, center.Y + 5, 6, 7);
                context.FillRectangle(new SolidColorBrush(Color.FromRgb(80, 50, 30)), townDoorRect);
                break;
        }
    }

    private void DrawPlayer(DrawingContext context, int x, int y, Direction facing)
    {
        // Try sprite first
        var sprite = TileSpriteCache.Get("party");
        if (sprite != null)
        {
            var destRect = new Rect(x, y, TileSize, TileSize);
            context.DrawImage(sprite, destRect);
            return;
        }

        // Fall back to code-drawn triangle
        var center = new Point(x + TileSize / 2, y + TileSize / 2);
        int size = TileSize - 8;

        var points = new Point[3];
        switch (facing)
        {
            case Direction.North:
                points[0] = new Point(center.X, center.Y - size / 2);
                points[1] = new Point(center.X - size / 2, center.Y + size / 2);
                points[2] = new Point(center.X + size / 2, center.Y + size / 2);
                break;
            case Direction.South:
                points[0] = new Point(center.X, center.Y + size / 2);
                points[1] = new Point(center.X - size / 2, center.Y - size / 2);
                points[2] = new Point(center.X + size / 2, center.Y - size / 2);
                break;
            case Direction.East:
                points[0] = new Point(center.X + size / 2, center.Y);
                points[1] = new Point(center.X - size / 2, center.Y - size / 2);
                points[2] = new Point(center.X - size / 2, center.Y + size / 2);
                break;
            case Direction.West:
                points[0] = new Point(center.X - size / 2, center.Y);
                points[1] = new Point(center.X + size / 2, center.Y - size / 2);
                points[2] = new Point(center.X + size / 2, center.Y + size / 2);
                break;
        }

        var geometry = new PolylineGeometry(points, true);
        context.DrawGeometry(PlayerBrush, new Pen(Brushes.Black, 2), geometry);
    }

    public void Refresh()
    {
        Dispatcher.UIThread.Post(InvalidateVisual);
    }
}
