using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using UltimaIII.Avalonia.Services.Audio;
using UltimaIII.Core.Engine;
using UltimaIII.Core.Enums;
using UltimaIII.Core.Models;

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

        HasSaveFile = SaveService.HasAnySaves();

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
    private void LoadGame()
    {
        _audioService.PlaySoundEffect(SoundEffect.MenuConfirm);
        CurrentView = new LoadGameViewModel(_gameEngine, this);
        IsMainMenuVisible = false;
    }

    public void ReturnToMainMenu()
    {
        HasSaveFile = SaveService.HasAnySaves();
        CurrentView = null;
        IsMainMenuVisible = true;
        _audioService.PlayMusic(MusicTrack.MainMenu);
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
                _audioService.PlayMusic(ResolveCombatMusic());
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
                _audioService.PlayMusic(ResolveDungeonMusic());
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

    private MusicTrack ResolveDungeonMusic()
    {
        var mapId = _gameEngine.Party.CurrentMapId;
        bool isDeep = _gameEngine.Party.DungeonLevel >= 5;

        return GetDungeonName(mapId) switch
        {
            "doom" => isDeep ? MusicTrack.DungeonDoomDeep : MusicTrack.DungeonDoom,
            "fire" => isDeep ? MusicTrack.DungeonFireDeep : MusicTrack.DungeonFire,
            "time" => isDeep ? MusicTrack.DungeonTimeDeep : MusicTrack.DungeonTime,
            "snake" => isDeep ? MusicTrack.DungeonSnakeDeep : MusicTrack.DungeonSnake,
            _ => MusicTrack.Dungeon
        };
    }

    private MusicTrack ResolveCombatMusic()
    {
        if (_gameEngine.CurrentMap?.MapType != MapType.Dungeon)
            return MusicTrack.Combat;

        var mapId = _gameEngine.Party.CurrentMapId;
        return GetDungeonName(mapId) switch
        {
            "doom" => MusicTrack.CombatDoom,
            "fire" => MusicTrack.CombatFire,
            "time" => MusicTrack.CombatTime,
            "snake" => MusicTrack.CombatSnake,
            _ => MusicTrack.Combat
        };
    }

    private static string? GetDungeonName(string mapId)
    {
        // mapId format: "dungeon_doom_l3"
        if (!mapId.StartsWith("dungeon_")) return null;
        var withoutPrefix = mapId["dungeon_".Length..];
        var underscoreIndex = withoutPrefix.IndexOf('_');
        return underscoreIndex > 0 ? withoutPrefix[..underscoreIndex] : withoutPrefix;
    }
}
