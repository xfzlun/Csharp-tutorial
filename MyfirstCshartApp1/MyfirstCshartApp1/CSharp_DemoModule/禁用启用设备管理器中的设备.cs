
//使用到的Win32Api
[Flags]
public enum DIGCF : uint
{
    DEFAULT = 0x00000001,
    PRESENT = 0x00000002,
    ALLCLASSES = 0x00000004,
    PROFILE = 0x00000008,
    DEVICEINTERFACE = 0x00000010
}

[StructLayout(LayoutKind.Sequential)]
public struct SP_DEVINFO_DATA
{
    public UInt32 cbSize;
    public Guid ClassGuid;
    public UInt32 DevInst;
    public IntPtr Reserved;
}

[StructLayout(LayoutKind.Sequential)]
public struct SP_DEVINFO_DATA
{
    public UInt32 cbSize;
    public Guid ClassGuid;
    public UInt32 DevInst;
    public IntPtr Reserved;
}
[StructLayout(LayoutKind.Sequential)]
public struct SP_CLASSINSTALL_HEADER
{
    public UInt32 cbSize;
    public DIF InstallFunction;
}
[StructLayout(LayoutKind.Sequential)]
public struct SP_PROPCHANGE_PARAMS
{
    public SP_CLASSINSTALL_HEADER ClassInstallHeader;
    public DICS StateChange;
    public DICS_FLAG Scope;
    public UInt32 HwProfile;
}
public enum DIF : uint
{
    SELECTDEVICE = 0x00000001,
    INSTALLDEVICE = 0x00000002,
    ASSIGNRESOURCES = 0x00000003,
    PROPERTIES = 0x00000004,
    REMOVE = 0x00000005,
    FIRSTTIMESETUP = 0x00000006,
    FOUNDDEVICE = 0x00000007,
    SELECTCLASSDRIVERS = 0x00000008,
    VALIDATECLASSDRIVERS = 0x00000009,
    INSTALLCLASSDRIVERS = 0x0000000A,
    CALCDISKSPACE = 0x0000000B,
    DESTROYPRIVATEDATA = 0x0000000C,
    VALIDATEDRIVER = 0x0000000D,
    DETECT = 0x0000000F,
    INSTALLWIZARD = 0x00000010,
    DESTROYWIZARDDATA = 0x00000011,
    PROPERTYCHANGE = 0x00000012,
    ENABLECLASS = 0x00000013,
    DETECTVERIFY = 0x00000014,
    INSTALLDEVICEFILES = 0x00000015,
    UNREMOVE = 0x00000016,
    SELECTBESTCOMPATDRV = 0x00000017,
    ALLOW_INSTALL = 0x00000018,
    REGISTERDEVICE = 0x00000019,
    NEWDEVICEWIZARD_PRESELECT = 0x0000001A,
    NEWDEVICEWIZARD_SELECT = 0x0000001B,
    NEWDEVICEWIZARD_PREANALYZE = 0x0000001C,
    NEWDEVICEWIZARD_POSTANALYZE = 0x0000001D,
    NEWDEVICEWIZARD_FINISHINSTALL = 0x0000001E,
    UNUSED1 = 0x0000001F,
    INSTALLINTERFACES = 0x00000020,
    DETECTCANCEL = 0x00000021,
    REGISTER_COINSTALLERS = 0x00000022,
    ADDPROPERTYPAGE_ADVANCED = 0x00000023,
    ADDPROPERTYPAGE_BASIC = 0x00000024,
    RESERVED1 = 0x00000025,
    TROUBLESHOOTER = 0x00000026,
    POWERMESSAGEWAKE = 0x00000027,
    ADDREMOTEPROPERTYPAGE_ADVANCED = 0x00000028,
    UPDATEDRIVER_UI = 0x00000029,
    FINISHINSTALL_ACTION = 0x0000002A,
    RESERVED2 = 0x00000030,
}
public enum DICS : uint
{
    ENABLE = 0x00000001,
    DISABLE = 0x00000002,
    PROPCHANGE = 0x00000003,
    START = 0x00000004,
    STOP = 0x00000005,
}
[Flags]
public enum DICS_FLAG : uint
{
    GLOBAL = 0x00000001,
    CONFIGSPECIFIC = 0x00000002,
    CONFIGGENERAL = 0x00000004,
}

