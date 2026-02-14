using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using ProjectGenesis.Utils;

namespace ProjectGenesis.Patches
{
    public static class BirthStarPatches
    {
        [HarmonyPatch(typeof(StarGen), nameof(StarGen.CreateStarPlanets))]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> StarGen_CreateStarPlanets_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var matcher = new CodeMatcher(instructions);

            /*
                else { Array.Clear((Array) StarGen.pGas, 0, StarGen.pGas.Length); ... ]

                IL_066e: br           IL_0d25 // inited, goto final gen
                IL_0673: ldsfld       float64[] StarGen::pGas
             */

            matcher.MatchForward(false, CodeMatchUtils.Br,
                new CodeMatch(OpCodes.Ldsfld, AccessTools.Field(typeof(StarGen), nameof(StarGen.pGas))));

            var finalLabel = matcher.Operand;


            /*
                { ... StarGen.pGas[2] = 0.0; } else if (star.spectr == ESpectrType.M)

                IL_06c4: br           IL_0bb5  // BirthStar pGas inited, goto core gen
                IL_06c9: ldarg.1      // star
                IL_06ca: ldfld        valuetype ESpectrType StarData::spectr
                IL_06cf: brtrue       IL_077d // check not ESpectrType.M
             */

            matcher.MatchForward(false, CodeMatchUtils.Br, new CodeMatch(OpCodes.Ldarg_1),
                new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(StarData), nameof(StarData.spectr))), CodeMatchUtils.BrTrue);

            matcher.Operand = finalLabel;

            // insert ModifyBirthStar call
            matcher.InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_0), new CodeInstruction(OpCodes.Ldarg_1),
                new CodeInstruction(OpCodes.Ldloc_0), // dotNet35Random2
                new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(BirthStarPatches), nameof(ModifyBirthStar))));

            return matcher.InstructionEnumeration();
        }

        public static void ModifyBirthStar(GalaxyData galaxy, StarData star, DotNet35Random dotNet35Random2)
        {
            star.planetCount = 5;
            star.planets = new PlanetData[star.planetCount];

            // ReSharper disable JoinDeclarationAndInitializer
            int infoSeed;
            int genSeed;

            infoSeed = dotNet35Random2.Next();
            genSeed = dotNet35Random2.Next();
            star.planets[0] = PlanetGen.CreatePlanet(galaxy, star, new int[] { 9, }, 0, 0, 1, 1, false, infoSeed, genSeed);

            infoSeed = dotNet35Random2.Next();
            genSeed = dotNet35Random2.Next();
            star.planets[1] = PlanetGen.CreatePlanet(galaxy, star, new int[] { 21, }, 1, 0, 2, 2, true, infoSeed, genSeed);

            infoSeed = dotNet35Random2.Next();
            genSeed = dotNet35Random2.Next();
            star.planets[2] = PlanetGen.CreatePlanet(galaxy, star, new int[] { 1, }, 2, 2, 1, 1, false, infoSeed, genSeed);

            infoSeed = dotNet35Random2.Next();
            genSeed = dotNet35Random2.Next();
            star.planets[3] = PlanetGen.CreatePlanet(galaxy, star, new int[] { 7, }, 3, 2, 2, 2, false, infoSeed, genSeed);

            infoSeed = dotNet35Random2.Next();
            genSeed = dotNet35Random2.Next();
            star.planets[4] = PlanetGen.CreatePlanet(galaxy, star, new int[] { 20, }, 4, 0, 3, 3, false, infoSeed, genSeed);
        }
    }
}
