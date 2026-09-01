using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Emit;
using HarmonyLib;
using ProjectGenesis.Utils;
using UnityEngine;

// ReSharper disable InconsistentNaming
// ReSharper disable MemberCanBeInternal

namespace ProjectGenesis.Patches
{
    /// <summary>
    /// 行星大气/海洋池化：
    /// 每颗行星拥有有限的海洋储量与大气储量（照原油"有限矿脉"模式），
    /// 采集会消耗储量、储量越低采集越慢、耗尽停产；火电等燃烧会把排放物注入大气池。
    /// - 海洋储量与地表面积相关：waterAmount = 4π·R² × 海洋系数
    /// - 大气储量与风速绑定（正比）：gasAmounts = gasSpeeds × 大气系数
    /// </summary>
    public static class PlanetAtmospherePatches
    {
        /// <summary>大气池固定气体（按物品 ID，含排放目标 水/氢）</summary>
        internal static readonly int[] GasItemIds =
        {
            ProtoID.I水, ProtoID.I氢, ProtoID.I重氢, ProtoID.I氮, ProtoID.I氧,
            ProtoID.I二氧化碳, ProtoID.I二氧化硫, ProtoID.I甲烷, ProtoID.I氨, ProtoID.I氦,
        };

        /// <summary>停产底线（初始储量的比例，参照原油保留 2500 底线的思路）</summary>
        internal const float FloorRatio = 0.05f;

        /// <summary>海洋储量系数（单位：每单位表面积的水量）</summary>
        internal const float WaterAreaFactor = 100f;

        /// <summary>大气储量系数（单位：每单位采集速度的储量）</summary>
        internal const float GasSpeedFactor = 10000000f;

        private static readonly Dictionary<int, float[]> PlanetGasAmounts = new Dictionary<int, float[]>();
        private static readonly Dictionary<int, float> PlanetWaterAmounts = new Dictionary<int, float>();

        // ==================== 数据访问 ====================

        /// <summary>获取行星海洋池（无数据则按主题初始化）</summary>
        internal static float GetWaterPool(PlanetData planet)
        {
            if (!PlanetWaterAmounts.TryGetValue(planet.id, out float value)) value = InitPlanet(planet);

            return value;
        }

        /// <summary>获取行星大气池（按 GasItemIds 对齐）</summary>
        internal static float[] GetGasPool(PlanetData planet)
        {
            if (!PlanetGasAmounts.TryGetValue(planet.id, out float[] value)) InitPlanet(planet);

            return PlanetGasAmounts[planet.id];
        }

        /// <summary>获取初始海洋储量（不初始化，仅查询）</summary>
        internal static float GetInitialWater(PlanetData planet) => 4f * Mathf.PI * planet.radius * planet.radius * WaterAreaFactor;

        /// <summary>获取初始大气储量（按 gasSpeeds 正比于风速）</summary>
        internal static float[] GetInitialGas(PlanetData planet)
        {
            var result = new float[GasItemIds.Length];

            for (int i = 0; i < planet.gasItems.Length && i < planet.gasSpeeds.Length; i++)
            {
                int index = Array.IndexOf(GasItemIds, planet.gasItems[i]);

                if (index >= 0) result[index] = planet.gasSpeeds[i] * GasSpeedFactor;
            }

            return result;
        }

        /// <summary>按主题初始化行星池数据（海洋∝地表面积，大气∝风速）</summary>
        private static float InitPlanet(PlanetData planet)
        {
            PlanetGasAmounts[planet.id] = GetInitialGas(planet);
            float water = GetInitialWater(planet);
            PlanetWaterAmounts[planet.id] = water;

            return water;
        }

        /// <summary>消耗海洋池</summary>
        internal static void ConsumeWater(int planetId, int amount)
        {
            if (!PlanetWaterAmounts.TryGetValue(planetId, out float value)) return;

            PlanetWaterAmounts[planetId] = Math.Max(0f, value - amount);
        }

        /// <summary>消耗/注入大气池（amount 为正扣池，为负注池）</summary>
        internal static void ModifyGas(int planetId, int gasIndex, int amount)
        {
            if (gasIndex < 0 || gasIndex >= GasItemIds.Length) return;

            if (!PlanetGasAmounts.TryGetValue(planetId, out float[] value)) return;

            value[gasIndex] = Math.Max(0f, value[gasIndex] + amount);
        }

