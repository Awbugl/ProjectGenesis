using System;
using System.Collections.Generic;
using ProjectGenesis.Patches.UI.Utils;
using ProjectGenesis.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectGenesis.Patches.UI.QTools.MyComboBox
{
    public abstract class MyComboBox : MonoBehaviour
    {
        public int selectIndex;
        public Text labelText;
        public UIComboBox comboBox;
        public Image iconImg;
        public UIButton button;
        protected int DefaultSprite;

        public event Action<int> OnIndexChange;

        internal static T CreateComboBox<T>(float x, float y, RectTransform parent, string label = "", int fontSize = 18)
            where T : MyComboBox
        {
            UIComboBox src = UIRoot.instance.optionWindow.msaaComp;
            GameObject go = Instantiate(src.transform.parent.gameObject);

            go.name = "my-combobox";
            T cb = go.AddComponent<T>();
            cb.selectIndex = 0;

            RectTransform rect = Util.NormalizeRectWithTopLeft(cb, x, y, parent);

            cb.comboBox = go.GetComponentInChildren<UIComboBox>();

            DestroyImmediate(cb.GetComponent<Localizer>());
            cb.labelText = cb.GetComponent<Text>();
            cb.labelText.fontSize = fontSize;
            cb.SetLabelText(label);

            Util.CreateSignalIcon("", "", out UIButton button, out Image iconImage);
            cb.iconImg = iconImage;
            cb.button = button;

            Util.NormalizeRectWithTopLeft(button, 170, -5, rect);

            cb.button.button.onClick.RemoveAllListeners();
            cb.button.button.onClick.AddListener(cb.OnUIButtonClick);

            return cb;
        }

        protected void Init(List<string> items, int itemIndex, int defaultSprite)
        {
            comboBox.Items = items;
            DefaultSprite = defaultSprite;
            iconImg.sprite = LDB.signals.IconSprite(defaultSprite);
            comboBox.ItemButtons = new List<Button>();
            comboBox.UpdateItems();
            comboBox.onItemIndexChange.RemoveAllListeners();
            comboBox.onItemIndexChange.AddListener(ComboBoxIndexChange);
            comboBox.itemIndex = itemIndex;
        }

        public void OnUIButtonClick() => comboBox.itemIndex = (comboBox.itemIndex + 1) % comboBox.Items.Count;

        public void ComboBoxIndexChange()
        {
            selectIndex = comboBox.itemIndex;
            OnItemIndexChange();
            OnIndexChange?.Invoke(selectIndex);
        }

        public abstract void OnItemIndexChange();

        public void SetLabelText(string val)
        {
            if (labelText != null) labelText.text = val.TranslateFromJson();
        }
    }
}
