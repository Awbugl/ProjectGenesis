using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using ProjectGenesis.Utils;

// ReSharper disable MemberCanBeInternal

namespace ProjectGenesis.Patches.Logic.PlanetFocus
{
    public static partial class PlanetFocusPatches
    {
        internal const int FocusMaxCount = 2;

        private static readonly ConcurrentDictionary<int, int[]> PlanetFocuses = new ConcurrentDictionary<int, int[]>();

        internal static readonly Dictionary<int, string> FocusIds = new Dictionary<int, string>
        {
            { 6522, "工厂电力需求 -10%" },
            { 6523, "研究上传速度 +10%" },
            { 6524, "火力发电效率 +20%" },
            { 6525, "风力发电效率 +20%" },
            { 6526, "光伏发电效率 +20%" },
            { 6527, "聚变发电效率 +10%" },
            { 6528, "矿物采集速度 +25%" },
            { 6529, "裂变发电效率 +20%" },
            { 6530, "配送物流速度 +25%" },
            { 6531, "电力威胁增长 -20%" },
            { 6532, "太空威胁增长 -20%" },
            { 6533, "部队建造速度 -20%" },
            { 6534, "基地扩张速度 -20%" },
        };

        internal static void SetPlanetFocus(int planetId, int index, int focusId)
        {
            if (!PlanetFocuses.ContainsKey(planetId)) PlanetFocuses[planetId] = new int[FocusMaxCount];

            PlanetFocuses[planetId][index] = focusId;
            SyncPlanetFocusData.Sync(planetId, index, focusId);
        }

        internal static void SyncPlanetFocus(int planetId, int index, int focusId)
        {
            if (!PlanetFocuses.ContainsKey(planetId)) PlanetFocuses[planetId] = new int[FocusMaxCount];

            PlanetFocuses[planetId][index] = focusId;
        }

        internal static int[] GetPlanetFocus(int planetId)
        {
            if (!PlanetFocuses.ContainsKey(planetId)) PlanetFocuses[planetId] = new int[FocusMaxCount];

            return PlanetFocuses[planetId];
        }

        public static void ExportPlanetFocus(int planetId, BinaryWriter w)
        {
            if (!PlanetFocuses.ContainsKey(planetId)) PlanetFocuses[planetId] = new int[FocusMaxCount];

            int[] datas = PlanetFocuses[planetId];

            w.Write(datas.Length);
            w.Write(planetId);

            foreach (int id in datas) w.Write(id);
        }

        public static void ImportPlanetFocus(BinaryReader r)
        {
            int count = r.ReadInt32();
            int planetId = r.ReadInt32();

            var arr = new int[count];

            for (var j = 0; j < count; j++) arr[j] = r.ReadInt32();

            PlanetFocuses[planetId] = arr;
        }
    }
}
