using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;

// ReSharper disable InconsistentNaming

namespace ProjectGenesis.Patches
{
    public static class PlanetThemePatches
    {
        [HarmonyPatch(typeof(PlanetGen), nameof(PlanetGen.SetPlanetTheme))]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> PlanetGen_SetPlanetTheme_ActiveGasItems_Transpiler(
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

        [HarmonyPatch(typeof(PlanetGen), nameof(PlanetGen.SetPlanetTheme))]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> PlanetGen_SetPlanetTheme_RemoveOcean_Transpiler(
            IEnumerable<CodeInstruction> instructions)
        {
            var matcher = new CodeMatcher(instructions);

            matcher.MatchForward(false, new CodeMatch(OpCodes.Ldloc_S),
                new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(ThemeProto), nameof(ThemeProto.PlanetType))),
                new CodeMatch(OpCodes.Ldc_I4_3));

            var themeProto = matcher.Operand;

            var label = matcher.Advance(3).Operand;

            matcher.Advance(1).InsertAndAdvance(new CodeInstruction(OpCodes.Ldloc_S, themeProto),
                new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(ThemeProto), nameof(ThemeProto.WaterItemId))),
                new CodeInstruction(OpCodes.Ldc_I4_0), new CodeInstruction(OpCodes.Bgt_S, label));

            matcher.MatchForward(false, new CodeMatch(OpCodes.Ldloc_S),
                new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(ThemeProto), nameof(ThemeProto.PlanetType))),
                new CodeMatch(OpCodes.Ldc_I4_3));

            themeProto = matcher.Operand;

            label = matcher.Advance(3).Operand;

            matcher.Advance(1).InsertAndAdvance(new CodeInstruction(OpCodes.Ldloc_S, themeProto),
                new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(ThemeProto), nameof(ThemeProto.WaterItemId))),
                new CodeInstruction(OpCodes.Ldc_I4_0), new CodeInstruction(OpCodes.Bgt_S, label));

            return matcher.InstructionEnumeration();
        }
    }
}
