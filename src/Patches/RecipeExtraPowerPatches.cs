using System.Collections.Generic;
using HarmonyLib;
using ProjectGenesis.Utils;

// ReSharper disable InconsistentNaming

namespace ProjectGenesis.Patches
{
    public static class RecipeExtraPowerPatches
    {
        // ReSharper disable once MemberCanBePrivate.Global
        public static readonly Dictionary<int, int> RecipePowerRate = new Dictionary<int, int>
        {
            { ProtoID.R水电解, 20000 },
            { ProtoID.R二氧化硫还原, 10000 },
            { ProtoID.R海水淡化, 10000 },
            { ProtoID.R盐水电解, 10000 },
        };

        [HarmonyPatch(typeof(AssemblerComponent), nameof(AssemblerComponent.SetPCState))]
        [HarmonyPostfix]
        public static void AssemblerComponent_SetPCState_Postfix(AssemblerComponent __instance, PowerConsumerComponent[] pcPool)
        {
            if (!__instance.replicating) return;

            if (!RecipePowerRate.TryGetValue(__instance.recipeId, out int num)) return;

            ref PowerConsumerComponent component = ref pcPool[__instance.pcId];
            component.requiredEnergy = (component.workEnergyPerTick + __instance.speedOverride * num) * (1000 + __instance.extraPowerRatio) / 1000L;
        }
    }
}
