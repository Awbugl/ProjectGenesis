using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;

// ReSharper disable InconsistentNaming

namespace ProjectGenesis.Patches.UI.BeltColorFix
{
    public static class BeltFixPatches
    {
        private static readonly FieldInfo BeltComponent_Speed_Field = AccessTools.Field(typeof(BeltComponent), nameof(BeltComponent.speed));

        private static readonly FieldInfo PrefabDesc_beltSpeed_Field = AccessTools.Field(typeof(PrefabDesc), nameof(PrefabDesc.beltSpeed));

        [HarmonyPatch(typeof(CargoTraffic), nameof(CargoTraffic.AlterBeltRenderer))]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> AddColors(IEnumerable<CodeInstruction> instructions)
        {
            CodeMatcher matcher = new CodeMatcher(instructions).MatchForward(false, new CodeMatch(OpCodes.Ldloc_1),
                                                                             new CodeMatch(OpCodes.Ldfld, BeltComponent_Speed_Field),
                                                                             new CodeMatch(OpCodes.Ldc_I4_1));

            CodeMatcher matcher2 = matcher.Clone();
            matcher2.MatchForward(true, new CodeMatch(OpCodes.Ldloc_S), new CodeMatch(OpCodes.Stloc_S));

            object arg = matcher2.Operand;
            matcher2.Advance(1);
            object label = matcher2.Operand;

            matcher.Advance(2).InsertAndAdvance(new CodeInstruction(OpCodes.Ldloc_S, arg)).SetInstruction(
                Transpilers.EmitDelegate<Func<int, int, int>>((speed, other) =>
                {
                    if (speed == 10) return other + 8;
                    if (speed == 5) return other + 4;
                    return other;
                })).Advance(1).InsertAndAdvance(new CodeInstruction(OpCodes.Stloc_S, arg)).SetInstruction(new CodeInstruction(OpCodes.Br, label));

            return matcher.InstructionEnumeration();
        }

        [HarmonyPatch(typeof(ConnGizmoRenderer), nameof(ConnGizmoRenderer.Update))]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> BuildTool_BlueprintCopy_UpdatePreviewModels_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            CodeMatcher matcher = new CodeMatcher(instructions).MatchForward(false, new CodeMatch(OpCodes.Ldfld, PrefabDesc_beltSpeed_Field));

            matcher.Advance(1).InsertAndAdvance(
                new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(BeltFixPatches), nameof(BeltSpeed_Patch))));

            return matcher.InstructionEnumeration();
        }

        [HarmonyPatch(typeof(CargoTraffic), nameof(CargoTraffic.SetBeltSelected))]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> CargoTraffic_SetBeltSelected_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            CodeMatcher matcher = new CodeMatcher(instructions).MatchForward(false, new CodeMatch(OpCodes.Ldfld, BeltComponent_Speed_Field));

            matcher.Advance(1).InsertAndAdvance(
                new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(BeltFixPatches), nameof(BeltSpeed_Patch))));


            return matcher.InstructionEnumeration();
        }

        [HarmonyPatch(typeof(BuildTool_BlueprintPaste), nameof(BuildTool_BlueprintPaste.UpdatePreviewModels))]
        [HarmonyPatch(typeof(BuildTool_BlueprintCopy), nameof(BuildTool_BlueprintCopy.UpdatePreviewModels))]
        [HarmonyPatch(typeof(BuildTool_Path), nameof(BuildTool_Path.UpdateGizmos))]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> BuildTool_Path_UpdateGizmos_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            CodeMatcher matcher = new CodeMatcher(instructions).MatchForward(false, new CodeMatch(OpCodes.Ldfld, PrefabDesc_beltSpeed_Field));

            matcher.Advance(1).InsertAndAdvance(
                new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(BeltFixPatches), nameof(BeltSpeed_Patch))));

            matcher.MatchForward(false, new CodeMatch(OpCodes.Ldfld, PrefabDesc_beltSpeed_Field));

            matcher.Advance(1).InsertAndAdvance(
                new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(BeltFixPatches), nameof(BeltSpeed_Patch))));

            return matcher.InstructionEnumeration();
        }

        public static int BeltSpeed_Patch(int beltSpeed)
        {
            switch (beltSpeed)
            {
                case 10:
                    return 5;

                case 5:
                    return 2;

                case 3:
                    return 1;

                default:
                    return beltSpeed;
            }
        }
    }
}
