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
                foreach (var t in pair.Value) w.Write(t);
            }
        }

        internal static void Import(BinaryReader r)
        {
            ReInitAll();

            try
            {
                var slotdatacount = r.ReadInt32();

                for (var j = 0; j < slotdatacount; j++)
                {
                    var key = r.ReadInt32();
                    var length = r.ReadInt32();
                    var datas = new int[length];
                    for (var i = 0; i < length; i++)
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
            if (_planetFocuses.TryGetValue(planetId, out var focuses))
                // ReSharper disable once LoopCanBeConvertedToQuery
                // ReSharper disable once ForCanBeConvertedToForeach
                for (var index = 0; index < focuses.Length; ++index)
                {
                    if (focuses[index] == focusid) return true;
                }

            return false;
        }
    }
}
