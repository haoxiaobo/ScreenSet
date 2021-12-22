using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Serialization;
using HotKeyUtils;
using Microsoft.Win32;
//using PInvoke;
using ScreenUtils;

namespace SetScr
{
    public partial class Form1 : Form
    {

        AllSets setting = null;
        string sSettingFile = null;
        int iSelcetedIndex = 0;
        bool bRealClose = false;
        public Form1()
        {
            LoadSet();
            InitializeComponent();
        }

        private void LoadSet()
        {
            sSettingFile = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\SetScr\\Settings.xml";
            if (File.Exists(sSettingFile))
            {
                XmlSerializer ser = new XmlSerializer(typeof(AllSets));
                using (var sr = new StreamReader(sSettingFile, Encoding.UTF8))
                {
                    this.setting = (AllSets)ser.Deserialize(sr);
                }                
            }
            else
            {
                this.setting = new AllSets();
            }
        }

        private const int WM_HOTKEY = 0x312; //窗口消息：热键
        private const int WM_CREATE = 0x1; //窗口消息：创建
        private const int WM_DESTROY = 0x2; //窗口消息：销毁

        private const int HotKeyIDResetALL = 1000; //热键ID（自定义）

        private const int HotKeyIDSetMode1 = 1001; //热键ID（自定义）
        private const int HotKeyIDSetMode2 = 1002; //热键ID（自定义）
        private const int HotKeyIDSetMode3 = 1003; //热键ID（自定义）
        private const int HotKeyIDSetMode4 = 1004; //热键ID（自定义）
        private const int HotKeyIDSetMode5 = 1005; //热键ID（自定义）

        private const int HotKeyIDLoopMode = 1100; //热键ID（自定义）
        private const int HotKeyIDShowMain = 2000; //热键ID（自定义）



        protected override void WndProc(ref Message msg)
        {
            base.WndProc(ref msg);
            switch (msg.Msg)
            {
                case WM_HOTKEY: //窗口消息：热键
                    int tmpWParam = msg.WParam.ToInt32();
                    switch (tmpWParam)
                    {
                        case HotKeyIDLoopMode:
                            // 循环
                            this.SwitchScreenSetting();
                            break;
                        case HotKeyIDShowMain:
                            this.ShowInTaskbar = true;
                            this.Show();                            
                            break;
                        case HotKeyIDResetALL:
                            this.ResetAll();
                            break;
                        case HotKeyIDSetMode1:
                        case HotKeyIDSetMode2:
                        case HotKeyIDSetMode3:
                        case HotKeyIDSetMode4:
                        case HotKeyIDSetMode5:
                            this.SetScreemSetting(tmpWParam - HotKeyIDSetMode1);
                            break;

                    }
                    break;

                case WM_CREATE: //窗口消息：创建
                    HotKeyUtil.RegHotKey(this.Handle, HotKeyIDLoopMode,
                        KeyModifiers.Ctrl | KeyModifiers.Alt | KeyModifiers.WindowsKey,
                        Keys.Oemtilde);

                    HotKeyUtil.RegHotKey(this.Handle, HotKeyIDResetALL,
                       KeyModifiers.Ctrl | KeyModifiers.Alt | KeyModifiers.WindowsKey,
                       Keys.D0);

                    HotKeyUtil.RegHotKey(this.Handle, HotKeyIDSetMode1,
                        KeyModifiers.Ctrl | KeyModifiers.Alt | KeyModifiers.WindowsKey,
                        Keys.D1);
                    HotKeyUtil.RegHotKey(this.Handle, HotKeyIDSetMode2,
                        KeyModifiers.Ctrl | KeyModifiers.Alt | KeyModifiers.WindowsKey,
                        Keys.D2);
                    HotKeyUtil.RegHotKey(this.Handle, HotKeyIDSetMode3,
                        KeyModifiers.Ctrl | KeyModifiers.Alt | KeyModifiers.WindowsKey,
                        Keys.D3);
                    HotKeyUtil.RegHotKey(this.Handle, HotKeyIDSetMode4,
                        KeyModifiers.Ctrl | KeyModifiers.Alt | KeyModifiers.WindowsKey,
                        Keys.D4);
                    HotKeyUtil.RegHotKey(this.Handle, HotKeyIDSetMode5,
                        KeyModifiers.Ctrl | KeyModifiers.Alt | KeyModifiers.WindowsKey,
                        Keys.D5);

                    HotKeyUtil.RegHotKey(this.Handle, HotKeyIDShowMain,
                        KeyModifiers.Ctrl | KeyModifiers.Alt | KeyModifiers.WindowsKey,
                        Keys.Up);

                    break;
                case WM_DESTROY: //窗口消息：销毁
                    HotKeyUtil.UnRegHotKey(this.Handle, HotKeyIDLoopMode); //销毁热键
                    break;
                default:
                    break;
            }
        }

