using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;

namespace ProjectGenesis.Patches.Logic.PlanetFocus
{
    public static partial class PlanetFocusPatches
    {
        [HarmonyPatch(typeof(PowerSystem), "GameTick")]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> EnergyCap_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var matcher = new CodeMatcher(instructions);

            // wind
            matcher.MatchForward(false,
                                 new CodeMatch(OpCodes.Call,
                                               AccessTools.Method(typeof(PowerGeneratorComponent), nameof(PowerGeneratorComponent.EnergyCap_Wind))));
            matcher.Advance(1).InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_0));
            matcher.InsertAndAdvance(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(PlanetFocusPatches), nameof(EnergyCap_Wind))));

            //pv
            matcher.MatchForward(false,
                                 new CodeMatch(OpCodes.Call,
                                               AccessTools.Method(typeof(PowerGeneratorComponent), nameof(PowerGeneratorComponent.EnergyCap_PV))));
            matcher.Advance(1).InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_0));
            matcher.InsertAndAdvance(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(PlanetFocusPatches), nameof(EnergyCap_PV))));

            //fuel
            matcher.MatchForward(false, new CodeMatch(OpCodes.Ldarg_0), new CodeMatch(OpCodes.Ldfld), new CodeMatch(OpCodes.Ldloc_S),
                                 new CodeMatch(OpCodes.Ldelema),
                                 new CodeMatch(OpCodes.Call,
                                               AccessTools.Method(typeof(PowerGeneratorComponent), nameof(PowerGeneratorComponent.EnergyCap_Fuel))));

            List<CodeInstruction> ins = matcher.InstructionsWithOffsets(0, 3);

            matcher.Advance(5).InsertAndAdvance(ins);
            matcher.InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_0));
            matcher.InsertAndAdvance(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(PlanetFocusPatches), nameof(EnergyCap_Fuel))));

            return matcher.InstructionEnumeration();
        }

        public static long EnergyCap_Wind(long power, PowerSystem powerSystem)
        {
            var exist = ContainsFocus(powerSystem.factory.planetId, 6525);
            return exist ? (long)(power * 1.2) : power;
        }

        public static long EnergyCap_PV(long power, PowerSystem powerSystem)
        {
            var exist = ContainsFocus(powerSystem.factory.planetId, 6526);
            return exist ? (long)(power * 1.2) : power;
        }

        public static long EnergyCap_Fuel(long power, ref PowerGeneratorComponent component, PowerSystem powerSystem)
        {
            int focusid;
            double extra;

            switch (component.fuelMask)
            {
                case 1:
                    focusid = 6524;
                    extra = 1.2;
                    break;
                
                case 2:
                    focusid = 6529;
                    extra = 1.2;
                    break;

                case 16:
                    focusid = 6527;
                    extra = 1.1;
                    break;

                default:
                    return power;
            }

            var exist = ContainsFocus(powerSystem.factory.planetId, focusid);
            return exist ? (long)(power * extra) : power;
        }

        [HarmonyPatch(typeof(UIPowerGeneratorWindow), "_OnUpdate")]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> UIPowerGeneratorWindow_OnUpdate_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var matcher = new CodeMatcher(instructions);

            matcher.MatchForward(false, new CodeMatch(OpCodes.Ldloc_0),
                                 new CodeMatch(OpCodes.Ldfld,
                                               AccessTools.Field(typeof(PowerGeneratorComponent),
                                                                 nameof(PowerGeneratorComponent.capacityCurrentTick))));

            var comp = matcher.Instruction;

            matcher.Advance(2).InsertAndAdvance(comp);

            matcher.InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_0),
                                     new CodeInstruction(OpCodes.Ldfld,
                                                         AccessTools.Field(typeof(UIPowerGeneratorWindow), nameof(UIPowerGeneratorWindow.factory))));

            matcher.InsertAndAdvance(new CodeInstruction(OpCodes.Call,
                                                         AccessTools.Method(typeof(PlanetFocusPatches), nameof(UIPowerGeneratorWindow_OnUpdate))));
            
            return matcher.InstructionEnumeration();
        }

        public static long UIPowerGeneratorWindow_OnUpdate(long power, PowerGeneratorComponent component, PlanetFactory factory)
        {
            int focusid;
            double extra;

            if (component.wind)
            {
                focusid = 6525;
                extra = 1.2;
            }
            else if (component.photovoltaic)
            {
                focusid = 6526;
                extra = 1.2;
            }
            else
            {
                switch (component.fuelMask)
                {
                    case 1:
                        focusid = 6524;
                        extra = 1.2;
                        break;
                    
                    case 2:
                        focusid = 6529;
                        extra = 1.2;
                        break;

                    case 16:
                        focusid = 6527;
                        extra = 1.1;
                        break;

                    default:
                        return power;
                }
            }

            var exist = ContainsFocus(factory.planetId, focusid);
            return exist ? (long)(power * extra) : power;
        }
    }
}
