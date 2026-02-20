using Avalonia.Controls;
using Avalonia.Input;
using UltimaIII.Avalonia.ViewModels;

namespace UltimaIII.Avalonia.Views;

public partial class ScreenshotView : UserControl
{
    public ScreenshotView()
    {
        InitializeComponent();

        var textBox = this.FindControl<TextBox>("ScreenshotNameBox");
        if (textBox != null)
        {
            textBox.AttachedToVisualTree += (_, _) =>
            {
                textBox.Focus();
                textBox.SelectAll();
            };
            textBox.KeyDown += OnTextBoxKeyDown;
        }
    }

    private void OnTextBoxKeyDown(object? sender, KeyEventArgs e)
    {
        if (DataContext is not ScreenshotViewModel vm) return;

        switch (e.Key)
        {
            case Key.Return:
                vm.ConfirmScreenshotCommand.Execute(null);
                e.Handled = true;
                break;
            case Key.Escape:
                vm.CancelCommand.Execute(null);
                e.Handled = true;
                break;
        }
    }
}
