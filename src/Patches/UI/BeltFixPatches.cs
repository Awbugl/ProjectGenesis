using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;

// ReSharper disable InconsistentNaming

namespace ProjectGenesis.Patches
{
    public static class BeltFixPatches
    {
        private static readonly FieldInfo BeltComponent_Speed_Field = AccessTools.Field(typeof(BeltComponent), nameof(BeltComponent.speed));

        private static readonly FieldInfo PrefabDesc_beltSpeed_Field = AccessTools.Field(typeof(PrefabDesc), nameof(PrefabDesc.beltSpeed));

        [HarmonyPatch(typeof(ConnGizmoRenderer), nameof(ConnGizmoRenderer.Update))]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> ConnGizmoRenderer_Update_Transpiler(
            IEnumerable<CodeInstruction> instructions)
        {
            var matcher = new CodeMatcher(instructions);

            matcher.MatchForward(false, new CodeMatch(OpCodes.Ldfld, PrefabDesc_beltSpeed_Field));

            matcher.Advance(1).InsertAndAdvance(new CodeInstruction(OpCodes.Call,
                AccessTools.Method(typeof(BeltFixPatches), nameof(BeltSpeed_Patch))));

            return matcher.InstructionEnumeration();
        }

        [HarmonyPatch(typeof(CargoTraffic), nameof(CargoTraffic.SetBeltSelected))]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> CargoTraffic_SetBeltSelected_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var matcher = new CodeMatcher(instructions);

            matcher.MatchForward(false, new CodeMatch(OpCodes.Ldfld, BeltComponent_Speed_Field));

            matcher.Advance(1).InsertAndAdvance(new CodeInstruction(OpCodes.Call,
                AccessTools.Method(typeof(BeltFixPatches), nameof(BeltSpeed_Patch))));


            return matcher.InstructionEnumeration();
        }

        [HarmonyPatch(typeof(BuildTool_BlueprintCopy), nameof(BuildTool_BlueprintCopy.UpdatePreviewModels))]
        [HarmonyPatch(typeof(BuildTool_Path), nameof(BuildTool_Path.UpdateGizmos))]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> BuildTool_Path_UpdateGizmos_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var matcher = new CodeMatcher(instructions);

            matcher.MatchForward(false, new CodeMatch(OpCodes.Ldfld, PrefabDesc_beltSpeed_Field));

            matcher.Advance(1).InsertAndAdvance(new CodeInstruction(OpCodes.Call,
                AccessTools.Method(typeof(BeltFixPatches), nameof(BeltSpeed_Patch))));

            matcher.MatchForward(false, new CodeMatch(OpCodes.Ldfld, PrefabDesc_beltSpeed_Field));

            matcher.Advance(1).InsertAndAdvance(new CodeInstruction(OpCodes.Call,
                AccessTools.Method(typeof(BeltFixPatches), nameof(BeltSpeed_Patch))));

            return matcher.InstructionEnumeration();
        }

        [HarmonyPatch(typeof(CargoTraffic), nameof(CargoTraffic.AlterBeltRenderer))]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> CargoTraffic_AlterBeltRenderer_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var matcher = new CodeMatcher(instructions);

            matcher.MatchForward(false, new CodeMatch(OpCodes.Ldfld, BeltComponent_Speed_Field));

            matcher.Advance(1).InsertAndAdvance(new CodeInstruction(OpCodes.Call,
                AccessTools.Method(typeof(BeltFixPatches), nameof(BeltSpeed_Patch))));

            matcher.MatchForward(false, new CodeMatch(OpCodes.Ldfld, BeltComponent_Speed_Field));

            matcher.Advance(1).InsertAndAdvance(new CodeInstruction(OpCodes.Call,
                AccessTools.Method(typeof(BeltFixPatches), nameof(BeltSpeed_Patch))));


            return matcher.InstructionEnumeration();
        }

        [HarmonyPatch(typeof(PathRenderingBatch), nameof(PathRenderingBatch.AddNode))]
        [HarmonyPatch(typeof(PathRenderingBatch), nameof(PathRenderingBatch.AddNodeNoRefresh))]
        [HarmonyPrefix]
        public static void PathRenderingBatch_AddNode_Prefix(ref int speed) => speed = BeltSpeed_Patch(speed);

        public static int BeltSpeed_Patch(int beltSpeed)
        {
            switch (beltSpeed)
            {
                case 10: return 5;

                case 5: return 2;

                case 2: return 1;

                default: return beltSpeed;
            }
        }
    }
}
