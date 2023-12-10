using System;
using System.Collections.Generic;
using System.Linq;
using ProjectGenesis.Utils;

namespace ProjectGenesis.Patches.Logic
{
    internal static class QTools
    {
        internal static Dictionary<Utils.ERecipeType, List<ItemProto>> RecipeTypeFactoryMap;

        internal static Dictionary<Utils.ERecipeType, List<ItemProto>> GetFactoryDict()
        {
            var dict = new Dictionary<Utils.ERecipeType, List<ItemProto>>();

            ItemProto[] items = LDB.items.dataArray;

            foreach (ItemProto proto in items)
            {
                PrefabDesc protoPrefabDesc = proto.prefabDesc;

                if (!protoPrefabDesc.isAssembler) continue;

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

    internal class NodeDataSet
    {
        private readonly List<NodeData> _needs = new List<NodeData>(); // final product

        private readonly List<NodeData> _inputs = new List<NodeData>(); // ore and input

        private readonly Dictionary<ItemProto, NodeData> _datas = new Dictionary<ItemProto, NodeData>(); // middle tier products 

        private readonly Dictionary<ItemProto, NodeOptions> _customOptions = new Dictionary<ItemProto, NodeOptions>(); // middle tier products 

        private readonly Dictionary<Utils.ERecipeType, ItemProto> _defaultMachine = new Dictionary<Utils.ERecipeType, ItemProto>();

        private EProliferatorStrategy _defaultStrategy = EProliferatorStrategy.Nonuse;

        public void RefreshNeeds()
        {
            _datas.Clear();
            _inputs.Clear();
            
            foreach (NodeData node in _needs) { }
        }

        internal void SetDefaultMachine(Utils.ERecipeType type, ItemProto proto) => _defaultMachine[type] = proto;

        internal void SetDefaultStrategy(EProliferatorStrategy strategy) => _defaultStrategy = strategy;

        public NodeData AddItemNeed(ItemProto proto, float count)
        {
            RecipeProto recipe = proto.recipes.FirstOrDefault();
            
            if (recipe == null)
            {
                recipe = new RecipeProto()
                         {
                             _iconSprite = LDB.signals.IconSprite(509),
                             name = "",
                             Type = ERecipeType.None,
                             GridIndex = 0,
                             TimeSpend = 60,
                             Items = new int[] { },
                             ItemCounts = new int[] { },
                             Results = new int[] { proto.ID },
                             ResultCounts = new int[] { 1 }
                         };
            }

            var type = (Utils.ERecipeType)recipe.Type;

            ItemProto factory = _defaultMachine[type];

            if (factory == null)
            {
                factory = QTools.RecipeTypeFactoryMap[type][0];
                SetDefaultMachine(type, factory);
            }

            var data = new NodeData
                       {
                           DataSet = this, Item = proto, ItemCount = count, Options = new NodeOptions(proto, factory, recipe, _defaultStrategy)
                       };

            data.RefreshFactoryCount();
            data.Options.OnOptionsChange += OnOptionsChange;
            _needs.Add(data);
            return data;
        }

        private void OnOptionsChange(NodeOptions options) => _customOptions[options.Item] = options;
    }

    internal class NodeData
    {
        internal NodeDataSet DataSet;
        internal ItemProto Item;
        internal float ItemCount;
        internal NodeOptions Options;

        internal void RefreshFactoryCount()
        {
            int idx = Array.IndexOf(Options.Recipe.Results, Item.ID);

            float count = ItemCount * Options.Recipe.TimeSpend / Options.Recipe.ResultCounts[idx] / Options.Factory.prefabDesc.assemblerSpeed / 0.36f;

            switch (Options.Strategy)
            {
                case EProliferatorStrategy.ExtraProducts:
                    count /= 1.25f;
                    break;

                case EProliferatorStrategy.ProductionSpeedup:
                    count /= 2f;
                    break;
            }

            Options.FactoryCount = count;
        }

        internal void UpdateNeeds() => DataSet.RefreshNeeds();
    }

    internal class NodeOptions
    {
        internal ItemProto Item { get; }
        
        internal float FactoryCount { get; set; }
        
        internal NodeOptions(
            ItemProto item,
            ItemProto factory,
            RecipeProto recipe,
            EProliferatorStrategy strategy,
            bool asRaw)
        {
            Item = item;
            _factory = factory;
            _recipe = recipe;
            _strategy = strategy;
            _asRaw = asRaw;
        }

        internal event Action<NodeOptions> OnOptionsChange;

        private ItemProto _factory;

        internal ItemProto Factory
        {
            get => _factory;
            set
            {
                _factory = value;
                OnOptionsChange?.Invoke(this);
            }
        }

        private bool _asRaw;
        
        internal bool AsRaw
        {
            get => _asRaw;
            set
            {
                _asRaw = value;
                OnOptionsChange?.Invoke(this);
            }
        }
        
        private RecipeProto _recipe;

        internal RecipeProto Recipe
        {
            get => _recipe;
            set
            {
                _recipe = value;
                OnOptionsChange?.Invoke(this);
            }
        }

        private EProliferatorStrategy _strategy;

        internal EProliferatorStrategy Strategy
        {
            get => _strategy;
            set
            {
                _strategy = value;
                OnOptionsChange?.Invoke(this);
            }
        }
    }

    internal enum EProliferatorStrategy
    {
        Nonuse,
        ExtraProducts,
        ProductionSpeedup
    }
}
