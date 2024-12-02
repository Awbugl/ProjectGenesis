using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using HarmonyLib;
using ProjectGenesis.Utils;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

// ReSharper disable InconsistentNaming

namespace ProjectGenesis.Patches.UI
{
    public static class ResearchLabPatches
    {
        [HarmonyPatch(typeof(UILabWindow), nameof(UILabWindow._OnCreate))]
        [HarmonyPostfix]
        public static void UILabWindow_OnCreate_Postfix(UILabWindow __instance)
        {
            __instance.GetComponent<RectTransform>().sizeDelta = new Vector2(640, 430);
            __instance.transform.Find("matrix-group/lines").gameObject.SetActive(false);

            const int len = 9;

            Array.Resize(ref __instance.itemButtons, len);
            Array.Resize(ref __instance.itemIcons, len);
            Array.Resize(ref __instance.itemPercents, len);
            Array.Resize(ref __instance.itemLocks, len);
            Array.Resize(ref __instance.itemCountTexts, len);
            Array.Resize(ref __instance.itemIncs, len * 3);

            GameObject itemButtonsGameObject = __instance.itemButtons[0].gameObject;

            for (var i = 6; i < len; i++)
            {
                GameObject newButton = Object.Instantiate(itemButtonsGameObject, itemButtonsGameObject.transform.parent);

                __instance.itemButtons[i] = newButton.GetComponent<UIButton>();
                __instance.itemIcons[i] = newButton.transform.Find("icon").GetComponent<Image>();
                __instance.itemPercents[i] = newButton.transform.Find("fg").GetComponent<Image>();
                __instance.itemLocks[i] = newButton.transform.Find("locked").GetComponent<Image>();
                __instance.itemCountTexts[i] = newButton.transform.Find("cnt-text").GetComponent<Text>();

                for (var j = 0; j < 3; j++)
                {
                    Transform transform = newButton.transform.Find("icon");
                    __instance.itemIncs[i * 3 + j] = transform.GetChild(j).GetComponent<Image>();
                }
            }

            SetPosition(__instance);
            SwapPosition(__instance, 4, 5);
        }

        private static void SetPosition(UILabWindow window)
        {
            for (var i = 0; i < window.itemButtons.Length; i++)
            {
                window.itemButtons[i].gameObject.GetComponent<RectTransform>().anchoredPosition =

                    // ReSharper disable once PossibleLossOfFraction
                    new Vector2((i % 3 - 1) * 105, i / 3 * -105 + 105);
            }
        }

        private static void SwapPosition(UILabWindow window, int pos1, int pos2)
        {
            RectTransform rectTransform1 = window.itemButtons[pos1].gameObject.GetComponent<RectTransform>();
            RectTransform rectTransform2 = window.itemButtons[pos2].gameObject.GetComponent<RectTransform>();

            (rectTransform1.anchoredPosition, rectTransform2.anchoredPosition) =
                (rectTransform2.anchoredPosition, rectTransform1.anchoredPosition);
        }

        [HarmonyPatch(typeof(UILabWindow), nameof(UILabWindow._OnUpdate))]
        [HarmonyPostfix]
        public static void UILabWindow_OnUpdate_Postfix(UILabWindow __instance)
        {
            LabComponent lab = __instance.factorySystem.labPool[__instance.labId];
            if (lab.id != __instance.labId)
            {
                __instance._Close();
                return;
            }

            if (lab.matrixMode)
            {
                for (var index = 0; index < __instance.itemButtons.Length; ++index)
                {
                    bool enabled = __instance.itemIcons[index].enabled;
                    UIButton button = __instance.itemButtons[index];
                    if (enabled)
                    {
                        RectTransform rectTransform = button.gameObject.GetComponent<RectTransform>();
                        float x = rectTransform.anchoredPosition.x;
                        rectTransform.anchoredPosition = new Vector2(x, 0);
                    }

                    button.gameObject.SetActive(enabled);
                }
            }
            else
            {
                for (var i = 0; i < __instance.itemButtons.Length; i++)
                {
                    UIButton button = __instance.itemButtons[i];
                    button.gameObject.SetActive(true);
                    button.gameObject.GetComponent<RectTransform>().anchoredPosition =

                        // ReSharper disable once PossibleLossOfFraction
                        new Vector2((i % 3 - 1) * 105, i / 3 * -105 + 105);
                }

                SwapPosition(__instance, 4, 5);
            }
        }

