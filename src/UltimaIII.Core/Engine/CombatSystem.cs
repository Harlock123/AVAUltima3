using UltimaIII.Core.Enums;
using UltimaIII.Core.Models;

namespace UltimaIII.Core.Engine;

/// <summary>
/// Combat action types.
/// </summary>
public enum CombatActionType
{
    Attack,
    Cast,
    UseItem,
    Move,
    Pass,
    Flee
}

/// <summary>
/// Represents a combat action.
/// </summary>
public record CombatAction(
    CombatActionType Type,
    int TargetX = -1,
    int TargetY = -1,
    SpellType? Spell = null,
    string? ItemId = null
);

/// <summary>
/// Result of a combat action.
/// </summary>
public record CombatResult(
    bool Success,
    string Message,
    int Damage = 0,
    bool TargetKilled = false
);

/// <summary>
/// A combatant in the combat system (either player character or monster).
/// </summary>
public interface ICombatant
{
    string Name { get; }
    int CurrentHP { get; }
    bool IsAlive { get; }
    bool CanAct { get; }
    int X { get; set; }
    int Y { get; set; }
    int GetDefense();
    void TakeDamage(int damage);
    StatusEffect Status { get; set; }
}

/// <summary>
/// Wrapper to make Character implement ICombatant explicitly.
/// </summary>
public class CharacterCombatant : ICombatant
{
    private readonly Character _character;

    public CharacterCombatant(Character character)
    {
        _character = character;
    }

    public string Name => _character.Name;
    public int CurrentHP => _character.CurrentHP;
    public bool IsAlive => _character.IsAlive;
    public bool CanAct => _character.CanAct;
    public int X { get => _character.CombatX; set => _character.CombatX = value; }
    public int Y { get => _character.CombatY; set => _character.CombatY = value; }
    public int GetDefense() => _character.GetDefense();
    public void TakeDamage(int damage) => _character.TakeDamage(damage);
    public StatusEffect Status { get => _character.Status; set => _character.Status = value; }
    public Character Character => _character;
}

/// <summary>
/// Wrapper to make Monster implement ICombatant.
/// </summary>
public class MonsterCombatant : ICombatant
{
    private readonly Monster _monster;

    public MonsterCombatant(Monster monster)
    {
        _monster = monster;
    }

    public string Name => _monster.Definition.Name;
    public int CurrentHP => _monster.CurrentHP;
    public bool IsAlive => _monster.IsAlive;
    public bool CanAct => _monster.CanAct;
    public int X { get => _monster.X; set => _monster.X = value; }
    public int Y { get => _monster.Y; set => _monster.Y = value; }
    public int GetDefense() => _monster.Definition.Defense;
    public void TakeDamage(int damage) => _monster.TakeDamage(damage);
    public StatusEffect Status { get => _monster.Status; set => _monster.Status = value; }
    public Monster Monster => _monster;
}

/// <summary>
/// Manages tactical turn-based combat.
/// </summary>
public class CombatSystem
{
    public const int GridWidth = 11;
    public const int GridHeight = 11;

    private readonly Random _rng;
    private readonly List<ICombatant> _turnOrder = new();
    private int _currentTurnIndex;
    private TileType[,] _terrain;

    public List<CharacterCombatant> PlayerCharacters { get; } = new();
    public List<MonsterCombatant> Monsters { get; } = new();
    public bool IsCombatActive { get; private set; }
    public ICombatant? CurrentCombatant => _turnOrder.Count > 0 ? _turnOrder[_currentTurnIndex] : null;
    public bool IsPlayerTurn => CurrentCombatant is CharacterCombatant;
    public List<string> CombatLog { get; } = new();

    public event Action<string>? OnCombatMessage;
    public event Action? OnCombatEnd;
    public event Action? OnTurnChanged;
    public event Action<int, int, bool>? OnSpellEffect; // (targetX, targetY, isBeneficial)

    public CombatSystem(Random? rng = null)
    {
        _rng = rng ?? new Random();
        _terrain = new TileType[GridWidth, GridHeight];
    }

