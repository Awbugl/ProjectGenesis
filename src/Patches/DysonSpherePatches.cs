using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;

namespace ProjectGenesis.Patches
{
    public static class DysonSpherePatches
    {
        [HarmonyPatch(typeof(DysonFrame), nameof(DysonFrame.segCount), MethodType.Getter)]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> DysonFrame_segCount_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var matcher = new CodeMatcher(instructions);

            matcher.MatchForward(false, new CodeMatch(OpCodes.Ldc_I4_2), new CodeMatch(OpCodes.Mul), new CodeMatch(OpCodes.Stloc_0));

            matcher.SetInstructionAndAdvance(new CodeInstruction(OpCodes.Ldc_I4_4))
               .SetInstructionAndAdvance(new CodeInstruction(OpCodes.Div));

            return matcher.InstructionEnumeration();
        }

        [HarmonyPatch(typeof(DysonSphere), nameof(DysonSphere.Init))]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> DysonSphere_Init_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var matcher = new CodeMatcher(instructions);

            matcher.MatchForward(false,
                new CodeMatch(OpCodes.Stfld, AccessTools.Field(typeof(DysonSphere), nameof(DysonSphere.energyGenPerSail))));

            matcher.InsertAndAdvance(new CodeInstruction(OpCodes.Ldc_I8, (long)8), new CodeInstruction(OpCodes.Mul));

            matcher.MatchForward(false,
                new CodeMatch(OpCodes.Stfld, AccessTools.Field(typeof(DysonSphere), nameof(DysonSphere.energyGenPerShell))));

            matcher.InsertAndAdvance(new CodeInstruction(OpCodes.Ldc_I8, (long)8), new CodeInstruction(OpCodes.Mul));

            return matcher.InstructionEnumeration();
        }
    }
}
