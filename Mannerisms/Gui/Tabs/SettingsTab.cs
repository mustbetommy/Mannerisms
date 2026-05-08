using System;
using System.Linq;
using Dalamud.Bindings.ImGui;
using ECommons.ImGuiMethods;
using Mannerisms.Util;
using Dalamud.Interface.Utility.Raii;
using ECommons.DalamudServices;
using FFXIVClientStructs.FFXIV.Client.UI;

namespace Mannerisms.Gui.Tabs;

public class SettingsTab(Plugin plugin)
{
    public void Draw()
    {
        using var tab = ImRaii.TabItem("Settings##settings_tab");
        if (!tab) return;

        using var child = ImRaii.Child("##settings_tab_child");
        if (!child) return;
        
        ImGui.Spacing();

        ImGuiUtils.DrawKeyValueTable("##settings_table", (addRow) =>
        {
            var modified = false;
            
            // Max Suggestions
            addRow(() =>
                {
                    ImGui.Text("Max Suggestions");
                    ImGui.SameLine();
                    ImGuiEx.InfoMarker("The maximum number of suggestions displayed.");
                },
                () =>
                {
                    ImGui.SetNextItemWidth(ImGuiUtils.ScaledFloat(200f));
                    modified |= ImGui.InputInt("##max_suggestions", ref plugin.Config.MaxSuggestions, 1);
                    modified |= GeneralUtils.ValidateMin(ref plugin.Config.MaxSuggestions, 1);
                    modified |= GeneralUtils.ValidateMax(ref plugin.Config.MaxSuggestions, 10);
                });

            // Suggestion Timeout
            addRow(() =>
                {
                    ImGui.Text("Suggestions Timeout");
                    ImGui.SameLine();
                    ImGuiEx.InfoMarker("The duration of a suggestion when an gesture is found.");
                },
                () =>
                {
                    ImGui.SetNextItemWidth(ImGuiUtils.ScaledFloat(200f));
                    modified |= ImGui.InputInt("##suggestion_timeout", ref plugin.Config.SuggestionTimeout, 1);
                    modified |= GeneralUtils.ValidateMin(ref plugin.Config.SuggestionTimeout, 1);
                    modified |= GeneralUtils.ValidateMax(ref plugin.Config.SuggestionTimeout, 300);
                });
            
            // Notification Sound
            addRow(() =>
                {
                    ImGui.Text("Notification Sound");
                    ImGui.SameLine();
                    ImGuiEx.InfoMarker("The sound to play when a new suggestion is found.");
                },
                () =>
                {
                    var values = Enumerable.Range(0, 17).ToArray();
                    var names = values.Select(n => n == 0 ? "No Sound" : $"Type {n}").ToArray(); 
                    var currentSound = (int) plugin.Config.NotificationSound;

                    ImGui.SetNextItemWidth(ImGuiUtils.ScaledFloat(200f));
                    if (ImGui.Combo("##notification_sound", ref currentSound, names, names.Length))
                    {
                        modified = true;
                        plugin.Config.NotificationSound = (uint) currentSound;

                        if (currentSound > 0)
                        {
                            UIGlobals.PlayChatSoundEffect(plugin.Config.NotificationSound);
                        }
                    }
                });
            
            // Suggestion Timeout
            addRow(() =>
                {
                    ImGui.Text("Keep Suggestions Open");
                    ImGui.SameLine();
                    ImGuiEx.InfoMarker("Keep the suggestions window open at all times.");
                },
                () =>
                {
                    modified |= ImGui.Checkbox("##keep_suggestions_open", ref plugin.Config.KeepSuggestionsOpen);
                });
            
            // Accept Suggestion Keybind
            addRow(() =>
                {
                    ImGui.Text("Accept Suggestion");
                    ImGui.SameLine();
                    ImGuiEx.InfoMarker(" This key will accept the top suggestion.");
                },
                () =>
                {
                    modified |= ImGuiUtils.DrawKeybindInput("##accept_suggestion_keybind", ref plugin.Config.AcceptSuggestionKeybind, ImGuiUtils.ScaledFloat(200f));
                });

            // Dismiss Suggestion Keybind
            addRow(() =>
                {
                    ImGui.Text("Dismiss Suggestion");
                    ImGui.SameLine();
                    ImGuiEx.InfoMarker("This key will dismiss the top suggestion.");
                },
                () =>
                {
                    modified |= ImGuiUtils.DrawKeybindInput("##dismiss_suggestion_keybind", ref plugin.Config.DismissSuggestionKeybind, ImGuiUtils.ScaledFloat(200f));
                });

            if (modified)
            {
                plugin.Config.MarkDirty();
            }
        });
    }
}