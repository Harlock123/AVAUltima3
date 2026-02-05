using System;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using UltimaIII.Core.Engine;
using UltimaIII.Core.Enums;
using UltimaIII.Core.Models;

namespace UltimaIII.Avalonia.ViewModels;

public partial class GameViewModel : ViewModelBase
{
    private readonly GameEngine _gameEngine;
    private readonly MainViewModel _mainViewModel;

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

    public ObservableCollection<PartyMemberViewModel> PartyMembers { get; } = new();
    public ObservableCollection<string> MessageLog { get; } = new();
    public CombatViewModel? CombatVm { get; private set; }

    public GameEngine GameEngine => _gameEngine;

    public GameViewModel(GameEngine gameEngine, MainViewModel mainViewModel)
    {
        _gameEngine = gameEngine;
        _mainViewModel = mainViewModel;

        // Subscribe to game events
        _gameEngine.OnMessage += OnGameMessage;
        _gameEngine.OnPartyMoved += RefreshDisplay;
        _gameEngine.OnMapChanged += RefreshDisplay;

        // Initialize party display
        RefreshPartyDisplay();
        RefreshDisplay();
    }

    private void OnGameMessage(string message)
    {
        MessageLog.Add(message);
        while (MessageLog.Count > 10)
            MessageLog.RemoveAt(0);
    }

    private void RefreshPartyDisplay()
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

        // Refresh party HP/MP
        foreach (var vm in PartyMembers)
        {
            vm.Refresh();
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
        if (IsCombatMode) return;
        _gameEngine.MoveParty(direction);
        RefreshDisplay();
    }

    [RelayCommand]
    private void Search()
    {
        if (IsCombatMode) return;
        _gameEngine.Search();
    }

    [RelayCommand]
    private void Rest()
    {
        if (IsCombatMode) return;
        _gameEngine.Rest();
        RefreshDisplay();
    }

    [RelayCommand]
    private void ExitLocation()
    {
        if (IsCombatMode) return;
        _gameEngine.ExitLocation();
        RefreshDisplay();
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

    public void EnterCombat()
    {
        IsCombatMode = true;
        CombatVm = new CombatViewModel(_gameEngine.Combat, this);
    }

    public void ExitCombat()
    {
        IsCombatMode = false;
        CombatVm = null;
        RefreshDisplay();
    }

    public void HandleKeyPress(string key)
    {
        if (IsCombatMode && CombatVm != null)
        {
            CombatVm.HandleKeyPress(key);
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
        }
    }
}

public partial class PartyMemberViewModel : ObservableObject
{
    private readonly Character _character;

    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private int _currentHp;

    [ObservableProperty]
    private int _maxHp;

    [ObservableProperty]
    private int _currentMp;

    [ObservableProperty]
    private int _maxMp;

    [ObservableProperty]
    private string _status = string.Empty;

    [ObservableProperty]
    private string _classRace = string.Empty;

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
        ClassRace = $"{_character.Race} {_character.Class}";

        if (_character.Status == StatusEffect.None)
            Status = "OK";
        else
            Status = _character.Status.ToString();
    }

    public double HpPercentage => MaxHp > 0 ? (double)CurrentHp / MaxHp : 0;
    public double MpPercentage => MaxMp > 0 ? (double)CurrentMp / MaxMp : 0;
}
