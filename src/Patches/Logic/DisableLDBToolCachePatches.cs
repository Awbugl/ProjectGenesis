using System.IO;
using BepInEx;
using HarmonyLib;
using xiaoye97;

namespace ProjectGenesis.Patches.Logic
{
    public static class DisableLDBToolCachePatches
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(LDBTool), "Bind")]
        public static bool LDBTool_Bind() => ProjectGenesis.LDBToolCacheValue;

        [HarmonyPostfix]
        [HarmonyAfter(LDBToolPlugin.MODGUID)]
        [HarmonyPatch(typeof(VFPreload), "InvokeOnLoadWorkEnded")]
        public static void DeleteFiles()
        {
            if (!ProjectGenesis.LDBToolCacheValue) return;

            try
            {
                var path = Path.Combine(Paths.ConfigPath, "LDBTool");
                if (Directory.Exists(path)) Directory.Delete(path, true);
            }
            catch
            {
                // ignored
            }
        }
    }
}
