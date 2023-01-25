using ProjectGenesis.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectGenesis.Patches.UI.UIPlanetBase
{
    /// <summary>
    ///   special thanks to https://github.com/hetima/DSP_PlanetFinder/tree/main/PlanetFinder
    /// </summary>
    internal static class Util
    {
        private static readonly UIButton Src = UIRoot.instance.uiGame.dysonEditor.controlPanel.hierarchy.swarmPanel.orbitAddButton;
        private static readonly GameObject TransformGameObject = UIRoot.instance.uiGame.beltWindow.iconTagButton.transform.gameObject;

        internal static RectTransform NormalizeRectWithTopLeft(
            Component cmp,
            float left,
            float top,
            Transform parent = null)
        {
            var rect = cmp.transform as RectTransform;
            if (parent != null)
            {
                rect.SetParent(parent, false);
            }

            rect.anchorMax = new Vector2(0f, 1f);
            rect.anchorMin = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition3D = new Vector3(left, -top, 0f);
            return rect;
        }

        public static RectTransform NormalizeRectWithMargin(
            Component cmp,
            float top,
            float left,
            float bottom,
            float right,
            Transform parent = null)
        {
            var rect = cmp.transform as RectTransform;
            if (parent != null)
            {
                rect.SetParent(parent, false);
            }

            rect.anchoredPosition3D = Vector3.zero;
            rect.localScale = Vector3.one;
            rect.anchorMax = Vector2.one;
            rect.anchorMin = Vector2.zero;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.offsetMax = new Vector2(-right, -top);
            rect.offsetMin = new Vector2(left, bottom);
            return rect;
        }

        public static Text CreateText(string label, int fontSize = 14, TextAnchor anchor = TextAnchor.MiddleLeft)
        {
            var stateText = UIRoot.instance.uiGame.assemblerWindow.stateText;
            var txt = Object.Instantiate(stateText);
            txt.gameObject.name = "txt_" + label;
            txt.text = label;
            txt.color = new Color(1f, 1f, 1f, 0.4f);
            txt.alignment = anchor;
            //txt_.supportRichText = false;
            txt.fontSize = fontSize;
            return txt;
        }

        internal static UIButton CreateButton(string label, float width = 0f, float height = 0f)
        {
            var btn = Object.Instantiate(Src);
            btn.gameObject.name = "btn_" + label;
            if (btn.transitions.Length >= 1)
            {
                btn.transitions[0].target.color = new Color(0.2392f, 0.6f, 0.9f, 0.078f);
            }

            var btnText = btn.transform.Find("Text").GetComponent<Text>();
            btnText.text = label;
            btnText.fontSize = 17;
            Object.Destroy(btn.transform.Find("Text").GetComponent<Localizer>());
            var btnRect = btn.transform as RectTransform;
            if (width == 0f || height == 0f)
            {
                btnRect.sizeDelta = new Vector2(btnText.preferredWidth + 14f, 24f); //22
            }
            else
            {
                btnRect.sizeDelta = new Vector2(width, height);
            }

            return btn;
        }

        internal static void CreateSignalIcon(out UIButton iconButton, out Image iconImage)
        {
            var go = Object.Instantiate(TransformGameObject);

            go.name = "signal-button";
            go.SetActive(true);
            var rect = (RectTransform)go.transform;
            for (var i = rect.childCount - 1; i >= 0; --i) Object.Destroy(rect.GetChild(i).gameObject);

            iconButton = rect.GetComponent<UIButton>();
            iconButton.tips.tipTitle = "选择星球倾向".TranslateFromJson();
            iconButton.tips.tipText = "选择星球倾向描述".TranslateFromJson();

            iconImage = rect.GetComponent<Image>();
        }
    }
}
