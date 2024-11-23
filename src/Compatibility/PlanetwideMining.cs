using System.Collections.Generic;
using System.Reflection;
using BepInEx;
using BepInEx.Bootstrap;
using HarmonyLib;

namespace ProjectGenesis.Compatibility
{
    internal static class PlanetwideMining
    {
        internal const string GUID = "930f5bae-66d2-4917-988b-162fe2456643";

        internal static void Awake()
        {
            if (!Chainloader.PluginInfos.TryGetValue(GUID, out PluginInfo pluginInfo)) return;

            Assembly assembly = pluginInfo.Instance.GetType().Assembly;

            ref List<EVeinType> resourceTypes =
                ref AccessTools.StaticFieldRefAccess<List<EVeinType>>(assembly.GetType("PlanetwideMining.PlanetwideMining"),
                    "ResourceTypes");

            resourceTypes.Add((EVeinType)15);
            resourceTypes.Add((EVeinType)16);
            resourceTypes.Add((EVeinType)17);
            resourceTypes.Add((EVeinType)18);
        }
    }
}
