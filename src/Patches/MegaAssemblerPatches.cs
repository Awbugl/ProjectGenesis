using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using ProjectGenesis.Utils;
using UnityEngine;
using ERecipeType_1 = ERecipeType;

// ReSharper disable InconsistentNaming

namespace ProjectGenesis.Patches
{
    internal static class MegaAssemblerPatches
    {
        internal const int TrashSpeed = 60000;
        internal const int MegaAssemblerSpeed = 400000;

        #region Internal Fields & Functions

        private static int TmpSandCount;
        private static ConcurrentDictionary<(int, int), SlotData[]> _slotdata = new ConcurrentDictionary<(int, int), SlotData[]>();

        internal static void SyncSlots((int, int) id, SlotData[] slotDatas) => _slotdata[id] = slotDatas;

        internal static void SyncSlot((int, int) id, int slotId, SlotData slotData)
        {
            lock (_slotdata)
            {
                if (_slotdata.TryGetValue(id, out SlotData[] slotDatas))
                {
                    slotDatas[slotId] = slotData;
                }
                else
                {
                    slotDatas = new SlotData[12];
                    slotDatas[slotId] = slotData;
                    _slotdata[id] = slotDatas;
                }
            }
        }

        private static SlotData[] GetSlots(int planetId, int entityId)
        {
            var id = (planetId, entityId);

            if (!_slotdata.ContainsKey(id) || _slotdata[id] == null) _slotdata[id] = new SlotData[12];

            return _slotdata[id];
        }

        #endregion

        #region MegaAssemblerPatches

        private static readonly FieldInfo EntityData_StationId_Field = AccessTools.Field(typeof(EntityData), nameof(EntityData.stationId)),
                                          EntityData_AssemblerId_Field = AccessTools.Field(typeof(EntityData), nameof(EntityData.assemblerId)),
                                          PlanetFactory_EntityPool_Field = AccessTools.Field(typeof(PlanetFactory), nameof(PlanetFactory.entityPool)),
                                          FactorySystem_AssemblerPool_Field
                                              = AccessTools.Field(typeof(FactorySystem), nameof(FactorySystem.assemblerPool)),
                                          AssemblerComponent_Speed_Field
                                              = AccessTools.Field(typeof(AssemblerComponent), nameof(AssemblerComponent.speed));

        private static readonly MethodInfo AssemblerComponent_InternalUpdate_Method
                                               = AccessTools.Method(typeof(AssemblerComponent), nameof(AssemblerComponent.InternalUpdate)),
                                           MegaAssembler_AssemblerComponent_InternalUpdate_Patch_Method
                                               = AccessTools.Method(typeof(MegaAssemblerPatches),
                                                                    nameof(GameTick_AssemblerComponent_InternalUpdate_Patch)),
                                           MegaAssembler_AssemblerComponent_UpdateNeeds_Patch_Method
                                               = AccessTools.Method(typeof(MegaAssemblerPatches), nameof(AssemblerComponent_UpdateNeeds_Patch));

