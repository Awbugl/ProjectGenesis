using System.Collections.Generic;
using NGPT;
using ProjectGenesis.Patches.Logic;
using ProjectGenesis.Patches.UI.QTools.MyComboBox;
using ProjectGenesis.Patches.UI.Utils;
using ProjectGenesis.Utils;
using UnityEngine;
using UnityEngine.UI;
using static ProjectGenesis.Patches.Logic.QTools;
using Utils_ERecipeType = ProjectGenesis.Utils.ERecipeType;

namespace ProjectGenesis.Patches.UI.QTools
{
    /// <summary>
    ///     special thanks to https://github.com/hetima/DSP_PlanetFinder/tree/main/PlanetFinder
    ///     special thanks to https://github.com/starfi5h/DSP_Mod_Support/tree/main/FactoryLocator/src/UI
    /// </summary>

    // ReSharper disable once InconsistentNaming
    public class UIQToolsWindow : ManualBehaviour
    {
        public bool isOpening;

        private UIButton _pauseButton;
        private Sprite _playSprite;
        private Sprite _pauseSprite;
        private string _playString;
        private string _pauseString;
        private Image _pauseImg;
        private Text _pauseText;

        private RectTransform[] _tabs;
        private UIButton[] _tabButtons;
        private Tweener[] _tabTweeners;
        private Text[] _tabTexts;
        private Image _tabSlider;
        private int _tabIndex;
        private RectTransform _list;
        private RectTransform _listContent;
        private RectTransform _rightInfoArea;

        private GameObject _labelText;

        private ProliferatorComboBox _proliferatorComboBox;
        private Dictionary<Utils_ERecipeType, ItemComboBox> _recipeMachines;

        private NodeDataSet _data;
        private List<ProductDetail> _productDetailPool;
        private int curReuseIndex;

        private void CreateUI()
        {
            MyKeyBinder.CreateKeyBinder(30, 20, _tabs[0], ProjectGenesis.QToolsHotkey, "计算器快捷键");

            _recipeMachines = new Dictionary<Utils_ERecipeType, ItemComboBox>
                              {
                                  [Utils_ERecipeType.Smelt] = MyComboBox.MyComboBox.CreateComboBox<ItemComboBox>(30, 110, _tabs[0], "默认生产设备"),
                                  [Utils_ERecipeType.矿物处理] = MyComboBox.MyComboBox.CreateComboBox<ItemComboBox>(430, 110, _tabs[0]),
                                  [Utils_ERecipeType.Assemble] = MyComboBox.MyComboBox.CreateComboBox<ItemComboBox>(830, 110, _tabs[0]),
                                  [Utils_ERecipeType.标准制造] = MyComboBox.MyComboBox.CreateComboBox<ItemComboBox>(30, 155, _tabs[0]),
                                  [Utils_ERecipeType.高精度加工] = MyComboBox.MyComboBox.CreateComboBox<ItemComboBox>(430, 155, _tabs[0]),
                                  [Utils_ERecipeType.Refine] = MyComboBox.MyComboBox.CreateComboBox<ItemComboBox>(830, 155, _tabs[0]),
                                  [Utils_ERecipeType.Chemical] = MyComboBox.MyComboBox.CreateComboBox<ItemComboBox>(30, 200, _tabs[0]),
                                  [Utils_ERecipeType.高分子化工] = MyComboBox.MyComboBox.CreateComboBox<ItemComboBox>(430, 200, _tabs[0]),
                                  [Utils_ERecipeType.Particle] = MyComboBox.MyComboBox.CreateComboBox<ItemComboBox>(830, 200, _tabs[0])
                              };

            _proliferatorComboBox = MyComboBox.MyComboBox.CreateComboBox<ProliferatorComboBox>(30, 290, _tabs[0], "默认增产策略");

            CreateLabelText("工厂", 255, 0);
            CreateLabelText("配方选取", 415, 0);
            CreateLabelText("增产策略", 850, 0);

            _labelText.GetComponent<Text>().text = "物品".TranslateFromJson();
            Util.NormalizeRectWithTopLeft(_labelText.transform, -5, 0);
        }

        public void PostInit()
        {
            _pauseButton.onClick += OnPauseButtonClick;

            _tabButtons[0].onClick += OnTabButtonClick;
            _tabButtons[1].onClick += OnTabButtonClick;

            _tabs[0].gameObject.SetActive(true);

            if (RecipeTypeFactoryMap == null) RecipeTypeFactoryMap = GetFactoryDict();

            _data = new NodeDataSet();
            _productDetailPool = new List<ProductDetail>();

            foreach (KeyValuePair<Utils_ERecipeType, ItemComboBox> pair in _recipeMachines)
            {
                List<ItemProto> recipeTypeFactory = RecipeTypeFactoryMap[pair.Key];
                pair.Value.Init(pair.Key, recipeTypeFactory);
                _data.SetDefaultMachine(pair.Key, recipeTypeFactory[0]);
                pair.Value.OnItemChange += DefaultMachinesChange;
            }

            _proliferatorComboBox.Init();
            _proliferatorComboBox.OnIndexChange += OnProliferatorChange;

            _data.OnNeedRefreshed += RefreshProductDetails;
        }

