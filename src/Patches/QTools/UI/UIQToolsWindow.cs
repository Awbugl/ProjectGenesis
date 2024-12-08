using System.Collections.Generic;
using CommonAPI.Systems;
using NGPT;
using ProjectGenesis.Utils;
using UnityEngine;
using UnityEngine.UI;
using static ProjectGenesis.Patches.QTools;
using Utils_ERecipeType = ProjectGenesis.Utils.ERecipeType;

namespace ProjectGenesis.Patches
{
    /// <summary>
    ///     special thanks to https://github.com/hetima/DSP_PlanetFinder/tree/main/PlanetFinder
    ///     special thanks to https://github.com/starfi5h/DSP_Mod_Support/tree/main/FactoryLocator/src/UI
    /// </summary>

    // ReSharper disable once InconsistentNaming
    public class UIQToolsWindow : ManualBehaviour
    {
        public bool isOpening;
        private InputField _addItemCountInput;
        private Text _asRawsLabelText;
        private Text _byproductsLabelText;
        private UIButton _clearNeedsButton;

        private UIButton _clearOptionsButton;
        private string _countText;

        private NodeDataSet _data;
        private Text _factoryLabelText;
        private ObjectPool<ItemNeedDetail> _itemCounterPool;

        private GameObject _labelTextPrefab;
        private RectTransform _list;
        private RectTransform _listContent;

        private Text _needLabelText;
        private ObjectPool<ItemNeedDetail> _needPool;

        private UIButton _pauseButton;
        private Image _pauseImg;
        private Sprite _pauseSprite;
        private string _pauseString;
        private Text _pauseText;
        private Sprite _playSprite;
        private string _playString;
        private ObjectPool<ProductDetail> _productDetailPool;

        private ProliferatorComboBox _proliferatorComboBox;
        private Text _rawsLabelText;
        private Dictionary<Utils_ERecipeType, ItemComboBox> _recipeMachines;
        private RectTransform _rightContent;
        private RectTransform _rightInfo;

        private UIButton _selectItemButton;
        private UIButton[] _tabButtons;
        private int _tabIndex;

        private RectTransform[] _tabs;
        private Image _tabSlider;
        private Text[] _tabTexts;
        private Tweener[] _tabTweeners;

        public void LateUpdate()
        {
            bool fullscreenPaused = GameMain.isFullscreenPaused;
            _pauseButton.highlighted = !fullscreenPaused;
            _pauseImg.sprite = fullscreenPaused ? _pauseSprite : _playSprite;
            _pauseText.text = (fullscreenPaused ? _pauseString : _playString).Translate();
        }

