using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

// ReSharper disable InconsistentNaming

namespace ProjectGenesis.Patches.UI
{
    public static class UpdateLogoPatches
    {
        [HarmonyPatch(typeof(UIMainMenu), nameof(UIMainMenu._OnOpen))]
        [HarmonyPostfix]
        public static void UIMainMenu_OnOpen() => UpdateLogo();

        [HarmonyPatch(typeof(UIEscMenu), nameof(UIEscMenu._OnOpen))]
        [HarmonyPostfix]
        public static void UIEscMenu_OnOpen() => UpdateLogo();

        [HarmonyPatch(typeof(UIOptionWindow), nameof(UIOptionWindow.OnApplyClick))]
        [HarmonyPatch(typeof(UIOptionWindow), nameof(UIOptionWindow.OnCancelClick))]
        [HarmonyPostfix]
        public static void UpdateGameOption_Apply() => UpdateLogo();

        private static void UpdateLogo()
        {
            GameObject mainLogo = GameObject.Find("UI Root/Overlay Canvas/Main Menu/dsp-logo");
            GameObject escLogo = GameObject.Find("UI Root/Overlay Canvas/In Game/Esc Menu/logo");

            string iconstr = Localization.isZHCN ? "Assets/texpack/黑雾中文图标" : "Assets/texpack/黑雾英文图标";
            Texture2D texture = Resources.Load<Sprite>(iconstr).texture;
            mainLogo.GetComponent<RawImage>().texture = texture;
            escLogo.GetComponent<RawImage>().texture = texture;
            mainLogo.GetComponent<RectTransform>().sizeDelta = new Vector2(600f, 250f);
            mainLogo.GetComponent<RectTransform>().anchoredPosition = new Vector2(120, -60);
            escLogo.GetComponent<RectTransform>().sizeDelta = new Vector2(600f, 250f);
            escLogo.GetComponent<RectTransform>().anchoredPosition = new Vector2(30, 300);
        }
    }
}
