using System;
using System.Collections.Generic;
using HarmonyLib;

// ReSharper disable InconsistentNaming

namespace ProjectGenesis.Patches
{
    public static class InitialRecipePatches
    {
        private static Dictionary<int, RecipeExecuteData> _originRecipeExecuteData;

        private static bool _initialized;

        [HarmonyPatch(typeof(GameData), nameof(GameData.SetForNewGame))]
        [HarmonyPrefix]
        public static void SetRecipeForNewGame()
        {
            if (DSPGame.LoadDemoIndex > 0 || DSPGame.IsMenuDemo) return;

            RecipeProto.InitRecipeItems();
        }

        [HarmonyPatch(typeof(GameSave), nameof(GameSave.LoadCurrentGameInResource))]
        [HarmonyPrefix]
        public static void LoadCurrentGameInResource()
        {
            if (DSPGame.LoadDemoIndex > 0 || DSPGame.IsMenuDemo)
            {
                if (_initialized)
                    RecipeProto.recipeExecuteData = _originRecipeExecuteData;
                else
                    SaveOriginRecipeExecuteData();
            }
        }

        private static void SaveOriginRecipeExecuteData()
        {
            if (_initialized) return;

            _originRecipeExecuteData = RecipeProto.recipeExecuteData ?? throw new ArgumentNullException();
            _initialized = true;
        }
    }
}
