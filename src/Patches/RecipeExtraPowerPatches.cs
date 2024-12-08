using System.Collections.Generic;
using HarmonyLib;
using ProjectGenesis.Utils;

// ReSharper disable InconsistentNaming

namespace ProjectGenesis.Patches
{
    public static class RecipeExtraPowerPatches
    {
        /// <summary>
        /// 1 == 600kw
        /// </summary>
        public static readonly Dictionary<int, int> RecipePowerRate = new Dictionary<int, int>
        {
            { ProtoID.R水电解, 2 },
            { ProtoID.R重氢, 4 },
            { ProtoID.R三元精金, 4 },
            { ProtoID.R光子物质化, 4 },
            { ProtoID.R奇异物质, 4 },
            { ProtoID.R氦核转化, 4 },
            { ProtoID.R质能储存高效, 4 },
            { ProtoID.R能量物质化, 4 },
        };

        [HarmonyPatch(typeof(AssemblerComponent), nameof(AssemblerComponent.SetPCState))]
        [HarmonyPostfix]
        public static void AssemblerComponent_SetPCState_Postfix(AssemblerComponent __instance, PowerConsumerComponent[] pcPool)
        {
            if (!__instance.replicating) return;

            if (!RecipePowerRate.TryGetValue(__instance.recipeId, out int num)) return;

            ref PowerConsumerComponent component = ref pcPool[__instance.pcId];
            component.requiredEnergy = (component.workEnergyPerTick + __instance.speedOverride * num) * (1000 + __instance.extraPowerRatio)
                                     / 1000L;
        }
    }
}
