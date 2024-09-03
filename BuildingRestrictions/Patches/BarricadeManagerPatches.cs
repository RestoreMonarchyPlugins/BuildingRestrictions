using HarmonyLib;
using SDG.Unturned;

namespace RestoreMonarchy.BuildingRestrictions.Patches
{
    [HarmonyPatch(typeof(BarricadeManager))]
    static class BarricadeManagerPatches
    {
        [HarmonyPatch(nameof(BarricadeManager.destroyBarricade), [typeof(BarricadeDrop), typeof(byte), typeof(byte), typeof(ushort)])]
        [HarmonyPostfix]
        static void destroyBarricadePostfix(BarricadeDrop barricade)
        {
            BuildingRestrictionsPlugin.Instance.Database.RemovePlayerBarricade(barricade.model);
        }
    }
}
