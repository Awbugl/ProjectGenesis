using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;

// ReSharper disable InconsistentNaming

namespace ProjectGenesis.Patches.Logic
{
    public static class PlanetAlgorithmPatches
    {
        private static readonly FieldInfo PlanetAlgorithm_planet_FieldInfo = AccessTools.Field(typeof(PlanetAlgorithm), "planet");

        [HarmonyPatch(typeof(PlanetAlgorithm), nameof(PlanetAlgorithm.GenerateVeins))]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> PlanetAlgorithm1_GenerateVeins_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            CodeMatcher matcher = new CodeMatcher(instructions).MatchForward(true, new CodeMatch(OpCodes.Ldarg_0),
                                                                             new CodeMatch(OpCodes.Ldfld, PlanetAlgorithm_planet_FieldInfo),
                                                                             new CodeMatch(OpCodes.Ldfld,
                                                                                           AccessTools.Field(typeof(PlanetData),
                                                                                                             nameof(PlanetData.waterItemId))));

            object label = matcher.Advance(1).Operand;

            matcher.Advance(1).InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_0),
                                                new CodeInstruction(OpCodes.Ldfld, PlanetAlgorithm_planet_FieldInfo),
                                                new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(PlanetData), nameof(PlanetData.theme))),
                                                new CodeInstruction(OpCodes.Ldc_I4_1), new CodeInstruction(OpCodes.Beq_S, label));

            return matcher.InstructionEnumeration();
        }
    }
}
