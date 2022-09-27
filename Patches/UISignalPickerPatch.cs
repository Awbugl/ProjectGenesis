using System.Collections.Generic;
using CommonAPI.Systems;
using CommonAPI.Systems.UI;
using HarmonyLib;
using UnityEngine;

// ReSharper disable RedundantAssignment
// ReSharper disable InconsistentNaming

namespace ProjectGenesis.Patches
{
    public static class UISignalPickerPatch
    {
        private static List<UITabButton> _tabs;

        [HarmonyPatch(typeof(UISignalPicker), "_OnCreate")]
        [HarmonyPostfix]
        public static void Create(UISignalPicker __instance)
        {
            TabData[] allTabs = TabSystem.GetAllTabs();
            _tabs = new List<UITabButton>(allTabs.Length - 3);
            foreach (var tabData in allTabs)
            {
                if (tabData != null)
                {
                    var gameObject = Object.Instantiate(TabSystem.GetTabPrefab(), __instance.pickerTrans, false);
                    ((RectTransform)gameObject.transform).anchoredPosition = new Vector2((tabData.tabIndex + 4) * 70 - 54, -75f);
                    var component = gameObject.GetComponent<UITabButton>();
                    var newIcon = Resources.Load<Sprite>(tabData.tabIconPath);
                    component.Init(newIcon, tabData.tabName, tabData.tabIndex + 4,
                                   i => AccessTools.Method(typeof(UISignalPicker), "OnTypeButtonClick").Invoke(__instance, new object[] { i }));
                    _tabs.Add(component);
                }
            }
        }

        [HarmonyPatch(typeof(UISignalPicker), "OnTypeButtonClick")]
        [HarmonyPostfix]
        public static void OnTypeClicked(int type)
        {
            foreach (var tab in _tabs) tab.TabSelected(type);
        }

        [HarmonyPatch(typeof(UIShowSignalTipExtension), "OnUpdate")]
        [HarmonyPrefix]
        public static bool UIShowSignalTipExtension_OnUpdate(UISignalPicker picker)
        {
            var hoveredIndex = AccessTools.FieldRefAccess<UISignalPicker, int>(picker, "hoveredIndex");
            var signalArray = AccessTools.FieldRefAccess<UISignalPicker, int[]>(picker, "signalArray");
            return hoveredIndex >= 0 && hoveredIndex < signalArray.Length;
        }

        [HarmonyPatch(typeof(UISignalPicker), "RefreshIcons")]
        [HarmonyPostfix]
        public static void RefreshIcons(
            UISignalPicker __instance,
            ref int ___currentType,
            ref uint[] ___indexArray,
            ref int[] ___signalArray)
        {
            if (___currentType == 7 || ___currentType == 8)
            {
                var iconSet = GameMain.iconSet;
                ItemProto[] dataArray = LDB.items.dataArray;
                foreach (var t in dataArray)
                {
                    if (t.GridIndex >= 1101)
                    {
                        var num4 = t.GridIndex / 1000;
                        if (num4 == ___currentType - 4)
                        {
                            var num5 = (t.GridIndex - num4 * 1000) / 100 - 1;
                            var num6 = t.GridIndex % 100 - 1;
                            if (num5 >= 0 && num6 >= 0 && num5 < 7 && num6 < 17)
                            {
                                var index5 = num5 * 17 + num6;
                                if (index5 >= 0 && index5 < ___indexArray.Length)
                                {
                                    var index6 = SignalProtoSet.SignalId(ESignalType.Item, t.ID);
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
