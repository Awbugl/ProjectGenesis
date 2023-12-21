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
            int tankId = __instance.tankId;

            TankComponent tankComponent = __instance.storage.tankPool[tankId];
            if (tankComponent.id != tankId) return;

            int fluidId = tankComponent.fluidId;

            if (FluidColor.TryGetValue(fluidId, out Color value))
            {
                __instance.exchangeAndColoring(value);
                return;
            }
            else if (FluidWithoutIconColor.TryGetValue(fluidId, out value))
            {
                __instance.exchangeAndColoring(value);
            }
        }
    }
}
