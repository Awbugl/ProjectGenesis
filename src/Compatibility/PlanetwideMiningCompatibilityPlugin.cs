using System.Collections.Generic;
using System.Reflection;
using BepInEx;
using BepInEx.Bootstrap;
using HarmonyLib;

// ReSharper disable InconsistentNaming
// ReSharper disable MemberCanBeInternal
// ReSharper disable once RedundantAssignment
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable ClassNeverInstantiated.Global

namespace ProjectGenesis.Compatibility
{
    [BepInPlugin(MODGUID, MODNAME, VERSION)]
    [BepInDependency(PlanetwideMiningGUID, BepInDependency.DependencyFlags.SoftDependency)]
    public class PlanetwideMiningCompatibilityPlugin : BaseUnityPlugin
    {
        public const string MODGUID = "org.LoShin.GenesisBook.Compatibility.PlanetwideMining";
        public const string MODNAME = "GenesisBook.Compatibility.PlanetwideMining";
        public const string VERSION = "1.0.0";

        private const string PlanetwideMiningGUID = "930f5bae-66d2-4917-988b-162fe2456643";

        public void Awake()
        {
            Chainloader.PluginInfos.TryGetValue(PlanetwideMiningGUID, out PluginInfo pluginInfo);

            if (pluginInfo == null) return;

            Assembly assembly = pluginInfo.Instance.GetType().Assembly;
           
            ref List<EVeinType> ResourceTypes
                = ref AccessTools.StaticFieldRefAccess<List<EVeinType>>(assembly.GetType("PlanetwideMining.PlanetwideMining"), "ResourceTypes");

            ResourceTypes.Add((EVeinType)15);
            ResourceTypes.Add((EVeinType)16);
            ResourceTypes.Add((EVeinType)17);
        }
    }
}
