using ScreenUtils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SetScr
{
    [Serializable]
    public  class AllSets
    {
        public List<SetItem> SetItems { get; set; } = new List<SetItem>();

    }

    [Serializable]
    public class SetItem
    {
        public ScreenSettingSet SettingSet { get; set; }

        public HotKeyUtils.KeyModifiers Modifiers { get; set; }

        public Keys Key { get; set; }

        internal string ToBreifString()
        {
            return SettingSet.ToBreifString();
        }
    }
}
