using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using UltimaIII.Avalonia.Services.Audio;
using UltimaIII.Core.Engine;

namespace UltimaIII.Avalonia.ViewModels;

public partial class QuitDialogViewModel : ViewModelBase
{
    private readonly GameEngine _gameEngine;
    private readonly GameViewModel _parentViewModel;
    private readonly IAudioService _audioService;

    [ObservableProperty]
    private bool _showSavePrompt;

    [ObservableProperty]
    private string _farewellMessage = "See you again soon, adventurer...";

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    public QuitDialogViewModel(GameEngine gameEngine, GameViewModel parentViewModel, bool recentlySaved)
    {
        _gameEngine = gameEngine;
        _parentViewModel = parentViewModel;
        _audioService = AudioService.Instance;
        ShowSavePrompt = !recentlySaved;
    }

    [RelayCommand]
    private void SaveAndQuit()
    {
        try
        {
            var mapName = _gameEngine.CurrentMap?.Name ?? "Unknown";
            var saveName = $"Day {_gameEngine.Party.DayCount} - {mapName}";
            SaveService.SaveGame(_gameEngine, saveName);
            _audioService.PlaySoundEffect(SoundEffect.MenuConfirm);
            ShowSavePrompt = false;
            FarewellMessage = "Game saved. See you again soon, adventurer...";
            // Brief delay then quit
            QuitGame();
        }
        catch (Exception ex)
        {
            StatusMessage = $"Save failed: {ex.Message}";
        }
    }

    [RelayCommand]
    private void QuitWithoutSaving()
    {
        _audioService.PlaySoundEffect(SoundEffect.MenuConfirm);
        ShowSavePrompt = false;
        QuitGame();
    }

    [RelayCommand]
    private void ConfirmQuit()
    {
        QuitGame();
    }

    [RelayCommand]
    private void Cancel()
    {
        _audioService.PlaySoundEffect(SoundEffect.MenuCancel);
        _parentViewModel.CloseQuitDialog();
    }

    private void QuitGame()
    {
        Environment.Exit(0);
    }
}
