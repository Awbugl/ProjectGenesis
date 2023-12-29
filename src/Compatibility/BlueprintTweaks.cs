using BepInEx.Bootstrap;

namespace ProjectGenesis.Compatibility
{
    internal static class BlueprintTweaks
    {
        internal const string GUID = "org.kremnev8.plugin.BlueprintTweaks";

        internal static bool Installed;

        internal static void Awake() => Installed = Chainloader.PluginInfos.TryGetValue(GUID, out _);
    }
}