        private void CreateUI()
        {
            MyKeyBinder.CreateKeyBinder(30, 20, _tabs[0], ProjectGenesis.QToolsHotkey, "计算器快捷键");

            _recipeMachines = new Dictionary<Utils_ERecipeType, ItemComboBox>
            {
                [Utils_ERecipeType.Smelt] = MyComboBox.CreateComboBox<ItemComboBox>(30, 110, _tabs[0], "默认生产设备"),
                [Utils_ERecipeType.矿物处理] = MyComboBox.CreateComboBox<ItemComboBox>(430, 110, _tabs[0]),
                [Utils_ERecipeType.Assemble] = MyComboBox.CreateComboBox<ItemComboBox>(30, 155, _tabs[0]),
                [Utils_ERecipeType.标准制造] = MyComboBox.CreateComboBox<ItemComboBox>(430, 155, _tabs[0]),
                [Utils_ERecipeType.高精度加工] = MyComboBox.CreateComboBox<ItemComboBox>(30, 200, _tabs[0]),
                [Utils_ERecipeType.Refine] = MyComboBox.CreateComboBox<ItemComboBox>(430, 200, _tabs[0]),
                [Utils_ERecipeType.Chemical] = MyComboBox.CreateComboBox<ItemComboBox>(30, 245, _tabs[0]),
                [Utils_ERecipeType.高分子化工] = MyComboBox.CreateComboBox<ItemComboBox>(430, 245, _tabs[0]),
                [Utils_ERecipeType.Particle] = MyComboBox.CreateComboBox<ItemComboBox>(30, 290, _tabs[0]),
                [Utils_ERecipeType.Research] = MyComboBox.CreateComboBox<ItemComboBox>(430, 290, _tabs[0]),
            };

            _proliferatorComboBox = MyComboBox.CreateComboBox<ProliferatorComboBox>(30, 380, _tabs[0], "默认增产策略");

            _clearOptionsButton = Util.MakeSmallTextButton("清空设置".TranslateFromJson(), 80, 24);
            Util.NormalizeRectWithTopLeft(_clearOptionsButton, 1635, 2, _labelTextPrefab.transform);

            CreateLabelText("工厂", 255, 0);
            CreateLabelText("配方选取", 415, 0);
            CreateLabelText("增产策略", 850, 0);

            _labelTextPrefab.GetComponent<Text>().text = "物品".TranslateFromJson();
            Util.NormalizeRectWithTopLeft(_labelTextPrefab.transform, -5, 0);

            Util.NormalizeRectWithTopLeft(Util.CreateLabelText(_labelTextPrefab, "添加需求：".TranslateFromJson()), 20, 19, _rightContent);

            _needLabelText = Util.CreateLabelText(_labelTextPrefab, "需求：".TranslateFromJson());
            Util.NormalizeRectWithTopLeft(_needLabelText, 20, 60, _rightContent);

            _asRawsLabelText = Util.CreateLabelText(_labelTextPrefab, "额外输入：".TranslateFromJson());
            Util.NormalizeRectWithTopLeft(_asRawsLabelText, 20, 160, _rightContent);

            _rawsLabelText = Util.CreateLabelText(_labelTextPrefab, "原料需求：".TranslateFromJson());
            Util.NormalizeRectWithTopLeft(_rawsLabelText, 20, 260, _rightContent);

            _byproductsLabelText = Util.CreateLabelText(_labelTextPrefab, "副产物：".TranslateFromJson());
            Util.NormalizeRectWithTopLeft(_byproductsLabelText, 20, 360, _rightContent);

            _factoryLabelText = Util.CreateLabelText(_labelTextPrefab, "工厂：".TranslateFromJson());
            Util.NormalizeRectWithTopLeft(_factoryLabelText, 20, 460, _rightContent);

            var inputObj = GameObject.Find("UI Root/Overlay Canvas/In Game/Planet & Star Details/planet-detail-ui/name-input");
            GameObject go = Instantiate(inputObj, _rightContent);
            go.name = "input";
            Util.NormalizeRectWithTopLeft(go.transform, 60, 20);

            _addItemCountInput = go.GetComponent<InputField>();
            _addItemCountInput.GetComponent<RectTransform>().sizeDelta = new Vector2(80, 28);
            _addItemCountInput.text = "60";
            _countText = _addItemCountInput.text;

            Util.NormalizeRectWithTopLeft(_addItemCountInput, 120, 20, _rightContent);

            _selectItemButton = Util.MakeSmallTextButton("选择物品".TranslateFromJson(), 80, 24);
            Util.NormalizeRectWithTopLeft(_selectItemButton, 210, 22, _rightContent);
            _clearNeedsButton = Util.MakeSmallTextButton("清空需求".TranslateFromJson(), 80, 24);
            Util.NormalizeRectWithTopLeft(_clearNeedsButton, 300, 22, _rightContent);
        }

