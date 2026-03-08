using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;

namespace ProjectGenesis.Patches
{
    /// <summary>
    /// 护盾填充度显示由四舍五入调整为截断
    /// </summary>
    public static class UIPlanetShieldDetailPatches
    {
        [HarmonyTranspiler]
        [HarmonyPatch(typeof(UIPlanetShieldDetail), nameof(UIPlanetShieldDetail.Refresh))]
        [HarmonyPatch(typeof(UIZS_PlanetaryShieldInfo), nameof(UIZS_PlanetaryShieldInfo.Refresh))]
        [HarmonyPatch(typeof(UIFieldGeneratorWindow), nameof(UIFieldGeneratorWindow.Refresh))]
        public static IEnumerable<CodeInstruction> Refresh_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var matcher = new CodeMatcher(instructions);

            while (true)
            {
                matcher.MatchForward(true, new CodeMatch(OpCodes.Ldstr, "0.00%"), new CodeMatch(OpCodes.Call));

                if (matcher.IsInvalid) break;

                matcher.SetAndAdvance(OpCodes.Call, AccessTools.Method(typeof(UIPlanetShieldDetailPatches), nameof(TruncatePercent)));
            }

            return matcher.InstructionEnumeration();
        }

        public static string TruncatePercent(ref double value, string format)
        {
            var truncated = Math.Truncate(value * 10000.0) / 100.0;
            return truncated.ToString("0.00") + "%";
        }
    }
}
