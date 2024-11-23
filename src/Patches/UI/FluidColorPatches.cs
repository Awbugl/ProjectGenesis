using HarmonyLib;
using ProjectGenesis.Utils;

// ReSharper disable InconsistentNaming

namespace ProjectGenesis.Patches.UI
{
    internal static class FluidColorPatches
    {
        [HarmonyPatch(typeof(UITankWindow), nameof(UITankWindow._OnUpdate))]
        [HarmonyPostfix]
        public static void UITankWindow_OnUpdate(UITankWindow __instance)
        {
            int tankId = __instance.tankId;

            TankComponent tankComponent = __instance.storage.tankPool[tankId];

            if (tankComponent.id != tankId) return;

            int fluidId = tankComponent.fluidId;

            if (IconDescUtils.IconDescs.TryGetValue(fluidId, out IconDescUtils.ModIconDesc value))
                __instance.exchangeAndColoring(value.Color);
        }
    }
}
