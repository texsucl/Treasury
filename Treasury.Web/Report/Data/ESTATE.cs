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
    public class ESTATE : ReportData
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
select * from ITEM_REAL_ESTATE
where ITEM_ID in (select ITEM_ID from temp)
)
,
temp3 as
(
select * from ITEM_BOOK
where GROUP_NO = (select top 1 GROUP_NO from temp2)
and ITEM_ID = 'D1014'
),
temp4 as
(
 select
'1' AS TYPE,
(select COL_VALUE from temp3 where COL = 'BOOK_NO') AS BOOK_NO,
(select COL_VALUE from temp3 where COL = 'BUILDING_NAME') AS BUILDING_NAME,
(select COL_VALUE from temp3 where COL = 'LOCATED') AS LOCATED,
 null AS ESTATE_FROM_TO,
 null AS TOTAL
 UNION ALL
 select
 '2' AS TYPE,
 null AS BOOK_NO,
 null AS BUILDING_NAME,
 null AS LOCATED,
 ESTATE_FORM_NO AS ESTATE_FROM_TO,
 COUNT(*)  AS TOTAL 
 from temp2
 group by ESTATE_FORM_NO
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