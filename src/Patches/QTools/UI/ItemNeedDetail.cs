using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectGenesis.Patches
{
    public class ItemNeedDetail : MonoBehaviour
    {
        private UIButton _button;
        private Text _countText;
        private NodeData _data;

        private Image _image;
        private UIButton _itemButton;
        private bool _isNeed;
        private RectTransform _rect;

        internal static ItemNeedDetail CreateItemNeedDetail(float x, float y, RectTransform parent, bool needButton = true)
        {
            var src = new GameObject { name = "my-ItemCounter", };
            src.AddComponent<RectTransform>();
            ItemNeedDetail cb = src.AddComponent<ItemNeedDetail>();
            cb._rect = UIUtil.NormalizeRectWithTopLeft(cb, x, y, parent);

            Destroy(src.GetComponentInChildren<Text>());

            UIUtil.CreateItemTipButton(out cb._itemButton, out cb._image);
            UIUtil.NormalizeRectWithTopLeft(cb._itemButton, 0, 0, cb._rect);
            UIUtil.NormalizeRectWithTopLeft(cb._image, 0, 0, cb._rect);
            UIUtil.RemovePersistentCalls(cb._itemButton.gameObject);

            UIUtil.NormalizeRectWithTopLeft(UIUtil.CreateText("\u2573", 18), 50, 11, cb._rect);
            cb._countText = UIUtil.CreateText("", 18);
            UIUtil.NormalizeRectWithTopLeft(cb._countText, 70, 10, cb._rect);

            if (needButton)
            {
                cb._button = UIUtil.MakeSmallTextButton("\u2573", 20, 20);
                UIUtil.NormalizeRectWithTopLeft(cb._button, 125, 10, cb._rect);

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
            if (_isNeed) { _data.RemoveNeed(); }
            else
            {
                _data.Options.AsRaw = false;
                _data.RefreshNeeds();
            }
        }

        internal void SetPos(float x, float y) => _rect = UIUtil.NormalizeRectWithTopLeft(this, x, y);

        internal void SetData(NodeData data, bool isNeed = false, string format = "F2")
        {
            _isNeed = isNeed;
            _data = data;
            _image.sprite = data.Item.iconSprite;
            _countText.text = data.ItemCount.ToString(format);

            _itemButton.tips.itemId = data.Item.ID;
            _itemButton.tips.type = UIButton.ItemTipType.IgnoreIncPoint;
        }
    }
}
