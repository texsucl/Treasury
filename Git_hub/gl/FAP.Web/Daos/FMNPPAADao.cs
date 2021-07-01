

using FAP.Web.AS400Models;
using FAP.Web.BO;
using FAP.Web.Models;
using FAP.Web.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.EasycomClient;

/// <summary>
/// FMNPPAA  支票退回回存／外幣無匯款帳號主檔  
/// ------------------------------------------
/// modify by daiyu 20191007
/// 需求單號:
/// 逾期未兌領AML，於AS400主動通知執行完畢後，產製【逾期未兌領支票-疑似禁制名單】
/// ------------------------------------------
/// 修改歷程：20191025 daiyu
/// 需求單號：201910240295-00
/// 修改內容：1.新增FAP_VE_TRACE時，若保局範圍沒有指定...以"0"寫入
///           2.【OAP0004 指定保局範圍作業】若查詢條件"保局範圍=0"時，要可以查到
///            2.1 null值
///            2.2 空白
///            2.3 "0"
/// ------------------------------------------
/// 修改歷程：20191128 daiyu
/// 需求單號：201910290100-00
/// 修改內容：1.修改要被保人姓名長度與資料庫長度一致時，AS400欄位少"OE、OF"問題
///           2.回寫制裁名單檢核結果
/// ------------------------------------------
/// 修改歷程：20200819 daiyu
/// 需求單號：202008120153-00
/// 修改內容：1.【OAP0042 電訪暨簡訊標準設定作業】報表查詢
/// ------------------------------------------
/// </summary>
namespace FAP.Web.Daos
{
    public class FMNPPAADao
    {
        static private NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public List<TelDispatchRptModel> qryForTelDispatchRpt(EacConnection conn400, List<TelDispatchRptModel> rows)
        {
            logger.Info("qryForTelDispatchRpt begin size:" + rows.Count);


            List<TelDispatchRptModel> dataList = new List<TelDispatchRptModel>();

            string strSQLA = @"
SELECT PPAA.CHECK_NO, PPAA.SYSTEM, PPAA.CHECK_SHRT
      , PPAA.STATUS, PPAA.APPL_ID, PPAA.APPL_NAME, PPAA.INS_ID, PPAA.INS_NAME 
      , POLIP.SEC_STAT, POLIP.BIRTH_DATE, POLIP.COLLECTOR
      , CUST.BIRTH_DATE APPL_BIRTH_DATE
  FROM LMNPPAA5 PPAA LEFT JOIN LCVPOLIP POLIP ON PPAA.POLICY_NO = POLIP.INS_ID AND PPAA.POLICY_SEQ = POLIP.SEQ_NO
                     LEFT JOIN LNBCUSTP CUST ON POLIP.APPL_ID = CUST.ID_NO
    WHERE PPAA.PGM_TYPE= '2'
      AND PPAA.SYSTEM = :SYSTEM
      AND PPAA.CHECK_NO = :CHECK_NO 
      AND PPAA.CHECK_SHRT = :CHECK_SHRT
";

            string strSQLF = @"
SELECT PPAA.CHECK_NO, PPAA.SYSTEM, PPAA.CHECK_SHRT
      , PPAA.STATUS, PPAA.APPL_ID, PPAA.APPL_NAME, PPAA.INS_ID, PPAA.INS_NAME 
      , POLI.SYSMARK, POLI.SECRT_YN, POLI.BIRTH_YY, POLI.BIRTH_MM, POLI.BIRTH_DD, POLI.ABIRTH_YY, POLI.ABIRTH_MM, POLI.ABIRTH_DD, POLI.SRVMAN_ID
  FROM LMNPPAA5 PPAA LEFT JOIN LPMPOLI1 POLI ON PPAA.POLICY_NO = POLI.POLICY_NO AND PPAA.ID_DUP = POLI.ID_DUP AND PPAA.POLICY_SEQ = POLI.POLICY_SEQ
    WHERE PPAA.PGM_TYPE= '2'
      AND PPAA.SYSTEM = :SYSTEM
      AND PPAA.CHECK_NO = :CHECK_NO 
      AND PPAA.CHECK_SHRT = :CHECK_SHRT
";

            try
            {
                FMNPPADDao fMNPPADDao = new FMNPPADDao();

                int i = 0;
                foreach (TelDispatchRptModel d in rows)
                {
                    d.sec_stat = "N";
                    EacCommand cmd = new EacCommand();
                    cmd.Connection = conn400;

                    i++;
                    if (i % 500 == 0)
                        logger.Info(i);


                    if ("A".Equals(d.system))
                        cmd.CommandText = strSQLA;
                    else
                        cmd.CommandText = strSQLF;

                    cmd.Parameters.Clear();

                    cmd.Parameters.Add("SYSTEM", d.system);
                    cmd.Parameters.Add("CHECK_NO", d.check_no);
                    cmd.Parameters.Add("CHECK_SHRT", d.check_acct_short);
                    //cmd.Parameters.Add("STATUS", status);

                    try
                    {
                        DbDataReader result = cmd.ExecuteReader();

                        while (result.Read())
                        {
                            d.ppaa_status = result["STATUS"]?.ToString().Trim();
                            d.appl_id = result["APPL_ID"]?.ToString().Trim();
                            d.appl_name = result["APPL_NAME"]?.ToString().Trim();
                            d.ins_id = result["INS_ID"]?.ToString().Trim();
                            d.ins_name = result["INS_NAME"]?.ToString().Trim();

                            if ("A".Equals(d.system))
                            {
                                d.sysmark = "";
                                d.sec_stat = StringUtil.toString(result["SEC_STAT"]?.ToString().Trim()) == "" ? "N" : result["SEC_STAT"]?.ToString().Trim();
                                d.ins_birth = result["BIRTH_DATE"]?.ToString().Trim();
                                d.appl_birth = result["APPL_BIRTH_DATE"]?.ToString().Trim(); 
                                d.send_id = result["COLLECTOR"]?.ToString().Trim();     //add by daiyu 20210225
                            }
                            else
                            {
                                d.sysmark = result["SYSMARK"]?.ToString().Trim();
                                d.sec_stat = StringUtil.toString(result["SECRT_YN"]?.ToString().Trim()) == "" ? "N" : result["SECRT_YN"]?.ToString().Trim();
                                d.appl_birth = StringUtil.toString(result["ABIRTH_YY"]?.ToString()) + StringUtil.toString(result["ABIRTH_MM"]?.ToString()).PadLeft(2, '0') + StringUtil.toString(result["ABIRTH_DD"]?.ToString()).PadLeft(2, '0');
                                d.ins_birth = StringUtil.toString(result["BIRTH_YY"]?.ToString()) + StringUtil.toString(result["BIRTH_MM"]?.ToString()).PadLeft(2, '0') + StringUtil.toString(result["BIRTH_DD"]?.ToString()).PadLeft(2, '0');
                                d.send_id = result["SRVMAN_ID"]?.ToString().Trim();     //add by daiyu 20210225
                            }
                        }

                        

                        //fMNPPADDao.qryForOAP0042(conn400, d);


                    }
                    catch (Exception ex)
                    {
                        logger.Error(ex.ToString());
                        // throw ex;
                    }


                    cmd.Dispose();
                    cmd = null;

                }
                return rows;
            }
            catch (Exception e)
            {
                logger.Error(e.ToString());
                throw e;
            }
        }


