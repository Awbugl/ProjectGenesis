using BepInEx.Bootstrap;

namespace ProjectGenesis.Compatibility
{
    internal static class DSPBattle
    {
        internal const string GUID = "com.ckcz123.DSP_Battle";

        internal static bool Installed;

        internal static void Awake() => Installed = Chainloader.PluginInfos.TryGetValue(GUID, out _);
    }
}
