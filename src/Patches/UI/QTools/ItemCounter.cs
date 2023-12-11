using ProjectGenesis.Patches.Logic;
using ProjectGenesis.Patches.UI.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectGenesis.Patches.UI.QTools
{
    public class ItemCounter : MonoBehaviour
    {
        private RectTransform _rect;

        private Image _image;

        private Text _countText;

        internal static ItemCounter CreateItemCounter(float x, float y, RectTransform parent)
        {
            GameObject src = Instantiate(Configs.builtin.uiItemTipPrefab.recipeEntry.transform.GetChild(1).gameObject);
            var cb = src.AddComponent<ItemCounter>();

            cb._rect = Util.NormalizeRectWithTopLeft(cb, x, y, parent);
            src.name = "my-ItemCounter";

            cb._image = src.GetComponent<Image>();
            cb._countText = src.GetComponentInChildren<Text>();
            
            return cb;
        }

        internal void SetPos(float x, float y) => _rect = Util.NormalizeRectWithTopLeft(this, x, y);

        internal void SetData(NodeData data)
        {
            _image.sprite = data.Item.iconSprite;
            _countText.text = data.ItemCount.ToString("F2");
        }
    }
}
