using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;

namespace Easylog.Http
{
    /// <summary>
    /// 过滤字段 (日志字段)
    /// </summary>
    public abstract class GetFilterField
    {
        
    }

    /// <summary>
    /// 来自 Url
    /// </summary>
    public class GetFilterFieldByUrl : GetFilterField
    {
        public Func<string, string> GetFilterFieldFunc { get; set; }
    }

    /// <summary>
    /// 来自 QueryString
    /// </summary>
    public class GetFilterFieldByQueryString : GetFilterField
    {
        public Func<QueryString, string> GetFilterFieldFunc { get; set; }
    }

    /// <summary>
    /// 来自 RequestHeaders
    /// </summary>
    public class GetFilterFieldByRequestHeaders : GetFilterField
    {
        public Func<IDictionary<string, string>, string> GetFilterFieldFunc { get; set; }
    }

    /// <summary>
    /// 来自 RequestBody
    /// </summary>
    public class GetFilterFieldByRequestBody : GetFilterField
    {
        public Func<string, string> GetFilterFieldFunc { get; set; }
    }

    /// <summary>
    /// 来自 ResponseBody
    /// </summary>
    public class GetFilterFieldByResponseBody : GetFilterField
    {
        public Func<string, string> GetFilterFieldFunc { get; set; }
    }
}