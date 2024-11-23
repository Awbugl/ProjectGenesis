using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using CommonAPI.Systems;
using CommonAPI.Systems.UI;
using HarmonyLib;
using UnityEngine;

// ReSharper disable RedundantAssignment
// ReSharper disable InconsistentNaming

namespace ProjectGenesis.Patches.UI
{
    public static class UISignalPickerPatches
    {
        private static List<UITabButton> _tabs;

        private static readonly FieldInfo currentTypeField = AccessTools.Field(typeof(UISignalPicker), nameof(UISignalPicker.currentType));

        [HarmonyPatch(typeof(UISignalPicker), nameof(UISignalPicker._OnCreate))]
        [HarmonyPostfix]
        public static void Create(UISignalPicker __instance)
        {
            TabData[] allTabs = TabSystem.GetAllTabs();
            _tabs = new List<UITabButton>();

            foreach (TabData tabData in allTabs)
            {
                if (tabData == null) continue;

                GameObject gameObject = Object.Instantiate(TabSystem.GetTabPrefab(), __instance.pickerTrans, false);
                ((RectTransform)gameObject.transform).anchoredPosition = new Vector2((tabData.tabIndex + 4) * 70 - 54, -75f);
                UITabButton component = gameObject.GetComponent<UITabButton>();
                Sprite newIcon = Resources.Load<Sprite>(tabData.tabIconPath);
                component.Init(newIcon, tabData.tabName, tabData.tabIndex + 5, __instance.OnTypeButtonClick);
                _tabs.Add(component);
            }

            Transform typeButton6Transform = __instance.typeButton6.transform;
            __instance.typeButton7.transform.localPosition = typeButton6Transform.localPosition;
            typeButton6Transform.localPosition = __instance.typeButton5.transform.localPosition;
            __instance.typeButton5.gameObject.SetActive(false);
        }

        [HarmonyPatch(typeof(UISignalPicker), nameof(UISignalPicker.OnTypeButtonClick))]
        [HarmonyPostfix]
        public static void OnTypeClicked(int type)
        {
            foreach (UITabButton tab in _tabs) tab.TabSelected(type);
        }

        [HarmonyPatch(typeof(UIShowSignalTipExtension), nameof(UIShowSignalTipExtension.OnUpdate))]
        [HarmonyPriority(Priority.VeryHigh)]
        [HarmonyPrefix]
        public static bool UIShowSignalTipExtension_OnUpdate(UISignalPicker picker) =>
            picker.hoveredIndex >= 0 && picker.hoveredIndex < picker.signalArray.Length;

        [HarmonyPatch(typeof(UISignalPicker), nameof(UISignalPicker._OnUpdate))]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> UISignalPicker_OnUpdate_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var matcher = new CodeMatcher(instructions);

            matcher.MatchForward(true, new CodeMatch(OpCodes.Ldarg_0), new CodeMatch(OpCodes.Ldfld, currentTypeField),
                new CodeMatch(OpCodes.Ldc_I4_2));

            object labal = matcher.Advance(1).Operand;

            matcher.Advance(1).InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_0), new CodeInstruction(OpCodes.Ldfld, currentTypeField),
                new CodeInstruction(OpCodes.Ldc_I4_8), new CodeInstruction(OpCodes.Bge, labal));

            return matcher.InstructionEnumeration();
        }

        [HarmonyPatch(typeof(UISignalPicker), nameof(UISignalPicker._OnUpdate))]
        [HarmonyPatch(typeof(UISignalPicker), nameof(UISignalPicker.RefreshIcons))]
        [HarmonyPatch(typeof(UISignalPicker), nameof(UISignalPicker.TestMouseIndex))]
        [HarmonyTranspiler]
        [HarmonyPriority(Priority.Last)]
        public static IEnumerable<CodeInstruction> UISignalPicker_Transpiler(IEnumerable<CodeInstruction> instructions)
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

        [HarmonyPatch(typeof(UIShowSignalTipExtension), nameof(UIShowSignalTipExtension.OnUpdate))]
        [HarmonyTranspiler]
        [HarmonyPriority(Priority.Last)]
        public static IEnumerable<CodeInstruction> UIShowSignalTipExtension_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            foreach (CodeInstruction ci in instructions)
            {
                if (ci.opcode == OpCodes.Ldc_I4_S && (sbyte)ci.operand == 12) ci.operand = (sbyte)17;

                yield return ci;
            }
        }

        [HarmonyPatch(typeof(UISignalPicker), nameof(UISignalPicker.SetMaterialProps))]
        [HarmonyTranspiler]
        [HarmonyPriority(Priority.Last)]
        public static IEnumerable<CodeInstruction> UISignalPicker_SetMaterialProps_Transpiler(IEnumerable<CodeInstruction> instructions)
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

        [HarmonyPatch(typeof(UISignalPicker), nameof(UISignalPicker.RefreshIcons))]
        [HarmonyPostfix]
        public static void RefreshIcons(UISignalPicker __instance, ref int ___currentType, ref uint[] ___indexArray,
            ref int[] ___signalArray)
        {
            if (___currentType <= 7) return;

            IconSet iconSet = GameMain.iconSet;
            ItemProto[] dataArray = LDB.items.dataArray;

            foreach (ItemProto t in dataArray)
            {
                if (t.GridIndex < 1101) continue;

                int num4 = t.GridIndex / 1000;

                if (num4 != ___currentType - 5) continue;

                int num5 = (t.GridIndex - num4 * 1000) / 100 - 1;
                int num6 = t.GridIndex % 100 - 1;

                if (num5 < 0 || num6 < 0 || num5 >= 7 || num6 >= 17) continue;

                int index5 = num5 * 17 + num6;

                if (index5 < 0 || index5 >= ___indexArray.Length) continue;

                int index6 = SignalProtoSet.SignalId(ESignalType.Item, t.ID);
                ___indexArray[index5] = iconSet.signalIconIndex[index6];
                ___signalArray[index5] = index6;
            }
        }
    }
}
