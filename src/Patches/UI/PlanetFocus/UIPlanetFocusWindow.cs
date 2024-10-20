using CommonAPI.Systems;
using ProjectGenesis.Patches.UI.Utils;
using ProjectGenesis.Utils;
using UnityEngine;
using UnityEngine.UI;
using static ProjectGenesis.Patches.Logic.PlanetFocus.PlanetFocusPatches;
using static ProjectGenesis.Patches.UI.Utils.Util;
using static ProjectGenesis.Patches.UI.Utils.MyWindowCtl;

namespace ProjectGenesis.Patches.UI.PlanetFocus
{
    /// <summary>
    ///     special thanks to https://github.com/hetima/DSP_PlanetFinder/tree/main/PlanetFinder
    ///     special thanks to https://github.com/starfi5h/DSP_Mod_Support/tree/main/FactoryLocator/src/UI
    /// </summary>
    public class UIPlanetFocusWindow : ManualBehaviour
    {
        internal static int CurPlanetId;
        private static Sprite _tagNotSelectedSprite;
        public RectTransform windowTrans;
        public Text nameText;

        private readonly UIButton[] _iconBtns = new UIButton[FocusMaxCount];
        private readonly Image[] _iconImgs = new Image[FocusMaxCount];
        private readonly Text[] _iconTexts = new Text[FocusMaxCount];

        private int[] _currentFocusIds;

        private RectTransform _tab1;

        internal static UIPlanetFocusWindow CreateWindow() =>
            CreateWindow<UIPlanetFocusWindow>("UIPlanetFocusWindow", "星球基地".TranslateFromJson());

        public void OpenWindow() => MyWindowCtl.OpenWindow(this);

        public override void _OnCreate()
        {
            windowTrans = GetRectTransform(this);
            windowTrans.sizeDelta = new Vector2(380f, 250f);

            CreateUI();
        }

        private void CreateUI()
        {
            var tab = new GameObject();
            _tab1 = tab.AddComponent<RectTransform>();
            NormalizeRectWithMargin(_tab1, 40, 40, 40, 40, windowTrans);
            tab.name = "tab-1";

            nameText = CreateText("", 16);
            NormalizeRectWithTopLeft(nameText.transform, 0f, 20f, _tab1);

            for (var i = 0; i < FocusMaxCount; ++i)
            {
                CreateSignalIcon("选择星球倾向", "选择星球倾向描述", out UIButton iconBtn, out Image iconImage);
                _iconBtns[i] = iconBtn;
                _iconImgs[i] = iconImage;
                _iconTexts[i] = CreateText("", 16);

                // works for 2x2
                // Util.NormalizeRectWithTopLeft(iconBtn.transform, (i & 1) * 60, 60 + (i >> 1) * 60, _tab1);
                // Util.NormalizeRectWithTopLeft(_iconTexts[i].transform, 150, 60 + i * 30, _tab1);

                NormalizeRectWithTopLeft(iconBtn.transform, 0, 60 + i * 60, _tab1);
                NormalizeRectWithTopLeft(_iconTexts[i].transform, 55, 72 + i * 60, _tab1);

                int id = i;
                iconBtn.onClick += _ => OnIconBtnClick(id);
                iconBtn.onRightClick += _ => OnIconBtnRightClick(id);
            }

            _tagNotSelectedSprite = _iconImgs[0].sprite;
        }

        public override void _OnUpdate()
        {
            if (VFInput.escape)
            {
                VFInput.UseEscape();
                _Close();
            }
        }

        internal void OnPlanetChanged(int planetId)
        {
            _currentFocusIds = GetPlanetFocus(planetId);

            for (var i = 0; i < FocusMaxCount; ++i)
            {
                int currentFocusId = _currentFocusIds[i];

                if (currentFocusId == 0)
                {
                    _iconImgs[i].sprite = _tagNotSelectedSprite;
                    _iconTexts[i].text = "";

                    continue;
                }

                ItemProto proto = LDB.items.Select(currentFocusId);
                _iconTexts[i].text = FocusIds[currentFocusId].TranslateFromJson();
                Sprite sprite = proto.iconSprite;

                if (sprite != null) _iconImgs[i].sprite = sprite;
            }
        }

        private void OnIconBtnClick(int id)
        {
            UIRoot.instance.uiGame.itemPicker.currentType = ProjectGenesis.TableID[0];
            UIItemPickerExtension.Popup(new Vector2(-300f, 250f), j => OnPickReturn(j, id), true,
                itemProto => FocusIds.ContainsKey(itemProto.ID) && GameMain.data.history.TechUnlocked(itemProto.PreTechOverride));
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

            int currentFocusId = proto.ID;

            if (_currentFocusIds == null) _currentFocusIds = GetPlanetFocus(CurPlanetId);

            // ReSharper disable once LoopCanBeConvertedToQuery
            foreach (int t in _currentFocusIds)
            {
                if (t == currentFocusId)
                {
                    UIRealtimeTip.Popup("不能重复选择".TranslateFromJson());

                    return;
                }
            }

            _currentFocusIds[id] = currentFocusId;
            SetPlanetFocus(CurPlanetId, id, currentFocusId);
            _iconTexts[id].text = FocusIds[currentFocusId].TranslateFromJson();
            Sprite sprite = proto.iconSprite;

            if (sprite != null) _iconImgs[id].sprite = sprite;
        }
    }
}
