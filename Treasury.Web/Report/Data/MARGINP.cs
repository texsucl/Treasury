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
    public class MARGINP : ReportData
    {
        public override DataSet GetData(List<reportParm> parms)
        {
            var resultsTable = new DataSet();

            var _Parameters = new List<SqlParameter>();
            var par = parms.Where(x => x.key == "aply_No").FirstOrDefault();
            string aply_No = string.Empty;
            if (par != null)
            {
                aply_No = par.value;
            }  
            //?.value ?? string.Empty;
            SetDetail(aply_No);
            using (var conn = new SqlConnection(defaultConnection))
            {   
                string sql = string.Empty;
                sql += @"
with temp as
(
select * from OTHER_ITEM_APLY
where APLY_NO =  @APLY_NO
),
code as
(
select CODE_TYPE,CODE,CODE_VALUE from sys_code where CODE_TYPE in ('MARGIN_ITEM','MARGIN_TAKE_OF_TYPE','INVENTORY_STATUS')
)
select 
ROW_NUMBER() OVER(order by ITEM_ID) AS ROW_NUMBER,
(select top 1 CODE_VALUE from code where CODE_TYPE = 'MARGIN_TAKE_OF_TYPE' and CODE = MARGIN_TAKE_OF_TYPE) AS MARGIN_TAKE_OF_TYPE, --類別
TRAD_PARTNERS, --交易對象
ITEM_ID, --歸檔編號	 
AMOUNT, --金額
(select top 1 CODE_VALUE from code where CODE_TYPE = 'MARGIN_ITEM' and CODE = MARGIN_ITEM) AS MARGIN_ITEM_NAME, --物品名稱
MARGIN_ITEM_ISSUER,--物品發行人
PLEDGE_ITEM_NO,--質押標的號碼
convert(varchar, EFFECTIVE_DATE_B, 111) as 'EFFECTIVE_DATE_B' ,--有效區間(起)
DESCRIPTION
from
ITEM_DEP_RECEIVED
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