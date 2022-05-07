using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Management;
using System.Management.Automation;
using System.Management.Automation.Runspaces;


namespace windowapptutorial
{
    public partial class Form2 : Form
    {
        public Form2()
        {
            InitializeComponent();
        }

        private void button3_Click(object sender, EventArgs e)
        {

        }

        private void Form2_Load(object sender, EventArgs e)
        {

        }
    }
}


namespace DHCP.Test
{
    class Class2
    {


        private static string RunPowershell(string filePath, string parameters)
        {
            RunspaceConfiguration runspaceConfiguration = RunspaceConfiguration.Create();
            Runspace runspace = RunspaceFactory.CreateRunspace(runspaceConfiguration);
            runspace.Open();
            RunspaceInvoke scriptInvoker = new RunspaceInvoke(runspace);
            Pipeline pipeline = runspace.CreatePipeline();
            Command scriptCommand = new Command(filePath);
            Collection<CommandParameter> commandParameters = new Collection<CommandParameter>();


            string[] tempParas = parameters.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < tempParas.Length; i += 2)
            {
                CommandParameter commandParm = new CommandParameter(tempParas[i], tempParas[i + 1]);
                commandParameters.Add(commandParm);
                scriptCommand.Parameters.Add(commandParm);
            }


            pipeline.Commands.Add(scriptCommand);
            var re = pipeline.Invoke();//在pipeline管道类线程上执行委托，并且获取到执行命令后的返回值
            string kk = "";
            foreach (var a in re)
            {
                kk = a.ToString() + kk;//打印返回信息
            }
            if (pipeline.Error.Count > 0)
            {
                throw new Exception("脚本执行失败");
            }
            runspace.Close();//关闭通信通道
            return kk;
        }
        static void Main(string[] args)
        {
            string re = RunPowershell(@".\test2.ps1", "");//执行项目中的脚本，注意，脚本ps1文件需要放在源代码DHCP.Test\bin\Debug 文件下
            Console.WriteLine(re);
            Console.ReadLine();
        }
    }
}


//再接下来是直接执行远程指令的方法，通过Runspace类直接传输字符串型的powershell指令

namespace DHCP.Test
{
    class Class1
    {

        static int ipnum = 0;
        static string[] ip1 = new string[100];
        //添加方法，需传入各项指定好的参数，MAC地址有点问题，
        //各项参数 1.DHCP服务器的总IP   2.作用域的IP  3.保留地址的IP（即电脑的IP）4. 与保留地址对应的MAC地址  5.名称  6.备注
        static void AddVlan(string DHCPip, string Vlanip, string ip, string mac, string ComputerName, string Remark)

        //旧式执行代码，使用dos端的执行命令，不严谨，有点小瑕疵，无法对流动的保留地址进行更新，也就是活动标识为active的ip地址
        {
            string KK = "netsh Dhcp Server " + DHCPip + " Scope " + Vlanip + " Add reservedip " + ip + " " + mac + " \"" + ComputerName + "\" \"" + Remark + "\" \"BOTH\"";
            string cmd = "$uname=\"123\\123\"\n$pwd = ConvertTo-SecureString -AsPlainText \"123\" -Force\n$cred =    New-Object System.Management.Automation.PSCredential($uname,$pwd)\n" + "$a = {" + KK + "}\n" + "Invoke-Command -Credential $cred -command $a -ComputerName 10.10.11.111\n" + "$?";
            if (CheckPSVlan(ip) == "False")
            {
                Console.WriteLine("ip:" + ip + "没有重复，正在添加");
                InvokeSystemPS(cmd);
                if (CheckPSVlan(ip) == "False")
                {
                    Console.WriteLine("添加失败，mac地址" + mac + "有问题，请重新输入");
                }
                else
                {
                    Console.WriteLine("已经成功添加ip:" + ip + "到域:" + Vlanip + "下");
                }
            }
            else
            {
                Console.WriteLine("输入IP:" + ip + "有重复，请重新输入IP");
            }
        }


        static void AddVlan1(string DHCPip, string Vlanip, string ip, string mac, string ComputerName, string Remark)
        {
            string KK = "netsh Dhcp Server " + DHCPip + " Scope " + Vlanip + " Add reservedip " + ip + " " + mac + " \"" + ComputerName + "\" \"" + Remark + "\" \"BOTH\"";
            string cmd = "$uname=\"123\\123\"\n$pwd = ConvertTo-SecureString -AsPlainText \"123\" -Force\n$cred =    New-Object System.Management.Automation.PSCredential($uname,$pwd)\n" + "$a = {" + KK + "}\n" + "Invoke-Command -Credential $cred -command $a -ComputerName 10.10.11.111\n" + "$?";
            InvokeSystemPS(cmd);
        }


        static void AddVlanNew(string DHCPip, string Vlanip, string ip, string mac, string ComputerName, string Remark)

        //新式的执行代码，使用powershell端的指令，Add方式
        {
            string str = "add-DhcpServerv4Reservation –ComputerName " + DHCPip + " -ScopeId " + Vlanip + " -IPAddress " + ip + " -ClientId " + mac + " -Name " + ComputerName + " -Description " + "\"" + Remark + "\"";
            string cmd = "$uname=\"123\\123\"\n$pwd = ConvertTo-SecureString -AsPlainText \"123\" -Force\n$cred =   New-Object System.Management.Automation.PSCredential($uname,$pwd)\n" + "$a = {" + str + "}\n" + "Invoke-Command -Credential $cred -command $a -ComputerName " + DHCPip + "\n" + "$?";

            InvokeSystemPS(cmd);
        }

