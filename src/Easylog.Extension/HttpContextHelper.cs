using System;
using Microsoft.AspNetCore.Http;

namespace Easylog.Extension
{
    internal static class HttpContextHelper
    {
        internal static string GetTraceGuid(IHttpContextAccessor httpContext)
        {
            if (httpContext == null) return "00000000-0000-0000-0000-000000000000";

            return (string)httpContext.HttpContext.Items["EasyLog.Extension.HttpContextHelper.GetTraceGuid"]
                ?? (string)(httpContext.HttpContext.Items["EasyLog.Extension.HttpContextHelper.GetTraceGuid"] = Guid.NewGuid().ToString());
        }
    }
}
