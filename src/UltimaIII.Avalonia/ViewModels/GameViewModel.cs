using System;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using UltimaIII.Avalonia.Services.Audio;
using UltimaIII.Core.Engine;
using UltimaIII.Core.Enums;
using UltimaIII.Core.Models;

namespace UltimaIII.Avalonia.ViewModels;

public partial class GameViewModel : ViewModelBase
{
    private readonly GameEngine _gameEngine;
    private readonly MainViewModel _mainViewModel;
    private readonly IAudioService _audioService;

    [ObservableProperty]
    private bool _isCombatMode = false;

    [ObservableProperty]
    private string _locationName = string.Empty;

    [ObservableProperty]
    private string _timeDisplay = string.Empty;

    [ObservableProperty]
    private int _gold;

    [ObservableProperty]
    private int _food;

    // Viewport settings
    public const int ViewportWidth = 15;
    public const int ViewportHeight = 15;
    public const int TileSize = 32;

    [ObservableProperty]
    private bool _isShopMode = false;

    [ObservableProperty]
    private ShopViewModel? _shopVm;

    [ObservableProperty]
    private CombatViewModel? _combatVm;

    [ObservableProperty]
    private bool _isInventoryMode = false;

    [ObservableProperty]
    private InventoryViewModel? _inventoryVm;

    [ObservableProperty]
    private bool _isSaveMode = false;

    [ObservableProperty]
    private SaveDialogViewModel? _saveDialogVm;

    [ObservableProperty]
    private bool _isQuitMode = false;

    [ObservableProperty]
    private QuitDialogViewModel? _quitDialogVm;

    [ObservableProperty]
    private bool _isQuestDialogMode = false;

    [ObservableProperty]
    private QuestDialogViewModel? _questDialogVm;

    [ObservableProperty]
    private bool _isQuestLogMode = false;

    [ObservableProperty]
    private QuestLogViewModel? _questLogVm;

    [ObservableProperty]
    private bool _isFieldSpellMode = false;

    [ObservableProperty]
    private FieldSpellViewModel? _fieldSpellVm;

    private bool _recentlySaved;

    public ObservableCollection<PartyMemberViewModel> PartyMembers { get; } = new();
    public ObservableCollection<string> MessageLog { get; } = new();

    public GameEngine GameEngine => _gameEngine;

    public GameViewModel(GameEngine gameEngine, MainViewModel mainViewModel)
    {
        _gameEngine = gameEngine;
        _mainViewModel = mainViewModel;
        _audioService = AudioService.Instance;

        // Subscribe to game events
        _gameEngine.OnMessage += OnGameMessage;
        _gameEngine.OnPartyMoved += OnPartyMoved;
        _gameEngine.OnMapChanged += OnMapChanged;
        _gameEngine.OnStateChanged += OnStateChanged;

        // Initialize party display
        RefreshPartyDisplay();
        RefreshDisplay();
    }

    private void OnGameMessage(string message)
    {
        MessageLog.Add(message);
        while (MessageLog.Count > 10)
            MessageLog.RemoveAt(0);

        // Play contextual sound effects based on message content
        PlayMessageSound(message);
    }

    private void PlayMessageSound(string message)
    {
        var lowerMessage = message.ToLowerInvariant();

        if (lowerMessage.Contains("blocked") || lowerMessage.Contains("can't go"))
        {
            _audioService.PlaySoundEffect(SoundEffect.Blocked);
        }
        else if (lowerMessage.Contains("opened") || lowerMessage.Contains("door opens"))
        {
            _audioService.PlaySoundEffect(SoundEffect.DoorOpen);
        }
        else if (lowerMessage.Contains("locked"))
        {
            _audioService.PlaySoundEffect(SoundEffect.DoorLocked);
        }
        else if (lowerMessage.Contains("gold") || lowerMessage.Contains("coins"))
        {
            _audioService.PlaySoundEffect(SoundEffect.GoldPickup);
        }
        else if (lowerMessage.Contains("found") || lowerMessage.Contains("picked up"))
        {
            _audioService.PlaySoundEffect(SoundEffect.ItemPickup);
        }
        else if (lowerMessage.Contains("level up") || lowerMessage.Contains("leveled up"))
        {
            _audioService.PlaySoundEffect(SoundEffect.LevelUp);
        }
        else if (lowerMessage.Contains("stairs") || lowerMessage.Contains("ladder"))
        {
            _audioService.PlaySoundEffect(SoundEffect.Stairs);
        }
        else if (lowerMessage.Contains("teleport") || lowerMessage.Contains("moongate"))
        {
            _audioService.PlaySoundEffect(SoundEffect.Teleport);
        }
    }

