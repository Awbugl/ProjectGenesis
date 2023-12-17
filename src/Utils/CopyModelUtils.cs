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
            ModelProto tankModel = CopyModelProto(121, 501, Color.HSVToRGB(0.5571f, 0.3188f, 0.8980f));
            LDBTool.PreAddProto(tankModel);

            ModelProto oreFactoryModel = CopyModelProto(194, 502, Color.HSVToRGB(0.2035f, 0.8326f, 0.9373f));
            LDBTool.PreAddProto(oreFactoryModel);

            ModelProto testCraftingTableModel = CopyModelProto(49, 503, Color.HSVToRGB(0.0710f, 0.7412f, 0.8941f));
            LDBTool.PreAddProto(testCraftingTableModel);

            ModelProto testCraftingTableModel2 = CopyModelProto(49, 504, Color.HSVToRGB(0.6174f, 0.6842f, 0.9686f));
            LDBTool.PreAddProto(testCraftingTableModel2);

            ModelProto testCraftingTableModel3 = CopyModelProto(49, 505, Color.HSVToRGB(0.1404f, 0.8294f, 0.9882f));
            LDBTool.PreAddProto(testCraftingTableModel3);

            ModelProto testCraftingTableModel4 = CopyModelProto(49, 506, Color.HSVToRGB(0.9814f, 0.6620f, 0.8471f));
            LDBTool.PreAddProto(testCraftingTableModel4);

            ModelProto antiMatterModel = CopyModelProto(118, 507, Color.HSVToRGB(0.5985f, 0.7333f, 0.2353f));
            LDBTool.PreAddProto(antiMatterModel);

            ModelProto megapumper = CopyModelProto(119, 508, Color.HSVToRGB(0.6174f, 0.6842f, 0.9686f));
            LDBTool.PreAddProto(megapumper);

            ModelProto atomicBattery = CopyModelProto(46, 509, Color.HSVToRGB(0.4174f, 0.742f, 0.9686f));
            LDBTool.PreAddProto(atomicBattery);

            ModelProto trashTower = CopyModelProto(49, 510, new Color(0.3216F, 0.8157F, 0.09020F));
            LDBTool.PreAddProto(trashTower);

            ModelProto testCraftingTableModel6 = CopyModelProto(49, 511, new Color(0.3059F, 0.2196F, 0.4941F));
            LDBTool.PreAddProto(testCraftingTableModel6);

            ModelProto testMissileModel1 = CopyModelProto(432, 513, new Color(0.3059F, 0.2196F, 0.4941F));
            LDBTool.PreAddProto(testMissileModel1);

            ModelProto testMissileModel2 = CopyModelProto(432, 514, new Color(0.3059F, 0.2196F, 0.4941F));
            LDBTool.PreAddProto(testMissileModel2);

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

            //_TintColor
            //      Values on Ray Receiver = Color32(230, 255, 253, 255)
            collectEffectMat.SetColor("_TintColor", new Color32(131, 127, 197, 255));

            //_AuroraColor
            //      Values on Ray Receiver = White

            //_PolarColor
            //      Values on Ray Receiver = Color32(234, 177, 255, 203)
            collectEffectMat.SetColor("_PolarColor", new Color32(234, 255, 253, 170));

            //_Multiplier: Entire Effect Transparency
            //      Values on Ray Receiver = 40

            //_AuroraMultiplier: Aurora Transparency
            //      Values on Ray Receiver = 0.2

            //_AlphaMultiplier Also Entire Effect Transparency?
            //      Values on Ray Receiver = 1

            //_AuroraMask: ??
            //      Values on Ray Receiver = (0.03, 0.01, -0.1, 1)

            //_PScle: particle strength ("sparkle")
            //      Values on Ray Receiver = 20

            //_WrpScale: wrap scale ??
            //      Values on Ray Receiver = 0.1

            //_UVSpeed: ?? area of the aurora texture sampled?
            //      Values on Ray Receiver = (-0.07, 0, 0.1, 1)

            //_InvFade: Soft Particles Factor Range(0.01, 3) ??
            //      Values on Ray Receiver = 0.414

            //_SideFade: Also Entire Effect Transparency? Range(0,2)
            //      Values on Ray Receiver = 0.429

            //_NoiseScale ("Disturbance control x: Disturbance speed yz Disturbance strength in lateral and longitudinal directions w: Rotational disturbance", Vector) = (1,1,1,1) //15 1.8 1.6 0.4
            //      Values on Ray Receiver = (15, 1.8, 1.6, 0.4)
            //collectEffectMat.SetVector("_NoiseScale", new Vector4(75f, 1f, 20f, 0.1f));

            //_Aurora: Aurora control x: height of aurora (percentage of atmosphere) y: width z: thickness of aurora (percentage of aurora to ground) w: disturbance
            //      Values on Ray Receiver = (75, 2.45, 20, 0.1)
            collectEffectMat.SetVector("_Aurora", new Vector4(75f, 1f, 20f, 0.1f));

            //_Beam: Light column control x: width y: height (percentage from starting point to atmosphere) z: starting height w: intensity
            //      Values on Ray Receiver = (10, 78, 10, 0.1)
            collectEffectMat.SetVector("_Beam", new Vector4(12f, 78f, 24f, 1f));

            //_Particle: Particle Controls x: scale y: height z: velocity w: stagger value
            //      Values on Ray Receiver = (1, 12.7, 1.2, 0.5)
            collectEffectMat.SetVector("_Particle", new Vector4(2f, 30f, 5f, 0.8f));

            //_Circle: Aperture Control x: Zoom y: Height z: Speed w: Ellipse Deformation
            //      Values on Ray Receiver = (1.49, 10, 1, 0.01)
            collectEffectMat.SetVector("_Circle", new Vector4(2.5f, 34f, 1f, 0.04f));

            newMats.Add(collectEffectMat);
            ProtoRegistry.RegisterModel(512, "Assets/genesis-models/entities/prefabs/atmospheric-collect-station", newMats.ToArray());
        }

        private static ModelProto CopyModelProto(int oriId, int id, Color color)
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
            }

            try
            {
                model.prefabDesc.lodMaterials[0][0].color = color;
                model.prefabDesc.lodMaterials[1][0].color = color;
                model.prefabDesc.lodMaterials[2][0].color = color;
            }
            catch
            {
                // ignored
            }

            model.prefabDesc.modelIndex = id;
            model.prefabDesc.hasBuildCollider = true;
            model.prefabDesc.colliders = desc.colliders;
            model.prefabDesc.buildCollider = desc.buildCollider;
            model.prefabDesc.buildColliders = desc.buildColliders;
            model.prefabDesc.colliderPrefab = desc.colliderPrefab;

            model.sid = "";
            model.SID = "";
            return model;
        }
    }
}
