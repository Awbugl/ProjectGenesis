using System;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using ProjectGenesis.Compatibility;
using ProjectGenesis.Utils;

// ReSharper disable InconsistentNaming

namespace ProjectGenesis.Patches
{
    internal static partial class MegaAssemblerPatches
    {
        internal const int MegaAssemblerSpeed = 300000;

        private static readonly FieldInfo EntityData_StationId_Field = AccessTools.Field(typeof(EntityData), nameof(EntityData.stationId)),
                                          EntityData_AssemblerId_Field =
                                              AccessTools.Field(typeof(EntityData), nameof(EntityData.assemblerId)),
                                          PlanetFactory_EntityPool_Field =
                                              AccessTools.Field(typeof(PlanetFactory), nameof(PlanetFactory.entityPool));

        public static void GameTick_AssemblerComponent_InternalUpdate_Patch(PlanetFactory factory, ref AssemblerComponent component,
            float power)
        {
            if (component.speed < MegaAssemblerSpeed) return;

            SlotData[] slotdata = GetSlots(factory.planetId, component.entityId);
            CargoTraffic cargoTraffic = factory.cargoTraffic;
            SignData[] entitySignPool = factory.entitySignPool;

            if (component.recipeId == ProtoID.R物质分解)
            {
                if (power < 0.1f) return;

                UpdateTrashInputSlots(ref component, power, factory, cargoTraffic, slotdata);

                int sandCount = component.produced[0];

                if (sandCount < 800 || GameMain.mainPlayer == null) return;

                GameMain.mainPlayer.sandCount += sandCount;
                component.produced[0] = 0;
            }
            else
            {
                UpdateOutputSlots(ref component, cargoTraffic, slotdata, entitySignPool, GameMain.history.stationPilerLevel);
                UpdateInputSlots(ref component, cargoTraffic, slotdata, entitySignPool);
            }
        }

        private static void UpdateOutputSlots(ref AssemblerComponent __instance, CargoTraffic traffic, SlotData[] slotdata,
            SignData[] signPool, int maxPilerCount)
        {
            for (var index1 = 0; index1 < slotdata.Length; ++index1)
            {
                ref SlotData slotData = ref slotdata[index1];

                if (slotData.dir == IODir.Output)
                {
                    int beltId = slotData.beltId;

                    if (beltId <= 0) continue;

                    BeltComponent beltComponent = traffic.beltPool[beltId];
                    CargoPath cargoPath = traffic.GetCargoPath(beltComponent.segPathId);

                    if (cargoPath == null) continue;

                    int index2 = slotData.storageIdx - 1;
                    var itemId = 0;

                    if (index2 >= 0)
                    {
                        RecipeExecuteData executeData = __instance.recipeExecuteData;

                        if (index2 < executeData.products.Length)
                        {
                            itemId = executeData.products[index2];
                            int produced = __instance.produced[index2];

                            if (itemId > 0 && produced > 0)
                            {
                                int num2 = produced < maxPilerCount ? produced : maxPilerCount;

                                if (cargoPath.TryInsertItemAtHeadAndFillBlank(itemId, (byte)num2, 0)) __instance.produced[index2] -= num2;
                            }
                        }
                        else
                        {
                            int index3 = index2 - executeData.products.Length;

                            if (index3 < executeData.requires.Length)
                            {
                                itemId = executeData.requires[index3];
                                int served = __instance.served[index3];

                                if (itemId > 0 && served > 0)
                                {
                                    int num2 = served < maxPilerCount ? served : maxPilerCount;
                                    var inc = (int)((double)__instance.incServed[index3] * num2 / __instance.served[index3]);

                                    if (cargoPath.TryInsertItemAtHeadAndFillBlank(itemId, (byte)num2, (byte)inc))
                                    {
                                        __instance.incServed[index3] -= inc;
                                        __instance.served[index3] -= num2;
                                    }
                                }
                            }
                        }
                    }

                    if (itemId <= 0) continue;

                    int entityId = beltComponent.entityId;
                    signPool[entityId].iconType = 1U;
                    signPool[entityId].iconId0 = (uint)itemId;
                }
                else if (slotData.dir != IODir.Input)
                {
                    slotData.beltId = 0;
                    slotData.counter = 0;
                }
            }
        }

