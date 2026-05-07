using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Utility;
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
    private readonly Plugin plugin;
    private Stopwatch configModifiedStopwatch = new();

    private readonly GesturesTab _gesturesTab;
    private readonly SettingsTab _settingsTab;

    public ConfigWindow(Plugin plugin)
        : base($"{Plugin.Name} v{Plugin.Version}###{Plugin.Name}_{nameof(ConfigWindow)}", ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse)
    {
        AllowClickthrough = false;
        this.plugin = plugin;

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

    #region Lifecycle

    public override void PreDraw()
    {
        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(800, 400),
            MaximumSize = ImGuiHelpers.MainViewport.Size * 1 / ImGuiHelpers.GlobalScale * 0.95f
        };
    }

    public override void Draw()
    {
        ImGui.BeginTabBar("ConfigTabs");
        _gesturesTab.Draw();
        _settingsTab.Draw();
        ImGui.EndTabBar();

        plugin.Config.CheckSave();
    }

    public override void OnClose()
    {
        configModifiedStopwatch.Reset();
        plugin.Config.Save();
        plugin.ReloadCurrentCharacter();
        base.OnClose();
    }

    #endregion
}
