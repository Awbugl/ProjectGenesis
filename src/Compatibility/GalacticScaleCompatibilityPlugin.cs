using BepInEx;
using BepInEx.Bootstrap;

// ReSharper disable CommentTypo
// ReSharper disable InconsistentNaming
// ReSharper disable MemberCanBeInternal
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable ClassNeverInstantiated.Global

namespace ProjectGenesis.Compatibility
{
    [BepInPlugin(MODGUID, MODNAME, VERSION)]
    [BepInDependency(GalacticScaleGUID, BepInDependency.DependencyFlags.SoftDependency)]
    public class GalacticScaleCompatibilityPlugin : BaseUnityPlugin
    {
        public const string MODGUID = "org.LoShin.GenesisBook.Compatibility.GalacticScale";
        public const string MODNAME = "GenesisBook.Compatibility.GalacticScale";
        public const string VERSION = "1.0.0";

        private const string GalacticScaleGUID = "dsp.galactic-scale.2";

        internal static bool GalacticScaleInstalled;

        public void Awake() => GalacticScaleInstalled = Chainloader.PluginInfos.TryGetValue(GalacticScaleGUID, out _);
    }
}
