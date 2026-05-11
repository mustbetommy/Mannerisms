using Dalamud.Game.ClientState.Conditions;
using ECommons.DalamudServices;
using Lumina.Excel.Sheets;
using System;
using System.Text.RegularExpressions;

namespace Mannerisms.Util;

public static partial class CharacterUtils
{
    private static bool _currentCharacterFound;

    private static string _currentName = string.Empty;
    private static string _currentWorld = string.Empty;

    private static void FetchCurrentCharacter()
    {
        if (_currentCharacterFound) return;
        if (PluginService.Objects.LocalPlayer == null) return;
        
        try
        {
            _currentName = PluginService.Objects.LocalPlayer?.Name.TextValue ?? string.Empty;
            var currentCharacterWorld = PluginService.Objects.LocalPlayer?.HomeWorld.RowId ?? 0;
            var currentCharacterWorldData = PluginService.Data.GetExcelSheet<World>().GetRowOrDefault(currentCharacterWorld);
            _currentWorld = currentCharacterWorldData?.Name.ExtractText() ?? $"UnknownWorld#{currentCharacterWorld}";
            _currentCharacterFound = true;
        } catch (Exception e)
        {
            Svc.Log.Error($"Error: {e.Message}");
        }
    }

    public static bool TryGetCurrent(out string name, out string world)
    {
        FetchCurrentCharacter();

        if (_currentCharacterFound)
        {
            name = _currentName;
            world = _currentWorld;
            return true;
        }

        name = null!;
        world = null!;
        return false;
    }

    public static bool TryGetTarget(out string target)
    {
        if (Svc.Targets.Target == null)
        {
            // No target.
            target = null!;
            return false;
        }

        target = SanitizeName(Svc.Targets.Target.Name.TextValue);
        return true;
    }

    public static bool IsAvailable()
    {
        return !Svc.Condition[ConditionFlag.InCombat] &&
               !Svc.Condition[ConditionFlag.Occupied] &&
               !Svc.Condition[ConditionFlag.Occupied30] &&
               !Svc.Condition[ConditionFlag.Occupied33] &&
               !Svc.Condition[ConditionFlag.Occupied38] &&
               !Svc.Condition[ConditionFlag.Occupied39] &&
               !Svc.Condition[ConditionFlag.OccupiedInCutSceneEvent] &&
               !Svc.Condition[ConditionFlag.OccupiedInEvent] &&
               !Svc.Condition[ConditionFlag.OccupiedInQuestEvent] &&
               !Svc.Condition[ConditionFlag.OccupiedSummoningBell] &&
               !Svc.Condition[ConditionFlag.WatchingCutscene] &&
               !Svc.Condition[ConditionFlag.WatchingCutscene78] &&
               !Svc.Condition[ConditionFlag.BetweenAreas] &&
               !Svc.Condition[ConditionFlag.BetweenAreas51] &&
               !Svc.Condition[ConditionFlag.TradeOpen] &&
               !Svc.Condition[ConditionFlag.Crafting] &&
               !Svc.Condition[ConditionFlag.ExecutingCraftingAction] &&
               !Svc.Condition[ConditionFlag.ExecutingGatheringAction] &&
               !Svc.Condition[ConditionFlag.PreparingToCraft] &&
               !Svc.Condition[ConditionFlag.Unconscious] &&
               !Svc.Condition[ConditionFlag.MeldingMateria] &&
               !Svc.Condition[ConditionFlag.Gathering] &&
               !Svc.Condition[ConditionFlag.OperatingSiegeMachine] &&
               !Svc.Condition[ConditionFlag.CarryingItem] &&
               !Svc.Condition[ConditionFlag.CarryingObject] &&
               !Svc.Condition[ConditionFlag.BeingMoved] &&
               !Svc.Condition[ConditionFlag.RidingPillion] &&
               !Svc.Condition[ConditionFlag.Mounting] &&
               !Svc.Condition[ConditionFlag.Mounting71] &&
               !Svc.Condition[ConditionFlag.ParticipatingInCustomMatch] &&
               !Svc.Condition[ConditionFlag.PlayingLordOfVerminion] &&
               !Svc.Condition[ConditionFlag.ChocoboRacing] &&
               !Svc.Condition[ConditionFlag.PlayingMiniGame] &&
               !Svc.Condition[ConditionFlag.Performing] &&
               !Svc.Condition[ConditionFlag.Fishing] &&
               !Svc.Condition[ConditionFlag.Transformed] &&
               !Svc.Condition[ConditionFlag.UsingHousingFunctions] &&
               Svc.Objects.LocalPlayer?.IsTargetable == true &&
               !Svc.ClientState.IsGPosing &&
               !Svc.ClientState.IsPvP;
    }

    public static bool IsCurrent(string name, string world)
    {
        return name == _currentName && world == _currentWorld;
    }

    public static string SanitizeName(string name) => SanitizeNameRegex().Replace(name, "").Trim();
    [GeneratedRegex(@"[^a-zA-Z' ]")]
    private static partial Regex SanitizeNameRegex();
}
