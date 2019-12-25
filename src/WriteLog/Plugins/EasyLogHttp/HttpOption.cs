using System.Collections.Generic;

namespace EasyLog.WriteLog
{
    public class HttpOption
    {
        internal HttpOption() { }

        public HttpOptionItem RequestOption { get; set; }

        public HttpOptionItem ResponseOption { get; set; }

        public IFilterGetWay Filter1 { get; set; }

        public IFilterGetWay Filter2 { get; set; }
    }

    public class HttpOptionItem
    {
        public bool IsHasBody { get; set; }

        /// <summary>
        /// [必填]  body size 限制, 防止过大的数据进入日志, 浪费资源
        /// </summary>
        public int BodySizeLimit { get; set; } = -1;

        public string[] HasHeaders { get; set; }

        public bool IsHasCookies { get; set; }
    }
}