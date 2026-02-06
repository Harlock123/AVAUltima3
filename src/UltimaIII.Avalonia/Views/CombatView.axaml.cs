using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using UltimaIII.Avalonia.Controls;
using UltimaIII.Avalonia.ViewModels;

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

        // Wire up mouse target selection
        CombatMap.TargetSelected += OnTargetSelected;
    }

    private void OnTargetSelected(object? sender, TargetSelectedEventArgs e)
    {
        if (DataContext is CombatViewModel vm)
        {
            // Update target position (single click moves cursor)
            vm.TargetX = e.GridX;
            vm.TargetY = e.GridY;

            // Double-click confirms the target
            if (e.IsDoubleClick)
            {
                vm.ConfirmTargetCommand.Execute(null);
            }
        }
    }

    protected override void OnUnloaded(RoutedEventArgs e)
    {
        base.OnUnloaded(e);
        _renderTimer?.Stop();
        _renderTimer = null;
        CombatMap.TargetSelected -= OnTargetSelected;
    }
}
