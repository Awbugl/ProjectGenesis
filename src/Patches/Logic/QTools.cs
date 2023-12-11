using System;
using System.Collections.Generic;
using System.Linq;
using ProjectGenesis.Utils;

namespace ProjectGenesis.Patches.Logic
{
    internal static class QTools
    {
        internal static readonly ItemProto ProliferatorProto = LDB.items.Select(1143);

        internal static Dictionary<Utils.ERecipeType, List<ItemProto>> RecipeTypeFactoryMap;

        internal static Dictionary<Utils.ERecipeType, List<ItemProto>> GetFactoryDict()
        {
            var dict = new Dictionary<Utils.ERecipeType, List<ItemProto>>();

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
        internal readonly Dictionary<ItemProto, NodeData> Byproducts = new Dictionary<ItemProto, NodeData>(); // by product

        internal readonly Dictionary<ItemProto, NodeData> Needs = new Dictionary<ItemProto, NodeData>(); // final product

        internal readonly Dictionary<ItemProto, NodeData> Raws = new Dictionary<ItemProto, NodeData>(); // ore

        internal readonly Dictionary<ItemProto, NodeData> AsRaws = new Dictionary<ItemProto, NodeData>(); // AsRaws

        internal readonly Dictionary<ItemProto, NodeData> Datas = new Dictionary<ItemProto, NodeData>(); // middle tier products 

        private readonly Dictionary<ItemProto, NodeOptions> _customOptions = new Dictionary<ItemProto, NodeOptions>();

        private readonly Dictionary<Utils.ERecipeType, ItemProto> _defaultMachine = new Dictionary<Utils.ERecipeType, ItemProto>();

        private float ProliferatorCount => _totalProliferatedItemCount / 74;

        private float _totalProliferatedItemCount;

        private EProliferatorStrategy _defaultStrategy = EProliferatorStrategy.Nonuse;

        internal event Action OnNeedRefreshed;

        public void RefreshNeeds()
        {
            _totalProliferatedItemCount = 0f;
            Datas.Clear();
            Raws.Clear();
            AsRaws.Clear();
            Byproducts.Clear();

            foreach (NodeData node in Needs.Values)
            {
                if (_customOptions.TryGetValue(node.Item, out NodeOptions option)) node.Options = option;

                AddNodeChilds(node);
            }

            // ReSharper disable once LoopCanBePartlyConvertedToQuery
            foreach (NodeData node in Byproducts.Values.ToArray())
            {
                if (ReuseByProducts(node, AsRaws))
                {
                    ReuseByProducts(node, Raws);
                }
            }

            if (_totalProliferatedItemCount > 0) MergeRaws(ItemRaw(QTools.ProliferatorProto, ProliferatorCount));

            OnNeedRefreshed?.Invoke();
        }

        private bool ReuseByProducts(NodeData node, Dictionary<ItemProto, NodeData> datas)
        {
            if (datas.TryGetValue(node.Item, out NodeData t))
            {
                if (t.ItemCount >= node.ItemCount)
                {
                    t.ItemCount -= node.ItemCount;

                    Byproducts.Remove(node.Item);

                    if (t.ItemCount < 1e-6) datas.Remove(t.Item);

                    return false;
                }
                else
                {
                    node.ItemCount -= t.ItemCount;

                    datas.Remove(t.Item);

                    if (node.ItemCount < 1e-6)
                    {
                        Byproducts.Remove(node.Item);
                        return false;
                    }

                    return true;
                }
            }

            return true;
        }

        private void AddNodeChilds(NodeData node)
        {
            if (node.Item.isRaw)
            {
                MergeRaws(node);
                return;
            }

            if (node.Options.AsRaw)
            {
                MergeAsRaws(node);
                return;
            }

            MergeData(node);

            RecipeProto recipe = node.Options.Recipe;

            int idx = Array.IndexOf(recipe.Results, node.Item.ID);

            int resultsLength = recipe.Results.Length;

            if (resultsLength > 1)
                for (int index = 0; index < resultsLength; index++)
                {
                    if (idx != index)
                    {
                        ItemProto proto = LDB.items.Select(recipe.Results[index]);
                        float count = node.ItemCount * recipe.ResultCounts[index] / recipe.ResultCounts[idx];
                        MergeByproducts(ItemRaw(proto, count));
                    }
                }

            int itemsLength = recipe.Items.Length;

            for (int index = 0; index < itemsLength; index++)
            {
                ItemProto proto = LDB.items.Select(recipe.Items[index]);
                float count = node.ItemCount * recipe.ItemCounts[index] / recipe.ResultCounts[idx];

                if (node.Options.Strategy == EProliferatorStrategy.ExtraProducts) count *= 0.8f;

                if (node.Options.Strategy != EProliferatorStrategy.Nonuse) _totalProliferatedItemCount += count;

                NodeData nodeData = ItemNeed(proto, count);

                AddNodeChilds(nodeData);
            }
        }

        private NodeData MergeNode(Dictionary<ItemProto, NodeData> datas, NodeData node)
        {
            if (datas.TryGetValue(node.Item, out NodeData t))
            {
                t.ItemCount += node.ItemCount;
                return t;
            }
            else
            {
                datas.Add(node.Item, node);
                return node;
            }
        }

        private void MergeData(NodeData node) => MergeNode(Datas, node).RefreshFactoryCount();

        private void MergeNeeds(NodeData node) => MergeNode(Needs, node);

        private void MergeRaws(NodeData node) => MergeNode(Raws, node);

        private void MergeAsRaws(NodeData node) => MergeNode(AsRaws, node);

        private void MergeByproducts(NodeData node) => MergeNode(Byproducts, node);

        public void ClearNeeds()
        {
            Needs.Clear();
            RefreshNeeds();
        }

        internal void SetDefaultMachine(Utils.ERecipeType type, ItemProto proto) => _defaultMachine[type] = proto;

        internal void SetDefaultStrategy(EProliferatorStrategy strategy) => _defaultStrategy = strategy;

        public void AddItemNeed(ItemProto proto, float count)
        {
            NodeData data = ItemNeed(proto, count);
            MergeNeeds(data);
            RefreshNeeds();
        }

        public void RemoveNeed(NodeData nodeData)
        {
            Needs.Remove(nodeData.Item);
            RefreshNeeds();
        }

        private NodeData ItemNeed(ItemProto proto, float count)
        {
            if (_customOptions.TryGetValue(proto, out NodeOptions option))
            {
                if (proto.isRaw || option.AsRaw) return ItemRaw(proto, count, option);

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
                           DataSet = this,
                           Item = proto,
                           ItemCount = count,
                           Options = new NodeOptions(proto, null, null, EProliferatorStrategy.Nonuse, true)
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
            EProliferatorStrategy strategy = _defaultStrategy;

            if (strategy == EProliferatorStrategy.ExtraProducts && !recipe.productive) strategy = EProliferatorStrategy.Nonuse;

            var data = new NodeData
                       {
                           DataSet = this, Item = proto, ItemCount = count, Options = new NodeOptions(proto, factory, recipe, strategy, false)
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
            PrefabDesc factoryPrefabDesc = Options.Factory.prefabDesc;

            float assemblerSpeed = factoryPrefabDesc.assemblerSpeed;

            if (factoryPrefabDesc.isLab) assemblerSpeed = factoryPrefabDesc.labAssembleSpeed * 10000;

            int idx = Array.IndexOf(Options.Recipe.Results, Item.ID);

            float count = ItemCount * Options.Recipe.TimeSpend / Options.Recipe.ResultCounts[idx] / assemblerSpeed / 0.36f;

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

        public void RemoveNeed() => DataSet.RemoveNeed(this);
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
