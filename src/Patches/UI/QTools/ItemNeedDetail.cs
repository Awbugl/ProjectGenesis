﻿using ProjectGenesis.Patches.Logic;
using ProjectGenesis.Patches.UI.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectGenesis.Patches.UI.QTools
{
    public class ItemNeedDetail : MonoBehaviour
    {
        private RectTransform _rect;

        private Image _image;
        private Text _countText;

        private UIButton _button;
        private NodeData _data;
        private bool _isNeed;

        internal static ItemNeedDetail CreateItemNeedDetail(
            float x,
            float y,
            RectTransform parent,
            bool needButton = true)
        {
            GameObject src = Instantiate(Configs.builtin.uiItemTipPrefab.recipeEntry.transform.GetChild(1).gameObject);
            var cb = src.AddComponent<ItemNeedDetail>();

            cb._rect = Util.NormalizeRectWithTopLeft(cb, x, y, parent);
            src.name = "my-ItemCounter";
            cb._image = src.GetComponent<Image>();
            Destroy(src.GetComponentInChildren<Text>());

            Util.NormalizeRectWithTopLeft(Util.CreateText("\u00d7", 18), 50, 10, cb._rect);
            cb._countText = Util.CreateText("", 18);
            Util.NormalizeRectWithTopLeft(cb._countText, 70, 10, cb._rect);

            if (needButton)
            {
                cb._button = Util.MakeHiliteTextButton("\u00d7", 20, 20);
                Util.NormalizeRectWithTopLeft(cb._button, 125, 10, cb._rect);

                cb.Init();
            }

            return cb;
        }

        private void Init()
        {
            _button.button.onClick.RemoveAllListeners();
            _button.button.onClick.AddListener(OnButtonClick);
        }

        private void OnButtonClick()
        {
            if (_isNeed)
            {
                _data.RemoveNeed();
            }
            else
            {
                _data.Options.AsRaw = false;
                _data.UpdateNeeds();
            }
        }

        internal void SetPos(float x, float y) => _rect = Util.NormalizeRectWithTopLeft(this, x, y);

        internal void SetData(NodeData data, bool isNeed = true)
        {
            _isNeed = isNeed;
            _data = data;
            _image.sprite = data.Item.iconSprite;
            _countText.text = data.ItemCount.ToString("F2");
        }
    }
}