        static void DeleteVlan(string ip, string mac)//删除方法，IP为保留地址，mac为对应的mac地址，但是随意输入的mac地址也能删除，所以猜测IP才是最主要的
        {
            string KK1 = "netsh dhcp server 10.10.11.111 scope 10.10.22.0 delete reservedip " + ip + " " + mac;
            string cmd1 = "$uname=\"123\\123\"\n$pwd = ConvertTo-SecureString -AsPlainText \"123\" -Force\n$cred =    New-Object System.Management.Automation.PSCredential($uname,$pwd)\n" + "$a = {" + KK1 + "}\n" + "Invoke-Command -Credential $cred -command $a -ComputerName 10.10.11.111\n" + "$?";
            if (CheckPSVlan(ip) != "False")
            {
                InvokeSystemPS(cmd1);
                Console.WriteLine("IP:" + ip + "已经成功删除");
            }
            else
            {
                Console.WriteLine("目标IP:" + ip + "不存在");
            }

        }


        //不仅仅10.10.1.133一个基地
        // static string CheckPSVlan(string DHCPIP,string ip)
        //正式使用时候备用，传入查询ip的地址和DHCP服务器的IP
        static string CheckPSVlan(string ip)//通过GET方法检查有没有重复的IP，如果没有则返回False(注意是字符串，F为大写)，即域下面没有存在保留地址

        //查询指定IP信息，不过只返回该IP下的IPAddress信息
        {
            string KK1 = "Get-DhcpServerv4Lease -IPAddress " + ip + " | fl IPAddress";
            string cmd1 = "$uname=\"123\\123\"\n$pwd = ConvertTo-SecureString -AsPlainText \"123\" -Force\n$cred =    New-Object System.Management.Automation.PSCredential($uname,$pwd)\n" + "$a = {" + KK1 + "}\n" + "Invoke-Command -Credential $cred -command $a -ComputerName 10.10.11.111\n" + "$?";


            //如果返回值为False时，则没有对应IP，如果返回其他值则是已经存在重复IP
            return InvokeSystemPS(cmd1);
        }


        public static string InvokeSystemPS(string cmd)//提交方法，将命令传入，打开与powershell交互的工作流，提交命令，并获得返回值
        {
            string kk = "";
            try
            {
                List<string> ps = new List<string>();
                //开启计算机的安全设置，允许执行可能会用到
                //开启最高的执行权限
                //Unrestricted——允许所有的script运行
                ps.Add("Set-ExecutionPolicy RemoteSigned");
                ps.Add("Set-ExecutionPolicy -ExecutionPolicy Unrestricted");
                ps.Add(cmd);
                Runspace runspace = RunspaceFactory.CreateRunspace();
                runspace.Open();
                Pipeline pipeline = runspace.CreatePipeline();
                foreach (var scr in ps)
                {
                    pipeline.Commands.AddScript(scr);
                }
                var test = pipeline.Invoke();//Execute the ps script
                foreach (var item in test)
                {
                    //Type typename = item.ImmediateBaseObject.GetType();//获得通道中返回的最原始基类对象
                    string member = test[0].Members.ElementAt(3).Value.ToString();//返回对象中的第四个属性，保留活动标识，AddressState
                    string a = member.Substring(0, member.Length);//分隔保留活动标识字符串
                                                                  //var A = Convert.ChangeType(item.ImmediateBaseObject, typename);//返回指定类型的对象

                    Console.WriteLine(typename);//打印返回基类对象信息
                    Console.WriteLine(a);//打印保留活动标识字符串
                                         //Console.WriteLine(item.ToString());//打印从通道流中的返回值信息
                                         //}
                    foreach (var a in test)
                    {
                        kk = a.ToString() + kk;
                        Console.WriteLine(kk);//打印返回信息
                    }
                    runspace.Close();


                }
            catch (Exception ex)
            {
                throw ex;
            }
            return kk;
        }


        static string CheckPSVlan1(string ip)

        //查询指定IP信息，并返回此IP下的所有信息
        {
            string KK1 = "Get-DhcpServerv4Lease -IPAddress " + ip;
            string cmd1 = "$uname=\"123\\123\"\n$pwd = ConvertTo-SecureString -AsPlainText \"123\" -Force\n$cred =    New-Object System.Management.Automation.PSCredential($uname,$pwd)\n" + "$a = {" + KK1 + "}\n" + "Invoke-Command -Credential $cred -command $a -ComputerName 10.10.11.111\n" + "$?";
            //如果返回值为False时，则没有对应IP，如果返回其他值则是已经存在重复IP

            //get 命令，获取服务器中是否存在指定IP，查询语句
            return InvokeSystemPS(cmd1);
        }

        static void Main(string[] args)
        {
            List<string[]> ip = new List<string[]>();
            string[] es = { "10.10.11.111", "10.10.22.0", "10.10.22.127", "4B-99-B6-D4-AE-8D", "测试电脑", "这是一个备注" };
            ip.Add(es);
            //AddVlanNew(es[0],es[1],es[2],es[3],es[4],es[5]);
            CheckPSVlan1("10.10.22.127");
            //CheckPSVlan("10.10.22.127");
            //AddVlan1(ip[0][0], ip[0][1], ip[0][2], ip[0][3], ip[0][4], ip[0][5]);
            //DeleteVlan(ip[0][2],ip[0][3]);
            Console.ReadLine();
        }
    }
}
