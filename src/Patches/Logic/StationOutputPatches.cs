using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;

namespace ProjectGenesis.Patches.Logic
{
    public static class StationOutputPatches
    {
        [HarmonyPatch(typeof(StationComponent), nameof(StationComponent.UpdateOutputSlots))]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> UpdateOutputSlots_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var matcher = new CodeMatcher(instructions);

            matcher.MatchForward(false, new CodeMatch(OpCodes.Ldloc_S),
                                 new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(CargoPath), nameof(CargoPath.buffer))));

            matcher.SetInstructionAndAdvance(new CodeInstruction(OpCodes.Nop)).SetInstructionAndAdvance(new CodeInstruction(OpCodes.Nop))
                   .SetInstructionAndAdvance(new CodeInstruction(OpCodes.Nop)).SetInstructionAndAdvance(new CodeInstruction(OpCodes.Nop))
                   .SetInstructionAndAdvance(new CodeInstruction(OpCodes.Nop));

            return matcher.InstructionEnumeration();
        }
    }
}
