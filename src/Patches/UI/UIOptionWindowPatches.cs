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
                                DisableMessageToggle,
                                ChangeStackingLogicToggle;

        private static void Init()
        {
            var queryObj = GameObject.Find("UI Root/Overlay Canvas/Top Windows/Option Window/details/content-3/list/scroll-view/viewport/content/demolish-query");

            Transform pageParent = GameObject.Find("UI Root/Overlay Canvas/Top Windows/Option Window/details/content-5/advisor-tips").transform.parent;

            CreateSettingObject(queryObj, pageParent, "gb-ldbtc-setting", "UseLDBToolCache".TranslateFromJson(), "UseLDBToolCacheAdditionalText".TranslateFromJson(),
                new Vector2(30, -220), EnableLDBToolCacheEntry.Value, out LDBToolCacheToggle);

            CreateSettingObject(queryObj, pageParent, "gb-htc-setting", "EnableHideTechMode".TranslateFromJson(), "EnableHideTechModeAdditionalText".TranslateFromJson(),
                new Vector2(30, -260), EnableHideTechModeEntry.Value, out HideTechModeToggle);

            CreateSettingObject(queryObj, pageParent, "gb-smb-setting", "DisableMessageBox".TranslateFromJson(), "DisableMessageBoxAdditionalText".TranslateFromJson(),
                new Vector2(30, -300), DisableMessageBoxEntry.Value, out DisableMessageToggle);

            CreateSettingObject(queryObj, pageParent, "gb-csl-setting", "ChangeStackingLogic".TranslateFromJson(), "ChangeStackingLogicAdditionalText".TranslateFromJson(),
                new Vector2(30, -340), ChangeStackingLogicEntry.Value, out ChangeStackingLogicToggle);
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

            Transform additonalText = settingObj.transform.GetChild(1);
            Object.DestroyImmediate(additonalText.GetComponent<Localizer>());
            additonalText.GetComponent<Text>().text = additionalText;
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
            LDBToolCacheToggle.isOn = EnableLDBToolCacheEntry.Value;
            HideTechModeToggle.isOn = EnableHideTechModeEntry.Value;
            DisableMessageToggle.isOn = DisableMessageBoxEntry.Value;
            ChangeStackingLogicToggle.isOn = ChangeStackingLogicEntry.Value;
        }

        [HarmonyPatch(typeof(UIOptionWindow), nameof(UIOptionWindow.OnApplyClick))]
        [HarmonyPostfix]
        public static void UIOptionWindow_OnApplyClick_Postfix() => SetConfig(LDBToolCacheToggle.isOn, HideTechModeToggle.isOn, DisableMessageToggle.isOn, ChangeStackingLogicToggle.isOn);
    }
}
