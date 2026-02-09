using Avalonia.Controls;
using Avalonia.Input;
using UltimaIII.Avalonia.ViewModels;

namespace UltimaIII.Avalonia.Views;

public partial class SaveDialogView : UserControl
{
    public SaveDialogView()
    {
        InitializeComponent();

        var textBox = this.FindControl<TextBox>("SaveNameBox");
        if (textBox != null)
        {
            textBox.AttachedToVisualTree += (_, _) => textBox.Focus();
            textBox.KeyDown += OnTextBoxKeyDown;
        }
    }

    private void OnTextBoxKeyDown(object? sender, KeyEventArgs e)
    {
        if (DataContext is not SaveDialogViewModel vm) return;

        switch (e.Key)
        {
            case Key.Return:
                vm.ConfirmSaveCommand.Execute(null);
                e.Handled = true;
                break;
            case Key.Escape:
                vm.CancelCommand.Execute(null);
                e.Handled = true;
                break;
        }
    }
}
