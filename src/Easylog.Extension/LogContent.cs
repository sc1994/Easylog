using System;
using System.Linq;
using System.Text;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace Easylog.Extension
{
    public static class LogContent
    {
        public static (string template, object[] @params) GetContent(
            object log,
            string stack,
            IHttpContextAccessor httpContext,
            IHostingEnvironment environment,
            string f1,
            string f2,
            string c1,
            string c2,
            string c3,
            Exception ex)
        {
            var parse = StackParse.GetStackParse(stack);

            c1 = c1 ?? parse.C1;
            c2 = c2 ?? parse.C2;
            c3 = c3 ?? parse.C3;
            var exception = ex?.ToString();
            var logStr = SerializerAdapter.Object(log);

            var tempLate = new StringBuilder("<{ip}> <{environment}> <{app}> <{trace}>\r\n");
            tempLate.AppendLine(" log_body: {log}");
            if (!string.IsNullOrWhiteSpace(f1))
                tempLate.AppendLine("  filter1: {filter1}");
            if (!string.IsNullOrWhiteSpace(f2))
                tempLate.AppendLine("  filter2: {filter2}");
            if (!string.IsNullOrWhiteSpace(c1))
                tempLate.AppendLine("category1: {category1}");
            if (!string.IsNullOrWhiteSpace(c2))
                tempLate.AppendLine("category2: {category2}");
            if (!string.IsNullOrWhiteSpace(c3))
                tempLate.AppendLine("category3: {category3}");
            if (parse.Calls?.Any() == true)
                tempLate.AppendLine("    calls: {calls}");
            if (!string.IsNullOrWhiteSpace(exception))
                tempLate.AppendLine("  exception: {exception}");

            var @params = new object[]
            {
                NetworkHelper.Ip,
                environment?.EnvironmentName ?? "DEF",
                environment?.ApplicationName ?? "DEF",
                HttpContextHelper.GetTraceGuid(httpContext),
                logStr,
                f1,
                f2,
                c1,
                c2,
                c3,
                parse.Calls,
                exception
            }
            .Where(x =>
            {
                if (x is string s && !string.IsNullOrWhiteSpace(s)) return true;
                if (x is string[] a && a?.Any() == true) return true;
                return false;
            }).ToArray();
            return (tempLate.ToString(), @params);
        }
    }
}