    private void OnPartyMoved()
    {
        _audioService.PlaySoundEffect(SoundEffect.Footstep);
        RefreshDisplay();
    }

    private void OnMapChanged()
    {
        _audioService.PlaySoundEffect(SoundEffect.Stairs);
        RefreshDisplay();
    }

    private void OnStateChanged(GameState newState)
    {
        if (newState == GameState.Combat)
        {
            EnterCombat();
        }
        else if (IsCombatMode && newState != GameState.Combat)
        {
            ExitCombat();
        }

        // Shop state is handled by TryEnterShop/ExitShop directly
        // so we only need to handle if engine exits shop externally
        if (IsShopMode && newState != GameState.Shop)
        {
            IsShopMode = false;
            ShopVm = null;
        }

        RefreshDisplay();
    }

    public void RefreshPartyDisplay()
    {
        PartyMembers.Clear();
        foreach (var member in _gameEngine.Party.Members)
        {
            PartyMembers.Add(new PartyMemberViewModel(member));
        }
    }

    private void RefreshDisplay()
    {
        LocationName = _gameEngine.CurrentMap?.Name ?? "Unknown";
        Gold = _gameEngine.Party.Gold;
        Food = _gameEngine.Party.Food;

        var party = _gameEngine.Party;
        TimeDisplay = party.IsNight ? "Night" : "Day";
        TimeDisplay += $" - Day {party.DayCount}";

        RefreshPartyStats();
    }

    public void RefreshPartyStats()
    {
        // Determine active combatant during combat
        Character? activeCombatant = null;
        if (IsCombatMode && _gameEngine.Combat?.CurrentCombatant is CharacterCombatant cc)
            activeCombatant = cc.Character;

        foreach (var vm in PartyMembers)
        {
            vm.Refresh();
            vm.IsActiveTurn = activeCombatant != null && vm.Character == activeCombatant;
        }
    }

    [RelayCommand]
    private void MoveNorth() => Move(Direction.North);

    [RelayCommand]
    private void MoveSouth() => Move(Direction.South);

    [RelayCommand]
    private void MoveEast() => Move(Direction.East);

    [RelayCommand]
    private void MoveWest() => Move(Direction.West);

    private void Move(Direction direction)
    {
        if (IsCombatMode || IsShopMode || IsInventoryMode || IsSaveMode || IsQuitMode || IsQuestDialogMode || IsQuestLogMode || IsFieldSpellMode) return;
        _recentlySaved = false;
        _gameEngine.MoveParty(direction);
        RefreshDisplay();
    }

    [RelayCommand]
    private void Search()
    {
        if (IsCombatMode || IsShopMode || IsInventoryMode || IsSaveMode || IsQuestDialogMode || IsQuestLogMode) return;
        _gameEngine.Search();
    }

    [RelayCommand]
    private void Rest()
    {
        if (IsCombatMode || IsShopMode || IsInventoryMode || IsSaveMode || IsQuestDialogMode || IsQuestLogMode) return;
        _gameEngine.Rest();
        RefreshDisplay();
    }

    [RelayCommand]
    private void ExitLocation()
    {
        if (IsCombatMode || IsShopMode || IsInventoryMode || IsSaveMode || IsQuestDialogMode || IsQuestLogMode) return;
        _gameEngine.ExitLocation();
        RefreshDisplay();
    }

    [RelayCommand]
    private void SaveGame()
    {
        if (IsCombatMode || IsShopMode || IsInventoryMode || IsSaveMode || IsQuestDialogMode || IsQuestLogMode) return;
        IsSaveMode = true;
        SaveDialogVm = new SaveDialogViewModel(_gameEngine, this);
    }

    public void CloseSaveDialog(bool saved = false)
    {
        if (saved) _recentlySaved = true;
        IsSaveMode = false;
        SaveDialogVm = null;
        RefreshDisplay();
    }

