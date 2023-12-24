using System;
using System.Collections.Generic;
using System.Linq;
using Utils_ERecipeType = ProjectGenesis.Utils.ERecipeType;

namespace ProjectGenesis.Patches.UI.QTools.MyComboBox
{
    public class ItemComboBox : MyComboBox
    {
        private List<ItemProto> _items;
        private Utils_ERecipeType _type;

        public event Action<(Utils_ERecipeType, ItemProto)> OnItemChange;

        public void Init(Utils_ERecipeType type, List<ItemProto> items, int itemIndex)
        {
            OnItemChange = null;

            _type = type;
            _items = items;

            Init(_items.Select(i => i.name).ToList(), itemIndex, 509);

            OnItemIndexChange();
        }

        public override void OnItemIndexChange()
        {
            ItemProto itemProto = _items[selectIndex];

            iconImg.sprite = itemProto.iconSprite;

            OnItemChange?.Invoke((_type, itemProto));
        }
    }
}
