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
    private bool _isSelectingTarget;

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
    }

    private void OnCombatEnd()
    {
        // Check if combat was won or lost (based on remaining party members)
        bool partyWon = _combat.PlayerCharacters.Any(pc => pc.IsAlive);
        if (partyWon)
        {
            _audioService.PlayMusic(MusicTrack.Victory);
        }
        // Note: If party lost, MainViewModel will switch to Defeat music via GameState.GameOver

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

        // Default to nearest enemy
        var nearestEnemy = _combat.Monsters.Where(m => m.IsAlive).FirstOrDefault();
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
        _audioService.PlaySoundEffect(SoundEffect.MenuSelect);
        // TODO: Show spell selection menu
        // For now, default to first available offensive spell
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
        if (IsSelectingTarget)
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
