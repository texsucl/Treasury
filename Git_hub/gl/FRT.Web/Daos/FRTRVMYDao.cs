using FRT.Web.AS400Models;
using FRT.Web.BO;
using FRT.Web.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.EasycomClient;
using System.Linq;
using System.Web;

namespace FRT.Web.Daos
{
    public class FRTRVMYDao
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
        public List<ORTB014Model> qryForORTB014(string bankNo, string bankAct, string qDateB, string qDateE)
        {
            EacConnection con = new EacConnection();
            EacCommand cmd = new EacCommand();

            List<ORTB014Model> rows = new List<ORTB014Model>();

            bool bbankNo = string.IsNullOrEmpty(bankNo);
            bool bbankAct = string.IsNullOrEmpty(bankAct);
            bool bqDateB = string.IsNullOrEmpty(qDateB);
            bool bqDateE = string.IsNullOrEmpty(qDateE);

            string dateB = DateUtil.ADDateToChtDate(qDateB);
            string dateE = DateUtil.ADDateToChtDate(qDateE);

            string strSQL = "";
            try
            {
                con.ConnectionString = CommonUtil.GetEasycomConn();
                con.Open();
                cmd.Connection = con;

                strSQL += "SELECT M.BANK_NO, M.BANK_ACT, M.FAIL_CODE, M.ENTRY_DATE, M.FEE_SEQN, M.REJECT_PAY, M.UPD_DATE, M.UPD_TIMEN, M.UPD_ID, " +
                    " M.APPR_DATE, M.APPR_TIMEN, M.APPR_ID, M.FILLER_10, M.FILLER_20, M.FILLER_08N" +
                    ", CODE.TEXT " +
                    " FROM LRTRVMY1 M LEFT JOIN LPMCODE1 CODE ON M.FAIL_CODE = CODE.REF_NO AND CODE.SRCE_FROM = 'RT' AND CODE.GROUP_ID = 'FAIL-CODE'" +
                    " WHERE 1=1 ";

                if (!bbankNo)
                {
                    strSQL += " AND M.BANK_NO = :BANK_NO";
                    cmd.Parameters.Add("BANK_NO", bankNo);
                }

                if (!bbankAct)
                {
                    strSQL += " AND M.BANK_ACT = :BANK_ACT";
                    cmd.Parameters.Add("BANK_ACT", bankAct);
                }

                if (!bqDateB)
                {
                    strSQL += " AND M.ENTRY_DATE >= :DATEB";
                    cmd.Parameters.Add("BANK_ACT", dateB);
                }

                if (!bqDateE)
                {
                    strSQL += " AND M.ENTRY_DATE <= :DATEE";
                    cmd.Parameters.Add("BANK_ACT", dateE);
                }

                strSQL += " ORDER BY M.BANK_ACT";

                logger.Info("strSQ:" + strSQL);
                cmd.CommandText = strSQL;

                DbDataReader result = cmd.ExecuteReader();
                int bankNoId = result.GetOrdinal("BANK_NO");
                int bankActId = result.GetOrdinal("BANK_ACT");
                int failCode = result.GetOrdinal("FAIL_CODE");
                int entryDateId = result.GetOrdinal("ENTRY_DATE");
                int feeSeqnId = result.GetOrdinal("FEE_SEQN");
                int rejectPayId = result.GetOrdinal("REJECT_PAY");
                int updDateId = result.GetOrdinal("UPD_DATE");
                int updTimeNId = result.GetOrdinal("UPD_TIMEN");
                int updIdId = result.GetOrdinal("UPD_ID");
                int apprDateId = result.GetOrdinal("APPR_DATE");
                int apprTimeNId = result.GetOrdinal("APPR_TIMEN");
                int apprIdId = result.GetOrdinal("APPR_ID");
                int filler10Id = result.GetOrdinal("FILLER_10");
                int filler20Id = result.GetOrdinal("FILLER_20");
                int filler08NId = result.GetOrdinal("FILLER_08N");
                int textId = result.GetOrdinal("TEXT");

                while (result.Read())
                {
                    ORTB014Model d = new ORTB014Model();
                    
                    d.bankNo = StringUtil.toString(result.GetString(bankNoId));
                    d.bankAct = StringUtil.toString(result.GetString(bankActId));
                    d.failCode = StringUtil.toString(result.GetString(failCode));
                    d.entryDate = result[entryDateId].ToString();
                    d.feeSeqn = StringUtil.toString(result.GetString(feeSeqnId));
                    d.rejectPay = StringUtil.toString(result.GetString(rejectPayId));
                    d.updDate = result[updDateId].ToString();
                    d.updTimeN = result[updTimeNId].ToString();
                    d.updId = StringUtil.toString(result.GetString(updIdId));
                    d.apprDate = result[apprDateId].ToString();
                    d.apprTimeN = result[apprTimeNId].ToString();
                    d.apprId = StringUtil.toString(result.GetString(apprIdId));
                    d.filler10 = StringUtil.toString(result.GetString(filler10Id));
                    d.filler20 = StringUtil.toString(result.GetString(filler20Id));
                    d.filler08N = result[filler08NId].ToString();

                    d.failCodeDesc = result.GetString(textId);

                    if (("".Equals(StringUtil.toString(d.rejectPay)) && "Y".Equals(StringUtil.toString(d.filler10)))
                        || ("Y".Equals(StringUtil.toString(d.rejectPay)) && "X".Equals(StringUtil.toString(d.filler10).ToUpper())))
                        d.status = "2";
                    else
                        d.status = "1";

                    d.tempId = d.bankNo + d.bankAct + d.failCode;

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


        public List<ORTB014Model> qryForORTB014A()
        {
            EacConnection con = new EacConnection();
            EacCommand cmd = new EacCommand();

            List<ORTB014Model> rows = new List<ORTB014Model>();



            string strSQL = "";
            try
            {
                con.ConnectionString = CommonUtil.GetEasycomConn();
                con.Open();
                cmd.Connection = con;

                strSQL += "SELECT 'R' AS TYPE, M.BANK_NO, M.BANK_ACT, M.FAIL_CODE, M.REJECT_PAY " +
                    ", M.UPD_DATE, M.UPD_TIMEN, M.UPD_ID, M.FILLER_10 " +
                    ", CODE.TEXT " +
                    " FROM LRTRVMY1 M LEFT JOIN LPMCODE1 CODE ON M.FAIL_CODE = CODE.REF_NO " +
                    " WHERE 1=1 " +
                    " AND CODE.SRCE_FROM = 'RT' AND CODE.GROUP_ID = 'FAIL-CODE' " +
                    " AND M.REJECT_PAY = '' AND M.FILLER_10 = 'Y' " +
                    " UNION " +
                    "SELECT 'S' AS TYPE, M.BANK_NO, M.BANK_ACT, M.FAIL_CODE, M.REJECT_PAY " +
                    ", M.UPD_DATE, M.UPD_TIMEN, M.UPD_ID, M.FILLER_10 " +
                    ", CODE.TEXT " +
                    " FROM LRTRVMY1 M LEFT JOIN LPMCODE1 CODE ON M.FAIL_CODE = CODE.REF_NO " +
                    " WHERE 1=1 " +
                    " AND CODE.SRCE_FROM = 'RT' AND CODE.GROUP_ID = 'FAIL-CODE' " +
                    " AND M.REJECT_PAY = 'Y' AND M.FILLER_10 = 'X' ";


                logger.Info("strSQ:" + strSQL);
                cmd.CommandText = strSQL;

                DbDataReader result = cmd.ExecuteReader();
                int typeId = result.GetOrdinal("TYPE");
                int bankNoId = result.GetOrdinal("BANK_NO");
                int bankActId = result.GetOrdinal("BANK_ACT");
                int failCode = result.GetOrdinal("FAIL_CODE");
                int rejectPayId = result.GetOrdinal("REJECT_PAY");
                int updDateId = result.GetOrdinal("UPD_DATE");
                int updTimeNId = result.GetOrdinal("UPD_TIMEN");
                int updIdId = result.GetOrdinal("UPD_ID");
                int filler10Id = result.GetOrdinal("FILLER_10");
                int textId = result.GetOrdinal("TEXT");

                while (result.Read())
                {
                    ORTB014Model d = new ORTB014Model();
                    string type = StringUtil.toString(result.GetString(typeId));
                    d.status = type == "R" ? "維護為拒絕付款" : "拒絕付款維護為停用";
                    d.bankNo = StringUtil.toString(result.GetString(bankNoId));
                    d.bankAct = StringUtil.toString(result.GetString(bankActId));
                    d.failCode = StringUtil.toString(result.GetString(failCode));
                    d.rejectPay = StringUtil.toString(result.GetString(rejectPayId));
                    d.updDate = result[updDateId].ToString();
                    d.updTimeN = result[updTimeNId].ToString();
                    d.updId = StringUtil.toString(result.GetString(updIdId));
                    d.filler10 = StringUtil.toString(result.GetString(filler10Id));

                    d.failCodeDesc = result.GetString(textId);

                    d.tempId = d.bankNo + d.bankAct + d.failCode;

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
        /// ORTB014 申請新增拒絕付款 或 停用
        /// </summary>
        /// <param name="rejectPayO"></param>
        /// <param name="filler10O"></param>
        /// <param name="procData"></param>
        /// <param name="conn"></param>
        public void UpdForORTB014(string rejectPayO, string filler10O, ORTB014Model procData, EacConnection conn)
        {
            DateTime uDt = DateTime.Now;

            string strSQL = "";
            strSQL += @"
UPDATE LRTRVMY1 
  SET REJECT_PAY = :REJECT_PAY
     ,UPD_DATE  = :UPD_DATE 
     ,UPD_TIMEN = :UPD_TIMEN
     ,UPD_ID   = :UPD_ID  
     ,FILLER_10  = :FILLER_10 
 WHERE BANK_NO = :BANK_NO 
  AND BANK_ACT = :BANK_ACT
  AND REJECT_PAY = :REJECT_PAY_O
  AND FILLER_10 = :FILLER_10_O ";

            EacCommand cmd = new EacCommand();
            cmd.Connection = conn;

            cmd.Parameters.Add("REJECT_PAY", StringUtil.toString(procData.rejectPay));
            cmd.Parameters.Add("UPD_DATE", (uDt.Year - 1911).ToString() + uDt.Month.ToString().PadLeft(2, '0') + uDt.Day.ToString().PadLeft(2, '0'));
            cmd.Parameters.Add("UPD_TIMEN", uDt.ToString("HHmmssff"));
            cmd.Parameters.Add("UPD_ID", StringUtil.toString(procData.updId));

            cmd.Parameters.Add("FILLER_10", StringUtil.toString(procData.filler10));

            cmd.Parameters.Add("BANK_NO", procData.bankNo);
            cmd.Parameters.Add("BANK_ACT", procData.bankAct);
            cmd.Parameters.Add("REJECT_PAY_O", rejectPayO);
            cmd.Parameters.Add("FILLER_10_O", filler10O);

            cmd.CommandText = strSQL;
            cmd.ExecuteNonQuery();

            cmd.Dispose();
            cmd = null;
        }


        /// <summary>
        /// ORTB014A 覆核新增拒絕付款 或 停用
        /// </summary>
        /// <param name="procData"></param>
        /// <param name="conn"></param>
        public void UpdForORTB014Appr(string apprStat, ORTB014Model procData, EacConnection conn)
        {
            DateTime uDt = DateTime.Now;

            string strSQL = "";
            if ("2".Equals(apprStat))
            strSQL += @"
UPDATE LRTRVMY1 
  SET REJECT_PAY = :REJECT_PAY
     ,APPR_DATE = :APPR_DATE  
     ,APPR_TIMEN = :APPR_TIMEN  
     ,APPR_ID = :APPR_ID  
     ,FILLER_10  = :FILLER_10 
 WHERE BANK_NO = :BANK_NO 
  AND BANK_ACT = :BANK_ACT
  AND FAIL_CODE = :FAIL_CODE";
            else
                strSQL += @"
UPDATE LRTRVMY1 
  SET FILLER_10  = :FILLER_10 
 WHERE BANK_NO = :BANK_NO 
  AND BANK_ACT = :BANK_ACT
  AND FAIL_CODE = :FAIL_CODE";

            EacCommand cmd = new EacCommand();
            cmd.Connection = conn;

            if ("2".Equals(apprStat)) {
                cmd.Parameters.Add("REJECT_PAY", StringUtil.toString(procData.rejectPay));
                cmd.Parameters.Add("APPR_DATE", procData.apprDate);
                cmd.Parameters.Add("APPR_TIMEN", procData.apprTimeN);
                cmd.Parameters.Add("APPR_ID", procData.apprId);
            }
                
            cmd.Parameters.Add("FILLER_10", StringUtil.toString(procData.filler10));

            cmd.Parameters.Add("BANK_NO", procData.bankNo);
            cmd.Parameters.Add("BANK_ACT", procData.bankAct);
            cmd.Parameters.Add("FAIL_CODE", procData.failCode);

            cmd.CommandText = strSQL;
            cmd.ExecuteNonQuery();

            cmd.Dispose();
            cmd = null;
        }

    }
}