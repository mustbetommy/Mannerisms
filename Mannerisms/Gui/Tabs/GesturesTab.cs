using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using ECommons.DalamudServices;
using ECommons.ImGuiMethods;
using Mannerisms.Data;
using Mannerisms.Util;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Numerics;

namespace Mannerisms.Gui.Tabs;

public class GesturesTab(Plugin plugin)
{
    private string? _selectedCharacter;
    private CharacterData? _selectedCharacterData;

    public void Draw()
    {
        if (ImGui.BeginTabItem("Gestures"))
        {
            ImGui.BeginChild("GesturesTabChild", ImGui.GetContentRegionAvail());
            DrawCharactersList();
            DrawCharacterGestures();
            ImGui.EndChild();
            ImGui.EndTabItem();
        }
    }

    private void DrawTargetingInformation()
    {
        ImGuiUtils.Separator();
        ImGui.TextColored(new Vector4(1f, 1f, 0.3f, 1f), "Information About Targeting");
        ImGui.TextWrapped("In order to maximize immersion and to prevent your character from emoting \"randomly\" while chatting from a distance, " +
            "gestures that are marked with \"Needs Target\" will only play under the following circumstances:");
        ImGui.BulletText("The message was sent via /say chat while you have a target.");
        ImGui.BulletText("The message was sent via /party or /alliance chat while you have a target that is in your party.");
        ImGui.BulletText("The message was sent via /tell while you are targeting the recipient.");
        ImGui.Dummy(new Vector2(0f, 4f));
    }

    private void DrawCharactersList()
    {
        ImGui.BeginGroup();
        if (ImGui.BeginChild("CharacterSelect", ImGuiHelpers.ScaledVector2(240f, 0), true))
        {
            if (plugin.Config.CharactersList.Count == 0)
            {
                _selectedCharacter = null;
                _selectedCharacterData = null;
            }

            var charFound = CharacterUtils.TryGetCurrent(out var charName, out var charWorld);

            foreach (var character in plugin.Config.CharactersList)
            {
                if (ImGui.Selectable(character.Key, _selectedCharacter == character.Key))
                {
                    _selectedCharacter = character.Key;
                    _selectedCharacterData = character.Value;
                }

                // Context Menu
                {
                    if (ImGui.IsItemClicked(ImGuiMouseButton.Right))
                    {
                        ImGui.OpenPopup($"##context_character_{character.Key}");
                    }

                    if (ImGui.BeginPopup($"##context_character_{character.Key}"))
                    {
                        if (ImGui.Selectable("Copy character data"))
                        {
                            var json = JsonConvert.SerializeObject(character.Value, Formatting.None);
                            ImGui.SetClipboardText(json);
                            ChatUtils.Print("Character data copied to clipboard.");
                        }
                        ImGui.EndPopup();

                        if (ImGui.Selectable("Paste character data"))
                        {
                            var json = JsonConvert.SerializeObject(character.Value, Formatting.None);
                            ImGui.SetClipboardText(json);
                            ChatUtils.Print("Character data copied to clipboard.");
                        }
                        ImGui.EndPopup();
                    }
                }
            }

            ImGui.EndChild();
        }
        ImGui.EndGroup();
    }

    private void DrawCharacterGestures()
    {
        ImGui.SameLine();
        if (ImGui.BeginChild("CharacterGesturesEditor", ImGuiHelpers.ScaledVector2(0), true))
        {
            if (_selectedCharacter == null || _selectedCharacterData == null)
            {
                ImGui.TextWrapped("Select a character from the list.");

            } else
            {
                // Button: Copy Character
                {
                    using (ImRaii.PushFont(UiBuilder.IconFont))
                    {
                        if (ImGui.Button($"{(char)FontAwesomeIcon.Copy}##copy_character", new Vector2(26f)))
                        {
                            var json = JsonConvert.SerializeObject(_selectedCharacterData, Formatting.None);
                            ImGui.SetClipboardText(json);
                            ChatUtils.Print("Character data copied to clipboard.");
                        }
                    }

                    if (ImGui.IsItemHovered())
                    {
                        ImGui.SetTooltip("Copy this character's data to your clipboard.");
                    }
                }

                // Button: Paste Character
                {
                    ImGui.SameLine(0, 4f);
                    using (ImRaii.PushFont(UiBuilder.IconFont))
                    {
                        using (ImRaii.Disabled(!ImGui.GetIO().KeyShift))
                        {
                            if (ImGui.Button($"{(char)FontAwesomeIcon.Paste}##copy_character", new Vector2(26f)) && ImGui.GetIO().KeyShift)
                            {
                                try
                                {
                                    var json = ImGui.GetClipboardText();
                                    var imported = JsonConvert.DeserializeObject<CharacterData>(json);
                                    if (imported != null)
                                    {
                                        _selectedCharacterData = plugin.Config.ReplaceCharacterData(_selectedCharacterData, imported);
                                        ChatUtils.Print("Character data imported from clipboard.");
                                    }
                                }
                                catch
                                {
                                    ChatUtils.Print("Clipboard does not contain valid character data.");
                                }
                            }
                        }
                    }

                    if (ImGui.IsItemHovered())
                    {
                        ImGui.SetTooltip($"Attempt to replace this character's data with the data from your clipboard.{Environment.NewLine}Hold SHIFT and click to paste.");
                    }
                }

                ImGui.Dummy(new Vector2(0f, 4f));

                ImGui.BeginTabBar("CharacterGesturesEditorTabs");
                if (ImGui.BeginTabItem("Common Gestures"))
                {
                    ImGui.BeginChild("##common_gestures_editor_tab", ImGui.GetContentRegionAvail());
                    DrawCommonGesturesTab();
                    DrawTargetingInformation();
                    ImGui.EndChild();
                    ImGui.EndTabItem();
                }

                if (ImGui.BeginTabItem("Simple Gestures"))
                {
                    ImGui.BeginChild("##simple_gestures_editor_tab", ImGui.GetContentRegionAvail());
                    DrawSimpleGesturesEditorTab();
                    DrawTargetingInformation();
                    ImGui.EndChild();
                    ImGui.EndTabItem();
                }

                if (ImGui.BeginTabItem("Advanced Gestures"))
                {
                    ImGui.BeginChild("##advanced_gestures_editor_tab", ImGui.GetContentRegionAvail());
                    DrawAdvancedGesturesEditorTab();
                    DrawTargetingInformation();
                    ImGui.EndChild();
                    ImGui.EndTabItem();
                }
                ImGui.EndTabBar();
            }
            ImGui.EndChild();
        }
    }

