namespace UltimaIII.Core.Models;

public record FetchDropInfo(string QuestId, string ItemId, string[] DropMonsterIds, int DropChancePercent);

public static class QuestRegistry
{
    private static readonly Dictionary<string, QuestDefinition> AllQuests;
    private static readonly List<FetchDropInfo> FetchDrops;

    // Undead monster IDs for category-based kill quests
    private static readonly HashSet<string> UndeadMonsterIds = new()
    {
        "skeleton", "zombie", "ghoul", "wraith", "vampire", "lich"
    };

    static QuestRegistry()
    {
        AllQuests = new Dictionary<string, QuestDefinition>();
        FetchDrops = new List<FetchDropInfo>();

        RegisterAllQuests();
        RegisterFetchDrops();
    }

    private static void RegisterAllQuests()
    {
        // === Early Game (Levels 1-3) ===

        Register(new QuestDefinition
        {
            Id = "kill_rats_britain", Name = "Rat Problem", Type = QuestType.Kill,
            Description = "The tavern has a rat problem spilling into the streets.",
            GiverTownId = "britain", GiverNpcName = "Barkeep Martha", GiverType = QuestGiverType.TavernNpc,
            OfferText = "Rats are overrunning the cellars! Kill 5 Giant Rats and I'll make it worth your while.",
            ProgressText = "Still got rats down there. Keep at it!",
            CompleteText = "The cellar is clear! Here's your reward, adventurer.",
            Objective = new QuestObjective { TargetMonsterIds = new[] { "giant_rat" }, TargetCount = 5 },
            Reward = new QuestReward { Gold = 50, Experience = 25 },
            MinPartyLevel = 1
        });

        Register(new QuestDefinition
        {
            Id = "kill_goblins_britain", Name = "Goblin Menace", Type = QuestType.Kill,
            Description = "Goblins are raiding travelers on the roads near Britain.",
            GiverTownId = "britain", GiverNpcName = "Captain Alaric", GiverType = QuestGiverType.TownNpc,
            OfferText = "Goblins plague our roads! Slay 8 of the wretches and the Crown will reward you.",
            ProgressText = "The goblin menace persists. Press on, warrior.",
            CompleteText = "The roads are safer thanks to you. The Crown is grateful!",
            Objective = new QuestObjective { TargetMonsterIds = new[] { "goblin" }, TargetCount = 8 },
            Reward = new QuestReward { Gold = 100, Experience = 50 },
            MinPartyLevel = 1
        });

        Register(new QuestDefinition
        {
            Id = "kill_spiders_yew", Name = "Web of Terror", Type = QuestType.Kill,
            Description = "Giant spiders are infesting the forest near Yew.",
            GiverTownId = "yew", GiverNpcName = "Herbalist Thorne", GiverType = QuestGiverType.TownNpc,
            OfferText = "The forest crawls with giant spiders! I need 6 slain so I can gather herbs safely.",
            ProgressText = "Still too many webs in the forest. Keep hunting!",
            CompleteText = "The webs are clearing! Thank you, brave ones. Take these herbs and coin.",
            Objective = new QuestObjective { TargetMonsterIds = new[] { "giant_spider" }, TargetCount = 6 },
            Reward = new QuestReward { Gold = 75, Experience = 40 },
            MinPartyLevel = 1
        });

        Register(new QuestDefinition
        {
            Id = "fetch_old_ring", Name = "The Lost Ring", Type = QuestType.Fetch,
            Description = "Elder Mira lost her husband's ring to the undead.",
            GiverTownId = "yew", GiverNpcName = "Elder Mira", GiverType = QuestGiverType.TavernNpc,
            OfferText = "My husband's ring was taken when the dead rose. Please, find it among the skeletons and zombies.",
            ProgressText = "Have you found the ring yet? Please keep searching...",
            CompleteText = "My ring! Oh thank you! My husband's memory lives on. Please, take this reward.",
            Objective = new QuestObjective { RequiredItemId = "quest_old_ring" },
            Reward = new QuestReward { Gold = 150, Experience = 60 },
            MinPartyLevel = 1
        });

        Register(new QuestDefinition
        {
            Id = "kill_orcs_montor", Name = "Orc Raiders", Type = QuestType.Kill,
            Description = "Orc warbands threaten the outskirts of Montor.",
            GiverTownId = "montor", GiverNpcName = "Commander Steele", GiverType = QuestGiverType.TownNpc,
            OfferText = "Orc raiders grow bold! Slay 10 of them and Montor will stand in your debt.",
            ProgressText = "The orcs still raid our farms. Don't stop now!",
            CompleteText = "The orc threat has weakened! Montor honors your service.",
            Objective = new QuestObjective { TargetMonsterIds = new[] { "orc" }, TargetCount = 10 },
            Reward = new QuestReward { Gold = 150, Experience = 75 },
            MinPartyLevel = 2
        });

        Register(new QuestDefinition
        {
            Id = "explore_doom1", Name = "Scout the Depths", Type = QuestType.Explore,
            Description = "Sergeant Bren needs reconnaissance of the Dungeon of Doom.",
            GiverTownId = "montor", GiverNpcName = "Sergeant Bren", GiverType = QuestGiverType.TavernNpc,
            OfferText = "We need someone to scout the first level of the Dungeon of Doom. Can you survive it?",
            ProgressText = "You haven't been to the dungeon yet? Get moving, soldier!",
            CompleteText = "Excellent reconnaissance! Your report will save lives. Here's your pay.",
            Objective = new QuestObjective { TargetMapId = "dungeon_doom_l1" },
            Reward = new QuestReward { Gold = 100, Experience = 50 },
            MinPartyLevel = 2
        });

        // === Mid Game (Levels 3-6) ===

        Register(new QuestDefinition
        {
            Id = "fetch_moonstone", Name = "The Lost Moonstone", Type = QuestType.Fetch,
            Description = "A moonstone of great power was lost to the wraiths.",
            GiverTownId = "moon", GiverNpcName = "Sage Lunara", GiverType = QuestGiverType.TownNpc,
            OfferText = "A moonstone was stolen by wraiths. It pulses with lunar energy. Retrieve it for me.",
            ProgressText = "The moonstone must be recovered. The wraiths hoard it still.",
            CompleteText = "The moonstone! Its power is vital to my research. You have my deepest thanks.",
            Objective = new QuestObjective { RequiredItemId = "quest_moonstone" },
            Reward = new QuestReward { Gold = 200, Experience = 100 },
            MinPartyLevel = 3
        });

        Register(new QuestDefinition
        {
            Id = "kill_undead_moon", Name = "Undead Purge", Type = QuestType.Kill,
            Description = "The undead threaten Moon's sacred grounds.",
            GiverTownId = "moon", GiverNpcName = "Priestess Ayla", GiverType = QuestGiverType.TavernNpc,
            OfferText = "The undead desecrate our holy places! Destroy 12 of any undead kind.",
            ProgressText = "The dead still walk. Continue your holy work!",
            CompleteText = "The sanctity of our grounds is restored. Blessings upon you!",
            Objective = new QuestObjective { TargetMonsterIds = new[] { "skeleton", "zombie", "ghoul", "wraith", "vampire", "lich" }, TargetCount = 12 },
            Reward = new QuestReward { Gold = 250, Experience = 120, ItemId = "gem_topaz_flawed" },
            MinPartyLevel = 3
        });

        Register(new QuestDefinition
        {
            Id = "fetch_serpent_fang", Name = "Serpent's Bane", Type = QuestType.Fetch,
            Description = "Alchemist Voss needs a giant spider's fang for an antivenom.",
            GiverTownId = "grey", GiverNpcName = "Alchemist Voss", GiverType = QuestGiverType.TownNpc,
            OfferText = "I need a Serpent Fang from the giant spiders. Their venom is key to my antivenom research.",
            ProgressText = "No fang yet? The spiders carry them. Keep hunting!",
            CompleteText = "A perfect specimen! This antivenom will save many lives. Here, take this.",
            Objective = new QuestObjective { RequiredItemId = "quest_serpent_fang" },
            Reward = new QuestReward { Gold = 200, Experience = 80 },
            MinPartyLevel = 3
        });

        Register(new QuestDefinition
        {
            Id = "explore_fire2", Name = "Mapping the Inferno", Type = QuestType.Explore,
            Description = "A cartographer needs maps of the deeper fire dungeon.",
            GiverTownId = "grey", GiverNpcName = "Cartographer Finn", GiverType = QuestGiverType.TavernNpc,
            OfferText = "My maps end at the first level of the Fire Dungeon. Reach level 2 and report back!",
            ProgressText = "You haven't reached level 2 yet? It's deeper in!",
            CompleteText = "Magnificent! With your observations I can complete my atlas. Well earned!",
            Objective = new QuestObjective { TargetMapId = "dungeon_fire_l2" },
            Reward = new QuestReward { Gold = 175, Experience = 100 },
            MinPartyLevel = 3
        });

        Register(new QuestDefinition
        {
            Id = "kill_trolls_fawn", Name = "Troll Bridge", Type = QuestType.Kill,
            Description = "Trolls have claimed the roads near Fawn.",
            GiverTownId = "fawn", GiverNpcName = "Warden Kira", GiverType = QuestGiverType.TownNpc,
            OfferText = "Trolls block our trade routes! Slay 6 of the brutes and reopen the roads.",
            ProgressText = "The trolls still hold the bridges. We need them cleared!",
            CompleteText = "The roads are open! Trade flows freely again. Fawn is in your debt.",
            Objective = new QuestObjective { TargetMonsterIds = new[] { "troll" }, TargetCount = 6 },
            Reward = new QuestReward { Gold = 300, Experience = 150, ItemId = "gem_ruby_flawed" },
            MinPartyLevel = 4
        });

        Register(new QuestDefinition
        {
            Id = "fetch_ancient_amulet", Name = "The Sunken Amulet", Type = QuestType.Fetch,
            Description = "A powerful amulet was lost to the ghouls.",
            GiverTownId = "fawn", GiverNpcName = "Mystic Rowan", GiverType = QuestGiverType.TavernNpc,
            OfferText = "An Ancient Amulet of warding was lost to the ghouls. I must have it back!",
            ProgressText = "The amulet is still out there. The ghouls feast upon its power.",
            CompleteText = "The amulet! Its protective wards are intact. You've saved us all.",
            Objective = new QuestObjective { RequiredItemId = "quest_ancient_amulet" },
            Reward = new QuestReward { Gold = 250, Experience = 125 },
            MinPartyLevel = 4
        });

        // === Late Game (Levels 5-8, with chains) ===

        Register(new QuestDefinition
        {
            Id = "kill_vampires_death", Name = "Vampire Hunt", Type = QuestType.Kill,
            Description = "Vampires terrorize the people of Death Gulch.",
            GiverTownId = "death_gulch", GiverNpcName = "Van Helsig", GiverType = QuestGiverType.TownNpc,
            OfferText = "Vampires plague this town! Destroy 5 of the fiends and I'll share what I know of the Lich.",
            ProgressText = "The vampires still lurk in the shadows. Keep hunting!",
            CompleteText = "Five vampires destroyed! You've proven yourself. Now, about that Lich...",
            Objective = new QuestObjective { TargetMonsterIds = new[] { "vampire" }, TargetCount = 5 },
            Reward = new QuestReward { Gold = 400, Experience = 200 },
            MinPartyLevel = 5
        });

        Register(new QuestDefinition
        {
            Id = "fetch_lich_phylactery", Name = "Destroy the Lich", Type = QuestType.Fetch,
            Description = "Find and destroy the Lich's phylactery to end its reign.",
            GiverTownId = "death_gulch", GiverNpcName = "Van Helsig", GiverType = QuestGiverType.TownNpc,
            OfferText = "The Lich can only be truly destroyed by shattering its phylactery. Find it among the liches!",
            ProgressText = "The phylactery still exists. The Lich reforms each night!",
            CompleteText = "The phylactery is destroyed! The Lich's power crumbles. You are true heroes!",
            Objective = new QuestObjective { RequiredItemId = "quest_lich_phylactery" },
            Reward = new QuestReward { Gold = 600, Experience = 400 },
            PrerequisiteQuestId = "kill_vampires_death",
            MinPartyLevel = 6
        });

        Register(new QuestDefinition
        {
            Id = "explore_snake5", Name = "Into the Depths", Type = QuestType.Explore,
            Description = "Explore the deepest reaches of the Snake Dungeon.",
            GiverTownId = "devil_guard", GiverNpcName = "Warden Grimjaw", GiverType = QuestGiverType.TownNpc,
            OfferText = "The Snake Dungeon hides great secrets on level 5. Reach it and return alive!",
            ProgressText = "Level 5 of the Snake Dungeon awaits. Deeper!",
            CompleteText = "You survived level 5! Your report reveals much. Well done, adventurer.",
            Objective = new QuestObjective { TargetMapId = "dungeon_snake_l5" },
            Reward = new QuestReward { Gold = 350, Experience = 200 },
            MinPartyLevel = 5
        });

        Register(new QuestDefinition
        {
            Id = "kill_daemons_devil", Name = "Daemon Bane", Type = QuestType.Kill,
            Description = "Daemons are emerging from the dungeon depths.",
            GiverTownId = "devil_guard", GiverNpcName = "Warden Grimjaw", GiverType = QuestGiverType.TownNpc,
            OfferText = "Daemons pour forth from below! Slay 3 of the abominations before they overwhelm us!",
            ProgressText = "The daemons still come. We need them destroyed!",
            CompleteText = "Three daemons banished! The rift weakens. Devil Guard stands because of you.",
            Objective = new QuestObjective { TargetMonsterIds = new[] { "daemon" }, TargetCount = 3 },
            Reward = new QuestReward { Gold = 500, Experience = 350, ItemId = "gem_onyx_perfect" },
            PrerequisiteQuestId = "explore_snake5",
            MinPartyLevel = 6
        });

        Register(new QuestDefinition
        {
            Id = "fetch_fire_crystal", Name = "The Fire Crystal", Type = QuestType.Fetch,
            Description = "A crystal of elemental fire is held by dragons.",
            GiverTownId = "moon", GiverNpcName = "Sage Lunara", GiverType = QuestGiverType.TownNpc,
            OfferText = "I've discovered that dragons guard a Fire Crystal. Retrieve it and we unlock ancient power!",
            ProgressText = "The Fire Crystal remains with the dragons. Be brave!",
            CompleteText = "The Fire Crystal! Its flames dance with ancient magic. Remarkable!",
            Objective = new QuestObjective { RequiredItemId = "quest_fire_crystal" },
            Reward = new QuestReward { Gold = 800, Experience = 500 },
            PrerequisiteQuestId = "fetch_moonstone",
            MinPartyLevel = 6
        });

        Register(new QuestDefinition
        {
            Id = "fetch_time_shard", Name = "The Time Shard", Type = QuestType.Fetch,
            Description = "A fragment of crystallized time is guarded by balrons.",
            GiverTownId = "moon", GiverNpcName = "Sage Lunara", GiverType = QuestGiverType.TownNpc,
            OfferText = "The final piece — a Time Shard, held by the balrons themselves. This is the ultimate test!",
            ProgressText = "The Time Shard eludes us still. The balrons are fearsome guardians.",
            CompleteText = "The Time Shard! With moonstone, fire, and time united... the way forward is clear!",
            Objective = new QuestObjective { RequiredItemId = "quest_time_shard" },
            Reward = new QuestReward { Gold = 1000, Experience = 750, ItemId = "gem_diamond_perfect" },
            PrerequisiteQuestId = "fetch_fire_crystal",
            MinPartyLevel = 7
        });
    }

