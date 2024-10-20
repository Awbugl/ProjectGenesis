using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;

namespace ProjectGenesis.Patches.Logic
{
    public static class InserterComponentPatches
    {
        [HarmonyTranspiler]
        [HarmonyPatch(typeof(ItemProto), nameof(ItemProto.GetPropValue))]
        public static IEnumerable<CodeInstruction> ItemProto_GetPropValue_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var matcher = new CodeMatcher(instructions);
            matcher.MatchForward(false, new CodeMatch(OpCodes.Ldstr, "<color=#61D8FFB8>120"));
            matcher.SetOperandAndAdvance("<color=#61D8FFB8>240");

            return matcher.InstructionEnumeration();
        }

        [HarmonyTranspiler]
        [HarmonyPatch(typeof(UIInserterWindow), nameof(UIInserterWindow._OnUpdate))]
        public static IEnumerable<CodeInstruction> UIInserterWindow__OnUpdate_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var matcher = new CodeMatcher(instructions);
            matcher.MatchForward(false, new CodeMatch(OpCodes.Ldstr, "120"));
            matcher.SetOperandAndAdvance("240");

            return matcher.InstructionEnumeration();
        }

        [HarmonyTranspiler]
        [HarmonyPatch(typeof(InserterComponent), nameof(InserterComponent.InternalUpdate_Bidirectional))]
        public static IEnumerable<CodeInstruction> InserterComponent_InternalUpdate_Bidirectional_Transpiler(
            IEnumerable<CodeInstruction> instructions)
        {
            var matcher = new CodeMatcher(instructions);
            matcher.MatchForward(false, new CodeMatch(OpCodes.Ldc_I4_1));
            matcher.SetOpcodeAndAdvance(OpCodes.Ldc_I4_3);
            matcher.MatchForward(false, new CodeMatch(OpCodes.Ldc_I4_1), new CodeMatch(OpCodes.Stloc_3));
            matcher.SetOpcodeAndAdvance(OpCodes.Ldc_I4_3);

            return matcher.InstructionEnumeration();
        }
    }
}
