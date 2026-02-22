using System;
using System.ComponentModel;
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
            _subscribedVm.PropertyChanged -= OnVmPropertyChanged;
            _subscribedVm = null;
        }

        // Subscribe to new VM
        if (DataContext is CombatViewModel vm)
        {
            vm.OnSpellEffect += OnSpellEffect;
            vm.PropertyChanged += OnVmPropertyChanged;
            _subscribedVm = vm;
        }
    }

    private void OnVmPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(CombatViewModel.SelectedSpellIndex) && _subscribedVm != null)
        {
            ScrollSpellIntoView(_subscribedVm.SelectedSpellIndex);
        }
        else if (e.PropertyName == nameof(CombatViewModel.SelectedItemIndex) && _subscribedVm != null)
        {
            ScrollItemIntoView(_subscribedVm.SelectedItemIndex);
        }
    }

    private void ScrollSpellIntoView(int index)
    {
        if (index < 0) return;

        // Each spell row is roughly 27px (13px font + 6+3 padding + 2 margin)
        const double rowHeight = 27;
        double targetTop = index * rowHeight;
        double targetBottom = targetTop + rowHeight;

        var sv = SpellScrollViewer;
        if (sv == null) return;

        var viewportHeight = sv.Viewport.Height;
        var currentOffset = sv.Offset.Y;

        if (targetBottom > currentOffset + viewportHeight)
        {
            sv.Offset = new global::Avalonia.Vector(0, targetBottom - viewportHeight);
        }
        else if (targetTop < currentOffset)
        {
            sv.Offset = new global::Avalonia.Vector(0, targetTop);
        }
    }

    private void ScrollItemIntoView(int index)
    {
        if (index < 0) return;

        const double rowHeight = 27;
        double targetTop = index * rowHeight;
        double targetBottom = targetTop + rowHeight;

        var sv = ItemScrollViewer;
        if (sv == null) return;

        var viewportHeight = sv.Viewport.Height;
        var currentOffset = sv.Offset.Y;

        if (targetBottom > currentOffset + viewportHeight)
        {
            sv.Offset = new global::Avalonia.Vector(0, targetBottom - viewportHeight);
        }
        else if (targetTop < currentOffset)
        {
            sv.Offset = new global::Avalonia.Vector(0, targetTop);
        }
    }

    private void OnSpellEffect(int x, int y, bool isBeneficial, bool isAoe)
    {
        Dispatcher.UIThread.Post(() => CombatMap?.ShowSpellEffect(x, y, isBeneficial, isAoe));
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
            _subscribedVm.PropertyChanged -= OnVmPropertyChanged;
            _subscribedVm = null;
        }
    }
}
