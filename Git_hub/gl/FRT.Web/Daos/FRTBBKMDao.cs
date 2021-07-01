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
    public class FRTBBKMDao
    {
        static private NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();


        /// <summary>
        /// 查詢-快速付款銀行類型維護作業
        /// </summary>
        /// <param name="bankCode"></param>
        /// <param name="bankType"></param>
        /// <returns></returns>
        public List<ORTB002Model> qryForORTB002(string bankCode, string bankType)
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
                strSQL += "SELECT  M.BANK_CODE, M.BANK_TYPE,BANK.BANK_NAME " +
                    " FROM LRTBBKM1 M LEFT JOIN LDCBANK1 BANK ON M.BANK_CODE = BANK.BANK_NO " +
                    //" FROM GLSILIB/LRTBBKM1 M LEFT JOIN LP_FBDB/LDCBANK1 BANK ON M.BANK_CODE = BANK.BANK_NO " +
                    " WHERE 1 = 1 ";

                if (!"".Equals(StringUtil.toString(bankCode))) {
                    strSQL += " AND BANK_CODE = :BANK_CODE";
                    cmd.Parameters.Add("BANK_CODE", bankCode);
                }

                if (!"".Equals(StringUtil.toString(bankType)))
                {
                    logger.Info("BANK_TYPE:" + bankType);

                    strSQL += " AND BANK_TYPE = :BANK_TYPE";
                    cmd.Parameters.Add("BANK_TYPE", bankType);
                }
                strSQL += " ORDER BY BANK_TYPE, BANK_CODE";


                logger.Info("strSQ:" + strSQL);
                cmd.CommandText = strSQL;



                DbDataReader result = cmd.ExecuteReader();
                int bankCodeId = result.GetOrdinal("BANK_CODE");
                int bankTypeId = result.GetOrdinal("BANK_TYPE");
                int bankNameId = result.GetOrdinal("BANK_NAME");



                while (result.Read())
                {
                    ORTB002Model d = new ORTB002Model();
                    string tempId = StringUtil.toString(result.GetString(bankCodeId));
                    tempId = tempId == "***" ? "000" : tempId;
                    d.tempId = tempId;
                    d.bankCode = StringUtil.toString(result.GetString(bankCodeId));
                    d.bankType = StringUtil.toString(result.GetString(bankTypeId));
                    d.bankName = d.bankCode == "***" ? "其他" : StringUtil.toString(result.GetString(bankNameId));
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
        /// 判斷快速付款銀行類型覆核作業"執行核可"
        /// </summary>
        /// <param name="apprId"></param>
        /// <param name="procData"></param>
        /// <param name="conn"></param>
        /// <param name="transaction"></param>
        /// <returns></returns>
        public void apprFRTBBKM0(string apprId, List<ORTB002Model> procData, EacConnection conn, EacTransaction transaction)
        {
            logger.Info("apprFRTBBKM0 begin!");

            foreach (ORTB002Model d in procData)
            {
                switch (d.status)
                {
                    case "A":
                        insertFRTBBKM0(apprId, d, conn, transaction);
                        break;
                    case "D":
                        deleteFRTBBKM0(apprId, d, conn, transaction);
                        break;
                    case "U":
                        updateFRTBBKM0(apprId, d, conn, transaction);
                        break;
                }

            }

            logger.Info("apprFRTBBKM0 end!");
          
          

        }


        /// <summary>
        /// 新增"FRTBBKM0  快速付款銀行類型資料檔 "
        /// </summary>
        /// <param name="apprId"></param>
        /// <param name="procData"></param>
        /// <param name="conn"></param>
        /// <param name="transaction"></param>
        public void insertFRTBBKM0(string apprId, ORTB002Model procData, EacConnection conn, EacTransaction transaction)
        {
            logger.Info("insertFRTBBKM0 begin!");

            string[] nowStr = DateUtil.getCurChtDateTime(4).Split(' ');
            EacCommand cmd = new EacCommand();


            string strSQLI = "";
            strSQLI += "insert into LRTBBKM1 ";
            strSQLI += " (BANK_CODE, BANK_TYPE, UPD_ID, UPD_DATE, UPD_TIME, APPR_ID, APPR_DATE, APPR_TIME) ";
            strSQLI += " VALUES ";
            strSQLI += " (:BANK_CODE, :BANK_TYPE, :UPD_ID, :UPD_DATE, :UPD_TIME, :APPR_ID, :APPR_DATE, :APPR_TIME) ";


            try
            {
                cmd.Connection = conn;
                cmd.Transaction = transaction;

                cmd.Parameters.Add("BANK_CODE", StringUtil.toString(procData.bankCode));
                cmd.Parameters.Add("BANK_TYPE", StringUtil.toString(procData.bankType));

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
                logger.Info("insertFRTBBKM0 end!");

            }
            catch (Exception e)
            {
                logger.Error(e.ToString());
                throw e;
            }

        }


        /// <summary>
        /// 異動"FRTBBKM0  快速付款銀行類型資料檔 "
        /// </summary>
        /// <param name="apprId"></param>
        /// <param name="procData"></param>
        /// <param name="conn"></param>
        /// <param name="transaction"></param>
        public void updateFRTBBKM0(string apprId, ORTB002Model procData, EacConnection conn, EacTransaction transaction)
        {
            logger.Info("updateFRTBBKM0 begin!");

            string[] nowStr = DateUtil.getCurChtDateTime(4).Split(' ');
            EacCommand cmd = new EacCommand();


            string strSQL = "";
            strSQL += "update LRTBBKM1 " +
                        " set BANK_TYPE = :BANK_TYPE " +
                        " ,UPD_ID = :UPD_ID " +
                        " ,UPD_DATE = :UPD_DATE " +
                        " ,UPD_TIME = :UPD_TIME " +
                        " ,APPR_ID = :APPR_ID " +
                        " ,APPR_DATE = :APPR_DATE " +
                        " ,APPR_TIME = :APPR_TIME " +
                        " where BANK_CODE = :BANK_CODE";


            try
            {
                cmd.Connection = conn;
                cmd.Transaction = transaction;

                
                cmd.Parameters.Add("BANK_TYPE", StringUtil.toString(procData.bankType));

                cmd.Parameters.Add("UPD_ID", StringUtil.toString(procData.updId));
                cmd.Parameters.Add("UPD_DATE", procData.updDate);
                cmd.Parameters.Add("UPD_TIME", procData.updTime);

                cmd.Parameters.Add("APPR_ID", StringUtil.toString(apprId));
                cmd.Parameters.Add("APPR_DATE", nowStr[0]);
                cmd.Parameters.Add("APPR_TIME", nowStr[1]);

                cmd.Parameters.Add("BANK_CODE", StringUtil.toString(procData.bankCode));

                cmd.CommandText = strSQL;

                cmd.ExecuteNonQuery();


                cmd.Dispose();
                cmd = null;
                //con.Close();
                //con = null;
                logger.Info("updateFRTBBKM0 end!");

            }
            catch (Exception e)
            {
                logger.Error(e.ToString());
                throw e;
            }

        }



        /// <summary>
        /// 刪除"FRTBBKM0  快速付款銀行類型資料檔 "
        /// </summary>
        /// <param name="apprId"></param>
        /// <param name="procData"></param>
        /// <param name="conn"></param>
        /// <param name="transaction"></param>
        public void deleteFRTBBKM0(string apprId, ORTB002Model procData, EacConnection conn, EacTransaction transaction)
        {
            logger.Info("deleteFRTBBKM0 begin!");

            EacCommand cmd = new EacCommand();


            string strSQLD = "";
            strSQLD += "DELETE LRTBBKM1 " +
                        " WHERE  1 = 1 " +
                        " AND BANK_CODE = :BANK_CODE ";

            try
            {
                cmd.Connection = conn;
                cmd.Transaction = transaction;


                cmd.Parameters.Add("BANK_CODE", StringUtil.toString(procData.bankCode));

                cmd.CommandText = strSQLD;

                cmd.ExecuteNonQuery();


                cmd.Dispose();
                cmd = null;
                //con.Close();
                //con = null;
                logger.Info("deleteFRTBBKM0 end!");
       
            }
            catch (Exception e)
            {
                logger.Error(e.ToString());
                throw e;
            }

        }

    }
}