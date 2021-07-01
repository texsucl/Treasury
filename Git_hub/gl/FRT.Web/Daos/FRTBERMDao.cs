using FRT.Web.AS400Models;
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
    public class FRTBERMDao
    {
        static private NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public FRTBERMModel qryByErrCode(string errCode)
        {
            FRTBERMModel d = new FRTBERMModel();

            EacConnection con = new EacConnection();
            EacCommand cmd = new EacCommand();

            List<FRTBERMModel> rows = new List<FRTBERMModel>();

            string strSQL = "";
            try
            {
                con.ConnectionString = CommonUtil.GetEasycomConn();
                con.Open();
                cmd.Connection = con;
                strSQL += "SELECT  M.ERR_CODE, M.ERR_DESC, M.ERR_BELONG, M.TRANS_CODE " +
                    " FROM LRTBERM1 M " +
                    " WHERE 1 = 1 ";
                strSQL += " AND ERR_CODE = :ERR_CODE";
                cmd.Parameters.Add("ERR_CODE", errCode);

                logger.Info("strSQ:" + strSQL);
                cmd.CommandText = strSQL;



                DbDataReader result = cmd.ExecuteReader();
                int errCodeId = result.GetOrdinal("ERR_CODE");
                int errDescId = result.GetOrdinal("ERR_DESC");
                int errBelongId = result.GetOrdinal("ERR_BELONG");
                int transCodeId = result.GetOrdinal("TRANS_CODE");


                while (result.Read())
                {
                    d.errCode = StringUtil.toString(result.GetString(errCodeId));
                    d.errDesc = StringUtil.toString(result.GetString(errDescId));
                    d.errBelong = StringUtil.toString(result.GetString(errBelongId));
                    d.transCode = StringUtil.toString(result.GetString(transCodeId));
                }


                cmd.Dispose();
                cmd = null;
                con.Close();
                con = null;

                return d;
            }
            catch (Exception e)
            {
                logger.Error(e.ToString());
                throw e;


            }

        }

        public List<FRTBERMModel> qryFRTBERM(string errCode, string errBelong, string transCode)
        {


            EacConnection con = new EacConnection();
            EacCommand cmd = new EacCommand();

            List<FRTBERMModel> rows = new List<FRTBERMModel>();

            string strSQL = "";
            try
            {
                con.ConnectionString = CommonUtil.GetEasycomConn();
                con.Open();
                cmd.Connection = con;
                strSQL += "SELECT  M.ERR_CODE, M.ERR_DESC, M.ERR_BELONG, M.TRANS_CODE " +
                    " FROM LRTBERM1 M " +
                    " WHERE 1 = 1 ";


                if (!"".Equals(StringUtil.toString(errCode)))
                {
                    strSQL += " AND ERR_CODE = :ERR_CODE";
                    cmd.Parameters.Add("ERR_CODE", errCode);
                }

                if (!"".Equals(StringUtil.toString(errBelong)))
                {
                    strSQL += " AND ERR_BELONG = :ERR_BELONG";
                    cmd.Parameters.Add("ERR_BELONG", errBelong);
                }

                if (!"".Equals(StringUtil.toString(transCode)))
                {
                    strSQL += " AND TRANS_CODE = :TRANS_CODE";
                    cmd.Parameters.Add("TRANS_CODE", transCode);
                }
                strSQL += " ORDER BY ERR_CODE";

                logger.Info("strSQ:" + strSQL);
                cmd.CommandText = strSQL;



                DbDataReader result = cmd.ExecuteReader();
                int errCodeId = result.GetOrdinal("ERR_CODE");
                int errDescId = result.GetOrdinal("ERR_DESC");
                int errBelongId = result.GetOrdinal("ERR_BELONG");
                int transCodeId = result.GetOrdinal("TRANS_CODE");


                while (result.Read())
                {
                    FRTBERMModel d = new FRTBERMModel();
                    d.errCode = StringUtil.toString(result.GetString(errCodeId));
                    d.errDesc = StringUtil.toString(result.GetString(errDescId));
                    d.errBelong = StringUtil.toString(result.GetString(errBelongId));
                    d.transCode = StringUtil.toString(result.GetString(transCodeId));
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
        /// 查詢-快速付款匯款失敗原因對照TABLE檔維護作業
        /// </summary>
        /// <param name="bankCode"></param>
        /// <param name="bankType"></param>
        /// <returns></returns>
        public List<ORTB004Model> qryForORTB004(string errCode, string errBelong, string transCode)
        {


            EacConnection con = new EacConnection();
            EacCommand cmd = new EacCommand();

            List<ORTB004Model> rows = new List<ORTB004Model>();

            string strSQL = "";
            try
            {
                con.ConnectionString = CommonUtil.GetEasycomConn();
                con.Open();
                cmd.Connection = con;
                strSQL += "SELECT  M.ERR_CODE, M.ERR_DESC, M.ERR_BELONG, M.TRANS_CODE, CODE.TEXT " +
                    " FROM LRTBERM1 M LEFT JOIN FPMCODE0 CODE ON M.TRANS_CODE = CODE.REF_NO AND CODE.GROUP_ID = 'FAIL-CODE' AND CODE.SRCE_FROM = 'RT'" +
                    " WHERE 1 = 1 ";


                if (!"".Equals(StringUtil.toString(errCode))) {
                    strSQL += " AND ERR_CODE = :ERR_CODE";
                    cmd.Parameters.Add("ERR_CODE", errCode);
                }

                if (!"".Equals(StringUtil.toString(errBelong)))
                {
                    strSQL += " AND ERR_BELONG = :ERR_BELONG";
                    cmd.Parameters.Add("ERR_BELONG", errBelong);
                }

                if (!"".Equals(StringUtil.toString(transCode)))
                {
                    strSQL += " AND TRANS_CODE = :TRANS_CODE";
                    cmd.Parameters.Add("TRANS_CODE", transCode);
                }

                strSQL += " ORDER BY ERR_CODE";
                logger.Info("strSQ:" + strSQL);
                cmd.CommandText = strSQL;



                DbDataReader result = cmd.ExecuteReader();
                int errCodeId = result.GetOrdinal("ERR_CODE");
                int errDescId = result.GetOrdinal("ERR_DESC");
                int errBelongId = result.GetOrdinal("ERR_BELONG");
                int transCodeId = result.GetOrdinal("TRANS_CODE");
                int textId = result.GetOrdinal("TEXT");


                while (result.Read())
                {
                    ORTB004Model d = new ORTB004Model();
                    d.tempId = StringUtil.toString(result.GetString(errCodeId));
                    d.errCode = StringUtil.toString(result.GetString(errCodeId));
                    d.errDesc = StringUtil.toString(result.GetString(errDescId));
                    d.errBelong = StringUtil.toString(result.GetString(errBelongId));
                    d.transCode = StringUtil.toString(result.GetString(transCodeId));
                    d.transCodeDesc = StringUtil.toString(result.GetString(textId));
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
        /// 快速付款匯款失敗原因對照TABLE覆核作業"執行核可"
        /// </summary>
        /// <param name="apprId"></param>
        /// <param name="procData"></param>
        /// <param name="conn"></param>
        /// <param name="transaction"></param>
        /// <returns></returns>
        public void apprFRTBERM0(string apprId, List<ORTB004Model> procData, EacConnection conn, EacTransaction transaction)
        {
            logger.Info("apprFRTBERM0 begin!");

            foreach (ORTB004Model d in procData)
            {
                switch (d.status)
                {
                    case "A":
                        insertFRTBERM0(apprId, d, conn, transaction);
                        break;
                    case "D":
                        deleteFRTBERM0(apprId, d, conn, transaction);
                        break;
                    case "U":
                        updateFRTBERM0(apprId, d, conn, transaction);
                        break;
                }

            }

            logger.Info("apprFRTBERM0 end!");
          
          

        }


        /// <summary>
        /// 新增"FRTBERM0  快速付款匯款失敗原因對照檔 "
        /// </summary>
        /// <param name="apprId"></param>
        /// <param name="procData"></param>
        /// <param name="conn"></param>
        /// <param name="transaction"></param>
        public void insertFRTBERM0(string apprId, ORTB004Model procData, EacConnection conn, EacTransaction transaction)
        {
            logger.Info("insertFRTBERM0 begin!");

            string[] nowStr = DateUtil.getCurChtDateTime(4).Split(' ');
            EacCommand cmd = new EacCommand();


            string strSQLI = "";
            strSQLI += "insert into LRTBERM1 ";
            strSQLI += " (ERR_CODE, ERR_DESC, ERR_BELONG, TRANS_CODE, UPD_ID, UPD_DATE, UPD_TIME, APPR_ID, APPR_DATE, APPR_TIME) ";
            strSQLI += " VALUES ";
            strSQLI += " (:ERR_CODE, :ERR_DESC, :ERR_BELONG, :TRANS_CODE, :UPD_ID, :UPD_DATE, :UPD_TIME, :APPR_ID, :APPR_DATE, :APPR_TIME) ";


            try
            {
                cmd.Connection = conn;
                cmd.Transaction = transaction;

                cmd.Parameters.Add("ERR_CODE", StringUtil.toString(procData.errCode));
                cmd.Parameters.Add("ERR_DESC", StringUtil.toString(procData.errDesc).PadRight(StringUtil.toString(procData.errDesc).Length * 2, ' '));
                cmd.Parameters.Add("ERR_BELONG", StringUtil.toString(procData.errBelong));
                cmd.Parameters.Add("TRANS_CODE", StringUtil.toString(procData.transCode));

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
                logger.Info("insertFRTBERM0 end!");

            }
            catch (Exception e)
            {
                logger.Error(e.ToString());
                throw e;
            }

        }


        /// <summary>
        /// 異動"FRTBERM0  快速付款匯款失敗原因對照檔 "
        /// </summary>
        /// <param name="apprId"></param>
        /// <param name="procData"></param>
        /// <param name="conn"></param>
        /// <param name="transaction"></param>
        public void updateFRTBERM0(string apprId, ORTB004Model procData, EacConnection conn, EacTransaction transaction)
        {
            logger.Info("updateFRTBERM0 begin!");

            string[] nowStr = DateUtil.getCurChtDateTime(4).Split(' ');
            EacCommand cmd = new EacCommand();


            string strSQL = "";
            strSQL += "update LRTBERM1 " +
                        " set ERR_DESC = :ERR_DESC " +
                        " ,ERR_BELONG = :ERR_BELONG " +
                        " ,TRANS_CODE = :TRANS_CODE " +
                        " ,UPD_ID = :UPD_ID " +
                        " ,UPD_DATE = :UPD_DATE " +
                        " ,UPD_TIME = :UPD_TIME " +
                        " ,APPR_ID = :APPR_ID " +
                        " ,APPR_DATE = :APPR_DATE " +
                        " ,APPR_TIME = :APPR_TIME " +
                        " where ERR_CODE = :ERR_CODE";


            try
            {
                cmd.Connection = conn;
                cmd.Transaction = transaction;

                
                cmd.Parameters.Add("ERR_DESC", StringUtil.toString(procData.errDesc).PadRight(StringUtil.toString(procData.errDesc).Length * 2, ' '));
                cmd.Parameters.Add("ERR_BELONG", StringUtil.toString(procData.errBelong));
                cmd.Parameters.Add("TRANS_CODE", StringUtil.toString(procData.transCode));

                cmd.Parameters.Add("UPD_ID", StringUtil.toString(procData.updId));
                cmd.Parameters.Add("UPD_DATE", procData.updDate);
                cmd.Parameters.Add("UPD_TIME", procData.updTime);

                cmd.Parameters.Add("APPR_ID", StringUtil.toString(apprId));
                cmd.Parameters.Add("APPR_DATE", nowStr[0]);
                cmd.Parameters.Add("APPR_TIME", nowStr[1]);

                cmd.Parameters.Add("ERR_CODE", StringUtil.toString(procData.errCode));

                cmd.CommandText = strSQL;

                cmd.ExecuteNonQuery();


                cmd.Dispose();
                cmd = null;
                //con.Close();
                //con = null;
                logger.Info("updateFRTBERM0 end!");

            }
            catch (Exception e)
            {
                logger.Error(e.ToString());
                throw e;
            }

        }



        /// <summary>
        /// 刪除"FRTBERM0  快速付款匯款失敗原因對照檔 "
        /// </summary>
        /// <param name="apprId"></param>
        /// <param name="procData"></param>
        /// <param name="conn"></param>
        /// <param name="transaction"></param>
        public void deleteFRTBERM0(string apprId, ORTB004Model procData, EacConnection conn, EacTransaction transaction)
        {
            logger.Info("deleteFRTBERM0 begin!");

            EacCommand cmd = new EacCommand();


            string strSQLD = "";
            strSQLD += "DELETE LRTBERM1 " +
                        " WHERE  1 = 1 " +
                        " AND ERR_CODE = :ERR_CODE ";

            try
            {
                cmd.Connection = conn;
                cmd.Transaction = transaction;


                cmd.Parameters.Add("ERR_CODE", StringUtil.toString(procData.errCode));

                cmd.CommandText = strSQLD;

                cmd.ExecuteNonQuery();


                cmd.Dispose();
                cmd = null;
                //con.Close();
                //con = null;
                logger.Info("deleteFRTBERM0 end!");
       
            }
            catch (Exception e)
            {
                logger.Error(e.ToString());
                throw e;
            }

        }

    }
}