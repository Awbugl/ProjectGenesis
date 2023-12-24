using HarmonyLib;

// ReSharper disable InconsistentNaming

namespace ProjectGenesis.Patches.UI
{
    public static class UITurretWindowPatches
    {
        [HarmonyPatch(typeof(UITurretWindow), nameof(UITurretWindow._OnUpdate))]
        [HarmonyPostfix]
        public static void UITurretWindow_OnUpdate(UITurretWindow __instance)
        {
            __instance.handFillAmmoIcon0.gameObject.SetActive(false);
            __instance.handFillAmmoIcon1.gameObject.SetActive(false);
            __instance.handFillAmmoIcon2.gameObject.SetActive(false);
        }
    }
}
