using System;

namespace Easylog.Extension
{
    public static class SerializerAdapter
    {
        public static string Object(object value)
        {
            switch (value)
            {
                case string _:
                case sbyte _:
                case short _:
                case int _:
                case long _:
                case byte _:
                case ushort _:
                case uint _:
                case ulong _:
                case char _:
                case float _:
                case double _:
                case decimal _:
                case bool _:
                case Enum _: return value.ToString();
                default: return Newtonsoft.Json.JsonConvert.SerializeObject(value);
            }
        }
    }
}
