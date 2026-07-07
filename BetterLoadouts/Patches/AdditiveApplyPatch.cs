using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using EFT.Builds;
using EFT.InventoryLogic;
using EFT.UI;
using SPT.Reflection.Patching;

namespace BetterLoadouts.Patches;

public class AdditiveApplyPatch : ModulePatch
{
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
            throw new Exception("BetterLoadouts: could not find ignore-list method on EquipmentBuildsScreen (IEnumerable<string> with EEquipmentBuildType param)");

        _selectedBuildField = typeof(EquipmentBuildsScreen)
            .GetFields(BindingFlags.NonPublic | BindingFlags.Instance)
            .FirstOrDefault(f =>
                f.FieldType.GetProperty("Equipment")?.PropertyType == typeof(InventoryEquipment) &&
                f.FieldType.GetProperty("BuildType")?.PropertyType == typeof(EEquipmentBuildType));

        if (_selectedBuildField != null)
            _equipmentProperty = _selectedBuildField.FieldType.GetProperty("Equipment");

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
            return;
        }

        if (_selectedBuildField == null)
        {
            return;
        }

        var selectedBuild = _selectedBuildField.GetValue(__instance);
        if (selectedBuild == null)
        {
            return;
        }

        var equipment = _equipmentProperty?.GetValue(selectedBuild) as InventoryEquipment;
        if (equipment == null)
        {
            return;
        }

        var ignoreList = new List<string>(__result);
        var added = false;

        foreach (var slotName in InventoryEquipment.AllSlotNames)
        {
            var slotId = GClass867.ToStringNoBox(slotName);
            if (ignoreList.Contains(slotId))
            {
                continue;
            }

            var slot = equipment.GetSlot(slotName);
            if (slot?.ContainedItem == null)
            {
                ignoreList.Add(slotId);
                added = true;
            }
        }

        if (added)
        {
            __result = ignoreList;
        }
    }
}
