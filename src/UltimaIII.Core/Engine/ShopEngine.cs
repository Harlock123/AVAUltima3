using UltimaIII.Core.Enums;
using UltimaIII.Core.Models;

namespace UltimaIII.Core.Engine;

public static class ShopEngine
{
    public static string BuyItem(Party party, Character character, Item item)
    {
        if (party.Gold < item.Value)
            return "Not enough gold!";

        // Check class restrictions for equipment
        var classDef = character.ClassDef;
        if (item is Weapon weapon && !classDef.CanUseWeapon(weapon.WeaponType))
            return $"{character.Name} cannot use {item.Name}!";
        if (item is Armor armor && !classDef.CanUseArmor(armor.ArmorType))
            return $"{character.Name} cannot use {item.Name}!";
        if (item is Shield shield && !classDef.CanUseShield(shield.ShieldType))
            return $"{character.Name} cannot use {item.Name}!";

        party.SpendGold(item.Value);
        character.Inventory.Add(item);
        return $"{character.Name} bought {item.Name} for {item.Value}g.";
    }

    public static string SellItem(Party party, Character character, Item item)
    {
        int sellPrice = item.Value / 2;
        if (sellPrice <= 0)
            return "That item has no value.";

        // Unequip if currently equipped
        if (character.EquippedWeapon == item)
            character.EquippedWeapon = Weapon.Hands;
        if (character.EquippedArmor == item)
            character.EquippedArmor = Armor.None;
        if (character.EquippedShield == item)
            character.EquippedShield = Shield.None;

        character.Inventory.Remove(item);
        party.AddGold(sellPrice);
        return $"Sold {item.Name} for {sellPrice}g.";
    }

    public static string EquipItem(Character character, Item item)
    {
        var classDef = character.ClassDef;

        if (item is Weapon weapon)
        {
            if (!classDef.CanUseWeapon(weapon.WeaponType))
                return $"{character.Name} cannot wield {item.Name}!";

            // Unequip current weapon to inventory if it's not Hands
            if (character.EquippedWeapon != null && character.EquippedWeapon != Weapon.Hands)
                character.Inventory.Add(character.EquippedWeapon);

            character.EquippedWeapon = weapon;
            character.Inventory.Remove(item);

            // Two-handed weapon: unequip shield
            if (weapon.IsTwoHanded && character.EquippedShield != null && character.EquippedShield != Shield.None)
            {
                character.Inventory.Add(character.EquippedShield);
                character.EquippedShield = Shield.None;
                return $"{character.Name} equips {item.Name} (shield removed).";
            }

            return $"{character.Name} equips {item.Name}.";
        }

        if (item is Armor armor)
        {
            if (!classDef.CanUseArmor(armor.ArmorType))
                return $"{character.Name} cannot wear {item.Name}!";

            if (character.EquippedArmor != null && character.EquippedArmor != Armor.None)
                character.Inventory.Add(character.EquippedArmor);

            character.EquippedArmor = armor;
            character.Inventory.Remove(item);
            return $"{character.Name} equips {item.Name}.";
        }

        if (item is Shield shield)
        {
            if (!classDef.CanUseShield(shield.ShieldType))
                return $"{character.Name} cannot use {item.Name}!";

            // Can't equip shield with two-handed weapon
            if (character.EquippedWeapon is { IsTwoHanded: true })
                return $"Cannot equip a shield with a two-handed weapon!";

            if (character.EquippedShield != null && character.EquippedShield != Shield.None)
                character.Inventory.Add(character.EquippedShield);

            character.EquippedShield = shield;
            character.Inventory.Remove(item);
            return $"{character.Name} equips {item.Name}.";
        }

        return "Cannot equip that item.";
    }

    public static string HealCharacter(Party party, Character character)
    {
        if (!character.IsAlive)
            return $"{character.Name} is dead! Seek resurrection.";

        if (character.CurrentHP >= character.MaxHP)
            return $"{character.Name} is already at full health.";

        int missingHp = character.MaxHP - character.CurrentHP;
        int cost = missingHp * 3;

        if (party.Gold < cost)
        {
            // Heal what we can afford
            int affordable = party.Gold / 3;
            if (affordable <= 0)
                return "Not enough gold!";

            int partialCost = affordable * 3;
            party.SpendGold(partialCost);
            character.Heal(affordable);
            return $"{character.Name} healed {affordable} HP for {partialCost}g.";
        }

        party.SpendGold(cost);
        character.Heal(missingHp);
        return $"{character.Name} fully healed for {cost}g.";
    }

