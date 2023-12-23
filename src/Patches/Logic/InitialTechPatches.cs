using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using HarmonyLib;
using ProjectGenesis.Utils;

// ReSharper disable InconsistentNaming
// ReSharper disable RemoveRedundantBraces
// ReSharper disable ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator

namespace ProjectGenesis.Patches.Logic
{
    public static class InitialTechPatches
    {
        private static readonly List<int> InitialTechs = new List<int> { ProtoID.T戴森球计划, ProtoID.T科学理论, ProtoID.T工业化生产 },
                                          BonusTechs = new List<int> { ProtoID.T电磁学, ProtoID.T基础机械组件, ProtoID.T武器系统, ProtoID.T电磁驱动 };

        [HarmonyPatch(typeof(GameData), "SetForNewGame")]
        [HarmonyPostfix]
        public static void SetForNewGame(GameData __instance)
        {
            if (DSPGame.IsMenuDemo) return;

            foreach (int tech in InitialTechs)
            {
                if (!__instance.history.TechUnlocked(tech)) __instance.history.UnlockTech(tech);
            }

            foreach (int tech in BonusTechs)
            {
                if (!__instance.history.TechUnlocked(tech)) __instance.history.UnlockTech(tech);
            }
        }

        [HarmonyPatch(typeof(GameData), "Import")]
        [HarmonyPostfix]
        public static void Import(GameData __instance)
        {
            if (DSPGame.IsMenuDemo) return;

            foreach (int tech in InitialTechs)
            {
                if (!__instance.history.TechUnlocked(tech)) __instance.history.UnlockTech(tech);
            }

            foreach (int tech in BonusTechs)
            {
                if (!__instance.history.TechUnlocked(tech)) __instance.history.UnlockTech(tech);
            }

            foreach (var (key, value) in __instance.history.techStates)
            {
                if (!value.unlocked) continue;

                TechProto techProto = LDB.techs.Select(key);
                if (techProto == null) continue;

                foreach (int t in techProto.UnlockRecipes)
                {
                    __instance.history.UnlockRecipe(t);
                }
            }
        }

        [HarmonyPatch(typeof(UITechNode), "DoBuyoutTech")]
        [HarmonyPatch(typeof(UITechNode), "DoStartTech")]
        [HarmonyPatch(typeof(UITechNode), "OnPointerEnter")]
        [HarmonyPatch(typeof(UITechNode), "OnPointerExit")]
        [HarmonyPatch(typeof(UITechNode), "OnPointerDown")]
        [HarmonyPatch(typeof(UITechNode), "OnOtherIconClick")]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> UITechNode_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var matcher = new CodeMatcher(instructions);
            matcher.MatchForward(true, new CodeMatch(OpCodes.Ldarg_0),
                                 new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(UITechNode), nameof(UITechNode.techProto))),
                                 new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(Proto), nameof(Proto.ID))), new CodeMatch(OpCodes.Ldc_I4_1));

            matcher.SetInstructionAndAdvance(Transpilers.EmitDelegate<Func<int, bool>>(id => InitialTechs.Contains(id)));
            matcher.SetOpcodeAndAdvance(OpCodes.Brfalse_S);
            return matcher.InstructionEnumeration();
        }

        [HarmonyPatch(typeof(UITechTree), "OnTechUnlocked")]
        [HarmonyPostfix]
        public static void UITechTree_OnQueueUpdate_Postfix(UITechTree __instance)
        {
            if (!ProjectGenesis.EnableHideTechModeEntry.Value) return;

            GameHistoryData history = GameMain.history;
            foreach (var (tech, node) in __instance.nodes)
            {
                if (node == null || tech > 1999 || node.techProto.IsHiddenTech) continue;

                bool techUnlocked = TechUnlocked(history, tech);
                node.gameObject.SetActive(techUnlocked || AnyPreTechUnlocked(history, node.techProto));
                if (node.techProto.postTechArray.Length > 0) node.connGroup.gameObject.SetActive(techUnlocked);
            }
        }

        [HarmonyPatch(typeof(UITechTree), "OnPageChanged")]
        [HarmonyPostfix]
        public static void UITechTree_OnPageChanged_Postfix(UITechTree __instance)
        {
            if (!ProjectGenesis.EnableHideTechModeEntry.Value) return;

            if (__instance.page != 0) return;

            GameHistoryData history = GameMain.history;

            foreach (var (tech, node) in __instance.nodes)
            {
                if (node == null || tech > 1999 || node.techProto.IsHiddenTech) continue;

                bool techUnlocked = TechUnlocked(history, tech);
                node.gameObject.SetActive(techUnlocked || AnyPreTechUnlocked(history, node.techProto));
                if (node.techProto.postTechArray.Length > 0) node.connGroup.gameObject.SetActive(techUnlocked);
            }
        }

        private static bool TechUnlocked(GameHistoryData history, int tech) => history.TechUnlocked(tech);

        private static bool AnyPreTechUnlocked(GameHistoryData history, TechProto proto) => proto.PreTechs.Any(i => TechUnlocked(history, i));
    }
}
