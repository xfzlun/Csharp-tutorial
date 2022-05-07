using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Management;

//利用WMI获取指定硬件的信息
namespace getsysteminformation
{
    internal class Program
    {
        static void Main(string[] args)
        {
            //这里利用WMI获取硬件信息, 我们用硬盘信息演示，查询API接口
            //https://docs.microsoft.com/zh-tw/windows/win32/cimwin32prov/win32-provider
            ManagementObjectSearcher maSearcher = new ManagementObjectSearcher("select * from Win32_Fan");
            //Win32_DiskDrive还可以换成Win32U
            //调用ManagementObjectSearcher.Get()可以取得相关信息的集合
            //用foreach展开
            foreach(ManagementObject moObject in maSearcher.Get())
            {
                //通过ManagementObject.GetPropertyValue(键名)可以得到对应的值
                //moObject.GetPropertyValue()
                //这里我们作为演示，遍历显示所有
                foreach(PropertyData propData in moObject.Properties)
                {
                    Console.Write(propData.Name);
                    Console.Write(":");
                    //Write与WiriteLine有什么不同？
                    //为什么一定要加上面这两行？
                    //propData.Value可能是空的null，我们要先处理一下
                    //propData.Value也有可能是数组，所以需要个循环输出
                    if (propData.Value != null)
                    {
                    //Console.WriteLine(propData.Value.ToString());
                        Console.Write("{");
                        foreach(var item in (Array)propData.Value)
                        {
                            Console.Write(item.ToString());
                            Console.Write(",");
                        }
                        Console.Write("\b}");

                    }
                    else
                        Console.WriteLine(propData.Value.ToString());
                }
                //加一个分割线
                Console.WriteLine("---------------------------------------");
            }
            Console.ReadKey();
        }
    }
}
