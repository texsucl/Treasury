

using FAP.Web.AS400Models;
using FAP.Web.BO;
using FAP.Web.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.EasycomClient;

/// <summary>
/// FAPPPAW  逾期未兌領信函歸戶工作檔  
/// ------------------------------------------
/// modify by daiyu 20191008
/// 需求單號:
/// 逾期未兌領AML，於AS400主動通知執行完畢後，產製【逾期未兌領支票-禁制名單_名單刪除通知】
/// ------------------------------------------
/// modify by daiyu 20200722
/// 需求單號:202007150155-00
/// 特殊抽件線上問題，修改OAP0009 戶政查詢地址匯入時先刪除AS400的資料
/// ------------------------------------------
/// modify by daiyu 20200811
/// 需求單號:
/// 修改AS400更新OPEN清理計畫檔的挑檔方式
/// ------------------------------------------
/// </summary>
namespace FAP.Web.Daos
{
    public class FAPPPAWDao
    {
        static private NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();


        /// <summary>
        /// 逾期未兌領支票-禁制名單_名單刪除通知(Filler_14=03*)
        /// </summary>
        /// <param name="conn400"></param>
        /// <param name="exec_date"></param>
        /// <param name="upd_id"></param>
        /// <returns></returns>
        public List<VeAMLRptModel> qryForBAP7029P2(EacConnection conn400, string exec_date, string upd_id)
        {
            logger.Info("qryForBAP7029P2 begin!");

            List<VeAMLRptModel> rtnList = new List<VeAMLRptModel>();

            EacCommand cmdQ = new EacCommand();

            string strSQLQ = @"
SELECT DISTINCT AW.CHECK_NO
      ,AW.CHECK_SHRT
      ,AW.PAID_ID
      ,AA.PAID_NAME
      ,AA.POLICY_NO
      ,AA.POLICY_SEQ
      ,AA.ID_DUP
      ,AA.CHECK_DATE
      ,AA.CHECK_AMT
      ,AA.SYSTEM
      ,LAPPPAT1.C_SERV_ID
      ,LAPPPAT1.C_SERV_NM
      ,LAPPPAT1.R_BANK_CD
      ,LAPPPAT1.R_SUB_BANK
      ,LAPPPAT1.R_BANK_ACT
      ,LAPPPAT1.C_PAID_NM
      ,LAPPPAT1.CHECK_FLAG
      ,LMNPPAD1.REPORT_NO
  FROM LAPPPAW1 AW JOIN LMNPPAA5 AA ON AW.CHECK_NO = AA.CHECK_NO AND AW.CHECK_SHRT = AA.CHECK_SHRT AND AA.PGM_TYPE = '2' AND AA.FILLER_14 = '03*'
                   JOIN LMNPPAD1 ON     LMNPPAD1.SYSTEM = AA.SYSTEM 
                                    AND LMNPPAD1.SOURCE_OP = AA.SOURCE_OP 
                                    AND LMNPPAD1.POLICY_NO = AA.POLICY_NO
                                    AND LMNPPAD1.POLICY_SEQ = AA.POLICY_SEQ 
                                    AND LMNPPAD1.ID_DUP = AA.ID_DUP 
                                    AND LMNPPAD1.MEMBER_ID = AA.MEMBER_ID
                                    AND LMNPPAD1.CHANGE_ID = AA.CHANGE_ID 
                                    AND LMNPPAD1.PAID_ID = AA.PAID_ID 
                                    AND LMNPPAD1.CHECK_NO = AA.CHECK_NO
                   JOIN LAPPPAT1 ON     LAPPPAT1.SYSTEM = AA.SYSTEM 
                                    AND LAPPPAT1.SOURCE_OP = AA.SOURCE_OP 
                                    AND LAPPPAT1.POLICY_NO = AA.POLICY_NO
                                    AND LAPPPAT1.POLICY_SEQ = AA.POLICY_SEQ 
                                    AND LAPPPAT1.ID_DUP = AA.ID_DUP 
                                    AND LAPPPAT1.MEMBER_ID = AA.MEMBER_ID
                                    AND LAPPPAT1.CHANGE_ID = AA.CHANGE_ID 
                                    AND LAPPPAT1.O_PAID_ID = AA.PAID_ID 
                                    AND LAPPPAT1.CHECK_NO = AA.CHECK_NO
    WHERE AW.DEPT_DATE1 = :exec_date 
      AND AW.REPORT_TP = 'X0005'
      AND AW.DATA_FLAG = '*'
 ORDER BY LAPPPAT1.C_SERV_ID, LMNPPAD1.REPORT_NO, AW.CHECK_NO, AW.CHECK_SHRT
";

            try
            {
                cmdQ.Connection = conn400;
                cmdQ.CommandText = strSQLQ;

                cmdQ.Parameters.Clear();

                cmdQ.Parameters.Add("exec_date", exec_date);

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
                    d.check_date = result["CHECK_DATE"]?.ToString().Trim();
                    d.check_amt = result["CHECK_AMT"]?.ToString().Trim();
                    d.system = result["SYSTEM"]?.ToString().Trim();
                    d.c_serv_id = result["C_SERV_ID"]?.ToString().Trim();
                    d.c_serv_nm = result["C_SERV_NM"]?.ToString().Trim();
                    d.r_bank_cd = result["R_BANK_CD"]?.ToString().Trim();
                    d.r_sub_bank = result["R_SUB_BANK"]?.ToString().Trim();
                    d.r_bank_act = result["R_BANK_ACT"]?.ToString().Trim();
                    d.c_paid_nm = result["C_PAID_NM"]?.ToString().Trim();
                    d.report_no = result["REPORT_NO"]?.ToString().Trim();
                    d.check_flag = result["CHECK_FLAG"]?.ToString().Trim();
                    rtnList.Add(d);
                }


                cmdQ.Dispose();
                cmdQ = null;

                logger.Info("qryForBAP7029P2 end!");

            }
            catch (Exception e)
            {
                logger.Error(e.ToString());
                throw e;
            }

