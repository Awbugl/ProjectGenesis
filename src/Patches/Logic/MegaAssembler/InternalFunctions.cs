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
                var planetId = r.ReadInt32();
                var factory = GameMain.galaxy.PlanetById(planetId)?.factory;
                var entityId = r.ReadInt32();
                var length = r.ReadInt32();
                var datas = new SlotData[length];

                for (var i = 0; i < length; i++)
                {
                    datas[i] = new SlotData
                               {
                                   dir = (IODir)r.ReadInt32(), beltId = r.ReadInt32(), storageIdx = r.ReadInt32(), counter = r.ReadInt32()
                               };

                    if (factory == null) continue;

                    factory.ReadObjectConn(entityId, i, out _, out var otherObjId, out _);

                    if (otherObjId <= 0 || factory.entityPool[otherObjId].beltId != datas[i].beltId)
                    {
                        var beltComponent = factory.cargoTraffic.beltPool[datas[i].beltId];
                        ref var signData = ref factory.entitySignPool[beltComponent.entityId];
                        signData.iconType = 0U;
                        signData.iconId0 = 0U;

                        datas[i] = new SlotData();
                    }
                }

                _slotdata.TryAdd((planetId, entityId), datas);
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