        /// <summary>按物品 ID 扣大气池（大气采集站产出路径）</summary>
        internal static void ConsumeGas(PlanetFactory factory, int itemId, int amount)
        {
            int index = Array.IndexOf(GasItemIds, itemId);

            if (index < 0) return;

            ModifyGas(factory.planetId, index, amount);
        }

        /// <summary>当前池/初始池 的剩余比例（0-1），池未初始化视为 1</summary>
        internal static float GetPoolRatio(PlanetData planet, float current, float initial)
        {
            if (initial <= 0f) return 1f;

            return Mathf.Clamp01(current / initial);
        }

        // ==================== 存档（全局） ====================

        internal static void Export(BinaryWriter w)
        {
            w.Write(PlanetWaterAmounts.Count);

            foreach (KeyValuePair<int, float> pair in PlanetWaterAmounts)
            {
                w.Write(pair.Key);
                w.Write(pair.Value);
                w.Write(PlanetGasAmounts.TryGetValue(pair.Key, out float[] gas) ? gas.Length : 0);

                if (PlanetGasAmounts.TryGetValue(pair.Key, out float[] gas2))
                    foreach (float v in gas2) w.Write(v);
            }
        }

        internal static void Import(BinaryReader r)
        {
            PlanetWaterAmounts.Clear();
            PlanetGasAmounts.Clear();

            int count = r.ReadInt32();

            for (int i = 0; i < count; i++)
            {
                int planetId = r.ReadInt32();
                float water = r.ReadSingle();
                int gasCount = r.ReadInt32();
                var gas = new float[gasCount];

                for (int j = 0; j < gasCount; j++) gas[j] = r.ReadSingle();

                PlanetWaterAmounts[planetId] = water;
                PlanetGasAmounts[planetId] = gas;
            }
        }

        // ==================== 存档（行星级，联机同步） ====================

        internal static void ExportPlanetData(int planetId, BinaryWriter w)
        {
            w.Write(planetId);
            w.Write(PlanetWaterAmounts.TryGetValue(planetId, out float water) ? water : 0f);
            w.Write(PlanetGasAmounts.TryGetValue(planetId, out float[] gas) ? gas.Length : 0);

            if (PlanetGasAmounts.TryGetValue(planetId, out float[] gas2))
                foreach (float v in gas2) w.Write(v);
        }

        internal static void ImportPlanetData(BinaryReader r)
        {
            int planetId = r.ReadInt32();
            float water = r.ReadSingle();
            int gasCount = r.ReadInt32();
            var gas = new float[gasCount];

            for (int i = 0; i < gasCount; i++) gas[i] = r.ReadSingle();

            PlanetWaterAmounts[planetId] = water;
            PlanetGasAmounts[planetId] = gas;
        }

        // ==================== 抽水机（海洋消耗） ====================

        /// <summary>
        /// 抽水机速度挂钩：采集速度 ∝ 海洋池剩余比例（照原油"越采越慢"），
        /// 低于停产底线则 speed=0（不再累积时间，自然停产）。
        /// 直接改写 MinerComponent.speed 字段，比 transpiler 改动时间累积逻辑更稳。
        /// </summary>
        [HarmonyPatch(typeof(MinerComponent), nameof(MinerComponent.InternalUpdate))]
        [HarmonyPrefix]
        public static void MinerComponent_InternalUpdate_Prefix(ref MinerComponent __instance, PlanetFactory factory)
        {
            // 只处理抽水机（EMinerType.Water）
            if (__instance.type != EMinerType.Water) return;

            PlanetData planet = factory.planet;

            // 地形 check：地基填海/改造地面后，抽水机下方不再有水面 → 停产
            // （QueryHeight 返回地形高度，低于水位 waterHeight 才算水下）
            PlanetRawData rawData = planet.data;

            if (rawData != null)
            {
                Vector3 pos = factory.entityPool[__instance.entityId].pos;

                if (rawData.QueryHeight(pos) >= planet.waterHeight)
                {
                    __instance.speed = 0;
                    return;
                }
            }

            float pool = GetWaterPool(planet);
            float initial = GetInitialWater(planet);
            float ratio = GetPoolRatio(planet, pool, initial);

            // 停产底线：低于初始 5% 时停产
            if (ratio <= FloorRatio) ratio = 0f;

            // 行星主题加成：抽水速度 × 主题倍率（海洋行星快、熔岩行星慢）
            float themeMultiplier = PlanetThemeMultiplierPatches.GetMultiplier(planet,
                PlanetThemeMultiplierPatches.MultiplierType.PumpSpeed);

            __instance.speed = (int)(10000f * ratio * themeMultiplier);
        }

