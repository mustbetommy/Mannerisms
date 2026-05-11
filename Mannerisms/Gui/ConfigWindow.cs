using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Interface.Windowing;
using ECommons;
using ECommons.ImGuiMethods;
using Mannerisms.Gui.Tabs;
using System.Diagnostics;
using System.Numerics;

namespace Mannerisms.Gui;

public class ConfigWindow : Window
{
    private readonly Plugin _plugin;
    private readonly Stopwatch _configModifiedStopwatch = new();

    private readonly GesturesTab _gesturesTab;
    private readonly SettingsTab _settingsTab;

    public ConfigWindow(Plugin plugin)
        : base($"{Plugin.Name} v{Plugin.Version}###{Plugin.Name}_{nameof(ConfigWindow)}", ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse)
    {
        AllowClickthrough = false;
        _plugin = plugin;

        _gesturesTab = new GesturesTab(plugin);
        _settingsTab = new SettingsTab(plugin);

        TitleBarButtons.Add(new TitleBarButton
        {
            Icon = FontAwesomeIcon.Heart,
            ShowTooltip = () =>
            {
                using (ImRaii.Tooltip())
                    ImGuiEx.IconWithText(FontAwesomeIcon.Coffee, "Ko-fi");
            },
            Priority = 1,
            IconOffset = new Vector2(1.5f, 1),
            Click = _ =>
            {
                GenericHelpers.ShellStart("https://ko-fi.teejaylabs.com/");
            },
            AvailableClickthrough = true,
        });

        #if DEBUG
        IsOpen = true;
        #endif
    }

    public override void PreDraw()
    {
        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(800, 400),
            MaximumSize = new Vector2(float.MaxValue, float.MaxValue),
        };
    }

    public override void Draw()
    {
        _plugin.Config.CheckSave();
        
        using var tabBar = ImRaii.TabBar("##config_window_tabs");
        if (!tabBar) return;
        
        _gesturesTab.Draw();
        _settingsTab.Draw();
    }

    public override void OnClose()
    {
        _configModifiedStopwatch.Reset();
        _plugin.Config.Save();
        _plugin.ReloadCurrentCharacter();
        base.OnClose();
    }
}
