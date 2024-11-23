using System.Collections.Generic;
using System.IO;
using NebulaAPI;
using NebulaAPI.Interfaces;
using NebulaAPI.Networking;
using NebulaAPI.Packets;
using ProjectGenesis.Patches.Logic;
using ProjectGenesis.Patches.Logic.MegaAssembler;
using ProjectGenesis.Patches.Logic.PlanetFocus;
using ProjectGenesis.Patches.Logic.QuantumStorage;

// ReSharper disable MemberCanBeInternal
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable ClassNeverInstantiated.Global

namespace ProjectGenesis.Utils
{
    public class SyncSlotsData
    {
        public SyncSlotsData() {}

        public SyncSlotsData(int planetId, int entityId, SlotData[] data)
        {
            PlanetId = planetId;
            EntityId = entityId;

            using (IWriterProvider p = NebulaModAPI.GetBinaryWriter())
            {
                BinaryWriter w = p.BinaryWriter;

                w.Write(data.Length);

                for (var i = 0; i < data.Length; i++)
                {
                    w.Write((int)data[i].dir);
                    w.Write(data[i].beltId);
                    w.Write(data[i].storageIdx);
                    w.Write(data[i].counter);
                }

                SlotsData = p.CloseAndGetBytes();
            }
        }

        public int PlanetId { get; set; }
        public int EntityId { get; set; }
        public byte[] SlotsData { get; set; }

        internal static void Sync(int planetId, int entityId, SlotData[] slotsData)
        {
            if (NebulaModAPI.IsMultiplayerActive)
                NebulaModAPI.MultiplayerSession.Network.SendPacket(new SyncSlotsData(planetId, entityId, slotsData));
        }

        internal static void OnReceive(int planetId, int entityId, byte[] data)
        {
            SlotData[] slotsData;

            using (IReaderProvider p = NebulaModAPI.GetBinaryReader(data))
            using (BinaryReader r = p.BinaryReader)
            {
                int length = r.ReadInt32();
                slotsData = new SlotData[length];

                for (var i = 0; i < length; i++)
                {
                    slotsData[i] = new SlotData
                    {
                        dir = (IODir)r.ReadInt32(),
                        beltId = r.ReadInt32(),
                        storageIdx = r.ReadInt32(),
                        counter = r.ReadInt32(),
                    };
                }

                p.BinaryReader.Close();
            }

            MegaAssemblerPatches.SyncSlots((planetId, entityId), slotsData);
        }
    }

    [RegisterPacketProcessor]
    public class SyncSlotsDataProcessor : BasePacketProcessor<SyncSlotsData>
    {
        public override void ProcessPacket(SyncSlotsData packet, INebulaConnection conn) =>
            SyncSlotsData.OnReceive(packet.PlanetId, packet.EntityId, packet.SlotsData);
    }

    public class SyncSlotData
    {
        public SyncSlotData() {}

        public SyncSlotData(int planetId, int slotId, int entityId, SlotData data)
        {
            PlanetId = planetId;
            SlotId = slotId;
            EntityId = entityId;

            using (IWriterProvider p = NebulaModAPI.GetBinaryWriter())
            {
                BinaryWriter w = p.BinaryWriter;

                w.Write((int)data.dir);
                w.Write(data.beltId);
                w.Write(data.storageIdx);
                w.Write(data.counter);

                SlotData = p.CloseAndGetBytes();
            }
        }

        public int PlanetId { get; set; }
        public int SlotId { get; set; }
        public int EntityId { get; set; }

        public byte[] SlotData { get; set; }

        internal static void Sync(int planetId, int slotId, int entityId, SlotData slotData)
        {
            if (NebulaModAPI.IsMultiplayerActive)
                NebulaModAPI.MultiplayerSession.Network.SendPacket(new SyncSlotData(planetId, slotId, entityId, slotData));
        }

        internal static void OnReceive(int planetId, int slotId, int entityId, byte[] data)
        {
            SlotData slotData;

            using (IReaderProvider p = NebulaModAPI.GetBinaryReader(data))
            using (BinaryReader r = p.BinaryReader)
            {
                slotData = new SlotData
                {
                    dir = (IODir)r.ReadInt32(),
                    beltId = r.ReadInt32(),
                    storageIdx = r.ReadInt32(),
                    counter = r.ReadInt32(),
                };


                p.BinaryReader.Close();
            }

            MegaAssemblerPatches.SyncSlot((planetId, entityId), slotId, slotData);
        }
    }

    [RegisterPacketProcessor]
    public class SyncSlotDataProcessor : BasePacketProcessor<SyncSlotData>
    {
        public override void ProcessPacket(SyncSlotData packet, INebulaConnection conn) =>
            SyncSlotData.OnReceive(packet.PlanetId, packet.SlotId, packet.EntityId, packet.SlotData);
    }

    public class SyncPlanetFocusData
    {
        public SyncPlanetFocusData() {}

