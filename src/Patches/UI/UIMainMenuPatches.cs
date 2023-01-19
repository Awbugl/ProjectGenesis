using System.Text;
using HarmonyLib;
using ProjectGenesis.Compatibility;
using ProjectGenesis.Utils;

namespace ProjectGenesis.Patches.UI
{
    public static class UIMainMenuPatches
    {
        private static bool _shown;

        [HarmonyPatch(typeof(VFPreload), "InvokeOnLoadWorkEnded")]
        [HarmonyPostfix]
        public static void OnMainMenuOpen()
        {
            if (_shown) return;

            var sb = new StringBuilder();

            if (IncompatibleCheckPlugin.GalacticScaleInstalled) sb.AppendLine("GalacticScaleInstalled".TranslateFromJson());

            if (IncompatibleCheckPlugin.DSPBattleInstalled) sb.AppendLine("DSPBattleInstalled".TranslateFromJson());

            sb.AppendLine("GenesisBookLoadMessage".TranslateFromJson());

            UIMessageBox.Show("GenesisBookLoadTitle".TranslateFromJson(), sb.ToString(), "Ok".TranslateFromJson(), UIMessageBox.INFO);

            _shown = true;
        }
    }
}
