using UltimaIII.Core.Enums;

namespace UltimaIII.Core.Models;

public static class ItemRegistry
{
    private static readonly List<Weapon> Weapons = new()
    {
        new Weapon { Id = "dagger", Name = "Dagger", Description = "A small blade", WeaponType = WeaponType.Dagger, MinDamage = 1, MaxDamage = 4, Value = 20 },
        new Weapon { Id = "sling", Name = "Sling", Description = "A simple ranged weapon", WeaponType = WeaponType.Sling, MinDamage = 1, MaxDamage = 6, Range = 3, Value = 30 },
        new Weapon { Id = "mace", Name = "Mace", Description = "A heavy bludgeon", WeaponType = WeaponType.Mace, MinDamage = 2, MaxDamage = 8, Value = 50 },
        new Weapon { Id = "short_sword", Name = "Short Sword", Description = "A reliable blade", WeaponType = WeaponType.Sword, MinDamage = 2, MaxDamage = 8, Value = 80, MaxSockets = 1 },
        new Weapon { Id = "axe", Name = "Axe", Description = "A sturdy chopping weapon", WeaponType = WeaponType.Axe, MinDamage = 3, MaxDamage = 10, Value = 100, MaxSockets = 1 },
        new Weapon { Id = "bow", Name = "Bow", Description = "A longbow for ranged combat", WeaponType = WeaponType.Bow, MinDamage = 2, MaxDamage = 10, Range = 4, IsTwoHanded = true, Value = 150, MaxSockets = 1 },
        new Weapon { Id = "long_sword", Name = "Long Sword", Description = "A fine blade", WeaponType = WeaponType.Sword, MinDamage = 3, MaxDamage = 12, Value = 200, MaxSockets = 1 },
        new Weapon { Id = "staff", Name = "Staff", Description = "A staff that channels magic", WeaponType = WeaponType.Staff, MinDamage = 1, MaxDamage = 6, HitBonus = 2, IsTwoHanded = true, Value = 250, MaxSockets = 1 },
        new Weapon { Id = "crossbow", Name = "Crossbow", Description = "A powerful ranged weapon", WeaponType = WeaponType.Crossbow, MinDamage = 3, MaxDamage = 12, Range = 5, IsTwoHanded = true, Value = 250, MaxSockets = 1 },
        new Weapon { Id = "halberd", Name = "Halberd", Description = "A polearm with long reach", WeaponType = WeaponType.Axe, MinDamage = 4, MaxDamage = 14, IsTwoHanded = true, Value = 300, MaxSockets = 2 },
        new Weapon { Id = "great_sword", Name = "Great Sword", Description = "A massive two-handed blade", WeaponType = WeaponType.GreatSword, MinDamage = 4, MaxDamage = 16, IsTwoHanded = true, Value = 350, MaxSockets = 2 },
        new Weapon { Id = "2h_axe", Name = "2H Axe", Description = "A devastating battle axe", WeaponType = WeaponType.Axe, MinDamage = 5, MaxDamage = 18, IsTwoHanded = true, Value = 400, MaxSockets = 2 },
        new Weapon { Id = "wand", Name = "Wand", Description = "A wand crackling with power", WeaponType = WeaponType.Exotic, MinDamage = 1, MaxDamage = 4, HitBonus = 3, Value = 500, MaxSockets = 2 },
    };

    private static readonly List<Armor> Armors = new()
    {
        new Armor { Id = "cloth", Name = "Cloth Armor", Description = "Simple cloth garments", ArmorType = ArmorType.Cloth, Defense = 1, Value = 30 },
        new Armor { Id = "leather", Name = "Leather Armor", Description = "Sturdy leather protection", ArmorType = ArmorType.Leather, Defense = 2, Value = 100, MaxSockets = 1 },
        new Armor { Id = "chain_mail", Name = "Chain Mail", Description = "Interlocking metal rings", ArmorType = ArmorType.Chain, Defense = 4, Value = 300, MaxSockets = 1 },
        new Armor { Id = "plate_mail", Name = "Plate Mail", Description = "Full plate armor", ArmorType = ArmorType.Plate, Defense = 6, Value = 800, MaxSockets = 2 },
    };

    private static readonly List<Shield> Shields = new()
    {
        new Shield { Id = "small_shield", Name = "Small Shield", Description = "A light buckler", ShieldType = ShieldType.SmallShield, Defense = 1, Value = 50 },
        new Shield { Id = "large_shield", Name = "Large Shield", Description = "A sturdy kite shield", ShieldType = ShieldType.LargeShield, Defense = 2, Value = 150, MaxSockets = 1 },
        new Shield { Id = "tower_shield", Name = "Tower Shield", Description = "A massive tower shield", ShieldType = ShieldType.LargeShield, Defense = 3, Value = 400, MaxSockets = 1 },
    };

