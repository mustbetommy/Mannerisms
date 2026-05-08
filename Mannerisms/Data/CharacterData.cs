using ECommons;
using Mannerisms.Util;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Mannerisms.Data;

public class CharacterData(string name, string world)
{
    public string Key => $"{Name}@{World}";
    public string Name = name;
    public string World = world;

    [JsonIgnore] private bool _commonGesturesVerified;
    public bool SuggestEmotions = true;
    public Dictionary<string, CommonGesture> CommonGestures = [];
    public readonly List<SimpleGesture> SimpleGestures = [];
    public readonly List<AdvancedGesture> AdvancedGestures = [];

    public bool VerifyCommonGestures()
    {
        if (_commonGesturesVerified) return false;

        Dictionary<string, CommonGesture> newCommonGestures = [];
        
        foreach (var commonGesture in CommonGestureUtil.List)
        {
            CommonGesture cleanEntry = new()
            {
                InternalKey = commonGesture.Key,
                Command = commonGesture.Value.DefaultCommand,
            };

            if (CommonGestures.TryGetValue(commonGesture.Key, out var existing))
            {
                if (existing.Command.IsNullOrEmpty())
                {
                    existing.Command = commonGesture.Value.DefaultCommand;
                }
                cleanEntry = existing;
            }

            newCommonGestures.TryAdd(commonGesture.Key, cleanEntry);
        }

        _commonGesturesVerified = true;
        CommonGestures = newCommonGestures;

        return true;
    }
}