        public override bool _OnInit()
        {
            _pauseButton.onClick += OnPauseButtonClick;

            _tabButtons[0].onClick += OnTabButtonClick;
            _tabButtons[1].onClick += OnTabButtonClick;

            _clearOptionsButton.button.onClick.RemoveAllListeners();
            _clearOptionsButton.button.onClick.AddListener(OnClearOptionsButtonClick);

            _addItemCountInput.onValueChanged.RemoveAllListeners();
            _addItemCountInput.onValueChanged.AddListener(OnInputValueChanged);
            _addItemCountInput.characterValidation = InputField.CharacterValidation.Decimal;

            _selectItemButton.button.onClick.RemoveAllListeners();
            _selectItemButton.button.onClick.AddListener(OnItemSelectButtonClick);

            _clearNeedsButton.button.onClick.RemoveAllListeners();
            _clearNeedsButton.button.onClick.AddListener(OnClearNeedsButtonClick);

            _tabs[0].gameObject.SetActive(true);

            _data = new NodeDataSet();
            _productDetailPool = new ObjectPool<ProductDetail>(GetProductDetail);
            _itemCounterPool = new ObjectPool<ItemNeedDetail>(GetItemCounter);
            _needPool = new ObjectPool<ItemNeedDetail>(GetItemNeedDetail);

            foreach (KeyValuePair<Utils_ERecipeType, ItemComboBox> pair in _recipeMachines)
            {
                List<ItemProto> recipeTypeFactory = RecipeTypeFactoryMap[pair.Key];

                if (!DefaultMachine.TryGetValue(pair.Key, out ItemProto factory))
                {
                    pair.Value.Init(pair.Key, recipeTypeFactory, 0);
                    SetDefaultMachine(pair.Key, recipeTypeFactory[0]);
                }
                else
                {
                    var index = recipeTypeFactory.IndexOf(factory);
                    pair.Value.Init(pair.Key, recipeTypeFactory, index);
                }

                pair.Value.OnItemChange += DefaultMachinesChange;
            }

            _proliferatorComboBox.Init(0);
            _proliferatorComboBox.OnIndexChange += OnProliferatorChange;

            _data.OnNeedRefreshed += RefreshProductDetails;

            return true;
        }

        private void RefreshProductDetails()
        {
            _productDetailPool.RecycleAll();

            var y = 20;

            foreach (NodeData t in _data.Datas.Values)
            {
                ProductDetail productDetail = _productDetailPool.Alloc();
                productDetail.SetPos(y);
                productDetail.SetData(t);
                y += 60;
            }

            Util.NormalizeRectWithTopLeft(_needLabelText, 20, 60, _rightContent);

            _needPool.RecycleAll();

            int x = -180;
            y = 100;

            foreach (NodeData t in _data.Needs.Values)
            {
                x += 200;

                if (x > 350)
                {
                    x = 20;
                    y += 45;
                }

                ItemNeedDetail counter = _needPool.Alloc();
                counter.SetPos(x, y);
                counter.SetData(t, true);
            }

            Util.NormalizeRectWithTopLeft(_asRawsLabelText, 20, y + 60, _rightContent);

            x = -180;
            y += 100;

            foreach (NodeData t in _data.AsRaws.Values)
            {
                x += 200;

                if (x > 350)
                {
                    x = 20;
                    y += 45;
                }

                ItemNeedDetail counter = _needPool.Alloc();
                counter.SetPos(x, y);
                counter.SetData(t);
            }

            _itemCounterPool.RecycleAll();

            Util.NormalizeRectWithTopLeft(_rawsLabelText, 20, y + 60, _rightContent);

            x = -180;
            y += 100;

            foreach (NodeData t in _data.Raws.Values)
            {
                x += 200;

                if (x > 350)
                {
                    x = 20;
                    y += 45;
                }

                ItemNeedDetail counter = _itemCounterPool.Alloc();
                counter.SetPos(x, y);
                counter.SetData(t);
            }

            Util.NormalizeRectWithTopLeft(_byproductsLabelText, 20, y + 60, _rightContent);

            x = -180;
            y += 100;

            foreach (NodeData t in _data.Byproducts.Values)
            {
                x += 200;

                if (x > 350)
                {
                    x = 20;
                    y += 45;
                }

                ItemNeedDetail counter = _itemCounterPool.Alloc();
                counter.SetPos(x, y);
                counter.SetData(t);
            }

            Util.NormalizeRectWithTopLeft(_factoryLabelText, 20, y + 60, _rightContent);

            x = -180;
            y += 100;

            _data.CalcFactories();

            foreach (NodeData t in _data.Factories.Values)
            {
                x += 200;

                if (x > 350)
                {
                    x = 20;
                    y += 45;
                }

                ItemNeedDetail counter = _itemCounterPool.Alloc();
                counter.SetPos(x, y);
                counter.SetData(t, false, "F0");
            }
        }

