namespace UltimaIII.Core.Models;

/// <summary>
/// Stores one recruitable NPC per town tavern.
/// </summary>
public class TavernRoster
{
    private readonly Dictionary<string, Character> _npcs = new();

    public Character? GetNpc(string townId) =>
        _npcs.TryGetValue(townId, out var npc) ? npc : null;

    public void SetNpc(string townId, Character npc) =>
        _npcs[townId] = npc;

    public Character? TakeNpc(string townId)
    {
        if (!_npcs.Remove(townId, out var npc)) return null;
        return npc;
    }

    public bool HasNpc(string townId) => _npcs.ContainsKey(townId);

    public IReadOnlyDictionary<string, Character> AllNpcs => _npcs;

    public void Clear() => _npcs.Clear();
}
