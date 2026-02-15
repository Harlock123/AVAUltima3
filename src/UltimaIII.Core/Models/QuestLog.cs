namespace UltimaIII.Core.Models;

public class QuestProgress
{
    public string QuestId { get; init; } = string.Empty;
    public int KillCount { get; set; }
    public bool LocationVisited { get; set; }
}

public class QuestLog
{
    private readonly Dictionary<string, QuestProgress> _activeQuests = new();

    public IReadOnlyDictionary<string, QuestProgress> ActiveQuests => _activeQuests;

    public void AcceptQuest(string questId)
    {
        if (_activeQuests.ContainsKey(questId)) return;
        _activeQuests[questId] = new QuestProgress { QuestId = questId };
    }

    public bool IsQuestActive(string questId) => _activeQuests.ContainsKey(questId);

    public QuestProgress? GetProgress(string questId) =>
        _activeQuests.GetValueOrDefault(questId);

    public void RemoveQuest(string questId) => _activeQuests.Remove(questId);

    public void IncrementKillCount(string questId, int amount = 1)
    {
        if (_activeQuests.TryGetValue(questId, out var progress))
        {
            progress.KillCount += amount;
        }
    }

    public void MarkExplored(string questId)
    {
        if (_activeQuests.TryGetValue(questId, out var progress))
        {
            progress.LocationVisited = true;
        }
    }

    public void Clear() => _activeQuests.Clear();

    public List<QuestProgress> GetAllProgress() => _activeQuests.Values.ToList();
}
