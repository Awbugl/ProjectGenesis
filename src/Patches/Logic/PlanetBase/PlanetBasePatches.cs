using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using ProjectGenesis.Utils;

namespace ProjectGenesis.Patches.Logic.PlanetBase
{
    public static partial class PlanetBasePatches
    {
        internal const int FocusMaxCount = 2;

        private static ConcurrentDictionary<int, int[]> _planetBases = new ConcurrentDictionary<int, int[]>();

        internal static readonly Dictionary<int, string> FilterIds = new Dictionary<int, string>()
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
            _planetBases[planetId][index] = focusid;
            SyncPlanetFocusData.Sync(planetId, index, focusid);
        }

        internal static void SyncPlanetFocus(int planetId, int index, int focusid)
        {
            if (!_planetBases.ContainsKey(planetId)) _planetBases[planetId] = new int[FocusMaxCount];
            _planetBases[planetId][index] = focusid;
        }

        internal static int[] GetPlanetBase(int planetId)
        {
            if (!_planetBases.ContainsKey(planetId)) _planetBases[planetId] = new int[FocusMaxCount];
            return _planetBases[planetId];
        }

        public static void ExportPlanetFocus(int planetId, BinaryWriter w)
        {
            if (!_planetBases.ContainsKey(planetId)) _planetBases[planetId] = new int[FocusMaxCount];

            var datas = _planetBases[planetId];

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

            _planetBases[planetId] = arr;
        }
    }
}
