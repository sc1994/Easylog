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
using System.Collections.Generic;
using System.Linq.Expressions;

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

            if (easyLog.LoggingRequest?.IncludeHeaders?.Count > 0) // 提前处理数据小写, 节约资源
                easyLog.LoggingRequest.IncludeHeaders = easyLog.LoggingRequest.IncludeHeaders.Select(x => x.ToLower()).ToList();
            if (easyLog.LoggingResponse?.IncludeHeaders?.Count > 0)
                easyLog.LoggingResponse.IncludeHeaders = easyLog.LoggingResponse.IncludeHeaders.Select(x => x.ToLower()).ToList();

            app.Use(async (context, next) => // 添加管道 , 处理request,response 的body 参考 https://stackoverflow.com/questions/43403941/how-to-read-asp-net-core-response-body
            {
                var method = context.Request.Method;
                var url = context.Request.Scheme + "://" + context.Request.Host + context.Request.Path.Value + context.Request.QueryString.Value;
                // 请求 headers
                IDictionary<string, string> requestHeaders = null;
                if (easyLog.LoggingRequest?.IncludeHeaders?.Count > 0)
                {
                    requestHeaders = context.Request.Headers
                                     .Where(x => easyLog.LoggingRequest.IncludeHeaders.Any(a => a == x.Key.ToLower()))
                                     .ToDictionary(x => x.Key, x => x.Value.ToString());
                }
                // 请求 body
                string requestBody = null;
                if (easyLog.LoggingRequest?.IsIncludeBody == true)
                {
                    context.Request.EnableBuffering(bufferThreshold, bufferLimit);
                    requestBody = await FormatRequest(context.Request);
                }
                // 请求 cookies
                IDictionary<string, string> requestCookies = null;
                if (easyLog.LoggingRequest?.IsIncludeCookies == true)
                {
                    requestCookies = context.Request.Cookies.ToDictionary(x => x.Key, x => x.Value);
                }
                // 响应 body
                string responseBody = null;
                if (easyLog.LoggingResponse?.IsIncludeBody == true)
                {
                    responseBody = await FormatResponse(context.Response, next); // 执行管道, 并获取响应body
                }
                else
                {
                    await next(); // 执行下一个管道
                }
                // 响应 headers
                IDictionary<string, string> responseHeader = null;
                if (easyLog.LoggingResponse?.IncludeHeaders?.Count > 0)
                    responseHeader = context.Response.Headers
                                             .Where(x => easyLog.LoggingResponse.IncludeHeaders.Any(a => a == x.Key.ToLower()))
                                             .ToDictionary(x => x.Key, x => x.Value.ToString());
                // 响应 cookies
                object responseCookies = null;
                if (easyLog.LoggingResponse?.IsIncludeCookies == true)
                    responseCookies = context.Response.Cookies;

                var f1 = GetFilter(easyLog.Filter1, url, requestHeaders, requestBody, requestCookies, responseHeader, responseBody, null); // TODO: response cookies
                var f2 = GetFilter(easyLog.Filter2, url, requestHeaders, requestBody, requestCookies, responseHeader, responseBody, null); // TODO: response cookies

                // 记录日志
                EasyLogStart.Logger.Information(
                    "easy_log_http({app},{method},{url},{requestHeaders},{requestBody},{requestCookies},{responseHeader},{responseBody},{responseCookies},{filter1},{filter2},{ip},{trace})",
                    new object[]
                    {
                        EasyLogStart.App,
                        method,
                        url,
                        requestHeaders,
                        requestBody,
                        requestCookies,
                        responseHeader,
                        responseBody,
                        responseCookies,
                        f1,
                        f2,
                        Stores.Ip,
                        Stores.GetTrace(context)
                    });
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
                    try
                    {
                        await next();
                    }
                    catch (Exception ex)
                    {
                        // TODO: error
                    }
                    memStream.Position = 0;
                    // Trace.WriteLine(memStream.Length); TODO: 限制响应文件的大小进入日志
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

        private static string GetFilter(
            IFilterGetWay filterGetWay,
            string url,
            IDictionary<string, string> requestHeaders,
            string requestBody,
            IDictionary<string, string> requestCookies,
            IDictionary<string, string> responseHeader,
            string responseBody,
            IDictionary<string, string> responseCookies
        )
        {
            if (filterGetWay == null) return null;
            if (filterGetWay is FilterGetWayString fs)
            {
                string param;
                switch (fs.FilterWay)
                {
                    case FilterGetWayStringEnum.Url: param = url; break;
                    case FilterGetWayStringEnum.RequestBody: param = requestBody; break;
                    case FilterGetWayStringEnum.ResponseBody: param = responseBody; break;
                    default: goto Error;
                }
                return fs.GetFilterFunc(param);
            }
            else if (filterGetWay is FilterGetWayDictionary fd)
            {
                IDictionary<string, string> param;
                switch (fd.FilterWay)
                {
                    case FilterGetWayDictionaryEnum.RequestHeaders: param = requestHeaders; break;
                    case FilterGetWayDictionaryEnum.RequestCookies: param = requestCookies; break;
                    case FilterGetWayDictionaryEnum.ResponseHeaders: param = responseHeader; break;
                    case FilterGetWayDictionaryEnum.ResponseCookies: param = responseCookies; break;
                    default: goto Error;
                }
                return fd.GetFilterFunc(param);
            }
        Error:
            throw new Exception("没有匹配到过滤方式");
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

        public LoggingHttpItem LoggingRequest { get; set; }

        public LoggingHttpItem LoggingResponse { get; set; }

        public IFilterGetWay Filter1 { get; set; }

        public IFilterGetWay Filter2 { get; set; }
    }

    public class LoggingHttpItem
    {
        public bool IsIncludeBody { get; set; }

        public List<string> IncludeHeaders { get; set; }

        public bool IsIncludeCookies { get; set; }
    }

    public interface IFilterGetWay
    {

    }

    public class FilterGetWayString : IFilterGetWay
    {
        /// <summary>
        /// 过滤方式 
        /// </summary>
        public FilterGetWayStringEnum FilterWay { get; set; }

        public Func<string, string> GetFilterFunc { get; set; }
    }

    public class FilterGetWayDictionary : IFilterGetWay
    {
        /// <summary>
        /// 过滤方式 
        /// </summary>
        public FilterGetWayDictionaryEnum FilterWay { get; set; }

        public Func<IDictionary<string, string>, string> GetFilterFunc { get; set; }
    }

    public enum FilterGetWayStringEnum
    {
        Url,
        RequestBody,
        ResponseBody
    }

    public enum FilterGetWayDictionaryEnum
    {
        RequestHeaders,
        RequestCookies,
        ResponseHeaders,
        ResponseCookies
    }
}