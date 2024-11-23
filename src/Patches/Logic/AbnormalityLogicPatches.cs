using System.Collections.Generic;
using HarmonyLib;

// ReSharper disable InconsistentNaming
// ReSharper disable RedundantAssignment

namespace ProjectGenesis.Patches.Logic
{
    public static class AbnormalityLogicPatches
    {
        [HarmonyPatch(typeof(UIAbnormalityTip), nameof(UIAbnormalityTip._OnInit))]
        [HarmonyPostfix]
        public static void UIAbnormalityTip_OnInit(UIAbnormalityTip __instance, ref bool ___isWarned, ref bool ___willClose,
            ref float ___closeDelayTime)
        {
            ___isWarned = true;
            ___willClose = true;
            __instance.mainTweener.Play1To0Continuing();
            ___closeDelayTime = 3f;
        }

        [HarmonyPatch(typeof(AbnormalityLogic), nameof(AbnormalityLogic.GameTick))]
        [HarmonyPatch(typeof(AbnormalityLogic), nameof(AbnormalityLogic.InitDeterminators))]
        [HarmonyPatch(typeof(MilkyWayWebClient), nameof(MilkyWayWebClient.SendUploadLoginRequest))]
        [HarmonyPatch(typeof(MilkyWayWebClient), nameof(MilkyWayWebClient.SendUploadRecordRequest))]
        [HarmonyPatch(typeof(STEAMX), nameof(STEAMX.UploadScoreToLeaderboard))]
        [HarmonyPrefix]
        [HarmonyPriority(Priority.VeryHigh)]
        public static bool Skip() => false;

        [HarmonyPatch(typeof(AbnormalityLogic), nameof(AbnormalityLogic.InitDeterminators))]
        [HarmonyPostfix]
        [HarmonyPriority(Priority.VeryHigh)]
        public static void InitDeterminators(AbnormalityLogic __instance)
        {
            if (__instance.determinators == null) __instance.determinators = new Dictionary<int, AbnormalityDeterminator>();
        }
    }
}
