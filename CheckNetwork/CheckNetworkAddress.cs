using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Net;
using System.Text.RegularExpressions;

namespace CheckNetwork
{
    class CheckNetworkAddress
    {
        private IPAddress[] _ipAddresses = null;

        /// <summary>
        /// コンストラクタ呼び出しでIPアドレスを取得
        /// </summary>
        public CheckNetworkAddress()
        {
            foreach (NetworkInterface nic in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (nic.NetworkInterfaceType == NetworkInterfaceType.Loopback ||
                    nic.NetworkInterfaceType == NetworkInterfaceType.Tunnel)
                {
                    continue;
                }
                _ipAddresses = nic.GetIPProperties().UnicastAddresses.
                    Where(x => x.Address.AddressFamily == AddressFamily.InterNetwork).
                    Select(x => x.Address).ToArray();
            }
        }

        /// <summary>
        /// ネットワークアドレスと一致チェック
        /// </summary>
        /// <param name="networkAddress"></param>
        /// <returns></returns>
        public bool IsMatch(string networkAddress)
        {
            return IsMatch(new string[1] { networkAddress });
        }

        /// <summary>
        /// ネットワークアドレスと一致チェック
        /// 複数の引数でチェック
        /// </summary>
        /// <param name="networkAddress"></param>
        /// <param name="networkAddresses"></param>
        /// <returns></returns>
        public bool IsMAtch(string networkAddress, params string[] networkAddresses)
        {
            List<string> tempList = new List<string>(networkAddresses);
            tempList.Insert(0, networkAddress);
            return IsMatch(tempList.ToArray());
        }

        /// <summary>
        /// ネットワークアドレスと一致チェック
        /// 文字列配列あるいは「,」区切り文字列をチェック。
        /// </summary>
        /// <param name="networkAddresses"></param>
        /// <returns></returns>
        public bool IsMatch(string[] networkAddresses)
        {
            Regex reg_comma = new Regex(@",\s*");
            Regex reg_nwaddr = new Regex(
                @"^((\d\d?|1\d\d|2[0-4]\d|25[0-5])\.){3}(\d\d?|1\d\d|2[0-4]\d|25[0-5])/(\d+|((\d\d?|1\d\d|2[0-4]\d|25[0-5])\.){3}(\d\d?|1\d\d|2[0-4]\d|25[0-5]))$");
            foreach (string networkAddress in networkAddresses)
            {
                if (string.IsNullOrEmpty(networkAddress)) { continue; }
                if (reg_nwaddr.IsMatch(networkAddress))
                {
                    string networkAddr = networkAddress.Substring(0, networkAddress.IndexOf("/"));
                    string subnetMask = networkAddress.Substring(networkAddress.IndexOf("/") + 1);

                    byte[] ntwBytes = IPAddress.Parse(networkAddr).GetAddressBytes();
                    byte[] maskBytes = int.TryParse(subnetMask, out int tempInt) ?
                        BitConverter.GetBytes(~(uint.MaxValue >> tempInt)).Reverse().ToArray() :
                        IPAddress.Parse(subnetMask).GetAddressBytes();


                }
            }

            return true;

        }



        private void test()
        {
            /*
            Console.WriteLine(nic.Name);

            byte[] networkAddr = IPAddress.Parse("192.168.151.0").GetAddressBytes();
            byte[] subnetMask = IPAddress.Parse("255.255.255.0").GetAddressBytes();



            foreach (UnicastIPAddressInformation ip in nic.GetIPProperties().UnicastAddresses)
            {
                if (ip.Address.AddressFamily == AddressFamily.InterNetwork)
                {
                    byte[] ipAddr = ip.Address.GetAddressBytes();

                    if ((networkAddr[0] & subnetMask[0]) == (ipAddr[0] & subnetMask[0]) &&
                        (networkAddr[1] & subnetMask[1]) == (ipAddr[1] & subnetMask[1]) &&
                        (networkAddr[2] & subnetMask[2]) == (ipAddr[2] & subnetMask[2]) &&
                        (networkAddr[3] & subnetMask[3]) == (ipAddr[3] & subnetMask[3]))
                    {
                        Console.WriteLine("●");
                        Console.WriteLine(ip.Address);
                    }
                }
            }
            */
        }
    }
}
