using FRT.Web.BO;
using FRT.Web.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.EasycomClient;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

/// <summary>
/// 功能說明：快速付款銀行出款匯總表
/// 初版作者：20210224 Mark
/// 修改歷程：20210224 Mark
///           需求單號：202101280265-00
///           初版
/// </summary>

namespace FRT.Web.Report.Data
{
    public class ORTB020 : ReportData
    {
        public override DataSet GetData(List<reportParm> parms)
        {
            var resultsTable = new DataSet();

            var _Parameters = new List<SqlParameter>();
            string VHR_NO1_DT = parms.Where(x => x.key == "VHR_NO1_DT").FirstOrDefault()?.value.stringToDate().dateToStringT() ?? DateTime.Now.dateToStringT();
            string VHR_NO1 = parms.Where(x => x.key == "VHR_NO1").FirstOrDefault()?.value ?? string.Empty;
            using (EacConnection conn = new EacConnection(CommonUtil.GetEasycomConn()))
            {
                conn.Open();
                using (EacCommand com = new EacCommand(conn))
                {
                    string sql = string.Empty;
                    com.Parameters.Add("VHR_NO1_DT", VHR_NO1_DT);
                    sql = $@"
SELECT 
CASE WHEN LENGTH(TRIM(VHR_NO1_DT)) = '7'
THEN SUBSTR(TRIM(VHR_NO1_DT),1,3) || ' 年 ' || SUBSTR(TRIM(VHR_NO1_DT),4,2) || ' 月 ' || SUBSTR(TRIM(VHR_NO1_DT),6,2) || ' 日 '
     WHEN LENGTH(TRIM(VHR_NO1_DT)) = '6'
THEN SUBSTR(TRIM(VHR_NO1_DT),1,2) || ' 年 ' || SUBSTR(TRIM(VHR_NO1_DT),3,2) || ' 月 ' || SUBSTR(TRIM(VHR_NO1_DT),5,2) || ' 日 '
END AS VHR_NO1_DT,
VHR_NO1,
CASE WHEN LENGTH(TRIM(CLOSE_DATE)) = '7'
THEN SUBSTR(TRIM(CLOSE_DATE),1,3) || '/' || SUBSTR(TRIM(CLOSE_DATE),4,2) || '/' || SUBSTR(TRIM(CLOSE_DATE),6,2) 
     WHEN LENGTH(TRIM(CLOSE_DATE)) = '6'
THEN SUBSTR(TRIM(CLOSE_DATE),1,2) || '/' || SUBSTR(TRIM(CLOSE_DATE),3,2) || '/' || SUBSTR(TRIM(CLOSE_DATE),5,2) 
END AS CLOSE_DATE,
SUM(REMIT_AMT) AS REMIT_AMT
from FRTBARM0
where VHR_NO1_DT = :VHR_NO1_DT ";
                    if (!VHR_NO1.IsNullOrWhiteSpace())
                    {
                        sql += @" AND VHR_NO1 = :VHR_NO1 ";
                        com.Parameters.Add("VHR_NO1", VHR_NO1);
                    }
                    sql += @" 
GROUP BY VHR_NO1_DT,VHR_NO1,CLOSE_DATE
ORDER BY VHR_NO1_DT,VHR_NO1,CLOSE_DATE
WITH UR;
";
                    com.CommandText = sql;
                    com.Prepare();
                    var adapter = new EacDataAdapter();
                    adapter.SelectCommand = com;
                    adapter.Fill(resultsTable);
                    com.Dispose();
                }
                conn.Dispose();
                conn.Close();
            }

            return resultsTable;
        }
    }
}