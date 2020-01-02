using System;
using System.Linq;
using Microsoft.AspNetCore.Http;
using static Newtonsoft.Json.JsonConvert;
using static EasyLog.WriteLog.EasyLogStart;
using Serilog.Events;
using Microsoft.Extensions.Hosting;

namespace EasyLog.WriteLog
{
    /// <summary>
    /// easy log 
    /// </summary>
    public class EasyLogger
    {
        private readonly string _trace;
        private readonly string _env;

        /// <summary>
        /// 创建不包含追踪值和环境值的日志记录对象
        /// </summary>
        public EasyLogger() { }

        /// <summary>
        /// 创建完全的日志记录对象
        /// </summary>
        /// <param name="httpAccessor"></param>
        /// <param name="env"></param>
        public EasyLogger(IHttpContextAccessor httpAccessor, IHostEnvironment env)
        {
            _trace = Stores.GetTrace(httpAccessor.HttpContext);
            _env = Stores.EnvironmentName = env.EnvironmentName;
        }
        /// <summary>
        /// Debug
        /// </summary>
        /// <param name="log">日志内容</param>
        /// <param name="category1">分类1 (默认方法所在的文件夹名称)</param>
        /// <param name="category2">分类2 (默认方法所在的.cs文件名)</param>
        /// <param name="category3">分类3 (默认方法名)</param>
        /// <param name="filter1">过滤1 用于精确定位日志 (比如可以是当前登录人员的id)</param>
        /// <param name="filter2">过滤2 (过滤1的扩展字段)</param>
        public void Debug(string log, string category1 = null, string category2 = null, string category3 = null, string filter1 = null, string filter2 = null)
            => ToLog(LogEventLevel.Debug, log, category1, category2, category3, filter1, filter2);

        /// <summary>
        /// Information
        /// </summary>
        /// <param name="log">日志内容</param>
        /// <param name="category1">分类1 (默认方法所在的文件夹名称)</param>
        /// <param name="category2">分类2 (默认方法所在的.cs文件名)</param>
        /// <param name="category3">分类3 (默认方法名)</param>
        /// <param name="filter1">过滤1 用于精确定位日志 (比如可以是当前登录人员的id)</param>
        /// <param name="filter2">过滤2 (过滤1的扩展字段)</param>
        public void Information(string log, string category1 = null, string category2 = null, string category3 = null, string filter1 = null, string filter2 = null)
            => ToLog(LogEventLevel.Information, log, category1, category2, category3, filter1, filter2);

        /// <summary>
        /// Warning
        /// </summary>
        /// <param name="log">日志内容</param>
        /// <param name="category1">分类1 (默认方法所在的文件夹名称)</param>
        /// <param name="category2">分类2 (默认方法所在的.cs文件名)</param>
        /// <param name="category3">分类3 (默认方法名)</param>
        /// <param name="filter1">过滤1 用于精确定位日志 (比如可以是当前登录人员的id)</param>
        /// <param name="filter2">过滤2 (过滤1的扩展字段)</param>
        /// <param name="exception">异常</param>
        public void Warning(string log, string category1 = null, string category2 = null, string category3 = null, string filter1 = null, string filter2 = null, Exception exception = null)
            => ToLog(LogEventLevel.Warning, log, category1, category2, category3, filter1, filter2, exception);

        /// <summary>
        /// Error
        /// </summary>
        /// <param name="log">日志内容</param>
        /// <param name="exception">异常</param>
        /// <param name="category1">分类1 (默认方法所在的文件夹名称)</param>
        /// <param name="category2">分类2 (默认方法所在的.cs文件名)</param>
        /// <param name="category3">分类3 (默认方法名)</param>
        /// <param name="filter1">过滤1 用于精确定位日志 (比如可以是当前登录人员的id)</param>
        /// <param name="filter2">过滤2 (过滤1的扩展字段)</param>
        public void Error(string log, Exception exception, string category1 = null, string category2 = null, string category3 = null, string filter1 = null, string filter2 = null)
            => ToLog(LogEventLevel.Error, log, category1, category2, category3, filter1, filter2, exception);

        /// <summary>
        /// Debug
        /// </summary>
        /// <param name="log">日志内容</param>
        /// <param name="category1">分类1 (默认方法所在的文件夹名称)</param>
        /// <param name="category2">分类2 (默认方法所在的.cs文件名)</param>
        /// <param name="category3">分类3 (默认方法名)</param>
        /// <param name="filter1">过滤1 用于精确定位日志 (比如可以是当前登录人员的id)</param>
        /// <param name="filter2">过滤2 (过滤1的扩展字段)</param>
        public void Debug(object log, string category1 = null, string category2 = null, string category3 = null, string filter1 = null, string filter2 = null)
            => Debug(SerializeObject(log), category1, category2, category3, filter1, filter2);

        /// <summary>
        /// Information
        /// </summary>
        /// <param name="log">日志内容</param>
        /// <param name="category1">分类1 (默认方法所在的文件夹名称)</param>
        /// <param name="category2">分类2 (默认方法所在的.cs文件名)</param>
        /// <param name="category3">分类3 (默认方法名)</param>
        /// <param name="filter1">过滤1 用于精确定位日志 (比如可以是当前登录人员的id)</param>
        /// <param name="filter2">过滤2 (过滤1的扩展字段)</param>
        public void Information(object log, string category1 = null, string category2 = null, string category3 = null, string filter1 = null, string filter2 = null)
            => Information(SerializeObject(log), category1, category2, category3, filter1, filter2);

