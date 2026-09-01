using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
    }
}