    private static readonly List<Consumable> Consumables = new()
    {
        new Consumable { Id = "torch", Name = "Torch", Description = "Lights the darkness", Effect = "light", EffectStrength = 5, Value = 20 },
        new Consumable { Id = "healing_potion", Name = "Healing Potion", Description = "Restores 30 HP", Effect = "heal", EffectStrength = 30, Value = 75 },
        new Consumable { Id = "keys", Name = "Keys", Description = "Opens locked doors", Effect = "unlock", EffectStrength = 1, Value = 30 },
    };

    private static readonly List<QuestItem> QuestItems = new()
    {
        new QuestItem { Id = "quest_old_ring", Name = "Old Ring", Description = "A tarnished ring found on the undead", QuestId = "fetch_old_ring", Value = 0 },
        new QuestItem { Id = "quest_moonstone", Name = "Moonstone", Description = "A glowing stone pulsing with lunar energy", QuestId = "fetch_moonstone", Value = 0 },
        new QuestItem { Id = "quest_serpent_fang", Name = "Serpent Fang", Description = "A venomous fang from a giant beast", QuestId = "fetch_serpent_fang", Value = 0 },
        new QuestItem { Id = "quest_ancient_amulet", Name = "Ancient Amulet", Description = "An amulet of forgotten power", QuestId = "fetch_ancient_amulet", Value = 0 },
        new QuestItem { Id = "quest_lich_phylactery", Name = "Lich Phylactery", Description = "The vessel binding a lich to unlife", QuestId = "fetch_lich_phylactery", Value = 0 },
        new QuestItem { Id = "quest_dragon_scale", Name = "Dragon Scale", Description = "A shimmering scale from a great wyrm", QuestId = "fetch_dragon_scale", Value = 0 },
        new QuestItem { Id = "quest_fire_crystal", Name = "Fire Crystal", Description = "A crystal blazing with elemental fire", QuestId = "fetch_fire_crystal", Value = 0 },
        new QuestItem { Id = "quest_time_shard", Name = "Time Shard", Description = "A fragment of crystallized time", QuestId = "fetch_time_shard", Value = 0 },
    };

