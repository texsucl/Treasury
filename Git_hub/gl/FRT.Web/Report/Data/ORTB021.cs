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
/// 功能說明：快速付款結案日出款立帳資料沖銷 報表
/// 初版作者：20210224 Mark
/// 修改歷程：20210224 Mark
///           需求單號：202101280265-00
///           初版
/// </summary>

namespace FRT.Web.Report.Data
{
    public class ORTB021 : ReportData
    {
        public override DataSet GetData(List<reportParm> parms)
        {
            var resultsTable = new DataSet();

            var _Parameters = new List<SqlParameter>();
            string CLOSE_DATE_S = parms.Where(x => x.key == "CLOSE_DATE_S").FirstOrDefault()?.value.stringCheckDate();
            string CLOSE_DATE_E = parms.Where(x => x.key == "CLOSE_DATE_E").FirstOrDefault()?.value.stringCheckDate();

            using (EacConnection conn = new EacConnection(CommonUtil.GetEasycomConn()))
            {
                conn.Open();
                using (EacCommand com = new EacCommand(conn))
                {
                    string sqlp = string.Empty;
                    if (!CLOSE_DATE_S.IsNullOrWhiteSpace())
                    {
                        sqlp += " AND CLOSE_DATE >= :CLOSE_DATE_S ";
                        com.Parameters.Add("CLOSE_DATE_S", CLOSE_DATE_S);
                    }
                    if (!CLOSE_DATE_E.IsNullOrWhiteSpace())
                    {
                        sqlp += " AND CLOSE_DATE <= :CLOSE_DATE_E ";
                        com.Parameters.Add("CLOSE_DATE_E", CLOSE_DATE_E);
                    }

                    string sql = string.Empty;
                    sql = $@"
select 
CLOSE_DATE,
CASE WHEN LENGTH(TRIM(CLOSE_DATE)) = '7'
THEN SUBSTR(TRIM(CLOSE_DATE),1,3) || '/' || SUBSTR(TRIM(CLOSE_DATE),4,2) || '/' || SUBSTR(TRIM(CLOSE_DATE),6,2) 
     WHEN LENGTH(TRIM(CLOSE_DATE)) = '6'
THEN SUBSTR(TRIM(CLOSE_DATE),1,2) || '/' || SUBSTR(TRIM(CLOSE_DATE),3,2) || '/' || SUBSTR(TRIM(CLOSE_DATE),5,2) 
END AS CLOSE_DATE,
SYS_TYPE,
POLICY_NO || ' ' || POLICY_SEQ || ' ' || ID_DUP AS POLICY_NUM,
CHANGE_ID,
AREA,
SRCE_FROM,
SRCE_KIND,
FAST_NO,
REMIT_AMT,
CASE WHEN REMIT_STAT = '5'
THEN 'Y'
ELSE '' END AS STAT
from FRTBARM0
WHERE 1 =1 
{sqlp}
AND ( VHR_NO2 =  '' OR REMIT_STAT = '5')
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