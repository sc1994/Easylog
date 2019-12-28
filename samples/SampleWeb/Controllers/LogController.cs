using System;
using Microsoft.AspNetCore.Mvc;

namespace SampleWeb.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class LogController : ControllerBase
    {
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
            throw new System.Exception("测试一个异常");
        }
    }
}