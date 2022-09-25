using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

// ReSharper disable InconsistentNaming
// ReSharper disable LoopCanBePartlyConvertedToQuery
// ReSharper disable ForCanBeConvertedToForeach
// ReSharper disable once CommentTypo

namespace ProjectGenesis.Patches
{
    /// <summary>
    /// from https: //github.com/appuns/DSPMoreRecipes/blob/main/DSPMoreRecipes.cs
    /// </summary>
    public static class GridIndexExpandPatches
    {
        private static bool _itemresized, _reciperesized, _signalresized, _resized2;

        private static ref TU FieldRefAccess<T, TU>(T instance, string fieldName) => ref FieldRefAccess<T, TU>(fieldName)(instance);

        private static AccessTools.FieldRef<T, TU> FieldRefAccess<T, TU>(string fieldName)
            => AccessTools.FieldRefAccess<T, TU>(AccessTools.Field(typeof(T), fieldName));

        [HarmonyPatch(typeof(UIReplicatorWindow), "_OnInit")]
        [HarmonyPrefix]
        public static void UIReplicatorWindow_OnInit_Prefix()
        {
            ref Text[] local = ref AccessTools.FieldRefAccess<UIReplicatorWindow, Text[]>(UIRoot.instance.uiGame.replicator, "queueNumTexts");
            Array.Resize(ref local, 17);
        }

        [HarmonyPatch(typeof(VFPreload), "InvokeOnLoadWorkEnded")]
        [HarmonyPostfix]
        [HarmonyPriority(1)]
        public static void VFPreload_InvokeOnLoadWorkEnded_Postfix()
        {
            if (!_resized2)
            {
                ref var local1 = ref FieldRefAccess<UIReplicatorWindow, RectTransform>(UIRoot.instance.uiGame.replicator, "windowRect");
                local1.sizeDelta = new Vector2(local1.sizeDelta.x + 230f, local1.sizeDelta.y);
                ref var local2 = ref FieldRefAccess<UIReplicatorWindow, RectTransform>(UIRoot.instance.uiGame.replicator, "recipeGroup");
                local2.sizeDelta = new Vector2(local2.sizeDelta.x + 230f, local2.sizeDelta.y);
                ref var local3 = ref FieldRefAccess<UIAssemblerWindow, RectTransform>(UIRoot.instance.uiGame.assemblerWindow, "recipeGroup");
                local3.sizeDelta = new Vector2(local3.sizeDelta.x + 230f, local3.sizeDelta.y);
                ref var local4 = ref FieldRefAccess<UIRecipePicker, RectTransform>(UIRoot.instance.uiGame.recipePicker, "pickerTrans");
                local4.sizeDelta = new Vector2(local4.sizeDelta.x + 230f, local4.sizeDelta.y);
                ref var local5 = ref FieldRefAccess<UIItemPicker, RectTransform>(UIRoot.instance.uiGame.itemPicker, "pickerTrans");
                local5.sizeDelta = new Vector2(local5.sizeDelta.x + 230f, local5.sizeDelta.y);
                ref var local6 = ref FieldRefAccess<UISignalPicker, RectTransform>(UIRoot.instance.uiGame.signalPicker, "pickerTrans");
                local6.sizeDelta = new Vector2(local6.sizeDelta.x + 230f, local6.sizeDelta.y);

                GameObject.Find("UI Root/Overlay Canvas/In Game/Windows/Replicator Window/queue-group").GetComponentInChildren<RectTransform>()
                          .sizeDelta = new Vector2(782f, 46f);
                _resized2 = true;
            }
        }

        [HarmonyPatch(typeof(UIRecipePicker), "_OnOpen")]
        [HarmonyPostfix]
        public static void UIRecipePicker_OnOpen_Postfix()
        {
            if (_reciperesized) return;
            // ReSharper disable once PossibleInvalidCastExceptionInForeachLoop
            foreach (GameObject gameObject in Object.FindObjectsOfType(typeof(GameObject)))
            {
                if (gameObject.name.Contains("Recipe"))
                    foreach (Transform transform in gameObject.transform)
                    {
                        if (transform.name.Contains("content"))
                        {
                            transform.GetComponent<RectTransform>().sizeDelta
                                = new Vector2(transform.GetComponent<RectTransform>().sizeDelta.x + 230f,
                                              transform.GetComponent<RectTransform>().sizeDelta.y);
                            _reciperesized = true;
                        }
                    }
            }
        }

