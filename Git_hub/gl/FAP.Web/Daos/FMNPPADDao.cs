

using FAP.Web.AS400Models;
using FAP.Web.BO;
using FAP.Web.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.EasycomClient;

/// <summary>
/// FMNPPAD  支票退回回存／外幣無匯款帳號比對明細檔  
/// ------------------------------------------
/// 修改歷程：20191128 daiyu
/// 需求單號：201910290100-00
/// 修改內容：修改要被保人姓名長度與資料庫長度一致時，AS400欄位少"OE、OF"問題
/// ------------------------------------------
/// 修改歷程：20200807 daiyu
/// 需求單號：202008120153-00
/// 修改內容：1.補入空白地址
///           2.FOR電訪需求查詢服務人員相關資料
/// ------------------------------------------
/// </summary>
namespace FAP.Web.Daos
{
    public class FMNPPADDao
    {
        static private NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();


        public TelDispatchRptModel qryForOAP0042(EacConnection conn400, TelDispatchRptModel d)
        {


            EacCommand cmdQ = new EacCommand();
            string strSQLQ = @"
SELECT distinct
       AD.POLICY_NO
      ,AD.POLICY_SEQ
      ,AD.ID_DUP
      ,AD.SYSTEM
      ,AD.CHECK_NO
      ,AD.SEND_ID
      ,AD.SEND_NAME
      ,AD.SEND_UNIT
  FROM LMNPPAD1 AD 
    WHERE AD.SYSTEM = :SYSTEM 
      AND AD.SOURCE_OP = 'VE'
      AND AD.POLICY_NO = :POLICY_NO
      AND AD.POLICY_SEQ = :POLICY_SEQ
      AND AD.ID_DUP = :ID_DUP
      AND AD.CHECK_NO = :CHECK_NO
";

            try
            {
                cmdQ.Connection = conn400;
                cmdQ.CommandText = strSQLQ;

                cmdQ.Parameters.Clear();

                cmdQ.Parameters.Add("SYSTEM", d.system);
                cmdQ.Parameters.Add("POLICY_NO", d.policy_no);
                cmdQ.Parameters.Add("POLICY_SEQ", d.policy_seq);
                cmdQ.Parameters.Add("ID_DUP", d.id_dup);
                cmdQ.Parameters.Add("CHECK_NO", d.check_no);

                DbDataReader result = cmdQ.ExecuteReader();

                while (result.Read())
                {
                    d.send_id = result["SEND_ID"]?.ToString().Trim();
                    d.send_name = result["SEND_NAME"]?.ToString().Trim();
                    d.send_unit = result["SEND_UNIT"]?.ToString().Trim();
                    return d;
                }


                cmdQ.Dispose();
                cmdQ = null;



            }
            catch (Exception e)
            {
                logger.Error(e.ToString());
                throw e;
            }

            return d;

        }




