using CommonAPI.Systems;
using HarmonyLib;
using ProjectGenesis.Utils;
using UnityEngine;
using UnityEngine.UI;
using static ProjectGenesis.Patches.Logic.PlanetBase.PlanetBasePatches;

namespace ProjectGenesis.Patches.UI.UIPlanetBase
{
    /// <summary>
    ///   special thanks to https://github.com/hetima/DSP_PlanetFinder/tree/main/PlanetFinder
    ///   special thanks to https://github.com/starfi5h/DSP_Mod_Support/tree/main/FactoryLocator/src/UI
    /// </summary>
    public class UIPlanetBaseWindow : ManualBehaviour
    {
        public RectTransform windowTrans;
        public Text nameText;

        private RectTransform _tab1;

        private int[] _currentFocusIds;

        private readonly UIButton[] _iconBtns = new UIButton[FocusMaxCount];
        private readonly Image[] _iconImgs = new Image[FocusMaxCount];
        private readonly Text[] _iconTexts = new Text[FocusMaxCount];

        internal static int CurPlanetId;
        private static Sprite _tagNotSelectedSprite;

        internal static UIPlanetBaseWindow CreateWindow()
        {
            var win = MyWindowCtl.CreateWindow<UIPlanetBaseWindow>("UIPlanetBaseWindow", "星球基地".TranslateFromJson());
            return win;
        }

        public void OpenWindow() => MyWindowCtl.OpenWindow(this);

        protected override void _OnCreate()
        {
            windowTrans = MyWindowCtl.GetRectTransform(this);
            windowTrans.sizeDelta = new Vector2(380f, 250f);

            CreateUI();
        }

        private void CreateUI()
        {
            var tab = new GameObject();
            _tab1 = tab.AddComponent<RectTransform>();
            Util.NormalizeRectWithMargin(_tab1, 40, 40, 40, 40, windowTrans);
            tab.name = "tab-1";

            nameText = Util.CreateText("星球倾向", 16);
            Util.NormalizeRectWithTopLeft(nameText.transform, 0f, 20f, _tab1);

            for (var i = 0; i < FocusMaxCount; ++i)
            {
                Util.CreateSignalIcon(out var iconBtn, out var iconImage);
                _iconBtns[i] = iconBtn;
                _iconImgs[i] = iconImage;
                _iconTexts[i] = Util.CreateText("", 16);

                // works for 2x2
                // Util.NormalizeRectWithTopLeft(iconBtn.transform, (i & 1) * 60, 60 + (i >> 1) * 60, _tab1);
                // Util.NormalizeRectWithTopLeft(_iconTexts[i].transform, 150, 60 + i * 30, _tab1);

                Util.NormalizeRectWithTopLeft(iconBtn.transform, 0, 60 + i * 60, _tab1);
                Util.NormalizeRectWithTopLeft(_iconTexts[i].transform, 55, 72 + i * 60, _tab1);

                var id = i;
                iconBtn.onClick += _ => OnIconBtnClick(id);
                iconBtn.onRightClick += _ => OnIconBtnRightClick(id);
            }

            _tagNotSelectedSprite = _iconImgs[0].sprite;
        }

        protected override void _OnUpdate()
        {
            if (VFInput.escape)
            {
                VFInput.UseEscape();
                _Close();
            }
        }

        internal void OnPlanetChanged(int planetId)
        {
            _currentFocusIds = GetPlanetBase(planetId);

            for (var i = 0; i < FocusMaxCount; ++i)
            {
                var currentFocusId = _currentFocusIds[i];
                if (currentFocusId == 0)
                {
                    _iconImgs[i].sprite = _tagNotSelectedSprite;
                    _iconTexts[i].text = "";
                    continue;
                }

                var proto = LDB.items.Select(currentFocusId);
                _iconTexts[i].text = FilterIds[currentFocusId];
                var sprite = proto.iconSprite;
                if (sprite != null) _iconImgs[i].sprite = sprite;
            }
        }

        private void OnIconBtnClick(int id)
        {
            AccessTools.FieldRefAccess<int>(typeof(UIItemPicker), "currentType")(UIRoot.instance.uiGame.itemPicker) = ProjectGenesis.TableID[0];
            UIItemPickerExtension.Popup(new Vector2(-300f, 250f), j => OnPickReturn(j, id), true, itemProto => FilterIds.ContainsKey(itemProto.ID));
        }

        private void OnIconBtnRightClick(int id)
        {
            _currentFocusIds[id] = 0;
            SetPlanetFocus(CurPlanetId, id, 0);
            _iconImgs[id].sprite = _tagNotSelectedSprite;
            _iconTexts[id].text = "";
        }

        private void OnPickReturn(ItemProto proto, int id)
        {
            if (proto == null) return;

            var currentFocusId = proto.ID;

            // ReSharper disable once LoopCanBeConvertedToQuery
            foreach (var t in _currentFocusIds)
            {
                if (t == currentFocusId)
                {
                    UIRealtimeTip.Popup("不能重复选择".TranslateFromJson());
                    return;
                }
            }
            
            _currentFocusIds[id] = currentFocusId;
            SetPlanetFocus(CurPlanetId, id, currentFocusId);
            _iconTexts[id].text = FilterIds[currentFocusId].TranslateFromJson();
            var sprite = proto.iconSprite;
            if (sprite != null) _iconImgs[id].sprite = sprite;
        }
    }
}
