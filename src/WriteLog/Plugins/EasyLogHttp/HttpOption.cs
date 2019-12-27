namespace EasyLog.WriteLog
{
    public class HttpOption
    {
        internal HttpOption() { }

        public HttpOptionItem RequestOption { get; set; }

        public HttpOptionItem ResponseOption { get; set; }

        /// <summary>
        /// 过滤字段 (在请求或者响应中获取属性值作为过滤条件以供日志搜索和定位)
        ///     
        /// /// </summary>
        public IFilterGetWay Filter1 { get; set; } // TODO 命名还要推敲

        public IFilterGetWay Filter2 { get; set; } // TODO 命名还要推敲

        /// <summary>
        /// 黑名单(满足条件的数据将不会记录)
        ///     比如: 1. 文件下载接口,可能会记录一些无意义的二进制字节
        ///           2. 开放的日志捕获接口, 防止记录双份日志
        /// </summary>
        /// <value></value>
        public IFilterGetWay Blacklist { get; set; } // TODO 扩展出 bool 委托

        /// <summary>
        /// 白名单 (只有满足条件的http才会被记录)
        ///     比如: 1. 重要的数据插入接口, 方便在数据插入失败后补数据.
        /// </summary>
        /// <value></value>
        public IFilterGetWay Whitelist { get; set; }
    }

    public class HttpOptionItem
    {
        public bool IsHasBody { get; set; } // TODO 不是简单的一句话,而是配置一系列的规则

        /// <summary>
        /// [必填]  body size 限制, 防止过大的数据进入日志, 浪费资源
        /// </summary>
        public int BodySizeLimit { get; set; } = -1;

        public string[] HasHeaders { get; set; }

        public bool IsHasCookies { get; set; }
    }
}