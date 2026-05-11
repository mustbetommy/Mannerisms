using Dalamud.Game.Chat;
using Dalamud.Game.Text;
using ECommons.DalamudServices;
using Mannerisms.Util;
using System;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;
using ECommons;

namespace Mannerisms.Data;

public abstract class GestureBase
{
    public bool Enabled = true;
    public string Command = string.Empty;
    public bool IsCaseSensitive = true;
    public bool IsTargetOnly = true;

    public virtual bool IsMatch(IHandleableChatMessage message, string senderName)
    {
        if (!Enabled || Command.IsNullOrEmpty())
        {
            return false;
        }
        
        if (!IsTargetOnly)
        {
            return true;
        }

        if (!CharacterUtils.TryGetTarget(out var targetName))
        {
            return false;
        }

        switch (message.LogKind)
        {
            case XivChatType.TellOutgoing when targetName != senderName:
                return false;
            case XivChatType.Party:
            case XivChatType.Alliance:
            {
                var isInParty = Svc.Party.Select(member => CharacterUtils.SanitizeName(member.Name.TextValue))
                    .Any(memberName => memberName == targetName);

                if (!isInParty)
                {
                    return false;
                }

                break;
            }
        }

        return true;
    }

    protected bool IsPatternMatch(IHandleableChatMessage message, string pattern)
    {
        try
        {
            var options = IsCaseSensitive ? RegexOptions.None : RegexOptions.IgnoreCase;
            return Regex.IsMatch(message.Message.TextValue, pattern, options);
        }
        catch (Exception e)
        {
            Svc.Log.Error($"Invalid regex pattern '{pattern}': {e.Message}");
        }

        return false;
    }
}

public class AdvancedGesture : GestureBase
{
    public string Pattern = string.Empty;
    
    public override bool IsMatch(IHandleableChatMessage message, string senderName)
    {
        return base.IsMatch(message, senderName) && IsPatternMatch(message, Pattern);
    }
}

public class CommonGesture : GestureBase
{
    public string InternalKey = string.Empty;
    
    public override bool IsMatch(IHandleableChatMessage message, string senderName)
    {
        if (!base.IsMatch(message, senderName)) 
        {
            return false;
        }

        if (!CommonGestureUtil.List.TryGetValue(InternalKey, out var gesture)) return false;
        
        IsCaseSensitive = gesture.IsCaseSensitive;
        IsTargetOnly = gesture.IsTargetOnly;
            
        return IsPatternMatch(message, gesture.Pattern);
    }
}

public class EmotionGestureInternal : GestureBase
{
    private readonly string _pattern;

    public EmotionGestureInternal(string command, string pattern)
    {
        Command = command;
        _pattern = pattern;
        IsTargetOnly = false;
    }

    public bool IsMatch(IHandleableChatMessage message)
    {
        return IsPatternMatch(message, _pattern);
    }
}

public enum ESimpleGesturePatternMatchType
{
    Anywhere,
    Start,
    End,
    [Description("Full Message")]
    FullMessage,
}

public class SimpleGesture : GestureBase
{
    // ReSharper disable once MemberCanBePrivate.Global
    // Because it needs to be saved to the plugin config JSON.
    public string Pattern = string.Empty;
    public string Terms = string.Empty;
    public ESimpleGesturePatternMatchType MatchType = ESimpleGesturePatternMatchType.Anywhere;

    public void GeneratePattern()
    {
        if (string.IsNullOrEmpty(Terms))
        {
            Pattern = string.Empty;
            return;
        }

        Pattern = string.Empty;

        if (MatchType is ESimpleGesturePatternMatchType.Start or ESimpleGesturePatternMatchType.FullMessage)
        {
            Pattern += "^";
        }

        var termsList = Terms.Split(',')
            .Select(t => t.Trim())
            .Where(t => !string.IsNullOrEmpty(t))
            .Select(t =>
            {
                var preserved = t.Replace("+", "\x00").Replace("*", "\x01");
                var escaped = Regex.Escape(preserved);
                return escaped.Replace("\x00", "+").Replace("\x01", "*");
            });

        var joined = string.Join("|", termsList);
    
        if (MatchType == ESimpleGesturePatternMatchType.Anywhere)
        {
            // Only use word boundaries for "anywhere" mode to prevent partial word matches
            // Check if the terms start/end with word characters
            var firstChar = joined[0];
            var lastChar = joined[^1];
            var prefix = char.IsLetterOrDigit(firstChar) ? "\\b" : "";
            var suffix = char.IsLetterOrDigit(lastChar) ? "\\b" : "";
            Pattern += $"{prefix}({joined}){suffix}";
        }
        else
        {
            Pattern += $"({joined})";
        }

        Pattern += "[!?]*";

        if (MatchType is ESimpleGesturePatternMatchType.End or ESimpleGesturePatternMatchType.FullMessage)
        {
            Pattern += "$";
        }
    }
    
    public override bool IsMatch(IHandleableChatMessage message, string senderName)
    {
        return base.IsMatch(message, senderName) && IsPatternMatch(message, Pattern);
    }
}
