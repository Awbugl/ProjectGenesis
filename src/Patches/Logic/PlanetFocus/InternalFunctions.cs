using System.Collections.Generic;
using System.IO;

namespace ProjectGenesis.Patches.Logic.PlanetFocus
{
    public static partial class PlanetFocusPatches
    {
        internal static void Export(BinaryWriter w)
        {
            lock (PlanetFocuses)
            {
                w.Write(PlanetFocuses.Count);

                foreach (KeyValuePair<int, int[]> pair in PlanetFocuses)
                {
                    w.Write(pair.Key);
                    w.Write(pair.Value.Length);

                    foreach (int t in pair.Value) w.Write(t);
                }
            }
        }

        internal static void Import(BinaryReader r)
        {
            ReInitAll();

            try
            {
                int slotdatacount = r.ReadInt32();

                for (var j = 0; j < slotdatacount; j++)
                {
                    int key = r.ReadInt32();
                    int length = r.ReadInt32();
                    var datas = new int[length];

                    for (var i = 0; i < length; i++) datas[i] = r.ReadInt32();

                    PlanetFocuses.TryAdd(key, datas);
                }
            }
            catch (EndOfStreamException)
            {
                // ignored
            }
        }

        internal static void IntoOtherSave() => ReInitAll();

        private static void ReInitAll() => PlanetFocuses.Clear();

        private static bool ContainsFocus(int planetId, int focusId)
        {
            if (!PlanetFocuses.TryGetValue(planetId, out int[] focuses)) return false;

            // ReSharper disable once LoopCanBeConvertedToQuery
            // ReSharper disable once ForCanBeConvertedToForeach
            for (var index = 0; index < focuses.Length; ++index)
            {
                if (focuses[index] == focusId) return true;
            }

            return false;
        }
    }
}
