

using FAP.Web.AS400Models;
using FAP.Web.BO;
using FAP.Web.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.EasycomClient;

/// <summary>
/// FAPPPAS   逾期未兌領制裁名單確認檔 
/// ------------------------------------------
/// add by daiyu 20191126
/// 需求單號：201910290100-01
/// 修改內容：初版
/// ------------------------------------------
/// </summary>
namespace FAP.Web.Daos
{
    public class FAPPPASDao
    {
        static private NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();


        /// <summary>
        /// 逾期未兌領支票-禁制名單_名單刪除通知(Filler_14=03*)
        /// </summary>
        /// <param name="conn400"></param>
        /// <param name="exec_date"></param>
        /// <param name="upd_id"></param>
        /// <returns></returns>
        public FAPPPASModel qryByCinNo(EacConnection conn400, string cin_no)
        {

            logger.Info("qryByCinNo begin!");
            FAPPPASModel model = new FAPPPASModel();

            EacCommand cmdQ = new EacCommand();

            string strSQLQ = @"
SELECT CIN_NO
      ,UNIT
      ,SOURCE_ID
      ,PAID_ID
      ,PAID_NAME
      ,APPL_ID
      ,QUERY_ID
      ,RTN_CODE
      ,IS_SAN
      ,STATUS
      ,ENTRY_DT
      ,ENTRY_TM
      ,CANCEL_MK
  FROM LAPPPAS1
    WHERE CIN_NO = :cin_no 
";

            try
            {
                cmdQ.Connection = conn400;
                cmdQ.CommandText = strSQLQ;

                cmdQ.Parameters.Clear();

                cmdQ.Parameters.Add("cin_no", cin_no);

                DbDataReader result = cmdQ.ExecuteReader();

                while (result.Read())
                {
                    model.cin_no = result["CIN_NO"]?.ToString().Trim();
                    model.unit = result["UNIT"]?.ToString().Trim();
                    model.source_id = result["SOURCE_ID"]?.ToString().Trim();
                    model.paid_id = result["PAID_ID"]?.ToString().Trim();
                    model.paid_name = result["PAID_NAME"]?.ToString().Trim();
                    model.appl_id = result["APPL_ID"]?.ToString().Trim();
                    model.query_id = result["QUERY_ID"]?.ToString().Trim();
                    model.rtn_code = result["RTN_CODE"]?.ToString().Trim();
                    model.is_san = result["IS_SAN"]?.ToString().Trim();
                    model.status = result["STATUS"]?.ToString().Trim();
                    model.entry_dt = result["ENTRY_DT"]?.ToString().Trim();
                    model.entry_tm = result["ENTRY_TM"]?.ToString().Trim();
                    model.cancel_mk = result["CANCEL_MK"]?.ToString().Trim();
                }

                cmdQ.Dispose();
                cmdQ = null;

                logger.Info("qryByCinNo end!");

            }
            catch (Exception e)
            {
                logger.Error(e.ToString());
                throw e;
            }

            return model;

        }



