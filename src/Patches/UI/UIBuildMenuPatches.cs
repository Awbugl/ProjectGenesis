using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using ProjectGenesis.Patches.UI.Utils;
using ProjectGenesis.Utils;
using UnityEngine;
using UnityEngine.UI;

// ReSharper disable InconsistentNaming
// ReSharper disable Unity.UnknownResource

namespace ProjectGenesis.Patches.UI
{
    public static class UIBuildMenuPatches
    {
        private static readonly FieldInfo UIBuildMenu_currentCategory_Field =
            AccessTools.Field(typeof(UIBuildMenu), nameof(UIBuildMenu.currentCategory));

        [HarmonyPatch(typeof(UIBuildMenu), nameof(UIBuildMenu._OnCreate))]
        [HarmonyPostfix]
        public static void UIBuildMenu_OnCreate(UIBuildMenu __instance)
        {
            UIButton categoryButton = __instance.categoryButtons[1];

            UIButton btn = Object.Instantiate(categoryButton, categoryButton.transform.parent);

            btn.transform.localPosition = new Vector3(-337, 1, 0);

            Util.RemovePersistentCalls(btn.gameObject);

            btn.button.onClick.AddListener(OnCategoryButtonClick);

            btn.tips.tipTitle = "巨构类".TranslateFromJson();

            Image img = btn.transform.GetChild(0).GetComponent<Image>();
            img.sprite = Resources.Load<Sprite>("Icons/Tech/1604");

            Text text = btn.transform.GetChild(1).GetComponent<Text>();
            text.text = "-";

            __instance.categoryIcons[12] = img;
            __instance.categoryTips[12] = text;
            __instance.categoryButtons[12] = btn;

            // ReSharper disable once LoopCanBePartlyConvertedToQuery
            foreach (UIButton button in __instance.categoryButtons)
                if (button != null)
                    SetButtonPosition(button);

            SetButtonPosition(__instance.blueprintButton);
            return;

            void SetButtonPosition(UIButton button)
            {
                Transform buttonTransform = button.transform;

                Vector3 transformLocalPosition = buttonTransform.localPosition;

                float pos = transformLocalPosition.x + 26;

                buttonTransform.localPosition = new Vector3(pos, transformLocalPosition.y, transformLocalPosition.z);
            }
        }

        private static void OnCategoryButtonClick() => UIRoot.instance.uiGame.buildMenu.OnCategoryButtonClick(12);

        [HarmonyPatch(typeof(UIBuildMenu), nameof(UIBuildMenu._OnUpdate))]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> UIBuildMenu_OnUpdate_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var matcher = new CodeMatcher(instructions);

            matcher.MatchForward(false, new CodeMatch(OpCodes.Ldc_I4_S, (sbyte)49));

            matcher.Advance(5).InsertAndAdvance(new CodeInstruction(OpCodes.Call,
                AccessTools.Method(typeof(UIBuildMenuPatches), nameof(OnUpdate_KeyCode_Patch))));

            matcher.MatchForward(true, new CodeMatch(OpCodes.Ldloc_2), new CodeMatch(OpCodes.Ldc_I4_S, (sbyte)9));