        public List<TelDispatchRptModel> qryForOAP0042(EacConnection conn400, List<TelDispatchRptModel> rows, string ppaa_status)
        {
            logger.Info("qryForOAP0042 begin size:" + rows.Count);

            //string status = "'" + ppaa_status.Replace('|', ',') + "'";
            string[] statusArr = ppaa_status.Split('|');

            List<TelDispatchRptModel> dataList = new List<TelDispatchRptModel>();

            string strSQLA = @"
SELECT PPAA.CHECK_NO, PPAA.SYSTEM, PPAA.CHECK_SHRT
      , PPAA.STATUS
      , POLIP.SEC_STAT
  FROM LMNPPAA5 PPAA LEFT JOIN LCVPOLIP POLIP ON PPAA.POLICY_NO = POLIP.INS_ID AND PPAA.POLICY_SEQ = POLIP.SEQ_NO
    WHERE PPAA.PGM_TYPE= '2'
      AND PPAA.SYSTEM = :SYSTEM
      AND PPAA.CHECK_NO = :CHECK_NO 
      AND PPAA.CHECK_SHRT = :CHECK_SHRT
";

            string strSQLF = @"
SELECT PPAA.CHECK_NO, PPAA.SYSTEM, PPAA.CHECK_SHRT
      , PPAA.STATUS
      , POLI.SYSMARK, POLI.SECRT_YN
  FROM LMNPPAA5 PPAA LEFT JOIN LPMPOLI1 POLI ON PPAA.POLICY_NO = POLI.POLICY_NO AND PPAA.ID_DUP = POLI.ID_DUP AND PPAA.POLICY_SEQ = POLI.POLICY_SEQ
    WHERE PPAA.PGM_TYPE= '2'
      AND PPAA.SYSTEM = :SYSTEM
      AND PPAA.CHECK_NO = :CHECK_NO 
      AND PPAA.CHECK_SHRT = :CHECK_SHRT
";

            try
            {
                

                int i = 0;
                foreach (TelDispatchRptModel d in rows)
                {

                    EacCommand cmd = new EacCommand();
                    cmd.Connection = conn400;

                    i++;
                    if (i % 500 == 0)
                        logger.Info(i);

 
                    if ("A".Equals(d.system))
                        cmd.CommandText = strSQLA;
                    else
                        cmd.CommandText = strSQLF;

                    cmd.Parameters.Clear();

                    cmd.Parameters.Add("SYSTEM", d.system);
                    cmd.Parameters.Add("CHECK_NO", d.check_no);
                    cmd.Parameters.Add("CHECK_SHRT", d.check_acct_short);
                    //cmd.Parameters.Add("STATUS", status);

                    try
                    {
                        DbDataReader result = cmd.ExecuteReader();

                        while (result.Read())
                        {
                            d.ppaa_status = result["STATUS"]?.ToString().Trim();
                            //d.appl_id = result["APPL_ID"]?.ToString().Trim();
                            //d.appl_name = result["APPL_NAME"]?.ToString().Trim();
                            //d.ins_id = result["INS_ID"]?.ToString().Trim();
                            //d.ins_name = result["INS_NAME"]?.ToString().Trim();

                            if ("A".Equals(d.system))
                            {
                                d.sysmark = "";
                                d.sec_stat = result["SEC_STAT"]?.ToString().Trim();

                            }
                            else
                            {
                                d.sysmark = result["SYSMARK"]?.ToString().Trim();
                                d.sec_stat = result["SECRT_YN"]?.ToString().Trim();
                            }
                        }


                        if("".Equals(StringUtil.toString(d.ppaa_status)) || (!"".Equals(ppaa_status) & ppaa_status.IndexOf(StringUtil.toString(d.ppaa_status)) < 0))
                            dataList.Add(d);
                            
                    }
                    catch (Exception ex) {
                        logger.Error(ex.ToString());
                       // throw ex;
                    }


                    cmd.Dispose();
                    cmd = null;

                }
                return dataList;
            }
            catch (Exception e) {
                logger.Error(e.ToString());
                throw e;
            }
        }


