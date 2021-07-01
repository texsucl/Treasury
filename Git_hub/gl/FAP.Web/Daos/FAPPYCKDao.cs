

using FAP.Web.AS400Models;
using FAP.Web.BO;
using FAP.Web.ViewModels;
using Fubon.Utility;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.EasycomClient;
using System.Linq;

/// <summary>
/// FAPPYCK 應付票據檔(F系統)
/// ----------------------------------------------------
/// 修改歷程：20210125 daiyu
/// 需求單號：
/// 修改內容：修改OAP0050判斷支票檢核方式
/// </summary>
namespace FAP.Web.Daos
{
    public class FAPPYCKDao
    {
        static private NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        /// <summary>
        /// 「OAP0001上傳應付未付主檔/明細檔作業」資料檢核
        /// </summary>
        /// <param name="conn400"></param>
        /// <param name="checkNo"></param>
        /// <param name="bankCode"></param>
        /// <returns>
        /// 空白:正常
        /// 1:支票號碼+帳戶簡稱不存在
        /// 2:支票狀態有誤
        /// </returns>
        public OAP0001PYCK qryForOAP0001(EacConnection conn400, string bankCode, string checkNo)
        {
            logger.Info("qryForOAP0001 begin!");

            OAP0001PYCK pyck = new OAP0001PYCK();

            EacCommand cmdQ = new EacCommand();
            string strSQLQ = @"
SELECT PYCK.CHECK_NO, PYCK.BANK_CODE, PYCK.CHECK_STAT, PYCK.AMOUNT, PYCK.CHECK_YY, PYCK.CHECK_MM, PYCK.CHECK_DD, PYCK.RECEIVER
      , GLPY.SQL_VHRDT
      , CKER.DEL_CODE
  FROM LAPPYCK3 PYCK LEFT JOIN LGLGLPY4 GLPY on PYCK.APLY_NO = GLPY.APLY_NO AND PYCK.APLY_SEQ = GLPY.APLY_SEQ
                     LEFT JOIN LAPCKER1 CKER on PYCK.BANK_CODE = CKER.BANK_CODE and PYCK.CHECK_NO = CKER.CHECK_NO
    WHERE PYCK.BANK_CODE = :BANK_CODE 
      AND PYCK.CHECK_NO = :CHECK_NO";

            try
            {
                cmdQ.Connection = conn400;
                cmdQ.CommandText = strSQLQ;

                cmdQ.Parameters.Clear();

                cmdQ.Parameters.Add("ACCT_ABBR", bankCode);
                cmdQ.Parameters.Add("CHECK_NO", checkNo);


                DbDataReader result = cmdQ.ExecuteReader();

                while (result.Read())
                {
                    pyck.checkNo = StringUtil.toString(result["CHECK_NO"]?.ToString());
                    pyck.checkShrt = StringUtil.toString(result["BANK_CODE"]?.ToString());
                    pyck.checkStat = StringUtil.toString(result["CHECK_STAT"]?.ToString());
                    pyck.checkAmt = StringUtil.toString(result["AMOUNT"]?.ToString());
                    pyck.checkDate = result["CHECK_YY"]?.ToString() 
                        + result["CHECK_MM"]?.ToString().PadLeft(2, '0') 
                        + result["CHECK_DD"]?.ToString().PadLeft(2, '0');
                    pyck.sqlVhrdt = StringUtil.toString(result["SQL_VHRDT"]?.ToString());
                    pyck.delCode = StringUtil.toString(result["DEL_CODE"]?.ToString());
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


        public OAP0050PYCK qryForOAP0050(EacConnection conn400, string bankCode, string checkNo)
        {
            logger.Info("qryForOAP0050 begin!");

            OAP0050PYCK pyck = new OAP0050PYCK();

            EacCommand cmdQ = new EacCommand();
            string strSQLQ = @"
SELECT PYCK.CHECK_NO, PYCK.BANK_CODE, PYCK.CHECK_STAT, PYCK.AMOUNT, PYCK.CHECK_YY, PYCK.CHECK_MM, PYCK.CHECK_DD, PYCK.RECEIVER
      , GLPY.SQL_VHRDT, GLPY.SQL_PAIDTP
      , CKER.DEL_CODE, CKER.CHECK_NO DEL_CHECK_NO
  FROM LAPPYCK3 PYCK LEFT JOIN LGLGLPY4 GLPY on PYCK.APLY_NO = GLPY.APLY_NO AND PYCK.APLY_SEQ = GLPY.APLY_SEQ AND GLPY.ACT_CODE = 'D' AND GLPY.SQL_ACTNUM IN ('1258024', '2161027', '5990001', '5800461', '49900201')
                     LEFT JOIN LAPCKER1 CKER on PYCK.BANK_CODE = CKER.BANK_CODE and PYCK.CHECK_NO = CKER.CHECK_NO
    WHERE PYCK.BANK_CODE = :BANK_CODE 
      AND PYCK.CHECK_NO = :CHECK_NO";

            try
            {
                cmdQ.Connection = conn400;
                cmdQ.CommandText = strSQLQ;

                cmdQ.Parameters.Clear();

                cmdQ.Parameters.Add("ACCT_ABBR", bankCode);
                cmdQ.Parameters.Add("CHECK_NO", checkNo);


                DbDataReader result = cmdQ.ExecuteReader();

                while (result.Read())
                {
                    pyck.checkNo = StringUtil.toString(result["CHECK_NO"]?.ToString());
                    pyck.checkShrt = StringUtil.toString(result["BANK_CODE"]?.ToString());
                    pyck.checkStat = StringUtil.toString(result["CHECK_STAT"]?.ToString());
                    pyck.checkAmt = StringUtil.toString(result["AMOUNT"]?.ToString());
                    pyck.checkDate = result["CHECK_YY"]?.ToString()
                        + result["CHECK_MM"]?.ToString().PadLeft(2, '0')
                        + result["CHECK_DD"]?.ToString().PadLeft(2, '0');
                    pyck.sqlVhrdt = StringUtil.toString(result["SQL_VHRDT"]?.ToString());
                    pyck.delCode = StringUtil.toString(result["DEL_CODE"]?.ToString());
                    pyck.receiver = StringUtil.toString(result["RECEIVER"]?.ToString());
                    pyck.sqlPaidtp = StringUtil.toString(result["SQL_PAIDTP"]?.ToString());
                    pyck.checkNo_del = StringUtil.toString(result["DEL_CHECK_NO"]?.ToString());
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
            logger.Info("qryCheckPolicy begin!");

            List<VeCleanModel> dataList = new List<VeCleanModel>();
            dataList = qryCheckPoliMcrf(conn400, bankCode, checkNo);

            if (dataList.Count == 0) {
                dataList = qryCheckPoliNbas(conn400, bankCode, checkNo);
            }

            return dataList;

        }



        public List<VeCleanModel> qryCheckPoliMcrf(EacConnection conn400, string bankCode, string checkNo)
        {
            logger.Info("qryCheckPoliMcrf begin!");

            List<VeCleanModel> rows = new List<VeCleanModel>();

            EacCommand cmdQ = new EacCommand();
            string strSQLMcrf = @"
SELECT distinct PYCK.CHECK_NO, PYCK.BANK_CODE, PYCK.RECEIVER, PYCK.AMOUNT, PYCK.CHECK_YY, PYCK.CHECK_MM, PYCK.CHECK_DD, PYCK.SRCE_FROM,
       PYCK.APLY_NO, PYCK.APLY_SEQ,
       MCRF.POLICY_NO , MCRF.POLICY_SEQ , MCRF.DUP_ID ID_DUP, MCRF.PAID_TAL MAIN_AMT,  MCRF.MEMBER_ID,  MCRF.CHANGE_ID, 
       SUBSTR(CODE.TEXT,1,3) TEXT
  FROM LAPPYCK3 PYCK JOIN LAPMCRF5 MCRF on PYCK.APLY_NO = MCRF.APLY_NO AND PYCK.APLY_SEQ = MCRF.APLY_SEQ
                LEFT JOIN LPMCODE1 CODE on CODE.GROUP_ID = 'O_PAID_CD' AND CODE.SRCE_FROM = 'AP' AND PYCK.SRCE_FROM = CODE.REF_NO
    WHERE PYCK.BANK_CODE = :BANK_CODE 
      AND PYCK.CHECK_NO = :CHECK_NO";


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
                    d.system = "F";
                    d.check_no = StringUtil.toString(result["CHECK_NO"]?.ToString());
                    d.check_acct_short = StringUtil.toString(result["BANK_CODE"]?.ToString());
                    d.paid_name = StringUtil.toString(result["RECEIVER"]?.ToString());
                    d.check_amt = StringUtil.toString(result["AMOUNT"]?.ToString());
                    d.check_date = result["CHECK_YY"]?.ToString()
                        + result["CHECK_MM"]?.ToString().PadLeft(2, '0')
                        + result["CHECK_DD"]?.ToString().PadLeft(2, '0');
                    d.policy_no = StringUtil.toString(result["POLICY_NO"]?.ToString());
                    d.policy_seq = StringUtil.toString(result["POLICY_SEQ"]?.ToString());
                    d.id_dup = StringUtil.toString(result["ID_DUP"]?.ToString());
                    d.main_amt = StringUtil.toString(result["MAIN_AMT"]?.ToString());
                    d.o_paid_cd =StringUtil.toString(result["TEXT"]?.ToString());
                    d.member_id = StringUtil.toString(result["MEMBER_ID"]?.ToString());
                    d.change_id = StringUtil.toString(result["CHANGE_ID"]?.ToString());
                    d.aply_no = StringUtil.toString(result["APLY_NO"]?.ToString());
                    d.aply_seq = StringUtil.toString(result["APLY_SEQ"]?.ToString());

                    rows.Add(d);
                }

                cmdQ.Dispose();
                cmdQ = null;

                logger.Info("qryCheckPoliMcrf end!");

            }
            catch (Exception e)
            {
                logger.Error(e.ToString());
                throw e;
            }


            return rows;

        }


        public List<VeCleanModel> qryCheckPoliNbas(EacConnection conn400, string bankCode, string checkNo)
        {
            logger.Info("qryCheckPoliNbas begin!");

            List<VeCleanModel> rows = new List<VeCleanModel>();
            EacCommand cmdQ = new EacCommand();

            string strSQL = @"
SELECT distinct PYCK.CHECK_NO, PYCK.BANK_CODE, PYCK.RECEIVER, PYCK.AMOUNT, PYCK.CHECK_YY, PYCK.CHECK_MM, PYCK.CHECK_DD, PYCK.SRCE_FROM,
       PYCK.APLY_NO, PYCK.APLY_SEQ,
       NBAS.POL_NUM POLICY_NO, NBAS.POL_SEQ POLICY_SEQ, NBAS.ID_DUP ID_DUP, NBAS.PAY_AMT MAIN_AMT,
       SUBSTR(CODE.TEXT,1,3) TEXT
  FROM LAPPYCK3 PYCK JOIN LAPNBAS8 NBAS on PYCK.APLY_NO = NBAS.APLY_NO AND PYCK.APLY_SEQ = NBAS.APLY_SEQ
                LEFT JOIN LPMCODE1 CODE on CODE.GROUP_ID = 'O_PAID_CD' AND CODE.SRCE_FROM = 'AP' AND PYCK.SRCE_FROM = CODE.REF_NO
    WHERE PYCK.BANK_CODE = :BANK_CODE 
      AND PYCK.CHECK_NO = :CHECK_NO
 ORDER BY NBAS.POL_NUM,  NBAS.POL_SEQ, NBAS.ID_DUP ";

            try
            {
                cmdQ.Connection = conn400;
                cmdQ.CommandText = strSQL;

                cmdQ.Parameters.Clear();

                cmdQ.Parameters.Add("ACCT_ABBR", bankCode);
                cmdQ.Parameters.Add("CHECK_NO", checkNo);


                DbDataReader result = cmdQ.ExecuteReader();


                while (result.Read())
                {
                    VeCleanModel d = new VeCleanModel();
                    d.system = "F";
                    d.check_no = StringUtil.toString(result["CHECK_NO"]?.ToString());
                    d.check_acct_short = StringUtil.toString(result["BANK_CODE"]?.ToString());
                    d.paid_name = StringUtil.toString(result["RECEIVER"]?.ToString());
                    d.check_amt = StringUtil.toString(result["AMOUNT"]?.ToString());
                    d.check_date = result["CHECK_YY"]?.ToString()
                        + result["CHECK_MM"]?.ToString().PadLeft(2, '0')
                        + result["CHECK_DD"]?.ToString().PadLeft(2, '0');
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

                logger.Info("qryCheckPoliNbas end!");

            }
            catch (Exception e)
            {
                logger.Error(e.ToString());
                throw e;
            }


            return rows.GroupBy(o => new { o.check_no, o.check_acct_short, o.paid_name, o.check_amt, o.check_date, o.policy_no, o.policy_seq, o.id_dup, o.o_paid_cd })
                                                .Select(group => new VeCleanModel
                                                {
                                                    check_no = group.Key.check_no,
                                                    check_acct_short = group.Key.check_acct_short,
                                                    paid_name = group.Key.paid_name,
                                                    check_amt = group.Key.check_amt,
                                                    check_date = group.Key.check_date,
                                                    policy_no = group.Key.policy_no,
                                                    policy_seq = group.Key.policy_seq,
                                                    id_dup = group.Key.id_dup,
                                                    o_paid_cd = group.Key.o_paid_cd,
                                                    main_amt = group.Sum(x => Convert.ToInt64(x.main_amt)).ToString()
                                                }).ToList<VeCleanModel>();

        }

    }
}