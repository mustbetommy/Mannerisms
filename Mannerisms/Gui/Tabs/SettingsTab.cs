using Dalamud.Bindings.ImGui;
using ECommons.ImGuiMethods;
using Mannerisms.Util;
using System.Numerics;

namespace Mannerisms.Gui.Tabs;

public class SettingsTab(Plugin plugin)
{
    public void Draw()
    {
        if (ImGui.BeginTabItem("Settings"))
        {
            ImGui.BeginChild("##SettingsTab", new Vector2(ImGui.GetContentRegionAvail().X, ImGui.GetContentRegionAvail().Y));
            ImGui.Spacing();

            ImGuiUtils.DrawKeyValueTable("##SettingsTable", (addRow) =>
            {
                // Max Suggestions
                addRow(() =>
                    {
                        ImGui.Text("Max Suggestions");
                        ImGui.SameLine();
                        ImGuiEx.InfoMarker("The maximum number of suggestions displayed.");
                    },
                    () =>
                    {
                        ImGui.SetNextItemWidth(200f);
                        plugin.Config.MarkDirtyIf(ImGui.InputInt("##max_suggestions", ref plugin.Config.MaxSuggestions, 1));
                        plugin.Config.MarkDirtyIf(GeneralUtils.ValidateMin(ref plugin.Config.MaxSuggestions, 1));
                        plugin.Config.MarkDirtyIf(GeneralUtils.ValidateMax(ref plugin.Config.MaxSuggestions, 10));
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
                        ImGui.SetNextItemWidth(200f);
                        plugin.Config.MarkDirtyIf(ImGui.InputInt("##suggestion_timeout", ref plugin.Config.SuggestionTimeout, 1));
                        plugin.Config.MarkDirtyIf(GeneralUtils.ValidateMin(ref plugin.Config.SuggestionTimeout, 1));
                        plugin.Config.MarkDirtyIf(GeneralUtils.ValidateMax(ref plugin.Config.SuggestionTimeout, 300));
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
                        plugin.Config.MarkDirtyIf(ImGui.Checkbox("##keep_suggestions_open", ref plugin.Config.KeepSuggestionsOpen));
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
                        plugin.Config.MarkDirtyIf(ImGuiUtils.DrawKeyBindInput("##accept_suggestion_keybind", ref plugin.Config.AcceptSuggestionKeybind, 200f));
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
                        plugin.Config.MarkDirtyIf(ImGuiUtils.DrawKeyBindInput("##dismiss_suggestion_keybind", ref plugin.Config.DismissSuggestionKeybind, 200f));
                    });
            });

            ImGui.EndChild();
            ImGui.EndTabItem();
        }
    }
}