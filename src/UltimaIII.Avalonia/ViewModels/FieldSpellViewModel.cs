using System;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using UltimaIII.Avalonia.Services.Audio;
using UltimaIII.Core.Engine;
using UltimaIII.Core.Enums;
using UltimaIII.Core.Models;

namespace UltimaIII.Avalonia.ViewModels;

public enum FieldSpellMode { SelectCaster, SelectSpell, SelectTarget }

public partial class FieldSpellViewModel : ViewModelBase
{
    private readonly GameEngine _gameEngine;
    private readonly GameViewModel _parentViewModel;
    private readonly IAudioService _audioService;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsCasterMode))]
    [NotifyPropertyChangedFor(nameof(IsSpellMode))]
    [NotifyPropertyChangedFor(nameof(IsTargetMode))]
    [NotifyPropertyChangedFor(nameof(ModeTitle))]
    [NotifyPropertyChangedFor(nameof(ControlsHint))]
    private FieldSpellMode _currentMode = FieldSpellMode.SelectCaster;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    [ObservableProperty]
    private int _selectedCasterIndex;

    [ObservableProperty]
    private int _selectedSpellIndex;

    [ObservableProperty]
    private int _selectedTargetIndex;

    public bool IsCasterMode => CurrentMode == FieldSpellMode.SelectCaster;
    public bool IsSpellMode => CurrentMode == FieldSpellMode.SelectSpell;
    public bool IsTargetMode => CurrentMode == FieldSpellMode.SelectTarget;

    public string ModeTitle => CurrentMode switch
    {
        FieldSpellMode.SelectCaster => "SELECT CASTER",
        FieldSpellMode.SelectSpell => $"{_selectedCasterCharacter?.Name}'s SPELLS",
        FieldSpellMode.SelectTarget => "SELECT TARGET",
        _ => "CAST SPELL"
    };

    public string ControlsHint => CurrentMode switch
    {
        FieldSpellMode.SelectCaster => "W/S: Browse   Enter: Select   Esc: Close",
        FieldSpellMode.SelectSpell => "W/S: Browse   Enter: Cast   Esc: Back",
        FieldSpellMode.SelectTarget => "W/S: Browse   Enter: Confirm   Esc: Back",
        _ => ""
    };

    public ObservableCollection<FieldCasterVm> Casters { get; } = new();
    public ObservableCollection<FieldSpellItemVm> Spells { get; } = new();
    public ObservableCollection<FieldTargetVm> Targets { get; } = new();

    public bool HasCasters => Casters.Count > 0;
    public bool NoCasters => Casters.Count == 0;

    private Character? _selectedCasterCharacter;
    private Spell? _selectedSpell;

    public FieldSpellViewModel(GameEngine gameEngine, GameViewModel parentViewModel)
    {
        _gameEngine = gameEngine;
        _parentViewModel = parentViewModel;
        _audioService = AudioService.Instance;

        PopulateCasters();
    }

    private void PopulateCasters()
    {
        Casters.Clear();
        int index = 0;
        foreach (var member in _gameEngine.Party.Members)
        {
            if (FieldSpellService.CanCastFieldSpells(member))
            {
                Casters.Add(new FieldCasterVm(member, index == 0));
                index++;
            }
        }

        OnPropertyChanged(nameof(HasCasters));
        OnPropertyChanged(nameof(NoCasters));

        if (Casters.Count == 0)
        {
            StatusMessage = "No one can cast spells right now.";
        }
        else
        {
            SelectedCasterIndex = 0;
        }
    }

    private void PopulateSpells()
    {
        Spells.Clear();
        if (_selectedCasterCharacter == null) return;

        var spells = FieldSpellService.GetFieldSpells(_selectedCasterCharacter);
        for (int i = 0; i < spells.Count; i++)
        {
            Spells.Add(new FieldSpellItemVm(spells[i], _selectedCasterCharacter.CurrentMP, i == 0));
        }

        SelectedSpellIndex = 0;
    }

    private void PopulateTargets()
    {
        Targets.Clear();
        if (_selectedSpell == null) return;

        // For resurrect spells, show dead members
        bool forResurrect = _selectedSpell.CuresStatus.HasFlag(StatusEffect.Dead);

        foreach (var member in _gameEngine.Party.Members)
        {
            if (forResurrect)
            {
                // Show dead members for resurrect
                if (!member.IsAlive)
                    Targets.Add(new FieldTargetVm(member, Targets.Count == 0));
            }
            else
            {
                // Show living members for heal/cure/buff
                Targets.Add(new FieldTargetVm(member, Targets.Count == 0));
            }
        }

        SelectedTargetIndex = 0;

        if (Targets.Count == 0)
        {
            StatusMessage = forResurrect ? "No one is dead." : "No valid targets.";
            CurrentMode = FieldSpellMode.SelectSpell;
        }
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
            case "RETURN":
            case "ENTER":
            case "SPACE":
                Confirm();
                break;
            case "ESCAPE":
            case "C":
                GoBack();
                break;
        }
    }

    private void MoveSelection(int delta)
    {
        _audioService.PlaySoundEffect(SoundEffect.MenuSelect);

        switch (CurrentMode)
        {
            case FieldSpellMode.SelectCaster:
                if (Casters.Count == 0) return;
                Casters[SelectedCasterIndex].IsSelected = false;
                SelectedCasterIndex = Math.Clamp(SelectedCasterIndex + delta, 0, Casters.Count - 1);
                Casters[SelectedCasterIndex].IsSelected = true;
                break;

            case FieldSpellMode.SelectSpell:
                if (Spells.Count == 0) return;
                Spells[SelectedSpellIndex].IsSelected = false;
                SelectedSpellIndex = Math.Clamp(SelectedSpellIndex + delta, 0, Spells.Count - 1);
                Spells[SelectedSpellIndex].IsSelected = true;
                break;

            case FieldSpellMode.SelectTarget:
                if (Targets.Count == 0) return;
                Targets[SelectedTargetIndex].IsSelected = false;
                SelectedTargetIndex = Math.Clamp(SelectedTargetIndex + delta, 0, Targets.Count - 1);
                Targets[SelectedTargetIndex].IsSelected = true;
                break;
        }
    }

    private void Confirm()
    {
        switch (CurrentMode)
        {
            case FieldSpellMode.SelectCaster:
                if (Casters.Count == 0) return;
                _selectedCasterCharacter = Casters[SelectedCasterIndex].Character;
                PopulateSpells();
                CurrentMode = FieldSpellMode.SelectSpell;
                StatusMessage = string.Empty;
                OnPropertyChanged(nameof(ModeTitle));
                _audioService.PlaySoundEffect(SoundEffect.MenuConfirm);
                break;

            case FieldSpellMode.SelectSpell:
                if (Spells.Count == 0) return;
                var spellVm = Spells[SelectedSpellIndex];
                if (!spellVm.CanAfford)
                {
                    StatusMessage = $"Not enough MP! ({spellVm.Spell.ManaCost} needed)";
                    return;
                }
                _selectedSpell = spellVm.Spell;

                // Self-only spells cast immediately
                if (_selectedSpell.TargetsSelf && !_selectedSpell.TargetsParty)
                {
                    CastOnTarget(_selectedCasterCharacter!);
                    return;
                }

                // Party-wide spells (no target needed) — check AreaOfEffect or mass heal
                if (_selectedSpell.TargetsParty && _selectedSpell.AreaOfEffect > 0)
                {
                    CastOnParty();
                    return;
                }

                // Mass heal spells (Xen_Corp, Vas_Mani) target entire party
                if (_selectedSpell.TargetsParty && _selectedSpell.HealAmount > 0 &&
                    !_selectedSpell.TargetsSelf)
                {
                    CastOnParty();
                    return;
                }

                // Need target selection
                PopulateTargets();
                if (Targets.Count > 0)
                {
                    CurrentMode = FieldSpellMode.SelectTarget;
                    StatusMessage = string.Empty;
                    _audioService.PlaySoundEffect(SoundEffect.MenuConfirm);
                }
                break;

            case FieldSpellMode.SelectTarget:
                if (Targets.Count == 0) return;
                var target = Targets[SelectedTargetIndex].Character;
                CastOnTarget(target);
                break;
        }
    }

    private void CastOnTarget(Character target)
    {
        if (_selectedCasterCharacter == null || _selectedSpell == null) return;

        var result = FieldSpellService.CastFieldSpell(_selectedCasterCharacter, _selectedSpell, target);
        StatusMessage = result;
        _audioService.PlaySoundEffect(SoundEffect.MenuConfirm);

        // Refresh and stay in spell list for more casting
        _parentViewModel.RefreshPartyStats();
        PopulateSpells();
        CurrentMode = FieldSpellMode.SelectSpell;
        OnPropertyChanged(nameof(ModeTitle));

        // If caster ran out of MP, go back to caster selection
        if (!FieldSpellService.CanCastFieldSpells(_selectedCasterCharacter))
        {
            StatusMessage += " (Out of MP)";
            PopulateCasters();
            CurrentMode = FieldSpellMode.SelectCaster;
            OnPropertyChanged(nameof(ModeTitle));
        }
    }

    private void CastOnParty()
    {
        if (_selectedCasterCharacter == null || _selectedSpell == null) return;

        var result = FieldSpellService.CastPartySpell(
            _selectedCasterCharacter, _selectedSpell, _gameEngine.Party.Members);
        StatusMessage = result;
        _audioService.PlaySoundEffect(SoundEffect.MenuConfirm);

        _parentViewModel.RefreshPartyStats();
        PopulateSpells();
        CurrentMode = FieldSpellMode.SelectSpell;
        OnPropertyChanged(nameof(ModeTitle));

        if (!FieldSpellService.CanCastFieldSpells(_selectedCasterCharacter))
        {
            StatusMessage += " (Out of MP)";
            PopulateCasters();
            CurrentMode = FieldSpellMode.SelectCaster;
            OnPropertyChanged(nameof(ModeTitle));
        }
    }

    private void GoBack()
    {
        switch (CurrentMode)
        {
            case FieldSpellMode.SelectCaster:
                _audioService.PlaySoundEffect(SoundEffect.MenuCancel);
                _parentViewModel.CloseFieldSpellCasting();
                break;

            case FieldSpellMode.SelectSpell:
                CurrentMode = FieldSpellMode.SelectCaster;
                StatusMessage = string.Empty;
                OnPropertyChanged(nameof(ModeTitle));
                _audioService.PlaySoundEffect(SoundEffect.MenuCancel);
                break;

            case FieldSpellMode.SelectTarget:
                CurrentMode = FieldSpellMode.SelectSpell;
                StatusMessage = string.Empty;
                OnPropertyChanged(nameof(ModeTitle));
                _audioService.PlaySoundEffect(SoundEffect.MenuCancel);
                break;
        }
    }
}

