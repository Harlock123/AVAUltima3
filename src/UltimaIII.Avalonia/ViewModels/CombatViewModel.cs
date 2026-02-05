using System;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using UltimaIII.Core.Engine;
using UltimaIII.Core.Enums;
using UltimaIII.Core.Models;

namespace UltimaIII.Avalonia.ViewModels;

public partial class CombatViewModel : ViewModelBase
{
    private readonly CombatSystem _combat;
    private readonly GameViewModel _parentViewModel;

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
        // TODO: Show spell selection menu
        // For now, default to first available offensive spell
    }

    [RelayCommand]
    private void Pass()
    {
        if (!IsPlayerTurn || !_combat.IsCombatActive) return;

        var action = new CombatAction(CombatActionType.Pass);
        _combat.ExecutePlayerAction(action);
    }

    [RelayCommand]
    private void Flee()
    {
        if (!IsPlayerTurn || !_combat.IsCombatActive) return;

        var action = new CombatAction(CombatActionType.Flee);
        _combat.ExecutePlayerAction(action);
    }

    [RelayCommand]
    private void ConfirmTarget()
    {
        if (!IsSelectingTarget || !IsPlayerTurn) return;

        var action = new CombatAction(PendingAction, TargetX, TargetY, PendingSpell);
        _combat.ExecutePlayerAction(action);

        IsSelectingTarget = false;
        PendingSpell = null;
    }

    [RelayCommand]
    private void CancelTarget()
    {
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
                case "A":
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