        private ProductDetail GetProductDetail() => ProductDetail.CreateProductDetail(0, 40, _listContent);

        private ItemNeedDetail GetItemCounter() => ItemNeedDetail.CreateItemNeedDetail(0, 40, _rightContent, false);

        private ItemNeedDetail GetItemNeedDetail() => ItemNeedDetail.CreateItemNeedDetail(0, 40, _rightContent);

        private void OnProliferatorChange(int obj) => SetDefaultStrategy(_proliferatorComboBox.Strategy);

        private void DefaultMachinesChange((Utils_ERecipeType type, ItemProto proto) obj) => SetDefaultMachine(obj.type, obj.proto);

        public void OnTabButtonClick(int idx) => SetTabIndex(idx, false);

        public void OnInputValueChanged(string s) => _countText = s;

        public void OnItemSelectButtonClick()
        {
            UIItemPickerExtension.Popup(new Vector2(-400f, 300f), OnItemSelectReturn, true, null);

            UIRoot.instance.uiGame.itemPicker.transform.SetAsLastSibling();
        }

        public void OnClearOptionsButtonClick() => ClearOptions();

        public void OnClearNeedsButtonClick() => _data.ClearNeeds();

        private void OnItemSelectReturn(ItemProto obj)
        {
            if (obj == null) return;

            if (float.TryParse(_countText, out float count) && count > 0)
                _data.AddItemNeed(obj, count);
            else
                UIRealtimeTip.Popup("输入的数值有误".TranslateFromJson());
        }

        public void SetTabIndex(int index, bool immediate)
        {
            if ((_tabIndex != index) | immediate)
                for (var index1 = 0; index1 < _tabButtons.Length; ++index1)
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
                _tabSlider.rectTransform.anchoredPosition =
                    Vector2.Lerp(anchoredPosition, new Vector2(160 * index, anchoredPosition.y), 0.2f);
            }

            _tabIndex = index;
        }

        private void CreateLabelText(string s, float left, float top) =>
            Util.NormalizeRectWithTopLeft(Util.CreateLabelText(_labelTextPrefab, s.TranslateFromJson()), left, top);

        public override void _OnUpdate()
        {
            SetTabIndex(_tabIndex, false);

            if (ProjectGenesis.QToolsHotkey.Value.IsDown())
            {
                if (isOpening)
                    CloseWindow();
                else
                    OpenWindow();
            }

            if (isOpening && VFInput.escape)
            {
                VFInput.UseEscape();
                CloseWindow();
            }
        }

        public void OpenWindow()
        {
            Util.OpenWindow(this);
            SetTabIndex(_data.IsEmpty() ? 0 : 1, false);
            Util.NormalizeRectWithTopLeft(_rightInfo, _list.rect.width, 52);
            isOpening = true;
            GameMain.isFullscreenPaused = true;
        }

        public void CloseWindow()
        {
            _Close();
            isOpening = false;

            if (GameMain.mainPlayer.fastTravelling) return;

            GameMain.isFullscreenPaused = false;
        }

