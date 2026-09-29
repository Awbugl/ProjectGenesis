using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using CommonAPI.Systems;
using HarmonyLib;
using ProjectGenesis.Utils;
using UnityEngine;
using Object = UnityEngine.Object;

// ReSharper disable InconsistentNaming
// ReSharper disable LoopCanBePartlyConvertedToQuery

namespace ProjectGenesis.Patches
{
    public static class UILootFilterPatches
    {
        // vanilla types: 0 item page, 1 building page, 2 enemy drop (UILootFilter.kTypeDrop)
        // tab pages added by TabSystem use their tabIndex (3, 4, ...) as type, and show items of the page with the same index
        private const int DropType = 2;

        private static List<UITabButton> _tabs;

        private static readonly FieldInfo currentTypeField = AccessTools.Field(typeof(UILootFilter), nameof(UILootFilter.currentType));

        [HarmonyPatch(typeof(UILootFilter), nameof(UILootFilter._OnCreate))]
        [HarmonyPostfix]
        public static void Create(UILootFilter __instance)
        {
            TabData[] allTabs = TabSystem.GetAllTabs();
            _tabs = new List<UITabButton>();

            // button slots (x = slot * 70 - 54): 1 item, 2 building, then the tab pages, then enemy drop
            var index = 2;

            foreach (TabData tabData in allTabs)
            {
                if (tabData == null) continue;

                index = tabData.tabIndex;
                GameObject gameObject = Object.Instantiate(TabSystem.GetTabPrefab(), __instance.filterTrans, false);

                ((RectTransform)gameObject.transform).anchoredPosition = new Vector2(index * 70 - 54, -72f);
                UITabButton component = gameObject.GetComponent<UITabButton>();
                Sprite newIcon = Resources.Load<Sprite>(tabData.tabIconPath);
                component.Init(newIcon, tabData.tabName, tabData.tabIndex, __instance.OnTypeButtonClick);
                _tabs.Add(component);
            }

            // enemy drop button goes after the tab pages
            __instance.typeButton3.transform.localPosition = new Vector3((index + 1) * 70 - 54, -40, 0);
        }

        [HarmonyPatch(typeof(UILootFilter), nameof(UILootFilter.OnTypeButtonClick))]
        [HarmonyPriority(Priority.VeryHigh)]
        [HarmonyPrefix]
        public static void OnTypeClicked_Prefix(int type) => UILootFilter.showAll = type == DropType;

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
               .SetInstructionAndAdvance(Transpilers.EmitDelegate<Func<UILootFilter, int>>(filter => filter.currentType == DropType ? 14 : 17));

            matcher.MatchForward(false, new CodeMatch(OpCodes.Ldc_I4_S, (sbyte)14));

            matcher.InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_0))
               .SetInstructionAndAdvance(Transpilers.EmitDelegate<Func<UILootFilter, int>>(filter => filter.currentType == DropType ? 14 : 17));

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
               .SetInstructionAndAdvance(Transpilers.EmitDelegate<Func<UILootFilter, int>>(filter => filter.currentType == DropType ? 14 : 17));

            matcher.MatchForward(false, new CodeMatch(OpCodes.Ldc_I4_S, (sbyte)14));

            matcher.InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_0))
               .SetInstructionAndAdvance(Transpilers.EmitDelegate<Func<UILootFilter, int>>(filter => filter.currentType == DropType ? 14 : 17));

            matcher.MatchForward(false, new CodeMatch(OpCodes.Ldc_I4_S, (sbyte)14));

            matcher.InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_0))
               .SetInstructionAndAdvance(Transpilers.EmitDelegate<Func<UILootFilter, int>>(filter => filter.currentType == DropType ? 14 : 17));

            matcher.MatchForward(false, new CodeMatch(OpCodes.Ldc_I4_S, (sbyte)14));

            matcher.InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_0))
               .SetInstructionAndAdvance(Transpilers.EmitDelegate<Func<UILootFilter, int>>(filter => filter.currentType == DropType ? 14 : 17));

            return matcher.InstructionEnumeration();
        }

        // vanilla treats "currentType < 2" as pick filter pages, tab pages have larger types: "currentType != 2" instead
        [HarmonyPatch(typeof(UILootFilter), nameof(UILootFilter._OnUpdate))]
        [HarmonyPatch(typeof(UILootFilter), nameof(UILootFilter.OnBoxMouseDown))]
        [HarmonyPatch(typeof(UILootFilter), nameof(UILootFilter.RefreshWindow))]
        [HarmonyPatch(typeof(UILootFilter), nameof(UILootFilter.RefreshIcons))]
        [HarmonyPatch(typeof(UILootFilter), nameof(UILootFilter.TestMouseIndex))]
        [HarmonyPatch(typeof(UILootFilter), nameof(UILootFilter.SetMaterialProps))]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> UILootFilter_currentTypeField_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var matcher = new CodeMatcher(instructions);

            CodeMatch[] matches =
            {
                new CodeMatch(OpCodes.Ldarg_0), new CodeMatch(OpCodes.Ldfld, currentTypeField), new CodeMatch(OpCodes.Ldc_I4_2),
                new CodeMatch(i => i.opcode == OpCodes.Blt || i.opcode == OpCodes.Blt_S || i.opcode == OpCodes.Bge
                                || i.opcode == OpCodes.Bge_S),
            };

            matcher.MatchForward(true, matches);

            do
            {
                OpCode opcode = matcher.Opcode;

                if (opcode == OpCodes.Blt) opcode = OpCodes.Bne_Un;
                else if (opcode == OpCodes.Blt_S) opcode = OpCodes.Bne_Un_S;
                else if (opcode == OpCodes.Bge) opcode = OpCodes.Beq;
                else opcode = OpCodes.Beq_S;

                matcher.SetOpcodeAndAdvance(opcode);
                matcher.MatchForward(true, matches);
            }
            while (matcher.IsValid);

            return matcher.InstructionEnumeration();
        }

        [HarmonyPatch(typeof(UILootFilter), nameof(UILootFilter.RefreshIcons))]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> UILootFilter_RefreshIcons_Page_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var matcher = new CodeMatcher(instructions);

            // gridPage == currentType + 1  ->  gridPage == GetPickPage(this)
            matcher.MatchForward(false, new CodeMatch(OpCodes.Ldarg_0), new CodeMatch(OpCodes.Ldfld, currentTypeField),
                new CodeMatch(OpCodes.Ldc_I4_1), CodeMatchUtils.Add);

            matcher.Advance(1).SetAndAdvance(OpCodes.Call, AccessTools.Method(typeof(UILootFilterPatches), nameof(GetPickPage)))
               .SetAndAdvance(OpCodes.Nop, null).SetAndAdvance(OpCodes.Nop, null);

            return matcher.InstructionEnumeration();
        }

        public static int GetPickPage(UILootFilter filter) => filter.currentType < DropType ? filter.currentType + 1 : filter.currentType;

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
            __instance.contentTrans.sizeDelta = __instance.currentType == DropType ? new Vector2(644f, 414f) : new Vector2(782f, 322f);

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
                    Transpilers.EmitDelegate<Func<UILootFilter, float>>(filter => filter.currentType == DropType ? 14f : 17f));

            return matcher.InstructionEnumeration();
        }
    }
}
