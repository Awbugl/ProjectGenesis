using HarmonyLib;

// ReSharper disable InconsistentNaming

namespace ProjectGenesis.Patches
{
    public static partial class QuantumStoragePatches
    {
        [HarmonyPatch(typeof(BuildingParameters), nameof(BuildingParameters.ApplyPrebuildParametersToEntity))]
        [HarmonyPrefix]
        public static bool BuildingParameters_ApplyPrebuildParametersToEntity_Prefix(int entityId, int[] parameters, PlanetFactory factory)
        {
            if (entityId <= 0 || factory.entityPool[entityId].id != entityId) return false;

            int storageId = factory.entityPool[entityId].storageId;
            if (storageId == 0) return true;

            if (parameters == null || parameters.Length <= 2) return false;

            StorageComponent storageComponent = factory.factoryStorage.storagePool[storageId];

            if (storageComponent.size == QuantumStorageSize) QuantumStorageOrbitChange(factory.planetId, storageId, parameters[2]);

            return true;
        }

        [HarmonyPatch(typeof(BuildingParameters), nameof(BuildingParameters.FromParamsArray))]
        [HarmonyPostfix]
        public static void BuildingParameters_FromParamsArray(ref BuildingParameters __instance, int[] _parameters)
        {
            if (__instance.type != BuildingType.Storage) return;

            if (_parameters == null) return;

            __instance.mode2 = _parameters[2];
        }

        [HarmonyPatch(typeof(BuildingParameters), nameof(BuildingParameters.ToParamsArray))]
        [HarmonyPostfix]
        public static void BuildingParameters_ToParamsArray(ref BuildingParameters __instance, ref int[] _parameters, ref int _paramCount)
        {
            if (__instance.type != BuildingType.Storage) return;

            if (_parameters == null) return;

            _parameters[2] = __instance.mode2;
        }

        [HarmonyPatch(typeof(BuildingParameters), nameof(BuildingParameters.CopyFromFactoryObject))]
        [HarmonyPostfix]
        public static void BuildingParameters_CopyFromFactoryObject_Postfix(ref BuildingParameters __instance, int objectId,
            PlanetFactory factory)
        {
            if (__instance.type != BuildingType.Storage) return;

            if (objectId > 0 && factory.entityPool[objectId].id == objectId)
            {
                int storageId = factory.entityPool[objectId].storageId;
                if (storageId == 0) return;

                StorageComponent storageComponent = factory.factoryStorage.storagePool[storageId];

                if (storageComponent.size == QuantumStorageSize) __instance.mode2 = QueryOrbitId(factory.planetId, storageId);
            }
        }

        [HarmonyPatch(typeof(BuildingParameters), nameof(BuildingParameters.PasteToFactoryObject))]
        [HarmonyPrefix]
        public static bool BuildingParameters_PasteToFactoryObject_Prefix(ref BuildingParameters __instance, int objectId,
            PlanetFactory factory)
        {
            if (__instance.type != BuildingType.Storage) return true;

            if (objectId <= 0 || factory.entityPool[objectId].id != objectId) return false;

            int storageId = factory.entityPool[objectId].storageId;
            if (storageId == 0) return true;

            StorageComponent storageComponent = factory.factoryStorage.storagePool[storageId];

            if (storageComponent.size == QuantumStorageSize) QuantumStorageOrbitChange(factory.planetId, storageId, __instance.mode2);

            return true;
        }
    }
}
