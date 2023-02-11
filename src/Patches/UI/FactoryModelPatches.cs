using System;
using HarmonyLib;
using PowerNetworkStructures;
using ProjectGenesis.Utils;
using UnityEngine;

// ReSharper disable InconsistentNaming
// ReSharper disable LoopCanBeConvertedToQuery
// ReSharper disable Unity.PreferAddressByIdToGraphicsParams

namespace ProjectGenesis.Patches.UI
{
    public static class FactoryModelPatches
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(UIPowerGizmo), "DrawArea")]
        [HarmonyPatch(typeof(UIPowerGizmo), "DrawCover")]
        public static bool UIPowerGizmo_Draw(ref UIPowerGizmo __instance, Vector3 center, float radius) => radius < 2000;

        private static Material atmosphericCollectStationMaterial;

        [HarmonyPatch(typeof(FactoryModel), "InitCollectorMaterial")]
        [HarmonyPostfix]
        public static void FactoryModel_InitCollectorMaterial(FactoryModel __instance)
        {
            var objectRenderer = __instance.gpuiManager.GetObjectRenderer(ProtoIDUsedByPatches.M大气采集器);
            if (objectRenderer != null) atmosphericCollectStationMaterial = objectRenderer.lodBatches[0].materials[2];
        }

        [HarmonyPatch(typeof(FactoryModel), "SetCollectorEffectColor")]
        [HarmonyPostfix]
        public static void FactoryModel_SetCollectorEffectColor(FactoryModel __instance)
        {
            if (!(atmosphericCollectStationMaterial != null)) return;
            atmosphericCollectStationMaterial.SetColor("_TintColor", __instance.collectorEffectColor);
        }

        [HarmonyPatch(typeof(FactoryModel), "OnCameraPostRender")]
        [HarmonyPrefix]
        public static bool FactoryModel_OnCameraPostRender(FactoryModel __instance)
        {
            if (GameMain.isPaused ||
                GameMain.inOtherScene ||
                __instance.planet == null ||
                !__instance.planet.factoryLoaded ||
                __instance.planet != GameMain.localPlanet)
                return false;

            foreach (var powerNodeComponent in __instance.planet.factory.powerSystem.nodePool)
            {
                if (powerNodeComponent.coverRadius < 2000) continue;

                if (PowerSystemRenderer.powerGraphOn || PowerSystemRenderer.forceConsumersOn || __instance.drawPowerConsumers)
                    __instance.powerSystemRenderer.DrawConsumers();

                if (!__instance.disableEntitySigns && EntitySignRenderer.entitySignOn) __instance.entitySignRenderer.Draw();

                return false;
            }

            return true;
        }

        [HarmonyPatch(typeof(PowerSystem), "line_arragement_for_add_node")]
        [HarmonyPrefix]
        public static void line_arragement_for_add_node_Prefix(PowerSystem __instance, Node node, ref int[] ___tmp_state)
        {
            var connsCount = node.conns.Count * 2;
            if (___tmp_state == null || ___tmp_state.Length < connsCount) Array.Resize(ref ___tmp_state, connsCount);
        }

        [HarmonyPatch(typeof(PowerSystem), "line_arragement_for_add_node")]
        [HarmonyPostfix]
        public static void line_arragement_for_add_node_Postfix(PowerSystem __instance, ref int[] ___tmp_state)
        {
            if (___tmp_state.Length != 1024) Array.Resize(ref ___tmp_state, 1024);
        }
    }
}