        /// <summary>
        /// 「OAP0001上傳應付未付主檔/明細檔作業」資料檢核
        /// </summary>
        /// <param name="conn400"></param>
        /// <param name="checkNo"></param>
        /// <param name="bankCode"></param>
        /// <returns>
        /// 空白:正常
        /// 1:狀態有誤
        /// </returns>
        public string qryForOAP0001(EacConnection conn400, string checkShrt, string checkNo, string system)
        {
            logger.Info("qryForOAP0001 begin!");

            string rtnMsg = "";

            EacCommand cmdQ = new EacCommand();
            string strSQLQ = @"
SELECT CHECK_NO, SYSTEM, CHECK_SHRT
  FROM LMNPPAA5 
    WHERE CHECK_NO = :CHECK_NO 
         AND CHECK_SHRT = :CHECK_SHRT";

            try
            {
                cmdQ.Connection = conn400;
                cmdQ.CommandText = strSQLQ;

                cmdQ.Parameters.Clear();

                cmdQ.Parameters.Add("CHECK_NO", checkNo);
             //   cmdQ.Parameters.Add("SYSTEM", system);
                cmdQ.Parameters.Add("CHECK_SHRT", checkShrt);

                DbDataReader result = cmdQ.ExecuteReader();
                bool bExist = false;

                while (result.Read())
                {
                    bExist = true;
                }

                if (bExist)
                    rtnMsg = "1";
                else
                    rtnMsg = "";


                cmdQ.Dispose();
                cmdQ = null;

                logger.Info("qryForOAP0001 end!");

            }
            catch (Exception e)
            {
                logger.Error(e.ToString());
                throw e;
            }



            return rtnMsg;

        }



        public int delForOAP0002(OAP0002Model d, EacConnection conn, EacTransaction transaction)
        {
            logger.Info("delForOAP0002 begin!");
            int cnt = 0;
            EacCommand cmd = new EacCommand();

            string strSQL = "";
            strSQL = @"
DELETE LMNPPAA5 
WHERE CHECK_NO = :CHECK_NO
  AND CHECK_SHRT = :CHECK_SHRT
  AND PGM_TYPE = '2'
 ";

            try
            {
                cmd.Connection = conn;
                cmd.Transaction = transaction;
                cmd.CommandText = strSQL;

                cmd.Parameters.Add("CHECK_NO", d.check_no);
                cmd.Parameters.Add("CHECK_SHRT", d.check_acct_short);

                cnt = cmd.ExecuteNonQuery();

                cmd.Dispose();
                cmd = null;

                logger.Info("delForOAP0002 end!");
                return cnt;

            }
            catch (Exception e)
            {
                logger.Error(e.ToString());
                throw e;
            }

        }