    public void StartCombat(Party party, List<Monster> monsters, TileType baseTerrain = TileType.Grass)
    {
        PlayerCharacters.Clear();
        Monsters.Clear();
        _turnOrder.Clear();
        CombatLog.Clear();

        // Initialize terrain
        InitializeTerrain(baseTerrain);

        // Place party members on the left side
        int partyIndex = 0;
        foreach (var member in party.GetActiveMembersForCombat())
        {
            var combatant = new CharacterCombatant(member);
            combatant.X = 1;
            combatant.Y = 3 + partyIndex;
            PlayerCharacters.Add(combatant);
            partyIndex++;
        }

        // Place monsters on the right side
        int monsterIndex = 0;
        foreach (var monster in monsters)
        {
            var combatant = new MonsterCombatant(monster);
            combatant.X = GridWidth - 2;
            combatant.Y = 2 + (monsterIndex % 7);
            Monsters.Add(combatant);
            monsterIndex++;
        }

        // Determine turn order based on dexterity/speed
        DetermineInitiative();

        IsCombatActive = true;
        _currentTurnIndex = 0;

        LogMessage("Combat begins!");
        OnTurnChanged?.Invoke();

        // If a monster won initiative, execute their turn automatically
        if (!IsPlayerTurn && IsCombatActive)
        {
            ExecuteMonsterTurn();
        }
    }

    private void InitializeTerrain(TileType baseTerrain)
    {
        for (int y = 0; y < GridHeight; y++)
        {
            for (int x = 0; x < GridWidth; x++)
            {
                _terrain[x, y] = baseTerrain;
            }
        }

        // Add some random obstacles
        int obstacles = _rng.Next(3, 7);
        for (int i = 0; i < obstacles; i++)
        {
            int x = _rng.Next(3, GridWidth - 3);
            int y = _rng.Next(1, GridHeight - 1);
            _terrain[x, y] = _rng.Next(2) == 0 ? TileType.Forest : TileType.Mountain;
        }
    }

    private void DetermineInitiative()
    {
        var allCombatants = new List<(ICombatant combatant, int initiative)>();

        foreach (var pc in PlayerCharacters)
        {
            int initiative = pc.Character.Stats.Dexterity + _rng.Next(1, 21);
            allCombatants.Add((pc, initiative));
        }

        foreach (var monster in Monsters)
        {
            int initiative = monster.Monster.Definition.Speed + _rng.Next(1, 21);
            allCombatants.Add((monster, initiative));
        }

        _turnOrder.Clear();
        foreach (var (combatant, _) in allCombatants.OrderByDescending(c => c.initiative))
        {
            _turnOrder.Add(combatant);
        }
    }

    public TileType GetTerrain(int x, int y)
    {
        if (x < 0 || x >= GridWidth || y < 0 || y >= GridHeight)
            return TileType.Void;
        return _terrain[x, y];
    }

    public ICombatant? GetCombatantAt(int x, int y)
    {
        foreach (var pc in PlayerCharacters)
        {
            if (pc.X == x && pc.Y == y && pc.IsAlive)
                return pc;
        }
        foreach (var monster in Monsters)
        {
            if (monster.X == x && monster.Y == y && monster.IsAlive)
                return monster;
        }
        return null;
    }

    public bool IsValidTarget(int x, int y, bool forPlayer)
    {
        var target = GetCombatantAt(x, y);
        if (target == null) return false;

        if (forPlayer)
            return target is MonsterCombatant;
        else
            return target is CharacterCombatant;
    }

    public CombatResult ExecutePlayerAction(CombatAction action)
    {
        if (!IsCombatActive || !IsPlayerTurn)
            return new CombatResult(false, "Not player's turn");

        var combatant = CurrentCombatant as CharacterCombatant;
        if (combatant == null || !combatant.CanAct)
            return new CombatResult(false, "Character cannot act");

        CombatResult result = action.Type switch
        {
            CombatActionType.Attack => ExecuteAttack(combatant, action.TargetX, action.TargetY),
            CombatActionType.Cast => ExecuteSpell(combatant, action.Spell!.Value, action.TargetX, action.TargetY),
            CombatActionType.Move => ExecuteMove(combatant, action.TargetX, action.TargetY),
            CombatActionType.Pass => new CombatResult(true, $"{combatant.Name} waits."),
            CombatActionType.Flee => AttemptFlee(combatant),
            _ => new CombatResult(false, "Invalid action")
        };

        LogMessage(result.Message);

        if (result.Success)
        {
            AdvanceTurn();
        }

        return result;
    }

