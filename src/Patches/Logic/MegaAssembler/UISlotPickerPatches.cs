using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;

namespace ProjectGenesis.Patches.Logic.MegaAssembler
{
    internal static partial class MegaAssemblerPatches
    {
        private static readonly FieldInfo IsStationField = AccessTools.Field(typeof(PrefabDesc), nameof(PrefabDesc.isStation));
        private static readonly FieldInfo IsAssemblerField = AccessTools.Field(typeof(PrefabDesc), nameof(PrefabDesc.isAssembler));

        [HarmonyPatch(typeof(BuildTool_Path), "DeterminePreviews")]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> BuildTool_Path_DeterminePreviews_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var matcher = new CodeMatcher(instructions).MatchForward(false, new CodeMatch(OpCodes.Ldloc_S),
                                                                     new CodeMatch(OpCodes.Ldfld, IsStationField));

            var instruction = matcher.Instruction;

            matcher.Advance(2).InsertAndAdvance(new CodeInstruction(instruction), new CodeInstruction(OpCodes.Ldfld, IsAssemblerField),
                                                new CodeInstruction(OpCodes.Or));

            return matcher.InstructionEnumeration();
        }

        [HarmonyPatch(typeof(UISlotPicker), "Determine")]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> UISlotPicker_Determine_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var matcher = new CodeMatcher(instructions).MatchForward(false, new CodeMatch(OpCodes.Ldfld, EntityData_StationId_Field),
                                                                     new CodeMatch(OpCodes.Stloc_S), new CodeMatch(OpCodes.Ldloc_S),
                                                                     new CodeMatch(OpCodes.Ldc_I4_0), new CodeMatch(OpCodes.Ble));

            var pos = matcher.Pos;
            matcher.Advance(3).InsertAndAdvance(matcher.InstructionsInRange(pos - 5, pos - 1));
            matcher.InsertAndAdvance(new CodeInstruction(OpCodes.Ldfld, EntityData_AssemblerId_Field), new CodeInstruction(OpCodes.Or));
            return matcher.InstructionEnumeration();
        }
    }
}
