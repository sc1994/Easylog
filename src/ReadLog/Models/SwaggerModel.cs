using System;
using System.Collections.Generic;
using System.IO;
using Flurl.Http;
using static Newtonsoft.Json.JsonConvert;

namespace ReadLog.Models
{
    public class SwaggerModel
    {
        public Dictionary<string, SwaggerPaths> Paths { get; set; }
    }

    public class SwaggerPaths
    {
        public SwaggerPost Post { get; set; }
    }

    public class SwaggerPost
    {
        public string Summary { get; set; }
    }

    public static class SwaggerStorage
    {
        public static Dictionary<string, List<string>> Poll = new Dictionary<string, List<string>>();

        public static Dictionary<string, List<string>> Game = new Dictionary<string, List<string>>();

        private static readonly object _lock = new object();

        public static void ToPoll()
        {
            To("http://10.101.72.28:6303", "/swagger/v1/swagger.json", Poll);
        }

        public static void ToGame()
        {
            To("http://10.101.72.28:6300", "/tccxgc/surpriseapi/swagger/docs/v1", Game, "/tccxgc/surpriseapi");
        }

        private static void To(string baseUrl, string path, Dictionary<string, List<string>> souce, string secondPath = "")
        {
            lock (baseUrl + path)
            {
                var str = (baseUrl + path)
                        .GetAsync().Result.Content
                        .ReadAsStringAsync().Result;
                var model = DeserializeObject<SwaggerModel>(str);
                souce.Clear();
                foreach (var item in model.Paths)
                {
                    try
                    {
                        if (item.Value?.Post?.Summary == null) continue;
                        var value = baseUrl + secondPath + item.Key;
                        if (Poll.TryGetValue(item.Value.Post.Summary, out var v))
                            v.Add(value);
                        else
                            souce.Add(item.Value.Post.Summary, new List<string> { value });
                    }
                    catch (Exception ex)
                    {
                        if (!Directory.Exists("logs"))
                            Directory.CreateDirectory("logs");
                        File.WriteAllLines($"logs/{DateTime.Now:yyyyMMdd}.log", new[] { ex.ToString() });
                    }
                }
            }
        }
    }
}