        public int updForOAP0002Summary(OAP0002Model d, string updId, string updDate
            , EacConnection conn, EacTransaction transaction)
        {
            //open畫面以"西元年"呈現...回寫AS400時..要轉成民國年
            string re_paid_dt = DateUtil.ADDateToChtDate(d.re_paid_date_n, 4, "");  //modify by daiyu 20201028

            logger.Info("updForOAP0002Summary begin!");

            int cnt = 0;
            EacCommand cmd = new EacCommand();

            string strSQL = "";
            strSQL = @"
UPDATE LMNPPAA5 
SET STATUS = :STATUS
   ,FILLER_10 = :FILLER_10
   ,FILLER_14 = :FILLER_14
   ,PAID_ID = :PAID_ID
   ,R_PAID_TP = :R_PAID_TP
   ,AREA = :AREA
   ,SRCE_FROM = :SRCE_FROM
   ,SRCE_KIND = :SRCE_KIND
   ,PAY_NO = :PAY_NO
   ,PAY_SEQ = :PAY_SEQ
   ,R_PAID_NO = :R_PAID_NO
   ,R_PAID_SEQ = :R_PAID_SEQ
   ,R_PAID_NUM = :R_PAID_NUM
   ,RT_SYSTEM = :RT_SYSTEM
   ,RT_POLI_NO = :RT_POLI_NO
   ,RT_POLI_SE = :RT_POLI_SE
   ,RT_ID_DUP = :RT_ID_DUP
   ,R_BANK_NO1 = :R_BANK_NO1
   ,R_BANK_NO2 = :R_BANK_NO2
   ,R_BANK_ACT = :R_BANK_ACT
   ,R_PAID_ID = :R_PAID_ID
   ,R_PAID_DT = :R_PAID_DT
   ,O_PAID_CD = :O_PAID_CD
   ,UPD_ID = :UPD_ID
   ,UPD_DATE = :UPD_DATE
WHERE CHECK_NO = :CHECK_NO
  AND CHECK_SHRT = :CHECK_SHRT
  AND PGM_TYPE = '2'
 ";

            try
            {
                cmd.Connection = conn;
                cmd.Transaction = transaction;
                cmd.CommandText = strSQL;
                cmd.Parameters.Add("STATUS", d.status);
                cmd.Parameters.Add("FILLER_10", d.filler_10);
                cmd.Parameters.Add("FILLER_14", d.filler_14);
                cmd.Parameters.Add("PAID_ID", d.paid_id);
                cmd.Parameters.Add("R_PAID_TP", d.re_paid_type);
                cmd.Parameters.Add("AREA", d.area);
                cmd.Parameters.Add("SRCE_FROM", d.srce_from);
                cmd.Parameters.Add("SRCE_KIND", d.source_kind);
                cmd.Parameters.Add("PAY_NO", d.pay_no);
                cmd.Parameters.Add("PAY_SEQ", d.pay_seq);
                cmd.Parameters.Add("R_PAID_NO", d.re_paid_no);
                cmd.Parameters.Add("R_PAID_SEQ", d.re_paid_seq);
                cmd.Parameters.Add("R_PAID_NUM", d.re_paid_check_no);
                cmd.Parameters.Add("RT_SYSTEM", d.rt_system);
                cmd.Parameters.Add("RT_POLI_NO", d.rt_policy_no);
                cmd.Parameters.Add("RT_POLI_SE", d.rt_policy_seq);
                cmd.Parameters.Add("RT_ID_DUP", d.rt_id_dup);
                cmd.Parameters.Add("R_BANK_NO1", d.re_bank_code);
                cmd.Parameters.Add("R_BANK_NO2", d.re_sub_bank);
                cmd.Parameters.Add("R_BANK_ACT", d.re_bank_account);
                cmd.Parameters.Add("R_PAID_ID", d.re_paid_id);
                cmd.Parameters.Add("R_PAID_DT", re_paid_dt);
                cmd.Parameters.Add("O_PAID_CD", d.o_paid_cd);
                cmd.Parameters.Add("UPD_ID", updId);
                cmd.Parameters.Add("UPD_DATE", updDate);

                cmd.Parameters.Add("CHECK_NO", d.check_no);
                cmd.Parameters.Add("CHECK_SHRT", d.check_acct_short);

                cnt = cmd.ExecuteNonQuery();

                cmd.Dispose();
                cmd = null;

                logger.Info("updForOAP0002Summary end!");

                return cnt;

            }
            catch (Exception e)
            {
                logger.Error(e.ToString());
                throw e;
            }


        }


