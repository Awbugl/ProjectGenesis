using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using ProjectGenesis.Utils;

namespace ProjectGenesis.Patches.Logic
{
    public static class MegaPumpPatches
    {
        [HarmonyPatch(typeof(BuildTool_Click), "CheckBuildConditions")]
        [HarmonyPatch(typeof(BuildTool_BlueprintPaste), "CheckBuildConditions")]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> BuildTool_Click_CheckBuildConditions_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var matcher = new CodeMatcher(instructions);
            while (true)
            {
                matcher.MatchForward(false, new CodeMatch(OpCodes.Ldloc_S), new CodeMatch(OpCodes.Ldc_I4_S, (sbyte)21),
                                     new CodeMatch(OpCodes.Stfld, AccessTools.Field(typeof(BuildPreview), nameof(BuildPreview.condition))));

                if (matcher.IsInvalid) break;

                var ins = matcher.Instruction;
                matcher.InsertAndAdvance(ins);
                matcher.Advance(1).SetAndAdvance(OpCodes.Call, AccessTools.Method(typeof(MegaPumpPatches), nameof(CheckBuildConditions)));
            }

            return matcher.InstructionEnumeration();
        }

        [HarmonyPatch(typeof(BuildTool_Addon), "CheckBuildConditions")]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> BuildTool_Addon_CheckBuildConditions_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var matcher = new CodeMatcher(instructions);
            while (true)
            {
                matcher.MatchForward(false, new CodeMatch(OpCodes.Ldarg_0),
                                     new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(BuildTool_Addon), nameof(BuildTool_Addon.handbp))),
                                     new CodeMatch(OpCodes.Ldc_I4_S, (sbyte)21),
                                     new CodeMatch(OpCodes.Stfld, AccessTools.Field(typeof(BuildPreview), nameof(BuildPreview.condition))));

                if (matcher.IsInvalid) break;

                var ins = matcher.Instruction;
                var ins2 = matcher.Advance(1).Instruction;
                matcher.Advance(1).InsertAndAdvance(ins);
                matcher.InsertAndAdvance(ins2);
                matcher.SetAndAdvance(OpCodes.Call, AccessTools.Method(typeof(MegaPumpPatches), nameof(CheckBuildConditions)));
            }

            return matcher.InstructionEnumeration();
        }

        public static int CheckBuildConditions(BuildPreview preview) => preview.item.ModelIndex == ProtoIDUsedByPatches.M大抽水机 ? 0 : 21;
    }
}
