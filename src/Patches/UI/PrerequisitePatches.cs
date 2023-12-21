using HarmonyLib;

namespace ProjectGenesis.Patches.UI
{
    public static class PrerequisitePatches
    {
        [HarmonyPatch(typeof(UITechNode), "DeterminePrerequisiteSuffice")]
        [HarmonyPatch(typeof(UITechNode), "HasPrerequisite")]
        [HarmonyPrefix]
        [HarmonyPriority(Priority.VeryHigh)]
        public static bool PrerequisiteLogic() => false;
    }
}
