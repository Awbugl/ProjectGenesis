using HarmonyLib;

// ReSharper disable InconsistentNaming

namespace ProjectGenesis.Patches.UI
{
    public static class UIMainMenuPatches
    {
        [HarmonyPatch(typeof(UIMainMenu), nameof(UIMainMenu.UpdateDemoScene))]
        [HarmonyPostfix]
        public static void UpdateDemoScene(UIMainMenu __instance)
        {
            if (DSPGame.LoadDemoIndex != -3) return;

            __instance.scenesCutBlackImage.gameObject.SetActive(false);
        }
    }
}
