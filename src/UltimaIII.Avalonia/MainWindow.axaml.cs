using Avalonia.Controls;
using Avalonia.Input;
using UltimaIII.Avalonia.ViewModels;

namespace UltimaIII.Avalonia;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainViewModel();

        KeyDown += OnKeyDown;
    }

    private void OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (DataContext is MainViewModel mainVm && mainVm.CurrentView is GameViewModel gameVm)
        {
            string key = e.Key.ToString();
            gameVm.HandleKeyPress(key);
            e.Handled = true;
        }
    }
}
