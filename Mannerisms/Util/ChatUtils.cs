using ECommons.Automation;

namespace Mannerisms.Util;

public static class ChatUtils
{
    public static void Print(string message)
    {
        Chat.ExecuteCommand($"/echo [Mannerisms] {message}");
    }
}