            matcher.InsertAndAdvance(
                new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(UIBuildMenuPatches), nameof(OnUpdate_For_Patch))),
                new CodeInstruction(OpCodes.Stloc_2), new CodeInstruction(OpCodes.Ldloc_2));

            matcher.Advance(1)
               .InsertAndAdvance(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(UIBuildMenuPatches), nameof(OnUpdate_Patch))));
            matcher.SetOpcodeAndAdvance(OpCodes.Brtrue);


            matcher.MatchForward(true, new CodeMatch(OpCodes.Ldarg_0), new CodeMatch(OpCodes.Ldfld, UIBuildMenu_currentCategory_Field),
                new CodeMatch(OpCodes.Ldc_I4_S, (sbyte)9));
            matcher.Advance(1)
               .InsertAndAdvance(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(UIBuildMenuPatches), nameof(OnUpdate_Patch))));
            matcher.SetOpcodeAndAdvance(OpCodes.Brfalse);

            return matcher.InstructionEnumeration();
        }

        [HarmonyPatch(typeof(UIBuildMenu), nameof(UIBuildMenu.SetCurrentCategory))]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> UIBuildMenu_SetCurrentCategory_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var matcher = new CodeMatcher(instructions);

            matcher.MatchForward(true, new CodeMatch(OpCodes.Ldarg_1), new CodeMatch(OpCodes.Ldc_I4_S, (sbyte)9));
            matcher.Advance(1)
               .InsertAndAdvance(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(UIBuildMenuPatches), nameof(OnUpdate_Patch))));
            matcher.SetOpcodeAndAdvance(OpCodes.Brfalse);

            matcher.MatchForward(true, new CodeMatch(OpCodes.Ldarg_0), new CodeMatch(OpCodes.Ldfld, UIBuildMenu_currentCategory_Field),
                new CodeMatch(OpCodes.Ldc_I4_S, (sbyte)9));
            matcher.Advance(1)
               .InsertAndAdvance(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(UIBuildMenuPatches), nameof(OnUpdate_Patch))));
            matcher.SetOpcodeAndAdvance(OpCodes.Brfalse);

            return matcher.InstructionEnumeration();
        }

        [HarmonyPatch(typeof(UIBuildMenu), nameof(UIBuildMenu.DoPlayerPackageChange))]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> UIFunctionPanel_DoPlayerPackageChange_Transpiler(
            IEnumerable<CodeInstruction> instructions)
        {
            var matcher = new CodeMatcher(instructions);

            matcher.MatchForward(true, new CodeMatch(OpCodes.Ldarg_0), new CodeMatch(OpCodes.Ldfld, UIBuildMenu_currentCategory_Field),
                new CodeMatch(OpCodes.Ldc_I4_S, (sbyte)9));
            matcher.Advance(1)
               .InsertAndAdvance(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(UIBuildMenuPatches), nameof(OnUpdate_Patch))));
            matcher.SetOpcodeAndAdvance(OpCodes.Brfalse);

            return matcher.InstructionEnumeration();
        }

        [HarmonyPatch(typeof(UIFunctionPanel), nameof(UIFunctionPanel._OnUpdate))]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> UIFunctionPanel_OnUpdate_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var matcher = new CodeMatcher(instructions);

            matcher.MatchForward(true, new CodeMatch(OpCodes.Ldarg_0), new CodeMatch(OpCodes.Ldfld),
                new CodeMatch(OpCodes.Ldfld, UIBuildMenu_currentCategory_Field), new CodeMatch(OpCodes.Ldc_I4_S, (sbyte)9));
            matcher.Advance(1)
               .InsertAndAdvance(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(UIBuildMenuPatches), nameof(OnUpdate_Patch))));
            matcher.SetOpcodeAndAdvance(OpCodes.Brfalse);

            return matcher.InstructionEnumeration();
        }

        [HarmonyPatch(typeof(UIFunctionPanel), nameof(UIFunctionPanel._OnUpdate))]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> UIFunctionPanel_OnUpdate_Transpiler_SetWidth(IEnumerable<CodeInstruction> instructions)
        {
            foreach (CodeInstruction ci in instructions)
            {
                if (ci.opcode == OpCodes.Ldc_R4)
                    switch ((float)ci.operand)
                    {
                        case 780f:
                            ci.operand = 830f;
                            break;

                        case 810f:
                            ci.operand = 860f;
                            break;

                        case 820f:
                            ci.operand = 870f;
                            break;
                    }

                yield return ci;
            }
        }

        public static bool OnUpdate_Patch(int currentCategory, int limit) => currentCategory <= limit || currentCategory == 12;

        public static int OnUpdate_For_Patch(int currentCategory) => currentCategory == 10 ? 12 : currentCategory;

        public static int OnUpdate_KeyCode_Patch(int keyCode) => keyCode == 60 ? 45 : keyCode; // '-' key
    }
}
