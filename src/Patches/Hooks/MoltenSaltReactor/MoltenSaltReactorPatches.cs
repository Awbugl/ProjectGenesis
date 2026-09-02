using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using ProjectGenesis.Utils;

// ReSharper disable InconsistentNaming

namespace ProjectGenesis.Patches
{
    /// <summary>
    /// 熔盐堆：燃烧钍燃料、产出铀燃料的发电建筑。
    /// 思路：发电组件每消耗 1 个钍燃料，就往 productCount 里累计 1 个铀燃料；
    /// 分拣器/无人机通过燃料取出路径（PickFuelFrom / PickFrom）把铀燃料带走。
    /// 本实现是旧版"燃料棒回收（消耗燃料棒产出空燃料棒）"patch 针对当前游戏版本的适配，
    /// 旧实现因游戏更新（UIPowerGeneratorWindow / UIInserterBuildTip 改动）被移除。
    /// </summary>
    public static class MoltenSaltReactorPatches
    {
        /// <summary>产物缓存上限（与原版射线接收站光子缓存上限一致）</summary>
        private const int MaxProductCount = 20;

        /// <summary>
        /// 熔盐堆燃料掩码（钍燃料专用 FuelType=32）。
        /// 注意：ItemProto.fuelNeeds 数组长度为 64，且按位掩码匹配（i & FuelType），
        /// 可用的单个位只有 1/2/4/8/16/32，64 会越界，故用 32。
        /// </summary>
        internal const short MoltenSaltFuelMask = 32;

        /// <summary>每消耗 1 个钍燃料累计 1 个铀燃料，并计入生产统计（缓存满时铀丢弃，不计统计避免漂移）。
        /// 由统一发电钩子（Transpliers/PowerGeneratorComponent_GameTick）调用，不再单独挂 transpiler。</summary>
        internal static void OnFuelBurned(ref PowerGeneratorComponent component, int[] consumeRegister)
        {
            // 只处理熔盐堆的专用燃料（curFuelId 是本次被消耗的燃料）
            if (component.curFuelId != ProtoID.I钍燃料) return;

            // 产物缓存已满：本次铀丢弃，不累计也不计统计（避免统计漂移）
            if (component.productCount >= MaxProductCount) return;

            component.productCount += 1;

            // 生产统计：找到当前工厂对应的统计项，计入铀燃料产量
            // ReSharper disable once LoopCanBePartlyConvertedToQuery
            foreach (FactoryProductionStat stat in GameMain.data.statistics.production.factoryStatPool)
            {
                if (stat.consumeRegister != consumeRegister) continue;

                stat.productRegister[ProtoID.I铀燃料] += 1;

                return;
            }
        }

        /// <summary>
        /// 燃料取出钩子（两个重载都挂）：原版燃料取出失败时，尝试取出产物铀燃料。
        /// </summary>
        [HarmonyPatch(typeof(PowerGeneratorComponent), nameof(PowerGeneratorComponent.PickFuelFrom),
            new[] { typeof(int), typeof(int), }, new[] { ArgumentType.Normal, ArgumentType.Out, })]
        [HarmonyPatch(typeof(PowerGeneratorComponent), nameof(PowerGeneratorComponent.PickFuelFrom),
            new[] { typeof(int), typeof(int), typeof(int), }, new[] { ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Out, })]
        [HarmonyPostfix]
        public static void PickFuelFrom_Postfix(ref PowerGeneratorComponent __instance, int filter, ref int inc, ref int __result)
        {
            // 原版已成功取出燃料（或该发电机没有产物）则不动
            if (__result != 0) return;

            if (__instance.fuelMask != MoltenSaltFuelMask) return;

            if (filter != ProtoID.I铀燃料 && filter != 0) return;

            var count = (int)__instance.productCount;

            if (count <= 0) return;

            __instance.productCount = count - 1;
            inc = 0;
            __result = ProtoID.I铀燃料;
        }

        /// <summary>
        /// 分拣器/物流取出钩子（entityId 重载）：目标实体是熔盐堆发电机时，原版取不到东西则取产物。
        /// </summary>
        [HarmonyPatch(typeof(PlanetFactory), nameof(PlanetFactory.PickFrom),
            new[]
            {
                typeof(int), typeof(int), typeof(int), typeof(int[]),
                typeof(byte), typeof(byte),
            },
            new[]
            {
                ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Normal,
                ArgumentType.Out, ArgumentType.Out,
            })]
        [HarmonyPostfix]
        public static void PickFrom_Postfix(PlanetFactory __instance, int entityId, int offset, int filter, ref byte inc,
            ref int __result)
        {
            if (__result != 0) return;

            EntityData entityData = __instance.entityPool[entityId];

            if (entityData.powerGenId == 0) return;

            PickProduct(__instance, entityData.powerGenId, offset, filter, ref inc, ref __result);
        }