        /// <summary>
        /// Warning
        /// </summary>
        /// <param name="log">日志内容</param>
        /// <param name="category1">分类1 (默认方法所在的文件夹名称)</param>
        /// <param name="category2">分类2 (默认方法所在的.cs文件名)</param>
        /// <param name="category3">分类3 (默认方法名)</param>
        /// <param name="filter1">过滤1 用于精确定位日志 (比如可以是当前登录人员的id)</param>
        /// <param name="filter2">过滤2 (过滤1的扩展字段)</param>
        public void Warning(object log, string category1 = null, string category2 = null, string category3 = null, string filter1 = null, string filter2 = null)
            => Warning(SerializeObject(log), category1, category2, category3, filter1, filter2);

        /// <summary>
        /// Error
        /// </summary>
        /// <param name="log">日志内容</param>
        /// <param name="exception">异常</param>
        /// <param name="category1">分类1 (默认方法所在的文件夹名称)</param>
        /// <param name="category2">分类2 (默认方法所在的.cs文件名)</param>
        /// <param name="category3">分类3 (默认方法名)</param>
        /// <param name="filter1">过滤1 用于精确定位日志 (比如可以是当前登录人员的id)</param>
        /// <param name="filter2">过滤2 (过滤1的扩展字段)</param>
        public void Error(object log, Exception exception, string category1 = null, string category2 = null, string category3 = null, string filter1 = null, string filter2 = null)
            => Error(SerializeObject(log), exception, category1, category2, category3, filter1, filter2);


        private void ToLog(LogEventLevel level, string log, string category1 = null, string category2 = null, string category3 = null, string filter1 = null, string filter2 = null, Exception exception = null)
        {
            string[] calls = null;
            if (category1 == null && category2 == null && category3 == null)
            {
                (category1, category2, category3, calls) = GetDefaultClassify(Environment.StackTrace);
            } // TODO 需要细分获取内容, 减少堆栈分析次数, 应所见即所得(传入和非传入一一对应,不影响其他参数的获取)
              // 
              // <{filter1}> <{filter2}>

            var ex = exception.ToString();
            var tempLate = "<{ip}> <{environment}> <{app}> <{category1}> <{category2}> <{category3}> <{trace}>";
            tempLate += "\r\n  log       : <{log}>";
            if (!string.IsNullOrWhiteSpace(filter1))
                tempLate += "\r\n  filter1   : <{filter1}>";
            if (!string.IsNullOrWhiteSpace(filter1))
                tempLate += "\r\n  filter2   : <{filter2}>";
            if (calls?.Any() == true)
                tempLate += "\r\n  calls     : <{calls}>";
            if (!string.IsNullOrWhiteSpace(ex))
                tempLate += "\r\n  exception : <{exception}>";

            var @params = new object[]
            {
                App,
                category1,
                category2,
                category3,
                log,
                filter1,
                filter2,
                Stores.Ip,
                _trace,
                calls,
                _env,
                ex
            }
            .Where(x =>
            {
                if (x is string s && !string.IsNullOrWhiteSpace(s)) return true;
                if (x is string[] a && a?.Any() == true) return true;
                return false;
            }).ToArray();
            switch (level)
            {
                case LogEventLevel.Debug:
                    EasyLogStart.Logger.Debug(tempLate, @params); return;
                case LogEventLevel.Information:
                    EasyLogStart.Logger.Information(tempLate, @params); return;
                case LogEventLevel.Warning:
                    EasyLogStart.Logger.Warning(tempLate, @params); return;
                case LogEventLevel.Error:
                    EasyLogStart.Logger.Error(tempLate, @params); return;
            }
        }


        private (string category1, string category2, string category3, string[] calls) GetDefaultClassify(string stackTrace)
        {
            string[] split = new[] { "at ", "在 ", " in ", " 位置 ", ".cs:line", ".cs:行号" };
            try
            {
                var ats = GetStackTraceFileAddress(stackTrace).Select(x => x.Trim()).ToArray();
                if (ats.Length < 1) goto ReturnDefault;
                var categories = new string[3];
                var item = ats[0];

                var i = item.Split(split, StringSplitOptions.RemoveEmptyEntries);
                var calls = i.TryIndex(0)?.Split(new[] { "." }, StringSplitOptions.RemoveEmptyEntries).ToList(); // 最终路径的调调用方法
                var paths = i.TryIndex(1)?.Split(new[] { "\\" }, StringSplitOptions.RemoveEmptyEntries).ToList();

                paths.WriteLine("解析文件路径");
                calls.WriteLine("解析最终的执行方法");

                categories[1] = paths?.LastOrDefault();
                _ = paths?.Remove(categories[1]);
                categories[0] = paths?.LastOrDefault();
                categories[2] = calls?.LastOrDefault();

                // 全调用方法集合
                var allCalls = ats.Select(x => x.Split(split, StringSplitOptions.RemoveEmptyEntries).TryIndex(0))
                                  .Where(x => x != null)
                                  .Reverse()
                                  .ToArray();
                allCalls.WriteLine("全调用方法集合");

                return (categories[0], categories[1], categories[2], allCalls);
            }
            catch (Exception exception)
            {
                Error("解析堆栈获取分类信息失败:" + stackTrace, exception, "WriteLog", "EasyLogger", "GetDefaultClassify");
                goto ReturnDefault;
            }

        ReturnDefault: return (null, null, null, null);
        }

        /// <summary>
        /// 获取堆栈文件位置
        /// </summary>
        private string[] GetStackTraceFileAddress(string stackTrace)
        {
            var ats = stackTrace.Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries)
                                .Where(
                                    x => (x.Contains("  at") || x.Contains("  在"))
                                    && (x.Contains(" in ") || x.Contains(" 位置 "))
                                    && (x.Contains(".cs:line ") || x.Contains(".cs:行号 "))
                                    && !x.Contains(@"WriteLog\EasyLogger.cs"));
            ats.WriteLine("筛选堆栈中包含文件和方法调用的文本行");
            return ats.ToArray();
        }
    }
}