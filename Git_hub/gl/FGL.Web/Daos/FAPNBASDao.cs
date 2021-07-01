
using FGL.Web.AS400Models;
using FGL.Web.BO;
using FGL.Web.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.EasycomClient;

/// <summary>
/// FAPNBASDao 應付溢繳退費檔
/// </summary>
namespace FGL.Web.Daos
{
    public class FAPNBASDao
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
SELECT main.*, code.TEXT from (  
SELECT CASE WHEN NBAS.TYPE = 'L' THEN 'LE' ELSE 'PRA' END AS TYPE, NBAS.PRO_NO, SUM(NBAS.PAY_AMT) PAY_AMT
, NBAS.FIELD01_4
  FROM LAPNBAS2 NBAS 
 WHERE NBAS.APLY_NO = ''                 
   AND NBAS.PRO_NO <> ''                 
   AND NBAS.ACPT_ID <> ''                
   AND NBAS.FIELD10_1 = ''               
   AND NBAS.CODE = ''      
   AND ((NBAS.ACPT_YY = :_ENTRY_YY_B AND NBAS.ACPT_MM = :_ENTRY_MM_B) 
     OR (NBAS.ACPT_YY = :_ENTRY_YY_E AND NBAS.ACPT_MM = :_ENTRY_MM_E AND NBAS.ACPT_DD <= :_ENTRY_DD_E))              
GROUP  BY TYPE, NBAS.PRO_NO, NBAS.FIELD01_4
) main LEFT JOIN FPMCODE0 CODE                             
  ON main.TYPE = CODE.REF_NO                               
  AND CODE.SRCE_FROM = 'RT' AND CODE.GROUP_ID = 'RESOURCE' 
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
                    d.RESOURCE = result["TYPE"]?.ToString();
                    d.RESOURCE_DESC = result["TEXT"]?.ToString();
                    d.VHR_NO = result["PRO_NO"]?.ToString();
                    d.PAID_TYPE = "支票";
                    d.AMT = result["PAY_AMT"]?.ToString();
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