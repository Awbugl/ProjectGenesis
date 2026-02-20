using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;

// ReSharper disable InconsistentNaming

namespace ProjectGenesis.Patches
{
    public static class AssemblerComponent_InternalUpdate
    {
        [HarmonyPatch(typeof(FactorySystem), nameof(FactorySystem.GameTick))]
        [HarmonyPatch(typeof(GameLogic), nameof(GameLogic._assembler_parallel))]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> FactorySystem_GameTick_Transpiler(IEnumerable<CodeInstruction> instructions,
            MethodBase original)
        {
            var matcher = new CodeMatcher(instructions);

            /*
                // load factory
                factoryInstructions

                Ldloc_S local1
                Ldloc_S power1
                Call MegaAssembler_AssemblerComponent_InternalUpdate_Patch_Method

                IL_05de: ldloc.s      local
                IL_05e0: ldloc.s      power_V_40
                IL_05e2: ldloc.1      // productRegister
                IL_05e3: ldloc.2      // consumeRegister
                IL_05e4: call         instance unsigned int32 AssemblerComponent::InternalUpdate(float32, int32[], int32[])
                IL_05e9: stloc.s      _state_V_39
            */

            CodeInstruction[] factoryInstructions;

            switch (original.Name)
            {
                case "_assembler_parallel":
                    matcher.MatchForward(false, new CodeMatch(OpCodes.Ldloc_S),
                        new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(PlanetFactory), nameof(PlanetFactory.entityAnimPool))));
                    factoryInstructions = new[] { new CodeInstruction(matcher.Instruction), };
                    break;

                default:
                    factoryInstructions = new[]
                    {
                        new CodeInstruction(OpCodes.Ldarg_0),
                        new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(FactorySystem), nameof(FactorySystem.factory))),
                    };
                    break;
            }

            matcher.Start();

            // insert AssemblerComponent_InternalUpdate_Patch_Method
            while (true)
            {
                matcher.MatchForward(false,
                    new CodeMatch(OpCodes.Call, AccessTools.Method(typeof(AssemblerComponent), nameof(AssemblerComponent.InternalUpdate))));

                if (matcher.IsInvalid) break;

                var local1 = matcher.Advance(-4).Instruction;
                var power1 = matcher.InstructionAt(1);

                matcher.InsertAndAdvance(factoryInstructions);

                matcher.InsertAndAdvance(new CodeInstruction(local1), new CodeInstruction(power1),
                    new CodeInstruction(OpCodes.Call,
                        AccessTools.Method(typeof(AssemblerComponent_InternalUpdate),
                            nameof(GameTick_AssemblerComponent_InternalUpdate_PrePatch))));

                matcher.Advance(5);
            }

            return matcher.InstructionEnumeration();
        }

        public static void GameTick_AssemblerComponent_InternalUpdate_PrePatch(PlanetFactory factory, ref AssemblerComponent component,
            float power)
        {
            MegaAssemblerPatches.GameTick_AssemblerComponent_InternalUpdate_Patch(factory, ref component, power);
            NegentropySmelterPatches.GameTick_AssemblerComponent_InternalUpdate_Patch(factory, ref component, power);
            PlanetFocusPatches.GetWorkEnergyPerTick(factory, ref component);
        }
    }
}
