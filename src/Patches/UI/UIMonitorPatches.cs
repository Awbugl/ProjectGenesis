using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;

// ReSharper disable InconsistentNaming

namespace ProjectGenesis.Patches.UI
{
    public static class UIMonitorPatches
    {
        [HarmonyPatch(typeof(UIMonitorWindow), "_OnInit")]
        [HarmonyPostfix]
        public static void UIMonitorWindow__OnInit(ref UIMonitorWindow __instance) => __instance.cargoFlowSlider.maxValue = 600;

        [HarmonyPatch(typeof(MonitorComponent), "SetTargetCargoBytes")]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> SetTargetCargoBytes_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var matcher = new CodeMatcher(instructions);

            matcher.MatchForward(false, new CodeMatch(OpCodes.Ldc_I4, 72000));
            matcher.SetOperandAndAdvance(144000);

            matcher.MatchForward(false, new CodeMatch(OpCodes.Ldc_I4, 72000));
            matcher.SetOperandAndAdvance(144000);

            return matcher.InstructionEnumeration();
        }

        [HarmonyPatch(typeof(UIMonitorWindow), "OnPeriodValueChange")]
        [HarmonyPatch(typeof(UIMonitorWindow), "RefreshMonitorWindow")]
        [HarmonyPatch(typeof(UIMonitorWindow), "OnCargoFlowValueChange")]
        [HarmonyPatch(typeof(UIMonitorWindow), "OnCargoFlowInputFieldChange")]
        [HarmonyPatch(typeof(UIMonitorWindow), "OnCargoFlowInputFieldEndEdit")]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> OnCargoFlow_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var matcher = new CodeMatcher(instructions);

            matcher.MatchForward(false, new CodeMatch(OpCodes.Ldc_R4, 120f));
            matcher.SetOperandAndAdvance(240f);

            return matcher.InstructionEnumeration();
        }
    }
}
