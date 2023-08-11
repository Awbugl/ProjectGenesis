using System;
using HarmonyLib;
using ProjectGenesis.Utils;

// ReSharper disable InconsistentNaming

namespace ProjectGenesis.Patches.Logic.MegaAssembler
{
    internal static partial class MegaAssemblerPatches
    {
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
                if (assembler.id != assemblerId || assembler.speed < TrashSpeed || parameters == null || parameters.Length < 2048) return;

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

                assembler.forceAccMode = parameters[0] > 0;

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

        [HarmonyPostfix]
        [HarmonyPatch(typeof(BuildingParameters), "FromParamsArray")]
        public static void BuildingParameters_FromParamsArray(ref BuildingParameters __instance, int[] _parameters)
        {
            if (_parameters == null || _parameters.Length < 2048) return;

            if (__instance.parameters.Length < 2048) Array.Resize(ref __instance.parameters, 2048);

            Array.Copy(_parameters, __instance.parameters, 2048);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(BuildingParameters), "ToParamsArray")]
        public static void BuildingParameters_ToParamsArray(ref BuildingParameters __instance, ref int[] _parameters, ref int _paramCount)
        {
            if (__instance.type != BuildingType.Assembler) return;

            if (__instance.parameters.Length >= 2048)
            {
                if (_parameters == null || _parameters.Length < 2048) Array.Resize(ref _parameters, 2048);
                Array.Copy(__instance.parameters, _parameters, 2048);
                _paramCount = _parameters.Length;
            }
            else
            {
                _paramCount = __instance.parameters.Length;
                if (_parameters == null || _parameters.Length < _paramCount) Array.Resize(ref _parameters, _paramCount);
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
                if (factory.entityPool.Length <= objectId || factory.entityPool[objectId].id != objectId || __instance.type != BuildingType.Assembler)
                    return;

                var assemblerId = factory.entityPool[objectId].assemblerId;
                if (assemblerId <= 0 || factory.factorySystem.assemblerPool.Length <= assemblerId) return;

                var assembler = factory.factorySystem.assemblerPool[assemblerId];
                if (assembler.id != assemblerId || assembler.speed < TrashSpeed) return;

                if (__instance.parameters == null || __instance.parameters.Length < 2048) Array.Resize(ref __instance.parameters, 2048);

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
            else
            {
                var index2 = -objectId;
                PrebuildData[] prebuildPool = factory.prebuildPool;
                ref var prebuildData = ref prebuildPool[index2];
                if (index2 <= 0 || prebuildData.id != index2) return;

                if (prebuildData.parameters != null)
                {
                    var length = prebuildData.parameters.Length;
                    if (__instance.parameters == null || __instance.parameters.Length < length) Array.Resize(ref __instance.parameters, length);
                    Array.Copy(prebuildData.parameters, __instance.parameters, length);
                }

                __instance.recipeId = prebuildData.recipeId;
                var recipeProto = LDB.recipes.Select(prebuildData.recipeId);
                if (recipeProto != null) __instance.recipeType = recipeProto.Type;
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
                if (factory.entityPool.Length <= objectId || factory.entityPool[objectId].id != objectId || __instance.type != BuildingType.Assembler)
                    return;

                var assemblerId = factory.entityPool[objectId].assemblerId;
                if (assemblerId <= 0 || factory.factorySystem.assemblerPool.Length <= assemblerId) return;

                ref var assembler = ref factory.factorySystem.assemblerPool[assemblerId];
                if (assembler.id != assemblerId || assembler.speed < TrashSpeed) return;

                var itemProto = LDB.items.Select(factory.entityPool[objectId].protoId);
                if (itemProto == null || itemProto.prefabDesc == null) return;

                var containsRecipeType = ContainsRecipeType(itemProto.prefabDesc.assemblerRecipeType, __instance.recipeType);

                if (assembler.recipeId != __instance.recipeId &&
                    (__instance.recipeId == 0 ||
                     (__instance.recipeId > 0 && containsRecipeType && GameMain.history.RecipeUnlocked(__instance.recipeId))))
                {
                    factory.factorySystem.TakeBackItems_Assembler(GameMain.mainPlayer, assemblerId);
                    assembler.SetRecipe(__instance.recipeId, factory.entitySignPool);
                    __result = true;
                }

                if (__instance.parameters != null && __instance.parameters.Length >= 1 && containsRecipeType)
                    assembler.forceAccMode = __instance.parameters[0] > 0;
            }
            else
            {
                var index1 = -objectId;
                PrebuildData[] prebuildPool = factory.prebuildPool;

                ref var prebuildData = ref prebuildPool[index1];
                if (index1 <= 0 || prebuildData.id != index1) return;

                var itemProto = LDB.items.Select(prebuildData.protoId);
                if (itemProto == null || itemProto.prefabDesc == null) return;

                if (!itemProto.prefabDesc.isAssembler ||
                    __instance.type != BuildingType.Assembler ||
                    !ContainsRecipeType(itemProto.prefabDesc.assemblerRecipeType, __instance.recipeType))
                    return;

                if (__instance.parameters != null)
                {
                    var length = __instance.parameters.Length;
                    if (prebuildData.parameters == null || prebuildData.parameters.Length < length) Array.Resize(ref prebuildData.parameters, length);
                    Array.Copy(__instance.parameters, prebuildData.parameters, length);
                }

                prebuildData.recipeId = __instance.recipeId;
                prebuildData.filterId = __instance.filterId;
                __instance.ToParamsArray(ref prebuildData.parameters, ref prebuildData.paramCount);
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
                if (factory.entityPool.Length <= objectId || factory.entityPool[objectId].id != objectId || __instance.type != BuildingType.Assembler)
                    return;

                var assemblerId = factory.entityPool[objectId].assemblerId;
                if (assemblerId <= 0 || factory.factorySystem.assemblerPool.Length <= assemblerId) return;

                var assembler = factory.factorySystem.assemblerPool[assemblerId];
                if (assembler.id != assemblerId || assembler.speed < TrashSpeed || assembler.recipeId == __instance.recipeId) return;

                var itemProto = LDB.items.Select(factory.entityPool[objectId].protoId);
                if (itemProto != null &&
                    itemProto.prefabDesc != null &&
                    (__instance.recipeId == 0 ||
                     (__instance.recipeId > 0 &&
                      ContainsRecipeType(itemProto.prefabDesc.assemblerRecipeType, __instance.recipeType) &&
                      GameMain.history.RecipeUnlocked(__instance.recipeId))))
                    __result = true;
            }
            else
            {
                var index2 = -objectId;
                PrebuildData[] prebuildPool = factory.prebuildPool;
                if (index2 <= 0 || prebuildPool[index2].id != index2) return;

                var itemProto = LDB.items.Select(prebuildPool[index2].protoId);
                if (itemProto != null &&
                    itemProto.prefabDesc != null &&
                    itemProto.prefabDesc.isAssembler &&
                    __instance.type == BuildingType.Assembler &&
                    ContainsRecipeType(itemProto.prefabDesc.assemblerRecipeType, __instance.recipeType))
                    __result = true;
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
    }
}
