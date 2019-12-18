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

        public static void AddEasyLog(this IServiceCollection services)
        {
            services.AddScoped<EasyLogger, EasyLogger>();
        }

        public static void UseEasyLog(this IApplicationBuilder app, Action<EasyLogOption> options)
        {
            if (app is null) throw new ArgumentNullException(nameof(app));
            if (options is null) throw new ArgumentNullException(nameof(options));

            var easyLog = new EasyLogOption();
            options(easyLog);
            App = easyLog.App;
            Logger = easyLog.Serilog;
            Debug = easyLog.Debug;
        }
        public class EasyLogOption
        {
            internal EasyLogOption() { }
            public Logger Serilog { get; set; }
            public string App { get; set; }
            public bool Debug { get; set; }
        }

    }
}