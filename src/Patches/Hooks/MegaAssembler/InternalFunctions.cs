using System.Collections.Generic;
using System.IO;

namespace ProjectGenesis.Patches
{
    internal static partial class MegaAssemblerPatches
    {
        internal static void Export(BinaryWriter w)
        {
            lock (Slotdata)
            {
                w.Write(Slotdata.Count);

                foreach (KeyValuePair<(int, int), SlotData[]> pair in Slotdata)
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
        }

        internal static void Import(BinaryReader r)
        {
            ReInitAll();

            int slotdatacount = r.ReadInt32();

            for (var j = 0; j < slotdatacount; j++)
            {
                int planetId = r.ReadInt32();
                int entityId = r.ReadInt32();
                int length = r.ReadInt32();
                var datas = new SlotData[length];

                for (var i = 0; i < length; i++)
                {
                    datas[i] = new SlotData
                    {
                        dir = (IODir)r.ReadInt32(),
                        beltId = r.ReadInt32(),
                        storageIdx = r.ReadInt32(),
                        counter = r.ReadInt32(),
                    };
                }

                Slotdata.TryAdd((planetId, entityId), datas);
            }
        }

        internal static void IntoOtherSave() => ReInitAll();

        private static void ReInitAll() => Slotdata.Clear();

        internal static bool ContainsRecipeTypeRevert(ERecipeType recipetype, ERecipeType filter) => ContainsRecipeType(filter, recipetype);

        internal static bool ContainsRecipeType(ERecipeType filter, ERecipeType recipetype)
        {
            var type = (Utils.ERecipeType)recipetype;

            switch ((Utils.ERecipeType)filter)
            {
                case Utils.ERecipeType.所有化工:
                    return type == Utils.ERecipeType.Chemical || type == Utils.ERecipeType.Refine || type == Utils.ERecipeType.高分子化工;

                case Utils.ERecipeType.所有熔炉: return type == Utils.ERecipeType.Smelt || type == Utils.ERecipeType.标准冶炼 || type == Utils.ERecipeType.高热冶炼;

                case Utils.ERecipeType.复合制造: return type == Utils.ERecipeType.Assemble || type == Utils.ERecipeType.标准制造;

                case Utils.ERecipeType.所有制造:
                    return type == Utils.ERecipeType.Chemical || type == Utils.ERecipeType.Refine || type == Utils.ERecipeType.Assemble
                        || type == Utils.ERecipeType.Particle || type == Utils.ERecipeType.标准制造 || type == Utils.ERecipeType.高精度加工
                        || type == Utils.ERecipeType.Research || type == Utils.ERecipeType.高分子化工;

                default: return filter == recipetype;
            }
        }
    }
}
