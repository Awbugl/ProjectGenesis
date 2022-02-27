using HarmonyLib;
using System.Text;

namespace ProjectGenesis.Patches
{
    class OceanDischargePatches
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(PlanetFactory), "ApplyPickTarget")]
        public static void PlanetFactory_ApplyPickTarget(ref PlanetFactory __instance, int entityId, int pickTarget,
                                                         int slotId, int offset)
        {
            int minerId = __instance.entityPool[entityId].minerId;
            if (minerId > 0 && __instance.entityPool[entityId].stationId == 0
                            && __instance.factorySystem.minerPool[minerId].id == minerId
                            && __instance.entityPool[pickTarget].id == pickTarget)
            {
                if (__instance.factorySystem.minerPool[minerId].type == EMinerType.Water)
                {
                    __instance.factorySystem.minerPool[minerId].insertTarget = -pickTarget;
                }
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(PlanetFactory), "ApplyEntityDisconnection")]
        public static void PlanetFactory_ApplyEntityDisconnection(ref PlanetFactory __instance, int otherEntityId,
                                                                  int removingEntityId, int otherSlotId,
                                                                  int removingSlotId)
        {
            if (otherEntityId == 0) return;
            int minerId = __instance.entityPool[otherEntityId].minerId;
            if (minerId > 0)
            {
                MinerComponent miner = __instance.factorySystem.minerPool[minerId];
                if (miner.id == minerId && miner.type == EMinerType.Water && miner.insertTarget == -removingEntityId)
                {
                    __instance.factorySystem.minerPool[minerId].insertTarget = 0;
                }
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(MinerComponent), "InternalUpdate")]
        public static void MinerComponent_InternalUpdate(ref MinerComponent __instance, PlanetFactory factory,
                                                         VeinData[] veinPool, float power, float miningRate,
                                                         float miningSpeed, int[] productRegister)
        {
            if (power < 0.1f) return;
            if (__instance.type == EMinerType.Water && __instance.insertTarget < 0
                                                    && (1813 == 0 || GameMain.history.TechUnlocked(1813)))
            {
                int beltId = factory.entityPool[-__instance.insertTarget].beltId;
                if (beltId > 0)
                {
                    if (__instance.time >= __instance.period)
                    {
                        __instance.time -= __instance.period;
                        byte stack, inc;
                        FactoryProductionStat factoryProductionStat
                            = GameMain.statistics.production.factoryStatPool[factory.index];
                        int[] consumeRegister = factoryProductionStat.consumeRegister;
                        int itemId = factory.cargoTraffic.TryPickItemAtRear(beltId, 0, GameMain.history.TechUnlocked(1815) ? null : ItemProto.fluids, out stack,
                                                                            out inc);
                        if (itemId > 0)
                        {
                            lock (consumeRegister)
                            {
                                consumeRegister[itemId] += stack;
                            }
                        }
                    }
                }
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(TechProto), "UnlockFunctionText")]
        public static void TechProto_UnlockFunctionText(ref TechProto __instance, ref string __result, StringBuilder sb)
        {
            switch (__instance.ID)
            {
                case 1813:
                    __result = "将传送带反向连入抽水站,可以将不需要的液体排入大海.(抽水站蓄满时才会排放)".Translate();
                    break;

                case 1508: // (int)科技.任务完成
                    __result = "欢迎加入创世之书讨论群:991895539".Translate();
                    break;
                
                case 1814:
                    __result = "化工厂生产速度翻倍";
                    break;

                case 1815:
                    __result = "海洋排污也能排放固体";
                    break;
            }
        }
    }
}

