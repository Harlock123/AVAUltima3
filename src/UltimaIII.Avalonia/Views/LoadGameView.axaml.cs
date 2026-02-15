using Avalonia.Controls;
using Avalonia.Input;
using UltimaIII.Avalonia.ViewModels;

namespace UltimaIII.Avalonia.Views;

public partial class LoadGameView : UserControl
{
    public LoadGameView()
    {
        InitializeComponent();
    }

    private void SaveEntry_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (sender is Border border && border.DataContext is SaveEntryViewModel entry &&
            DataContext is LoadGameViewModel vm)
        {
            vm.SelectAndLoad(entry);
            e.Handled = true;
        }
    }
}
