using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;

namespace ProjectGenesis.Patches.Logic.MegaAssembler
{
    internal static partial class MegaAssemblerPatches
    {
        internal static void Export(BinaryWriter w)
        {
            w.Write(_slotdata.Count);

            foreach (KeyValuePair<(int, int), SlotData[]> pair in _slotdata)
            {
                w.Write(pair.Key.Item1);
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

        internal static void Import(BinaryReader r)
        {
            ReInitAll();

            var slotdatacount = r.ReadInt32();

            for (var j = 0; j < slotdatacount; j++)
            {
                var key = r.ReadInt32();
                var key2 = r.ReadInt32();
                var length = r.ReadInt32();
                var datas = new SlotData[length];
                for (var i = 0; i < length; i++)
                {
                    datas[i] = new SlotData
                               {
                                   dir = (IODir)r.ReadInt32(), beltId = r.ReadInt32(), storageIdx = r.ReadInt32(), counter = r.ReadInt32()
                               };
                }

                _slotdata.TryAdd((key, key2), datas);
            }
        }

        internal static void IntoOtherSave() => ReInitAll();

        private static void ReInitAll()
        {
            _slotdata = new ConcurrentDictionary<(int, int), SlotData[]>();
            TmpSandCount = 0;
        }

        private static bool ContainsRecipeType(ERecipeType filter, ERecipeType recipetype)
        {
            var type = (Utils.ERecipeType)recipetype;

            switch ((Utils.ERecipeType)filter)
            {
                case Utils.ERecipeType.所有化工:
                    return type == Utils.ERecipeType.Chemical || type == Utils.ERecipeType.Refine || type == Utils.ERecipeType.高分子化工;

                case Utils.ERecipeType.所有熔炉:
                    return type == Utils.ERecipeType.Smelt || type == Utils.ERecipeType.矿物处理;

                case Utils.ERecipeType.所有高精:
                    return type == Utils.ERecipeType.高精度加工 || type == Utils.ERecipeType.电路蚀刻;

                default:
                    return filter == recipetype;
            }
        }
    }
}
