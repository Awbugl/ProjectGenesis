using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using HarmonyLib;

namespace ProjectGenesis.Patches
{
    public static class MegaAssemblerPatches
    {
        private static readonly ConcurrentDictionary<int, SlotData[]> Slotdata
            = new ConcurrentDictionary<int, SlotData[]>();

        private static readonly ConcurrentDictionary<int, PlanetFactory> PlanetFactories
            = new ConcurrentDictionary<int, PlanetFactory>();

        private static void InitSlots(int assemblerId) { Slotdata[assemblerId] = new SlotData[12]; }

        private static void InitPlanetFactory(int assemblerId, ref PlanetFactory factory)
        {
            PlanetFactories[assemblerId] = factory;
        }

        private static PlanetFactory GetPlanetFactory(int assemblerId)
        {
            if (PlanetFactories.ContainsKey(assemblerId)) return PlanetFactories[assemblerId];

            for (var index = 0; index < GameMain.statistics.gameData.factories.Length; index++)
            {
                var factory = GameMain.statistics.gameData.factories[index];
                foreach (var assembler in factory.factorySystem.assemblerPool)
                {
                    if (assembler.id == assemblerId)
                    {
                        InitPlanetFactory(assemblerId, ref factory);
                        return factory;
                    }
                }
            }

            return null;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(PlanetFactory), "ApplyInsertTarget")]
        public static void PlanetFactory_ApplyInsertTarget(ref PlanetFactory __instance, int entityId, int insertTarget,
                                                           int slotId, int offset)
        {
            if (entityId == 0) return;
            int assemblerId = __instance.entityPool[entityId].assemblerId;
            if (assemblerId > 0)
            {
                AssemblerComponent assembler = __instance.factorySystem.assemblerPool[assemblerId];
                if (assembler.id == assemblerId && assembler.speed > 100000)
                {
                    if (!PlanetFactories.ContainsKey(assemblerId)) InitPlanetFactory(assemblerId, ref __instance);
                    if (!Slotdata.ContainsKey(assemblerId)) InitSlots(assemblerId);
                    int beltId = __instance.entityPool[insertTarget].beltId;
                    if (beltId <= 0) return;
                    Slotdata[assemblerId][slotId].dir = IODir.Output;
                    Slotdata[assemblerId][slotId].beltId = beltId;
                }
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(PlanetFactory), "ApplyPickTarget")]
        public static void PlanetFactory_ApplyPickTarget(ref PlanetFactory __instance, int entityId, int pickTarget,
                                                         int slotId, int offset)
        {
            if (entityId == 0) return;
            int assemblerId = __instance.entityPool[entityId].assemblerId;
            if (assemblerId > 0)
            {
                AssemblerComponent assembler = __instance.factorySystem.assemblerPool[assemblerId];
                if (assembler.id == assemblerId && assembler.speed > 100000)
                {
                    if (!PlanetFactories.ContainsKey(assemblerId)) InitPlanetFactory(assemblerId, ref __instance);
                    if (!Slotdata.ContainsKey(assemblerId)) InitSlots(assemblerId);

                    int beltId = __instance.entityPool[pickTarget].beltId;
                    if (beltId <= 0) return;
                    Slotdata[assemblerId][slotId].dir = IODir.Input;
                    Slotdata[assemblerId][slotId].beltId = beltId;
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
            int assemblerId = __instance.entityPool[otherEntityId].assemblerId;
            if (assemblerId > 0)
            {
                AssemblerComponent assembler = __instance.factorySystem.assemblerPool[assemblerId];
                if (assembler.id == assemblerId && assembler.speed > 100000)
                {
                    if (!PlanetFactories.ContainsKey(assemblerId)) InitPlanetFactory(assemblerId, ref __instance);
                    if (!Slotdata.ContainsKey(assemblerId)) InitSlots(assemblerId);

                    int beltId = __instance.entityPool[removingEntityId].beltId;
                    if (beltId <= 0) return;
                    Slotdata[assemblerId][otherSlotId].dir = IODir.None;
                    Slotdata[assemblerId][otherSlotId].beltId = 0;
                }
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(AssemblerComponent), "InternalUpdate")]
        public static void AssemblerComponent_InternalUpdate(ref AssemblerComponent __instance, float power,
                                                             int[] productRegister, int[] consumeRegister)
        {
            if (power < 0.1f) return;

            if (__instance.speed > 100000)
            {
                var factory = GetPlanetFactory(__instance.id);
                if (!Slotdata.ContainsKey(__instance.id)) InitSlots(__instance.id);

                CargoTraffic cargoTraffic = factory.cargoTraffic;
                SignData[] entitySignPool = factory.entitySignPool;

                int stationPilerLevel = GameMain.history.stationPilerLevel;

                UpdateInputSlots(ref __instance, cargoTraffic, entitySignPool);
                UpdateOutputSlots(ref __instance, cargoTraffic, entitySignPool, stationPilerLevel);
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(AssemblerComponent), "UpdateNeeds")]
        public static bool AssemblerComponent_UpdateNeeds(ref AssemblerComponent __instance)
        {
            int length = __instance.requires.Length;
            var cache = __instance.speed > 100000
                ? 10
                : 3;
            for (int i = 0; i < length; ++i)
                __instance.needs[i] = __instance.served[i] < __instance.requireCounts[i] * cache
                    ? __instance.requires[i]
                    : 0;
            return false;
        }

        public static void UpdateOutputSlots(ref AssemblerComponent __instance, CargoTraffic traffic,
                                             SignData[] signPool, int maxPilerCount)
        {
            var slots = Slotdata[__instance.id];

            for (int index3 = 0; index3 < maxPilerCount; ++index3)
            {
                for (int index4 = 0; index4 < slots.Length; ++index4)
                {
                    if (slots[index4].dir == IODir.Output)
                    {
                        int entityId = traffic.beltPool[slots[index4].beltId].entityId;
                        CargoPath cargoPath = traffic.GetCargoPath(traffic.beltPool[slots[index4].beltId].segPathId);
                        if (cargoPath != null)
                        {
                            int index6 = 0;
                            if (index6 >= 0 && index6 < __instance.products.Length)
                            {
                                int itemId = __instance.products[index6];
                                if (itemId > 0 && __instance.produced[index6] > 0)
                                {
                                    if (cargoPath.TryUpdateItemAtHeadAndFillBlank(itemId, maxPilerCount, 1, 0))
                                    {
                                        --__instance.produced[index6];
                                    }
                                }
                            }

                            signPool[entityId].iconType = 0U;
                            signPool[entityId].iconId0 = 0U;
                        }
                    }
                    else if (slots[index4].dir != IODir.Input)
                    {
                        slots[index4].beltId = 0;
                        slots[index4].counter = 0;
                    }
                }
            }
        }

        public static void UpdateInputSlots(ref AssemblerComponent __instance, CargoTraffic traffic,
                                            SignData[] signPool)
        {
            var slots = Slotdata[__instance.id];
            for (int index = 0; index < slots.Length; ++index)
            {
                if (slots[index].dir == IODir.Input)
                {
                    int entityId = traffic.beltPool[slots[index].beltId].entityId;
                    CargoPath cargoPath = traffic.GetCargoPath(traffic.beltPool[slots[index].beltId].segPathId);
                    if (cargoPath != null)
                    {
                        int needIdx = -1;
                        byte stack;
                        byte inc;
                        int itemId = cargoPath.TryPickItemAtRear(__instance.needs, out needIdx, out stack, out inc);
                        if (needIdx >= 0)
                        {
                            int stack1 = (int)stack;
                            int inc1 = (int)inc;
                            if (itemId > 0)
                            {
                                if (needIdx < __instance.needs.Length && __instance.needs[needIdx] == itemId)
                                {
                                    __instance.served[needIdx] += stack1;
                                    __instance.incServed[needIdx] += inc1;
                                }
                            }

                            slots[index].storageIdx = needIdx + 1;
                        }

                        if (itemId > 0)
                        {
                            signPool[entityId].iconType = 1U;
                            signPool[entityId].iconId0 = (uint)itemId;
                        }
                    }
                }
                else if (slots[index].dir != IODir.Output)
                {
                    slots[index].beltId = 0;
                    slots[index].counter = 0;
                }
            }
        }

        public static void Export(BinaryWriter w)
        {
            w.Write(Slotdata.Count);

            foreach (var data in Slotdata)
            {
                w.Write(data.Key);
                w.Write(data.Value.Length);
                for (int i = 0; i < data.Value.Length; i++)
                {
                    w.Write((int)data.Value[i].dir);
                    w.Write(data.Value[i].beltId);
                }
            }
        }

        public static void Import(BinaryReader r)
        {
            Slotdata.Clear();

            int count = r.ReadInt32();

            for (int j = 0; j < count; j++)
            {
                int key = r.ReadInt32();
                int length = r.ReadInt32();
                SlotData[] datas = new SlotData[length];
                for (int i = 0; i < length; i++)
                {
                    SlotData data = new SlotData();

                    data.dir = (IODir)r.ReadInt32();
                    data.beltId = r.ReadInt32();
                    datas[i] = data;
                }

                Slotdata.TryAdd(key, datas);
            }
        }
    }
}
