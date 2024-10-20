using System.Collections.Generic;
using System.Text;
using CommonAPI.Systems;
using ProjectGenesis.Patches.Logic.QTools;
using ProjectGenesis.Patches.UI.QTools.MyComboBox;
using ProjectGenesis.Patches.UI.Utils;
using ProjectGenesis.Utils;
using UnityEngine;
using UnityEngine.UI;
using ProjectGenesis_Utils_ERecipeType = ProjectGenesis.Utils.ERecipeType;
using Utils_ERecipeType = ProjectGenesis.Utils.ERecipeType;

namespace ProjectGenesis.Patches.UI.QTools
{
    public class ProductDetail : MonoBehaviour
    {
        private static UIItemTip _uiItemTip;

        public Image itemImg;
        public UIButton itemImgButton;
        public Text itemCountText;

        public ItemComboBox factoryComboBox;
        public Text factoryCountText;
        public UIButton factoryButton;

        public UIButton recipeImgButton;
        public UIRecipeEntry recipeEntry;

        public Text proliferatorText;

        public ProliferatorComboBox currentProliferatorComboBox;

        private NodeData _data;

        private ProliferatorComboBox _proliferatorComboBoxNormal, _proliferatorComboBoxNonProductive;

        private RectTransform _rect;

        internal static ProductDetail CreateProductDetail(float x, float y, RectTransform parent)
        {
            var go = new GameObject { name = "my-combobox", };
            go.AddComponent<RectTransform>();
            ProductDetail cb = go.AddComponent<ProductDetail>();
            cb._rect = Util.NormalizeRectWithTopLeft(cb, x, y, parent);
            go.name = "my-combobox";

            Util.CreateSignalIcon("", "右键点击：将其设置为原材料", out UIButton itemButton, out Image itemImage);
            cb.itemImg = itemImage;
            cb.itemImgButton = itemButton;
            Util.NormalizeRectWithTopLeft(itemButton, 20, 0, cb._rect);
            cb.itemImgButton.onRightClick += cb.OnItemButtonRightClick;

            Util.NormalizeRectWithTopLeft(Util.CreateText("\u2573", 18), 80, 11, cb._rect);
            cb.itemCountText = Util.CreateText("", 18);
            Util.NormalizeRectWithTopLeft(cb.itemCountText, 100, 10, cb._rect);

            cb.factoryComboBox = MyComboBox.MyComboBox.CreateComboBox<ItemComboBox>(100, 5, cb._rect);
            cb.factoryComboBox.comboBox.gameObject.SetActive(false);
            cb.factoryButton = cb.factoryComboBox.button;
            cb.factoryButton.tips.tipTitle = "";
            cb.factoryButton.tips.tipText = "左键点击：更换生产设备".TranslateFromJson();

            Util.NormalizeRectWithTopLeft(Util.CreateText("\u2573", 18), 320, 11, cb._rect);
            cb.factoryCountText = Util.CreateText("", 18);
            Util.NormalizeRectWithTopLeft(cb.factoryCountText, 340, 10, cb._rect);

            Util.CreateSignalIcon("可调整配方", "左键点击：更换配方", out UIButton recipebutton, out Image recipeImage);
            recipeImage.sprite = LDB.signals.IconSprite(401);
            cb.recipeImgButton = recipebutton;
            Util.NormalizeRectWithTopLeft(recipebutton, 440, 0, cb._rect);
            cb.recipeImgButton.button.onClick.RemoveAllListeners();
            cb.recipeImgButton.button.onClick.AddListener(cb.OnRecipeButtonClick);
            cb.recipeImgButton.onRightClick += cb.OnRecipeButtonRightClick;

            cb.recipeEntry = NewUIRecipeEntry(cb._rect);
            Util.NormalizeRectWithTopLeft(cb.recipeEntry, 550, 0, cb._rect);
            cb.recipeEntry.gameObject.SetActive(true);

            cb._proliferatorComboBoxNormal = MyComboBox.MyComboBox.CreateComboBox<ProliferatorComboBox>(700, 10, cb._rect);
            cb._proliferatorComboBoxNormal.gameObject.SetActive(false);
            cb._proliferatorComboBoxNormal.comboBox.gameObject.SetActive(false);

            cb._proliferatorComboBoxNonProductive = MyComboBox.MyComboBox.CreateComboBox<ProliferatorComboBox>(700, 10, cb._rect);
            cb._proliferatorComboBoxNormal.comboBox.gameObject.SetActive(false);
            cb._proliferatorComboBoxNonProductive.comboBox.gameObject.SetActive(false);

            cb.proliferatorText = Util.CreateText("", 18);
            Util.NormalizeRectWithTopLeft(cb.proliferatorText, 925, 15, cb._rect);

            cb.Init();

            return cb;
        }

