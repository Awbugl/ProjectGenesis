using System.Collections.Generic;
using HarmonyLib;
using ProjectGenesis.Utils;
using UnityEngine;
using UnityEngine.UI;
using static ProjectGenesis.ProjectGenesis;

// ReSharper disable InconsistentNaming

namespace ProjectGenesis.Patches
{
    public static class UIOptionWindowPatches
    {
        private static UIToggle LDBToolCacheToggle, HideTechModeToggle, ShowMessageToggle;

        private static UIComboBox ProductOverflowComboBox;
        /// <summary>
        /// 包含左边字体（名称）、中间勾选框、右边字体（额外说明）
        /// </summary>
        private static GameObject QueryObj;
        /// <summary>
        /// 包含左边字体（名称）
        /// </summary>
        private static GameObject TipLevelTextObj;
        /// <summary>
        /// 包含中间下拉框、右边字体（额外说明）
        /// </summary>
        private static GameObject TipLevelComboboxAndTextObj;

        private static void Init()
        {
            QueryObj = GameObject.Find(
                "UI Root/Overlay Canvas/Top Windows/Option Window/details/content-3/list/scroll-view/viewport/content/demolish-query");
            TipLevelTextObj = GameObject.Find("UI Root/Overlay Canvas/Top Windows/Option Window/details/content-5/labels/tiplevel");
            TipLevelComboboxAndTextObj = GameObject.Find("UI Root/Overlay Canvas/Top Windows/Option Window/details/content-5/comps/ComboBox");
            
            Transform pageParent = TipLevelComboboxAndTextObj.transform.parent.parent;

            CreateSettingObject(pageParent, "gb-ldbtc-setting", "UseLDBToolCache".TranslateFromJson(),
                "UseLDBToolCacheAdditionalText".TranslateFromJson(), new Vector2(30, -220), LDBToolCacheEntry.Value,
                out LDBToolCacheToggle);

            CreateSettingObject(pageParent, "gb-htc-setting", "HideTechMode".TranslateFromJson(),
                "HideTechModeAdditionalText".TranslateFromJson(), new Vector2(30, -260), HideTechModeEntry.Value, out HideTechModeToggle);

            CreateSettingObject(pageParent, "gb-smb-setting", "ShowMessageBox".TranslateFromJson(),
                "ShowMessageBoxAdditionalText".TranslateFromJson(), new Vector2(30, -300), ShowMessageBoxEntry.Value,
                out ShowMessageToggle);

            CreateSettingObject(pageParent, "gb-csl-setting", "ProductOverflow".TranslateFromJson(),
                "ProductOverflowAdditionalText".TranslateFromJson(),
                new List<string> {
                    "默认设置".TranslateFromJson(),
                    "启用全部".TranslateFromJson(),
                    "禁用全部".TranslateFromJson(),
                }, new Vector2(30, -340), ProductOverflowEntry.Value, out ProductOverflowComboBox);
        }

        private static void CreateSettingObject(Transform parent, string name, string text, string additionalText, Vector2 position,
            bool defaultValue, out UIToggle toggle)
        {
            GameObject settingObj = Object.Instantiate(QueryObj, parent);

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

        private static void CreateSettingObject(Transform parent, string name, string text, string additionalText, List<string> values,
            Vector2 position, int index, out UIComboBox comboBox)
        {
            GameObject textObj = Object.Instantiate(TipLevelTextObj, parent);
            Object.DestroyImmediate(textObj.GetComponent<Localizer>());
            textObj.GetComponent<Text>().text = text;
            var textObjTransform = (RectTransform)textObj.transform;
            textObjTransform.anchoredPosition = position;

            GameObject comboBoxObj = Object.Instantiate(TipLevelComboboxAndTextObj, parent);
            comboBoxObj.name = name;
            Object.DestroyImmediate(comboBoxObj.GetComponent<Localizer>());
            // comboBoxObj.transform.Find("Main Button/Text").GetComponent<Text>().text = text;
            var comboBoxObjTransform = (RectTransform)comboBoxObj.transform;
            comboBoxObjTransform.anchoredPosition = new Vector2(position.x + 310, position.y);

            comboBox = comboBoxObj.GetComponentInChildren<UIComboBox>();
            comboBox.Items = values;
            comboBox.ItemButtons = new List<Button>();
            comboBox.UpdateItems();
            comboBox.onItemIndexChange.RemoveAllListeners();
            comboBox.itemIndex = index;
            ((RectTransform)comboBox.transform).sizeDelta = new Vector2(400, 30);

            if (string.IsNullOrEmpty(additionalText)) return;

            GameObject gameObject = Object.Instantiate(QueryObj.transform.GetChild(1).gameObject, comboBoxObjTransform);

            Object.DestroyImmediate(gameObject.GetComponent<Localizer>());
            gameObject.GetComponent<Text>().text = additionalText;

            var gameObjectTransform = (RectTransform)gameObject.transform;
            gameObjectTransform.anchoredPosition = new Vector2(420, 0);
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
