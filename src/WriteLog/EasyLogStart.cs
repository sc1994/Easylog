using System;
using Microsoft.AspNetCore.Builder;
using Serilog.Core;
using Microsoft.Extensions.DependencyInjection;
using System.IO;
using Microsoft.AspNetCore.Http;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using static Newtonsoft.Json.JsonConvert;
using System.Diagnostics;
using Newtonsoft.Json;
using System.Collections.Generic;

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

        /// <summary>
        /// 启用 EasyLog 记录 Http 内容
        /// </summary>
        /// <param name="app"></param>
        /// <param name="options"></param>
        public static void UseEasyLogHttp(this IApplicationBuilder app, Action<EasyLogHttpOption> options)
        {
            if (app is null) throw new ArgumentNullException(nameof(app));
            if (options is null) throw new ArgumentNullException(nameof(options));

            var easyLog = new EasyLogHttpOption();
            options(easyLog);

            app.Use(async (context, next) =>
            {
                context.Request.EnableBuffering(bufferThreshold, bufferLimit);
                var method = context.Request.Method;
                var url = context.Request.Scheme + "://" + context.Request.Host + context.Request.Path.Value + context.Request.QueryString.Value;
                var header = context.Request.Headers.ToDictionary(x => x.Key, x => x.Value);
                var bodyStr = await FormatRequest(context.Request);
                object body;
                if (context.Request.ContentType?.ToLower().Contains("application/json") == true)
                    body = DeserializeObject<object>(bodyStr);
                else
                    body = bodyStr;
                var cookies = context.Request.Cookies.ToDictionary(x => x.Key, x => x.Value);


                object response = null;
                if (easyLog.IsLoggingResponseBody)
                {
                    var responseStr = await FormatResponse(context.Response, next);
                    if (context.Response.ContentType?.ToLower().Contains("application/json") == true)
                        response = DeserializeObject<object>(responseStr);
                    else
                        response = responseStr;
                }
                else
                {
                    await next();
                }

                Trace.WriteLine($@"
=================================================================
{SerializeObject(new { method, url, header, body, cookies, response }, Formatting.Indented)}
=================================================================
");
            });
        }

        private static async Task<string> FormatRequest(HttpRequest request)
        {
            using (var reader = new StreamReader(
                request.Body,
                Encoding.UTF8,
                false,
                bufferLimit,
                leaveOpen: true))
            {
                var r = await reader.ReadToEndAsync();
                request.Body.Position = 0; // 重置到起点 , 被这个坑看好久
                return r;
            }
        }

        private static async Task<string> FormatResponse(HttpResponse response, Func<Task> next)
        {
            Stream originalBody = response.Body;
            string responseBody = null;
            try
            {
                using (var memStream = new MemoryStream())
                {
                    response.Body = memStream;
                    await next();
                    memStream.Position = 0;
                    responseBody = new StreamReader(memStream).ReadToEnd();

                    memStream.Position = 0;
                    await memStream.CopyToAsync(originalBody);
                }
            }
            finally
            {
                response.Body = originalBody;
            }
            return responseBody;
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

    public class EasyLogHttpOption
    {
        internal EasyLogHttpOption() { }

        /// <summary>
        /// 需要记录的 Request Headers
        /// </summary>
        public List<string> LoggingRequestHeaders { get; } = new List<string>();

        /// <summary>
        /// 是否记录请求 Body
        /// </summary>
        public bool IsLoggingRequestBody { get; set; }

        /// <summary>
        /// 需要记录的 Response Headers
        /// </summary>
        public List<string> LoggingResponseHeaders { get; } = new List<string>();

        /// <summary>
        /// 是否记录响应 Body
        /// </summary>
        public bool IsLoggingResponseBody { get; set; }
    }
}