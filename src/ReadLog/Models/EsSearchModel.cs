
using System.Collections.Generic;
namespace ReadLog.Models.EsSearchModel
{
    using System;

    using System.Globalization;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    public partial class EsSearchModel
    {
        [JsonProperty("query")]
        public Query Query { get; set; }

        [JsonProperty("from")]
        public long From { get; set; }

        [JsonProperty("size")]
        public long Size { get; set; }

        [JsonProperty("sort", NullValueHandling = NullValueHandling.Ignore)]
        public Sort[] Sort { get; set; }

        [JsonProperty("highlight", NullValueHandling = NullValueHandling.Ignore)]
        public Highlight Highlight { get; set; }

        public static EsSearchModel FromJson(string json) => JsonConvert.DeserializeObject<EsSearchModel>(json, ReadLog.Models.EsSearchModel.Converter.Settings);
    }

    public partial class Highlight
    {
        [JsonProperty("fields", NullValueHandling = NullValueHandling.Ignore)]
        public HighlightFields Fields { get; set; }
    }

    public partial class Sort
    {
        [JsonProperty("@timestamp", NullValueHandling = NullValueHandling.Ignore)]
        public Timestamp Timestamp { get; set; }
    }

    public partial class Query
    {
        [JsonProperty("bool")]
        public Bool Bool { get; set; }
    }

    public partial class Bool
    {
        [JsonProperty("must")]
        public List<Must> Must { get; set; }
    }

    public partial class Must
    {
        [JsonProperty("range", NullValueHandling = NullValueHandling.Ignore)]
        public Range Range { get; set; }

        [JsonProperty("term", NullValueHandling = NullValueHandling.Ignore)]
        public Term Term { get; set; }

        [JsonProperty("match", NullValueHandling = NullValueHandling.Ignore)]
        public Match Match { get; set; }
    }

    public class Match
    {
        [JsonProperty("fields.filter1.keyword", NullValueHandling = NullValueHandling.Ignore)]
        public string FieldsFilter1Keyword { get; set; }
        [JsonProperty("fields.filter2.keyword", NullValueHandling = NullValueHandling.Ignore)]
        public string FieldsFilter2Keyword { get; set; }
        [JsonProperty("fields.msg", NullValueHandling = NullValueHandling.Ignore)]
        public string FieldsMsgKeyword { get; set; }
    }

    public partial class Range
    {
        [JsonProperty("@timestamp")]
        public Timestamp Timestamp { get; set; }
    }

    public partial class Timestamp
    {
        [JsonProperty("gt", NullValueHandling = NullValueHandling.Ignore)]
        public DateTimeOffset? Gt { get; set; }

        [JsonProperty("lt", NullValueHandling = NullValueHandling.Ignore)]
        public DateTimeOffset? Lt { get; set; }

        [JsonProperty("order", NullValueHandling = NullValueHandling.Ignore)]
        public string Order { get; set; }
    }

    public partial class Term
    {
        [JsonProperty("level.keyword", NullValueHandling = NullValueHandling.Ignore)]
        public string LevelKeyword { get; set; }
        [JsonProperty("fields.module.keyword", NullValueHandling = NullValueHandling.Ignore)]
        public string FieldsModuleKeyword { get; set; }
        [JsonProperty("fields.category.keyword", NullValueHandling = NullValueHandling.Ignore)]
        public string FieldsCategoryKeyword { get; set; }
        [JsonProperty("fields.sub_category.keyword", NullValueHandling = NullValueHandling.Ignore)]
        public string FieldsSubCategoryKeyword { get; set; }
        [JsonProperty("fields.ip.keyword", NullValueHandling = NullValueHandling.Ignore)]
        public string FieldsIpKeyword { get; set; }
        [JsonProperty("fields.app.keyword", NullValueHandling = NullValueHandling.Ignore)]
        public string FieldsAppKeyword { get; set; }
    }

    public class HighlightFields
    {
        [JsonProperty("fields.msg", NullValueHandling = NullValueHandling.Ignore)]
        public object FieldsMsg { get; set; } // todo 设置项
    }

    internal static class Converter
    {
        public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
            DateParseHandling = DateParseHandling.None,
            Converters = {
                new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal }
            },
        };
    }
}