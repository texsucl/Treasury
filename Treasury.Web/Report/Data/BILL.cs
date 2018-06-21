using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using Treasury.WebUtility;

namespace Treasury.Web.Report.Data
{
    public class BILL : ReportData
    {
        public override DataSet GetData(List<reportParm> parms)
        {
            var resultsTable = new DataSet();
            using (var conn = new SqlConnection(defaultConnection))
            {
                string sql = string.Empty;
                sql += $@"
Select '1' as test1
";
                using (var cmd = new SqlCommand(sql, conn))
                {
                    conn.Open();
                    var adapter = new SqlDataAdapter(cmd);
                    adapter.Fill(resultsTable);                  
                }
            }
            return resultsTable;
        }

    }
}