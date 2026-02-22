using UltimaIII.Core.Enums;
using UltimaIII.Core.Models;

namespace UltimaIII.Core.Engine;

/// <summary>
/// Handles gem socketing logic for the Temple.
/// </summary>
public static class SocketingService
{
    /// <summary>
    /// Get all socketable equipment for a character (equipped items with MaxSockets > 0).
    /// </summary>
    public static List<Item> GetSocketableEquipment(Character character)
    {
        var items = new List<Item>();
        if (character.EquippedWeapon != null && character.EquippedWeapon.MaxSockets > 0)
            items.Add(character.EquippedWeapon);
        if (character.EquippedArmor != null && character.EquippedArmor.MaxSockets > 0)
            items.Add(character.EquippedArmor);
        if (character.EquippedShield != null && character.EquippedShield.MaxSockets > 0)
            items.Add(character.EquippedShield);
        return items;
    }

    /// <summary>
    /// Check if a gem can be socketed into an item based on GemSlotTarget compatibility.
    /// </summary>
    public static bool CanSocketGem(Item item, Gem gem)
    {
        return item switch
        {
            Weapon => gem.SlotTarget.HasFlag(GemSlotTarget.Weapon),
            Armor => gem.SlotTarget.HasFlag(GemSlotTarget.Armor),
            Shield => gem.SlotTarget.HasFlag(GemSlotTarget.Shield),
            _ => false
        };
    }

    /// <summary>
    /// Calculate the gold cost to socket a gem into equipment.
    /// </summary>
    public static int GetSocketingCost(Gem gem, Item equipment)
    {
        // Base cost by tier
        int baseCost = gem.Tier switch
        {
            GemTier.Chipped => 50,
            GemTier.Flawed => 150,
            GemTier.Perfect => 400,
            _ => 100
        };

        // Multiplier by equipment value
        int multiplier = equipment.Value switch
        {
            <= 100 => 1,
            <= 300 => 2,
            _ => 3
        };

        return baseCost * multiplier;
    }

    /// <summary>
    /// Socket a gem into equipment at the given socket index.
    /// Spends gold, removes gem from party inventory, replaces socket (old gem destroyed).
    /// </summary>
    /// <returns>True if successful, false if validation fails.</returns>
    public static bool SocketGem(Party party, Item equipment, int socketIndex, Gem gem)
    {
        // Validate
        if (!CanSocketGem(equipment, gem))
            return false;

        List<Gem?> sockets = equipment switch
        {
            Weapon w => w.Sockets,
            Armor a => a.Sockets,
            Shield s => s.Sockets,
            _ => null
        } ?? new List<Gem?>();

        if (socketIndex < 0 || socketIndex >= sockets.Count)
            return false;

        int cost = GetSocketingCost(gem, equipment);
        if (party.Gold < cost)
            return false;

        // Execute
        party.SpendGold(cost);
        party.RemoveFromInventory(gem);
        sockets[socketIndex] = gem; // Old gem destroyed (replaced)

        return true;
    }
}
