using FRT.Web.BO;
using FRT.Web.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.EasycomClient;
using System.Data.Common;
using static FRT.Web.Enum.Ref;
using FRT.Web.Service.Interface;

/// <summary>
/// 功能說明：快速付款結案日出款立帳資料沖銷查詢
/// 初版作者：20210224 Mark
/// 修改歷程：20210224 Mark
///           需求單號：202101280265-00
///           初版
/// </summary>

namespace FRT.Web.Service.Actual
{
    public class ORTB021 : IORTB021
    {
        /// <summary>
        /// 查詢 快速付款結案日出款立帳資料沖銷查詢
        /// </summary>
        /// <returns></returns>
        public MSGReturnModel<List<ORTB021ViewModel>> GetSearchData(string close_date_s, string close_date_e)
        {
            MSGReturnModel<List<ORTB021ViewModel>> results = new MSGReturnModel<List<ORTB021ViewModel>>();
            List<ORTB021ViewModel> datas = new List<ORTB021ViewModel>();
            var CLOSE_DATE_S = close_date_s.stringCheckDate();
            var CLOSE_DATE_E = close_date_e.stringCheckDate();
            try
            {
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
CASE WHEN LENGTH(TRIM(CLOSE_DATE)) = '7'
THEN SUBSTR(TRIM(CLOSE_DATE),1,3) || '/' || SUBSTR(TRIM(CLOSE_DATE),4,2) || '/' || SUBSTR(TRIM(CLOSE_DATE),6,2) 
     WHEN LENGTH(TRIM(CLOSE_DATE)) = '6'
THEN SUBSTR(TRIM(CLOSE_DATE),1,2) || '/' || SUBSTR(TRIM(CLOSE_DATE),3,2) || '/' || SUBSTR(TRIM(CLOSE_DATE),5,2) 
END AS CLOSE_DATE,
SUM(REMIT_AMT) AS REMIT_AMT,
SUM(CASE WHEN VHR_NO2 <> '' THEN REMIT_AMT ELSE 0 END) AS WRITE_OFF_AMT,
SUM(CASE WHEN VHR_NO2 = '' THEN REMIT_AMT ELSE 0 END) AS REMAIN_AMT,
SUM(CASE WHEN REMIT_STAT = '5' THEN REMIT_AMT ELSE 0 END) AS CANCEL_AMT
FROM FRTBARM0
WHERE 1 = 1 
{sqlp}
GROUP BY CLOSE_DATE
ORDER BY CLOSE_DATE
WITH UR;
";
                      
                        com.CommandText = sql;
                        com.Prepare();
                        DbDataReader dbresult = com.ExecuteReader();
                        while (dbresult.Read())
                        {
                            var model = new ORTB021ViewModel();
                            model.CLOSE_DATE = dbresult["CLOSE_DATE"]?.ToString()?.Trim(); //結案日
                            model.REMIT_AMT = dbresult["REMIT_AMT"]?.ToString()?.Trim().formateThousand(); //立帳金額
                            model.WRITE_OFF_AMT = dbresult["WRITE_OFF_AMT"]?.ToString()?.Trim().formateThousand(); //沖銷金額
                            model.REMAIN_AMT = dbresult["REMAIN_AMT"]?.ToString()?.Trim().formateThousand(); //剩餘金額
                            model.CANCEL_AMT = dbresult["CANCEL_AMT"]?.ToString()?.Trim().formateThousand(); //取消金額
                            datas.Add(model);
                        }
                        com.Dispose();
                    }
                    conn.Dispose();
                    conn.Close();
                }
                if (datas.Any())
                {
                    results.RETURN_FLAG = true;
                    results.Datas = datas;
                }
                else
                { 
                    results.DESCRIPTION = MessageType.query_Not_Find.GetDescription();
                }
            }
            catch (Exception ex)
            {
                NLog.LogManager.GetCurrentClassLogger().Error(ex.ToString());
                results.DESCRIPTION = ex.Message;
            }
            return results;
        }
    }
}