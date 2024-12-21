using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using ProjectGenesis.Utils;

// ReSharper disable InconsistentNaming

namespace ProjectGenesis.Patches
{
    public static class GoalLogicPatches
    {
        [HarmonyPatch(typeof(GD_GroupElectromagnetism), nameof(GD_GroupElectromagnetism.OnDetermineComplete))]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> GD_GroupElectromagnetism_OnDetermineComplete_Transpiler(
            IEnumerable<CodeInstruction> instructions)
        {
            var matcher = new CodeMatcher(instructions);

            matcher.MatchForward(false, new CodeMatch(OpCodes.Ldc_I4, 1001));

            matcher.SetOperandAndAdvance(ProtoID.T高效电浆控制);
            GD_GroupElectromagnetism
            return matcher.InstructionEnumeration();
        }
    }
}
