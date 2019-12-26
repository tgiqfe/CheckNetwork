using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckNetwork
{
    class Program
    {
        static void Main(string[] args)
        {
            /*
            CheckComputerName check = new CheckComputerName();
            string sourceText = "aaaa, bbbb, cccc";
            Console.WriteLine(check.IsMatch(sourceText));
            */
            /*
            CheckNetworkAddress checkNW = new CheckNetworkAddress();
            bool result = checkNW.IsMatch("192.168.151.129/255.255.255.128");
            Console.WriteLine(result);
            */
            CheckNetworkAddress checkNW2 = new CheckNetworkAddress();
            bool result = checkNW2.IsMatch("192.168.150.0/24, 192.168.152.0/24, 192.168.153.0/24");
            Console.WriteLine(result);

            Console.ReadLine();
        }
    }
}