        private void SetScreemSetting(int iSettingIndex)
        {
            if (this.setting == null || this.setting.SetItems.Count <= iSettingIndex)
            {
                return;
            }

            var si = this.setting.SetItems[iSettingIndex];
            ScreenUtil.ApplyScreenSettings(si.SettingSet);

        }

        private void ResetAll()
        {
            var all = ScreenUtil.GetAllDesktopDisplaySettings();
            int iOffsetX = 0;
            foreach (var si in all.ScreenSetInfos)
            {
                DEVMODE dm = si.DevMode;
                if (dm.dmPelsHeight > dm.dmPelsWidth)
                {
                    // swap width and height
                    (dm.dmPelsHeight, dm.dmPelsWidth) = (dm.dmPelsWidth, dm.dmPelsHeight);
                }
                dm.dmPositionY = 0;
                dm.dmPositionX = iOffsetX;
                dm.dmDisplayOrientation = (int)PInvoke.DEVMODE.DisplayOrientationOptions.DMDO_DEFAULT;

                dm.dmFields = dm.dmFields| (int)FieldUseFlags.DM_POSITION;
                iOffsetX += dm.dmPelsWidth;
                var flag = ChangeDisplaySettingsFlags.CDS_RESET;

                if (si.Device.StateFlags.HasFlag(DisplayDeviceStateFlags.PrimaryDevice))
                {
                    flag |= ChangeDisplaySettingsFlags.CDS_SET_PRIMARY;
                }

                if (DISP_CHANGE.Successful != ScreenUtil.ChangeDisplaySettingsEx(si.DeviceName,
                    ref dm, IntPtr.Zero, flag, IntPtr.Zero))
                {
                    break;
                }
            }
        }

