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

public enum ShopTab { Buy, Sell, Equip, Services }

public partial class ShopViewModel : ViewModelBase
{
    private readonly GameEngine _gameEngine;
    private readonly GameViewModel _parentViewModel;
    private readonly IAudioService _audioService;
    private readonly ShopDefinition _shopDef;

    [ObservableProperty]
    private string _shopName = string.Empty;

    [ObservableProperty]
    private string _welcomeMessage = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsBuyTab))]
    [NotifyPropertyChangedFor(nameof(IsSellTab))]
    [NotifyPropertyChangedFor(nameof(IsEquipTab))]
    [NotifyPropertyChangedFor(nameof(IsServicesTab))]
    private ShopTab _currentTab;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SelectedItemDescription))]
    private int _selectedItemIndex;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SelectedCharacterName))]
    private int _selectedCharacterIndex;

    [ObservableProperty]
    private int _partyGold;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    public bool HasBuyTab => _shopDef.HasBuyTab;
    public bool HasSellTab => _shopDef.HasSellTab;
    public bool HasEquipTab => _shopDef.HasEquipTab;
    public bool HasServicesTab => _shopDef.HasServicesTab;

    public bool IsBuyTab => CurrentTab == ShopTab.Buy;
    public bool IsSellTab => CurrentTab == ShopTab.Sell;
    public bool IsEquipTab => CurrentTab == ShopTab.Equip;
    public bool IsServicesTab => CurrentTab == ShopTab.Services;

    public bool IsTavern => _shopDef.Type == ShopType.Tavern;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SellSourceLabel))]
    private bool _isSellingFromParty;

    public string SellSourceLabel => IsSellingFromParty ? "Party Inventory" : "Character Inventory";

    public ObservableCollection<ShopItemViewModel> Items { get; } = new();

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

            var shopItem = Items[SelectedItemIndex];
            return shopItem.GetDescription(GetSelectedCharacter());
        }
    }

    public ShopViewModel(GameEngine gameEngine, GameViewModel parentViewModel, ShopType shopType, string? displayName = null)
    {
        _gameEngine = gameEngine;
        _parentViewModel = parentViewModel;
        _audioService = AudioService.Instance;
        _shopDef = ShopDefinition.Get(shopType);

        ShopName = displayName ?? _shopDef.Name;
        WelcomeMessage = _shopDef.WelcomeMessage;
        PartyGold = gameEngine.Party.Gold;

        // Default to first available tab
        if (_shopDef.HasBuyTab) CurrentTab = ShopTab.Buy;
        else if (_shopDef.HasSellTab) CurrentTab = ShopTab.Sell;
        else if (_shopDef.HasEquipTab) CurrentTab = ShopTab.Equip;
        else if (_shopDef.HasServicesTab) CurrentTab = ShopTab.Services;

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
        var character = GetSelectedCharacter();
        SelectedItemIndex = -1;

        switch (CurrentTab)
        {
            case ShopTab.Buy:
                if (_shopDef.Type == ShopType.Tavern)
                {
                    // Tavern sells food
                    Items.Add(new ShopItemViewModel("25 Food", 5, "Provisions for the journey", true, true, false));
                    Items.Add(new ShopItemViewModel("50 Food", 10, "Provisions for the journey", _gameEngine.Party.Gold >= 10, true, false));
                    Items.Add(new ShopItemViewModel("100 Food", 20, "Provisions for the journey", _gameEngine.Party.Gold >= 20, true, false));
                }
                else
                {
                    foreach (var item in _shopDef.GetBuyInventory())
                    {
                        bool canAfford = _gameEngine.Party.Gold >= item.Value;
                        bool canUse = character != null && ShopEngine.CanCharacterUse(character, item);
                        Items.Add(new ShopItemViewModel(item, canAfford, canUse));
                    }
                }
                break;

            case ShopTab.Sell:
                if (IsSellingFromParty)
                {
                    foreach (var item in _gameEngine.Party.GetInventoryItems())
                    {
                        if (item.Value > 0)
                            Items.Add(new ShopItemViewModel(item, true, true, isSelling: true));
                    }
                }
                else if (character != null)
                {
                    foreach (var item in character.Inventory)
                    {
                        if (item.Value > 0)
                            Items.Add(new ShopItemViewModel(item, true, true, isSelling: true));
                    }
                }
                break;

            case ShopTab.Equip:
                if (character != null)
                {
                    foreach (var item in character.Inventory)
                    {
                        if (item is Weapon or Armor or Shield)
                        {
                            bool canUse = ShopEngine.CanCharacterUse(character, item);
                            bool isEquipped = item == character.EquippedWeapon ||
                                              item == character.EquippedArmor ||
                                              item == character.EquippedShield;
                            Items.Add(new ShopItemViewModel(item, true, canUse, isEquipped: isEquipped));
                        }
                    }
                }
                break;

            case ShopTab.Services:
                if (_shopDef.Type == ShopType.Healer && character != null)
                {
                    int healCost = (character.MaxHP - character.CurrentHP) * 3;
                    Items.Add(new ShopItemViewModel("Heal", healCost, $"Restore HP ({healCost}g)",
                        _gameEngine.Party.Gold >= healCost && character.IsAlive && character.CurrentHP < character.MaxHP, true, false));
                    Items.Add(new ShopItemViewModel("Cure", 50, "Remove status ailments (50g)",
                        _gameEngine.Party.Gold >= 50 && character.IsAlive && character.Status != StatusEffect.None, true, false));
                    Items.Add(new ShopItemViewModel("Resurrect", 200, "Bring back the dead (200g)",
                        _gameEngine.Party.Gold >= 200 && !character.IsAlive, true, false));
                }
                else if (_shopDef.Type == ShopType.Inn)
                {
                    int livingCount = _gameEngine.Party.GetLivingMembers().Count();
                    int innCost = 50 + (25 * livingCount);
                    Items.Add(new ShopItemViewModel("Rest at Inn", innCost, $"Restore all HP/MP ({innCost}g)",
                        _gameEngine.Party.Gold >= innCost, true, false));
                }
                break;
        }

        if (Items.Count > 0)
        {
            SelectedItemIndex = 0;
            Items[0].IsSelected = true;
        }
    }

    [RelayCommand]
    private void SwitchTab(string tabName)
    {
        if (Enum.TryParse<ShopTab>(tabName, out var tab))
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
        RefreshGold();
    }

    [RelayCommand]
    private void PrevCharacter()
    {
        var members = _gameEngine.Party.Members;
        if (members.Count == 0) return;

        SelectedCharacterIndex = (SelectedCharacterIndex - 1 + members.Count) % members.Count;
        _audioService.PlaySoundEffect(SoundEffect.MenuSelect);
        RefreshItems();
        RefreshGold();
    }

    [RelayCommand]
    private void Confirm()
    {
        if (SelectedItemIndex < 0 || SelectedItemIndex >= Items.Count) return;

        var selectedItem = Items[SelectedItemIndex];
        var character = GetSelectedCharacter();
        if (character == null) return;

        string result;

        switch (CurrentTab)
        {
            case ShopTab.Buy:
                if (_shopDef.Type == ShopType.Tavern)
                {
                    int portions = SelectedItemIndex switch { 0 => 1, 1 => 2, _ => 4 };
                    result = ShopEngine.BuyFood(_gameEngine.Party, portions);
                }
                else
                {
                    if (selectedItem.Item == null) return;
                    // Create a new copy of the item for the character
                    var itemCopy = CreateItemCopy(selectedItem.Item);
                    result = ShopEngine.BuyItem(_gameEngine.Party, character, itemCopy);
                }
                break;

            case ShopTab.Sell:
                if (selectedItem.Item == null) return;
                if (IsSellingFromParty)
                    result = ShopEngine.SellPartyItem(_gameEngine.Party, selectedItem.Item);
                else
                    result = ShopEngine.SellItem(_gameEngine.Party, character, selectedItem.Item);
                break;

            case ShopTab.Equip:
                if (selectedItem.Item == null) return;
                result = ShopEngine.EquipItem(character, selectedItem.Item);
                break;

            case ShopTab.Services:
                if (_shopDef.Type == ShopType.Healer)
                {
                    result = SelectedItemIndex switch
                    {
                        0 => ShopEngine.HealCharacter(_gameEngine.Party, character),
                        1 => ShopEngine.CureStatus(_gameEngine.Party, character),
                        2 => ShopEngine.Resurrect(_gameEngine.Party, character),
                        _ => "Unknown service."
                    };
                }
                else if (_shopDef.Type == ShopType.Inn)
                {
                    result = ShopEngine.RestAtInn(_gameEngine.Party);
                }
                else
                {
                    return;
                }
                break;

            default:
                return;
        }

        StatusMessage = result;
        _audioService.PlaySoundEffect(SoundEffect.MenuConfirm);
        RefreshItems();
        RefreshGold();
        _parentViewModel.RefreshPartyStats();
    }

    [RelayCommand]
    private void ExitShop()
    {
        _audioService.PlaySoundEffect(SoundEffect.MenuCancel);
        _parentViewModel.ExitShop();
    }

    private void RefreshGold()
    {
        PartyGold = _gameEngine.Party.Gold;
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
                Confirm();
                break;
            case "ESCAPE":
                ExitShop();
                break;
            case "D1":
            case "1":
                if (HasBuyTab) SwitchTab("Buy");
                break;
            case "D2":
            case "2":
                if (HasSellTab) SwitchTab("Sell");
                else if (HasServicesTab) SwitchTab("Services");
                break;
            case "D3":
            case "3":
                if (HasEquipTab) SwitchTab("Equip");
                else if (HasServicesTab) SwitchTab("Services");
                break;
            case "D4":
            case "4":
                if (HasServicesTab) SwitchTab("Services");
                break;
            case "TAB":
                CycleTab();
                break;
            case "Q":
                if (CurrentTab == ShopTab.Sell)
                {
                    IsSellingFromParty = !IsSellingFromParty;
                    _audioService.PlaySoundEffect(SoundEffect.MenuSelect);
                    RefreshItems();
                }
                break;
        }
    }

    private void CycleTab()
    {
        var tabs = new[] { ShopTab.Buy, ShopTab.Sell, ShopTab.Equip, ShopTab.Services };
        var available = tabs.Where(t => t switch
        {
            ShopTab.Buy => HasBuyTab,
            ShopTab.Sell => HasSellTab,
            ShopTab.Equip => HasEquipTab,
            ShopTab.Services => HasServicesTab,
            _ => false
        }).ToList();

        if (available.Count <= 1) return;

        int idx = available.IndexOf(CurrentTab);
        int next = (idx + 1) % available.Count;
        CurrentTab = available[next];
        _audioService.PlaySoundEffect(SoundEffect.MenuSelect);
        RefreshItems();
    }

    private static Item CreateItemCopy(Item original) => ItemRegistry.CloneItem(original);
}

