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
        private static readonly UIButton OrbitAddButton =
            UIRoot.instance.uiGame.dysonEditor.controlPanel.hierarchy.swarmPanel.orbitAddButton;

        private static readonly GameObject TransformGameObject = UIRoot.instance.uiGame.beltWindow.iconTagButton.transform.gameObject;
        private static readonly UIAssemblerWindow UIGameAssemblerWindow = UIRoot.instance.uiGame.assemblerWindow;
        private static readonly UIButton CategoryButton = UIRoot.instance.uiGame.functionPanel.buildMenu.categoryButtons[1];

        internal static RectTransform NormalizeRectWithTopLeft(Component cmp, float left, float top, Transform parent = null)
        {
            var rect = (RectTransform)cmp.transform;

            if (parent != null) rect.SetParent(parent, false);

            rect.anchorMax = new Vector2(0f, 1f);
            rect.anchorMin = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition3D = new Vector3(left, -top, 0f);

            return rect;
        }

        public static void NormalizeRectWithMargin(Component cmp, float top, float left, float bottom, float right, Transform parent = null)
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

            Text btnText = btn.transform.Find("Text").GetComponent<Text>();
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

            Text btnText = btn.GetComponentInChildren<Text>();
            Image btnImage = btn.GetComponentInChildren<Image>();

            btnText.text = text;
            btnText.fontSize = 17;
            btnImage.sprite = img;

            return btn;
        }

        internal static void CreateSignalIcon(string tipTitle, string tipText, out UIButton iconButton, out Image iconImage)
        {
            GameObject go = Object.Instantiate(TransformGameObject);

            go.name = "signal-button";
            go.SetActive(true);
            var rect = (RectTransform)go.transform;

            for (int i = rect.childCount - 1; i >= 0; --i) Object.Destroy(rect.GetChild(i).gameObject);

            iconButton = rect.GetComponent<UIButton>();
            iconButton.tips.tipTitle = tipTitle.TranslateFromJson();
            iconButton.tips.tipText = tipText.TranslateFromJson();

            iconImage = rect.GetComponent<Image>();
        }

        public static UIButton MakeIconButtonB(Sprite sprite, float size = 60)
        {
            GameObject go = Object.Instantiate(UIRoot.instance.uiGame.researchQueue.pauseButton.gameObject);
            UIButton btn = go.GetComponent<UIButton>();
            var rect = (RectTransform)go.transform;

            //rect.sizeDelta = new Vector2(size, size);
            float scale = size / 60;
            rect.localScale = new Vector3(scale, scale, scale);
            Image img = go.transform.Find("icon")?.GetComponent<Image>();

            if (img != null) img.sprite = sprite;

            btn.tips.tipText = "";
            btn.tips.tipTitle = "";
            btn.tips.delay = 0.7f;

            return btn;
        }

        internal static void RemovePersistentCalls(GameObject go)
        {
            Button oldbutton = go.GetComponent<Button>();
            UIButton btn = go.GetComponent<UIButton>();

            if (btn != null && oldbutton != null)
            {
                Object.DestroyImmediate(oldbutton);
                btn.button = go.AddComponent<Button>();
            }
        }

        public static UIButton MakeIconButtonC(Sprite sprite, float size = 30)
        {
            GameObject src = UIRoot.instance.uiGame.starmap.northButton.transform.parent.Find("tip")?.gameObject;
            GameObject go = Object.Instantiate(src);

            RemovePersistentCalls(go);
            UIButton btn = go.GetComponent<UIButton>();
            var rect = (RectTransform)go.transform;

            for (int i = rect.childCount - 1; i >= 0; --i) Object.Destroy(rect.GetChild(i).gameObject);

            rect.DetachChildren();

            if (size > 0) rect.sizeDelta = new Vector2(size, size);

            //float scale = size / rect.sizeDelta.y; //y=30
            //rect.localScale = new Vector3(scale, scale, scale);
            Image img = go.GetComponent<Image>();

            if (img != null) img.sprite = sprite;

            btn.tips.tipText = "";
            btn.tips.tipTitle = "";
            btn.tips.delay = 0.6f;

            if (btn.transitions != null && btn.transitions.Length > 0) btn.transitions = new UIButton.Transition[] { btn.transitions[0], };

            return btn;
        }

        public static UIButton MakeSmallTextButton(string label = "", float width = 0, float height = 0)
        {
            UIAssemblerWindow assemblerWindow = UIRoot.instance.uiGame.assemblerWindow;
            GameObject go = Object.Instantiate(assemblerWindow.copyButton.gameObject);
            UIButton btn = go.GetComponent<UIButton>();
            Transform child = go.transform.Find("Text");
            Object.DestroyImmediate(child.GetComponent<Localizer>());
            Text txt = child.GetComponent<Text>();
            txt.text = label;
            btn.tips.tipText = "";
            btn.tips.tipTitle = "";

            if (width > 0 || height > 0)
            {
                var rect = (RectTransform)go.transform;

                if (width == 0) width = rect.sizeDelta.x;

                if (height == 0) height = rect.sizeDelta.y;

                rect.sizeDelta = new Vector2(width, height);
            }

            go.transform.localScale = Vector3.one;

            return btn;
        }

        public static UIButton MakeHiliteTextButton(string label, float width = 0f, float height = 0f)
        {
            UIDESwarmPanel swarmPanel = UIRoot.instance.uiGame.dysonEditor.controlPanel.hierarchy.swarmPanel;
            UIButton src = swarmPanel.orbitButtons[0];
            UIButton btn = Object.Instantiate(src);

            if (btn.transitions.Length >= 2)
            {
                btn.transitions[0].normalColor = new Color(0.1f, 0.1f, 0.1f, 0.68f);
                btn.transitions[0].highlightColorOverride = new Color(0.9906f, 0.5897f, 0.3691f, 0.4f);
                btn.transitions[1].normalColor = new Color(1f, 1f, 1f, 0.6f);
                btn.transitions[1].highlightColorOverride = new Color(0.2f, 0.1f, 0.1f, 0.9f);
            }

            Text btnText = btn.transform.Find("Text").GetComponent<Text>();
            btnText.text = label;
            btnText.fontSize = 14;

            Transform transform;
            (transform = btn.transform).Find("frame")?.gameObject.SetActive(false);
            var btnRect = (RectTransform)transform;

            btnRect.sizeDelta = width == 0f || height == 0f ? new Vector2(btnText.preferredWidth + 14f, 22f) : new Vector2(width, height);

            return btn;
        }
    }
}
