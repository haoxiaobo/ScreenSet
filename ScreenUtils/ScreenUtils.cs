using PInvoke;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;


namespace ScreenUtils
{
    public class ScreenUtil
    {
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern bool EnumDisplayDevices(
            string lpDevice, uint iDevNum, 
            ref DISPLAY_DEVICE lpDisplayDevice, 
            uint dwFlags);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern bool EnumDisplaySettings(
            string deviceName, int modeNum, ref DEVMODE devMode);


        // PInvoke declaration for ChangeDisplaySettings Win32 API
        [DllImport("user32.dll", CharSet=CharSet.Auto)]
        public static extern int ChangeDisplaySettings(
             ref DEVMODE lpDevMode,
             int dwFlags);


        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern DISP_CHANGE ChangeDisplaySettingsEx(
            string lpszDeviceName, ref DEVMODE lpDevMode, 
            IntPtr hwnd, 
            ChangeDisplaySettingsFlags dwflags, 
            IntPtr lParam);

        public static DISPLAY_DEVICE[] GetAllMonitors() {
            uint iDevNum = 0;
            List<DISPLAY_DEVICE> lst = new List<DISPLAY_DEVICE>();

            DISPLAY_DEVICE dd = DISPLAY_DEVICE.Create();
            while (EnumDisplayDevices(null, iDevNum++, ref dd, 0))
            {
                uint iMonitor = 0;
                DISPLAY_DEVICE mdd = DISPLAY_DEVICE.Create();
                while (EnumDisplayDevices(dd.DeviceName, iMonitor++, ref mdd, 0)) {
                    lst.Add(mdd);
                    mdd = DISPLAY_DEVICE.Create();
                }
                //lst.Add(dd);
                dd = DISPLAY_DEVICE.Create();
            }
            return lst.ToArray();
        }

        public static DISPLAY_DEVICE[] GetAllDisplayDevices()
        {
            uint iDevNum = 0;
            List<DISPLAY_DEVICE> lst = new List<DISPLAY_DEVICE>();

            DISPLAY_DEVICE dd = DISPLAY_DEVICE.Create();
            while (EnumDisplayDevices(null, iDevNum++, ref dd, 0))
            {
                lst.Add(dd);                   
                dd = DISPLAY_DEVICE.Create();
            }
            return lst.ToArray();
        }


        public static DEVMODE[] GetAllDisplaySettingModes()
        {
            List<DEVMODE> lst = new List<DEVMODE>();
            DEVMODE vDevMode = new DEVMODE();
            int i = 0;
            while (EnumDisplaySettings(null, i, ref vDevMode))
            {
                lst.Add(vDevMode);
                i++;
                vDevMode = new DEVMODE();
            }

            return lst.ToArray();
        }


        public static ScreenSettingSet GetAllDesktopDisplaySettings()
        {
            var devs = ScreenUtil.GetAllDisplayDevices()
                .Where(d=>d.StateFlags.HasFlag(DisplayDeviceStateFlags.AttachedToDesktop))
                .ToArray();
            
            List<ScreenSetInfo> lst = new List<ScreenSetInfo>(devs.Length);
            foreach (var dev in devs)
            {
                var dm = DEVMODE.Create();
                if (ScreenUtil.EnumDisplaySettings(
                    dev.DeviceName, ScreenUtil.ENUM_CURRENT_SETTINGS,
                    ref dm))
                {
                    lst.Add(new ScreenSetInfo() { 
                        DeviceName = dev.DeviceName, Device = dev, DevMode = dm }) ;
                }
            }
            ScreenSettingSet set = new ScreenSettingSet() {
                ScreenSetInfos = lst,
                Name = "设置集"
            };
            return set;
        }

        public static DISP_CHANGE ApplyScreenSettings(ScreenSettingSet settingSet)
        {
            // 这里有个问题：屏幕并不是一定会按需要的位置移动位置，可能与移动的顺序、位置的重合有关。
            // 解决方案：总是把第一个的位置定为0，0，然后其它屏幕的位置以第一个的相对位置重新排布。
            int iIndex = 0;
            int px0 = 0; int py0 = 0;
            foreach (var ssi in settingSet.ScreenSetInfos)
            {
                if (iIndex == 0) {
                    px0 = ssi.DevMode.dmPositionX;
                    py0 = ssi.DevMode.dmPositionY;                    
                }

                var dm = ssi.DevMode;
                dm.dmPositionX = ssi.DevMode.dmPositionX - px0;
                dm.dmPositionY = ssi.DevMode.dmPositionY - py0;
                ssi.DevMode = dm;
                iIndex++;
            }

            foreach (var ssi in settingSet.ScreenSetInfos)
            {
                DEVMODE dm = ssi.DevMode;
                var flag = ChangeDisplaySettingsFlags.CDS_RESET;
                
                if (ssi.Device.StateFlags.HasFlag(DisplayDeviceStateFlags.PrimaryDevice)) 
                {
                    flag |= ChangeDisplaySettingsFlags.CDS_SET_PRIMARY;
                }

                dm.dmFields = dm.dmFields | (int)FieldUseFlags.DM_POSITION;

                var r = ScreenUtils.ScreenUtil.ChangeDisplaySettingsEx(
                    ssi.Device.DeviceName, ref dm, IntPtr.Zero,
                    ChangeDisplaySettingsFlags.CDS_NORESET| ChangeDisplaySettingsFlags.CDS_UPDATEREGISTRY,
                    IntPtr.Zero
                );

                r = ScreenUtils.ScreenUtil.ChangeDisplaySettingsEx(
                    ssi.Device.DeviceName, ref dm, IntPtr.Zero,
                    flag,
                    IntPtr.Zero
                );

                if (r != DISP_CHANGE.Successful)
                {
                    return r;
                }
            }
            return DISP_CHANGE.Successful;
        }


