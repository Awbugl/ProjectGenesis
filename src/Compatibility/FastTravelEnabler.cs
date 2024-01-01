using BepInEx.Bootstrap;

namespace ProjectGenesis.Compatibility
{
    internal static class FastTravelEnabler
    {
        internal const string GUID = "com.hetima.dsp.FastTravelEnabler";

        internal static bool Installed;

        internal static void Awake() => Installed = Chainloader.PluginInfos.TryGetValue(GUID, out _);
    }
}
