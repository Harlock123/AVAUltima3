using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
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
    private static readonly IBrush DefaultTerrainBrush = new SolidColorBrush(Color.FromRgb(60, 60, 60));

    // Spell effect animation
    private record SpellEffect(int X, int Y, bool IsBeneficial, DateTime StartTime, int Seed, double DurationMs);
    private readonly List<SpellEffect> _activeEffects = new();
    private DispatcherTimer? _effectTimer;
    private const double EffectDurationMs = 600;
    private const double AoeEffectDurationMs = 1200;

    static CombatMapControl()
    {
        AffectsRender<CombatMapControl>(CombatSystemProperty, TileSizeProperty, TargetXProperty, TargetYProperty, ShowTargetProperty);
        AffectsMeasure<CombatMapControl>(TileSizeProperty);
    }

    /// <summary>
    /// Raised when a target is selected via mouse click while ShowTarget is true.
    /// Event args contain the grid X and Y coordinates.
    /// </summary>
    public event EventHandler<TargetSelectedEventArgs>? TargetSelected;

    public CombatMapControl()
    {
        ClipToBounds = true;
    }

    public void ShowSpellEffect(int x, int y, bool isBeneficial, bool isAoe = false)
    {
        double duration = isAoe ? AoeEffectDurationMs : EffectDurationMs;
        _activeEffects.Add(new SpellEffect(x, y, isBeneficial, DateTime.UtcNow, (x * 31 + y * 17) ^ Environment.TickCount, duration));

        if (_effectTimer == null)
        {
            _effectTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(50) };
            _effectTimer.Tick += (_, _) =>
            {
                _activeEffects.RemoveAll(e => (DateTime.UtcNow - e.StartTime).TotalMilliseconds > e.DurationMs);
                if (_activeEffects.Count == 0)
                {
                    _effectTimer.Stop();
                }
                InvalidateVisual();
            };
        }

        _effectTimer.Start();
        InvalidateVisual();
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);

        if (!ShowTarget) return;

        var point = e.GetPosition(this);
        int gridX = (int)(point.X / TileSize);
        int gridY = (int)(point.Y / TileSize);

        // Validate grid bounds
        if (gridX >= 0 && gridX < CombatSystem.GridWidth &&
            gridY >= 0 && gridY < CombatSystem.GridHeight)
        {
            // Update target position
            TargetX = gridX;
            TargetY = gridY;

            // If it's a double-click or left button, also confirm the target
            if (e.ClickCount == 2 || e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            {
                TargetSelected?.Invoke(this, new TargetSelectedEventArgs(gridX, gridY, e.ClickCount == 2));
            }

            e.Handled = true;
            InvalidateVisual();
        }
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        return new Size(CombatSystem.GridWidth * TileSize, CombatSystem.GridHeight * TileSize);
    }

    public override void Render(DrawingContext context)
    {
        base.Render(context);

        int gridWidth = CombatSystem.GridWidth;
        int gridHeight = CombatSystem.GridHeight;
        var combat = CombatSystem;

        // Always draw the grid, even if combat is null
        for (int y = 0; y < gridHeight; y++)
        {
            for (int x = 0; x < gridWidth; x++)
            {
                var rect = new Rect(x * TileSize, y * TileSize, TileSize, TileSize);

                IBrush brush = DefaultTerrainBrush;
                if (combat != null)
                {
                    var terrain = combat.GetTerrain(x, y);
                    brush = TerrainBrushes.TryGetValue(terrain, out var b) ? b : DefaultTerrainBrush;

                    // Try sprite first, fall back to code drawing
                    var terrainSprite = TileSpriteCache.Get("combat_" + terrain.ToString().ToLowerInvariant())
                                    ?? TileSpriteCache.Get(terrain.ToString().ToLowerInvariant());
                    if (terrainSprite != null)
                    {
                        context.DrawImage(terrainSprite, rect);
                    }
                    else if (terrain == TileType.Forest)
                    {
                        context.FillRectangle(brush, rect);
                        var center = rect.Center;
                        var treeBrush = new SolidColorBrush(Color.FromRgb(20, 60, 20));
                        context.FillRectangle(treeBrush, new Rect(center.X - 4, center.Y - 8, 8, 16));
                    }
                    else if (terrain == TileType.Mountain)
                    {
                        context.FillRectangle(brush, rect);
                        var center = rect.Center;
                        var mountainPen = new Pen(Brushes.DarkGray, 2);
                        context.DrawLine(mountainPen, new Point(center.X - 10, center.Y + 10),
                            new Point(center.X, center.Y - 8));
                        context.DrawLine(mountainPen, new Point(center.X + 10, center.Y + 10),
                            new Point(center.X, center.Y - 8));
                    }
                    else
                    {
                        context.FillRectangle(brush, rect);
                    }
                }
                else
                {
                    context.FillRectangle(brush, rect);
                }

                // Grid lines
                var pen = new Pen(Brushes.Black, 0.5);
                context.DrawRectangle(pen, rect);
            }
        }

        if (combat == null) return;

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
            var spriteKey = pc.Character.Class.ToString().ToLowerInvariant();
            DrawCombatant(context, pc.X, pc.Y, pc.Name, pc.IsAlive, true,
                pc == combat.CurrentCombatant, spriteKey);
        }

        foreach (var monster in combat.Monsters)
        {
            if (!monster.IsAlive) continue;
            var spriteKey = monster.Monster.Definition.Id;
            DrawCombatant(context, monster.X, monster.Y, monster.Name, monster.IsAlive, false,
                monster == combat.CurrentCombatant, spriteKey);
        }

        // Draw spell effects on top of everything
        DrawSpellEffects(context);
    }

    private void DrawSpellEffects(DrawingContext context)
    {
        if (_activeEffects.Count == 0) return;

        var now = DateTime.UtcNow;

        foreach (var effect in _activeEffects)
        {
            double elapsed = (now - effect.StartTime).TotalMilliseconds;
            double progress = Math.Clamp(elapsed / effect.DurationMs, 0, 1);

            // Alpha fades out over the duration
            byte alpha = (byte)(255 * (1.0 - progress));

            // Base tile position
            int baseX = effect.X * TileSize;
            int baseY = effect.Y * TileSize;

            // Draw a semi-transparent glow over the tile
            byte glowAlpha = (byte)(80 * (1.0 - progress));
            var glowColor = effect.IsBeneficial
                ? Color.FromArgb(glowAlpha, 80, 160, 255)
                : Color.FromArgb(glowAlpha, 255, 60, 40);
            var glowBrush = new SolidColorBrush(glowColor);
            context.FillRectangle(glowBrush, new Rect(baseX, baseY, TileSize, TileSize));

            // Draw 8 sparkle particles
            var rng = new Random(effect.Seed);
            int sparkleCount = 8;
            for (int i = 0; i < sparkleCount; i++)
            {
                // Base position within tile (randomized per-sparkle)
                double fx = rng.NextDouble() * (TileSize - 4) + 2;
                double fy = rng.NextDouble() * (TileSize - 4) + 2;

                // Sparkles drift upward for beneficial, outward for detrimental
                if (effect.IsBeneficial)
                {
                    fy -= progress * 12;
                }
                else
                {
                    double angle = rng.NextDouble() * Math.PI * 2;
                    fx += Math.Cos(angle) * progress * 6;
                    fy += Math.Sin(angle) * progress * 6;
                }

                // Stagger sparkle visibility — each sparkle fades at different rate
                double sparklePhase = (double)i / sparkleCount;
                double sparkleAlpha = Math.Clamp(1.0 - (progress - sparklePhase * 0.3) / 0.7, 0, 1);
                byte sa = (byte)(alpha * sparkleAlpha);
                if (sa < 10) continue;

                // Sparkle size pulses
                double size = 2 + Math.Sin(progress * Math.PI * 3 + i) * 1.5;

                var sparkleColor = effect.IsBeneficial
                    ? Color.FromArgb(sa, 140, 200, 255)
                    : Color.FromArgb(sa, 255, 100, 60);
                var sparkleBrush = new SolidColorBrush(sparkleColor);

                var sparkleRect = new Rect(baseX + fx - size / 2, baseY + fy - size / 2, size, size);
                context.FillRectangle(sparkleBrush, sparkleRect);

                // Bright center dot
                var brightColor = effect.IsBeneficial
                    ? Color.FromArgb(sa, 220, 240, 255)
                    : Color.FromArgb(sa, 255, 220, 180);
                var brightBrush = new SolidColorBrush(brightColor);
                context.FillRectangle(brightBrush, new Rect(baseX + fx - 0.5, baseY + fy - 0.5, 1, 1));
            }
        }
    }

    private void DrawCombatant(DrawingContext context, int gridX, int gridY, string name,
        bool isAlive, bool isPlayer, bool isCurrentTurn, string spriteKey)
    {
        int x = gridX * TileSize;
        int y = gridY * TileSize;
        var fullRect = new Rect(x, y, TileSize, TileSize);

        // Try sprite first
        var sprite = isAlive ? TileSpriteCache.Get(spriteKey) : null;
        if (sprite != null)
        {
            context.DrawImage(sprite, fullRect);

            // Draw current turn indicator around sprite
            if (isCurrentTurn)
            {
                var indicatorPen = new Pen(CurrentTurnBrush, 3);
                context.DrawRectangle(indicatorPen, fullRect);
            }
            return;
        }

        // Fall back to code-drawn circle
        var rect = new Rect(x + 4, y + 4, TileSize - 8, TileSize - 8);

        IBrush brush;
        if (!isAlive)
            brush = DeadBrush;
        else if (isPlayer)
            brush = PlayerBrush;
        else
            brush = MonsterBrush;

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

public class TargetSelectedEventArgs : EventArgs
{
    public int GridX { get; }
    public int GridY { get; }
    public bool IsDoubleClick { get; }

    public TargetSelectedEventArgs(int gridX, int gridY, bool isDoubleClick = false)
    {
        GridX = gridX;
        GridY = gridY;
        IsDoubleClick = isDoubleClick;
    }
}
