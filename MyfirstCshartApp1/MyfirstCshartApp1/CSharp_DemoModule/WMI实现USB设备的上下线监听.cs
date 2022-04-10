using System;
using System.Text.RegularExpressions;
using System.Management;
using System.Collections.Generic;

/// <summary>
/// 添加USB设备监视与监听
/// </summary>
/// <returns></returns>
public bool AddUSBEventWatcher()
{
    try
    {
        var scope = new ManagementScope("root\\CIMV2");
        var insert = new WqlEventQuery("__InstanceCreationEvent", TimeSpan.FromSeconds(1), "TargetInstance isa 'Win32_USBControllerDevice'");
        var remove = new WqlEventQuery("__InstanceDeletionEvent", TimeSpan.FromSeconds(1), "TargetInstance isa 'Win32_USBControllerDevice'");

        _insertWatcher = new ManagementEventWatcher(scope, insert);
        _removeWatcher = new ManagementEventWatcher(scope, remove);

        ///WMI服务USB加载响应事件
        _insertWatcher.EventArrived += OnUSBInserted;
        ///WMI服务USB移除响应事件
        _removeWatcher.EventArrived += OnUSBRemoved;

        ///开启监听
        _insertWatcher.Start();
        _removeWatcher.Start();

        return true;
    }
    catch (Exception)
    {
        Dispose();
        return false;
    }
}


/// <summary>
/// Usb设备上线处理方法
/// </summary>
/// <param name="sender"></param>
/// <param name="e"></param>
private void OnUSBInserted(object sender, EventArrivedEventArgs e)
{
    string dependent = UsbDeviceInfo.WhoUsbControllerDevice(e).Dependent;
    string text = dependent.Replace("\\\\", "\\");

    ///Usb存储类设备标志
    if (text.StartsWith("USBSTOR\\"))
    {
        UsbStorageInserted?.Invoke(this, new UsbStorageCreatEventArgs(text, dependent));
    }
    else if (text.StartsWith("HID\\"))
    {
        PnPEntityInfo[] pnPEntityInfos = UsbDeviceInfo.WhoPnPEntity(text);

        for (int i = 0; !(pnPEntityInfos == null) && i < pnPEntityInfos.Length; i++)
        {
            ///通过guid去判定当前上线设备是什么类别的设备
            if (pnPEntityInfos[i].ClassGuid == Mouse)
            {
                HIDMouseInserted?.Invoke(this, pnPEntityInfos[i]);
            }
            else if (pnPEntityInfos[i].ClassGuid == Keyboard)
            {
                HIDKeyboardInserted?.Invoke(this, pnPEntityInfos[i]);
            }
        }
    }
}


/// <summary>
/// Usb设备下线处理方法
/// </summary>
/// <param name="sender"></param>
/// <param name="e"></param>
private void OnUSBRemoved(object sender, EventArrivedEventArgs e)
{
    string dependent = UsbDeviceInfo.WhoUsbControllerDevice(e).Dependent;
    string text = dependent.Replace("\\\\", "\\");

    ///Usb存储类设备标志
    if (text.StartsWith("USBSTOR\\"))
    {
        UsbStorageRemoved?.Invoke(this, new UsbStorageDeleteEventArgs(text));
    }
}



//方法调用

static void Main(string[] args)
{
    UsbMonitor();

    Console.ReadLine();

    Dispose();
}
/// <summary>
/// Usb设备上下线监控方法
/// </summary>
static void UsbMonitor()
{
    _usbDeviceWatcher = UsbDeviceWatcher.Instance;

    _usbDeviceWatcher.UsbStorageInserted += UsbStorageInsertedHandler;

    _usbDeviceWatcher.UsbStorageRemoved += UsbStorageRemovedHandler;
}


//项目地址：https://github.com/LcFireRabbit/Utils