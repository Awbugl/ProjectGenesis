using System.Collections.Generic;
using CommonAPI.Systems;
using HarmonyLib;
using ProjectGenesis.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectGenesis.Patches.UI.UIPlanetBase
{
    /// <summary>
    ///   special thanks to https://github.com/hetima/DSP_PlanetFinder/tree/main/PlanetFinder
    ///   special thanks to https://github.com/starfi5h/DSP_Mod_Support/tree/main/FactoryLocator/src/UI
    /// </summary>
    public class UIPlanetBaseWindow : ManualBehaviour
    {
        private const int FocusMaxCount = 4;

        public RectTransform windowTrans;
        public Text nameText;
        private RectTransform _tab1;
        private UIButton[] _iconBtns;
        private Image[] _iconImgs;
        private Text[] _iconTexts;

        private int _currentbtnid;
        private static Sprite _tagNotSelectedSprite;

        private readonly List<int> _filterIds = new List<int>()
                                                {
                                                    6522,
                                                    6523,
                                                    6524,
                                                    6525,
                                                    6526,
                                                    6527,
                                                    6528
                                                };

        internal static UIPlanetBaseWindow CreateWindow()
        {
            var win = MyWindowCtl.CreateWindow<UIPlanetBaseWindow>("UIPlanetBaseWindow", "星球基地".TranslateFromJson());
            return win;
        }

        public void OpenWindow() => MyWindowCtl.OpenWindow(this);

        protected override void _OnCreate()
        {
            windowTrans = MyWindowCtl.GetRectTransform(this);
            windowTrans.sizeDelta = new Vector2(500f, 300f);

            CreateUI();
        }

        private void CreateUI()
        {
            var tabIndex = 1;

            _tab1 = AddTab(tabIndex);

            var @base = _tab1;
            var y = 0f;
            var x = 0f;

            // Planet Name / System Name
            nameText = Util.CreateText("Name", 16, TextAnchor.MiddleCenter);
            nameText.rectTransform.sizeDelta = new Vector2(180f, 20f);
            AddElement(nameText.transform as RectTransform, 0f, 0f, ref x, ref y, @base);

            x = 0f;
            var text = CreateTitleText("星球倾向".TranslateFromJson());
            AddElement(text.transform as RectTransform, 0f, 40f, ref x, ref y, @base);

            _iconBtns = new UIButton[FocusMaxCount];
            _iconImgs = new Image[FocusMaxCount];
            _iconTexts = new Text[FocusMaxCount];

            for (var i = 0; i < FocusMaxCount; ++i)
            {
                Util.CreateSignalIcon(out var iconBtn, out var iconImage);
                _iconBtns[i] = iconBtn;
                _iconImgs[i] = iconImage;
                _iconTexts[i] = Util.CreateText("", 14, TextAnchor.MiddleCenter);
                _iconTexts[i].rectTransform.sizeDelta = new Vector2(180f, 20f);
                Util.NormalizeRectWithTopLeft(iconBtn.transform, i * 90 + 45, 100, @base);
                Util.NormalizeRectWithTopLeft(_iconTexts[i].transform, i * 90 + 45, 150, @base);
                var i1 = i;
                iconBtn.onClick += _ => OnIconBtnClick(i1);
                iconBtn.onRightClick += _ => OnIconBtnRightClick(i1);
            }

            _tagNotSelectedSprite = _iconImgs[0].sprite;
        }

        private void AddElement(
            RectTransform rect,
            float deltaX,
            float deltaY,
            ref float x,
            ref float y,
            RectTransform @base)
        {
            x += deltaX;
            y += deltaY;
            if (rect != null) Util.NormalizeRectWithTopLeft(rect, x, y, @base);
        }

        private Text CreateTitleText(string label)
        {
            var src = MyWindowCtl.GetTitleText(this);
            var txt = Instantiate(src);
            txt.gameObject.name = "label";
            txt.text = label;
            txt.color = new Color(1f, 1f, 1f, 0.5f);
            ((RectTransform)txt.transform).sizeDelta = new Vector2(txt.preferredWidth + 40f, 30f);
            return txt;
        }

        private RectTransform AddTab(int tabIndex)
        {
            var tab = new GameObject();
            var tabRect = tab.AddComponent<RectTransform>();
            Util.NormalizeRectWithMargin(tabRect, 54f, 36f, 0f, 0f, windowTrans);
            tab.name = "tab-" + tabIndex;
            return tabRect;
        }

        internal void OnUpdate() => _OnUpdate();

        protected override void _OnUpdate()
        {
            if (VFInput.escape)
            {
                VFInput.UseEscape();
                _Close();
            }
        }

        private void OnIconBtnClick(int id)
        {
            _currentbtnid = id;
            AccessTools.FieldRefAccess<int>(typeof(UIItemPicker), "currentType")(UIRoot.instance.uiGame.itemPicker) = ProjectGenesis.TableID[0];
            UIItemPickerExtension.Popup(new Vector2(-300f, 250f), OnPickReturn, true, itemProto => _filterIds.Contains(itemProto.ID));
        }

        private void OnIconBtnRightClick(int id)
        {
            _currentbtnid = id;
            _iconImgs[_currentbtnid].sprite = _tagNotSelectedSprite;
        }

        private void OnPickReturn(ItemProto proto)
        {
            var sprite = proto?.iconSprite;
            if (sprite != null) _iconImgs[_currentbtnid].sprite = sprite;
        }
    }
}