            return rtnList;

        }


        /// <summary>
        /// 逾期未兌領支票-禁制名單_名單刪除通知(Filler_14=03)
        /// </summary>
        /// <param name="conn400"></param>
        /// <param name="exec_date"></param>
        /// <param name="upd_id"></param>
        /// <returns></returns>
        public List<VeAMLRptModel> qryForBAP7029P1(EacConnection conn400, string exec_date, string upd_id)
        {
            logger.Info("qryForBAP7029P1 begin!");

            List<VeAMLRptModel> rtnList = new List<VeAMLRptModel>();

            EacCommand cmdQ = new EacCommand();

            string strSQLQ = @"
SELECT DISTINCT AW.CHECK_NO
      ,AW.CHECK_SHRT
      ,AA.PAID_ID
      ,AA.PAID_NAME
      ,AA.POLICY_NO
      ,AA.POLICY_SEQ
      ,AA.ID_DUP
      ,AA.CHECK_AMT
      ,AA.SYSTEM
  FROM LAPPPAW1 AW JOIN LMNPPAA5 AA ON AW.CHECK_NO = AA.CHECK_NO AND AW.CHECK_SHRT = AA.CHECK_SHRT AND AA.PGM_TYPE = '2'
    WHERE AW.DEPT_DATE1 = :exec_date 
      AND AW.REPORT_TP = 'X0005'
      AND AW.DATA_FLAG = ''
 ORDER BY AA.SYSTEM, AA.PAID_ID, AW.CHECK_NO, AW.CHECK_SHRT
";

            try
            {
                cmdQ.Connection = conn400;
                cmdQ.CommandText = strSQLQ;

                cmdQ.Parameters.Clear();

                cmdQ.Parameters.Add("exec_date", exec_date);

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

                    rtnList.Add(d);
                }


                cmdQ.Dispose();
                cmdQ = null;

                logger.Info("qryForBAP7029P1 end!");

            }
            catch (Exception e)
            {
                logger.Error(e.ToString());
                throw e;
            }

