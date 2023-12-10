using System.Collections.Generic;
using ProjectGenesis.Patches.Logic;

namespace ProjectGenesis.Patches.UI.QTools.MyComboBox
{
    public class ProliferatorComboBox : SignalComboBox
    {
        internal EProliferatorStrategy Strategy => (EProliferatorStrategy)selectIndex;
        
        public void Init() => Init(new List<int> { 509, 1143, 1143 }, new List<string> { "不使用增产剂", "增产", "加速" });
    }
}
