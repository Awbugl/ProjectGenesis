using System;
using BepInEx;
using HarmonyLib;
using ProjectGenesis.Utils;
using xiaoye97;

// ReSharper disable InconsistentNaming
// ReSharper disable MemberCanBeInternal
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable LoopCanBePartlyConvertedToQuery

namespace ProjectGenesis.Compatibility
{
    [BepInPlugin(MODGUID, MODNAME, VERSION)]
    [BepInDependency(MoreMegaStructureGUID)]
    public class MoreMegaStructureCompatibilityPlugin : BaseUnityPlugin
    {
        public const string MODGUID = "org.LoShin.GenesisBook.Compatibility.MoreMegaStructure";
        public const string MODNAME = "GenesisBook.Compatibility.MoreMegaStructure";
        public const string VERSION = "1.0.0";

        private const string MoreMegaStructureGUID = "Gnimaerd.DSP.plugin.MoreMegaStructure";

        public void Awake()
        {
            var harmonyMethod
                = new HarmonyMethod(typeof(MoreMegaStructureCompatibilityPlugin), nameof(LDBToolOnPostAddDataAction))
                  {
                      after = new[] { LDBToolPlugin.MODGUID }
                  };

            new Harmony(MODGUID).Patch(AccessTools.Method(typeof(VFPreload), "InvokeOnLoadWorkEnded"), null, harmonyMethod);
        }

        private static readonly int[] AddedRecipes =
        {
            330, 331, 332, 333, 334, 335, 336, 337, 350, 338, 339, 340, 341, 342, 554, 351, 556, 558, 562, 559, 561, 553, 560, 557, 555, 552, 376,
            377, 378, 379, 380, 381, 343, 344, 345, 346, 347, 348, 349, 363, 364, 375
        };

        private static readonly int[] AddedItems =
        {
            9480, 9481, 9482, 9483, 9484, 9485, 9486, 9487, 9488, 9489, 9490, 9491, 9492, 9500, 9493, 9494, 9495, 9496, 9497, 9498, 9499, 9501,
            9502, 9512
        };

        public static void LDBToolOnPostAddDataAction()
        {
            foreach (var strings in LDB.strings.dataArray)
            {
                switch (strings.Name)
                {
                    case "引力钻头描述":
                        strings.ZHCN = "借助黑洞本身的引力，引力钻头能够将物质从黑洞中取出。借助谐振盘，黑洞原质将能够被解压并在星系内输送。";
                        strings.ENUS
                            = "The graviton drill can pull matter out of the black hole using the gravity of the black hole itself. With the help of the resonant disc, the matter from the black hole will be able to be decompressed and transported within the galaxy.";
                        break;

                    case "物质解压器功能文本":
                        strings.ZHCN = "产出一些基础资源（例如铁块），可被对应的物质重构器接收。";
                        break;

                    case "单极磁石重构装置":
                        strings.ZHCN = "钨矿重构装置";
                        strings.ENUS = "Tungsten receiver";
                        break;
                }
            }

            foreach (var recipeID in AddedRecipes)
            {
                var recipeProto = LDB.recipes.Select(recipeID);
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
                    var recipeProtoResult = recipeProto.Results[0];

                    switch (recipeProtoResult)
                    {
                        case ProtoIDUsedByPatches.I巨型精密组装厂:
                        case ProtoIDUsedByPatches.I巨型对撞机:
                            recipeProto.ItemCounts[0] = 60;
                            continue;

                        case ProtoIDUsedByPatches.I人造恒星:
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

            foreach (var itemID in AddedItems)
            {
                var itemProto = LDB.items.Select(itemID);
                if (itemProto == null) continue;
                itemProto.name = itemProto.Name.Translate();
                itemProto.description = itemProto.Description.Translate();

                if (itemProto.ID == 9500)
                {
                    itemProto.recipes = null;
                    AccessTools.Method(typeof(ItemProto), "FindRecipes").Invoke(itemProto, null);
                }
            }
        }
    }
}
