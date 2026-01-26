using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;

namespace ProjectGenesis.Patches
{
    public static partial class PlanetFocusPatches
    {
        private static readonly ConcurrentDictionary<short, long> ModelPowerCosts = new ConcurrentDictionary<short, long>();

        [HarmonyPatch(typeof(FactorySystem), nameof(FactorySystem.GameTick))]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> FactorySystem_GameTick_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var matcher = new CodeMatcher(instructions);
            
            var setPcState = AccessTools.Method(typeof(AssemblerComponent), nameof(AssemblerComponent.SetPCState));
            var factoryField = AccessTools.Field(typeof(FactorySystem), nameof(FactorySystem.factory));
            var getWorkEnergyPerTick = AccessTools.Method(typeof(PlanetFocusPatches), nameof(GetWorkEnergyPerTick));
            
            matcher.MatchForward(false,
                new CodeMatch(OpCodes.Ldloc_S),
                new CodeMatch(OpCodes.Ldloc_S),
                new CodeMatch(OpCodes.Call, setPcState));
            
            CodeInstruction local = matcher.InstructionAt(0);
            matcher.InsertAndAdvance(new CodeInstruction(local.opcode, local.operand));
            matcher.InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_0));
            matcher.InsertAndAdvance(new CodeInstruction(OpCodes.Ldfld, factoryField));
            matcher.InsertAndAdvance(new CodeInstruction(OpCodes.Call, getWorkEnergyPerTick));
        
            return matcher.InstructionEnumeration();
        }

        public static void GetWorkEnergyPerTick(ref AssemblerComponent assembler, PlanetFactory factory)
        {
            short modelIndex = factory.entityPool[assembler.entityId].modelIndex;

            if (!ModelPowerCosts.TryGetValue(modelIndex, out long workEnergyPerTick))
            {
                workEnergyPerTick = LDB.models.Select(modelIndex).prefabDesc.workEnergyPerTick;
                ModelPowerCosts.TryAdd(modelIndex, workEnergyPerTick);
            }

            if (ContainsFocus(factory.planetId, 6522)) workEnergyPerTick = (long)(workEnergyPerTick * 0.9f);

            factory.powerSystem.consumerPool[assembler.pcId].workEnergyPerTick = workEnergyPerTick;

            return;
        }
    }
}
