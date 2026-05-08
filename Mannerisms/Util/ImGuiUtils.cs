using Dalamud.Bindings.ImGui;
using Dalamud.Game.ClientState.Keys;
using Dalamud.Interface.Utility.Raii;
using ECommons;
using ECommons.DalamudServices;
using ECommons.ImGuiMethods;
using System;
using System.Collections.Generic;
using System.Numerics;
using Dalamud.Interface.Utility;

namespace Mannerisms.Util;

public enum EEmoteComboType
{
    Emote,
    Expression,
    Both,
}

public class TableOptions<T>
{
    public bool IsManaged = false;

    public required List<(string Name, float Width, string Hint, bool Centered)> Columns;
    public bool HasHeader = true;
    public string AddNewLabel = "Add New";

    public required Func<int, int, T, T> OnDrawColumn;
    public Action? OnAddNew;
    public Action? OnModified;
}

public static class ImGuiUtils
{

    private static uint _activeKeyBindInputId;
    
    public static readonly float CheckboxScale = ScaledFloat(22f);
    public static Vector2 CheckboxSize = ImGuiHelpers.ScaledVector2(22f);

    public static void StretchNext() => ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X);
    public static void CenterInColumn(float width) => ImGui.SetCursorPosX(ImGui.GetCursorPosX() + (ImGui.GetColumnWidth() - width) / 2f);

    public static void Separator()
    {
        ImGui.Spacing();
        var pos = ImGui.GetCursorScreenPos();
        var width = ImGui.GetContentRegionAvail().X;
        var drawList = ImGui.GetWindowDrawList();
        drawList.AddLine(pos, pos + new Vector2(width, 0), ImGui.GetColorU32(ImGuiCol.Separator), 1f);
        ImGui.Spacing();
    }

    public static void Spacer()
    {
        ImGui.Dummy(ImGuiHelpers.ScaledVector2(0, 4f));
    }

    public static float ScaledFloat(float v) => v * ImGuiHelpers.GlobalScale;

    public static bool DrawEmoteCombo(string id, EEmoteComboType type, string currentSelection, ref string outSelection)
    {
        using var group = ImRaii.Group();
        
        var hasCurrentEmote = EmoteUtils.TryGetAny(currentSelection, out var currentEmote);
        if (hasCurrentEmote && ThreadLoadImageHandler.TryGetIconTextureWrap(currentEmote.Icon, false, out var iconPicture))
        {
            ImGui.Image(iconPicture.Handle, CheckboxSize);
            ImGui.SameLine();
        }

        StretchNext();

        using var combo = ImRaii.Combo($"##emote_combo_{id}", currentEmote?.Name, ImGuiComboFlags.HeightLargest);
        if (combo)
        {
            var searchInput = string.Empty;
            
            if (ImGui.IsWindowAppearing())
            {
                searchInput = string.Empty;
                ImGui.SetKeyboardFocusHere();
            }

            ImGui.SetNextItemWidth(Math.Max(ScaledFloat(200f), ImGui.GetContentRegionAvail().X));
            ImGui.InputTextWithHint($"##emote_combo_search_input_{id}", "Search...", ref searchInput, 40);

            using var searchResults = ImRaii.Child($"##emote_combo_search_results_{id}",
                new Vector2(ImGui.GetContentRegionAvail().X, ScaledFloat(300f)));
            
            if (searchResults)
            {
                using (ImRaii.PushColor(ImGuiCol.FrameBgHovered, ImGui.GetColorU32(ImGuiCol.ButtonHovered)))
                {
                    IEnumerable<KeyValuePair<string, CachedEmote>> list = [];

                    switch (type)
                    {
                        case EEmoteComboType.Emote: list = EmoteUtils.GetEmotesList(); break;
                        case EEmoteComboType.Expression: list = EmoteUtils.GetExpressionsList(); break;
                        case EEmoteComboType.Both: list = EmoteUtils.GetCombinedList(); break;
                    }

                    foreach (var emote in list)
                    {
                        if (!string.IsNullOrWhiteSpace(searchInput))
                        {
                            if (!emote.Value.Name.Contains(searchInput, StringComparison.InvariantCultureIgnoreCase)) continue;
                        }

                        ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X);
                        if (DrawEmoteComboItem(emote.Value.Icon, emote.Value.Name))
                        {
                            outSelection = emote.Value.Command;
                            ImGui.CloseCurrentPopup();
                        }
                    }
                }
            }
        }

        return outSelection != currentSelection;
    }

    private static bool DrawEmoteComboItem(uint icon, string text)
    {
        var itemPos = ImGui.GetCursorScreenPos();
        var itemSize = new Vector2(ImGui.GetTextLineHeight()) + ImGui.GetStyle().FramePadding * 2;
        var frameSize = new Vector2(ImGui.CalcItemWidth(), ImGui.GetFrameHeight());

        using (ImRaii.PushColor(ImGuiCol.FrameBg, ImGui.GetColorU32(ImGuiCol.FrameBgHovered), ImGui.IsMouseHoveringRect(itemPos, itemPos + frameSize)))
        {
            using var child = ImRaii.ChildFrame(ImGui.GetID($"##icon_text_{icon}_{text}"), frameSize);
            if (child)
            {
                var d = ImGui.GetWindowDrawList();
                var iconDisplay = Svc.Texture.GetFromGameIcon(icon).GetWrapOrDefault();
                if (iconDisplay != null) d.AddImage(iconDisplay.Handle, itemPos, itemPos + new Vector2(itemSize.Y));
                var textSize = ImGui.CalcTextSize(text);
                d.AddText(itemPos + new Vector2(itemSize.Y + ImGui.GetStyle().FramePadding.X, itemSize.Y / 2f - textSize.Y / 2f), ImGui.GetColorU32(ImGuiCol.Text), text);
            }
        }

        return ImGui.IsItemClicked();
    }

    public static bool DrawKeybindInput(string id, ref CustomKeybind currentKeybind, float width)
    {
        var iid = ImGui.GetID($"##{id}_keybind");
        var isListening = _activeKeyBindInputId == iid;

        var keybindLabel = currentKeybind.Key == VirtualKey.NO_KEY
            ? "None"
            : KeyStateUtils.GetKeyName(currentKeybind.Key);

        var buttonLabel = isListening
            ? "Press a key..."
            : keybindLabel;

        if (ImGui.Button($"{buttonLabel}##{id}_keybind", new Vector2(width, 0)))
        {
            _activeKeyBindInputId = iid;
        }

        if (ImGui.IsItemHovered())
        {
            ImGui.SetTooltip("Press key to bind. Press Backspace to clear.");
        }

        var modified = false;
        
        if (isListening)
        {
            var previousKey = currentKeybind.Key;
            foreach (var key in PluginService.KeyState.GetValidVirtualKeys())
            {
                if (!PluginService.KeyState[key]) continue;
                currentKeybind.Key = key == VirtualKey.BACK ? VirtualKey.NO_KEY : key;
                modified |= previousKey != currentKeybind.Key;
                _activeKeyBindInputId = 0;
            }
        }
        
        ImGui.SameLine();
        modified |= ImGui.Checkbox($"Ctrl##{id}_keybind_ctrl", ref currentKeybind.UseCtrl);
        ImGui.SameLine();
        modified |= ImGui.Checkbox($"Alt##{id}_keybind_alt", ref currentKeybind.UseAlt);
        ImGui.SameLine();
        modified |= ImGui.Checkbox($"Shift##{id}_keybind_shift", ref currentKeybind.UseShift);

        return modified;
    }

    public static void DrawKeyValueTable(string id, Action<Action<Action, Action>> drawRows)
    {
        using var padding = ImRaii.PushStyle(ImGuiStyleVar.CellPadding, ImGuiHelpers.ScaledVector2(0f, 4f));
        using var table = ImRaii.Table($"##key_value_table_{id}", 2);
        
        if (table)
        {
            ImGui.TableSetupColumn($"##key_value_table_field_{id}", ImGuiTableColumnFlags.WidthFixed, ScaledFloat(200f));
            ImGui.TableSetupColumn($"##key_value_table_value_{id}", ImGuiTableColumnFlags.WidthStretch);

            drawRows((drawLabel, drawValue) =>
            {
                ImGui.TableNextRow();
                ImGui.TableNextColumn();
                drawLabel();
                ImGui.TableNextColumn();
                drawValue();
            });
        }
    }

    public static void DrawTable<T>(string id, List<T> items, TableOptions<T> options)
    {
        var rowToDelete = -1;
        var rowToMoveUp = -1;
        
        using var padding = ImRaii.PushStyle(ImGuiStyleVar.CellPadding, ImGuiHelpers.ScaledVector2(2f, 2f));
        var columnCount = options.IsManaged ? options.Columns.Count + 1 : options.Columns.Count;
        
        using (var table = ImRaii.Table($"##custom_table_{id}", columnCount))
        {
            if (table)
            {
                if (options.IsManaged)
                {
                    ImGui.TableSetupColumn($"##custom_table_column_{id}_actions", ImGuiTableColumnFlags.WidthFixed,
                        ScaledFloat(3f * CheckboxScale + 8f));
                }

                foreach (var column in options.Columns)
                {
                    var columnFlags = column.Width < 0f
                        ? ImGuiTableColumnFlags.WidthStretch
                        : ImGuiTableColumnFlags.WidthFixed;
                    var columnWidth = ScaledFloat(Math.Abs(column.Width));
                    ImGui.TableSetupColumn($"{column.Name}##custom_table_column_{id}_{column.Name}", columnFlags,
                        columnWidth);
                }

                if (options.HasHeader)
                {
                    ImGui.TableNextRow(ImGuiTableRowFlags.Headers);
                    var colIndex = 0;

                    // Skip the actions column
                    if (options.IsManaged)
                    {
                        ImGui.TableSetColumnIndex(0);
                        ImGui.TableHeader("");
                        colIndex++;
                    }

                    foreach (var column in options.Columns)
                    {
                        ImGui.TableSetColumnIndex(colIndex);
                        if (column.Centered)
                        {
                            var textWidth = ImGui.CalcTextSize(column.Name).X;
                            CenterInColumn(textWidth);
                        }

                        ImGui.TableHeader(column.Name);

                        if (ImGui.IsItemHovered() && !column.Hint.IsNullOrEmpty())
                        {
                            ImGui.SetTooltip(column.Hint);
                        }

                        colIndex++;
                    }

                    ImGui.TableNextRow();
                    ImGui.TableNextColumn();
                    ImGui.Dummy(Vector2.Zero);
                }

                for (var i = 0; i < items.Count; i++)
                {
                    var item = items[i];

                    using var rowId = ImRaii.PushId($"##custom_table_row_{id}_{i}");
                    ImGui.TableNextRow();

                    // Column: Actions
                    if (options.IsManaged)
                    {
                        ImGui.TableNextColumn();
                        using var spacing = ImRaii.PushStyle(ImGuiStyleVar.ItemSpacing, ImGuiHelpers.ScaledVector2(4f));

                        // Button: Delete
                        using (ImRaii.Disabled(!ImGui.GetIO().KeyShift))
                        {
                            if (ImGui.Button($"X##custom_table_delete_{id}", ImGuiUtils.CheckboxSize) && ImGui.GetIO().KeyShift)
                            {
                                rowToDelete = i;
                            }
                        }

                        if (ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled) && !ImGui.GetIO().KeyShift)
                        {
                            ImGui.SetTooltip("Hold SHIFT to delete.");
                        }

                        // Button: Move Up
                        ImGui.SameLine();
                        using (ImRaii.Disabled(i <= 0))
                        {
                            if (ImGui.ArrowButton($"##custom_table_up_{id}", ImGuiDir.Up))
                            {
                                rowToMoveUp = i;
                            }
                        }

                        // Button: Move Down
                        ImGui.SameLine();
                        using (ImRaii.Disabled(i >= items.Count - 1))
                        {
                            if (ImGui.ArrowButton($"##custom_table_down_{id}", ImGuiDir.Down))
                            {
                                rowToMoveUp = i + 1;
                            }
                        }
                    }

                    // Custom Columns
                    for (var j = 0; j < options.Columns.Count; j++)
                    {
                        ImGui.TableNextColumn();
                        items[i] = options.OnDrawColumn(j, i, item);
                    }
                }
            }
        }

        // Possibly delete a gesture
        if (options.IsManaged)
        {
            if (rowToDelete >= 0)
            {
                items.RemoveAt(rowToDelete);
                options.OnModified?.Invoke();
            }

            // Possibly move a gesture
            if (rowToMoveUp > 0)
            {
                var move = items[rowToMoveUp];
                items.RemoveAt(rowToMoveUp);
                items.Insert(rowToMoveUp - 1, move);
                options.OnModified?.Invoke();
            }
        }

        // Button: Add New
        if (options.IsManaged)
        {
            if (items.Count > 0)
            {
                ImGui.Spacing();
            }

            if (ImGui.Button($"{options.AddNewLabel}##add_gesture"))
            {
                options.OnAddNew?.Invoke();
                options.OnModified?.Invoke();
            }
        }
    }
}
