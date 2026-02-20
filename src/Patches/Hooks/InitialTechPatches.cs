using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using HarmonyLib;
using ProjectGenesis.Utils;
using UnityEngine;

// ReSharper disable InconsistentNaming
// ReSharper disable RemoveRedundantBraces
// ReSharper disable LoopCanBePartlyConvertedToQuery
// ReSharper disable ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator

namespace ProjectGenesis.Patches
{
    public static class InitialTechPatches
    {
        internal static readonly List<int> InitialTechs = new List<int>
                                          {
                                              ProtoID.T戴森球计划,
                                              ProtoID.T科学理论,
                                              ProtoID.T工业化生产,
                                          };

        private static readonly List<int> BonusTechs = new List<int>
                                          {
                                              ProtoID.T电磁学,
                                              ProtoID.T流体储存封装,
                                              ProtoID.T基础机械组件,
                                              ProtoID.T武器系统,
                                              ProtoID.T电磁驱动,
                                              ProtoID.T物品仓储,
                                          };

        [HarmonyPatch(typeof(GameData), nameof(GameData.SetForNewGame))]
        [HarmonyPostfix]
        public static void SetForNewGame(GameData __instance)
        {
            if (DSPGame.LoadDemoIndex > 0 || DSPGame.IsMenuDemo) { return; }

            __instance.history.recipeUnlocked.Remove(5);

            foreach (int tech in InitialTechs.Concat(BonusTechs))
            {
                if (__instance.history.TechUnlocked(tech)) { continue; }

                __instance.history.UnlockTech(tech);
            }
        }

        [HarmonyPatch(typeof(GameData), nameof(GameData.Import))]
        [HarmonyPostfix]
        public static void Import(GameData __instance)
        {
            if (DSPGame.LoadDemoIndex > 0 || DSPGame.IsMenuDemo) { return; }

            foreach (int tech in InitialTechs.Concat(BonusTechs))
            {
                if (__instance.history.TechUnlocked(tech)) { continue; }

                __instance.history.UnlockTech(tech);
            }

            foreach ((int key, TechState value) in __instance.history.techStates)
            {
                if (!value.unlocked) { continue; }

                TechProto techProto = LDB.techs.Select(key);

                if (techProto == null) { continue; }

                foreach (int t in techProto.UnlockRecipes) { __instance.history.UnlockRecipe(t); }
            }
        }
    }
}
