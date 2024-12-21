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
            ref Dictionary<int, IconToolNew.IconDesc> itemIconDescs =
                ref AccessTools.StaticFieldRefAccess<Dictionary<int, IconToolNew.IconDesc>>(typeof(ProtoRegistry), "itemIconDescs");

        #region TechProto

            foreach (TechProtoJson protoJson in TechProtos())
            {
                if (LDB.techs.Exist(protoJson.ID)) { protoJson.ToProto(LDB.techs.Select(protoJson.ID)); }
                else { LDBTool.PreAddProto(protoJson.ToProto()); }
            }

        #endregion

        #region Mod ItemProto

            foreach (ItemProtoJson protoJson in ItemModProtos())
            {
                protoJson.GridIndex = GetTableID(protoJson.GridIndex);
                itemIconDescs.Add(protoJson.ID, IconDescUtils.GetIconDesc(protoJson.ID));
                LDBTool.PreAddProto(protoJson.ToProto());
            }

        #endregion

        #region Vanilla ItemProto

            foreach (ItemProtoJson protoJson in ItemVanillaProtos())
            {
                protoJson.GridIndex = GetTableID(protoJson.GridIndex);
                ItemProto proto = LDB.items.Select(protoJson.ID);

                if (proto.IconPath != protoJson.IconPath) { itemIconDescs.Add(protoJson.ID, IconDescUtils.GetIconDesc(protoJson.ID)); }

                protoJson.ToProto(proto);
            }

        #endregion

        #region RecipeProto

            foreach (RecipeProtoJson protoJson in RecipeProtos())
            {
                protoJson.GridIndex = GetTableID(protoJson.GridIndex);

                if (LDB.recipes.Exist(protoJson.ID)) { protoJson.ToProto(LDB.recipes.Select(protoJson.ID)); }
                else { LDBTool.PreAddProto(protoJson.ToProto()); }
            }

        #endregion

        #region TutorialProto

            foreach (TutorialProtoJson protoJson in TutorialProtos()) { LDBTool.PreAddProto(protoJson.ToProto()); }

        #endregion

        #region GoalProto

            foreach (GoalProtoJson protoJson in GoalProtos())
            {
                if (LDB.goals.Exist(protoJson.ID)) { protoJson.ToProto(LDB.goals.Select(protoJson.ID)); }
                else { LDBTool.PreAddProto(protoJson.ToProto()); }
            }

        #endregion

            int GetTableID(int gridIndex)
            {
                if (gridIndex >= 5000) { return (tableID[2] - 5) * 1000 + gridIndex; }

                if (gridIndex >= 4000) { return (tableID[1] - 4) * 1000 + gridIndex; }

                if (gridIndex >= 3000) { return (tableID[0] - 3) * 1000 + gridIndex; }

                return gridIndex;
            }
        }

        internal static void PrefabDescPostFix()
        {
            PrefabDescJson[] prefabDescs = PrefabDescs();

            foreach (PrefabDescJson json in prefabDescs) { json.ToPrefabDesc(LDB.models.Select(json.ModelID).prefabDesc); }

            PrefabDesc megaPumper = LDB.models.Select(ProtoID.M大抽水机).prefabDesc;
            megaPumper.waterPoints = new[] { Vector3.zero, };
            megaPumper.portPoses = new[] { megaPumper.portPoses[0], };
        }
    }
}
