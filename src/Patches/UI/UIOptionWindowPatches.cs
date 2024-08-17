using System.Collections.Generic;
using HarmonyLib;
using ProjectGenesis.Utils;
using UnityEngine;
using UnityEngine.UI;
using static ProjectGenesis.ProjectGenesis;

// ReSharper disable InconsistentNaming

namespace ProjectGenesis.Patches.UI
{
    public static class UIOptionWindowPatches
    {
        private static UIToggle LDBToolCacheToggle,
            HideTechModeToggle,
            ShowMessageToggle;

        private static UIComboBox ProductOverflowComboBox;

        private static void Init()
        {
            var queryObj = GameObject.Find("UI Root/Overlay Canvas/Top Windows/Option Window/details/content-3/list/scroll-view/viewport/content/demolish-query");

            var languageObj = GameObject.Find("UI Root/Overlay Canvas/Top Windows/Option Window/details/content-5/language");

            Transform pageParent = languageObj.transform.parent;

            CreateSettingObject(queryObj, pageParent, "gb-ldbtc-setting", "UseLDBToolCache".TranslateFromJson(), "UseLDBToolCacheAdditionalText".TranslateFromJson(),
                new Vector2(30, -220), LDBToolCacheEntry.Value, out LDBToolCacheToggle);

            CreateSettingObject(queryObj, pageParent, "gb-htc-setting", "HideTechMode".TranslateFromJson(), "HideTechModeAdditionalText".TranslateFromJson(),
                new Vector2(30, -260), HideTechModeEntry.Value, out HideTechModeToggle);

            CreateSettingObject(queryObj, pageParent, "gb-smb-setting", "ShowMessageBox".TranslateFromJson(), "ShowMessageBoxAdditionalText".TranslateFromJson(),
                new Vector2(30, -300), ShowMessageBoxEntry.Value, out ShowMessageToggle);

            CreateSettingObject(languageObj, pageParent, "gb-csl-setting", "ProductOverflow".TranslateFromJson(),
                "ProductOverflowAdditionalText".TranslateFromJson(),
                new List<string> { "默认设置".TranslateFromJson(), "启用全部".TranslateFromJson(), "禁用全部".TranslateFromJson() },
                new Vector2(30, -340), ProductOverflowEntry.Value, out ProductOverflowComboBox);
        }

        private static void CreateSettingObject(GameObject oriObj, Transform parent, string name, string text, string additionalText, Vector2 position, bool defaultValue,
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

            Transform transform = settingObj.transform.GetChild(1);
            Object.DestroyImmediate(transform.GetComponent<Localizer>());
            transform.GetComponent<Text>().text = additionalText;
        }

        private static void CreateSettingObject(GameObject oriObj, Transform parent, string name, string text, string additionalText, List<string> values, Vector2 position,
            int index,
            out UIComboBox comboBox)
        {
            GameObject settingObj = Object.Instantiate(oriObj, parent);

            settingObj.name = name;
            Object.DestroyImmediate(settingObj.GetComponent<Localizer>());
            settingObj.GetComponent<Text>().text = text;

            var settingObjTransform = (RectTransform)settingObj.transform;
            settingObjTransform.anchoredPosition = position;

            comboBox = settingObj.GetComponentInChildren<UIComboBox>();
            comboBox.Items = values;
            comboBox.ItemButtons = new List<Button>();
            comboBox.UpdateItems();
            comboBox.onItemIndexChange.RemoveAllListeners();
            comboBox.itemIndex = index;
            ((RectTransform)comboBox.transform).sizeDelta = new Vector2(400, 30);

            Transform transform = settingObj.transform.GetChild(0);
            transform.localPosition = new Vector3(680, 0, 0);
            Object.DestroyImmediate(transform.GetComponent<Localizer>());
            Text component = transform.GetComponent<Text>();
            component.text = additionalText;
            component.fontSize = 16;
            component.color = new Color(1, 1, 1, 0.7843f);
        }

        [HarmonyPatch(typeof(UIOptionWindow), nameof(UIOptionWindow._OnOpen))]
        [HarmonyPostfix]
        public static void UIOptionWindow_OnOpen_Postfix()
        {
            if (!LDBToolCacheToggle) Init();

            Reset();
        }

        [HarmonyPatch(typeof(UIOptionWindow), nameof(UIOptionWindow.OnRevertButtonClick))]
        [HarmonyPostfix]
        public static void UIOptionWindow_OnRevertButtonClick_Postfix(int idx)
        {
            if (idx == 4) Reset();
        }

        private static void Reset()
        {
            LDBToolCacheToggle.isOn = LDBToolCacheEntry.Value;
            HideTechModeToggle.isOn = HideTechModeEntry.Value;
            ShowMessageToggle.isOn = ShowMessageBoxEntry.Value;
            ProductOverflowComboBox.itemIndex = ProductOverflowEntry.Value;
        }

        [HarmonyPatch(typeof(UIOptionWindow), nameof(UIOptionWindow.OnApplyClick))]
        [HarmonyPostfix]
        public static void UIOptionWindow_OnApplyClick_Postfix() =>
            SetConfig(LDBToolCacheToggle.isOn, HideTechModeToggle.isOn, ShowMessageToggle.isOn, ProductOverflowComboBox.itemIndex);
    }
}