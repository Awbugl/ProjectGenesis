using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using HarmonyLib;
using ProjectGenesis.Utils;

// ReSharper disable InconsistentNaming
// ReSharper disable RemoveRedundantBraces
// ReSharper disable LoopCanBePartlyConvertedToQuery
// ReSharper disable ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator

namespace ProjectGenesis.Patches.Logic
{
    public static class InitialTechPatches
    {
        private static readonly List<int> InitialTechs =
                new List<int> { ProtoID.T戴森球计划, ProtoID.T科学理论, ProtoID.T工业化生产, },
            BonusTechs = new List<int>
            {
                ProtoID.T电磁学,
                ProtoID.T基础机械组件,
                ProtoID.T武器系统,
                ProtoID.T电磁驱动,
                ProtoID.T物品仓储,
            };

        [HarmonyPatch(typeof(GameData), nameof(GameData.SetForNewGame))]
        [HarmonyPostfix]
        public static void SetForNewGame(GameData __instance)
        {
            if (DSPGame.IsMenuDemo)
            {
                return;
            }

            foreach (int tech in InitialTechs.Concat(BonusTechs))
            {
                if (__instance.history.TechUnlocked(tech))
                {
                    continue;
                }

                __instance.history.UnlockTech(tech);
            }
        }

        [HarmonyPatch(typeof(GameData), nameof(GameData.Import))]
        [HarmonyPostfix]
        public static void Import(GameData __instance)
        {
            if (DSPGame.IsMenuDemo)
            {
                return;
            }

            foreach (int tech in InitialTechs.Concat(BonusTechs))
            {
                if (__instance.history.TechUnlocked(tech))
                {
                    continue;
                }

                __instance.history.UnlockTech(tech);
            }

            foreach ((int key, TechState value) in __instance.history.techStates)
            {
                if (!value.unlocked)
                {
                    continue;
                }

                TechProto techProto = LDB.techs.Select(key);

                if (techProto == null)
                {
                    continue;
                }

                foreach (int t in techProto.UnlockRecipes)
                {
                    __instance.history.UnlockRecipe(t);
                }
            }
        }

        [HarmonyPatch(typeof(UITechNode), nameof(UITechNode.DoBuyoutTech))]
        [HarmonyPatch(typeof(UITechNode), nameof(UITechNode.DoStartTech))]
        [HarmonyPatch(typeof(UITechNode), nameof(UITechNode.OnPointerEnter))]
        [HarmonyPatch(typeof(UITechNode), nameof(UITechNode.OnPointerExit))]
        [HarmonyPatch(typeof(UITechNode), nameof(UITechNode.OnPointerDown))]
        [HarmonyPatch(typeof(UITechNode), nameof(UITechNode.OnOtherIconClick))]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> UITechNode_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var matcher = new CodeMatcher(instructions);
            matcher.MatchForward(true, new CodeMatch(OpCodes.Ldarg_0),
                new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(UITechNode), nameof(UITechNode.techProto))),
                new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(Proto), nameof(Proto.ID))),
                new CodeMatch(OpCodes.Ldc_I4_1));

            matcher.SetInstructionAndAdvance(
                Transpilers.EmitDelegate<Func<int, bool>>(id => InitialTechs.Contains(id)));
            matcher.SetOpcodeAndAdvance(OpCodes.Brfalse_S);

            return matcher.InstructionEnumeration();
        }

        [HarmonyPatch(typeof(UITechTree), nameof(UITechTree.OnTechUnlocked))]
        [HarmonyPostfix]
        public static void UITechTree_OnQueueUpdate_Postfix(UITechTree __instance)
        {
            if (!ProjectGenesis.HideTechModeEntry.Value)
            {
                return;
            }

            RefreshNode(__instance);
        }

        [HarmonyPatch(typeof(UITechTree), nameof(UITechTree.OnPageChanged))]
        [HarmonyPostfix]
        public static void UITechTree_OnPageChanged_Postfix(UITechTree __instance)
        {
            if (!ProjectGenesis.HideTechModeEntry.Value)
            {
                return;
            }

            if (__instance.page != 0)
            {
                return;
            }

            RefreshNode(__instance);
        }

        private static void RefreshNode(UITechTree __instance)
        {
            GameHistoryData history = GameMain.history;

            foreach ((int tech, UITechNode node) in __instance.nodes)
            {
                if (tech > 1999 || node == null || node.techProto.IsHiddenTech)
                {
                    continue;
                }

                bool techUnlocked = history.TechUnlocked(tech);
                node.gameObject.SetActive(techUnlocked || node.techProto.PreTechs.Any(history.TechUnlocked));

                if (node.techProto.postTechArray.Length > 0)
                {
                    node.connGroup.gameObject.SetActive(techUnlocked);
                }
            }
        }

        [HarmonyPatch(typeof(UITechNode), nameof(UITechNode.HasPrerequisite))]
        [HarmonyPatch(typeof(UITechNode), nameof(UITechNode.DeterminePrerequisiteSuffice))]
        [HarmonyPrefix]
        public static bool UITechNode_Prerequisite_Prefix(ref bool __result)
        {
            __result = false;
            return false;
        }

        [HarmonyPatch(typeof(UITechNode), nameof(UITechNode.SetTechPrerequisite))]
        [HarmonyPrefix]
        public static bool UITechNode_Prerequisite_Prefix()
        {
            return false;
        }
    }
}