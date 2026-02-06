using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

// ReSharper disable InconsistentNaming

namespace ProjectGenesis.Patches
{
    /// <summary>
    ///     special thanks for
    ///     https://github.com/Shad0wlife/DSP_AssemblerUI  
    /// </summary>
    public static class AssemblerSpeedPatches
    {
        private const int MaxItems = 5;

        private const float InputY = 25f, OutputY = -50f;
        private const float InputStartX = 130f, OutputStartX = 1f;
        private const float InputSpacing = 50f, OutputSpacing = -64f;
        private static string PerMinuteText;

        private sealed class LabelData
        {
            public GameObject GameObject;
            public RectTransform RectTransform;
            public Text Text;
            public ContentSizeFitter Fitter;
            public float LastValue = -1f;
            public float Width;
            public bool IsActive;
        }

        private static readonly LabelData[,] LabelGrid = new LabelData[MaxItems, 2]; // [index, 0=input/1=output]
        private static readonly bool[,] LabelExists = new bool[MaxItems, 2];

        private static Transform _parent;
        private static Vector3 _vanillaPos;
        private static GameObject _template;
        private static int _currentInputs, _currentOutputs;

        [HarmonyPatch(typeof(UIAssemblerWindow), nameof(UIAssemblerWindow._OnCreate))]
        [HarmonyPostfix]
        private static void UIAssemblerWindow_OnCreate()
        {
            _template = UIRoot.instance.uiGame.assemblerWindow.speedText.gameObject;
            if (_template == null) return;

            _parent = _template.transform.parent;
            _vanillaPos = _template.transform.localPosition;

            PerMinuteText = "每分钟".Translate();

            var localizer = _template.GetComponentInChildren<Localizer>();
            if (localizer != null) Object.Destroy(localizer);
        }

        private static GameObject CreateNewLabelObject()
        {
            var go = Object.Instantiate(_template, _parent, false);

            var fitter = go.GetComponent<ContentSizeFitter>();
            if (fitter == null)
            {
                fitter = go.AddComponent<ContentSizeFitter>();
                fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
                fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            }

            var data = go.AddComponent<LabelCache>();
            data.rect = go.GetComponent<RectTransform>();
            data.text = go.GetComponent<Text>();
            data.fitter = fitter;

            return go;
        }

        private class LabelCache : MonoBehaviour
        {
            public RectTransform rect;
            public Text text;
            public ContentSizeFitter fitter;
        }

        [HarmonyPatch(typeof(UIAssemblerWindow), nameof(UIAssemblerWindow.OnServingBoxChange))]
        [HarmonyPostfix]
        public static void OnServingBoxChange(UIAssemblerWindow __instance)
        {
            if (__instance.assemblerId == 0 || __instance.factory == null) return;

            ref var assembler = ref __instance.factorySystem.assemblerPool[__instance.assemblerId];
            if (assembler.recipeId <= 0) return;

            var recipeData = assembler.recipeExecuteData;

            SetupSide(recipeData?.requireCounts?.Length, true);
            SetupSide(recipeData?.productCounts?.Length, false);
        }

        private static void SetupSide(int? count, bool isInput)
        {
            int actualCount = Math.Min(count ?? 0, MaxItems);
            int typeIndex = isInput ? 0 : 1;
            ref int current = ref isInput ? ref _currentInputs : ref _currentOutputs;


            for (int i = 0; i < actualCount; i++)
            {
                if (!LabelExists[i, typeIndex]) { CreateLabel(i, typeIndex, actualCount, isInput); }
                else if (!LabelGrid[i, typeIndex].IsActive)
                {
                    LabelGrid[i, typeIndex].GameObject.SetActive(true);
                    LabelGrid[i, typeIndex].IsActive = true;
                }

                UpdateLabelLayout(i, typeIndex, actualCount, isInput);
            }

            for (int i = actualCount; i < current; i++)
            {
                if (!LabelExists[i, typeIndex]) continue;

                var label = LabelGrid[i, typeIndex];
                label.GameObject.SetActive(false);
                label.IsActive = false;
            }

            current = actualCount;
        }

