using System;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using UltimaIII.Avalonia.Services.Audio;
using UltimaIII.Core.Engine;

namespace UltimaIII.Avalonia.ViewModels;

public partial class LoadGameViewModel : ViewModelBase
{
    private readonly GameEngine _gameEngine;
    private readonly MainViewModel _mainViewModel;
    private readonly IAudioService _audioService;

    [ObservableProperty]
    private int _selectedIndex;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    public ObservableCollection<SaveEntryViewModel> Saves { get; } = new();

    public LoadGameViewModel(GameEngine gameEngine, MainViewModel mainViewModel)
    {
        _gameEngine = gameEngine;
        _mainViewModel = mainViewModel;
        _audioService = AudioService.Instance;
        RefreshSaveList();
    }

    private void RefreshSaveList()
    {
        Saves.Clear();
        var saves = SaveService.GetAllSaves();

        for (int i = 0; i < saves.Count; i++)
        {
            Saves.Add(new SaveEntryViewModel(saves[i], i == SelectedIndex));
        }

        if (Saves.Count > 0 && SelectedIndex >= Saves.Count)
            SelectedIndex = 0;

        UpdateSelection();
    }

    private void UpdateSelection()
    {
        for (int i = 0; i < Saves.Count; i++)
        {
            Saves[i].IsSelected = i == SelectedIndex;
        }
    }

    public void HandleKeyPress(string key)
    {
        if (Saves.Count == 0 && key.ToUpper() != "ESCAPE")
            return;

        switch (key.ToUpper())
        {
            case "W":
            case "UP":
                if (SelectedIndex > 0)
                {
                    SelectedIndex--;
                    UpdateSelection();
                    _audioService.PlaySoundEffect(SoundEffect.MenuSelect);
                }
                break;

            case "S":
            case "DOWN":
                if (SelectedIndex < Saves.Count - 1)
                {
                    SelectedIndex++;
                    UpdateSelection();
                    _audioService.PlaySoundEffect(SoundEffect.MenuSelect);
                }
                break;

            case "RETURN":
                LoadSelected();
                break;

            case "DELETE":
            case "BACK":
                DeleteSelected();
                break;

            case "ESCAPE":
                _audioService.PlaySoundEffect(SoundEffect.MenuCancel);
                _mainViewModel.ReturnToMainMenu();
                break;
        }
    }

    private void LoadSelected()
    {
        if (Saves.Count == 0 || SelectedIndex < 0 || SelectedIndex >= Saves.Count)
            return;

        var entry = Saves[SelectedIndex];

        try
        {
            var save = SaveService.LoadSaveFile(entry.FilePath);
            if (save == null)
            {
                StatusMessage = "Failed to load save file.";
                return;
            }

            _audioService.PlaySoundEffect(SoundEffect.MenuConfirm);
            _gameEngine.NewGame();
            _gameEngine.LoadGame(save);
            _mainViewModel.CurrentView = new GameViewModel(_gameEngine, _mainViewModel);
        }
        catch (Exception ex)
        {
            StatusMessage = $"Load failed: {ex.Message}";
        }
    }

    private void DeleteSelected()
    {
        if (Saves.Count == 0 || SelectedIndex < 0 || SelectedIndex >= Saves.Count)
            return;

        var entry = Saves[SelectedIndex];
        SaveService.DeleteSave(entry.FilePath);
        _audioService.PlaySoundEffect(SoundEffect.MenuCancel);

        RefreshSaveList();

        if (Saves.Count == 0)
        {
            StatusMessage = "No saved games.";
        }
    }
}

public partial class SaveEntryViewModel : ObservableObject
{
    [ObservableProperty]
    private bool _isSelected;

    public string FilePath { get; }
    public string SaveName { get; }
    public string SavedAt { get; }
    public string Summary { get; }
    public string DisplayMarker => IsSelected ? ">" : " ";

    public SaveEntryViewModel(SaveFileInfo info, bool isSelected)
    {
        FilePath = info.FilePath;
        SaveName = info.SaveName;
        SavedAt = info.SavedAt.ToString("yyyy-MM-dd HH:mm");
        Summary = info.Summary;
        IsSelected = isSelected;
    }

    partial void OnIsSelectedChanged(bool value)
    {
        OnPropertyChanged(nameof(DisplayMarker));
    }
}
