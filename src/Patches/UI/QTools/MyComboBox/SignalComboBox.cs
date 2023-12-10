using System.Collections.Generic;
using System.Linq;
using ProjectGenesis.Utils;

namespace ProjectGenesis.Patches.UI.QTools.MyComboBox
{
    public class SignalComboBox : MyComboBox
    {
        private List<int> _items;

        public void Init(List<int> items, string defaultString = "", int defaultSprite = 509)
        {
            _items = items;

            List<string> list = items.Select(i => i > 0 ? LDB.items.Select(i).name : defaultString.TranslateFromJson()).ToList();

            base.Init(list, defaultSprite);
        }

        public void Init(List<int> items, List<string> overrideString, int defaultSprite = 509)
        {
            _items = items;

            var list = new List<string>(items.Count);

            for (int i = 0; i < items.Count; i++)
            {
                string s = overrideString[i];

                list.Add(s == null ? LDB.items.Select(i).name : s.TranslateFromJson());
            }

            base.Init(list, defaultSprite);
        }

        public new void Init(List<string> overrideString, int defaultSprite = 509)
            => base.Init(overrideString.Select(i => i.TranslateFromJson()).ToList(), defaultSprite);

        public override void OnItemIndexChange() => iconImg.sprite = LDB.signals.IconSprite(_items != null ? _items[selectIndex] : DefaultSprite);
    }
}
