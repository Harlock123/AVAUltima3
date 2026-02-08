using System;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using UltimaIII.Avalonia.Services.Audio;
using UltimaIII.Core.Engine;
using UltimaIII.Core.Enums;
using UltimaIII.Core.Models;

namespace UltimaIII.Avalonia.ViewModels;

public enum InventoryTab { All, Weapons, Armor, Shields, Items }

public partial class InventoryViewModel : ViewModelBase
{
    private readonly GameEngine _gameEngine;
    private readonly GameViewModel _parentViewModel;
    private readonly IAudioService _audioService;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsAllTab))]
    [NotifyPropertyChangedFor(nameof(IsWeaponsTab))]
    [NotifyPropertyChangedFor(nameof(IsArmorTab))]
    [NotifyPropertyChangedFor(nameof(IsShieldsTab))]
    [NotifyPropertyChangedFor(nameof(IsItemsTab))]
    private InventoryTab _currentTab;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SelectedItemDescription))]
    private int _selectedItemIndex;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SelectedCharacterName))]
    private int _selectedCharacterIndex;

    [ObservableProperty]
    private int _partyGold;

    [ObservableProperty]
    private int _totalItems;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    public bool IsAllTab => CurrentTab == InventoryTab.All;
    public bool IsWeaponsTab => CurrentTab == InventoryTab.Weapons;
    public bool IsArmorTab => CurrentTab == InventoryTab.Armor;
    public bool IsShieldsTab => CurrentTab == InventoryTab.Shields;
    public bool IsItemsTab => CurrentTab == InventoryTab.Items;

    public ObservableCollection<InventoryItemViewModel> Items { get; } = new();

    public string SelectedCharacterName
    {
        get
        {
            var members = _gameEngine.Party.Members;
            if (SelectedCharacterIndex >= 0 && SelectedCharacterIndex < members.Count)
                return members[SelectedCharacterIndex].Name;
            return "---";
        }
    }

    public string SelectedItemDescription
    {
        get
        {
            if (SelectedItemIndex < 0 || SelectedItemIndex >= Items.Count)
                return string.Empty;

            return Items[SelectedItemIndex].GetDescription(GetSelectedCharacter());
        }
    }

    public InventoryViewModel(GameEngine gameEngine, GameViewModel parentViewModel)
    {
        _gameEngine = gameEngine;
        _parentViewModel = parentViewModel;
        _audioService = AudioService.Instance;

        CurrentTab = InventoryTab.All;
        PartyGold = gameEngine.Party.Gold;
        RefreshItems();
    }

    private Character? GetSelectedCharacter()
    {
        var members = _gameEngine.Party.Members;
        if (SelectedCharacterIndex >= 0 && SelectedCharacterIndex < members.Count)
            return members[SelectedCharacterIndex];
        return null;
    }

    private void RefreshItems()
    {
        Items.Clear();
        SelectedItemIndex = -1;

        ItemCategory? filter = CurrentTab switch
        {
            InventoryTab.Weapons => ItemCategory.Weapon,
            InventoryTab.Armor => ItemCategory.Armor,
            InventoryTab.Shields => ItemCategory.Shield,
            InventoryTab.Items => ItemCategory.Consumable,
            _ => null
        };

        var character = GetSelectedCharacter();
        var items = _gameEngine.Party.GetInventoryItems(filter);

        foreach (var item in items)
        {
            bool canUse = character != null && ShopEngine.CanCharacterUse(character, item);
            Items.Add(new InventoryItemViewModel(item, canUse));
        }

        TotalItems = _gameEngine.Party.SharedInventory.Count;

        if (Items.Count > 0)
        {
            SelectedItemIndex = 0;
            Items[0].IsSelected = true;
        }
    }

    [RelayCommand]
    private void SwitchTab(string tabName)
    {
        if (Enum.TryParse<InventoryTab>(tabName, out var tab))
        {
            CurrentTab = tab;
            _audioService.PlaySoundEffect(SoundEffect.MenuSelect);
            RefreshItems();
        }
    }

    [RelayCommand]
    private void NextCharacter()
    {
        var members = _gameEngine.Party.Members;
        if (members.Count == 0) return;

        SelectedCharacterIndex = (SelectedCharacterIndex + 1) % members.Count;
        _audioService.PlaySoundEffect(SoundEffect.MenuSelect);
        RefreshItems();
    }

    [RelayCommand]
    private void PrevCharacter()
    {
        var members = _gameEngine.Party.Members;
        if (members.Count == 0) return;

        SelectedCharacterIndex = (SelectedCharacterIndex - 1 + members.Count) % members.Count;
        _audioService.PlaySoundEffect(SoundEffect.MenuSelect);
        RefreshItems();
    }

    [RelayCommand]
    private void EquipItem()
    {
        if (SelectedItemIndex < 0 || SelectedItemIndex >= Items.Count) return;

        var selectedItem = Items[SelectedItemIndex];
        if (selectedItem.Item == null) return;

        var character = GetSelectedCharacter();
        if (character == null) return;

        if (selectedItem.Item is not (Weapon or Armor or Shield))
        {
            StatusMessage = "Cannot equip that item.";
            return;
        }

        var result = ShopEngine.EquipFromParty(_gameEngine.Party, character, selectedItem.Item);
        StatusMessage = result;
        _audioService.PlaySoundEffect(SoundEffect.MenuConfirm);
        RefreshItems();
        PartyGold = _gameEngine.Party.Gold;
        _parentViewModel.RefreshPartyStats();
    }

    [RelayCommand]
    private void CloseInventory()
    {
        _audioService.PlaySoundEffect(SoundEffect.MenuCancel);
        _parentViewModel.CloseInventory();
    }

    private void MoveSelection(int delta)
    {
        if (Items.Count == 0) return;

        if (SelectedItemIndex >= 0 && SelectedItemIndex < Items.Count)
            Items[SelectedItemIndex].IsSelected = false;

        SelectedItemIndex = Math.Clamp(SelectedItemIndex + delta, 0, Items.Count - 1);
        Items[SelectedItemIndex].IsSelected = true;
    }

    public void HandleKeyPress(string key)
    {
        switch (key.ToUpper())
        {
            case "W":
            case "UP":
                MoveSelection(-1);
                break;
            case "S":
            case "DOWN":
                MoveSelection(1);
                break;
            case "A":
            case "LEFT":
                PrevCharacter();
                break;
            case "D":
            case "RIGHT":
                NextCharacter();
                break;
            case "RETURN":
            case "ENTER":
            case "SPACE":
                EquipItem();
                break;
            case "ESCAPE":
            case "I":
                CloseInventory();
                break;
            case "D1":
            case "1":
                SwitchTab("All");
                break;
            case "D2":
            case "2":
                SwitchTab("Weapons");
                break;
            case "D3":
            case "3":
                SwitchTab("Armor");
                break;
            case "D4":
            case "4":
                SwitchTab("Shields");
                break;
            case "D5":
            case "5":
                SwitchTab("Items");
                break;
            case "TAB":
                CycleTab();
                break;
        }
    }

    private void CycleTab()
    {
        var tabs = Enum.GetValues<InventoryTab>();
        int idx = Array.IndexOf(tabs, CurrentTab);
        int next = (idx + 1) % tabs.Length;
        CurrentTab = tabs[next];
        _audioService.PlaySoundEffect(SoundEffect.MenuSelect);
        RefreshItems();
    }
}