    [RelayCommand]
    private void QuitGame()
    {
        if (IsCombatMode || IsShopMode || IsInventoryMode || IsSaveMode || IsQuitMode || IsQuestDialogMode || IsQuestLogMode || IsFieldSpellMode) return;
        IsQuitMode = true;
        QuitDialogVm = new QuitDialogViewModel(_gameEngine, this, _recentlySaved);
    }

    public void CloseQuitDialog()
    {
        IsQuitMode = false;
        QuitDialogVm = null;
    }

    [RelayCommand]
    private void OpenDoorNorth() => OpenDoor(Direction.North);

    [RelayCommand]
    private void OpenDoorSouth() => OpenDoor(Direction.South);

    [RelayCommand]
    private void OpenDoorEast() => OpenDoor(Direction.East);

    [RelayCommand]
    private void OpenDoorWest() => OpenDoor(Direction.West);

    private void OpenDoor(Direction direction)
    {
        if (IsCombatMode) return;
        _gameEngine.OpenDoor(direction);
    }

    public void EnterShop(ShopType shopType, string? displayName = null)
    {
        if (IsShopMode || IsCombatMode) return;
        IsShopMode = true;
        ShopVm = new ShopViewModel(_gameEngine, this, shopType, displayName);
        _audioService.PlaySoundEffect(SoundEffect.DoorOpen);
    }

    public void ExitShop()
    {
        IsShopMode = false;
        ShopVm = null;
        _gameEngine.ExitShop();
        RefreshDisplay();
    }

    public void OpenInventory()
    {
        if (IsCombatMode || IsShopMode || IsInventoryMode || IsSaveMode) return;
        IsInventoryMode = true;
        InventoryVm = new InventoryViewModel(_gameEngine, this);
    }

    public void CloseInventory()
    {
        IsInventoryMode = false;
        InventoryVm = null;
        RefreshDisplay();
    }

    public void EnterCombat()
    {
        if (IsCombatMode) return;
        IsCombatMode = true;
        CombatVm = new CombatViewModel(_gameEngine.Combat, this);
    }

    public void ExitCombat()
    {
        CombatVm?.Cleanup();
        IsCombatMode = false;
        CombatVm = null;
        RefreshDisplay();
    }

    public void OpenQuestDialog(string townId, string npcName)
    {
        if (IsCombatMode || IsShopMode || IsInventoryMode || IsSaveMode || IsQuestDialogMode || IsQuestLogMode) return;
        IsQuestDialogMode = true;
        QuestDialogVm = new QuestDialogViewModel(_gameEngine, this, townId, npcName);
    }

    public void CloseQuestDialog()
    {
        IsQuestDialogMode = false;
        QuestDialogVm = null;
        RefreshDisplay();
    }

    public void OpenQuestLog()
    {
        if (IsCombatMode || IsShopMode || IsInventoryMode || IsSaveMode || IsQuestDialogMode || IsQuestLogMode) return;
        IsQuestLogMode = true;
        QuestLogVm = new QuestLogViewModel(_gameEngine, this);
    }

    public void CloseQuestLog()
    {
        IsQuestLogMode = false;
        QuestLogVm = null;
        RefreshDisplay();
    }

    public void OpenFieldSpellCasting()
    {
        if (IsCombatMode || IsShopMode || IsInventoryMode || IsSaveMode || IsQuestDialogMode || IsQuestLogMode || IsFieldSpellMode) return;
        IsFieldSpellMode = true;
        FieldSpellVm = new FieldSpellViewModel(_gameEngine, this);
    }

    public void CloseFieldSpellCasting()
    {
        IsFieldSpellMode = false;
        FieldSpellVm = null;
        RefreshPartyStats();
        RefreshDisplay();
    }

    private void TryTalkToNpc()
    {
        if (IsCombatMode || IsShopMode || IsQuestDialogMode || IsQuestLogMode) return;

        var (townId, npcName) = _gameEngine.TryTalkToNpc();
        if (townId != null && npcName != null)
        {
            OpenQuestDialog(townId, npcName);
        }
        else
        {
            _gameEngine.AddMessage("No one to talk to here.");
        }
    }

