using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using ProjectGenesis.Utils;

namespace ProjectGenesis.Patches.Logic.PlanetBase
{
    public static partial class PlanetFocusPatches
    {
        internal const int FocusMaxCount = 2;

        private static ConcurrentDictionary<int, int[]> _planetFocuses = new ConcurrentDictionary<int, int[]>();

        internal static readonly Dictionary<int, string> FocusIds = new Dictionary<int, string>()
                                                                     {
                                                                         { 6522, "工厂电力需求 -10%".TranslateFromJson() },
                                                                         { 6523, "研究上传速度 +10%".TranslateFromJson() },
                                                                         { 6524, "火力发电效率 +20%".TranslateFromJson() },
                                                                         { 6525, "风力发电效率 +20%".TranslateFromJson() },
                                                                         { 6526, "光伏发电效率 +20%".TranslateFromJson() },
                                                                         { 6527, "聚变发电效率 +10%".TranslateFromJson() },
                                                                         { 6528, "矿物采集速度 +25%".TranslateFromJson() }
                                                                     };

        internal static void SetPlanetFocus(int planetId, int index, int focusid)
        {
            _planetFocuses[planetId][index] = focusid;
            SyncPlanetFocusData.Sync(planetId, index, focusid);
        }

        internal static void SyncPlanetFocus(int planetId, int index, int focusid)
        {
            if (!_planetFocuses.ContainsKey(planetId)) _planetFocuses[planetId] = new int[FocusMaxCount];
            _planetFocuses[planetId][index] = focusid;
        }

        internal static int[] GetPlanetFocus(int planetId)
        {
            if (!_planetFocuses.ContainsKey(planetId)) _planetFocuses[planetId] = new int[FocusMaxCount];
            return _planetFocuses[planetId];
        }

        public static void ExportPlanetFocus(int planetId, BinaryWriter w)
        {
            if (!_planetFocuses.ContainsKey(planetId)) _planetFocuses[planetId] = new int[FocusMaxCount];

            var datas = _planetFocuses[planetId];

            w.Write(datas.Length);
            w.Write(planetId);
            foreach (var id in datas) w.Write(id);
        }

        public static void ImportPlanetFocus(BinaryReader r)
        {
            var count = r.ReadInt32();
            var planetId = r.ReadInt32();

            var arr = new int[count];
            for (var j = 0; j < count; j++) arr[j] = r.ReadInt32();

            _planetFocuses[planetId] = arr;
        }
    }
}
