using System.Collections.Generic;

namespace Mannerisms.Util;

public class CommonGestureInternal
{
    public string Label = string.Empty;
    public List<string> Examples = [];
    public string ExamplesString => string.Join(", ", Examples);
    public string DefaultCommand = string.Empty;
    public string Pattern = string.Empty;
    public bool  IsCaseSensitive;
    public bool IsTargetOnly;
}

public static class CommonGestureUtil
{
    public static readonly Dictionary<string, CommonGestureInternal> List = new()
    {
        ["greetings"] = new CommonGestureInternal()
        {
            Label = "Greetings",
            Examples = ["hi", "hey", "hello", "sup", "etc."],
            DefaultCommand = "/wave",
            Pattern = @"^(he+y+a?|sup+|hi+|hello+)\b",
            IsCaseSensitive = false,
            IsTargetOnly = true,
        },
        ["goodbyes"] = new CommonGestureInternal()
        {
            Label = "Goodbyes",
            Examples = ["goodbye", "bye", "nini", "etc."],
            DefaultCommand = "/goodbye",
            Pattern = @"^(nini+|bye+|goodbye+|see ya+)\b",
            IsCaseSensitive = false,
            IsTargetOnly = true,
        },
        ["affirmatives"] = new CommonGestureInternal()
        {
            Label = "Affirmatives",
            Examples = ["yes", "yeah", "yep", "same", "etc."], 
            DefaultCommand = "/yes",
            Pattern = @"^(y(e+|e+a+|a+|i+)(h*|s*|p*)|sa+me+|fr+|tru+e+|su+re+)\b",
            IsCaseSensitive = false,
            IsTargetOnly = true,
        },
        ["negatives"] = new CommonGestureInternal()
        {
            Label = "Negatives",
            Examples = ["no", "nah", "nope", "etc."], 
            DefaultCommand = "/no",
            Pattern = @"^(ny*o+|no+pe+|lie+s+|na+h+|nu+|nu uh+)\b",
            IsCaseSensitive = false,
            IsTargetOnly = true,
        },
        ["chuckles"] = new CommonGestureInternal()
        {
            Label = "Chuckles",
            Examples = ["haha", "hehe", "hihi", "etc."], 
            DefaultCommand = "/chuckle",
            Pattern = @"\b(haha*|hehe*|hihi)\b",
            IsCaseSensitive = false,
            IsTargetOnly = true,
        },
        ["laughs"] = new CommonGestureInternal()
        {
            Label = "Laughs",
            Examples = ["lol", "lmao", "lmfao", "etc."], 
            DefaultCommand = "/laugh",
            Pattern = @"\b(lmf?ao+|lo+l+)\b",
            IsCaseSensitive = false,
            IsTargetOnly = true,
        },
    };
}