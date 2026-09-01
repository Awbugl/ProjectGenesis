using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using ProjectGenesis.Utils;
using UnityEngine;

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

        /// <summary>有机类配方（生物行星特色加成对象）</summary>
        private static readonly HashSet<int> OrganicRecipes = new HashSet<int>
        {
            108,  // 增产剂
            509,  // 催化重整（苯）
            546,  // 有机晶体
            549,  // 有机晶体活化（聚酰亚胺）
            550,  // 有机晶体重组
            714,  // 钛晶石
            709,  // 钛晶石（高效）
            771,  // 聚苯硫醚
            772,  // 聚酰亚胺
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

        /// <summary>取行星温度（主题温度 + 行星偏移）</summary>
        internal static float GetTemperature(PlanetData planet)
        {
            if (planet == null || planet.theme <= 0) return 0f;

            ThemeProto theme = LDB.themes.Select(planet.theme);

            return theme == null ? 0f : theme.Temperature + planet.temperatureBias;
        }

        /// <summary>
        /// 温度速度因子：高温利于冶炼（熔炼热自足）、低温利于化工（放热反应易散热）。
        /// 基准 0°C 为 1，每 ±50°C 变化 ±25%（clamp 0.5~1.5）。
        /// </summary>
        internal static float GetTemperatureSpeedFactor(PlanetData planet, bool smelt)
        {
            float t = GetTemperature(planet);
            float factor = smelt ? 1f + t / 200f : 1f - t / 200f;

            return Mathf.Clamp(factor, 0.5f, 1.5f);
        }

        /// <summary>是否有机类配方</summary>
        internal static bool IsOrganicRecipe(int recipeId) => OrganicRecipes.Contains(recipeId);

        // ==================== 火电/风电挂钩（与 PlanetFocus.EnergyCap_Transpiler 同位置） ====================

        /// <summary>
        /// 发电容量挂钩：风电 × 主题风电倍率、火电 × 主题火电倍率。
        /// 与 PlanetFocus 的 EnergyCap_Transpiler 挂同一方法，链式执行互不干扰。
        /// </summary>
        [HarmonyPatch(typeof(PowerSystem), nameof(PowerSystem.GameTick))]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> PowerSystem_GameTick_ThemeTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            CodeMatcher matcher = new CodeMatcher(instructions);

            // 风电：EnergyCap_Wind 调用后插入 ThemeEnergyCap_Wind(power, powerSystem, ref component)
            matcher.MatchForward(false,
                new CodeMatch(OpCodes.Call,
                    AccessTools.Method(typeof(PowerGeneratorComponent), nameof(PowerGeneratorComponent.EnergyCap_Wind))));

            CodeInstruction comp = matcher.InstructionAt(-2);

            matcher.Advance(1).InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_0)).InsertAndAdvance(comp)
               .InsertAndAdvance(new CodeInstruction(OpCodes.Call,
                    AccessTools.Method(typeof(PlanetThemeMultiplierPatches), nameof(ThemeEnergyCap_Wind))));

            // 火电：EnergyCap_Fuel 调用后插入 ThemeEnergyCap_Fuel(power, powerSystem, ref component)
            matcher.MatchForward(false,
                new CodeMatch(OpCodes.Call,
                    AccessTools.Method(typeof(PowerGeneratorComponent), nameof(PowerGeneratorComponent.EnergyCap_Fuel))));

            comp = matcher.InstructionAt(-1);

            matcher.Advance(1).InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_0)).InsertAndAdvance(comp)
               .InsertAndAdvance(new CodeInstruction(OpCodes.Call,
                    AccessTools.Method(typeof(PlanetThemeMultiplierPatches), nameof(ThemeEnergyCap_Fuel))));

            return matcher.InstructionEnumeration();
        }

        /// <summary>风电容量 × 主题倍率</summary>
        public static long ThemeEnergyCap_Wind(long power, PowerSystem powerSystem, ref PowerGeneratorComponent component)
        {
            if (!component.wind) return power;

            float m = GetMultiplier(powerSystem.factory.planet, MultiplierType.WindPower);

            return m <= 0f ? 0L : (long)(power * m);
        }

        /// <summary>火电容量 × 主题倍率（仅燃料掩码 1 的火电）</summary>
        public static long ThemeEnergyCap_Fuel(long power, PowerSystem powerSystem, ref PowerGeneratorComponent component)
        {
            if (component.fuelMask != 1) return power;

            float m = GetMultiplier(powerSystem.factory.planet, MultiplierType.ThermalPower);

            return m <= 0f ? 0L : (long)(power * m);
        }
    }
}
