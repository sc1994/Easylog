using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Easylog.Extension
{
    internal static class StackParse
    {
        private readonly static ConcurrentDictionary<string, StackParseResult> _stackStore = new ConcurrentDictionary<string, StackParseResult>();
        private readonly static string[] _stackSplit = new[] { "at ", "在 ", " in ", " 位置 ", ".cs:line", ".cs:行号" };
        private readonly static StackParseResult _defaultParse = new StackParseResult();

        internal static StackParseResult GetStackParse(string stack)
        {
            if (_stackStore.TryGetValue(stack, out var parse1))
                return parse1;
            lock (stack)
            {
                if (_stackStore.TryGetValue(stack, out var parse2))
                    return parse2;

                var fileLines = GetStackFileLines(stack);
                if (fileLines.Length < 1) goto Default;

                var cs = new string[3];
                var item = fileLines[0];

                var i = item.Split(_stackSplit, StringSplitOptions.RemoveEmptyEntries);
                var calls = i.TryIndex(0)?.Split(new[] { "." }, StringSplitOptions.RemoveEmptyEntries).ToList(); // 最终路径的调调用方法
                var paths = i.TryIndex(1)?.Split(new[] { "\\" }, StringSplitOptions.RemoveEmptyEntries).ToList();

                cs[1] = paths?.LastOrDefault();
                _ = paths?.Remove(cs[1]);
                cs[0] = paths?.LastOrDefault();
                cs[2] = calls?.LastOrDefault();

                // 全调用方法集合
                var allCalls = fileLines.Select(x => x.Split(_stackSplit, StringSplitOptions.RemoveEmptyEntries).TryIndex(0))
                                        .Where(x => x != null)
                                        .Reverse()
                                        .ToArray();

                var r = new StackParseResult
                {
                    C1 = cs[0] ?? "DEF",
                    C2 = cs[1] ?? "DEF",
                    C3 = cs[2] ?? "DEF",
                    Calls = allCalls
                };
                _stackStore.TryAdd(stack, r);
            }

            Default: return _defaultParse;
        }

        /// <summary>
        /// 获取堆栈文件位置
        /// </summary>
        private static string[] GetStackFileLines(string stackTrace)
        {
            var ats = stackTrace.Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries)
                                .Where(
                                    x => (x.Contains("  at") || x.Contains("  在"))
                                    && (x.Contains(" in ") || x.Contains(" 位置 "))
                                    && (x.Contains(".cs:line ") || x.Contains(".cs:行号 "))
                                    && !x.Contains(@"\EasyLog\"))
                                .Select(x => x.Trim());
            return ats.ToArray();
        }

        private static T TryIndex<T>(this IEnumerable<T> source, int index, T def = default)
        {
            var array = source.ToArray();
            if (array.Length > index)
                return array[index];
            return def;
        }

        internal class StackParseResult
        {
            public string C1 { get; set; } = "Default";
            public string C2 { get; set; } = "Default";
            public string C3 { get; set; } = "Default";
            public string[] Calls { get; set; } = null;
        }
    }
}
