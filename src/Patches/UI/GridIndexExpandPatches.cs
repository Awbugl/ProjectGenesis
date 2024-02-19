using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using UnityEngine;

// ReSharper disable InconsistentNaming
// ReSharper disable CompareOfFloatsByEqualityOperator
// ReSharper disable LoopCanBePartlyConvertedToQuery
// ReSharper disable ForCanBeConvertedToForeach
// ReSharper disable once CommentTypo
// ReSharper disable PossibleInvalidCastExceptionInForeachLoop

namespace ProjectGenesis.Patches.UI
{
    /// <summary>
    ///     special thanks for https://github.com/appuns/DSPMoreRecipes/blob/main/DSPMoreRecipes.cs
    /// </summary>
    internal static class GridIndexExpandPatches
    {
        [HarmonyPatch(typeof(UIReplicatorWindow), nameof(UIReplicatorWindow._OnInit))]
        [HarmonyPostfix]
        public static void UIReplicatorWindow_OnInit_Postfix(UIReplicatorWindow __instance)
        {
            __instance.windowRect.sizeDelta = new Vector2(900, 811);
            __instance.recipeGroup.sizeDelta = new Vector2(782, 322);
            __instance.queueGroup.GetComponentInChildren<RectTransform>().sizeDelta = new Vector2(782f, 46f);

            __instance.recipeGroup.GetChild(0).GetChild(9).gameObject.SetActive(false);

            Array.Resize(ref __instance.queueNumTexts, 17);

            for (var index = 14; index < 17; ++index) __instance.CreateQueueText(index);
        }

        [HarmonyPatch(typeof(UIGame), nameof(UIGame._OnInit))]
        [HarmonyPostfix]
        [HarmonyPriority(Priority.Last)]
        public static void UIGame_OnInit_Postfix(UIGame __instance)
        {
            __instance.assemblerWindow.recipeGroup.sizeDelta = new Vector2(190, 100);
            __instance.recipePicker.pickerTrans.sizeDelta = new Vector2(830, 476);
            __instance.itemPicker.pickerTrans.sizeDelta = new Vector2(830, 476);
            __instance.signalPicker.pickerTrans.sizeDelta = new Vector2(830, 476);
            __instance.lootFilter.filterTrans.sizeDelta = new Vector2(830, 476);
        }

        [HarmonyPatch(typeof(UIRecipePicker), nameof(UIRecipePicker._OnCreate))]
        [HarmonyPatch(typeof(UISignalPicker), nameof(UISignalPicker._OnCreate))]
        [HarmonyPatch(typeof(UIItemPicker), nameof(UIItemPicker._OnCreate))]
        [HarmonyPatch(typeof(UILootFilter), nameof(UILootFilter._OnCreate))]
        [HarmonyPostfix]
        [HarmonyPriority(Priority.Last)]
        public static void UIRecipePicker_OnOpen_Postfix(ManualBehaviour __instance) =>
            __instance.transform.Find("content").GetComponent<RectTransform>().sizeDelta = new Vector2(782, 322);

        [HarmonyPatch(typeof(UIReplicatorWindow), nameof(UIReplicatorWindow.TestMouseRecipeIndex))]
        [HarmonyPatch(typeof(UIReplicatorWindow), nameof(UIReplicatorWindow.SetSelectedRecipeIndex))]
        [HarmonyPatch(typeof(UIReplicatorWindow), nameof(UIReplicatorWindow.SetSelectedRecipe))]
        [HarmonyPatch(typeof(UIReplicatorWindow), nameof(UIReplicatorWindow._OnUpdate))]
        [HarmonyPatch(typeof(UIReplicatorWindow), nameof(UIReplicatorWindow.RepositionQueueText))]
        [HarmonyPatch(typeof(UIReplicatorWindow), nameof(UIReplicatorWindow.RefreshQueueIcons))]
        [HarmonyPatch(typeof(UIReplicatorWindow), nameof(UIReplicatorWindow.TestMouseQueueIndex))]
        [HarmonyPatch(typeof(UIRecipePicker), nameof(UIRecipePicker._OnUpdate))]
        [HarmonyPatch(typeof(UIRecipePicker), nameof(UIRecipePicker.RefreshIcons))]
        [HarmonyPatch(typeof(UIRecipePicker), nameof(UIRecipePicker.TestMouseIndex))]
        [HarmonyPatch(typeof(UIItemPicker), nameof(UIItemPicker._OnUpdate))]
        [HarmonyPatch(typeof(UIItemPicker), nameof(UIItemPicker.RefreshIcons))]
        [HarmonyPatch(typeof(UIItemPicker), nameof(UIItemPicker.TestMouseIndex))]
        [HarmonyTranspiler]
        [HarmonyPriority(Priority.Last)]
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            foreach (CodeInstruction ci in instructions)
            {
                if (ci.opcode == OpCodes.Ldc_I4_S)
                {
                    if ((sbyte)ci.operand == 14) ci.operand = (sbyte)17;
                }

                if (ci.opcode == OpCodes.Ldc_I4_8) ci.opcode = OpCodes.Ldc_I4_7;

                yield return ci;
            }
        }

        [HarmonyPatch(typeof(UIReplicatorWindow), nameof(UIReplicatorWindow.RefreshRecipeIcons))]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> RefreshRecipeIcons_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var matcher = new CodeMatcher(instructions);

            matcher.MatchForward(false, new CodeMatch(OpCodes.Ldc_I4_8));
            matcher.SetOpcodeAndAdvance(OpCodes.Ldc_I4_7);

            matcher.MatchForward(false, new CodeMatch(OpCodes.Ldc_I4_S, (sbyte)14));
            matcher.SetOperandAndAdvance((sbyte)17);

            matcher.MatchForward(false, new CodeMatch(OpCodes.Ldc_I4_S, (sbyte)14));
            matcher.SetOperandAndAdvance((sbyte)17);

            return matcher.InstructionEnumeration();
        }

        [HarmonyPatch(typeof(UIReplicatorWindow), nameof(UIReplicatorWindow.SetMaterialProps))]
        [HarmonyPatch(typeof(UIRecipePicker), nameof(UIRecipePicker.SetMaterialProps))]
        [HarmonyPatch(typeof(UIItemPicker), nameof(UIItemPicker.SetMaterialProps))]
        [HarmonyTranspiler]
        [HarmonyPriority(Priority.Last)]
        public static IEnumerable<CodeInstruction> SetMaterialProps_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            foreach (CodeInstruction ci in instructions)
            {
                if (ci.opcode == OpCodes.Ldc_R4)
                {
                    var operand = (float)ci.operand;

                    if (operand is 14f) ci.operand = 17f;

                    if (operand is 8f) ci.operand = 7f;
                }

                yield return ci;
            }
        }

        [HarmonyPatch(typeof(UIStationStorage), nameof(UIStationStorage.OnSelectItemButtonClick))]
        [HarmonyTranspiler]
        [HarmonyPriority(Priority.Last)]
        public static IEnumerable<CodeInstruction> UIStationStorage_OnSelectItemButtonClick_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var matcher = new CodeMatcher(instructions);
            matcher.MatchForward(false, new CodeMatch(OpCodes.Ldc_R4, -300f));
            matcher.SetOperandAndAdvance(-600f);

            return matcher.InstructionEnumeration();
        }
    }
}
