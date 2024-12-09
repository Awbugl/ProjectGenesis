using System.Collections.Generic;
using HarmonyLib;
using ProjectGenesis.Utils;

// ReSharper disable InconsistentNaming

namespace ProjectGenesis.Patches
{
    public static class RecipeExtraPowerPatches
    {
        /// <summary>
        /// 1 == 300kw
        /// </summary>
        public static readonly Dictionary<int, int> RecipePowerRate = new Dictionary<int, int>
        {
            { ProtoID.R水电解, 4 },
            { ProtoID.R羰基合成, 4 },
            { ProtoID.R重氢, 8 },
            { ProtoID.R三元精金, 16 },
            { ProtoID.R光子物质化, 8 },
            { ProtoID.R奇异物质, 8 },
            { ProtoID.R氦核转化, 8 },
            { ProtoID.R质能储存高效, 8 },
            { ProtoID.R能量物质化, 8 },
        };

        [HarmonyPatch(typeof(AssemblerComponent), nameof(AssemblerComponent.SetPCState))]
        [HarmonyPostfix]
        public static void AssemblerComponent_SetPCState_Postfix(AssemblerComponent __instance, PowerConsumerComponent[] pcPool)
        {
            if (!__instance.replicating) return;

            ref PowerConsumerComponent component = ref pcPool[__instance.pcId];

            int num = RecipePowerRate.GetValueOrDefault(__instance.recipeId, 1);

            component.requiredEnergy = (component.workEnergyPerTick + __instance.speedOverride * num / 2)
              * (1000 + __instance.extraPowerRatio) / 1000;
        }
    }
}
