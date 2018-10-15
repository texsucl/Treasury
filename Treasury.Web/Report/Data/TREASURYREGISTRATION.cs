using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using Treasury.Web.Models;
using Treasury.Web.Service.Actual;
using Treasury.WebUtility;

namespace Treasury.Web.Report.Data
{
    public class TREASURYREGISTRATION : ReportData
    {
        public override DataSet GetData(List<reportParm> parms)
        {
            var resultsTable = new DataSet();

            var _Parameters = new List<SqlParameter>();
            var APLY_DT_From = parms.Where(x => x.key == "APLY_DT_From").FirstOrDefault()?.value;
            var APLY_DT_To = parms.Where(x => x.key == "APLY_DT_To").FirstOrDefault()?.value;
            var vOpenTreaType = parms.Where(x => x.key == "vOpenTreaType").FirstOrDefault()?.value;
          
            //string aply_No = string.Empty;
            //if (par != null)
            //{
            //    aply_No = par.value;
            //}
            //SetDetail(aply_No);
            using (var conn = new SqlConnection(defaultConnection))
            {
                string sql = string.Empty;
                sql += @"
with 
code as
(
select CODE_TYPE,CODE,CODE_VALUE from sys_code where CODE_TYPE in ('OPEN_TREA_TYPE')
)
select 
ROW_NUMBER() OVER(order by OPEN_TREA_DATE,TREA_REGISTER_ID) AS ROW_NUMBER,
OPEN_TREA_DATE,--入庫日期
TREA_REGISTER_ID, --金庫登記簿單號
ACTUAL_PUT_TIME, --入庫時間
ACTUAL_GET_TIME, --出庫時間
(select top 1 CODE_VALUE from code where CODE_TYPE = 'OPEN_TREA_TYPE' and code = OPEN_TREA_TYPE) AS OPEN_TREA_TYPE, --開庫模式
OPEN_TREA_REASON --指定開庫原因
from
TREA_OPEN_REC
where ACTUAL_PUT_TIME BETWEEN @APLY_DT_From AND @APLY_DT_To AND OPEN_TREA_TYPE = @vOpenTreaType
";
                using (var cmd = new SqlCommand(sql, conn))
                {
                    _Parameters.Add(new SqlParameter("@vOpenTreaType", vOpenTreaType));
                    _Parameters.Add(new SqlParameter("@APLY_DT_From", APLY_DT_From));
                    _Parameters.Add(new SqlParameter("@APLY_DT_To", APLY_DT_To));
                    cmd.Parameters.AddRange(_Parameters.ToArray());
                    conn.Open();
                    var adapter = new SqlDataAdapter(cmd);
                    adapter.Fill(resultsTable);
                  
                }
            }
            return resultsTable;
        }
    }
}