using NebulaAPI;
using ProjectGenesis.Patches;

// ReSharper disable UnusedType.Global
// ReSharper disable MemberCanBePrivate.Global

namespace ProjectGenesis.Utils
{
    public class SyncSlotsData
    {
        public SyncSlotsData() { }

        public SyncSlotsData(
            string guid,
            int planetId,
            int entityId,
            SlotData[] slotsData)
        {
            Guid = guid;
            PlanetId = planetId;
            EntityId = entityId;
            SlotsData = slotsData;
        }

        internal string Guid { get; set; }
        internal int PlanetId { get; set; }
        internal int EntityId { get; set; }

        internal SlotData[] SlotsData { get; set; }

        internal static void Sync(int planetId, int entityId, SlotData[] slotsData)
        {
            if (NebulaModAPI.IsMultiplayerActive)
            {
                ProjectGenesis.logger.LogInfo("Sending SyncSlotsData packet");
                NebulaModAPI.MultiplayerSession.Network.SendPacket(new SyncSlotsData(ProjectGenesis.MODGUID, planetId, entityId, slotsData));
            }
        }

        internal static void OnReceive(
            string guid,
            int planetId,
            int entityId,
            SlotData[] slotsData)
        {
            if (guid != ProjectGenesis.MODGUID) return;
            ProjectGenesis.logger.LogInfo("Processing SyncSlotsData packet");
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
            SlotData slotData)
        {
            Guid = guid;
            PlanetId = planetId;
            SlotId = slotId;
            EntityId = entityId;
            SlotData = slotData;
        }

        internal string Guid { get; set; }
        internal int PlanetId { get; set; }
        internal int SlotId { get; set; }
        internal int EntityId { get; set; }

        internal SlotData SlotData { get; set; }

        internal static void Sync(
            int planetId,
            int slotId,
            int entityId,
            SlotData slotData)
        {
            if (NebulaModAPI.IsMultiplayerActive)
            {
                ProjectGenesis.logger.LogInfo("Sending SyncSlotData packet");
                NebulaModAPI.MultiplayerSession.Network.SendPacket(new SyncSlotData(ProjectGenesis.MODGUID, planetId, slotId, entityId, slotData));
            }
        }

        internal static void OnReceive(
            string guid,
            int planetId,
            int slotId,
            int entityId,
            SlotData slotData)
        {
            if (guid != ProjectGenesis.MODGUID) return;
            ProjectGenesis.logger.LogInfo("Processing SyncSlotData packet");
            MegaAssemblerPatches.SyncSlot((planetId, entityId), slotId, slotData);
        }
    }

    [RegisterPacketProcessor]
    public class SyncSlotDataProcessor : BasePacketProcessor<SyncSlotData>
    {
        public override void ProcessPacket(SyncSlotData packet, INebulaConnection conn)
            => SyncSlotData.OnReceive(packet.Guid, packet.PlanetId, packet.SlotId, packet.EntityId, packet.SlotData);
    }
}
