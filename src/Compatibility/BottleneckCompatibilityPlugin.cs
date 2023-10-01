using System.Reflection;
using BepInEx;
using BepInEx.Bootstrap;
using HarmonyLib;
using ProjectGenesis.Utils;

// ReSharper disable InconsistentNaming
// ReSharper disable MemberCanBeInternal
// ReSharper disable once RedundantAssignment
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable ClassNeverInstantiated.Global

namespace ProjectGenesis.Compatibility
{
    [BepInPlugin(MODGUID, MODNAME, VERSION)]
    [BepInDependency(BottleneckGUID, BepInDependency.DependencyFlags.SoftDependency)]
    public class BottleneckCompatibilityPlugin : BaseUnityPlugin
    {
        public const string MODGUID = "org.LoShin.GenesisBook.Compatibility.Bottleneck";
        public const string MODNAME = "GenesisBook.Compatibility.Bottleneck";
        public const string VERSION = "1.0.0";

        private const string BottleneckGUID = "Bottleneck";

        public void Awake()
        {
            Chainloader.PluginInfos.TryGetValue(BottleneckGUID, out PluginInfo pluginInfo);

            if (pluginInfo == null) return;

            Assembly assembly = pluginInfo.Instance.GetType().Assembly;
            new Harmony(MODGUID).Patch(AccessTools.Method(assembly.GetType("Bottleneck.Stats.ResearchTechHelper"), "GetMaxIncIndex"),
                                       new HarmonyMethod(typeof(BottleneckCompatibilityPlugin), nameof(GetMaxIncIndex_Prefix)));
        }

        public static bool GetMaxIncIndex_Prefix(ref int __result)
        {
            __result = GameMain.history.techStates[ProtoIDUsedByPatches.T物品增产].unlocked ? 4 : 0;
            return false;
        }
    }
}
