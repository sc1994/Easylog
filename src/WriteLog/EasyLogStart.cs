using System;
using Microsoft.AspNetCore.Builder;
using Serilog.Core;
using Microsoft.Extensions.DependencyInjection;

namespace EasyLog.WriteLog
{
    public static class EasyLogStart
    {
        internal static Logger Logger { get; private set; }
        internal static string App { get; private set; }
        internal static bool Debug { get; private set; }
        const int bufferLimit = 409600; // 最大缓冲限制 400KB (对于request body 来说绝对够用的)
        const int bufferThreshold = 204800; // 阈值 , 超过阈值的会写入文件 200KB 

        /// <summary>
        /// 注入 EasyLogger
        /// </summary>
        /// <param name="services"></param>
        public static void AddEasyLogger(this IServiceCollection services)
        {
            services.AddScoped<EasyLogger, EasyLogger>();
        }

        /// <summary>
        /// 启用 EasyLogger
        /// </summary>
        /// <param name="app"></param>
        /// <param name="options"></param>
        public static void UseEasyLogger(this IApplicationBuilder app, Action<EasyLogOption> options)
        {
            if (app is null) throw new ArgumentNullException(nameof(app));
            if (options is null) throw new ArgumentNullException(nameof(options));

            var easyLog = new EasyLogOption();
            options(easyLog);
            App = easyLog.AppId;
            Logger = easyLog.Serilog;
            Debug = easyLog.IsDebug;
        }
    }

    public class EasyLogOption
    {
        internal EasyLogOption() { }
        /// <summary>
        /// 配置 Serilog 
        /// </summary>
        public Logger Serilog { get; set; }
        /// <summary>
        /// 应用程序标识
        /// </summary>
        public string AppId { get; set; }
        /// <summary>
        /// 开启调试(会在调试中输出一些堆栈解析信息)
        /// </summary>
        public bool IsDebug { get; set; }
    }
}