        private static void CreateLabel(int index, int typeIndex, int total, bool isInput)
        {
            var go = CreateNewLabelObject();
            var cache = go.GetComponent<LabelCache>();

            go.name = isInput ? $"speed-in-{index}" : $"speed-out-{index}";

            var data = new LabelData
            {
                GameObject = go,
                RectTransform = cache.rect,
                Text = cache.text,
                Fitter = cache.fitter,
                IsActive = true,
                Width = cache.text.preferredWidth,
            };

            LabelGrid[index, typeIndex] = data;
            LabelExists[index, typeIndex] = true;

            PositionLabel(ref data, index, total, isInput);
        }

        private static void UpdateLabelLayout(int index, int typeIndex, int total, bool isInput)
        {
            var label = LabelGrid[index, typeIndex];
            if (label.Fitter != null)
            {
                label.Fitter.SetLayoutHorizontal();
                label.Fitter.SetLayoutVertical();
                label.Width = label.Text.preferredWidth;
            }

            PositionLabel(ref label, index, total, isInput);
        }

        private static void PositionLabel(ref LabelData label, int index, int total, bool isInput)
        {
            float x = isInput
                ? InputStartX - label.Width * 0.5f + index * InputSpacing
                : OutputStartX - label.Width * 0.5f + (total - 1 - index) * OutputSpacing;

            float y = isInput ? InputY : OutputY;
            label.RectTransform.localPosition = new Vector3(x, y, _vanillaPos.z);
        }

        [HarmonyPatch(typeof(UIAssemblerWindow), nameof(UIAssemblerWindow._OnUpdate))]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> UIAssemblerWindow_OnUpdate_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var matcher = new CodeMatcher(instructions);

            /*
                IL_0805: stloc.s      V_10
                IL_0807: ldarg.0      // this
                IL_0808: ldfld        class [UnityEngine.UI]UnityEngine.UI.Text UIAssemblerWindow::speedText
                IL_080d: ldloca.s     V_10
                IL_080f: ldstr        "0.0"
             */

            matcher.MatchForward(false, new CodeMatch(OpCodes.Ldarg_0),
                new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(UIAssemblerWindow), nameof(UIAssemblerWindow.speedText))));

            var v10 = matcher.InstructionAt(-1).operand;

            matcher.InsertAndAdvance(new CodeInstruction(OpCodes.Ldloc_S, v10), new CodeInstruction(OpCodes.Ldloc_0),
                new CodeInstruction(OpCodes.Ldfld,
                    AccessTools.Field(typeof(AssemblerComponent), nameof(AssemblerComponent.recipeExecuteData))),
                new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(AssemblerSpeedPatches), nameof(BatchUpdate))));

            return matcher.InstructionEnumeration();
        }

        public static void BatchUpdate(float baseSpeed, RecipeExecuteData data)
        {
            int inCount = Math.Min(data.requireCounts.Length, _currentInputs);
            for (int i = 0; i < inCount; i++)
                if (LabelExists[i, 0])
                    UpdateLabelValue(ref LabelGrid[i, 0], data.requireCounts[i] * baseSpeed, i, _currentInputs, true);

            int outCount = Math.Min(data.productCounts.Length, _currentOutputs);
            for (int i = 0; i < outCount; i++)
                if (LabelExists[i, 1])
                    UpdateLabelValue(ref LabelGrid[i, 1], data.productCounts[i] * baseSpeed, i, _currentOutputs, false);
        }

        private static void UpdateLabelValue(ref LabelData label, float value, int index, int total, bool isInput)
        {
            if (Mathf.Abs(value - label.LastValue) < 0.5f) return;

            label.LastValue = value;
            label.Text.text = $"{value:0.#}{PerMinuteText}";

            float newWidth = label.Text.preferredWidth;
            if (!(Mathf.Abs(newWidth - label.Width) > 0.1f)) return;

            label.Width = newWidth;
            PositionLabel(ref label, index, total, isInput);
        }
    }
}