        private static void UpdateTrashInputSlots(ref AssemblerComponent __instance, float power, PlanetFactory factory,
            CargoTraffic traffic, SlotData[] slotdata)
        {
            for (var index = 0; index < slotdata.Length; ++index)
            {
                if (slotdata[index].dir == IODir.Input)
                {
                    int beltId = slotdata[index].beltId;

                    if (beltId <= 0) continue;

                    BeltComponent beltComponent = traffic.beltPool[beltId];
                    CargoPath cargoPath = traffic.GetCargoPath(beltComponent.segPathId);

                    if (cargoPath == null) continue;

                    int itemId = traffic.TryPickItemAtRear(beltId, 0, null, out byte stack, out _);

                    if (itemId <= 0) continue;

                    ref int sandCount = ref __instance.produced[0];

                    if (itemId == ProtoID.I沙土) { sandCount += stack; }
                    else
                    {
                        FactoryProductionStat factoryProductionStat = GameMain.statistics.production.factoryStatPool[factory.index];
                        int[] productRegister = factoryProductionStat.productRegister;
                        int[] consumeRegister = factoryProductionStat.consumeRegister;

                        lock (consumeRegister) consumeRegister[itemId] += stack;

                        ItemProto itemProto = LDB.items.Select(itemId);

                        if (MoreMegaStructure.Installed && MoreMegaStructure.FastAssembleItems.TryGetValue(itemId, out int recipeId))
                        {
                            RecipeProto recipe = LDB.recipes.Select(recipeId);

                            int count;

                            if (recipe.Results.Length == 1) { count = stack * 3 * recipe.ItemCounts[0] / recipe.ResultCounts[0] / 4; }
                            else
                            {
                                int idx = Array.IndexOf(recipe.Results, itemId);

                                count = stack * 3 * recipe.ItemCounts[idx] / recipe.ResultCounts[idx] / recipe.Results.Length / 4;
                            }

                            TryAddItemToPackage(9500, ref count, productRegister);

                            if (count <= 0) continue;

                            count *= 400;
                            sandCount += count;
                            productRegister[ProtoID.I沙土] += count;
                        }
                        else if (itemProto.CanBuild)
                        {
                            RecipeProto recipe = itemProto.recipes.FirstOrDefault();

                            if (recipe != null)
                            {
                                for (var i = 0; i < recipe.Items.Length; i++)
                                {
                                    int recipeItem = recipe.Items[i];
                                    int count = recipe.ItemCounts[i] * stack * 3 / 4;

                                    TryAddItemToPackage(recipeItem, ref count, productRegister);

                                    if (count <= 0) continue;

                                    count *= 40;
                                    sandCount += count;
                                    productRegister[ProtoID.I沙土] += count;
                                }

                                continue;
                            }
                        }

                        var stack1 = (int)(stack * 40 * power);
                        sandCount += stack1;
                        productRegister[ProtoID.I沙土] += stack1;
                    }
                }
                else if (slotdata[index].dir != IODir.Output)
                {
                    slotdata[index].beltId = 0;
                    slotdata[index].counter = 0;
                }
            }
        }

        private static void TryAddItemToPackage(int itemId, ref int count, int[] productRegister)
        {
            Player player = GameMain.data.mainPlayer;

            if (itemId <= 0 || count <= 0) return;

            int package = player.package.AddItemStacked(itemId, count, 0, out _);
            int count1 = count - package;

            if (count1 > 0 && player.deliveryPackage.unlocked)
            {
                int count2 = player.deliveryPackage.AddItem(itemId, count1, 0, out _);
                count1 -= count2;
            }

            productRegister[itemId] += count - count1;

            count = count1;
        }

