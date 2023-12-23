using System.Collections.Generic;
using CommonAPI.Systems;
using UnityEngine;
using xiaoye97;

// ReSharper disable CommentTypo
// ReSharper disable LoopCanBePartlyConvertedToQuery
// ReSharper disable Unity.UnknownResource
// ReSharper disable Unity.PreferAddressByIdToGraphicsParams

namespace ProjectGenesis.Utils
{
    internal static class CopyModelUtils
    {
        internal static void AddCopiedModelProto()
        {
            CopyModelProto(121, ProtoID.M量子储液罐, Color.HSVToRGB(0.5571f, 0.3188f, 0.8980f));
            CopyModelProto(194, ProtoID.M矿物处理厂, Color.HSVToRGB(0.2035f, 0.8326f, 0.9373f));
            CopyModelProto(49, ProtoID.M天穹装配厂, Color.HSVToRGB(0.0710f, 0.7412f, 0.8941f));
            CopyModelProto(49, ProtoID.M物质裂解塔, Color.HSVToRGB(0.6174f, 0.6842f, 0.9686f));
            CopyModelProto(49, ProtoID.M巨型化学反应釜, Color.HSVToRGB(0.1404f, 0.8294f, 0.9882f));
            CopyModelProto(49, ProtoID.M精密结构组装厂, Color.HSVToRGB(0.9814f, 0.6620f, 0.8471f));
            CopyModelProto(56, ProtoID.M人造恒星MK2);
            CopyModelProto(119, ProtoID.M大抽水机, Color.HSVToRGB(0.6174f, 0.6842f, 0.9686f));
            CopyModelProto(46, ProtoID.M同位素温差发电机, Color.HSVToRGB(0.4174f, 0.742f, 0.9686f));
            CopyModelProto(49, ProtoID.M物质分解设施, new Color(0.3216F, 0.8157F, 0.09020F));
            CopyModelProto(49, ProtoID.M巨型粒子对撞机, new Color(0.3059F, 0.2196F, 0.4941F));
            CopyModelProto(432, ProtoID.M洲际导弹组, new Color(0.0000f, 0.7490f, 1.0000f));
            CopyModelProto(432, ProtoID.M反物质导弹组, new Color(0.3059F, 0.2196F, 0.4941F));
            CopyModelProto(374, ProtoID.M高斯机枪塔MK2, new Color(0.0000f, 0.7490f, 1.0000f));
            CopyModelProto(373, ProtoID.M高频激光塔MK2, new Color(0.5765f, 0.4392f, 0.8588f));
            CopyModelProto(52, ProtoID.M量子储物仓, new Color(0.7373f, 0.2118f, 0.8510f));

            AddAtmosphericCollectStation();
        }

        private static void AddAtmosphericCollectStation()
        {
            ModelProto oriModel = LDB.models.Select(ProtoID.M星际物流运输站);
            PrefabDesc desc = oriModel.prefabDesc;

            var newMats = new List<Material>();
            foreach (Material[] lodMats in desc.lodMaterials)
            {
                if (lodMats == null) continue;

                foreach (Material mat in lodMats)
                {
                    if (mat == null) continue;
                    var newMaterial = new Material(mat);
                    newMaterial.SetColor("_Color", new Color32(60, 179, 113, 255));
                    newMats.Add(newMaterial);
                }
            }

            oriModel = LDB.models.Select(ProtoID.M射线接收站); // ray receiver
            var collectEffectMat = new Material(oriModel.prefabDesc.lodMaterials[0][3]);

            collectEffectMat.SetColor("_TintColor", new Color32(131, 127, 197, 255));
            collectEffectMat.SetColor("_PolarColor", new Color32(234, 255, 253, 170));
            collectEffectMat.SetVector("_Aurora", new Vector4(75f, 1f, 20f, 0.1f));
            collectEffectMat.SetVector("_Beam", new Vector4(12f, 78f, 24f, 1f));
            collectEffectMat.SetVector("_Particle", new Vector4(2f, 30f, 5f, 0.8f));
            collectEffectMat.SetVector("_Circle", new Vector4(2.5f, 34f, 1f, 0.04f));

            newMats.Add(collectEffectMat);

            ModelProto registerModel = ProtoRegistry.RegisterModel(ProtoID.M大气采集器,
                                                                   "Assets/genesis-models/entities/prefabs/atmospheric-collect-station",
                                                                   newMats.ToArray());

            registerModel.HpMax = 300000;
            registerModel.RuinId = 384;
            registerModel.RuinType = ERuinType.Normal;
            registerModel.RuinCount = 1;
        }

