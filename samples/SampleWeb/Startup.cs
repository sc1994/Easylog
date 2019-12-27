using System;
using System.Linq;
using EasyLog.WriteLog;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using SampleWeb.Controllers;
using SampleWeb.Services;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.Elasticsearch;

namespace SampleWeb
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();

            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "SampleWeb",
                    Version = "v1"
                });
            });

            services.AddHttpContextAccessor();
            services.AddEasyLogger();

            services.AddScoped<LogTestService, LogTestService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, EasyLogger log, IHostEnvironment env)
        {
            // 启用 http 日志
            app.UseEasyLogHttp(options =>
            {
                options.RequestOption = new HttpOptionItem
                {
                    IsHasBody = true,
                    HasHeaders = new[]
                    {
                        "Accept",
                        "Content-Type",
                        "Host",
                        // "User-Agent",
                        "Origin",
                        "Content-Length"
                    },
                    IsHasCookies = true,
                    BodySizeLimit = 1024 * 10,
                };
                options.ResponseOption = new HttpOptionItem
                {
                    IsHasBody = true,
                    HasHeaders = new[]
                    {
                        "Content-Type",
                        "Server",
                        "Content-Length"
                    },
                    BodySizeLimit = 1024 * 30
                };
                options.Filter1 = new FilterGetWayByDictionary
                {
                    FilterWay = FilterGetWayDictionaryEnum.RequestHeaders,
                    GetFilterFunc = headers =>
                    {
                        // if (headers.TryGetValue("", out var v))
                        //     return v;
                        // return null;
                        // 建议在配置获取过滤规则的时候, 尽量做一些容错判定, 虽然内部代码会去兼容异常, 但是过于频繁的异常捕获是会消耗一定性能的
                        return headers.FirstOrDefault(x => x.Key.ToLower() == "user-token").Value;
                    }
                };
                // options.Filter2 = new FilterGetWayByString
                // {
                //     FilterWay = FilterGetWayStringEnum.Url,
                //     GetFilterFunc = url =>
                //     {
                //         var a = url.Split(new[] { "?id=" }, StringSplitOptions.RemoveEmptyEntries);
                //         if (a.Length == 2) return a[1].Split('&')[0]; // 获取url的params为id的值

                //         a = url.Split('/');
                //         return a[a.Length - 1].Split('?')[0]; // 打底规则, 如果上面的规则没有匹配, 则获取url地址的最后一位
                //     }
                // };
                // options.Blacklist = new FilterGetWayByDictionary
                // {
                //     FilterWay = FilterGetWayDictionaryEnum.RequestCookies,
                //     GetFilterFunc = header =>
                //     {

                //     }
                // };
            });

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("../swagger/v1/swagger.json", "SampleWeb");
            });

            // 启用 easy log 
            app.UseEasyLogger(options =>
            {
                options.AppId = "SampleWeb";
                options.IsDebug = true;
                options.Serilog =
                    new LoggerConfiguration()
                        .WriteTo.ColoredConsole(
                            restrictedToMinimumLevel: LogEventLevel.Debug
                        )
                        .WriteTo.Elasticsearch(new ElasticsearchSinkOptions(new Uri("http://localhost:9222/"))
                        {
                            AutoRegisterTemplate = true,
                            MinimumLogEventLevel = LogEventLevel.Debug
                        })
                        .CreateLogger();
            });
        }
    }
}