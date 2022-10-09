using UnityEngine;
using xiaoye97;

#pragma warning disable CS0618

namespace ProjectGenesis.Utils
{
    internal static class CopyModelUtils
    {
        internal static void AddCopiedModelProto()
        {
            var tankModel = CopyModelProto(121, 401, Color.HSVToRGB(0.5571f, 0.3188f, 0.8980f));
            LDBTool.PreAddProto(tankModel);
            var oreFactoryModel = CopyModelProto(194, 402, Color.HSVToRGB(0.2035f, 0.8326f, 0.9373f));
            LDBTool.PreAddProto(oreFactoryModel);
            var testCraftingTableModel = CopyModelProto(49, 403, Color.HSVToRGB(0.0710f, 0.7412f, 0.8941f));
            LDBTool.PreAddProto(testCraftingTableModel);
            var testCraftingTableModel2 = CopyModelProto(49, 404, Color.HSVToRGB(0.6174f, 0.6842f, 0.9686f));
            LDBTool.PreAddProto(testCraftingTableModel2);
            var testCraftingTableModel3 = CopyModelProto(49, 405, Color.HSVToRGB(0.1404f, 0.8294f, 0.9882f));
            LDBTool.PreAddProto(testCraftingTableModel3);
            var testCraftingTableModel4 = CopyModelProto(49, 406, Color.HSVToRGB(0.9814f, 0.6620f, 0.8471f));
            LDBTool.PreAddProto(testCraftingTableModel4);
            var testCraftingTableModel5 = CopyModelProto(49, 410, new Color(0.3216F, 0.8157F, 0.09020F));
            LDBTool.PreAddProto(testCraftingTableModel5);
            var testCraftingTableModel6 = CopyModelProto(49, 411, new Color(0.3059F,0.2196F,0.4941F));
            LDBTool.PreAddProto(testCraftingTableModel6);
            var antiMatterModel = CopyModelProto(118, 407, Color.HSVToRGB(0.5985f, 0.7333f, 0.2353f));
            LDBTool.PreAddProto(antiMatterModel);
            var assembleModel = CopyModelProto(67, 408, Color.HSVToRGB(0.9688f, 0.9068f, 0.9255f));
            LDBTool.PreAddProto(assembleModel);
            var circleModel = CopyModelProto(69, 409, Color.grey);
            LDBTool.PreAddProto(circleModel);
        }

        private static ModelProto CopyModelProto(int oriId, int id, Color color)
        {
            var oriModel = LDB.models.Select(oriId);
            var model = oriModel.Copy();
            model.Name = id.ToString(); //这俩至少有一个必须加，否则LDBTool报冲突导致后面null
            model.ID = id;
            var desc = oriModel.prefabDesc;
            model.prefabDesc = new PrefabDesc(id, desc.prefab, desc.colliderPrefab);
            for (var i = 0; i < model.prefabDesc.lodMaterials.Length; i++)
            {
                if (model.prefabDesc.lodMaterials[i] == null) continue;
                for (var j = 0; j < model.prefabDesc.lodMaterials[i].Length; j++)
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
