using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;

namespace ProjectGenesis.Patches
{
    public static class EnemyDropPatches
    {
        [HarmonyPatch(typeof(GameHistoryData), nameof(GameHistoryData.ItemCanDropByEnemy))]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> GameHistoryData_ItemCanDropByEnemy_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var matcher = new CodeMatcher(instructions);

            //unlock itemId 5205
            matcher.MatchForward(false, new CodeMatch(OpCodes.Ldarg_1), new CodeMatch(OpCodes.Ldc_I4, 5205));

            matcher.SetInstructionAndAdvance(new CodeInstruction(OpCodes.Nop)).SetInstructionAndAdvance(new CodeInstruction(OpCodes.Nop))
               .SetOpcodeAndAdvance(OpCodes.Br_S);

            return matcher.InstructionEnumeration();
        }

        [HarmonyPatch(typeof(UILootFilter), nameof(UILootFilter.SetDropRateTip))]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> UILootFilter_SetDropRateTip_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var matcher = new CodeMatcher(instructions);

            // dropCountText * 3
            matcher.MatchForward(false, new CodeMatch(OpCodes.Ldc_R4, 2f), new CodeMatch(OpCodes.Mul), new CodeMatch(OpCodes.Stloc_1))
               .SetOperandAndAdvance(6f);

            // dropCountMaxText * 3
            matcher.MatchForward(false, new CodeMatch(OpCodes.Ldc_R4, 2f), new CodeMatch(OpCodes.Mul), new CodeMatch(OpCodes.Stloc_2))
               .SetOperandAndAdvance(6f);

            return matcher.InstructionEnumeration();
        }

        [HarmonyPatch(typeof(EnemyDFGroundSystem), nameof(EnemyDFGroundSystem.RandomDropItemOnce))]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> EnemyDFGroundSystem_RandomDropItemOnce_Transpiler(
            IEnumerable<CodeInstruction> instructions)
        {
            var matcher = new CodeMatcher(instructions);

            // count * 3
            matcher.MatchForward(true, new CodeMatch(OpCodes.Ldarg_3), new CodeMatch(OpCodes.Ldloc_S), new CodeMatch(OpCodes.Ldloc_S),
                    new CodeMatch(OpCodes.Add), new CodeMatch(OpCodes.Conv_I4))
               .InsertAndAdvance(new CodeInstruction(OpCodes.Ldc_R8, 3d), new CodeInstruction(OpCodes.Mul));

            return matcher.InstructionEnumeration();
        }
    }
}
