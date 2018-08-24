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
    public class STOCK : ReportData
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
temp2 as
(
select * from ITEM_STOCK
where ITEM_ID in (select ITEM_ID from temp)
)
,
temp3 as
(
select * from ITEM_BOOK  
where GROUP_NO = (select top 1 GROUP_NO from temp2)
and ITEM_ID = 'D1015'
)
,
code as
(
select * from sys_code
where CODE_TYPE = 'STOCK_TYPE'
),
temp4 as
(
select
'1' AS TYPE,
(select Top 1 GROUP_NO from temp3 ) AS GROUP_NO, --編號
(select top 1 COL_VALUE from temp3 where COL = 'NAME') AS NAME, --股票名稱
(select SUM(ISNULL(NUMBER_OF_SHARES,0)) from temp2) AS NUMBER_OF_SHARES_TOTAL, --總股數
null AS ROW_NUMBER,
null AS STOCK_TYPE,
null AS STOCK_NO_PREAMBLE,
null AS STOCK_NO_B,
null AS STOCK_NO_E,
null AS STOCK_CNT,
null AS DENOMINATION,
null AS DENOMINATION_TOTAL,
null AS NUMBER_OF_SHARES,
null AS MEMO
UNION ALL
select
'2' AS TYPE,
null AS GROUP_NO,
null AS NAME,
null AS NUMBER_OF_SHARES_TOTAL,
ROW_NUMBER() OVER(order by ITEM_ID) AS ROW_NUMBER,
(select top 1 CODE_VALUE from code where CODE = STOCK_TYPE)  AS STOCK_TYPE , --類型
STOCK_NO_PREAMBLE, -- 序號前置碼
STOCK_NO_B, --序號(起)
STOCK_NO_E, --序號(迄) 
STOCK_CNT, --張數
DENOMINATION, --單張面額
STOCK_CNT * DENOMINATION AS DENOMINATION_TOTAL, --面額小計
NUMBER_OF_SHARES, --股數
MEMO  --備註
from temp2
)
select * from temp4;
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