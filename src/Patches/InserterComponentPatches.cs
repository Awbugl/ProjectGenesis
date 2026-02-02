using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;

namespace ProjectGenesis.Patches
{
    public static class InserterComponentPatches
    {
        /// <summary>
        /// 白爪建筑说明里面的120改为240
        /// </summary>
        [HarmonyPatch(typeof(ItemProto), nameof(ItemProto.GetPropValue))]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> ItemProto_GetPropValue_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            //IL_0901: ldstr        "<color=#61D8FFB8>120"
            //改为
            //IL_0901: ldstr        "<color=#61D8FFB8>240"
            var matcher = new CodeMatcher(instructions);
            matcher.MatchForward(false, new CodeMatch(OpCodes.Ldstr, "<color=#61D8FFB8>120"));
            matcher.SetOperandAndAdvance("<color=#61D8FFB8>240");

            return matcher.InstructionEnumeration();
        }

        /// <summary>
        /// 白爪实时小窗口速率说明的120改为240
        /// </summary>
        [HarmonyPatch(typeof(UIInserterWindow), nameof(UIInserterWindow._OnUpdate))]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> UIInserterWindow__OnUpdate_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            //IL_042d: ldstr        "120"
            //改为
            //IL_042d: ldstr        "240"
            var matcher = new CodeMatcher(instructions);
            matcher.MatchForward(false, new CodeMatch(OpCodes.Ldstr, "120"));
            matcher.SetOperandAndAdvance("240");

            return matcher.InstructionEnumeration();
        }

        /// <summary>
        /// 提高循环次数，从2次改为4次以确保白爪的处理速度达到240/s
        /// </summary>
        [HarmonyPatch(typeof(InserterComponent), nameof(InserterComponent.InternalUpdate_Bidirectional))]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> InserterComponent_InternalUpdate_Bidirectional_Transpiler(
            IEnumerable<CodeInstruction> instructions)
        {
            var matcher = new CodeMatcher(instructions);

            //int num1 = 1; 改成 int num1 = 3;
            //num1 = 1 的时候是2次循环（1->0），num1 = 3 的时候是4次循环（3->2->1->0）
            matcher.MatchForward(false, new CodeMatch(OpCodes.Ldc_I4_1), new CodeMatch(OpCodes.Stloc_3));
            matcher.SetOpcodeAndAdvance(OpCodes.Ldc_I4_3);

            //int num6 = 1; 改成 int num6 = 3;
            matcher.MatchForward(false, new CodeMatch(OpCodes.Ldc_I4_1), new CodeMatch(OpCodes.Stloc_3));
            matcher.SetOpcodeAndAdvance(OpCodes.Ldc_I4_3);

            return matcher.InstructionEnumeration();
        }
    }
}
