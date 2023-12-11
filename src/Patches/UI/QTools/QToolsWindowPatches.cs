using HarmonyLib;

// ReSharper disable InconsistentNaming

namespace ProjectGenesis.Patches.UI.QTools
{
    public static class QToolsWindowPatches
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(UIGame), "_OnInit")]
        public static void Init(UIRoot __instance) => ProjectGenesis.QToolsWindow = UIQToolsWindow.CreateWindow();
    }
}
