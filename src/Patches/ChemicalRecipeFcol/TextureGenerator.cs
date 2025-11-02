using System.Collections.Generic;
using System.IO;
using System.Linq;
using ProjectGenesis.Utils;
using UnityEngine;

namespace ProjectGenesis.Patches
{
    public static partial class ChemicalRecipeFcolPatches
    {
        private static readonly Dictionary<int, int> RecipeIdPos = new Dictionary<int, int>
        {
            { ProtoID.R二氧化碳, 431 },
            { ProtoID.R催化重整, 432 },
            { ProtoID.R四氢双环戊二烯, 433 },
            { ProtoID.R有机晶体重组, 434 },
            { ProtoID.R水电解, 435 },
            { ProtoID.R盐水电解, 436 },
            { ProtoID.R合成氨, 437 },
            { ProtoID.R三氯化铁, 438 },
            { ProtoID.R氨氧化, 439 },
            { ProtoID.R甲烷偶联, 440 },
            { ProtoID.R聚苯硫醚, 441 },
            { ProtoID.R聚酰亚胺, 442 },
            { ProtoID.R钨矿筛选, 443 },
            { ProtoID.R海水淡化, 444 },
            { ProtoID.R有机晶体活化, 445 },
            { ProtoID.R二氧化硫还原, 446 },
            { ProtoID.R增产剂, 447 },
            { ProtoID.R氦原子提取, 448 },
            { ProtoID.R硅石筛选, 449 },
            { ProtoID.R甲烷, 450 },
            { ProtoID.R放射性矿物处理, 451 },
        };

        private static readonly Dictionary<int, Color32> ItemColors = new Dictionary<int, Color32>
        {
            { 1117, new Color32(153, 163, 87, 255) },
            { 7101, new Color32(244, 244, 244, 255) },
            { 1114, new Color32(244, 244, 244, 255) },
            { 7020, new Color32(58, 97, 164, 255) },
            { 1014, new Color32(229, 153, 255, 255) },
            { 1124, new Color32(121, 110, 114, 255) },
            { 1123, new Color32(150, 161, 174, 255) },
            { 6201, new Color32(54, 54, 54, 255) },
            { 7708, new Color32(210, 190, 144, 255) },
            { 7709, new Color32(21, 21, 21, 255) },
            { 1000, new Color32(165, 198, 208, 255) },
            { 1003, new Color32(55, 179, 114, 255) },
            { 1004, new Color32(195, 220, 250, 255) },
            { 1005, new Color32(137, 145, 165, 255) },
            { 1143, new Color32(21, 175, 251, 255) },
        };

        private static readonly Dictionary<int, int[][]> InputIndexMap = new Dictionary<int, int[][]>
        {
            { 1, new[] { new[] { 8, 7, 5, 4, 3, }, } }, // 
            { 2, new[] { new[] { 8, 5, 4, }, new[] { 7, 3, }, } },
            { 3, new[] { new[] { 8, 5, }, new[] { 7, 4, }, new[] { 3, }, } },
        };

        private static Color32 GetItemColor(int itemId)
        {
            if (ItemColors.TryGetValue(itemId, out var color)) return color;

            return IconDescUtils.IconDescs.TryGetValue(itemId, out var t) ? t.Color : IconDescUtils.ExportIconDesc(itemId).faceColor;
        }

        private static void SetDefaultColors(Texture2D texture, int column)
        {
            Color defaultColor = new Color32(165, 198, 208, 255);
            Color[] colors = Enumerable.Repeat(defaultColor, 16).ToArray();
            texture.SetPixels(column, 0, 1, 16, colors);
        }

        private static void ApplyColors(Texture2D texture, int column, Color32 color, int[] positions)
        {
            foreach (int pos in positions) texture.SetPixel(column, pos, color);
        }

        private static void ApplyRecipeColors(Texture2D texture, int column, RecipeProto proto)
        {
            ApplyColors(texture, column, GetItemColor(proto.Results[0]), new[]
            {
                9, 6, 2, 1,
                0,
            });

            if (!InputIndexMap.TryGetValue(proto.Items.Length, out int[][] mappings)) return;

            for (int i = 0; i < mappings.Length; i++) ApplyColors(texture, column, GetItemColor(proto.Items[i]), mappings[i]);
        }

        internal static void GenerateRecipeTexture(string outputFilePath)
        {
            int[] recipes = {};

            int[] recipePos = recipes.Select(i => RecipeIdPos[i]).ToArray();

            Texture2D texture = TextureHelper.GetTexture("化工厂渲染索引");

            for (var index = 0; index < recipes.Length; index++)
            {
                int pos = recipePos[index];
                SetDefaultColors(texture, pos);

                RecipeProto proto = LDB.recipes.Select(recipes[index]);

                if (proto == null) continue;

                if (proto.Type == ERecipeType.Chemical || proto.Type == (ERecipeType)16) ApplyRecipeColors(texture, pos, proto);
            }

            texture.Apply();

            File.WriteAllBytes(outputFilePath, texture.EncodeToPNG());
        }
    }
}
