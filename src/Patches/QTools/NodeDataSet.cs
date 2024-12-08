using System;
using System.Collections.Generic;
using System.Linq;
using ProjectGenesis.Utils;
using UnityEngine;
using static ProjectGenesis.Patches.QTools;

namespace ProjectGenesis.Patches
{
    internal class NodeDataSet
    {
        internal readonly Dictionary<ItemProto, NodeData> AsRaws = new Dictionary<ItemProto, NodeData>(); // AsRaws

        internal readonly Dictionary<ItemProto, NodeData> Byproducts = new Dictionary<ItemProto, NodeData>(); // by product

        internal readonly Dictionary<ItemProto, NodeData> Datas = new Dictionary<ItemProto, NodeData>(); // middle tier products 

        internal readonly Dictionary<ItemProto, NodeData> Factories = new Dictionary<ItemProto, NodeData>(); // middle tier products 

        internal readonly Dictionary<ItemProto, NodeData> Needs = new Dictionary<ItemProto, NodeData>(); // final product

        internal readonly Dictionary<ItemProto, NodeData> Raws = new Dictionary<ItemProto, NodeData>(); // ore

        private float _totalProliferatedItemCount;

        private float ProliferatorCount => _totalProliferatedItemCount / 74;

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
                if (CustomOptions.TryGetValue(node.Item, out NodeOptions option)) node.Options = option;

                AddNodeChilds(node);
            }

            // ReSharper disable once LoopCanBePartlyConvertedToQuery
            foreach (NodeData node in Byproducts.Values.ToArray())
                if (ReuseByProducts(node, AsRaws))
                    ReuseByProducts(node, Raws);

            if (_totalProliferatedItemCount > 0) MergeRaws(ItemRaw(QTools.ProliferatorProto, ProliferatorCount));

            OnNeedRefreshed?.Invoke();
        }

        public void CalcFactories()
        {
            Factories.Clear();

            // ReSharper disable once ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
            foreach (NodeData node in Datas.Values)
            {
                NodeOptions nodeOptions = node.Options;

                if (!nodeOptions.AsRaw && nodeOptions.Factory != null)
                    MergeFactories(ItemRaw(nodeOptions.Factory, Mathf.Ceil(nodeOptions.FactoryCount)));
            }
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

                node.ItemCount -= t.ItemCount;

                datas.Remove(t.Item);

                if (node.ItemCount < 1e-6)
                {
                    Byproducts.Remove(node.Item);

                    return false;
                }

                return true;
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

            if (Byproducts.TryGetValue(node.Item, out NodeData t))
            {
                if (t.ItemCount >= node.ItemCount)
                {
                    t.ItemCount -= node.ItemCount;

                    if (t.ItemCount < 1e-6) Byproducts.Remove(t.Item);

                    return;
                }

                node.ItemCount -= t.ItemCount;

                Byproducts.Remove(t.Item);

                if (node.ItemCount < 1e-6) return;
            }

            MergeData(node);

            RecipeProto recipe = node.Options.Recipe;

            int idx = Array.IndexOf(recipe.Results, node.Item.ID);

            int resultsLength = recipe.Results.Length;

            if (resultsLength > 1)
                for (var index = 0; index < resultsLength; index++)
                {
                    if (idx != index)
                    {
                        ItemProto proto = LDB.items.Select(recipe.Results[index]);
                        float count = node.ItemCount * recipe.ResultCounts[index] / recipe.ResultCounts[idx];
                        MergeByproducts(ItemRaw(proto, count));
                    }
                }

            int itemsLength = recipe.Items.Length;

            for (var index = 0; index < itemsLength; index++)
            {
                ItemProto proto = LDB.items.Select(recipe.Items[index]);
                float count = node.ItemCount * recipe.ItemCounts[index] / recipe.ResultCounts[idx];

                if (node.Options.Factory.ModelIndex == ProtoID.M负熵熔炉) count *= 0.5f;

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

            datas.Add(node.Item, node);

            return node;
        }

        private void MergeData(NodeData node) => MergeNode(Datas, node).RefreshFactoryCount();

        private void MergeNeeds(NodeData node) => MergeNode(Needs, node);

        private void MergeRaws(NodeData node) => MergeNode(Raws, node);

        private void MergeAsRaws(NodeData node) => MergeNode(AsRaws, node);

        private void MergeByproducts(NodeData node) => MergeNode(Byproducts, node);

        private void MergeFactories(NodeData node) => MergeNode(Factories, node);

        public void ClearNeeds()
        {
            Needs.Clear();
            RefreshNeeds();
        }

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
            if (CustomOptions.TryGetValue(proto, out NodeOptions option))
            {
                if (proto.isRaw || option.AsRaw) return ItemRaw(proto, count, option);

                NodeData data = ItemNeed(proto, count, option);
                data.RefreshFactoryCount();

                return data;
            }

            RecipeProto recipe = proto.recipes.FirstOrDefault();
            Utils.ERecipeType type = (Utils.ERecipeType?)recipe?.Type ?? Utils.ERecipeType.None;

            if (type == Utils.ERecipeType.None) return ItemRaw(proto, count);

            if (!DefaultMachine.TryGetValue(type, out ItemProto factory))
            {
                factory = QTools.RecipeTypeFactoryMap[type][0];
                SetDefaultMachine(type, factory);
            }

            return ItemNeed(proto, count, factory, recipe, !string.IsNullOrWhiteSpace(proto.miningFrom));
        }

        private NodeData ItemRaw(ItemProto proto, float count, NodeOptions option)
        {
            var data = new NodeData
            {
                DataSet = this,
                Item = proto,
                ItemCount = count,
                Options = option,
            };

            return data;
        }

        private NodeData ItemRaw(ItemProto proto, float count)
        {
            var data = new NodeData
            {
                DataSet = this,
                Item = proto,
                ItemCount = count,
                Options = new NodeOptions(proto, null, null, EProliferatorStrategy.Nonuse, true),
            };

            data.Options.OnOptionsChange += OnOptionsChange;

            return data;
        }

        private NodeData ItemNeed(ItemProto proto, float count, NodeOptions option)
        {
            var data = new NodeData
            {
                DataSet = this,
                Item = proto,
                ItemCount = count,
                Options = option,
            };

            data.RefreshFactoryCount();

            return data;
        }

        private NodeData ItemNeed(ItemProto proto, float count, ItemProto factory, RecipeProto recipe, bool asRaw = false)
        {
            EProliferatorStrategy strategy = DefaultStrategy;

            if (strategy == EProliferatorStrategy.ExtraProducts && !recipe.productive) strategy = EProliferatorStrategy.Nonuse;

            var data = new NodeData
            {
                DataSet = this,
                Item = proto,
                ItemCount = count,
                Options = new NodeOptions(proto, factory, recipe, strategy, asRaw),
            };

            data.RefreshFactoryCount();
            data.Options.OnOptionsChange += OnOptionsChange;

            return data;
        }

        private void OnOptionsChange(NodeOptions options) => CustomOptions[options.Item] = options;

        internal bool IsEmpty() => Needs.Count == 0;
    }
}
