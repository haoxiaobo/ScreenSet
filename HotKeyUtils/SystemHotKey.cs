using System;
using System.Text;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace HotKeyUtils
{
    public class HotKeyUtil
    {
        /// <summary>
        /// 如果函数执行成功，返回值不为0。
        /// 如果函数执行失败，返回值为0。要得到扩展错误信息，调用GetLastError。
        /// </summary>
        /// <param name="hWnd">要定义热键的窗口的句柄</param>
        /// <param name="id">定义热键ID（不能与其它ID重复）</param>
        /// <param name="fsModifiers">标识热键是否在按Alt、Ctrl、Shift、Windows等键时才会生效</param>
        /// <param name="vk">定义热键的内容</param>
        /// <returns></returns>
        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool RegisterHotKey(IntPtr hWnd, int id, KeyModifiers fsModifiers, Keys vk);

        /// <summary>
        /// 注销热键
        /// </summary>
        /// <param name="hWnd">要取消热键的窗口的句柄</param>
        /// <param name="id">要取消热键的ID</param>
        /// <returns></returns>
        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool UnregisterHotKey(IntPtr hWnd, int id);
        

        /// <summary>
        /// 注册热键
        /// </summary>
        /// <param name="hwnd">窗口句柄</param>
        /// <param name="hotKey_id">热键ID</param>
        /// <param name="keyModifiers">组合键</param>
        /// <param name="key">热键</param>
        public static int RegHotKey(IntPtr hwnd, int hotKeyId, KeyModifiers keyModifiers, Keys key)
        {
            if (!RegisterHotKey(hwnd, hotKeyId, keyModifiers, key))
            {
                int errorCode = Marshal.GetLastWin32Error();
                return errorCode;
            }
            return 0;
        }

        /// <summary>
        /// 注销热键
        /// </summary>
        /// <param name="hwnd">窗口句柄</param>
        /// <param name="hotKey_id">热键ID</param>
        public static void UnRegHotKey(IntPtr hwnd, int hotKeyId)
        {
            //注销指定的热键
            UnregisterHotKey(hwnd, hotKeyId);
        }
    }

    /// <summary>
    /// 辅助键名称。
    /// Alt, Ctrl, Shift, WindowsKey
    /// </summary>
    [Flags()]
    public enum KeyModifiers
    {
        None = 0,
        Alt = 1,
        Ctrl = 2,
        Shift = 4,
        WindowsKey = 8
    }
}

/* 示例代码
 * 下面的代码放在form里。
private const int WM_HOTKEY = 0x312; //窗口消息：热键
private const int WM_CREATE = 0x1; //窗口消息：创建
private const int WM_DESTROY = 0x2; //窗口消息：销毁

private const int HotKeyID = 1; //热键ID（自定义）

protected override void WndProc(ref Message msg)
{
    base.WndProc(ref msg);
    switch (msg.Msg)
    {
        case WM_HOTKEY: //窗口消息：热键
            int tmpWParam = msg.WParam.ToInt32();
            if (tmpWParam == HotKeyID)
            {
                System.Windows.Forms.SendKeys.Send("^v");
            }
            break;
        case WM_CREATE: //窗口消息：创建
            HotKeyUtil.RegHotKey(this.Handle, HotKeyID, 
                HotKeyUtil.KeyModifiers.None, Keys.F1);
            break;
        case WM_DESTROY: //窗口消息：销毁
            HotKeyUtil.UnRegHotKey(this.Handle, HotKeyID); //销毁热键
            break;
        default:
            break;
    }
}
 */
