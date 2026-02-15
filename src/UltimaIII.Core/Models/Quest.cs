namespace UltimaIII.Core.Models;

public enum QuestType
{
    Kill,
    Fetch,
    Explore
}

public enum QuestGiverType
{
    TavernNpc,
    TownNpc
}

public class QuestObjective
{
    /// <summary>Kill quest: monster ID(s) to kill. Supports "undead" as a category.</summary>
    public string[] TargetMonsterIds { get; init; } = Array.Empty<string>();

    /// <summary>Kill quest: how many to kill.</summary>
    public int TargetCount { get; init; }

    /// <summary>Fetch quest: item ID to find and return.</summary>
    public string RequiredItemId { get; init; } = string.Empty;

    /// <summary>Explore quest: map ID to visit.</summary>
    public string TargetMapId { get; init; } = string.Empty;
}

public class QuestReward
{
    public int Gold { get; init; }
    public int Experience { get; init; }
    public string? ItemId { get; init; }
}

public class QuestDefinition
{
    public string Id { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public QuestType Type { get; init; }

    public string GiverTownId { get; init; } = string.Empty;
    public string GiverNpcName { get; init; } = string.Empty;
    public QuestGiverType GiverType { get; init; }

    public string OfferText { get; init; } = string.Empty;
    public string ProgressText { get; init; } = string.Empty;
    public string CompleteText { get; init; } = string.Empty;

    public QuestObjective Objective { get; init; } = new();
    public QuestReward Reward { get; init; } = new();

    public string? PrerequisiteQuestId { get; init; }
    public int MinPartyLevel { get; init; } = 1;
}
