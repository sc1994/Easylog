using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using static Newtonsoft.Json.JsonConvert;
using static WriteLog.EasyLogStart;

namespace WriteLog
{
    public class EasyLog
    {
        private const string _messageTemp = "easy_logger({app},{category1},{category2},{category3},{log},{filter1},{filter2},{trace},{ip},{trace},{ex})";
        private const string _traceName = "easy_logger_trace_mame";
        private readonly IHttpContextAccessor _httpAccessor;

        public EasyLog(IHttpContextAccessor httpAccessor)
        {
            _httpAccessor = httpAccessor;
        }

        public async void DebugAsync()
        {
            await Task.Run(() =>
            {
                var (category1, category2, category3) = GetDefaultClassify(Environment.StackTrace);
                LogFile(new[] { category1, category2, category3 });
            });
        }

        public void Debug()
        {
            var (category1, category2, category3) = GetDefaultClassify(Environment.StackTrace);
            LogFile(new[] { category1, category2, category3 });
        }

        public async void DebugJsonAsync(object log,
                                                string category1 = null,
                                                string category2 = null,
                                                string category3 = null,
                                                string filter1 = null,
                                                string filter2 = null)
        {
            if (category1 == null && category2 == null && category3 == null)
            {
                (category1, category2, category3) = GetDefaultClassify(Environment.StackTrace);
            }
            Logger.Information(
                _messageTemp,
                new[]
                {
                    App,
                    category1,
                    category2,
                    category3,
                    SerializeObject(log),
                    filter1,
                    filter2,
                    "TODO:",
                    "TODO:",
                    GetTrace(),
                    null
                });
        }

        public void DebugJson()
        {

        }

        public async void InfoAsync()
        {
            await Task.Run(() =>
            {

            });
        }

        public async void WarnAsync()
        {
            await Task.Run(() =>
            {

            });
        }

        public async void ErrorAsync(Exception ex)
        {
            await Task.Run(() =>
            {

            });
        }

        private (string category1, string category2, string category3) GetDefaultClassify(string stackTrace)
        {
            try
            {
                var ats = GetStackTraceFileAddress(stackTrace).Select(x => x.Trim()).ToArray();
                if (ats.Length < 1) goto ReturnDefault;
                var categories = new List<string>();
                foreach (var item in ats)
                {
                    /*
                    TODO 
                        使用 调用链将不能很好的展示层级关系,因为根据调用链的不同, 同一个方法将会出现在不同的层级上
                        使用 文件路径很好的展示了层级但是却不能   
                    */
                    var i = item.Split(new[] { "at ", " 在 ", " in ", " 位置 " }, StringSplitOptions.RemoveEmptyEntries)
                                .FirstOrDefault()
                                ?.Split(new[] { "." }, StringSplitOptions.RemoveEmptyEntries);
                    if (i?.Count() < 1) continue;
                    categories.Add(string.Join(".", i.Skip(i.Count() - 2)));
                }

                if (categories.Count < 1) goto ReturnDefault;
                categories = categories.Skip(categories.Count - 3).ToList();
                if (categories.Count <= 3)
                    return (TryGetArrayItem(categories, 2, "Default"), TryGetArrayItem(categories, 1, "Default"), TryGetArrayItem(categories, 0, "Default"));
                return (categories[categories.Count - 1], categories[categories.Count - 2], categories[categories.Count - 3]);
            }
            catch (Exception ex)
            {
                ErrorAsync(ex);
                goto ReturnDefault;
            }

        ReturnDefault: return ("Default", "Default", "Default");
        }

        /// <summary>
        /// 获取堆栈文件位置
        /// </summary>
        /// <param name="stackTrace"></param>
        /// <returns></returns>
        private string[] GetStackTraceFileAddress(string stackTrace)
        {
            var ats = stackTrace.Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries)
                                .Where(
                                    x => (x.Contains("  at") || x.Contains("  在"))
                                    && (x.Contains(" in ") || x.Contains(" 位置 "))
                                    && (x.Contains(".cs:line ") || x.Contains(".cs:行号 "))
                                    && !x.Contains(@"WriteLog\EasyLogger.cs"));
            return ats.ToArray();
        }

        private T TryGetArrayItem<T>(IEnumerable<T> source, int index, T def)
        {
            var array = source.ToArray();
            if (array.Length > index)
                return array[index];
            return def;
        }

        private string GetTrace()
        {
            var t = (string)_httpAccessor.HttpContext.Items[_traceName];
            if (string.IsNullOrWhiteSpace(t))
                return (string)(_httpAccessor.HttpContext.Items[_traceName] = Guid.NewGuid().ToString());
            return t;
        }

        private void LogFile(object msg)
        {
            File.AppendAllText("D:/1.log", $@"
=============================================================================
{SerializeObject(msg, Formatting.Indented)}
=============================================================================
            ");
        }

    }

    public class EasyLoggerOptions
    {

    }
}


//   var a = Environment.StackTrace;
//   var b = a.Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries)
//       .Where(
//           x => (x.Contains("  at") || x.Contains("  在"))
//           && (x.Contains(" in ") || x.Contains(" 位置 "))
//           && (x.Contains(".cs:line ") || x.Contains(".cs:行号 ")));
//   //foreach (var item in b)
//   //{
//   File.AppendAllLines("D:/1.log", b);