using System;
using System.Collections.Generic;
using CommonAPI.Systems;
using ProjectGenesis.Patches.UI.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectGenesis.Patches.UI.UIQTools
{
    public class ProductDetail : MonoBehaviour
    {
        private static UIItemTip _uiItemTip;

        private RectTransform _rect;

        public Image itemImg;
        public UIButton itemImgButton;
        public Text itemCountText;

        public Image factoryImg;
        public UIButton factoryImgButton;
        public Text factoryCountText;

        public Image recipeImg;
        public UIButton recipeImgButton;
        public UIRecipeEntry recipeEntry;

        public ItemComboBox proliferatorComboBox;

        public ItemProto item;
        public float itemCount;
        public ItemProto factory;
        public RecipeProto recipe;

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
            cb.itemImgButton.button.onClick.RemoveAllListeners();
            cb.itemImgButton.button.onClick.AddListener(cb.OnItemButtonClick);
            cb.itemImgButton.onRightClick += cb.OnItemButtonRightClick;

            Util.NormalizeRectWithTopLeft(Util.CreateText("\u00d7", 18), 80, 10, cb._rect);
            cb.itemCountText = Util.CreateText("", 18);
            Util.NormalizeRectWithTopLeft(cb.itemCountText, 100, 10, cb._rect);

            Util.CreateSignalIcon("", "左键点击：更换生产设备", out UIButton factorybutton, out Image factoryImage);
            cb.factoryImg = factoryImage;
            cb.factoryImgButton = factorybutton;
            Util.NormalizeRectWithTopLeft(factorybutton, 280, 0, cb._rect);
            cb.factoryImgButton.button.onClick.RemoveAllListeners();
            cb.factoryImgButton.button.onClick.AddListener(cb.OnFactoryButtonClick);

            Util.NormalizeRectWithTopLeft(Util.CreateText("\u00d7", 18), 340, 10, cb._rect);
            cb.factoryCountText = Util.CreateText("", 18);
            Util.NormalizeRectWithTopLeft(cb.factoryCountText, 360, 10, cb._rect);

            Util.CreateSignalIcon("", "左键点击：更换配方", out UIButton recipebutton, out Image recipeImage);
            cb.recipeImg = recipeImage;
            cb.recipeImgButton = recipebutton;
            Util.NormalizeRectWithTopLeft(recipebutton, 440, 0, cb._rect);
            cb.recipeImgButton.button.onClick.RemoveAllListeners();
            cb.recipeImgButton.button.onClick.AddListener(cb.OnRecipeButtonClick);

            cb.recipeEntry = NewUIRecipeEntry(cb._rect);
            Util.NormalizeRectWithTopLeft(cb.recipeEntry, 500, 0, cb._rect);
            cb.recipeEntry.gameObject.SetActive(true);

            cb.proliferatorComboBox = MyComboBox.CreateComboBox<ItemComboBox>(700, 10, cb._rect);

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
            proliferatorComboBox.Init(new List<int> { 509, 1143, 1143 }, new List<string> { "不使用增产剂", "增产", "加速" });
        }

        public void SetData(
            int itemId,
            float count,
            int recipeId,
            int factoryId)
        {
            SetItem(LDB.items.Select(itemId), count);
            ChangeRecipe(LDB.recipes.Select(recipeId));
            ChangeFactory(LDB.items.Select(factoryId));
        }

        public void OnItemButtonClick() { }

        private void OnItemButtonRightClick(int obj) { }

        public void OnFactoryButtonClick() { }

        public void OnRecipeButtonClick() => UIRecipePickerExtension.Popup(new Vector2(300f, -135f), ChangeRecipe, Filter);

        private void SetItem(ItemProto itemProto, float count)
        {
            item = itemProto;
            itemCount = count;
            itemImg.sprite = itemProto.iconSprite;
            itemImgButton.tips.tipTitle = itemProto.name;
            itemCountText.text = count.ToString("F2");
        }

        public void ChangeFactory(ItemProto factoryProto)
        {
            factory = factoryProto;
            factoryImg.sprite = factoryProto.iconSprite;
            factoryImgButton.tips.tipTitle = factoryProto.name;
            factoryCountText.text = CalcFactoryCount().ToString("F2");
        }

        private float CalcFactoryCount()
        {
            int idx = Array.IndexOf(recipe.Results, item.ID);

            return itemCount / recipe.ResultCounts[idx] * recipe.TimeSpend / factory.prefabDesc.assemblerSpeed / 0.36f;
        }

        public void ChangeRecipe(RecipeProto recipeProto)
        {
            recipe = recipeProto;
            recipeImg.sprite = recipeProto.iconSprite;
            recipeImgButton.tips.tipTitle = recipeProto.name;
            recipeEntry.SetRecipe(recipeProto);
        }

        private bool Filter(RecipeProto recipeProto) => item.recipes.Contains(recipeProto);
    }
}
