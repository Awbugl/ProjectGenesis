using System;
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

        private static readonly Harmony HarmonyPatch = new Harmony("ProjectGenesis.Compatibility." + GUID);

        private static readonly int[] AddedRecipes =
        {
            330, 331, 332, 333,
            334, 335, 336, 337,
            338, 339, 340, 341,
            342, 343, 344, 345,
            346, 347, 348, 349,
            350, 351, 363, 364,
            365, 366, 367, 368,
            369, 370, 371, 372,
            373, 375, 376, 377,
            378, 379, 380, 381,
            552, 553, 554, 555,
            556, 557, 558, 559,
            560, 561, 562,
        };

        private static bool _finished;

        internal static void Awake()
        {
            if (!Chainloader.PluginInfos.TryGetValue(GUID, out _)) return;

            HarmonyPatch.Patch(AccessTools.Method(typeof(VFPreload), nameof(VFPreload.InvokeOnLoadWorkEnded)), null,
                new HarmonyMethod(typeof(MoreMegaStructure), nameof(LDBToolOnPostAddDataAction))
                {
                    after = new[] { LDBToolPlugin.MODGUID, },
                });
        }

        public static void LDBToolOnPostAddDataAction()
        {
            if (_finished) return;

            foreach (int recipeID in AddedRecipes)
            {
                RecipeProto recipeProto = LDB.recipes.Select(recipeID);

                if (recipeProto == null) continue;

                recipeProto.Type = (ERecipeType)10;

                switch (recipeProto.ID)
                {
                    case 350:
                        recipeProto.Type = (ERecipeType)21;
                        recipeProto.Items = Array.Empty<int>();
                        recipeProto.ItemCounts = Array.Empty<int>();
                        recipeProto.Handcraft = false;

                        continue;

                    case 366:
                        recipeProto.Items = new[]
                        {
                            5203, 6271, 7707, 1124,
                            1118,
                        };

                        continue;
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

                        case 9487:
                            recipeProto.ItemCounts[0] = 2;
                            recipeProto.ItemCounts[1] = 1;
                            recipeProto.Items[1] = 6503;

                            continue;
                    }
                }
            }

            ItemProto itemProto = LDB.items.Select(9500);
            itemProto.recipes = null;
            itemProto.FindRecipes();
            itemProto.isRaw = true;

            _finished = true;
        }
    }
}
