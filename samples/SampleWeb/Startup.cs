using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EasyLog.WriteLog;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
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
        public void Configure(
            IApplicationBuilder app,
            IWebHostEnvironment env
        )
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();

                // 启用 http 日志
                app.UseEasyLogHttp(options =>
                {
                    options.LoggingRequest = new LoggingHttpItem
                    {
                        IsIncludeBody = true,
                        IncludeHeaders = new List<string>
                        {
                            "Content-Type",
                            "Host",
                            "User-Agent",
                            "Origin",
                            "Content-Length"
                        },
                        IsIncludeCookies = true
                    };
                    options.LoggingResponse = new LoggingHttpItem
                    {
                        IsIncludeBody = true,
                        IncludeHeaders = new List<string>
                        {
                            "Content-Type",
                            "Server"
                        }
                    };
                    options.Filter1 = new FilterGetWayDictionary
                    {
                        FilterWay = FilterGetWayDictionaryEnum.RequestHeaders,
                        GetFilterFunc = headers =>
                        {
                            // 建议在配置获取过滤规则的时候, 尽量做一些容错判定, 虽然内部代码会去兼容异常, 但是过于频繁的异常捕获是会消耗一定性能的
                            return headers.FirstOrDefault(x => x.Key == "Content-Type").Value;
                        }
                    };
                    options.Filter2 = new FilterGetWayString
                    {
                        FilterWay = FilterGetWayStringEnum.Url,
                        GetFilterFunc = url =>
                        {
                            var a = url.Split(new[] { "?id=" }, StringSplitOptions.RemoveEmptyEntries);
                            if (a.Length == 2) return a[1].Split('&')[0]; // 获取url的params为id的值

                            a = url.Split('/');
                            return a[a.Length - 1].Split('?')[0]; // 打底规则, 如果上面的规则没有匹配, 则获取url地址的最后一位
                        }
                    };
                });
            }

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
                options.IsDebug = false;
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


    public class RequestLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestLoggingMiddleware> _logger;

        public RequestLoggingMiddleware(
        RequestDelegate next,
        ILogger<RequestLoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            var injectedRequestStream = new MemoryStream();

            try
            {
                var requestLog =
                $"REQUEST HttpMethod: {context.Request.Method}, Path: {context.Request.Path}";

                using (var bodyReader = new StreamReader(context.Request.Body))
                {
                    var bodyAsText = bodyReader.ReadToEnd();
                    if (string.IsNullOrWhiteSpace(bodyAsText) == false)
                    {
                        requestLog += $", Body : {bodyAsText}";
                    }

                    var bytesToWrite = Encoding.UTF8.GetBytes(bodyAsText);
                    injectedRequestStream.Write(bytesToWrite, 0, bytesToWrite.Length);
                    injectedRequestStream.Seek(0, SeekOrigin.Begin);
                    context.Request.Body = injectedRequestStream;
                }

                _logger.LogTrace(requestLog);

                await _next.Invoke(context);
            }
            finally
            {
                injectedRequestStream.Dispose();
            }
        }
    }
}