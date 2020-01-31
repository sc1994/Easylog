using System.Net;
using System.Net.Sockets;

namespace Easylog.Extension
{
    internal static class NetworkHelper
    {
        private static string _ip;

        public static string Ip
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(_ip))
                    return _ip;
                lock ("1")
                {
                    if (!string.IsNullOrWhiteSpace(_ip))
                        return _ip;
                    var host = Dns.GetHostEntry(Dns.GetHostName());
                    foreach (var ip in host.AddressList)
                    {
                        if (ip.AddressFamily == AddressFamily.InterNetwork)
                        {
                            return _ip = ip.ToString();
                        }
                    }
                    return _ip = "0.0.0.0";
                }
            }
        }
    }
}
