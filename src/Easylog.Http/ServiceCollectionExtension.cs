using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Easylog.Http;
using Microsoft.AspNetCore.Http;
using static Newtonsoft.Json.JsonConvert;

namespace Microsoft.AspNetCore.Builder
{
    public static class ApplicationBuilderExtension
    {
        static EasylogHttpOptionItem RequestOption;
        static EasylogHttpOptionItem ResponseOption;

        public static void UseMiddlewareEasylogger(this IApplicationBuilder app, Action<EasylogHttpOption> options)
        {

        }


        private static string GetHost(HttpRequest request)
            => request.Scheme + "://" + request.Host;

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
            if (RequestOption == null || RequestOption.BodyLimit == 0) return null;
            if (request?.Body == null) return null;
            try
            {
                var contentLength = int.MaxValue;
                if (RequestOption.BodyLimit != -1
                    && int.TryParse(request.Headers["Content-Length"], out contentLength)
                    && contentLength > RequestOption.BodyLimit) // 拦截掉过大的请求体
                    return null;
                request.EnableBuffering(contentLength, contentLength);
                using (var reader = new StreamReader(request.Body,
                                                     Encoding.UTF8,
                                                     false,
                                                     contentLength,
                                                     leaveOpen: true))
                {
                    var r = await reader.ReadToEndAsync();
                    request.Body.Position = 0; // 重置到起点 , 被这个坑了好久
                    return r;
                }
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
                if (RequestOption?.HasCookies == true)
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
                if (ResponseOption != null && ResponseOption.BodyLimit != 0)
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
                        if (memStream.Length <= ResponseOption.BodyLimit || ResponseOption.BodyLimit == -1)
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
                if (ResponseOption?.HasCookies == true)
                    return DeserializeObject<IDictionary<string, string>>(SerializeObject(response.Cookies));
                return null;
            }
            catch (Exception ex)
            {
                throw new EasyLogHttpException("获取 ResponseCookies 发生异常: " + ex.Message, false, ex);
            }
        }

        private static string GetFilter(
            GetFilterField filterField,
            string url,
            QueryString queryString,
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
                if (filterField == null) return null;

                switch (filterField)
                {
                    case GetFilterFieldByQueryString s:
                        return s.GetFilterFieldFunc(queryString);
                    case GetFilterFieldByRequestHeaders reqH:
                        return reqH.GetFilterFieldFunc(requestHeaders);
                    case GetFilterFieldByRequestBody reqB:
                        return reqB.GetFilterFieldFunc(requestBody);
                    case GetFilterFieldByResponseBody resB:
                        return resB.GetFilterFieldFunc(responseBody);
                    case GetFilterFieldByUrl u:
                        return u.GetFilterFieldFunc(url); // TODO 还需补充
                    default: throw new Exception("没有匹配到相关的过滤字段获取方式");
                }
            }
            catch (Exception ex)
            {
                throw new EasyLogHttpException("获取 RequestCookies 发生异常: " + ex.Message, false, ex);
            }
        }
    }
}