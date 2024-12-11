using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using ProjectGenesis.Utils;

namespace ProjectGenesis.Patches
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

            foreach ((Utils.ERecipeType _, List<ItemProto> value) in dict)
                value.Sort((proto1, proto2) => (int)(FactorySpeed(proto1) - FactorySpeed(proto2)));

            return dict;
        }

        internal static float FactorySpeed(ItemProto proto)
        {
            PrefabDesc desc = proto.prefabDesc;
            return desc.isLab ? desc.labAssembleSpeed : desc.assemblerSpeed;
        }

        internal static readonly Dictionary<ItemProto, NodeOptions> CustomOptions = new Dictionary<ItemProto, NodeOptions>();

        internal static readonly Dictionary<Utils.ERecipeType, ItemProto> DefaultMachine = new Dictionary<Utils.ERecipeType, ItemProto>();

        internal static EProliferatorStrategy DefaultStrategy = EProliferatorStrategy.Nonuse;

        internal static void SetDefaultMachine(Utils.ERecipeType type, ItemProto proto) => DefaultMachine[type] = proto;

        internal static void SetDefaultStrategy(EProliferatorStrategy strategy) => DefaultStrategy = strategy;

        public static void ClearOptions() => CustomOptions.Clear();

        public static void Export(BinaryWriter w)
        {
            w.Write((int)DefaultStrategy);

            w.Write(DefaultMachine.Count);

            foreach (var (type, item) in DefaultMachine)
            {
                w.Write((int)type);
                w.Write(item.ID);
            }

            w.Write(CustomOptions.Count);

            foreach (var (item, option) in CustomOptions)
            {
                w.Write(item.ID);
                option.Export(w);
            }
        }

        public static void Import(BinaryReader r)
        {
            var strategy = (EProliferatorStrategy)r.ReadInt32();

            DefaultStrategy = strategy;

            var defaultMachineCount = r.ReadInt32();

            for (int i = 0; i < defaultMachineCount; i++)
            {
                var type = (Utils.ERecipeType)r.ReadInt32();
                var itemId = r.ReadInt32();
                DefaultMachine[type] = LDB.items.Select(itemId);
            }

            var customOptionsCount = r.ReadInt32();

            for (int i = 0; i < customOptionsCount; i++)
            {
                var itemId = r.ReadInt32();
                var item = LDB.items.Select(itemId);

                var option = NodeOptions.Import(r);
                CustomOptions[item] = option;
            }
        }

        public static void IntoOtherSave()
        {
            // do nothing
        }
    }
}
