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
using UnityEngine.UI;
using ERecipeType_1 = ERecipeType;
using Object = UnityEngine.Object;

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

        private static SlotData[] GetSlots(int planetId, int entityId)
        {
            var id = (planetId, entityId);

            if (!_slotdata.ContainsKey(id) || _slotdata[id] == null) _slotdata[id] = new SlotData[12];

            return _slotdata[id];
        }

        #endregion

        #region MegaAssemblerPatches

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
                        var recipeType = LDB.recipes.Select(recipeId).Type;
                        var itemProto = LDB.items.Select(factory.entityPool[entityId].protoId);

                        if (recipeId == 0)
                            assembler.SetRecipe(0, factory.entitySignPool);
                        else if (recipeId > 0 &&
                                 ContainsRecipeType(itemProto.prefabDesc.assemblerRecipeType, recipeType) &&
                                 factory.gameData.history.RecipeUnlocked(recipeId))
                            assembler.SetRecipe(recipeId, factory.entitySignPool);
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
                    if (recipeProto != null)
                    {
                        __instance.recipeType = recipeProto.Type;
                    }
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
                    {
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
                        {
                            if (assembler.recipeId != __instance.recipeId)
                            {
                                var itemProto = LDB.items.Select(factory.entityPool[objectId].protoId);
                                if (itemProto != null &&
                                    itemProto.prefabDesc != null &&
                                    (__instance.recipeId == 0 ||
                                     __instance.recipeId > 0 &&
                                     ContainsRecipeType(itemProto.prefabDesc.assemblerRecipeType, __instance.recipeType) &&
                                     GameMain.history.RecipeUnlocked(__instance.recipeId)))
                                    __result = true;
                            }
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
                    SyncSlotsData.Sync(__instance.planetId, entityId, slotdata);
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
                    SyncSlotsData.Sync(__instance.planetId, entityId, slotdata);
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

                    SyncSlotsData.Sync(__instance.planetId, otherEntityId, slotdata);
                }
            }
        }

        [HarmonyPatch(typeof(FactorySystem), "GameTick", typeof(long), typeof(bool))]
        [HarmonyPatch(typeof(FactorySystem), "GameTick", typeof(long), typeof(bool), typeof(int), typeof(int), typeof(int))]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> FactorySystem_GameTick_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var matcher = new CodeMatcher(instructions).MatchForward(true, new CodeMatch(OpCodes.Ldloc_1), new CodeMatch(OpCodes.Ldloc_2),
                                                                     new CodeMatch(OpCodes.Call,
                                                                                   AccessTools.Method(typeof(AssemblerComponent), "InternalUpdate")));

            var matcher2 = matcher.Clone();
            matcher2.MatchBack(true, new CodeMatch(OpCodes.Ldarg_0),
                               new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(FactorySystem), "assemblerPool")),
                               new CodeMatch(OpCodes.Ldloc_S));

            var index1 = matcher2.Operand;
            var power1 = matcher2.Advance(2).Operand;

            matcher.Advance(1).InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_0),
                                                new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(FactorySystem), "assemblerPool")),
                                                new CodeInstruction(OpCodes.Ldloc_S, index1),
                                                new CodeInstruction(OpCodes.Ldelema, typeof(AssemblerComponent)),
                                                new CodeInstruction(OpCodes.Ldarg_0), new CodeInstruction(OpCodes.Ldloc_S, power1),
                                                new CodeInstruction(OpCodes.Call,
                                                                    AccessTools.Method(typeof(MegaAssemblerPatches),
                                                                                       "AssemblerComponent_InternalUpdate_T")));

            matcher.MatchForward(true, new CodeMatch(OpCodes.Ldloc_1), new CodeMatch(OpCodes.Ldloc_2),
                                 new CodeMatch(OpCodes.Call, AccessTools.Method(typeof(AssemblerComponent), "InternalUpdate")));

            var matcher3 = matcher.Clone();
            matcher3.MatchBack(true, new CodeMatch(OpCodes.Ldarg_0),
                               new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(FactorySystem), "assemblerPool")),
                               new CodeMatch(OpCodes.Ldloc_S));

            var index2 = matcher3.Operand;
            var power2 = matcher3.Advance(2).Operand;

            matcher.Advance(1).InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_0),
                                                new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(FactorySystem), "assemblerPool")),
                                                new CodeInstruction(OpCodes.Ldloc_S, index2),
                                                new CodeInstruction(OpCodes.Ldelema, typeof(AssemblerComponent)),
                                                new CodeInstruction(OpCodes.Ldarg_0), new CodeInstruction(OpCodes.Ldloc_S, power2),
                                                new CodeInstruction(OpCodes.Call,
                                                                    AccessTools.Method(typeof(MegaAssemblerPatches),
                                                                                       "AssemblerComponent_InternalUpdate_T")));


            return matcher.InstructionEnumeration();
        }

        public static void AssemblerComponent_InternalUpdate_T(ref AssemblerComponent __instance, FactorySystem factorySystem, float power)
        {
            // 巨型建筑效果
            if (power < 0.1f) return;

            if (__instance.speed >= TrashSpeed)
            {
                var factory = factorySystem.factory;
                SlotData[] slotdata = GetSlots(factory.planetId, __instance.entityId);

                var cargoTraffic = factory.cargoTraffic;
                SignData[] entitySignPool = factory.entitySignPool;

                var stationPilerLevel = GameMain.history.stationPilerLevel;

                UpdateInputSlots(ref __instance, factory, cargoTraffic, slotdata, entitySignPool);
                UpdateOutputSlots(ref __instance, cargoTraffic, slotdata, entitySignPool, stationPilerLevel);
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(AssemblerComponent), "InternalUpdate")]
        public static void AssemblerComponent_InternalUpdate(
            ref AssemblerComponent __instance,
            float power,
            int[] productRegister,
            int[] consumeRegister)
        {
            // 化工技术革新效果
            if (GameMain.history.TechUnlocked(1513))
                if (__instance.recipeType == (ERecipeType_1)Utils.ERecipeType.Chemical ||
                    __instance.recipeType == (ERecipeType_1)Utils.ERecipeType.Refine ||
                    __instance.recipeType == (ERecipeType_1)Utils.ERecipeType.高分子化工)
                    if (__instance.speed == 20000)
                        __instance.speed = 40000;
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
                        var entityId = traffic.beltPool[slotdata[index1].beltId].entityId;
                        var cargoPath = traffic.GetCargoPath(traffic.beltPool[slotdata[index1].beltId].segPathId);
                        if (cargoPath != null)
                        {
                            var index2 = slotdata[index1].storageIdx - 1;
                            var itemId = 0;
                            if (index2 >= 0)
                                if (index2 < __instance.products.Length)
                                {
                                    itemId = __instance.products[index2];
                                    if (itemId > 0 && __instance.produced[index2] > 0)
                                    {
                                        var num2 = __instance.produced[index2] < maxPilerCount ? __instance.produced[index2] : maxPilerCount;
                                        if (cargoPath.TryInsertItemAtHeadAndFillBlank(itemId, (byte)num2, 0)) __instance.produced[index2] -= num2;
                                    }
                                }

                            signPool[entityId].iconType = 1U;
                            signPool[entityId].iconId0 = (uint)itemId;
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
                                var itemId = factory.cargoTraffic.TryPickItemAtRear(slotdata[index].beltId, 0, null, out var stack, out _);

                                if (itemId > 0)
                                {
                                    var consumeRegister = GameMain.statistics.production.factoryStatPool[factory.index].consumeRegister;

                                    lock (consumeRegister)
                                    {
                                        consumeRegister[itemId] += stack;
                                    }

                                    TmpSandCount += stack;

                                    if (TmpSandCount > 1000 && GameMain.mainPlayer != null)
                                    {
                                        GameMain.mainPlayer.SetSandCount(GameMain.mainPlayer.sandCount + TmpSandCount * 20);
                                        TmpSandCount = 0;
                                    }
                                }
                            }
                            else
                            {
                                var itemId = cargoPath.TryPickItemAtRear(__instance.needs, out var needIdx, out var stack, out var inc);
                                if (needIdx >= 0)
                                {
                                    if (itemId > 0 && __instance.needs[needIdx] == itemId)
                                    {
                                        __instance.served[needIdx] += stack;
                                        __instance.incServed[needIdx] += inc;
                                    }
                                }
                                else if (itemId > 0)
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
                    if (dataArray[index1].GridIndex >= 1101 && history.RecipeUnlocked(dataArray[index1].ID))
                        if (___filter == ERecipeType_1.None || ContainsRecipeType(___filter, dataArray[index1].Type))
                        {
                            var num1 = dataArray[index1].GridIndex / 1000;
                            var num2 = (dataArray[index1].GridIndex - num1 * 1000) / 100 - 1;
                            var num3 = dataArray[index1].GridIndex % 100 - 1;
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

        [HarmonyPatch(typeof(UIBeltBuildTip), "SetOutputEntity")]
        [HarmonyPrefix]
        public static bool UIBeltBuildTip_SetOutputEntity(
            ref UIBeltBuildTip __instance,
            ref List<int> ___filterItems,
            ref List<Image> ___iconImages,
            ref List<Text> ___iconNumbers,
            ref GameHistoryData ___history,
            int entityId,
            int slot)
        {
            if (__instance.outputEntityId == entityId && slot == __instance.outputSlotId) return false;

            __instance.outputEntityId = entityId;
            __instance.outputSlotId = slot;

            if (___filterItems == null)
            {
                ___filterItems = new List<int>();
                ___iconImages = new List<Image>();
                ___iconNumbers = new List<Text>();
            }

            ___filterItems.Clear();
            ___filterItems.Add(0);

            if (entityId <= 0)
            {
                __instance.outputSlotId = -1;
                __instance.filterGroup.SetActive(false);
            }
            else if (__instance.outputSlotId < 0)
            {
                __instance.outputSlotId = -1;
                __instance.filterGroup.SetActive(false);
            }
            else
            {
                var factory = GameMain.mainPlayer.factory;
                if (factory != null)
                {
                    var entityData = factory.entityPool[entityId];

                    if (entityData.assemblerId > 0)
                    {
                        var assemblerComponent = factory.factorySystem.assemblerPool[entityData.assemblerId];

                        if (assemblerComponent.speed >= TrashSpeed)
                        {
                            var assemblerComponentProducts = assemblerComponent.products;
                            if (assemblerComponentProducts != null && assemblerComponentProducts.Length > 0)
                            {
                                ___filterItems.AddRange(assemblerComponentProducts);
                            }
                            else
                            {
                                ___filterItems.Add(0);
                                ___filterItems.Add(0);
                                ___filterItems.Add(0);
                                ___filterItems.Add(0);
                            }

                            __instance.selectedIndex = 0;
                            if (slot >= 0 && slot < 12)
                                __instance.selectedIndex = GetSlots(factory.planetId, entityId)[slot].storageIdx;
                            else
                                Assert.CannotBeReached();

                            if (entityData.stationId > 0) entityData.stationId = 0;
                        }
                    }

                    if (entityData.stationId > 0)
                    {
                        var stationComponent = factory.transport.stationPool[entityData.stationId];
                        Assert.NotNull(stationComponent);
                        if (stationComponent != null && !stationComponent.isVeinCollector)
                        {
                            // ReSharper disable once LoopCanBeConvertedToQuery
                            foreach (var stationStore in stationComponent.storage) ___filterItems.Add(stationStore.itemId);

                            if (stationComponent.isStellar && ___history.logisticShipWarpDrive) ___filterItems.Add(1210);
                            __instance.selectedIndex = 0;
                            if (slot >= 0 && slot < stationComponent.slots.Length)
                                __instance.selectedIndex = stationComponent.slots[slot].storageIdx;
                            else
                                Assert.CannotBeReached();
                        }
                    }
                }

                if (__instance.selectedIndex < 0)
                    __instance.selectedIndex = 0;
                else if (__instance.selectedIndex >= ___filterItems.Count) __instance.selectedIndex = ___filterItems.Count - 1;
                for (var index = 0; index < ___filterItems.Count; ++index)
                {
                    var itemProto = LDB.items.Select(___filterItems[index]);
                    var sprite = itemProto?.iconSprite;
                    Image image;
                    Text text;
                    if (index < ___iconImages.Count)
                    {
                        image = ___iconImages[index];
                        text = ___iconNumbers[index];
                    }
                    else
                    {
                        image = Object.Instantiate(__instance.iconImageProto, __instance.iconImageProto.transform.parent);
                        ___iconImages.Add(image);
                        text = Object.Instantiate(__instance.iconNumberProto, __instance.iconNumberProto.transform.parent);
                        ___iconNumbers.Add(text);
                    }

                    image.gameObject.SetActive(true);
                    image.enabled = itemProto != null;
                    image.sprite = sprite;
                    image.rectTransform.anchoredPosition = new Vector2(index % 3 * 50 + 13, index / 3 * -50 - 37);
                    text.gameObject.SetActive(true);
                    text.enabled = index > 0;
                    text.alignment = itemProto != null ? TextAnchor.UpperRight : TextAnchor.MiddleCenter;
                    text.fontSize = itemProto != null ? 12 : 25;
                    text.text = "#" + index;
                    text.rectTransform.anchoredPosition = new Vector2(index % 3 * 50 + 13, index / 3 * -50 - 37);
                }

                for (var count = ___filterItems.Count; count < ___iconImages.Count; ++count) ___iconImages[count].gameObject.SetActive(false);
                for (var count = ___filterItems.Count; count < ___iconNumbers.Count; ++count) ___iconNumbers[count].gameObject.SetActive(false);
                __instance.filterGroup.SetActive(___filterItems.Count > 1);
                __instance.filterTrans.sizeDelta = new Vector2(50 * (___filterItems.Count > 3 ? 3 : ___filterItems.Count) + 16,
                                                               50 * ((___filterItems.Count - 1) / 3 + 1) + 64);
            }

            return false;
        }

        [HarmonyPatch(typeof(UIBeltBuildTip), "SetFilterToEntity")]
        [HarmonyPrefix]
        public static bool UIBeltBuildTip_SetFilterToEntity(ref UIBeltBuildTip __instance, ref List<int> ___filterItems)
        {
            if (__instance.outputEntityId <= 0 || __instance.outputSlotId < 0) return false;

            var factory = GameMain.mainPlayer.factory;
            if (factory == null) return false;
            var entityData = factory.entityPool[__instance.outputEntityId];

            if (entityData.assemblerId > 0)
                if (__instance.outputSlotId < 12)
                {
                    var assemblerComponent = factory.factorySystem.assemblerPool[entityData.assemblerId];

                    if (assemblerComponent.speed >= TrashSpeed)
                    {
                        SlotData[] slotDatas = GetSlots(factory.planetId, __instance.outputEntityId);
                        slotDatas[__instance.outputSlotId].storageIdx = __instance.selectedIndex;
                        SyncSlotsData.Sync(factory.planetId, __instance.outputEntityId, slotDatas);
                        if (entityData.stationId > 0) entityData.stationId = 0;
                    }
                }

            if (entityData.stationId > 0)
            {
                var stationComponent = factory.transport.stationPool[entityData.stationId];
                Assert.NotNull(stationComponent);
                if (stationComponent == null || __instance.outputSlotId >= stationComponent.slots.Length) return false;
                if (stationComponent.isVeinCollector)
                    stationComponent.slots[__instance.outputSlotId].storageIdx = 1;
                else
                    stationComponent.slots[__instance.outputSlotId].storageIdx = __instance.selectedIndex;
            }

            return false;
        }

        [HarmonyPatch(typeof(UISlotPicker), "SetOutputEntity")]
        [HarmonyPrefix]
        public static bool UISlotPicker_SetOutputEntity(
            ref UISlotPicker __instance,
            int entityId,
            int slot,
            ref List<int> ___filterItems,
            ref List<UIButton> ___iconButtons,
            ref List<Image> ___iconImages,
            ref List<Text> ___iconNumbers,
            ref GameData ___gameData,
            ref GameHistoryData ___history)
        {
            if (___filterItems == null || ___filterItems.Count == 0)
            {
                ___filterItems = new List<int>();
                ___iconButtons = new List<UIButton>();
                ___iconImages = new List<Image>();
                ___iconNumbers = new List<Text>();
            }

            if (___gameData.localPlanet == null || ___gameData.localPlanet.factory == null)
            {
                entityId = 0;
                slot = -1;
                __instance.outputEntityId = entityId;
                __instance.outputSlotId = slot;
                ___filterItems.Clear();
                ___filterItems.Add(0);
                __instance._Close();
            }
            else
            {
                if (__instance.outputEntityId == entityId && slot == __instance.outputSlotId) return false;
                var factory = GameMain.mainPlayer.factory;
                if (factory == null) return false;
                __instance.outputEntityId = entityId;
                __instance.outputSlotId = slot;
                ___filterItems.Clear();
                ___filterItems.Add(0);
                if (entityId <= 0)
                {
                    __instance.outputSlotId = -1;
                    __instance._Close();
                }
                else if (__instance.outputSlotId < 0)
                {
                    __instance.outputSlotId = -1;
                    __instance._Close();
                }
                else
                {
                    var entityData = factory.entityPool[__instance.outputEntityId];

                    if (entityData.assemblerId > 0)
                    {
                        var assemblerComponent = factory.factorySystem.assemblerPool[entityData.assemblerId];

                        if (assemblerComponent.speed >= TrashSpeed)
                        {
                            var assemblerComponentProducts = assemblerComponent.products;
                            if (assemblerComponentProducts != null && assemblerComponentProducts.Length > 0)
                                ___filterItems.AddRange(assemblerComponentProducts);
                            else
                                ___filterItems.AddRange(Enumerable.Repeat(0, 4));

                            __instance.selectedIndex = 0;
                            if (slot >= 0 && slot < 12)
                                __instance.selectedIndex = GetSlots(factory.planetId, entityId)[slot].storageIdx;
                            else
                                Assert.CannotBeReached();

                            entityData.stationId = 0;
                        }
                    }

                    if (entityData.stationId > 0)
                    {
                        var stationComponent = factory.transport.stationPool[entityData.stationId];
                        Assert.NotNull(stationComponent);
                        if (stationComponent != null)
                        {
                            // ReSharper disable once LoopCanBeConvertedToQuery
                            foreach (var stationStore in stationComponent.storage) ___filterItems.Add(stationStore.itemId);
                            if (stationComponent.isStellar && ___history.logisticShipWarpDrive) ___filterItems.Add(1210);

                            __instance.selectedIndex = 0;
                            if (slot >= 0 && slot < stationComponent.slots.Length)
                                __instance.selectedIndex = stationComponent.slots[slot].storageIdx;
                            else
                                Assert.CannotBeReached();
                        }
                    }


                    if (__instance.selectedIndex < 0)
                        __instance.selectedIndex = 0;

                    else if (__instance.selectedIndex >= ___filterItems.Count) __instance.selectedIndex = ___filterItems.Count - 1;
                    for (var index = 0; index < ___filterItems.Count; ++index)
                    {
                        var itemProto = LDB.items.Select(___filterItems[index]);
                        var sprite = itemProto?.iconSprite;
                        UIButton uiButton;
                        Image image;
                        Text text;
                        if (index < ___iconImages.Count)
                        {
                            uiButton = ___iconButtons[index];
                            image = ___iconImages[index];
                            text = ___iconNumbers[index];
                        }
                        else
                        {
                            uiButton = Object.Instantiate(__instance.iconButtonProto, __instance.iconButtonProto.transform.parent);
                            uiButton.onClick += __instance.OnIconButtonClick;
                            ___iconButtons.Add(uiButton);
                            image = Object.Instantiate(__instance.iconImageProto, __instance.iconImageProto.transform.parent);
                            ___iconImages.Add(image);
                            text = Object.Instantiate(__instance.iconNumberProto, __instance.iconNumberProto.transform.parent);
                            ___iconNumbers.Add(text);
                        }

                        var idx = index;
                        Vector2 itemPos;
                        if (idx == 0)
                        {
                            itemPos = ___filterItems.Count > 3 ? new Vector2(55f, -5f) : new Vector2(55f, 45f);
                        }
                        else
                        {
                            --idx;
                            itemPos = ___filterItems.Count > 4 ? new Vector2(idx % 3 * 50 - 45, idx / 3 * -50 + 95) : new Vector2(idx * 50 - 45, 45f);
                        }

                        uiButton.gameObject.SetActive(true);
                        uiButton.data = index;
                        ((RectTransform)uiButton.transform).anchoredPosition = itemPos;
                        image.gameObject.SetActive(true);
                        image.enabled = itemProto != null;
                        image.sprite = sprite;
                        image.rectTransform.anchoredPosition = itemPos;
                        text.gameObject.SetActive(true);
                        text.enabled = index > 0;
                        text.alignment = itemProto != null ? TextAnchor.UpperRight : TextAnchor.MiddleCenter;
                        text.fontSize = itemProto != null ? 12 : 25;
                        text.text = "#" + index;
                        text.rectTransform.anchoredPosition = itemPos;
                        if (index == 0) __instance.noneText.rectTransform.anchoredPosition = itemPos;
                    }

                    for (var count = ___filterItems.Count; count < ___iconButtons.Count; ++count) ___iconButtons[count].gameObject.SetActive(false);
                    for (var count = ___filterItems.Count; count < ___iconImages.Count; ++count) ___iconImages[count].gameObject.SetActive(false);
                    for (var count = ___filterItems.Count; count < ___iconNumbers.Count; ++count) ___iconNumbers[count].gameObject.SetActive(false);
                    __instance.expand = false;
                    __instance.itemGroup.SetActive(false);
                    __instance.bgTrans.sizeDelta
                        = __instance.expand ? new Vector2(158f, ___filterItems.Count > 4 ? 158f : 108f) : new Vector2(50f, 50f);
                }
            }

            return false;
        }

        [HarmonyPatch(typeof(UISlotPicker), "SetFilterToEntity")]
        [HarmonyPrefix]
        public static bool UISlotPicker_SetFilterToEntity(ref UISlotPicker __instance)
        {
            if (__instance.outputEntityId <= 0 || __instance.outputSlotId < 0) return false;
            var factory = GameMain.mainPlayer.factory;
            if (factory == null) return false;
            var entityData = factory.entityPool[__instance.outputEntityId];


            if (entityData.assemblerId > 0)
                if (__instance.outputSlotId < 12)
                {
                    var assemblerComponent = factory.factorySystem.assemblerPool[entityData.assemblerId];

                    if (assemblerComponent.speed >= TrashSpeed)
                    {
                        SlotData[] slotDatas = GetSlots(factory.planetId, __instance.outputEntityId);
                        slotDatas[__instance.outputSlotId].storageIdx = __instance.selectedIndex;
                        SyncSlotsData.Sync(factory.planetId, __instance.outputEntityId, slotDatas);
                        if (entityData.stationId > 0) entityData.stationId = 0;
                    }
                }

            if (entityData.stationId > 0)
            {
                var stationComponent = factory.transport.stationPool[entityData.stationId];
                Assert.NotNull(stationComponent);


                if (stationComponent != null && __instance.outputSlotId < stationComponent.slots.Length)
                    stationComponent.slots[__instance.outputSlotId].storageIdx = __instance.selectedIndex;
            }

            return false;
        }

        #endregion

        #region DeterminePatch

        private static readonly FieldInfo isStationField = AccessTools.Field(typeof(PrefabDesc), nameof(PrefabDesc.isStation));
        private static readonly FieldInfo isAssemblerField = AccessTools.Field(typeof(PrefabDesc), nameof(PrefabDesc.isAssembler));

        [HarmonyPatch(typeof(BuildTool_Path), "DeterminePreviews")]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> BuildTool_Path_DeterminePreviews_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> codes = instructions.ToList();

            var index = codes.FindIndex(code => code.LoadsField(isStationField));

            return codes.Take(index + 1).Append(new CodeInstruction(codes[index - 1])).Append(new CodeInstruction(OpCodes.Ldfld, isAssemblerField))
                        .Append(new CodeInstruction(OpCodes.Or)).Concat(codes.Skip(index + 1));
        }

        private static readonly FieldInfo StationIdField = AccessTools.Field(typeof(EntityData), nameof(EntityData.stationId));
        private static readonly FieldInfo AssemblerIdField = AccessTools.Field(typeof(EntityData), nameof(EntityData.assemblerId));

        [HarmonyPatch(typeof(UISlotPicker), "Determine")]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> UISlotPicker_Determine_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> codes = instructions.ToList();

            var index = codes.FindIndex(code => code.LoadsField(StationIdField));

            if (codes[index + 1].opcode == OpCodes.Stloc_S &&
                codes[index + 2].opcode == OpCodes.Ldloc_S &&
                codes[index + 1].operand == codes[index + 2].operand)
            {
                CodeInstruction[] result = codes.Take(index + 1).Concat(codes.Skip(index - 5).Take(5))
                                                .Append(new CodeInstruction(OpCodes.Ldfld, AssemblerIdField)).Append(new CodeInstruction(OpCodes.Or))
                                                .Concat(codes.Skip(index + 1)).ToArray();


                return result;
            }

            return codes;
        }

        #endregion

        #region IModCanSave

        internal static void Export(BinaryWriter w)
        {
            w.Write(_slotdata.Count);

            foreach (var (key, value) in _slotdata)
            {
                w.Write(key.Item1);
                w.Write(key.Item2);
                w.Write(value.Length);
                for (var i = 0; i < value.Length; i++)
                {
                    w.Write((int)value[i].dir);
                    w.Write(value[i].beltId);
                    w.Write(value[i].storageIdx);
                    w.Write(value[i].counter);
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

        #endregion
    }
}
