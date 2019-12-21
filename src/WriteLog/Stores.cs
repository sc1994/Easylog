using System;
using System.Net;
using System.Net.Sockets;
using Microsoft.AspNetCore.Http;

namespace EasyLog.WriteLog
{
    internal static class Stores
    {
        internal const string MessageTemp = "easy_logger({app},{category1},{category2},{category3},{log},{filter1},{filter2},{ip},{trace},{calls},{exception})";
        internal const string TraceName = "easy_logger_trace_mame";
        internal static string Ip => GetLocalIpAddress();

        /// <summary>
        /// 获取追踪guid, 此值藏于 HttpContext.Items 中 . 
        /// </summary>
        public static string GetTrace(HttpContext httpContext)
        {
            if (httpContext?.Items == null)
            {
                "无法捕获IHttpContextAccessor".WriteLine();
                return null;
            }
            var t = (string)httpContext.Items[Stores.TraceName];
            if (string.IsNullOrWhiteSpace(t))
                return (string)(httpContext.Items[Stores.TraceName] = Guid.NewGuid().ToString());
            return t;
        }
        
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