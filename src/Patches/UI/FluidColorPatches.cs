using HarmonyLib;
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
            var tankComponent = __instance.storage.tankPool[__instance.tankId];
            if (tankComponent.id != __instance.tankId) return;

            if (FluidColor.TryGetValue(tankComponent.fluidId, out var value)) __instance.exchangeAndColoring(value);
        }
    }
}
