using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using UltimaIII.Avalonia.ViewModels;

namespace UltimaIII.Avalonia;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainViewModel();

        // Use tunneling event to capture keys before any child control
        AddHandler(KeyDownEvent, OnPreviewKeyDown, RoutingStrategies.Tunnel);

        // Ensure window can receive focus
        Focusable = true;

        // Grab focus when window is activated
        Activated += (s, e) => Focus();
    }

    private void OnPreviewKeyDown(object? sender, KeyEventArgs e)
    {
        if (DataContext is not MainViewModel mainVm) return;

        if (mainVm.CurrentView is GameViewModel gameVm)
        {
            // Let TextBox controls handle input when save dialog is open
            if (gameVm.IsSaveMode) return;

            string key = e.Key.ToString();
            gameVm.HandleKeyPress(key);
            e.Handled = true;
        }
        else if (mainVm.CurrentView is LoadGameViewModel loadVm)
        {
            string key = e.Key.ToString();
            loadVm.HandleKeyPress(key);
            e.Handled = true;
        }
    }
}
