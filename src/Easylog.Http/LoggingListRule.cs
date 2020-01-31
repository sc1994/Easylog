using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;

namespace Easylog.Http
{
    /// <summary>
    /// 记录规则 (黑白名单)
    /// </summary>
    public abstract class LoggingListRule
    {

    }

    /// <summary>
    /// 来自 Url
    /// </summary>
    public class LoggingListRuleByUrl : LoggingListRule
    {
        public Func<string, bool> GetLoggingListRuleFunc { get; set; }
    }

    /// <summary>
    /// 来自 QueryString
    /// </summary>
    public class LoggingListRuleByQueryString : LoggingListRule
    {
        public Func<QueryString, bool> GetLoggingListRuleFunc { get; set; }
    }

    /// <summary>
    /// 来自 RequestHeaders
    /// </summary>
    public class LoggingListRuleByRequestHeaders : LoggingListRule
    {
        public Func<IDictionary<string, string>, bool> GetLoggingListRuleFunc { get; set; }
    }

    /// <summary>
    /// 来自 RequestBody
    /// </summary>
    public class LoggingListRuleByRequestBody : LoggingListRule
    {
        public Func<string, bool> GetLoggingListRuleFunc { get; set; }
    }

    /// <summary>
    /// 来自 RequestBody
    /// </summary>
    public class LoggingListRuleByRequestBody<TRequestBody> : LoggingListRule
    {
        public Func<TRequestBody, bool> GetLoggingListRuleFunc { get; set; }
    }

    /// <summary>
    /// 来自 ResponseBody
    /// </summary>
    public class LoggingListRuleByResponseBody : LoggingListRule
    {
        public Func<string, bool> GetLoggingListRuleFunc { get; set; }
    }

    /// <summary>
    /// 来自 ResponseBody
    /// </summary>
    public class LoggingListRuleByResponseBody<TResponseBody> : LoggingListRule
    {
        public Func<TResponseBody, bool> GetLoggingListRuleFunc { get; set; }
    }

    public enum ListLogTypeEnum
    {
        Blacklist,
        Whitelist
    }
}