        public void updForOAP0002Policy(FAP_PPAA_D_HIS d, string system, string sourceOp, string paidId, string checkNo
            , string updId, string updDate
            , EacConnection conn, EacTransaction transaction)
        {
            logger.Info("updForOAP0002Policy begin!");


            EacCommand cmd = new EacCommand();

            string strSQL = "";
            strSQL = @"
UPDATE LMNPPAA5 
SET POLICY_NO = :POLICY_NO_ATF
   ,POLICY_SEQ = :POLICY_SEQ_ATF
   ,ID_DUP = :ID_DUP_ATF
   ,MEMBER_ID = :MEMBER_ID_ATF
   ,CHANGE_ID = :CHANGE_ID_ATF
   ,UPD_ID = :UPD_ID
   ,UPD_DATE = :UPD_DATE
WHERE SYSTEM = :SYSTEM
  AND SOURCE_OP = :SOURCE_OP
  AND POLICY_NO = :POLICY_NO
  AND POLICY_SEQ = :POLICY_SEQ
  AND ID_DUP = :ID_DUP
  AND MEMBER_ID = :MEMBER_ID
  AND CHANGE_ID = :CHANGE_ID
  AND PAID_ID = :PAID_ID
  AND CHECK_NO = :CHECK_NO
  AND PGM_TYPE = '2'
 ";

            try
            {
                cmd.Connection = conn;
                cmd.Transaction = transaction;
                cmd.CommandText = strSQL;
                cmd.Parameters.Add("POLICY_NO_ATF", d.policy_no_aft);
                cmd.Parameters.Add("POLICY_SEQ_ATF", d.policy_seq_aft);
                cmd.Parameters.Add("ID_DUP_ATF", d.id_dup_aft);
                cmd.Parameters.Add("MEMBER_ID", d.member_id_aft);
                cmd.Parameters.Add("CHANGE_ID", d.change_id_aft);
                cmd.Parameters.Add("UPD_ID", updId);
                cmd.Parameters.Add("UPD_DATE", updDate);

                cmd.Parameters.Add("SYSTEM", d.system);
                cmd.Parameters.Add("SOURCE_OP", d.source_op);
                cmd.Parameters.Add("POLICY_NO", d.policy_no);
                cmd.Parameters.Add("POLICY_SEQ", d.policy_seq);
                cmd.Parameters.Add("ID_DUP", d.id_dup);
                cmd.Parameters.Add("MEMBER_ID", d.member_id);
                cmd.Parameters.Add("CHANGE_ID", d.change_id);
                cmd.Parameters.Add("PAID_ID", d.paid_id);
                cmd.Parameters.Add("CHECK_NO", d.check_no);



                cmd.ExecuteNonQuery();

                cmd.Dispose();
                cmd = null;

                logger.Info("updForOAP0002Policy end!");

            }
            catch (Exception e)
            {
                logger.Error(e.ToString());
                throw e;
            }

        }


