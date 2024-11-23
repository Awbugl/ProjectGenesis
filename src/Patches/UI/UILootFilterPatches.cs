using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using CommonAPI.Systems;
using HarmonyLib;
using UnityEngine;
using Object = UnityEngine.Object;

// ReSharper disable InconsistentNaming
// ReSharper disable LoopCanBePartlyConvertedToQuery

namespace ProjectGenesis.Patches.UI
{
    public static class UILootFilterPatches
    {
        private static List<UITabButton> _tabs;

        private static readonly FieldInfo currentTypeField = AccessTools.Field(typeof(UILootFilter), nameof(UILootFilter.currentType));

        [HarmonyPatch(typeof(UILootFilter), nameof(UILootFilter._OnCreate))]
        [HarmonyPostfix]
        public static void Create(UILootFilter __instance)
        {
            TabData[] allTabs = TabSystem.GetAllTabs();
            _tabs = new List<UITabButton>();
            var index = 1;

            foreach (TabData tabData in allTabs)
            {
                if (tabData == null) continue;

                index = tabData.tabIndex - 1;
                GameObject gameObject = Object.Instantiate(TabSystem.GetTabPrefab(), __instance.filterTrans, false);

                ((RectTransform)gameObject.transform).anchoredPosition = new Vector2(index * 70 - 54, -72f);
                UITabButton component = gameObject.GetComponent<UITabButton>();
                Sprite newIcon = Resources.Load<Sprite>(tabData.tabIconPath);
                component.Init(newIcon, tabData.tabName, index, __instance.OnTypeButtonClick);
                _tabs.Add(component);
            }

            __instance.typeButton2.transform.localPosition = new Vector3((index + 1) * 70 - 54, -40, 0);
        }

        [HarmonyPatch(typeof(UILootFilter), nameof(UILootFilter.OnTypeButtonClick))]
        [HarmonyPriority(Priority.VeryHigh)]
        [HarmonyPrefix]
        public static void OnTypeClicked_Prefix(int type) => UILootFilter.showAll = type == 1;

        [HarmonyPatch(typeof(UILootFilter), nameof(UILootFilter.OnTypeButtonClick))]
        [HarmonyPostfix]
        public static void OnTypeClicked_Postfix(int type)
        {
            foreach (UITabButton tab in _tabs) tab.TabSelected(type);
        }

        [HarmonyPatch(typeof(UILootFilter), nameof(UILootFilter._OnUpdate))]
        [HarmonyPatch(typeof(UILootFilter), nameof(UILootFilter.RepositionGridText))]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var matcher = new CodeMatcher(instructions);

            matcher.MatchForward(false, new CodeMatch(OpCodes.Ldc_I4_S, (sbyte)14));

