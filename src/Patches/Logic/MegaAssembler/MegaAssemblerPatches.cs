using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using ProjectGenesis.Utils;

// ReSharper disable InconsistentNaming

namespace ProjectGenesis.Patches.Logic.MegaAssembler
{
    internal static partial class MegaAssemblerPatches
    {
        private const int TrashSpeed = 60000;

        private static readonly FieldInfo EntityData_StationId_Field = AccessTools.Field(typeof(EntityData), nameof(EntityData.stationId)),
                                          EntityData_AssemblerId_Field = AccessTools.Field(typeof(EntityData), nameof(EntityData.assemblerId)),
                                          PlanetFactory_EntityPool_Field = AccessTools.Field(typeof(PlanetFactory), nameof(PlanetFactory.entityPool)),
                                          FactorySystem_AssemblerPool_Field
                                              = AccessTools.Field(typeof(FactorySystem), nameof(FactorySystem.assemblerPool));

        private static readonly MethodInfo AssemblerComponent_InternalUpdate_Method
                                               = AccessTools.Method(typeof(AssemblerComponent), nameof(AssemblerComponent.InternalUpdate)),
                                           MegaAssembler_AssemblerComponent_InternalUpdate_Patch_Method
                                               = AccessTools.Method(typeof(MegaAssemblerPatches),
                                                                    nameof(GameTick_AssemblerComponent_InternalUpdate_Patch));

        [HarmonyPatch(typeof(FactorySystem), nameof(FactorySystem.GameTick), typeof(long), typeof(bool))]
        [HarmonyPatch(typeof(FactorySystem), nameof(FactorySystem.GameTick), typeof(long), typeof(bool), typeof(int), typeof(int), typeof(int))]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> FactorySystem_GameTick_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            var matcher = new CodeMatcher(instructions, generator);

            matcher.MatchForward(false, new CodeMatch(OpCodes.Ldloc_S), new CodeMatch(OpCodes.Ldloc_S), new CodeMatch(OpCodes.Ldloc_1),
                                 new CodeMatch(OpCodes.Ldloc_2), new CodeMatch(OpCodes.Call, AssemblerComponent_InternalUpdate_Method));

            object local1 = matcher.Operand;
            object power1 = matcher.Advance(1).Operand;

            matcher.CreateLabelAt(matcher.Pos + 4, out Label label1);

            matcher.Advance(-1).InsertAndAdvance(new CodeInstruction(OpCodes.Ldc_I4_0), new CodeInstruction(OpCodes.Ldloc_S, local1),
                                                 new CodeInstruction(OpCodes.Ldarg_0), new CodeInstruction(OpCodes.Ldloc_S, power1),
                                                 new CodeInstruction(OpCodes.Call, MegaAssembler_AssemblerComponent_InternalUpdate_Patch_Method),
                                                 new CodeInstruction(OpCodes.Brfalse_S, label1), new CodeInstruction(OpCodes.Pop));

            matcher.Advance(5).MatchForward(false, new CodeMatch(OpCodes.Ldloc_S), new CodeMatch(OpCodes.Ldloc_S), new CodeMatch(OpCodes.Ldloc_1),
                                            new CodeMatch(OpCodes.Ldloc_2), new CodeMatch(OpCodes.Call, AssemblerComponent_InternalUpdate_Method));

            if (matcher.IsValid)
            {
                object local2 = matcher.Operand;
                object power2 = matcher.Advance(1).Operand;

                matcher.CreateLabelAt(matcher.Pos + 4, out Label label2);

                matcher.Advance(-1).InsertAndAdvance(new CodeInstruction(OpCodes.Ldc_I4_0), new CodeInstruction(OpCodes.Ldloc_S, local2),
                                                     new CodeInstruction(OpCodes.Ldarg_0), new CodeInstruction(OpCodes.Ldloc_S, power2),
                                                     new CodeInstruction(OpCodes.Call, MegaAssembler_AssemblerComponent_InternalUpdate_Patch_Method),
                                                     new CodeInstruction(OpCodes.Brfalse_S, label2), new CodeInstruction(OpCodes.Pop));
            }

            return matcher.InstructionEnumeration();
        }

