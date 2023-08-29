using System.Collections.Generic;
using System.Text;
using BepInEx;
using BepInEx.Bootstrap;
using HarmonyLib;
using ProjectGenesis.Patches.UI;
using ProjectGenesis.Utils;
using UnityEngine;

namespace ProjectGenesis.Compatibility
{
    /// <summary>
    ///     special thanks for https://github.com/kremnev8/DSP-Mods/blob/master/Mods/BlueprintTweaks/InstallationChecker.cs
    /// </summary>

    // ReSharper disable InconsistentNaming
    // ReSharper disable MemberCanBeInternal
    // ReSharper disable MemberCanBePrivate.Global
    // ReSharper disable ClassNeverInstantiated.Global
    [BepInPlugin(MODGUID, MODNAME, VERSION)]
    [BepInDependency(DSPBattleGUID, BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency(BlueprintTweaksCompatibilityPlugin.MODGUID, BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency(BottleneckCompatibilityPlugin.MODGUID, BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency(MoreMegaStructureCompatibilityPlugin.MODGUID, BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency(GalacticScaleCompatibilityPlugin.MODGUID, BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency(PlanetwideMiningCompatibilityPlugin.MODGUID, BepInDependency.DependencyFlags.SoftDependency)]
    public class IncompatibleCheckPlugin : BaseUnityPlugin
    {
        public const string MODGUID = "org.LoShin.GenesisBook.Compatibility.Check";
        public const string MODNAME = "GenesisBook.Compatibility.Check";
        public const string VERSION = "1.0.0";

        private const string DSPBattleGUID = "com.ckcz123.DSP_Battle";

        internal static bool DSPBattleInstalled;

        public void Awake()
        {
            BepInEx.Logging.Logger.Listeners.Add(new HarmonyLogListener());

            Dictionary<string, PluginInfo> pluginInfos = Chainloader.PluginInfos;
            DSPBattleInstalled = pluginInfos.ContainsKey(DSPBattleGUID);

            new Harmony(MODGUID).Patch(AccessTools.Method(typeof(VFPreload), "InvokeOnLoadWorkEnded"), null,
                                       new HarmonyMethod(typeof(IncompatibleCheckPlugin), nameof(OnMainMenuOpen)) { priority = Priority.Last });
        }

        private static bool _shown;

        public static void OnMainMenuOpen()
        {
            if (_shown) return;
            _shown = true;

            if (!ProjectGenesis.ShowMessageBoxValue) return;

            var sb = new StringBuilder();

            if (DSPBattleInstalled) sb.AppendLine("DSPBattleInstalled".TranslateFromJson());

            sb.AppendLine("GenesisBookLoadMessage".TranslateFromJson());

            UIMessageBox.Show("GenesisBookLoadTitle".TranslateFromJson(), sb.ToString(), "确定".TranslateFromJson(), "跳转交流群".TranslateFromJson(),
                              "跳转日志".TranslateFromJson(), UIMessageBox.INFO, null, OpenBrowser, OpenLog);
        }

        public static void OpenBrowser() => Application.OpenURL("创世之书链接".TranslateFromJson());

        public static void OpenLog() => Application.OpenURL(System.IO.Path.Combine(ProjectGenesis.ModPath, "CHANGELOG.md"));
    }
}
