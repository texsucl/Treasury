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
    public class BILL : ReportData
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
select 
ROW_NUMBER() OVER(ORDER BY ISSUING_BANK,CHECK_TYPE,CHECK_NO_TRACK) AS ROW,
'1' AS TYPE,
ISSUING_BANK,
CHECK_TYPE,
CHECK_NO_TRACK,
CHECK_NO_B,
CHECK_NO_E,
(ISNULL(CAST(CHECK_NO_E AS int),0) - ISNULL(CAST(CHECK_NO_B AS int),0) + 1)  AS Total
from BLANK_NOTE_APLY
where APLY_NO = @APLY_NO
UNION ALL
select 
null AS ROW,
'2' AS TYPE,
ISSUING_BANK,
CHECK_TYPE,
CHECK_NO_TRACK,
CHECK_NO_B,
CHECK_NO_E,
(ISNULL(CAST(CHECK_NO_E AS int),0) - ISNULL(CAST(CHECK_NO_B AS int),0) + 1)  AS Total
from ITEM_BLANK_NOTE
";

                if (_REC.APLY_SECT == null)
                {
                    sql += @"
where APLY_DEPT = @APLY_DEPT
Order By ISSUING_BANK,CHECK_TYPE,CHECK_NO_TRACK
";
                    _Parameters.Add(new SqlParameter("@APLY_DEPT", _REC.APLY_DEPT));
                }
                else
                {
                    sql += @"
where APLY_DEPT = @APLY_DEPT
and APLY_SECT = @APLY_SECT 
Order By ISSUING_BANK,CHECK_TYPE,CHECK_NO_TRACK
";
                    _Parameters.Add(new SqlParameter("@APLY_DEPT", _REC.APLY_DEPT));
                    _Parameters.Add(new SqlParameter("@APLY_SECT", _REC.APLY_SECT));
                }
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