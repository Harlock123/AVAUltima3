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
    private CombatViewModel? _subscribedVm;

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

        // Subscribe to spell effects when DataContext is set
        DataContextChanged += OnDataContextChanged;
    }

    private void OnDataContextChanged(object? sender, EventArgs e)
    {
        // Unsubscribe from old VM
        if (_subscribedVm != null)
        {
            _subscribedVm.OnSpellEffect -= OnSpellEffect;
            _subscribedVm = null;
        }

        // Subscribe to new VM
        if (DataContext is CombatViewModel vm)
        {
            vm.OnSpellEffect += OnSpellEffect;
            _subscribedVm = vm;
        }
    }

    private void OnSpellEffect(int x, int y, bool isBeneficial)
    {
        Dispatcher.UIThread.Post(() => CombatMap?.ShowSpellEffect(x, y, isBeneficial));
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

        if (_subscribedVm != null)
        {
            _subscribedVm.OnSpellEffect -= OnSpellEffect;
            _subscribedVm = null;
        }
    }
}