    private void DrawCommonGesturesTab()
    {
        if (_selectedCharacter == null || _selectedCharacterData == null)
        {
            return;
        }

        ImGui.Dummy(new Vector2(0f, 4f));
        ImGui.TextWrapped("Common gestures are pre-made patterns that cover everyday chat reactions. " +
            "Toggle the ones you want, and your character will perform the emote whenever you type a matching phrase.");
        ImGui.Dummy(new Vector2(0f, 4f));

        plugin.Config.MarkDirtyIf(_selectedCharacterData.VerifyCommonGestures());
        ImGuiUtils.DrawTable("##common_gestures", CommonGestureUtil.List.ToList(), new() {
            Columns =
            [
                ("##enabled", ImGuiUtils.CheckboxScale + 4f, "", false),
                ("Type", 100f, "", false),
                ("Examples", 180f, "", false),
                ("Needs Target (?)", 120f, $"Makes it so that the emote only plays if you have a target.{Environment.NewLine}See the help text below for more information.", true),
                ("Emote", -1, "", false),
            ],
            HasHeader = true,
            onDrawColumn = (col, index, item) =>
            {
                var current = _selectedCharacterData.CommonGestures[item.Key];
                switch (col)
                {
                    case 0:
                        plugin.Config.MarkDirtyIf(ImGui.Checkbox($"##common_gesture_{item.Key}", ref current.Enabled));
                        break;

                    case 1:
                        ImGui.Text(item.Value.Label);
                        break;

                    case 2:
                        ImGuiEx.Text(item.Value.ExamplesString);
                        break;
                    
                    case 3:
                        var text = item.Value.IsTargetOnly ? "" : "";
                        ImGuiUtils.CenterInColumn(ImGui.CalcTextSize(text).X);
                        ImGuiEx.Text(text);
                        break;

                    case 4:
                        plugin.Config.MarkDirtyIf(
                            ImGuiUtils.DrawEmoteCombo( $"##common_gesture_emote_{item.Key}", EEmoteComboType.Both, current.Command, ref current.Command));
                        break;
                }

                return item;
            },
        });

        ImGui.Spacing();
    }

