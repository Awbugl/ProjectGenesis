using System.Reflection;
using BepInEx;
using BepInEx.Bootstrap;
using HarmonyLib;
using ProjectGenesis.Utils;

// ReSharper disable InconsistentNaming
// ReSharper disable once RedundantAssignment

namespace ProjectGenesis.Compatibility
{
    internal static class Bottleneck
    {
        internal const string GUID = "Bottleneck";

        internal static void Awake()
        {
            Chainloader.PluginInfos.TryGetValue(GUID, out PluginInfo pluginInfo);

            if (pluginInfo == null) return;

            Assembly assembly = pluginInfo.Instance.GetType().Assembly;
            new Harmony("org.LoShin.GenesisBook.Compatibility.Bottleneck").Patch(
                AccessTools.Method(assembly.GetType("Bottleneck.Stats.ResearchTechHelper"), "GetMaxIncIndex"),
                new HarmonyMethod(typeof(Bottleneck), nameof(GetMaxIncIndex_Prefix)));
        }

        public static bool GetMaxIncIndex_Prefix(ref int __result)
        {
            __result = GameMain.history.techStates[ProtoID.T物品增产].unlocked ? 4 : 0;
            return false;
        }
    }
}
