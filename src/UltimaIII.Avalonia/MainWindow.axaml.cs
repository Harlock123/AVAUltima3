using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media.Imaging;
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

        // Screenshot: Ctrl+Shift+S (works on any screen)
        if (e.Key == Key.S &&
            e.KeyModifiers.HasFlag(KeyModifiers.Control) &&
            e.KeyModifiers.HasFlag(KeyModifiers.Shift))
        {
            if (!mainVm.IsScreenshotMode)
                CaptureScreenshot(mainVm);
            e.Handled = true;
            return;
        }

        // Let TextBox handle input when screenshot dialog is open
        if (mainVm.IsScreenshotMode) return;

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

    private void CaptureScreenshot(MainViewModel mainVm)
    {
        try
        {
            var scaling = RenderScaling;
            var pixelWidth = (int)(Bounds.Width * scaling);
            var pixelHeight = (int)(Bounds.Height * scaling);

            if (pixelWidth <= 0 || pixelHeight <= 0) return;

            var bitmap = new RenderTargetBitmap(
                new PixelSize(pixelWidth, pixelHeight),
                new Vector(96 * scaling, 96 * scaling));
            bitmap.Render(this);

            var screenName = mainVm.GetCurrentScreenName();
            var defaultName = $"{screenName}_{DateTime.Now:yyyy-MM-dd}";

            mainVm.OpenScreenshot(bitmap, defaultName);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Screenshot capture failed: {ex.Message}");
        }
    }
}
