using Dalamud.Game.ClientState.Keys;
using System.Collections.Generic;
using Dalamud.Bindings.ImGui;
using ECommons.DalamudServices;

namespace Mannerisms.Util;

public class CustomKeybind
{
    public VirtualKey Key = VirtualKey.NO_KEY;
    public bool IsValid => Key != VirtualKey.NO_KEY; 
    
    public bool UseShift = false;
    public bool UseCtrl = false;
    public bool UseAlt = false;

    private bool CtrlCheck => !UseCtrl || ImGui.GetIO().KeyCtrl;
    private bool AltCheck => !UseAlt || ImGui.GetIO().KeyAlt;
    private bool ShiftCheck => !UseShift || ImGui.GetIO().KeyShift;

    public string GetName()
    {
        var parts = new List<string>();
        if (UseCtrl) parts.Add("Ctrl");
        if (UseAlt) parts.Add("Alt");
        if (UseShift) parts.Add("Shift");
        parts.Add(KeyStateUtils.GetKeyName(Key));
        return string.Join(" + ", parts);
    }

    public bool IsPressed(bool consume)
    {
        if (!IsValid)
        {
            return false;
        }
        
        // Svc.Log.Info($"Checking {Key}: Is pressed = {PluginService.KeyState[Key]} | Ctrl = {CtrlCheck} |  Alt = {AltCheck} | Shift = {ShiftCheck}");
        var isPressed = Key != VirtualKey.NO_KEY && PluginService.KeyState[Key] && CtrlCheck && AltCheck  && ShiftCheck;
        if (consume && isPressed)
        {
            Consume();
        }
        return isPressed;
    }

    public void Consume()
    {
        if (!IsValid)
        {
            return;
        }
        
        PluginService.KeyState[Key] = false;
    }
}

public static class KeyStateUtils
{
    private static readonly Dictionary<VirtualKey, string> KeyNames = new()
    {
        { VirtualKey.OEM_1, ";" },
        { VirtualKey.OEM_2, "/" },
        { VirtualKey.OEM_3, "`" },
        { VirtualKey.OEM_4, "[" },
        { VirtualKey.OEM_5, "\\" },
        { VirtualKey.OEM_6, "]" },
        { VirtualKey.OEM_7, "'" },
        { VirtualKey.OEM_PLUS, "=" },
        { VirtualKey.OEM_MINUS, "-" },
        { VirtualKey.OEM_COMMA, "," },
        { VirtualKey.OEM_PERIOD, "." },
        { VirtualKey.SPACE, "Space" },
        { VirtualKey.RETURN, "Enter" },
        { VirtualKey.BACK, "Backspace" },
        { VirtualKey.DELETE, "Delete" },
        { VirtualKey.INSERT, "Insert" },
        { VirtualKey.HOME, "Home" },
        { VirtualKey.END, "End" },
        { VirtualKey.PRIOR, "Page Up" },
        { VirtualKey.NEXT, "Page Down" },
        { VirtualKey.CAPITAL, "Caps Lock" },
        { VirtualKey.SCROLL, "Scroll Lock" },
        { VirtualKey.NUMLOCK, "Num Lock" },
        { VirtualKey.LSHIFT, "Left Shift" },
        { VirtualKey.RSHIFT, "Right Shift" },
        { VirtualKey.LCONTROL, "Left Ctrl" },
        { VirtualKey.RCONTROL, "Right Ctrl" },
        { VirtualKey.LMENU, "Left Alt" },
        { VirtualKey.RMENU, "Right Alt" },
    };

    public static string GetKeyName(VirtualKey key)
    {
        return KeyNames.TryGetValue(key, out var name) ? name : key.ToString();
    }
}
