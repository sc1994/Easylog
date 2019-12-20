using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using Newtonsoft.Json;

namespace EasyLog.WriteLog
{
    internal static class Extends
    {
        internal static void WriteLine(this object msg, string title = "")
        {
            WriteLine(JsonConvert.SerializeObject(msg, Formatting.Indented), title);
        }

        internal static void WriteLine(this string msg, string title = "")
        {
            if (!EasyLogStart.Debug) return;
            // return;
            Trace.WriteLine($@"
=================================={title}===========================================
{msg}
==================================={title}==========================================
            ");
        }

        /// <summary>
        /// 获取数组项 , 当index不存在的时候, 容错处理输出默认值
        /// </summary>
        internal static T TryIndex<T>(this IEnumerable<T> source, int index, T def = default)
        {
            var array = source.ToArray();
            if (array.Length > index)
                return array[index];
            return def;
        }
    }
}