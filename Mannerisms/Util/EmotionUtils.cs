using System.Collections.Generic;
using System.Linq;
using Dalamud.Game.Chat;
using Mannerisms.Data;

namespace Mannerisms.Util;

public static class EmotionUtils
{
    private static readonly List<EmotionGestureInternal> Emotions =
    [
        new EmotionGestureInternal("/smile", @"(:\)+|c:)(?=\s|$)"),
        new EmotionGestureInternal("/grin", @"(:(D|B)+)(?=\s|$)"),
        new EmotionGestureInternal("/cry", @"(:'(c|\()+)(?=\s|$)"),
        new EmotionGestureInternal("/sad", @"((:(c|\(|/)+)|D:)(?=\s|$)"),
        new EmotionGestureInternal("/amazed", @"(:(o|O)+)(?=\s|$)"),
        new EmotionGestureInternal("/smirk", @"(:(3|P)+c?)(?=\s|$)"),
        new EmotionGestureInternal("/wink", @"(;(\)|D|P|B)+)(?=\s|$)"),
    ];

    public static bool TryMatchEmotion(IHandleableChatMessage message, out EmotionGestureInternal emotionGesture)
    {
        foreach (var emotion in Emotions.Where(emotion => emotion.IsMatch(message)))
        {
            emotionGesture = emotion;
            return true;
        }

        emotionGesture = null!;
        return false;
    }
}