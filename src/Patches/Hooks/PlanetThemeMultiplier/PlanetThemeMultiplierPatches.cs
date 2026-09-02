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

        // ==================== 主题副产物（大气采集站附带产出） ====================

        /// <summary>
        /// 主题副产物表（主题 ID → 副产物物品 ID + 附带比例 主:副）：
        /// 大气采集站每采集 ratio 单位主产物，附带产出 1 单位主题特色物品。
        /// </summary>
        private static readonly Dictionary<int, int[]> ThemeByproducts = new Dictionary<int, int[]>
        {
            { 9, new[] { 1112, 10 } },   // 熔岩：金刚石（碳尘高压结晶）
            { 10, new[] { 1113, 10 } },  // 冰原冻土：晶格硅（低温硅晶）
            { 16, new[] { 1121, 10 } },  // 水世界：重氢（海水提氘）
            { 17, new[] { 1108, 12 } },  // 黑石盐滩：石材（盐岩结晶）
            { 23, new[] { 1105, 10 } },  // 橙晶荒漠：高纯硅块（沙尘硅）
            { 25, new[] { 1117, 8 } },   // 潘多拉沼泽：有机晶体（孢子富集）
        };

        /// <summary>获取行星主题副产物（无副产物返回 false）</summary>
        internal static bool TryGetByproduct(PlanetData planet, out int itemId, out int ratio)
        {
            itemId = 0;
            ratio = 0;

            if (planet == null || planet.theme <= 0) return false;

            if (!ThemeByproducts.TryGetValue(planet.theme, out int[] data)) return false;

            itemId = data[0];
            ratio = data[1];

            return true;
        }

        // ==================== 工厂速度挂钩（注入统一 InternalUpdate pre-patch） ====================

        /// <summary>是否冶炼类配方（Type 1/11/13/19）</summary>
        private static bool IsSmeltType(ERecipeType type) =>
            type == ERecipeType.Smelt || (int)type == (int)Utils_ERecipeType.标准冶炼 ||
            (int)type == (int)Utils_ERecipeType.高热冶炼 || (int)type == (int)Utils_ERecipeType.所有熔炉;

        /// <summary>是否化工类配方（Type 2/16/17）</summary>
        private static bool IsChemicalType(ERecipeType type) =>
            type == ERecipeType.Chemical || (int)type == (int)Utils_ERecipeType.高分子化工 ||
            (int)type == (int)Utils_ERecipeType.所有化工;

        /// <summary>
        /// 主题速度挂钩：每 tick 在 InternalUpdate 前按行星主题重算 component.speed（幂等）。
        /// 速度 = prefabDesc 基础速度 × 温度因子（冶炼/化工）× 有机因子（生物行星有机配方）。
        /// 巨构（speed ≥ 300000）不受行星主题影响。
        /// </summary>
        public static void GameTick_AssemblerComponent_InternalUpdate_Patch(PlanetFactory factory, ref AssemblerComponent component,
            float power)
        {
            // 巨构保留自身速度，不受主题影响
            if (component.speed >= MegaAssemblerPatches.MegaAssemblerSpeed) return;

            if (power < 0.1f) return;

            PlanetData planet = factory.planet;

            if (planet == null) return;

            // 基础速度（从 prefabDesc 重算，保证幂等不叠加）
            ItemProto itemProto = LDB.items.Select(factory.entityPool[component.entityId].protoId);

            if (itemProto?.prefabDesc == null) return;

            int baseSpeed = itemProto.prefabDesc.assemblerSpeed;

            float factor = 1f;

            // 温度因子：高温利于冶炼、低温利于化工
            if (IsSmeltType(component.recipeType)) factor *= GetTemperatureSpeedFactor(planet, true);
            else if (IsChemicalType(component.recipeType)) factor *= GetTemperatureSpeedFactor(planet, false);

            // 有机因子：生物行星的有机类配方加成
            if (IsOrganicRecipe(component.recipeId)) factor *= GetMultiplier(planet, MultiplierType.OrganicSpeed);

            component.speed = (int)(baseSpeed * factor);

            // 增产效力：主题倍率 >1 时提高输入增产剂累计（等效增产剂更耐用、效力更高）
            // 设上限 255 防止每 tick 乘算的指数膨胀（长期趋向满级增产，符合"更耐用"设计）
            float incPower = GetMultiplier(planet, MultiplierType.IncPower);

            if (incPower > 1f && component.incServed != null)
            {
                for (int i = 0; i < component.incServed.Length; i++)
                {
                    if (component.incServed[i] > 0)
                        component.incServed[i] = Math.Min((int)(component.incServed[i] * incPower), 255);
                }
            }
        }

        // ==================== 主题副产物槽（大气采集站） ====================

        /// <summary>
        /// 大气采集站副产物槽：在 station 的 collectionIds/storage 尾部追加副产物槽，
        /// 采集速度 0（由附带逻辑按比例填充），物流无人机可正常取走。
        /// </summary>
        [HarmonyPatch(typeof(PlanetTransport), nameof(PlanetTransport.NewStationComponent))]
        [HarmonyPostfix]
        public static void NewStationComponent_Postfix(ref StationComponent __result, PrefabDesc _desc, PlanetFactory factory)
        {
            if (!_desc.isCollectStation) return;

            if (!TryGetByproduct(factory.planet, out int itemId, out int ratio)) return;

            int oldLen = __result.collectionIds.Length;

            // 扩展 collectionIds / collectionPerTick / currentCollections（副产槽速度为 0，由附带逻辑填充）
            Array.Resize(ref __result.collectionIds, oldLen + 1);
            __result.collectionIds[oldLen] = itemId;
            Array.Resize(ref __result.collectionPerTick, oldLen + 1);
            __result.collectionPerTick[oldLen] = 0f;
            Array.Resize(ref __result.currentCollections, oldLen + 1);
            __result.currentCollections[oldLen] = 0f;

            // storage 扩展（Init 已按旧长度建槽，富余则复用空闲槽）
            if (__result.storage.Length <= oldLen)
            {
                Array.Resize(ref __result.storage, oldLen + 1);
                Array.Resize(ref __result.priorityLocks, oldLen + 1);
            }

            __result.storage[oldLen].itemId = itemId;
            __result.storage[oldLen].count = 0;
            __result.storage[oldLen].inc = 0;
            __result.storage[oldLen].remoteLogic = ELogisticStorage.Supply;
            __result.storage[oldLen].max = __result.storage[0].max;
            __result.storage[oldLen].keepMode = 0;
            __result.storage[oldLen].keepIncRatio = 0f;
        }

        /// <summary>
        /// 附带产出：主产物每采集 ratio 单位，向副产物槽填充 1 单位主题特色物品。
        /// </summary>
        public static void ByproductFill(StationComponent station, PlanetFactory factory, int itemId, int amount, int[] productRegister)
        {
            if (amount <= 0) return;

            if (!TryGetByproduct(factory.planet, out int byproductId, out int ratio)) return;

            // 定位副产槽
            int index = -1;

            for (int i = 0; i < station.collectionIds.Length; i++)
            {
                if (station.collectionIds[i] == byproductId)
                {
                    index = i;
                    break;
                }
            }

            if (index < 0 || index >= station.storage.Length) return;

            int add = amount / ratio;

            if (add <= 0) return;

            lock (station.storage)
            {
                station.storage[index].count += add;

                if (productRegister != null)
                    lock (productRegister)
                    {
                        productRegister[byproductId] += add;
                    }
            }
        }
    }
}