        /// <summary>
        /// 分拣器/物流取出钩子（typedId 重载，分拣器走的是这个）：目标实体是熔盐堆发电机时取产物。
        /// </summary>
        [HarmonyPatch(typeof(PlanetFactory), nameof(PlanetFactory.PickFrom),
            new[]
            {
                typeof(uint), typeof(int), typeof(int), typeof(int[]),
                typeof(byte), typeof(byte),
            },
            new[]
            {
                ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Normal,
                ArgumentType.Out, ArgumentType.Out,
            })]
        [HarmonyPostfix]
        public static void PickFrom_TypedId_Postfix(PlanetFactory __instance, uint ioTargetTypedId, int offset, int filter,
            ref byte inc, ref int __result)
        {
            // 只处理 PowerGen 目标
            if ((EFactoryIOTargetType)(ioTargetTypedId & (uint)EFactoryIOTargetType.TypeMask) != EFactoryIOTargetType.PowerGen) return;

            int powerGenId = (int)(ioTargetTypedId & (uint)EFactoryIOTargetType.IdMask);

            if (powerGenId == 0) return;

            PickProduct(__instance, powerGenId, offset, filter, ref inc, ref __result);
        }

        private static void PickProduct(PlanetFactory factory, int powerGenId, int offset, int filter, ref byte inc, ref int result)
        {
            if (result != 0) return;

            // 只处理熔盐堆，且只响应铀燃料（或未筛选取全部）
            if (filter != ProtoID.I铀燃料 && filter != 0) return;

            ref PowerGeneratorComponent component = ref factory.powerSystem.genPool[powerGenId];

            if (component.id != powerGenId || component.fuelMask != MoltenSaltFuelMask) return;

            // 发电机→发电机的燃料转运场景：无明确筛选时不拦截，避免抢燃料
            if (filter == 0 && offset > 0 && factory.powerSystem.genPool[offset].id == offset) return;

            lock (factory.entityMutexs[component.entityId])
            {
                var count = (int)component.productCount;

                if (count <= 0) return;

                component.productCount = count - 1;
                inc = 0;
                result = ProtoID.I铀燃料;
            }
        }

        /// <summary>
        /// 分拣器放置提示：目标实体是熔盐堆时，把铀燃料加入可选筛选，方便玩家指定"只取铀"。
        /// </summary>
        [HarmonyPatch(typeof(UIBeltBuildTip), nameof(UIBeltBuildTip.SetOutputEntity))]
        [HarmonyPostfix]
        public static void UIBeltBuildTip_SetOutputEntity_Postfix(UIBeltBuildTip __instance, int entityId)
        {
            if (entityId <= 0) return;

            PlanetFactory factory = GameMain.mainPlayer?.factory;

            if (factory == null) return;

            EntityData entityData = factory.entityPool[entityId];

            if (entityData.powerGenId <= 0) return;

            PowerGeneratorComponent component = factory.powerSystem.genPool[entityData.powerGenId];

            if (component.id != entityData.powerGenId || component.fuelMask != MoltenSaltFuelMask) return;

            // filterItems 是私有字段，用 Traverse 追加
            List<int> filterItems = Traverse.Create(__instance).Field("filterItems").GetValue<List<int>>();

            if (filterItems != null && !filterItems.Contains(ProtoID.I铀燃料)) filterItems.Add(ProtoID.I铀燃料);
        }

        /// <summary>
        /// 实体信息面板：熔盐堆的产物（铀燃料）数量展示。
        /// </summary>
        [HarmonyPatch(typeof(EntityBriefInfo), nameof(EntityBriefInfo.SetBriefInfo))]
        [HarmonyPostfix]
        public static void SetBriefInfo_Postfix(EntityBriefInfo __instance, PlanetFactory _factory, int _entityId)
        {
            if (_factory == null || _entityId <= 0) return;

            EntityData entityData = _factory.entityPool[_entityId];

            if (entityData.id == 0 || entityData.powerGenId == 0) return;

            PowerGeneratorComponent component = _factory.powerSystem.genPool[entityData.powerGenId];

            if (component.id != entityData.powerGenId || component.fuelMask != MoltenSaltFuelMask) return;

            var productCount = (int)component.productCount;

            if (productCount > 0) __instance.storage.Add(ProtoID.I铀燃料, productCount, 0);
        }
    }
}
