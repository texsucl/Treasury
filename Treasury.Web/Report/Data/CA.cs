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
    public class CA : ReportData
    {
        public override DataSet GetData(List<reportParm> parms)
        {
            var resultsTable = new DataSet();

            var _Parameters = new List<SqlParameter>(); 
            string aply_No = parms.Where(x => x.key == "aply_No").FirstOrDefault()?.value ?? string.Empty;
            SetDetail(aply_No);
            using (var conn = new SqlConnection(defaultConnection))
            {
                string sql = string.Empty;               
                sql += $@"
with temp as
(
select * from OTHER_ITEM_APLY
where APLY_NO =  @APLY_NO
),
code as
(
select CODE_TYPE,CODE,CODE_VALUE from sys_code where CODE_TYPE in ('CA_DESC','CA_USE')
)
select 
ROW_NUMBER() OVER(order by ITEM_ID) AS ROW_NUMBER,
(select top 1 CODE_VALUE from code where CODE_TYPE = 'CA_USE' and CODE = CA_USE) AS CA_USE, --用途
(select top 1 CODE_VALUE from code where CODE_TYPE = 'CA_DESC' and CODE = CA_DESC ) AS CA_DESC, --類型
BANK, --銀行
CA_NUMBER, --號碼
MEMO --備註
from
ITEM_CA
where ITEM_ID in (select ITEM_ID from temp)
";
                using (var cmd = new SqlCommand(sql, conn))
                {
                    _Parameters.Add(new SqlParameter("@APLY_NO", aply_No));
                    cmd.Parameters.AddRange(_Parameters.ToArray());
                    conn.Open();
                    var adapter = new SqlDataAdapter(cmd);
                    adapter.Fill(resultsTable);

                    SetExtensionParm();
                }
            }
            return resultsTable;
        }

    }
}