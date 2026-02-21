using System;
using System.ComponentModel;
using Avalonia.Controls;
using UltimaIII.Avalonia.ViewModels;

namespace UltimaIII.Avalonia.Views;

public partial class ShopView : UserControl
{
    private ShopViewModel? _subscribedVm;

    public ShopView()
    {
        InitializeComponent();
        DataContextChanged += OnDataContextChanged;
    }

    private void OnDataContextChanged(object? sender, EventArgs e)
    {
        if (_subscribedVm != null)
        {
            _subscribedVm.PropertyChanged -= OnVmPropertyChanged;
            _subscribedVm = null;
        }

        if (DataContext is ShopViewModel vm)
        {
            vm.PropertyChanged += OnVmPropertyChanged;
            _subscribedVm = vm;
        }
    }

    private void OnVmPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ShopViewModel.SelectedItemIndex) && _subscribedVm != null)
        {
            ScrollItemIntoView(_subscribedVm.SelectedItemIndex);
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
            sv.Offset = new global::Avalonia.Vector(0, targetBottom - viewportHeight);
        else if (targetTop < currentOffset)
            sv.Offset = new global::Avalonia.Vector(0, targetTop);
    }
}
