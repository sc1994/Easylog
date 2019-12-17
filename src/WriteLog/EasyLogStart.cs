using System;
using Microsoft.AspNetCore.Builder;
using Serilog.Core;
using Microsoft.Extensions.DependencyInjection;

namespace WriteLog
{
    public static class EasyLogStart
    {
        internal static Logger Logger { get; private set; }
        internal static string App { get; private set; }

        public static void AddEasyLog(this IServiceCollection service)
        {
            service.AddScoped<EasyLog, EasyLog>();
        }

        public static void UseEasyLog(this IApplicationBuilder app, Action<EasyLogOption> options)
        {
            if (app is null) throw new ArgumentNullException(nameof(app));
            if (options is null) throw new ArgumentNullException(nameof(options));

            var easyLog = new EasyLogOption();
            options(easyLog);
            App = easyLog.App;
            Logger = easyLog.Serilog;
        }
        public class EasyLogOption
        {
            internal EasyLogOption() { }
            public Logger Serilog { get; set; }
            public string App { get; set; }
        }

    }
}