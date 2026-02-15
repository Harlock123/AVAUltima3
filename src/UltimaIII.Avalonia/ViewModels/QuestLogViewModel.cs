using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using UltimaIII.Avalonia.Services.Audio;
using UltimaIII.Core.Engine;
using UltimaIII.Core.Models;

namespace UltimaIII.Avalonia.ViewModels;

public partial class QuestLogViewModel : ViewModelBase
{
    private readonly GameEngine _gameEngine;
    private readonly GameViewModel _gameVm;
    private readonly IAudioService _audioService;

    [ObservableProperty]
    private int _selectedIndex;

    [ObservableProperty]
    private string _selectedQuestDetail = string.Empty;

    public ObservableCollection<QuestLogEntryViewModel> Entries { get; } = new();

    public bool HasQuests => Entries.Count > 0;
    public bool NoQuests => Entries.Count == 0;

    public QuestLogViewModel(GameEngine gameEngine, GameViewModel gameVm)
    {
        _gameEngine = gameEngine;
        _gameVm = gameVm;
        _audioService = AudioService.Instance;

        RefreshQuests();
    }

    private void RefreshQuests()
    {
        Entries.Clear();
        SelectedIndex = 0;

        foreach (var progress in _gameEngine.Party.QuestLog.GetAllProgress())
        {
            var quest = QuestRegistry.FindById(progress.QuestId);
            if (quest == null) continue;

            bool isComplete = QuestEngine.IsQuestComplete(_gameEngine.Party, quest);
            Entries.Add(new QuestLogEntryViewModel(quest, progress, isComplete));
        }

        OnPropertyChanged(nameof(HasQuests));
        OnPropertyChanged(nameof(NoQuests));

        if (Entries.Count > 0)
        {
            UpdateSelection();
        }
    }

    private void UpdateSelection()
    {
        for (int i = 0; i < Entries.Count; i++)
        {
            Entries[i].IsSelected = i == SelectedIndex;
        }

        if (SelectedIndex >= 0 && SelectedIndex < Entries.Count)
        {
            var entry = Entries[SelectedIndex];
            SelectedQuestDetail = $"{entry.Quest.Description}\n\nReturn to: {entry.Quest.GiverNpcName} in {FormatTownName(entry.Quest.GiverTownId)}";
        }
        else
        {
            SelectedQuestDetail = "";
        }
    }

    [RelayCommand]
    private void Close()
    {
        _gameVm.CloseQuestLog();
    }

    public void HandleKeyPress(string key)
    {
        switch (key.ToUpper())
        {
            case "W":
            case "UP":
                if (Entries.Count > 0)
                {
                    SelectedIndex = (SelectedIndex - 1 + Entries.Count) % Entries.Count;
                    UpdateSelection();
                    _audioService.PlaySoundEffect(SoundEffect.MenuSelect);
                }
                break;

            case "S":
            case "DOWN":
                if (Entries.Count > 0)
                {
                    SelectedIndex = (SelectedIndex + 1) % Entries.Count;
                    UpdateSelection();
                    _audioService.PlaySoundEffect(SoundEffect.MenuSelect);
                }
                break;

            case "ESCAPE":
            case "J":
                Close();
                _audioService.PlaySoundEffect(SoundEffect.MenuCancel);
                break;
        }
    }

    private static string FormatTownName(string townId)
    {
        return townId.Replace("_", " ")
            .Split(' ')
            .Select(w => w.Length > 0 ? char.ToUpper(w[0]) + w[1..] : w)
            .Aggregate((a, b) => a + " " + b);
    }
}

public partial class QuestLogEntryViewModel : ObservableObject
{
    public QuestDefinition Quest { get; }
    public QuestProgress Progress { get; }
    public bool IsComplete { get; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(DisplayText))]
    private bool _isSelected;

    public string DisplayText
    {
        get
        {
            string prefix = IsSelected ? "> " : "  ";
            string status = IsComplete ? "[DONE] " : "";
            string progressText = Quest.Type switch
            {
                QuestType.Kill => $" ({Progress.KillCount}/{Quest.Objective.TargetCount})",
                QuestType.Fetch => IsComplete ? " (Item found)" : "",
                QuestType.Explore => Progress.LocationVisited ? " (Visited)" : "",
                _ => ""
            };
            return $"{prefix}{status}{Quest.Name}{progressText}";
        }
    }

    public QuestLogEntryViewModel(QuestDefinition quest, QuestProgress progress, bool isComplete)
    {
        Quest = quest;
        Progress = progress;
        IsComplete = isComplete;
    }
}
