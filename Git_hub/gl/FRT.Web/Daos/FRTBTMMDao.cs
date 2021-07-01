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
    public class FRTBTMMDao
    {
        static private NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();


        /// <summary>
        /// 查詢-電文類別及時間控制維護作業
        /// </summary>
        /// <param name="bankType"></param>
        /// <param name="textType"></param>
        /// <returns></returns>
        public List<ORTB003Model> qryForORTB003(string bankType, string textType)
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
                strSQL += "SELECT BANK_TYPE, TEXT_TYPE, STR_TIME, END_TIME, TIMEOUT_D" +
                    " FROM LRTBTMM1 WHERE 1 = 1 ";

                if (!"".Equals(StringUtil.toString(bankType))) {
                    strSQL += " AND BANK_TYPE = :BANK_TYPE";
                    cmd.Parameters.Add("BANK_TYPE", bankType);
                }

                if (!"".Equals(StringUtil.toString(textType)))
                {
                    strSQL += " AND TEXT_TYPE = :TEXT_TYPE";
                    cmd.Parameters.Add("TEXT_TYPE", textType);
                }


                logger.Info("strSQ:" + strSQL);
                cmd.CommandText = strSQL;



                DbDataReader result = cmd.ExecuteReader();
                int bankTypeId = result.GetOrdinal("BANK_TYPE");
                int textTypeId = result.GetOrdinal("TEXT_TYPE");
                int strTimeId = result.GetOrdinal("STR_TIME");
                int endTimeId = result.GetOrdinal("END_TIME");
                int timeoutDId = result.GetOrdinal("TIMEOUT_D");

                while (result.Read())
                {
                    ORTB003Model d = new ORTB003Model();
                    d.tempId = StringUtil.toString(result.GetString(bankTypeId)) + "-"
                        + StringUtil.toString(result.GetString(textTypeId)) + "-"
                        + StringUtil.toString(result.GetString(strTimeId)).Substring(0, 4) + "-"
                        + StringUtil.toString(result.GetString(endTimeId)).Substring(0, 4);
                    d.bankType = StringUtil.toString(result.GetString(bankTypeId));
                    d.textType = StringUtil.toString(result.GetString(textTypeId));
                    d.strTime = StringUtil.toString(result.GetString(strTimeId)).Substring(0, 4);
                    d.endTime = StringUtil.toString(result.GetString(endTimeId)).Substring(0, 4);
                    d.timeoutD = result[timeoutDId].ToString();
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
                throw (e);

            }

        }

        /// <summary>
        /// 判斷電文類別及時間控制覆核作業"執行核可"
        /// </summary>
        /// <param name="apprId"></param>
        /// <param name="procData"></param>
        /// <param name="conn"></param>
        /// <param name="transaction"></param>
        /// <returns></returns>
        public void apprFRTBTMM0(string apprId, List<ORTB003Model> procData, EacConnection conn, EacTransaction transaction)
        {
            logger.Info("apprFRTBTMM0 begin!");

            foreach (ORTB003Model d in procData)
            {
                switch (d.status)
                {
                    case "A":
                        insertFRTBTMM0(apprId, d, conn, transaction);
                        break;
                    case "D":
                        deleteFRTBTMM0(apprId, d, conn, transaction);
                        break;
                    case "U":
                        updateFRTBTMM0(apprId, d, conn, transaction);
                        break;
                }
            }


            logger.Info("apprFRTBTMM0 end!");
          
          

        }


        /// <summary>
        /// 新增"FRTBTMM0  電文類別及時間控制檔 "
        /// </summary>
        /// <param name="apprId"></param>
        /// <param name="procData"></param>
        /// <param name="conn"></param>
        /// <param name="transaction"></param>
        public void insertFRTBTMM0(string apprId, ORTB003Model procData, EacConnection conn, EacTransaction transaction)
        {
            logger.Info("insertFRTBTMM0 begin!");

            string[] nowStr = DateUtil.getCurChtDateTime(4).Split(' ');
            EacCommand cmd = new EacCommand();


            string strSQLI = "";
            strSQLI += "insert into LRTBTMM1 ";
            strSQLI += " (BANK_TYPE, TEXT_TYPE, STR_TIME, END_TIME, TIMEOUT_D, UPD_ID, UPD_DATE, UPD_TIME, APPR_ID, APPR_DATE, APPR_TIME) ";
            strSQLI += " VALUES ";
            strSQLI += " (:BANK_TYPE, :TEXT_TYPE, :STR_TIME, :END_TIME, :TIMEOUT_D, :UPD_ID, :UPD_DATE, :UPD_TIME, :APPR_ID, :APPR_DATE, :APPR_TIME) ";


            try
            {
                cmd.Connection = conn;
                cmd.Transaction = transaction;

                cmd.Parameters.Add("BANK_TYPE", StringUtil.toString(procData.bankType));
                cmd.Parameters.Add("TEXT_TYPE", StringUtil.toString(procData.textType));
                cmd.Parameters.Add("STR_TIME", StringUtil.toString(procData.strTime));
                cmd.Parameters.Add("END_TIME", StringUtil.toString(procData.endTime));
                cmd.Parameters.Add("TIMEOUT_D", StringUtil.toString(procData.timeoutD));

                cmd.Parameters.Add("UPD_ID", StringUtil.toString(procData.updId));
                cmd.Parameters.Add("UPD_DATE", procData.updDate);
                cmd.Parameters.Add("UPD_TIME", procData.updTime);

                cmd.Parameters.Add("APPR_ID", StringUtil.toString(apprId));
                cmd.Parameters.Add("APPR_DATE", nowStr[0]);
                cmd.Parameters.Add("APPR_TIME", nowStr[1]);

                cmd.CommandText = strSQLI;

                cmd.ExecuteNonQuery();


                cmd.Dispose();
                cmd = null;
                //con.Close();
                //con = null;
                logger.Info("insertFRTBTMM0 end!");

            }
            catch (Exception e)
            {
                logger.Error(e.ToString());
                throw e;
            }

        }



        /// <summary>
        /// 異動"FRTBTMM0  電文類別及時間控制檔 "
        /// </summary>
        /// <param name="apprId"></param>
        /// <param name="procData"></param>
        /// <param name="conn"></param>
        /// <param name="transaction"></param>
        public void updateFRTBTMM0(string apprId, ORTB003Model procData, EacConnection conn, EacTransaction transaction)
        {
            logger.Info("updateFRTBTMM0 begin!");

            string[] nowStr = DateUtil.getCurChtDateTime(4).Split(' ');
            EacCommand cmd = new EacCommand();


            string strSQL = "";
            strSQL += "update LRTBTMM1 " +
                        " set TIMEOUT_D = :TIMEOUT_D " +
                        " ,UPD_ID = :UPD_ID " +
                        " ,UPD_DATE = :UPD_DATE " +
                        " ,UPD_TIME = :UPD_TIME " +
                        " ,APPR_ID = :APPR_ID " +
                        " ,APPR_DATE = :APPR_DATE " +
                        " ,APPR_TIME = :APPR_TIME " +
                        " where BANK_TYPE = :BANK_TYPE" +
                        " and TEXT_TYPE = :TEXT_TYPE"+
                        " AND STR_TIME = :STR_TIME " +
                        " AND END_TIME = :END_TIME ";


            try
            {
                cmd.Connection = conn;
                cmd.Transaction = transaction;

                cmd.Parameters.Add("TIMEOUT_D", StringUtil.toString(procData.timeoutD));

                cmd.Parameters.Add("UPD_ID", StringUtil.toString(procData.updId));
                cmd.Parameters.Add("UPD_DATE", procData.updDate);
                cmd.Parameters.Add("UPD_TIME", procData.updTime);

                cmd.Parameters.Add("APPR_ID", StringUtil.toString(apprId));
                cmd.Parameters.Add("APPR_DATE", nowStr[0]);
                cmd.Parameters.Add("APPR_TIME", nowStr[1]);

                cmd.Parameters.Add("BANK_TYPE", StringUtil.toString(procData.bankType));
                cmd.Parameters.Add("TEXT_TYPE", StringUtil.toString(procData.textType));
                cmd.Parameters.Add("STR_TIME", StringUtil.toString(procData.strTime));
                cmd.Parameters.Add("END_TIME", StringUtil.toString(procData.endTime));

                cmd.CommandText = strSQL;

                cmd.ExecuteNonQuery();


                cmd.Dispose();
                cmd = null;
                //con.Close();
                //con = null;
                logger.Info("updateFRTBTMM0 end!");

            }
            catch (Exception e)
            {
                logger.Error(e.ToString());
                throw e;
            }

        }


        /// <summary>
        /// 刪除"FRTBTMM0  電文類別及時間控制檔 "
        /// </summary>
        /// <param name="apprId"></param>
        /// <param name="procData"></param>
        /// <param name="conn"></param>
        /// <param name="transaction"></param>
        public void deleteFRTBTMM0(string apprId, ORTB003Model procData, EacConnection conn, EacTransaction transaction)
        {
            logger.Info("deleteFRTBTMM0 begin!");

            EacCommand cmd = new EacCommand();


            string strSQLD = "";
            strSQLD += "DELETE LRTBTMM1 " +
                        " WHERE  1 = 1 " +
                        " AND BANK_TYPE = :BANK_TYPE " +
                        " AND TEXT_TYPE = :TEXT_TYPE " +
                        " AND STR_TIME = :STR_TIME " +
                        " AND END_TIME = :END_TIME ";

            try
            {
                cmd.Connection = conn;
                cmd.Transaction = transaction;


                cmd.Parameters.Add("BANK_TYPE", StringUtil.toString(procData.bankType));
                cmd.Parameters.Add("TEXT_TYPE", StringUtil.toString(procData.textType));
                cmd.Parameters.Add("STR_TIME", StringUtil.toString(procData.strTime));
                cmd.Parameters.Add("END_TIME", StringUtil.toString(procData.endTime));

                cmd.CommandText = strSQLD;

                cmd.ExecuteNonQuery();


                cmd.Dispose();
                cmd = null;
                //con.Close();
                //con = null;
                logger.Info("deleteFRTBTMM0 end!");
       
            }
            catch (Exception e)
            {
                logger.Error(e.ToString());
                throw e;
            }

        }

    }
}