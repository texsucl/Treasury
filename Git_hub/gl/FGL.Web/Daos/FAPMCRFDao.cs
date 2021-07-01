
using FGL.Web.AS400Models;
using FGL.Web.BO;
using FGL.Web.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.EasycomClient;

/// <summary>
/// FAPMCRFDao 應付變更退費檔
/// </summary>
namespace FGL.Web.Daos
{
    public class FAPMCRFDao
    {
        static private NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();


        /// <summary>
        /// For BGL0009 未接收報表(給付流程優化)
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="rows"></param>
        /// <param name="conn"></param>
        /// <returns></returns>
        public List<BGL00009Model> qryForBGL0009(List<BGL00009Model> rows, EacConnection conn, DateTime end_date)
        {
            logger.Info("qryForBGL0009 begin!");
            int _ENTRY_YY_B, _ENTRY_MM_B, _ENTRY_YY_E, _ENTRY_MM_E, _ENTRY_DD_E = 0;

            _ENTRY_YY_B = end_date.AddMonths(-1).Year - 1911;
            _ENTRY_MM_B = end_date.AddMonths(-1).Month;

            _ENTRY_YY_E = end_date.Year - 1911;
            _ENTRY_MM_E = end_date.Month;
            _ENTRY_DD_E = end_date.Day;

            EacCommand cmd = new EacCommand();
            string strSQL = @"
SELECT MCRF.SRCE_FROM, MCRF.VHR_NO1, SUM(MCRF.PAID_TAL) PAID_TAL
, MCRF.FIELD01_4
, CODE.TEXT
  FROM LAPMCRF1 MCRF LEFT JOIN FPMCODE0 CODE ON MCRF.SRCE_FROM = CODE.REF_NO AND CODE.SRCE_FROM = 'RT' AND CODE.GROUP_ID = 'RESOURCE'
 WHERE MCRF.VHR_NO1 <> ''                                 
   AND MCRF.APLY_NO = ''                                  
   AND MCRF.ACPT_ID <> ''                                 
   AND MCRF.FIELD10_1 = ''                                
   AND MCRF.PASS_YY = 0 AND MCRF.PASS_MM = 0 AND MCRF.PASS_DD = 0   
   AND ((MCRF.GEN_YY = :_ENTRY_YY_B AND MCRF.GEN_MM = :_ENTRY_MM_B) 
     OR (MCRF.GEN_YY = :_ENTRY_YY_E AND MCRF.GEN_MM = :_ENTRY_MM_E AND MCRF.GEN_DD <= :_ENTRY_DD_E))    
GROUP BY MCRF.SRCE_FROM, MCRF.VHR_NO1, MCRF.FIELD01_4, CODE.TEXT                  
";

            try
            {
                cmd.Connection = conn;
                cmd.CommandText = strSQL;
                cmd.Parameters.Clear();
                cmd.Parameters.Add("_ENTRY_YY_B", _ENTRY_YY_B);
                cmd.Parameters.Add("_ENTRY_MM_B", _ENTRY_MM_B);
                cmd.Parameters.Add("_ENTRY_YY_E", _ENTRY_YY_E);
                cmd.Parameters.Add("_ENTRY_MM_E", _ENTRY_MM_E);
                cmd.Parameters.Add("_ENTRY_DD_E", _ENTRY_DD_E);

                DbDataReader result = cmd.ExecuteReader();

                while (result.Read())
                {
                    BGL00009Model d = new BGL00009Model();
                    d.SYS_TYPE = "F";
                    d.RESOURCE = result["SRCE_FROM"]?.ToString();
                    d.RESOURCE_DESC = result["TEXT"]?.ToString();
                    d.VHR_NO = result["VHR_NO1"]?.ToString();
                    d.PAID_TYPE = "支票";
                    d.AMT = result["PAID_TAL"]?.ToString();
                    d.COMP_TYPE = StringUtil.toString(result["FIELD01_4"]?.ToString()) == "" ? "FUBON" : "OIU";

                    //20190813 add by daiyu 
                    if (Convert.ToDouble(d.AMT).CompareTo(0) != 0)
                        rows.Add(d);
                }

                cmd.Dispose();
                cmd = null;

                logger.Info("qryForBGL0009 end!");

                return rows;

            }
            catch (Exception e)
            {
                logger.Error(e.ToString());
                throw e;
            }

        }




    }
}