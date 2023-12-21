using System;

namespace ProjectGenesis.Patches.Logic.QTools
{
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
}
