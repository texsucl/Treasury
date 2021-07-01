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
    public class FRTBARHDao
    {
        static private NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public List<ORTB009Model> qryForORTB009(string fastNo)
        {


            EacConnection con = new EacConnection();
            EacCommand cmd = new EacCommand();

            List<ORTB009Model> rows = new List<ORTB009Model>();

            string strSQL = "";
            try
            {
                con.ConnectionString = CommonUtil.GetEasycomConn();
                con.Open();
                cmd.Connection = con;

                strSQL += "SELECT H.APPLY_NO, H.FAST_NO, H.UPD_ID, H.UPD_DATE, H.UPD_TIME  " +
                    " , M.PAID_ID, M.BANK_CODE, M.SUB_BANK, M.BANK_ACT,  M.RCV_NAME, M.REMIT_AMT " +
                    " FROM LRTBARH1 H JOIN LRTBARM1 M ON H.FAST_NO = M.FAST_NO " +
                    " WHERE H.APPR_STAT = '1' " +
                    " AND H.APLY_TYPE = '4' ";


                if (!"".Equals(StringUtil.toString(fastNo)))
                {
                    strSQL += " AND H.FAST_NO = :FAST_NO";
                    cmd.Parameters.Add("FAST_NO", fastNo);
                }


                cmd.CommandText = strSQL;


                DbDataReader result = cmd.ExecuteReader();
                int applyNoId = result.GetOrdinal("APPLY_NO");
                int fastNoId = result.GetOrdinal("FAST_NO");
                int updId = result.GetOrdinal("UPD_ID");
                int updDateId = result.GetOrdinal("UPD_DATE");
                int updTimeId = result.GetOrdinal("UPD_TIME");

                int paidIdId = result.GetOrdinal("PAID_ID");
                int bankCodeId = result.GetOrdinal("BANK_CODE");
                int subBankId = result.GetOrdinal("SUB_BANK");
                int bankActId = result.GetOrdinal("BANK_ACT");
                int rcvNameId = result.GetOrdinal("RCV_NAME");
                int remitAmtId = result.GetOrdinal("REMIT_AMT");

                //logger.Info("updDate:" + updDateId);
                //int updTime = result.GetOrdinal("UPD_TIME");

                while (result.Read())
                {
                    ORTB009Model d = new ORTB009Model();
                    d.applyNo = StringUtil.toString(result.GetString(applyNoId));
                    d.fastNo = StringUtil.toString(result.GetString(fastNoId));
                    d.paidId = StringUtil.toString(result.GetString(paidIdId));
                    d.bankCode = StringUtil.toString(result.GetString(bankCodeId)) + StringUtil.toString(result.GetString(subBankId));
                    //  d.subBank = StringUtil.toString(result.GetString(subBankId));
                    d.bankAct = StringUtil.toString(result.GetString(bankActId));
                    d.rcvName = StringUtil.toString(result.GetString(rcvNameId));
                    d.remitAmt = result[remitAmtId].ToString();
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
                logger.Error(e.ToString);
                throw e;
            }

        }



        /// <summary>
        /// 依條件查詢待覆核的資料(qryForORTB006)
        /// </summary>
        /// <param name="fastNo"></param>
        /// <param name="policyNo"></param>
        /// <param name="policySeq"></param>
        /// <param name="idDup"></param>
        /// <returns></returns>
        public List<ORTB006Model> qryForORTB006(string fastNo, string policyNo, string policySeq, string idDup)
        {


            EacConnection con = new EacConnection();
            EacCommand cmd = new EacCommand();

            List<ORTB006Model> rows = new List<ORTB006Model>();

            string strSQL = "";
            try
            {
                con.ConnectionString = CommonUtil.GetEasycomConn();
                con.Open();
                cmd.Connection = con;

                strSQL += "SELECT H.APPLY_NO, H.FAST_NO, H.FAIL_CODE, H.UPD_ID, H.UPD_DATE, H.UPD_TIME,  " +
                    " M.REMIT_DATE, M.BANK_CODE, M.SUB_BANK, M.BANK_ACT,  M.RCV_NAME, M.REMIT_AMT, M.ENTRY_ID , M.FILLER_20 " +
                    " FROM LRTBARH1 H JOIN LRTBARM1 M ON H.FAST_NO = M.FAST_NO " +
                    " WHERE H.APPR_STAT = '1' " +
                    " AND H.APLY_TYPE = '1' ";


                if (!"".Equals(StringUtil.toString(fastNo)))
                {
                    strSQL += " AND H.FAST_NO = :FAST_NO";
                    cmd.Parameters.Add("FAST_NO", fastNo);
                }

                if (!"".Equals(StringUtil.toString(policyNo)))
                {
                    strSQL += " AND M.POLICY_NO = :POLICY_NO";
                    cmd.Parameters.Add("POLICY_NO", policyNo);
                }

                if (!"".Equals(StringUtil.toString(policySeq)))
                {
                    strSQL += " AND M.POLICY_SEQ = :POLICY_SEQ";
                    cmd.Parameters.Add("POLICY_SEQ", policySeq);
                }

                if (!"".Equals(StringUtil.toString(idDup)))
                {
                    strSQL += " AND M.ID_DUP = :ID_DUP";
                    cmd.Parameters.Add("ID_DUP", idDup);
                }


                cmd.CommandText = strSQL;


                DbDataReader result = cmd.ExecuteReader();
                int applyNoId = result.GetOrdinal("APPLY_NO");
                int fastNoId = result.GetOrdinal("FAST_NO");
                int remitDateId = result.GetOrdinal("REMIT_DATE");
                int bankCodeId = result.GetOrdinal("BANK_CODE");
                int subBankId = result.GetOrdinal("SUB_BANK");
                int bankActId = result.GetOrdinal("BANK_ACT");
                int rcvNameId = result.GetOrdinal("RCV_NAME");
                int remitAmtId = result.GetOrdinal("REMIT_AMT");
                int failCodeId = result.GetOrdinal("FAIL_CODE");
                int entryId = result.GetOrdinal("ENTRY_ID");
                int updId = result.GetOrdinal("UPD_ID");
                int updDateId = result.GetOrdinal("UPD_DATE");
                int updTimeId = result.GetOrdinal("UPD_TIME");

                //logger.Info("updDate:" + updDateId);
                //int updTime = result.GetOrdinal("UPD_TIME");

                while (result.Read())
                {
                    ORTB006Model d = new ORTB006Model();
                    d.applyNo = StringUtil.toString(result.GetString(applyNoId));
                    d.fastNo = StringUtil.toString(result.GetString(fastNoId));
                    d.remitDate = result[remitDateId].ToString();
                    d.bankCode = StringUtil.toString(result.GetString(bankCodeId)) + StringUtil.toString(result.GetString(subBankId));
                    //  d.subBank = StringUtil.toString(result.GetString(subBankId));
                    d.bankAct = StringUtil.toString(result.GetString(bankActId));
                    d.rcvName = StringUtil.toString(result.GetString(rcvNameId));
                    d.remitAmt = result[remitAmtId].ToString();
                    d.failCode = StringUtil.toString(result.GetString(failCodeId));
                    d.entryId = StringUtil.toString(result.GetString(entryId));
                    d.updId = StringUtil.toString(result.GetString(updId));
                    d.updDate = result[updDateId].ToString();
                    d.updTime = result[updTimeId].ToString();
                    d.filler_20 = result["FILLER_20"]?.ToString(); 
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
                logger.Error(e.ToString);
                throw e;
            }

        }


        /// <summary>
        /// 查詢- 檢核 人工修改匯款失敗原因作業
        /// </summary>
        /// <returns></returns>
        public List<FRTBARHModel> qryForChk(string fastNo, string aplyType)
        {
            EacConnection con = new EacConnection();
            EacCommand cmd = new EacCommand();

            List<FRTBARHModel> rows = new List<FRTBARHModel>();

            string strSQL = "";
            try
            {
                con.ConnectionString = CommonUtil.GetEasycomConn();
                con.Open();
                cmd.Connection = con;

                strSQL += "SELECT M.APPLY_NO, M.FAST_NO " +
                    " FROM LRTBARH1 M " +
                    " WHERE M.APPR_STAT = '1' ";


                strSQL += " AND M.FAST_NO = :FAST_NO";
                cmd.Parameters.Add("FAST_NO", fastNo);

                if (!"".Equals(StringUtil.toString(aplyType)))
                {
                    strSQL += " AND M.APLY_TYPE = :APLY_TYPE";
                    cmd.Parameters.Add("APLY_TYPE", aplyType);
                }

                cmd.CommandText = strSQL;


                DbDataReader result = cmd.ExecuteReader();
                int applyNoId = result.GetOrdinal("APPLY_NO");
                int fastNoId = result.GetOrdinal("FAST_NO");
                

                while (result.Read())
                {
                    FRTBARHModel d = new FRTBARHModel();
                    d.applyNo = StringUtil.toString(result.GetString(applyNoId));
                    d.fastNo = StringUtil.toString(result.GetString(fastNoId));
                   
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
        /// 新增"FRTBARH0 快速付款匯款申請異動檔"
        /// </summary>
        /// <param name="applyNo"></param>
        /// <param name="procData"></param>
        /// <returns></returns>
        public int insertFRTBARH0(string applyNo, List<FRTBARHModel> procData)
        {
            int execCnt = 0;

            string[] nowStr = DateUtil.getCurChtDateTime(4).Split(' ');

            EacConnection con = new EacConnection();
            EacCommand cmd = new EacCommand();

            string strSQL = "";
            strSQL += "insert into LRTBARH1 ";
            strSQL += " (APPLY_NO, FAST_NO, APLY_TYPE, FAIL_CODE, APPR_STAT, STATUS, UPD_ID, UPD_DATE, UPD_TIME) ";
            strSQL += " VALUES ";
            strSQL += " (:APPLY_NO, :FAST_NO, :APLY_TYPE, :FAIL_CODE, :APPR_STAT, 'U', :UPD_ID, :UPD_DATE, :UPD_TIME) ";

            try
            {
                con.ConnectionString = CommonUtil.GetEasycomConn();
                con.Open();
                cmd.Connection = con;
                cmd.CommandText = strSQL;

                foreach (FRTBARHModel d in procData) {
                    cmd.Parameters.Clear();
                    cmd.Parameters.Add("APPLY_NO", applyNo);

                    cmd.Parameters.Add("FAST_NO", StringUtil.toString(d.fastNo));
                    cmd.Parameters.Add("APLY_TYPE", StringUtil.toString(d.aplyType));
                    cmd.Parameters.Add("FAIL_CODE", StringUtil.toString(d.failCode));
                    cmd.Parameters.Add("APPR_STAT", "1");
                    cmd.Parameters.Add("UPD_ID", StringUtil.toString(d.updId));
                    cmd.Parameters.Add("UPD_DATE", nowStr[0]);
                    cmd.Parameters.Add("UPD_TIME", nowStr[1]);

                    cmd.ExecuteNonQuery();
                    execCnt++;
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
        /// 異動"FRTBARH0 快速付款匯款申請異動檔"
        /// </summary>
        /// <param name="apprId"></param>
        /// <param name="apprStat"></param>
        /// <param name="procData"></param>
        /// <param name="conn"></param>
        /// <param name="transaction"></param>
        /// <returns></returns>
        public int updateFRTBARH0(string apprId, string apprStat, List<FRTBARHModel> procData, EacConnection conn, EacTransaction transaction)
        {
            logger.Info("updateFRTBARH0 begin! apprStat = " + apprStat);
            int execCnt = 0;

            string[] nowStr = DateUtil.getCurChtDateTime(4).Split(' ');

            EacCommand cmd = new EacCommand();

            List<FRTBARHModel> rows = new List<FRTBARHModel>();

            string strSQL = "";
            strSQL += "UPDATE LRTBARH1 " +
                    "  SET APPR_STAT = :APPR_STAT " +
                    " ,APPR_ID = :APPR_ID " +
                    " ,APPR_DATE = :APPR_DATE " +
                    " ,APPR_TIME = :APPR_TIME " +
                    " WHERE  1 = 1 " +
                    " AND APPLY_NO = :APPLY_NO " +
                    " AND FAST_NO = :FAST_NO ";

            logger.Info("strSQL:" + strSQL);

            try
            {
                cmd.Connection = conn;
                cmd.Transaction = transaction;
                cmd.CommandText = strSQL;

                foreach (FRTBARHModel d in procData)
                {
                    cmd.Parameters.Clear();

                    logger.Info("APPLY_NO = " + d.applyNo);
                    logger.Info("FAST_NO = " + d.fastNo);
                    logger.Info("APPR_STAT = " + apprStat);
                    logger.Info("APPR_ID = " + apprId);

                    cmd.Parameters.Add("APPR_STAT", apprStat);
                    cmd.Parameters.Add("APPR_ID", StringUtil.toString(apprId));
                    cmd.Parameters.Add("APPR_DATE", nowStr[0]);
                    cmd.Parameters.Add("APPR_TIME", nowStr[1]);

                    cmd.Parameters.Add("APPLY_NO", StringUtil.toString(d.applyNo));
                    cmd.Parameters.Add("FAST_NO", StringUtil.toString(d.fastNo));

                    cmd.ExecuteNonQuery();
                    execCnt++;
                }

                cmd.Dispose();
                cmd = null;
      
                logger.Info("updateFRTBARH0 end! ");
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