using HarmonyLib;
using UnityEngine;
using static ProjectGenesis.Utils.IconDescUtils;

// ReSharper disable InconsistentNaming

namespace ProjectGenesis.Patches.UI
{
    internal static class FluidColorPatches
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(UITankWindow), "_OnUpdate")]
        public static void UITankWindow_OnUpdate(ref UITankWindow __instance)
        {
            TankComponent tankComponent = __instance.storage.tankPool[__instance.tankId];
            if (tankComponent.id != __instance.tankId) return;

            if (FluidColor.TryGetValue(tankComponent.fluidId, out Color32 value)) __instance.exchangeAndColoring(value);
        }
    }
}
