using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ProjectGenesis.Utils;

namespace ProjectGenesis.Patches.Logic.MegaAssembler
{
    internal static partial class MegaAssemblerPatches
    {
        private static ConcurrentDictionary<(int, int), SlotData[]> _slotdata = new ConcurrentDictionary<(int, int), SlotData[]>();

        internal static void SyncSlots((int, int) id, SlotData[] slotDatas) => _slotdata[id] = slotDatas;

        internal static void SyncSlot((int, int) id, int slotId, SlotData slotData)
        {
            lock (_slotdata)
            {
                if (_slotdata.TryGetValue(id, out SlotData[] slotDatas))
                {
                    slotDatas[slotId] = slotData;
                }
                else
                {
                    slotDatas = new SlotData[12];
                    slotDatas[slotId] = slotData;
                    _slotdata[id] = slotDatas;
                }
            }
        }

        private static SlotData[] GetSlots(int planetId, int entityId)
        {
            var id = (planetId, entityId);

            if (!_slotdata.ContainsKey(id) || _slotdata[id] == null) _slotdata[id] = new SlotData[12];

            return _slotdata[id];
        }

        private static void SetEmpty(int planetId, int entityId)
        {
            var id = (planetId, entityId);
            if (!_slotdata.ContainsKey(id)) return;
            _slotdata[id] = new SlotData[12];
            SyncSlotsData.Sync(planetId, entityId, _slotdata[id]);
        }

        public static void ExportPlanetData(int planetId, BinaryWriter w)
        {
            KeyValuePair<(int, int), SlotData[]>[] datas = _slotdata.Where(pair => pair.Key.Item1 == planetId).ToArray();

            w.Write(datas.Length);
            w.Write(planetId);
            foreach (KeyValuePair<(int, int), SlotData[]> pair in datas)
            {
                w.Write(pair.Key.Item2);
                w.Write(pair.Value.Length);
                for (var i = 0; i < pair.Value.Length; i++)
                {
                    w.Write((int)pair.Value[i].dir);
                    w.Write(pair.Value[i].beltId);
                    w.Write(pair.Value[i].storageIdx);
                    w.Write(pair.Value[i].counter);
                }
            }
        }

        public static void ImportPlanetData(BinaryReader r)
        {
            var count = r.ReadInt32();
            var planetId = r.ReadInt32();

            for (var j = 0; j < count; j++)
            {
                var entityId = r.ReadInt32();
                var length = r.ReadInt32();
                var datas = new SlotData[length];
                for (var i = 0; i < length; i++)
                {
                    datas[i] = new SlotData
                               {
                                   dir = (IODir)r.ReadInt32(), beltId = r.ReadInt32(), storageIdx = r.ReadInt32(), counter = r.ReadInt32()
                               };
                }

                _slotdata[(planetId, entityId)] = datas;
            }
        }
    }
}
