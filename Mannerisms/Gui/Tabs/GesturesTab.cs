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
        using var tab = ImRaii.TabItem("Gestures##gestures_tab");
        if (!tab) return;

        using var child = ImRaii.Child("##gestures_tab_child");
        if (!child) return;
        
        DrawCharactersList();
        ImGui.SameLine();
        DrawCharacterGestures();
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
        ImGuiUtils.Spacer();
    }

    private void DrawCharactersList()
    {
        using var group = ImRaii.Group();
        using var child = ImRaii.Child("##character_select", ImGuiHelpers.ScaledVector2(240f, 0), true);
        if (!child) return;
        
        if (plugin.Config.CharactersList.Count == 0)
        {
            _selectedCharacter = null;
            _selectedCharacterData = null;
        }

        foreach (var character in plugin.Config.CharactersList)
        {
            if (ImGui.Selectable(character.Key, _selectedCharacter == character.Key))
            {
                _selectedCharacter = character.Key;
                _selectedCharacterData = character.Value;
            }

            {
                if (ImGui.IsItemClicked(ImGuiMouseButton.Right))
                {
                    ImGui.OpenPopup($"##context_character_{character.Key}");
                }

                using var popup = ImRaii.Popup($"##context_character_{character.Key}");
                if (popup)
                {
                    if (ImGui.Selectable("Copy character data"))
                    {
                        var json = JsonConvert.SerializeObject(character.Value, Formatting.None);
                        ImGui.SetClipboardText(json);
                        ChatUtils.Print("Character data copied to clipboard.");
                    }

                    if (ImGui.Selectable("Paste character data"))
                    {
                        try
                        {
                            var json = ImGui.GetClipboardText();
                            var imported = JsonConvert.DeserializeObject<CharacterData>(json);
                            if (imported != null)
                            {
                                plugin.Config.ReplaceCharacterData(character.Value, imported);
                                plugin.Config.MarkDirty();
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
        }
    }

    private void DrawCharacterGestures()
    {
        using var child = ImRaii.Child("##character_gestures", Vector2.Zero, true);
        if (!child) return;
        
        if (_selectedCharacter == null || _selectedCharacterData == null)
        {
            ImGui.TextWrapped("Select a character from the list.");
            return;
        }
        
        // Button: Copy Character
        {
            using (ImRaii.PushFont(UiBuilder.IconFont))
            {
                if (ImGui.Button($"{(char)FontAwesomeIcon.Copy}##copy_character_data",  ImGuiHelpers.ScaledVector2(26f)))
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
        
        ImGui.SameLine(0, ImGuiUtils.ScaledFloat(4f));

        // Button: Paste Character
        {
            using (ImRaii.Disabled(!ImGui.GetIO().KeyShift))
            {
                using (ImRaii.PushFont(UiBuilder.IconFont))
                {
                    if (ImGui.Button($"{(char)FontAwesomeIcon.Paste}##paste_character_data", ImGuiHelpers.ScaledVector2(26f)) && ImGui.GetIO().KeyShift)
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

        ImGuiUtils.Spacer();

        using var tabBar = ImRaii.TabBar("##character_gestures_tabs");
        if (!tabBar) return;

        DrawGestureSubTab("Common Gestures", "1", DrawCommonGesturesTab);
        DrawGestureSubTab("Simple Gestures", "2", DrawSimpleGesturesEditorTab);
        DrawGestureSubTab("Advanced Gestures", "3", DrawAdvancedGesturesEditorTab);
    }
    
    private void DrawGestureSubTab(string label, string id, Action drawContent)
    {
        using var tab = ImRaii.TabItem($"{label}##character_gestures_tab_{id}");
        if (!tab) return;

        using var child = ImRaii.Child($"##character_gestures_tab_{id}_child", ImGui.GetContentRegionAvail());
        if (!child) return;

        drawContent();
        DrawTargetingInformation();
    }
    
    private void DrawCommonGesturesTab()
    {
        if (_selectedCharacter == null || _selectedCharacterData == null)
        {
            return;
        }

        ImGuiUtils.Spacer();
        ImGui.TextWrapped("Common gestures are pre-made patterns that cover everyday chat reactions. " +
            "Toggle the ones you want, and your character will perform the emote whenever you type a matching phrase.");
        ImGuiUtils.Spacer();
        
        // Checkbox: Suggest Emotions
        {
            plugin.Config.MarkDirtyIf(ImGui.Checkbox("Suggest Emotions##suggest_emotions", ref _selectedCharacterData.SuggestEmotions));
            ImGui.SameLine();
            ImGuiEx.InfoMarker($"Suggest emotions like /smile, /sad, /amazed{Environment.NewLine}for common emotes like :), :(, :o");
            ImGuiUtils.Spacer();
        }

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
            OnDrawColumn = (col, index, item) =>
            {
                var id = $"##common_gesture_{index}_{col}";
                var modified = false;
                
                var current = _selectedCharacterData.CommonGestures[item.Key];
                
                switch (col)
                {
                    case 0:
                        modified |= ImGui.Checkbox(id, ref current.Enabled);
                        if (ImGui.IsItemHovered())
                        {
                            ImGui.SetTooltip("Enabled?");
                        }
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
                        modified |= ImGuiUtils.DrawEmoteCombo( id, EEmoteComboType.Both, current.Command, ref current.Command);
                        break;
                }

                if (!modified) return item;

                plugin.Config.MarkDirty();

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

        ImGuiUtils.Spacer();
        ImGui.TextWrapped("Build gestures using comma-separated words. Tips:");
        ImGui.BulletText("A plus sign (+) means one or more of the previous character (e.g. bye+ matches byeee)");
        ImGui.BulletText("An asterisk (*) means zero or more of the previous character (e.g. hm?* matches hm and hm?)");
        ImGuiUtils.Spacer();

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
            OnAddNew = () =>
            {
                _selectedCharacterData.SimpleGestures.Add(new SimpleGesture());
            },
            OnDrawColumn = (col, index, gesture) =>
            {
                var id = $"##simple_gesture_{index}_{col}";
                var modified = false;
                
                switch (col)
                {
                    case 0:
                        modified |= ImGui.Checkbox(id, ref gesture.Enabled);
                        if (ImGui.IsItemHovered())
                        {
                            ImGui.SetTooltip($"Enabled?##{id}");
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
            OnModified = () =>
            {
                plugin.Config.MarkDirty();
            },
        });
    }

    private void DrawAdvancedGesturesEditorTab()
    {
        if (_selectedCharacter == null || _selectedCharacterData == null)
        {
            return;
        }

        ImGuiUtils.Spacer();
        ImGui.TextWrapped("Write your own Regular Expression (Regex) to match messages. " +
            "This gives you full control over pattern matching for complex or unusual triggers.");
        ImGuiUtils.Spacer();

        ImGuiUtils.DrawTable("AdvancedGesturesEditor", _selectedCharacterData.AdvancedGestures, new()
        {
            IsManaged = true,
            Columns = [
                ("##enabled", ImGuiUtils.CheckboxScale, "", false),
                ("Pattern", 300f, "", false),
                ("Case Sensitive", 110f, "", true),
                ("Needs Target (?)", 120f, $"Makes it so that the emote only plays if you have a target.{Environment.NewLine}See the help text above for more information.", true),
                ("Emote", -1, "", false),
            ],
            HasHeader = true,
            AddNewLabel = "Add Gesture",
            OnAddNew = () =>
            {
                _selectedCharacterData.AdvancedGestures.Add(new AdvancedGesture());
            },
            OnDrawColumn = (col, index, gesture) =>
            {
                var id = $"##advanced_gesture_{index}_{col}";
                var modified = false;
                
                switch (col)
                {
                    case 0:
                        modified |= ImGui.Checkbox(id, ref gesture.Enabled);
                        if (ImGui.IsItemHovered())
                        {
                            ImGui.SetTooltip($"Enabled?##{id}");
                        }
                        break;
                    case 1:
                        ImGuiUtils.StretchNext();
                        using (ImRaii.PushFont(UiBuilder.MonoFont))
                        {
                            modified |= ImGui.InputText(id, ref gesture.Pattern);
                        }
                        break;
                    case 2:
                        ImGuiUtils.CenterInColumn(ImGuiUtils.CheckboxScale);
                        modified |= ImGui.Checkbox(id, ref gesture.IsCaseSensitive);
                        break;
                    case 3:
                        ImGuiUtils.CenterInColumn(ImGuiUtils.CheckboxScale);
                        modified |= ImGui.Checkbox(id, ref gesture.IsTargetOnly);
                        break;
                    case 4:
                        modified |= ImGuiUtils.DrawEmoteCombo(id, EEmoteComboType.Both, gesture.Command, ref gesture.Command);
                        break;
                };
                
                if (!modified) return gesture;

                plugin.Config.MarkDirty();

                return gesture;
            },
            OnModified = () =>
            {
                plugin.Config.MarkDirty();
            },
        });
    }
}
