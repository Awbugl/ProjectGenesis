using HarmonyLib;
using ProjectGenesis.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectGenesis.Patches.UI
{
    public static class UISettingPatches
    {
        internal static bool CurrentAtmosphericEmission;

        [HarmonyPatch(typeof(VFPreload), "InvokeOnLoadWorkEnded")]
        [HarmonyPostfix]
        [HarmonyPriority(Priority.Last)]
        public static void VFPreload_InvokeOnLoadWorkEnded_Postfix()
        {
            var languageObj = GameObject.Find("UI Root/Overlay Canvas/Top Windows/Option Window/details/content-5/advisor-tips");
            var settingObj = Object.Instantiate(languageObj, languageObj.transform.parent);

            settingObj.name = "gb-ae-setting";
            Object.DestroyImmediate(settingObj.GetComponent<Localizer>());
            settingObj.GetComponent<Text>().text = "EnableAtmosphericEmission".TranslateFromJson();

            var settingObjTransform = (RectTransform)settingObj.transform;
            settingObjTransform.anchoredPosition = new Vector2(30, -180);

            CurrentAtmosphericEmission = ProjectGenesis.AtmosphericEmissionValue;

            var toggle = settingObj.GetComponentInChildren<UIToggle>();
            toggle.isOn = CurrentAtmosphericEmission;
            toggle.toggle.onValueChanged.RemoveAllListeners();
            toggle.toggle.onValueChanged.AddListener((isOn) => { CurrentAtmosphericEmission = isOn; });
        }

        [HarmonyPatch(typeof(UIOptionWindow), "OnCancelClick")]
        [HarmonyPostfix]
        public static void UIOptionWindow_OnCancelClick_Postfix() => CurrentAtmosphericEmission = ProjectGenesis.AtmosphericEmissionValue;

        [HarmonyPatch(typeof(UIOptionWindow), "OnApplyClick")]
        [HarmonyPostfix]
        public static void UIOptionWindow_OnApplyClick_Postfix()
        {
            if (CurrentAtmosphericEmission != ProjectGenesis.AtmosphericEmissionValue)
                ProjectGenesis.SetAtmosphericEmission(CurrentAtmosphericEmission);
        }
    }
}
