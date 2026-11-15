using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using ProjectGenesis.Utils;
using UnityEngine;
using Utils_ERecipeType = ProjectGenesis.Utils.ERecipeType;

// ReSharper disable InconsistentNaming
// ReSharper disable MemberCanBeInternal

namespace ProjectGenesis.Patches
{
    /// <summary>
    /// 行星主题特色：
    /// 1. 温度机制（通用，不查主题表）：高温行星冶炼快化工慢，低温反之
    /// 2. 主题倍率表：抽水/大气采集/火电/风电/有机化工/增产效力（按 ThemeProto 细化）
    /// 3. 挂钩全部挂到现有代码（PowerSystem.GameTick 与 PlanetFocus 同位置）
    /// </summary>
    public static class PlanetThemeMultiplierPatches
    {
        /// <summary>倍率类型</summary>
        internal enum MultiplierType
        {
            PumpSpeed,     // 抽水速度
            CollectSpeed,  // 大气采集速度
            ThermalPower,  // 火电效率
            WindPower,     // 风电效率
            OrganicSpeed,  // 有机化工速度
            IncPower,      // 增产剂效力
        }

        /// <summary>主题排他配方：配方 ID → 可用主题 ID（其他星球停摆）</summary>
        private static readonly Dictionary<int, int[]> ExclusiveRecipeThemes = new Dictionary<int, int[]>
        {
            { 905, new[] { 16 } }, // 海水提氘：水世界
        };

        /// <summary>
        /// 主题倍率表（主题 ID → 六维倍率：抽水/大气采集/火电/风电/有机化工/增产效力）。
        /// 每个主题有突出专长与明显短板，形成选址取舍。
        /// </summary>
        private static readonly Dictionary<int, float[]> ThemeMultipliers = new Dictionary<int, float[]>
        {
            // 1 地中海：均衡基准
            { 1, new[] { 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f } },
            // 6 干旱荒漠：无氧难发电，采集艰难
            { 6, new[] { 1.0f, 0.5f, 0.8f, 1.0f, 1.0f, 1.0f } },
            // 7 灰烬冻土：硫大气采集加成，火电乏力
            { 7, new[] { 1.0f, 1.1f, 0.8f, 1.0f, 1.0f, 1.0f } },
            // 8 海洋丛林：海洋+含氧+生物富饶
            { 8, new[] { 1.5f, 1.0f, 1.2f, 1.0f, 1.5f, 1.2f } },
            // 9 熔岩：火电受限，采集难
            { 9, new[] { 0.5f, 0.5f, 0.8f, 0.8f, 1.0f, 1.0f } },
            // 10 冰原冻土：低温但火电正常
            { 10, new[] { 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f } },
            // 11 贫瘠荒漠：无大气，无火电
            { 11, new[] { 1.0f, 0.0f, 0.5f, 1.0f, 1.0f, 1.0f } },
            // 12 戈壁
            { 12, new[] { 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f } },
            // 13 火山灰：硫烟弥漫采集受阻
            { 13, new[] { 0.8f, 0.7f, 0.9f, 0.9f, 1.0f, 1.0f } },
            // 14 红石：富氧大气火电充沛
            { 14, new[] { 1.0f, 1.0f, 1.3f, 1.0f, 1.0f, 1.0f } },
            // 15 草原：宜居全能
            { 15, new[] { 1.0f, 1.0f, 1.15f, 1.0f, 1.0f, 1.0f } },
            // 16 水世界：抽水星球，无地可用
            { 16, new[] { 3.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f } },
            // 17 黑石盐滩：无海洋抽水困难，化工加成（温度机制叠加）
            { 17, new[] { 0.5f, 1.0f, 0.9f, 1.0f, 1.0f, 1.0f } },
            // 18 樱林海：海洋+生物
            { 18, new[] { 1.3f, 1.0f, 1.1f, 1.0f, 1.3f, 1.2f } },
            // 19 飓风石林：狂风大气采集/风电王者
            { 19, new[] { 0.8f, 3.0f, 0.9f, 2.0f, 1.0f, 1.0f } },
            // 20 猩红冰湖
            { 20, new[] { 1.1f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f } },
            // 22 热带草原：海洋+含氧
            { 22, new[] { 1.2f, 1.0f, 1.1f, 1.0f, 1.0f, 1.0f } },
            // 23 橙晶荒漠：风沙遮蔽大气
            { 23, new[] { 0.9f, 0.7f, 0.9f, 1.0f, 1.0f, 1.0f } },
            // 24 极寒冻土：化工圣地（温度机制叠加）
            { 24, new[] { 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f } },
            // 25 潘多拉沼泽：生物工厂，有机+增产冠绝全星系
            { 25, new[] { 1.3f, 1.0f, 1.1f, 1.0f, 3.0f, 1.5f } },
        };

        /// <summary>取行星主题倍率（未收录主题默认 1）</summary>
        internal static float GetMultiplier(PlanetData planet, MultiplierType type)
        {
            if (planet == null || planet.theme <= 0) return 1f;

            if (!ThemeMultipliers.TryGetValue(planet.theme, out float[] multipliers)) return 1f;

            return multipliers[(int)type];
        }

        // ==================== 主题排他配方门禁（注入统一 InternalUpdate pre-patch） ====================

        /// <summary>
        /// 工厂速度不再随主题变化（战略版设计：工厂全宇宙统一，主题只影响环境/资源/排他工艺）。
        /// 此处只做主题排他配方门禁。
        /// </summary>
        public static void GameTick_AssemblerComponent_InternalUpdate_Patch(PlanetFactory factory, ref AssemblerComponent component,
            float power)
        {
            if (ExclusiveRecipeThemes.TryGetValue(component.recipeId, out int[] themes) &&
                (factory.planet == null || Array.IndexOf(themes, factory.planet.theme) < 0))
            {
                component.speed = 0;
                return;
            }
        }

    }
}
