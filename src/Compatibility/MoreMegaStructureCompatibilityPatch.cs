using BepInEx;
using HarmonyLib;
using ProjectGenesis.Patches;

// ReSharper disable InconsistentNaming
// ReSharper disable MemberCanBeInternal
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable ClassNeverInstantiated.Global

namespace ProjectGenesis.Compatibility
{
    [BepInPlugin(MODGUID, MODNAME, VERSION)]
    [BepInDependency(MoreMegaStructureGUID)]
    public class MoreMegaStructureCompatibilityPatch : BaseUnityPlugin
    {
        public const string MODGUID = "org.LoShin.GenesisBook.MoreMegaStructureCompatibilityPatch";
        public const string MODNAME = "GenesisBook.MoreMegaStructureCompatibilityPatch";
        public const string VERSION = "1.0.0";

        private const string MoreMegaStructureGUID = "Gnimaerd.DSP.plugin.MoreMegaStructure";

        public void Awake() => Harmony.CreateAndPatchAll(typeof(MoreMegaStructureEditDataPatches));
    }
}
