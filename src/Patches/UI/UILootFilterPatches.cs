using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using CommonAPI.Systems;
using HarmonyLib;
using UnityEngine;
using Object = UnityEngine.Object;

// ReSharper disable InconsistentNaming

namespace ProjectGenesis.Patches.UI
{
    public static class UILootFilterPatches
    {
        private static List<UITabButton> _tabs;

        private static readonly FieldInfo currentTypeField = AccessTools.Field(typeof(UILootFilter), "currentType");

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
                    ((RectTransform)gameObject.transform).anchoredPosition = new Vector2((tabData.tabIndex - 1) * 70 - 54, -72f);
                    var component = gameObject.GetComponent<UITabButton>();
                    var newIcon = Resources.Load<Sprite>(tabData.tabIconPath);
                    component.Init(newIcon, tabData.tabName, tabData.tabIndex,
                                   i => AccessTools.Method(typeof(UILootFilter), "OnTypeButtonClick").Invoke(__instance, new object[] { i }));
                    _tabs.Add(component);
                }
            }

            __instance.typeButton2.transform.localPosition = new Vector3(296, -40, 0);
        }

        [HarmonyPatch(typeof(UILootFilter), "OnTypeButtonClick")]
        [HarmonyPostfix]
        public static void OnTypeClicked(int type)
        {
            foreach (UITabButton tab in _tabs) tab.TabSelected(type);
        }

        [HarmonyPatch(typeof(UILootFilter), "_OnUpdate")]
        [HarmonyPatch(typeof(UILootFilter), "RepositionGridText")]
        [HarmonyPatch(typeof(UILootFilter), "RefreshIcons")]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var matcher = new CodeMatcher(instructions);

            matcher.MatchForward(false, new CodeMatch(OpCodes.Ldc_I4_S, (sbyte)14));

            matcher.InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_0))
                   .SetInstructionAndAdvance(Transpilers.EmitDelegate<Func<UILootFilter, int>>(filter => filter.currentType == 2 ? 14 : 17));

            matcher.MatchForward(false, new CodeMatch(OpCodes.Ldc_I4_S, (sbyte)14));

            matcher.InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_0))
                   .SetInstructionAndAdvance(Transpilers.EmitDelegate<Func<UILootFilter, int>>(filter => filter.currentType == 2 ? 14 : 17));

            return matcher.InstructionEnumeration();
        }

        [HarmonyPatch(typeof(UILootFilter), "TestMouseIndex")]
        [HarmonyTranspiler]
        [HarmonyPriority(Priority.Last)]
        public static IEnumerable<CodeInstruction> RefreshIcons_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var matcher = new CodeMatcher(instructions);

            matcher.MatchForward(false, new CodeMatch(OpCodes.Ldc_I4_S, (sbyte)14));

            matcher.InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_0))
                   .SetInstructionAndAdvance(Transpilers.EmitDelegate<Func<UILootFilter, int>>(filter => filter.currentType == 2 ? 14 : 17));

            matcher.MatchForward(false, new CodeMatch(OpCodes.Ldc_I4_S, (sbyte)14));

            matcher.InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_0))
                   .SetInstructionAndAdvance(Transpilers.EmitDelegate<Func<UILootFilter, int>>(filter => filter.currentType == 2 ? 14 : 17));

            matcher.MatchForward(false, new CodeMatch(OpCodes.Ldc_I4_S, (sbyte)14));

            matcher.InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_0))
                   .SetInstructionAndAdvance(Transpilers.EmitDelegate<Func<UILootFilter, int>>(filter => filter.currentType == 2 ? 14 : 17));

            matcher.MatchForward(false, new CodeMatch(OpCodes.Ldc_I4_S, (sbyte)14));

            matcher.InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_0))
                   .SetInstructionAndAdvance(Transpilers.EmitDelegate<Func<UILootFilter, int>>(filter => filter.currentType == 2 ? 14 : 17));

            return matcher.InstructionEnumeration();
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

        [HarmonyPatch(typeof(UILootFilter), "OnBoxMouseDown")]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> UILootFilter_OnBoxMouseDown_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            CodeMatcher matcher = new CodeMatcher(instructions).MatchForward(true, new CodeMatch(OpCodes.Ldarg_0),
                                                                             new CodeMatch(OpCodes.Ldfld, currentTypeField),
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

            matcher.MatchForward(false, new CodeMatch(OpCodes.Ldc_R4, 692f), new CodeMatch(OpCodes.Ldc_R4, 536f));

            matcher.SetOperandAndAdvance(830f).SetOperandAndAdvance(500f);

            return matcher.InstructionEnumeration();
        }

        [HarmonyPatch(typeof(UILootFilter), "RefreshWindow")]
        [HarmonyPostfix]
        public static void RefreshWindow_Postfix(UILootFilter __instance)
        {
            __instance.contentTrans.sizeDelta = __instance.currentType == 2 ? new Vector2(644f, 414f) : new Vector2(782f, 322f);
        }

        [HarmonyPatch(typeof(UILootFilter), "SetMaterialProps")]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> UILootFilter_SetMaterialProps_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            CodeMatcher matcher = new CodeMatcher(instructions).MatchForward(true, new CodeMatch(OpCodes.Ldarg_0),
                                                                             new CodeMatch(OpCodes.Ldfld, currentTypeField),
                                                                             new CodeMatch(OpCodes.Ldc_I4_1));

            matcher.SetOpcodeAndAdvance(OpCodes.Ldc_I4_2).SetOpcodeAndAdvance(OpCodes.Bne_Un_S);

            matcher.MatchForward(false, new CodeMatch(OpCodes.Ldc_I4_8));
            matcher.SetOpcodeAndAdvance(OpCodes.Ldc_I4_7);

            matcher.MatchForward(false, new CodeMatch(OpCodes.Ldc_R4, 14f));

            matcher.InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_0))
                   .SetInstructionAndAdvance(Transpilers.EmitDelegate<Func<UILootFilter, float>>(filter => filter.currentType == 2 ? 14f : 17f));

            return matcher.InstructionEnumeration();
        }
    }
}
