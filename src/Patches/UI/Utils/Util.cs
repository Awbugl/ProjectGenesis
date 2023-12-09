using ProjectGenesis.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectGenesis.Patches.UI.Utils
{
    /// <summary>
    ///     special thanks to https://github.com/hetima/DSP_PlanetFinder/tree/main/PlanetFinder
    /// </summary>
    internal static class Util
    {
        private static readonly UIButton OrbitAddButton = UIRoot.instance.uiGame.dysonEditor.controlPanel.hierarchy.swarmPanel.orbitAddButton;
        private static readonly GameObject TransformGameObject = UIRoot.instance.uiGame.beltWindow.iconTagButton.transform.gameObject;
        private static readonly UIAssemblerWindow UIGameAssemblerWindow = UIRoot.instance.uiGame.assemblerWindow;
        private static readonly UIButton CategoryButton = UIRoot.instance.uiGame.functionPanel.buildMenu.categoryButtons[0];

        internal static RectTransform NormalizeRectWithTopLeft(
            Component cmp,
            float left,
            float top,
            Transform parent = null)
        {
            var rect = (RectTransform)cmp.transform;
            if (parent != null) rect.SetParent(parent, false);

            rect.anchorMax = new Vector2(0f, 1f);
            rect.anchorMin = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition3D = new Vector3(left, -top, 0f);
            return rect;
        }

        public static void NormalizeRectWithMargin(
            Component cmp,
            float top,
            float left,
            float bottom,
            float right,
            Transform parent = null)
        {
            var rect = (RectTransform)cmp.transform;
            if (parent != null) rect.SetParent(parent, false);

            rect.anchoredPosition3D = Vector3.zero;
            rect.localScale = Vector3.one;
            rect.anchorMax = Vector2.one;
            rect.anchorMin = Vector2.zero;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.offsetMax = new Vector2(-right, -top);
            rect.offsetMin = new Vector2(left, bottom);
        }

        public static Text CreateText(string label, int fontSize = 14, TextAnchor anchor = TextAnchor.MiddleLeft)
        {
            Text stateText = UIGameAssemblerWindow.stateText;
            Text txt = Object.Instantiate(stateText);
            txt.gameObject.name = "txt_" + label;
            txt.text = label;
            txt.color = new Color(1f, 1f, 1f, 0.4f);
            txt.alignment = anchor;
            //txt_.supportRichText = false;
            txt.fontSize = fontSize;
            return txt;
        }

        public static Text CreateLabelText(GameObject obj, string label)
        {
            Text txt = Object.Instantiate(obj.GetComponent<Text>(), obj.transform.parent);
            Object.Destroy(txt.gameObject.GetComponent<Localizer>());
            txt.gameObject.name = "txt_" + label;
            txt.text = label;
            return txt;
        }

        public static Text CreateTitleText(string label)
        {
            Text src = MyWindowCtl.GetTitleText(UIGameAssemblerWindow);
            Text txt = Object.Instantiate(src);
            txt.gameObject.name = "label";
            txt.text = label;
            txt.color = new Color(1f, 1f, 1f, 0.5f);
            ((RectTransform)txt.transform).sizeDelta = new Vector2(txt.preferredWidth + 40f, 30f);
            return txt;
        }

        internal static UIButton CreateButton(string label, float width = 0f, float height = 0f)
        {
            UIButton btn = Object.Instantiate(OrbitAddButton);
            btn.gameObject.name = "btn_" + label;
            if (btn.transitions.Length >= 1) btn.transitions[0].target.color = new Color(0.2392f, 0.6f, 0.9f, 0.078f);

            var btnText = btn.transform.Find("Text").GetComponent<Text>();
            btnText.text = label;
            btnText.fontSize = 17;
            Object.Destroy(btn.transform.Find("Text").GetComponent<Localizer>());
            var btnRect = (RectTransform)btn.transform;
            if (width == 0f || height == 0f)
                btnRect.sizeDelta = new Vector2(btnText.preferredWidth + 14f, 24f); //22
            else
                btnRect.sizeDelta = new Vector2(width, height);

            return btn;
        }

        internal static UIButton CreateCategoryButton(Sprite img, string text = null)
        {
            UIButton btn = Object.Instantiate(CategoryButton);
            btn.gameObject.name = "btn_" + (text ?? "");
            if (btn.transitions.Length >= 1) btn.transitions[0].target.color = new Color(0.2392f, 0.6f, 0.9f, 0.078f);

            var btnText = btn.GetComponentInChildren<Text>();
            var btnImage = btn.GetComponentInChildren<Image>();

            btnText.text = text;
            btnText.fontSize = 17;
            btnImage.sprite = img;

            Object.Destroy(btn.transform.Find("Text").GetComponent<Localizer>());

            return btn;
        }

        internal static void CreateSignalIcon(
            string tipTitle,
            string tipText,
            out UIButton iconButton,
            out Image iconImage)
        {
            GameObject go = Object.Instantiate(TransformGameObject);

            go.name = "signal-button";
            go.SetActive(true);
            var rect = (RectTransform)go.transform;
            for (int i = rect.childCount - 1; i >= 0; --i)
            {
                Object.Destroy(rect.GetChild(i).gameObject);
            }

            iconButton = rect.GetComponent<UIButton>();
            iconButton.tips.tipTitle = tipTitle.TranslateFromJson();
            iconButton.tips.tipText = tipText.TranslateFromJson();

            iconImage = rect.GetComponent<Image>();
        }

        internal static void CreateSignalImage(out Image iconImage)
        {
            GameObject go = Object.Instantiate(TransformGameObject);

            go.name = "signal-button";
            go.SetActive(true);
            var rect = (RectTransform)go.transform;

            for (int i = rect.childCount - 1; i >= 0; --i)
            {
                Object.Destroy(rect.GetChild(i).gameObject);
            }

            var iconButton = rect.GetComponent<UIButton>();
            Object.Destroy(iconButton);
            iconImage = rect.GetComponent<Image>();
        }
    }
}
