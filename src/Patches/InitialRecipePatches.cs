using HarmonyLib;

// ReSharper disable InconsistentNaming

namespace ProjectGenesis.Patches
{
    public static class InitialRecipePatches
    {
        [HarmonyPatch(typeof(GameData), nameof(GameData.SetForNewGame))]
        [HarmonyPrefix]
        public static void SetRecipeForNewGame(GameData __instance)
        {
            if (DSPGame.LoadDemoIndex > 0 || DSPGame.IsMenuDemo) return;

            RecipeProto.InitRecipeItems();
        }
    }
}