    private static void RegisterFetchDrops()
    {
        // Early game: common monsters, high drop chance
        FetchDrops.Add(new FetchDropInfo("fetch_old_ring", "quest_old_ring", new[] { "skeleton", "zombie" }, 50));
        // Mid game: moderate monsters, good drop chance
        FetchDrops.Add(new FetchDropInfo("fetch_moonstone", "quest_moonstone", new[] { "wraith" }, 60));
        FetchDrops.Add(new FetchDropInfo("fetch_serpent_fang", "quest_serpent_fang", new[] { "giant_spider" }, 50));
        FetchDrops.Add(new FetchDropInfo("fetch_ancient_amulet", "quest_ancient_amulet", new[] { "ghoul" }, 60));
        // Late game: rare monsters are hard to find, so high drop chance when you do
        FetchDrops.Add(new FetchDropInfo("fetch_lich_phylactery", "quest_lich_phylactery", new[] { "lich" }, 80));
        FetchDrops.Add(new FetchDropInfo("fetch_fire_crystal", "quest_fire_crystal", new[] { "dragon" }, 75));
        FetchDrops.Add(new FetchDropInfo("fetch_time_shard", "quest_time_shard", new[] { "balron" }, 75));
    }

    private static void Register(QuestDefinition quest) => AllQuests[quest.Id] = quest;

