using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using HarmonyLib;
using UnityEngine;

// ReSharper disable InconsistentNaming

namespace ProjectGenesis.Patches
{
    internal static class UITechNodePatches
    {
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
                new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(Proto), nameof(Proto.ID))), new CodeMatch(OpCodes.Ldc_I4_1));

            matcher.SetInstructionAndAdvance(Transpilers.EmitDelegate<Func<int, bool>>(id => InitialTechPatches.InitialTechs.Contains(id)));
            matcher.SetOpcodeAndAdvance(OpCodes.Brfalse_S);

            return matcher.InstructionEnumeration();
        }

        [HarmonyPatch(typeof(UITechTree), nameof(UITechTree.OnTechUnlocked))]
        [HarmonyPostfix]
        public static void UITechTree_OnQueueUpdate_Postfix(UITechTree __instance)
        {
            if (!ProjectGenesis.HideTechModeEntry.Value) return;

            RefreshNode(__instance);
        }

        [HarmonyPatch(typeof(UITechTree), nameof(UITechTree.OnPageChanged))]
        [HarmonyPostfix]
        public static void UITechTree_OnPageChanged_Postfix(UITechTree __instance)
        {
            if (!ProjectGenesis.HideTechModeEntry.Value) return;

            if (__instance.page != 0) return;

            RefreshNode(__instance);
        }

        private static void RefreshNode(UITechTree __instance)
        {
            GameHistoryData history = GameMain.history;

            foreach ((int techId, UITechNode node) in __instance.nodes)
            {
                TechProto tech = node?.techProto;
                if (techId > 1999 || node == null || tech.IsHiddenTech) continue;

                bool techUnlocked = history.TechUnlocked(techId);
                bool active = techUnlocked;
                if (tech.PreTechs.Length > 0)
                    active |= tech.PreTechs.Any(history.TechUnlocked);
                else if (tech.PreTechsImplicit.Length > 0)
                    active |= tech.PreTechsImplicit.Any(history.TechUnlocked);
                else
                    active = true;

                node.gameObject.SetActive(active);

                if (node.techProto.postTechArray.Length > 0) node.connGroup.gameObject.SetActive(techUnlocked);
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
        public static bool UITechNode_Prerequisite_Prefix() => false;

        [HarmonyPatch(typeof(UITechNode), nameof(UITechNode.UpdateLayoutDynamic))]
        [HarmonyPostfix]
        public static void UITechNode_UpdateLayoutDynamic(UITechNode __instance, bool forceUpdate = false, bool forceReset = false)
        {
            float num4 = Mathf.Clamp(
                Mathf.Max(__instance.unlockText.preferredWidth - 40f + __instance.unlockTextTrans.anchoredPosition.x,
                    Math.Min(__instance.techProto.unlockRecipeArray.Length, 3) * 46) + __instance.baseWidth, __instance.minWidth,
                __instance.maxWidth);

            float x = __instance.focusState < 1f
                ? Mathf.Lerp(__instance.minWidth, num4, __instance.focusState)
                : Mathf.Lerp(num4, __instance.maxWidth, __instance.focusState - 1f);

            __instance.panelRect.sizeDelta = new Vector2(x, __instance.panelRect.sizeDelta.y);

            __instance.titleText.rectTransform.sizeDelta =
                new Vector2(x - (GameMain.history.TechState(__instance.techProto.ID).curLevel > 0 ? 65 : 25), 24f);
        }
    }
}
