using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using HarmonyLib;

namespace ProjectGenesis.Patches.Logic.AddVein
{
    public static partial class AddVeinPatches
    {
        [HarmonyPatch(typeof(UIPlanetDetail), nameof(UIPlanetDetail.OnPlanetDataSet))]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> OnPlanetDataSet_ChangeVeinData_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var matcher = new CodeMatcher(instructions);

            matcher.MatchForward(false, new CodeMatch(OpCodes.Ldloc_S), new CodeMatch(OpCodes.Ldc_I4_6), new CodeMatch(OpCodes.Blt));

            object index = matcher.Operand;
            object jump = matcher.Advance(2).Operand;
            object endlabel = matcher.Advance(1).Labels.First();

            matcher.InsertAndAdvance(new CodeInstruction(OpCodes.Ldloc_S, index), new CodeInstruction(OpCodes.Ldc_I4, 15), new CodeInstruction(OpCodes.Beq, endlabel),
                new CodeInstruction(OpCodes.Ldc_I4, 14), new CodeInstruction(OpCodes.Stloc_S, index), new CodeInstruction(OpCodes.Br, jump));

            matcher.MatchForward(false, new CodeMatch(OpCodes.Ldloc_S), new CodeMatch(OpCodes.Ldc_I4_S, (sbyte)15), new CodeMatch(OpCodes.Blt));

            index = matcher.Operand;

            object addLabel = matcher.Clone().MatchBack(false, new CodeMatch(OpCodes.Bne_Un)).Operand;

            matcher.Advance(2).InsertAndAdvance(new CodeInstruction(OpCodes.Beq, addLabel), new CodeInstruction(OpCodes.Ldloc_S, index),
                new CodeInstruction(OpCodes.Ldc_I4_S, VeinTypeCount));

            return matcher.InstructionEnumeration();
        }

        [HarmonyPatch(typeof(UIStarDetail), nameof(UIStarDetail.OnStarDataSet))]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> OnStarDataSet_ChangeVeinData_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var matcher = new CodeMatcher(instructions);

            matcher.MatchForward(false, new CodeMatch(OpCodes.Ldloc_S), new CodeMatch(OpCodes.Ldc_I4_7), new CodeMatch(OpCodes.Bge));

            matcher.Advance(1)
                   .SetInstructionAndAdvance(new CodeInstruction(OpCodes.Call,
                        AccessTools.Method(typeof(AddVeinPatches), nameof(OnStarDataSet_ChangeVeinData_HighlightPatches)))).SetOpcodeAndAdvance(OpCodes.Brfalse);

            matcher.MatchForward(false, new CodeMatch(OpCodes.Ldloc_S), new CodeMatch(OpCodes.Ldc_I4_7), new CodeMatch(OpCodes.Clt));

            matcher.Advance(1)
                   .SetInstructionAndAdvance(new CodeInstruction(OpCodes.Call,
                        AccessTools.Method(typeof(AddVeinPatches), nameof(OnStarDataSet_ChangeVeinData_HighlightPatches)))).SetOpcodeAndAdvance(OpCodes.Nop);

            matcher.MatchForward(false, new CodeMatch(OpCodes.Ldloc_S), new CodeMatch(OpCodes.Ldc_I4_S, (sbyte)15), new CodeMatch(OpCodes.Blt));

            object index = matcher.Operand;

            matcher.InsertAndAdvance(new CodeInstruction(OpCodes.Ldloc_S, index),
                new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(AddVeinPatches), nameof(OnStarDataSet_ChangeVeinData_IndexPatches))),
                new CodeInstruction(OpCodes.Stloc_S, index));

            matcher.Advance(1).SetOperandAndAdvance(VeinTypeCount);

            return matcher.InstructionEnumeration();
        }

        public static bool OnStarDataSet_ChangeVeinData_HighlightPatches(int index) => index < 7 || index == 15;

        public static int OnStarDataSet_ChangeVeinData_IndexPatches(int index)
        {
            if (index == 7) return 15;

            if (index == 16) return 7;

            if (index == 15) return 16;

            return index;
        }

        [HarmonyPatch(typeof(UIPlanetDetail), nameof(UIPlanetDetail.RefreshDynamicProperties))]
        [HarmonyPatch(typeof(UIStarDetail), nameof(UIStarDetail.RefreshDynamicProperties))]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> RefreshDynamicProperties_ChangeVeinData_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var matcher = new CodeMatcher(instructions);
            matcher.MatchForward(false, new CodeMatch(OpCodes.Ldstr, "未知珍奇信号"));

            object jmp = matcher.Advance(-2).Operand;

            CodeInstruction refId = matcher.Advance(-2).Instruction;
            CodeInstruction entry = matcher.Advance(-1).Instruction;

            matcher.InsertAndAdvance(new CodeInstruction(entry), new CodeInstruction(refId), new CodeInstruction(OpCodes.Ldc_I4, 15),
                new CodeInstruction(OpCodes.Beq, jmp));

            jmp = matcher.Advance(11).Operand;

            matcher.Advance(-3).InsertAndAdvance(new CodeInstruction(entry), new CodeInstruction(refId), new CodeInstruction(OpCodes.Ldc_I4, 15),
                new CodeInstruction(OpCodes.Beq, jmp));

            return matcher.InstructionEnumeration();
        }
    }
}
