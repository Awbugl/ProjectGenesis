using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

// ReSharper disable InconsistentNaming

namespace ProjectGenesis.Patches.UI
{
    public static class UpdateLogoPatches
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(UIMainMenu), "_OnOpen")]
        public static void UIMainMenu_OnOpen() => UpdateLogo();

        [HarmonyPostfix]
        [HarmonyPatch(typeof(UIEscMenu), "_OnOpen")]
        public static void UIEscMenu_OnOpen() => UpdateLogo();

        [HarmonyPostfix]
        [HarmonyPatch(typeof(GameOption), "Apply")]
        public static void UpdateGameOption_Apply() => UpdateLogo();

        private static void UpdateLogo()
        {
            GameObject mainLogo = GameObject.Find("UI Root/Overlay Canvas/Main Menu/dsp-logo");
            GameObject escLogo = GameObject.Find("UI Root/Overlay Canvas/In Game/Esc Menu/logo");

            string iconstr = Localization.language == Language.zhCN ? "Assets/texpack/中文图标" : "Assets/texpack/英文图标";
            Texture2D texture = Resources.Load<Sprite>(iconstr).texture;
            mainLogo.GetComponent<RawImage>().texture = texture;
            escLogo.GetComponent<RawImage>().texture = texture;
            mainLogo.GetComponent<RectTransform>().sizeDelta = new Vector2(texture.width, texture.height);
            escLogo.GetComponent<RectTransform>().sizeDelta = new Vector2(texture.width, texture.height);
        }
    }
}
