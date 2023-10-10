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
    [BepInDependency(DSPBattleGUID, BepInDependency.DependencyFlags.SoftDependency)]
    public class DSPBattleCompatibilityPlugin : BaseUnityPlugin
    {
        public const string MODGUID = "org.LoShin.GenesisBook.Compatibility.DSPBattle";
        public const string MODNAME = "GenesisBook.Compatibility.DSPBattle";
        public const string VERSION = "1.0.0";

        private const string DSPBattleGUID = "com.ckcz123.DSP_Battle";

        internal static bool DSPBattleInstalled;

        public void Awake() => DSPBattleInstalled = Chainloader.PluginInfos.TryGetValue(DSPBattleGUID, out _);
    }
}