        /// <summary>
        /// 抽水机产出扣池钩子：在 Water 分支 productCount += num14（IL_071a）之后扣海洋池。
        /// 匹配 waterItemId 写入 productId 后第一个 stfld productCount，即抽水机产水记账处。
        /// </summary>
        [HarmonyPatch(typeof(MinerComponent), nameof(MinerComponent.InternalUpdate))]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> MinerComponent_InternalUpdate_Transpiler(
            IEnumerable<CodeInstruction> instructions)
        {
            /*
                目标 IL（Assembly-CSharp.dll / MinerComponent.InternalUpdate Water 分支）：

                    IL_06f6: ldarg.0
                    IL_06f7: ldarg.1
                    IL_06f8: callvirt     PlanetData PlanetFactory::get_planet()
                    IL_06fd: ldfld        int32 PlanetData::waterItemId
                    IL_0702: stfld        int32 MinerComponent::productId      // productId = planet.waterItemId

                    IL_0710: ldarg.0
                    IL_0711: ldarg.0
                    IL_0712: ldfld        int32 MinerComponent::productCount
                    IL_0717: ldloc.s      23                                    // num14 = time / period
                    IL_0719: add
                    IL_071a: stfld        int32 MinerComponent::productCount   // productCount += num14
             */
            CodeMatcher matcher = new CodeMatcher(instructions);

            // 定位抽水机分支：planet.waterItemId -> productId
            matcher.MatchForward(false,
                new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(PlanetData), nameof(PlanetData.waterItemId))),
                new CodeMatch(OpCodes.Stfld, AccessTools.Field(typeof(MinerComponent), nameof(MinerComponent.productId))));

            // 其后第一个 productCount 写入点 = productCount += num14
            matcher.MatchForward(true,
                new CodeMatch(OpCodes.Stfld, AccessTools.Field(typeof(MinerComponent), nameof(MinerComponent.productCount))));

