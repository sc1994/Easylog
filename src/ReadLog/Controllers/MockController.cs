using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Flurl.Http;
using Microsoft.AspNetCore.Mvc;
using ReadLog.Models;

namespace ReadLog.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MockController : ControllerBase
    {
        [HttpGet("apis/{sub}")]
        public IEnumerable<dynamic> GetApis(string sub)
        {
            var result = new List<string>();
            result.AddRange(SwaggerStorage.Poll.Keys.Where(x => x.ToLower().Contains(sub.ToLower()))
                                          .SelectMany(x =>
                                          {
                                              if (SwaggerStorage.Poll.TryGetValue(sub, out var v)) return v;
                                              return null;
                                          }).Where(x => x != null));
            result.AddRange(SwaggerStorage.Game.Keys.Where(x => x.ToLower().Contains(sub.ToLower()))
                                          .SelectMany(x =>
                                          {
                                              if (SwaggerStorage.Game.TryGetValue(sub, out var v)) return v;
                                              return null;
                                          }).Where(x => x != null));
            return result.Select(x => new { url = x, body = "", response = "" });
        }

        [HttpGet]
        public async Task<string> Mock(string url, string body)
        {
            var res = await url
                .WithHeader("Content-Type", "application/json")
                .PostStringAsync(body);

            return await res.Content.ReadAsStringAsync();
        }

        [HttpGet("allurls")]
        public IEnumerable<string> AllUrls()
        {
            var result = new List<string>();
            IEnumerable<string> Format(KeyValuePair<string, List<string>> param)
            {
                return param.Value.Select(x => $"{x}-->{param.Key}");
            }
            result.AddRange(SwaggerStorage.Poll.SelectMany(Format));
            result.AddRange(SwaggerStorage.Game.SelectMany(Format));
            return result;
        }

        [HttpGet("ref")]
        public bool Ref()
        {
            Parallel.Invoke(
                SwaggerStorage.ToPoll,
                SwaggerStorage.ToGame
            );
            return true;
        }

        [HttpGet("debug")]
        public dynamic Debug()
        {
            return new
            {
                SwaggerStorage.Poll,
                SwaggerStorage.Game
            };
        }
    }
}