    private void DrawSimpleGesturesEditorTab()
    {
        if (_selectedCharacter == null || _selectedCharacterData == null)
        {
            return;
        }

        ImGui.Dummy(new Vector2(0f, 4f));
        ImGui.TextWrapped("Build gestures using comma-separated words. Tips:");
        ImGui.BulletText("A plus sign (+) means one or more of the previous character (e.g. bye+ matches byeee)");
        ImGui.BulletText("An asterisk (*) means zero or more of the previous character (e.g. hm?* matches hm and hm?)");
        ImGui.Dummy(new Vector2(0f, 4f));

        ImGuiUtils.DrawTable("SimpleGesturesEditor", _selectedCharacterData.SimpleGestures, new()
        {
            IsManaged = true,
            Columns = [
                ("##enabled", ImGuiUtils.CheckboxScale, "", false),
                ("Terms", 200f, "", false),
                ("Match", 120f, "", false),
                ("Case Sensitive", 110f, "", true),
                ("Needs Target (?)", 120f, $"Makes it so that the emote only plays if you have a target.{Environment.NewLine}See the help text below for more information.", true),
                ("Emote", -1, "", false),
            ],
            HasHeader = true,
            AddNewLabel = "Add Gesture",
            onAddNew = () =>
            {
                _selectedCharacterData.SimpleGestures.Add(new SimpleGesture());
            },
            onDrawColumn = (col, index, gesture) =>
            {
                var id = $"##SimpleGesture_{index}_{col}";
                var modified = false;
                
                switch (col)
                {
                    case 0:
                        modified |= ImGui.Checkbox(id, ref gesture.Enabled);
                        if (ImGui.IsItemHovered())
                        {
                            ImGui.SetTooltip("Enabled?");
                        }
                        break;
                    case 1:
                        ImGuiUtils.StretchNext();
                        using (ImRaii.PushFont(UiBuilder.MonoFont))
                        {
                            modified |= ImGui.InputText(id, ref gesture.Terms);
                        }
                        break;
                    case 2:
                        var matchTypeValues = Enum.GetValues<ESimpleGesturePatternMatchType>();
                        var matchTypeNames = matchTypeValues.Select(v => v.GetDescription()).ToArray();
                        var matchTypeIndex = Array.IndexOf(matchTypeValues, gesture.MatchType);

                        ImGuiUtils.StretchNext();
                        if (ImGui.Combo(id, ref matchTypeIndex, matchTypeNames, matchTypeNames.Length))
                        {
                            gesture.MatchType = matchTypeValues[matchTypeIndex];
                            modified = true;
                        }
                        break;
                    case 3:
                        ImGuiUtils.CenterInColumn(ImGuiUtils.CheckboxScale);
                        modified |= ImGui.Checkbox(id, ref gesture.IsCaseSensitive);
                        break;
                    case 4:
                        ImGuiUtils.CenterInColumn(ImGuiUtils.CheckboxScale);
                        modified |= ImGui.Checkbox(id, ref gesture.IsTargetOnly);
                        break;
                    case 5:
                        modified |= ImGuiUtils.DrawEmoteCombo(id, EEmoteComboType.Both, gesture.Command, ref gesture.Command);
                        break;
                };

                if (!modified) return gesture;
                
                gesture.GeneratePattern();
                plugin.Config.MarkDirty();

                return gesture;
            },
            onModified = () =>
            {
                plugin.Config.MarkDirty();
            },
        });

        ImGui.EndTabItem();
    }

    private void DrawAdvancedGesturesEditorTab()
    {
        if (_selectedCharacter == null || _selectedCharacterData == null)
        {
            return;
        }

        ImGui.Dummy(new Vector2(0f, 4f));
        ImGui.TextWrapped("Write your own Regular Expression (Regex) to match messages. " +
            "This gives you full control over pattern matching for complex or unusual triggers.");
        ImGui.Dummy(new Vector2(0f, 4f));

        ImGuiUtils.DrawTable("AdvancedGesturesEditor", _selectedCharacterData.AdvancedGestures, new()
        {
            IsManaged = true,
            Columns = [
                ("##enabled", ImGuiUtils.CheckboxScale, "", false),
                ("Pattern", -1f, "", false),
                ("Case Sensitive", 110f, "", true),
                ("Needs Target (?)", 120f, $"Makes it so that the emote only plays if you have a target.{Environment.NewLine}See the help text above for more information.", true),
                ("Emote", 150f, "", false),
            ],
            HasHeader = true,
            AddNewLabel = "Add Gesture",
            onAddNew = () =>
            {
                _selectedCharacterData.AdvancedGestures.Add(new AdvancedGesture());
            },
            onDrawColumn = (col, index, gesture) =>
            {
                var id = $"##Gesture_{index}_{col}";
                switch (col)
                {
                    case 0:
                        plugin.Config.MarkDirtyIf(ImGui.Checkbox(id, ref gesture.Enabled));
                        break;
                    case 1:
                        ImGuiUtils.StretchNext();
                        using (ImRaii.PushFont(UiBuilder.MonoFont))
                        {
                            plugin.Config.MarkDirtyIf(ImGui.InputText(id, ref gesture.Pattern));
                        }
                        break;
                    case 2:
                        ImGuiUtils.CenterInColumn(ImGuiUtils.CheckboxScale);
                        plugin.Config.MarkDirtyIf(ImGui.Checkbox(id, ref gesture.IsCaseSensitive));
                        break;
                    case 3:
                        ImGuiUtils.CenterInColumn(ImGuiUtils.CheckboxScale);
                        plugin.Config.MarkDirtyIf(ImGui.Checkbox(id, ref gesture.IsTargetOnly));
                        break;
                    case 4:
                        plugin.Config.MarkDirtyIf(ImGuiUtils.DrawEmoteCombo(id, EEmoteComboType.Both, gesture.Command, ref gesture.Command));
                        break;
                };

                return gesture;
            },
            onModified = () =>
            {
                plugin.Config.MarkDirty();
            },
        });
    }
}