        public void updForOAP0002Policy(List<FMNPPAAModel> ppaaList, string updId, string updDate
            , EacConnection conn, EacTransaction transaction)
        {
            logger.Info("updForOAP0002Policy begin!");

            EacCommand cmd = new EacCommand();

            string strSQL = "";
            strSQL = @"
UPDATE LMNPPAD1 
SET R_STATUS = '6'
   ,UPD_ID = :UPD_ID
   ,UPD_DATE = :UPD_DATE
WHERE SYSTEM = :SYSTEM
  AND SOURCE_OP = :SOURCE_OP
  AND PAID_ID = :PAID_ID
  AND CHANGE_ID = :CHANGE_ID
  AND MEMBER_ID = :MEMBER_ID
  AND CHECK_NO = :CHECK_NO
  AND ID_DUP = :ID_DUP
  AND POLICY_NO = :POLICY_NO
  AND POLICY_SEQ = :POLICY_SEQ
  AND PGM_TYPE = '2'
 ";

            try
            {
                cmd.Connection = conn;
                cmd.Transaction = transaction;
                cmd.CommandText = strSQL;

                foreach (FMNPPAAModel d in ppaaList) {
                    cmd.Parameters.Clear();
                    cmd.Parameters.Add("UPD_ID", updId);
                    cmd.Parameters.Add("UPD_DATE", updDate);

                    cmd.Parameters.Add("SYSTEM", d.system);
                    cmd.Parameters.Add("SOURCE_OP", d.source_op);
                    cmd.Parameters.Add("PAID_ID", d.paid_id);
                    cmd.Parameters.Add("CHANGE_ID", d.change_id);
                    cmd.Parameters.Add("MEMBER_ID", d.member_id);
                    cmd.Parameters.Add("CHECK_NO", d.check_no);
                    cmd.Parameters.Add("ID_DUP", d.id_dup);
                    cmd.Parameters.Add("POLICY_NO", d.policy_no);
                    cmd.Parameters.Add("POLICY_SEQ", d.policy_seq);

                    cmd.ExecuteNonQuery();
                }


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




        public void delForOAP0002(List<FMNPPAAModel> ppaaList, EacConnection conn, EacTransaction transaction)
        {
            logger.Info("delForOAP0002 begin!");

            EacCommand cmd = new EacCommand();

            string strSQL = "";
            strSQL = @"
DELETE LMNPPAD1 
WHERE SYSTEM = :SYSTEM
  AND SOURCE_OP = :SOURCE_OP
  AND PAID_ID = :PAID_ID
  AND CHANGE_ID = :CHANGE_ID
  AND MEMBER_ID = :MEMBER_ID
  AND CHECK_NO = :CHECK_NO
  AND ID_DUP = :ID_DUP
  AND POLICY_NO = :POLICY_NO
  AND POLICY_SEQ = :POLICY_SEQ
  AND PGM_TYPE = '2'
 ";

            try
            {
                cmd.Connection = conn;
                cmd.Transaction = transaction;
                cmd.CommandText = strSQL;

                foreach (FMNPPAAModel d in ppaaList) {
                    cmd.Parameters.Clear();
                    cmd.Parameters.Add("SYSTEM", d.system);
                    cmd.Parameters.Add("SOURCE_OP", d.source_op);
                    cmd.Parameters.Add("PAID_ID", d.paid_id);
                    cmd.Parameters.Add("CHANGE_ID", d.change_id);
                    cmd.Parameters.Add("MEMBER_ID", d.member_id);
                    cmd.Parameters.Add("CHECK_NO", d.check_no);
                    cmd.Parameters.Add("ID_DUP", d.id_dup);
                    cmd.Parameters.Add("POLICY_NO", d.policy_no);
                    cmd.Parameters.Add("POLICY_SEQ", d.policy_seq);


                    cmd.ExecuteNonQuery();
                }
                

                cmd.Dispose();
                cmd = null;

                logger.Info("delForOAP0002 end!");

            }
            catch (Exception e)
            {
                logger.Error(e.ToString());
                throw e;
            }

        }


        /// <summary>
        /// OAP0003補入空白地址
        /// </summary>
        /// <param name="report_no"></param>
        /// <param name="r_zip_code"></param>
        /// <param name="r_addr"></param>
        /// <param name="conn"></param>
        /// <param name="transaction"></param>
        /// <returns></returns>
        public int updForOAP0003(string report_no, string r_zip_code, string r_addr, EacConnection conn, EacTransaction transaction)
        {
            logger.Info("updForOAP0003 begin!");

            EacCommand cmd = new EacCommand();

            string strSQL = "";
            strSQL = @"
UPDATE LMNPPAD5 
  SET R_ZIP_CODE = :R_ZIP_CODE ";

            strSQL += ", R_ADDR = '" + (r_addr.Length > 35 ? r_addr.Substring(0, 35) : r_addr) + "'";


            strSQL += " WHERE REPORT_NO = :REPORT_NO AND PGM_TYPE = '2'";

            try
            {
                cmd.Connection = conn;
                cmd.Transaction = transaction;
                cmd.CommandText = strSQL;

                cmd.Parameters.Add("R_ZIP_CODE", r_zip_code);
                cmd.Parameters.Add("REPORT_NO", report_no);
                int cnt = cmd.ExecuteNonQuery();


                cmd.Dispose();
                cmd = null;

                logger.Info("updForOAP0003 end!");

                return cnt;

            }
            catch (Exception e)
            {
                logger.Error(e.ToString());
                throw e;
            }

        }



        public int delForOAP0003(string report_no, EacConnection conn, EacTransaction transaction)
        {
            logger.Info("delForOAP0002 begin!");

            EacCommand cmd = new EacCommand();

            string strSQL = "";
            strSQL = @"
DELETE LMNPPAD5 
WHERE REPORT_NO = :REPORT_NO
  AND PGM_TYPE = '2'
 ";

            try
            {
                cmd.Connection = conn;
                cmd.Transaction = transaction;
                cmd.CommandText = strSQL;

                cmd.Parameters.Add("REPORT_NO", report_no);
                int cnt = cmd.ExecuteNonQuery();


                cmd.Dispose();
                cmd = null;

                logger.Info("delForOAP0003 end!");

                return cnt;

            }
            catch (Exception e)
            {
                logger.Error(e.ToString());
                throw e;
            }

        }



        public void insertForOAP0001(string usrId, List<OAP0001FileModel> fileList, EacConnection conn, EacTransaction transaction)
        {
            logger.Info("insert FMNPPAD0 begin!");


           

            string strSQLC = "";
            string strSQL = "";
            strSQL = @"insert into FMNPPAD0 
(  SYSTEM    
,SOURCE_OP 
,POLICY_NO 
,POLICY_SEQ
,ID_DUP    
,MEMBER_ID 
,CHANGE_ID 
,PAID_ID   
,CURRENCY  
,CHECK_NO  
,MAIN_AMT  
,PGM_TYPE  
,R_CODE     
,R_STATUS  
,R_ZIP_CODE
,R_ADDR    
,DEPT_DATE1
,SEND_ID   
,SEND_NAME 
,SEND_UNIT 
,REPORT    
,UPD_ID   
,UPD_DATE 
,PAID_NAME  
) VALUES (
 :SYSTEM    
,:SOURCE_OP 
,:POLICY_NO 
,:POLICY_SEQ
,:ID_DUP    
,:MEMBER_ID 
,:CHANGE_ID 
,:PAID_ID   

,:CURRENCY  
,:CHECK_NO  
,:MAIN_AMT  
,:PGM_TYPE  
,:R_CODE     
,:R_STATUS  
,:R_ZIP_CODE
,:R_ADDR    
,:DEPT_DATE1
,:SEND_ID   
,:SEND_NAME 
,:SEND_UNIT 
,:REPORT    
,:UPD_ID   
,:UPD_DATE ";


            try
            {
                string curChtDate = DateUtil.getCurChtDate(4);

                foreach (OAP0001FileModel d in fileList) {
                    if (!"03".Equals(StringUtil.toString(d.filler14))) {    //add by daiyu 20200114
                        EacCommand cmd = new EacCommand();
                        cmd.Connection = conn;
                        cmd.Transaction = transaction;
                        cmd.CommandText = strSQL;

                        //modify daiyu 20191128 
                        strSQLC = "";
                        d.paidName = (d.paidName.Length > 25 ? d.paidName.Substring(0, 25) : d.paidName);   //給付對象姓名 
                        strSQLC += " ,'" + d.paidName + "'";
                        strSQLC += " )";

                        cmd.CommandText = strSQL + strSQLC;
                        //endregion


                        cmd.Parameters.Clear();

                        //if (!"".Equals(d.rZipCode) && !"".Equals(d.rAddr)) {
                        cmd.Parameters.Add("SYSTEM", d.system);
                        cmd.Parameters.Add("SOURCE_OP", "VE");
                        cmd.Parameters.Add("POLICY_NO", d.policyNo);
                        cmd.Parameters.Add("POLICY_SEQ", d.policySeq == "" ? "0" : d.policySeq);
                        cmd.Parameters.Add("ID_DUP", d.idDup);
                        cmd.Parameters.Add("MEMBER_ID", d.memberId);
                        cmd.Parameters.Add("CHANGE_ID", d.changeId);
                        cmd.Parameters.Add("PAID_ID", d.paidId);
                        // cmd.Parameters.Add("PAID_NAME", d.paidName);
                        cmd.Parameters.Add("CURRENCY", d.currency);
                        cmd.Parameters.Add("CHECK_NO", d.checkNo);
                        cmd.Parameters.Add("MAIN_AMT", d.mainAmt);
                        cmd.Parameters.Add("PGM_TYPE", "2");
                        cmd.Parameters.Add("R_CODE", "VE1");
                        cmd.Parameters.Add("R_STATUS", "1");
                        cmd.Parameters.Add("R_ZIP_CODE", d.rZipCode);
                        cmd.Parameters.Add("R_ADDR", d.rAddr);
                        cmd.Parameters.Add("DEPT_DATE1", curChtDate);
                        cmd.Parameters.Add("SEND_ID", d.sendId);
                        cmd.Parameters.Add("SEND_NAME", d.sendName);
                        cmd.Parameters.Add("SEND_UNIT", d.sendUnit);
                        cmd.Parameters.Add("REPORT", d.report);
                        cmd.Parameters.Add("UPD_ID", usrId);
                        cmd.Parameters.Add("UPD_DATE", curChtDate);

                        cmd.ExecuteNonQuery();

                        //}
                        cmd.Dispose();
                        cmd = null;
                    }
                }


                logger.Info("insert FMNPPAD0 end!");

            }
            catch (Exception e)
            {
                logger.Error(e.ToString());
                throw e;
            }

        }






        public List<OAP0003PoliModel> qryForOAP0003(EacConnection conn400, string reportNo)
        {
            logger.Info("qryForOAP0003 begin!");

            List<OAP0003PoliModel> rtnList = new List<OAP0003PoliModel>();

            EacCommand cmdQ = new EacCommand();
            string strSQLQ = @"
SELECT AD.REPORT_NO
      ,AD.SYSTEM
      ,AD.PAID_ID
      ,AD.PAID_NAME
      ,AD.R_STATUS
      ,AD.CHECK_NO
      ,AD.POLICY_NO
      ,AD.POLICY_SEQ
      ,AD.ID_DUP
      ,AD.CHANGE_ID
      ,AD.MAIN_AMT
      ,AA.O_PAID_CD
      ,AA.CHECK_SHRT
      ,AA.FILLER_14
      ,AD.R_ZIP_CODE
      ,AD.R_ADDR
  FROM LMNPPAD5 AD JOIN LMNPPAA1 AA 
      ON AD.SYSTEM = AA.SYSTEM
     AND AD.SOURCE_OP = AA.SOURCE_OP
     AND AD.PAID_ID = AA.PAID_ID
     AND AD.CHANGE_ID = AA.CHANGE_ID
     AND AD.MEMBER_ID = AA.MEMBER_ID
     AND AD.CHECK_NO = AA.CHECK_NO
     AND AD.ID_DUP = AA.ID_DUP
     AND AD.POLICY_NO = AA.POLICY_NO
     AND AD.POLICY_SEQ = AA.POLICY_SEQ
    WHERE AD.REPORT_NO = :REPORT_NO 
      AND AD.PGM_TYPE = '2'";

            try
            {
                cmdQ.Connection = conn400;
                cmdQ.CommandText = strSQLQ;

                cmdQ.Parameters.Clear();

                cmdQ.Parameters.Add("REPORT_NO", reportNo);

                DbDataReader result = cmdQ.ExecuteReader();

                while (result.Read())
                {
                    OAP0003PoliModel d = new OAP0003PoliModel();
                    d.report_no = result["REPORT_NO"]?.ToString().Trim();
                    d.system = result["SYSTEM"]?.ToString().Trim();
                    d.paid_id = result["PAID_ID"]?.ToString().Trim();
                    d.paid_name = result["PAID_NAME"]?.ToString().Trim();
                    d.r_status = result["R_STATUS"]?.ToString().Trim();
                    d.check_no = result["CHECK_NO"]?.ToString().Trim();
                    d.check_acct_short = result["CHECK_SHRT"]?.ToString().Trim();
                    d.policy_no = result["POLICY_NO"]?.ToString().Trim();
                    d.policy_seq = result["POLICY_SEQ"]?.ToString().Trim();
                    d.id_dup = result["ID_DUP"]?.ToString().Trim();
                    d.change_id = result["CHANGE_ID"]?.ToString().Trim();
                    d.main_amt = result["MAIN_AMT"]?.ToString().Trim();
                    d.o_paid_cd = result["O_PAID_CD"]?.ToString().Trim();
                    d.filler_14 = result["FILLER_14"]?.ToString().Trim();   //add by daiyu 20191122
                   

                    rtnList.Add(d);
                }


                cmdQ.Dispose();
                cmdQ = null;

                logger.Info("qryForOAP0003 end!");

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