        private void SwitchScreenSetting()
        {
            if (this.setting.SetItems.Count == 0)
                return;

            var si = this.setting.SetItems[this.iSelcetedIndex];
            var r = ScreenUtils.ScreenUtil.ApplyScreenSettings(si.SettingSet);
            iSelcetedIndex++;
            if (iSelcetedIndex >= this.setting.SetItems.Count)
            {
                iSelcetedIndex = 0;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            
            return;
            /*
            // initialize the DEVMODE structure
            var dm = DEVMODE.Create();

            if (ScreenUtil.EnumDisplaySettings(
                null,
                ScreenUtil.ENUM_CURRENT_SETTINGS,
                ref dm))
            {
                // swap width and height
                int temp = dm.dmPelsHeight;
                dm.dmPelsHeight = dm.dmPelsWidth;
                dm.dmPelsWidth = temp;

                // determine new orientation
                switch (dm.dmDisplayOrientation)
                {
                    case (int)PInvoke.DEVMODE.DisplayOrientationOptions.DMDO_DEFAULT:
                        dm.dmDisplayOrientation = (int)PInvoke.DEVMODE.DisplayOrientationOptions.DMDO_270;
                        break;
                    case (int)PInvoke.DEVMODE.DisplayOrientationOptions.DMDO_270:
                        dm.dmDisplayOrientation = (int)PInvoke.DEVMODE.DisplayOrientationOptions.DMDO_180;
                        break;
                    case (int)PInvoke.DEVMODE.DisplayOrientationOptions.DMDO_180:
                        dm.dmDisplayOrientation = (int)PInvoke.DEVMODE.DisplayOrientationOptions.DMDO_90;
                        break;
                    case (int)PInvoke.DEVMODE.DisplayOrientationOptions.DMDO_90:
                        dm.dmDisplayOrientation = (int)PInvoke.DEVMODE.DisplayOrientationOptions.DMDO_DEFAULT;
                        break;
                    default:
                        // unknown orientation value
                        // add exception handling here
                        break;
                }

                int iRet = ScreenUtils.ScreenUtil.ChangeDisplaySettings(ref dm, 0);
                if (ScreenUtil.DISP_CHANGE_SUCCESSFUL != iRet)
                {
                    // add exception handling here
                }
            }
            */
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.UpdateUI();
            if (SetScr.Properties.Settings.Default.AutoHide)
            {
                this.BeginInvoke(new Action(() =>
                {
                    this.Hide();
                }));
            }
        }

        private void SaveSettings()
        {
            var dirName = Path.GetDirectoryName(sSettingFile);
            if (!Directory.Exists(dirName))
            {
                Directory.CreateDirectory(dirName);
            }

            XmlSerializer ser = new XmlSerializer(typeof(AllSets));

            using (var sw = new StreamWriter(sSettingFile, false, Encoding.UTF8))
            {
                var xmlWriter = XmlWriter.Create(sw, new XmlWriterSettings
                {
                    Indent = true
                });
                ser.Serialize(sw, this.setting);
            }
        }

        private void btnSetCurSet_Click(object sender, EventArgs e)
        {
            var set = ScreenUtil.GetAllDesktopDisplaySettings();
            frmNewSet frmNewSet = new frmNewSet();
            frmNewSet.txtSetName.Text = set.Name;
            if (DialogResult.OK != frmNewSet.ShowDialog())
            {
                return;
            }
            set.Name = frmNewSet.txtSetName.Text.Trim();

            SetItem si = new SetItem() {  
                Key = Keys.F1, 
                Modifiers = 
                    HotKeyUtils.KeyModifiers.Ctrl | 
                    HotKeyUtils.KeyModifiers.Alt| 
                    HotKeyUtils.KeyModifiers.WindowsKey,
                SettingSet = set
            };
            this.setting.SetItems.Add(si);
            this.SaveSettings();

            this.UpdateUI();
        }

        private void UpdateUI()
        {
            this.lstSets.Items.Clear();
            foreach (var set in this.setting.SetItems)
            {
                var lvi = this.lstSets.Items.Add(set.SettingSet.Name);
                lvi.SubItems.Add(set.ToBreifString());
                lvi.Tag = set;
            }

            RegistryKey registryKey = Registry.CurrentUser.OpenSubKey
                ("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
            string startapp = registryKey.GetValue("ScreenSet") as string;
            
            this.chkAutoStart.Checked = (Application.ExecutablePath == startapp);
            this.chkHideWindow.Checked = SetScr.Properties.Settings.Default.AutoHide;
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            this.lstSets.Items.RemoveAt(this.lstSets.SelectedIndices[0]);

            this.UpdateSetByUI();
            this.SaveSettings();
        }

        private void UpdateSetByUI()
        {
            this.setting.SetItems.Clear();
            foreach (ListViewItem li in this.lstSets.Items)
            {
                this.setting.SetItems.Add((SetItem)li.Tag);
            }
        }

        private void btnApplySet_Click(object sender, EventArgs e)
        {
            SetItem si = (SetItem)this.lstSets.SelectedItems[0].Tag;
            var r = ScreenUtils.ScreenUtil.ApplyScreenSettings(si.SettingSet);
            if (r != DISP_CHANGE.Successful)
            {
                MessageBox.Show("Error:"+r.ToString());
            }
        }

        private void notifyIcon1_DoubleClick(object sender, EventArgs e)
        {
            this.Show();
            this.ShowInTaskbar = true;
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!this.bRealClose)
            {
                e.Cancel = true;
                this.ShowInTaskbar = false;
                this.Hide();                
            }
        }

        private void miShow_Click(object sender, EventArgs e)
        {
            this.Show();
            this.ShowInTaskbar = true;
        }

        private void miExit_Click(object sender, EventArgs e)
        {
            this.bRealClose = true;
            this.Close();
        }

        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            
            g.Clear(Color.White);

            g.TranslateTransform(100, 100);
            g.ScaleTransform(0.08F, 0.08F);

            
            if (this.lstSets.SelectedItems.Count == 0)
                return;
            var setitem = this.lstSets.SelectedItems[0].Tag as SetItem;
            foreach (var si in setitem.SettingSet.ScreenSetInfos)
            {

                g.FillRectangle(si.Device.StateFlags.HasFlag(DisplayDeviceStateFlags.PrimaryDevice)? 
                    Brushes.LightBlue:Brushes.LightPink, 
                    si.DevMode.dmPositionX, si.DevMode.dmPositionY, 
                    si.DevMode.dmPelsWidth, si.DevMode.dmPelsHeight);

                g.DrawRectangle(Pens.Black,
                    si.DevMode.dmPositionX, si.DevMode.dmPositionY,
                    si.DevMode.dmPelsWidth, si.DevMode.dmPelsHeight);

                StringFormat sf = new StringFormat() {};
                var font = new Font(FontFamily.GenericSerif, 100f);
                g.DrawString(si.DeviceName,
                    font, Brushes.Black,
                    si.DevMode.dmPositionX + 100, si.DevMode.dmPositionY + 100,
                    sf);; ;
            }

            g.DrawLine(new Pen(Color.Red, 3f), -100, 0, 100, 0);
            g.DrawLine(new Pen(Color.Red, 3f), 0, 100, 0, -100);
        }

        private void lstSets_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.pictureBox1.Invalidate();
        }

        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            this.Show();
            this.ShowInTaskbar = true;
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("MAILTO:郝晓波<xiaobohao@qq.com>?subject=关于SrceenSet的问题");
        }       

        private void chkAutoStart_CheckedChanged(object sender, EventArgs e)
        {
            RegistryKey registryKey = Registry.CurrentUser.OpenSubKey
                    ("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);

            if (this.chkAutoStart.Checked)
            {
                registryKey.SetValue("ScreenSet", Application.ExecutablePath, RegistryValueKind.String);
            }
            else
            {
                registryKey.DeleteValue("ScreenSet", false);
            }
        }

        private void chkHideWindow_CheckedChanged(object sender, EventArgs e)
        {
            SetScr.Properties.Settings.Default.AutoHide = this.chkHideWindow.Checked;
            SetScr.Properties.Settings.Default.Save();
        }
    }
}
