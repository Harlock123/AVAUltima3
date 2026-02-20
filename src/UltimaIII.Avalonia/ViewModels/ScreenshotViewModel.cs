using System;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Avalonia.Media.Imaging;
using UltimaIII.Avalonia.Services.Audio;

namespace UltimaIII.Avalonia.ViewModels;

public partial class ScreenshotViewModel : ViewModelBase
{
    private readonly MainViewModel _parentViewModel;
    private readonly IAudioService _audioService;
    private readonly RenderTargetBitmap _capturedBitmap;

    [ObservableProperty]
    private string _screenshotName = string.Empty;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    public ScreenshotViewModel(MainViewModel parentViewModel, RenderTargetBitmap capturedBitmap, string defaultName)
    {
        _parentViewModel = parentViewModel;
        _audioService = AudioService.Instance;
        _capturedBitmap = capturedBitmap;
        ScreenshotName = defaultName;
    }

    [RelayCommand]
    public void ConfirmScreenshot()
    {
        if (string.IsNullOrWhiteSpace(ScreenshotName))
        {
            StatusMessage = "Screenshot name cannot be empty.";
            return;
        }

        try
        {
            var dir = Path.Combine(AppContext.BaseDirectory, "Screenshots");
            Directory.CreateDirectory(dir);

            var safeName = string.Join("_", ScreenshotName.Trim().Split(Path.GetInvalidFileNameChars()));
            var filePath = Path.Combine(dir, $"{safeName}.png");

            int counter = 1;
            while (File.Exists(filePath))
            {
                filePath = Path.Combine(dir, $"{safeName}_{counter}.png");
                counter++;
            }

            using var stream = File.Create(filePath);
            _capturedBitmap.Save(stream);

            _audioService.PlaySoundEffect(SoundEffect.MenuConfirm);
            _parentViewModel.CloseScreenshot($"Screenshot saved: {Path.GetFileName(filePath)}");
        }
        catch (Exception ex)
        {
            StatusMessage = $"Failed to save: {ex.Message}";
        }
    }

    [RelayCommand]
    public void Cancel()
    {
        _audioService.PlaySoundEffect(SoundEffect.MenuCancel);
        _parentViewModel.CloseScreenshot();
    }

    public void Dispose()
    {
        _capturedBitmap.Dispose();
    }
}
