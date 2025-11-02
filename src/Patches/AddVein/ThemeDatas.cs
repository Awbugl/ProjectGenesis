using System;
using System.Collections.Generic;
using ProjectGenesis.Utils;

namespace ProjectGenesis.Patches
{
    internal static partial class ModifyPlanetTheme
    {
        internal static readonly Dictionary<int, ThemeData> ThemeDatas = new Dictionary<int, ThemeData>
        {
            {
                1, // 地中海
                new ThemeData(new[] { ProtoID.I氮, ProtoID.I氧, }, new[] { 0.7f, 0.3f, }, new[] { 8, 16, 18, }, new[]
                {
                    1.0f, 1.0f, 0.0f, 0.4f, //
                    1.0f, 1.0f, 0.0f, 1.0f, //
                    1.0f, 1.0f, 1.0f, 0.4f, //
                })
            },
            {
                6, // 干旱荒漠
                new ThemeData(new[] { ProtoID.I二氧化碳, }, new[] { 0.8f, }, new[] { 16, 18, 19, }, new[]
                {
                    0.0f, 0.7f, 0.2f, 0.8f, //
                    0.0f, 1.0f, 0.6f, 0.5f, //
                    0.0f, 0.8f, 0.6f, 0.4f, //
                })
            },
            {
                7, // 灰烬冻土
                new ThemeData(new[] { ProtoID.I氮, ProtoID.I二氧化硫, }, new[] { 0.5f, 0.5f, }, new[] { 16, 18, 20, }, new[]
                {
                    0.0f, 0.7f, 0.2f, 0.8f, //
                    0.0f, 1.0f, 0.6f, 0.5f, //
                    0.0f, 0.4f, 0.5f, 0.4f, //
                })
            },
            {
                8, // 海洋丛林
                new ThemeData(new[] { ProtoID.I氮, ProtoID.I氧, }, new[] { 0.6f, 0.4f, }, new[] { 18, }, new[]
                {
                    0.0f, 0.8f, 0.6f, 0.9f, //
                })
            },
            {
                9, // 熔岩
                new ThemeData(new[] { ProtoID.I二氧化硫, ProtoID.I二氧化碳, }, new[] { 0.7f, 0.2f, }, new[] { 16, 17, 18, }, new[]
                {
                    0.0f, 1.0f, 0.3f, 0.7f, //
                    0.0f, 0.6f, 0.3f, 0.8f, //
                    0.0f, 0.8f, 0.5f, 0.8f, //
                })
            },
            {
                10, // 冰原冻土
                new ThemeData(new[] { ProtoID.I氮, ProtoID.I二氧化碳, }, new[] { 0.7f, 0.3f, }, new[] { 16, 17, 20, }, new[]
                {
                    0.0f, 1.0f, 0.7f, 0.7f, //
                    0.0f, 0.6f, 0.3f, 0.8f, //
                    0.0f, 0.4f, 0.6f, 0.4f, //
                })
            },
            {
                11, // 贫瘠荒漠
                new ThemeData(Array.Empty<int>(), Array.Empty<float>(),  new[] { 20, }, new[]
                {
                    0.0f, 0.6f, 0.6f, 0.7f, //
                })
            },
            {
                12, // 戈壁
                new ThemeData(new[] { ProtoID.I二氧化碳, }, new[] { 1f, }, new[] { 16, 17, }, new[]
                {
                    0.0f, 1.0f, 0.7f, 0.7f, //
                    0.0f, 0.6f, 0.3f, 0.8f, //
                })
            },
            {
                13, // 火山灰
                new ThemeData(new[] { ProtoID.I二氧化硫, ProtoID.I二氧化碳, }, new[] { 0.7f, 0.4f, }, new[] { 17, 18, }, new[]
                {
                    0.0f, 0.6f, 0.3f, 0.8f, //
                    0.0f, 1.0f, 0.5f, 0.8f, //
                })
            },
            {
                14, // 红石
                new ThemeData(new[] { ProtoID.I氮, ProtoID.I氧, }, new[] { 0.5f, 0.5f, }, new[] { 18, }, new[]
                {
                    0.0f, 0.8f, 0.6f, 0.9f, //
                })
            },
            {
                15, // 草原
                new ThemeData(new[] { ProtoID.I氮, ProtoID.I氧, }, new[] { 0.7f, 0.2f, }, new[] { 18, }, new[]
                {
                    0.0f, 0.8f, 0.6f, 0.9f, //
                })
            },
            {
                16, // 水世界
                new ThemeData(new[] { ProtoID.I氮, ProtoID.I氧, }, new[] { 0.5f, 0.5f, }, new[] { 18, }, new[]
                {
                    0.0f, 1.0f, 0.9f, 0.9f, //
                })
            },
            {
                17, // 黑石盐滩
                new ThemeData(new[] { ProtoID.I氮, }, new[] { 1f, }, new[] { 16, 19, }, new[]
                {
                    0.0f, 1.0f, 0.7f, 0.9f, //
                    0.0f, 1.0f, 0.7f, 0.9f, //
                })
            },
            {
                18, // 樱林海
                new ThemeData(new[] { ProtoID.I氮, ProtoID.I氧, }, new[] { 0.7f, 0.3f, }, Array.Empty<int>(), Array.Empty<float>())
            },
            {
                19, // 飓风石林
                new ThemeData(new[] { ProtoID.I二氧化硫, }, new[] { 0.8f, }, new[] { 18, 19, }, new[]
                {
                    0.0f, 1.0f, 1.0f, 0.7f, //
                    0.0f, 0.4f, 0.7f, 0.4f, //
                })
            },
            {
                20, // 猩红冰湖
                new ThemeData(new[] { ProtoID.I氮, }, new[] { 0.8f, }, new[] { 18, 20, }, new[]
                {
                    0.0f, 0.7f, 0.5f, 0.9f, //
                    0.0f, 1.0f, 0.7f, 0.9f, //
                })
            },
            {
                22, // 热带草原
                new ThemeData(new[] { ProtoID.I氮, ProtoID.I氧, }, new[] { 0.7f, 0.2f, }, Array.Empty<int>(), Array.Empty<float>())
            },
            {
                23, // 橙晶荒漠
                new ThemeData(new[] { ProtoID.I二氧化碳, }, new[] { 0.8f, }, new[] { 17, 18, 19, }, new[]
                {
                    0.0f, 0.6f, 0.5f, 0.8f, //
                    0.0f, 1.0f, 0.7f, 0.8f, //
                    0.0f, 0.5f, 0.4f, 0.6f, //
                })
            },
            {
                24, // 极寒冻土
                new ThemeData(new[] { ProtoID.I氮, }, new[] { 0.8f, }, Array.Empty<int>(), Array.Empty<float>())
            },
            {
                25, // 潘多拉沼泽
                new ThemeData(new[] { ProtoID.I氮, ProtoID.I氧, }, new[] { 0.7f, 0.2f, }, Array.Empty<int>(), Array.Empty<float>())
            },
        };

        public struct ThemeData
        {
            public readonly int[] GasItems;
            public readonly float[] GasSpeedFactors;
            public readonly int[] RareVeins;
            public readonly float[] RareSettings;

            public ThemeData(int[] gasItems, float[] gasSpeedFactors, int[] rareVeins, float[] rareSettings)
            {
                GasItems = gasItems;
                GasSpeedFactors = gasSpeedFactors;
                RareVeins = rareVeins;
                RareSettings = rareSettings;
            }
        }
    }
}
