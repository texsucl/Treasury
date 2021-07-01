using FRT.Web.BO;
using FRT.Web.ViewModels;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.EasycomClient;
using System.Linq;
using System.Web;

namespace FRT.Web.Daos
{
    public class FRTBBKHDao
    {
        static private NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();




        /// <summary>
        /// 依條件查詢待覆核的資料
        /// </summary>
        /// <param name="bankCode"></param>
        /// <returns></returns>
        public List<ORTB002Model> qryForSTAT1(string bankCode)
        {


            EacConnection con = new EacConnection();
            EacCommand cmd = new EacCommand();

            List<ORTB002Model> rows = new List<ORTB002Model>();

            string strSQL = "";
            try
            {
                con.ConnectionString = CommonUtil.GetEasycomConn();
                con.Open();
                cmd.Connection = con;
                strSQL += "SELECT M.APPLY_NO, M.BANK_CODE, BANK.BANK_NAME, M.BANK_TYPE, M.STATUS, M.UPD_ID, M.UPD_DATE, M.UPD_TIME " +
                            " FROM LRTBBKH1 M LEFT JOIN LDCBANK1 BANK ON M.BANK_CODE = BANK.BANK_NO" + 
                           " WHERE 1 = 1 AND M.APPR_STAT = '1'";

                if (!"".Equals(StringUtil.toString(bankCode)))
                {
                    strSQL += " AND BANK_CODE = :BANK_CODE";
                    cmd.Parameters.Add("BANK_CODE", bankCode);
                }

                cmd.CommandText = strSQL;


                DbDataReader result = cmd.ExecuteReader();
                int aplyNoId = result.GetOrdinal("APPLY_NO");
                int bankCodeId = result.GetOrdinal("BANK_CODE");
                int bankNameId = result.GetOrdinal("BANK_NAME");
                int bankTypeId = result.GetOrdinal("BANK_TYPE");
                int statusId = result.GetOrdinal("STATUS");
                int updId = result.GetOrdinal("UPD_ID");
                int updDateId = result.GetOrdinal("UPD_DATE");
                int updTimeId = result.GetOrdinal("UPD_TIME");

                //logger.Info("updDate:" + updDateId);
                //int updTime = result.GetOrdinal("UPD_TIME");

                while (result.Read())
                {
                    ORTB002Model d = new ORTB002Model();
                    d.tempId = StringUtil.toString(result.GetString(aplyNoId)) + "|" 
                        + StringUtil.toString(result.GetString(bankCodeId));
                    d.aplyNo = StringUtil.toString(result.GetString(aplyNoId));
                    d.bankCode = StringUtil.toString(result.GetString(bankCodeId));
                    d.bankType = StringUtil.toString(result.GetString(bankTypeId));
                    d.bankName = d.bankCode == "***" ? "其他" :StringUtil.toString(result.GetString(bankNameId));
                    d.status = StringUtil.toString(result.GetString(statusId));
                    d.statusDesc = d.status == "A" ? "新增" : (d.status == "D" ? "刪除" : "修改");
                    d.updId = StringUtil.toString(result.GetString(updId));
                    d.updDate = result[updDateId].ToString();
                    d.updTime = result[updTimeId].ToString();

                    rows.Add(d);
                }


                cmd.Dispose();
                cmd = null;
                con.Close();
                con = null;

                return rows;
            }
            catch (Exception e)
            {
                logger.Error(e.ToString());
                throw e;

            }

        }

        /// <summary>
        /// 查詢-判斷快速付款銀行類型維護覆核作業
        /// </summary>
        /// <returns></returns>
        public List<ORTB002Model> qryForORTB002A()
        {
            return qryForSTAT1("");
        }



