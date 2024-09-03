using HarmonyLib;
using SDG.Unturned;
using UnityEngine;

namespace RestoreMonarchy.BuildingRestrictions.Patches
{
    [HarmonyPatch(typeof(StructureManager))]
    static class StructureManagerPatches
    {
        [HarmonyPatch(nameof(StructureManager.destroyStructure), [typeof(StructureDrop), typeof(byte), typeof(byte), typeof(Vector3), typeof(bool)])]
        [HarmonyPostfix]
        static void destroyBarricadePostfix(StructureDrop structure)
        {
            BuildingRestrictionsPlugin.Instance.Database.RemovePlayerStructure(structure.model);
        }
    }
}
