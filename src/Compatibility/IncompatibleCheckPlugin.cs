using System.Collections.Generic;
using BepInEx;
using HarmonyLib;
using ProjectGenesis.Compatibility.BlueprintTweaks;
using ProjectGenesis.Compatibility.Bottleneck;
using ProjectGenesis.Compatibility.MoreMegaStructure;
using ProjectGenesis.Patches.UI;

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
    [BepInDependency(GalacticScaleGUID, BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency(DSPBattleGUID, BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency(BlueprintTweaksCompatibilityPlugin.MODGUID, BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency(BottleneckCompatibilityPlugin.MODGUID, BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency(MoreMegaStructureCompatibilityPlugin.MODGUID, BepInDependency.DependencyFlags.SoftDependency)]
    public class IncompatibleCheckPlugin : BaseUnityPlugin
    {
        public const string MODGUID = "org.LoShin.GenesisBook.IncompatibleCheckPatch";
        public const string MODNAME = "GenesisBook.IncompatibleCheckPatch";
        public const string VERSION = "1.0.0";

        private const string GalacticScaleGUID = "dsp.galactic-scale.2", DSPBattleGUID = "com.ckcz123.DSP_Battle";

        internal static bool GalacticScaleInstalled, DSPBattleInstalled;

        public void Awake()
        {
            Dictionary<string, PluginInfo> pluginInfos = BepInEx.Bootstrap.Chainloader.PluginInfos;
            GalacticScaleInstalled = pluginInfos.ContainsKey(GalacticScaleGUID);
            DSPBattleInstalled = pluginInfos.ContainsKey(DSPBattleGUID);
            Harmony.CreateAndPatchAll(typeof(UIMainMenuPatches));
        }
    }
}
