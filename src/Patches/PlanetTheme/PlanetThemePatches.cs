using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using ProjectGenesis.Utils;

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
        public static IEnumerable<CodeInstruction> PlanetGen_SetPlanetTheme_FixBirthStarTheme_Transpiler(
            IEnumerable<CodeInstruction> instructions)
        {
            var matcher = new CodeMatcher(instructions);

            /*
                IL_0027: ldarg.1      // themeIds
                IL_0028: ldlen
                IL_0029: conv.i4
                IL_002a: stloc.1      // length1
             */

            matcher.MatchForward(false, new CodeMatch(OpCodes.Ldarg_1), new CodeMatch(OpCodes.Ldlen), new CodeMatch(OpCodes.Conv_I4),
                new CodeMatch(OpCodes.Stloc_1));

            matcher.Advance(1);
            matcher.InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_1),
                new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(PlanetThemePatches), nameof(FixBirthStarTheme))));

            return matcher.InstructionEnumeration();
        }

        public static void FixBirthStarTheme(int[] themeIds)
        {
            if (themeIds.Length == 1) PlanetGen.tmp_theme.AddRange(themeIds);
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

            object themeProto = matcher.Operand;

            object label = matcher.Advance(3).Operand;

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

        [HarmonyPatch(typeof(PlanetAlgorithm), nameof(PlanetAlgorithm.CalcWaterPercent))]
        [HarmonyPrefix]
        public static void PlanetAlgorithm_RegenerateTerrain(PlanetAlgorithm __instance)
        {
            PlanetData planet = __instance.planet;

            switch (planet.waterItemId)
            {
                case ProtoID.I盐酸:
                case ProtoID.I甲烷:
                    var planetAlgorithm3 = new PlanetAlgorithm3();
                    planetAlgorithm3.Reset(planet.seed, planet);
                    planetAlgorithm3.GenerateTerrain(planet.mod_x, planet.mod_y);
                    break;
            }
        }
    }
}
