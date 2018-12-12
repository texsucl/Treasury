using System;
using System.Collections.Generic;
using System.Linq;

namespace Treasury.WebUtility
{
    public static class Jqgrid
    {
        public static string dynToJqgridResult(
             this jqGridParam jdata,
             List<System.Dynamic.ExpandoObject> data
            )
        {
            if (0.Equals(data.Count))
                return "{\"total\":\"1\",\"page\":\"1\",\"records\":\"0\"}";
                //return new
                //{
                //    total = 1,
                //    page = 1,
                //    records = 0,
                //};

            var count = data.Count;
            int pageIndex = jdata.page;
            int pageSize = jdata.rows;
            int totalRecords = count;
            int totalPages = (int)Math.Ceiling((float)totalRecords / (float)pageSize);

            var datas = data.Skip((pageIndex - 1) * pageSize).Take(pageSize);

            var tTarget = data.First() as System.Dynamic.IDynamicMetaObjectProvider;
            var names = tTarget.GetMetaObject(System.Linq.Expressions.Expression.Constant(tTarget)).GetDynamicMemberNames();
            var jsonStr = string.Empty;
            jsonStr += "[";
            List<string> allStr = new List<string>();
            foreach (var _data in datas)
            {
                var _jsonStr = "{";
                List<string> _jsonStrs = new List<string>();
                foreach (var name in names)
                {
                    _jsonStrs.Add($" \"{name}\":\"{(_data as IDictionary<string, object>)[name]}\" ");
                }
                _jsonStr += (string.Join(",", _jsonStrs) + "}");
                allStr.Add(_jsonStr);
            }
            jsonStr += string.Join(",", allStr);
            jsonStr += "]";
        
            return "{"+ string.Format("\"total\":\"{0}\",\"page\":{1},\"records\":\"{2}\",\"rows\":{3}", totalPages, pageIndex, count, jsonStr) + "}" ;
        }

        public static object modelToJqgridResult<T>(
            this jqGridParam jdata,
            List<T> data,
            bool thousandFlag = false,
            List<string> noThousand = null) where T : class
        {
            if (0.Equals(data.Count))
                return new
                {
                    total = 1,
                    page = 1,
                    records = 0,
                };
            if (jdata._search)
            {
                switch (jdata.searchOper)
                {
                    case "ne": //不等於
                        data = data.Where(x =>
                                typeof(T).GetProperty(jdata.searchField)
                                .GetValue(x, null).ToString()
                                 != jdata.searchString).ToList();
                        break;
                    //case "bw": //開始於
                    //    break;
                    //case "bn": //不開始於
                    //    break;
                    //case "ew": //結束於
                    //    break;
                    //case "en": //不結束於
                    //    break;
                    //case "cn": //包含
                    //    break;
                    //case "nc": //不包含
                    //    break;
                    //case "nu": //is null
                    //    break;
                    //case "nn": //is not null
                    //    break;
                    //case "in": //在其中
                    //    break;
                    //case "ni": //不在其中
                    //    break;
                    case "eq": //等於
                    default:
                        data = data.Where(x =>
                                typeof(T).GetProperty(jdata.searchField)
                                .GetValue(x, null).ToString()
                                .Equals(jdata.searchString)).ToList();
                        break;
                }
            }

            var count = data.Count;
            int pageIndex = jdata.page;
            int pageSize = jdata.rows;
            int totalRecords = count;
            int totalPages = (int)Math.Ceiling((float)totalRecords / (float)pageSize);

            if (thousandFlag && data.Any())
            {
                data.ForEach(x =>
                {
                    x.GetType().GetProperties().ToList().ForEach(y =>
                    {
                        object val = y.GetValue(x);
                        if (noThousand != null && noThousand.Any())
                        {
                            if (val != null)
                                if (!noThousand.Contains(y.Name))
                                    y.SetValue(x, val.ToString().formateThousand());
                        }
                        else
                        {
                            if (val != null)
                                y.SetValue(x, val.ToString().formateThousand());
                        }
                    });
                });
            }

            if (!jdata.sidx.IsNullOrWhiteSpace())
            {
                var _p = typeof(T).GetProperty(jdata.sidx);
                if (_p == null)
                {
                    _p = typeof(T).GetProperty(jdata.sidx.Substring(0, (jdata.sidx.LastIndexOf('_') == -1 ? jdata.sidx.Length : jdata.sidx.LastIndexOf('_'))));
                }
                if (_p != null)
                {
                    if ("asc".Equals(jdata.sord))
                    {
                        data = data.OrderBy(x => _p.GetValue(x, null)).ToList();
                    }
                    else
                    {
                        data = data.OrderByDescending(x => _p.GetValue(x, null)).ToList();
                    }
                }
            }

            var datas = data.Skip((pageIndex - 1) * pageSize).Take(pageSize);

            return new
            {
                total = totalPages,
                page = pageIndex,
                records = count,
                rows = datas
            };
        }
    }
}