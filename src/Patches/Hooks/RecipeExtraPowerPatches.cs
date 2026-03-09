using System.Text;
using HarmonyLib;
using UnityEngine;

// ReSharper disable InconsistentNaming

namespace ProjectGenesis.Patches
{
    public static class RecipeExtraPowerPatches
    {
        /// <summary>
        /// 1 == 300kw
        /// </summary>
        private static readonly StringBuilder _sb = new StringBuilder("         ", 8);

        [HarmonyPatch(typeof(AssemblerComponent), nameof(AssemblerComponent.SetPCState))]
        [HarmonyPrefix]
        public static bool AssemblerComponent_SetPCState_Prefix(ref AssemblerComponent __instance, PowerConsumerComponent[] pcPool)
        {
            ref PowerConsumerComponent component = ref pcPool[__instance.pcId];

            int num = LDB.recipes.Select(__instance.recipeId)?.PowerFactor ?? 1;
            if (num < 2 || !__instance.replicating) return true;

            component.requiredEnergy = __instance.replicating
                ? (__instance.speedOverride * num / 2 + component.workEnergyPerTick) * (1000 + __instance.extraPowerRatio) / 1000
                : component.idleEnergyPerTick;

            return false;
        }

        [HarmonyPatch(typeof(UIAssemblerWindow), nameof(UIAssemblerWindow._OnUpdate))]
        [HarmonyPostfix]
        public static void UIAssemblerWindow_OnUpdate_Postfix(UIAssemblerWindow __instance)
        {
            if (__instance.powerIcon.color == Color.clear) return;

            AssemblerComponent assembler = __instance.factorySystem.assemblerPool[__instance.assemblerId];
            if (assembler.id != __instance.assemblerId) return;

            int num = LDB.recipes.Select(assembler.recipeId)?.PowerFactor ?? 1;
            if (num < 2 || !assembler.replicating) return;

            long extraPowerW = assembler.speedOverride * num / 2 * (1000 + assembler.extraPowerRatio) / 100 * 6;

            StringBuilderUtility.WriteKMGPower(_sb, 7, extraPowerW);

            __instance.stateText.text = $"[+{_sb.ToString().Trim()} ]" + __instance.stateText.text;
        }
    }
}
