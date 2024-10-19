using System;
using System.Collections.Generic;
using CommonAPI.Systems;
using HarmonyLib;
using UnityEngine;
using xiaoye97;
using static ProjectGenesis.Utils.JsonHelper;

// ReSharper disable RemoveRedundantBraces

namespace ProjectGenesis.Utils
{
    internal static class JsonDataUtils
    {
        internal static void ImportJson(int[] tableID)
        {
            ref Dictionary<int, IconToolNew.IconDesc> itemIconDescs
                = ref AccessTools.StaticFieldRefAccess<Dictionary<int, IconToolNew.IconDesc>>(typeof(ProtoRegistry),
                    "itemIconDescs");

            #region TechProto

            foreach (TechProtoJson techjson in TechProtos())
            {
                if (LDB.techs.Exist(techjson.ID))
                {
                    techjson.ToProto(LDB.techs.Select(techjson.ID));
                }
                else
                {
                    LDBTool.PreAddProto(techjson.ToProto());
                }
            }

            #endregion

            #region Mod ItemProto

            foreach (ItemProtoJson itemjson in ItemModProtos())
            {
                itemjson.GridIndex = GetTableID(itemjson.GridIndex);
                itemIconDescs.Add(itemjson.ID, IconDescUtils.GetIconDesc(itemjson.ID));
                LDBTool.PreAddProto(itemjson.ToProto());
            }

            #endregion

            #region Vanilla ItemProto

            foreach (ItemProtoJson itemjson in ItemVanillaProtos())
            {
                itemjson.GridIndex = GetTableID(itemjson.GridIndex);
                ItemProto proto = LDB.items.Select(itemjson.ID);

                if (proto.IconPath != itemjson.IconPath)
                {
                    itemIconDescs.Add(itemjson.ID, IconDescUtils.GetIconDesc(itemjson.ID));
                }

                itemjson.ToProto(proto);
            }

            #endregion

            #region RecipeProto

            foreach (RecipeProtoJson recipeJson in RecipeProtos())
            {
                recipeJson.GridIndex = GetTableID(recipeJson.GridIndex);

                if (LDB.recipes.Exist(recipeJson.ID))
                {
                    recipeJson.ToProto(LDB.recipes.Select(recipeJson.ID));
                }
                else
                {
                    LDBTool.PreAddProto(recipeJson.ToProto());
                }
            }

            #endregion

            #region TutorialProto

            TutorialProtoJson[] tutorialProtos = TutorialProtos();

            foreach (TutorialProtoJson json in tutorialProtos)
            {
                LDBTool.PreAddProto(json.ToProto());
            }

            #endregion

            int GetTableID(int gridIndex)
            {
                if (gridIndex >= 5000)
                {
                    return (tableID[2] - 5) * 1000 + gridIndex;
                }

                if (gridIndex >= 4000)
                {
                    return (tableID[1] - 4) * 1000 + gridIndex;
                }

                if (gridIndex >= 3000)
                {
                    return (tableID[0] - 3) * 1000 + gridIndex;
                }

                return gridIndex;
            }
        }

        internal static void PrefabDescPostFix()
        {
            PrefabDescJson[] prefabDescs = PrefabDescs();

            foreach (PrefabDescJson json in prefabDescs)
            {
                json.ToPrefabDesc(LDB.models.Select(json.ModelID).prefabDesc);
            }

            PrefabDesc megaPumper = LDB.models.Select(ProtoID.M大抽水机).prefabDesc;
            megaPumper.waterPoints = new[] { Vector3.zero, };
            megaPumper.portPoses = new[] { megaPumper.portPoses[0], };
        }

        internal static void ModifyUpgradeTech()
        {
            TechProto tech = LDB.techs.Select(ProtoID.T批量建造1);
            tech.ItemPoints = new[] { 3 };

            for (int i = ProtoID.T宇宙探索1; i <= ProtoID.T宇宙探索4; i++)
            {
                TechProto techProto = LDB.techs.Select(i);
                techProto.Items = new[] { 6001, };
                techProto.ItemPoints = new[] { techProto.ItemPoints[0], };
                techProto.PreTechsImplicit = Array.Empty<int>();
            }
        }
    }
}