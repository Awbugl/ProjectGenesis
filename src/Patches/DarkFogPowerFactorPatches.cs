using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;

namespace ProjectGenesis.Patches
{
    public static class DarkFogPowerFactorPatches
    {
        [HarmonyPatch(typeof(DFGBaseComponent), nameof(DFGBaseComponent.UpdateFactoryThreat))]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> DFGBaseComponent_UpdateFactoryThreat_Transpiler(
            IEnumerable<CodeInstruction> instructions)
        {
            var matcher = new CodeMatcher(instructions);

            matcher.End().MatchBack(false, new CodeMatch(OpCodes.Ldc_R8, 0.85));

            matcher.InsertAndAdvance(new CodeInstruction(OpCodes.Ldc_R8, 10.0), new CodeInstruction(OpCodes.Mul));

            return matcher.InstructionEnumeration();
        }
    }
}
