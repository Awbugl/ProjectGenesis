using NebulaAPI;

namespace ProjectGenesis.Patches
{
    public class NcModSaveRequest
    {
        public NcModSaveRequest() { }

        internal NcModSaveRequest(string guid)
        {
            Guid = guid;
        }

        internal string Guid { get; set; }

        internal static void OnReceive(string guid, INebulaConnection conn)
        {
            if (guid != ProjectGenesis.MODGUID) return;
            conn.SendPacket(new NcModSaveData(ProjectGenesis.MODGUID, ProjectGenesis.Export()));
        }
    }

    [RegisterPacketProcessor]
    public class NcModSaveRequestProcessor : BasePacketProcessor<NcModSaveRequest>
    {
        public override void ProcessPacket(NcModSaveRequest packet, INebulaConnection conn) => NcModSaveRequest.OnReceive(packet.Guid, conn);
    }

    public class NcModSaveData
    {
        public NcModSaveData() { }

        internal NcModSaveData(string guid, byte[] bytes)
        {
            Guid = guid;
            Bytes = bytes;
        }

        internal string Guid { get; set; }
        internal byte[] Bytes { get; set; }

        internal static void OnReceive(string guid, byte[] bytes)
        {
            if (guid != ProjectGenesis.MODGUID) return;
            ProjectGenesis.Import(bytes);
        }
    }

    [RegisterPacketProcessor]
    public class NcModSaveDataProcessor : BasePacketProcessor<NcModSaveData>
    {
        public override void ProcessPacket(NcModSaveData packet, INebulaConnection conn)
        {
            if (IsHost)
                // Broadcast changes to other users
                NebulaModAPI.MultiplayerSession.Network.SendPacketExclude(packet, conn);

            NcModSaveData.OnReceive(packet.Guid, packet.Bytes);
        }
    }

    public class SyncSlotsData
    {
        public SyncSlotsData() { }

        public SyncSlotsData(string guid, int entityId, SlotData[] slotsData)
        {
            Guid = guid;
            EntityId = entityId;
            SlotsData = slotsData;
        }

        internal string Guid { get; set; }
        internal int EntityId { get; set; }

        internal SlotData[] SlotsData { get; set; }

        internal static void Sync(int entityId, SlotData[] slotsData)
        {
            if (NebulaModAPI.IsMultiplayerActive)
                NebulaModAPI.MultiplayerSession.Network.SendPacket(new SyncSlotsData(ProjectGenesis.MODGUID, entityId, slotsData));
        }

        internal static void OnReceive(string guid, int entityId, SlotData[] slotsData)
        {
            if (guid != ProjectGenesis.MODGUID) return;
            MegaAssemblerPatches.SyncSlots(entityId, slotsData);
        }
    }

    [RegisterPacketProcessor]
    public class SyncSlotsDataProcessor : BasePacketProcessor<SyncSlotsData>
    {
        public override void ProcessPacket(SyncSlotsData packet, INebulaConnection conn)
        {
            if (IsHost)
                // Broadcast changes to other users
                NebulaModAPI.MultiplayerSession.Network.SendPacketExclude(packet, conn);

            SyncSlotsData.OnReceive(packet.Guid, packet.EntityId, packet.SlotsData);
        }
    }

    public class EntityId2PlanetIdData
    {
        public EntityId2PlanetIdData() { }

        public EntityId2PlanetIdData(string guid, int entityId, int planetId)
        {
            Guid = guid;
            EntityId = entityId;
            PlanetId = planetId;
        }

        internal string Guid { get; set; }
        internal int EntityId { get; set; }
        internal int PlanetId { get; set; }

        internal static void Sync(int entityId, int planetId)
        {
            if (NebulaModAPI.IsMultiplayerActive)
                NebulaModAPI.MultiplayerSession.Network.SendPacket(new EntityId2PlanetIdData(ProjectGenesis.MODGUID, entityId, planetId));
        }

        internal static void OnReceive(string guid, int entityId, int planetId)
        {
            if (guid != ProjectGenesis.MODGUID) return;
            MegaAssemblerPatches.SyncEntityId(entityId, planetId);
        }
    }

    [RegisterPacketProcessor]
    public class EntityId2PlanetIdDataProcessor : BasePacketProcessor<EntityId2PlanetIdData>
    {
        public override void ProcessPacket(EntityId2PlanetIdData packet, INebulaConnection conn)
        {
            if (IsHost)
                // Broadcast changes to other users
                NebulaModAPI.MultiplayerSession.Network.SendPacketExclude(packet, conn);

            EntityId2PlanetIdData.OnReceive(packet.Guid, packet.EntityId, packet.PlanetId);
        }
    }
}
