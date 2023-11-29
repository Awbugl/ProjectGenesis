﻿using HarmonyLib;
using ProjectGenesis.Utils;
using UnityEngine;
using UnityEngine.UI;
using static ProjectGenesis.ProjectGenesis;

// ReSharper disable InconsistentNaming

namespace ProjectGenesis.Patches.UI
{
    public static class UIOptionWindowPatches
    {
        private static UIToggle LDBToolCacheToggle, HideTechModeToggle, DisableMessageToggle;

        private static void Init()
        {
            GameObject queryObj
                = GameObject.Find("UI Root/Overlay Canvas/Top Windows/Option Window/details/content-3/list/scroll-view/viewport/content/demolish-query");

            Transform pageParent = GameObject.Find("UI Root/Overlay Canvas/Top Windows/Option Window/details/content-5/advisor-tips").transform
                                             .parent;

            CreateSettingObject(queryObj, pageParent, "gb-ldbtc-setting", "UseLDBToolCache".TranslateFromJson(),
                                "UseLDBToolCacheAdditionalText".TranslateFromJson(), new Vector2(30, -180), LDBToolCacheValue,
                                out LDBToolCacheToggle);

            CreateSettingObject(queryObj, pageParent, "gb-htc-setting", "EnableHideTechMode".TranslateFromJson(),
                                "EnableHideTechModeAdditionalText".TranslateFromJson(), new Vector2(30, -220), HideTechModeValue,
                                out HideTechModeToggle);

            CreateSettingObject(queryObj, pageParent, "gb-smb-setting", "DisableMessageBox".TranslateFromJson(),
                                "DisableMessageBoxAdditionalText".TranslateFromJson(), new Vector2(30, -260), DisableMessageBoxValue,
                                out DisableMessageToggle);
        }

        private static void CreateSettingObject(
            GameObject oriObj,
            Transform parent,
            string name,
            string text,
            string additionalText,
            Vector2 position,
            bool defaultValue,
            out UIToggle toggle)
        {
            GameObject settingObj = Object.Instantiate(oriObj, parent);

            settingObj.name = name;
            Object.DestroyImmediate(settingObj.GetComponent<Localizer>());
            settingObj.GetComponent<Text>().text = text;

            var settingObjTransform = (RectTransform)settingObj.transform;
            settingObjTransform.anchoredPosition = position;

            toggle = settingObj.GetComponentInChildren<UIToggle>();
            toggle.isOn = defaultValue;
            toggle.toggle.onValueChanged.RemoveAllListeners();

            Transform additonalText = settingObj.transform.GetChild(1);
            Object.DestroyImmediate(additonalText.GetComponent<Localizer>());
            additonalText.GetComponent<Text>().text = additionalText;
        }

        [HarmonyPatch(typeof(UIOptionWindow), "_OnOpen")]
        [HarmonyPostfix]
        public static void UIOptionWindow_OnOpen_Postfix()
        {
            if (!LDBToolCacheToggle) Init();

            Reset();
        }

        [HarmonyPatch(typeof(UIOptionWindow), "OnRevertButtonClick")]
        [HarmonyPostfix]
        public static void Reset()
        {
            LDBToolCacheToggle.isOn = LDBToolCacheValue;
            HideTechModeToggle.isOn = HideTechModeValue;
            DisableMessageToggle.isOn = DisableMessageBoxValue;
        }

        [HarmonyPatch(typeof(UIOptionWindow), "OnApplyClick")]
        [HarmonyPostfix]
        public static void UIOptionWindow_OnApplyClick_Postfix()
            => SetConfig(LDBToolCacheToggle.isOn, HideTechModeToggle.isOn, DisableMessageToggle.isOn);
    }
}