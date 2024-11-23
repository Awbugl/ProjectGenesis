using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;

namespace ProjectGenesis.Patches.Logic
{
    public static class SpaceMissilePatches
    {
        [HarmonyPatch(typeof(DefenseSystem), nameof(DefenseSystem.UpdateOtherGlobalTargets))]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> DefenseSystem_UpdateOtherGlobalTargets_Transpiler(
            IEnumerable<CodeInstruction> instructions)
        {
            var matcher = new CodeMatcher(instructions);

            matcher.MatchForward(false, new CodeMatch(OpCodes.Ldloc_0), new CodeMatch(OpCodes.Ldnull));

            matcher.SetInstructionAndAdvance(new CodeInstruction(OpCodes.Nop))
               .SetInstructionAndAdvance(new CodeInstruction(OpCodes.Nop))
               .SetInstructionAndAdvance(new CodeInstruction(OpCodes.Nop))
               .SetInstructionAndAdvance(new CodeInstruction(OpCodes.Nop));

            matcher.MatchForward(false, new CodeMatch(OpCodes.Ldloc_3), new CodeMatch(OpCodes.Ldloc_S), new CodeMatch(OpCodes.Ldelem_Ref));

            matcher.Advance(1).SetInstructionAndAdvance(new CodeInstruction(OpCodes.Nop))
               .SetInstructionAndAdvance(new CodeInstruction(OpCodes.Nop))
               .SetInstructionAndAdvance(new CodeInstruction(OpCodes.Nop))
               .SetInstructionAndAdvance(new CodeInstruction(OpCodes.Nop))
               .SetInstructionAndAdvance(new CodeInstruction(OpCodes.Nop))
               .SetInstructionAndAdvance(new CodeInstruction(OpCodes.Nop));

            return matcher.InstructionEnumeration();
        }
    }
}
