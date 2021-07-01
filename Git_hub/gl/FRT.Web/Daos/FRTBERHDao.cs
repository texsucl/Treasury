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
    public class FRTBERHDao
    {
        static private NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();




        /// <summary>
        /// 依條件查詢待覆核的資料
        /// </summary>
        /// <param name="bankType"></param>
        /// <param name="textType"></param>
        /// <returns></returns>
        public List<ORTB004Model> qryForSTAT1(string errCode)
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

                strSQL += "SELECT   M.APPLY_NO, M.ERR_CODE, M.ERR_DESC, M.ERR_BELONG, M.TRANS_CODE, CODE.TEXT " +
                    ", M.STATUS, M.UPD_ID, M.UPD_DATE, M.UPD_TIME" +
                    " FROM LRTBERH1 M LEFT JOIN FPMCODE0 CODE ON M.TRANS_CODE = CODE.REF_NO AND CODE.GROUP_ID = 'FAIL-CODE' AND CODE.SRCE_FROM = 'RT'" +
                    " WHERE 1 = 1 AND M.APPR_STAT = '1'";


                if (!"".Equals(StringUtil.toString(errCode)))
                {
                    strSQL += " AND M.ERR_CODE = :ERR_CODE";
                    cmd.Parameters.Add("ERR_CODE", errCode);
                }


                cmd.CommandText = strSQL;


                DbDataReader result = cmd.ExecuteReader();
                int aplyNoId = result.GetOrdinal("APPLY_NO");
                int errCodeId = result.GetOrdinal("ERR_CODE");
                int errDescId = result.GetOrdinal("ERR_DESC");
                int errBelongId = result.GetOrdinal("ERR_BELONG");
                int transCodeId = result.GetOrdinal("TRANS_CODE");
                int textId = result.GetOrdinal("TEXT");
                int statusId = result.GetOrdinal("STATUS");
                int updId = result.GetOrdinal("UPD_ID");
                int updDateId = result.GetOrdinal("UPD_DATE");
                int updTimeId = result.GetOrdinal("UPD_TIME");

                //logger.Info("updDate:" + updDateId);
                //int updTime = result.GetOrdinal("UPD_TIME");

                while (result.Read())
                {
                    ORTB004Model d = new ORTB004Model();
                    d.tempId = StringUtil.toString(result.GetString(aplyNoId)) + "|" 
                        + StringUtil.toString(result.GetString(errCodeId));
                    d.aplyNo = StringUtil.toString(result.GetString(aplyNoId));

                    d.errCode = StringUtil.toString(result.GetString(errCodeId));
                    d.errDesc = StringUtil.toString(result.GetString(errDescId));
                    d.errBelong = StringUtil.toString(result.GetString(errBelongId));
                    d.transCode = StringUtil.toString(result.GetString(transCodeId));
                    d.transCodeDesc = StringUtil.toString(result.GetString(textId));

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
        /// 查詢-快速付款匯款失敗原因對照TABLE覆核作業
        /// </summary>
        /// <returns></returns>
        public List<ORTB004Model> qryForORTB004A()
        {
            return qryForSTAT1("");
        }


        /// <summary>
        /// 新增"FRTBERH0 快速付款匯款失敗原因對照TABLE異動檔"
        /// </summary>
        /// <param name="applyNo"></param>
        /// <param name="procData"></param>
        /// <returns></returns>
        public int insertFRTBERH0(string applyNo, List<ORTB004Model> procData)
        {
            int execCnt = 0;

            string[] nowStr = DateUtil.getCurChtDateTime(4).Split(' ');

            EacConnection con = new EacConnection();
            EacCommand cmd = new EacCommand();

            string strSQL = "";
            strSQL += "insert into LRTBERH1 ";
            strSQL += " (APPLY_NO, ERR_CODE, ERR_DESC, ERR_BELONG, TRANS_CODE, STATUS, APPR_STAT, UPD_ID, UPD_DATE, UPD_TIME) ";
            strSQL += " VALUES ";
            strSQL += " (:APPLY_NO, :ERR_CODE, :ERR_DESC, :ERR_BELONG, :TRANS_CODE, :STATUS, :APPR_STAT, :UPD_ID, :UPD_DATE, :UPD_TIME) ";

            try
            {
                con.ConnectionString = CommonUtil.GetEasycomConn();
                con.Open();
                cmd.Connection = con;
                cmd.CommandText = strSQL;

                foreach (ORTB004Model d in procData) {
                    if (!"".Equals(d.status)) {
                        cmd.Parameters.Clear();
                        cmd.Parameters.Add("APPLY_NO", applyNo);

                        logger.Info(StringUtil.toString(d.errDesc));

                        cmd.Parameters.Add("ERR_CODE", StringUtil.toString(d.errCode));

                        cmd.Parameters.Add("ERR_DESC", StringUtil.toString(d.errDesc).PadRight(StringUtil.toString(d.errDesc).Length * 2, ' '));
                        cmd.Parameters.Add("ERR_BELONG", StringUtil.toString(d.errBelong));
                        cmd.Parameters.Add("TRANS_CODE", StringUtil.toString(d.transCode));


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
        /// 異動"FRTBERH0 快速付款匯款失敗原因對照TABLE異動檔"
        /// </summary>
        /// <param name="apprId"></param>
        /// <param name="apprStat"></param>
        /// <param name="procData"></param>
        /// <param name="conn"></param>
        /// <param name="transaction"></param>
        /// <returns></returns>
        public int updateFRTBERH0(string apprId, string apprStat, List<ORTB004Model> procData, EacConnection conn, EacTransaction transaction)
        {
            logger.Info("updateFRTBERH0 begin! apprStat = " + apprStat);
            int execCnt = 0;

            string[] nowStr = DateUtil.getCurChtDateTime(4).Split(' ');

            EacCommand cmd = new EacCommand();

            List<ORTB004Model> rows = new List<ORTB004Model>();

            string strSQL = "";
            strSQL += "UPDATE LRTBERH1 " +
                    "  SET APPR_STAT = :APPR_STAT " +
                    " ,APPR_ID = :APPR_ID  " +
                    " ,APPR_DATE = :APPR_DATE  " +
                    " ,APPR_TIME = :APPR_TIME  " +
                    " WHERE  1 = 1 " +
                    " AND APPLY_NO = :APPLY_NO " +
                    " AND ERR_CODE = :ERR_CODE ";

            logger.Info("strSQL:" + strSQL);

            try
            {
                cmd.Connection = conn;
                cmd.Transaction = transaction;
                cmd.CommandText = strSQL;

                foreach (ORTB004Model d in procData)
                {
                    cmd.Parameters.Clear();

                    logger.Info("APPLY_NO = " + d.aplyNo);
                    logger.Info("ERR_CODE = " + d.errCode);

                    logger.Info("APPR_STAT = " + apprStat);
                    logger.Info("APPR_ID = " + apprId);


                    cmd.Parameters.Add("APPR_STAT", apprStat);
                    cmd.Parameters.Add("APPR_ID", StringUtil.toString(apprId));
                    cmd.Parameters.Add("APPR_DATE", nowStr[0]);
                    cmd.Parameters.Add("APPR_TIME", nowStr[1]);

                    cmd.Parameters.Add("APPLY_NO", StringUtil.toString(d.aplyNo));
                    cmd.Parameters.Add("ERR_CODE", StringUtil.toString(d.errCode));

                    cmd.ExecuteNonQuery();
                    execCnt++;


                }

                cmd.Dispose();
                cmd = null;
                //con.Close();
                //con = null;
                logger.Info("updateFRTBERH0 end! ");
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