using System.Collections.Generic;
using ProjectGenesis.Utils;

namespace ProjectGenesis.Patches.UI.QTools.MyComboBox
{
    public class SignalComboBox : MyComboBox
    {
        protected List<int> Items;

        public void Init(List<int> items, List<string> overrideString, int itemIndex, int defaultSprite = 509)
        {
            Items = items;

            var list = new List<string>(items.Count);

            for (var i = 0; i < items.Count; i++)
            {
                string s = overrideString[i];

                list.Add(s == null ? LDB.items.Select(i).name : s.TranslateFromJson());
            }

            base.Init(list, itemIndex, defaultSprite);
        }

        public override void OnItemIndexChange() =>
            iconImg.sprite = LDB.signals.IconSprite(Items != null ? Items[selectIndex] : DefaultSprite);
    }
}