public partial class InventoryItemViewModel : ObservableObject
{
    public Item? Item { get; }
    public string Name { get; }
    public string StatsText { get; }
    public bool CanUse { get; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(DisplayText))]
    private bool _isSelected;

    public double ItemOpacity => CanUse ? 1.0 : 0.5;

    public string DisplayText
    {
        get
        {
            var marker = IsSelected ? "> " : "  ";
            var usable = !CanUse ? " [Cannot Use]" : "";
            var qty = Item is { IsStackable: true, Quantity: > 1 } ? $" x{Item.Quantity}" : "";
            return $"{marker}{Name}{qty}{usable}";
        }
    }

    public InventoryItemViewModel(Item item, bool canUse)
    {
        Item = item;
        Name = item.Name;
        CanUse = canUse;

        StatsText = item switch
        {
            Weapon w => $"Dmg: {w.MinDamage}-{w.MaxDamage}" + (w.Range > 1 ? $" Rng: {w.Range}" : "") + (w.IsTwoHanded ? " (2H)" : ""),
            Armor a => $"Def: {a.Defense}",
            Shield s => $"Def: {s.Defense}",
            Consumable c => c.Description,
            _ => item.Description
        };
    }

    public string GetDescription(Character? character)
    {
        if (Item == null) return StatsText;

        var parts = new System.Collections.Generic.List<string>();
        parts.Add(Item.Description);
        parts.Add($"Value: {Item.Value}g");

        if (Item is Weapon w)
        {
            parts.Add($"Damage: {w.MinDamage}-{w.MaxDamage}");
            if (w.HitBonus > 0) parts.Add($"Hit Bonus: +{w.HitBonus}");
            if (w.Range > 1) parts.Add($"Range: {w.Range}");
            if (w.IsTwoHanded) parts.Add("Two-Handed");

            if (character?.EquippedWeapon != null && character.EquippedWeapon != Weapon.Hands)
            {
                var cur = character.EquippedWeapon;
                int dmgDiff = (w.MinDamage + w.MaxDamage) / 2 - (cur.MinDamage + cur.MaxDamage) / 2;
                parts.Add(dmgDiff > 0 ? $"vs current: +{dmgDiff} avg dmg" : $"vs current: {dmgDiff} avg dmg");
            }
        }
        else if (Item is Armor a)
        {
            parts.Add($"Defense: {a.Defense}");
            if (character?.EquippedArmor != null && character.EquippedArmor != Armor.None)
            {
                int defDiff = a.Defense - character.EquippedArmor.Defense;
                parts.Add(defDiff > 0 ? $"vs current: +{defDiff} def" : $"vs current: {defDiff} def");
            }
        }
        else if (Item is Shield s)
        {
            parts.Add($"Defense: {s.Defense}");
            if (character?.EquippedShield != null && character.EquippedShield != Shield.None)
            {
                int defDiff = s.Defense - character.EquippedShield.Defense;
                parts.Add(defDiff > 0 ? $"vs current: +{defDiff} def" : $"vs current: {defDiff} def");
            }
        }

        return string.Join("  |  ", parts);
    }
}
