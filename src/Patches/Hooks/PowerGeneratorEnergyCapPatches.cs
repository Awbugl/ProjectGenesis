using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;

// ReSharper disable InconsistentNaming

namespace ProjectGenesis.Patches
{
    public static class PowerGeneratorEnergyCapPatches
    {
        [HarmonyPatch(typeof(PowerGeneratorComponent), nameof(PowerGeneratorComponent.EnergyCap_Fuel))]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> PowerGeneratorComponent_EnergyCap_Fuel_Transpiler(
            IEnumerable<CodeInstruction> instructions)
        {
            var matcher = new CodeMatcher(instructions);


            matcher.MatchForward(false,
                new CodeMatch(OpCodes.Stfld,
                    AccessTools.Field(typeof(PowerGeneratorComponent), nameof(PowerGeneratorComponent.capacityCurrentTick))));
            matcher.Advance(1);
            matcher.InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Call,
                    AccessTools.Method(typeof(PowerGeneratorEnergyCapPatches), nameof(CapacityCurrentTickPatch))));

            return matcher.InstructionEnumeration();
        }

        public static void CapacityCurrentTickPatch(ref PowerGeneratorComponent component)
        {
            short curFuelId = component.curFuelId;
            if (curFuelId == 0) return;

            float reactorInc = LDB.items.Select(curFuelId).ReactorInc;
            if (reactorInc == 0) return;

            component.capacityCurrentTick += (long)(component.capacityCurrentTick * reactorInc);
        }
    }
}
