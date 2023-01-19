using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using HarmonyLib;
using ProjectGenesis.Utils;

namespace ProjectGenesis.Patches.Logic
{
    public static class ThermalPowerGenPatches
    {
        private static int _fuelNeed;

        [HarmonyPatch(typeof(PlanetFactory), "EntityFastFillIn")]
        [HarmonyPatch(typeof(PlanetFactory), "InsertInto")]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> PlanetFactory_InsertInto_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var matcher = new CodeMatcher(instructions);

            matcher.MatchForward(true, new CodeMatch(OpCodes.Ldsfld, AccessTools.Field(typeof(ItemProto), nameof(ItemProto.fuelNeeds))),
                                 new CodeMatch(OpCodes.Ldloc_S), new CodeMatch(OpCodes.Ldloc_S), new CodeMatch(OpCodes.Ldelema),
                                 new CodeMatch(OpCodes.Ldfld,
                                               AccessTools.Field(typeof(PowerGeneratorComponent), nameof(PowerGeneratorComponent.fuelMask))),
                                 new CodeMatch(OpCodes.Ldelem_Ref));

            matcher.Advance(1).InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_0),
                                                new CodeInstruction(OpCodes.Call,
                                                                    AccessTools.Method(typeof(ThermalPowerGenPatches),
                                                                                       nameof(ThermalPowerGen_InsertMethod))));

            return matcher.InstructionEnumeration();
        }

        [HarmonyPatch(typeof(UIPowerGeneratorWindow), "OnFuelButtonClick")]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> OnSpawnTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            var matcher = new CodeMatcher(instructions);

            matcher.MatchForward(true, new CodeMatch(OpCodes.Ldsfld, AccessTools.Field(typeof(ItemProto), nameof(ItemProto.fuelNeeds))),
                                 new CodeMatch(OpCodes.Ldloc_0),
                                 new CodeMatch(OpCodes.Ldfld,
                                               AccessTools.Field(typeof(PowerGeneratorComponent), nameof(PowerGeneratorComponent.fuelMask))),
                                 new CodeMatch(OpCodes.Ldelem_Ref));

            matcher.Advance(1).InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_0),
                                                new CodeInstruction(OpCodes.Call,
                                                                    AccessTools.Method(typeof(ThermalPowerGenPatches),
                                                                                       nameof(UIPowerGenerator_InsertMethod))));

            return matcher.InstructionEnumeration();
        }

        private static readonly int[] FuelRods = new[] { 1801, 6216, 6217, 6218 };

        public static int[] UIPowerGenerator_InsertMethod(int[] fuelNeeds, UIPowerGeneratorWindow window)
        {
            if (_fuelNeed == 0) _fuelNeed = ItemProto.fuelNeeds[1][0];

            if (fuelNeeds[0] == _fuelNeed && !window.factory.planet.gasItems.Contains(ProtoIDUsedByPatches.I氧))
            {
                var playerInhandItemId = window.player.inhandItemId;

                if (playerInhandItemId > 0 && window.player.inhandItemCount > 0)
                {
                    if (playerInhandItemId == 1000) return null;

                    if (FuelRods.Contains(playerInhandItemId)) return FuelRods;

                    if (fuelNeeds.Contains(playerInhandItemId))
                    {
                        UIRealtimeTip.Popup("需要氧气".TranslateFromJson());
                        return null;
                    }
                }
            }

            return fuelNeeds;
        }

        public static int[] ThermalPowerGen_InsertMethod(int[] fuelNeeds, PlanetFactory factory)
        {
            if (_fuelNeed == 0) _fuelNeed = ItemProto.fuelNeeds[1][0];

            return fuelNeeds[0] == _fuelNeed && !factory.planet.gasItems.Contains(ProtoIDUsedByPatches.I氧) ? FuelRods : fuelNeeds;
        }
    }
}
