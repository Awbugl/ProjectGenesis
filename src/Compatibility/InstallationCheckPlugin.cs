using System;
using System.IO;
using System.Reflection;
using System.Text;
using BepInEx;
using BepInEx.Bootstrap;
using HarmonyLib;
using ProjectGenesis.Compatibility;
using ProjectGenesis.Patches.UI;
using ProjectGenesis.Utils;
using UnityEngine;

namespace ProjectGenesis
{
    /// <summary>
    ///     special thanks for https://github.com/kremnev8/DSP-Mods/blob/master/Mods/BlueprintTweaks/InstallationChecker.cs
    /// </summary>

    // ReSharper disable InconsistentNaming
    // ReSharper disable MemberCanBeInternal
    // ReSharper disable MemberCanBePrivate.Global
    // ReSharper disable ClassNeverInstantiated.Global
    [BepInPlugin(MODGUID, MODNAME, VERSION)]
    [BepInDependency(DSPBattleCompatibilityPlugin.MODGUID, BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency(BlueprintTweaksCompatibilityPlugin.MODGUID, BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency(BottleneckCompatibilityPlugin.MODGUID, BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency(MoreMegaStructureCompatibilityPlugin.MODGUID, BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency(GalacticScaleCompatibilityPlugin.MODGUID, BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency(PlanetwideMiningCompatibilityPlugin.MODGUID, BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency(PlanetVeinUtilizationCompatibilityPlugin.MODGUID, BepInDependency.DependencyFlags.SoftDependency)]
    public class InstallationCheckPlugin : BaseUnityPlugin
    {
        public const string MODGUID = "org.LoShin.GenesisBook.InstallationCheck";
        public const string MODNAME = "GenesisBook.InstallationCheck";
        public const string VERSION = "1.0.0";

        private static bool _shown;

        private static bool PreloaderInstalled;

        public void Awake()
        {
            BepInEx.Logging.Logger.Listeners.Add(new HarmonyLogListener());

            FieldInfo birthResourcePoint2 = AccessTools.DeclaredField(typeof(PlanetData), nameof(PlanetData.birthResourcePoint2));
            PreloaderInstalled = birthResourcePoint2 != null;

            new Harmony(MODGUID).Patch(AccessTools.Method(typeof(VFPreload), "InvokeOnLoadWorkEnded"), null,
                                       new HarmonyMethod(typeof(InstallationCheckPlugin), nameof(OnMainMenuOpen)) { priority = Priority.Last });
        }

        public static void OnMainMenuOpen()
        {
            if (_shown) return;
            _shown = true;

            string msg = string.Empty;

            if (ProjectGenesis.ShowMessageBoxValue) msg = "GenesisBookLoadMessage";

            if (DSPBattleCompatibilityPlugin.DSPBattleInstalled) msg = "DSPBattleInstalled";

            if (!ProjectGenesis.LoadCompleted) msg = "ProjectGenesisNotLoaded";

            if (!PreloaderInstalled) msg = "PreloaderNotInstalled";

            if (!string.IsNullOrEmpty(msg))
                UIMessageBox.Show("GenesisBookLoadTitle".TranslateFromJson(), msg.TranslateFromJson(), "确定".TranslateFromJson(),
                                  "跳转交流群".TranslateFromJson(), "跳转日志".TranslateFromJson(), UIMessageBox.INFO, null, OpenBrowser, OpenLog);
        }

        public static void OpenBrowser() => Application.OpenURL("创世之书链接".TranslateFromJson());

        public static void OpenLog() => Application.OpenURL(Path.Combine(ProjectGenesis.ModPath, "CHANGELOG.md"));
    }
}