        // constants
        public const int ENUM_CURRENT_SETTINGS = -1;
        public const int ENUM_REGISTRY_SETTINGS = -2;
        public const int DISP_CHANGE_SUCCESSFUL = 0;
        // add more constants as needed …
    }

    public enum DISP_CHANGE : int
    {
        Successful = 0,
        Restart = 1,
        Failed = -1,
        BadMode = -2,
        NotUpdated = -3,
        BadFlags = -4,
        BadParam = -5,
        BadDualView = -6
    }
    [Flags()]
    public enum ChangeDisplaySettingsFlags : uint
    {
        CDS_NONE = 0,
        CDS_UPDATEREGISTRY = 0x00000001,
        CDS_TEST = 0x00000002,
        CDS_FULLSCREEN = 0x00000004,
        CDS_GLOBAL = 0x00000008,
        CDS_SET_PRIMARY = 0x00000010,
        CDS_VIDEOPARAMETERS = 0x00000020,
        CDS_ENABLE_UNSAFE_MODES = 0x00000100,
        CDS_DISABLE_UNSAFE_MODES = 0x00000200,
        CDS_RESET = 0x40000000,
        CDS_RESET_EX = 0x20000000,
        CDS_NORESET = 0x10000000
    }

    [Flags()]
    public enum DisplayDeviceStateFlags : int
    {
        /// <summary>The device is part of the desktop.</summary>
        AttachedToDesktop = 0x1,
        MultiDriver = 0x2,
        /// <summary>The device is part of the desktop.</summary>
        PrimaryDevice = 0x4,
        /// <summary>Represents a pseudo device used to mirror application drawing for remoting or other purposes.</summary>
        MirroringDriver = 0x8,
        /// <summary>The device is VGA compatible.</summary>
        VGACompatible = 0x10,
        /// <summary>The device is removable; it cannot be the primary display.</summary>
        Removable = 0x20,
        /// <summary>The device has more display modes than its output devices support.</summary>
        ModesPruned = 0x8000000,
        Remote = 0x4000000,
        Disconnect = 0x2000000
    }

    [Serializable]
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    public struct DISPLAY_DEVICE
    {
        [MarshalAs(UnmanagedType.U4)]
        public int cb;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string DeviceName;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
        public string DeviceString;
        [MarshalAs(UnmanagedType.U4)]
        public DisplayDeviceStateFlags StateFlags;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
        public string DeviceID;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
        public string DeviceKey;

        public static DISPLAY_DEVICE Create()
        {
            DISPLAY_DEVICE result = default(DISPLAY_DEVICE);
            result.cb = (int)Marshal.SizeOf(result);
            return result;            
        }
    }

    [Flags]
    public enum FieldUseFlags : uint
    {
        //
        // 摘要:
        //     Not set.
        NONE = 0x0u,
        DM_ORIENTATION = 0x1u,
        DM_PAPERSIZE = 0x2u,
        DM_PAPERLENGTH = 0x4u,
        DM_PAPERWIDTH = 0x8u,
        DM_SCALE = 0x10u,
        DM_POSITION = 0x20u,
        DM_NUP = 0x40u,
        DM_DISPLAYORIENTATION = 0x80u,
        DM_COPIES = 0x100u,
        DM_DEFAULTSOURCE = 0x200u,
        DM_PRINTQUALITY = 0x400u,
        DM_COLOR = 0x800u,
        DM_DUPLEX = 0x1000u,
        DM_YRESOLUTION = 0x2000u,
        DM_TTOPTION = 0x4000u,
        DM_COLLATE = 0x8000u,
        DM_FORMNAME = 0x10000u,
        DM_LOGPIXELS = 0x20000u,
        DM_BITSPERPEL = 0x40000u,
        DM_PELSWIDTH = 0x80000u,
        DM_PELSHEIGHT = 0x100000u,
        DM_DISPLAYFLAGS = 0x200000u,
        DM_DISPLAYFREQUENCY = 0x400000u,
        DM_ICMMETHOD = 0x800000u,
        DM_ICMINTENT = 0x1000000u,
        DM_MEDIATYPE = 0x2000000u,
        DM_DITHERTYPE = 0x4000000u,
        DM_PANNINGWIDTH = 0x8000000u,
        DM_PANNINGHEIGHT = 0x10000000u,
        DM_DISPLAYFIXEDOUTPUT = 0x20000000u
    }

    [Serializable]
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public struct DEVMODE
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string dmDeviceName;

        public short dmSpecVersion;
        public short dmDriverVersion;
        public short dmSize;
        public short dmDriverExtra;
        public int dmFields;
        public int dmPositionX;
        public int dmPositionY;
        public int dmDisplayOrientation;
        public int dmDisplayFixedOutput;
        public short dmColor;
        public short dmDuplex;
        public short dmYResolution;
        public short dmTTOption;
        public short dmCollate;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string dmFormName;

        public short dmLogPixels;
        public short dmBitsPerPel;
        public int dmPelsWidth;
        public int dmPelsHeight;
        public int dmDisplayFlags;
        public int dmDisplayFrequency;
        public int dmICMMethod;
        public int dmICMIntent;
        public int dmMediaType;
        public int dmDitherType;
        public int dmReserved1;
        public int dmReserved2;
        public int dmPanningWidth;
        public int dmPanningHeight;

        public static DEVMODE Create()
        {
            DEVMODE result = default(DEVMODE);
            //result.cb = (uint)sizeof(DEVMODE);
            return result;
        }
    };
}