            return rtnList;

        }





        public List<VeCleanModel> qryForVeClean(EacConnection conn400, string exec_date, string upd_id, string report_tp)
        {
            logger.Info("qryForVeClean begin!");

            List<VeCleanModel> rtnList = new List<VeCleanModel>();

            EacCommand cmdQ = new EacCommand();

            string strSQLQ = @"
SELECT DISTINCT 
       AW.CHECK_NO
      ,AW.CHECK_SHRT
      ,AW.DATA_FLAG
      ,AW.FILLER_10
      ,AW.FILLER_16
      ,AA.SYSTEM
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
  FROM LAPPPAW1 AW JOIN LMNPPAA5 AA ON AW.CHECK_SHRT = AA.CHECK_SHRT AND AW.CHECK_NO = AA.CHECK_NO
    WHERE AW.DEPT_DATE1 = :exec_date 
      AND AW.REPORT_TP = :report_tp 
      and aa.pgm_type = '2'
 ORDER BY AW.CHECK_NO, AW.CHECK_SHRT
";

            try
            {
                cmdQ.Connection = conn400;
                cmdQ.CommandText = strSQLQ;

                cmdQ.Parameters.Clear();

                cmdQ.Parameters.Add("exec_date", exec_date);
                cmdQ.Parameters.Add("report_tp", report_tp);

                DbDataReader result = cmdQ.ExecuteReader();

                while (result.Read())
                {
                    VeCleanModel d = new VeCleanModel();

                    d.check_no = result["CHECK_NO"]?.ToString().Trim();
                    d.check_acct_short = result["CHECK_SHRT"]?.ToString().Trim();
                    d.data_flag = result["DATA_FLAG"]?.ToString().Trim();
                    d.filler_10 = result["FILLER_10"]?.ToString().Trim();
                    d.filler_16 = result["FILLER_16"]?.ToString().Trim();

                    d.system = result["SYSTEM"]?.ToString().Trim();
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



        public void updateForOAP0009(List<FAPPPAWModel> dataList, EacConnection conn, EacTransaction transaction)
        {
            logger.Info("updateForOAP0009 begin!");


            EacCommand cmd = new EacCommand();


            string strSQL = "";
            strSQL = @"
 UPDATE FAPPPAW0 
   SET R_ZIP_CODE = :R_ZIP_CODE
      ,R_ADDR = :R_ADDR
 WHERE CHECK_NO = :CHECK_NO
";

            try
            {
                cmd.Connection = conn;
                cmd.Transaction = transaction;
                cmd.CommandText = strSQL;

                foreach (FAPPPAWModel d in dataList)
                {
                    cmd.Parameters.Clear();

                    cmd.Parameters.Add("R_ZIP_CODE", d.r_zip_code);
                    cmd.Parameters.Add("R_ADDR", d.r_addr);
                    cmd.Parameters.Add("CHECK_NO", d.check_no);

                    cmd.ExecuteNonQuery();
                }

                cmd.Dispose();
                cmd = null;

                logger.Info("updateForOAP0009 end!");

            }
            catch (Exception e)
            {
                logger.Error(e.ToString());
                throw e;
            }
        }

        public void insertForOAP0001(List<FAPPPAWModel> dataList, EacConnection conn, EacTransaction transaction)
        {
            logger.Info("insert FAPPPAW0 begin!");


            EacCommand cmd = new EacCommand();


            string strSQL = "";


            strSQL = @"insert into FAPPPAW0 
            ( REPORT_TP
             ,SYSTEM
             ,DEPT_GROUP
             ,CHECK_NO
             ,CHECK_SHRT
             ,DEPT_DATE1
             ,FILLER_16
             ,ENTRY_ID
             ,ENTRY_DATE
             ,ENTRY_TIME
            ) VALUES (
               :REPORT_TP
             , :SYSTEM
             , :DEPT_GROUP
             , :CHECK_NO
             , :CHECK_SHRT
             , :DEPT_DATE1
             , :FILLER_16
             , :ENTRY_ID
             , :ENTRY_DATE
             , :ENTRY_TIME
            ) ";


            try
            {
                cmd.Connection = conn;
                cmd.Transaction = transaction;
                cmd.CommandText = strSQL;


                foreach (FAPPPAWModel d in dataList)
                {
                    cmd.Parameters.Clear();

                    cmd.Parameters.Add("REPORT_TP", StringUtil.toString(d.report_tp));
                    cmd.Parameters.Add("SYSTEM", StringUtil.toString(d.system));
                    cmd.Parameters.Add("DEPT_GROUP", StringUtil.toString(d.dept_group));
                    cmd.Parameters.Add("CHECK_NO", StringUtil.toString(d.check_no));
                    cmd.Parameters.Add("CHECK_SHRT", StringUtil.toString(d.check_shrt));
                    cmd.Parameters.Add("DEPT_DATE1", StringUtil.toString(d.dept_date1) == "" ? "0" : d.dept_date1);
                    cmd.Parameters.Add("FILLER_16", StringUtil.toString(d.dept_date1) == "" ? "0" : d.dept_date1);
                    cmd.Parameters.Add("ENTRY_ID", StringUtil.toString(d.entry_id));
                    cmd.Parameters.Add("ENTRY_DATE", d.entry_date == "" ? "0" : d.entry_date);
                    cmd.Parameters.Add("ENTRY_TIME", d.entry_time == "" ? "0" : d.entry_time);



                    cmd.ExecuteNonQuery();

                }

                cmd.Dispose();
                cmd = null;

                logger.Info("insertForOAP0001 FAPPPAW0 end!");

            }
            catch (Exception e)
            {
                logger.Error(e.ToString());
                throw e;
            }
        }

        public void insert(List<FAPPPAWModel> dataList, EacConnection conn, EacTransaction transaction)
        {
            logger.Info("insert FAPPPAW0 begin!");


            EacCommand cmd = new EacCommand();


            string strSQL = "";


            strSQL = @"insert into FAPPPAW0 
            ( REPORT_TP
             ,SYSTEM
             ,DEPT_GROUP
             ,PAID_ID
             ,CHECK_NO
             ,CHECK_SHRT
             ,DATA_FLAG
             ,DEPT_DATE1
             ,R_ZIP_CODE
             ,R_ADDR
             ,ENTRY_ID
             ,ENTRY_DATE
             ,ENTRY_TIME
            ) VALUES (
              :REPORT_TP
             ,:SYSTEM
             ,:DEPT_GROUP
             ,:PAID_ID
             ,:CHECK_NO
             ,:CHECK_SHRT
             ,:DATA_FLAG
             ,:DEPT_DATE1
             ,:R_ZIP_CODE
             ,:R_ADDR
             ,:ENTRY_ID
             ,:ENTRY_DATE
             ,:ENTRY_TIME
            ) ";


            try
            {
                cmd.Connection = conn;
                cmd.Transaction = transaction;
                cmd.CommandText = strSQL;


                foreach (FAPPPAWModel d in dataList)
                {
                    cmd.Parameters.Clear();

                    cmd.Parameters.Add("REPORT_TP", StringUtil.toString(d.report_tp));
                    cmd.Parameters.Add("SYSTEM", StringUtil.toString(d.system));
                    cmd.Parameters.Add("DEPT_GROUP", StringUtil.toString(d.dept_group));
                    cmd.Parameters.Add("PAID_ID", StringUtil.toString(d.paid_id));
                    cmd.Parameters.Add("CHECK_NO", StringUtil.toString(d.check_no));
                    cmd.Parameters.Add("CHECK_SHRT", StringUtil.toString(d.check_shrt));
                    cmd.Parameters.Add("DATA_FLAG", StringUtil.toString(d.data_flag));
                    cmd.Parameters.Add("DEPT_DATE1", StringUtil.toString(d.dept_date1) == "" ? "0" : d.dept_date1);
                    cmd.Parameters.Add("R_ZIP_CODE", StringUtil.toString(d.r_zip_code));
                    cmd.Parameters.Add("R_ADDR", StringUtil.toString(d.r_addr));
                    cmd.Parameters.Add("ENTRY_ID", StringUtil.toString(d.entry_id));
                    cmd.Parameters.Add("ENTRY_DATE", d.entry_date == "" ? "0" : d.entry_date);
                    cmd.Parameters.Add("ENTRY_TIME", d.entry_time == "" ? "0" : d.entry_time);



                    cmd.ExecuteNonQuery();

                }

                cmd.Dispose();
                cmd = null;

                logger.Info("insert FAPPPAW0 end!");

            }
            catch (Exception e)
            {
                logger.Error(e.ToString());
                throw e;
            }
        }

        public void delByCheckNo(string report_tp, string dept_group, string check_no, EacConnection conn, EacTransaction transaction)
        {
            logger.Info("delByCheckNo begin!");

            EacCommand cmd = new EacCommand();


            string strSQL = "";
            strSQL = @"
DELETE FAPPPAW0 
  WHERE REPORT_TP = :report_tp
    AND DEPT_GROUP = :dept_group
    and check_no = :check_no
 ";


            try
            {
                cmd.Connection = conn;
                cmd.Transaction = transaction;
                cmd.CommandText = strSQL;

                cmd.Parameters.Clear();

                cmd.Parameters.Add("report_tp", report_tp);
                cmd.Parameters.Add("dept_group", dept_group);
                cmd.Parameters.Add("check_no", check_no);

                cmd.ExecuteNonQuery();


                cmd.Dispose();
                cmd = null;

                logger.Info("delByCheckNo FAPPPAW0 end!");

            }
            catch (Exception e)
            {
                logger.Error(e.ToString());
                throw e;
            }
        }


        /// <summary>
        /// 刪除FAPPPAW0
        /// ADD BY DAIYU 20200722
        /// </summary>
        /// <param name="report_tp"></param>
        /// <param name="dept_group"></param>
        /// <param name="conn"></param>
        /// <param name="transaction"></param>
        public void delete(string report_tp, string dept_group, EacConnection conn, EacTransaction transaction)
        {
            logger.Info("delete begin!");


            EacCommand cmd = new EacCommand();


            string strSQL = "";


            strSQL = @"
DELETE FAPPPAW0 
  WHERE REPORT_TP = :report_tp
    AND DEPT_GROUP = :dept_group
 ";


            try
            {
                cmd.Connection = conn;
                cmd.Transaction = transaction;
                cmd.CommandText = strSQL;

                cmd.Parameters.Clear();

                cmd.Parameters.Add("report_tp", report_tp);
                cmd.Parameters.Add("dept_group", dept_group);

                cmd.ExecuteNonQuery();


                cmd.Dispose();
                cmd = null;

                logger.Info("delete FAPPPAW0 end!");

            }
            catch (Exception e)
            {
                logger.Error(e.ToString());
                throw e;
            }
        }


    }
}