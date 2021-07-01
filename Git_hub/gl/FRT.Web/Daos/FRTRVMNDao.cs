using FRT.Web.AS400Models;
using FRT.Web.BO;
using FRT.Web.Models;
using FRT.Web.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.EasycomClient;
using System.Linq;
using System.Web;

namespace FRT.Web.Daos
{
    public class FRTRVMNDao
    {
        static private NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();


        /// <summary>
        /// 特殊帳號檔維護作業及訊息table維護作業
        /// </summary>
        /// <param name="bankNo"></param>
        /// <param name="bankAct"></param>
        /// <param name="qDateB"></param>
        /// <param name="qDateE"></param>
        /// <returns></returns>
        public ORT0104Model qryForORT0104(ORT0104Model d)
        {
            EacConnection con = new EacConnection();
            EacCommand cmd = new EacCommand();

            d.corp_no = StringUtil.toString(d.currency) == "NTD" ? "1" : "3";

            string strSQL = "";
            try
            {
                con.ConnectionString = CommonUtil.GetEasycomConn();
                con.Open();
                cmd.Connection = con;

                strSQL += "SELECT RVMN.CURRENCY , RVMN.CORP_NO, RVMN.VHR_NO1, RVMN.PRO_NO, RVMN.PAID_ID, RVMN.FAIL_CODE, RVMN.SEQN, " +
                    " MAIN.PAYMENT, MAIN.REMIT_AMT, MAIN.BANK_NO, MAIN.BANK_ACT, MAIN.FEE_SEQN, MAIN.FBO_NO, " +
                    " BANK.BANK_NAME " +
                    " FROM LRTRVMN1 RVMN JOIN LRTMAIN1 MAIN ON RVMN.CORP_NO = MAIN.CORP_NO AND RVMN.VHR_NO1 = MAIN.VHR_NO1 AND RVMN.PRO_NO = MAIN.PRO_NO AND RVMN.PAID_ID = MAIN.PAID_ID " +
                    "               LEFT JOIN FDCBANK0 BANK ON MAIN.BANK_NO = BANK.BANK_NO " +
                    " WHERE 1=1 " +
                    "  AND RVMN.CORP_NO = :CORP_NO " +
                    "  AND RVMN.VHR_NO1 = :VHR_NO1 " +
                    "  AND RVMN.PRO_NO = :PRO_NO " +
                    "  AND RVMN.PAID_ID = :PAID_ID " +
                    "  AND RVMN.CURRENCY = :CURRENCY " +
                    "  AND (RVMN.SEQN = '' OR (RVMN.SEQN <> '' AND RVMN.ORT0104 = 'Y'))";

                cmd.Parameters.Add("CORP_NO", StringUtil.toString(d.corp_no));
                cmd.Parameters.Add("VHR_NO1", StringUtil.toString(d.vhr_no1));
                cmd.Parameters.Add("PRO_NO", StringUtil.toString(d.pro_no));
                cmd.Parameters.Add("PAID_ID", StringUtil.toString(d.paid_id));
                cmd.Parameters.Add("CURRENCY", StringUtil.toString(d.currency));


                //logger.Info("strSQ:" + strSQL);
                cmd.CommandText = strSQL;

                DbDataReader result = cmd.ExecuteReader();

                ORT0104Model model = new ORT0104Model();

                while (result.Read())
                {

                    model.currency = result["CURRENCY"]?.ToString().Trim();
                    model.corp_no = result["CORP_NO"]?.ToString().Trim();
                    model.vhr_no1 = result["VHR_NO1"]?.ToString().Trim();
                    model.pro_no = result["PRO_NO"]?.ToString().Trim();
                    model.paid_id = result["PAID_ID"]?.ToString().Trim();
                    model.fail_code = result["FAIL_CODE"]?.ToString().Trim();
                    model.seqn = result["SEQN"]?.ToString().Trim();

                    model.payment = result["PAYMENT"]?.ToString().Trim();
                    model.remit_amt = result["REMIT_AMT"]?.ToString().Trim();
                    model.bank_no = result["BANK_NO"]?.ToString().Trim();
                    model.bank_act = result["BANK_ACT"]?.ToString().Trim();
                    model.fee_seqn = result["FEE_SEQN"]?.ToString().Trim();
                    model.fbo_no = result["FBO_NO"]?.ToString().Trim();

                    model.bank_name = result["BANK_NAME"]?.ToString().Trim();

                    break;
                }



                cmd.Dispose();
                cmd = null;
                con.Close();
                con = null;

                return model;
            }
            catch (Exception e)
            {
                logger.Error(e.ToString());
                throw e;
            }

        }


        public void updForORT0104A(string apprId, FRT_RVMN_HIS procData, EacConnection conn, EacTransaction transaction)
        {
            string[] curDateTime = DateUtil.getCurChtDateTime(4).Split(' ');

            DateTime uDt = Convert.ToDateTime(procData.update_datetime);
            DateTime nowDt = DateTime.Now;

            string _date = (uDt.Year - 1911).ToString().PadLeft(4, '0') + uDt.Month.ToString().PadLeft(2, '0') + uDt.Day.ToString().PadLeft(2, '0');
            string _time = uDt.Hour.ToString().PadLeft(2, '0') + uDt.Minute.ToString().PadLeft(2, '0') + uDt.Second.ToString().PadLeft(2, '0');

            string strSQL = "";
            strSQL += @"
UPDATE LRTRVMN1 
  SET FAIL_CODE = :FAIL_CODE
     ,SEQN = :SEQN
     ,UPD_DATE = :UPD_DATE
     ,UPD_TIME = :UPD_TIME
     ,UPD_ID = :UPD_ID
     ,ORT0104 = 'Y'
 WHERE CORP_NO = :CORP_NO
  AND VHR_NO1 = :VHR_NO1
  AND PRO_NO = :PRO_NO
  AND PAID_ID = :PAID_ID";

            EacCommand cmd = new EacCommand();
            cmd.Connection = conn;
            cmd.Transaction = transaction;

            cmd.Parameters.Add("FAIL_CODE", procData.fail_code);
            cmd.Parameters.Add("SEQN", procData.seqn);
            cmd.Parameters.Add("UPD_DATE", _date);
            cmd.Parameters.Add("UPD_TIME", _time);
            cmd.Parameters.Add("UPD_ID", procData.update_id);


            cmd.Parameters.Add("CORP_NO", procData.corp_no);
            cmd.Parameters.Add("VHR_NO1", procData.vhr_no1);
            cmd.Parameters.Add("PRO_NO", procData.pro_no);
            cmd.Parameters.Add("PAID_ID", procData.paid_id);


            cmd.CommandText = strSQL;
            cmd.ExecuteNonQuery();

            cmd.Dispose();
            cmd = null;
        }


    }
}