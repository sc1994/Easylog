using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Easylog.Extension;
using Easylog.Http;
using static Easylog.Http.LogHttpContent;

namespace Microsoft.AspNetCore.Builder
{
    public static class ApplicationBuilderExtension
    {
        public static void UseMiddlewareEasylogger(
            this IApplicationBuilder app,
            Func<EasylogHttpOption> options)
        {
            var optionsContent = options();
            RequestOption = optionsContent.RequestOption;
            ResponseOption = optionsContent.ResponseOption;
            Filter1 = optionsContent.Filter1;
            Filter2 = optionsContent.Filter2;

            app.Use(async (context, next) =>
            {
                var url = GetHost(context.Request);
                var reqH = GetRequestHeaders(context.Request);
                var reqB = await GetRequestBodyAsync(context.Request);
                var reqC = GetRequestCookies(context.Request);

                var resH = GetResponseHeaders(context.Response);
                var resB = await GetResponseBodyAsync(context.Response, next);
                var resC = GetResponseCookies(context.Response);

                // TODO 黑名单

                var f1 = GetFilter(Filter1, url, reqH, reqB, reqC, resH, resB, resC);
                var f2 = GetFilter(Filter2, url, reqH, reqB, reqC, resH, resB, resC);

// context. TODO:
                var param = new List<object> { NetworkHelper.Ip, "TODO", "TODO", "TODO" };
                var template = new StringBuilder("<{ip}> <{environment}> <{app}> <{trace}>\r\n");
                template.AppendLine("             url: {url}"); param.Add(url);
                if (!string.IsNullOrWhiteSpace(f1))
                    template.AppendLine("         filter1: {f1}"); param.Add(f1);
                if (!string.IsNullOrWhiteSpace(f2))
                    template.AppendLine("         filter2: {f2}"); param.Add(f2);
                if (reqH?.Any() == true)
                    template.AppendLine(" request_headers: {reqH}"); param.Add(reqH);
                if (!string.IsNullOrWhiteSpace(reqB))
                    template.AppendLine("    request_body: {reqB}"); param.Add(reqB);
                template.AppendLine(" request_cooikes: {reqC}"); param.Add(reqC);
                if (resH?.Any() == true)
                    template.AppendLine("response_headers: {resH}"); param.Add(resH);
                if (!string.IsNullOrWhiteSpace(resB))
                    template.AppendLine("   response_body: {resB}"); param.Add(resB);
                template.AppendLine("response_cooikes: {resC}"); param.Add(resC);



            });

        }
    }
}