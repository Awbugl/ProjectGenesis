using HarmonyLib;
using ProjectGenesis.Utils;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using static ProjectGenesis.ProjectGenesis;

namespace ProjectGenesis.Patches.UI
{
    public static class UISettingPatches
    {
        private static bool _currentAtmosphericEmission, _currentLDBToolCache, _currentHideTechMode;

        [HarmonyPatch(typeof(VFPreload), "InvokeOnLoadWorkEnded")]
        [HarmonyPostfix]
        [HarmonyPriority(Priority.Last)]
        public static void VFPreload_InvokeOnLoadWorkEnded_Postfix()
        {
            var queryObj
                = GameObject.Find("UI Root/Overlay Canvas/Top Windows/Option Window/details/content-3/list/scroll-view/viewport/content/demolish-query");

            var pageParent = GameObject.Find("UI Root/Overlay Canvas/Top Windows/Option Window/details/content-5/advisor-tips").transform.parent;

            CreateSettingObject(queryObj, pageParent, "gb-ae-setting", "EnableAtmosphericEmission".TranslateFromJson(),
                                "EnableAtmosphericEmissionAdditionalText".TranslateFromJson(), new Vector2(30, -180), AtmosphericEmissionValue,
                                SetAtmosphericEmissionValue);

            CreateSettingObject(queryObj, pageParent, "gb-ldbtc-setting", "EnableLDBToolCache".TranslateFromJson(),
                                "EnableLDBToolCacheAdditionalText".TranslateFromJson(), new Vector2(30, -220), LDBToolCacheValue,
                                SetLDBToolCacheValue);

            CreateSettingObject(queryObj, pageParent, "gb-htc-setting", "EnableHideTechMode".TranslateFromJson(),
                                "EnableHideTechModeAdditionalText".TranslateFromJson(), new Vector2(30, -260), HideTechModeValue,
                                SetHideTechModeValue);
        }

        private static void SetAtmosphericEmissionValue(bool value) => _currentAtmosphericEmission = value;

        private static void SetLDBToolCacheValue(bool value) => _currentLDBToolCache = value;

        private static void SetHideTechModeValue(bool value) => _currentHideTechMode = value;

        private static void CreateSettingObject(
            GameObject oriObj,
            Transform parent,
            string name,
            string text,
            string additionalText,
            Vector2 position,
            bool defaultValue,
            UnityAction<bool> action)
        {
            var settingObj = Object.Instantiate(oriObj, parent);

            settingObj.name = name;
            Object.DestroyImmediate(settingObj.GetComponent<Localizer>());
            settingObj.GetComponent<Text>().text = text;

            var settingObjTransform = (RectTransform)settingObj.transform;
            settingObjTransform.anchoredPosition = position;

            var toggle = settingObj.GetComponentInChildren<UIToggle>();
            toggle.isOn = defaultValue;
            toggle.toggle.onValueChanged.RemoveAllListeners();
            toggle.toggle.onValueChanged.AddListener(action);

            var additonalText = settingObj.transform.GetChild(1);
            Object.DestroyImmediate(additonalText.GetComponent<Localizer>());
            additonalText.GetComponent<Text>().text = additionalText;
        }

        [HarmonyPatch(typeof(UIOptionWindow), "OnCancelClick")]
        [HarmonyPostfix]
        public static void UIOptionWindow_OnCancelClick_Postfix()
        {
            _currentAtmosphericEmission = AtmosphericEmissionValue;
            _currentLDBToolCache = LDBToolCacheValue;
            _currentHideTechMode = HideTechModeValue;
        }

        [HarmonyPatch(typeof(UIOptionWindow), "OnApplyClick")]
        [HarmonyPostfix]
        public static void UIOptionWindow_OnApplyClick_Postfix()
            => SetConfig(_currentAtmosphericEmission, _currentLDBToolCache, _currentHideTechMode);
    }
}
