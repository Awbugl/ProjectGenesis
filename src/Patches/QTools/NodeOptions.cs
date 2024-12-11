using System;
using System.IO;

namespace ProjectGenesis.Patches
{
    internal class NodeOptions
    {
        private bool _asRaw;

        private ItemProto _factory;

        private RecipeProto _recipe;

        private EProliferatorStrategy _strategy;

        internal NodeOptions(ItemProto item, ItemProto factory, RecipeProto recipe, EProliferatorStrategy strategy, bool asRaw)
        {
            Item = item;
            _factory = factory;
            _recipe = recipe;
            _strategy = strategy;
            _asRaw = asRaw;
        }

        internal ItemProto Item { get; }

        internal float FactoryCount { get; set; }

        internal ItemProto Factory
        {
            get => _factory;
            set
            {
                _factory = value;
                OnOptionsChange?.Invoke(this);
            }
        }

        internal bool AsRaw
        {
            get => _asRaw;
            set
            {
                _asRaw = value;
                OnOptionsChange?.Invoke(this);
            }
        }

        internal RecipeProto Recipe
        {
            get => _recipe;
            set
            {
                _recipe = value;
                OnOptionsChange?.Invoke(this);
            }
        }

        internal EProliferatorStrategy Strategy
        {
            get => _strategy;
            set
            {
                _strategy = value;
                OnOptionsChange?.Invoke(this);
            }
        }

        internal event Action<NodeOptions> OnOptionsChange;

        public void Export(BinaryWriter w)
        {
            w.Write(Item.ID);
            w.Write(_factory.ID);
            w.Write(_recipe.ID);
            w.Write((int)_strategy);
            w.Write(_asRaw);
        }

        public static NodeOptions Import(BinaryReader r)
        {
            var itemId = r.ReadInt32();
            var factoryId = r.ReadInt32();
            var recipeId = r.ReadInt32();
            var strategy = (EProliferatorStrategy)r.ReadInt32();
            var asRaw = r.ReadBoolean();

            return new NodeOptions(LDB.items.Select(itemId), LDB.items.Select(factoryId), LDB.recipes.Select(recipeId), strategy, asRaw);
        }
    }
}
