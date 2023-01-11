using System.IO;
using BepInEx;
using HarmonyLib;

namespace ProjectGenesis.Patches
{
    public static class DeleteLDBConfigPatches
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(DSPGame), "Awake")]
        public static void DSPGame_Awake()
        {
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