        private static void CopyModelProto(int oriId, int id, Color? color = null)
        {
            ModelProto oriModel = LDB.models.Select(oriId);
            ModelProto model = oriModel.Copy();
            model.Name = id.ToString();
            model.ID = id;

            PrefabDesc desc = oriModel.prefabDesc;
            GameObject prefab = desc.prefab ? desc.prefab : Resources.Load<GameObject>(oriModel.PrefabPath);
            GameObject colliderPrefab = desc.colliderPrefab ? desc.colliderPrefab : Resources.Load<GameObject>(oriModel._colliderPath);

            ref PrefabDesc modelPrefabDesc = ref model.prefabDesc;
            modelPrefabDesc = prefab == null ? PrefabDesc.none :
                colliderPrefab == null       ? new PrefabDesc(id, prefab) : new PrefabDesc(id, prefab, colliderPrefab);

            foreach (Material[] lodMaterial in modelPrefabDesc.lodMaterials)
            {
                if (lodMaterial == null) continue;

                for (int j = 0; j < lodMaterial.Length; j++)
                {
                    ref Material material = ref lodMaterial[j];

                    if (material == null) continue;
                    material = new Material(material);
                }

                if (!color.HasValue) continue;

                try
                {
                    lodMaterial[0].color = color.Value;
                }
                catch
                {
                    // ignored
                }
            }

            modelPrefabDesc.modelIndex = id;
            modelPrefabDesc.hasBuildCollider = desc.hasBuildCollider;
            modelPrefabDesc.colliders = desc.colliders;
            modelPrefabDesc.buildCollider = desc.buildCollider;
            modelPrefabDesc.buildColliders = desc.buildColliders;
            modelPrefabDesc.colliderPrefab = desc.colliderPrefab;
            modelPrefabDesc.dragBuild = desc.dragBuild;
            modelPrefabDesc.dragBuildDist = desc.dragBuildDist;
            modelPrefabDesc.blueprintBoxSize = desc.blueprintBoxSize;
            modelPrefabDesc.roughHeight = desc.roughHeight;
            modelPrefabDesc.roughWidth = desc.roughWidth;
            modelPrefabDesc.roughRadius = desc.roughRadius;
            modelPrefabDesc.barHeight = desc.barHeight;
            modelPrefabDesc.barWidth = desc.barWidth;

            model.sid = "";
            model.SID = "";

            LDBTool.PreAddProto(model);
        }

        private static ModelProto Copy(this ModelProto proto)
            => new ModelProto
            {
                ObjectType = proto.ObjectType,
                RuinType = proto.RuinType,
                RendererType = proto.RendererType,
                HpMax = proto.HpMax,
                HpUpgrade = proto.HpUpgrade,
                HpRecover = proto.HpRecover,
                RuinId = proto.RuinId,
                RuinCount = proto.RuinCount,
                RuinLifeTime = proto.RuinLifeTime,
                PrefabPath = proto.PrefabPath,
                _colliderPath = proto._colliderPath,
                _ruinPath = proto._ruinPath,
                _wreckagePath = proto._wreckagePath,
                _ruinOriginModelIndex = proto._ruinOriginModelIndex
            };

        internal static void ModelPostFix()
        {
            ModelProto modelProto = LDB.models.Select(ProtoID.M大气采集器);
            modelProto._ruinPath = "Entities/Prefabs/Ruins/interstellar-logistic-station-ruins";
            modelProto._wreckagePath = "Entities/Prefabs/Wreckages/interstellar-logistic-station-wreckages";

            PrefabDesc prefabDesc = LDB.models.Select(ProtoID.M同位素温差发电机).prefabDesc;
            ref Material[] prefabDescLODMaterial = ref prefabDesc.lodMaterials[0];
            prefabDescLODMaterial[2].SetColor("_TintColor", new Color(0.2715f, 1.7394f, 0.1930f));

            prefabDesc = LDB.models.Select(ProtoID.M人造恒星MK2).prefabDesc;
            var texture = Resources.Load<Texture>("Assets/texpack/人造恒星MK2材质");
            prefabDescLODMaterial = ref prefabDesc.lodMaterials[0];
            prefabDescLODMaterial[0].SetTexture("_EmissionTex", texture);
            prefabDescLODMaterial[1].SetColor("_TintColor", new Color(0.1804f, 0.4953f, 1.3584f));  //亮部
            prefabDescLODMaterial[1].SetColor("_TintColor1", new Color(0.1294f, 0.3130f, 1.1508f)); //暗部
            prefabDescLODMaterial[1].SetColor("_RimColor", new Color(0.4157f, 0.6784f, 1.0000f));   //边缘特效
        }

        internal static void ItemPostFix()
        {
            LDB.items.Select(ProtoID.I水).recipes = new List<RecipeProto> { LDB.recipes.Select(ProtoID.R海水淡化) };
            LDB.items.Select(ProtoID.I氢).isRaw = true;
        }
    }
}
