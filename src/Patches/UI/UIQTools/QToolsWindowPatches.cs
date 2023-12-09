using HarmonyLib;

// ReSharper disable InconsistentNaming

namespace ProjectGenesis.Patches.UI.UIQTools
{
    public static class QToolsWindowPatches
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(UIRoot), "_OnCreate")]
        public static void Init(UIRoot __instance) => ProjectGenesis.QToolsWindow = UIQToolsWindow.CreateWindow();
    }
}
