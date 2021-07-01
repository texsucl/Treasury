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
    public class FRTBTMHDao
    {
        static private NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();




        /// <summary>
        /// 依條件查詢待覆核的資料
        /// </summary>
        /// <param name="bankType"></param>
        /// <param name="textType"></param>
        /// <returns></returns>
        public List<ORTB003Model> qryForSTAT1(string bankType, string textType)
        {


            EacConnection con = new EacConnection();
            EacCommand cmd = new EacCommand();

            List<ORTB003Model> rows = new List<ORTB003Model>();

            string strSQL = "";
            try
            {
                con.ConnectionString = CommonUtil.GetEasycomConn();
                con.Open();
                cmd.Connection = con;
                strSQL += "SELECT M.APPLY_NO, M.BANK_TYPE, M.TEXT_TYPE, M.STR_TIME, M.END_TIME, M.TIMEOUT_D, M.STATUS," +
                    " M.UPD_ID, M.UPD_DATE, M.UPD_TIME " +
                            " FROM LRTBTMH1 M " + 
                           " WHERE 1 = 1 AND M.APPR_STAT = '1'";

                if (!"".Equals(StringUtil.toString(bankType)))
                {
                    strSQL += " AND BANK_TYPE = :BANK_TYPE";
                    cmd.Parameters.Add("BANK_TYPE", bankType);
                }

                if (!"".Equals(StringUtil.toString(textType)))
                {
                    strSQL += " AND TEXT_TYPE = :TEXT_TYPE";
                    cmd.Parameters.Add("TEXT_TYPE", textType);
                }


                cmd.CommandText = strSQL;


                DbDataReader result = cmd.ExecuteReader();
                int aplyNoId = result.GetOrdinal("APPLY_NO");
                int bankTypeId = result.GetOrdinal("BANK_TYPE");
                int textTypeId = result.GetOrdinal("TEXT_TYPE");
                int strTimeId = result.GetOrdinal("STR_TIME");
                int endTimeId = result.GetOrdinal("END_TIME");
                int timeoutDId = result.GetOrdinal("TIMEOUT_D");
                int statusId = result.GetOrdinal("STATUS");
                int updId = result.GetOrdinal("UPD_ID");
                int updDateId = result.GetOrdinal("UPD_DATE");
                int updTimeId = result.GetOrdinal("UPD_TIME");
                //logger.Info("updDate:" + updDateId);
                //int updTime = result.GetOrdinal("UPD_TIME");

                while (result.Read())
                {
                    ORTB003Model d = new ORTB003Model();
                    d.tempId = StringUtil.toString(result.GetString(aplyNoId)) + "|" 
                        + StringUtil.toString(result.GetString(bankTypeId)) + "|"
                        + StringUtil.toString(result.GetString(strTimeId)) + "|"
                        + StringUtil.toString(result.GetString(strTimeId)) + "|"
                        + StringUtil.toString(result.GetString(endTimeId));
                    d.aplyNo = StringUtil.toString(result.GetString(aplyNoId));
                    d.bankType = StringUtil.toString(result.GetString(bankTypeId));
                    d.textType = StringUtil.toString(result.GetString(textTypeId));
                    d.strTime = StringUtil.toString(result.GetString(strTimeId)).Substring(0, 4);
                    d.endTime = StringUtil.toString(result.GetString(endTimeId)).Substring(0, 4);
                    d.timeoutD = result[timeoutDId].ToString();

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
                throw e;

            }

        }


        /// <summary>
        /// 查詢-判斷電文類別及時間控制覆核作業
        /// </summary>
        /// <returns></returns>
        public List<ORTB003Model> qryForORTB003A()
        {
            return qryForSTAT1("", "");
        }

        /// <summary>
        /// 新增"FRTBTMH0 電文類別及時間控制異動檔"
        /// </summary>
        /// <param name="applyNo"></param>
        /// <param name="procData"></param>
        /// <returns></returns>
        public int insertFRTBTMH0(string applyNo, List<ORTB003Model> procData)
        {
            int execCnt = 0;

            string[] nowStr = DateUtil.getCurChtDateTime(4).Split(' ');

            EacConnection con = new EacConnection();
            EacCommand cmd = new EacCommand();


            string strSQL = "";
            strSQL += "insert into LRTBTMH1 ";
            strSQL += " (APPLY_NO, BANK_TYPE, TEXT_TYPE, STR_TIME, END_TIME, TIMEOUT_D, STATUS, APPR_STAT, UPD_ID, UPD_DATE, UPD_TIME) ";
            strSQL += " VALUES ";
            strSQL += " (:APPLY_NO, :BANK_TYPE, :TEXT_TYPE, :STR_TIME, :END_TIME, :TIMEOUT_D, :STATUS, :APPR_STAT, :UPD_ID, :UPD_DATE, :UPD_TIME) ";

            try
            {
                con.ConnectionString = CommonUtil.GetEasycomConn();
                con.Open();
                cmd.Connection = con;
                cmd.CommandText = strSQL;

                foreach (ORTB003Model d in procData) {
                    if (!"".Equals(d.status)) {
                        cmd.Parameters.Clear();
                        cmd.Parameters.Add("APPLY_NO", applyNo);
 
                        cmd.Parameters.Add("BANK_TYPE", StringUtil.toString(d.bankType));
                        cmd.Parameters.Add("TEXT_TYPE", StringUtil.toString(d.textType));
                        cmd.Parameters.Add("STR_TIME", StringUtil.toString(d.strTime).PadLeft(4, '0') + "0000");
                        cmd.Parameters.Add("END_TIME", StringUtil.toString(d.endTime).PadLeft(4, '0') + "0000");
                        cmd.Parameters.Add("TIMEOUT_D", StringUtil.toString(d.timeoutD));

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
                return 0;

            }

        }



        /// <summary>
        /// 異動"FRTBTMH0 電文類別及時間控制異動檔"
        /// </summary>
        /// <param name="apprId"></param>
        /// <param name="apprStat"></param>
        /// <param name="procData"></param>
        /// <param name="conn"></param>
        /// <param name="transaction"></param>
        /// <returns></returns>
        public int updateFRTBTMH0(string apprId, string apprStat, List<ORTB003Model> procData, EacConnection conn, EacTransaction transaction)
        {
            logger.Info("updateFRTBTMH0 begin! apprStat = " + apprStat);
            int execCnt = 0;

            string[] nowStr = DateUtil.getCurChtDateTime(4).Split(' ');

            EacCommand cmd = new EacCommand();

            List<ORTB003Model> rows = new List<ORTB003Model>();

            string strSQL = "";
            strSQL += "UPDATE LRTBTMH1 " +
                      "  SET APPR_STAT = :APPR_STAT " +
                          " ,APPR_ID = :APPR_ID  " +
                    " ,APPR_DATE = :APPR_DATE  " +
                    " ,APPR_TIME = :APPR_TIME  " +
                    " WHERE  1 = 1 " +
                    " AND APPLY_NO = :APPLY_NO " +
                    " AND BANK_TYPE = :BANK_TYPE " +
                    " AND TEXT_TYPE = :TEXT_TYPE " +
                    " AND STR_TIME = :STR_TIME " +
                    " AND END_TIME = :END_TIME ";

            logger.Info("strSQL:" + strSQL);

            try
            {
                cmd.Connection = conn;
                cmd.Transaction = transaction;
                cmd.CommandText = strSQL;

                foreach (ORTB003Model d in procData)
                {
                    cmd.Parameters.Clear();

                    logger.Info("APPLY_NO = " + d.aplyNo);
                    logger.Info("BANK_TYPE = " + d.bankType);
                    logger.Info("TEXT_TYPE = " + d.textType);
                    logger.Info("STR_TIME = " + StringUtil.toString(d.strTime).PadLeft(4, '0') + "0000");
                    logger.Info("END_TIME = " + StringUtil.toString(d.endTime).PadLeft(4, '0') + "0000");

                    logger.Info("APPR_STAT = " + apprStat);
                    logger.Info("APPR_ID = " + apprId);


                    cmd.Parameters.Add("APPR_STAT", apprStat);
                    cmd.Parameters.Add("APPR_ID", StringUtil.toString(apprId));
                    cmd.Parameters.Add("APPR_DATE", nowStr[0]);
                    cmd.Parameters.Add("APPR_TIME", nowStr[1]);

                    cmd.Parameters.Add("APPLY_NO", StringUtil.toString(d.aplyNo));
                    cmd.Parameters.Add("BANK_TYPE", StringUtil.toString(d.bankType));
                    cmd.Parameters.Add("TEXT_TYPE", StringUtil.toString(d.textType));
                    cmd.Parameters.Add("STR_TIME", StringUtil.toString(d.strTime).PadLeft(4, '0') + "0000");
                    cmd.Parameters.Add("END_TIME", StringUtil.toString(d.endTime).PadLeft(4, '0') + "0000");

                    cmd.ExecuteNonQuery();
                    execCnt++;


                }

                cmd.Dispose();
                cmd = null;
                //con.Close();
                //con = null;
                logger.Info("updateFRTBTMH0 end! ");
                return execCnt;
            }
            catch (Exception e)
            {
                logger.Error(e.ToString());
                return 0;

            }

        }

    }
}