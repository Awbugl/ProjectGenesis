using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;

// ReSharper disable InconsistentNaming

namespace ProjectGenesis.Patches
{
    /// <summary>
    /// 统一发电钩子：只替换 PowerSystem.GameTick 中 GenEnergyByFuel 的调用目标
    /// （参数与返回完全一致，零 IL 注入），在方法内先执行原逻辑再分发各功能。
    /// 性能约定：
    /// - 未燃料（curFuelId == 0）时入口仅一次比较即返回
    /// - 各分发方自身带快速短路（熔盐堆/排污只在真正烧了对应燃料时进入）
    /// </summary>
    public static class PowerGeneratorComponent_GameTick_PrePatch
    {
        [HarmonyPatch(typeof(PowerSystem), nameof(PowerSystem.GameTick))]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> PowerSystem_GameTick_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            CodeMatcher matcher = new CodeMatcher(instructions);

            // 仅替换调用目标：call GenEnergyByFuel(long, int[]) → call PrePatch_Method(ref, long, int[])
            // 栈帧一致（this/ref + int64 + int[]），无任何插入指令，不存在无效 IL 风险
            matcher.MatchForward(false,
                new CodeMatch(OpCodes.Call,
                    AccessTools.Method(typeof(PowerGeneratorComponent), nameof(PowerGeneratorComponent.GenEnergyByFuel))));

            matcher.SetAndAdvance(OpCodes.Call,
                AccessTools.Method(typeof(PowerGeneratorComponent_GameTick_PrePatch), nameof(GenEnergyByFuel_PrePatch_Method)));

            return matcher.InstructionEnumeration();
        }

        /// <summary>发电燃烧统一入口：先原逻辑，再按需分发（全部快速短路）</summary>
        public static void GenEnergyByFuel_PrePatch_Method(ref PowerGeneratorComponent component, long energy, int[] consumeRegister)
        {
            // 原版燃烧逻辑
            component.GenEnergyByFuel(energy, consumeRegister);

            // 未燃料：一次比较即返回（大多数发电机关机/无燃料 tick 走此路径）
            if (component.curFuelId <= 0) return;

            MoltenSaltReactorPatches.OnFuelBurned(ref component, consumeRegister);
            PlanetAtmospherePatches.OnFuelBurned(ref component, consumeRegister);
        }
    }
}
