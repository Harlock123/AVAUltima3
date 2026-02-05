using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using UltimaIII.Core.Engine;
using UltimaIII.Core.Enums;
using UltimaIII.Core.Models;

namespace UltimaIII.Avalonia.ViewModels;

public partial class CharacterCreationViewModel : ViewModelBase
{
    private readonly GameEngine _gameEngine;
    private readonly MainViewModel _mainViewModel;

    [ObservableProperty]
    private string _characterName = string.Empty;

    [ObservableProperty]
    private Race _selectedRace = Race.Human;

    [ObservableProperty]
    private CharacterClass _selectedClass = CharacterClass.Fighter;

    [ObservableProperty]
    private int _strength = 10;

    [ObservableProperty]
    private int _dexterity = 10;

    [ObservableProperty]
    private int _intelligence = 10;

    [ObservableProperty]
    private int _wisdom = 10;

    [ObservableProperty]
    private int _remainingPoints;

    [ObservableProperty]
    private string _raceDescription = string.Empty;

    [ObservableProperty]
    private string _classDescription = string.Empty;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private bool _canCreateCharacter = false;

    public ObservableCollection<CharacterSlotViewModel> PartySlots { get; } = new();

    public IReadOnlyList<Race> AvailableRaces { get; } = Enum.GetValues<Race>();
    public IReadOnlyList<CharacterClass> AvailableClasses { get; } = Enum.GetValues<CharacterClass>();

    public CharacterCreationViewModel(GameEngine gameEngine, MainViewModel mainViewModel)
    {
        _gameEngine = gameEngine;
        _mainViewModel = mainViewModel;
        _remainingPoints = Stats.StartingStatPoints - (Strength + Dexterity + Intelligence + Wisdom);

        // Initialize party slots
        for (int i = 0; i < Party.MaxPartySize; i++)
        {
            PartySlots.Add(new CharacterSlotViewModel { SlotNumber = i + 1 });
        }

        UpdateDescriptions();
        ValidateCharacter();
    }

    partial void OnSelectedRaceChanged(Race value)
    {
        UpdateDescriptions();
        ValidateCharacter();
    }

    partial void OnSelectedClassChanged(CharacterClass value)
    {
        UpdateDescriptions();
        ValidateCharacter();
    }

    partial void OnCharacterNameChanged(string value)
    {
        ValidateCharacter();
    }

    partial void OnStrengthChanged(int value)
    {
        UpdateRemainingPoints();
        ValidateCharacter();
    }

    partial void OnDexterityChanged(int value)
    {
        UpdateRemainingPoints();
        ValidateCharacter();
    }

    partial void OnIntelligenceChanged(int value)
    {
        UpdateRemainingPoints();
        ValidateCharacter();
    }

    partial void OnWisdomChanged(int value)
    {
        UpdateRemainingPoints();
        ValidateCharacter();
    }

    private void UpdateRemainingPoints()
    {
        RemainingPoints = Stats.StartingStatPoints - (Strength + Dexterity + Intelligence + Wisdom);
    }

    private void UpdateDescriptions()
    {
        var raceDef = RaceDefinition.Get(SelectedRace);
        RaceDescription = $"{raceDef.Description}\nModifiers: STR {FormatMod(raceDef.StatModifiers.StrengthMod)}, " +
                         $"DEX {FormatMod(raceDef.StatModifiers.DexterityMod)}, " +
                         $"INT {FormatMod(raceDef.StatModifiers.IntelligenceMod)}, " +
                         $"WIS {FormatMod(raceDef.StatModifiers.WisdomMod)}";

        var classDef = ClassDefinition.Get(SelectedClass);
        ClassDescription = $"{classDef.Description}\n" +
                          $"Requires: STR {classDef.Requirements.MinStrength}, " +
                          $"DEX {classDef.Requirements.MinDexterity}, " +
                          $"INT {classDef.Requirements.MinIntelligence}, " +
                          $"WIS {classDef.Requirements.MinWisdom}";
    }

    private string FormatMod(int mod) => mod >= 0 ? $"+{mod}" : mod.ToString();

