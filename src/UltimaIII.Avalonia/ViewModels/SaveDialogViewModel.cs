using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using UltimaIII.Avalonia.Services.Audio;
using UltimaIII.Core.Engine;

namespace UltimaIII.Avalonia.ViewModels;

public partial class SaveDialogViewModel : ViewModelBase
{
    private readonly GameEngine _gameEngine;
    private readonly GameViewModel _parentViewModel;
    private readonly IAudioService _audioService;

    [ObservableProperty]
    private string _saveName = string.Empty;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    public SaveDialogViewModel(GameEngine gameEngine, GameViewModel parentViewModel)
    {
        _gameEngine = gameEngine;
        _parentViewModel = parentViewModel;
        _audioService = AudioService.Instance;

        // Pre-fill with contextual default name
        var mapName = gameEngine.CurrentMap?.Name ?? "Unknown";
        SaveName = $"Day {gameEngine.Party.DayCount} - {mapName}";
    }

    [RelayCommand]
    public void ConfirmSave()
    {
        if (string.IsNullOrWhiteSpace(SaveName))
        {
            StatusMessage = "Save name cannot be empty.";
            return;
        }

        try
        {
            SaveService.SaveGame(_gameEngine, SaveName.Trim());
            _gameEngine.AddMessage("Game saved.");
            _audioService.PlaySoundEffect(SoundEffect.MenuConfirm);
            _parentViewModel.CloseSaveDialog(saved: true);
        }
        catch (Exception ex)
        {
            StatusMessage = $"Save failed: {ex.Message}";
        }
    }

    [RelayCommand]
    public void Cancel()
    {
        _audioService.PlaySoundEffect(SoundEffect.MenuCancel);
        _parentViewModel.CloseSaveDialog();
    }
}
