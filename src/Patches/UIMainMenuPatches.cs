using System.Text;
using HarmonyLib;
using ProjectGenesis.Patches;
using ProjectGenesis.Utils;

public static class UIMainMenuPatches
{
    [HarmonyPatch(typeof(VFPreload), "InvokeOnLoadWorkEnded")]
    [HarmonyPostfix]
    public static void OnMainMenuOpen()
    {
        var sb = new StringBuilder();

        if (IncompatibleCheckPatch.GalacticScaleInstalled) sb.AppendLine("GalacticScaleInstalled".TranslateFromJson());

        if (IncompatibleCheckPatch.DSPBattleInstalled) sb.AppendLine("DSPBattleInstalled".TranslateFromJson());

        sb.AppendLine("GenesisBookLoadMessage".TranslateFromJson());

        UIMessageBox.Show("GenesisBookLoadTitle".TranslateFromJson(), sb.ToString(), "Ok".TranslateFromJson(), UIMessageBox.INFO);
    }
}