        private void AddTestItem()
        {
            _data.AddItemNeed(LDB.items.Select(1404), 720);
        }

        private void RefreshProductDetails()
        {
            _productDetailPool.ForEach(i => i.gameObject.SetActive(false));
            curReuseIndex = 0;

            foreach (NodeData t in _data.Inputs)
            {
                // TODO: raw meterial layer output
            }

            foreach (NodeData t in _data.Byproducts)
            {
                // TODO: Byproducts meterial layer output
            }
            
            int y = 40;

            foreach (NodeData t in _data.Needs)
            {
                ProductDetail productDetail = GetProductDetail();
                productDetail.SetPos(y);
                productDetail.SetData(t);
                productDetail.gameObject.SetActive(true);

                y += 60;
            }

            foreach (NodeData t in _data.Datas.Values)
            {
                ProductDetail productDetail = GetProductDetail();
                productDetail.SetPos(y);
                productDetail.SetData(t);
                productDetail.gameObject.SetActive(true);

                y += 60;
            }
        }

        private ProductDetail GetProductDetail()
        {
            if (curReuseIndex >= _productDetailPool.Count)
            {
                var productDetail = ProductDetail.CreateProductDetail(0, 40, _listContent);
                productDetail.Init();
                _productDetailPool.Add(productDetail);
                return productDetail;
            }
            else
            {
                return _productDetailPool[curReuseIndex++];
            }
        }

        private void OnProliferatorChange(int obj) => _data.SetDefaultStrategy(_proliferatorComboBox.Strategy);

        private void DefaultMachinesChange((Utils_ERecipeType type, ItemProto proto) obj) => _data.SetDefaultMachine(obj.type, obj.proto);

        public void OnTabButtonClick(int idx) => SetTabIndex(idx, false);

        public void SetTabIndex(int index, bool immediate)
        {
            if (_tabIndex != index | immediate)
                for (int index1 = 0; index1 < _tabButtons.Length; ++index1)
                {
                    if (index1 == index)
                    {
                        _tabTexts[index1].color = Color.white;
                        _tabTweeners[index1].gameObject.SetActive(true);
                        _tabTweeners[index1].Play0To1();
                    }
                    else
                    {
                        _tabTexts[index1].color = new Color(1f, 1f, 1f, 0.55f);
                        _tabTweeners[index1].normalizedTime = 0.0f;
                        _tabTweeners[index1].gameObject.SetActive(false);
                    }
                }

            if (immediate)
            {
                _tabSlider.rectTransform.anchoredPosition = new Vector2(160 * index, _tabSlider.rectTransform.anchoredPosition.y);
            }
            else if (_tabIndex == index)
            {
                Vector2 anchoredPosition = _tabSlider.rectTransform.anchoredPosition;
                _tabSlider.rectTransform.anchoredPosition = Vector2.Lerp(anchoredPosition, new Vector2(160 * index, anchoredPosition.y), 0.2f);
            }

            _tabIndex = index;
        }

        private void CreateLabelText(string s, float left, float top)
            => Util.NormalizeRectWithTopLeft(Util.CreateLabelText(_labelText, s.TranslateFromJson()), left, top);

        public override void _OnUpdate()
        {
            SetTabIndex(_tabIndex, false);

            if (ProjectGenesis.QToolsHotkey.Value.IsDown())
            {
                if (!isOpening)
                    OpenWindow();
                else
                    CloseWindow();
            }

            if (isOpening && VFInput.escape)
            {
                VFInput.UseEscape();
                CloseWindow();
            }
        }

        public void LateUpdate()
        {
            bool fullscreenPaused = GameMain.isFullscreenPaused;
            _pauseButton.highlighted = !fullscreenPaused;
            _pauseImg.sprite = fullscreenPaused ? _pauseSprite : _playSprite;
            _pauseText.text = (fullscreenPaused ? _pauseString : _playString).Translate();
        }

        public void OpenWindow()
        {
            MyWindowCtl.OpenWindow(this);
            SetTabIndex(0, false);
            isOpening = true;
            GameMain.isFullscreenPaused = true;
        }

        public void CloseWindow()
        {
            _Close();
            isOpening = false;
            if (UIRoot.instance.uiGame.starmap.fastTravelling) return;
            GameMain.isFullscreenPaused = false;
        }

        public void OnPauseButtonClick(int obj)
        {
            if (UIRoot.instance.uiGame.starmap.fastTravelling) return;
            GameMain.isFullscreenPaused = !GameMain.isFullscreenPaused;
        }

