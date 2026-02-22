using UltimaIII.Core.Models;

namespace UltimaIII.Core.Engine;

public static class QuestEngine
{
    public static List<QuestDefinition> GetAvailableQuests(Party party, string townId, string npcName)
    {
        var allForNpc = QuestRegistry.GetQuestsForNpc(townId, npcName);
        return allForNpc.Where(q =>
            !party.QuestLog.IsQuestActive(q.Id) &&
            !party.CompletedQuests.Contains(q.Id) &&
            (q.PrerequisiteQuestId == null || party.CompletedQuests.Contains(q.PrerequisiteQuestId)) &&
            GetPartyLevel(party) >= q.MinPartyLevel
        ).ToList();
    }

    public static List<QuestDefinition> GetTurnInQuests(Party party, string townId, string npcName)
    {
        var allForNpc = QuestRegistry.GetQuestsForNpc(townId, npcName);
        return allForNpc.Where(q =>
            party.QuestLog.IsQuestActive(q.Id) &&
            IsQuestComplete(party, q)
        ).ToList();
    }

    public static List<QuestDefinition> GetInProgressQuests(Party party, string townId, string npcName)
    {
        var allForNpc = QuestRegistry.GetQuestsForNpc(townId, npcName);
        return allForNpc.Where(q =>
            party.QuestLog.IsQuestActive(q.Id) &&
            !IsQuestComplete(party, q)
        ).ToList();
    }

    public static bool IsQuestComplete(Party party, QuestDefinition quest)
    {
        var progress = party.QuestLog.GetProgress(quest.Id);
        if (progress == null) return false;

        return quest.Type switch
        {
            QuestType.Kill => progress.KillCount >= quest.Objective.TargetCount,
            QuestType.Fetch => party.SharedInventory.Any(i => i.Id == quest.Objective.RequiredItemId),
            QuestType.Explore => progress.LocationVisited,
            _ => false
        };
    }

    public static void AcceptQuest(Party party, QuestDefinition quest)
    {
        party.QuestLog.AcceptQuest(quest.Id);
    }

    public static (int gold, int xp, string? itemId) TurnInQuest(Party party, QuestDefinition quest)
    {
        // Remove fetch item from inventory if applicable
        if (quest.Type == QuestType.Fetch)
        {
            var item = party.SharedInventory.FirstOrDefault(i => i.Id == quest.Objective.RequiredItemId);
            if (item != null)
            {
                party.RemoveFromInventory(item);
            }
        }

        // Award rewards
        party.AddGold(quest.Reward.Gold);

        // Award XP to all living members
        foreach (var member in party.GetLivingMembers())
        {
            member.GainExperience(quest.Reward.Experience);
        }

        // Award item reward if specified
        if (!string.IsNullOrEmpty(quest.Reward.ItemId))
        {
            var rewardTemplate = ItemRegistry.FindById(quest.Reward.ItemId);
            if (rewardTemplate != null)
            {
                party.AddToInventory(ItemRegistry.CloneItem(rewardTemplate));
            }
        }

        // Mark completed
        party.QuestLog.RemoveQuest(quest.Id);
        party.CompletedQuests.Add(quest.Id);

        return (quest.Reward.Gold, quest.Reward.Experience, quest.Reward.ItemId);
    }

    public static List<string> OnMonsterKilled(Party party, string monsterId)
    {
        var messages = new List<string>();

        foreach (var progress in party.QuestLog.GetAllProgress())
        {
            var quest = QuestRegistry.FindById(progress.QuestId);
            if (quest == null || quest.Type != QuestType.Kill) continue;

            if (QuestRegistry.MonsterCountsForQuest(quest.Id, monsterId))
            {
                party.QuestLog.IncrementKillCount(quest.Id);
                var updated = party.QuestLog.GetProgress(quest.Id)!;

                if (updated.KillCount >= quest.Objective.TargetCount)
                {
                    messages.Add($"Quest complete: {quest.Name}! Return to {quest.GiverNpcName}.");
                }
                else
                {
                    messages.Add($"Quest: {quest.Name} ({updated.KillCount}/{quest.Objective.TargetCount})");
                }
            }
        }

        return messages;
    }

    public static List<string> OnMapEntered(Party party, string mapId)
    {
        var messages = new List<string>();

        foreach (var progress in party.QuestLog.GetAllProgress())
        {
            var quest = QuestRegistry.FindById(progress.QuestId);
            if (quest == null || quest.Type != QuestType.Explore) continue;

            if (quest.Objective.TargetMapId == mapId && !progress.LocationVisited)
            {
                party.QuestLog.MarkExplored(quest.Id);
                messages.Add($"Quest complete: {quest.Name}! Return to {quest.GiverNpcName}.");
            }
        }

        return messages;
    }

    public static List<(string ItemId, string QuestName)> GetQuestItemDrops(Party party, string monsterId, Random rng)
    {
        var drops = new List<(string, string)>();
        var fetchDrops = QuestRegistry.GetFetchDropsForMonster(monsterId);

        foreach (var dropInfo in fetchDrops)
        {
            // Only drop if quest is active and item not already in inventory
            if (!party.QuestLog.IsQuestActive(dropInfo.QuestId)) continue;
            if (party.SharedInventory.Any(i => i.Id == dropInfo.ItemId)) continue;

            if (rng.Next(100) < dropInfo.DropChancePercent)
            {
                var quest = QuestRegistry.FindById(dropInfo.QuestId);
                var questName = quest?.Name ?? dropInfo.QuestId;
                drops.Add((dropInfo.ItemId, questName));
            }
        }

        return drops;
    }

    private static int GetPartyLevel(Party party)
    {
        if (party.IsEmpty) return 1;
        return party.Members.Max(m => m.Level);
    }
}
