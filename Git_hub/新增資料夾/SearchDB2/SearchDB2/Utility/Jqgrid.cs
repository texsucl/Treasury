using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SearchDB2.Utility
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

            return "{" + string.Format("\"total\":\"{0}\",\"page\":{1},\"records\":\"{2}\",\"rows\":{3}", totalPages, pageIndex, count, jsonStr) + "}";
        }
    }
}