        public SyncPlanetFocusData(int planetId, int index, int focusId)
        {
            PlanetId = planetId;
            Index = index;
            FocusId = focusId;
        }

        public int PlanetId { get; set; }
        public int Index { get; set; }
        public int FocusId { get; set; }

        internal static void Sync(int planetId, int index, int focusId)
        {
            if (NebulaModAPI.IsMultiplayerActive)
                NebulaModAPI.MultiplayerSession.Network.SendPacket(new SyncPlanetFocusData(planetId, index, focusId));
        }

        internal static void OnReceive(int planetId, int index, int focusId) =>
            PlanetFocusPatches.SyncPlanetFocus(planetId, index, focusId);
    }

    [RegisterPacketProcessor]
    public class SyncPlanetFocusDataProcessor : BasePacketProcessor<SyncPlanetFocusData>
    {
        public override void ProcessPacket(SyncPlanetFocusData packet, INebulaConnection conn) =>
            SyncPlanetFocusData.OnReceive(packet.PlanetId, packet.Index, packet.FocusId);
    }

    public class SyncNewQuantumStorageData
    {
        public SyncNewQuantumStorageData() {}

        public SyncNewQuantumStorageData(int planetId, int storageId, int orbitId)
        {
            PlanetId = planetId;
            StorageId = storageId;
            OrbitId = orbitId;
        }

        public int PlanetId { get; set; }
        public int StorageId { get; set; }
        public int OrbitId { get; set; }

        internal static void Sync(int planetId, int storageId, int orbitId)
        {
            if (NebulaModAPI.IsMultiplayerActive)
                NebulaModAPI.MultiplayerSession.Network.SendPacket(new SyncNewQuantumStorageData(planetId, storageId, orbitId));
        }

        internal static void OnReceive(int planetId, int storageId, int orbitId) =>
            QuantumStoragePatches.SyncNewQuantumStorage(planetId, storageId, orbitId);
    }

    [RegisterPacketProcessor]
    public class SyncNewQuantumStorageDataProcessor : BasePacketProcessor<SyncNewQuantumStorageData>
    {
        public override void ProcessPacket(SyncNewQuantumStorageData packet, INebulaConnection conn) =>
            SyncNewQuantumStorageData.OnReceive(packet.PlanetId, packet.StorageId, packet.OrbitId);
    }

    public class SyncRemoveQuantumStorageData
    {
        public SyncRemoveQuantumStorageData() {}

        public SyncRemoveQuantumStorageData(int planetId, int storageId, int orbitId)
        {
            PlanetId = planetId;
            StorageId = storageId;
            OrbitId = orbitId;
        }

        public int PlanetId { get; set; }
        public int StorageId { get; set; }
        public int OrbitId { get; set; }

        internal static void Sync(int planetId, int storageId, int orbitId)
        {
            if (NebulaModAPI.IsMultiplayerActive)
                NebulaModAPI.MultiplayerSession.Network.SendPacket(new SyncRemoveQuantumStorageData(planetId, storageId, orbitId));
        }

        internal static void OnReceive(int planetId, int storageId, int orbitId) =>
            QuantumStoragePatches.SyncRemoveQuantumStorage(planetId, storageId, orbitId);
    }

    [RegisterPacketProcessor]
    public class SyncRemoveQuantumStorageDataProcessor : BasePacketProcessor<SyncRemoveQuantumStorageData>
    {
        public override void ProcessPacket(SyncRemoveQuantumStorageData packet, INebulaConnection conn) =>
            SyncRemoveQuantumStorageData.OnReceive(packet.PlanetId, packet.StorageId, packet.OrbitId);
    }

    public class SyncQuantumStorageOrbitChangeData
    {
        public SyncQuantumStorageOrbitChangeData() {}

        public SyncQuantumStorageOrbitChangeData(int planetId, int storageId, int orbitId)
        {
            PlanetId = planetId;
            StorageId = storageId;
            OrbitId = orbitId;
        }

        public int PlanetId { get; set; }
        public int StorageId { get; set; }
        public int OrbitId { get; set; }

        internal static void Sync(int planetId, int storageId, int orbitId)
        {
            if (NebulaModAPI.IsMultiplayerActive)
                NebulaModAPI.MultiplayerSession.Network.SendPacket(new SyncQuantumStorageOrbitChangeData(planetId, storageId, orbitId));
        }

        internal static void OnReceive(int planetId, int storageId, int orbitId) =>
            QuantumStoragePatches.SyncQuantumStorageOrbitChange(planetId, storageId, orbitId);
    }

    [RegisterPacketProcessor]
    public class SyncQuantumStorageOrbitChangeDataProcessor : BasePacketProcessor<SyncQuantumStorageOrbitChangeData>
    {
        public override void ProcessPacket(SyncQuantumStorageOrbitChangeData packet, INebulaConnection conn) =>
            SyncQuantumStorageOrbitChangeData.OnReceive(packet.PlanetId, packet.StorageId, packet.OrbitId);
    }

