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

        private static readonly FieldInfo currentTypeField = AccessTools.Field(typeof(UISignalPicker), "currentType");

        [HarmonyPatch(typeof(UISignalPicker), "_OnCreate")]
        [HarmonyPostfix]
        public static void Create(UISignalPicker __instance)
        {
            TabData[] allTabs = TabSystem.GetAllTabs();
            _tabs = new List<UITabButton>();
            foreach (TabData tabData in allTabs)
            {
                if (tabData != null)
                {
                    GameObject gameObject = Object.Instantiate(TabSystem.GetTabPrefab(), __instance.pickerTrans, false);
                    ((RectTransform)gameObject.transform).anchoredPosition = new Vector2((tabData.tabIndex + 5) * 70 - 54, -75f);
                    var component = gameObject.GetComponent<UITabButton>();
                    var newIcon = Resources.Load<Sprite>(tabData.tabIconPath);
                    component.Init(newIcon, tabData.tabName, tabData.tabIndex + 5,
                                   i => AccessTools.Method(typeof(UISignalPicker), "OnTypeButtonClick").Invoke(__instance, new object[] { i }));
                    _tabs.Add(component);
                }
            }
        }

        [HarmonyPatch(typeof(UISignalPicker), "OnTypeButtonClick")]
        [HarmonyPostfix]
        public static void OnTypeClicked(int type)
        {
            foreach (UITabButton tab in _tabs) tab.TabSelected(type);
        }

        [HarmonyPatch(typeof(UIShowSignalTipExtension), "OnUpdate")]
        [HarmonyPrefix]
        public static bool UIShowSignalTipExtension_OnUpdate(UISignalPicker picker) => picker.hoveredIndex >= 0 && picker.hoveredIndex < picker.signalArray.Length;

        [HarmonyPatch(typeof(UISignalPicker), "_OnUpdate")]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> UISignalPicker_OnUpdate_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            CodeMatcher matcher = new CodeMatcher(instructions).MatchForward(true, new CodeMatch(OpCodes.Ldarg_0),
                                                                             new CodeMatch(OpCodes.Ldfld, currentTypeField),
                                                                             new CodeMatch(OpCodes.Ldc_I4_2));

            object labal = matcher.Advance(1).Operand;

            matcher.Advance(1).InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_0), new CodeInstruction(OpCodes.Ldfld, currentTypeField),
                                                new CodeInstruction(OpCodes.Ldc_I4_8), new CodeInstruction(OpCodes.Bge, labal));

            return matcher.InstructionEnumeration();
        }

        [HarmonyPatch(typeof(UISignalPicker), "RefreshIcons")]
        [HarmonyPostfix]
        public static void RefreshIcons(
            UISignalPicker __instance,
            ref int ___currentType,
            ref uint[] ___indexArray,
            ref int[] ___signalArray)
        {
            if (___currentType > 7)
            {
                IconSet iconSet = GameMain.iconSet;
                ItemProto[] dataArray = LDB.items.dataArray;
                foreach (ItemProto t in dataArray)
                {
                    if (t.GridIndex >= 1101)
                    {
                        int num4 = t.GridIndex / 1000;
                        if (num4 == ___currentType - 4)
                        {
                            int num5 = (t.GridIndex - num4 * 1000) / 100 - 1;
                            int num6 = t.GridIndex % 100 - 1;
                            if (num5 >= 0 && num6 >= 0 && num5 < 7 && num6 < 17)
                            {
                                int index5 = num5 * 17 + num6;
                                if (index5 >= 0 && index5 < ___indexArray.Length)
                                {
                                    int index6 = SignalProtoSet.SignalId(ESignalType.Item, t.ID);
                                    ___indexArray[index5] = iconSet.signalIconIndex[index6];
                                    ___signalArray[index5] = index6;
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