        public void insertAmlResult(FAPPPASModel model, EacConnection conn)
        {
            logger.Info("updateAmlResult begin!");
            string[] chtDt = BO.DateUtil.getCurChtDateTime(4).Split(' ');

            model.entry_dt = chtDt[0];
            model.entry_tm = chtDt[1];

            EacCommand cmd = new EacCommand();


            string strSQL = "";
            strSQL = @"
 INSERT INTO LAPPPAS1 
 (UNIT, SOURCE_ID, PAID_ID, PAID_NAME, QUERY_ID, RTN_CODE, IS_SAN, STATUS, ENTRY_DT, ENTRY_TM, CIN_NO, APPL_ID, CANCEL_MK, CANCEL_DT, CANCEL_TM)
 VALUES
 (:UNIT, :SOURCE_ID, :PAID_ID, :PAID_NAME, :QUERY_ID, :RTN_CODE, :IS_SAN, :STATUS, :ENTRY_DT, :ENTRY_TM, :CIN_NO, :APPL_ID, :CANCEL_MK, :CANCEL_DT, :CANCEL_TM)
";

            try
            {
                cmd.Connection = conn;
                cmd.CommandText = strSQL;


                cmd.Parameters.Clear();

                cmd.Parameters.Add("UNIT", model.unit);
                cmd.Parameters.Add("SOURCE_ID", model.source_id);
                cmd.Parameters.Add("PAID_ID", model.paid_id);
                cmd.Parameters.Add("PAID_NAME", model.paid_name);
                cmd.Parameters.Add("QUERY_ID", model.query_id);
                cmd.Parameters.Add("RTN_CODE", model.rtn_code);
                cmd.Parameters.Add("IS_SAN", model.is_san);
                cmd.Parameters.Add("STATUS", model.status);
                cmd.Parameters.Add("ENTRY_DT", model.entry_dt);
                cmd.Parameters.Add("ENTRY_TM", model.entry_tm);
                cmd.Parameters.Add("CIN_NO", model.cin_no);
                cmd.Parameters.Add("APPL_ID", model.appl_id);
                cmd.Parameters.Add("CANCEL_MK", "");
                cmd.Parameters.Add("CANCEL_DT", "0");
                cmd.Parameters.Add("CANCEL_TM", "0");

                cmd.ExecuteNonQuery();


                cmd.Dispose();
                cmd = null;

                logger.Info("insertAmlResult end!");

            }
            catch (Exception e)
            {
                logger.Error(e.ToString());
                throw e;
            }
        }



        public void updateAmlResult(FAPPPASModel model, EacConnection conn)
        {
            logger.Info("updateAmlResult begin!");
            string[] chtDt = BO.DateUtil.getCurChtDateTime(4).Split(' ');

            model.entry_dt = chtDt[0];
            model.entry_tm = chtDt[1];

            EacCommand cmd = new EacCommand();


            string strSQL = "";
            strSQL = @"
 UPDATE LAPPPAS1 
   SET UNIT = :UNIT
      ,SOURCE_ID = :SOURCE_ID
      ,PAID_ID = :PAID_ID
      ,PAID_NAME = :PAID_NAME
      ,QUERY_ID = :QUERY_ID
      ,RTN_CODE = :RTN_CODE
      ,IS_SAN = :IS_SAN
      ,STATUS = :STATUS
      ,ENTRY_DT = :ENTRY_DT
      ,ENTRY_TM = :ENTRY_TM
      ,CANCEL_MK = ''
      ,CANCEL_DT = 0
      ,CANCEL_TM = 0
 WHERE CIN_NO = :CIN_NO
";

            try
            {
                cmd.Connection = conn;
                cmd.CommandText = strSQL;


                cmd.Parameters.Clear();

                cmd.Parameters.Add("UNIT", model.unit);
                cmd.Parameters.Add("SOURCE_ID", model.source_id);
                cmd.Parameters.Add("PAID_ID", model.paid_id);
                cmd.Parameters.Add("PAID_NAME", model.paid_name);
                cmd.Parameters.Add("QUERY_ID", model.query_id);
                cmd.Parameters.Add("RTN_CODE", model.rtn_code);
                cmd.Parameters.Add("IS_SAN", model.is_san);
                cmd.Parameters.Add("STATUS", model.status);
                cmd.Parameters.Add("ENTRY_DT", model.entry_dt);
                cmd.Parameters.Add("ENTRY_TM", model.entry_tm);
                cmd.Parameters.Add("CIN_NO", model.cin_no);

                cmd.ExecuteNonQuery();
                

                cmd.Dispose();
                cmd = null;

                logger.Info("updateAmlResult end!");

            }
            catch (Exception e)
            {
                logger.Error(e.ToString());
                throw e;
            }
        }

    }
}