using System.IO;
using BepInEx;
using HarmonyLib;
using xiaoye97;

namespace ProjectGenesis.Patches.Logic
{
    public static class DisableLDBToolCachePatches
    {
        private static bool _finished;

        [HarmonyPatch(typeof(LDBTool), "Bind")]
        [HarmonyPriority(Priority.VeryHigh)]
        [HarmonyPrefix]
        public static bool LDBTool_Bind() => ProjectGenesis.LDBToolCacheEntry.Value;

        [HarmonyPatch(typeof(VFPreload), nameof(VFPreload.InvokeOnLoadWorkEnded))]
        [HarmonyAfter(LDBToolPlugin.MODGUID)]
        [HarmonyPostfix]
        public static void DeleteFiles()
        {
            if (_finished) return;

            if (!ProjectGenesis.LDBToolCacheEntry.Value) return;

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