    private CombatResult ExecuteAttack(CharacterCombatant attacker, int targetX, int targetY)
    {
        var target = GetCombatantAt(targetX, targetY);
        if (target == null || target is CharacterCombatant)
            return new CombatResult(false, "Invalid target");

        // Check range
        int distance = Math.Max(Math.Abs(attacker.X - targetX), Math.Abs(attacker.Y - targetY));
        int range = attacker.Character.GetWeaponRange();

        if (distance > range)
            return new CombatResult(false, "Target out of range");

        // Get monster definition for creature-type bonuses
        var monsterDef = (target as MonsterCombatant)?.Monster.Definition;

        // Attack roll (with creature-type attack bonus)
        int attackBonus = attacker.Character.GetAttackBonus()
                        + attacker.Character.GetAttackBonusVsTarget(monsterDef);
        int defenseValue = target.GetDefense();
        int roll = _rng.Next(1, 21) + attackBonus;

        if (roll >= 10 + defenseValue)
        {
            // Hit! Roll damage with creature-type bonus
            int damage = attacker.Character.RollDamage(_rng, monsterDef);

            // Check for critical hit
            bool isCritical = false;
            int critChance = attacker.Character.GetCriticalHitChance();
            if (critChance > 0 && _rng.Next(100) < critChance)
            {
                damage *= 2;
                isCritical = true;
            }

            target.TakeDamage(damage);

            bool killed = !target.IsAlive;
            string critText = isCritical ? "CRITICAL! " : "";
            string message = killed
                ? $"{critText}{attacker.Name} strikes {target.Name} for {damage} damage, slaying it!"
                : $"{critText}{attacker.Name} hits {target.Name} for {damage} damage.";

            return new CombatResult(true, message, damage, killed);
        }
        else
        {
            return new CombatResult(true, $"{attacker.Name} misses {target.Name}.");
        }
    }

    private CombatResult ExecuteMove(CharacterCombatant character, int targetX, int targetY)
    {
        // Check if target is adjacent (1 tile away in any direction)
        int dx = Math.Abs(character.X - targetX);
        int dy = Math.Abs(character.Y - targetY);

        if (dx > 1 || dy > 1)
            return new CombatResult(false, "Can only move one tile at a time");

        if (dx == 0 && dy == 0)
            return new CombatResult(false, "Already at that position");

        if (!IsValidMove(targetX, targetY))
            return new CombatResult(false, "Cannot move there");

        character.X = targetX;
        character.Y = targetY;

        return new CombatResult(true, $"{character.Name} moves.");
    }

    private CombatResult ExecuteSpell(CharacterCombatant caster, SpellType spellType, int targetX, int targetY)
    {
        var spell = Spell.Get(spellType);
        var character = caster.Character;

        // Check if character can cast this spell
        var classDef = character.ClassDef;
        bool canCast = (spell.School == SpellSchool.Wizard && classDef.CanUseWizardSpells && spell.Level <= classDef.MaxWizardSpellLevel) ||
                       (spell.School == SpellSchool.Cleric && classDef.CanUseClericSpells && spell.Level <= classDef.MaxClericSpellLevel);

        if (!canCast)
            return new CombatResult(false, $"{character.Name} cannot cast {spell.Name}!");

        if (!character.SpendMana(spell.ManaCost))
            return new CombatResult(false, $"Not enough mana for {spell.Name}!");

        // Execute spell effect
        bool isBeneficial = spell.HealAmount > 0 || spell.CuresStatus != StatusEffect.None;

        if (spell.HealAmount > 0)
        {
            // Healing spell
            var target = GetCombatantAt(targetX, targetY) as CharacterCombatant;
            if (target != null)
            {
                target.Character.Heal(spell.HealAmount);
                OnSpellEffect?.Invoke(targetX, targetY, true);
                return new CombatResult(true, $"{character.Name} casts {spell.Name}, healing {target.Name} for {spell.HealAmount}!");
            }
        }

        if (spell.MinDamage > 0)
        {
            // Damage spell
            var target = GetCombatantAt(targetX, targetY);
            if (target == null)
                return new CombatResult(true, $"{character.Name} casts {spell.Name} but hits nothing!");

            int damage = spell.RollDamage(_rng);
            target.TakeDamage(damage);
            OnSpellEffect?.Invoke(targetX, targetY, false);

            bool killed = !target.IsAlive;
            return new CombatResult(true,
                $"{character.Name} casts {spell.Name} on {target.Name} for {damage} damage!" + (killed ? " It is destroyed!" : ""),
                damage, killed);
        }

        if (spell.CuresStatus != StatusEffect.None)
        {
            var target = GetCombatantAt(targetX, targetY) as CharacterCombatant;
            if (target != null)
            {
                var cured = target.Status & spell.CuresStatus;
                if (cured != StatusEffect.None)
                {
                    target.Status &= ~spell.CuresStatus;
                    OnSpellEffect?.Invoke(targetX, targetY, true);
                    return new CombatResult(true, $"{character.Name} casts {spell.Name}, curing {target.Name} of {cured}!");
                }
                return new CombatResult(true, $"{character.Name} casts {spell.Name} on {target.Name}, but there is nothing to cure.");
            }
        }

        if (spell.AppliesStatus != StatusEffect.None)
        {
            var target = GetCombatantAt(targetX, targetY);
            if (target != null)
            {
                target.Status |= spell.AppliesStatus;
                OnSpellEffect?.Invoke(targetX, targetY, false);
                return new CombatResult(true, $"{character.Name} casts {spell.Name} on {target.Name}!");
            }
        }

        return new CombatResult(true, $"{character.Name} casts {spell.Name}.");
    }