    public void HandleKeyPress(string key)
    {
        // Save dialog handles its own input via TextBox KeyDown
        if (IsSaveMode) return;

        // Quit dialog input
        if (IsQuitMode && QuitDialogVm != null)
        {
            switch (key.ToUpper())
            {
                case "RETURN":
                case "ENTER":
                    if (!QuitDialogVm.ShowSavePrompt)
                        QuitDialogVm.ConfirmQuitCommand.Execute(null);
                    else
                        QuitDialogVm.SaveAndQuitCommand.Execute(null);
                    break;
                case "ESCAPE":
                    QuitDialogVm.CancelCommand.Execute(null);
                    break;
            }
            return;
        }

        if (IsCombatMode && CombatVm != null)
        {
            CombatVm.HandleKeyPress(key);
            return;
        }

        if (IsShopMode && ShopVm != null)
        {
            ShopVm.HandleKeyPress(key);
            return;
        }

        if (IsInventoryMode && InventoryVm != null)
        {
            InventoryVm.HandleKeyPress(key);
            return;
        }

        if (IsQuestDialogMode && QuestDialogVm != null)
        {
            QuestDialogVm.HandleKeyPress(key);
            return;
        }

        if (IsQuestLogMode && QuestLogVm != null)
        {
            QuestLogVm.HandleKeyPress(key);
            return;
        }

        if (IsFieldSpellMode && FieldSpellVm != null)
        {
            FieldSpellVm.HandleKeyPress(key);
            return;
        }

        switch (key.ToUpper())
        {
            case "W":
            case "UP":
                Move(Direction.North);
                break;
            case "S":
            case "DOWN":
                Move(Direction.South);
                break;
            case "A":
            case "LEFT":
                Move(Direction.West);
                break;
            case "D":
            case "RIGHT":
                Move(Direction.East);
                break;
            case "SPACE":
                Search();
                break;
            case "R":
                Rest();
                break;
            case "X":
                ExitLocation();
                break;
            case "B":
                TryEnterShop();
                break;
            case "I":
                OpenInventory();
                break;
            case "T":
                TryTalkToNpc();
                break;
            case "J":
                OpenQuestLog();
                break;
            case "C":
                OpenFieldSpellCasting();
                break;
            case "F5":
                SaveGame();
                break;
            case "F12":
                QuitGame();
                break;
        }
    }

    private void TryEnterShop()
    {
        if (IsCombatMode || IsShopMode) return;

        if (_gameEngine.TryEnterShop() && _gameEngine.CurrentShopType.HasValue)
        {
            EnterShop(_gameEngine.CurrentShopType.Value, _gameEngine.CurrentShopName);
        }
    }
}

public partial class PartyMemberViewModel : ObservableObject
{
    private readonly Character _character;

    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HpPercentage))]
    private int _currentHp;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HpPercentage))]
    private int _maxHp;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(MpPercentage))]
    private int _currentMp;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(MpPercentage))]
    private int _maxMp;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(XpPercentage))]
    private int _currentXp;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(XpPercentage))]
    private int _xpForNextLevel;

    [ObservableProperty]
    private string _status = string.Empty;

    [ObservableProperty]
    private string _classRace = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(BorderBrush))]
    private bool _isActiveTurn;

    public string BorderBrush => IsActiveTurn ? "#ffd700" : "#1a1a3a";

    public Character Character => _character;

    public PartyMemberViewModel(Character character)
    {
        _character = character;
        Refresh();
    }

    public void Refresh()
    {
        Name = _character.Name;
        CurrentHp = _character.CurrentHP;
        MaxHp = _character.MaxHP;
        CurrentMp = _character.CurrentMP;
        MaxMp = _character.MaxMP;
        CurrentXp = _character.Experience;
        XpForNextLevel = _character.ExperienceForNextLevel;
        ClassRace = $"Lv{_character.Level} {_character.Race} {_character.Class}";

        if (_character.Status == StatusEffect.None)
            Status = "OK";
        else
            Status = _character.Status.ToString();
    }

    public double HpPercentage => MaxHp > 0 ? (double)CurrentHp / MaxHp : 0;
    public double MpPercentage => MaxMp > 0 ? (double)CurrentMp / MaxMp : 0;
    public double XpPercentage => XpForNextLevel > 0 ? (double)CurrentXp / XpForNextLevel : 0;
}
