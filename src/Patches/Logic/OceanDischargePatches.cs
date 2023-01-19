using HarmonyLib;
using ProjectGenesis.Utils;

// ReSharper disable InconsistentNaming

namespace ProjectGenesis.Patches.Logic
{
    internal static class OceanDischargePatches
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(PlanetFactory), "ApplyPickTarget")]
        public static void PlanetFactory_ApplyPickTarget(
            ref PlanetFactory __instance,
            int entityId,
            int pickTarget,
            int slotId,
            int offset)
        {
            var minerId = __instance.entityPool[entityId].minerId;
            if (minerId > 0 &&
                __instance.entityPool[entityId].stationId == 0 &&
                __instance.factorySystem.minerPool[minerId].id == minerId &&
                __instance.entityPool[pickTarget].id == pickTarget)
                if (__instance.factorySystem.minerPool[minerId].type == EMinerType.Water)
                    __instance.factorySystem.minerPool[minerId].insertTarget = -pickTarget;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(PlanetFactory), "ApplyEntityDisconnection")]
        public static void PlanetFactory_ApplyEntityDisconnection(
            ref PlanetFactory __instance,
            int otherEntityId,
            int removingEntityId,
            int otherSlotId,
            int removingSlotId)
        {
            if (otherEntityId == 0) return;
            var minerId = __instance.entityPool[otherEntityId].minerId;
            if (minerId > 0)
            {
                var miner = __instance.factorySystem.minerPool[minerId];
                if (miner.id == minerId && miner.type == EMinerType.Water && miner.insertTarget == -removingEntityId)
                    __instance.factorySystem.minerPool[minerId].insertTarget = 0;
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(MinerComponent), "InternalUpdate")]
        public static void MinerComponent_InternalUpdate_PreFix(
            ref MinerComponent __instance,
            PlanetFactory factory,
            VeinData[] veinPool,
            float power,
            float miningRate,
            float miningSpeed,
            int[] productRegister)
        {
            if (__instance.type == EMinerType.Water &&
                __instance.insertTarget < 0 &&
                GameMain.history.TechUnlocked(ProtoIDUsedByPatches.T海洋排污1) &&
                factory.entityPool[-__instance.insertTarget].beltId > 0)
                __instance.speedDamper = 1;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(MinerComponent), "InternalUpdate")]
        public static void MinerComponent_InternalUpdate(
            ref MinerComponent __instance,
            PlanetFactory factory,
            VeinData[] veinPool,
            float power,
            float miningRate,
            float miningSpeed,
            int[] productRegister)
        {
            if (power < 0.1f) return;
            if (__instance.type == EMinerType.Water && __instance.insertTarget < 0 && GameMain.history.TechUnlocked(ProtoIDUsedByPatches.T海洋排污1))
            {
                var beltId = factory.entityPool[-__instance.insertTarget].beltId;
                if (beltId > 0)
                    if (__instance.time >= __instance.period)
                    {
                        __instance.time -= __instance.period;
                        var factoryProductionStat = GameMain.statistics.production.factoryStatPool[factory.index];
                        var consumeRegister = factoryProductionStat.consumeRegister;

                        var itemId = factory.cargoTraffic.TryPickItemAtRear(beltId, 0,
                                                                            GameMain.history.TechUnlocked(ProtoIDUsedByPatches.T海洋排污2)
                                                                                ? null
                                                                                : ItemProto.fluids, out var stack, out _);

                        if (itemId > 0)
                            lock (consumeRegister)
                            {
                                consumeRegister[itemId] += stack;
                            }
                    }
            }
        }
    }
}
