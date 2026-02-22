using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using UltimaIII.Avalonia.Services.Audio;
using UltimaIII.Core.Engine;
using UltimaIII.Core.Enums;
using UltimaIII.Core.Models;

namespace UltimaIII.Avalonia.ViewModels;

public enum TempleStep
{
    SelectEquipment,
    SelectSocket,
    SelectGem,
    Confirm
}

public partial class TempleViewModel : ViewModelBase
{
    private readonly GameEngine _gameEngine;
    private readonly GameViewModel _parentViewModel;
    private readonly IAudioService _audioService;

    [ObservableProperty]
    private string _templeName = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SelectedCharacterName))]
    private int _selectedCharacterIndex;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(StepTitle))]
    [NotifyPropertyChangedFor(nameof(IsEquipmentStep))]
    [NotifyPropertyChangedFor(nameof(IsSocketStep))]
    [NotifyPropertyChangedFor(nameof(IsGemStep))]
    [NotifyPropertyChangedFor(nameof(IsConfirmStep))]
    private TempleStep _currentStep = TempleStep.SelectEquipment;

    [ObservableProperty]
    private int _selectedEquipmentIndex;

    [ObservableProperty]
    private int _selectedSocketIndex;

    [ObservableProperty]
    private int _selectedGemIndex;

    [ObservableProperty]
    private int _partyGold;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    public ObservableCollection<TempleEquipmentItem> EquipmentItems { get; } = new();
    public ObservableCollection<TempleSocketItem> SocketItems { get; } = new();
    public ObservableCollection<TempleGemItem> GemItems { get; } = new();

    public string SelectedCharacterName => _gameEngine.Party.Members.Count > SelectedCharacterIndex
        ? _gameEngine.Party.Members[SelectedCharacterIndex].Name
        : "";

    public string StepTitle => CurrentStep switch
    {
        TempleStep.SelectEquipment => "Select Equipment to Socket",
        TempleStep.SelectSocket => "Select Socket Slot",
        TempleStep.SelectGem => "Select Gem to Socket",
        TempleStep.Confirm => "Confirm Socketing",
        _ => ""
    };

    public bool IsEquipmentStep => CurrentStep == TempleStep.SelectEquipment;
    public bool IsSocketStep => CurrentStep == TempleStep.SelectSocket;
    public bool IsGemStep => CurrentStep == TempleStep.SelectGem;
    public bool IsConfirmStep => CurrentStep == TempleStep.Confirm;

    // Selected state for confirm step
    private Item? _chosenEquipment;
    private Gem? _chosenGem;
    private int _chosenSocketSlot;
    private int _socketCost;

    public string ConfirmEquipmentName => _chosenEquipment?.Name ?? "";
    public string ConfirmSocketInfo => _chosenEquipment != null
        ? $"Socket {_chosenSocketSlot + 1} of {GetMaxSockets(_chosenEquipment)}"
        : "";
    public string ConfirmGemName => _chosenGem?.Name ?? "";
    public string ConfirmGemDescription => _chosenGem?.Description ?? "";
    public string ConfirmCost => $"{_socketCost} gold";
    public string ConfirmOldGem
    {
        get
        {
            var sockets = GetSockets(_chosenEquipment);
            if (sockets != null && _chosenSocketSlot < sockets.Count && sockets[_chosenSocketSlot] != null)
                return $"Replaces: {sockets[_chosenSocketSlot]!.Name} (will be DESTROYED)";
            return "";
        }
    }

    public TempleViewModel(GameEngine gameEngine, GameViewModel parentViewModel, string? displayName = null)
    {
        _gameEngine = gameEngine;
        _parentViewModel = parentViewModel;
        _audioService = AudioService.Instance;

        TempleName = displayName ?? "Temple";
        PartyGold = gameEngine.Party.Gold;
        StatusMessage = "Welcome, seeker. Bring thy gems and I shall bind their power.";

        RefreshEquipment();
    }

    private Character CurrentCharacter => _gameEngine.Party.Members[SelectedCharacterIndex];

    private void CycleCharacter(int delta)
    {
        int count = _gameEngine.Party.Members.Count;
        SelectedCharacterIndex = (SelectedCharacterIndex + delta + count) % count;
        CurrentStep = TempleStep.SelectEquipment;
        RefreshEquipment();
        _audioService.PlaySoundEffect(SoundEffect.MenuSelect);
    }

    private void RefreshEquipment()
    {
        EquipmentItems.Clear();
        SelectedEquipmentIndex = 0;

        var items = SocketingService.GetSocketableEquipment(CurrentCharacter);
        if (items.Count == 0)
        {
            StatusMessage = $"{CurrentCharacter.Name} has no socketable equipment.";
            return;
        }

        for (int i = 0; i < items.Count; i++)
        {
            var item = items[i];
            var sockets = GetSockets(item);
            string socketDisplay = sockets != null
                ? string.Join(" ", sockets.Select(s => s != null ? $"[{s.Name}]" : "[Empty]"))
                : "";
            EquipmentItems.Add(new TempleEquipmentItem
            {
                Item = item,
                DisplayText = $"{item.Name}  {socketDisplay}",
                IsSelected = i == 0
            });
        }

        StatusMessage = "Select equipment to socket. [W/S] Navigate  [Enter] Select  [A/D] Character  [Esc] Exit";
    }

    private void RefreshSockets()
    {
        SocketItems.Clear();
        SelectedSocketIndex = 0;

        if (_chosenEquipment == null) return;

        var sockets = GetSockets(_chosenEquipment);
        if (sockets == null) return;

        for (int i = 0; i < sockets.Count; i++)
        {
            var gem = sockets[i];
            SocketItems.Add(new TempleSocketItem
            {
                Index = i,
                DisplayText = gem != null ? $"Socket {i + 1}: {gem.Name} ({gem.Description})" : $"Socket {i + 1}: [Empty]",
                IsEmpty = gem == null,
                IsSelected = i == 0
            });
        }

        StatusMessage = "Select a socket slot. [W/S] Navigate  [Enter] Select  [Esc] Back";
    }

    private void RefreshGems()
    {
        GemItems.Clear();
        SelectedGemIndex = 0;

        if (_chosenEquipment == null) return;

        var partyGems = _gameEngine.Party.GetInventoryItems(ItemCategory.Gem)
            .OfType<Gem>()
            .Where(g => SocketingService.CanSocketGem(_chosenEquipment, g))
            .ToList();

        if (partyGems.Count == 0)
        {
            StatusMessage = "No compatible gems in inventory!";
            return;
        }

        for (int i = 0; i < partyGems.Count; i++)
        {
            var gem = partyGems[i];
            int cost = SocketingService.GetSocketingCost(gem, _chosenEquipment);
            GemItems.Add(new TempleGemItem
            {
                Gem = gem,
                DisplayText = $"{gem.Name}  {gem.Description}  Cost: {cost}g",
                Cost = cost,
                CanAfford = _gameEngine.Party.Gold >= cost,
                IsSelected = i == 0
            });
        }

        StatusMessage = "Select a gem to socket. [W/S] Navigate  [Enter] Select  [Esc] Back";
    }

    public void HandleKeyPress(string key)
    {
        var upper = key.ToUpper();

        switch (upper)
        {
            case "ESCAPE":
                HandleEscape();
                break;
            case "A":
            case "LEFT":
                CycleCharacter(-1);
                break;
            case "D":
            case "RIGHT":
                CycleCharacter(1);
                break;
            case "W":
            case "UP":
                MoveSelection(-1);
                break;
            case "S":
            case "DOWN":
                MoveSelection(1);
                break;
            case "RETURN":
            case "ENTER":
                ConfirmSelection();
                break;
        }
    }

    private void HandleEscape()
    {
        switch (CurrentStep)
        {
            case TempleStep.SelectEquipment:
                _parentViewModel.CloseTemple();
                break;
            case TempleStep.SelectSocket:
                CurrentStep = TempleStep.SelectEquipment;
                RefreshEquipment();
                break;
            case TempleStep.SelectGem:
                CurrentStep = TempleStep.SelectSocket;
                RefreshSockets();
                break;
            case TempleStep.Confirm:
                CurrentStep = TempleStep.SelectGem;
                RefreshGems();
                break;
        }
        _audioService.PlaySoundEffect(SoundEffect.MenuCancel);
    }

    private void MoveSelection(int delta)
    {
        switch (CurrentStep)
        {
            case TempleStep.SelectEquipment:
                if (EquipmentItems.Count == 0) return;
                EquipmentItems[SelectedEquipmentIndex].IsSelected = false;
                SelectedEquipmentIndex = Math.Clamp(SelectedEquipmentIndex + delta, 0, EquipmentItems.Count - 1);
                EquipmentItems[SelectedEquipmentIndex].IsSelected = true;
                break;
            case TempleStep.SelectSocket:
                if (SocketItems.Count == 0) return;
                SocketItems[SelectedSocketIndex].IsSelected = false;
                SelectedSocketIndex = Math.Clamp(SelectedSocketIndex + delta, 0, SocketItems.Count - 1);
                SocketItems[SelectedSocketIndex].IsSelected = true;
                break;
            case TempleStep.SelectGem:
                if (GemItems.Count == 0) return;
                GemItems[SelectedGemIndex].IsSelected = false;
                SelectedGemIndex = Math.Clamp(SelectedGemIndex + delta, 0, GemItems.Count - 1);
                GemItems[SelectedGemIndex].IsSelected = true;
                break;
        }
        _audioService.PlaySoundEffect(SoundEffect.MenuSelect);
    }

    private void ConfirmSelection()
    {
        switch (CurrentStep)
        {
            case TempleStep.SelectEquipment:
                if (EquipmentItems.Count == 0) return;
                _chosenEquipment = EquipmentItems[SelectedEquipmentIndex].Item;
                CurrentStep = TempleStep.SelectSocket;
                RefreshSockets();
                _audioService.PlaySoundEffect(SoundEffect.MenuConfirm);
                break;

            case TempleStep.SelectSocket:
                if (SocketItems.Count == 0) return;
                _chosenSocketSlot = SocketItems[SelectedSocketIndex].Index;
                CurrentStep = TempleStep.SelectGem;
                RefreshGems();
                _audioService.PlaySoundEffect(SoundEffect.MenuConfirm);
                break;

            case TempleStep.SelectGem:
                if (GemItems.Count == 0) return;
                var gemItem = GemItems[SelectedGemIndex];
                if (!gemItem.CanAfford)
                {
                    StatusMessage = "Not enough gold!";
                    _audioService.PlaySoundEffect(SoundEffect.MenuCancel);
                    return;
                }
                _chosenGem = gemItem.Gem;
                _socketCost = gemItem.Cost;
                CurrentStep = TempleStep.Confirm;
                OnPropertyChanged(nameof(ConfirmEquipmentName));
                OnPropertyChanged(nameof(ConfirmSocketInfo));
                OnPropertyChanged(nameof(ConfirmGemName));
                OnPropertyChanged(nameof(ConfirmGemDescription));
                OnPropertyChanged(nameof(ConfirmCost));
                OnPropertyChanged(nameof(ConfirmOldGem));
                StatusMessage = "[Enter] Confirm socketing  [Esc] Cancel";
                _audioService.PlaySoundEffect(SoundEffect.MenuConfirm);
                break;

            case TempleStep.Confirm:
                ExecuteSocketing();
                break;
        }
    }

    private void ExecuteSocketing()
    {
        if (_chosenEquipment == null || _chosenGem == null) return;

        bool success = SocketingService.SocketGem(
            _gameEngine.Party, _chosenEquipment, _chosenSocketSlot, _chosenGem);

        if (success)
        {
            PartyGold = _gameEngine.Party.Gold;
            StatusMessage = $"The {_chosenGem.Name} has been bound to {_chosenEquipment.Name}!";
            _gameEngine.AddMessage($"Socketed {_chosenGem.Name} into {_chosenEquipment.Name}.");
            _audioService.PlaySoundEffect(SoundEffect.MenuConfirm);
            _parentViewModel.RefreshPartyDisplay();

            // Return to equipment selection
            _chosenEquipment = null;
            _chosenGem = null;
            CurrentStep = TempleStep.SelectEquipment;
            RefreshEquipment();
        }
        else
        {
            StatusMessage = "Socketing failed!";
            _audioService.PlaySoundEffect(SoundEffect.MenuCancel);
        }
    }

    private static List<Gem?>? GetSockets(Item? item)
    {
        return item switch
        {
            Weapon w => w.Sockets,
            Armor a => a.Sockets,
            Shield s => s.Sockets,
            _ => null
        };
    }

    private static int GetMaxSockets(Item? item)
    {
        return item switch
        {
            Weapon w => w.MaxSockets,
            Armor a => a.MaxSockets,
            Shield s => s.MaxSockets,
            _ => 0
        };
    }
}

public partial class TempleEquipmentItem : ObservableObject
{
    public Item Item { get; init; } = null!;
    public string DisplayText { get; init; } = string.Empty;

    [ObservableProperty]
    private bool _isSelected;

    public string RowBackground => IsSelected ? "#3a3a6a" : "Transparent";
}

public partial class TempleSocketItem : ObservableObject
{
    public int Index { get; init; }
    public string DisplayText { get; init; } = string.Empty;
    public bool IsEmpty { get; init; }

    [ObservableProperty]
    private bool _isSelected;

    public string RowBackground => IsSelected ? "#3a3a6a" : "Transparent";
}

public partial class TempleGemItem : ObservableObject
{
    public Gem Gem { get; init; } = null!;
    public string DisplayText { get; init; } = string.Empty;
    public int Cost { get; init; }
    public bool CanAfford { get; init; }

    [ObservableProperty]
    private bool _isSelected;

    public string RowBackground => IsSelected ? "#3a3a6a" : "Transparent";
    public string TextColor => CanAfford ? "#e0e0e0" : "#808080";
}
