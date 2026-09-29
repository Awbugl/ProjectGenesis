using HarmonyLib;

// ReSharper disable InconsistentNaming

namespace ProjectGenesis.Patches
{
    public static class SectorModelPatches
    {
        /// <summary>
        ///     0.10.35: when switching games (new game / load), GameData.Destroy() clears mainPlayer but the camera still renders
        ///     one more frame, so vanilla OnCameraPostRender throws a NullReferenceException (reported by ErrorAnalyzer).
        ///     Skip that frame; nothing is rendered by it without a player anyway.
        /// </summary>
        [HarmonyPatch(typeof(SectorModel), nameof(SectorModel.OnCameraPostRender))]
        [HarmonyPrefix]
        public static bool SectorModel_OnCameraPostRender_Prefix()
        {
            Player player = GameMain.mainPlayer;

            return player != null && player.controller != null && player.controller.actionBuild != null;
        }
    }
}