        internal void SetPos(float y) => _rect = Util.NormalizeRectWithTopLeft(this, 0, y);

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
            _proliferatorComboBoxNormal.Init(0);
            _proliferatorComboBoxNonProductive.InitNoProductive(0);

            _proliferatorComboBoxNormal.OnIndexChange += OnProliferatorChange;
            _proliferatorComboBoxNonProductive.OnIndexChange += OnProliferatorChange;
        }

        internal void SetData(NodeData data)
        {
            _data = data;

            RecipeProto recipe = data.Options.Recipe;

            var type = (Utils_ERecipeType)recipe.Type;
            List<ItemProto> recipeTypeFactory = Logic.QTools.QTools.RecipeTypeFactoryMap[type];
            int index = recipeTypeFactory.IndexOf(data.Options.Factory);

            factoryComboBox.Init(type, recipeTypeFactory, index);
            factoryComboBox.OnItemChange += OnFactoryChange;

            itemImg.sprite = data.Item.iconSprite;
            itemImgButton.tips.tipTitle = data.Item.name;
            itemCountText.text = data.ItemCount.ToString("F2");

            factoryButton.tips.tipTitle = data.Options.Factory.name;
            factoryCountText.text = _data.Options.FactoryCount.ToString("F2");

            bool muitiRecipes = data.Item.recipes.Count > 1;
            bool canMining = !string.IsNullOrWhiteSpace(data.Item.miningFrom);
            bool buttonShow = muitiRecipes || canMining;

            if (buttonShow)
            {
                var sb = new StringBuilder();

                if (muitiRecipes) sb.AppendLine("左键点击：更换配方".TranslateFromJson());

                if (canMining) sb.AppendLine("右键点击：将其设置为原材料".TranslateFromJson());

                recipeImgButton.tips.tipTitle = (muitiRecipes ? canMining ? "可采集多配方物品" : "可调整配方" : "可采集物品").TranslateFromJson();
                recipeImgButton.tips.tipText = sb.ToString();
            }

            recipeImgButton.gameObject.SetActive(buttonShow);

            recipeEntry.SetRecipe(recipe);

            if (recipe.productive)
            {
                _proliferatorComboBoxNormal.gameObject.SetActive(true);
                _proliferatorComboBoxNonProductive.gameObject.SetActive(false);

                currentProliferatorComboBox = _proliferatorComboBoxNormal;
            }
            else
            {
                _proliferatorComboBoxNormal.gameObject.SetActive(false);
                _proliferatorComboBoxNonProductive.gameObject.SetActive(true);

                currentProliferatorComboBox = _proliferatorComboBoxNonProductive;
            }

            currentProliferatorComboBox.SetStrategySlience(data.Options.Strategy);
            proliferatorText.text = currentProliferatorComboBox.comboBox.m_Input.text;
        }

        private void OnFactoryChange((ProjectGenesis_Utils_ERecipeType, ItemProto proto) obj)
        {
            _data.Options.Factory = obj.proto;

            factoryButton.tips.tipTitle = obj.proto.name;
            RefreshFactoryCount();
            RefreshNeeds();
        }

        public void OnProliferatorChange(int obj)
        {
            proliferatorText.text = currentProliferatorComboBox.comboBox.m_Input.text;
            _data.Options.Strategy = currentProliferatorComboBox.Strategy;
            RefreshFactoryCount();
            RefreshNeeds();
        }

        private void OnItemButtonRightClick(int obj)
        {
            _data.Options.AsRaw = true;
            RefreshNeeds();
        }

        private void OnRecipeButtonRightClick(int obj)
        {
            if (string.IsNullOrWhiteSpace(_data.Item.miningFrom)) return;

            _data.Options.AsRaw = true;
            RefreshNeeds();
        }

        public void OnRecipeButtonClick()
        {
            if (_data.Item.recipes.Count < 2) return;

            UIRecipePickerExtension.Popup(new Vector2(-400f, 300f), OnRecipePickerReturn, true, Filter);

            UIRoot.instance.uiGame.recipePicker.transform.SetAsLastSibling();
        }

        private void RefreshNeeds() => _data.RefreshNeeds();

        private void RefreshFactoryCount()
        {
            _data.RefreshFactoryCount();
            factoryCountText.text = _data.Options.FactoryCount.ToString("F2");
        }

        public void OnRecipePickerReturn(RecipeProto recipeProto)
        {
            if (recipeProto == null) return;

            recipeEntry.SetRecipe(recipeProto);
            _data.Options.Recipe = recipeProto;

            _data.CheckFactory();
            RefreshFactoryCount();
            RefreshNeeds();
        }

        private bool Filter(RecipeProto recipeProto) => _data.Item.recipes.Contains(recipeProto);
    }
}
