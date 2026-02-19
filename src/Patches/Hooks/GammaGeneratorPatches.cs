using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;

namespace ProjectGenesis.Patches
{
    public static class GammaGeneratorPatches
    {
        [HarmonyPatch(typeof(PowerGeneratorComponent), nameof(PowerGeneratorComponent.GameTick_Gamma))]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> PowerGeneratorComponent_GameTick_Gamma_Transpiler(
            IEnumerable<CodeInstruction> instructions)
        {
            var matcher = new CodeMatcher(instructions);

            matcher.MatchForward(false,
                new CodeMatch(OpCodes.Ldfld,
                    AccessTools.Field(typeof(PowerGeneratorComponent), nameof(PowerGeneratorComponent.catalystPoint))),
                new CodeMatch(OpCodes.Ldc_I4_1), new CodeMatch(OpCodes.Sub));

            // catalystPoint cost * 10
            matcher.Advance(1).SetAndAdvance(OpCodes.Ldc_I4_S, (sbyte)10);

            matcher.MatchForward(false,
                new CodeMatch(OpCodes.Ldfld,
                    AccessTools.Field(typeof(PowerGeneratorComponent), nameof(PowerGeneratorComponent.catalystIncPoint))),
                new CodeMatch(OpCodes.Ldloc_2), new CodeMatch(OpCodes.Sub));

            // catalystIncPoint cost * 10
            matcher.Advance(2).InsertAndAdvance(new CodeInstruction(OpCodes.Ldc_I4_S, (sbyte)10), new CodeInstruction(OpCodes.Mul));

            return matcher.InstructionEnumeration();
        }

        [HarmonyPatch(typeof(UIPowerGeneratorWindow), nameof(UIPowerGeneratorWindow._OnUpdate))]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> UIPowerGeneratorWindow_OnUpdate_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var matcher = new CodeMatcher(instructions);

            matcher.MatchForward(false, new CodeMatch(OpCodes.Ldstr, "0.100 / min"));
            matcher.SetOperandAndAdvance("1 / min");

            matcher.MatchForward(false, new CodeMatch(OpCodes.Ldstr, "0.100 / min"));
            matcher.SetOperandAndAdvance("1 / min");

            return matcher.InstructionEnumeration();
        }
    }
}