        /// <summary>
        /// 新增"FRTBBKH0 快速付款銀行類型異動檔"
        /// </summary>
        /// <param name="applyNo"></param>
        /// <param name="procData"></param>
        /// <returns></returns>
        public int insertFRTBBKH0(string applyNo, List<ORTB002Model> procData)
        {
            int execCnt = 0;

            string[] nowStr = DateUtil.getCurChtDateTime(4).Split(' ');

            EacConnection con = new EacConnection();
            EacCommand cmd = new EacCommand();

            List<ORTB002Model> rows = new List<ORTB002Model>();

            string strSQL = "";
            strSQL += "insert into LRTBBKH1 ";
            strSQL += " (APPLY_NO, BANK_CODE, BANK_TYPE, STATUS, APPR_STAT, UPD_ID, UPD_DATE, UPD_TIME) ";
            strSQL += " VALUES ";
            strSQL += " (:APPLY_NO, :BANK_CODE, :BANK_TYPE, :STATUS, :APPR_STAT, :UPD_ID, :UPD_DATE, :UPD_TIME) ";

            try
            {
                con.ConnectionString = CommonUtil.GetEasycomConn();
                con.Open();
                cmd.Connection = con;
                cmd.CommandText = strSQL;

                foreach (ORTB002Model d in procData) {
                    if (!"".Equals(d.status)) {
                        cmd.Parameters.Clear();
                        cmd.Parameters.Add("APPLY_NO", applyNo);
                        cmd.Parameters.Add("BANK_CODE", StringUtil.toString(d.bankCode));
                        cmd.Parameters.Add("BANK_TYPE", StringUtil.toString(d.bankType));
                        cmd.Parameters.Add("STATUS", StringUtil.toString(d.status));
                        cmd.Parameters.Add("APPR_STAT", "1");
                        cmd.Parameters.Add("UPD_ID", StringUtil.toString(d.updId));
                        cmd.Parameters.Add("UPD_DATE", nowStr[0]);
                        cmd.Parameters.Add("UPD_TIME", nowStr[1]);

                        cmd.ExecuteNonQuery();
                        execCnt++;
                    }
                }

               

                cmd.Dispose();
                cmd = null;
                con.Close();
                con = null;

                return execCnt;
            }
            catch (Exception e)
            {
                logger.Error(e.ToString());
                throw e;

            }

        }



        /// <summary>
        /// 異動"FRTBbkH0 快速付款銀行類型異動檔"
        /// </summary>
        /// <param name="apprId"></param>
        /// <param name="apprStat"></param>
        /// <param name="procData"></param>
        /// <param name="conn"></param>
        /// <param name="transaction"></param>
        /// <returns></returns>
        public int updateFRTBBKH0(string apprId, string apprStat, List<ORTB002Model> procData, EacConnection conn, EacTransaction transaction)
        {
            logger.Info("updateFRTBBKH0 begin! apprStat = " + apprStat);
            int execCnt = 0;

            string[] nowStr = DateUtil.getCurChtDateTime(4).Split(' ');

            EacCommand cmd = new EacCommand();

            List<ORTB001Model> rows = new List<ORTB001Model>();

            string strSQL = "";
            strSQL += "UPDATE LRTBBKH1 " +
                      "  SET APPR_STAT = :APPR_STAT " +
                          " ,APPR_ID = :APPR_ID  " +
                    " ,APPR_DATE = :APPR_DATE  " +
                    " ,APPR_TIME = :APPR_TIME  " +
                    " WHERE  1 = 1 " +
                    " AND APPLY_NO = :APPLY_NO " +
                    " AND BANK_CODE = :BANK_CODE ";

            logger.Info("strSQL:" + strSQL);

            try
            {
                cmd.Connection = conn;
                cmd.Transaction = transaction;
                cmd.CommandText = strSQL;

                foreach (ORTB002Model d in procData)
                {
                    cmd.Parameters.Clear();

                    logger.Info("APPLY_NO = " + d.aplyNo);
                    logger.Info("BANK_CODE = " + d.bankCode);

                    logger.Info("APPR_STAT = " + apprStat);
                    logger.Info("APPR_ID = " + apprId);


                    cmd.Parameters.Add("APPR_STAT", apprStat);
                    cmd.Parameters.Add("APPR_ID", StringUtil.toString(apprId));
                    cmd.Parameters.Add("APPR_DATE", nowStr[0]);
                    cmd.Parameters.Add("APPR_TIME", nowStr[1]);

                    cmd.Parameters.Add("APPLY_NO", StringUtil.toString(d.aplyNo));
                    cmd.Parameters.Add("BANK_CODE", StringUtil.toString(d.bankCode));

                    cmd.ExecuteNonQuery();
                    execCnt++;


                }

                cmd.Dispose();
                cmd = null;
                //con.Close();
                //con = null;
                logger.Info("updateFRTBBKH0 end! ");
                return execCnt;
            }
            catch (Exception e)
            {
                logger.Error(e.ToString());
                throw e;

            }

        }

    }
}