using System;
using EasyLog.WriteLog;
using Microsoft.AspNetCore.Mvc;

namespace SampleWeb.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class LogController : ControllerBase
    {
        private readonly EasyLogger _log;

        public LogController(EasyLogger log)
        {
            _log = log;
        }

        [HttpGet]
        public string Get()
        {
            return DateTime.Now.ToString();
        }

        [HttpPost]
        public object Post(object a)
        {
            return new[] { a };
        }

        [HttpGet("error")]
        public void Error()
        {
            _log.Information("哈哈哈哈, 我要触发一个error");
            throw new System.Exception("测试一个异常");
        }
    }
}