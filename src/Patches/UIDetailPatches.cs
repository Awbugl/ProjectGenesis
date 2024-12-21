using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using ProjectGenesis.Utils;

namespace ProjectGenesis.Patches
{
    public static class UIDetailPatches
    {
        [HarmonyPatch(typeof(UIPlanetDetail), nameof(UIPlanetDetail.OnPlanetDataSet))]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> UIPlanetDetail_OnPlanetDataSet_ActiveGasItems_Transpiler(
            IEnumerable<CodeInstruction> instructions)
        {
            var matcher = new CodeMatcher(instructions);

            matcher.MatchForward(true, new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(PlanetData), nameof(PlanetData.type))),
                new CodeMatch(OpCodes.Ldc_I4_5), new CodeMatch(OpCodes.Beq));

            object label = matcher.Operand;

            matcher.MatchForward(false, new CodeMatch(OpCodes.Ldstr, "实际采集速度"));

            matcher.InstructionAt(-10).operand = label;

            matcher.MatchForward(false, new CodeMatch(OpCodes.Br)).SetOperandAndAdvance(label);

            return matcher.InstructionEnumeration();
        }

        [HarmonyPatch(typeof(UIStarDetail), nameof(UIStarDetail.OnStarDataSet))]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> UIStarDetail_OnStarDataSet_ActiveGasItems_Transpiler(
            IEnumerable<CodeInstruction> instructions)
        {
            var matcher = new CodeMatcher(instructions);

            matcher.MatchForward(false, new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(PlanetData), nameof(PlanetData.type))),
                new CodeMatch(OpCodes.Ldc_I4_5));
            matcher.Advance(-1);
            matcher.SetAndAdvance(OpCodes.Nop, null);
            matcher.SetAndAdvance(OpCodes.Nop, null);
            matcher.SetAndAdvance(OpCodes.Nop, null);
            matcher.SetAndAdvance(OpCodes.Nop, null);

            return matcher.InstructionEnumeration();
        }

        [HarmonyPatch(typeof(UIPlanetDetail), nameof(UIPlanetDetail.OnPlanetDataSet))]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> UIPlanetDetail_OnPlanetDataSet_FixCollectorSpeed_Transpiler(
            IEnumerable<CodeInstruction> instructions)
        {
            var matcher = new CodeMatcher(instructions);

            matcher.MatchForward(false,
                new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(StationComponent), nameof(StationComponent.collectionPerTick))));

            matcher.Advance(-2).InsertAndAdvance(new CodeInstruction(OpCodes.Call,
                AccessTools.Method(typeof(UIDetailPatches), nameof(GetMiningSpeedScale))));

            return matcher.InstructionEnumeration();
        }

        [HarmonyPatch(typeof(UIStarDetail), nameof(UIStarDetail.OnStarDataSet))]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> UIStarDetail_OnStarDataSet_FixCollectorSpeed_Transpiler(
            IEnumerable<CodeInstruction> instructions)
        {
            var matcher = new CodeMatcher(instructions);

            matcher.MatchForward(false,
                new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(StationComponent), nameof(StationComponent.collectionPerTick))));

            matcher.Advance(-2).InsertAndAdvance(new CodeInstruction(OpCodes.Call,
                AccessTools.Method(typeof(UIDetailPatches), nameof(GetMiningSpeedScale))));

            return matcher.InstructionEnumeration();
        }

        public static double GetMiningSpeedScale(double original) => GameMain.history.miningSpeedScale;

        [HarmonyPatch(typeof(UIPlanetDetail), nameof(UIPlanetDetail.OnPlanetDataSet))]
        [HarmonyPatch(typeof(UIStarDetail), nameof(UIStarDetail.OnStarDataSet))]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> OnDataSet_ChangeHighlightWaterId_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var matcher = new CodeMatcher(instructions);
            matcher.MatchForward(false, new CodeMatch(OpCodes.Ldc_I4, ProtoID.I水));
            matcher.SetOperandAndAdvance(ProtoID.I海水);

            matcher.MatchForward(false, new CodeMatch(OpCodes.Ldc_I4, ProtoID.I水));
            matcher.SetOperandAndAdvance(ProtoID.I海水);

            return matcher.InstructionEnumeration();
        }
    }
}