    private static readonly List<Gem> Gems = new()
    {
        // Ruby — Weapon: bonus damage
        new Gem { Id = "gem_ruby_chipped", Name = "Chipped Ruby", Description = "+1 damage", GemType = GemType.Ruby, Tier = GemTier.Chipped, SlotTarget = GemSlotTarget.Weapon, BonusValue = 1 },
        new Gem { Id = "gem_ruby_flawed", Name = "Flawed Ruby", Description = "+2 damage", GemType = GemType.Ruby, Tier = GemTier.Flawed, SlotTarget = GemSlotTarget.Weapon, BonusValue = 2 },
        new Gem { Id = "gem_ruby_perfect", Name = "Perfect Ruby", Description = "+4 damage", GemType = GemType.Ruby, Tier = GemTier.Perfect, SlotTarget = GemSlotTarget.Weapon, BonusValue = 4 },
        // Sapphire — Weapon: bonus hit chance
        new Gem { Id = "gem_sapphire_chipped", Name = "Chipped Sapphire", Description = "+1 hit bonus", GemType = GemType.Sapphire, Tier = GemTier.Chipped, SlotTarget = GemSlotTarget.Weapon, BonusValue = 1 },
        new Gem { Id = "gem_sapphire_flawed", Name = "Flawed Sapphire", Description = "+2 hit bonus", GemType = GemType.Sapphire, Tier = GemTier.Flawed, SlotTarget = GemSlotTarget.Weapon, BonusValue = 2 },
        new Gem { Id = "gem_sapphire_perfect", Name = "Perfect Sapphire", Description = "+3 hit bonus", GemType = GemType.Sapphire, Tier = GemTier.Perfect, SlotTarget = GemSlotTarget.Weapon, BonusValue = 3 },
        // Emerald — Armor/Shield: bonus max HP
        new Gem { Id = "gem_emerald_chipped", Name = "Chipped Emerald", Description = "+3 max HP", GemType = GemType.Emerald, Tier = GemTier.Chipped, SlotTarget = GemSlotTarget.Armor | GemSlotTarget.Shield, BonusValue = 3 },
        new Gem { Id = "gem_emerald_flawed", Name = "Flawed Emerald", Description = "+6 max HP", GemType = GemType.Emerald, Tier = GemTier.Flawed, SlotTarget = GemSlotTarget.Armor | GemSlotTarget.Shield, BonusValue = 6 },
        new Gem { Id = "gem_emerald_perfect", Name = "Perfect Emerald", Description = "+10 max HP", GemType = GemType.Emerald, Tier = GemTier.Perfect, SlotTarget = GemSlotTarget.Armor | GemSlotTarget.Shield, BonusValue = 10 },
        // Diamond — Weapon: crit % / Armor: defense
        new Gem { Id = "gem_diamond_chipped", Name = "Chipped Diamond", Description = "+3% crit / +1 defense", GemType = GemType.Diamond, Tier = GemTier.Chipped, SlotTarget = GemSlotTarget.Weapon | GemSlotTarget.Armor, BonusValue = 1, BonusPercent = 3 },
        new Gem { Id = "gem_diamond_flawed", Name = "Flawed Diamond", Description = "+5% crit / +2 defense", GemType = GemType.Diamond, Tier = GemTier.Flawed, SlotTarget = GemSlotTarget.Weapon | GemSlotTarget.Armor, BonusValue = 2, BonusPercent = 5 },
        new Gem { Id = "gem_diamond_perfect", Name = "Perfect Diamond", Description = "+8% crit / +3 defense", GemType = GemType.Diamond, Tier = GemTier.Perfect, SlotTarget = GemSlotTarget.Weapon | GemSlotTarget.Armor, BonusValue = 3, BonusPercent = 8 },
        // Topaz — Weapon: undead damage / Armor: magic defense
        new Gem { Id = "gem_topaz_chipped", Name = "Chipped Topaz", Description = "+2 vs undead / +1 magic def", GemType = GemType.Topaz, Tier = GemTier.Chipped, SlotTarget = GemSlotTarget.Weapon | GemSlotTarget.Armor, BonusValue = 1, BonusPercent = 2 },
        new Gem { Id = "gem_topaz_flawed", Name = "Flawed Topaz", Description = "+3 vs undead / +2 magic def", GemType = GemType.Topaz, Tier = GemTier.Flawed, SlotTarget = GemSlotTarget.Weapon | GemSlotTarget.Armor, BonusValue = 2, BonusPercent = 3 },
        new Gem { Id = "gem_topaz_perfect", Name = "Perfect Topaz", Description = "+5 vs undead / +3 magic def", GemType = GemType.Topaz, Tier = GemTier.Perfect, SlotTarget = GemSlotTarget.Weapon | GemSlotTarget.Armor, BonusValue = 3, BonusPercent = 5 },
        // Amethyst — Armor/Shield: status resistance %
        new Gem { Id = "gem_amethyst_chipped", Name = "Chipped Amethyst", Description = "10% status resistance", GemType = GemType.Amethyst, Tier = GemTier.Chipped, SlotTarget = GemSlotTarget.Armor | GemSlotTarget.Shield, BonusPercent = 10 },
        new Gem { Id = "gem_amethyst_flawed", Name = "Flawed Amethyst", Description = "20% status resistance", GemType = GemType.Amethyst, Tier = GemTier.Flawed, SlotTarget = GemSlotTarget.Armor | GemSlotTarget.Shield, BonusPercent = 20 },
        new Gem { Id = "gem_amethyst_perfect", Name = "Perfect Amethyst", Description = "35% status resistance", GemType = GemType.Amethyst, Tier = GemTier.Perfect, SlotTarget = GemSlotTarget.Armor | GemSlotTarget.Shield, BonusPercent = 35 },
        // Onyx — Weapon: lifesteal %
        new Gem { Id = "gem_onyx_chipped", Name = "Chipped Onyx", Description = "5% lifesteal", GemType = GemType.Onyx, Tier = GemTier.Chipped, SlotTarget = GemSlotTarget.Weapon, BonusPercent = 5 },
        new Gem { Id = "gem_onyx_flawed", Name = "Flawed Onyx", Description = "10% lifesteal", GemType = GemType.Onyx, Tier = GemTier.Flawed, SlotTarget = GemSlotTarget.Weapon, BonusPercent = 10 },
        new Gem { Id = "gem_onyx_perfect", Name = "Perfect Onyx", Description = "15% lifesteal", GemType = GemType.Onyx, Tier = GemTier.Perfect, SlotTarget = GemSlotTarget.Weapon, BonusPercent = 15 },
        // Opal — Shield: damage reflect
        new Gem { Id = "gem_opal_chipped", Name = "Chipped Opal", Description = "Reflect 1 damage", GemType = GemType.Opal, Tier = GemTier.Chipped, SlotTarget = GemSlotTarget.Shield, BonusValue = 1 },
        new Gem { Id = "gem_opal_flawed", Name = "Flawed Opal", Description = "Reflect 2 damage", GemType = GemType.Opal, Tier = GemTier.Flawed, SlotTarget = GemSlotTarget.Shield, BonusValue = 2 },
        new Gem { Id = "gem_opal_perfect", Name = "Perfect Opal", Description = "Reflect 4 damage", GemType = GemType.Opal, Tier = GemTier.Perfect, SlotTarget = GemSlotTarget.Shield, BonusValue = 4 },
    };

