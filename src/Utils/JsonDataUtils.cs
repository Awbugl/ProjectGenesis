using System;
using System.Collections.Generic;
using CommonAPI.Systems;
using HarmonyLib;
using UnityEngine;
using xiaoye97;
using ERecipeType_1 = ERecipeType;

#pragma warning disable CS0618

namespace ProjectGenesis.Utils
{
    internal static class JsonDataUtils
    {
        internal static void ImportJson(int[] tableID)
        {
            #region StringProto

            StringProtoJson[] stringProtoJsons = JsonHelper.StringProtos();

            LDB.strings.OnAfterDeserialize();

            foreach (var stringProto in stringProtoJsons)
            {
                if (LDB.strings.Exist(stringProto.Name))
                    ProtoRegistry.EditString(stringProto.Name, stringProto.ENUS, stringProto.ZHCN);
                else
                    ProtoRegistry.RegisterString(stringProto.Name, stringProto.ENUS, stringProto.ZHCN);
            }

            #endregion

            #region TechProto

            var templateTech = LDB.techs.Select(1311);

            foreach (var techjson in JsonHelper.TechProtos())
            {
                var proto = LDB.techs.Exist(techjson.ID) ? LDB.techs.Select(techjson.ID) : templateTech.Copy();
                proto.ID = techjson.ID;
                proto.Name = techjson.Name;
                proto.Desc = techjson.Desc;
                proto.Conclusion = techjson.Conclusion;
                proto.Published = techjson.Published;
                proto.IconPath = techjson.IconPath;
                proto.IsLabTech = techjson.IsLabTech;
                proto.PreTechs = techjson.PreTechs;
                proto.PreTechsImplicit = techjson.PreTechsImplicit;
                proto.PreTechsMax = techjson.PreTechsMax;
                proto.Items = techjson.Items ?? Array.Empty<int>();
                proto.ItemPoints = techjson.ItemPoints ?? Array.Empty<int>();
                proto.AddItems = techjson.AddItems ?? Array.Empty<int>();
                proto.AddItemCounts = techjson.AddItemCounts ?? Array.Empty<int>();
                proto.Position = new Vector2(techjson.Position[0], techjson.Position[1]);
                proto.HashNeeded = techjson.HashNeeded;
                proto.UnlockRecipes = techjson.UnlockRecipes;
                proto.UnlockFunctions = techjson.UnlockFunctions;
                proto.UnlockValues = techjson.UnlockValues;
                proto.Level = techjson.Level;
                proto.MaxLevel = techjson.MaxLevel;
                proto.LevelCoef1 = techjson.LevelCoef1;
                proto.LevelCoef2 = techjson.LevelCoef2;
                proto.PropertyOverrideItems = techjson.PropertyOverrideItems;
                proto.PropertyItemCounts = techjson.PropertyItemCounts;

                if (!LDB.techs.Exist(techjson.ID)) LDBTool.PreAddProto(proto);
            }

            #endregion

            #region ItemProto

            ref Dictionary<int, IconToolNew.IconDesc> itemIconDescs
                = ref AccessTools.StaticFieldRefAccess<Dictionary<int, IconToolNew.IconDesc>>(typeof(ProtoRegistry), "itemIconDescs");

            foreach (var itemjson in JsonHelper.ItemProtos())
            {
                itemjson.GridIndex = GetTableID(itemjson.GridIndex);

                var exist = LDB.items.Exist(itemjson.ID);

                var proto = exist
                                ? LDB.items.Select(itemjson.ID)
                                : ProtoRegistry.RegisterItem(itemjson.ID, itemjson.Name, itemjson.Description, itemjson.IconPath, itemjson.GridIndex,
                                                             itemjson.StackSize, (EItemType)itemjson.Type, IconDescUtils.GetIconDesc(itemjson.ID));

                if (exist && proto.IconPath != itemjson.IconPath) itemIconDescs.Add(itemjson.ID, IconDescUtils.GetIconDesc(itemjson.ID));

                proto.ID = itemjson.ID;
                proto.Name = itemjson.Name.TranslateFromJson();
                proto.Description = itemjson.Description;
                proto.IconPath = itemjson.IconPath;
                proto.StackSize = itemjson.StackSize;
                proto.GridIndex = itemjson.GridIndex;
                proto.FuelType = itemjson.FuelType;
                proto.HeatValue = itemjson.HeatValue;
                proto.ReactorInc = itemjson.ReactorInc;
                proto.DescFields = itemjson.DescFields ?? Array.Empty<int>();
                proto.IsFluid = itemjson.IsFluid;
                proto.Type = (EItemType)itemjson.Type;
                proto.SubID = itemjson.SubID;
                proto.MiningFrom = itemjson.MiningFrom;
                proto.ProduceFrom = itemjson.ProduceFrom;
                proto.Grade = itemjson.Grade;
                proto.Upgrades = itemjson.Upgrades ?? Array.Empty<int>();
                proto.IsEntity = itemjson.IsEntity;
                proto.CanBuild = itemjson.CanBuild;
                proto.BuildInGas = itemjson.BuildInGas;
                proto.ModelIndex = itemjson.ModelIndex;
                proto.ModelCount = itemjson.ModelCount;
                proto.HpMax = itemjson.HpMax;
                proto.Ability = itemjson.Ability;
                proto.Potential = itemjson.Potential;
                proto.BuildIndex = itemjson.BuildIndex;
                proto.BuildMode = itemjson.BuildMode;
                proto.UnlockKey = itemjson.UnlockKey;
                proto.MechaMaterialID = itemjson.MechaMaterialID;
                proto.PreTechOverride = itemjson.PreTechOverride;
                proto.Productive = itemjson.Productive;
            }

            #endregion

            #region RecipeProto

            foreach (var recipeJson in JsonHelper.RecipeProtos())
            {
                recipeJson.GridIndex = GetTableID(recipeJson.GridIndex);

                var proto = LDB.recipes.Exist(recipeJson.ID)
                                ? LDB.recipes.Select(recipeJson.ID)
                                : ProtoRegistry.RegisterRecipe(recipeJson.ID, (ERecipeType_1)recipeJson.Type, recipeJson.Time, recipeJson.Input,
                                                               recipeJson.InCounts, recipeJson.Output ?? Array.Empty<int>(),
                                                               recipeJson.OutCounts ?? Array.Empty<int>(), recipeJson.Description, 0,
                                                               recipeJson.GridIndex, recipeJson.Name, recipeJson.IconPath);

                proto.Explicit = recipeJson.Explicit;
                proto.Name = recipeJson.Name;
                proto.Handcraft = recipeJson.Handcraft;
                proto.Type = (ERecipeType_1)recipeJson.Type;
                proto.TimeSpend = recipeJson.Time;
                proto.Items = recipeJson.Input;
                proto.ItemCounts = recipeJson.InCounts;
                proto.Results = recipeJson.Output ?? Array.Empty<int>();
                proto.ResultCounts = recipeJson.OutCounts ?? Array.Empty<int>();
                proto.Description = recipeJson.Description;
                proto.GridIndex = recipeJson.GridIndex;
                proto.IconPath = recipeJson.IconPath;
                proto.NonProductive = recipeJson.NonProductive;
            }

            #endregion

            int GetTableID(int gridIndex)
            {
                if (gridIndex >= 4000) return tableID[1] * 1000 + (gridIndex - 4000);
                if (gridIndex >= 3000) return tableID[0] * 1000 + (gridIndex - 3000);
                return gridIndex;
            }
        }

