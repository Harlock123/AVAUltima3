using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using UltimaIII.Core.Engine;
using UltimaIII.Core.Enums;

namespace UltimaIII.Avalonia.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    private readonly GameEngine _gameEngine;

    [ObservableProperty]
    private ViewModelBase? _currentView;

    [ObservableProperty]
    private bool _isMainMenuVisible = true;

    public MainViewModel()
    {
        _gameEngine = new GameEngine();
        _gameEngine.OnStateChanged += OnGameStateChanged;
    }

    public GameEngine GameEngine => _gameEngine;

    [RelayCommand]
    private void NewGame()
    {
        _gameEngine.NewGame();
        CurrentView = new CharacterCreationViewModel(_gameEngine, this);
        IsMainMenuVisible = false;
    }

    [RelayCommand]
    private void ContinueGame()
    {
        // TODO: Implement save/load
    }

    [RelayCommand]
    private void Exit()
    {
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
                if (CurrentView is GameViewModel gameVm)
                {
                    gameVm.EnterCombat();
                }
                break;

            case GameState.Overworld:
            case GameState.Town:
            case GameState.Dungeon:
                if (CurrentView is GameViewModel)
                {
                    // Already in game view
                }
                else
                {
                    CurrentView = new GameViewModel(_gameEngine, this);
                }
                break;

            case GameState.GameOver:
                // Show game over screen
                IsMainMenuVisible = true;
                CurrentView = null;
                break;
        }
    }
}
