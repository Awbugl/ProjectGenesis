using BepInEx;
using BepInEx.Bootstrap;

// ReSharper disable InconsistentNaming
// ReSharper disable MemberCanBeInternal
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable ClassNeverInstantiated.Global

namespace ProjectGenesis.Compatibility
{
    [BepInPlugin(MODGUID, MODNAME, VERSION)]
    [BepInDependency(FastTravelEnablerGUID, BepInDependency.DependencyFlags.SoftDependency)]
    public class FastTravelEnablerCompatibilityPlugin : BaseUnityPlugin
    {
        public const string MODGUID = "org.LoShin.GenesisBook.Compatibility.FastTravelEnabler";
        public const string MODNAME = "GenesisBook.Compatibility.FastTravelEnabler";
        public const string VERSION = "1.0.0";
        
        private const string FastTravelEnablerGUID = "com.hetima.dsp.FastTravelEnabler";

        internal static bool FastTravelEnablerInstalled;

        public void Awake() => FastTravelEnablerInstalled = Chainloader.PluginInfos.TryGetValue(FastTravelEnablerGUID, out _);
    }
}
