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
        private List<byte[]> _ipAddressBytesList = null;
        private List<string> _ipAddressStringList = null;
        public List<byte[]> IPAddressBytesList { get { return this._ipAddressBytesList; } }
        public List<string> IPAddressStringList { get { return this._ipAddressStringList; } }

        /// <summary>
        /// コンストラクタ呼び出しでIPアドレスを取得
        /// </summary>
        public CheckNetworkAddress()
        {
            _ipAddressBytesList = new List<byte[]>();
            _ipAddressStringList = new List<string>();

            foreach (NetworkInterface nic in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (nic.NetworkInterfaceType == NetworkInterfaceType.Loopback ||
                    nic.NetworkInterfaceType == NetworkInterfaceType.Tunnel)
                {
                    continue;
                }
                IPAddress[] ipAddresses = nic.GetIPProperties().UnicastAddresses.
                    Where(x => x.Address.AddressFamily == AddressFamily.InterNetwork).
                    Select(x => x.Address).ToArray();
                foreach (IPAddress ipAddress in ipAddresses)
                {
                    _ipAddressBytesList.Add(ipAddress.GetAddressBytes());
                    _ipAddressStringList.Add(ipAddress.ToString());
                }
            }
        }

        /// <summary>
        /// ネットワークアドレスと一致チェック
        /// </summary>
        /// <param name="networkAddress"></param>
        /// <returns></returns>
        public bool IsMatch(string networkAddress)
        {
            Regex reg_comma = new Regex(@",\s*");
            return IsMatch(reg_comma.Split(networkAddress));
        }

        /// <summary>
        /// ネットワークアドレスと一致チェック
        /// 複数の引数でチェック
        /// </summary>
        /// <param name="networkAddress"></param>
        /// <param name="networkAddresses"></param>
        /// <returns></returns>
        public bool IsMatch(string networkAddress, params string[] networkAddresses)
        {
            List<string> tempList = new List<string>(networkAddresses);
            tempList.Insert(0, networkAddress);
            return IsMatch(tempList);
        }

        /// <summary>
        /// ネットワークアドレスと一致チェック
        /// 引数List<string>で待ち受け
        /// </summary>
        /// <param name="networkAddresses"></param>
        /// <returns></returns>
        public bool IsMatch(List<string> networkAddresses)
        {
            return IsMatch(networkAddresses.ToArray());
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
                foreach (string address in reg_comma.Split(networkAddress))
                {
                    if (reg_nwaddr.IsMatch(address))
                    {
                        //  192.168.1.0/24 ←の形式でチェック
                        string networkAddr = networkAddress.Substring(0, networkAddress.IndexOf("/"));
                        string subnetMask = networkAddress.Substring(networkAddress.IndexOf("/") + 1);

                        byte[] networkAddressBytes = IPAddress.Parse(networkAddr).GetAddressBytes();
                        byte[] subnetMaskBytes = int.TryParse(subnetMask, out int tempInt) ?
                            BitConverter.GetBytes(~(uint.MaxValue >> tempInt)).Reverse().ToArray() :
                            IPAddress.Parse(subnetMask).GetAddressBytes();

                        foreach (byte[] ipAddressBytes in _ipAddressBytesList)
                        {
                            if ((networkAddressBytes[0] & subnetMaskBytes[0]) == (ipAddressBytes[0] & subnetMaskBytes[0]) &&
                                (networkAddressBytes[1] & subnetMaskBytes[1]) == (ipAddressBytes[1] & subnetMaskBytes[1]) &&
                                (networkAddressBytes[2] & subnetMaskBytes[2]) == (ipAddressBytes[2] & subnetMaskBytes[2]) &&
                                (networkAddressBytes[3] & subnetMaskBytes[3]) == (ipAddressBytes[3] & subnetMaskBytes[3]))
                            {
                                return true;
                            }
                        }
                    }
                    else if (address.Contains("*"))
                    {
                        //  ワイルドカードでチェック
                        string patternString = Regex.Replace(address, ".",
                        x =>
                        {
                            string y = x.Value;
                            if (y.Equals("?")) { return "\\."; }
                            else if (y.Equals("*")) { return ".*"; }
                            else { return Regex.Escape(y); }
                        });
                        if (!patternString.StartsWith("*")) { patternString = "^" + patternString; }
                        if (!patternString.EndsWith("*")) { patternString = patternString + "$"; }
                        Regex tempReg = new Regex(patternString, RegexOptions.IgnoreCase);

                        foreach (string ipAddressString in _ipAddressStringList)
                        {
                            if (tempReg.IsMatch(ipAddressString))
                            {
                                return true;
                            }
                        }
                    }
                }
            }
            return false;
        }
    }
}
