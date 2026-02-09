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

public partial class CombatViewModel : ViewModelBase
{
    private readonly CombatSystem _combat;
    private readonly GameViewModel _parentViewModel;
    private readonly IAudioService _audioService;

    [ObservableProperty]
    private string _currentCombatantName = string.Empty;

    [ObservableProperty]
    private bool _isPlayerTurn;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsNormalMode))]
    private bool _isSelectingTarget;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsNormalMode))]
    [NotifyPropertyChangedFor(nameof(SelectedSpellDescription))]
    private bool _isSelectingSpell;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SelectedSpellDescription))]
    private int _selectedSpellIndex;

    public ObservableCollection<SpellChoiceViewModel> AvailableSpells { get; } = new();

    public bool IsNormalMode => !IsSelectingTarget && !IsSelectingSpell;

    public string SelectedSpellDescription
    {
        get
        {
            if (!IsSelectingSpell || SelectedSpellIndex < 0 || SelectedSpellIndex >= AvailableSpells.Count)
                return string.Empty;

            var spell = AvailableSpells[SelectedSpellIndex].Spell;
            var parts = new System.Collections.Generic.List<string>();

            parts.Add(spell.Description);

            if (spell.MinDamage > 0 || spell.MaxDamage > 0)
                parts.Add($"Damage: {spell.MinDamage}-{spell.MaxDamage}");

            if (spell.HealAmount > 0)
                parts.Add($"Heals: {spell.HealAmount} HP");

            if (spell.Range > 1)
                parts.Add($"Range: {spell.Range}");

            if (spell.AreaOfEffect > 0)
                parts.Add($"AOE: {spell.AreaOfEffect}");

            if (spell.AppliesStatus != StatusEffect.None)
                parts.Add($"Inflicts: {spell.AppliesStatus}");

            if (spell.CuresStatus != StatusEffect.None)
                parts.Add($"Cures: {spell.CuresStatus}");

            // Target type
            var targets = new System.Collections.Generic.List<string>();
            if (spell.TargetsSelf) targets.Add("Self");
            if (spell.TargetsParty) targets.Add("Ally");
            if (spell.TargetsEnemy) targets.Add("Enemy");
            if (targets.Count > 0)
                parts.Add($"Target: {string.Join("/", targets)}");

            return string.Join("  |  ", parts);
        }
    }

    [ObservableProperty]
    private int _targetX;

    [ObservableProperty]
    private int _targetY;

    [ObservableProperty]
    private CombatActionType _pendingAction;

    [ObservableProperty]
    private SpellType? _pendingSpell;

    public ObservableCollection<string> CombatMessages { get; } = new();
    public ObservableCollection<CombatantViewModel> PlayerCombatants { get; } = new();
    public ObservableCollection<CombatantViewModel> EnemyCombatants { get; } = new();

    public CombatSystem Combat => _combat;

    public CombatViewModel(CombatSystem combat, GameViewModel parentViewModel)
    {
        _combat = combat;
        _parentViewModel = parentViewModel;
        _audioService = AudioService.Instance;

        _combat.OnCombatMessage += OnCombatMessage;
        _combat.OnTurnChanged += OnTurnChanged;
        _combat.OnCombatEnd += OnCombatEnd;

        RefreshCombatants();
        OnTurnChanged();
    }

    public void Cleanup()
    {
        _combat.OnCombatMessage -= OnCombatMessage;
        _combat.OnTurnChanged -= OnTurnChanged;
        _combat.OnCombatEnd -= OnCombatEnd;
    }

    private void OnCombatMessage(string message)
    {
        CombatMessages.Add(message);
        while (CombatMessages.Count > 8)
            CombatMessages.RemoveAt(0);

        // Play contextual sound effects based on combat message
        PlayCombatSound(message);
    }

    private void PlayCombatSound(string message)
    {
        var lowerMessage = message.ToLowerInvariant();

        if (lowerMessage.Contains("attacks") || lowerMessage.Contains("swings"))
        {
            _audioService.PlaySoundEffect(SoundEffect.SwordSwing);
        }

        if (lowerMessage.Contains("hits") || lowerMessage.Contains("strikes") || lowerMessage.Contains("damage"))
        {
            // Check if player or monster is hit
            if (lowerMessage.Contains("you take") || lowerMessage.Contains("party member"))
            {
                _audioService.PlaySoundEffect(SoundEffect.PlayerHit);
            }
            else
            {
                _audioService.PlaySoundEffect(SoundEffect.SwordHit);
            }
        }
        else if (lowerMessage.Contains("misses") || lowerMessage.Contains("missed"))
        {
            _audioService.PlaySoundEffect(SoundEffect.SwordMiss);
        }

        if (lowerMessage.Contains("defeated") || lowerMessage.Contains("slain") || lowerMessage.Contains("dies"))
        {
            _audioService.PlaySoundEffect(SoundEffect.MonsterDeath);
        }

        if (lowerMessage.Contains("casts") || lowerMessage.Contains("spell"))
        {
            _audioService.PlaySoundEffect(SoundEffect.SpellCast);
        }

        if (lowerMessage.Contains("healed") || lowerMessage.Contains("restored"))
        {
            _audioService.PlaySoundEffect(SoundEffect.SpellHeal);
        }

        if (lowerMessage.Contains("magic damage") || lowerMessage.Contains("fire") || lowerMessage.Contains("lightning"))
        {
            _audioService.PlaySoundEffect(SoundEffect.SpellDamage);
        }
    }

    private void OnTurnChanged()
    {
        IsPlayerTurn = _combat.IsPlayerTurn;
        var current = _combat.CurrentCombatant;
        CurrentCombatantName = current?.Name ?? "Unknown";

        RefreshCombatants();
        _parentViewModel.RefreshPartyStats();
    }

    private void OnCombatEnd()
    {
        // Music is handled by MainViewModel.OnGameStateChanged when the engine
        // transitions back to the appropriate state (Dungeon/Overworld/Town).
        // Playing Victory here would override the location music and loop forever.
        _parentViewModel.ExitCombat();
    }

    private void RefreshCombatants()
    {
        PlayerCombatants.Clear();
        foreach (var pc in _combat.PlayerCharacters)
        {
            PlayerCombatants.Add(new CombatantViewModel(pc));
        }

        EnemyCombatants.Clear();
        foreach (var monster in _combat.Monsters)
        {
            EnemyCombatants.Add(new CombatantViewModel(monster));
        }
    }

    [RelayCommand]
    private void Attack()
    {
        if (!IsPlayerTurn || !_combat.IsCombatActive) return;

        _audioService.PlaySoundEffect(SoundEffect.MenuSelect);
        PendingAction = CombatActionType.Attack;
        IsSelectingTarget = true;

        // Default to closest enemy by distance
        var current = _combat.CurrentCombatant!;
        var nearestEnemy = _combat.Monsters
            .Where(m => m.IsAlive)
            .OrderBy(m => Math.Max(Math.Abs(m.X - current.X), Math.Abs(m.Y - current.Y)))
            .FirstOrDefault();
        if (nearestEnemy != null)
        {
            TargetX = nearestEnemy.X;
            TargetY = nearestEnemy.Y;
        }
    }

    [RelayCommand]
    private void Cast()
    {
        if (!IsPlayerTurn || !_combat.IsCombatActive) return;

        var combatant = _combat.CurrentCombatant as CharacterCombatant;
        if (combatant == null) return;

        var character = combatant.Character;
        var classDef = character.ClassDef;
        var spells = Spell.GetSpellsForClass(classDef)
            .Where(s => !s.IsFieldOnly)
            .ToList();

        if (spells.Count == 0)
        {
            CombatMessages.Add($"{character.Name} cannot cast spells!");
            _audioService.PlaySoundEffect(SoundEffect.Blocked);
            return;
        }

        AvailableSpells.Clear();
        for (int i = 0; i < spells.Count; i++)
        {
            AvailableSpells.Add(new SpellChoiceViewModel(spells[i], character.CurrentMP >= spells[i].ManaCost, i == 0));
        }

        SelectedSpellIndex = 0;
        IsSelectingSpell = true;
        _audioService.PlaySoundEffect(SoundEffect.MenuSelect);
    }

    [RelayCommand]
    private void Pass()
    {
        if (!IsPlayerTurn || !_combat.IsCombatActive) return;

        _audioService.PlaySoundEffect(SoundEffect.MenuSelect);
        var action = new CombatAction(CombatActionType.Pass);
        _combat.ExecutePlayerAction(action);
    }

    [RelayCommand]
    private void Flee()
    {
        if (!IsPlayerTurn || !_combat.IsCombatActive) return;

        _audioService.PlaySoundEffect(SoundEffect.MenuSelect);
        var action = new CombatAction(CombatActionType.Flee);
        _combat.ExecutePlayerAction(action);
    }

    private void MovePlayer(int dx, int dy)
    {
        if (!IsPlayerTurn || !_combat.IsCombatActive) return;

        var currentCombatant = _combat.CurrentCombatant;
        if (currentCombatant == null) return;

        int newX = currentCombatant.X + dx;
        int newY = currentCombatant.Y + dy;

        var action = new CombatAction(CombatActionType.Move, newX, newY);
        var result = _combat.ExecutePlayerAction(action);

        if (result.Success)
        {
            _audioService.PlaySoundEffect(SoundEffect.Footstep);
        }
        else
        {
            _audioService.PlaySoundEffect(SoundEffect.Blocked);
        }
    }

    [RelayCommand]
    private void ConfirmTarget()
    {
        if (!IsSelectingTarget || !IsPlayerTurn) return;

        _audioService.PlaySoundEffect(SoundEffect.MenuConfirm);
        var action = new CombatAction(PendingAction, TargetX, TargetY, PendingSpell);
        _combat.ExecutePlayerAction(action);

        IsSelectingTarget = false;
        PendingSpell = null;
    }

    [RelayCommand]
    private void CancelTarget()
    {
        _audioService.PlaySoundEffect(SoundEffect.MenuCancel);
        IsSelectingTarget = false;
        PendingSpell = null;
    }

    [RelayCommand]
    private void ConfirmSpellSelection()
    {
        if (!IsSelectingSpell || SelectedSpellIndex < 0 || SelectedSpellIndex >= AvailableSpells.Count) return;

        var selectedSpellVm = AvailableSpells[SelectedSpellIndex];
        if (!selectedSpellVm.CanAfford)
        {
            CombatMessages.Add("Not enough MP!");
            _audioService.PlaySoundEffect(SoundEffect.Blocked);
            return;
        }

        var spell = selectedSpellVm.Spell;
        IsSelectingSpell = false;
        AvailableSpells.Clear();

        if (spell.TargetsSelf && !spell.TargetsEnemy && !spell.TargetsParty)
        {
            // Self-only spell - execute immediately on caster's position
            var current = _combat.CurrentCombatant!;
            var action = new CombatAction(CombatActionType.Cast, current.X, current.Y, spell.Type);
            _combat.ExecutePlayerAction(action);
        }
        else
        {
            // Need target selection
            PendingAction = CombatActionType.Cast;
            PendingSpell = spell.Type;
            IsSelectingTarget = true;

            if (spell.TargetsEnemy)
            {
                var caster = _combat.CurrentCombatant!;
                var nearestEnemy = _combat.Monsters
                    .Where(m => m.IsAlive)
                    .OrderBy(m => Math.Max(Math.Abs(m.X - caster.X), Math.Abs(m.Y - caster.Y)))
                    .FirstOrDefault();
                if (nearestEnemy != null)
                {
                    TargetX = nearestEnemy.X;
                    TargetY = nearestEnemy.Y;
                }
            }
            else if (spell.TargetsParty)
            {
                var current = _combat.CurrentCombatant!;
                TargetX = current.X;
                TargetY = current.Y;
            }
        }
    }

    [RelayCommand]
    private void CancelSpellSelection()
    {
        _audioService.PlaySoundEffect(SoundEffect.MenuCancel);
        IsSelectingSpell = false;
        AvailableSpells.Clear();
    }

    private void MoveSpellSelection(int delta)
    {
        if (AvailableSpells.Count == 0) return;

        AvailableSpells[SelectedSpellIndex].IsSelected = false;
        SelectedSpellIndex = Math.Clamp(SelectedSpellIndex + delta, 0, AvailableSpells.Count - 1);
        AvailableSpells[SelectedSpellIndex].IsSelected = true;
    }

    [RelayCommand]
    private void MoveTarget(string direction)
    {
        if (!IsSelectingTarget) return;

        switch (direction)
        {
            case "Up": TargetY = Math.Max(0, TargetY - 1); break;
            case "Down": TargetY = Math.Min(CombatSystem.GridHeight - 1, TargetY + 1); break;
            case "Left": TargetX = Math.Max(0, TargetX - 1); break;
            case "Right": TargetX = Math.Min(CombatSystem.GridWidth - 1, TargetX + 1); break;
        }
    }

    public void HandleKeyPress(string key)
    {
        if (IsSelectingSpell)
        {
            switch (key.ToUpper())
            {
                case "W":
                case "UP":
                    MoveSpellSelection(-1);
                    break;
                case "S":
                case "DOWN":
                    MoveSpellSelection(1);
                    break;
                case "RETURN":
                case "ENTER":
                case "SPACE":
                    ConfirmSpellSelection();
                    break;
                case "ESCAPE":
                    CancelSpellSelection();
                    break;
            }
        }
        else if (IsSelectingTarget)
        {
            switch (key.ToUpper())
            {
                case "W":
                case "UP":
                    MoveTarget("Up");
                    break;
                case "S":
                case "DOWN":
                    MoveTarget("Down");
                    break;
                case "A":
                case "LEFT":
                    MoveTarget("Left");
                    break;
                case "D":
                case "RIGHT":
                    MoveTarget("Right");
                    break;
                case "RETURN":
                case "ENTER":
                case "SPACE":
                    ConfirmTarget();
                    break;
                case "ESCAPE":
                    CancelTarget();
                    break;
            }
        }
        else if (IsPlayerTurn)
        {
            switch (key.ToUpper())
            {
                // Movement with arrow keys or WASD
                case "W":
                case "UP":
                    MovePlayer(0, -1);
                    break;
                case "S":
                case "DOWN":
                    MovePlayer(0, 1);
                    break;
                case "A":
                case "LEFT":
                    MovePlayer(-1, 0);
                    break;
                case "D":
                case "RIGHT":
                    MovePlayer(1, 0);
                    break;

                // Actions with specific keys
                case "T":  // T for aTtack (since A is now move left)
                    Attack();
                    break;
                case "C":
                    Cast();
                    break;
                case "P":
                    Pass();
                    break;
                case "F":
                    Flee();
                    break;
            }
        }
    }
}

