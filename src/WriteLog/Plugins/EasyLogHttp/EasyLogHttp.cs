using System;
using Microsoft.AspNetCore.Builder;
using System.IO;
using Microsoft.AspNetCore.Http;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using static Newtonsoft.Json.JsonConvert;

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
                string exception = null;
                string method = null, url = null, requestBody = null, responseBody = null, filter1 = null, filter2 = null;
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
                    responseCookies = GetResponseCookies(context.Response); // TODO 响应 cookies  响应cookies类型有问题
                    filter1 = GetFilter(easyLog.Filter1, url, requestHeaders, requestBody, requestCookies, responseHeader, responseBody, null); // TODO response cookies
                    filter2 = GetFilter(easyLog.Filter2, url, requestHeaders, requestBody, requestCookies, responseHeader, responseBody, null); // TODO response cookies
                }
                catch (EasyLogHttpException ex)
                {
                    exception = ex.ToString();
                    if (ex.HasNext)
                        await next();
                }
                catch (Exception ex)
                {
                    exception = ex.ToString();
                    throw;
                }
                finally
                {
                    var template = "<{ip}> <{environment}> <{app}> <{method}> <{url}> <{trace}>";
                    if (requestHeaders?.Any() == true) template += "\r\nRequestHeaders : {requestHeaders}";
                    if (!string.IsNullOrWhiteSpace(requestBody)) template += "\r\nRequestBody    : {requestBody}";
                    if (requestCookies?.Any() == true) template += "\r\nRequestCookies : {requestCookies}";
                    if (responseHeader?.Any() == true) template += "\r\nResponseHeader : {responseHeader}";
                    if (!string.IsNullOrWhiteSpace(responseBody)) template += "\r\nResponseBody   : {responseBody}";
                    if (responseCookies?.Any() == true) template += "\r\nResponseCookies: {responseCookies}";
                    if (!string.IsNullOrWhiteSpace(filter1)) template += "\r\nFilter1        : {filter1}";
                    if (!string.IsNullOrWhiteSpace(filter2)) template += "\r\nFilter2        : {filter2}";
                    if (!string.IsNullOrWhiteSpace(exception)) template += "\r\nException      : {exception}";
                    template = template.TrimEnd(',');
                    var @params = new object[]
                        {
                            Stores.Ip,
                            Stores.EnvironmentName,
                            EasyLogStart.App,
                            method,
                            url,
                            Stores.GetTrace(context),
                            requestHeaders,
                            requestBody,
                            requestCookies,
                            responseHeader,
                            responseBody,
                            responseCookies,
                            filter1,
                            filter2,
                            exception
                        }.Where(x =>
                        {
                            if (x is string s)
                                return !string.IsNullOrWhiteSpace(s);
                            else if (x is IDictionary<string, string> d)
                                return d?.Any() == true;
                            return false;
                        }).ToArray();

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
            try
            {
                if (RequestOption.HasHeaders?.Length > 0)
                    return request.Headers
                                  .Where(x => RequestOption.HasHeaders.Any(a => a == x.Key.ToLower()))
                                  .ToDictionary(x => x.Key, x => x.Value.ToString());
                return null;
            }
            catch (Exception ex)
            {
                throw new EasyLogHttpException("获取 RequestHeader 发生异常: " + ex.Message, true, ex);
            }
        }

        /// <summary>
        /// 参考 https://stackoverflow.com/questions/43403941/how-to-read-asp-net-core-response-body
        /// </summary>
        private static async Task<string> GetRequestBodyAsync(HttpRequest request)
        {
            try
            {
                if (RequestOption?.IsHasBody == true)
                {
                    if (request?.Body == null) return null;
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
                        var r = await reader.ReadToEndAsync();
                        request.Body.Position = 0; // 重置到起点 , 被这个坑了好久
                        return r;
                    }
                }
                return null;
            }
            catch (Exception ex)
            {
                throw new EasyLogHttpException("获取 RequestBody 发生异常: " + ex.Message, true, ex);
            }
        }

        private static IDictionary<string, string> GetRequestCookies(HttpRequest request)
        {
            try
            {
                if (RequestOption?.IsHasCookies == true)
                    return request.Cookies.ToDictionary(x => x.Key, x => x.Value);
                return null;
            }
            catch (Exception ex)
            {
                throw new EasyLogHttpException("获取 RequestCookies 发生异常: " + ex.Message, true, ex);
            }
        }

        /// <summary>
        /// 参考 https://stackoverflow.com/questions/43403941/how-to-read-asp-net-core-response-body
        /// </summary>
        private static async Task<string> GetResponseBodyAsync(HttpResponse response, Func<Task> next)
        {
            var nexted = false; // next 之后
            var needNext = true;
            try
            {
                string responseBody = null;
                if (ResponseOption?.IsHasBody == true)
                {
                    if (response?.Body == null) return null;
                    var originalBody = response.Body;
                    using (var memStream = new MemoryStream())
                    {
                        response.Body = memStream;
                        needNext = false;
                        await next();
                        nexted = true;
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
                    needNext = false;
                    await next();
                    nexted = true;
                }
                return responseBody;
            }
            catch (Exception ex)
            {
                if (nexted || needNext) // next 执行之后的异常(我的读取body代码异常), 直接抛出
                    throw new EasyLogHttpException("获取 ResponseBody 发生异常: " + ex.Message, needNext, ex);
                throw ex; // 将next中发生的异常向外抛出, 外层捕获并且在日志中呈现
            }
        }

        private static IDictionary<string, string> GetResponseHeaders(HttpResponse response)
        {
            try
            {
                // 响应 headers
                if (ResponseOption?.HasHeaders?.Length > 0)
                    return response.Headers
                                   .Where(x => ResponseOption.HasHeaders.Any(a => a == x.Key.ToLower()))
                                   .ToDictionary(x => x.Key, x => x.Value.ToString());
                return null;
            }
            catch (Exception ex)
            {
                throw new EasyLogHttpException("获取 ResponseHeaders 发生异常: " + ex.Message, false, ex);
            }
        }

        private static IDictionary<string, string> GetResponseCookies(HttpResponse response)
        {
            try
            {
                if (ResponseOption?.IsHasCookies == true)
                    return DeserializeObject<IDictionary<string, string>>(SerializeObject(response.Cookies));
                return null;
            }
            catch (Exception ex)
            {
                throw new EasyLogHttpException("获取 ResponseCookies 发生异常: " + ex.Message, false, ex);
            }
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
            try
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
            }
            catch (Exception ex)
            {
                throw new EasyLogHttpException("获取 RequestCookies 发生异常: " + ex.Message, false, ex);
            }
        Error:
            throw new Exception("没有匹配到过滤方式");
        }

        private static void LogError(object msg)
        {

        }
    }
}