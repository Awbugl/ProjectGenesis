using BepInEx;

// ReSharper disable InconsistentNaming
// ReSharper disable MemberCanBeInternal
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable ClassNeverInstantiated.Global

namespace ProjectGenesis.Compatibility
{
    [BepInPlugin(MODGUID, MODNAME, VERSION)]
    [BepInDependency(BlueprintTweaksGUID)]
    public class BlueprintTweaksCompatibilityPlugin : BaseUnityPlugin
    {
        public const string MODGUID = "org.LoShin.GenesisBook.Compatibility.BlueprintTweaks";
        public const string MODNAME = "GenesisBook.Compatibility.BlueprintTweaks";
        public const string VERSION = "1.0.0";

        private const string BlueprintTweaksGUID = "org.kremnev8.plugin.BlueprintTweaks";

        internal static bool BlueprintTweaksInstalled;

        public void Awake() => BlueprintTweaksInstalled = true;
    }
}
