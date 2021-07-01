

using FAP.Web.AS400Models;
using FAP.Web.BO;
using FAP.Web.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.EasycomClient;

/// <summary>
/// FFAPYCK 應付票據檔(A系統)
/// ----------------------------------------------------
/// 修改歷程：20210125 daiyu
/// 需求單號：
/// 修改內容：修改OAP0050判斷支票檢核方式
/// </summary>
namespace FAP.Web.Daos
{
    public class FFAPYCKDao
    {
        static private NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();


        /// <summary>
        /// 「OAP0001上傳應付未付主檔/明細檔作業」資料檢核
        /// </summary>
        /// <param name="conn400"></param>
        /// <param name="checkNo"></param>
        /// <param name="acctAbbr"></param>
        /// <returns>
        /// 空白:正常
        /// 1:支票號碼+帳戶簡稱不存在
        /// 2:支票狀態有誤
        /// </returns>
        public OAP0001PYCK qryForOAP0001(EacConnection conn400, string acctAbbr, string checkNo)
        {
            logger.Info("qryForOAP0001 begin!");

            OAP0001PYCK pyck = new OAP0001PYCK();


            EacCommand cmdQ = new EacCommand();
            string strSQLQ = @"
SELECT PYCK.CHECK_NO, PYCK.ACCT_ABBR, PYCK.CHECK_STAT, PYCK.RE_CK_F, PYCK.AMOUNT, PYCK.CHECK_DATE, PYCK.RECEIVER
      ,GLPY.SQL_VHRDT
  FROM LFAPYCKK2 PYCK LEFT JOIN LGLGLPY4 GLPY on PYCK.APLY_NO = GLPY.APLY_NO AND PYCK.APLY_SEQ = GLPY.APLY_SEQ
    WHERE PYCK.ACCT_ABBR = :ACCT_ABBR
      AND PYCK.CHECK_NO = :CHECK_NO";

            try
            {
                cmdQ.Connection = conn400;
                cmdQ.CommandText = strSQLQ;

                cmdQ.Parameters.Clear();

                cmdQ.Parameters.Add("ACCT_ABBR", acctAbbr);
                cmdQ.Parameters.Add("CHECK_NO", checkNo);


                DbDataReader result = cmdQ.ExecuteReader();


                while (result.Read())
                {
                    pyck.checkNo = StringUtil.toString(result["CHECK_NO"]?.ToString());
                    pyck.checkShrt = StringUtil.toString(result["ACCT_ABBR"]?.ToString());
                    pyck.checkStat = StringUtil.toString(result["CHECK_STAT"]?.ToString());
                    pyck.reCkF = StringUtil.toString(result["RE_CK_F"]?.ToString());
                    pyck.checkAmt = StringUtil.toString(result["AMOUNT"]?.ToString());
                    pyck.checkDate = StringUtil.toString(result["CHECK_DATE"]?.ToString());
                    pyck.sqlVhrdt = StringUtil.toString(result["SQL_VHRDT"]?.ToString());
                    pyck.receiver = StringUtil.toString(result["RECEIVER"]?.ToString());
                }

                cmdQ.Dispose();
                cmdQ = null;

                logger.Info("qryForOAP0001 end!");

            }
            catch (Exception e)
            {
                logger.Error(e.ToString());
                throw e;
            }


            return pyck;

        }


        public OAP0050PYCK qryForOAP0050(EacConnection conn400, string acctAbbr, string checkNo)
        {
            logger.Info("qryForOAP0050 begin!");

            OAP0050PYCK pyck = new OAP0050PYCK();


            EacCommand cmdQ = new EacCommand();
            string strSQLQ = @"
SELECT PYCK.CHECK_NO, PYCK.ACCT_ABBR, PYCK.CHECK_STAT, PYCK.RE_CK_F, PYCK.AMOUNT, PYCK.CHECK_DATE, PYCK.RECEIVER
      ,GLPY.SQL_VHRDT, GLPY.SQL_PAIDTP
  FROM LFAPYCKK2 PYCK LEFT JOIN LGLGLPY4 GLPY on PYCK.APLY_NO = GLPY.APLY_NO AND PYCK.APLY_SEQ = GLPY.APLY_SEQ AND GLPY.ACT_CODE = 'D' AND GLPY.SQL_ACTNUM IN ('1258024', '2161027', '5990001', '5800461', '49900201')
    WHERE PYCK.ACCT_ABBR = :ACCT_ABBR
      AND PYCK.CHECK_NO = :CHECK_NO";

            try
            {
                cmdQ.Connection = conn400;
                cmdQ.CommandText = strSQLQ;

                cmdQ.Parameters.Clear();

                cmdQ.Parameters.Add("ACCT_ABBR", acctAbbr);
                cmdQ.Parameters.Add("CHECK_NO", checkNo);


                DbDataReader result = cmdQ.ExecuteReader();


                while (result.Read())
                {
                    pyck.checkNo = StringUtil.toString(result["CHECK_NO"]?.ToString());
                    pyck.checkShrt = StringUtil.toString(result["ACCT_ABBR"]?.ToString());
                    pyck.checkStat = StringUtil.toString(result["CHECK_STAT"]?.ToString());
                    pyck.reCkF = StringUtil.toString(result["RE_CK_F"]?.ToString());
                    pyck.checkAmt = StringUtil.toString(result["AMOUNT"]?.ToString());
                    pyck.checkDate = StringUtil.toString(result["CHECK_DATE"]?.ToString());
                    pyck.sqlVhrdt = StringUtil.toString(result["SQL_VHRDT"]?.ToString());
                    pyck.receiver = StringUtil.toString(result["RECEIVER"]?.ToString());
                    pyck.sqlPaidtp = StringUtil.toString(result["SQL_PAIDTP"]?.ToString());
                }

                cmdQ.Dispose();
                cmdQ = null;

                logger.Info("qryForOAP0050 end!");

            }
            catch (Exception e)
            {
                logger.Error(e.ToString());
                throw e;
            }


            return pyck;

        }



