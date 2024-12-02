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

            ref PowerNodeComponent powerNodeComponent = ref powerSystem.nodePool[nodeId];

            powerNodeComponent.id = nodeId;
            powerNodeComponent.entityId = 0;
            powerNodeComponent.connectDistance = conn;
            powerNodeComponent.coverRadius = cover;
            powerNodeComponent.powerPoint = Pos;
            powerNodeComponent.isCharger = false;
            powerNodeComponent.workEnergyPerTick = 0;
            powerNodeComponent.idleEnergyPerTick = 0;

            OnNodeAdded(powerSystem, ref powerNodeComponent, nodeId);

            if (powerSystem.factory.planet.factoryLoaded) powerSystem.factory.planet.factoryModel.RefreshPowerNodes();
            return nodeId;
        }

        private static void OnNodeAdded(PowerSystem powerSystem, ref PowerNodeComponent powerNodeComponent, int nodeId)
        {
            Node node1 = new Node(nodeId)
            {
                connDistance2 = powerNodeComponent.connectDistance * powerNodeComponent.connectDistance,
                coverRadius2 = powerNodeComponent.coverRadius * powerNodeComponent.coverRadius,
            };

            for (int index1 = 1; index1 < powerSystem.netCursor; ++index1)
            {
                PowerNetwork powerNetwork = powerSystem.netPool[index1];

                if (powerNetwork == null || powerNetwork.id == 0) continue;

                List<Node> nodes = powerNetwork.nodes;

                foreach (Node n in nodes)
                {
                    powerSystem.list_sorted_add(node1.conns, n);
                    powerSystem.list_sorted_add(n.conns, node1);
                }
            }

            foreach (int consumer in powerSystem.netPool[0].consumers) powerSystem.list_sorted_add(node1.consumers, consumer);

            foreach (int consumer in node1.consumers) powerSystem.netPool[0].consumers.Remove(consumer);

            // ReSharper disable once ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
            foreach (Node conn in node1.conns)
                foreach (int consumer in conn.consumers)
                    powerSystem.list_sorted_add(node1.consumers, consumer);

            int network = node1.conns.Count > 0 ? powerSystem.nodePool[node1.conns[0].id].networkId : 0;
            if (network == 0) network = powerSystem.NewNetwork();

            PowerNetwork powerNetwork1 = powerSystem.netPool[network];

            powerSystem.list_sorted_add(powerNetwork1.nodes, node1);
            powerSystem.nodePool[nodeId].networkId = network;
            powerSystem.list_sorted_merge(powerNetwork1.consumers, node1.consumers);

            foreach (int consumer in node1.consumers) powerSystem.consumerPool[consumer].networkId = network;

            powerSystem._tmp_ints.Clear();

            // ReSharper disable once ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
            foreach (Node t in node1.conns)
            {
                int networkId = powerSystem.nodePool[t.id].networkId;
                Assert.Positive(networkId);
                if (networkId > 0 && networkId != network) powerSystem.list_sorted_add(powerSystem._tmp_ints, networkId);
            }

            foreach (int tmpInt in powerSystem._tmp_ints)
            {
                PowerNetwork powerNetwork2 = powerSystem.netPool[tmpInt];
                if (powerNetwork2 == null || powerNetwork2.id != tmpInt) { Assert.CannotBeReached(); }
                else
                {
                    foreach (Node node2 in powerNetwork2.nodes) powerSystem.nodePool[node2.id].networkId = network;
                    foreach (int consumer in powerNetwork2.consumers) powerSystem.consumerPool[consumer].networkId = network;
                    foreach (int generator in powerNetwork2.generators) powerSystem.genPool[generator].networkId = network;
                    foreach (int accumulator in powerNetwork2.accumulators) powerSystem.accPool[accumulator].networkId = network;
                    foreach (int exchanger in powerNetwork2.exchangers) powerSystem.excPool[exchanger].networkId = network;
                    powerSystem.list_sorted_merge(powerNetwork1.nodes, powerNetwork2.nodes);
                    powerSystem.list_sorted_merge(powerNetwork1.consumers, powerNetwork2.consumers);
                    powerSystem.list_sorted_merge(powerNetwork1.generators, powerNetwork2.generators);
                    powerSystem.list_sorted_merge(powerNetwork1.accumulators, powerNetwork2.accumulators);
                    powerSystem.list_sorted_merge(powerNetwork1.exchangers, powerNetwork2.exchangers);
                }
            }

            foreach (int t in powerSystem._tmp_ints) powerSystem.RemoveNetwork(t);

            powerSystem._tmp_ints.Clear();
        }

        [HarmonyPatch(typeof(UIPowerGizmo), nameof(UIPowerGizmo.DrawArea))]
        [HarmonyPatch(typeof(UIPowerGizmo), nameof(UIPowerGizmo.DrawCover))]
        [HarmonyPrefix]
        public static bool UIPowerGizmo_Draw(ref UIPowerGizmo __instance, Vector3 center, float radius) => radius < GlobalPowerCoverRadius;

        [HarmonyPatch(typeof(PowerSystemRenderer), nameof(PowerSystemRenderer.DrawDisks))]
        [HarmonyPatch(typeof(PowerSystemRenderer), nameof(PowerSystemRenderer.DrawConnections))]
        [HarmonyPatch(typeof(PowerSystemRenderer), nameof(PowerSystemRenderer.DrawArcs))]
        [HarmonyPrefix]
        public static bool PowerSystemRenderer_Draw_PreFix(PowerSystemRenderer __instance) =>
            !NodeIds.ContainsKey(__instance.powerSystem.planet.id);

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