        internal static UIQToolsWindow CreateWindow()
        {
            UIOptionWindow srcWin = UIRoot.instance.optionWindow;
            GameObject src = srcWin.gameObject;
            GameObject go = Instantiate(src, UIRoot.instance.uiGame.recipePicker.transform.parent);
            go.name = "ui-qtools";
            go.SetActive(false);
            Destroy(go.GetComponent<UIOptionWindow>());
            var win = go.AddComponent<UIQToolsWindow>();

            UIDETopFunction controlPanelTopFunction = UIRoot.instance.uiGame.dysonEditor.controlPanel.topFunction;
            UIButton pauseButton = controlPanelTopFunction.pauseButton;
            GameObject pGameObject = pauseButton.gameObject;
            GameObject p = Instantiate(pGameObject, go.transform);
            p.name = "ui-qtools-pauseButton";
            win._pauseButton = p.GetComponent<UIButton>();
            win._playSprite = controlPanelTopFunction.playSprite;
            win._pauseSprite = controlPanelTopFunction.pauseSprite;
            win._playString = controlPanelTopFunction.playString;
            win._pauseString = controlPanelTopFunction.pauseString;
            win._pauseImg = p.transform.GetChild(1).GetComponent<Image>();
            win._pauseText = p.GetComponentInChildren<Text>();

            win._tabs = new RectTransform[2];
            win._tabButtons = new UIButton[2];
            win._tabTweeners = new Tweener[2];
            win._tabTexts = new Text[2];

            Transform goTransform = go.transform;

            for (int i = 0; i < goTransform.childCount; i++)
            {
                Transform child = goTransform.GetChild(i);

                if (child.name == "title-text")
                {
                    DestroyImmediate(child.GetComponent<Localizer>());
                    var component = child.GetComponent<Text>();
                    component.text = "量化计算器".TranslateFromJson();
                    Util.NormalizeRectWithTopLeft(component, 80, 10);
                }

                if (child.name == "tab-line")
                {
                    for (int j = 3; j < child.childCount; j++)
                    {
                        Destroy(child.GetChild(j).gameObject);
                    }

                    for (int j = 1; j < 3; j++)
                    {
                        Transform transform = child.GetChild(j);
                        DestroyImmediate(transform.GetComponentInChildren<Localizer>());
                        var labelText = transform.GetComponentInChildren<Text>();
                        labelText.text = (j == 1 ? "设置" : "量化").TranslateFromJson();
                        win._tabTexts[j - 1] = labelText;
                        var btn = transform.GetComponent<UIButton>();
                        win._tabButtons[j - 1] = btn;
                    }

                    win._tabSlider = child.GetChild(0).GetComponent<Image>();
                }

                if (child.name == "details")
                {
                    Transform transform = child.GetChild(0);

                    win._tabTweeners[0] = transform.GetComponent<Tweener>();
                    win._tabs[0] = transform.GetComponent<RectTransform>();

                    for (int k = 0; k < transform.childCount; k++)
                    {
                        Destroy(transform.GetChild(k).gameObject);
                    }

                    transform = child.GetChild(3);

                    win._tabTweeners[1] = transform.GetComponent<Tweener>();
                    win._tabs[1] = transform.GetComponent<RectTransform>();

                    for (int k = 0; k < transform.childCount; k++)
                    {
                        Transform t = transform.GetChild(k);

                        if (t.name == "revert-button")
                        {
                            Destroy(t.gameObject);
                        }

                        if (t.name == "title")
                        {
                            win._labelText = t.GetChild(0).gameObject;
                            Destroy(win._labelText.GetComponent<Localizer>());

                            for (int j = 1; j < t.childCount; j++)
                            {
                                Destroy(t.GetChild(j).gameObject);
                            }
                        }

                        if (t.name == "list")
                        {
                            win._list = t.GetComponent<RectTransform>();
                            Vector2 sizeDelta = win._list.sizeDelta;
                            win._list.sizeDelta = new Vector2(sizeDelta.x - 400, sizeDelta.y);

                            Transform content = t.GetChild(0).GetChild(0).GetChild(0);
                            win._listContent = content.GetComponent<RectTransform>();

                            for (int j = 0; j < content.childCount; j++)
                            {
                                Destroy(content.GetChild(j).gameObject);
                            }
                        }

                        if (t.name == "cancel-button" || t.name == "apply-button") Destroy(t.gameObject);
                    }

                    var o = new GameObject();
                    win._rightInfoArea = o.AddComponent<RectTransform>();
                    Util.NormalizeRectWithTopLeft(win._rightInfoArea, 1360, 52);
                    o.name = "rightInfoArea";

                    Destroy(child.GetChild(1).gameObject);
                    Destroy(child.GetChild(2).gameObject);
                    Destroy(child.GetChild(4).gameObject);
                }

                if (child.name == "details") { }
            }

            win.CreateUI();

            win._Create();
            win._Init(win.data);

            return win;
        }
    }
}
