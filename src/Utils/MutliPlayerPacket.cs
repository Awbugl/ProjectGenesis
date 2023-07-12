using System.Collections.Generic;
using NebulaAPI;
using ProjectGenesis.Patches.Logic.LithographyAssembler;
using ProjectGenesis.Patches.Logic.MegaAssembler;
using ProjectGenesis.Patches.Logic.PlanetFocus;

// ReSharper disable MemberCanBeInternal
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable ClassNeverInstantiated.Global

namespace ProjectGenesis.Utils
{
    public class SyncSlotsData
    {
        public SyncSlotsData() { }

        public SyncSlotsData(
            string guid,
            int planetId,
            int entityId,
            SlotData[] data)
        {
            Guid = guid;
            PlanetId = planetId;
            EntityId = entityId;
            using (var p = NebulaModAPI.GetBinaryWriter())
            {
                var w = p.BinaryWriter;

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

        public string Guid { get; set; }
        public int PlanetId { get; set; }
        public int EntityId { get; set; }

        public byte[] SlotsData { get; set; }

        internal static void Sync(int planetId, int entityId, SlotData[] slotsData)
        {
            if (NebulaModAPI.IsMultiplayerActive)
                NebulaModAPI.MultiplayerSession.Network.SendPacket(new SyncSlotsData(ProjectGenesis.MODGUID, planetId, entityId, slotsData));
        }

        internal static void OnReceive(
            string guid,
            int planetId,
            int entityId,
            byte[] data)
        {
            if (guid != ProjectGenesis.MODGUID) return;

            SlotData[] slotsData;

            using (var p = NebulaModAPI.GetBinaryReader(data))
            using (var r = p.BinaryReader)
            {
                var length = r.ReadInt32();
                slotsData = new SlotData[length];
                for (var i = 0; i < length; i++)
                {
                    slotsData[i] = new SlotData
                                   {
                                       dir = (IODir)r.ReadInt32(), beltId = r.ReadInt32(), storageIdx = r.ReadInt32(), counter = r.ReadInt32()
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
        public override void ProcessPacket(SyncSlotsData packet, INebulaConnection conn)
            => SyncSlotsData.OnReceive(packet.Guid, packet.PlanetId, packet.EntityId, packet.SlotsData);
    }

    public class SyncSlotData
    {
        public SyncSlotData() { }

        public SyncSlotData(
            string guid,
            int planetId,
            int slotId,
            int entityId,
            SlotData data)
        {
            Guid = guid;
            PlanetId = planetId;
            SlotId = slotId;
            EntityId = entityId;

            using (var p = NebulaModAPI.GetBinaryWriter())
            {
                var w = p.BinaryWriter;

                w.Write((int)data.dir);
                w.Write(data.beltId);
                w.Write(data.storageIdx);
                w.Write(data.counter);

                SlotData = p.CloseAndGetBytes();
            }
        }

        public string Guid { get; set; }
        public int PlanetId { get; set; }
        public int SlotId { get; set; }
        public int EntityId { get; set; }

        public byte[] SlotData { get; set; }

        internal static void Sync(
            int planetId,
            int slotId,
            int entityId,
            SlotData slotData)
        {
            if (NebulaModAPI.IsMultiplayerActive)
                NebulaModAPI.MultiplayerSession.Network.SendPacket(new SyncSlotData(ProjectGenesis.MODGUID, planetId, slotId, entityId, slotData));
        }

        internal static void OnReceive(
            string guid,
            int planetId,
            int slotId,
            int entityId,
            byte[] data)
        {
            if (guid != ProjectGenesis.MODGUID) return;

            SlotData slotData;

            using (var p = NebulaModAPI.GetBinaryReader(data))
            using (var r = p.BinaryReader)
            {
                slotData = new SlotData { dir = (IODir)r.ReadInt32(), beltId = r.ReadInt32(), storageIdx = r.ReadInt32(), counter = r.ReadInt32() };


                p.BinaryReader.Close();
            }

            MegaAssemblerPatches.SyncSlot((planetId, entityId), slotId, slotData);
        }
    }

    [RegisterPacketProcessor]
    public class SyncSlotDataProcessor : BasePacketProcessor<SyncSlotData>
    {
        public override void ProcessPacket(SyncSlotData packet, INebulaConnection conn)
            => SyncSlotData.OnReceive(packet.Guid, packet.PlanetId, packet.SlotId, packet.EntityId, packet.SlotData);
    }

    public class SyncPlanetFocusData
    {
        public SyncPlanetFocusData() { }

        public SyncPlanetFocusData(
            string guid,
            int planetId,
            int index,
            int focusId)
        {
            Guid = guid;
            PlanetId = planetId;
            Index = index;
            FocusId = focusId;
        }

        public string Guid { get; set; }
        public int PlanetId { get; set; }
        public int Index { get; set; }
        public int FocusId { get; set; }

        internal static void Sync(int planetId, int index, int focusId)
        {
            if (NebulaModAPI.IsMultiplayerActive)
                NebulaModAPI.MultiplayerSession.Network.SendPacket(new SyncPlanetFocusData(ProjectGenesis.MODGUID, planetId, index, focusId));
        }

        internal static void OnReceive(
            string guid,
            int planetId,
            int index,
            int focusId)
        {
            if (guid != ProjectGenesis.MODGUID) return;
            PlanetFocusPatches.SyncPlanetFocus(planetId, index, focusId);
        }
    }

    [RegisterPacketProcessor]
    public class SyncPlanetFocusDataProcessor : BasePacketProcessor<SyncPlanetFocusData>
    {
        public override void ProcessPacket(SyncPlanetFocusData packet, INebulaConnection conn)
            => SyncPlanetFocusData.OnReceive(packet.Guid, packet.PlanetId, packet.Index, packet.FocusId);
    }

    public class SyncLithographyData
    {
        public SyncLithographyData() { }

        public SyncLithographyData(
            string guid,
            int planetId,
            int assemblerId,
            int itemId,
            int itemCount,
            int itemInc,
            int needCount)
        {
            Guid = guid;
            PlanetId = planetId;
            AssemblerId = assemblerId;
            ItemId = itemId;
            ItemCount = itemCount;
            ItemInc = itemInc;
            NeedCount = needCount;
        }

        public string Guid { get; set; }
        public int PlanetId { get; set; }
        public int AssemblerId { get; set; }
        public int ItemId { get; set; }
        public int ItemCount { get; set; }
        public int ItemInc { get; set; }
        public int NeedCount { get; set; }

        internal static void Sync(int planetId, int assemblerId, LithographyData data)
        {
            if (NebulaModAPI.IsMultiplayerActive)
                NebulaModAPI.MultiplayerSession.Network.SendPacket(new SyncLithographyData(ProjectGenesis.MODGUID, planetId, assemblerId, data.ItemId,
                                                                                           data.ItemCount, data.ItemInc, data.NeedCount));
        }

        internal static void OnReceive(
            string guid,
            int planetId,
            int assemblerId,
            int itemId,
            int itemCount,
            int itemInc,
            int needCount)

        {
            if (guid != ProjectGenesis.MODGUID) return;
            LithographyAssemblerPatches.SyncLithography((planetId, assemblerId),
                                                        new LithographyData
                                                        {
                                                            ItemId = itemId, ItemCount = itemCount, ItemInc = itemInc, NeedCount = needCount
                                                        });
        }
    }

    [RegisterPacketProcessor]
    public class SyncLithographyDataProcessor : BasePacketProcessor<SyncLithographyData>
    {
        public override void ProcessPacket(SyncLithographyData packet, INebulaConnection conn)
            => SyncLithographyData.OnReceive(packet.Guid, packet.PlanetId, packet.AssemblerId, packet.ItemId, packet.ItemCount, packet.ItemInc,
                                             packet.NeedCount);
    }

    public class GenesisBookPlanetLoadRequest
    {
        public int PlanetId { get; set; }

        public GenesisBookPlanetLoadRequest() { }

        public GenesisBookPlanetLoadRequest(int planetId)
        {
            PlanetId = planetId;
        }
    }

    [RegisterPacketProcessor]
    public class GenesisBookPlanetLoadRequestProcessor : BasePacketProcessor<GenesisBookPlanetLoadRequest>
    {
        public override void ProcessPacket(GenesisBookPlanetLoadRequest packet, INebulaConnection conn)
        {
            if (IsClient) return;

            byte[] data;

            using (var p = NebulaModAPI.GetBinaryWriter())
            {
                MegaAssemblerPatches.ExportPlanetData(packet.PlanetId, p.BinaryWriter);
                PlanetFocusPatches.ExportPlanetFocus(packet.PlanetId, p.BinaryWriter);
                LithographyAssemblerPatches.ExportPlanetData(packet.PlanetId, p.BinaryWriter);
                data = p.CloseAndGetBytes();
            }

            conn.SendPacket(new GenesisBookPlanetData(packet.PlanetId, data));
        }
    }

    public class GenesisBookPlanetData
    {
        public int PlanetId { get; set; }
        public byte[] BinaryData { get; set; }

        public GenesisBookPlanetData() { }

        public GenesisBookPlanetData(int id, byte[] data)
        {
            PlanetId = id;
            BinaryData = data;
        }
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
            if (!PendingData.TryGetValue(planetId, out var bytes)) return;
            PendingData.Remove(planetId);

            using (var p = NebulaModAPI.GetBinaryReader(bytes))
            {
                MegaAssemblerPatches.ImportPlanetData(p.BinaryReader);
                PlanetFocusPatches.ImportPlanetFocus(p.BinaryReader);
                LithographyAssemblerPatches.ImportPlanetData(p.BinaryReader);
            }
        }
    }
}