    private CombatResult AttemptFlee(CharacterCombatant character)
    {
        // 50% chance to flee, modified by dexterity
        int fleeChance = 50 + character.Character.Stats.Dexterity;
        if (_rng.Next(100) < fleeChance)
        {
            // Remove all party members from combat
            IsCombatActive = false;
            OnCombatEnd?.Invoke();
            return new CombatResult(true, "The party flees from combat!");
        }
        return new CombatResult(true, $"{character.Name} fails to escape!");
    }

    public void ExecuteMonsterTurn()
    {
        if (!IsCombatActive || IsPlayerTurn) return;

        var monster = CurrentCombatant as MonsterCombatant;
        if (monster == null || !monster.CanAct)
        {
            AdvanceTurn();
            return;
        }

        // Pick a target — spread monsters across players instead of all piling on one
        var target = PickTarget(monster);

        if (target == null)
        {
            AdvanceTurn();
            return;
        }

        int range = monster.Monster.Definition.Range;
        int distToTarget = Math.Max(Math.Abs(monster.X - target.X), Math.Abs(monster.Y - target.Y));

        // Check if monster should cast its special ability
        if (monster.Monster.Definition.SpecialAbility.HasValue && _rng.Next(100) < 40)
        {
            var spell = Spell.Get(monster.Monster.Definition.SpecialAbility.Value);

            // Find best target within spell range
            CharacterCombatant? spellTarget = null;
            int spellTargetDist = int.MaxValue;
            foreach (var pc in PlayerCharacters)
            {
                if (!pc.IsAlive) continue;
                int dist = Math.Max(Math.Abs(monster.X - pc.X), Math.Abs(monster.Y - pc.Y));
                if (dist <= spell.Range && dist < spellTargetDist)
                {
                    spellTargetDist = dist;
                    spellTarget = pc;
                }
            }

            if (spellTarget != null)
            {
                ExecuteMonsterSpell(monster, spell, spellTarget);
                AdvanceTurn();
                return;
            }
        }

        if (distToTarget <= range)
        {
            // Attack!
            int attackRoll = _rng.Next(1, 21) + monster.Monster.Definition.Speed / 2;
            int defense = target.GetDefense();

            if (attackRoll >= 10 + defense)
            {
                int damage = monster.Monster.RollDamage(_rng);
                target.TakeDamage(damage);

                string message = target.IsAlive
                    ? $"{monster.Name} hits {target.Name} for {damage} damage!"
                    : $"{monster.Name} strikes down {target.Name}!";
                LogMessage(message);

                // Apply status effects
                if (monster.Monster.Definition.InflictsStatus != StatusEffect.None && _rng.Next(100) < 25)
                {
                    target.Status |= monster.Monster.Definition.InflictsStatus;
                    LogMessage($"{target.Name} is affected by {monster.Monster.Definition.InflictsStatus}!");
                }
            }
            else
            {
                LogMessage($"{monster.Name} misses {target.Name}.");
            }
        }
        else
        {
            // Move toward target with spread-out behavior
            MoveToward(monster, target);
        }

        AdvanceTurn();
    }

