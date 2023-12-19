using System.Collections.Generic;
using CommonAPI.Systems;
using UnityEngine;
using xiaoye97;

// ReSharper disable CommentTypo
// ReSharper disable LoopCanBePartlyConvertedToQuery
// ReSharper disable Unity.PreferAddressByIdToGraphicsParams

#pragma warning disable CS0618

namespace ProjectGenesis.Utils
{
    internal static class CopyModelUtils
    {
        internal static void AddCopiedModelProto()
        {
            CopyModelProto(121, ProtoIDUsedByPatches.M量子储液罐, Color.HSVToRGB(0.5571f, 0.3188f, 0.8980f));
            CopyModelProto(194, ProtoIDUsedByPatches.M矿物处理厂, Color.HSVToRGB(0.2035f, 0.8326f, 0.9373f));
            CopyModelProto(49, ProtoIDUsedByPatches.M天穹装配厂, Color.HSVToRGB(0.0710f, 0.7412f, 0.8941f));
            CopyModelProto(49, ProtoIDUsedByPatches.M物质裂解塔, Color.HSVToRGB(0.6174f, 0.6842f, 0.9686f));
            CopyModelProto(49, ProtoIDUsedByPatches.M巨型化学反应釜, Color.HSVToRGB(0.1404f, 0.8294f, 0.9882f));
            CopyModelProto(49, ProtoIDUsedByPatches.M精密结构组装厂, Color.HSVToRGB(0.9814f, 0.6620f, 0.8471f));
            CopyModelProto(118, ProtoIDUsedByPatches.M反物质能量炉, Color.HSVToRGB(0.5985f, 0.7333f, 0.2353f));
            CopyModelProto(119, ProtoIDUsedByPatches.M大抽水机, Color.HSVToRGB(0.6174f, 0.6842f, 0.9686f));
            CopyModelProto(46, ProtoIDUsedByPatches.M同位素温差发电机, Color.HSVToRGB(0.4174f, 0.742f, 0.9686f));
            CopyModelProto(49, ProtoIDUsedByPatches.M物质分解设施, new Color(0.3216F, 0.8157F, 0.09020F));
            CopyModelProto(49, ProtoIDUsedByPatches.M巨型粒子对撞机, new Color(0.3059F, 0.2196F, 0.4941F));
            CopyModelProto(432, ProtoIDUsedByPatches.M洲际导弹组, new Color(0.3059F, 0.2196F, 0.4941F));
            CopyModelProto(432, ProtoIDUsedByPatches.M反物质导弹组, new Color(0.3059F, 0.2196F, 0.4941F));
            CopyModelProto(374, ProtoIDUsedByPatches.M高斯机枪塔MK2, new Color(0.0000f, 0.7490f, 1.0000f));
            CopyModelProto(373, ProtoIDUsedByPatches.M高频激光塔MK2, new Color(0.5765f, 0.4392f, 0.8588f));
            CopyModelProto(52, ProtoIDUsedByPatches.M量子储物仓, new Color(0.0000f, 0.7490f, 1.0000f));

            AddAtmosphericCollectStation();
        }

        private static void AddAtmosphericCollectStation()
        {
            var color = new Color32(60, 179, 113, 255);
            ModelProto oriModel = LDB.models.Select(50); //ILS
            Debug.Log(oriModel.name);
            PrefabDesc desc = oriModel.prefabDesc;
            var newMats = new List<Material>();

            foreach (Material[] lodMats in desc.lodMaterials)
            {
                if (lodMats == null) continue;

                foreach (Material mat in lodMats)
                {
                    if (mat == null) continue;
                    var newMaterial = new Material(mat);
                    newMaterial.SetColor("_Color", color);
                    newMats.Add(newMaterial);
                }
            }

            oriModel = LDB.models.Select(73); // ray receiver
            var collectEffectMat = new Material(oriModel.prefabDesc.lodMaterials[0][3]);
            collectEffectMat.SetColor("_TintColor", new Color32(131, 127, 197, 255));
            collectEffectMat.SetColor("_PolarColor", new Color32(234, 255, 253, 170));
            collectEffectMat.SetVector("_Aurora", new Vector4(75f, 1f, 20f, 0.1f));
            collectEffectMat.SetVector("_Beam", new Vector4(12f, 78f, 24f, 1f));
            collectEffectMat.SetVector("_Particle", new Vector4(2f, 30f, 5f, 0.8f));
            collectEffectMat.SetVector("_Circle", new Vector4(2.5f, 34f, 1f, 0.04f));

            newMats.Add(collectEffectMat);
            ModelProto registerModel = ProtoRegistry.RegisterModel(ProtoIDUsedByPatches.M大气采集器,
                                                                   "Assets/genesis-models/entities/prefabs/atmospheric-collect-station",
                                                                   newMats.ToArray());

            registerModel.HpMax = 300000;
            registerModel.RuinId = 384;
            registerModel.RuinType = ERuinType.Normal;
            registerModel.RuinCount = 1;
        }

        private static void CopyModelProto(int oriId, int id, Color color)
        {
            ModelProto oriModel = LDB.models.Select(oriId);
            ModelProto model = oriModel.Copy();
            model.Name = id.ToString();
            model.ID = id;
            PrefabDesc desc = oriModel.prefabDesc;
            model.prefabDesc = new PrefabDesc(id, desc.prefab, desc.colliderPrefab);

            for (int i = 0; i < model.prefabDesc.lodMaterials.Length; i++)
            {
                if (model.prefabDesc.lodMaterials[i] == null) continue;
                for (int j = 0; j < model.prefabDesc.lodMaterials[i].Length; j++)
                {
                    if (model.prefabDesc.lodMaterials[i][j] == null) continue;
                    model.prefabDesc.lodMaterials[i][j] = new Material(desc.lodMaterials[i][j]);
                }

                try
                {
                    model.prefabDesc.lodMaterials[i][0].color = color;
                }
                catch
                {
                    // ignored
                }
            }

            model.prefabDesc.modelIndex = id;
            model.prefabDesc.hasBuildCollider = true;
            model.prefabDesc.colliders = desc.colliders;
            model.prefabDesc.buildCollider = desc.buildCollider;
            model.prefabDesc.buildColliders = desc.buildColliders;
            model.prefabDesc.colliderPrefab = desc.colliderPrefab;

            model.sid = "";
            model.SID = "";

            LDBTool.PreAddProto(model);
        }

        internal static void ModelPostFix()
        {
            ModelProto modelProto = LDB.models.Select(ProtoIDUsedByPatches.M大气采集器);
            modelProto._ruinPath = "Entities/Prefabs/Ruins/interstellar-logistic-station-ruins";
            modelProto._wreckagePath = "Entities/Prefabs/Wreckages/interstellar-logistic-station-wreckages";

            ref PrefabDesc prefabDesc = ref LDB.models.Select(ProtoIDUsedByPatches.M同位素温差发电机).prefabDesc;
            ref Material[] prefabDescLODMaterial = ref prefabDesc.lodMaterials[0];
            prefabDescLODMaterial[2].SetColor("_TintColor", new Color(0.3861f, 2.4837f, 0.3137f, 0.7692f));
        }

        internal static void ItemPostFix()
        {
            LDB.items.Select(ProtoIDUsedByPatches.I水).recipes = new List<RecipeProto>() { LDB.recipes.Select(ProtoIDUsedByPatches.R海水淡化) };
            LDB.items.Select(ProtoIDUsedByPatches.I氢).isRaw = true;
        }
    }
}
