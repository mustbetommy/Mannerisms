using Dalamud.Game.Chat;
using Dalamud.Utility;
using ECommons.DalamudServices;
using Lumina.Excel.Sheets;
using Mannerisms.Data;
using System.Collections.Generic;
using System.Linq;

namespace Mannerisms.Util;

public class CachedEmote(string name, uint icon, string command)
{
    public readonly string Name = name;
    public readonly uint Icon = icon;
    public readonly string Command = command;
}

public static class EmoteUtils
{
    private static readonly Dictionary<string, CachedEmote> EmotesList = [];
    private static readonly Dictionary<string, CachedEmote> ExpressionsList = [];
    private static readonly Dictionary<string, CachedEmote> CombinedList = [];

    private static void FetchAll()
    {
        if (EmotesList.Count > 0) return;
        foreach (var emote in Svc.Data.GetExcelSheet<Emote>()!)
        {
            if (!emote.TextCommand.IsValid) continue;
            var command = emote.TextCommand.Value.Command.ToDalamudString().TextValue;
            if (command.IsNullOrEmpty()) continue;
            var icon = emote.Icon;
            var name = emote.Name.ToDalamudString().TextValue;
            var newCachedEmote = new CachedEmote(name, icon, command);
            if (emote.EmoteCategory.Value.RowId == 3)
            {
                ExpressionsList.TryAdd(command, newCachedEmote);
            } else
            {
                EmotesList.TryAdd(command, newCachedEmote);
            }
        }

        foreach (var emote in EmotesList)
        {
            CombinedList.TryAdd(emote.Key, emote.Value);
        }

        foreach (var expression in ExpressionsList)
        {
            CombinedList.TryAdd(expression.Key, expression.Value);
        }
    }

    public static IEnumerable<KeyValuePair<string, CachedEmote>> GetCombinedList()
    {
        FetchAll();
        return CombinedList;
    }

    public static IEnumerable<KeyValuePair<string, CachedEmote>> GetEmotesList() {
        FetchAll();
        return EmotesList.OrderBy(e => e.Key);
    }

    public static IEnumerable<KeyValuePair<string, CachedEmote>> GetExpressionsList() {
        FetchAll();
        return ExpressionsList.OrderBy(e => e.Key);
    }

    public static bool TryGetAny(string command, out CachedEmote emote)
    {
        if (TryGetEmote(command, out var foundEmote))
        {
            emote = foundEmote;
            return true;
        }

        if (TryGetExpression(command, out var foundExpression))
        {
            emote = foundExpression;
            return true;
        }

        emote = null!;
        return false;
    }

    public static bool TryGetEmote(string command, out CachedEmote emote)
    {
        FetchAll();

        if (EmotesList.TryGetValue(command, out var foundEmote))
        {
            emote = foundEmote;
            return true;
        }

        emote = null!;
        return false;
    }

    public static bool TryGetExpression(string command, out CachedEmote expression)
    {
        FetchAll();

        if (ExpressionsList.TryGetValue(command, out var foundExpression))
        {
            expression = foundExpression;
            return true;
        }

        expression = null!;
        return false;
    }
}
