using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using UltimaIII.Avalonia.Services.Audio;
using UltimaIII.Core.Engine;
using UltimaIII.Core.Models;

namespace UltimaIII.Avalonia.ViewModels;

public partial class QuestDialogViewModel : ViewModelBase
{
    private readonly GameEngine _gameEngine;
    private readonly GameViewModel _gameVm;
    private readonly IAudioService _audioService;
    private readonly string _townId;
    private readonly string _npcName;

    public enum DialogMode { QuestList, QuestDetail, TurnIn }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsQuestList))]
    [NotifyPropertyChangedFor(nameof(IsQuestDetail))]
    [NotifyPropertyChangedFor(nameof(IsTurnIn))]
    private DialogMode _mode = DialogMode.QuestList;

    [ObservableProperty]
    private string _npcDisplayName = string.Empty;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    [ObservableProperty]
    private int _selectedIndex;

    // Quest detail fields
    [ObservableProperty]
    private string _questName = string.Empty;

    [ObservableProperty]
    private string _questDescription = string.Empty;

    [ObservableProperty]
    private string _questOfferText = string.Empty;

    [ObservableProperty]
    private string _questObjectiveText = string.Empty;

    [ObservableProperty]
    private string _questRewardText = string.Empty;

    private QuestDefinition? _selectedQuest;

    public ObservableCollection<QuestListItemViewModel> QuestItems { get; } = new();

    public bool IsQuestList => Mode == DialogMode.QuestList;
    public bool IsQuestDetail => Mode == DialogMode.QuestDetail;
    public bool IsTurnIn => Mode == DialogMode.TurnIn;

    public QuestDialogViewModel(GameEngine gameEngine, GameViewModel gameVm, string townId, string npcName)
    {
        _gameEngine = gameEngine;
        _gameVm = gameVm;
        _audioService = AudioService.Instance;
        _townId = townId;
        _npcName = npcName;
        NpcDisplayName = npcName;

        RefreshQuestList();
    }

    private void RefreshQuestList()
    {
        QuestItems.Clear();
        SelectedIndex = 0;

        // Turn-in quests first
        var turnInQuests = QuestEngine.GetTurnInQuests(_gameEngine.Party, _townId, _npcName);
        foreach (var q in turnInQuests)
        {
            QuestItems.Add(new QuestListItemViewModel(q, QuestListItemType.TurnIn));
        }

        // In-progress quests
        var inProgressQuests = QuestEngine.GetInProgressQuests(_gameEngine.Party, _townId, _npcName);
        foreach (var q in inProgressQuests)
        {
            var progress = _gameEngine.Party.QuestLog.GetProgress(q.Id);
            QuestItems.Add(new QuestListItemViewModel(q, QuestListItemType.InProgress, progress));
        }

        // Available quests
        var availableQuests = QuestEngine.GetAvailableQuests(_gameEngine.Party, _townId, _npcName);
        foreach (var q in availableQuests)
        {
            QuestItems.Add(new QuestListItemViewModel(q, QuestListItemType.Available));
        }

        if (QuestItems.Count == 0)
        {
            StatusMessage = "No quests available.";
        }
        else
        {
            StatusMessage = "";
            UpdateSelection();
        }
    }

    private void UpdateSelection()
    {
        for (int i = 0; i < QuestItems.Count; i++)
        {
            QuestItems[i].IsSelected = i == SelectedIndex;
        }
    }

    private void ShowQuestDetail(QuestDefinition quest, QuestListItemType type)
    {
        _selectedQuest = quest;
        QuestName = quest.Name;
        QuestDescription = quest.Description;

        if (type == QuestListItemType.TurnIn)
        {
            Mode = DialogMode.TurnIn;
            QuestOfferText = quest.CompleteText;
            QuestRewardText = FormatReward(quest.Reward);
            QuestObjectiveText = "COMPLETE!";
        }
        else if (type == QuestListItemType.InProgress)
        {
            Mode = DialogMode.QuestDetail;
            var progress = _gameEngine.Party.QuestLog.GetProgress(quest.Id);
            QuestOfferText = quest.ProgressText;
            QuestObjectiveText = FormatProgress(quest, progress);
            QuestRewardText = FormatReward(quest.Reward);
        }
        else
        {
            Mode = DialogMode.QuestDetail;
            QuestOfferText = quest.OfferText;
            QuestObjectiveText = FormatObjective(quest);
            QuestRewardText = FormatReward(quest.Reward);
        }
    }

    [RelayCommand]
    private void Accept()
    {
        if (_selectedQuest == null) return;

        if (Mode == DialogMode.TurnIn)
        {
            var (gold, xp, itemId) = QuestEngine.TurnInQuest(_gameEngine.Party, _selectedQuest);
            _gameEngine.AddMessage($"Quest complete: {_selectedQuest.Name}!");
            _gameEngine.AddMessage($"Received {gold} gold and {xp} experience.");
            if (!string.IsNullOrEmpty(itemId))
            {
                var rewardItem = ItemRegistry.FindById(itemId);
                if (rewardItem != null)
                    _gameEngine.AddMessage($"Received {rewardItem.Name}!");
            }
            _audioService.PlaySoundEffect(SoundEffect.LevelUp);
            _gameVm.RefreshPartyDisplay();

            Mode = DialogMode.QuestList;
            RefreshQuestList();
        }
        else if (Mode == DialogMode.QuestDetail)
        {
            // Only accept if it's a new quest (not already active)
            if (!_gameEngine.Party.QuestLog.IsQuestActive(_selectedQuest.Id) &&
                !_gameEngine.Party.CompletedQuests.Contains(_selectedQuest.Id))
            {
                QuestEngine.AcceptQuest(_gameEngine.Party, _selectedQuest);
                _gameEngine.AddMessage($"Quest accepted: {_selectedQuest.Name}");
                _audioService.PlaySoundEffect(SoundEffect.MenuConfirm);

                Mode = DialogMode.QuestList;
                RefreshQuestList();
            }
            else
            {
                // Already active, just go back
                Mode = DialogMode.QuestList;
            }
        }
    }

    [RelayCommand]
    private void Decline()
    {
        Mode = DialogMode.QuestList;
        _selectedQuest = null;
    }

    [RelayCommand]
    private void Close()
    {
        _gameVm.CloseQuestDialog();
    }

    public void HandleKeyPress(string key)
    {
        switch (key.ToUpper())
        {
            case "W":
            case "UP":
                if (Mode == DialogMode.QuestList && QuestItems.Count > 0)
                {
                    SelectedIndex = (SelectedIndex - 1 + QuestItems.Count) % QuestItems.Count;
                    UpdateSelection();
                    _audioService.PlaySoundEffect(SoundEffect.MenuSelect);
                }
                break;

            case "S":
            case "DOWN":
                if (Mode == DialogMode.QuestList && QuestItems.Count > 0)
                {
                    SelectedIndex = (SelectedIndex + 1) % QuestItems.Count;
                    UpdateSelection();
                    _audioService.PlaySoundEffect(SoundEffect.MenuSelect);
                }
                break;

            case "RETURN":
            case "ENTER":
                if (Mode == DialogMode.QuestList && QuestItems.Count > 0 && SelectedIndex < QuestItems.Count)
                {
                    var item = QuestItems[SelectedIndex];
                    ShowQuestDetail(item.Quest, item.Type);
                    _audioService.PlaySoundEffect(SoundEffect.MenuConfirm);
                }
                else if (Mode == DialogMode.QuestDetail || Mode == DialogMode.TurnIn)
                {
                    Accept();
                }
                break;

            case "ESCAPE":
                if (Mode == DialogMode.QuestDetail || Mode == DialogMode.TurnIn)
                {
                    Decline();
                    _audioService.PlaySoundEffect(SoundEffect.MenuCancel);
                }
                else
                {
                    Close();
                    _audioService.PlaySoundEffect(SoundEffect.MenuCancel);
                }
                break;
        }
    }

    private static string FormatObjective(QuestDefinition quest)
    {
        return quest.Type switch
        {
            QuestType.Kill => $"Slay {quest.Objective.TargetCount} {FormatMonsterName(quest.Objective.TargetMonsterIds)}",
            QuestType.Fetch => $"Find: {GetItemName(quest.Objective.RequiredItemId)}",
            QuestType.Explore => $"Visit: {FormatMapName(quest.Objective.TargetMapId)}",
            _ => "Unknown objective"
        };
    }

    private static string FormatProgress(QuestDefinition quest, QuestProgress? progress)
    {
        if (progress == null) return FormatObjective(quest);

        return quest.Type switch
        {
            QuestType.Kill => $"Slain: {progress.KillCount}/{quest.Objective.TargetCount} {FormatMonsterName(quest.Objective.TargetMonsterIds)}",
            QuestType.Fetch => progress.LocationVisited ? "Item not yet found" : $"Find: {GetItemName(quest.Objective.RequiredItemId)}",
            QuestType.Explore => progress.LocationVisited ? "Location visited - return to quest giver!" : $"Visit: {FormatMapName(quest.Objective.TargetMapId)}",
            _ => "Unknown"
        };
    }

    private static string FormatReward(QuestReward reward)
    {
        var parts = new System.Collections.Generic.List<string>();
        if (reward.Gold > 0) parts.Add($"{reward.Gold} gold");
        if (reward.Experience > 0) parts.Add($"{reward.Experience} XP");
        return string.Join(", ", parts);
    }

    private static string FormatMonsterName(string[] monsterIds)
    {
        if (monsterIds.Length == 0) return "monsters";
        if (monsterIds.Length > 2) return "undead";
        return monsterIds[0].Replace("_", " ").Replace("giant ", "Giant ");
    }

    private static string GetItemName(string itemId)
    {
        var item = ItemRegistry.FindById(itemId);
        return item?.Name ?? itemId.Replace("quest_", "").Replace("_", " ");
    }

    private static string FormatMapName(string mapId)
    {
        return mapId.Replace("dungeon_", "Dungeon of ").Replace("_l", " Level ").Replace("_", " ");
    }
}

public enum QuestListItemType { Available, InProgress, TurnIn }

public partial class QuestListItemViewModel : ObservableObject
{
    public QuestDefinition Quest { get; }
    public QuestListItemType Type { get; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(DisplayText))]
    private bool _isSelected;

    public string DisplayText
    {
        get
        {
            string prefix = IsSelected ? "> " : "  ";
            string tag = Type switch
            {
                QuestListItemType.TurnIn => "[COMPLETE] ",
                QuestListItemType.InProgress => "[Active] ",
                QuestListItemType.Available => "[New] ",
                _ => ""
            };
            string progress = "";
            if (Type == QuestListItemType.InProgress && _progress != null && Quest.Type == QuestType.Kill)
            {
                progress = $" ({_progress.KillCount}/{Quest.Objective.TargetCount})";
            }
            return $"{prefix}{tag}{Quest.Name}{progress}";
        }
    }

    private readonly QuestProgress? _progress;

    public QuestListItemViewModel(QuestDefinition quest, QuestListItemType type, QuestProgress? progress = null)
    {
        Quest = quest;
        Type = type;
        _progress = progress;
    }
}
