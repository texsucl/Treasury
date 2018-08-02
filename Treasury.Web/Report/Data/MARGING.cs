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
    public class MARGING : ReportData
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
select * from sys_code
where CODE_TYPE = 'MARGING_TYPE'
)
select 
ROW_NUMBER() OVER(order by ITEM_ID) AS ROW_NUMBER,
ITEM_ID, --歸檔編號
TRAD_PARTNERS, --交易對象
(select top 1 CODE_VALUE from code where CODE = MARGIN_DEP_TYPE)  AS MARGIN_DEP_TYPE, --存出保證金類別
AMOUNT, --金額
WORKPLACE_CODE, --職場代號
DESCRIPTION, --說明
MEMO, --備註
BOOK_NO --冊號
from ITEM_REFUNDABLE_DEP
where ITEM_ID in (select ITEM_ID from temp) ;
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