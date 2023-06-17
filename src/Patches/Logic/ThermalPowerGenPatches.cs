using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using HarmonyLib;
using ProjectGenesis.Utils;

namespace ProjectGenesis.Patches.Logic
{
    public static class ThermalPowerGenPatches
    {
        [HarmonyPatch(typeof(PlanetFactory), "EntityFastFillIn")]
        [HarmonyPatch(typeof(PlanetFactory), "InsertInto")]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> PlanetFactory_InsertInto_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var matcher = new CodeMatcher(instructions);

            matcher.MatchForward(false, new CodeMatch(OpCodes.Ldsfld, AccessTools.Field(typeof(ItemProto), nameof(ItemProto.fuelNeeds))),
                                 new CodeMatch(OpCodes.Ldloc_S), new CodeMatch(OpCodes.Ldloc_S), new CodeMatch(OpCodes.Ldelema),
                                 new CodeMatch(OpCodes.Ldfld,
                                               AccessTools.Field(typeof(PowerGeneratorComponent), nameof(PowerGeneratorComponent.fuelMask))),
                                 new CodeMatch(OpCodes.Ldelem_Ref));

            matcher.SetAndAdvance(OpCodes.Nop, null);

            matcher.Advance(3).SetAndAdvance(OpCodes.Ldarg_0, null);

            matcher.SetAndAdvance(OpCodes.Call, AccessTools.Method(typeof(ThermalPowerGenPatches), nameof(ThermalPowerGen_InsertMethod)));

            return matcher.InstructionEnumeration();
        }

        public static int[] ThermalPowerGen_InsertMethod(ref PowerGeneratorComponent component, PlanetFactory factory)
        {
            var componentFuelMask = component.fuelMask;

            if (componentFuelMask == 5)
            {
                component.fuelMask = 2;
                return ItemProto.fuelNeeds[2];
            }

            if (componentFuelMask == 6)
            {
                component.fuelMask = 16;
                return ItemProto.fuelNeeds[16];
            }

            if (componentFuelMask != 1) return ItemProto.fuelNeeds[componentFuelMask];

            return factory.planet.gasItems.Contains(ProtoIDUsedByPatches.I氧) ? ItemProto.fuelNeeds[1] : FuelRods;
        }

        [HarmonyPatch(typeof(UIPowerGeneratorWindow), "OnFuelButtonClick")]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> OnSpawnTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            var matcher = new CodeMatcher(instructions);

            matcher.MatchForward(false, new CodeMatch(OpCodes.Ldsfld, AccessTools.Field(typeof(ItemProto), nameof(ItemProto.fuelNeeds))),
                                 new CodeMatch(OpCodes.Ldloc_0),
                                 new CodeMatch(OpCodes.Ldfld,
                                               AccessTools.Field(typeof(PowerGeneratorComponent), nameof(PowerGeneratorComponent.fuelMask))),
                                 new CodeMatch(OpCodes.Ldelem_Ref));

            matcher.SetAndAdvance(OpCodes.Nop, null);
            matcher.SetAndAdvance(OpCodes.Nop, null);
            matcher.SetAndAdvance(OpCodes.Ldarg_0, null);
            matcher.SetAndAdvance(OpCodes.Call, AccessTools.Method(typeof(ThermalPowerGenPatches), nameof(UIPowerGenerator_InsertMethod)));

            return matcher.InstructionEnumeration();
        }

        private static readonly int[] FuelRods = { 1801, 6216, 6217 };

        public static int[] UIPowerGenerator_InsertMethod(UIPowerGeneratorWindow window)
        {
            var component = window.powerSystem.genPool[window.generatorId];

            var componentFuelMask = component.fuelMask;

            if (componentFuelMask == 5)
            {
                component.fuelMask = 2;
                return ItemProto.fuelNeeds[2];
            }

            if (componentFuelMask == 6)
            {
                component.fuelMask = 16;
                return ItemProto.fuelNeeds[16];
            }

            if (componentFuelMask != 1) return ItemProto.fuelNeeds[componentFuelMask];

            if (window.factory.planet.gasItems.Contains(ProtoIDUsedByPatches.I氧)) return ItemProto.fuelNeeds[1];

            var playerInhandItemId = window.player.inhandItemId;

            if (playerInhandItemId > 0 && window.player.inhandItemCount > 0)
            {
                if (playerInhandItemId == 1000) return null;

                if (FuelRods.Contains(playerInhandItemId)) return FuelRods;

                if (ItemProto.fuelNeeds[1].Contains(playerInhandItemId))
                {
                    UIRealtimeTip.Popup("需要氧气".TranslateFromJson());
                    return null;
                }
            }

            return FuelRods;
        }
    }
}
