using BepInEx.Configuration;
using HarmonyLib;
using xiaoye97;

namespace ProjectGenesis.Utils
{
    internal static class LDBToolCacheUtils
    {
        internal static void Clear()
        {
            ClearConfigFile("CustomID");
            ClearConfigFile("CustomGridIndex");
            ClearConfigFile("CustomStringZHCN");
            ClearConfigFile("CustomStringENUS");
            ClearConfigFile("CustomStringFRFR");
        }

        private static void ClearConfigFile(string name) => AccessTools.StaticFieldRefAccess<ConfigFile>(typeof(LDBTool), name).Clear();
    }
}
