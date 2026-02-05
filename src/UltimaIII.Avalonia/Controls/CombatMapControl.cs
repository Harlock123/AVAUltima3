using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Threading;
using UltimaIII.Core.Engine;
using UltimaIII.Core.Enums;

namespace UltimaIII.Avalonia.Controls;

public class CombatMapControl : Control
{
    public static readonly StyledProperty<CombatSystem?> CombatSystemProperty =
        AvaloniaProperty.Register<CombatMapControl, CombatSystem?>(nameof(CombatSystem));

    public static readonly StyledProperty<int> TileSizeProperty =
        AvaloniaProperty.Register<CombatMapControl, int>(nameof(TileSize), 32);

    public static readonly StyledProperty<int> TargetXProperty =
        AvaloniaProperty.Register<CombatMapControl, int>(nameof(TargetX), -1);

    public static readonly StyledProperty<int> TargetYProperty =
        AvaloniaProperty.Register<CombatMapControl, int>(nameof(TargetY), -1);

    public static readonly StyledProperty<bool> ShowTargetProperty =
        AvaloniaProperty.Register<CombatMapControl, bool>(nameof(ShowTarget), false);

    public CombatSystem? CombatSystem
    {
        get => GetValue(CombatSystemProperty);
        set => SetValue(CombatSystemProperty, value);
    }

    public int TileSize
    {
        get => GetValue(TileSizeProperty);
        set => SetValue(TileSizeProperty, value);
    }

    public int TargetX
    {
        get => GetValue(TargetXProperty);
        set => SetValue(TargetXProperty, value);
    }

    public int TargetY
    {
        get => GetValue(TargetYProperty);
        set => SetValue(TargetYProperty, value);
    }

    public bool ShowTarget
    {
        get => GetValue(ShowTargetProperty);
        set => SetValue(ShowTargetProperty, value);
    }

    private static readonly Dictionary<TileType, IBrush> TerrainBrushes = new()
    {
        [TileType.Grass] = new SolidColorBrush(Color.FromRgb(50, 120, 50)),
        [TileType.Forest] = new SolidColorBrush(Color.FromRgb(30, 80, 30)),
        [TileType.Mountain] = new SolidColorBrush(Color.FromRgb(100, 100, 100)),
        [TileType.Floor] = new SolidColorBrush(Color.FromRgb(80, 80, 80))
    };

    private static readonly IBrush PlayerBrush = new SolidColorBrush(Color.FromRgb(100, 150, 255));
    private static readonly IBrush MonsterBrush = new SolidColorBrush(Color.FromRgb(255, 100, 100));
    private static readonly IBrush DeadBrush = new SolidColorBrush(Color.FromRgb(80, 80, 80));
    private static readonly IBrush TargetBrush = new SolidColorBrush(Color.FromArgb(100, 255, 255, 0));
    private static readonly IBrush CurrentTurnBrush = new SolidColorBrush(Color.FromRgb(255, 255, 0));

    public CombatMapControl()
    {
        ClipToBounds = true;
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        return new Size(CombatSystem.GridWidth * TileSize, CombatSystem.GridHeight * TileSize);
    }

    public override void Render(DrawingContext context)
    {
        base.Render(context);

        var combat = CombatSystem;
        if (combat == null)
        {
            context.FillRectangle(Brushes.Black, new Rect(0, 0, Bounds.Width, Bounds.Height));
            return;
        }

        // Draw terrain
        for (int y = 0; y < CombatSystem.GridHeight; y++)
        {
            for (int x = 0; x < CombatSystem.GridWidth; x++)
            {
                var terrain = combat.GetTerrain(x, y);
                var rect = new Rect(x * TileSize, y * TileSize, TileSize, TileSize);

                var brush = TerrainBrushes.TryGetValue(terrain, out var b) ? b : Brushes.DarkGray;
                context.FillRectangle(brush, rect);

                // Grid lines
                var pen = new Pen(Brushes.Black, 0.5);
                context.DrawRectangle(pen, rect);

                // Draw terrain features
                if (terrain == TileType.Forest)
                {
                    var center = rect.Center;
                    var treeBrush = new SolidColorBrush(Color.FromRgb(20, 60, 20));
                    context.FillRectangle(treeBrush, new Rect(center.X - 4, center.Y - 8, 8, 16));
                }
                else if (terrain == TileType.Mountain)
                {
                    var center = rect.Center;
                    var mountainPen = new Pen(Brushes.DarkGray, 2);
                    context.DrawLine(mountainPen, new Point(center.X - 10, center.Y + 10),
                        new Point(center.X, center.Y - 8));
                    context.DrawLine(mountainPen, new Point(center.X + 10, center.Y + 10),
                        new Point(center.X, center.Y - 8));
                }
            }
        }

        // Draw target cursor
        if (ShowTarget && TargetX >= 0 && TargetY >= 0)
        {
            var targetRect = new Rect(TargetX * TileSize, TargetY * TileSize, TileSize, TileSize);
            context.FillRectangle(TargetBrush, targetRect);
            var targetPen = new Pen(Brushes.Yellow, 3);
            context.DrawRectangle(targetPen, targetRect);
        }

        // Draw combatants
        foreach (var pc in combat.PlayerCharacters)
        {
            DrawCombatant(context, pc.X, pc.Y, pc.Name, pc.IsAlive, true,
                pc == combat.CurrentCombatant);
        }

        foreach (var monster in combat.Monsters)
        {
            DrawCombatant(context, monster.X, monster.Y, monster.Name, monster.IsAlive, false,
                monster == combat.CurrentCombatant);
        }
    }

    private void DrawCombatant(DrawingContext context, int gridX, int gridY, string name,
        bool isAlive, bool isPlayer, bool isCurrentTurn)
    {
        int x = gridX * TileSize;
        int y = gridY * TileSize;
        var rect = new Rect(x + 4, y + 4, TileSize - 8, TileSize - 8);

        IBrush brush;
        if (!isAlive)
            brush = DeadBrush;
        else if (isPlayer)
            brush = PlayerBrush;
        else
            brush = MonsterBrush;

        // Draw as circle
        var geometry = new EllipseGeometry(rect);
        context.DrawGeometry(brush, new Pen(Brushes.Black, 2), geometry);

        // Draw current turn indicator
        if (isCurrentTurn && isAlive)
        {
            var indicatorPen = new Pen(CurrentTurnBrush, 3);
            context.DrawGeometry(null, indicatorPen, geometry);
        }

        // Draw name/initial
        var formattedText = new FormattedText(
            name.Length > 0 ? name[0].ToString() : "?",
            System.Globalization.CultureInfo.CurrentCulture,
            FlowDirection.LeftToRight,
            new Typeface("Consolas", FontStyle.Normal, FontWeight.Bold),
            14,
            Brushes.White);

        var textX = x + (TileSize - formattedText.Width) / 2;
        var textY = y + (TileSize - formattedText.Height) / 2;
        context.DrawText(formattedText, new Point(textX, textY));
    }

    public void Refresh()
    {
        Dispatcher.UIThread.Post(InvalidateVisual);
    }
}