        [HarmonyPatch(typeof(UIItemPicker), "_OnOpen")]
        [HarmonyPostfix]
        public static void UIItemPicker_OnOpen_Postfix()
        {
            if (_itemresized) return;
            // ReSharper disable once PossibleInvalidCastExceptionInForeachLoop
            foreach (GameObject gameObject in Object.FindObjectsOfType(typeof(GameObject)))
            {
                if (gameObject.name.Contains("Item"))
                    foreach (Transform transform in gameObject.transform)
                    {
                        if (transform.name.Contains("content"))
                        {
                            transform.GetComponent<RectTransform>().sizeDelta
                                = new Vector2(transform.GetComponent<RectTransform>().sizeDelta.x + 230f,
                                              transform.GetComponent<RectTransform>().sizeDelta.y);
                            _itemresized = true;
                        }
                    }
            }
        }

        [HarmonyPatch(typeof(UISignalPicker), "_OnOpen")]
        [HarmonyPostfix]
        public static void UISignalPicker_OnOpen_Postfix()
        {
            if (_signalresized) return;
            // ReSharper disable once PossibleInvalidCastExceptionInForeachLoop
            foreach (GameObject gameObject in Object.FindObjectsOfType(typeof(GameObject)))
            {
                if (gameObject.name.Contains("Signal"))
                    foreach (Transform transform in gameObject.transform)
                    {
                        if (transform.name.Contains("content"))
                        {
                            transform.GetComponent<RectTransform>().sizeDelta
                                = new Vector2(transform.GetComponent<RectTransform>().sizeDelta.x + 230f,
                                              transform.GetComponent<RectTransform>().sizeDelta.y);
                            _signalresized = true;
                        }
                    }
            }
        }

        [HarmonyPatch(typeof(UIReplicatorWindow), "RefreshRecipeIcons")]
        [HarmonyPatch(typeof(UIReplicatorWindow), "TestMouseRecipeIndex")]
        [HarmonyPatch(typeof(UIReplicatorWindow), "SetSelectedRecipeIndex")]
        [HarmonyPatch(typeof(UIReplicatorWindow), "SetSelectedRecipe")]
        [HarmonyPatch(typeof(UIReplicatorWindow), "_OnInit")]
        [HarmonyPatch(typeof(UIReplicatorWindow), "RepositionQueueText")]
        [HarmonyPatch(typeof(UIReplicatorWindow), "RefreshQueueIcons")]
        [HarmonyPatch(typeof(UIReplicatorWindow), "TestMouseQueueIndex")]
        [HarmonyPatch(typeof(UIRecipePicker), "_OnUpdate")]
        [HarmonyPatch(typeof(UIRecipePicker), "RefreshIcons")]
        [HarmonyPatch(typeof(UIRecipePicker), "TestMouseIndex")]
        [HarmonyPatch(typeof(UIItemPicker), "_OnUpdate")]
        [HarmonyPatch(typeof(UIItemPicker), "RefreshIcons")]
        [HarmonyPatch(typeof(UIItemPicker), "TestMouseIndex")]
        [HarmonyPatch(typeof(UISignalPicker), "_OnUpdate")]
        [HarmonyPatch(typeof(UISignalPicker), "RefreshIcons")]
        [HarmonyPatch(typeof(UISignalPicker), "TestMouseIndex")]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var source = new List<CodeInstruction>(instructions);
            for (var index = 0; index < source.Count; ++index)
            {
                if (source[index].opcode == OpCodes.Ldc_I4_S && (sbyte)source[index].operand == 12) source[index].operand = 17;
            }

            return source.AsEnumerable();
        }

        [HarmonyPatch(typeof(UIReplicatorWindow), "SetMaterialProps")]
        [HarmonyPatch(typeof(UIRecipePicker), "SetMaterialProps")]
        [HarmonyPatch(typeof(UIItemPicker), "SetMaterialProps")]
        [HarmonyPatch(typeof(UISignalPicker), "SetMaterialProps")]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> SetMaterialProps_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var source = new List<CodeInstruction>(instructions);
            for (var index = 0; index < source.Count; ++index)
            {
                // ReSharper disable once CompareOfFloatsByEqualityOperator
                if (source[index].opcode == OpCodes.Ldc_R4 && source[index].operand is float operand && operand == 12.0) source[index].operand = 17f;
            }

            return source.AsEnumerable();
        }
    }
}
