using ReadLog.Models.EsSearchModel;
using ReadLog.Models.FiltrateModel;
using Newtonsoft.Json;
using ReadLog.Models.AggregationModel;

namespace ReadLog.Utility
{
    public static class Serialize
    {
        public static string ToJson(this FiltrateModel self) => JsonConvert.SerializeObject(self, ReadLog.Models.FiltrateModel.Converter.Settings);

        public static string ToJson(this EsSearchModel self) => JsonConvert.SerializeObject(self, ReadLog.Models.EsSearchModel.Converter.Settings);

        public static string ToJson(this AggregationModel self) => JsonConvert.SerializeObject(self, ReadLog.Models.AggregationModel.Converter.Settings);
    }
}