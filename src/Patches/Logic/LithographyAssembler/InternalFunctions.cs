﻿using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;

namespace ProjectGenesis.Patches.Logic.LithographyAssembler
{
    internal static partial class LithographyAssemblerPatches
    {
        internal static int GetLithographyLenId(int recipeId)
        {
            switch (recipeId)
            {
                case 751:
                case 752:
                    return 6201;

                case 753:
                case 754:
                    return 6202;

                case 755:
                case 756:
                    return 6203;

                default:
                    return 0;
            }
        }

        internal static void Export(BinaryWriter w)
        {
            w.Write(_lithographydata.Count);

            foreach (KeyValuePair<(int, int), LithographyData> pair in _lithographydata)
            {
                w.Write(pair.Key.Item1);
                w.Write(pair.Key.Item2);
                w.Write(pair.Value.ItemId);
                w.Write(pair.Value.ItemCount);
                w.Write(pair.Value.ItemInc);
                w.Write(pair.Value.NeedCount);
            }
        }

        internal static void Import(BinaryReader r)
        {
            ReInitAll();

            try
            {
                var count = r.ReadInt32();

                for (var j = 0; j < count; j++)
                {
                    var planetId = r.ReadInt32();
                    var assemblerId = r.ReadInt32();

                    _lithographydata.TryAdd((planetId, assemblerId),
                                            new LithographyData
                                            {
                                                ItemId = r.ReadInt32(), ItemCount = r.ReadInt32(), ItemInc = r.ReadInt32(), NeedCount = r.ReadInt32()
                                            });
                }
            }
            catch (EndOfStreamException)
            {
                // ignored
            }
        }

        internal static void IntoOtherSave() => ReInitAll();

        private static void ReInitAll() => _lithographydata = new ConcurrentDictionary<(int, int), LithographyData>();
    }
}