using CommonAPI.Systems;
using ProjectGenesis.Patches.Logic;
using ProjectGenesis.Patches.UI.QTools.MyComboBox;
using ProjectGenesis.Patches.UI.Utils;
using ProjectGenesis.Utils;
using UnityEngine;
using UnityEngine.UI;
using ERecipeType_1 = ERecipeType;
using ProjectGenesis_Utils_ERecipeType = ProjectGenesis.Utils.ERecipeType;
using Utils_ERecipeType = ProjectGenesis.Utils.ERecipeType;

namespace ProjectGenesis.Patches.UI.QTools
{
    public class ProductDetail : MonoBehaviour
    {
        private static UIItemTip _uiItemTip;
        private static GameObject _recipePickerTranslucentImageGameObject;

        private NodeData _data;

        private RectTransform _rect;

        public Image itemImg;
        public UIButton itemImgButton;
        public Text itemCountText;

        public ItemComboBox factoryComboBox;
        public Text factoryCountText;
        public UIButton factoryButton;

        public Image recipeImg;
        public UIButton recipeImgButton;
        public UIRecipeEntry recipeEntry;

        public ProliferatorComboBox proliferatorComboBox;

        internal static ProductDetail CreateProductDetail(float x, float y, RectTransform parent)
        {
            var go = new GameObject { name = "my-combobox" };
            go.AddComponent<RectTransform>();
            var cb = go.AddComponent<ProductDetail>();
            cb._rect = Util.NormalizeRectWithTopLeft(cb, x, y, parent);
            go.name = "my-combobox";

            Util.CreateSignalIcon("", "右键点击：将其设置为原材料", out UIButton itemButton, out Image itemImage);
            cb.itemImg = itemImage;
            cb.itemImgButton = itemButton;
            Util.NormalizeRectWithTopLeft(itemButton, 20, 0, cb._rect);
            cb.itemImgButton.onRightClick += cb.OnItemButtonRightClick;

            Util.NormalizeRectWithTopLeft(Util.CreateText("\u00d7", 18), 80, 10, cb._rect);
            cb.itemCountText = Util.CreateText("", 18);
            Util.NormalizeRectWithTopLeft(cb.itemCountText, 100, 10, cb._rect);

            cb.factoryComboBox = MyComboBox.MyComboBox.CreateComboBox<ItemComboBox>(100, 5, cb._rect);
            cb.factoryComboBox.comboBox.gameObject.SetActive(false);
            cb.factoryButton = cb.factoryComboBox.button;
            cb.factoryButton.tips.tipTitle = "";
            cb.factoryButton.tips.tipText = "左键点击：更换生产设备".TranslateFromJson();
            
            Util.NormalizeRectWithTopLeft(Util.CreateText("\u00d7", 18), 320, 10, cb._rect);
            cb.factoryCountText = Util.CreateText("", 18);
            Util.NormalizeRectWithTopLeft(cb.factoryCountText, 340, 10, cb._rect);

            Util.CreateSignalIcon("", "左键点击：更换配方", out UIButton recipebutton, out Image recipeImage);
            cb.recipeImg = recipeImage;
            cb.recipeImgButton = recipebutton;
            Util.NormalizeRectWithTopLeft(recipebutton, 440, 0, cb._rect);
            cb.recipeImgButton.button.onClick.RemoveAllListeners();
            cb.recipeImgButton.button.onClick.AddListener(cb.OnRecipeButtonClick);

            cb.recipeEntry = NewUIRecipeEntry(cb._rect);
            Util.NormalizeRectWithTopLeft(cb.recipeEntry, 500, 0, cb._rect);
            cb.recipeEntry.gameObject.SetActive(true);

            cb.proliferatorComboBox = MyComboBox.MyComboBox.CreateComboBox<ProliferatorComboBox>(700, 10, cb._rect);

            return cb;
        }

        private static UIRecipeEntry NewUIRecipeEntry(Transform parent)
        {
            if (_uiItemTip is null)
            {
                _uiItemTip = Instantiate(Configs.builtin.uiItemTipPrefab);
                _uiItemTip.gameObject.SetActive(false);
            }

            return Instantiate(_uiItemTip.recipeEntry, parent);
        }

        public void Init()
        {
            proliferatorComboBox.Init();
            proliferatorComboBox.OnIndexChange += OnProliferatorChange;
        }

        internal void SetData(NodeData data)
        {
            _data = data;

            var type = (Utils_ERecipeType)data.Options.Recipe.Type;

            factoryComboBox.Init(type, Logic.QTools.RecipeTypeFactoryMap[type]);
            factoryComboBox.OnItemChange += OnFactoryChange;

            itemImg.sprite = data.Item.iconSprite;
            itemImgButton.tips.tipTitle = data.Item.name;
            itemCountText.text = data.ItemCount.ToString("F2");
            factoryButton.tips.tipTitle = data.Options.Factory.name;

            ChangeRecipe(data.Options.Recipe, false);
        }

        public void SetEmpty() { }

        private void OnFactoryChange((ProjectGenesis_Utils_ERecipeType, ItemProto proto) obj)
        {
            _data.Options.Factory = obj.proto;
            
            factoryButton.tips.tipTitle = obj.proto.name;
            RefreshFactoryCount();
        }

        public void OnProliferatorChange(int obj)
        {
            _data.Options.Strategy = proliferatorComboBox.Strategy;
            RefreshFactoryCount();
            UpdateNeeds();
        }

        private void OnItemButtonRightClick(int obj) { }

        public void OnRecipeButtonClick()
        {
            UIRecipePickerExtension.Popup(VFInput.mouseMoveAxis, OnRecipePickerReturn, true, Filter);

            if (_recipePickerTranslucentImageGameObject == null)
                _recipePickerTranslucentImageGameObject = UIRoot.instance.uiGame.recipePicker.GetComponentInChildren<TranslucentImage>().gameObject;

            _recipePickerTranslucentImageGameObject.SetActive(false);
            UIRoot.instance.uiGame.recipePicker.transform.SetAsLastSibling();
        }

        public void ChangeRecipe(RecipeProto recipeProto, bool refresh = true)
        {
            if (recipeProto == null) return;

            recipeImg.sprite = recipeProto.iconSprite;
            recipeImgButton.tips.tipTitle = recipeProto.name;
            recipeEntry.SetRecipe(recipeProto);

            if (refresh)
            {
                _data.Options.Recipe = recipeProto;
                RefreshFactoryCount();
                UpdateNeeds();
            }
        }

        private void UpdateNeeds() => _data.UpdateNeeds();

        private void RefreshFactoryCount()
        {
            _data.RefreshFactoryCount();
            factoryCountText.text = _data.Options.FactoryCount.ToString("F2");
        }

        public void OnRecipePickerReturn(RecipeProto recipeProto)
        {
            ChangeRecipe(recipeProto);
            _recipePickerTranslucentImageGameObject.SetActive(true);
        }

        private bool Filter(RecipeProto recipeProto) => _data.Item.recipes.Contains(recipeProto);
    }
}
