

using FAP.Web.AS400Models;
using FAP.Web.BO;
using FAP.Web.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.EasycomClient;

/// <summary>
/// LIVJAMLD3  萊斯－ＡＭＬ回覆檔
/// ------------------------------------------
/// modify by daiyu 20191127
/// 需求單號:
/// 
/// ------------------------------------------
/// </summary>
namespace FAP.Web.Daos
{
    public class FIVJAMLDDao
    {
        static private NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();


        /// <summary>
        /// 查詢D檔RENEW的名單
        /// </summary>
        /// <param name="conn400"></param>
        /// <returns></returns>
        public List<AMLDFileModel> qryForRenew(EacConnection conn400, string unit)
        {
            logger.Info("qryForRenew begin!");

            List<AMLDFileModel> rtnList = new List<AMLDFileModel>();

            EacCommand cmdQ = new EacCommand();

            string strSQLQ = @"
SELECT AML.STAT_CODE
      ,AML.REQUEST_ID
      ,AML.TRANS_ID
      ,AML.SOURCE_ID
      ,AML.ROLE_ID
      ,AML.UNIT
      ,PPAS.PAID_NAME
  FROM CLSGEN/LIVJAMLD3 AML JOIN LAPPPAS1 PPAS ON AML.REQUEST_ID  = PPAS.CIN_NO 
    WHERE AML.UNIT = :UNIT
      AND AML.IS_READ = ''
";

            try
            {
                cmdQ.Connection = conn400;
                cmdQ.CommandText = strSQLQ;

                cmdQ.Parameters.Clear();
                cmdQ.Parameters.Add("UNIT", unit);

                DbDataReader result = cmdQ.ExecuteReader();

                while (result.Read())
                {
                    AMLDFileModel d = new AMLDFileModel();

                    d.stat_code = result["STAT_CODE"]?.ToString().Trim();
                    d.cin_no = result["REQUEST_ID"]?.ToString().Trim();
                    d.trans_id = result["TRANS_ID"]?.ToString().Trim();
                    d.paid_name = result["PAID_NAME"]?.ToString().Trim();
                    d.source_id = result["SOURCE_ID"]?.ToString().Trim();
                    d.role_id = result["ROLE_ID"]?.ToString().Trim();
                    d.unit = result["UNIT"]?.ToString().Trim();
                    rtnList.Add(d);
                }


                cmdQ.Dispose();
                cmdQ = null;

                logger.Info("qryForRenew end!");

            }
            catch (Exception e)
            {
                logger.Error(e.ToString());
                throw e;
            }

            return rtnList;

        }

        


        public void updReadMk(List<AMLDFileModel> dataList, EacConnection conn)
        {
            logger.Info("updRead begin!");


            EacCommand cmd = new EacCommand();


            string strSQL = "";
            strSQL = @"
 UPDATE CLSGEN/LIVJAMLD3 
   SET IS_READ = 'Y'
 WHERE UNIT = :UNIT
   AND TRANS_ID = :TRANS_ID
   AND SOURCE_ID = :SOURCE_ID
   AND ROLE_ID = :ROLE_ID
";

            try
            {
                cmd.Connection = conn;
                cmd.CommandText = strSQL;

                foreach (AMLDFileModel d in dataList)
                {
                    cmd.Parameters.Clear();

                    cmd.Parameters.Add("UNIT", d.unit);
                    cmd.Parameters.Add("TRANS_ID", d.trans_id);
                    cmd.Parameters.Add("SOURCE_ID", d.source_id);
                    cmd.Parameters.Add("ROLE_ID", d.role_id);

                    cmd.ExecuteNonQuery();
                }

                cmd.Dispose();
                cmd = null;

                logger.Info("updRead end!");

            }
            catch (Exception e)
            {
                logger.Error(e.ToString());
                throw e;
            }
        }



        public void insert(List<FAPPPAWModel> dataList, EacConnection conn, EacTransaction transaction)
        {
            logger.Info("insert FAPPPAW0 begin!");


            EacCommand cmd = new EacCommand();


            string strSQL = "";


            strSQL = @"insert into FAPPPAW0 
            ( REPORT_TP
             ,SYSTEM
             ,DEPT_GROUP
             ,PAID_ID
             ,CHECK_NO
             ,CHECK_SHRT
             ,DATA_FLAG
             ,DEPT_DATE1
             ,R_ZIP_CODE
             ,R_ADDR
             ,ENTRY_ID
             ,ENTRY_DATE
             ,ENTRY_TIME
            ) VALUES (
              :REPORT_TP
             ,:SYSTEM
             ,:DEPT_GROUP
             ,:PAID_ID
             ,:CHECK_NO
             ,:CHECK_SHRT
             ,:DATA_FLAG
             ,:DEPT_DATE1
             ,:R_ZIP_CODE
             ,:R_ADDR
             ,:ENTRY_ID
             ,:ENTRY_DATE
             ,:ENTRY_TIME
            ) ";


            try
            {
                cmd.Connection = conn;
                cmd.Transaction = transaction;
                cmd.CommandText = strSQL;


                foreach (FAPPPAWModel d in dataList)
                {
                    cmd.Parameters.Clear();

                    cmd.Parameters.Add("REPORT_TP", StringUtil.toString(d.report_tp));
                    cmd.Parameters.Add("SYSTEM", StringUtil.toString(d.system));
                    cmd.Parameters.Add("DEPT_GROUP", StringUtil.toString(d.dept_group));
                    cmd.Parameters.Add("PAID_ID", StringUtil.toString(d.paid_id));
                    cmd.Parameters.Add("CHECK_NO", StringUtil.toString(d.check_no));
                    cmd.Parameters.Add("CHECK_SHRT", StringUtil.toString(d.check_shrt));
                    cmd.Parameters.Add("DATA_FLAG", StringUtil.toString(d.data_flag));
                    cmd.Parameters.Add("DEPT_DATE1", StringUtil.toString(d.dept_date1) == "" ? "0" : d.dept_date1);
                    cmd.Parameters.Add("R_ZIP_CODE", StringUtil.toString(d.r_zip_code));
                    cmd.Parameters.Add("R_ADDR", StringUtil.toString(d.r_addr));
                    cmd.Parameters.Add("ENTRY_ID", StringUtil.toString(d.entry_id));
                    cmd.Parameters.Add("ENTRY_DATE", d.entry_date == "" ? "0" : d.entry_date);
                    cmd.Parameters.Add("ENTRY_TIME", d.entry_time == "" ? "0" : d.entry_time);



                    cmd.ExecuteNonQuery();

                }

                cmd.Dispose();
                cmd = null;

                logger.Info("insert FAPPPAW0 end!");

            }
            catch (Exception e)
            {
                logger.Error(e.ToString());
                throw e;
            }
        }



    }
}