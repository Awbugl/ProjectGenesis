using System.Collections.Generic;
using System.Reflection.Emit;
using CommonAPI.Systems;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

// ReSharper disable RedundantAssignment
// ReSharper disable InconsistentNaming

namespace ProjectGenesis.Patches
{
    public static class UISignalTagPickerPatches
    {
        private static List<UIButton> _customTabBtns;

        [HarmonyPatch(typeof(UISignalTagPicker), nameof(UISignalTagPicker._OnCreate))]
        [HarmonyPostfix]
        public static void Create(UISignalTagPicker __instance)
        {
            TabData[] allTabs = TabSystem.GetAllTabs();
            _customTabBtns = new List<UIButton>();

            Vector2 basePos = ((RectTransform)__instance.upgradeTab2Btn.transform).anchoredPosition;

            foreach (TabData tabData in allTabs)
            {
                if (tabData == null) continue;

                GameObject gameObject = Object.Instantiate(__instance.upgradeTab2Btn.gameObject, __instance.pickerTrans, false);
                RectTransform rectTransform = (RectTransform)gameObject.transform;

                rectTransform.anchoredPosition = new Vector2(basePos.x + (tabData.tabIndex - 5) * 48, basePos.y);

                UIButton uiButton = gameObject.GetComponent<UIButton>();

                uiButton.data = tabData.tabIndex + 6;

                Image iconImage = gameObject.transform.Find("icon").GetComponent<Image>();
                
                iconImage.sprite = Resources.Load<Sprite>(tabData.tabIconPath);

                _customTabBtns.Add(uiButton);
            }

            __instance.upgradeTab2Btn.gameObject.SetActive(false);
            __instance.upgradeTab1Btn.gameObject.SetActive(false);
            __instance.techTabBtn.gameObject.SetActive(false);
        }

        [HarmonyPatch(typeof(UISignalTagPicker), nameof(UISignalTagPicker._OnRegEvent))]
        [HarmonyPostfix]
        public static void OnRegEvent(UISignalTagPicker __instance)
        {
            if (_customTabBtns == null) return;

            foreach (UIButton btn in _customTabBtns) btn.onClick += __instance.OnTypeButtonClick;
        }

        [HarmonyPatch(typeof(UISignalTagPicker), nameof(UISignalTagPicker._OnUnregEvent))]
        [HarmonyPostfix]
        public static void OnUnregEvent(UISignalTagPicker __instance)
        {
            if (_customTabBtns == null) return;

            foreach (UIButton btn in _customTabBtns) btn.onClick -= __instance.OnTypeButtonClick;
        }

        [HarmonyPatch(typeof(UISignalTagPicker), nameof(UISignalTagPicker._OnUpdate))]
        [HarmonyPatch(typeof(UISignalTagPicker), nameof(UISignalTagPicker.RefreshIcons))]
        [HarmonyPatch(typeof(UISignalTagPicker), nameof(UISignalTagPicker.TestMouseIndex))]
        [HarmonyTranspiler]
        [HarmonyPriority(Priority.Last)]
        public static IEnumerable<CodeInstruction> UISignalTagPicker_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            foreach (CodeInstruction ci in instructions)
            {
                if (ci.opcode == OpCodes.Ldc_I4_S)
                {
                    var operand = (sbyte)ci.operand;

                    if (operand == 14) ci.operand = (sbyte)17;

                    if (operand == 10) ci.operand = (sbyte)7;
                }

                yield return ci;
            }
        }

        [HarmonyPatch(typeof(UISignalTagPicker), nameof(UISignalTagPicker.SetMaterialProps))]
        [HarmonyTranspiler]
        [HarmonyPriority(Priority.Last)]
        public static IEnumerable<CodeInstruction> UISignalTagPicker_SetMaterialProps_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            foreach (CodeInstruction ci in instructions)
            {
                if (ci.opcode == OpCodes.Ldc_R4)
                {
                    var operand = (float)ci.operand;

                    if (operand is 14f) ci.operand = 17f;

                    if (operand is 10f) ci.operand = 7f;
                }

                yield return ci;
            }
        }

        [HarmonyPatch(typeof(UISignalTagPicker), nameof(UISignalTagPicker.RefreshIcons))]
        [HarmonyPostfix]
        public static void RefreshIcons(UISignalTagPicker __instance)
        {
            if ((int)__instance.currentType <= 8) return;

            IconSet iconSet = GameMain.iconSet;
            ItemProto[] dataArray = LDB.items.dataArray;
            GameHistoryData history = GameMain.history;

            foreach (ItemProto t in dataArray)
            {
                if (t.GridIndex < 1101) continue;

                int tabIndex = t.GridIndex / 1000;

                if (tabIndex != (int)__instance.currentType - 6) continue;

                int row = (t.GridIndex - tabIndex * 1000) / 100 - 1;
                int col = t.GridIndex % 100 - 1;


                if (row < 0 || col < 0 || row >= 7 || col >= 17) continue;

                int index = row * 17 + col;
                if (index < 0 || index >= __instance.indexArray.Length) continue;


                if (!UISignalTagPicker.showUnlock && !history.ItemUnlocked(t.ID)) continue;

                int signalIndex = SignalProtoSet.SignalId(ESignalType.Item, t.ID);
                __instance.indexArray[index] = iconSet.signalIconIndex[signalIndex];
                __instance.signalArray[index] = signalIndex;
            }
        }

        [HarmonyPatch(typeof(UISignalTagPicker), nameof(UISignalTagPicker.OnTypeButtonClick))]
        [HarmonyPostfix]
        public static void OnTypeButtonClick_Postfix(UISignalTagPicker __instance, int type)
        {
            if (_customTabBtns == null) return;

            foreach (UIButton btn in _customTabBtns)
            {
                bool isSelected = btn.data == type;
                btn.highlighted = isSelected;
                btn.button.interactable = !isSelected;
            }
        }
    }
}
