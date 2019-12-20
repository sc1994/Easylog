using System;
using System.Collections.Generic;
using System.Linq;
using EasyLog.WriteLog;
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
        public WeatherForecastController(EasyLogger log, LogTestService service)
        {
            _log = log;
            _service = service;
        }

        [HttpGet]
        public IEnumerable<WeatherForecast> Get()
        {
            _service.Log();
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
            // _log.Information(request);
            return request;
        }
    }
}