    private static readonly Dictionary<string, Item> AllItems;

    static ItemRegistry()
    {
        AllItems = new Dictionary<string, Item>();
        // Register defaults
        AllItems[Weapon.Hands.Id] = Weapon.Hands;
        AllItems[Armor.None.Id] = Armor.None;
        AllItems[Shield.None.Id] = Shield.None;

        foreach (var w in Weapons) { InitSockets(w.Sockets, w.MaxSockets); AllItems[w.Id] = w; }
        foreach (var a in Armors) { InitSockets(a.Sockets, a.MaxSockets); AllItems[a.Id] = a; }
        foreach (var s in Shields) { InitSockets(s.Sockets, s.MaxSockets); AllItems[s.Id] = s; }
        foreach (var c in Consumables) AllItems[c.Id] = c;
        foreach (var q in QuestItems) AllItems[q.Id] = q;
        foreach (var g in Gems) AllItems[g.Id] = g;
    }

    private static void InitSockets(List<Gem?> sockets, int maxSockets)
    {
        sockets.Clear();
        for (int i = 0; i < maxSockets; i++)
            sockets.Add(null);
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

    public static List<Gem> GetAllGems() => new(Gems);

    public static Item CloneItem(Item original)
    {
        return original switch
        {
            Weapon w => CloneWeapon(w),
            Armor a => CloneArmor(a),
            Shield s => CloneShield(s),
            Gem g => new Gem
            {
                Id = g.Id, Name = g.Name, Description = g.Description, Value = g.Value, Weight = g.Weight,
                IsStackable = g.IsStackable, Quantity = 1,
                GemType = g.GemType, Tier = g.Tier, SlotTarget = g.SlotTarget,
                BonusValue = g.BonusValue, BonusPercent = g.BonusPercent
            },
            Consumable c => new Consumable
            {
                Id = c.Id, Name = c.Name, Description = c.Description, Value = c.Value, Weight = c.Weight,
                Quantity = 1,
                Effect = c.Effect, EffectStrength = c.EffectStrength
            },
            QuestItem q => new QuestItem
            {
                Id = q.Id, Name = q.Name, Description = q.Description, Value = q.Value, Weight = q.Weight,
                IsStackable = q.IsStackable, Quantity = 1,
                QuestId = q.QuestId
            },
            _ => throw new ArgumentException($"Unknown item type: {original.GetType().Name}")
        };
    }

    private static Weapon CloneWeapon(Weapon w)
    {
        var clone = new Weapon
        {
            Id = w.Id, Name = w.Name, Description = w.Description, Value = w.Value, Weight = w.Weight,
            IsStackable = w.IsStackable, Quantity = 1,
            WeaponType = w.WeaponType, MinDamage = w.MinDamage, MaxDamage = w.MaxDamage,
            Range = w.Range, HitBonus = w.HitBonus, IsTwoHanded = w.IsTwoHanded,
            MaxSockets = w.MaxSockets
        };
        CloneSockets(clone.Sockets, w.Sockets, w.MaxSockets);
        return clone;
    }

    private static Armor CloneArmor(Armor a)
    {
        var clone = new Armor
        {
            Id = a.Id, Name = a.Name, Description = a.Description, Value = a.Value, Weight = a.Weight,
            IsStackable = a.IsStackable, Quantity = 1,
            ArmorType = a.ArmorType, Defense = a.Defense, MagicDefense = a.MagicDefense,
            MaxSockets = a.MaxSockets
        };
        CloneSockets(clone.Sockets, a.Sockets, a.MaxSockets);
        return clone;
    }

    private static Shield CloneShield(Shield s)
    {
        var clone = new Shield
        {
            Id = s.Id, Name = s.Name, Description = s.Description, Value = s.Value, Weight = s.Weight,
            IsStackable = s.IsStackable, Quantity = 1,
            ShieldType = s.ShieldType, Defense = s.Defense,
            MaxSockets = s.MaxSockets
        };
        CloneSockets(clone.Sockets, s.Sockets, s.MaxSockets);
        return clone;
    }

    private static void CloneSockets(List<Gem?> target, List<Gem?> source, int maxSockets)
    {
        target.Clear();
        for (int i = 0; i < maxSockets; i++)
            target.Add(i < source.Count ? source[i] : null);
    }
}
