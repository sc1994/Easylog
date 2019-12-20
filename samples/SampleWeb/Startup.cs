using System;
using System.Diagnostics;
using System.IO;
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
                    options.IsLoggingRequestBody = true;
                    options.IsLoggingResponseBody = false;
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