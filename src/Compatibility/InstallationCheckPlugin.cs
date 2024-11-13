using System.IO;
using System.Reflection;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using ProjectGenesis.Patches.UI;
using ProjectGenesis.Utils;
using UnityEngine;

// ReSharper disable InconsistentNaming
// ReSharper disable MemberCanBeInternal
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable ClassNeverInstantiated.Global

namespace ProjectGenesis.Compatibility
{
    /// <summary>
    ///     special thanks for https://github.com/kremnev8/DSP-Mods/blob/master/Mods/BlueprintTweaks/InstallationChecker.cs
    /// </summary>
    [BepInPlugin(MODGUID, MODNAME, ProjectGenesis.VERSION)]
    [BepInDependency(BlueprintTweaks.GUID, BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency(Bottleneck.GUID, BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency(MoreMegaStructure.GUID, BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency(GalacticScale.GUID, BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency(PlanetwideMining.GUID, BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency(PlanetVeinUtilization.GUID, BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency(FastTravelEnabler.GUID, BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency(GigaStationsUpdated.GUID, BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency(LazyOutposting.GUID, BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency(WeaponPlus.GUID, BepInDependency.DependencyFlags.SoftDependency)]
    public class InstallationCheckPlugin : BaseUnityPlugin
    {
        public const string MODGUID = "org.LoShin.GenesisBook.InstallationCheck";
        public const string MODNAME = "GenesisBook.InstallationCheck";
        public const string PreferBepinExVersion = "5.4.17";
        private static bool MessageShown, PreloaderInstalled, BepinExVersionMatch;

        internal static ManualLogSource logger;

        public void Awake()
        {
            logger = Logger;

            BepInEx.Logging.Logger.Listeners.Add(new HarmonyLogListener());

            var currentVersion = typeof(Paths).Assembly.GetName().Version.ToString(3);

            BepinExVersionMatch = currentVersion == PreferBepinExVersion;

            FieldInfo birthResourcePoint2 = AccessTools.DeclaredField(typeof(PlanetData), nameof(PlanetData.birthResourcePoint2));
            PreloaderInstalled = birthResourcePoint2 != null;

            new Harmony(MODGUID).Patch(AccessTools.Method(typeof(VFPreload), nameof(VFPreload.InvokeOnLoadWorkEnded)), null,
                new HarmonyMethod(typeof(InstallationCheckPlugin), nameof(OnMainMenuOpen)) { priority = Priority.Last, });

            AwakeCompatibilityPatchers();
        }

        public static void AwakeCompatibilityPatchers()
        {
            MoreMegaStructure.Awake();
            PlanetVeinUtilization.Awake();
            BlueprintTweaks.Awake();
            Bottleneck.Awake();
            PlanetwideMining.Awake();
            FastTravelEnabler.Awake();
            GigaStationsUpdated.Awake();
            LazyOutposting.Awake();
            WeaponPlus.Awake();

            try { GalacticScale.Awake(); }
            catch (FileNotFoundException)
            {
                // ignore
            }
        }

        public static void OnMainMenuOpen()
        {
            if (MessageShown) return;

            MessageShown = true;

            string msg = null;

            if (ProjectGenesis.ShowMessageBoxEntry.Value) msg = "GenesisBookLoadMessage";

            if (!ProjectGenesis.LoadCompleted) msg = "ProjectGenesisNotLoaded";

            if (!BepinExVersionMatch) msg = "BepinExVersionNotMatch";

            if (!PreloaderInstalled) msg = "PreloaderNotInstalled";

            if (string.IsNullOrEmpty(msg)) return;

            UIMessageBox.Show("GenesisBookLoadTitle".TranslateFromJson(), msg.TranslateFromJson(), "确定".TranslateFromJson(),
                "跳转交流群".TranslateFromJson(), "跳转日志".TranslateFromJson(), UIMessageBox.INFO, null, OpenBrowser, OpenLog);
        }

        public static void OpenBrowser() => Application.OpenURL("创世之书链接".TranslateFromJson());

        public static void OpenLog() => Application.OpenURL(Path.Combine(ProjectGenesis.ModPath, "CHANGELOG.md"));
    }
}