        private static void UpdateInputSlots(ref AssemblerComponent __instance, CargoTraffic traffic, SlotData[] slotdata,
            SignData[] signPool)
        {
            for (var index = 0; index < slotdata.Length; ++index)
            {
                if (slotdata[index].dir == IODir.Input)
                {
                    int beltId = slotdata[index].beltId;

                    if (beltId <= 0) continue;

                    BeltComponent beltComponent = traffic.beltPool[beltId];
                    CargoPath cargoPath = traffic.GetCargoPath(beltComponent.segPathId);

                    if (cargoPath == null) continue;

                    int itemId = cargoPath.TryPickItemAtRear(__instance.needs, out int needIdx, out byte stack, out byte inc);

                    RecipeExecuteData executeData = __instance.recipeExecuteData;

                    if (needIdx >= 0 && itemId > 0 && __instance.needs[needIdx] == itemId)
                    {
                        __instance.served[needIdx] += stack;
                        __instance.incServed[needIdx] += inc;
                        slotdata[index].storageIdx = executeData.products.Length + needIdx + 1;
                    }

                    for (var i = 0; i < executeData.products.Length; i++)
                    {
                        if (__instance.produced[i] >= 50) continue;

                        itemId = traffic.TryPickItemAtRear(beltId, executeData.products[i], null, out stack, out _);

                        if (executeData.products[i] == itemId)
                        {
                            __instance.produced[i] += stack;
                            slotdata[index].storageIdx = i + 1;

                            break;
                        }
                    }

                    if (itemId <= 0) continue;

                    int entityId = beltComponent.entityId;
                    signPool[entityId].iconType = 1U;
                    signPool[entityId].iconId0 = (uint)itemId;
                }
                else if (slotdata[index].dir != IODir.Output)
                {
                    slotdata[index].beltId = 0;
                    slotdata[index].counter = 0;
                }
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(PlanetFactory), nameof(PlanetFactory.ApplyInsertTarget))]
        public static void PlanetFactory_ApplyInsertTarget(ref PlanetFactory __instance, int entityId, int insertTarget, int slotId,
            int offset)
        {
            if (entityId == 0) return;

            int assemblerId = __instance.entityPool[entityId].assemblerId;

            if (assemblerId <= 0) return;

            AssemblerComponent assembler = __instance.factorySystem.assemblerPool[assemblerId];

            if (assembler.id != assemblerId || assembler.speed < MegaAssemblerSpeed) return;

            int beltId = __instance.entityPool[insertTarget].beltId;

            if (beltId <= 0) return;

            SlotData[] slotdata = GetSlots(__instance.planetId, entityId);
            slotdata[slotId].dir = IODir.Output;
            slotdata[slotId].beltId = beltId;
            slotdata[slotId].counter = 0;
            SyncSlotData.Sync(__instance.planetId, slotId, entityId, slotdata[slotId]);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(PlanetFactory), nameof(PlanetFactory.ApplyPickTarget))]
        public static void PlanetFactory_ApplyPickTarget(ref PlanetFactory __instance, int entityId, int pickTarget, int slotId, int offset)
        {
            if (entityId == 0) return;

            int assemblerId = __instance.entityPool[entityId].assemblerId;

            if (assemblerId <= 0) return;

            AssemblerComponent assembler = __instance.factorySystem.assemblerPool[assemblerId];

            if (assembler.id != assemblerId || assembler.speed < MegaAssemblerSpeed) return;

            int beltId = __instance.entityPool[pickTarget].beltId;

            if (beltId <= 0) return;

            SlotData[] slotdata = GetSlots(__instance.planetId, entityId);
            slotdata[slotId].dir = IODir.Input;
            slotdata[slotId].beltId = beltId;
            slotdata[slotId].storageIdx = 0;
            slotdata[slotId].counter = 0;
            SyncSlotData.Sync(__instance.planetId, slotId, entityId, slotdata[slotId]);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(PlanetFactory), nameof(PlanetFactory.ApplyEntityDisconnection))]
        public static void PlanetFactory_ApplyEntityDisconnection(ref PlanetFactory __instance, int otherEntityId, int removingEntityId,
            int otherSlotId, int removingSlotId)
        {
            if (otherEntityId == 0) return;

            int assemblerId = __instance.entityPool[otherEntityId].assemblerId;

            if (assemblerId <= 0) return;

            AssemblerComponent assembler = __instance.factorySystem.assemblerPool[assemblerId];

            if (assembler.id != assemblerId || assembler.speed < MegaAssemblerSpeed) return;

            int beltId = __instance.entityPool[removingEntityId].beltId;

            if (beltId <= 0) return;

            SlotData[] slotdata = GetSlots(__instance.planetId, otherEntityId);

            slotdata[otherSlotId].dir = IODir.None;
            slotdata[otherSlotId].beltId = 0;
            slotdata[otherSlotId].counter = 0;

            SyncSlotData.Sync(__instance.planetId, otherSlotId, otherEntityId, slotdata[otherSlotId]);
        }

        [HarmonyPatch(typeof(PlanetFactory), nameof(PlanetFactory.RemoveEntityWithComponents))]
        [HarmonyPrefix]
        [HarmonyPriority(Priority.VeryHigh)]
        public static void PlanetFactory_RemoveEntityWithComponents(ref PlanetFactory __instance, int id)
        {
            if (id != 0)
            {
                EntityData entityData = __instance.entityPool[id];

                if (entityData.id != 0 && entityData.assemblerId != 0) SetEmpty(__instance.planetId, id);
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(FactorySystem), nameof(FactorySystem.NewAssemblerComponent))]
        public static void FactorySystem_NewAssemblerComponent(ref FactorySystem __instance, int entityId, int speed)
        {
            if (speed >= MegaAssemblerSpeed) __instance.factory.entityPool[entityId].stationId = 0;
        }

        [HarmonyPatch(typeof(PlanetFactory), nameof(PlanetFactory.Import))]
        [HarmonyPostfix]
        public static void PlanetFactory_Import(ref PlanetFactory __instance)
        {
            foreach (((int planetId, int entityId), SlotData[] datas) in Slotdata)
            {
                if (planetId != __instance.planetId) continue;

                for (var i = 0; i < datas.Length; i++)
                {
                    __instance.ReadObjectConn(entityId, i, out _, out int otherObjId, out _);

                    if (otherObjId <= 0 || __instance.entityPool[otherObjId].beltId != datas[i].beltId)
                    {
                        BeltComponent beltComponent = __instance.cargoTraffic.beltPool[datas[i].beltId];
                        ref SignData signData = ref __instance.entitySignPool[beltComponent.entityId];
                        signData.iconType = 0U;
                        signData.iconId0 = 0U;

                        datas[i] = new SlotData();
                    }
                }
            }
        }
    }
}
