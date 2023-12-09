using System.IO;
using BepInEx;
using HarmonyLib;
using xiaoye97;

namespace ProjectGenesis.Patches.Logic
{
    public static class DisableLDBToolCachePatches
    {
        private static bool _finished;

        [HarmonyPrefix]
        [HarmonyPatch(typeof(LDBTool), "Bind")]
        public static bool LDBTool_Bind() => ProjectGenesis.EnableLDBToolCacheEntry.Value;

        [HarmonyPostfix]
        [HarmonyAfter(LDBToolPlugin.MODGUID)]
        [HarmonyPatch(typeof(VFPreload), "InvokeOnLoadWorkEnded")]
        public static void DeleteFiles()
        {
            if (_finished) return;

            if (!ProjectGenesis.EnableLDBToolCacheEntry.Value) return;

            try
            {
                string path = Path.Combine(Paths.ConfigPath, "LDBTool");
                if (Directory.Exists(path)) Directory.Delete(path, true);
            }
            catch
            {
                // ignored
            }

            _finished = true;
        }
    }
}
