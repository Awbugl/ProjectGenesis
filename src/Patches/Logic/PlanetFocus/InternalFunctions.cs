using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;

namespace ProjectGenesis.Patches.Logic.PlanetFocus
{
    public static partial class PlanetFocusPatches
    {
        internal static void Export(BinaryWriter w)
        {
            w.Write(_planetFocuses.Count);

            foreach (KeyValuePair<int, int[]> pair in _planetFocuses)
            {
                w.Write(pair.Key);
                w.Write(pair.Value.Length);
                foreach (int t in pair.Value) w.Write(t);
            }
        }

        internal static void Import(BinaryReader r)
        {
            ReInitAll();

            try
            {
                int slotdatacount = r.ReadInt32();

                for (int j = 0; j < slotdatacount; j++)
                {
                    int key = r.ReadInt32();
                    int length = r.ReadInt32();
                    int[] datas = new int[length];
                    for (int i = 0; i < length; i++)
                    {
                        datas[i] = r.ReadInt32();
                    }

                    _planetFocuses.TryAdd(key, datas);
                }
            }
            catch (EndOfStreamException)
            {
                // ignored
            }
        }

        internal static void IntoOtherSave() => ReInitAll();

        private static void ReInitAll() => _planetFocuses = new ConcurrentDictionary<int, int[]>();

        private static bool ContainsFocus(int planetId, int focusid)
        {
            if (_planetFocuses.TryGetValue(planetId, out int[] focuses))
                // ReSharper disable once LoopCanBeConvertedToQuery
                // ReSharper disable once ForCanBeConvertedToForeach
                for (int index = 0; index < focuses.Length; ++index)
                {
                    if (focuses[index] == focusid) return true;
                }

            return false;
        }
    }
}
