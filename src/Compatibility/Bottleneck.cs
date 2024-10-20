using System.Reflection;
using BepInEx;
using BepInEx.Bootstrap;
using HarmonyLib;
using ProjectGenesis.Utils;

// ReSharper disable InconsistentNaming

namespace ProjectGenesis.Compatibility
{
    internal static class Bottleneck
    {
        internal const string GUID = "Bottleneck";

        private static readonly Harmony HarmonyPatch = new Harmony("ProjectGenesis.Compatibility." + GUID);

        internal static void Awake()
        {
            if (!Chainloader.PluginInfos.TryGetValue(GUID, out PluginInfo pluginInfo)) return;

            Assembly assembly = pluginInfo.Instance.GetType().Assembly;
            HarmonyPatch.Patch(AccessTools.Method(assembly.GetType("Bottleneck.Stats.ResearchTechHelper"), "GetMaxIncIndex"),
                new HarmonyMethod(typeof(Bottleneck), nameof(GetMaxIncIndex_Prefix)));
        }

        public static bool GetMaxIncIndex_Prefix(ref int __result)
        {
            __result = GameMain.history.techStates[ProtoID.T物品增产].unlocked ? 4 : 0;

            return false;
        }
    }
}