    public static QuestDefinition? FindById(string id) => AllQuests.GetValueOrDefault(id);

    public static List<QuestDefinition> GetAll() => AllQuests.Values.ToList();

    public static List<FetchDropInfo> GetFetchDropsForMonster(string monsterId)
    {
        return FetchDrops.Where(f => f.DropMonsterIds.Contains(monsterId)).ToList();
    }

    public static List<FetchDropInfo> GetFetchDropsForQuest(string questId)
    {
        return FetchDrops.Where(f => f.QuestId == questId).ToList();
    }

    public static List<QuestDefinition> GetQuestsForTown(string townId)
    {
        return AllQuests.Values.Where(q => q.GiverTownId == townId).ToList();
    }

    public static List<(string NpcName, QuestGiverType Type)> GetNpcsForTown(string townId)
    {
        return AllQuests.Values
            .Where(q => q.GiverTownId == townId)
            .Select(q => (q.GiverNpcName, q.GiverType))
            .Distinct()
            .ToList();
    }

    public static List<QuestDefinition> GetQuestsForNpc(string townId, string npcName)
    {
        return AllQuests.Values
            .Where(q => q.GiverTownId == townId && q.GiverNpcName == npcName)
            .ToList();
    }

    public static bool MonsterCountsForQuest(string questId, string monsterId)
    {
        var quest = FindById(questId);
        if (quest == null || quest.Type != QuestType.Kill) return false;
        return quest.Objective.TargetMonsterIds.Contains(monsterId);
    }

    public static bool IsUndeadMonster(string monsterId) => UndeadMonsterIds.Contains(monsterId);
}