    /// <summary>
    /// Picks a target player for a monster, distributing attackers across players
    /// so they don't all pile onto one character.
    /// </summary>
    private CharacterCombatant? PickTarget(MonsterCombatant monster)
    {
        var alivePlayers = PlayerCharacters.Where(pc => pc.IsAlive).ToList();
        if (alivePlayers.Count == 0) return null;

        // Count how many other alive monsters are already targeting each player
        // (approximated by which player each monster is closest to)
        var threatCount = new Dictionary<CharacterCombatant, int>();
        foreach (var pc in alivePlayers)
            threatCount[pc] = 0;

        foreach (var other in Monsters)
        {
            if (other == monster || !other.IsAlive) continue;
            CharacterCombatant? closestTo = null;
            int closestDist = int.MaxValue;
            foreach (var pc in alivePlayers)
            {
                int d = Math.Max(Math.Abs(other.X - pc.X), Math.Abs(other.Y - pc.Y));
                if (d < closestDist) { closestDist = d; closestTo = pc; }
            }
            if (closestTo != null) threatCount[closestTo]++;
        }

        // Score each player: prefer close + less-crowded targets
        CharacterCombatant? best = null;
        double bestScore = double.MaxValue;

        foreach (var pc in alivePlayers)
        {
            int dist = Math.Max(Math.Abs(monster.X - pc.X), Math.Abs(monster.Y - pc.Y));
            // Score = distance + crowding penalty (2 tiles per existing attacker)
            double score = dist + threatCount[pc] * 2.0;
            if (score < bestScore)
            {
                bestScore = score;
                best = pc;
            }
        }

        return best;
    }

    /// <summary>
    /// Moves a monster toward a target, trying multiple directions if the direct
    /// path is blocked, and preferring tiles that aren't adjacent to other monsters
    /// (to encourage spreading out / flanking).
    /// </summary>
    private void MoveToward(MonsterCombatant monster, CharacterCombatant target)
    {
        int dx = Math.Sign(target.X - monster.X);
        int dy = Math.Sign(target.Y - monster.Y);

        // Build candidate moves: direct diagonal first, then axis-aligned, then sidesteps
        var candidates = new List<(int x, int y)>();

        // Primary: direct toward target
        if (dx != 0 && dy != 0) candidates.Add((monster.X + dx, monster.Y + dy));
        if (dx != 0) candidates.Add((monster.X + dx, monster.Y));
        if (dy != 0) candidates.Add((monster.X, monster.Y + dy));

        // Sidesteps: move perpendicular to get around obstacles
        if (dx != 0)
        {
            candidates.Add((monster.X + dx, monster.Y + 1));
            candidates.Add((monster.X + dx, monster.Y - 1));
        }
        if (dy != 0)
        {
            candidates.Add((monster.X + 1, monster.Y + dy));
            candidates.Add((monster.X - 1, monster.Y + dy));
        }

        // Pure lateral moves as last resort
        candidates.Add((monster.X, monster.Y + 1));
        candidates.Add((monster.X, monster.Y - 1));
        candidates.Add((monster.X + 1, monster.Y));
        candidates.Add((monster.X - 1, monster.Y));

        // Remove duplicates
        var seen = new HashSet<(int, int)>();
        var unique = new List<(int x, int y)>();
        foreach (var c in candidates)
        {
            if (seen.Add(c)) unique.Add(c);
        }

        // Score each valid candidate: closer to target is better, being near
        // other monsters is worse (encourages spreading / flanking)
        (int x, int y)? bestMove = null;
        double bestScore = double.MaxValue;

        foreach (var (cx, cy) in unique)
        {
            if (!IsValidMove(cx, cy)) continue;

            int distToTarget = Math.Max(Math.Abs(cx - target.X), Math.Abs(cy - target.Y));

            // Count adjacent friendly monsters (penalize crowding)
            int adjacentMonsters = 0;
            foreach (var other in Monsters)
            {
                if (other == monster || !other.IsAlive) continue;
                if (Math.Max(Math.Abs(cx - other.X), Math.Abs(cy - other.Y)) <= 1)
                    adjacentMonsters++;
            }

            double score = distToTarget + adjacentMonsters * 1.5;

            if (score < bestScore)
            {
                bestScore = score;
                bestMove = (cx, cy);
            }
        }

        if (bestMove.HasValue)
        {
            monster.X = bestMove.Value.x;
            monster.Y = bestMove.Value.y;
            LogMessage($"{monster.Name} moves.");
        }
    }

