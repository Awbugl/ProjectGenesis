﻿using System;
using System.Collections.Generic;
using ProjectGenesis.Utils;
using static ProjectGenesis.Patches.QTools;

namespace ProjectGenesis.Patches
{
    internal class NodeData
    {
        internal NodeDataSet DataSet;
        internal ItemProto Item;
        internal float ItemCount;
        internal NodeOptions Options;
        internal bool IsNeed;

        internal void RefreshFactoryCount()
        {
            float assemblerSpeed = FactorySpeed(Options.Factory);

            int idx = Array.IndexOf(Options.Recipe.Results, Item.ID);

            float count = ItemCount * Options.Recipe.TimeSpend / Options.Recipe.ResultCounts[idx] / assemblerSpeed / 0.36f;

            switch (Options.Strategy)
            {
                case EProliferatorStrategy.ExtraProducts:
                    count *= 0.8f;

                    break;

                case EProliferatorStrategy.ProductionSpeedup:
                    count *= 0.5f;

                    break;
            }

            if (Options.Factory.ModelIndex == ProtoID.M负熵熔炉) count *= 0.5f;

            Options.FactoryCount = count;
        }

        public void CheckFactory()
        {
            var type = (Utils.ERecipeType)Options.Recipe.Type;
            List<ItemProto> recipeTypeFactory = RecipeTypeFactoryMap[type];

            if (!recipeTypeFactory.Contains(Options.Factory))
            {
                if (!DefaultMachine.TryGetValue(type, out ItemProto factory))
                {
                    factory = recipeTypeFactory[0];
                    SetDefaultMachine(type, factory);
                }

                Options.Factory = factory;

                if (Options.Strategy == EProliferatorStrategy.ExtraProducts && !Options.Recipe.productive)
                    Options.Strategy = EProliferatorStrategy.Nonuse;
            }
        }

        internal void RefreshNeeds() => DataSet.RefreshNeeds();

        public void RemoveNeed() => DataSet.RemoveNeed(this);

        public void MarkAsNeed() => IsNeed = true;
    }
}