            matcher.InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_0))
               .SetInstructionAndAdvance(Transpilers.EmitDelegate<Func<UILootFilter, int>>(filter => filter.currentType == 1 ? 14 : 17));

            matcher.MatchForward(false, new CodeMatch(OpCodes.Ldc_I4_S, (sbyte)14));

            matcher.InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_0))
               .SetInstructionAndAdvance(Transpilers.EmitDelegate<Func<UILootFilter, int>>(filter => filter.currentType == 1 ? 14 : 17));

            return matcher.InstructionEnumeration();
        }
        
        [HarmonyPatch(typeof(UILootFilter), nameof(UILootFilter.RefreshIcons))]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> RefreshIcons_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var matcher = new CodeMatcher(instructions);

            matcher.MatchForward(false, new CodeMatch(OpCodes.Ldc_I4_S, (sbyte)14));
            matcher.SetOperandAndAdvance((sbyte)17);
            
            matcher.MatchForward(false, new CodeMatch(OpCodes.Ldc_I4_S, (sbyte)14));
            matcher.SetOperandAndAdvance((sbyte)17); 
            
            matcher.MatchForward(false, new CodeMatch(OpCodes.Ldc_I4_S, (sbyte)14));
            matcher.SetOperandAndAdvance((sbyte)17);
            
            return matcher.InstructionEnumeration();
        }

        [HarmonyPatch(typeof(UILootFilter), nameof(UILootFilter.TestMouseIndex))]
        [HarmonyTranspiler]
        [HarmonyPriority(Priority.Last)]
        public static IEnumerable<CodeInstruction> TestMouseIndex_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var matcher = new CodeMatcher(instructions);

            matcher.MatchForward(false, new CodeMatch(OpCodes.Ldc_I4_S, (sbyte)14));

            matcher.InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_0))
               .SetInstructionAndAdvance(Transpilers.EmitDelegate<Func<UILootFilter, int>>(filter => filter.currentType == 1 ? 14 : 17));

            matcher.MatchForward(false, new CodeMatch(OpCodes.Ldc_I4_S, (sbyte)14));

            matcher.InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_0))
               .SetInstructionAndAdvance(Transpilers.EmitDelegate<Func<UILootFilter, int>>(filter => filter.currentType == 1 ? 14 : 17));

            matcher.MatchForward(false, new CodeMatch(OpCodes.Ldc_I4_S, (sbyte)14));

            matcher.InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_0))
               .SetInstructionAndAdvance(Transpilers.EmitDelegate<Func<UILootFilter, int>>(filter => filter.currentType == 1 ? 14 : 17));

            matcher.MatchForward(false, new CodeMatch(OpCodes.Ldc_I4_S, (sbyte)14));

            matcher.InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_0))
               .SetInstructionAndAdvance(Transpilers.EmitDelegate<Func<UILootFilter, int>>(filter => filter.currentType == 1 ? 14 : 17));

            return matcher.InstructionEnumeration();
        }

        [HarmonyPatch(typeof(UILootFilter), nameof(UILootFilter.OnBoxMouseDown))]
        [HarmonyPatch(typeof(UILootFilter), nameof(UILootFilter.RefreshWindow))]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> UILootFilter_OnBoxMouseDown_currentTypeField_Transpiler(
            IEnumerable<CodeInstruction> instructions)
        {
            var matcher = new CodeMatcher(instructions);

            matcher.MatchForward(true, new CodeMatch(OpCodes.Ldarg_0), new CodeMatch(OpCodes.Ldfld, currentTypeField));

            matcher.Advance(1).InsertAndAdvance(new CodeInstruction(OpCodes.Ldc_I4_1)).SetOpcodeAndAdvance(OpCodes.Beq_S);

            return matcher.InstructionEnumeration();
        }

        [HarmonyPatch(typeof(UILootFilter), nameof(UILootFilter.TestMouseIndex))]
        [HarmonyPatch(typeof(UILootFilter), nameof(UILootFilter.SetMaterialProps))]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> UILootFilter_SetMaterialProps_currentTypeField_Transpiler(
            IEnumerable<CodeInstruction> instructions)
        {
            var matcher = new CodeMatcher(instructions);

            matcher.MatchForward(true, new CodeMatch(OpCodes.Ldarg_0), new CodeMatch(OpCodes.Ldfld, currentTypeField));

            matcher.Advance(1).InsertAndAdvance(new CodeInstruction(OpCodes.Ldc_I4_1)).SetOpcodeAndAdvance(OpCodes.Bne_Un_S);

            return matcher.InstructionEnumeration();
        }

        [HarmonyPatch(typeof(UILootFilter), nameof(UILootFilter.RefreshIcons))]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> UILootFilter_RefreshIcons_currentTypeField_Transpiler(
            IEnumerable<CodeInstruction> instructions)
        {
            var matcher = new CodeMatcher(instructions);

            matcher.MatchForward(true, new CodeMatch(OpCodes.Ldarg_0), new CodeMatch(OpCodes.Ldfld, currentTypeField));

            matcher.Advance(1).InsertAndAdvance(new CodeInstruction(OpCodes.Ldc_I4_1)).SetOpcodeAndAdvance(OpCodes.Bne_Un_S);

            matcher.MatchForward(true, new CodeMatch(OpCodes.Ldarg_0), new CodeMatch(OpCodes.Ldfld, currentTypeField));

            matcher.Advance(1).InsertAndAdvance(new CodeInstruction(OpCodes.Ldc_I4_1)).SetOpcodeAndAdvance(OpCodes.Beq_S);

            return matcher.InstructionEnumeration();
        }

        [HarmonyPatch(typeof(UILootFilter), nameof(UILootFilter.RefreshWindow))]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> UILootFilter_RefreshWindow_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var matcher = new CodeMatcher(instructions);

            matcher.MatchForward(false, new CodeMatch(OpCodes.Ldc_R4, 692f), new CodeMatch(OpCodes.Ldc_R4, 536f));

            matcher.SetOperandAndAdvance(830f).SetOperandAndAdvance(500f);

            return matcher.InstructionEnumeration();
        }

        [HarmonyPatch(typeof(UILootFilter), nameof(UILootFilter.RefreshWindow))]
        [HarmonyPostfix]
        public static void RefreshWindow_Postfix(UILootFilter __instance)
        {
            __instance.contentTrans.sizeDelta = __instance.currentType == 1 ? new Vector2(644f, 414f) : new Vector2(782f, 322f);

            bool show = !__instance.showDropOnly;

            foreach (UITabButton uiTabButton in _tabs) uiTabButton.gameObject.SetActive(show);
        }

        [HarmonyPatch(typeof(UILootFilter), nameof(UILootFilter.SetMaterialProps))]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> UILootFilter_SetMaterialProps_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var matcher = new CodeMatcher(instructions);
            matcher.MatchForward(false, new CodeMatch(OpCodes.Ldc_I4_8));
            matcher.SetOpcodeAndAdvance(OpCodes.Ldc_I4_7);

            matcher.MatchForward(false, new CodeMatch(OpCodes.Ldc_R4, 14f));

            matcher.InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_0))
               .SetInstructionAndAdvance(
                    Transpilers.EmitDelegate<Func<UILootFilter, float>>(filter => filter.currentType == 1 ? 14f : 17f));

            return matcher.InstructionEnumeration();
        }
    }
}