        public void insertForOAP0001(string usrId, List<OAP0001FileModel> fileList, EacConnection conn, EacTransaction transaction)
        {
            logger.Info("insert FMNPPAA0 begin!");


            

            string strSQLC = "";
            string strSQL = "";
            strSQL = @"insert into FMNPPAA0 
( SYSTEM    
,SOURCE_OP 
,POLICY_NO 
,POLICY_SEQ
,ID_DUP    
,MEMBER_ID 
,CHANGE_ID 
,PAID_ID   
,APPL_ID   
,INS_ID    
,PGM_TYPE  
,O_PAID_TP 
,O_PAID_DT 
,CURRENCY  
,MAIN_AMT  
,CHECK_AMT  
,CHECK_NO  
,CHECK_SHRT
,CHECK_DATE
,DEL_CODE  
,O_PAID_CD 
,O_CHECK_DT
,UPD_ID    
,UPD_DATE  
,FILLER_14
,PAID_NAME 
,APPL_NAME 
,INS_NAME 
) VALUES (
  :SYSTEM    
,:SOURCE_OP 
,:POLICY_NO 
,:POLICY_SEQ
,:ID_DUP    
,:MEMBER_ID 
,:CHANGE_ID 
,:PAID_ID   
,:APPL_ID    
,:INS_ID     
,:PGM_TYPE  
,:O_PAID_TP 
,:O_PAID_DT 
,:CURRENCY  
,:MAIN_AMT  
,:CHECK_AMT 
,:CHECK_NO  
,:CHECK_SHRT
,:CHECK_DATE
,:DEL_CODE  
,:O_PAID_CD 
,:O_CHECK_DT
,:UPD_ID    
,:UPD_DATE  
,:FILLER_14";



            try
            {
                
                //cmd.CommandText = strSQL;

                foreach (OAP0001FileModel d in fileList) {
                    EacCommand cmd = new EacCommand();
                    cmd.Connection = conn;
                    cmd.Transaction = transaction;

                    //modify daiyu 20191128 
                    strSQLC = "";
                    d.paidName = (d.paidName.Length > 25 ? d.paidName.Substring(0, 25) : d.paidName);   //給付對象姓名 
                    d.applName = (d.applName.Length > 5 ? d.applName.Substring(0, 5) : d.applName); //要保人姓名
                    d.insName = (d.insName.Length > 5 ? d.insName.Substring(0, 5) : d.insName); //被保人姓名

                    strSQLC += " ,'" + d.paidName + "'";
                    strSQLC += " ,'" + d.applName + "'";
                    strSQLC += " ,'" + d.insName + "'";
                    strSQLC += " )";

                    cmd.CommandText = strSQL + strSQLC;
                    //endregion


                    cmd.Parameters.Clear();

                    cmd.Parameters.Add("SYSTEM", d.system);
                    cmd.Parameters.Add("SOURCE_OP", "VE");
                    cmd.Parameters.Add("POLICY_NO", d.policyNo);
                    cmd.Parameters.Add("POLICY_SEQ", d.policySeq == "" ? "0" : d.policySeq);
                    cmd.Parameters.Add("ID_DUP", d.idDup);
                    cmd.Parameters.Add("MEMBER_ID", d.memberId);
                    cmd.Parameters.Add("CHANGE_ID", d.changeId);
                    cmd.Parameters.Add("PAID_ID", d.paidId);
                    //cmd.Parameters.Add("PAID_NAME", d.paidName);
                    cmd.Parameters.Add("APPL_ID", d.applId);
                    //cmd.Parameters.Add("APPL_NAME", d.applName);
                    cmd.Parameters.Add("INS_ID", d.insId);
                    //cmd.Parameters.Add("INS_NAME", d.insName);
                    cmd.Parameters.Add("PGM_TYPE", "2");
                    cmd.Parameters.Add("O_PAID_TP", "D");
                    cmd.Parameters.Add("O_PAID_DT", d.oPaidDt);
                    cmd.Parameters.Add("CURRENCY", d.currency);
                    cmd.Parameters.Add("MAIN_AMT", d.mainAmt);
                    cmd.Parameters.Add("CHECK_AMT", d.checkAmt);
                    cmd.Parameters.Add("CHECK_NO", d.checkNo);
                    cmd.Parameters.Add("CHECK_SHRT", d.checkShrt);
                    cmd.Parameters.Add("CHECK_DATE", d.checkDate);
                    cmd.Parameters.Add("DEL_CODE", d.delCode);
                    cmd.Parameters.Add("O_PAID_CD", d.oPaidCd);
                    cmd.Parameters.Add("O_CHECK_DT", d.checkDate);
                    cmd.Parameters.Add("UPD_ID", usrId);
                    cmd.Parameters.Add("UPD_DATE", DateUtil.getCurChtDate(4));
                    cmd.Parameters.Add("FILLER_14", d.filler14);    //add by daiyu 20200114

                    cmd.ExecuteNonQuery();

                    cmd.Dispose();
                    cmd = null;
                }

                logger.Info("insert LMNPPAA5 end!");

            }
            catch (Exception e)
            {
                logger.Error(e.ToString());
                throw e;
            }

        }

        public List<FMNPPAAModel> qryForOAP0002(EacConnection conn400, string checkShrt, string checkNo)
        {
            logger.Info("qryForOAP0002 begin!");

            List<FMNPPAAModel> rtnList = new List<FMNPPAAModel>();

            EacCommand cmdQ = new EacCommand();
            string strSQLQ = @"
SELECT CHECK_NO  
      ,CHECK_SHRT
      ,SYSTEM    
      ,SOURCE_OP   
      ,CHECK_DATE
      ,CHECK_AMT 
      ,STATUS    
      ,FILLER_10 
      ,FILLER_14
      ,PAID_ID  
      ,PAID_NAME
      ,R_PAID_TP 
      ,AREA      
      ,SRCE_FROM 
      ,SRCE_KIND 
      ,PAY_NO    
      ,PAY_SEQ   
      ,R_PAID_NO 
      ,R_PAID_SEQ
      ,R_PAID_NUM
      ,RT_SYSTEM 
      ,RT_POLI_NO
      ,RT_POLI_SE
      ,RT_ID_DUP 
      ,R_BANK_NO1
      ,R_BANK_NO2
      ,R_BANK_ACT
      ,R_PAID_ID
      ,R_PAID_DT
      ,POLICY_NO
      ,POLICY_SEQ
      ,ID_DUP    
      ,MEMBER_ID 
      ,CHANGE_ID 
      ,MAIN_AMT  
      ,O_PAID_CD
  FROM LMNPPAA5 
    WHERE CHECK_NO = :CHECK_NO 
      AND CHECK_SHRT = :CHECK_SHRT
      AND PGM_TYPE = '2'";

            try
            {
                cmdQ.Connection = conn400;
                cmdQ.CommandText = strSQLQ;

                cmdQ.Parameters.Clear();

                cmdQ.Parameters.Add("CHECK_NO", checkNo);
                cmdQ.Parameters.Add("CHECK_SHRT", checkShrt);

                DbDataReader result = cmdQ.ExecuteReader();

                while (result.Read())
                {
                    FMNPPAAModel d = new FMNPPAAModel();
                    d.check_no = result["CHECK_NO"]?.ToString().Trim();
                    d.check_acct_short = result["CHECK_SHRT"]?.ToString().Trim();
                    d.system = result["SYSTEM"]?.ToString().Trim();
                    d.source_op = result["SOURCE_OP"]?.ToString().Trim();
                    d.check_date = result["CHECK_DATE"]?.ToString().Trim();
                    d.check_amt = result["CHECK_AMT"]?.ToString().Trim();
                    d.status = result["STATUS"]?.ToString().Trim();
                    d.filler_10 = result["FILLER_10"]?.ToString().Trim();
                    d.filler_14 = result["FILLER_14"]?.ToString().Trim();
                    d.paid_id = result["PAID_ID"]?.ToString().Trim();
                    d.paid_name = result["PAID_NAME"]?.ToString().Trim();
                    d.re_paid_type = result["R_PAID_TP"]?.ToString().Trim();
                    d.area = result["AREA"]?.ToString().Trim();
                    d.srce_from = result["SRCE_FROM"]?.ToString().Trim();
                    d.source_kind = result["SRCE_KIND"]?.ToString().Trim();
                    d.pay_no = result["PAY_NO"]?.ToString().Trim();
                    d.pay_seq = result["PAY_SEQ"]?.ToString().Trim();
                    d.re_paid_no = result["R_PAID_NO"]?.ToString().Trim();
                    d.re_paid_seq = result["R_PAID_SEQ"]?.ToString().Trim();
                    d.re_paid_check_no = result["R_PAID_NUM"]?.ToString().Trim();
                    d.rt_system = result["RT_SYSTEM"]?.ToString().Trim();
                    d.rt_policy_no = result["RT_POLI_NO"]?.ToString().Trim();
                    d.rt_policy_seq = result["RT_POLI_SE"]?.ToString().Trim();
                    d.rt_id_dup = result["RT_ID_DUP"]?.ToString().Trim();
                    d.re_bank_code = result["R_BANK_NO1"]?.ToString().Trim();
                    d.re_sub_bank = result["R_BANK_NO2"]?.ToString().Trim();
                    d.re_bank_account = result["R_BANK_ACT"]?.ToString().Trim();
                    d.re_paid_id = result["R_PAID_ID"]?.ToString().Trim();
                    //d.re_paid_date = result["R_PAID_DT"]?.ToString().Trim();
                    d.re_paid_date_n = result["R_PAID_DT"]?.ToString().Trim();  //modify by daiyu 20201027
                    d.policy_no = result["POLICY_NO"]?.ToString().Trim();
                    d.policy_seq = result["POLICY_SEQ"]?.ToString().Trim();
                    d.id_dup = result["ID_DUP"]?.ToString().Trim();
                    d.member_id = result["MEMBER_ID"]?.ToString().Trim();
                    d.change_id = result["CHANGE_ID"]?.ToString().Trim();
                    d.main_amt = result["MAIN_AMT"]?.ToString().Trim();
                    d.o_paid_cd = result["O_PAID_CD"]?.ToString().Trim();

                    //open畫面以"西元年"呈現...從AS400撈時..要將民國年轉成西元年
                    d.check_date = d.check_date == "" ? "" : DateUtil.As400ChtDateToADDate(d.check_date.PadLeft(7, '0'));



                    d.re_paid_date_n = d.re_paid_date_n == "" ? "" : (d.re_paid_date_n == "0" ?  "" :DateUtil.As400ChtDateToADDate(d.re_paid_date_n.PadLeft(7, '0')));  //modify by daiyu 20201027

                    rtnList.Add(d);
                }


                cmdQ.Dispose();
                cmdQ = null;

                logger.Info("qryForOAP0002 end!");

            }
            catch (Exception e)
            {
                logger.Error(e.ToString());
                throw e;
            }



            return rtnList;

        }

        public List<VeCleanModel> qryForVeClean(EacConnection conn400, string check_no, string check_shrt)
        {
            logger.Info("qryForVeClean begin!");

            List<VeCleanModel> rtnList = new List<VeCleanModel>();

            EacCommand cmdQ = new EacCommand();

            string strSQLQ = @"
SELECT DISTINCT AA.SYSTEM
      ,AA.CHECK_NO
      ,AA.CHECK_SHRT
      ,AA.PAID_ID
      ,AA.PAID_NAME
      ,AA.CHECK_AMT
      ,AA.CHECK_DATE
      ,AA.R_PAID_DT
      ,AA.R_PAID_TP
      ,AA.POLICY_NO
      ,AA.POLICY_SEQ
      ,AA.ID_DUP
      ,AA.MEMBER_ID
      ,AA.CHANGE_ID
      ,AA.MAIN_AMT
      ,AA.O_PAID_CD
  FROM LMNPPAA1 AA 
    WHERE AA.PGM_TYPE = '2'
      AND AA.CHECK_NO = :check_no
      AND AA.CHECK_SHRT = :check_shrt
 ORDER BY AA.CHECK_NO, AA.CHECK_SHRT
";

            try
            {
                cmdQ.Connection = conn400;
                cmdQ.CommandText = strSQLQ;

                cmdQ.Parameters.Clear();

                cmdQ.Parameters.Add("check_no", check_no);
                cmdQ.Parameters.Add("check_shrt", check_shrt);

                DbDataReader result = cmdQ.ExecuteReader();

                while (result.Read())
                {
                    VeCleanModel d = new VeCleanModel();
                    d.system = result["SYSTEM"]?.ToString().Trim();
                    d.check_no = result["CHECK_NO"]?.ToString().Trim();
                    d.check_acct_short = result["CHECK_SHRT"]?.ToString().Trim();
                    d.paid_id = result["PAID_ID"]?.ToString().Trim();
                    d.paid_name = result["PAID_NAME"]?.ToString().Trim();
                    d.check_amt = result["CHECK_AMT"]?.ToString().Trim();
                    d.check_date = result["CHECK_DATE"]?.ToString().Trim();
                    d.re_paid_date = result["R_PAID_DT"]?.ToString().Trim();
                    d.re_paid_type = result["R_PAID_TP"]?.ToString().Trim();
                    d.policy_no = result["POLICY_NO"]?.ToString().Trim();
                    d.policy_seq = result["POLICY_SEQ"]?.ToString().Trim();
                    d.id_dup = result["ID_DUP"]?.ToString().Trim();
                    d.member_id = result["MEMBER_ID"]?.ToString().Trim();
                    d.change_id = result["CHANGE_ID"]?.ToString().Trim();
                    d.main_amt = result["MAIN_AMT"]?.ToString().Trim();
                    d.o_paid_cd = result["O_PAID_CD"]?.ToString().Trim();
                    

                    //open畫面以"西元年"呈現...從AS400撈時..要將民國年轉成西元年
                    d.check_date = d.check_date == "0" ? null : DateUtil.As400ChtDateToADDate(d.check_date.PadLeft(7, '0'));
                    d.re_paid_date = d.re_paid_date == "0" ? null : DateUtil.As400ChtDateToADDate(d.re_paid_date.PadLeft(7, '0'));

                    rtnList.Add(d);
                }


                cmdQ.Dispose();
                cmdQ = null;

                logger.Info("qryForVeClean end!");

            }
            catch (Exception e)
            {
                logger.Error(e.ToString());
                throw e;
            }



            return rtnList;

        }




        /// <summary>
        /// add by daiyu 20191007
        /// 需求單號:
        /// 逾期未兌領AML，於AS400主動通知執行完畢後，產製【逾期未兌領支票-疑似禁制名單】
        /// </summary>
        /// <param name="conn400"></param>
        /// <param name="exec_date"></param>
        /// <param name="upd_id"></param>
        /// <param name="type"></param>
        /// <returns></returns>

        public List<VeAMLRptModel> qryForBAP7001(EacConnection conn400, string exec_date, string upd_id, string type)
        {
            logger.Info("qryForBAP7001 begin!");

            List<VeAMLRptModel> rtnList = new List<VeAMLRptModel>();

            EacCommand cmdQ = new EacCommand();

            string strSQLQ = @"
SELECT AA.CHECK_NO
      ,AA.CHECK_SHRT
      ,AA.PAID_ID
      ,AA.PAID_NAME
      ,AA.POLICY_NO
      ,AA.POLICY_SEQ
      ,AA.ID_DUP
      ,AA.CHECK_AMT
      ,AA.SYSTEM
      ,AW.DATA_FLAG
  FROM LAPPPAW1 AW JOIN LMNPPAA1 AA 
      ON AW.CHECK_SHRT = AA.CHECK_SHRT
     AND AW.CHECK_NO = AA.CHECK_NO
     AND AA.PGM_TYPE = '2'
    WHERE AW.REPORT_TP = 'X0006'
 ORDER BY  AA.SYSTEM, AA.PAID_ID, AA.CHECK_NO, AA.POLICY_NO,AA.POLICY_SEQ,AA.ID_DUP
";


            try
            {
                cmdQ.Connection = conn400;
                cmdQ.CommandText = strSQLQ;

                cmdQ.Parameters.Clear();

                //cmdQ.Parameters.Add("exec_date", exec_date);
                //cmdQ.Parameters.Add("upd_id", upd_id);

                DbDataReader result = cmdQ.ExecuteReader();

                while (result.Read())
                {
                    VeAMLRptModel d = new VeAMLRptModel();
                    d.check_no = result["CHECK_NO"]?.ToString().Trim();
                    d.check_acct_short = result["CHECK_SHRT"]?.ToString().Trim();
                    d.paid_id = result["PAID_ID"]?.ToString().Trim();
                    d.paid_name = result["PAID_NAME"]?.ToString().Trim();
                    d.policy_no = result["POLICY_NO"]?.ToString().Trim();
                    d.policy_seq = result["POLICY_SEQ"]?.ToString().Trim();
                    d.id_dup = result["ID_DUP"]?.ToString().Trim();
                    d.check_amt = result["CHECK_AMT"]?.ToString().Trim();
                    d.system = result["SYSTEM"]?.ToString().Trim();
                    d.data_flag = result["DATA_FLAG"]?.ToString().Trim();
                    rtnList.Add(d);
                }


                cmdQ.Dispose();
                cmdQ = null;

                logger.Info("qryForBAP7001 end!");

            }
            catch (Exception e)
            {
                logger.Error(e.ToString());
                throw e;
            }


            return rtnList;

        }
    }
}