using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Management;
using System.Net.NetworkInformation;


namespace nextseq_utils
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly List<NetworkInterface> adapters;

        public MainWindow()
        {
            InitializeComponent();
            adapters = GetNetworkInfo();
            foreach (NetworkInterface adapter in adapters)
            {
                string Name = adapter.Name;
                ComboBoxItem cbi = new ComboBoxItem();
                cbi.Content = Name;
                AdapterSelector.Items.Add(cbi);
            }
        }

        private static List<NetworkInterface> GetNetworkInfo()
        {
            List<NetworkInterface> result = new List<NetworkInterface>();
            foreach (NetworkInterface adapter in NetworkInterface.GetAllNetworkInterfaces())
            {
                result.Add(adapter);
            }
            return result;
        }

        private NetworkInterface GetAdapterByName(string name)
        {
            NetworkInterface adapter = null;
            foreach (NetworkInterface adapter2 in adapters)
            {
                if (adapter2.Name == name)
                {
                    adapter = adapter2;
                    break;
                }
            }
            return adapter;
        }

        private string GetGateWay(IPInterfaceProperties ip)
        {
            string gateWay = "网关信息不存在！";
            GatewayIPAddressInformationCollection gateways = ip.GatewayAddresses;
            foreach (GatewayIPAddressInformation gateway in gateways)
            {
                if (IsPingIP(gateway.Address.ToString()))
                {
                    gateWay = gateway.Address.ToString();
                    break;
                }
            }
            return gateWay;
        }

        private static bool IsPingIP(string ip)
        {
            try
            {
                Ping ping = new Ping();
                PingReply reply = ping.Send(ip, 1000);
                if (reply != null) { return true; } else { return false; }
            }
            catch
            {
                return false;
            }
        }

        private void SetAdapterInfo(NetworkInterface adapter)
        {
            IPInterfaceProperties ip = adapter.GetIPProperties();
            UnicastIPAddressInformationCollection ipCollection = ip.UnicastAddresses;
            foreach (UnicastIPAddressInformation item in ipCollection)
            {
                if (item.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                {
                    IpAddress.Text = item.Address.ToString();
                    subnetMask.Text = item.IPv4Mask.ToString();
                }
            }
            if (ip.DnsAddresses.Count > 0)
            {
                DNS.Text = ip.DnsAddresses[0].ToString();
                if (ip.DnsAddresses.Count > 1)
                {
                    DNS2.Text = ip.DnsAddresses[1].ToString();
                }
                else
                {
                    DNS2.Text = "备用DNS不存在！";
                }
            }
            else
            {
                DNS.Text = "DNS不存在！";
                DNS2.Text = "备用DNS不存在！";
            }
            Gateway.Text = GetGateWay(ip);
            if (ip.DhcpServerAddresses.Count > 0)
            {
                DHCPServer.Text = ip.DhcpServerAddresses.FirstOrDefault().ToString();
            }
            else
            {
                DHCPServer.Text = "DHCP服务不存在！";
            }
            PhysicalAddress pa = adapter.GetPhysicalAddress();
            byte[] bytes = pa.GetAddressBytes();
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < bytes.Length; i++)
            {
                sb.Append(bytes[i].ToString("X2"));
                if (i != bytes.Length - 1)
                {
                    sb.Append('-');
                }
            }
            MAC.Text = sb.ToString();
            IsAutoSelector.SelectedIndex = 0;
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string content = AdapterSelector.SelectedValue.ToString();
            content = content.Replace("System.Windows.Controls.ComboBoxItem: ", "");
            NetworkInterface adapter = GetAdapterByName(content);
            if (adapter != null)
            {
                SetAdapterInfo(adapter);
            }
        }

        private static void EnableDHCP(NetworkInterface adapter)
        {
            ManagementClass wmi = new ManagementClass("Win32_NetworkAdapterConfiguration");
            ManagementObjectCollection moc = wmi.GetInstances();
            foreach (ManagementObject m in moc)
            {
                if (!(bool)m["IPEnabled"])
                    continue;
                if (m["SettingID"].ToString() == adapter.Id)
                {
                    m.InvokeMethod("SetDNSServerSearchOrder", null);
                    m.InvokeMethod("EnableDHCP", null);
                    MessageBox.Show("已设置自动获取！");
                }
            }
        }

        private void ComboBox_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
        {
            string content = IsAutoSelector.SelectedValue.ToString();
            content = content.Replace("System.Windows.Controls.ComboBoxItem: ", "");
            if (content == "自动")
            {
                IpAddress.IsEnabled = false;
                subnetMask.IsEnabled = false;
                Gateway.IsEnabled = false;
                DNS.IsEnabled = false;
                DNS2.IsEnabled = false;
                DHCPServer.IsEnabled = false;
                SettingIP.IsEnabled = false;
                if (IsAutoSelector.Text == "手动")
                {
                    string name = AdapterSelector.SelectedValue.ToString();
                    name = name.Replace("System.Windows.Controls.ComboBoxItem: ", "");
                    NetworkInterface adapter = GetAdapterByName(name);
                    if (adapter != null)
                    {
                        EnableDHCP(adapter);
                        SetAdapterInfo(adapter);
                    }
                }
            }
            else if (content == "手动")
            {
                IpAddress.IsEnabled = true;
                subnetMask.IsEnabled = true;
                Gateway.IsEnabled = true;
                DNS.IsEnabled = true;
                DNS2.IsEnabled = true;
                DHCPServer.IsEnabled = true;
                SettingIP.IsEnabled = true;
            }
        }

        private static ManagementObject GetNetwork(NetworkInterface adapter)
        {
            string netState = "SELECT * From Win32_NetworkAdapter";
            ManagementObjectSearcher searcher = new ManagementObjectSearcher(netState);
            ManagementObjectCollection moc = searcher.Get();
            /*MessageBox.Show(adapter.Description);*/
            foreach (ManagementObject m in moc)
            {
                /*MessageBox.Show(m["Name"].ToString());*/
                if (m["Name"].ToString() == adapter.Description)
                {
                    return m;
                }
            }
            return null;
        }

        private static bool EnableAdapter(ManagementObject m)
        {
            try
            {
                m.InvokeMethod("Enable", null);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private static bool DisableAdapter(ManagementObject m)
        {
            try
            {
                m.InvokeMethod("Disable", null);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (AdapterSelector.Text == "")
            {
                MessageBox.Show("请选择网卡后操作！");
            }
            else
            {
                NetworkInterface adapter = GetAdapterByName(AdapterSelector.Text);
                if (adapter != null)
                {
                    if (EnableAdapter(GetNetwork(adapter)))
                    {
                        MessageBox.Show("开启网卡成功!");
                    }
                    else
                    {
                        MessageBox.Show("开启网卡失败!");
                    };
                }
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if (AdapterSelector.Text == "")
            {
                MessageBox.Show("请选择网卡后操作！");
            }
            else
            {
                NetworkInterface adapter = GetAdapterByName(AdapterSelector.Text);
                if (adapter != null)
                {
                    if (DisableAdapter(GetNetwork(adapter)))
                    {
                        MessageBox.Show("关闭网卡成功!");
                    }
                    else
                    {
                        MessageBox.Show("关闭网卡失败!");
                    };
                }
            }
        }

        private static bool SetIPAddress(NetworkInterface adapter, string[] ip, string[] submask, string[] gateway, string[] dns)
        {
            ManagementClass mc = new ManagementClass("Win32_NetworkAdapterConfiguration");
            ManagementObjectCollection moc = mc.GetInstances();
            string str = "";
            foreach (ManagementObject m in moc)
            {
                /*if (!(bool)m["IPEnabled"])
                    continue;*/
                if (m["SettingID"].ToString() == adapter.Id)
                {
                    if (ip != null && submask != null)
                    {
                        ManagementBaseObject inPar;
                        ManagementBaseObject outPar;
                        string caption = m["Caption"].ToString();
                        inPar = m.GetMethodParameters("EnableStatic");
                        inPar["IPAddress"] = ip;
                        inPar["SubnetMask"] = submask;
                        outPar = m.InvokeMethod("EnableStatic", inPar, null);
                        str = outPar["returnvalue"].ToString();
                        if (str != "0" && str != "1")
                        {
                            return false;
                        }
                    }
                    if (gateway != null)
                    {
                        ManagementBaseObject inPar;
                        ManagementBaseObject outPar;
                        string caption = m["Caption"].ToString();
                        inPar = m.GetMethodParameters("SetGateways");
                        inPar["DefaultIPGateway"] = gateway;
                        outPar = m.InvokeMethod("SetGateways", inPar, null);
                        str = outPar["returnvalue"].ToString();
                        if (str != "0" && str != "1")
                        {
                            return false;
                        }
                    }
                    if (dns != null)
                    {
                        ManagementBaseObject inPar;
                        ManagementBaseObject outPar;
                        inPar = m.GetMethodParameters("SetDNSServerSearchOrder");
                        inPar["DNSServerSearchOrder"] = dns;
                        outPar = m.InvokeMethod("SetDNSServerSearchOrder", inPar, null);
                        str = outPar["returnvalue"].ToString();
                        if (str != "0" && str != "1")
                        {
                            return false;
                        }
                    }
                    return true;
                }
            }
            return false;
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            NetworkInterface adapter = GetAdapterByName(AdapterSelector.Text);
            if (adapter != null)
            {
                string[] ip = { IpAddress.Text };
                string[] submask = { subnetMask.Text };
                string[] gateway;
                if (Gateway.Text != "网关信息不存在！" && Gateway.Text != "")
                {
                    gateway = new string[1];
                    gateway[0] = Gateway.Text;
                }
                else
                {
                    gateway = null;
                }
                string[] dns;
                if (DNS.Text != "DNS不存在！" && DNS.Text != "")
                {

                    if (DNS2.Text != "备用DNS不存在！" && DNS2.Text != "")
                    {
                        dns = new string[2];
                        dns[0] = DNS.Text;
                        dns[1] = DNS2.Text;
                    }
                    else
                    {
                        dns = new string[1];
                        dns[0] = DNS.Text;
                    }
                }
                else
                {
                    dns = null;
                }

                if (SetIPAddress(adapter, ip, submask, gateway, dns))
                {
                    MessageBox.Show("设置成功！");
                }
                else
                {
                    MessageBox.Show("设置失败！");
                };
            }
        }
    }
}
