using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ProjectGenesis.Utils;

namespace ProjectGenesis.Patches.Logic.LithographyAssembler
{
    internal static partial class LithographyAssemblerPatches
    {
        private static ConcurrentDictionary<(int, int), LithographyData> _lithographydata = new ConcurrentDictionary<(int, int), LithographyData>();

        internal static void SyncLithography((int, int) id, LithographyData lithographydata) => _lithographydata[id] = lithographydata;

        internal static LithographyData GetLithographyData(int planetId, int assemblerId)
        {
            var id = (planetId, assemblerId);

            if (!_lithographydata.ContainsKey(id)) _lithographydata[id] = new LithographyData();

            return _lithographydata[id];
        }

        internal static void SetLithographyData(int planetId, int assemblerId, LithographyData data)
        {
            var id = (planetId, assemblerId);
            if (!_lithographydata.ContainsKey(id)) return;
            _lithographydata[id] = data;
            SyncLithographyData.Sync(planetId, assemblerId, _lithographydata[id]);
        }

        internal static void SetEmpty(int planetId, int assemblerId, bool pop = true)
        {
            var id = (planetId, assemblerId);
            if (!_lithographydata.ContainsKey(id)) return;
            var data = _lithographydata[id];

            if (data.ItemId == 0 || data.ItemCount == 0) return;

            var mainPlayer = GameMain.mainPlayer;

            if (pop)
            {
                var upCount = mainPlayer.TryAddItemToPackage(data.ItemId, data.ItemCount, data.ItemInc, true);
                UIItemup.Up(data.ItemId, upCount);
            }
            else
            {
                mainPlayer.SetHandItemId_Unsafe(data.ItemId);
                mainPlayer.SetHandItemCount_Unsafe(data.ItemCount);
                mainPlayer.SetHandItemInc_Unsafe(data.ItemInc);
            }

            _lithographydata[id] = new LithographyData() { NeedCount = data.NeedCount };

            SyncLithographyData.Sync(planetId, assemblerId, data);
        }

        public static void ExportPlanetData(int planetId, BinaryWriter w)
        {
            KeyValuePair<(int, int), LithographyData>[] datas = _lithographydata.Where(pair => pair.Key.Item1 == planetId).ToArray();

            w.Write(datas.Length);
            w.Write(planetId);

            foreach (KeyValuePair<(int, int), LithographyData> pair in datas)
            {
                w.Write(pair.Key.Item2);
                w.Write(pair.Value.ItemId);
                w.Write(pair.Value.ItemCount);
                w.Write(pair.Value.ItemInc);
                w.Write(pair.Value.NeedCount);
            }
        }

        public static void ImportPlanetData(BinaryReader r)
        {
            var count = r.ReadInt32();
            var planetId = r.ReadInt32();

            for (var j = 0; j < count; j++)
            {
                var assemblerId = r.ReadInt32();
                _lithographydata[(planetId, assemblerId)] = new LithographyData
                                                            {
                                                                ItemId = r.ReadInt32(),
                                                                ItemCount = r.ReadInt32(),
                                                                ItemInc = r.ReadInt32(),
                                                                NeedCount = r.ReadInt32()
                                                            };
            }
        }
    }
}
