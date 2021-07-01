
using FGL.Web.AS400Models;
using FGL.Web.BO;
using FGL.Web.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.EasycomClient;

/// <summary>
/// FRTCOPHDao 給付主檔
/// </summary>
namespace FGL.Web.Daos
{
    public class FRTCOPHDao
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
SELECT SUM(COPL.REMIT_AMT) REMIT_AMT, COPH.RESOURCE, COPH.VHR_NO1
, COPH.FIELD01_1
, CODE.TEXT
  FROM FRTCOPH0 COPH      JOIN LRTCOPL95 COPL ON COPH.RESOURCE = COPL.RESOURCE AND COPH.VHR_NO1 = COPL.VHR_NO1
                     LEFT JOIN FPMCODE0 CODE ON COPH.RESOURCE = CODE.REF_NO AND CODE.SRCE_FROM = 'RT' AND CODE.GROUP_ID = 'RESOURCE'
    WHERE COPH.DATA_STAT = ''
      AND  COPH.SX_CODE = ''
      AND COPL.PAID_TYPE NOT IN ('Q', '3')
      AND ((COPH.ENTRY_YY = :_ENTRY_YY_B AND COPH.ENTRY_MM = :_ENTRY_MM_B) 
        OR (COPH.ENTRY_YY = :_ENTRY_YY_E AND COPH.ENTRY_MM = :_ENTRY_MM_E AND COPH.ENTRY_DD <= :_ENTRY_DD_E))
GROUP BY COPH.RESOURCE, COPH.VHR_NO1, COPH.FIELD01_1, CODE.TEXT 
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
                    d.RESOURCE = result["RESOURCE"]?.ToString();
                    d.RESOURCE_DESC = result["TEXT"]?.ToString();
                    d.VHR_NO = result["VHR_NO1"]?.ToString();
                    d.PAID_TYPE = "匯款";
                    d.AMT = result["REMIT_AMT"]?.ToString();
                    d.COMP_TYPE = StringUtil.toString(result["FIELD01_1"]?.ToString()) == "" ? "FUBON" : "OIU";

                    //20190813 add by daiyu 
                    if(Convert.ToDouble(d.AMT).CompareTo(0) != 0)
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