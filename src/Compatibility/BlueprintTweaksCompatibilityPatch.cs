using BepInEx;

// ReSharper disable InconsistentNaming
// ReSharper disable MemberCanBeInternal
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable ClassNeverInstantiated.Global

namespace ProjectGenesis.Compatibility
{
    [BepInPlugin(MODGUID, MODNAME, VERSION)]
    [BepInDependency(BlueprintTweaksGUID)]
    public class BlueprintTweaksCompatibilityPatch : BaseUnityPlugin
    {
        public const string MODGUID = "org.LoShin.GenesisBook.BlueprintTweaksCompatibilityPatch";
        public const string MODNAME = "GenesisBook.BlueprintTweaksCompatibilityPatch";
        public const string VERSION = "1.0.0";

        private const string BlueprintTweaksGUID = "org.kremnev8.plugin.BlueprintTweaks";

        internal static bool BlueprintTweaksInstalled;

        public void Awake() => BlueprintTweaksInstalled = true;
    }
}
