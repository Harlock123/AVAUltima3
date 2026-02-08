using UltimaIII.Core.Enums;

namespace UltimaIII.Core.Models;

public static class ItemRegistry
{
    private static readonly List<Weapon> Weapons = new()
    {
        new Weapon { Id = "dagger", Name = "Dagger", Description = "A small blade", WeaponType = WeaponType.Dagger, MinDamage = 1, MaxDamage = 4, Value = 20 },
        new Weapon { Id = "sling", Name = "Sling", Description = "A simple ranged weapon", WeaponType = WeaponType.Sling, MinDamage = 1, MaxDamage = 6, Range = 3, Value = 30 },
        new Weapon { Id = "mace", Name = "Mace", Description = "A heavy bludgeon", WeaponType = WeaponType.Mace, MinDamage = 2, MaxDamage = 8, Value = 50 },
        new Weapon { Id = "short_sword", Name = "Short Sword", Description = "A reliable blade", WeaponType = WeaponType.Sword, MinDamage = 2, MaxDamage = 8, Value = 80 },
        new Weapon { Id = "axe", Name = "Axe", Description = "A sturdy chopping weapon", WeaponType = WeaponType.Axe, MinDamage = 3, MaxDamage = 10, Value = 100 },
        new Weapon { Id = "bow", Name = "Bow", Description = "A longbow for ranged combat", WeaponType = WeaponType.Bow, MinDamage = 2, MaxDamage = 10, Range = 4, IsTwoHanded = true, Value = 150 },
        new Weapon { Id = "long_sword", Name = "Long Sword", Description = "A fine blade", WeaponType = WeaponType.Sword, MinDamage = 3, MaxDamage = 12, Value = 200 },
        new Weapon { Id = "staff", Name = "Staff", Description = "A staff that channels magic", WeaponType = WeaponType.Staff, MinDamage = 1, MaxDamage = 6, HitBonus = 2, IsTwoHanded = true, Value = 250 },
        new Weapon { Id = "crossbow", Name = "Crossbow", Description = "A powerful ranged weapon", WeaponType = WeaponType.Crossbow, MinDamage = 3, MaxDamage = 12, Range = 5, IsTwoHanded = true, Value = 250 },
        new Weapon { Id = "halberd", Name = "Halberd", Description = "A polearm with long reach", WeaponType = WeaponType.Axe, MinDamage = 4, MaxDamage = 14, IsTwoHanded = true, Value = 300 },
        new Weapon { Id = "great_sword", Name = "Great Sword", Description = "A massive two-handed blade", WeaponType = WeaponType.GreatSword, MinDamage = 4, MaxDamage = 16, IsTwoHanded = true, Value = 350 },
        new Weapon { Id = "2h_axe", Name = "2H Axe", Description = "A devastating battle axe", WeaponType = WeaponType.Axe, MinDamage = 5, MaxDamage = 18, IsTwoHanded = true, Value = 400 },
        new Weapon { Id = "wand", Name = "Wand", Description = "A wand crackling with power", WeaponType = WeaponType.Exotic, MinDamage = 1, MaxDamage = 4, HitBonus = 3, Value = 500 },
    };

    private static readonly List<Armor> Armors = new()
    {
        new Armor { Id = "cloth", Name = "Cloth Armor", Description = "Simple cloth garments", ArmorType = ArmorType.Cloth, Defense = 1, Value = 30 },
        new Armor { Id = "leather", Name = "Leather Armor", Description = "Sturdy leather protection", ArmorType = ArmorType.Leather, Defense = 2, Value = 100 },
        new Armor { Id = "chain_mail", Name = "Chain Mail", Description = "Interlocking metal rings", ArmorType = ArmorType.Chain, Defense = 4, Value = 300 },
        new Armor { Id = "plate_mail", Name = "Plate Mail", Description = "Full plate armor", ArmorType = ArmorType.Plate, Defense = 6, Value = 800 },
    };

    private static readonly List<Shield> Shields = new()
    {
        new Shield { Id = "small_shield", Name = "Small Shield", Description = "A light buckler", ShieldType = ShieldType.SmallShield, Defense = 1, Value = 50 },
        new Shield { Id = "large_shield", Name = "Large Shield", Description = "A sturdy kite shield", ShieldType = ShieldType.LargeShield, Defense = 2, Value = 150 },
        new Shield { Id = "tower_shield", Name = "Tower Shield", Description = "A massive tower shield", ShieldType = ShieldType.LargeShield, Defense = 3, Value = 400 },
    };

    private static readonly List<Consumable> Consumables = new()
    {
        new Consumable { Id = "torch", Name = "Torch", Description = "Lights the darkness", Effect = "light", EffectStrength = 5, Value = 20 },
        new Consumable { Id = "healing_potion", Name = "Healing Potion", Description = "Restores 30 HP", Effect = "heal", EffectStrength = 30, Value = 75 },
        new Consumable { Id = "keys", Name = "Keys", Description = "Opens locked doors", Effect = "unlock", EffectStrength = 1, Value = 30 },
    };

    private static readonly Dictionary<string, Item> AllItems;

    static ItemRegistry()
    {
        AllItems = new Dictionary<string, Item>();
        // Register defaults
        AllItems[Weapon.Hands.Id] = Weapon.Hands;
        AllItems[Armor.None.Id] = Armor.None;
        AllItems[Shield.None.Id] = Shield.None;

        foreach (var w in Weapons) AllItems[w.Id] = w;
        foreach (var a in Armors) AllItems[a.Id] = a;
        foreach (var s in Shields) AllItems[s.Id] = s;
        foreach (var c in Consumables) AllItems[c.Id] = c;
    }

    public static Item? FindById(string id)
    {
        // Handle backward compat for old save files
        if (id == "none") return null; // Ambiguous old ID, caller should handle context
        return AllItems.GetValueOrDefault(id);
    }

    public static List<Weapon> GetAllWeapons() => new(Weapons);
    public static List<Armor> GetAllArmor() => new(Armors);
    public static List<Shield> GetAllShields() => new(Shields);
    public static List<Consumable> GetAllConsumables() => new(Consumables);
}
