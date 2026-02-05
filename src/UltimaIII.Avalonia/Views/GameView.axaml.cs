using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;

namespace UltimaIII.Avalonia.Views;

public partial class GameView : UserControl
{
    private DispatcherTimer? _renderTimer;

    public GameView()
    {
        InitializeComponent();

        // Set up render refresh timer
        _renderTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(50) // 20 FPS for map refresh
        };
        _renderTimer.Tick += (s, e) => TileMap?.Refresh();
        _renderTimer.Start();
    }

    protected override void OnUnloaded(RoutedEventArgs e)
    {
        base.OnUnloaded(e);
        _renderTimer?.Stop();
        _renderTimer = null;
    }
}