        [HarmonyPatch(typeof(FactorySystem), nameof(FactorySystem.GameTick), typeof(long), typeof(bool), typeof(int), typeof(int), typeof(int))]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> FactorySystem_GameTick_Transpiler_2(
            IEnumerable<CodeInstruction> instructions,
            ILGenerator generator)
        {
            var matcher = new CodeMatcher(instructions, generator);

            matcher.MatchForward(false, new CodeMatch(OpCodes.Ldarg_0), new CodeMatch(OpCodes.Ldfld), new CodeMatch(OpCodes.Ldloc_S),
                                 new CodeMatch(OpCodes.Ldelema), new CodeMatch(OpCodes.Ldloc_S), new CodeMatch(OpCodes.Ldloc_1),
                                 new CodeMatch(OpCodes.Ldloc_2), new CodeMatch(OpCodes.Call, AssemblerComponent_InternalUpdate_Method));

            object index = matcher.Advance(2).Operand;
            object power = matcher.Advance(2).Operand;

            matcher.CreateLabelAt(matcher.Pos + 4, out Label label);

            matcher.Advance(-4).InsertAndAdvance(new CodeInstruction(OpCodes.Ldc_I4_0), new CodeInstruction(OpCodes.Ldarg_0),
                                                 new CodeInstruction(OpCodes.Ldfld, FactorySystem_AssemblerPool_Field),
                                                 new CodeInstruction(OpCodes.Ldloc_S, index),
                                                 new CodeInstruction(OpCodes.Ldelema, typeof(AssemblerComponent)),
                                                 new CodeInstruction(OpCodes.Ldarg_0), new CodeInstruction(OpCodes.Ldloc_S, power),
                                                 new CodeInstruction(OpCodes.Call, MegaAssembler_AssemblerComponent_InternalUpdate_Patch_Method),
                                                 new CodeInstruction(OpCodes.Brfalse_S, label), new CodeInstruction(OpCodes.Pop));

            return matcher.InstructionEnumeration();
        }

        public static bool GameTick_AssemblerComponent_InternalUpdate_Patch(
            ref AssemblerComponent __instance,
            FactorySystem factorySystem,
            float power)
        {
            PlanetFactory factory = factorySystem.factory;

            bool b = power >= 0.1f;

            // MegaBuildings
            if (__instance.speed >= TrashSpeed)
            {
                SlotData[] slotdata = GetSlots(factory.planetId, __instance.entityId);
                CargoTraffic cargoTraffic = factory.cargoTraffic;
                SignData[] entitySignPool = factory.entitySignPool;

                int stationPilerLevel = GameMain.history.stationPilerLevel;

                if (__instance.recipeId != ProtoID.R物质分解)
                {
                    UpdateOutputSlots(ref __instance, cargoTraffic, slotdata, entitySignPool, stationPilerLevel);
                    UpdateInputSlots(ref __instance, cargoTraffic, slotdata, entitySignPool);
                }
                else if (b)
                {
                    UpdateTrashInputSlots(ref __instance, power, factory, cargoTraffic, slotdata);

                    int sandCount = __instance.produced[0];

                    if (sandCount >= 800 && GameMain.mainPlayer != null)
                    {
                        GameMain.mainPlayer.sandCount += sandCount;
                        __instance.produced[0] = 0;
                    }
                }
            }

            if (factory.entityPool[__instance.entityId].protoId == ProtoID.I负熵熔炉 && __instance.replicating)
                __instance.extraTime += (int)(power * __instance.extraSpeed) +
                                        (int)(power * __instance.speedOverride * __instance.extraTimeSpend / __instance.timeSpend);

            return b;
        }

