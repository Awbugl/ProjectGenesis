using System;
using System.Collections.Generic;

namespace ProjectGenesis.Patches.Logic
{
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

        public void CheckFactory()
        {
            var type = (Utils.ERecipeType)Options.Recipe.Type;
            List<ItemProto> recipeTypeFactory = QTools.RecipeTypeFactoryMap[type];

            if (!recipeTypeFactory.Contains(Options.Factory))
            {
                if (!DataSet.DefaultMachine.TryGetValue(type, out ItemProto factory))
                {
                    factory = QTools.RecipeTypeFactoryMap[type][0];
                    DataSet.SetDefaultMachine(type, factory);
                }

                Options.Factory = factory;

                DataSet.CustomOptions.Remove(Item);
            }
        }

        internal void UpdateNeeds() => DataSet.RefreshNeeds();

        public void RemoveNeed() => DataSet.RemoveNeed(this);
    }
}
