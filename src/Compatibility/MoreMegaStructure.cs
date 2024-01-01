using System;
using BepInEx;
using BepInEx.Bootstrap;
using HarmonyLib;
using ProjectGenesis.Utils;
using xiaoye97;

// ReSharper disable InconsistentNaming
// ReSharper disable LoopCanBePartlyConvertedToQuery

namespace ProjectGenesis.Compatibility
{
    internal static class MoreMegaStructure
    {
        internal const string GUID = "Gnimaerd.DSP.plugin.MoreMegaStructure";

        private static readonly int[] AddedRecipes =
        {
            330, 331, 332, 333, 334, 335, 336, 337, 338, 339, 340, 341, 342, 343, 344, 345, 346, 347, 348, 349, 350, 351, 363, 364, 375, 376, 377,
            378, 379, 380, 381, 552, 553, 554, 555, 556, 557, 558, 559, 560, 561, 562
        };

        private static readonly int[] AddedItems =
        {
            9480, 9481, 9482, 9483, 9484, 9485, 9486, 9487, 9488, 9489, 9490, 9491, 9492, 9500, 9493, 9494, 9495, 9496, 9497, 9498, 9499, 9501,
            9502, 9512
        };

        private static bool _finished;

        internal static void Awake()
        {
            Chainloader.PluginInfos.TryGetValue(GUID, out PluginInfo pluginInfo);

            if (pluginInfo == null) return;

            var harmonyMethod
                = new HarmonyMethod(typeof(MoreMegaStructure), nameof(LDBToolOnPostAddDataAction))
                {
                    after = new[] { LDBToolPlugin.MODGUID }
                };

            new Harmony("org.LoShin.GenesisBook.Compatibility.MoreMegaStructure").Patch(AccessTools.Method(typeof(VFPreload), "InvokeOnLoadWorkEnded"), null, harmonyMethod);
        }

        public static void LDBToolOnPostAddDataAction()
        {
            if (_finished) return;

            foreach (int recipeID in AddedRecipes)
            {
                RecipeProto recipeProto = LDB.recipes.Select(recipeID);
                if (recipeProto == null) continue;
                recipeProto.Type = (ERecipeType)10;
                recipeProto.name = recipeProto.Name.Translate();
                recipeProto.description = recipeProto.Description.Translate();

                if (recipeProto.ID == 350)
                {
                    recipeProto.Type = (ERecipeType)21;
                    recipeProto.Items = Array.Empty<int>();
                    recipeProto.ItemCounts = Array.Empty<int>();
                    recipeProto.Handcraft = false;
                }

                if (recipeProto.Results.Length > 0)
                {
                    int recipeProtoResult = recipeProto.Results[0];

                    switch (recipeProtoResult)
                    {
                        case ProtoID.I巨型精密组装厂:
                        case ProtoID.I巨型对撞机:
                            recipeProto.ItemCounts[0] = 60;
                            continue;

                        case ProtoID.I人造恒星:
                            recipeProto.ItemCounts[0] = 30;
                            continue;

                        case 9487:
                            recipeProto.ItemCounts[0] = 2;
                            recipeProto.ItemCounts[1] = 1;
                            recipeProto.Items[1] = 6503;
                            continue;
                    }
                }
            }

            foreach (int itemID in AddedItems)
            {
                ItemProto itemProto = LDB.items.Select(itemID);
                if (itemProto == null) continue;
                itemProto.name = itemProto.Name.Translate();
                itemProto.description = itemProto.Description.Translate();

                if (itemProto.ID == 9500)
                {
                    itemProto.recipes = null;
                    AccessTools.Method(typeof(ItemProto), "FindRecipes").Invoke(itemProto, null);
                    itemProto.isRaw = true;
                }
            }

            _finished = true;
        }
    }
}