    private void ValidateCharacter()
    {
        ErrorMessage = string.Empty;

        if (string.IsNullOrWhiteSpace(CharacterName))
        {
            ErrorMessage = "Enter a name for your character.";
            CanCreateCharacter = false;
            return;
        }

        if (RemainingPoints < 0)
        {
            ErrorMessage = "You have used too many stat points.";
            CanCreateCharacter = false;
            return;
        }

        // Check class requirements with racial modifiers
        var stats = new Stats(Strength, Dexterity, Intelligence, Wisdom);
        stats.ApplyModifiers(RaceDefinition.Get(SelectedRace).StatModifiers);
        var requirements = ClassDefinition.Get(SelectedClass).Requirements;

        if (!requirements.MeetsRequirements(stats))
        {
            ErrorMessage = $"Stats don't meet {SelectedClass} requirements (after racial modifiers).";
            CanCreateCharacter = false;
            return;
        }

        if (_gameEngine.Party.IsFull)
        {
            ErrorMessage = "Party is full.";
            CanCreateCharacter = false;
            return;
        }

        CanCreateCharacter = true;
    }

    [RelayCommand]
    private void IncreaseStat(string statName)
    {
        if (RemainingPoints <= 0) return;

        switch (statName)
        {
            case "STR" when Strength < Stats.MaxStat: Strength++; break;
            case "DEX" when Dexterity < Stats.MaxStat: Dexterity++; break;
            case "INT" when Intelligence < Stats.MaxStat: Intelligence++; break;
            case "WIS" when Wisdom < Stats.MaxStat: Wisdom++; break;
        }
    }

    [RelayCommand]
    private void DecreaseStat(string statName)
    {
        switch (statName)
        {
            case "STR" when Strength > Stats.MinStat: Strength--; break;
            case "DEX" when Dexterity > Stats.MinStat: Dexterity--; break;
            case "INT" when Intelligence > Stats.MinStat: Intelligence--; break;
            case "WIS" when Wisdom > Stats.MinStat: Wisdom--; break;
        }
    }

    [RelayCommand]
    private void CreateCharacter()
    {
        if (!CanCreateCharacter) return;

        var stats = new Stats(Strength, Dexterity, Intelligence, Wisdom);
        var character = Character.Create(CharacterName, SelectedRace, SelectedClass, stats);

        if (_gameEngine.Party.AddMember(character))
        {
            // Update slot display
            int index = _gameEngine.Party.Count - 1;
            PartySlots[index].Character = character;
            PartySlots[index].IsOccupied = true;

            // Reset form for next character
            CharacterName = string.Empty;
            Strength = 10;
            Dexterity = 10;
            Intelligence = 10;
            Wisdom = 10;

            ValidateCharacter();
        }
    }

    [RelayCommand]
    private void RemoveCharacter(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= PartySlots.Count) return;

        var slot = PartySlots[slotIndex];
        if (slot.Character != null)
        {
            _gameEngine.Party.RemoveMember(slot.Character);
            slot.Character = null;
            slot.IsOccupied = false;

            // Reorder remaining slots
            RefreshPartySlots();
            ValidateCharacter();
        }
    }

    private void RefreshPartySlots()
    {
        var members = _gameEngine.Party.Members.ToList();
        for (int i = 0; i < PartySlots.Count; i++)
        {
            if (i < members.Count)
            {
                PartySlots[i].Character = members[i];
                PartySlots[i].IsOccupied = true;
            }
            else
            {
                PartySlots[i].Character = null;
                PartySlots[i].IsOccupied = false;
            }
        }
    }

    [RelayCommand]
    private void BeginQuest()
    {
        if (_gameEngine.Party.IsEmpty)
        {
            ErrorMessage = "You must create at least one character!";
            return;
        }

        _mainViewModel.StartGame();
    }
}

public partial class CharacterSlotViewModel : ObservableObject
{
    [ObservableProperty]
    private int _slotNumber;

    [ObservableProperty]
    private Character? _character;

    [ObservableProperty]
    private bool _isOccupied;

    public string DisplayName => Character?.Name ?? $"Slot {SlotNumber} (Empty)";
    public string DisplayClass => Character != null ? $"{Character.Race} {Character.Class}" : string.Empty;
    public string DisplayStats => Character != null
        ? $"HP:{Character.MaxHP} MP:{Character.MaxMP} STR:{Character.Stats.Strength} DEX:{Character.Stats.Dexterity} INT:{Character.Stats.Intelligence} WIS:{Character.Stats.Wisdom}"
        : string.Empty;
}
