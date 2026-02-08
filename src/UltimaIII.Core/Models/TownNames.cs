namespace UltimaIII.Core.Models;

public static class TownNames
{
    private static readonly Dictionary<string, List<string>> NamePools = new()
    {
        ["weapon_shop"] = new()
        {
            "The Rusty Blade", "Steel & Iron", "Warrior's Edge", "The Keen Edge",
            "Swords of Valor", "The Iron Fang", "Blademaster's Shop"
        },
        ["armor_shop"] = new()
        {
            "The Iron Shell", "Shields & Mail", "Forgeborn Armory", "The Plated Ox",
            "Guardian's Keep", "The Bronze Anvil"
        },
        ["tavern"] = new()
        {
            "The Prancing Pony", "The Drunken Dragon", "The Golden Tankard",
            "The Foaming Mug", "The Wanderer's Rest", "The Merry Bard"
        },
        ["healer"] = new()
        {
            "Temple of Light", "The Mending Hand", "Sister's Grace",
            "The White Lotus", "Sanctuary of Hope", "The Healing Touch"
        },
        ["guild"] = new()
        {
            "The Shadow Market", "Adventurer's Cache", "The Supply Vault",
            "The Rogue's Stash", "Dungeon Outfitters", "The Ready Pack"
        },
        ["inn"] = new()
        {
            "The Weary Traveler", "Moonlight Rest", "Hearthstone Inn",
            "The Cozy Hearth", "Pilgrim's Lodge", "The Soft Pillow"
        }
    };

    public static string GetRandomName(string shopType, Random rng)
    {
        if (NamePools.TryGetValue(shopType, out var names) && names.Count > 0)
            return names[rng.Next(names.Count)];
        return shopType;
    }
}
