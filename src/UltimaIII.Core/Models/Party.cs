using UltimaIII.Core.Enums;

namespace UltimaIII.Core.Models;

/// <summary>
/// The player's party of characters.
/// </summary>
public class Party
{
    public const int MaxPartySize = 4;

    private readonly List<Character> _members = new();
    private readonly List<Item> _sharedInventory = new();

    public IReadOnlyList<Character> Members => _members;
    public IReadOnlyList<Item> SharedInventory => _sharedInventory;
    public int Gold { get; set; } = 400; // Starting gold in Ultima III
    public int Food { get; set; } = 200; // Food units

    // World position
    public int X { get; set; }
    public int Y { get; set; }
    public Direction Facing { get; set; } = Direction.South;

    // Current map context
    public string CurrentMapId { get; set; } = "overworld";
    public int DungeonLevel { get; set; } = 0;

    // Vehicle
    public bool OnShip { get; set; } = false;
    public bool OnHorse { get; set; } = false;

    // Moon phases for special events
    public int MoonPhase1 { get; set; } = 0;
    public int MoonPhase2 { get; set; } = 0;

    // Time tracking
    public int TurnCount { get; set; } = 0;
    public int DayCount { get; set; } = 1;
    public bool IsNight => (TurnCount / 100) % 2 == 1;

    // Quest flags
    public HashSet<string> Marks { get; } = new();
    public HashSet<string> CompletedQuests { get; } = new();

    public bool IsFull => _members.Count >= MaxPartySize;
    public bool IsEmpty => _members.Count == 0;
    public int Count => _members.Count;

    public Character? Leader => _members.Count > 0 ? _members[0] : null;

    public bool AddMember(Character character)
    {
        if (IsFull) return false;
        _members.Add(character);
        return true;
    }

    public bool RemoveMember(Character character)
    {
        return _members.Remove(character);
    }

    public void ReorderMember(int fromIndex, int toIndex)
    {
        if (fromIndex < 0 || fromIndex >= _members.Count) return;
        if (toIndex < 0 || toIndex >= _members.Count) return;

        var member = _members[fromIndex];
        _members.RemoveAt(fromIndex);
        _members.Insert(toIndex, member);
    }

    public IEnumerable<Character> GetLivingMembers() =>
        _members.Where(m => m.IsAlive);

    public IEnumerable<Character> GetActiveMembersForCombat() =>
        _members.Where(m => m.CanAct);

    public bool AllDead => _members.All(m => !m.IsAlive);

    public void Move(Direction direction)
    {
        var (dx, dy) = direction.ToOffset();
        X += dx;
        Y += dy;
        Facing = direction;
    }

    public void AdvanceTime(int turns = 1)
    {
        TurnCount += turns;

        // Consume food
        if (TurnCount % 10 == 0 && Food > 0)
        {
            Food -= _members.Count;
            if (Food < 0) Food = 0;
        }

        // Starvation damage
        if (Food == 0 && TurnCount % 20 == 0)
        {
            foreach (var member in GetLivingMembers())
            {
                member.TakeDamage(1);
            }
        }

        // Poison damage
        foreach (var member in GetLivingMembers())
        {
            if (member.Status.HasFlag(StatusEffect.Poisoned))
            {
                member.TakeDamage(1);
            }
        }

        // Update moon phases (simplified)
        if (TurnCount % 25 == 0)
        {
            MoonPhase1 = (MoonPhase1 + 1) % 8;
        }
        if (TurnCount % 37 == 0)
        {
            MoonPhase2 = (MoonPhase2 + 1) % 8;
        }

        // Day counter
        if (TurnCount % 200 == 0)
        {
            DayCount++;
        }
    }

    // --- Shared Inventory ---

    public void AddToInventory(Item item)
    {
        if (item.IsStackable)
        {
            var existing = _sharedInventory.FirstOrDefault(i => i.Id == item.Id);
            if (existing != null)
            {
                existing.Quantity += item.Quantity;
                return;
            }
        }
        _sharedInventory.Add(item);
    }

    public bool RemoveFromInventory(Item item)
    {
        if (item.IsStackable && item.Quantity > 1)
        {
            item.Quantity--;
            return true;
        }
        return _sharedInventory.Remove(item);
    }

    public List<Item> GetInventoryItems(ItemCategory? filter = null)
    {
        if (filter == null)
            return _sharedInventory.ToList();
        return _sharedInventory.Where(i => i.Category == filter.Value).ToList();
    }

    public void ClearInventory() => _sharedInventory.Clear();

    // --- Marks ---

    public bool HasMark(string markId) => Marks.Contains(markId);

    public void AddMark(string markId) => Marks.Add(markId);

    public bool SpendGold(int amount)
    {
        if (Gold < amount) return false;
        Gold -= amount;
        return true;
    }

    public void AddGold(int amount)
    {
        Gold += amount;
    }

    public void Rest()
    {
        // Heal the party over time
        foreach (var member in GetLivingMembers())
        {
            member.Heal(member.MaxHP / 10);
            member.RestoreMana(member.MaxMP / 10);

        }

        AdvanceTime(10);
    }
}