        [HarmonyPostfix]
        [HarmonyPatch(typeof(FactorySystem), "NewAssemblerComponent")]
        public static void FactorySystem_NewAssemblerComponent(ref FactorySystem __instance, int entityId, int speed)
        {
            if (speed >= TrashSpeed) __instance.factory.entityPool[entityId].stationId = 0;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(BuildingParameters), "ApplyPrebuildParametersToEntity")]
        public static void BuildingParameters_ApplyPrebuildParametersToEntity(
            int entityId,
            int recipeId,
            int filterId,
            int[] parameters,
            PlanetFactory factory)
        {
            if (entityId <= 0 || factory.entityPool[entityId].id != entityId) return;
            var assemblerId = factory.entityPool[entityId].assemblerId;
            if (assemblerId > 0)
            {
                ref var assembler = ref factory.factorySystem.assemblerPool[assemblerId];
                if (assembler.id == assemblerId && assembler.speed >= TrashSpeed && parameters != null && parameters.Length >= 2048)
                {
                    assembler.forceAccMode = parameters[0] > 0;

                    if (assembler.recipeId != recipeId)
                    {
                        if (recipeId == 0)
                        {
                            assembler.SetRecipe(0, factory.entitySignPool);
                        }
                        else
                        {
                            var recipe = LDB.recipes.Select(recipeId);
                            var itemProto = LDB.items.Select(factory.entityPool[entityId].protoId);

                            if (recipeId > 0 &&
                                ContainsRecipeType(itemProto.prefabDesc.assemblerRecipeType, recipe.Type) &&
                                factory.gameData.history.RecipeUnlocked(recipeId))
                                assembler.SetRecipe(recipeId, factory.entitySignPool);
                        }
                    }

                    SlotData[] slots = GetSlots(factory.planetId, entityId);
                    const int num4 = 192;
                    for (var index = 0; index < slots.Length; ++index)
                    {
                        slots[index].dir = (IODir)parameters[num4 + index * 4];
                        slots[index].storageIdx = parameters[num4 + index * 4 + 1];
                    }

                    SyncSlotsData.Sync(factory.planetId, entityId, slots);
                }
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(BuildingParameters), "FromParamsArray")]
        public static void BuildingParameters_FromParamsArray(ref BuildingParameters __instance, int[] _parameters)
        {
            if (_parameters != null && _parameters.Length >= 2048)
            {
                if (__instance.parameters.Length < 2048) Array.Resize(ref __instance.parameters, 2048);

                Array.Copy(_parameters, 192, __instance.parameters, 192, 1856);
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(BuildingParameters), "ToParamsArray")]
        public static void BuildingParameters_ToParamsArray(ref BuildingParameters __instance, ref int[] _parameters, ref int _paramCount)
        {
            if (__instance.type == BuildingType.Assembler)
            {
                if (__instance.parameters.Length >= 2048)
                {
                    if (_parameters == null || _parameters.Length < 2048) _parameters = new int[2048];
                    Array.Copy(__instance.parameters, _parameters, 2048);
                    _paramCount = _parameters.Length;
                    return;
                }

                _paramCount = 1;

                if (_parameters == null || _parameters.Length < _paramCount) _parameters = new int[_paramCount];
                _parameters[0] = __instance.parameters[0];
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(BuildingParameters), "CopyFromFactoryObject")]
        public static void BuildingParameters_CopyFromFactoryObject(
            ref BuildingParameters __instance,
            int objectId,
            PlanetFactory factory,
            bool copyInserters)
        {
            if (objectId > 0)
            {
                if (factory.entityPool.Length > objectId && factory.entityPool[objectId].id == objectId && __instance.type == BuildingType.Assembler)
                {
                    var assemblerId = factory.entityPool[objectId].assemblerId;
                    if (assemblerId > 0 && factory.factorySystem.assemblerPool.Length > assemblerId)
                    {
                        var assembler = factory.factorySystem.assemblerPool[assemblerId];
                        if (assembler.id == assemblerId && assembler.speed >= TrashSpeed)
                        {
                            __instance.parameters = new int[2048];
                            __instance.parameters[0] = assembler.forceAccMode ? 1 : 0;
                            __instance.recipeId = assembler.recipeId;
                            __instance.recipeType = assembler.recipeType;

                            const int num2 = 192;

                            SlotData[] slots = GetSlots(factory.planetId, objectId);

                            for (var index = 0; index < slots.Length; ++index)
                            {
                                __instance.parameters[num2 + index * 4] = (int)slots[index].dir;
                                __instance.parameters[num2 + index * 4 + 1] = slots[index].storageIdx;
                            }

                            SyncSlotsData.Sync(factory.planetId, objectId, slots);
                        }
                    }
                }
            }
            else
            {
                var index2 = -objectId;
                PrebuildData[] prebuildPool = factory.prebuildPool;
                if (index2 > 0 && prebuildPool[index2].id == index2)
                {
                    __instance.recipeId = prebuildPool[index2].recipeId;
                    var recipeProto = LDB.recipes.Select(prebuildPool[index2].recipeId);
                    if (recipeProto != null) __instance.recipeType = recipeProto.Type;
                }
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(BuildingParameters), "PasteToFactoryObject")]
        public static void BuildingParameters_PasteToFactoryObject(
            ref BuildingParameters __instance,
            int objectId,
            PlanetFactory factory,
            ref bool __result)
        {
            if (objectId > 0)
            {
                if (factory.entityPool.Length > objectId && factory.entityPool[objectId].id == objectId && __instance.type == BuildingType.Assembler)
                {
                    var assemblerId = factory.entityPool[objectId].assemblerId;
                    if (assemblerId > 0 && factory.factorySystem.assemblerPool.Length > assemblerId)
                    {
                        ref var assembler = ref factory.factorySystem.assemblerPool[assemblerId];
                        if (assembler.id == assemblerId && assembler.speed >= TrashSpeed)
                        {
                            var itemProto = LDB.items.Select(factory.entityPool[objectId].protoId);
                            if (itemProto != null && itemProto.prefabDesc != null)
                            {
                                if (assembler.recipeId != __instance.recipeId &&
                                    (__instance.recipeId == 0 ||
                                     (__instance.recipeId > 0 &&
                                      ContainsRecipeType(itemProto.prefabDesc.assemblerRecipeType, __instance.recipeType) &&
                                      GameMain.history.RecipeUnlocked(__instance.recipeId))))
                                {
                                    factory.factorySystem.TakeBackItems_Assembler(GameMain.mainPlayer, assemblerId);
                                    {
                                        assembler.SetRecipe(__instance.recipeId, factory.entitySignPool);
                                        __result = true;
                                    }
                                }

                                if (__instance.parameters != null &&
                                    __instance.parameters.Length >= 1 &&
                                    ContainsRecipeType(itemProto.prefabDesc.assemblerRecipeType, __instance.recipeType))
                                    assembler.forceAccMode = __instance.parameters[0] > 0;
                            }
                        }
                    }
                }
            }
            else
            {
                var index1 = -objectId;
                PrebuildData[] prebuildPool = factory.prebuildPool;

                if (index1 > 0 && prebuildPool[index1].id == index1)
                {
                    var itemProto = LDB.items.Select(prebuildPool[index1].protoId);
                    if (itemProto != null && itemProto.prefabDesc != null)
                        if (itemProto.prefabDesc.isAssembler &&
                            __instance.type == BuildingType.Assembler &&
                            ContainsRecipeType(itemProto.prefabDesc.assemblerRecipeType, __instance.recipeType))
                        {
                            prebuildPool[index1].recipeId = __instance.recipeId;
                            prebuildPool[index1].filterId = __instance.filterId;
                            __instance.ToParamsArray(ref prebuildPool[index1].parameters, ref prebuildPool[index1].paramCount);
                        }
                }
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(BuildingParameters), "CanPasteToFactoryObject")]
        public static void BuildingParameters_CanPasteToFactoryObject(
            ref BuildingParameters __instance,
            int objectId,
            PlanetFactory factory,
            ref bool __result)
        {
            if (objectId > 0)
            {
                if (factory.entityPool.Length > objectId && factory.entityPool[objectId].id == objectId && __instance.type == BuildingType.Assembler)
                {
                    var assemblerId = factory.entityPool[objectId].assemblerId;
                    if (assemblerId > 0 && factory.factorySystem.assemblerPool.Length > assemblerId)
                    {
                        var assembler = factory.factorySystem.assemblerPool[assemblerId];
                        if (assembler.id == assemblerId && assembler.speed >= TrashSpeed)
                            if (assembler.recipeId != __instance.recipeId)
                            {
                                var itemProto = LDB.items.Select(factory.entityPool[objectId].protoId);
                                if (itemProto != null &&
                                    itemProto.prefabDesc != null &&
                                    (__instance.recipeId == 0 ||
                                     (__instance.recipeId > 0 &&
                                      ContainsRecipeType(itemProto.prefabDesc.assemblerRecipeType, __instance.recipeType) &&
                                      GameMain.history.RecipeUnlocked(__instance.recipeId))))
                                    __result = true;
                            }
                    }
                }
            }
            else
            {
                var index2 = -objectId;
                PrebuildData[] prebuildPool = factory.prebuildPool;
                if (index2 > 0 && prebuildPool[index2].id == index2)
                {
                    var itemProto = LDB.items.Select(prebuildPool[index2].protoId);
                    if (itemProto != null &&
                        itemProto.prefabDesc != null &&
                        itemProto.prefabDesc.isAssembler &&
                        __instance.type == BuildingType.Assembler &&
                        ContainsRecipeType(itemProto.prefabDesc.assemblerRecipeType, __instance.recipeType))
                        __result = true;
                }
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(BuildingParameters), "CopyFromBuildPreview")]
        public static void BuildingParameters_CopyFromBuildPreview(ref BuildingParameters __instance, BuildPreview bp)
        {
            __instance.recipeId = bp.recipeId;
            __instance.filterId = bp.filterId;
            var recipeProto = LDB.recipes.Select(__instance.recipeId);
            if (recipeProto != null) __instance.recipeType = recipeProto.Type;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(PlanetFactory), "ApplyInsertTarget")]
        public static void PlanetFactory_ApplyInsertTarget(
            ref PlanetFactory __instance,
            int entityId,
            int insertTarget,
            int slotId,
            int offset)
        {
            if (entityId == 0) return;
            var assemblerId = __instance.entityPool[entityId].assemblerId;
            if (assemblerId > 0)
            {
                var assembler = __instance.factorySystem.assemblerPool[assemblerId];
                if (assembler.id == assemblerId && assembler.speed >= TrashSpeed)
                {
                    var beltId = __instance.entityPool[insertTarget].beltId;
                    if (beltId <= 0) return;
                    SlotData[] slotdata = GetSlots(__instance.planetId, entityId);
                    slotdata[slotId].dir = IODir.Output;
                    slotdata[slotId].beltId = beltId;
                    slotdata[slotId].counter = 0;
                    SyncSlotData.Sync(__instance.planetId, slotId, entityId, slotdata[slotId]);
                }
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(PlanetFactory), "ApplyPickTarget")]
        public static void PlanetFactory_ApplyPickTarget(
            ref PlanetFactory __instance,
            int entityId,
            int pickTarget,
            int slotId,
            int offset)
        {
            if (entityId == 0) return;
            var assemblerId = __instance.entityPool[entityId].assemblerId;
            if (assemblerId > 0)
            {
                var assembler = __instance.factorySystem.assemblerPool[assemblerId];
                if (assembler.id == assemblerId && assembler.speed >= TrashSpeed)
                {
                    var beltId = __instance.entityPool[pickTarget].beltId;
                    if (beltId <= 0) return;
                    SlotData[] slotdata = GetSlots(__instance.planetId, entityId);
                    slotdata[slotId].dir = IODir.Input;
                    slotdata[slotId].beltId = beltId;
                    slotdata[slotId].storageIdx = 0;
                    slotdata[slotId].counter = 0;
                    SyncSlotData.Sync(__instance.planetId, slotId, entityId, slotdata[slotId]);
                }
            }
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
            var assemblerId = __instance.entityPool[otherEntityId].assemblerId;
            if (assemblerId > 0)
            {
                var assembler = __instance.factorySystem.assemblerPool[assemblerId];
                if (assembler.id == assemblerId && assembler.speed >= TrashSpeed)
                {
                    var beltId = __instance.entityPool[removingEntityId].beltId;
                    if (beltId <= 0) return;
                    SlotData[] slotdata = GetSlots(__instance.planetId, otherEntityId);

                    slotdata[otherSlotId].dir = IODir.None;
                    slotdata[otherSlotId].beltId = 0;
                    slotdata[otherSlotId].storageIdx = 0;
                    slotdata[otherSlotId].counter = 0;

                    SyncSlotData.Sync(__instance.planetId, otherSlotId, otherEntityId, slotdata[otherSlotId]);
                }
            }
        }

        [HarmonyPatch(typeof(FactorySystem), "GameTick", typeof(long), typeof(bool))]
        [HarmonyPatch(typeof(FactorySystem), "GameTick", typeof(long), typeof(bool), typeof(int), typeof(int), typeof(int))]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> FactorySystem_GameTick_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var matcher = new CodeMatcher(instructions).MatchForward(false, new CodeMatch(OpCodes.Ldloc_S), new CodeMatch(OpCodes.Ldloc_S),
                                                                     new CodeMatch(OpCodes.Ldloc_1), new CodeMatch(OpCodes.Ldloc_2),
                                                                     new CodeMatch(OpCodes.Call, AssemblerComponent_InternalUpdate_Method));

            var local1 = matcher.Operand;
            var power1 = matcher.Advance(1).Operand;

            matcher.Advance(4).InsertAndAdvance(new CodeInstruction(OpCodes.Ldloc_S, local1), new CodeInstruction(OpCodes.Ldarg_0),
                                                new CodeInstruction(OpCodes.Ldloc_S, power1),
                                                new CodeInstruction(OpCodes.Call, MegaAssembler_AssemblerComponent_InternalUpdate_Patch_Method));

            matcher.MatchForward(false, new CodeMatch(OpCodes.Ldloc_S), new CodeMatch(OpCodes.Ldloc_S), new CodeMatch(OpCodes.Ldloc_1),
                                 new CodeMatch(OpCodes.Ldloc_2), new CodeMatch(OpCodes.Call, AssemblerComponent_InternalUpdate_Method));

            if (matcher.IsValid)
            {
                var local2 = matcher.Operand;
                var power2 = matcher.Advance(1).Operand;

                matcher.Advance(4).InsertAndAdvance(new CodeInstruction(OpCodes.Ldloc_S, local2), new CodeInstruction(OpCodes.Ldarg_0),
                                                    new CodeInstruction(OpCodes.Ldloc_S, power2),
                                                    new CodeInstruction(OpCodes.Call, MegaAssembler_AssemblerComponent_InternalUpdate_Patch_Method));
            }

            return matcher.InstructionEnumeration();
        }

        [HarmonyPatch(typeof(FactorySystem), "GameTick", typeof(long), typeof(bool), typeof(int), typeof(int), typeof(int))]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> FactorySystem_GameTick_Transpiler_2(IEnumerable<CodeInstruction> instructions)
        {
            var matcher = new CodeMatcher(instructions).MatchForward(false, new CodeMatch(OpCodes.Ldloc_S), new CodeMatch(OpCodes.Ldelema),
                                                                     new CodeMatch(OpCodes.Ldloc_S), new CodeMatch(OpCodes.Ldloc_1),
                                                                     new CodeMatch(OpCodes.Ldloc_2),
                                                                     new CodeMatch(OpCodes.Call, AssemblerComponent_InternalUpdate_Method));


            var index = matcher.Operand;
            var power = matcher.Advance(2).Operand;

            matcher.Advance(4).InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_0),
                                                new CodeInstruction(OpCodes.Ldfld, FactorySystem_AssemblerPool_Field),
                                                new CodeInstruction(OpCodes.Ldloc_S, index),
                                                new CodeInstruction(OpCodes.Ldelema, typeof(AssemblerComponent)),
                                                new CodeInstruction(OpCodes.Ldarg_0), new CodeInstruction(OpCodes.Ldloc_S, power),
                                                new CodeInstruction(OpCodes.Call, MegaAssembler_AssemblerComponent_InternalUpdate_Patch_Method));

            return matcher.InstructionEnumeration();
        }

        public static void GameTick_AssemblerComponent_InternalUpdate_Patch(
            ref AssemblerComponent __instance,
            FactorySystem factorySystem,
            float power)
        {
            // 巨型建筑效果
            if (__instance.speed >= TrashSpeed)
            {
                var factory = factorySystem.factory;
                SlotData[] slotdata = GetSlots(factory.planetId, __instance.entityId);

                var cargoTraffic = factory.cargoTraffic;
                SignData[] entitySignPool = factory.entitySignPool;

                var stationPilerLevel = GameMain.history.stationPilerLevel;

                UpdateOutputSlots(ref __instance, cargoTraffic, slotdata, entitySignPool, stationPilerLevel);
                UpdateInputSlots(ref __instance, factory, cargoTraffic, slotdata, entitySignPool);
            }

            if (power < 0.1f) return;

            // 化工技术革新效果
            if (GameMain.history.TechUnlocked(1513))
            {
                var instanceRecipeType = __instance.recipeType;
                if (instanceRecipeType == (ERecipeType_1)Utils.ERecipeType.Chemical ||
                    instanceRecipeType == (ERecipeType_1)Utils.ERecipeType.Refine ||
                    instanceRecipeType == (ERecipeType_1)Utils.ERecipeType.高分子化工)
                    if (__instance.speed == 20000)
                        __instance.speed = 40000;
            }
        }

        private static void UpdateOutputSlots(
            ref AssemblerComponent __instance,
            CargoTraffic traffic,
            SlotData[] slotdata,
            SignData[] signPool,
            int maxPilerCount)
        {
            for (var index1 = 0; index1 < slotdata.Length; ++index1)
            {
                if (slotdata[index1].dir == IODir.Output)
                {
                    if (slotdata[index1].counter > 0)
                    {
                        --slotdata[index1].counter;
                    }
                    else
                    {
                        var beltComponent = traffic.beltPool[slotdata[index1].beltId];
                        var cargoPath = traffic.GetCargoPath(beltComponent.segPathId);
                        if (cargoPath != null)
                        {
                            var index2 = slotdata[index1].storageIdx - 1;
                            var itemId = 0;
                            if (index2 >= 0)
                            {
                                if (index2 < __instance.products.Length)
                                {
                                    itemId = __instance.products[index2];
                                    var produced = __instance.produced[index2];
                                    if (itemId > 0 && produced > 0)
                                    {
                                        var num2 = produced < maxPilerCount ? produced : maxPilerCount;
                                        if (cargoPath.TryInsertItemAtHeadAndFillBlank(itemId, (byte)num2, 0)) __instance.produced[index2] -= num2;
                                    }
                                }
                                else
                                {
                                    var index3 = index2 - __instance.products.Length;
                                    if (index3 < __instance.requires.Length)
                                    {
                                        itemId = __instance.requires[index3];
                                        var served = __instance.served[index3];
                                        if (itemId > 0 && served > 0)
                                        {
                                            var num2 = served < maxPilerCount ? served : maxPilerCount;
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

                            if (itemId > 0)
                            {
                                var entityId = beltComponent.entityId;
                                signPool[entityId].iconType = 1U;
                                signPool[entityId].iconId0 = (uint)itemId;
                            }
                        }
                    }
                }
                else if (slotdata[index1].dir != IODir.Input)
                {
                    slotdata[index1].beltId = 0;
                    slotdata[index1].counter = 0;
                }
            }
        }

        private static void UpdateInputSlots(
            ref AssemblerComponent __instance,
            PlanetFactory factory,
            CargoTraffic traffic,
            SlotData[] slotdata,
            SignData[] signPool)
        {
            for (var index = 0; index < slotdata.Length; ++index)
            {
                if (slotdata[index].dir == IODir.Input)
                {
                    if (slotdata[index].counter > 0)
                    {
                        --slotdata[index].counter;
                    }
                    else
                    {
                        var entityId = traffic.beltPool[slotdata[index].beltId].entityId;
                        var cargoPath = traffic.GetCargoPath(traffic.beltPool[slotdata[index].beltId].segPathId);
                        if (cargoPath != null)
                        {
                            if (__instance.recipeId == 429)
                            {
                                var itemId = traffic.TryPickItemAtRear(slotdata[index].beltId, 0, null, out var stack, out _);

                                if (itemId > 0)
                                {
                                    var consumeRegister = GameMain.statistics.production.factoryStatPool[factory.index].consumeRegister;

                                    lock (consumeRegister)
                                    {
                                        consumeRegister[itemId] += stack;
                                    }

                                    TmpSandCount += stack;

                                    if (TmpSandCount >= 1000 && GameMain.mainPlayer != null)
                                    {
                                        // This method will be called in a worker thread (not main UI thread).
                                        // Thus, calling `GameMain.mainPlayer.SetSandCount` which brings up sand tooltip UI
                                        // will crash the program.
                                        // Instead, we should increase the sand count directly.
                                        AccessTools.PropertySetter(typeof(Player), "sandCount")
                                                   .Invoke(GameMain.mainPlayer,
                                                           new object[] { Math.Min(1000000000, GameMain.mainPlayer.sandCount + TmpSandCount * 20) });
                                        TmpSandCount = 0;
                                    }
                                }
                            }
                            else
                            {
                                var itemId = cargoPath.TryPickItemAtRear(__instance.needs, out var needIdx, out var stack, out var inc);

                                if (needIdx >= 0 && itemId > 0 && __instance.needs[needIdx] == itemId)
                                {
                                    __instance.served[needIdx] += stack;
                                    __instance.incServed[needIdx] += inc;
                                }

                                for (var i = 0; i < __instance.products.Length; i++)
                                {
                                    if (__instance.produced[i] < 50)
                                    {
                                        itemId = traffic.TryPickItemAtRear(slotdata[index].beltId, __instance.products[i], null, out stack, out _);

                                        if (__instance.products[i] == itemId)
                                        {
                                            __instance.produced[i] += stack;
                                            break;
                                        }
                                    }
                                }

                                if (itemId > 0)
                                {
                                    signPool[entityId].iconType = 1U;
                                    signPool[entityId].iconId0 = (uint)itemId;
                                }
                            }
                        }
                    }
                }
                else if (slotdata[index].dir != IODir.Output)
                {
                    slotdata[index].beltId = 0;
                    slotdata[index].counter = 0;
                }
            }
        }

        [HarmonyPatch(typeof(AssemblerComponent), "UpdateNeeds")]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> AssemblerComponent_UpdateNeeds_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var matcher = new CodeMatcher(instructions);

            while (true)
            {
                matcher.MatchForward(false, new CodeMatch(OpCodes.Ldc_I4_3), new CodeMatch(OpCodes.Mul));

                if (matcher.IsInvalid) break;

                matcher.InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_0));
                matcher.InsertAndAdvance(new CodeInstruction(OpCodes.Ldfld, AssemblerComponent_Speed_Field));
                matcher.SetInstruction(new CodeInstruction(OpCodes.Call, MegaAssembler_AssemblerComponent_UpdateNeeds_Patch_Method));
            }

            return matcher.InstructionEnumeration();
        }

        public static sbyte AssemblerComponent_UpdateNeeds_Patch(int speed) => speed > TrashSpeed ? (sbyte)10 : (sbyte)3;

        // 绕开 CommonAPI 的 UIAssemblerWindowPatch.ChangePicker
        [HarmonyPatch(typeof(UIAssemblerWindow), "OnSelectRecipeClick")]
        [HarmonyPriority(1)]
        [HarmonyPrefix]
        public static bool UIAssemblerWindow_OnSelectRecipeClick(UIAssemblerWindow __instance)
        {
            if (__instance.assemblerId == 0 ||
                __instance.factory == null ||
                __instance.factorySystem.assemblerPool[__instance.assemblerId].id != __instance.assemblerId)
                return false;
            var entityId = __instance.factorySystem.assemblerPool[__instance.assemblerId].entityId;
            if (entityId == 0) return false;
            var itemProto = LDB.items.Select(__instance.factory.entityPool[entityId].protoId);
            if (itemProto == null) return false;

            var assemblerRecipeType = itemProto.prefabDesc.assemblerRecipeType;
            if (UIRecipePicker.isOpened)
                UIRecipePicker.Close();
            else
                UIRecipePicker.Popup(__instance.windowTrans.anchoredPosition + new Vector2(-300f, -135f),
                                     i => AccessTools.Method(typeof(UIAssemblerWindow), "OnRecipePickerReturn")
                                                     .Invoke(__instance, new object[] { i }), assemblerRecipeType);

            return false;
        }

        [HarmonyPatch(typeof(UIRecipePicker), "RefreshIcons")]
        [HarmonyPriority(int.MaxValue)]
        [HarmonyPostfix]
        public static void UIRecipePicker_RefreshIcons(
            UIRecipePicker __instance,
            ref ERecipeType_1 ___filter,
            ref int ___currentType,
            ref uint[] ___indexArray,
            ref RecipeProto[] ___protoArray)
        {
            var filter = (Utils.ERecipeType)___filter;
            if (filter == Utils.ERecipeType.所有化工 || filter == Utils.ERecipeType.所有熔炉 || filter == Utils.ERecipeType.所有高精)
            {
                Array.Clear(___indexArray, 0, ___indexArray.Length);
                Array.Clear(___protoArray, 0, ___protoArray.Length);
                var history = GameMain.history;
                RecipeProto[] dataArray = LDB.recipes.dataArray;
                var iconSet = GameMain.iconSet;
                // ReSharper disable once ForCanBeConvertedToForeach
                for (var index1 = 0; index1 < dataArray.Length; ++index1)
                {
                    var gridIndex = dataArray[index1].GridIndex;
                    if (gridIndex >= 1101 && history.RecipeUnlocked(dataArray[index1].ID))
                        if (___filter == ERecipeType_1.None || ContainsRecipeType(___filter, dataArray[index1].Type))
                        {
                            var num1 = gridIndex / 1000;
                            var num2 = (gridIndex - num1 * 1000) / 100 - 1;
                            var num3 = gridIndex % 100 - 1;
                            if (num2 >= 0 && num3 >= 0 && num2 < 7 && num3 < 17)
                            {
                                var index2 = num2 * 17 + num3;
                                if (index2 >= 0 && index2 < ___indexArray.Length && num1 == ___currentType)
                                {
                                    ___indexArray[index2] = iconSet.recipeIconIndex[dataArray[index1].ID];
                                    ___protoArray[index2] = dataArray[index1];
                                }
                            }
                        }
                }
            }
        }

        private static bool ContainsRecipeType(ERecipeType filter, ERecipeType recipetype)
        {
            var type = (Utils.ERecipeType)recipetype;

            switch ((Utils.ERecipeType)filter)
            {
                case Utils.ERecipeType.所有化工:
                    return type == Utils.ERecipeType.Chemical || type == Utils.ERecipeType.Refine || type == Utils.ERecipeType.高分子化工;

                case Utils.ERecipeType.所有熔炉:
                    return type == Utils.ERecipeType.Smelt || type == Utils.ERecipeType.矿物处理;

                case Utils.ERecipeType.所有高精:
                    return type == Utils.ERecipeType.高精度加工 || type == Utils.ERecipeType.电路蚀刻;

                default:
                    return filter == recipetype;
            }
        }

        #endregion

        #region BeltBuildTip & SlotPicker Patch

        private static readonly FieldInfo UIBeltBuildTip_FilterItems_Field = AccessTools.Field(typeof(UIBeltBuildTip), "filterItems"),
                                          UISlotPicker_FilterItems_Field = AccessTools.Field(typeof(UISlotPicker), "filterItems"),
                                          UIBeltBuildTip_SelectedIndex_Field
                                              = AccessTools.Field(typeof(UIBeltBuildTip), nameof(UIBeltBuildTip.selectedIndex)),
                                          UISlotPicker_SelectedIndex_Field
                                              = AccessTools.Field(typeof(UISlotPicker), nameof(UISlotPicker.selectedIndex)),
                                          UIBeltBuildTip_OutputEntityId_Field
                                              = AccessTools.Field(typeof(UIBeltBuildTip), nameof(UIBeltBuildTip.outputEntityId)),
                                          UISlotPicker_OutputEntityId_Field
                                              = AccessTools.Field(typeof(UISlotPicker), nameof(UISlotPicker.outputEntityId)),
                                          UIBeltBuildTip_OutputSlotId_Field
                                              = AccessTools.Field(typeof(UIBeltBuildTip), nameof(UIBeltBuildTip.outputSlotId)),
                                          UISlotPicker_OutputSlotId_Field
                                              = AccessTools.Field(typeof(UISlotPicker), nameof(UISlotPicker.outputSlotId));

        private static readonly MethodInfo MegaAssembler_SetOutputEntity_Patch_Method
                                               = AccessTools.Method(typeof(MegaAssemblerPatches), nameof(SetOutputEntity_Patch)),
                                           MegaAssembler_SetFilterToEntity_Patch_Method
                                               = AccessTools.Method(typeof(MegaAssemblerPatches), nameof(SetFilterToEntity_Patch));

        [HarmonyPatch(typeof(UIBeltBuildTip), "SetOutputEntity")]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> UIBeltBuildTip_SetOutputEntity_Transpiler(
            IEnumerable<CodeInstruction> instructions,
            ILGenerator generator)
        {
            var matcher = new CodeMatcher(instructions, generator).MatchForward(true, new CodeMatch(OpCodes.Ldloc_0),
                                                                                new CodeMatch(OpCodes.Ldfld, PlanetFactory_EntityPool_Field),
                                                                                new CodeMatch(OpCodes.Ldarg_1), new CodeMatch(OpCodes.Ldelem),
                                                                                new CodeMatch(OpCodes.Stloc_3));


            matcher.Advance(1).CreateLabelAt(matcher.Pos, out var label);

            matcher.InsertAndAdvance(new CodeInstruction(OpCodes.Ldloc_3), new CodeInstruction(OpCodes.Ldfld, EntityData_AssemblerId_Field),
                                     new CodeInstruction(OpCodes.Ldc_I4_0), new CodeInstruction(OpCodes.Ble, label),
                                     new CodeInstruction(OpCodes.Ldarg_0), new CodeInstruction(OpCodes.Ldloc_0), new CodeInstruction(OpCodes.Ldloc_3),
                                     new CodeInstruction(OpCodes.Ldarg_0), new CodeInstruction(OpCodes.Ldflda, UIBeltBuildTip_FilterItems_Field),
                                     new CodeInstruction(OpCodes.Ldarg_1), new CodeInstruction(OpCodes.Ldarg_2),
                                     new CodeInstruction(OpCodes.Call, MegaAssembler_SetOutputEntity_Patch_Method),
                                     new CodeInstruction(OpCodes.Stfld, UIBeltBuildTip_SelectedIndex_Field));

            return matcher.InstructionEnumeration();
        }

        [HarmonyPatch(typeof(UISlotPicker), "SetOutputEntity")]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> UISlotPicker_SetOutputEntity_Transpiler(
            IEnumerable<CodeInstruction> instructions,
            ILGenerator generator)
        {
            var matcher = new CodeMatcher(instructions, generator).MatchForward(true, new CodeMatch(OpCodes.Ldloc_0),
                                                                                new CodeMatch(OpCodes.Ldfld, PlanetFactory_EntityPool_Field),
                                                                                new CodeMatch(OpCodes.Ldarg_1), new CodeMatch(OpCodes.Ldelem),
                                                                                new CodeMatch(OpCodes.Stloc_1));

            matcher.Advance(1).CreateLabelAt(matcher.Pos, out var label);

            matcher.InsertAndAdvance(new CodeInstruction(OpCodes.Ldloc_1), new CodeInstruction(OpCodes.Ldfld, EntityData_AssemblerId_Field),
                                     new CodeInstruction(OpCodes.Ldc_I4_0), new CodeInstruction(OpCodes.Ble, label),
                                     new CodeInstruction(OpCodes.Ldarg_0), new CodeInstruction(OpCodes.Ldloc_0), new CodeInstruction(OpCodes.Ldloc_1),
                                     new CodeInstruction(OpCodes.Ldarg_0), new CodeInstruction(OpCodes.Ldflda, UISlotPicker_FilterItems_Field),
                                     new CodeInstruction(OpCodes.Ldarg_1), new CodeInstruction(OpCodes.Ldarg_2),
                                     new CodeInstruction(OpCodes.Call, MegaAssembler_SetOutputEntity_Patch_Method),
                                     new CodeInstruction(OpCodes.Stfld, UISlotPicker_SelectedIndex_Field));

            return matcher.InstructionEnumeration();
        }

        public static int SetOutputEntity_Patch(
            PlanetFactory factory,
            EntityData entityData,
            ref List<int> filterItems,
            int entityId,
            int slot)
        {
            var assemblerComponent = factory.factorySystem.assemblerPool[entityData.assemblerId];

            if (assemblerComponent.speed >= TrashSpeed)
            {
                if (assemblerComponent.recipeId > 0)
                {
                    filterItems.AddRange(assemblerComponent.products);
                    filterItems.AddRange(assemblerComponent.requires);
                }
                else
                {
                    filterItems.AddRange(Enumerable.Repeat(0, 6));
                }

                entityData.stationId = 0;

                if (slot >= 0 && slot < 12) return GetSlots(factory.planetId, entityId)[slot].storageIdx;
                Assert.CannotBeReached();

                return -1;
            }

            return -1;
        }

        [HarmonyPatch(typeof(UIBeltBuildTip), "SetFilterToEntity")]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> UIBeltBuildTip_SetFilterToEntity_Transpiler(
            IEnumerable<CodeInstruction> instructions,
            ILGenerator generator)
        {
            var matcher = new CodeMatcher(instructions, generator).MatchForward(true, new CodeMatch(OpCodes.Ldloc_0),
                                                                                new CodeMatch(OpCodes.Ldfld, PlanetFactory_EntityPool_Field),
                                                                                new CodeMatch(OpCodes.Ldarg_0),
                                                                                new CodeMatch(OpCodes.Ldfld, UIBeltBuildTip_OutputEntityId_Field),
                                                                                new CodeMatch(OpCodes.Ldelem), new CodeMatch(OpCodes.Stloc_1));

            matcher.Advance(1).CreateLabelAt(matcher.Pos, out var label);

            matcher.InsertAndAdvance(new CodeInstruction(OpCodes.Ldloc_1), new CodeInstruction(OpCodes.Ldfld, EntityData_AssemblerId_Field),
                                     new CodeInstruction(OpCodes.Ldc_I4_0), new CodeInstruction(OpCodes.Ble, label),
                                     new CodeInstruction(OpCodes.Ldloc_0), new CodeInstruction(OpCodes.Ldloc_1), new CodeInstruction(OpCodes.Ldarg_0),
                                     new CodeInstruction(OpCodes.Ldfld, UIBeltBuildTip_OutputEntityId_Field), new CodeInstruction(OpCodes.Ldarg_0),
                                     new CodeInstruction(OpCodes.Ldfld, UIBeltBuildTip_OutputSlotId_Field), new CodeInstruction(OpCodes.Ldarg_0),
                                     new CodeInstruction(OpCodes.Ldfld, UIBeltBuildTip_SelectedIndex_Field),
                                     new CodeInstruction(OpCodes.Call, MegaAssembler_SetFilterToEntity_Patch_Method));

            return matcher.InstructionEnumeration();
        }

        [HarmonyPatch(typeof(UISlotPicker), "SetFilterToEntity")]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> UISlotPicker_SetFilterToEntity_Transpiler(
            IEnumerable<CodeInstruction> instructions,
            ILGenerator generator)
        {
            var matcher = new CodeMatcher(instructions, generator).MatchForward(true, new CodeMatch(OpCodes.Ldloc_0),
                                                                                new CodeMatch(OpCodes.Ldfld, PlanetFactory_EntityPool_Field),
                                                                                new CodeMatch(OpCodes.Ldarg_0),
                                                                                new CodeMatch(OpCodes.Ldfld, UISlotPicker_OutputEntityId_Field),
                                                                                new CodeMatch(OpCodes.Ldelem), new CodeMatch(OpCodes.Stloc_1));

            matcher.Advance(1).CreateLabelAt(matcher.Pos, out var label);

            matcher.InsertAndAdvance(new CodeInstruction(OpCodes.Ldloc_1), new CodeInstruction(OpCodes.Ldfld, EntityData_AssemblerId_Field),
                                     new CodeInstruction(OpCodes.Ldc_I4_0), new CodeInstruction(OpCodes.Ble, label),
                                     new CodeInstruction(OpCodes.Ldloc_0), new CodeInstruction(OpCodes.Ldloc_1), new CodeInstruction(OpCodes.Ldarg_0),
                                     new CodeInstruction(OpCodes.Ldfld, UISlotPicker_OutputEntityId_Field), new CodeInstruction(OpCodes.Ldarg_0),
                                     new CodeInstruction(OpCodes.Ldfld, UISlotPicker_OutputSlotId_Field), new CodeInstruction(OpCodes.Ldarg_0),
                                     new CodeInstruction(OpCodes.Ldfld, UISlotPicker_SelectedIndex_Field),
                                     new CodeInstruction(OpCodes.Call, MegaAssembler_SetFilterToEntity_Patch_Method));

            return matcher.InstructionEnumeration();
        }

        // prevent a packet flood when the filter on a belt connecting.
        // special thanks for https://github.com/hubastard/nebula/tree/master/NebulaPatcher/Patches/Transpilers/UIBeltBuildTip_Transpiler.cs

        private static (int, int) LastSetId;
        private static int LastSlotId;
        private static int LastSelectedIndex;

        public static void SetFilterToEntity_Patch(
            PlanetFactory factory,
            EntityData entityData,
            int outputEntityId,
            int outputSlotId,
            int selectedIndex)
        {
            var assemblerComponent = factory.factorySystem.assemblerPool[entityData.assemblerId];

            if (assemblerComponent.speed >= TrashSpeed)
            {
                SlotData[] slotDatas = GetSlots(factory.planetId, outputEntityId);
                slotDatas[outputSlotId].storageIdx = selectedIndex;

                entityData.stationId = 0;

                if (!IsChangeCached((factory.planetId, outputEntityId), outputSlotId, selectedIndex))
                {
                    SyncSlotData.Sync(factory.planetId, outputSlotId, outputEntityId, slotDatas[outputSlotId]);
                    CacheChange((factory.planetId, outputEntityId), outputSlotId, selectedIndex);
                }
            }
        }

        private static bool IsChangeCached((int, int) id, int slotId, int selectedIndex)
            => LastSetId == id && LastSlotId == slotId && LastSelectedIndex == selectedIndex;

        private static void CacheChange((int, int) id, int slotId, int selectedIndex)
        {
            LastSetId = id;
            LastSlotId = slotId;
            LastSelectedIndex = selectedIndex;
        }

        #endregion

        #region DeterminePatch

        private static readonly FieldInfo isStationField = AccessTools.Field(typeof(PrefabDesc), nameof(PrefabDesc.isStation));
        private static readonly FieldInfo isAssemblerField = AccessTools.Field(typeof(PrefabDesc), nameof(PrefabDesc.isAssembler));

        [HarmonyPatch(typeof(BuildTool_Path), "DeterminePreviews")]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> BuildTool_Path_DeterminePreviews_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var matcher = new CodeMatcher(instructions).MatchForward(false, new CodeMatch(OpCodes.Ldloc_S),
                                                                     new CodeMatch(OpCodes.Ldfld, isStationField));

            var instruction = matcher.Instruction;

            matcher.Advance(2).InsertAndAdvance(new CodeInstruction(instruction), new CodeInstruction(OpCodes.Ldfld, isAssemblerField),
                                                new CodeInstruction(OpCodes.Or));

            return matcher.InstructionEnumeration();
        }

        [HarmonyPatch(typeof(UISlotPicker), "Determine")]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> UISlotPicker_Determine_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var matcher = new CodeMatcher(instructions).MatchForward(false, new CodeMatch(OpCodes.Ldfld, EntityData_StationId_Field),
                                                                     new CodeMatch(OpCodes.Stloc_S), new CodeMatch(OpCodes.Ldloc_S),
                                                                     new CodeMatch(OpCodes.Ldc_I4_0), new CodeMatch(OpCodes.Ble));

            var pos = matcher.Pos;
            matcher.Advance(3).InsertAndAdvance(matcher.InstructionsInRange(pos - 5, pos - 1));
            matcher.InsertAndAdvance(new CodeInstruction(OpCodes.Ldfld, EntityData_AssemblerId_Field), new CodeInstruction(OpCodes.Or));
            return matcher.InstructionEnumeration();
        }

        #endregion

        #region IModCanSave

        internal static void Export(BinaryWriter w)
        {
            w.Write(_slotdata.Count);

            foreach (KeyValuePair<(int, int), SlotData[]> pair in _slotdata)
            {
                w.Write(pair.Key.Item1);
                w.Write(pair.Key.Item2);
                w.Write(pair.Value.Length);
                for (var i = 0; i < pair.Value.Length; i++)
                {
                    w.Write((int)pair.Value[i].dir);
                    w.Write(pair.Value[i].beltId);
                    w.Write(pair.Value[i].storageIdx);
                    w.Write(pair.Value[i].counter);
                }
            }
        }

        internal static void Import(BinaryReader r)
        {
            ReInitAll();

            var _slotdatacount = r.ReadInt32();

            for (var j = 0; j < _slotdatacount; j++)
            {
                var key = r.ReadInt32();
                var key2 = r.ReadInt32();
                var length = r.ReadInt32();
                var datas = new SlotData[length];
                for (var i = 0; i < length; i++)
                {
                    datas[i] = new SlotData
                               {
                                   dir = (IODir)r.ReadInt32(), beltId = r.ReadInt32(), storageIdx = r.ReadInt32(), counter = r.ReadInt32()
                               };
                }

                _slotdata.TryAdd((key, key2), datas);
            }
        }

        internal static void IntoOtherSave() => ReInitAll();

        private static void ReInitAll()
        {
            _slotdata = new ConcurrentDictionary<(int, int), SlotData[]>();
            TmpSandCount = 0;
        }

        public static void ExportPlanetData(int planetId, BinaryWriter w)
        {
            KeyValuePair<(int, int), SlotData[]>[] datas = _slotdata.Where(pair => pair.Key.Item1 == planetId).ToArray();

            w.Write(datas.Length);
            w.Write(planetId);
            foreach (KeyValuePair<(int, int), SlotData[]> pair in datas)
            {
                w.Write(pair.Key.Item2);
                w.Write(pair.Value.Length);
                for (var i = 0; i < pair.Value.Length; i++)
                {
                    w.Write((int)pair.Value[i].dir);
                    w.Write(pair.Value[i].beltId);
                    w.Write(pair.Value[i].storageIdx);
                    w.Write(pair.Value[i].counter);
                }
            }
        }

        public static void ImportPlanetData(BinaryReader r)
        {
            var count = r.ReadInt32();
            var planetId = r.ReadInt32();

            for (var j = 0; j < count; j++)
            {
                var entityId = r.ReadInt32();
                var length = r.ReadInt32();
                var datas = new SlotData[length];
                for (var i = 0; i < length; i++)
                {
                    datas[i] = new SlotData
                               {
                                   dir = (IODir)r.ReadInt32(), beltId = r.ReadInt32(), storageIdx = r.ReadInt32(), counter = r.ReadInt32()
                               };
                }

                _slotdata[(planetId, entityId)] = datas;
            }
        }

        #endregion
    }
}
