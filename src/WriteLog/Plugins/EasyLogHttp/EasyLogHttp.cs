using System;
using Microsoft.AspNetCore.Builder;
using System.IO;
using Microsoft.AspNetCore.Http;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;

namespace EasyLog.WriteLog
{
    public static class EasyLogHttp
    {
        internal static HttpOptionItem RequestOption { get; private set; }

        internal static HttpOptionItem ResponseOption { get; private set; }

        /// <summary>
        /// 启用 EasyLog 记录 Http 内容
        /// </summary>
        /// <param name="app"></param>
        /// <param name="options"></param>
        public static void UseEasyLogHttp(this IApplicationBuilder app, Action<HttpOption> options)
        {
            if (app is null) throw new ArgumentNullException(nameof(app));
            if (options is null) throw new ArgumentNullException(nameof(options));

            var easyLog = new HttpOption();
            options(easyLog);

            if (easyLog.RequestOption?.HasHeaders?.Length > 0) // 提前处理数据小写
                easyLog.RequestOption.HasHeaders = easyLog.RequestOption.HasHeaders.Select(x => x.ToLower()).ToArray();
            if (easyLog.ResponseOption?.HasHeaders?.Length > 0) // 提前处理数据小写
                easyLog.ResponseOption.HasHeaders = easyLog.ResponseOption.HasHeaders.Select(x => x.ToLower()).ToArray();
            RequestOption = easyLog.RequestOption;
            ResponseOption = easyLog.ResponseOption;

            app.Use(async (context, next) => // 添加管道 , 处理request,response 
            {
                Exception exception = null;
                string method = null, url = null, requestBody = null, responseBody = null, f1 = null, f2 = null;
                IDictionary<string, string> requestHeaders = null, requestCookies = null, responseHeader = null, responseCookies = null;
                try
                {
                    method = context.Request.Method;
                    url = GetUrl(context.Request); // 拼接请求 url
                    requestHeaders = GetRequestHeaders(context.Request); // 请求 headers
                    requestBody = await GetRequestBodyAsync(context.Request); // 请求 body
                    requestCookies = GetRequestCookies(context.Request); // 请求 cookies
                    responseBody = await GetResponseBodyAsync(context.Response, next);  // 响应 body
                    responseHeader = GetResponseHeaders(context.Response); // 响应 headers (必须在执行 next 之后才有这个内容)
                                                                           // TODO 响应 cookies  响应cookies类型有问题
                    f1 = GetFilter(easyLog.Filter1, url, requestHeaders, requestBody, requestCookies, responseHeader, responseBody, null); // TODO: response cookies
                    f2 = GetFilter(easyLog.Filter2, url, requestHeaders, requestBody, requestCookies, responseHeader, responseBody, null); // TODO: response cookies
                }
                catch (Exception ex)
                {
                    // TODO 异常记录和异常抛出 , 目前整体抛出可能需要优化异常细节
                    exception = ex;
                    throw;
                }
                finally
                {
                    var template = "easy_log_http({app},{method},{url},{requestHeaders},{requestBody},{requestCookies},{responseHeader},{responseBody},{responseCookies},{filter1},{filter2},{ip},{trace},{ex})";
                    var @params = new object[]
                        {
                            EasyLogStart.App,
                            method,
                            url,
                            requestHeaders,
                            requestBody,
                            requestCookies,
                            responseHeader,
                            responseBody,
                            null, // responseCookies
                            f1,
                            f2,
                            Stores.Ip,
                            Stores.GetTrace(context),
                            exception
                        };

                    if (exception == null)
                        EasyLogStart.Logger.Information(template, @params);
                    else
                        EasyLogStart.Logger.Error(template, @params);
                }
            });
        }


        private static string GetUrl(HttpRequest request)
            => request.Scheme + "://" + request.Host + request.Path.Value + request.QueryString.Value;

        private static IDictionary<string, string> GetRequestHeaders(HttpRequest request)
        {
            if (RequestOption.HasHeaders?.Length > 0)
                return request.Headers
                              .Where(x => RequestOption.HasHeaders.Any(a => a == x.Key.ToLower()))
                              .ToDictionary(x => x.Key, x => x.Value.ToString());
            return null;
        }

        /// <summary>
        /// 参考 https://stackoverflow.com/questions/43403941/how-to-read-asp-net-core-response-body
        /// </summary>
        private static async Task<string> GetRequestBodyAsync(HttpRequest request)
        {
            if (RequestOption?.IsHasBody == true)
            {
                if (int.TryParse(request.Headers["Content-Length"], out var contentLength)
                    && contentLength > RequestOption.BodySizeLimit) // 拦截掉过大的请求体
                    return null;
                request.EnableBuffering(RequestOption.BodySizeLimit, RequestOption.BodySizeLimit);
                using (var reader = new StreamReader(request.Body,
                                                     Encoding.UTF8,
                                                     false,
                                                     RequestOption.BodySizeLimit,
                                                     leaveOpen: true))
                {
                    throw new Exception("kkkkk");
                    var r = await reader.ReadToEndAsync();
                    request.Body.Position = 0; // 重置到起点 , 被这个坑了好久
                    return r;
                }
            }
            return null;
        }

        private static IDictionary<string, string> GetRequestCookies(HttpRequest request)
        {
            if (RequestOption?.IsHasCookies == true)
                return request.Cookies.ToDictionary(x => x.Key, x => x.Value);
            return null;
        }

        /// <summary>
        /// 参考 https://stackoverflow.com/questions/43403941/how-to-read-asp-net-core-response-body
        /// </summary>
        private static async Task<string> GetResponseBodyAsync(HttpResponse response, Func<Task> next)
        {
            string responseBody = null;
            if (ResponseOption?.IsHasBody == true)
            {
                var originalBody = response.Body;
                using (var memStream = new MemoryStream())
                {
                    response.Body = memStream;
                    await next();

                    memStream.Position = 0;
                    if (memStream.Length <= ResponseOption.BodySizeLimit)
                    {
                        responseBody = new StreamReader(memStream).ReadToEnd();
                        memStream.Position = 0;
                    }
                    await memStream.CopyToAsync(originalBody);
                    response.Body = originalBody;
                }
            }
            else
            {
                await next();
                foreach (var item in response.Headers)
                {
                    (item.Key + ": " + item.Value).WriteLine();
                }
            }
            return responseBody;
        }

        private static IDictionary<string, string> GetResponseHeaders(HttpResponse response)
        {
            // 响应 headers
            if (ResponseOption?.HasHeaders?.Length > 0)
                return response.Headers
                               .Where(x => ResponseOption.HasHeaders.Any(a => a == x.Key.ToLower()))
                               .ToDictionary(x => x.Key, x => x.Value.ToString());
            return null;
        }

        private static IDictionary<string, string> GetResponseCookies(HttpResponse request, HttpOption option)
        {
            throw new NotImplementedException();
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
            if (filterGetWay is FilterGetWayByString fs)
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
            else if (filterGetWay is FilterGetWayByDictionary fd)
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

        private static void LogError(object msg)
        {

        }
    }
}