        internal static void SetBuildBar()
        {
            LDBTool.SetBuildBar(1, 10, 6261);
            LDBTool.SetBuildBar(3, 10, 2313);
            LDBTool.SetBuildBar(4, 4, 6229);
            LDBTool.SetBuildBar(5, 3, 6230);
            LDBTool.SetBuildBar(5, 4, 2303);
            LDBTool.SetBuildBar(5, 5, 2304);
            LDBTool.SetBuildBar(5, 6, 2305);
            LDBTool.SetBuildBar(5, 7, 2308);
            LDBTool.SetBuildBar(5, 8, 2309);
            LDBTool.SetBuildBar(5, 9, 2317);
            LDBTool.SetBuildBar(5, 10, 2310);
            LDBTool.SetBuildBar(6, 5, 6267);
            LDBTool.SetBuildBar(6, 6, 2209);
            LDBTool.SetBuildBar(7, 3, 6257);
            LDBTool.SetBuildBar(7, 4, 6258);
            LDBTool.SetBuildBar(7, 5, 6259);
            LDBTool.SetBuildBar(7, 6, 6260);
            LDBTool.SetBuildBar(7, 7, 6264);
            LDBTool.SetBuildBar(7, 8, 6265);
            LDBTool.SetBuildBar(7, 9, 6275);
            LDBTool.SetBuildBar(7, 10, 6276);
            LDBTool.SetBuildBar(2, 5, 6266);
        }
    }
}
