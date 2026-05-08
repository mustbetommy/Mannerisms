using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Windowing;
using ECommons.Automation;
using ECommons.ImGuiMethods;
using Mannerisms.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace Mannerisms.Gui;

public class EmotePickerWindow : Window
{
    private readonly Plugin _plugin;

    public EmotePickerWindow(Plugin plugin) : base(
        $"{Plugin.Name} v{Plugin.Version}###{Plugin.Name}_{nameof(EmotePickerWindow)}",
        ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoScrollbar)
    {
        AllowClickthrough = false;
        _plugin = plugin;
    }

    public override void PreDraw()
    {
        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(200, 40),
            MaximumSize = new Vector2(float.MaxValue, float.MaxValue),
        };
    }

    private void DrawHandle()
    {
        var handleWidth = ImGuiUtils.ScaledFloat(40f);
        var handleHeight = ImGuiUtils.ScaledFloat(3f);
        
        var cPos = ImGui.GetCursorScreenPos();
        var centerX = cPos.X + (ImGui.GetContentRegionAvail().X - handleWidth) / 2f;
        var dList = ImGui.GetWindowDrawList();

        dList.AddRectFilled(
            new Vector2(centerX, cPos.Y),
            new Vector2(centerX + handleWidth, cPos.Y + handleHeight),
            ImGui.GetColorU32(new Vector4(1f, 1f, 1f, 0.2f)),
            2f
        );

        ImGui.Dummy(new Vector2(0, handleHeight + 3f));
    }

    public override void Draw()
    {
        DrawHandle();

        if (_plugin.EmoteQueue.Emotes.Count == 0)
        {
            const string emptyText = "No Suggestions";
            var textWidth = ImGui.CalcTextSize(emptyText).X;
            ImGui.SetCursorPosX(ImGui.GetCursorPosX() + (ImGui.GetContentRegionAvail().X - textWidth) / 2f);
            ImGui.TextColored(new Vector4(1f, 1f, 1f, 0.35f), emptyText);
        }

        var toRemove = new List<string>();
        var toExecute = new List<string>();

        foreach (var emote in _plugin.EmoteQueue.Emotes.Take(_plugin.Config.MaxSuggestions))
        {
            var progress = 1f - (emote.Timer / emote.TimeoutThreshold);
            var color = progress > 0.6f
                ? new Vector4(0.3f, 0.6f, 0.3f, 0.25f)
                : progress > 0.3f
                    ? new Vector4(0.7f, 0.5f, 0.1f, 0.25f)
                    : new Vector4(0.6f, 0.2f, 0.2f, 0.25f);

            var remaining = (int)Math.Ceiling(emote.TimeoutThreshold - emote.Timer);
            var remainingText = $"{remaining}s";

            var cursorPos = ImGui.GetCursorScreenPos();
            var availWidth = ImGui.GetContentRegionAvail().X;
            var itemHeight = ImGuiUtils.CheckboxSize.Y + ImGui.GetStyle().FramePadding.Y * 2;
            var drawList = ImGui.GetWindowDrawList();

            // Rounded background fill
            drawList.AddRectFilled(
                cursorPos,
                cursorPos + new Vector2(availWidth, itemHeight),
                ImGui.GetColorU32(new Vector4(0.5f, 0.5f, 0.5f, 0.1f)),
                4f
            );

            // Progress bar with color shift
            drawList.AddRectFilled(
                cursorPos,
                cursorPos + new Vector2(availWidth * progress, itemHeight),
                ImGui.GetColorU32(color),
                4f
            );

            // Clickable selectable
            if (ImGui.Selectable($"##emote_{emote.Emote.Command}", false, ImGuiSelectableFlags.None, new Vector2(availWidth, itemHeight)))
            {
                toExecute.Add(emote.Emote.Command);
            }

            if (ImGui.IsItemClicked(ImGuiMouseButton.Right))
            {
                toRemove.Add(emote.Emote.Command);
            }

            if (ImGui.IsItemHovered())
            {
                ImGui.SetTooltip("Click to use · Right-click to dismiss");
            }

            var itemMin = ImGui.GetItemRectMin();
            var iconSize = ImGuiUtils.CheckboxSize.Y;
            var padding = ImGui.GetStyle().FramePadding;
            var iconOffset = new Vector2(padding.X + 4f, ((itemHeight - iconSize) / 2f) + 2f);

            // Icon
            if (ThreadLoadImageHandler.TryGetIconTextureWrap(emote.Emote.Icon, false, out var iconPicture))
            {
                drawList.AddImage(
                    iconPicture.Handle,
                    itemMin + iconOffset,
                    itemMin + iconOffset + new Vector2(iconSize)
                );
            }

            // Emote name
            var textPos = itemMin + new Vector2(iconSize + padding.X * 2 + iconOffset.X, (itemHeight - ImGui.GetTextLineHeight()) / 2f);
            drawList.AddText(textPos, ImGui.GetColorU32(ImGuiCol.Text), emote.Emote.Name);

            // Countdown timer right-aligned
            var timerSize = ImGui.CalcTextSize(remainingText);
            var timerPos = itemMin + new Vector2(availWidth - timerSize.X - padding.X, (itemHeight - ImGui.GetTextLineHeight()) / 2f);
            drawList.AddText(timerPos, ImGui.GetColorU32(new Vector4(1f, 1f, 1f, 0.5f)), remainingText);
        }

        foreach (var command in toExecute)
        {
            Chat.ExecuteCommand($"/{command.TrimStart('/')} motion");
            _plugin.EmoteQueue.Remove(command);
        }

        foreach (var command in toRemove)
        {
            _plugin.EmoteQueue.Remove(command);
        }
    }
}
