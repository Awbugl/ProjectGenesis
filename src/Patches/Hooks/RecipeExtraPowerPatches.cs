using System.Collections.Generic;
using System.Text;
using HarmonyLib;
using ProjectGenesis.Utils;
using UnityEngine;

// ReSharper disable InconsistentNaming

namespace ProjectGenesis.Patches
{
    public static class RecipeExtraPowerPatches
    {
        /// <summary>
        /// 1 == 300kw
        /// </summary>
        private static readonly Dictionary<int, int> RecipePowerRate = new Dictionary<int, int>
        {
            { ProtoID.R水电解, 4 },
            { ProtoID.R盐水电解, 4 },
            { ProtoID.R合成氨, 2 },
            { ProtoID.R氨氧化, 2 },
            { ProtoID.R有机晶体活化, 8 },
            { ProtoID.R放射性矿物处理, 8 },
            { ProtoID.R重氢, 18 },
            { ProtoID.R三元精金, 18 },
            { ProtoID.R光子物质化, 18 },
            { ProtoID.R氦核转化, 4 },
            { ProtoID.R质能储存高效, 8 },
            { ProtoID.R能量物质化, 8 },
        };

        private static readonly StringBuilder _sb = new StringBuilder("         ", 8);

        [HarmonyPatch(typeof(AssemblerComponent), nameof(AssemblerComponent.SetPCState))]
        [HarmonyPrefix]
        public static bool AssemblerComponent_SetPCState_Prefix(ref AssemblerComponent __instance, PowerConsumerComponent[] pcPool)
        {
            ref PowerConsumerComponent component = ref pcPool[__instance.pcId];

            int num = RecipePowerRate.GetValueOrDefault(__instance.recipeId, 1);
            if (num == 1 || !__instance.replicating) return true;

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

            int num = RecipePowerRate.GetValueOrDefault(assembler.recipeId, 1);
            if (num == 1 || !assembler.replicating) return;

            long extraPowerW = assembler.speedOverride * num / 2 * (1000 + assembler.extraPowerRatio) / 100 * 6;

            StringBuilderUtility.WriteKMGPower(_sb, 7, extraPowerW);

            __instance.stateText.text = $"[+{_sb.ToString().Trim()} ]" + __instance.stateText.text;
        }
    }
}
