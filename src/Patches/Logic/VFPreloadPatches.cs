using HarmonyLib;

// ReSharper disable InconsistentNaming
// ReSharper disable RedundantAssignment

namespace ProjectGenesis.Patches.Logic
{
    public static class VFPreloadPatches
    {
        [HarmonyPatch(typeof(VFPreload), nameof(VFPreload.IsSplashSolid))]
        [HarmonyPriority(Priority.VeryHigh)]
        [HarmonyPrefix]
        public static bool VFPreload_IsSplashSolid(ref bool __result)
        {
            __result = true;

            return false;
        }
    }
}
