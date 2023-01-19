using System.IO;
using BepInEx;
using HarmonyLib;

namespace ProjectGenesis.Patches.Logic
{
    public static class DeleteLDBConfig
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(DSPGame), "Awake")]
        [HarmonyPatch(typeof(GameMain), "OnDestroy")]
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
