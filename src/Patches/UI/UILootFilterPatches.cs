using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using CommonAPI.Systems;
using HarmonyLib;
using UnityEngine;

// ReSharper disable InconsistentNaming

namespace ProjectGenesis.Patches.UI
{
    public static class UILootFilterPatches
    {
        private static List<UITabButton> _tabs;

        private static readonly FieldInfo currentTypeField = AccessTools.Field(typeof(UILootFilter), "currentType");

        [HarmonyPatch(typeof(UILootFilter), "Popup")]
        [HarmonyPostfix]
        public static void Popup()
        {
            UILootFilter.showAll = true;
        }

        [HarmonyPatch(typeof(UILootFilter), "_OnCreate")]
        [HarmonyPostfix]
        public static void Create(UILootFilter __instance)
        {
            TabData[] allTabs = TabSystem.GetAllTabs();
            _tabs = new List<UITabButton>();
            foreach (TabData tabData in allTabs)
            {
                if (tabData != null)
                {
                    GameObject gameObject = Object.Instantiate(TabSystem.GetTabPrefab(), __instance.filterTrans, false);
                    ((RectTransform)gameObject.transform).anchoredPosition = new Vector2((tabData.tabIndex) * 70 - 54, -75f);
                    var component = gameObject.GetComponent<UITabButton>();
                    var newIcon = Resources.Load<Sprite>(tabData.tabIconPath);
                    component.Init(newIcon, tabData.tabName, tabData.tabIndex,
                                   i => AccessTools.Method(typeof(UILootFilter), "OnTypeButtonClick").Invoke(__instance, new object[] { i }));
                    _tabs.Add(component);
                }
            }
        }

        [HarmonyPatch(typeof(UILootFilter), "OnTypeButtonClick")]
        [HarmonyPostfix]
        public static void OnTypeClicked(int type)
        {
            foreach (UITabButton tab in _tabs) tab.TabSelected(type);
        }

        [HarmonyPatch(typeof(UILootFilter), "RefreshIcons")]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> UILootFilter_RefreshIcons_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            CodeMatcher matcher = new CodeMatcher(instructions).MatchForward(true, new CodeMatch(OpCodes.Ldarg_0),
                                                                             new CodeMatch(OpCodes.Ldfld, currentTypeField),
                                                                             new CodeMatch(OpCodes.Ldc_I4_1));

            matcher.SetOpcodeAndAdvance(OpCodes.Ldc_I4_2).SetOpcodeAndAdvance(OpCodes.Bne_Un_S);

            matcher.MatchForward(true, new CodeMatch(OpCodes.Ldarg_0), new CodeMatch(OpCodes.Ldfld, currentTypeField),
                                 new CodeMatch(OpCodes.Ldc_I4_1));

            matcher.SetOpcodeAndAdvance(OpCodes.Ldc_I4_2).SetOpcodeAndAdvance(OpCodes.Beq_S);

            return matcher.InstructionEnumeration();
        }

        [HarmonyPatch(typeof(UILootFilter), "RefreshWindow")]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> UILootFilter_RefreshWindow_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            CodeMatcher matcher = new CodeMatcher(instructions).MatchForward(true, new CodeMatch(OpCodes.Ldarg_0),
                                                                             new CodeMatch(OpCodes.Ldfld, currentTypeField),
                                                                             new CodeMatch(OpCodes.Ldc_I4_1));

            matcher.SetOpcodeAndAdvance(OpCodes.Ldc_I4_2).SetOpcodeAndAdvance(OpCodes.Beq_S);

            return matcher.InstructionEnumeration();
        }

        [HarmonyPatch(typeof(UILootFilter), "RefreshWindow")]
        [HarmonyPostfix]
        public static void UILootFilter_RefreshWindow_Postfix(UILootFilter __instance)
        {
            __instance.filterTrans.sizeDelta = new Vector2(830, 476);
        }
    }
}
