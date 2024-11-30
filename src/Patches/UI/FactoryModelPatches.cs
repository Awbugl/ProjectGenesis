using HarmonyLib;
using ProjectGenesis.Utils;
using UnityEngine;

// ReSharper disable InconsistentNaming
// ReSharper disable LoopCanBeConvertedToQuery
// ReSharper disable Unity.PreferAddressByIdToGraphicsParams

namespace ProjectGenesis.Patches.UI
{
    public static class FactoryModelPatches
    {
        private static Material atmosphericCollectStationMaterial;

        [HarmonyPatch(typeof(FactoryModel), nameof(FactoryModel.InitMaterial))]
        [HarmonyPostfix]
        public static void FactoryModel_InitCollectorMaterial(FactoryModel __instance)
        {
            ObjectRenderer objectRenderer = __instance.gpuiManager.GetObjectRenderer(ProtoID.M大气采集器);

            if (objectRenderer != null) atmosphericCollectStationMaterial = objectRenderer.lodBatches[0].materials[2];
        }

        [HarmonyPatch(typeof(FactoryModel), nameof(FactoryModel.SetCollectorEffectColor))]
        [HarmonyPostfix]
        public static void FactoryModel_SetCollectorEffectColor(FactoryModel __instance)
        {
            if (!(atmosphericCollectStationMaterial != null)) return;

            atmosphericCollectStationMaterial.SetColor("_TintColor", __instance.collectorEffectColor);
        }
    }
}