        [HarmonyPatch(typeof(UILabWindow), nameof(UILabWindow._OnUpdate))]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> UILabWindow_OnUpdate_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var matcher = new CodeMatcher(instructions);

            matcher.MatchForward(true, new CodeMatch(OpCodes.Ldloc_0),
                new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(LabComponent), nameof(LabComponent.timeSpend))));

            matcher.Advance(1).InsertAndAdvance(new CodeInstruction(OpCodes.Ldloc_0),
                new CodeInstruction(OpCodes.Call,
                    AccessTools.Method(typeof(ResearchLabPatches), nameof(UILabWindow_OnUpdate_Patch_Method))));

            matcher.MatchForward(false,
                new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(GameHistoryData), nameof(GameHistoryData.techSpeed))));

            matcher.MatchForward(false, new CodeMatch(OpCodes.Stloc_S));

            matcher.InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ResearchLabPatches), nameof(UISetResearchSpeed))));

            return matcher.InstructionEnumeration();
        }

        [HarmonyPatch(typeof(LabComponent), nameof(LabComponent.SetFunction))]
        [HarmonyPatch(typeof(LabComponent), nameof(LabComponent.InternalUpdateAssemble))]
        [HarmonyPatch(typeof(FactorySystem), nameof(FactorySystem.GameTickLabResearchMode))]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> LabComponent_SetFunction_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var matcher = new CodeMatcher(instructions);

            matcher.MatchForward(false,
                new CodeMatch(OpCodes.Ldsfld, AccessTools.Field(typeof(LabComponent), nameof(LabComponent.matrixIds))),
                new CodeMatch(OpCodes.Ldc_I4_0));

            matcher.InsertAndAdvance(new CodeInstruction(OpCodes.Call,
                AccessTools.Method(typeof(ResearchLabPatches), nameof(ChangeMatrixIds))));

            return matcher.InstructionEnumeration();
        }

        [HarmonyPatch(typeof(FactorySystem), nameof(FactorySystem.GameTickLabResearchMode))]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> FactorySystem_GameTickLabResearchMode_SetResearchSpeed(
            IEnumerable<CodeInstruction> instructions)
        {
            var matcher = new CodeMatcher(instructions);

            matcher.MatchForward(false,
                new CodeMatch(new CodeInstruction(OpCodes.Call,
                    AccessTools.Method(typeof(LabComponent), nameof(LabComponent.InternalUpdateResearch)))));

            var techSpeed = matcher.Advance(-6).Operand;

            var local = matcher.Advance(-2).Operand;

            matcher.InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_0), new CodeInstruction(OpCodes.Ldloc_S, local),
                new CodeInstruction(OpCodes.Ldloc_S, techSpeed),
                new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ResearchLabPatches), nameof(SetResearchSpeed))),
                new CodeInstruction(OpCodes.Stloc_S, techSpeed));

            return matcher.InstructionEnumeration();
        }

        public static float UISetResearchSpeed(float techSpeed, UILabWindow window)
        {
            LabComponent lab = window.factorySystem.labPool[window.labId];
            short modelIndex = window.factory.entityPool[lab.entityId].modelIndex;
            var labResearchSpeed = PlanetFactory.PrefabDescByModelIndex[modelIndex].labResearchSpeed;
            return techSpeed * labResearchSpeed;
        }

        public static float SetResearchSpeed(FactorySystem system, ref LabComponent component, float techSpeed)
        {
            short modelIndex = system.factory.entityPool[component.entityId].modelIndex;
            var labResearchSpeed = PlanetFactory.PrefabDescByModelIndex[modelIndex].labResearchSpeed;
            return techSpeed * labResearchSpeed;
        }

        [HarmonyPatch(typeof(LabMatrixEffect), nameof(LabMatrixEffect.Update))]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> LabMatrixEffect_Update_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var matcher = new CodeMatcher(instructions);

            matcher.MatchForward(false, new CodeMatch(OpCodes.Ldloc_S), new CodeMatch(OpCodes.Brfalse), new CodeMatch(OpCodes.Ldc_I4_0));

            object label = matcher.Advance(1).Operand;

            matcher.Advance(1).InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_0), new CodeInstruction(OpCodes.Ldloc_3),
                new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ResearchLabPatches), nameof(LabMatrixEffect_Patch_Method))),
                new CodeInstruction(OpCodes.Br_S, label));

            return matcher.InstructionEnumeration();
        }

        [HarmonyPatch(typeof(FactorySystem), nameof(FactorySystem.GameTickLabResearchMode))]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> FactorySystem_GameTickLabResearchMode_Transpiler(
            IEnumerable<CodeInstruction> instructions, ILGenerator ilGenerator)
        {
            var matcher = new CodeMatcher(instructions, ilGenerator);

            matcher.MatchForward(false, new CodeMatch(OpCodes.Ldc_I4_5), new CodeMatch(), new CodeMatch(OpCodes.Ldc_I4_S, (sbyte)32),
                new CodeMatch(OpCodes.Stloc_S));

            object num2 = matcher.Advance(-1).Operand;
            object brlabel = matcher.Advance(2).Operand;
            object index1 = matcher.Advance(2).Operand;

            matcher.Advance(2).InsertAndAdvance(new CodeInstruction(OpCodes.Ldloc_S, num2));
            matcher.CreateLabelAt(matcher.Pos - 1, out Label label);
            matcher.Advance(-5).Operand = label;

            matcher.Advance(5).InsertAndAdvance(new CodeInstruction(OpCodes.Ldloc_S, index1),
                new CodeInstruction(OpCodes.Call,
                    AccessTools.Method(typeof(ResearchLabPatches), nameof(FactorySystem_GameTickLabResearchMode_Patch_Method))),
                new CodeInstruction(OpCodes.Stloc_S, index1), new CodeInstruction(OpCodes.Br_S, brlabel));

            return matcher.InstructionEnumeration();
        }

        [HarmonyPatch(typeof(LabComponent), nameof(LabComponent.InternalUpdateResearch))]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> LabComponent_InternalUpdateResearch_Transpiler(IEnumerable<CodeInstruction> instructions,
            ILGenerator ilGenerator)
        {
            var matcher = new CodeMatcher(instructions, ilGenerator);

            matcher.MatchForward(false, new CodeMatch(OpCodes.Ldarg_0),
                new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(LabComponent), nameof(LabComponent.matrixPoints))));

            CodeMatcher matcher2 = matcher.Clone();

            matcher2.MatchForward(false, new CodeMatch(OpCodes.Ldarg_0),
                new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(LabComponent), nameof(LabComponent.matrixPoints))),
                new CodeMatch(OpCodes.Ldc_I4_5));

            var label = matcher2.Advance(5).Operand;

            matcher.InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Call,
                    AccessTools.Method(typeof(ResearchLabPatches), nameof(LabComponent_InternalUpdateResearch_Patch_Method))),
                new CodeInstruction(OpCodes.Dup), new CodeInstruction(OpCodes.Stloc_0), new CodeInstruction(OpCodes.Brtrue, label),
                new CodeInstruction(OpCodes.Ldc_I4_0), new CodeInstruction(OpCodes.Ret));

            return matcher.InstructionEnumeration();
        }

        [HarmonyPatch(typeof(PlanetFactory), nameof(PlanetFactory.InsertInto))]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> PlanetFactory_InsertInto_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var matcher = new CodeMatcher(instructions);

            matcher.MatchForward(false, new CodeMatch(OpCodes.Ldc_I4, 6001));

            matcher.InsertAndAdvance(new CodeInstruction(OpCodes.Call,
                AccessTools.Method(typeof(ResearchLabPatches), nameof(ChangeMatrixIds))));

            matcher.MatchForward(false, new CodeMatch(OpCodes.Ldc_I4_6));

            matcher.SetInstructionAndAdvance(new CodeInstruction(OpCodes.Ldsfld,
                AccessTools.Field(typeof(LabComponent), nameof(LabComponent.matrixIds))));
            matcher.InsertAndAdvance(new CodeInstruction(OpCodes.Ldlen));

            return matcher.InstructionEnumeration();
        }

        [HarmonyPatch(typeof(LabComponent), nameof(LabComponent.UpdateNeedsResearch))]
        [HarmonyPrefix]
        [HarmonyPriority(Priority.First)]
        public static bool LabComponent_UpdateNeedsResearch_Prefix(ref LabComponent __instance)
        {
            TechProto tech = LDB.techs.Select(__instance.techId);

            if (tech?.Items == null)
            {
                Array.Fill(__instance.needs, 0);
                return false;
            }

            var needIndex = 0;

            foreach (int need in tech.Items)
            {
                int itemIndex = Array.IndexOf(LabComponent.matrixIds, need);

                if (itemIndex >= 0 && __instance.matrixServed[itemIndex] < 36000) __instance.needs[needIndex++] = need;
            }

            for (int i = needIndex; i < __instance.needs.Length; i++) __instance.needs[i] = 0;

            return false;
        }

        public static int LabComponent_InternalUpdateResearch_Patch_Method(ref LabComponent labComponent)
        {
            int speed = (int)(GameMain.history.techSpeed + 2.0);

            for (var i = 0; i < LabComponent.matrixIds.Length; i++)
            {
                if (labComponent.matrixPoints[i] <= 0) continue;

                int point = labComponent.matrixServed[i] / labComponent.matrixPoints[i];

                if (point >= speed) continue;

                speed = point;

                if (speed != 0) continue;

                labComponent.replicating = false;
                return 0;
            }

            return speed;
        }

        public static void LabMatrixEffect_Patch_Method(LabMatrixEffect labMatrixEffect, TechProto techProto)
        {
            foreach (int item in techProto.Items)
            {
                switch (item)
                {
                    case ProtoID.I通量矩阵:
                        labMatrixEffect.techMatUse[0] = true;
                        labMatrixEffect.techMatUse[1] = true;
                        break;

                    case ProtoID.I张量矩阵:
                        labMatrixEffect.techMatUse[2] = true;
                        labMatrixEffect.techMatUse[3] = true;
                        break;

                    case ProtoID.I奇点矩阵:
                        labMatrixEffect.techMatUse[0] = true;
                        labMatrixEffect.techMatUse[1] = true;
                        labMatrixEffect.techMatUse[2] = true;
                        labMatrixEffect.techMatUse[3] = true;
                        labMatrixEffect.techMatUse[4] = true;
                        break;

                    default:
                        labMatrixEffect.techMatUse[item - LabComponent.matrixIds[0]] = true;
                        break;
                }
            }
        }

        public static int UILabWindow_OnUpdate_Patch_Method(int timeSpend, LabComponent component) =>
            timeSpend / component.productCounts[0];

        public static int ChangeMatrixIds(int itemId)
        {
            switch (itemId)
            {
                case ProtoID.I通量矩阵: return 6007;
                case ProtoID.I张量矩阵: return 6008;
                case ProtoID.I奇点矩阵: return 6009;
                default: return itemId;
            }
        }

        public static int FactorySystem_GameTickLabResearchMode_Patch_Method(int num2, int index1)
        {
            switch (num2)
            {
                case 6: //ProtoID.I通量矩阵:
                    index1 |= 3;
                    break;

                case 7: //ProtoID.I张量矩阵:
                    index1 |= 12;
                    break;

                case 8: //ProtoID.I奇点矩阵:
                    index1 |= 31;
                    break;
            }

            return index1;
        }

        [HarmonyPatch(typeof(LabComponent), nameof(LabComponent.UpdateOutputToNext))]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> LabComponent_UpdateOutputToNext_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var matcher = new CodeMatcher(instructions);

            matcher.MatchForward(true, new CodeMatch(OpCodes.Ldc_I4, 6006));

            object leaveLabel = matcher.Advance(1).Operand;

            matcher.Start().MatchForward(false,
                new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(LabComponent), nameof(LabComponent.nextLabId))),
                new CodeMatch(OpCodes.Ldelema),
                new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(LabComponent), nameof(LabComponent.needs))),
                new CodeMatch(OpCodes.Ldc_I4_0), new CodeMatch(OpCodes.Ldelem_I4));

            matcher.InsertAndAdvance(
                new CodeInstruction(OpCodes.Call,
                    AccessTools.Method(typeof(ResearchLabPatches), nameof(LabComponent_UpdateOutputToNext_Patch_Method))),
                new CodeInstruction(OpCodes.Br, leaveLabel));

            matcher.SetInstructionAndAdvance(new CodeInstruction(OpCodes.Nop)).SetInstructionAndAdvance(new CodeInstruction(OpCodes.Nop))
               .SetInstructionAndAdvance(new CodeInstruction(OpCodes.Nop)).SetInstructionAndAdvance(new CodeInstruction(OpCodes.Nop))
               .SetInstructionAndAdvance(new CodeInstruction(OpCodes.Nop)).SetInstructionAndAdvance(new CodeInstruction(OpCodes.Nop))
               .SetInstructionAndAdvance(new CodeInstruction(OpCodes.Nop));

            return matcher.InstructionEnumeration();
        }

        public static void LabComponent_UpdateOutputToNext_Patch_Method(LabComponent[] labPool, ref LabComponent labComponent)
        {
            ref LabComponent next = ref labPool[labComponent.nextLabId];

            for (var i = 0; i < LabComponent.matrixIds.Length; i++)
            {
                if (labComponent.matrixServed[i] >= 7200 && next.matrixServed[i] < 36000)
                {
                    int p = (labComponent.matrixServed[i] - 7200) / 3600 * 3600;
                    if (p > 36000) p = 36000;
                    int num = labComponent.split_inc(ref labComponent.matrixServed[i], ref labComponent.matrixIncServed[i], p);
                    next.matrixIncServed[i] += num;
                    next.matrixServed[i] += p;
                }
            }
        }

        [HarmonyPatch(typeof(TechProto), nameof(TechProto.GenPropertyOverrideItems))]
        [HarmonyPostfix]
        public static void TechProto_GenPropertyOverrideItems_Postfix(TechProto proto)
        {
            if (proto.PropertyOverrideItemArray?.Length > 0) return;

            if (!proto.Items.Any(i => Array.IndexOf(LabComponent.matrixIds, i) > 5)) return;

            var dict = new Dictionary<int, int>();

            long hashNeeded = proto.GetHashNeeded(proto.Level);

            int index;

            for (index = 0; index < proto.Items.Length; index++)
            {
                int item = proto.Items[index];
                int num = (int)(hashNeeded * proto.ItemPoints[index]) / 3600;

                switch (item)
                {
                    case ProtoID.I通量矩阵:
                        AddCount(ProtoID.I电磁矩阵, num);
                        AddCount(ProtoID.I能量矩阵, num);
                        break;

                    case ProtoID.I张量矩阵:
                        AddCount(ProtoID.I结构矩阵, num);
                        AddCount(ProtoID.I信息矩阵, num);
                        break;

                    case ProtoID.I奇点矩阵:
                        AddCount(ProtoID.I电磁矩阵, num);
                        AddCount(ProtoID.I能量矩阵, num);
                        AddCount(ProtoID.I结构矩阵, num);
                        AddCount(ProtoID.I信息矩阵, num);
                        AddCount(ProtoID.I引力矩阵, num);
                        break;

                    default:
                        AddCount(item, num);
                        break;
                }
            }

            proto.PropertyOverrideItemArray = new IDCNT[dict.Count];

            index = 0;

            foreach (var (item, count) in dict) proto.PropertyOverrideItemArray[index++] = new IDCNT(item, count);

            return;

            void AddCount(int itemId, int count)
            {
                if (!dict.TryAdd(itemId, count)) dict[itemId] += count;
            }
        }
    }
}
