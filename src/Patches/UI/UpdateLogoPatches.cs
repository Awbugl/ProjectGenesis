using System;
using HarmonyLib;
using ProjectGenesis.Utils;
using UnityEngine;
using UnityEngine.UI;

// ReSharper disable InconsistentNaming

namespace ProjectGenesis.Patches
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
            var mainLogo = GameObject.Find("UI Root/Overlay Canvas/Main Menu/dsp-logo");
            var escLogo = GameObject.Find("UI Root/Overlay Canvas/In Game/Esc Menu/logo");

            string iconstr;

            if (Localization.isZHCN)
            {
                iconstr = "黑雾中文图标";

                DateTime dateTime = DateTime.Now;

                if (dateTime > new DateTime(2024, 2, 9) && dateTime < new DateTime(2024, 2, 25)) iconstr = "创世Logo新春贺岁版";
            }
            else { iconstr = "黑雾英文图标"; }

            Texture2D texture = TextureHelper.GetTexture(iconstr);
            mainLogo.GetComponent<RawImage>().texture = texture;
            escLogo.GetComponent<RawImage>().texture = texture;
            mainLogo.GetComponent<RectTransform>().sizeDelta = new Vector2(600f, 250f);
            mainLogo.GetComponent<RectTransform>().anchoredPosition = new Vector2(120, -60);
            escLogo.GetComponent<RectTransform>().sizeDelta = new Vector2(600f, 250f);
            escLogo.GetComponent<RectTransform>().anchoredPosition = new Vector2(30, 300);
        }
    }
}