public partial class ShopItemViewModel : ObservableObject
{
    public Item? Item { get; }
    public string Name { get; }
    public int Price { get; }
    public string StatsText { get; }
    public bool CanAfford { get; }
    public bool CanUse { get; }
    public bool IsEquipped { get; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(DisplayText))]
    private bool _isSelected;

    public double ItemOpacity => CanAfford && CanUse ? 1.0 : 0.5;

    public string DisplayText
    {
        get
        {
            var marker = IsSelected ? "> " : "  ";
            var price = Price > 0 ? $" {Price}g" : "";
            var usable = !CanUse ? " [Cannot use]" : "";
            var equipped = IsEquipped ? " [Equipped]" : "";
            return $"{marker}{Name}{price}{usable}{equipped}";
        }
    }

    // Constructor for real items
    public ShopItemViewModel(Item item, bool canAfford, bool canUse, bool isSelling = false, bool isEquipped = false)
    {
        Item = item;
        Name = item.Name;
        Price = isSelling ? item.Value / 2 : item.Value;
        CanAfford = canAfford;
        CanUse = canUse;
        IsEquipped = isEquipped;

        StatsText = item switch
        {
            Weapon w => $"Dmg: {w.MinDamage}-{w.MaxDamage}" + (w.Range > 1 ? $" Rng: {w.Range}" : "") + (w.IsTwoHanded ? " (2H)" : ""),
            Armor a => $"Def: {a.Defense}",
            Shield s => $"Def: {s.Defense}",
            Consumable c => c.Description,
            _ => item.Description
        };
    }

    // Constructor for service items (no Item object)
    public ShopItemViewModel(string name, int price, string description, bool canAfford, bool canUse, bool isEquipped)
    {
        Item = null;
        Name = name;
        Price = price;
        StatsText = description;
        CanAfford = canAfford;
        CanUse = canUse;
        IsEquipped = isEquipped;
    }

    public string GetDescription(Character? character)
    {
        if (Item == null) return StatsText;

        var parts = new System.Collections.Generic.List<string>();
        parts.Add(Item.Description);

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