        public List<VeCleanModel> qryCheckPolicy(EacConnection conn400, string bankCode, string checkNo)
        {
            logger.Info("A-qryCheckPolicy begin!");
            List<VeCleanModel> rows = new List<VeCleanModel>();

            EacCommand cmdQ = new EacCommand();
            string strSQLMcrf = @"
SELECT distinct PYCK.CHECK_NO, PYCK.ACCT_ABBR, PYCK.RECEIVER, PYCK.AMOUNT, PYCK.CHECK_DATE,
      PYCK.APLY_NO, PYCK.APLY_SEQ,
      PAYH.INS_NO POLICY_NO , PAYH.INS_SEQ POLICY_SEQ ,'' ID_DUP, PAYH.AMOUNT MAIN_AMT,
      SUBSTR(CODE.TEXT,1,3) TEXT
  FROM LFAPYCKK2 PYCK JOIN LFAPAYHK4 PAYH on PYCK.APLY_NO = PAYH.APLY_NO AND PYCK.APLY_SEQ = PAYH.APLY_SEQ
                 LEFT JOIN LPMCODE1 CODE on CODE.GROUP_ID = 'O_PAID_CD' AND CODE.SRCE_FROM = 'AP' 
                                        AND ((PAYH.SRCE_KIND <> '37' AND SUBSTR(PAYH.SRCE_FROM, 1, 2) || SUBSTR(PAYH.SRCE_KIND, 1, 2) = CODE.REF_NO)
                                            OR
                                             (PAYH.SRCE_KIND = '37' AND SUBSTR(PAYH.SRCE_FROM, 1, 2) || SUBSTR(PAYH.SRCE_KIND, 1, 2) || '-' || SUBSTR(PAYH.APLY_NO, 1, 1)= CODE.REF_NO)
                                            )
    WHERE PYCK.ACCT_ABBR = :ACCT_ABBR
      AND PYCK.CHECK_NO = :CHECK_NO"
;


            try
            {
                cmdQ.Connection = conn400;
                cmdQ.CommandText = strSQLMcrf;

                cmdQ.Parameters.Clear();

                cmdQ.Parameters.Add("ACCT_ABBR", bankCode);
                cmdQ.Parameters.Add("CHECK_NO", checkNo);


                DbDataReader result = cmdQ.ExecuteReader();

                while (result.Read())
                {
                    VeCleanModel d = new VeCleanModel();
                    d.system = "A";
                    d.check_no = StringUtil.toString(result["CHECK_NO"]?.ToString());
                    d.check_acct_short = StringUtil.toString(result["ACCT_ABBR"]?.ToString());
                    d.paid_name = StringUtil.toString(result["RECEIVER"]?.ToString());
                    d.check_amt = StringUtil.toString(result["AMOUNT"]?.ToString());
                    d.check_date = result["CHECK_DATE"]?.ToString();
                    d.policy_no = StringUtil.toString(result["POLICY_NO"]?.ToString());
                    d.policy_seq = StringUtil.toString(result["POLICY_SEQ"]?.ToString());
                    d.id_dup = StringUtil.toString(result["ID_DUP"]?.ToString());
                    d.main_amt = StringUtil.toString(result["MAIN_AMT"]?.ToString());
                    d.o_paid_cd = StringUtil.toString(result["TEXT"]?.ToString());
                    d.aply_no = StringUtil.toString(result["APLY_NO"]?.ToString());
                    d.aply_seq = StringUtil.toString(result["APLY_SEQ"]?.ToString());

                    rows.Add(d);
                }

                cmdQ.Dispose();
                cmdQ = null;

                logger.Info("A-qryCheckPolicy end!");

            }
            catch (Exception e)
            {
                logger.Error(e.ToString());
                throw e;
            }


            return rows;

        }


    }
}