using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using UltimaIII.Avalonia.Services.Audio;
using UltimaIII.Core.Engine;
using UltimaIII.Core.Enums;

namespace UltimaIII.Avalonia.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    private readonly GameEngine _gameEngine;
    private readonly IAudioService _audioService;

    [ObservableProperty]
    private ViewModelBase? _currentView;

    [ObservableProperty]
    private bool _isMainMenuVisible = true;

    [ObservableProperty]
    private bool _hasSaveFile;

    public MainViewModel()
    {
        _gameEngine = new GameEngine();
        _gameEngine.OnStateChanged += OnGameStateChanged;
        _audioService = AudioService.Instance;

        HasSaveFile = SaveService.SaveExists();

        // Start main menu music
        _audioService.PlayMusic(MusicTrack.MainMenu);
    }

    public GameEngine GameEngine => _gameEngine;

    [RelayCommand]
    private void NewGame()
    {
        _audioService.PlaySoundEffect(SoundEffect.MenuConfirm);
        _gameEngine.NewGame();
        CurrentView = new FortuneTellerViewModel(_gameEngine, this);
        IsMainMenuVisible = false;
    }

    [RelayCommand]
    private void CustomGame()
    {
        _audioService.PlaySoundEffect(SoundEffect.MenuConfirm);
        _gameEngine.NewGame();
        CurrentView = new CharacterCreationViewModel(_gameEngine, this);
        IsMainMenuVisible = false;
    }

    [RelayCommand]
    private void ContinueGame()
    {
        _audioService.PlaySoundEffect(SoundEffect.MenuConfirm);

        var save = SaveService.LoadSaveFile();
        if (save == null) return;

        _gameEngine.NewGame();
        _gameEngine.LoadGame(save);

        CurrentView = new GameViewModel(_gameEngine, this);
        IsMainMenuVisible = false;
    }

    [RelayCommand]
    private void Exit()
    {
        _audioService.PlaySoundEffect(SoundEffect.MenuCancel);
        Environment.Exit(0);
    }

    public void StartGame()
    {
        _gameEngine.StartGame();
        CurrentView = new GameViewModel(_gameEngine, this);
    }

    private void OnGameStateChanged(GameState newState)
    {
        switch (newState)
        {
            case GameState.Combat:
                _audioService.PlayMusic(MusicTrack.Combat);
                break;

            case GameState.Overworld:
                _audioService.PlayMusic(MusicTrack.Overworld);
                if (CurrentView is not GameViewModel)
                {
                    CurrentView = new GameViewModel(_gameEngine, this);
                }
                break;

            case GameState.Town:
                _audioService.PlayMusic(MusicTrack.Town);
                if (CurrentView is not GameViewModel)
                {
                    CurrentView = new GameViewModel(_gameEngine, this);
                }
                break;

            case GameState.Dungeon:
                _audioService.PlayMusic(MusicTrack.Dungeon);
                if (CurrentView is not GameViewModel)
                {
                    CurrentView = new GameViewModel(_gameEngine, this);
                }
                break;

            case GameState.GameOver:
                _audioService.PlayMusic(MusicTrack.Defeat);
                // Show game over screen
                IsMainMenuVisible = true;
                CurrentView = null;
                break;
        }
    }
}