        public void OnPauseButtonClick(int obj)
        {
            if (GameMain.mainPlayer.fastTravelling) return;

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
            UIQToolsWindow win = go.AddComponent<UIQToolsWindow>();

            UITechTree uiGameTechTree = UIRoot.instance.uiGame.techTree;
            UIButton pauseButton = uiGameTechTree.pauseButton;
            GameObject pGameObject = pauseButton.gameObject;
            GameObject p = Instantiate(pGameObject, go.transform);
            p.name = "ui-qtools-pauseButton";
            win._pauseButton = p.GetComponent<UIButton>();
            win._playSprite = uiGameTechTree.playSprite;
            win._pauseSprite = uiGameTechTree.pauseSprite;
            win._playString = uiGameTechTree.playString;
            win._pauseString = uiGameTechTree.pauseString;
            Transform pauseButtonContent = p.transform.GetChild(2);
            win._pauseImg = pauseButtonContent.GetComponentInChildren<Image>();
            win._pauseText = pauseButtonContent.GetComponentInChildren<Text>();

            win._tabs = new RectTransform[2];
            win._tabButtons = new UIButton[2];
            win._tabTweeners = new Tweener[2];
            win._tabTexts = new Text[2];

            Transform goTransform = go.transform;

            for (var i = 0; i < goTransform.childCount; i++)
            {
                Transform child = goTransform.GetChild(i);

                if (child.name == "title-text")
                {
                    DestroyImmediate(child.GetComponent<Localizer>());
                    Text component = child.GetComponent<Text>();
                    component.text = "量化计算器".TranslateFromJson();
                    Util.NormalizeRectWithTopLeft(component, 80, 10);
                }

                if (child.name == "tab-line")
                {
                    for (var j = 3; j < child.childCount; j++) Destroy(child.GetChild(j).gameObject);

                    for (var j = 1; j < 3; j++)
                    {
                        Transform transform = child.GetChild(j);
                        DestroyImmediate(transform.GetComponentInChildren<Localizer>());
                        Text labelText = transform.GetComponentInChildren<Text>();
                        labelText.text = (j == 1 ? "设置" : "量化").TranslateFromJson();
                        win._tabTexts[j - 1] = labelText;
                        UIButton btn = transform.GetComponent<UIButton>();
                        win._tabButtons[j - 1] = btn;
                    }

                    win._tabSlider = child.GetChild(0).GetComponent<Image>();
                }

                if (child.name == "details")
                {
                    Transform transform = child.GetChild(0);

                    win._tabTweeners[0] = transform.GetComponent<Tweener>();
                    win._tabs[0] = transform.GetComponent<RectTransform>();

                    for (var k = 0; k < transform.childCount; k++) Destroy(transform.GetChild(k).gameObject);

                    transform = child.GetChild(3);

                    win._tabTweeners[1] = transform.GetComponent<Tweener>();
                    win._tabs[1] = transform.GetComponent<RectTransform>();

                    for (var k = 0; k < transform.childCount; k++)
                    {
                        Transform t = transform.GetChild(k);

                        if (t.name == "revert-button") Destroy(t.gameObject);

                        if (t.name == "title")
                        {
                            win._labelTextPrefab = t.GetChild(0).gameObject;
                            Destroy(win._labelTextPrefab.GetComponent<Localizer>());

                            for (var j = 1; j < t.childCount; j++) Destroy(t.GetChild(j).gameObject);
                        }

                        if (t.name == "list")
                        {
                            win._list = t.GetComponent<RectTransform>();
                            Vector2 sizeDelta = win._list.sizeDelta;
                            win._list.sizeDelta = new Vector2(sizeDelta.x - 400, sizeDelta.y);

                            Transform content = t.GetChild(0).GetChild(0).GetChild(0);
                            win._listContent = content.GetComponent<RectTransform>();

                            for (var j = 0; j < content.childCount; j++) Destroy(content.GetChild(j).gameObject);
                        }
                    }

                    win._rightInfo = Instantiate(win._list, win._list.parent);
                    win._rightInfo.name = "rightInfoArea";
                    win._rightContent = win._rightInfo.GetChild(0).GetChild(0).GetChild(0).GetComponent<RectTransform>();
                    win._rightInfo.sizeDelta = new Vector2(400, 650);
                    Util.NormalizeRectWithTopLeft(win._rightInfo, win._list.rect.x, 52, transform);

                    Destroy(child.GetChild(1).gameObject);
                    Destroy(child.GetChild(2).gameObject);
                    Destroy(child.GetChild(4).gameObject);
                }

                if (child.name == "cancel-button" || child.name == "apply-button") Destroy(child.gameObject);
            }

            win.CreateUI();

            win._Create();
            win._Init(win.data);

            return win;
        }
    }
}
