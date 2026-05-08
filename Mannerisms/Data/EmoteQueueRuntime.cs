using Mannerisms.Util;
using System.Collections.Generic;
using System.Linq;
using ECommons.DalamudServices;

namespace Mannerisms.Data;

public class EmoteQueueItem(CachedEmote emote, float timeoutThreshold)
{
    public float Timer;
    public float TimeoutThreshold = timeoutThreshold;
    public bool IsExpired => Timer >= TimeoutThreshold;
    public readonly CachedEmote Emote = emote;
}

public class EmoteQueueRuntime
{
    public readonly List<EmoteQueueItem> Emotes = [];

    public void EnqueueEmote(string command, float timeoutThreshold)
    {
        if (!EmoteUtils.TryGetAny(command, out var cachedEmote))
        {
            // The emote doesn't exist.
            Svc.Log.Debug("Tried to enqueue an emote that doesn't exist.");
            return;
        }
        
        var existing = Emotes.FirstOrDefault(e => e.Emote.Command == cachedEmote.Command);
        if (existing != null)
        {
            // This emote is already queued.
            // Set a new threshold, reset the timer, and bump it to first place.
            existing.TimeoutThreshold = timeoutThreshold;
            existing.Timer = 0;
            Emotes.Remove(existing);
            Emotes.Insert(0, existing);
        }
        else
        {
            Emotes.Insert(0, new EmoteQueueItem(cachedEmote, timeoutThreshold));
        }
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
