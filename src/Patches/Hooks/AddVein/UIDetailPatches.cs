using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;

namespace ProjectGenesis.Patches
{
    public static partial class AddVeinPatches
    {
        [HarmonyPatch(typeof(UIPlanetDetail), nameof(UIPlanetDetail.OnPlanetDataSet))]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> OnPlanetDataSet_ChangeVeinData_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var matcher = new CodeMatcher(instructions);

            /*
                // [214 42 - 214 50]
                IL_0655: ldloc.s      index2
                IL_0657: ldc.i4.1
                IL_0658: add
                IL_0659: stloc.s      index2

                // [214 30 - 214 40]
                IL_065b: ldloc.s      index2
                IL_065d: ldc.i4.6
                IL_065e: blt          IL_05c2
             */

            matcher.MatchForward(false, new CodeMatch(OpCodes.Ldloc_S), new CodeMatch(OpCodes.Ldc_I4_6), new CodeMatch(OpCodes.Blt));

            object index = matcher.Operand;
            object jump = matcher.InstructionAt(2).operand;

            matcher.InsertAndAdvance(new CodeInstruction(OpCodes.Ldloc_S, index),
                new CodeInstruction(OpCodes.Call,
                    AccessTools.Method(typeof(AddVeinPatches), nameof(OnPlanetDataSet_ChangeVeinData_IndexPatches))),
                new CodeInstruction(OpCodes.Stloc_S, index), new CodeInstruction(OpCodes.Ldloc_S, index),
                new CodeInstruction(OpCodes.Ldc_I4_S, (sbyte)14), new CodeInstruction(OpCodes.Beq, jump));

            matcher.MatchForward(false, new CodeMatch(OpCodes.Ldloc_S), new CodeMatch(OpCodes.Ldc_I4_S, (sbyte)15),
                new CodeMatch(OpCodes.Blt));

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

            matcher.Advance(1).SetInstructionAndAdvance(new CodeInstruction(OpCodes.Call,
                    AccessTools.Method(typeof(AddVeinPatches), nameof(OnStarDataSet_ChangeVeinData_HighlightPatches))))
               .SetOpcodeAndAdvance(OpCodes.Brfalse);

            matcher.MatchForward(false, new CodeMatch(OpCodes.Ldloc_S), new CodeMatch(OpCodes.Ldc_I4_7), new CodeMatch(OpCodes.Clt));

            matcher.Advance(1).SetInstructionAndAdvance(new CodeInstruction(OpCodes.Call,
                    AccessTools.Method(typeof(AddVeinPatches), nameof(OnStarDataSet_ChangeVeinData_HighlightPatches))))
               .SetOpcodeAndAdvance(OpCodes.Nop);

            matcher.MatchForward(false, new CodeMatch(OpCodes.Ldloc_S), new CodeMatch(OpCodes.Ldc_I4_S, (sbyte)15),
                new CodeMatch(OpCodes.Blt));

            object index = matcher.Operand;

            matcher.InsertAndAdvance(new CodeInstruction(OpCodes.Ldloc_S, index),
                new CodeInstruction(OpCodes.Call,
                    AccessTools.Method(typeof(AddVeinPatches), nameof(OnStarDataSet_ChangeVeinData_IndexPatches))),
                new CodeInstruction(OpCodes.Stloc_S, index));

            matcher.Advance(1).SetOperandAndAdvance(VeinTypeCount);

            return matcher.InstructionEnumeration();
        }

        public static bool OnStarDataSet_ChangeVeinData_HighlightPatches(int index) => index < 7 || index == 15;

        public static int OnStarDataSet_ChangeVeinData_IndexPatches(int index)
        {
            switch (index)
            {
                case 3: return 15;
                case 16: return 3;
                case 15: return 16;
                default: return index;
            }
        }

        public static int OnPlanetDataSet_ChangeVeinData_IndexPatches(int index)
        {
            switch (index)
            {
                case 2: return 14;
                case 15: return 2;
                default: return index;
            }
        }

        [HarmonyPatch(typeof(UIPlanetDetail), nameof(UIPlanetDetail.RefreshDynamicProperties))]
        [HarmonyPatch(typeof(UIStarDetail), nameof(UIStarDetail.RefreshDynamicProperties))]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> RefreshDynamicProperties_ChangeVeinData_Transpiler(
            IEnumerable<CodeInstruction> instructions)
        {
            var matcher = new CodeMatcher(instructions);
            matcher.MatchForward(false, new CodeMatch(OpCodes.Ldstr, "未知珍奇信号"));

            object jmp = matcher.Advance(-2).Operand;

            CodeInstruction refId = matcher.Advance(-2).Instruction;
            CodeInstruction entry = matcher.Advance(-1).Instruction;

            matcher.InsertAndAdvance(new CodeInstruction(entry), new CodeInstruction(refId), new CodeInstruction(OpCodes.Ldc_I4, 15),
                new CodeInstruction(OpCodes.Beq, jmp));

            jmp = matcher.Advance(11).Operand;

            matcher.Advance(-3).InsertAndAdvance(new CodeInstruction(entry), new CodeInstruction(refId),
                new CodeInstruction(OpCodes.Ldc_I4, 15), new CodeInstruction(OpCodes.Beq, jmp));

            return matcher.InstructionEnumeration();
        }
    }
}
