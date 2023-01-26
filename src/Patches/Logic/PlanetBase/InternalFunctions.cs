using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;

namespace ProjectGenesis.Patches.Logic.PlanetBase
{
    public static partial class PlanetBasePatches
    {
        internal static void Export(BinaryWriter w)
        {
            w.Write(_planetBases.Count);

            foreach (KeyValuePair<int, int[]> pair in _planetBases)
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
                    for (var i = 0; i < length; i++) datas[i] = r.ReadInt32();

                    _planetBases.TryAdd(key, datas);
                }
            }
            catch (EndOfStreamException)
            {
                // ignored
            }
        }

        internal static void IntoOtherSave() => ReInitAll();

        private static void ReInitAll() => _planetBases = new ConcurrentDictionary<int, int[]>();
        
        private static bool ContainsFocus(int planetId, int focusid)
        {
            if (!_planetBases.ContainsKey(planetId)) return false;
            return Array.Exists(_planetBases[planetId], i => i == focusid);
        }
    }
}