public partial class CombatantViewModel : ObservableObject
{
    private readonly ICombatant _combatant;

    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private int _currentHp;

    [ObservableProperty]
    private bool _isAlive;

    [ObservableProperty]
    private int _x;

    [ObservableProperty]
    private int _y;

    [ObservableProperty]
    private bool _isPlayer;

    public CombatantViewModel(ICombatant combatant)
    {
        _combatant = combatant;
        Name = combatant.Name;
        CurrentHp = combatant.CurrentHP;
        IsAlive = combatant.IsAlive;
        X = combatant.X;
        Y = combatant.Y;
        IsPlayer = combatant is CharacterCombatant;
    }
}

public partial class SpellChoiceViewModel : ObservableObject
{
    public Spell Spell { get; }
    public bool CanAfford { get; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(DisplayText))]
    private bool _isSelected;

    public string DisplayText
    {
        get
        {
            var marker = IsSelected ? "> " : "  ";
            var school = Spell.School == SpellSchool.Wizard ? "WIZ" : "CLR";
            var afford = CanAfford ? "" : " (low MP)";
            return $"{marker}[{school}] {Spell.Name} - MP:{Spell.ManaCost}{afford}";
        }
    }

    public double ItemOpacity => CanAfford ? 1.0 : 0.5;

    public SpellChoiceViewModel(Spell spell, bool canAfford, bool isSelected)
    {
        Spell = spell;
        CanAfford = canAfford;
        _isSelected = isSelected;
    }
}
