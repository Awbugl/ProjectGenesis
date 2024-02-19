using HarmonyLib;

// ReSharper disable InconsistentNaming

namespace ProjectGenesis.Patches.UI.QTools
{
    public static class QToolsWindowPatches
    {
        [HarmonyPatch(typeof(UIGame), nameof(UIGame._OnInit))]
        [HarmonyPostfix]
        public static void Init(UIRoot __instance)
        {
            if (ProjectGenesis.QToolsWindow) return;

            ProjectGenesis.QToolsWindow = UIQToolsWindow.CreateWindow();
        }
    }
}