[DllImport("setupapi.dll", SetLastError = true)]
public static extern bool SetupDiDestroyDeviceInfoList(IntPtr handle);
[DllImport("setupapi.dll", SetLastError = true)]
public static extern IntPtr SetupDiGetClassDevsW([In] ref Guid ClassGuid, [MarshalAs(UnmanagedType.LPWStr)] string Enumerator, IntPtr parent, DIGCF flags);
[DllImport("setupapi.dll", SetLastError = true)]
public static extern bool SetupDiEnumDeviceInfo(IntPtr deviceInfoSet, UInt32 memberIndex, [Out] out SP_DEVINFO_DATA deviceInfoData);
[DllImport("setupapi.dll", SetLastError = true)]
public static extern bool SetupDiSetClassInstallParams(IntPtr deviceInfoSet, [In] ref SP_DEVINFO_DATA deviceInfoData, [In] ref SP_PROPCHANGE_PARAMS classInstallParams, UInt32 ClassInstallParamsSize);
[DllImport("setupapi.dll", SetLastError = true)]
public static extern bool SetupDiChangeState(IntPtr deviceInfoSet, [In] ref SP_DEVINFO_DATA deviceInfoData);
[DllImport("setupapi.dll", SetLastError = true)]
pulic static extern bool SetupDiDestroyDeviceInfoList(IntPtr handle);


//找到设备所属类的GUID-到Device Manager, 开启设备，选择详细信息标签，属性选择”类Guid",把下面的值的部分复制出来

/// <summary>
/// HID
/// </summary>
private static readonly string HIDDevice = "{745a17a0-74d3-11d0-b6fe-00a0c90f57da}";
/// <summary>
/// Keyboard
/// </summary>
private static readonly string Keyboard = "{4d36e96b-e325-11ce-bfc1-08002be10318}";
/// <summary>
/// Mouse
/// </summary>
private static readonly string Mouse = "{4d36e96f-e325-11ce-bfc1-08002be10318}";
/// <summary>
/// USB
/// </summary>
private static readonly string USB = "{36fc9e60-c465-11cf-8056-444553540000}";


//设定禁用方法
public static void DisableDevice(string deviceClassGuid)
{
    IntPtr info = IntPtr.Zero;
    Guid NullGuid = Guid.Empty;
    try
    {
        info = SetupDiGetClassDevsW(ref NullGuid, null, IntPtr.Zero, DIGCF.ALLCLASSES | DIGCF.PROFILE);
        SP_DEVINFO_DATA devdata = new SP_DEVINFO_DATA();
        devdata.cbSize = (UInt32)Marshal.SizeOf(devdata);
        ///遍历设备
        for (uint i = 0; SetupDiEnumDeviceInfo(info, i, out devdata); i++)
        {
            if (devdata.ClassGuid == new Guid(deviceClassGuid))
            {
                SP_CLASSINSTALL_HEADER header = new SP_CLASSINSTALL_HEADER();
                header.cbSize = (UInt32)Marshal.SizeOf(header);
                header.InstallFunction = DIF.PROPERTYCHANGE;
                SP_PROPCHANGE_PARAMS propchangeparams = new SP_PROPCHANGE_PARAMS
                {
                    ClassInstallHeader = header,
                    StateChange = DICS.DISABLE,
                    Scope = DICS_FLAG.GLOBAL,
                    HwProfile = 0
                };
                SetupDiSetClassInstallParams(info, ref devdata, ref propchangeparams, (UInt32)Marshal.SizeOf(propchangeparams));
                SetupDiChangeState(info, ref devdata);
            }
        }
    }
    catch (Exception ex)
    {
        throw new Exception(string.Format("ChangeMouseState failed,the reason is {0}", ex.Message));
    }
    finally
    {
        if (info != IntPtr.Zero)
            SetupDiDestroyDeviceInfoList(info);
    }
}

//启用方法
public static void EnableDevice(string deviceClassGuid)
{
    IntPtr info = IntPtr.Zero;
    Guid NullGuid = Guid.Empty;
    try
    {
        info = SetupDiGetClassDevsW(ref NullGuid, null, IntPtr.Zero, DIGCF.ALLCLASSES);
        SP_DEVINFO_DATA devdata = new SP_DEVINFO_DATA();
        devdata.cbSize = (UInt32)Marshal.SizeOf(devdata);
        ///遍历设备
        for (uint i = 0; SetupDiEnumDeviceInfo(info, i, out devdata); i++)
        {
            if (devdata.ClassGuid == new Guid(deviceClassGuid))
            {
                SP_CLASSINSTALL_HEADER header = new SP_CLASSINSTALL_HEADER();
                header.cbSize = (UInt32)Marshal.SizeOf(header);
                header.InstallFunction = DIF.PROPERTYCHANGE;
                SP_PROPCHANGE_PARAMS propchangeparams = new SP_PROPCHANGE_PARAMS
                {
                    ClassInstallHeader = header,
                    StateChange = DICS.ENABLE,
                    Scope = DICS_FLAG.GLOBAL,
                    HwProfile = 0
                };
                SetupDiSetClassInstallParams(info, ref devdata, ref propchangeparams, (UInt32)Marshal.SizeOf(propchangeparams));
                SetupDiChangeState(info, ref devdata);
            }
        }
    }
    catch (Exception ex)
    {
        throw new Exception(string.Format("ChangeMouseState failed,the reason is {0}", ex.Message));
    }
    finally
    {
        if (info != IntPtr.Zero)
            SetupDiDestroyDeviceInfoList(info);
    }
}