        private static void UpdateOutputSlots(
            ref AssemblerComponent __instance,
            CargoTraffic traffic,
            SlotData[] slotdata,
            SignData[] signPool,
            int maxPilerCount)
        {
            for (int index1 = 0; index1 < slotdata.Length; ++index1)
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
                    int itemId = 0;

                    if (index2 >= 0)
                    {
                        if (index2 < __instance.products.Length)
                        {
                            itemId = __instance.products[index2];
                            int produced = __instance.produced[index2];
                            if (itemId > 0 && produced > 0)
                            {
                                int num2 = produced < maxPilerCount ? produced : maxPilerCount;
                                if (cargoPath.TryInsertItemAtHeadAndFillBlank(itemId, (byte)num2, 0)) __instance.produced[index2] -= num2;
                            }
                        }
                        else
                        {
                            int index3 = index2 - __instance.products.Length;
                            if (index3 < __instance.requires.Length)
                            {
                                itemId = __instance.requires[index3];
                                int served = __instance.served[index3];
                                if (itemId > 0 && served > 0)
                                {
                                    int num2 = served < maxPilerCount ? served : maxPilerCount;
                                    int inc = (int)((double)__instance.incServed[index3] * num2 / __instance.served[index3]);
                                    if (cargoPath.TryInsertItemAtHeadAndFillBlank(itemId, (byte)num2, (byte)inc))
                                    {
                                        __instance.incServed[index3] -= inc;
                                        __instance.served[index3] -= num2;
                                    }
                                }
                            }
                        }
                    }

                    if (itemId > 0)
                    {
                        int entityId = beltComponent.entityId;
                        signPool[entityId].iconType = 1U;
                        signPool[entityId].iconId0 = (uint)itemId;
                    }
                }
                else if (slotData.dir != IODir.Input)
                {
                    slotData.beltId = 0;
                    slotData.counter = 0;
                }
            }
        }

        private static void UpdateTrashInputSlots(
            ref AssemblerComponent __instance,
            float power,
            PlanetFactory factory,
            CargoTraffic traffic,
            SlotData[] slotdata)
        {
            for (int index = 0; index < slotdata.Length; ++index)
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

                    if (itemId == ProtoID.I沙土)
                    {
                        sandCount += stack;
                    }
                    else
                    {
                        FactoryProductionStat factoryProductionStat = GameMain.statistics.production.factoryStatPool[factory.index];
                        int[] productRegister = factoryProductionStat.productRegister;
                        int[] consumeRegister = factoryProductionStat.consumeRegister;

                        lock (consumeRegister)
                        {
                            consumeRegister[itemId] += stack;
                        }

                        ItemProto itemProto = LDB.items.Select(itemId);

                        int stack1 = (int)(stack * 40 * power);

                        if (itemProto.CanBuild)
                        {
                            RecipeProto recipe = itemProto.recipes.FirstOrDefault();

                            if (recipe != null)
                            {
                                for (int i = 0; i < recipe.Items.Length; i++)
                                {
                                    int recipeItem = recipe.Items[i];
                                    float recipeItemCount = recipe.ItemCounts[i] * stack * 0.75f;

                                    int count = recipeItemCount < 1 ? 1 : (int)recipeItemCount;
                                    TryAddItemToPackage(recipeItem, ref count, productRegister);

                                    if (count > 0)
                                    {
                                        count *= 40;
                                        sandCount += count;
                                        productRegister[ProtoID.I沙土] += count;
                                    }
                                }

                                continue;
                            }
                        }

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

        private static void UpdateInputSlots(
            ref AssemblerComponent __instance,
            CargoTraffic traffic,
            SlotData[] slotdata,
            SignData[] signPool)
        {
            for (int index = 0; index < slotdata.Length; ++index)
            {
                if (slotdata[index].dir == IODir.Input)
                {
                    int beltId = slotdata[index].beltId;
                    if (beltId <= 0) continue;
                    BeltComponent beltComponent = traffic.beltPool[beltId];
                    CargoPath cargoPath = traffic.GetCargoPath(beltComponent.segPathId);
                    if (cargoPath == null) continue;

                    int itemId = cargoPath.TryPickItemAtRear(__instance.needs, out int needIdx, out byte stack, out byte inc);

                    if (needIdx >= 0 && itemId > 0 && __instance.needs[needIdx] == itemId)
                    {
                        __instance.served[needIdx] += stack;
                        __instance.incServed[needIdx] += inc;
                        slotdata[index].storageIdx = __instance.products.Length + needIdx + 1;
                    }

                    for (int i = 0; i < __instance.products.Length; i++)
                    {
                        if (__instance.produced[i] >= 50) continue;

                        itemId = traffic.TryPickItemAtRear(beltId, __instance.products[i], null, out stack, out _);

                        if (__instance.products[i] == itemId)
                        {
                            __instance.produced[i] += stack;
                            slotdata[index].storageIdx = i + 1;
                            break;
                        }
                    }

                    if (itemId > 0)
                    {
                        int entityId = beltComponent.entityId;
                        signPool[entityId].iconType = 1U;
                        signPool[entityId].iconId0 = (uint)itemId;
                    }
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
        public static void PlanetFactory_ApplyInsertTarget(
            ref PlanetFactory __instance,
            int entityId,
            int insertTarget,
            int slotId,
            int offset)
        {
            if (entityId == 0) return;
            int assemblerId = __instance.entityPool[entityId].assemblerId;
            if (assemblerId <= 0) return;

            AssemblerComponent assembler = __instance.factorySystem.assemblerPool[assemblerId];
            if (assembler.id != assemblerId || assembler.speed < TrashSpeed) return;

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
        public static void PlanetFactory_ApplyPickTarget(
            ref PlanetFactory __instance,
            int entityId,
            int pickTarget,
            int slotId,
            int offset)
        {
            if (entityId == 0) return;
            int assemblerId = __instance.entityPool[entityId].assemblerId;
            if (assemblerId <= 0) return;

            AssemblerComponent assembler = __instance.factorySystem.assemblerPool[assemblerId];
            if (assembler.id != assemblerId || assembler.speed < TrashSpeed) return;

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
        public static void PlanetFactory_ApplyEntityDisconnection(
            ref PlanetFactory __instance,
            int otherEntityId,
            int removingEntityId,
            int otherSlotId,
            int removingSlotId)
        {
            if (otherEntityId == 0) return;
            int assemblerId = __instance.entityPool[otherEntityId].assemblerId;
            if (assemblerId <= 0) return;

            AssemblerComponent assembler = __instance.factorySystem.assemblerPool[assemblerId];
            if (assembler.id != assemblerId || assembler.speed < TrashSpeed) return;

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
            if (speed >= TrashSpeed) __instance.factory.entityPool[entityId].stationId = 0;
        }

        [HarmonyPatch(typeof(PlanetFactory), nameof(PlanetFactory.Import))]
        [HarmonyPostfix]
        public static void PlanetFactory_Import(ref PlanetFactory __instance)
        {
            foreach (((int planetId, int entityId), SlotData[] datas) in Slotdata)
            {
                if (planetId != __instance.planetId) continue;

                for (int i = 0; i < datas.Length; i++)
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
