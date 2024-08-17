using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;

namespace ProjectGenesis.Patches.Logic
{
    public static class GeothermalStrengthPatches
    {
        [HarmonyPatch(typeof(PowerSystem), nameof(PowerSystem.CalculateGeothermalStrenth))]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> SetTargetCargoBytes_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var matcher = new CodeMatcher(instructions);

            matcher.MatchForward(false, new CodeMatch(OpCodes.Ldc_R4, 3f));
            
            matcher.SetOperandAndAdvance(2f);

            return matcher.InstructionEnumeration();
        }
    }
}