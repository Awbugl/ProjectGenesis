using System.Collections.Concurrent;
using System.Collections.Generic;
using ProjectGenesis.Utils;

namespace ProjectGenesis.Patches.Logic.QTools
{
    internal static class QTools
    {
        internal static readonly ItemProto ProliferatorProto = LDB.items.Select(ProtoID.I增产剂);
        private static ConcurrentDictionary<Utils.ERecipeType, List<ItemProto>> _recipeTypeFactoryMap;

        internal static ConcurrentDictionary<Utils.ERecipeType, List<ItemProto>> RecipeTypeFactoryMap =>
            _recipeTypeFactoryMap ?? (_recipeTypeFactoryMap = GetFactoryDict());

        private static ConcurrentDictionary<Utils.ERecipeType, List<ItemProto>> GetFactoryDict()
        {
            var dict = new ConcurrentDictionary<Utils.ERecipeType, List<ItemProto>>();

            ItemProto[] items = LDB.items.dataArray;

            foreach (ItemProto proto in items)
            {
                PrefabDesc protoPrefabDesc = proto.prefabDesc;

                if (!protoPrefabDesc.isAssembler)
                {
                    if (protoPrefabDesc.isLab) dict.TryAddOrInsert(Utils.ERecipeType.Research, proto);

                    continue;
                }

                var filter = (Utils.ERecipeType)protoPrefabDesc.assemblerRecipeType;

                if (filter == Utils.ERecipeType.None) continue;

                switch (filter)
                {
                    case Utils.ERecipeType.所有化工:
                    {
                        dict.TryAddOrInsert(Utils.ERecipeType.Chemical, proto);
                        dict.TryAddOrInsert(Utils.ERecipeType.Refine, proto);
                        dict.TryAddOrInsert(Utils.ERecipeType.高分子化工, proto);

                        continue;
                    }

                    case Utils.ERecipeType.所有熔炉:
                    {
                        dict.TryAddOrInsert(Utils.ERecipeType.Smelt, proto);
                        dict.TryAddOrInsert(Utils.ERecipeType.矿物处理, proto);

                        continue;
                    }

                    case Utils.ERecipeType.所有制造:
                    {
                        dict.TryAddOrInsert(Utils.ERecipeType.Chemical, proto);
                        dict.TryAddOrInsert(Utils.ERecipeType.Refine, proto);
                        dict.TryAddOrInsert(Utils.ERecipeType.Assemble, proto);
                        dict.TryAddOrInsert(Utils.ERecipeType.Particle, proto);
                        dict.TryAddOrInsert(Utils.ERecipeType.标准制造, proto);
                        dict.TryAddOrInsert(Utils.ERecipeType.高精度加工, proto);
                        dict.TryAddOrInsert(Utils.ERecipeType.Research, proto);
                        dict.TryAddOrInsert(Utils.ERecipeType.高分子化工, proto);

                        continue;
                    }

                    case Utils.ERecipeType.复合制造:
                    {
                        dict.TryAddOrInsert(Utils.ERecipeType.Assemble, proto);
                        dict.TryAddOrInsert(Utils.ERecipeType.标准制造, proto);

                        continue;
                    }

                    default:
                    {
                        dict.TryAddOrInsert(filter, proto);

                        continue;
                    }
                }
            }

            return dict;
        }
    }

    internal enum EProliferatorStrategy
    {
        Nonuse,
        ExtraProducts,
        ProductionSpeedup,
    }
}