public partial class FieldCasterVm : ObservableObject
{
    public Character Character { get; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(DisplayText))]
    private bool _isSelected;

    public string DisplayText
    {
        get
        {
            var marker = IsSelected ? "> " : "  ";
            return $"{marker}{Character.Name}  Lv{Character.Level} {Character.Class}  MP: {Character.CurrentMP}/{Character.MaxMP}";
        }
    }

    public FieldCasterVm(Character character, bool isSelected)
    {
        Character = character;
        IsSelected = isSelected;
    }
}

public partial class FieldSpellItemVm : ObservableObject
{
    public Spell Spell { get; }
    public bool CanAfford { get; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(DisplayText))]
    private bool _isSelected;

    public double ItemOpacity => CanAfford ? 1.0 : 0.5;

    public string DisplayText
    {
        get
        {
            var marker = IsSelected ? "> " : "  ";
            var afford = CanAfford ? "" : " [No MP]";
            return $"{marker}{Spell.Name} ({Spell.ManaCost} MP) - {Spell.Description}{afford}";
        }
    }

    public FieldSpellItemVm(Spell spell, int casterMp, bool isSelected)
    {
        Spell = spell;
        CanAfford = casterMp >= spell.ManaCost;
        IsSelected = isSelected;
    }
}

public partial class FieldTargetVm : ObservableObject
{
    public Character Character { get; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(DisplayText))]
    private bool _isSelected;

    public string DisplayText
    {
        get
        {
            var marker = IsSelected ? "> " : "  ";
            var status = Character.Status != StatusEffect.None ? $"  [{Character.Status}]" : "";
            var alive = Character.IsAlive ? "" : "  [DEAD]";
            return $"{marker}{Character.Name}  HP: {Character.CurrentHP}/{Character.MaxHP}{status}{alive}";
        }
    }

    public FieldTargetVm(Character character, bool isSelected)
    {
        Character = character;
        IsSelected = isSelected;
    }
}
