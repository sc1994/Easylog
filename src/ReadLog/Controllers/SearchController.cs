using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Flurl.Http;
using ReadLog.Models.EsSearchModel;
using ReadLog.Models.FiltrateModel;
using ReadLog.Utility;
using Microsoft.AspNetCore.Mvc;
using static Newtonsoft.Json.JsonConvert;
using ReadLog.Models.AggregationModel;

namespace ReadLog.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SearchController : ControllerBase
    {
        private const string EsNode = "http://localhost:9222";

        /// <summary>
        /// 获取聚合数据
        /// </summary>
        /// <returns></returns>
        [HttpGet("aggregation")]
        public async Task<object> GetAggregation()
        {
            var res = await (EsNode + "/logstash*/_search")
                .WithHeader("Content-Type", "application/json")
                .PostStringAsync("{\"aggs\":{\"apps\":{\"terms\":{\"field\":\"fields.app.keyword\"},\"aggs\":{\"modules\":{\"terms\":{\"field\":\"fields.module.keyword\"},\"aggs\":{\"categorys\":{\"terms\":{\"field\":\"fields.category.keyword\"},\"aggs\":{\"sub_categorys\":{\"terms\":{\"field\":\"fields.sub_category.keyword\"}}}}}}}}},\"size\":0}");
            var json = await res.Content.ReadAsStringAsync();

            var agg = DeserializeObject<AggregationModel>(json);
            var result =
                agg.Aggregations.Apps?.Buckets.Select(app => new
                {
                    Label = $"{app.Key}({GetCount(app.DocCount)})",
                    Value = app.Key,
                    Children = app.Modules?.Buckets.Select(module => new
                    {
                        Label = $"{module.Key}({GetCount(module.DocCount)})",
                        Value = module.Key,
                        Children = module.Categorys?.Buckets.Select(category => new
                        {
                            Label = $"{category.Key}({GetCount(category.DocCount)})",
                            Value = category.Key,
                            Children = category.SubCategorys?.Buckets.Select(subCategory => new
                            {
                                Label = $"{subCategory.Key}({GetCount(subCategory.DocCount)})",
                                Value = subCategory.Key
                            })
                        })
                    })
                });

            return result;
        }

        private string GetCount(long count)
        {
            if (count > 1000000) return (count / 1000000M).ToString("0.0#") + "M";
            if (count > 1000) return count / 1000 + "K";
            return count + "";
        }

        /// <summary>
        /// Search
        /// </summary>
        /// <param name="filtrate"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<string> Search(FiltrateModel filtrate)
        {
            var search = new EsSearchModel
            {
                Query = new Query
                {
                    Bool = new Bool
                    {
                        Must = new List<Must>()
                    }
                },
                Size = filtrate.PageSize,
                From = filtrate.PageSize * (filtrate.PageIndex - 1),
            };

            DateTime? start = null, end = null;
            if (filtrate.Times?.Length > 1)
            {
                start = filtrate.Times[0].DateTime;
                end = filtrate.Times[1].DateTime;
            }
            else if (filtrate.TimeSelect > 0)
            {
                start = DateTime.Now.AddMinutes(-filtrate.TimeSelect);
                end = DateTime.Now;
            }
            if (start != null && end != null)
            {
                search.Query.Bool.Must.Add(new Must
                {
                    Range = new Models.EsSearchModel.Range
                    {
                        Timestamp = new Timestamp
                        {
                            Gt = start.Value,
                            Lt = end.Value
                        }
                    }
                });
            }

            Must GetClassifies(int index)
            {
                if (index >= (filtrate.Classifies?.Length ?? 0)) goto Error;

                if (index == 0)
                {
                    return new Must
                    {
                        Term = new Term
                        {
                            FieldsAppKeyword = filtrate.Classifies[0]
                        }
                    };
                }
                if (index == 1)
                {
                    return new Must
                    {
                        Term = new Term
                        {
                            FieldsModuleKeyword = filtrate.Classifies[1]
                        }
                    };
                }
                if (index == 2)
                {
                    return new Must
                    {
                        Term = new Term
                        {
                            FieldsCategoryKeyword = filtrate.Classifies[2]
                        }
                    };
                }
                if (index == 3)
                {
                    return new Must
                    {
                        Term = new Term
                        {
                            FieldsSubCategoryKeyword = filtrate.Classifies[3]
                        }
                    };
                }

            Error:
                throw new Exception("错误的索引值");
            }

            if (filtrate.LoseParent && filtrate.Classifies?.Length > 0)
            {
                search.Query.Bool.Must.Add(GetClassifies(filtrate.Classifies.Length - 1));
            }
            else if (filtrate.Classifies?.Length > 0)
            {
                search.Query.Bool.Must.AddRange(
                    filtrate.Classifies.Select((_, i) => GetClassifies(i))
                );
            }

            if (!string.IsNullOrWhiteSpace(filtrate.Ip))
            {
                search.Query.Bool.Must.Add(new Must
                {
                    Term = new Term
                    {
                        FieldsIpKeyword = filtrate.Ip
                    }
                });
            }
            if (!string.IsNullOrWhiteSpace(filtrate.Filter1))
            {
                search.Query.Bool.Must.Add(new Must
                {
                    Match = new Match
                    {
                        FieldsFilter1Keyword = filtrate.Filter1
                    }
                });
            }
            if (!string.IsNullOrWhiteSpace(filtrate.Filter2))
            {
                search.Query.Bool.Must.Add(new Must
                {
                    Match = new Match
                    {
                        FieldsFilter2Keyword = filtrate.Filter2
                    }
                });
            }
            if (!string.IsNullOrWhiteSpace(filtrate.Msg))
            {
                search.Query.Bool.Must.Add(new Must
                {
                    Match = new Match
                    {
                        FieldsMsgKeyword = filtrate.Msg
                    }
                });
                search.Highlight = new Highlight
                {
                    Fields = new HighlightFields
                    {
                        FieldsMsg = new object()
                    }
                };
            }
            if (!filtrate.KeywordSort)
            {
                search.Sort = new[]
                {
                    new Sort
                    {
                        Timestamp = new Timestamp
                        {
                            Order = "DESC"
                        }
                    }
                };
            }
            if (!string.IsNullOrWhiteSpace(filtrate.Lv))
            {
                search.Query.Bool.Must.Add(new Must
                {
                    Term = new Term
                    {
                        LevelKeyword = filtrate.Lv
                    }
                });
            }

            var res = await (EsNode + "/logstash*/_search").PostJsonAsync(search);
            var result = await res.Content.ReadAsStringAsync();

            return $"[{result},{search.ToJson()}]";
        }
    }
}
