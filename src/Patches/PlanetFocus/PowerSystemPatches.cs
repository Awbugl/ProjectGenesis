using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;

namespace ProjectGenesis.Patches
{
    public static partial class PlanetFocusPatches
    {
        [HarmonyPatch(typeof(PowerSystem), nameof(PowerSystem.GameTick))]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> EnergyCap_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var matcher = new CodeMatcher(instructions);

            // wind
            matcher.MatchForward(false,
                new CodeMatch(OpCodes.Call,
                    AccessTools.Method(typeof(PowerGeneratorComponent), nameof(PowerGeneratorComponent.EnergyCap_Wind))));

            var comp = matcher.InstructionAt(-2);
            
            matcher.Advance(1).InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_0)).InsertAndAdvance(comp)
               .InsertAndAdvance(new CodeInstruction(OpCodes.Call,
                    AccessTools.Method(typeof(PlanetFocusPatches), nameof(EnergyCap_Patch))));
            
            //pv
            matcher.MatchForward(false,
                new CodeMatch(OpCodes.Call,
                    AccessTools.Method(typeof(PowerGeneratorComponent), nameof(PowerGeneratorComponent.EnergyCap_PV))));

            comp = matcher.InstructionAt(-8);

            matcher.Advance(1).InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_0)).InsertAndAdvance(comp)
               .InsertAndAdvance(new CodeInstruction(OpCodes.Call,
                    AccessTools.Method(typeof(PlanetFocusPatches), nameof(EnergyCap_Patch))));

            //fuel
            matcher.MatchForward(false,
                new CodeMatch(OpCodes.Call,
                    AccessTools.Method(typeof(PowerGeneratorComponent), nameof(PowerGeneratorComponent.EnergyCap_Fuel))));

            comp = matcher.InstructionAt(-1);

            matcher.Advance(1).InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_0)).InsertAndAdvance(comp)
               .InsertAndAdvance(new CodeInstruction(OpCodes.Call,
                    AccessTools.Method(typeof(PlanetFocusPatches), nameof(EnergyCap_Patch))));

            return matcher.InstructionEnumeration();
        }

        public static long EnergyCap_Patch(long power, PowerSystem powerSystem, ref PowerGeneratorComponent component)
        {
            int focusId;
            double extra;

            if (component.wind)
            {
                focusId = 6525;
                extra = 1.2;
            }
            else if (component.photovoltaic)
            {
                focusId = 6526;
                extra = 1.2;
            }
            else
            {
                switch (component.fuelMask)
                {
                    case 1:
                        focusId = 6524;
                        extra = 1.2;
                        break;

                    case 2:
                        focusId = 6529;
                        extra = 1.2;
                        break;

                    case 16:
                        focusId = 6527;
                        extra = 1.1;
                        break;

                    default: return power;
                }
            }

            if (!ContainsFocus(powerSystem.factory.planetId, focusId)) return power;

            var capacityCurrentTick = (long)(power * extra);

            component.capacityCurrentTick = capacityCurrentTick;

            return capacityCurrentTick;
        }
    }
}
