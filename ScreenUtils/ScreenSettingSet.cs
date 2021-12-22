using ScreenUtils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScreenUtils
{
    [Serializable]
    public class ScreenSettingSet
    {
        public string Name { get; set; }
        public List<ScreenSetInfo> ScreenSetInfos { get; set; } = new List<ScreenSetInfo>();

        public string ToBreifString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("{0}屏幕", this.ScreenSetInfos.Count);
            var hcount = ScreenSetInfos.Where(
                ssi => ssi.DevMode.dmDisplayOrientation == 
                (int)PInvoke.DEVMODE.DisplayOrientationOptions.DMDO_DEFAULT)
                .Count();

            if (hcount > 0)
            {
                sb.AppendFormat(", {0}横", hcount);
            }

            var vcount = this.ScreenSetInfos.Count - hcount;
            if (vcount > 0)
            {
                sb.AppendFormat(", {0}纵", hcount);
            }

            return sb.ToString();
        }
    }

    [Serializable]

    public class ScreenSetInfo
    {
        public string DeviceName { get; set; }
        public DISPLAY_DEVICE Device { get; set; }
        public DEVMODE DevMode { get; set; }
    }
}
