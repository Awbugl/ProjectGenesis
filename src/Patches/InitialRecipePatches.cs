using System;
using System.Collections.Generic;
using HarmonyLib;

// ReSharper disable InconsistentNaming

namespace ProjectGenesis.Patches
{
    public static class InitialRecipePatches
    {
        private static Dictionary<int, RecipeExecuteData> _originRecipeExecuteData, _modifiedRecipeExecuteData;
        private static bool _originInitialized, _modifiedInitialized;

        [HarmonyPatch(typeof(GameMain), nameof(GameMain.Start))]
        [HarmonyPrefix]
        public static void SetRecipeForGame()
        {
            if (DSPGame.LoadDemoIndex > 0 || DSPGame.IsMenuDemo)
            {
                if (_originInitialized)
                {
                    RecipeProto.recipeExecuteData = _originRecipeExecuteData;
                }
                else
                {
                    _originRecipeExecuteData = RecipeProto.recipeExecuteData ?? throw new ArgumentNullException();
                    _originInitialized = true;
                }
            }
            else
            {
                if (_modifiedInitialized)
                {
                    RecipeProto.recipeExecuteData = _modifiedRecipeExecuteData;
                }
                else
                {
                    RecipeProto.InitRecipeItems();
                    _modifiedRecipeExecuteData = RecipeProto.recipeExecuteData;
                    _modifiedInitialized = true;
                }
            }
        }
    }
}
