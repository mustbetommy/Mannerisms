using Dalamud.Utility;
using Mannerisms.Util;
using System.Collections.Generic;
using System.Linq;

namespace Mannerisms.Data;

public class EmoteQueueItem
{
    public float Timer = 0;
    public float TimeoutThreshold;
    public bool IsExpired => Timer >= TimeoutThreshold;

    public readonly CachedEmote Emote;

    public EmoteQueueItem(CachedEmote emote, float timeoutThreshold, string? customLabel)
    {
        TimeoutThreshold = timeoutThreshold;
        Emote = emote;
    }
}

public class EmoteQueueRuntime
{
    public readonly List<EmoteQueueItem> Emotes = [];

    public bool TryAddEmote(string command, float timeoutThreshold, string? customLabel)
    {
        if (!EmoteUtils.TryGetAny(command, out var cachedEmote)) return false;
        var existing = Emotes.FirstOrDefault(e => e.Emote.Command == cachedEmote.Command);
        if (existing != null)
        {
            existing.TimeoutThreshold = timeoutThreshold;
            existing.Timer = 0;
            Emotes.Remove(existing);
            Emotes.Insert(0, existing);
        }
        else
        {
            Emotes.Insert(0, new(cachedEmote, timeoutThreshold, customLabel));
        }
        return true;

    }

    public void UpdateTimers(float time)
    {
        foreach (var emote in Emotes)
        {
            emote.Timer += time;
        }

        Emotes.RemoveAll(e => e.IsExpired);
    }

    public void Remove(string emoteCommand)
    {
        Emotes.RemoveAll(e => e.Emote.Command == emoteCommand);
    }
}
