using HarmonyLib;

// ReSharper disable InconsistentNaming
// ReSharper disable RedundantAssignment

namespace ProjectGenesis.Patches.UI
{
    public static class UIAbnormalityTipPatches
    {
        [HarmonyPatch(typeof(UIAbnormalityTip), "_OnInit")]
        [HarmonyPostfix]
        public static void UIAbnormalityTip__OnInit(
            ref UIAbnormalityTip __instance,
            ref bool ___isWarned,
            ref bool ___willClose,
            ref float ___closeDelayTime)
        {
            ___isWarned = true;
            ___willClose = true;
            __instance.mainTweener.Play1To0Continuing();
            ___closeDelayTime = 3f;
        }

        [HarmonyPatch(typeof(AbnormalityLogic), "GameTick")]
        [HarmonyPatch(typeof(AbnormalityLogic), "InitDeterminators")]
        [HarmonyPatch(typeof(AchievementLogic), "GameTick")]
        [HarmonyPatch(typeof(AchievementLogic), "InitAchievementDeterminators")]
        [HarmonyPrefix]
        [HarmonyPriority(Priority.VeryHigh)]
        public static bool Skip() => false;
    }
}
