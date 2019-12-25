using System;
using System.Collections.Generic;
using System.Linq;
using EasyLog.WriteLog;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using SampleWeb.Services;

namespace SampleWeb.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly EasyLogger _log;
        private readonly LogTestService _service;
        private readonly IWebHostEnvironment _env;
        public WeatherForecastController(EasyLogger log, LogTestService service, IWebHostEnvironment env)
        {
            _log = log;
            _service = service;
            _env = env;
        }

        [HttpGet]
        public IEnumerable<WeatherForecast> Get()
        {
            // _service.Log();
            // _log.Information("just information", filter1: "testF1");
            var rng = new Random();
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = rng.Next(-20, 55),
                Summary = Summaries[rng.Next(Summaries.Length)]
            })
            .ToArray();
        }

        [HttpPost]
        public object Post(object request)
        {
            throw new Exception("asdasds");
            // _log.Information(request);
            return request;
        }

        [HttpGet("env")]
        public object GetEnv()
        {
            return _env.EnvironmentName;
        }
    }
}