            // 在 stfld productCount 之后插入：
            //   ldarg.1                              // PlanetFactory factory
            //   ldloc.s  23                          // num14（本次产水量）
            //   call      ConsumeWater(int planetId, int amount)
            matcher.Advance(1).InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_1),
                new CodeInstruction(OpCodes.Ldloc_S, (byte)23),
                new CodeInstruction(OpCodes.Call,
                    AccessTools.Method(typeof(PlanetAtmospherePatches), nameof(ConsumeWater), new[] { typeof(int), typeof(int) })));

            return matcher.InstructionEnumeration();
        }

        // ==================== 大气采集站（大气消耗） ====================

        /// <summary>各站点原始采集速度（第一次调用时保存，避免多次缩放叠加）</summary>
        private static readonly Dictionary<int, float[]> OriginalCollectionPerTick = new Dictionary<int, float[]>();

        /// <summary>
        /// 大气采集站速度挂钩：采集速度 ∝ 大气池剩余比例（与抽水机同模式），
        /// 低于停产底线则速度为 0；不在池化气体列表中的产物不采集。
        /// 基于原始速度重算（幂等），不直接改字段以免缩放叠加。
        /// </summary>
        [HarmonyPatch(typeof(StationComponent), nameof(StationComponent.UpdateCollection))]
        [HarmonyPrefix]
        public static void StationComponent_UpdateCollection_Prefix(ref StationComponent __instance, PlanetFactory factory)
        {
            if (__instance.collectionPerTick == null) return;

            if (!__instance.isCollector) return;

            // 首次调用保存原始速度
            if (!OriginalCollectionPerTick.TryGetValue(__instance.id, out float[] original))
            {
                original = (float[])__instance.collectionPerTick.Clone();
                OriginalCollectionPerTick[__instance.id] = original;
            }

            PlanetData planet = factory.planet;
            float[] gasPool = GetGasPool(planet);
            float[] initialGas = GetInitialGas(planet);

            for (int i = 0; i < __instance.collectionIds.Length && i < original.Length; i++)
            {
                int index = Array.IndexOf(GasItemIds, __instance.collectionIds[i]);

                if (index < 0)
                {
                    __instance.collectionPerTick[i] = 0f;
                    continue;
                }

                float ratio = GetPoolRatio(planet, gasPool[index], initialGas[index]);

                // 停产底线
                if (ratio <= FloorRatio) ratio = 0f;

                // 行星主题加成：大气采集速度 × 主题倍率（飓风石林快、熔岩慢）
                float themeMultiplier = PlanetThemeMultiplierPatches.GetMultiplier(planet,
                    PlanetThemeMultiplierPatches.MultiplierType.CollectSpeed);

                __instance.collectionPerTick[i] = original[i] * ratio * themeMultiplier;
            }
        }

        /// <summary>
        /// 大气采集站产出扣池钩子：在 productRegister[itemId] += num（IL_00b4）之后扣大气池。
        /// </summary>
        [HarmonyPatch(typeof(StationComponent), nameof(StationComponent.UpdateCollection))]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> StationComponent_UpdateCollection_Transpiler(
            IEnumerable<CodeInstruction> instructions)
        {
            /*
                目标 IL（Assembly-CSharp.dll / StationComponent.UpdateCollection）：

                    IL_0099: ldarg.3                              // int[] productRegister
                    IL_009a: ldarg.0
                    IL_009b: ldfld       StationStore[] StationComponent::'storage'
                    IL_00a0: ldloc.0                              // i
                    IL_00a1: ldelema     StationStore
                    IL_00a6: ldfld       int32 StationStore::itemId
                    IL_00ab: ldelema     [netstandard]System.Int32
                    IL_00b0: dup
                    IL_00b1: ldind.i4
                    IL_00b2: ldloc.3                              // num（本次采集量）
                    IL_00b3: add
                    IL_00b4: stind.i4                             // productRegister[itemId] += num
             */
            CodeMatcher matcher = new CodeMatcher(instructions);

            // 定位 productRegister[itemId] += num（该序列在方法内唯一）
            matcher.MatchForward(false,
                new CodeMatch(OpCodes.Ldarg_3),
                new CodeMatch(OpCodes.Ldarg_0),
                new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(StationComponent), nameof(StationComponent.storage))),
                new CodeMatch(OpCodes.Ldloc_0),
                new CodeMatch(OpCodes.Ldelema, typeof(StationStore)),
                new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(StationStore), nameof(StationStore.itemId))),
                new CodeMatch(OpCodes.Ldelema, typeof(int)),
                new CodeMatch(OpCodes.Dup),
                new CodeMatch(OpCodes.Ldind_I4),
                new CodeMatch(OpCodes.Ldloc_3),
                new CodeMatch(OpCodes.Add),
                new CodeMatch(OpCodes.Stind_I4));

            // 在 stind.i4 之后插入：
            //   ldarg.1                                    // PlanetFactory factory
            //   ldarg.0; ldfld storage; ldloc.0; ldelema StationStore; ldfld itemId   // 重新取产物 ID
            //   ldloc.3                                    // num
            //   call      ConsumeGas(PlanetFactory, int, int)
            matcher.Advance(1).InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_1),
                new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(StationComponent), nameof(StationComponent.storage))),
                new CodeInstruction(OpCodes.Ldloc_0),
                new CodeInstruction(OpCodes.Ldelema, typeof(StationStore)),
                new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(StationStore), nameof(StationStore.itemId))),
                new CodeInstruction(OpCodes.Ldloc_3),
                new CodeInstruction(OpCodes.Call,
                    AccessTools.Method(typeof(PlanetAtmospherePatches), nameof(ConsumeGas),
                        new[] { typeof(PlanetFactory), typeof(int), typeof(int) })));

            // 再插入主题副产物填充（按比例附带产出）：
            //   ldarg.0                                    // StationComponent
            //   ldarg.1                                    // PlanetFactory
            //   ...storage[i].itemId 重新取...
            //   ldloc.3                                    // num
            //   ldarg.3                                    // int[] productRegister
            //   call      ByproductFill(StationComponent, PlanetFactory, int, int, int[])
            matcher.InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Ldarg_1),
                new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(StationComponent), nameof(StationComponent.storage))),
                new CodeInstruction(OpCodes.Ldloc_0),
                new CodeInstruction(OpCodes.Ldelema, typeof(StationStore)),
                new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(StationStore), nameof(StationStore.itemId))),
                new CodeInstruction(OpCodes.Ldloc_3),
                new CodeInstruction(OpCodes.Ldarg_3),
                new CodeInstruction(OpCodes.Call,
                    AccessTools.Method(typeof(PlanetThemeMultiplierPatches), nameof(PlanetThemeMultiplierPatches.ByproductFill),
                        new[] { typeof(StationComponent), typeof(PlanetFactory), typeof(int), typeof(int), typeof(int[]) })));

            return matcher.InstructionEnumeration();
        }

        // ==================== 排污（燃料燃烧 → 大气池注入） ====================

        /// <summary>排放量换算：每 3MJ 热值排 1 单位（至少 1）</summary>
        private const int HeatPerEmission = 3000000;

        /// <summary>
        /// 燃料 → 排放物气体映射（覆盖全部 FuelType>0 燃料）：
        /// 碳基 → CO₂、氢基 → 水、硫 → SO₂、氦聚变 → 氦；
        /// 未收录的核能/质能/蓄电器/黑雾燃料为清洁无排放。
        /// 排放量按热值自动换算：max(1, HeatValue / 3MJ)。
        /// </summary>
        private static readonly Dictionary<int, int> FuelEmissionGas = new Dictionary<int, int>
        {
            // 碳基燃料 → CO₂
            { 1006, ProtoID.I二氧化碳 },      // 煤矿
            { 1007, ProtoID.I二氧化碳 },      // 原油
            { 1011, ProtoID.I二氧化碳 },      // 可燃冰
            { 1030, ProtoID.I二氧化碳 },      // 木材
            { 1031, ProtoID.I二氧化碳 },      // 植物燃料
            { 1109, ProtoID.I二氧化碳 },      // 高能石墨
            { 1112, ProtoID.I二氧化碳 },      // 金刚石
            { 1114, ProtoID.I二氧化碳 },      // 焦油
            { 1117, ProtoID.I二氧化碳 },      // 有机晶体
            { 1123, ProtoID.I二氧化碳 },      // 石墨烯
            { 1124, ProtoID.I二氧化碳 },      // 碳纳米管
            { 1128, ProtoID.I二氧化碳 },      // 燃烧单元
            { 1129, ProtoID.I二氧化碳 },      // 爆破单元
            { 1130, ProtoID.I二氧化碳 },      // 晶石爆破单元
            { 1141, ProtoID.I二氧化碳 },      // 增产剂 Mk.I
            { 1142, ProtoID.I二氧化碳 },      // 增产剂 Mk.II
            { 6212, ProtoID.I二氧化碳 },      // 四氢双环戊二烯
            { 6216, ProtoID.I二氧化碳 },      // 四氢双环戊二烯燃料棒
            { 6217, ProtoID.I二氧化碳 },      // 煤油燃料棒
            { 7006, ProtoID.I二氧化碳 },      // 苯
            { 7008, ProtoID.I二氧化碳 },      // 甲烷
            { 7009, ProtoID.I二氧化碳 },      // 丙烯

            // 氢基燃料 → 水
            { 1120, ProtoID.I水 },            // 氢
            { 1121, ProtoID.I水 },            // 重氢
            { 1801, ProtoID.I水 },            // 氢燃料棒
            { 1802, ProtoID.I水 },            // 氘核燃料棒
            { 6245, ProtoID.I水 },            // 氘氦混合燃料棒
            { 7002, ProtoID.I水 },            // 氨（NH₃ 燃烧 → N₂ + H₂O）

            // 硫基燃料 → SO₂
            { 6205, ProtoID.I二氧化硫 },      // 二氧化硫

            // 氦聚变 → 氦
            { 6244, ProtoID.I氦 },            // 氦三燃料棒

            // 清洁无排放：钍/铀/MOX 棒、反物质棒、金色棒、核能/湮灭单元、
            // 蓄电器、能量碎片、创世之书/日志（黑雾能量）、钍燃料（熔盐堆）
        };


        /// <summary>
        /// 排污钩子：燃料发电机每消耗 1 个燃料，按燃料类型向所在星球大气池注入对应排放物
        /// （碳基→CO₂、氢基→水、硫→SO₂、氦聚变→氦；核能清洁），量按热值换算，
        /// 积累的大气成分可被大气采集站采集（成分影响产出）。
        /// 插入点与熔盐堆相同（consumeRegister[fuelId]++ 之后），两个 transpiler 互不干扰。
        /// </summary>
        [HarmonyPatch(typeof(PowerGeneratorComponent), nameof(PowerGeneratorComponent.GenEnergyByFuel))]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> GenEnergyByFuel_Emission_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            /*
                目标 IL（Assembly-CSharp.dll / PowerGeneratorComponent.GenEnergyByFuel）：

                    IL_0145: ldarg.0
                    IL_0146: ldarg.0
                    IL_0147: ldfld       int16 PowerGeneratorComponent::fuelCount
                    IL_014c: ldc.i4.1
                    IL_014d: sub
                    IL_014e: conv.i2
                    IL_014f: stfld       int16 PowerGeneratorComponent::fuelCount   // fuelCount--

                    IL_0154: ldarg.2
                    IL_0155: ldarg.0
                    IL_0156: ldfld       int16 PowerGeneratorComponent::fuelId
                    IL_015b: ldelema     [netstandard]System.Int32
                    IL_0160: dup
                    IL_0161: ldind.i4
                    IL_0162: ldc.i4.1
                    IL_0163: add
                    IL_0164: stind.i4                                             // consumeRegister[fuelId]++
             */
            CodeMatcher matcher = new CodeMatcher(instructions);

            // 匹配 consumeRegister[fuelId]++ 的完整序列（与熔盐堆相同锚点）
            matcher.MatchForward(false,
                new CodeMatch(OpCodes.Stfld, AccessTools.Field(typeof(PowerGeneratorComponent), nameof(PowerGeneratorComponent.fuelCount))),
                new CodeMatch(OpCodes.Ldarg_2),
                new CodeMatch(OpCodes.Ldarg_0),
                new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(PowerGeneratorComponent), nameof(PowerGeneratorComponent.fuelId))),
                new CodeMatch(OpCodes.Ldelema, typeof(int)),
                new CodeMatch(OpCodes.Dup),
                new CodeMatch(OpCodes.Ldind_I4),
                new CodeMatch(OpCodes.Ldc_I4_1),
                new CodeMatch(OpCodes.Add),
                new CodeMatch(OpCodes.Stind_I4));

            // 在 stind.i4 之后插入对 GenEnergyByFuel_Emission 的调用
            matcher.Advance(1).InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Ldarg_2),
                new CodeInstruction(OpCodes.Call,
                    AccessTools.Method(typeof(PlanetAtmospherePatches), nameof(GenEnergyByFuel_Emission))));

            return matcher.InstructionEnumeration();
        }

        /// <summary>燃料燃烧排放：按燃料类型向所在星球大气池注入对应排放物，量按热值换算</summary>
        public static void GenEnergyByFuel_Emission(ref PowerGeneratorComponent component, int[] consumeRegister)
        {
            if (component.fuelId <= 0) return;

            // 按燃料查排放映射（清洁燃料/未收录燃料无排放）
            if (!FuelEmissionGas.TryGetValue(component.fuelId, out int gasItemId)) return;

            float emissionScale = ProjectGenesis.EmissionScaleEntry?.Value ?? 1f;

            if (emissionScale <= 0f) return;

            // 通过消耗寄存器定位所在工厂（统计池索引与 GameData.factories 一致）
            PlanetFactory factory = null;

            FactoryProductionStat[] stats = GameMain.data.statistics.production.factoryStatPool;

            for (int i = 0; i < stats.Length; i++)
            {
                if (stats[i] == null || stats[i].consumeRegister != consumeRegister) continue;

                factory = GameMain.data.factories[i];
                break;
            }

            if (factory == null) return;

            int gasIndex = Array.IndexOf(GasItemIds, gasItemId);

            if (gasIndex < 0) return;

            // 排放量 = max(1, 热值 / 3MJ) × 倍率
            ItemProto fuel = LDB.items.Select(component.fuelId);

            if (fuel == null) return;

            int amount = Math.Max(1, (int)(fuel.HeatValue / (float)HeatPerEmission * emissionScale));

            if (amount <= 0) return;

            ModifyGas(factory.planetId, gasIndex, amount);
        }
    }
}
