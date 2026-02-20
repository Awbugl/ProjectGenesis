using System.Collections.Concurrent;

namespace ProjectGenesis.Patches
{
    public static partial class PlanetFocusPatches
    {
        private static readonly ConcurrentDictionary<short, long> ModelPowerCosts = new ConcurrentDictionary<short, long>();

        internal static void GetWorkEnergyPerTick(PlanetFactory factory, ref AssemblerComponent assembler)
        {
            short modelIndex = factory.entityPool[assembler.entityId].modelIndex;

            if (!ModelPowerCosts.TryGetValue(modelIndex, out long workEnergyPerTick))
            {
                workEnergyPerTick = LDB.models.Select(modelIndex).prefabDesc.workEnergyPerTick;
                ModelPowerCosts.TryAdd(modelIndex, workEnergyPerTick);
            }

            factory.powerSystem.consumerPool[assembler.pcId].workEnergyPerTick =
                ContainsFocus(factory.planetId, 6522) ? (long)(workEnergyPerTick * 0.9f) : workEnergyPerTick;
        }
    }
}
