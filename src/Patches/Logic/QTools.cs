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
        internal readonly List<NodeData> Byproducts = new List<NodeData>(); // by product

        internal readonly List<NodeData> Needs = new List<NodeData>(); // final product

        internal readonly List<NodeData> Inputs = new List<NodeData>(); // ore and input

        internal readonly Dictionary<ItemProto, NodeData> Datas = new Dictionary<ItemProto, NodeData>(); // middle tier products 

        private readonly Dictionary<ItemProto, NodeOptions> _customOptions = new Dictionary<ItemProto, NodeOptions>();

        private readonly Dictionary<Utils.ERecipeType, ItemProto> _defaultMachine = new Dictionary<Utils.ERecipeType, ItemProto>();

        private EProliferatorStrategy _defaultStrategy = EProliferatorStrategy.Nonuse;

        internal event Action OnNeedRefreshed;

        public void RefreshNeeds()
        {
            Datas.Clear();
            Inputs.Clear();

            foreach (NodeData node in Needs)
            {
                if (node.Options.AsRaw)
                {
                    Inputs.Add(node);
                    continue;
                }

                if (_customOptions.TryGetValue(node.Item, out NodeOptions option))
                {
                    node.Options = option;
                    node.RefreshFactoryCount();
                }

                AddNodeChilds(node);
            }

            OnNeedRefreshed?.Invoke();
        }

        private void AddNodeChilds(NodeData node)
        {
            if (node.Options.AsRaw)
            {
                Inputs.Add(node);
                return;
            }

            RecipeProto recipe = node.Options.Recipe;

            int idx = Array.IndexOf(recipe.Results, node.Item.ID);

            int resultsLength = recipe.Results.Length;

            if (resultsLength > 1 && !recipe.SpecialStackingLogic)
                for (int index = 0; index < resultsLength; index++)
                {
                    if (idx != index)
                    {
                        ItemProto proto = LDB.items.Select(recipe.Results[index]);
                        float count = node.ItemCount * recipe.ResultCounts[index] / recipe.ResultCounts[idx];
                        Byproducts.Add(ItemRaw(proto, count));
                    }
                }

            int itemsLength = recipe.Items.Length;

            for (int index = 0; index < itemsLength; index++)
            {
                ItemProto proto = LDB.items.Select(recipe.Items[index]);
                float count = node.ItemCount * recipe.ItemCounts[index] / recipe.ResultCounts[idx];
                if (node.Options.Strategy == EProliferatorStrategy.ExtraProducts) count *= 0.8f;

                NodeData nodeData = ItemNeed(proto, count);
                AddNodeChilds(nodeData);

                MergeData(nodeData);
            }
        }

        private void MergeData(NodeData nodeData)
        {
            if (Datas.TryGetValue(nodeData.Item, out NodeData t))
            {
                t.ItemCount += nodeData.ItemCount;
                t.RefreshFactoryCount();
            }
            else
            {
                Datas.Add(nodeData.Item, nodeData);
            }
        }

        internal void SetDefaultMachine(Utils.ERecipeType type, ItemProto proto) => _defaultMachine[type] = proto;

        internal void SetDefaultStrategy(EProliferatorStrategy strategy) => _defaultStrategy = strategy;

        public void AddItemNeed(ItemProto proto, float count)
        {
            NodeData data = ItemNeed(proto, count);
            Needs.Add(data);
            RefreshNeeds();
        }

        private NodeData ItemNeed(ItemProto proto, float count)
        {
            if (_customOptions.TryGetValue(proto, out NodeOptions option))
            {
                if (option.AsRaw) return ItemRaw(proto, count, option);

                NodeData data = ItemNeed(proto, count, option);
                data.RefreshFactoryCount();
                return data;
            }

            RecipeProto recipe = proto.recipes.FirstOrDefault();
            Utils.ERecipeType type = (Utils.ERecipeType?)recipe?.Type ?? Utils.ERecipeType.None;

            if (type == Utils.ERecipeType.None)
            {
                return ItemRaw(proto, count);
            }
            else
            {
                if (!_defaultMachine.TryGetValue(type, out ItemProto factory))
                {
                    factory = QTools.RecipeTypeFactoryMap[type][0];
                    SetDefaultMachine(type, factory);
                }

                return ItemNeed(proto, count, factory, recipe);
            }
        }

        private NodeData ItemRaw(ItemProto proto, float count, NodeOptions option)
        {
            var data = new NodeData { DataSet = this, Item = proto, ItemCount = count, Options = option };
            return data;
        }

        private NodeData ItemRaw(ItemProto proto, float count)
        {
            var data = new NodeData
                       {
                           DataSet = this, Item = proto, ItemCount = count, Options = new NodeOptions(proto, null, null, _defaultStrategy, true)
                       };

            data.Options.OnOptionsChange += OnOptionsChange;

            return data;
        }

        private NodeData ItemNeed(ItemProto proto, float count, NodeOptions option)
        {
            var data = new NodeData { DataSet = this, Item = proto, ItemCount = count, Options = option };

            data.RefreshFactoryCount();
            return data;
        }

        private NodeData ItemNeed(
            ItemProto proto,
            float count,
            ItemProto factory,
            RecipeProto recipe)
        {
            var data = new NodeData
                       {
                           DataSet = this, Item = proto, ItemCount = count, Options = new NodeOptions(proto, factory, recipe, _defaultStrategy, false)
                       };

            data.RefreshFactoryCount();
            data.Options.OnOptionsChange += OnOptionsChange;
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
