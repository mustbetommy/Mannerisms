using Dalamud.Configuration;
using ECommons.DalamudServices;
using Mannerisms.Data;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Mannerisms.Util;

namespace Mannerisms
{
    [Serializable]
    public class PluginConfig : IPluginConfiguration
    {
        [JsonIgnore] private Stopwatch saveTimer = new();
        [JsonIgnore] private bool isDirty = false;

        public int Version { get; set; } = 1;

        public SortedDictionary<string, CharacterData> CharactersList = new(StringComparer.OrdinalIgnoreCase);
        public int SuggestionTimeout = 10;
        public int MaxSuggestions = 3;
        public bool KeepSuggestionsOpen = false;
        public CustomKeybind AcceptSuggestionKeybind = new();
        public CustomKeybind DismissSuggestionKeybind = new();

        public void MarkDirty()
        {
            Svc.Log.Debug($"Marking config dirty.");
            isDirty = true;
            saveTimer.Restart();
        }

        public void MarkDirtyIf(bool isDirty)
        {
            if (isDirty)
            {
                MarkDirty();
            }
        }

        public void CheckSave()
        {
            if (isDirty && saveTimer.ElapsedMilliseconds > 1000)
            {
                Svc.Log.Debug($"Saving plugin config.");
                Save();
                isDirty = false;
                saveTimer.Reset();
            }
        }

        public void Save()
        {
            PluginService.PluginInterface.SavePluginConfig(this);
        }

        public bool TryAddCharacter(string name, string world)
        {
            MarkDirty();
            Svc.Log.Debug($"Adding character: {name}@{world}");
            return CharactersList.TryAdd($"{name}@{world}", new(name, world));
        }

        public bool TryDeleteCharacter(string key)
        {
            if (!CharactersList.ContainsKey(key)) return false;
            CharactersList.Remove(key);
            MarkDirty();
            return true;
        }

        public CharacterData ReplaceCharacterData(CharacterData oldData, CharacterData newData)
        {
            newData.Name = oldData.Name;
            newData.World = oldData.World;
            CharactersList[newData.Key] = newData;
            MarkDirty();
            return newData;
        }

        public bool TryLoadCharacter(string name, string world, out CharacterData? character)
        {
            if (CharactersList.TryGetValue($"{name}@{world}", out var foundCharacter))
            {
                character = foundCharacter;
                return true;
            }

            if (TryAddCharacter(name, world))
            {
                return TryLoadCharacter(name, world, out character);
            }

            character = null;
            return false;
        }
    }
}