    public class SyncGlobalPowerSupplyNodeIdData
    {
        public SyncGlobalPowerSupplyNodeIdData() {}

        public SyncGlobalPowerSupplyNodeIdData(int planetId, int nodeId)
        {
            PlanetId = planetId;
            NodeId = nodeId;
        }

        public int PlanetId { get; set; }
        public int NodeId { get; set; }

        internal static void Sync(int planetId, int nodeId)
        {
            if (NebulaModAPI.IsMultiplayerActive)
                NebulaModAPI.MultiplayerSession.Network.SendPacket(new SyncGlobalPowerSupplyNodeIdData(planetId, nodeId));
        }

        internal static void OnReceive(int planetId, int nodeId) => GlobalPowerSupplyPatches.SyncGlobalPowerSupplyNodeId(planetId, nodeId);
    }

    [RegisterPacketProcessor]
    public class SyncGlobalPowerSupplyNodeIdDataProcessor : BasePacketProcessor<SyncGlobalPowerSupplyNodeIdData>
    {
        public override void ProcessPacket(SyncGlobalPowerSupplyNodeIdData packet, INebulaConnection conn) =>
            SyncGlobalPowerSupplyNodeIdData.OnReceive(packet.PlanetId, packet.NodeId);
    }

    public class SyncSyncGlobalPowerSupplyRemoveData
    {
        public SyncSyncGlobalPowerSupplyRemoveData() {}

        public SyncSyncGlobalPowerSupplyRemoveData(int planetId) => PlanetId = planetId;

        public int PlanetId { get; set; }

        internal static void Sync(int planetId)
        {
            if (NebulaModAPI.IsMultiplayerActive)
                NebulaModAPI.MultiplayerSession.Network.SendPacket(new SyncSyncGlobalPowerSupplyRemoveData(planetId));
        }

        internal static void OnReceive(int planetId) => GlobalPowerSupplyPatches.SyncGlobalPowerSupplyRemoved(planetId);
    }

    [RegisterPacketProcessor]
    public class SyncSyncGlobalPowerSupplyRemoveDataProcessor : BasePacketProcessor<SyncSyncGlobalPowerSupplyRemoveData>
    {
        public override void ProcessPacket(SyncSyncGlobalPowerSupplyRemoveData packet, INebulaConnection conn) =>
            SyncSyncGlobalPowerSupplyRemoveData.OnReceive(packet.PlanetId);
    }

    public class GenesisBookPlanetLoadRequest
    {
        public GenesisBookPlanetLoadRequest() {}

        public GenesisBookPlanetLoadRequest(int planetId) => PlanetId = planetId;

        public int PlanetId { get; set; }
    }

    [RegisterPacketProcessor]
    public class GenesisBookPlanetLoadRequestProcessor : BasePacketProcessor<GenesisBookPlanetLoadRequest>
    {
        public override void ProcessPacket(GenesisBookPlanetLoadRequest packet, INebulaConnection conn)
        {
            if (IsClient) return;

            byte[] data;

            using (IWriterProvider p = NebulaModAPI.GetBinaryWriter())
            {
                MegaAssemblerPatches.ExportPlanetData(packet.PlanetId, p.BinaryWriter);
                PlanetFocusPatches.ExportPlanetFocus(packet.PlanetId, p.BinaryWriter);
                QuantumStoragePatches.ExportPlanetQuantumStorage(packet.PlanetId, p.BinaryWriter);
                data = p.CloseAndGetBytes();
            }

            conn.SendPacket(new GenesisBookPlanetData(packet.PlanetId, data));
        }
    }

    public class GenesisBookPlanetData
    {
        public GenesisBookPlanetData() {}

        public GenesisBookPlanetData(int id, byte[] data)
        {
            PlanetId = id;
            BinaryData = data;
        }

        public int PlanetId { get; set; }
        public byte[] BinaryData { get; set; }
    }

    [RegisterPacketProcessor]
    public class GenesisBookPlanetDataProcessor : BasePacketProcessor<GenesisBookPlanetData>
    {
        internal static readonly Dictionary<int, byte[]> PendingData = new Dictionary<int, byte[]>();

        public override void ProcessPacket(GenesisBookPlanetData packet, INebulaConnection conn)
        {
            if (IsHost) return;

            PendingData.Add(packet.PlanetId, packet.BinaryData);
        }

        public static void ProcessBytesLater(int planetId)
        {
            if (!NebulaModAPI.IsMultiplayerActive || NebulaModAPI.MultiplayerSession.LocalPlayer.IsHost) return;

            if (!PendingData.Remove(planetId, out byte[] bytes)) return;

            using (IReaderProvider p = NebulaModAPI.GetBinaryReader(bytes))
            {
                MegaAssemblerPatches.ImportPlanetData(p.BinaryReader);
                PlanetFocusPatches.ImportPlanetFocus(p.BinaryReader);
                QuantumStoragePatches.ImportPlanetQuantumStorage(p.BinaryReader);
            }
        }
    }
}
