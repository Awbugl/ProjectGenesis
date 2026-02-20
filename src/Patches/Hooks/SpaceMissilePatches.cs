using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;

namespace ProjectGenesis.Patches
{
    /// <summary>
    /// 取消寻找星际目标时对卫星的限制
    /// </summary>
    public static class SpaceMissilePatches
    {
        [HarmonyPatch(typeof(DefenseSystem), nameof(DefenseSystem.UpdateOtherGlobalTargets))]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> DefenseSystem_UpdateOtherGlobalTargets_Transpiler(
            IEnumerable<CodeInstruction> instructions)
        {
            var matcher = new CodeMatcher(instructions);

            /*
                PlanetData orbitAroundPlanet = this.planet.orbitAroundPlanet;
                if (orbitAroundPlanet == null) return;

                IL_0015: ldarg.0      // this
                IL_0016: ldfld        class PlanetData DefenseSystem::planet
                IL_001b: ldfld        class PlanetData PlanetData::orbitAroundPlanet
                IL_0020: stloc.0      // orbitAroundPlanet

                // [815 5 - 815 35]
                IL_0021: ldloc.0      // orbitAroundPlanet
                IL_0022: ldnull
                IL_0023: cgt.un
                IL_0025: brfalse      IL_00fe
             */

            // 取消 orbitAroundPlanet != null 限制
            matcher.MatchForward(false, new CodeMatch(OpCodes.Ldloc_0), new CodeMatch(OpCodes.Ldnull));

            matcher.SetInstructionAndAdvance(new CodeInstruction(OpCodes.Nop)).SetInstructionAndAdvance(new CodeInstruction(OpCodes.Nop))
               .SetInstructionAndAdvance(new CodeInstruction(OpCodes.Nop)).SetInstructionAndAdvance(new CodeInstruction(OpCodes.Nop));

            /*
                if (planets[index1].orbitAroundPlanet == orbitAroundPlanet && planets[index1] != this.planet)

                IL_006e: ldloc.3      // planets
                IL_006f: ldloc.s      index1
                IL_0071: ldelem.ref
                IL_0072: ldfld        class PlanetData PlanetData::orbitAroundPlanet
                IL_0077: ldloc.0      // orbitAroundPlanet
                IL_0078: bne.un.s     IL_00f0
            */

            // 取消 planets[index1].orbitAroundPlanet == orbitAroundPlanet 限制
            matcher.MatchForward(false, new CodeMatch(OpCodes.Ldloc_3), new CodeMatch(OpCodes.Ldloc_S), new CodeMatch(OpCodes.Ldelem_Ref));

            matcher.Advance(1).SetInstructionAndAdvance(new CodeInstruction(OpCodes.Nop))
               .SetInstructionAndAdvance(new CodeInstruction(OpCodes.Nop)).SetInstructionAndAdvance(new CodeInstruction(OpCodes.Nop))
               .SetInstructionAndAdvance(new CodeInstruction(OpCodes.Nop)).SetInstructionAndAdvance(new CodeInstruction(OpCodes.Nop))
               .SetInstructionAndAdvance(new CodeInstruction(OpCodes.Nop));

            return matcher.InstructionEnumeration();
        }
    }
}
