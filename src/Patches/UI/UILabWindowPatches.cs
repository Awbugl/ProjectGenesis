using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using ProjectGenesis.Utils;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

// ReSharper disable InconsistentNaming

namespace ProjectGenesis.Patches.UI
{
    public static class UILabWindowPatches
    {
        [HarmonyPatch(typeof(UILabWindow), nameof(UILabWindow._OnCreate))]
        [HarmonyPostfix]
        public static void UILabWindow_OnCreate_Postfix(UILabWindow __instance)
        {
            __instance.GetComponent<RectTransform>().sizeDelta = new Vector2(640, 420);
            __instance.transform.Find("matrix-group/lines").gameObject.SetActive(false);

            const int len = 9;

            Array.Resize(ref __instance.itemButtons, len);
            Array.Resize(ref __instance.itemIcons, len);
            Array.Resize(ref __instance.itemPercents, len);
            Array.Resize(ref __instance.itemLocks, len);
            Array.Resize(ref __instance.itemCountTexts, len);
            Array.Resize(ref __instance.itemIncs, len * 3);

            var itemButtonsGameObject = __instance.itemButtons[0].gameObject;

            for (int i = 6; i < len; i++)
            {
                var newButton = Object.Instantiate(itemButtonsGameObject,
                    itemButtonsGameObject.transform.parent);

                __instance.itemButtons[i] = newButton.GetComponent<UIButton>();
                __instance.itemIcons[i] = newButton.transform.Find("icon").GetComponent<Image>();
                __instance.itemPercents[i] = newButton.transform.Find("fg").GetComponent<Image>();
                __instance.itemLocks[i] = newButton.transform.Find("locked").GetComponent<Image>();
                __instance.itemCountTexts[i] = newButton.transform.Find("cnt-text").GetComponent<Text>();

                for (int j = 0; j < 3; j++)
                {
                    var transform = newButton.transform.Find("icon");
                    __instance.itemIncs[i * 3 + j] = transform.GetChild(j).GetComponent<Image>();
                }
            }

            for (var i = 0; i < __instance.itemButtons.Length; i++)
            {
                __instance.itemButtons[i].gameObject.GetComponent<RectTransform>().anchoredPosition =
                    // ReSharper disable once PossibleLossOfFraction
                    new Vector2((i % 3 - 1) * 105, (i / 3) * -105 + 105);
            }

            SwapPosition(__instance, 4, 5);
        }

        private static void SwapPosition(UILabWindow window, int pos1, int pos2)
        {
            var rectTransform1 = window.itemButtons[pos1].gameObject.GetComponent<RectTransform>();
            var rectTransform2 = window.itemButtons[pos2].gameObject.GetComponent<RectTransform>();

            (rectTransform1.anchoredPosition, rectTransform2.anchoredPosition) =
                (rectTransform2.anchoredPosition, rectTransform1.anchoredPosition);
        }

        [HarmonyPatch(typeof(LabComponent), nameof(LabComponent.InternalUpdateAssemble))]
        [HarmonyPostfix]
        public static void LabComponent_InternalUpdateAssemble_Postfix(ref LabComponent __instance, ref uint __result)
        {
            if (__result > 6) __result = 6;
        }
        
        [HarmonyPatch(typeof(LabComponent), nameof(LabComponent.InternalUpdateResearch))]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> LabComponent_InternalUpdateResearch_Transpiler(
            IEnumerable<CodeInstruction> instructions)
        {
        //IL_012f: ldarg.0      // this
        //IL_0130: ldc.i4.1
        //IL_0131: stfld        bool LabComponent::replicating
            var matcher = new CodeMatcher(instructions);

            matcher.MatchForward(false,
                
                new CodeMatch(OpCodes.Ldarg_0),
                
                new CodeMatch(OpCodes.Ldc_I4_1),
                
                new CodeMatch(OpCodes.Stfld,
                    AccessTools.Field(typeof(LabComponent), nameof(LabComponent.replicating))));

            matcher.InsertAndAdvance(new CodeInstruction(OpCodes.Call,
                AccessTools.Method(typeof(UILabWindowPatches), nameof(Patch_Method))));

            return matcher.InstructionEnumeration();
        }
        

        [HarmonyPatch(typeof(LabComponent), nameof(LabComponent.SetFunction))]
        [HarmonyPatch(typeof(LabMatrixEffect), nameof(LabMatrixEffect.Update))]
        [HarmonyPatch(typeof(FactorySystem), nameof(FactorySystem.GameTickLabResearchMode))]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> LabComponent_SetFunction_Transpiler(
            IEnumerable<CodeInstruction> instructions)
        {
            var matcher = new CodeMatcher(instructions);

            matcher.MatchForward(false,
                new CodeMatch(OpCodes.Ldsfld,
                    AccessTools.Field(typeof(LabComponent), nameof(LabComponent.matrixIds))));

            matcher.InsertAndAdvance(new CodeInstruction(OpCodes.Call,
                AccessTools.Method(typeof(UILabWindowPatches), nameof(Patch_Method))));

            return matcher.InstructionEnumeration();
        }

        [HarmonyPatch(typeof(FactorySystem), nameof(FactorySystem.GameTickLabResearchMode))]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> FactorySystem_GameTickLabResearchMode_Transpiler(
            IEnumerable<CodeInstruction> instructions, ILGenerator ilGenerator)
        {
            var matcher = new CodeMatcher(instructions, ilGenerator);

            /*
                  IL_0147: br.s         IL_0154
                  IL_0149: ldloc.s      num2
                  IL_014b: ldc.i4.5
                  IL_014c: bne.un.s     IL_0154
                  IL_014e: ldc.i4.s     32 // 0x20
                  IL_0150: stloc.s      index1
                  IL_0152: br.s         IL_0167
                  IL_0154: ldloc.s      index2
             */

            matcher.MatchForward(false,
                new CodeMatch(OpCodes.Ldc_I4_5),
                new CodeMatch(),
                new CodeMatch(OpCodes.Ldc_I4_S, (sbyte)32),
                new CodeMatch(OpCodes.Stloc_S));

            var num2 = matcher.Advance(-1).Operand;

            var brlabel = matcher.Advance(-1).Operand;

            matcher.CreateLabel(out var label);

            matcher.Operand = label;

            var index1 = matcher.Advance(5).Operand;

            matcher.Advance(2).InsertAndAdvance(new CodeInstruction(OpCodes.Ldloc_S, num2));

            matcher.AddLabelsAt(matcher.Pos - 1, new[] { label });

            matcher.InsertAndAdvance(new CodeInstruction(OpCodes.Ldloc_S, index1),
                new CodeInstruction(OpCodes.Call,
                    AccessTools.Method(typeof(UILabWindowPatches), nameof(Patch_Method2))),
                new CodeInstruction(OpCodes.Stloc_S, index1),
                new CodeInstruction(OpCodes.Br_S, brlabel));

            return matcher.InstructionEnumeration();
        }

        public static int Patch_Method(int itemId)
        {
            switch (itemId)
            {
                case ProtoID.I通量矩阵: return 6007;
                case ProtoID.I空间矩阵: return 6008;
                case ProtoID.I宇宙矩阵粗坯: return 6009;
                default: return itemId;
            }
        }

        public static int Patch_Method2(int num2, int index1)
        {
            switch (num2)
            {
                case 6: //ProtoID.I通量矩阵:
                    index1 |= 3;
                    break;
                case 7: //ProtoID.I空间矩阵:
                    index1 |= 12;
                    break;
                case 8: //ProtoID.I宇宙矩阵粗坯:
                    index1 |= 31;
                    break;
            }

            return index1;
        }
    }
}