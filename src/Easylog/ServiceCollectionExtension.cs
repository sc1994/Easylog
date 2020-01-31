using System;
using Microsoft.AspNetCore.Http;
using Serilog;
using Serilog.Core;
using Serilog.Sinks.Elasticsearch;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// serilog 扩展
    /// </summary>
    public static class ServiceCollectionExtension
    {
        public static void AddEasylogger(this IServiceCollection service)
        {
            service.AddTransient<IHttpContextAccessor>(); // 添加http上下文

            var serilog = CreateLogger();

            service.AddSingleton(serilog); // 添加日志单例

        }

        public static Logger CreateLogger() // TODO: 设置补全
        {
            return new LoggerConfiguration()
                .WriteTo.ColoredConsole()
                .WriteTo.File("logs/easylog.log", rollingInterval: RollingInterval.Day)
                .WriteTo.Elasticsearch(new ElasticsearchSinkOptions(new Uri("http://localhost:9222/"))
                {

                })
                .CreateLogger();
        }
    }
}
