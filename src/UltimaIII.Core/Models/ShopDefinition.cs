namespace UltimaIII.Core.Models;

public enum ShopType
{
    WeaponShop,
    ArmorShop,
    Tavern,
    Healer,
    Guild,
    Inn,
    Temple
}

public class ShopDefinition
{
    public ShopType Type { get; init; }
    public string Name { get; init; } = string.Empty;
    public string WelcomeMessage { get; init; } = string.Empty;
    public bool HasBuyTab { get; init; }
    public bool HasSellTab { get; init; }
    public bool HasEquipTab { get; init; }
    public bool HasServicesTab { get; init; }
    public bool HasRecruitTab { get; init; }

    public List<Item> GetBuyInventory()
    {
        return Type switch
        {
            ShopType.WeaponShop => new List<Item>(ItemRegistry.GetAllWeapons()),
            ShopType.ArmorShop => CombineArmorAndShields(),
            ShopType.Guild => new List<Item>(ItemRegistry.GetAllConsumables()),
            _ => new List<Item>()
        };
    }

    private static List<Item> CombineArmorAndShields()
    {
        var items = new List<Item>();
        items.AddRange(ItemRegistry.GetAllArmor());
        items.AddRange(ItemRegistry.GetAllShields());
        return items;
    }

    public static readonly Dictionary<string, ShopType> EntityIdToShopType = new()
    {
        ["weapon_shop"] = ShopType.WeaponShop,
        ["armor_shop"] = ShopType.ArmorShop,
        ["tavern"] = ShopType.Tavern,
        ["healer"] = ShopType.Healer,
        ["guild"] = ShopType.Guild,
        ["inn"] = ShopType.Inn,
        ["temple"] = ShopType.Temple
    };

    public static readonly Dictionary<ShopType, ShopDefinition> AllShops = new()
    {
        [ShopType.WeaponShop] = new ShopDefinition
        {
            Type = ShopType.WeaponShop,
            Name = "Weapon Shop",
            WelcomeMessage = "Fine blades and bows! What catches thy eye?",
            HasBuyTab = true,
            HasSellTab = true,
            HasEquipTab = true
        },
        [ShopType.ArmorShop] = new ShopDefinition
        {
            Type = ShopType.ArmorShop,
            Name = "Armor Shop",
            WelcomeMessage = "Protection for the brave! Browse my wares.",
            HasBuyTab = true,
            HasSellTab = true,
            HasEquipTab = true
        },
        [ShopType.Tavern] = new ShopDefinition
        {
            Type = ShopType.Tavern,
            Name = "Tavern",
            WelcomeMessage = "Welcome, weary traveler! Food and drink await.",
            HasBuyTab = true,
            HasSellTab = true,
            HasRecruitTab = true
        },
        [ShopType.Healer] = new ShopDefinition
        {
            Type = ShopType.Healer,
            Name = "Healer",
            WelcomeMessage = "I can mend thy wounds and cure thy ailments.",
            HasServicesTab = true
        },
        [ShopType.Guild] = new ShopDefinition
        {
            Type = ShopType.Guild,
            Name = "Guild",
            WelcomeMessage = "Supplies for the adventurer. Choose wisely.",
            HasBuyTab = true
        },
        [ShopType.Inn] = new ShopDefinition
        {
            Type = ShopType.Inn,
            Name = "Inn",
            WelcomeMessage = "Rest thy weary bones. A warm bed awaits.",
            HasServicesTab = true
        },
        [ShopType.Temple] = new ShopDefinition
        {
            Type = ShopType.Temple,
            Name = "Temple",
            WelcomeMessage = "Welcome, seeker. Bring thy gems and I shall bind their power to thy equipment.",
            HasServicesTab = true
        }
    };

    public static ShopDefinition Get(ShopType type) => AllShops[type];
}
