namespace Easylog.Http
{
    /// <summary>
    /// 设置哪些内容可以进入
    /// </summary>
    public class EasylogHttpOption
    {
        public EasylogHttpOptionItem RequestOption { get; set; }
        public EasylogHttpOptionItem ResponseOption { get; set; }
    }

    public class EasylogHttpOptionItem
    {
        /// <summary>
        /// 限制 body 体积, 大于限制的body将不会被记录
        /// <para>-1 不限制</para>
        /// <para> 0 恒限制</para>
        /// </summary>
        public int BodyLimit { get; set; }

        public string[] HasHeaders { get; set; }

        public bool HasCookies { get; set; }
    }
}