    private void ExecuteMonsterSpell(MonsterCombatant monster, Spell spell, CharacterCombatant target)
    {
        if (spell.MinDamage > 0)
        {
            int damage = spell.RollDamage(_rng);
            target.TakeDamage(damage);
            OnSpellEffect?.Invoke(target.X, target.Y, false);

            bool killed = !target.IsAlive;
            string msg = killed
                ? $"{monster.Name} casts {spell.Name} on {target.Name} for {damage} damage, slaying them!"
                : $"{monster.Name} casts {spell.Name} on {target.Name} for {damage} damage!";
            LogMessage(msg);

            if (spell.AppliesStatus != StatusEffect.None)
            {
                target.Status |= spell.AppliesStatus;
                LogMessage($"{target.Name} is {spell.AppliesStatus}!");
            }

            // AOE: hit nearby player characters too
            if (spell.AreaOfEffect > 0)
            {
                foreach (var pc in PlayerCharacters)
                {
                    if (pc == target || !pc.IsAlive) continue;
                    int dist = Math.Max(Math.Abs(target.X - pc.X), Math.Abs(target.Y - pc.Y));
                    if (dist <= spell.AreaOfEffect)
                    {
                        int aoeDamage = spell.RollDamage(_rng);
                        pc.TakeDamage(aoeDamage);
                        OnSpellEffect?.Invoke(pc.X, pc.Y, false);
                        if (spell.AppliesStatus != StatusEffect.None)
                            pc.Status |= spell.AppliesStatus;
                        LogMessage($"{pc.Name} is caught in the {spell.Name} for {aoeDamage} damage!");
                    }
                }
            }
        }
        else if (spell.AppliesStatus != StatusEffect.None)
        {
            target.Status |= spell.AppliesStatus;
            OnSpellEffect?.Invoke(target.X, target.Y, false);
            LogMessage($"{monster.Name} casts {spell.Name} on {target.Name}!");
            LogMessage($"{target.Name} is {spell.AppliesStatus}!");
        }
    }

    private bool IsValidMove(int x, int y)
    {
        if (x < 0 || x >= GridWidth || y < 0 || y >= GridHeight) return false;
        if (!_terrain[x, y].IsPassable()) return false;
        if (GetCombatantAt(x, y) != null) return false;
        return true;
    }

    private void AdvanceTurn()
    {
        // Check for combat end conditions
        if (!PlayerCharacters.Any(pc => pc.IsAlive))
        {
            IsCombatActive = false;
            LogMessage("The party has been defeated!");
            OnCombatEnd?.Invoke();
            return;
        }

        if (!Monsters.Any(m => m.IsAlive))
        {
            IsCombatActive = false;
            LogMessage("Victory! All enemies defeated!");
            OnCombatEnd?.Invoke();
            return;
        }

        // Find next living combatant
        int attempts = 0;
        do
        {
            _currentTurnIndex = (_currentTurnIndex + 1) % _turnOrder.Count;
            attempts++;
        } while (!_turnOrder[_currentTurnIndex].CanAct && attempts < _turnOrder.Count);

        OnTurnChanged?.Invoke();

        // If it's a monster's turn, execute it automatically
        if (!IsPlayerTurn && IsCombatActive)
        {
            // Small delay could be added here for UI
            ExecuteMonsterTurn();
        }
    }

    private void LogMessage(string message)
    {
        CombatLog.Add(message);
        OnCombatMessage?.Invoke(message);
    }

    public (int totalExp, int totalGold, List<Item> lootItems) GetCombatRewards()
    {
        int exp = 0;
        int gold = 0;
        var loot = new List<Item>();

        foreach (var monster in Monsters)
        {
            if (!monster.IsAlive)
            {
                exp += monster.Monster.Definition.ExperienceValue;
                gold += _rng.Next(monster.Monster.Definition.GoldDrop / 2, monster.Monster.Definition.GoldDrop + 1);

                foreach (var drop in monster.Monster.Definition.LootTable)
                {
                    if (_rng.Next(100) < drop.DropChancePercent)
                    {
                        var template = ItemRegistry.FindById(drop.ItemId);
                        if (template != null)
                        {
                            loot.Add(ItemRegistry.CloneItem(template));
                        }
                    }
                }
            }
        }

        return (exp, gold, loot);
    }
}
