using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;

namespace UltimaIII.Avalonia.Views;

public partial class CombatView : UserControl
{
    private DispatcherTimer? _renderTimer;

    public CombatView()
    {
        InitializeComponent();

        // Set up render refresh timer for combat
        _renderTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(100) // 10 FPS for combat refresh
        };
        _renderTimer.Tick += (s, e) => CombatMap?.Refresh();
        _renderTimer.Start();
    }

    protected override void OnUnloaded(RoutedEventArgs e)
    {
        base.OnUnloaded(e);
        _renderTimer?.Stop();
        _renderTimer = null;
    }
}
