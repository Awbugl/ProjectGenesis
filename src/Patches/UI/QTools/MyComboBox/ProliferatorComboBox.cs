using System.Collections.Generic;
using ProjectGenesis.Patches.Logic.QTools;

namespace ProjectGenesis.Patches.UI.QTools.MyComboBox
{
    public class ProliferatorComboBox : SignalComboBox
    {
        internal EProliferatorStrategy Strategy =>
            Items.Count == 3 ? (EProliferatorStrategy)selectIndex : (EProliferatorStrategy)(selectIndex * 2);

        public void Init(int strategy) =>
            Init(new List<int>
            {
                509,
                1143,
                1143,
            }, new List<string>
            {
                "不使用增产剂",
                "增产",
                "加速",
            }, strategy);

        public void InitNoProductive(int strategy) =>
            Init(new List<int>
            {
                509, 1143,
            }, new List<string>
            {
                "不使用增产剂", "加速",
            }, strategy);

        internal void SetStrategySlience(EProliferatorStrategy strategy)
        {
            UIComboBox uiComboBox = comboBox;
            uiComboBox._itemIndex = Items.Count == 3 ? (int)strategy : (int)strategy / 2;
            uiComboBox.m_Input.text = uiComboBox._itemIndex >= 0 ? uiComboBox.Items[uiComboBox._itemIndex] : "";
            selectIndex = uiComboBox._itemIndex;
            OnItemIndexChange();
        }
    }
}
