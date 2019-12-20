using System.Net;
using System.Net.Sockets;

namespace EasyLog.WriteLog
{
    internal static class Stores
    {
        internal const string MessageTemp = "easy_logger({app},{category1},{category2},{category3},{log},{filter1},{filter2},{ip},{trace},{calls},{exception})";
        internal const string TraceName = "easy_logger_trace_mame";
        internal static string Ip => GetLocalIpAddress();
        private static string _ip;
        private static string GetLocalIpAddress()
        {
            if (!string.IsNullOrWhiteSpace(_ip)) return _ip;
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return _ip = ip.ToString();
                }
            }
            return "0.0.0.0";
        }
    }
}