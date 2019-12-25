using System.Collections.Generic;
using Newtonsoft.Json;
using Xunit;
using Xunit.Abstractions;
using static Newtonsoft.Json.JsonConvert;

namespace ExtendTest
{
    public class ObjectToDictionary_Test : BaseTest
    {
        public ObjectToDictionary_Test(ITestOutputHelper output) : base(output) { }

        [Fact]
        public void Layers_Test()
        {
            var l = new LayersModel
            {
                A = "a",
                OneLayer = new OneLayer
                {
                    B = "b",
                    Underlying = new Underlying
                    {
                        C = "c"
                    }
                }
            }.ToDictionary();

            Output.WriteLine(SerializeObject(l, Formatting.Indented));
        }
    }

    static class ObjectToDictionary
    {
        public static IDictionary<string, object> ToDictionary<T>(this T source)
            where T : class
        {
            return DeserializeObject<Dictionary<string, object>>(SerializeObject(source));
            // var a = typeof (KeyValuePair<,>);

            // var type = source.GetType();
            // var r = type.GetProperties().Select(key =>
            // {
            //     var value = key.GetValue(source);
            //     if (value is string)
            //         return (key: key.Name, value);
            //     else
            //         return (key: key.Name, value: value.ToDictionary());
            // }).ToDictionary(x => x.key, x => x.value);

            // return r;
        }
    }

    class LayersModel
    {
        public string A { get; set; }

        public OneLayer OneLayer { get; set; }
    }

    class OneLayer
    {
        public string B { get; set; }

        public Underlying Underlying { get; set; }
    }

    class Underlying
    {
        public string C { get; set; }
    }
}
