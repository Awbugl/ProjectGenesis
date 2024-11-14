using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using HarmonyLib;
using PowerNetworkStructures;
using ProjectGenesis.Utils;
using UnityEngine;

// ReSharper disable InconsistentNaming

namespace ProjectGenesis.Patches.Logic
{
    public static class GlobalPowerSupplyPatches
    {
        private const int GlobalPowerCoverRadius = 1000;

        private static readonly Vector3 Pos = new Vector3(0.0f, 0.0f, 0.01f);

        private static readonly ConcurrentDictionary<int, List<int>> NodeIds = new ConcurrentDictionary<int, List<int>>();

        [HarmonyPatch(typeof(PlanetATField), nameof(PlanetATField.GameTick))]
        [HarmonyPostfix]
        public static void PlanetATField_GameTick_Postfix(PlanetATField __instance)
        {
            if (!__instance.gameData.history.TechUnlocked(ProtoID.T护盾载波调制)) return;

            int planetId = __instance.planet.id;
            PowerSystem factoryPowerSystem = __instance.factory.powerSystem;

            if (__instance.isSpherical)
            {
                if (NodeIds.ContainsKey(planetId)) return;

                int nodeId = NewNodeComponent(factoryPowerSystem, 400, GlobalPowerCoverRadius);

                NodeIds.TryAddOrInsert(planetId, nodeId);

                SyncGlobalPowerSupplyNodeIdData.Sync(planetId, nodeId);
            }
            else if (NodeIds.TryGetValue(planetId, out List<int> list))
            {
                SyncSyncGlobalPowerSupplyRemoveData.Sync(planetId);

                // ReSharper disable once ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
                foreach (int nodeId in list)
                    if (factoryPowerSystem.nodePool[nodeId].powerPoint == Pos)
                        factoryPowerSystem.RemoveNodeComponent(nodeId);

                NodeIds.Remove(planetId, out _);
            }
        }

        internal static void SyncGlobalPowerSupplyNodeId(int planetId, int nodeId) => NodeIds.TryAddOrInsert(planetId, nodeId);

        internal static void SyncGlobalPowerSupplyRemoved(int planetId)
        {
            if (!NodeIds.TryRemove(planetId, out List<int> list)) return;

            PlanetData planet = GameMain.galaxy.PlanetById(planetId);
            PlanetFactory factory = GameMain.data.GetOrCreateFactory(planet);

            foreach (int nodeId in list) factory.powerSystem.RemoveNodeComponent(nodeId);
        }

        private static int NewNodeComponent(PowerSystem powerSystem, float conn, float cover)
        {
            int nodeId;
            if (powerSystem.nodeRecycleCursor > 0) { nodeId = powerSystem.nodeRecycle[--powerSystem.nodeRecycleCursor]; }
            else
            {
                nodeId = powerSystem.nodeCursor++;
                if (nodeId == powerSystem.nodeCapacity) powerSystem.SetNodeCapacity(powerSystem.nodeCapacity * 2);
            }

            powerSystem.nodePool[nodeId].id = nodeId;
            powerSystem.nodePool[nodeId].entityId = 0;
            powerSystem.nodePool[nodeId].connectDistance = conn;
            powerSystem.nodePool[nodeId].coverRadius = cover;
            powerSystem.nodePool[nodeId].powerPoint = Pos;
            powerSystem.nodePool[nodeId].isCharger = false;
            powerSystem.nodePool[nodeId].workEnergyPerTick = 0;
            powerSystem.nodePool[nodeId].idleEnergyPerTick = 0;
            powerSystem.OnNodeAdded(nodeId);
            if (powerSystem.factory.planet.factoryLoaded) powerSystem.factory.planet.factoryModel.RefreshPowerNodes();
            return nodeId;
        }

        [HarmonyPatch(typeof(UIPowerGizmo), nameof(UIPowerGizmo.DrawArea))]
        [HarmonyPatch(typeof(UIPowerGizmo), nameof(UIPowerGizmo.DrawCover))]
        [HarmonyPrefix]
        public static bool UIPowerGizmo_Draw(ref UIPowerGizmo __instance, Vector3 center, float radius) => radius < GlobalPowerCoverRadius;

        [HarmonyPatch(typeof(FactoryModel), nameof(FactoryModel.OnCameraPostRender))]
        [HarmonyPrefix]
        public static bool FactoryModel_OnCameraPostRender(FactoryModel __instance)
        {
            if (GameMain.isPaused || GameMain.inOtherScene || __instance.planet == null || !__instance.planet.factoryLoaded
             || __instance.planet != GameMain.localPlanet)
                return false;

            // ReSharper disable once LoopCanBeConvertedToQuery
            foreach (PowerNodeComponent powerNodeComponent in __instance.planet.factory.powerSystem.nodePool)
            {
                if (powerNodeComponent.coverRadius < GlobalPowerCoverRadius) continue;

                if (PowerSystemRenderer.powerGraphOn || PowerSystemRenderer.forceConsumersOn || __instance.drawPowerConsumers)
                    __instance.powerSystemRenderer.DrawConsumers();

                if (!__instance.disableEntitySigns && EntitySignRenderer.entitySignOn) __instance.entitySignRenderer.Draw();

                return false;
            }

            return true;
        }

        [HarmonyPatch(typeof(PowerSystem), nameof(PowerSystem.line_arragement_for_add_node))]
        [HarmonyPatch(typeof(PowerSystem), nameof(PowerSystem.line_arragement_for_remove_node))]
        [HarmonyPrefix]
        public static bool PowerSystem_line_arragement_for_node_Prefix(PowerSystem __instance, Node node) =>
            __instance.planet.factory.powerSystem.nodePool[node.id].coverRadius < GlobalPowerCoverRadius;

        [HarmonyPatch(typeof(PowerNodeComponent), nameof(PowerNodeComponent.Import))]
        [HarmonyPostfix]
        public static void PowerNodeComponent_Import_Postfix(PowerNodeComponent __instance)
        {
            if (__instance.powerPoint == Vector3.zero) __instance.powerPoint = Pos;
        }

        internal static void Export(BinaryWriter w)
        {
            lock (NodeIds)
            {
                w.Write(NodeIds.Count);

                foreach (var (key, value) in NodeIds)
                {
                    w.Write(key);
                    w.Write(value.Count);
                    foreach (int nodeId in value) w.Write(nodeId);
                }
            }
        }

        internal static void Import(BinaryReader r)
        {
            ReInitAll();

            try
            {
                int nodeCount = r.ReadInt32();

                for (var i = 0; i < nodeCount; i++)
                {
                    int planetId = r.ReadInt32();

                    int count = r.ReadInt32();

                    var list = new List<int>();

                    for (var j = 0; j < count; j++) list.Add(r.ReadInt32());

                    NodeIds.TryAdd(planetId, list);
                }
            }
            catch (EndOfStreamException)
            {
                // ignored
            }
        }

        internal static void IntoOtherSave() => ReInitAll();

        private static void ReInitAll() => NodeIds.Clear();
    }
}