    public static string CureStatus(Party party, Character character)
    {
        if (!character.IsAlive)
            return $"{character.Name} is dead!";

        if (character.Status == StatusEffect.None)
            return $"{character.Name} has no ailments.";

        if (party.Gold < 50)
            return "Not enough gold! (50g needed)";

        party.SpendGold(50);
        character.Status = StatusEffect.None;
        return $"{character.Name} cured of all ailments for 50g.";
    }

    public static string Resurrect(Party party, Character character)
    {
        if (character.IsAlive)
            return $"{character.Name} is not dead!";

        if (party.Gold < 200)
            return "Not enough gold! (200g needed)";

        party.SpendGold(200);
        character.Status = StatusEffect.None;
        character.CurrentHP = 1;
        return $"{character.Name} has been resurrected for 200g!";
    }

    public static string BuyFood(Party party, int portions)
    {
        int cost = portions * 5;
        if (party.Gold < cost)
            return "Not enough gold!";

        party.SpendGold(cost);
        party.Food += portions * 25;
        return $"Bought {portions * 25} food for {cost}g.";
    }

    public static string RestAtInn(Party party)
    {
        int livingCount = party.GetLivingMembers().Count();
        int cost = 50 + (25 * livingCount);

        if (party.Gold < cost)
            return $"Not enough gold! ({cost}g needed)";

        party.SpendGold(cost);
        foreach (var member in party.GetLivingMembers())
        {
            member.CurrentHP = member.MaxHP;
            member.CurrentMP = member.MaxMP;
        }

        return $"The party rests well. All HP and MP restored for {cost}g.";
    }

    public static string EquipFromParty(Party party, Character character, Item item)
    {
        var classDef = character.ClassDef;

        if (item is Weapon weapon)
        {
            if (!classDef.CanUseWeapon(weapon.WeaponType))
                return $"{character.Name} cannot wield {item.Name}!";

            party.RemoveFromInventory(item);

            if (character.EquippedWeapon != null && character.EquippedWeapon != Weapon.Hands)
                party.AddToInventory(ItemRegistry.CloneItem(character.EquippedWeapon));

            character.EquippedWeapon = weapon;

            if (weapon.IsTwoHanded && character.EquippedShield != null && character.EquippedShield != Shield.None)
            {
                party.AddToInventory(ItemRegistry.CloneItem(character.EquippedShield));
                character.EquippedShield = Shield.None;
                return $"{character.Name} equips {item.Name} (shield removed).";
            }

            return $"{character.Name} equips {item.Name}.";
        }

        if (item is Armor armor)
        {
            if (!classDef.CanUseArmor(armor.ArmorType))
                return $"{character.Name} cannot wear {item.Name}!";

            party.RemoveFromInventory(item);

            if (character.EquippedArmor != null && character.EquippedArmor != Armor.None)
                party.AddToInventory(ItemRegistry.CloneItem(character.EquippedArmor));

            character.EquippedArmor = armor;
            return $"{character.Name} equips {item.Name}.";
        }

        if (item is Shield shield)
        {
            if (!classDef.CanUseShield(shield.ShieldType))
                return $"{character.Name} cannot use {item.Name}!";

            if (character.EquippedWeapon is { IsTwoHanded: true })
                return $"Cannot equip a shield with a two-handed weapon!";

            party.RemoveFromInventory(item);

            if (character.EquippedShield != null && character.EquippedShield != Shield.None)
                party.AddToInventory(ItemRegistry.CloneItem(character.EquippedShield));

            character.EquippedShield = shield;
            return $"{character.Name} equips {item.Name}.";
        }

        return "Cannot equip that item.";
    }

    public static string SellPartyItem(Party party, Item item)
    {
        int sellPrice = item.Value / 2;
        if (sellPrice <= 0)
            return "That item has no value.";

        party.RemoveFromInventory(item);
        party.AddGold(sellPrice);
        return $"Sold {item.Name} for {sellPrice}g.";
    }

    public static bool CanCharacterUse(Character character, Item item)
    {
        var classDef = character.ClassDef;
        return item switch
        {
            Weapon w => classDef.CanUseWeapon(w.WeaponType),
            Armor a => classDef.CanUseArmor(a.ArmorType),
            Shield s => classDef.CanUseShield(s.ShieldType),
            _ => true
        };
    }
}
