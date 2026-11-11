using EFT;
using EFT.Builds;
using EFT.InventoryLogic;
using EFT.UI;
using BepInEx.Logging;
using SPT.Reflection.Patching;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace BetterLoadouts.Patches;

public class AdditiveApplyPatch : ModulePatch
{
    private static readonly ManualLogSource Log = BepInEx.Logging.Logger.CreateLogSource("BetterLoadouts.AdditiveApply");
    private static FieldInfo? _selectedBuildField;
    private static PropertyInfo? _equipmentProperty;

    protected override MethodBase GetTargetMethod()
    {
        var method = typeof(EquipmentBuildsScreen)
            .GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            .FirstOrDefault(m =>
                m.ReturnType == typeof(IEnumerable<string>) &&
                m.GetParameters().Length == 1 &&
                m.GetParameters()[0].ParameterType == typeof(EEquipmentBuildType));

        if (method == null)
        {
            Log.LogError("Could not find the EquipmentBuildsScreen ignore-list method.");
            throw new Exception("BetterLoadouts: could not find ignore-list method on EquipmentBuildsScreen (IEnumerable<string> with EEquipmentBuildType param)");
        }

        if (Plugin.DebugLogging.Value)
            Log.LogInfo($"Found ignore-list method: {method.DeclaringType?.FullName}.{method.Name}");

        _selectedBuildField = typeof(EquipmentBuildsScreen)
            .GetFields(BindingFlags.NonPublic | BindingFlags.Instance)
            .FirstOrDefault(f =>
                f.FieldType.GetProperty("Equipment")?.PropertyType == typeof(InventoryEquipment) &&
                f.FieldType.GetProperty("BuildType")?.PropertyType == typeof(EEquipmentBuildType));

        if (_selectedBuildField != null)
        {
            _equipmentProperty = _selectedBuildField.FieldType.GetProperty("Equipment");
            if (Plugin.DebugLogging.Value)
                Log.LogInfo($"Found selected-build field: {_selectedBuildField.Name} ({_selectedBuildField.FieldType.FullName})");
        }
        else
        {
            Log.LogWarning("Could not find the selected-build field; additive apply will be skipped.");
        }

        return method;
    }

    [PatchPostfix]
    public static void Postfix(
        ref IEnumerable<string> __result,
        EquipmentBuildsScreen __instance,
        EEquipmentBuildType buildType)
    {
        if (buildType == EEquipmentBuildType.Storage)
        {
            if (Plugin.DebugLogging.Value)
                Log.LogInfo("Skipping additive apply for a storage build.");
            return;
        }

        if (_selectedBuildField == null)
        {
            if (Plugin.DebugLogging.Value)
                Log.LogInfo($"Skipping additive apply for {buildType}: selected-build field was not found.");
            return;
        }

        var selectedBuild = _selectedBuildField.GetValue(__instance);
        if (selectedBuild == null)
        {
            if (Plugin.DebugLogging.Value)
                Log.LogInfo($"Skipping additive apply for {buildType}: no build is selected.");
            return;
        }

        var equipment = _equipmentProperty?.GetValue(selectedBuild) as InventoryEquipment;
        if (equipment == null)
        {
            Log.LogWarning($"Skipping additive apply for {buildType}: selected build has no equipment.");
            return;
        }

        var ignoreList = new List<string>(__result);
        var originalCount = ignoreList.Count;
        var addedSlots = new List<string>();
        var added = false;

        foreach (var slotName in InventoryEquipment.AllSlotNames)
        {
            var slotId = EnumExtensions.ToStringNoBox(slotName);
            if (ignoreList.Contains(slotId))
            {
                continue;
            }

            var slot = equipment.GetSlot(slotName);
            if (slot?.ContainedItem == null)
            {
                ignoreList.Add(slotId);
                addedSlots.Add(slotId);
                added = true;
            }
        }

        if (added)
        {
            __result = ignoreList;
        }

        if (Plugin.DebugLogging.Value)
            Log.LogInfo($"Additive apply for {buildType}: {originalCount} existing ignored slots, {addedSlots.Count} empty slots added: {(addedSlots.Count == 0 ? "none" : string.Join(", ", addedSlots))}.");
    }
}
