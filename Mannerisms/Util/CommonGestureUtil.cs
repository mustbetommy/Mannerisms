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
            Pattern = "(he+y+a?|sup+|hi+|hello+)\\b",
            IsCaseSensitive = false,
            IsTargetOnly = true,
        },
        ["goodbyes"] = new CommonGestureInternal()
        {
            Label = "Goodbyes",
            Examples = ["goodbye", "bye", "nini", "etc."],
            DefaultCommand = "/goodbye",
            Pattern = "(nini+|bye+|goodbye+|see (ya+|yo+u+))\\b",
            IsCaseSensitive = false,
            IsTargetOnly = true,
        },
        ["affirmatives"] = new CommonGestureInternal()
        {
            Label = "Affirmatives",
            Examples = ["yes", "yeah", "yep", "same", "etc."], 
            DefaultCommand = "/yes",
            Pattern = "(y(e+|e+a+|a+|i+)(h*|s*|p*)|sa+me+|fr+|tru+e+|su+re+)\\b",
            IsCaseSensitive = false,
            IsTargetOnly = false,
        },
        ["negatives"] = new CommonGestureInternal()
        {
            Label = "Negatives",
            Examples = ["no", "nah", "nope", "etc."], 
            DefaultCommand = "/no",
            Pattern = "(ny*o+|lie+s+|na+h+|nu+|nu uh+)\\b",
            IsCaseSensitive = false,
            IsTargetOnly = false,
        },
        ["surprise"] = new CommonGestureInternal()
        {
            Label = "Surprise",
            Examples = ["oh", "wow", "woah", ":o", "etc."], 
            DefaultCommand = "/amazed",
            Pattern = "(wo+w+|woah+|wtf+|:o+|omg+|oh+)\\b",
            IsCaseSensitive = false,
            IsTargetOnly = false,
        },
        ["chuckles"] = new CommonGestureInternal()
        {
            Label = "Chuckles",
            Examples = ["haha", "hehe", "hihi", "etc."], 
            DefaultCommand = "/chuckle",
            Pattern = "(haha|hehe|hihi)\\b",
            IsCaseSensitive = false,
            IsTargetOnly = false,
        },
        ["laughs"] = new CommonGestureInternal()
        {
            Label = "Laughs",
            Examples = ["lol", "lmao", "lmfao", "etc."], 
            DefaultCommand = "/laugh",
            Pattern = "(lmf?ao+|lo+l+)\\b",
            IsCaseSensitive = false,
            IsTargetOnly = false,
        },
        ["cries"] = new CommonGestureInternal()
        {
            Label = "Cries",
            Examples = [":'(", ":'c"], 
            DefaultCommand = "/cry",
            Pattern = @"(:'(c|\()+)(?=\s|$)",
            IsCaseSensitive = false,
            IsTargetOnly = false,
        },
    };
}