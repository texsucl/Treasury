
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using FAP.Web.Models;
using System.Data.Entity.SqlServer;
using System.Transactions;
using FAP.Web.BO;
using FAP.Web.ViewModels;
using System.Web.Mvc;

/// <summary>
/// 功能說明：FAP_TEL_CHECK_HIS 電訪支票暫存檔
/// 初版作者：20200824 Daiyu
/// 修改歷程：20200824 Daiyu
/// 需求單號：
/// 修改內容：初版
/// --------------------------------------------------

/// </summary>
/// 

namespace FAP.Web.Daos
{
    public class FAPTelCheckHisDao
    {

        /// <summary>
        /// 重新派件覆核作業-查詢
        /// </summary>
        /// <param name="tel_interview_id_o"></param>
        /// <param name="check_no"></param>
        /// <returns></returns>
        public List<OAP0044Model> qryForOAP0044A()
        {
            using (new TransactionScope(TransactionScopeOption.Required, new TransactionOptions
            {
                IsolationLevel = System.Transactions.IsolationLevel.ReadUncommitted
            }))
            {
                using (dbFGLEntities db = new dbFGLEntities())
                {
                    var rows = (from m in db.FAP_VE_TRACE
                                join poli in db.FAP_VE_TRACE_POLI on new { m.check_no, m.check_acct_short } equals new { poli.check_no, poli.check_acct_short } into psPoli
                                from xPoli in psPoli.DefaultIfEmpty()
                                join tel in db.FAP_TEL_CHECK on new { m.check_no, m.check_acct_short } equals new { tel.check_no, tel.check_acct_short }
                                join telH in db.FAP_TEL_CHECK_HIS on new { tel.system, tel.check_no, tel.check_acct_short, tel.tel_std_type, tel.tel_std_aply_no } equals new { telH.system, telH.check_no, telH.check_acct_short, telH.tel_std_type, telH.tel_std_aply_no }

                                join telInterview in db.FAP_TEL_INTERVIEW on tel.tel_proc_no equals telInterview.tel_proc_no into psTelInterview
                                from xTelInterview in psTelInterview.DefaultIfEmpty()

                                where 1 == 1
                                   & m.status != "1"
                                   & telH.appr_stat == "1"
                                   & tel.data_flag == "Y"
                                   & tel.tel_std_type == "tel_assign_case"
                                   & telH.aply_no.StartsWith("0044")
                                   & (tel.dispatch_status == "3" || (tel.dispatch_status == "0" & tel.tel_interview_id != null) || (tel.dispatch_status == "1" & xTelInterview == null))

                                select new OAP0044Model
                                {
                                    aply_no = telH.aply_no,
                                    temp_id = m.paid_id == "" ? m.check_no : m.paid_id,
                                    tel_interview_id_o = tel.tel_interview_id,
                                    tel_interview_id = telH.tel_interview_id,
                                    system = m.system,
                                    check_no = m.check_no,
                                    check_acct_short = m.check_acct_short,
                                    check_date = m.check_date == null ? "" : (SqlFunctions.DatePart("year", m.check_date) - 1911) + "/" +
                                                  SqlFunctions.DatePart("m", m.check_date) + "/" +
                                                  SqlFunctions.DateName("day", m.check_date).Trim(),
                                    check_amt = (Decimal)m.check_amt,
                                    o_paid_cd = xPoli.o_paid_cd,
                                    paid_id = m.paid_id,
                                    paid_name = m.paid_name,
                                    data_status = tel.data_status,
                                    update_id = telH.update_id,
                                    update_datetime = telH.update_datetime == null ? "" : SqlFunctions.DateName("year", telH.update_datetime) + "/" +
                                                 SqlFunctions.DatePart("m", telH.update_datetime) + "/" +
                                                 SqlFunctions.DateName("day", telH.update_datetime).Trim() + " " +
                                                 SqlFunctions.DateName("hh", telH.update_datetime).Trim() + ":" +
                                                 SqlFunctions.DateName("n", telH.update_datetime).Trim() + ":" +
                                                 SqlFunctions.DateName("s", telH.update_datetime).Trim(),
                                    dispatch_date = telH.dispatch_date == null ? "" : SqlFunctions.DateName("year", telH.dispatch_date) + "/" +
                                                 SqlFunctions.DatePart("m", telH.dispatch_date) + "/" +
                                                 SqlFunctions.DateName("day", telH.dispatch_date).Trim() + " "
                                }).Distinct().ToList();

                    return rows;

                }
            }

        }



        public List<OAP0046DModel> qryByTelProcNo(string tel_proc_no, string data_type, string appr_stat)
        {
            bool bApprStat = true;
            if (!"".Equals(appr_stat))
                bApprStat = false;

            using (new TransactionScope(TransactionScopeOption.Required, new TransactionOptions
            {
                IsolationLevel = System.Transactions.IsolationLevel.ReadUncommitted
            }))
            {
                using (dbFGLEntities db = new dbFGLEntities())
                {
                    var rows = (from telM in db.FAP_TEL_CHECK_HIS
                                join m in db.FAP_VE_TRACE on new { telM.system, telM.check_acct_short, telM.check_no } equals new { m.system, m.check_acct_short, m.check_no }
                                join poli in db.FAP_VE_TRACE_POLI on new { m.check_no, m.check_acct_short } equals new { poli.check_no, poli.check_acct_short } into psPoli
                                from xPoli in psPoli.DefaultIfEmpty()
                                join telInterview in db.FAP_TEL_INTERVIEW_HIS on telM.aply_no equals telInterview.aply_no

                                where 1 == 1
                                   & telM.tel_std_type == "tel_assign_case"
                                   & telM.tel_proc_no == tel_proc_no
                                   & (bApprStat || (!bApprStat & telInterview.appr_stat == appr_stat))
                                   & telInterview.data_type == data_type
                                   & m.status != "1"
                                select new OAP0046DModel
                                {
                                    tel_proc_no = telM.tel_proc_no,
                                    tel_interview_id = telM.tel_interview_id,
                                    status = m.status,
                                    paid_id = m.paid_id,
                                    paid_name = m.paid_name,
                                    check_no = telM.check_no,
                                    check_acct_short = telM.check_acct_short,
                                    check_date = m.check_date == null ? "" : (SqlFunctions.DatePart("year", m.check_date) - 1911) + "/" +
                                                  SqlFunctions.DatePart("m", m.check_date) + "/" +
                                                  SqlFunctions.DateName("day", m.check_date).Trim(),
                                    main_amt = xPoli == null ? (Decimal)m.check_amt : (Decimal)xPoli.main_amt,
                                    check_amt = (Decimal)m.check_amt,
                                    o_paid_cd = xPoli.o_paid_cd == null ? "" : xPoli.o_paid_cd,
                                    system = m.system,
                                    policy_no = xPoli.policy_no == null ? "" : xPoli.policy_no,
                                    policy_seq = xPoli == null ? 0 : xPoli.policy_seq,
                                    id_dup = xPoli.id_dup == null ? "" : xPoli.id_dup,
                                    dispatch_status = telM.dispatch_status,
                                    tel_result = telInterview.tel_result == null ? "" : telInterview.tel_result,
                                    tel_appr_result = telInterview.tel_appr_result == null ? "" : telInterview.tel_appr_result,


                                }).Distinct().ToList<OAP0046DModel>();

                    return rows;

                }
            }

        }


        public List<OAP0046DModel> qryByCheckNo(string system, string check_acct_short, string check_no, string data_type, string appr_stat)
        {
            bool bApprStat = true;
            if (!"".Equals(appr_stat))
                bApprStat = false;

            using (new TransactionScope(TransactionScopeOption.Required, new TransactionOptions
            {
                IsolationLevel = System.Transactions.IsolationLevel.ReadUncommitted
            }))
            {
                using (dbFGLEntities db = new dbFGLEntities())
                {
                    var rows = (from telM in db.FAP_TEL_CHECK_HIS
                                join m in db.FAP_VE_TRACE on new { telM.system, telM.check_acct_short, telM.check_no } equals new { m.system, m.check_acct_short, m.check_no }
                                join poli in db.FAP_VE_TRACE_POLI on new { m.check_no, m.check_acct_short } equals new { poli.check_no, poli.check_acct_short } into psPoli
                                from xPoli in psPoli.DefaultIfEmpty()
                                join telInterview in db.FAP_TEL_INTERVIEW_HIS on telM.aply_no equals telInterview.aply_no 

                                where 1 == 1
                                   & telM.tel_std_type == "tel_assign_case"
                                   & telM.system == system
                                   & telM.check_acct_short == check_acct_short
                                   & telM.check_no == check_no
                                   & (bApprStat || (!bApprStat & telInterview.appr_stat == appr_stat))
                                   & telInterview.data_type == data_type
                                select new OAP0046DModel
                                {
                                    tel_proc_no = telM.tel_proc_no,
                                    tel_interview_id = telM.tel_interview_id,
                                    status = m.status,
                                    paid_id = m.paid_id,
                                    paid_name = m.paid_name,
                                    check_no = telM.check_no,
                                    check_acct_short = telM.check_acct_short,
                                    check_date = m.check_date == null ? "" : (SqlFunctions.DatePart("year", m.check_date) - 1911) + "/" +
                                                  SqlFunctions.DatePart("m", m.check_date) + "/" +
                                                  SqlFunctions.DateName("day", m.check_date).Trim(),
                                    main_amt = (Decimal)xPoli.main_amt,
                                    check_amt = (Decimal)m.check_amt,
                                    o_paid_cd = xPoli.o_paid_cd,
                                    system = m.system,
                                    policy_no = xPoli.policy_no,
                                    policy_seq = xPoli.policy_seq,
                                    id_dup = xPoli.id_dup,
                                    dispatch_status = telM.dispatch_status,
                                    tel_result = telInterview.tel_result == null ? "" : telInterview.tel_result,
                                    tel_appr_result = telInterview.tel_appr_result == null ? "" : telInterview.tel_appr_result

                                }).Distinct().ToList<OAP0046DModel>();

                    return rows;

                }
            }

        }

        


        public List<TelDispatchRptModel> qryOAP0043DRpt(string tel_std_type)
        {
            if (!"sms_notify_case".Equals(tel_std_type))
            {
                using (dbFGLEntities db = new dbFGLEntities())
                {



                    var rows = (from telM in db.FAP_TEL_CHECK
                                join telHis in db.FAP_TEL_CHECK_HIS.Where(x => x.appr_stat == "" || x.appr_stat == "1" || x.appr_stat == null) on new { telM.system, telM.check_no, telM.check_acct_short, telM.tel_std_type, telM.tel_std_aply_no }
                                      equals new { telHis.system, telHis.check_no, telHis.check_acct_short, telHis.tel_std_type, telHis.tel_std_aply_no } into psTelHis
                                from xTelHis in psTelHis.DefaultIfEmpty()

                                join m in db.FAP_VE_TRACE on new { telM.system, telM.check_acct_short, telM.check_no } equals new { m.system, m.check_acct_short, m.check_no }
                                join poli in db.FAP_VE_TRACE_POLI on new { m.check_no, m.check_acct_short } equals new { poli.check_no, poli.check_acct_short } into psPoli
                                from xPoli in psPoli.DefaultIfEmpty()

                                where 1 == 1
                                & telM.tel_std_type == tel_std_type
                                & telM.data_flag == "Y"
                                & telM.tel_interview_id == ""

                                select new TelDispatchRptModel
                                {
                                    temp_id = m.paid_id == "" ? m.check_no : m.paid_id,
                                    tel_interview_id = xTelHis.tel_interview_id == null ? "" : xTelHis.tel_interview_id,
                                    fsc_range = telM.fsc_range,
                                    check_no = telM.check_no,
                                    check_acct_short = telM.check_acct_short,
                                    amt_range = telM.amt_range,
                                    main_amt = xPoli == null ? (Decimal)0 : (Decimal)xPoli.main_amt,
                                    check_date = m.check_date == null ? "" : (SqlFunctions.DatePart("year", m.check_date) - 1911) + "/" +
                                                      SqlFunctions.DatePart("m", m.check_date) + "/" +
                                                      SqlFunctions.DateName("day", m.check_date).Trim(),
                                    check_amt = xPoli == null ? (Decimal)0 : (Decimal)m.check_amt,
                                    o_paid_cd = xPoli.o_paid_cd,
                                    paid_id = m.paid_id,
                                    paid_name = m.paid_name,
                                    system = m.system,
                                    policy_no = xPoli.policy_no,
                                    policy_seq = xPoli == null ? (int)0 : xPoli.policy_seq,
                                    id_dup = xPoli.id_dup,
                                    change_id = xPoli == null ? "" : xPoli.change_id,
                                    sec_stat = telM.sec_stat
                                }).Distinct().ToList<TelDispatchRptModel>();

                    return rows;
                }
            }
            else {
                using (dbFGLEntities db = new dbFGLEntities())
                {

                    var rows = (from telM in db.FAP_TEL_CHECK
                                join telHis in db.FAP_TEL_CHECK_HIS.Where(x => x.appr_stat == "" || x.appr_stat == "1" || x.appr_stat == null) on new { telM.system, telM.check_no, telM.check_acct_short, telM.tel_std_type, telM.tel_std_aply_no }
                                      equals new { telHis.system, telHis.check_no, telHis.check_acct_short, telHis.tel_std_type, telHis.tel_std_aply_no } into psTelHis
                                from xTelHis in psTelHis.DefaultIfEmpty()

                                join m in db.FAP_TEL_SMS_TEMP on new { telM.system, telM.check_acct_short, telM.check_no, telM.tel_std_aply_no } equals new { m.system, m.check_acct_short, m.check_no, m.tel_std_aply_no }
                                where 1 == 1
                                & telM.tel_std_type == tel_std_type
                                & telM.data_flag == "Y"
                                & telM.tel_interview_id == ""

                                select new TelDispatchRptModel
                                {
                                    temp_id = m.paid_id == "" ? m.check_no : m.paid_id,
                                    tel_interview_id = xTelHis.tel_interview_id == null ? "" : xTelHis.tel_interview_id,
                                    fsc_range = telM.fsc_range,
                                    check_no = telM.check_no,
                                    check_acct_short = telM.check_acct_short,
                                    amt_range = telM.amt_range,
                                    main_amt = (Decimal)m.check_amt,
                                    check_date = m.check_date == null ? "" : (SqlFunctions.DatePart("year", m.check_date)) + "/" +
                                                      SqlFunctions.DatePart("m", m.check_date) + "/" +
                                                      SqlFunctions.DateName("day", m.check_date).Trim(),
                                    check_amt = (Decimal)m.check_amt,
                                    o_paid_cd = m.o_paid_cd,
                                    paid_id = m.paid_id,
                                    paid_name = m.paid_name,
                                    system = m.system,
                                    policy_no = m.policy_no,
                                    policy_seq = m.policy_seq,
                                    id_dup = m.id_dup,
                                    appl_id = m.appl_id,
                                    appl_name = m.appl_name,
                                    ins_id = m.ins_id,
                                    ins_name = m.ins_name,
                                    //sysmark = xPoli.sysmark,
                                    //send_id = xPoli.send_id,
                                    //send_name = xPoli.send_name,
                                    //send_unit = xPoli.send_unit,
                                    //send_tel = xPoli.mobile,
                                    sec_stat = telM.sec_stat
                                }).Distinct().ToList<TelDispatchRptModel>();

                    return rows;
                }
            }
                
        }




        public List<TelDispatchRptModel> qryByAplyNo(string tel_std_type, string aply_no)
        {

            using (dbFGLEntities db = new dbFGLEntities())
            {

                var rows = (from telM in db.FAP_TEL_CHECK
                            join telHis in db.FAP_TEL_CHECK_HIS on new { telM.system, telM.check_no, telM.check_acct_short, telM.tel_std_type, telM.tel_std_aply_no }
                                  equals new { telHis.system, telHis.check_no, telHis.check_acct_short, telHis.tel_std_type, telHis.tel_std_aply_no }

                            join m in db.FAP_VE_TRACE on new { telM.system, telM.check_acct_short, telM.check_no } equals new { m.system, m.check_acct_short, m.check_no }
                            join poli in db.FAP_VE_TRACE_POLI on new { m.check_no, m.check_acct_short } equals new { poli.check_no, poli.check_acct_short } into psPoli
                            from xPoli in psPoli.DefaultIfEmpty()
                            where 1 == 1
                            & telHis.tel_std_type == tel_std_type
                            & telHis.aply_no == aply_no

                            select new TelDispatchRptModel
                            {
                                temp_id = m.paid_id == "" ? m.check_no : m.paid_id,
                                tel_interview_id = telHis.tel_interview_id == null ? "" : telHis.tel_interview_id,
                                dispatch_date = telHis.dispatch_date == null ? "" : (SqlFunctions.DatePart("year", telHis.dispatch_date)) + "/" +
                                                  SqlFunctions.DatePart("m", telHis.dispatch_date) + "/" +
                                                  SqlFunctions.DateName("day", telHis.dispatch_date).Trim(),
                                fsc_range = telM.fsc_range,
                                check_no = telM.check_no,
                                check_acct_short = telM.check_acct_short,
                                amt_range = telM.amt_range,
                                main_amt = (Decimal)xPoli.main_amt,
                                check_date = m.check_date == null ? "" : (SqlFunctions.DatePart("year", m.check_date) - 1911) + "/" +
                                                  SqlFunctions.DatePart("m", m.check_date) + "/" +
                                                  SqlFunctions.DateName("day", m.check_date).Trim(),
                                check_amt = (Decimal)m.check_amt,
                                o_paid_cd = xPoli.o_paid_cd,
                                paid_id = m.paid_id,
                                paid_name = m.paid_name,
                                system = m.system,
                                policy_no = xPoli.policy_no,
                                policy_seq = xPoli.policy_seq,
                                id_dup = xPoli.id_dup,
                                sec_stat = telM.sec_stat,
                                status = m.status
                            }).Distinct().ToList<TelDispatchRptModel>();

                return rows;
            }
        }


        public void updateApprOAP0044A(FAP_TEL_CHECK_HIS d, SqlConnection conn, SqlTransaction transaction)
        {

            try
            {
                string sql = @"UPDATE [FAP_TEL_CHECK_HIS]
SET appr_stat = @appr_stat
  , appr_id = @appr_id
  , approve_datetime = @approve_datetime
WHERE aply_no = @aply_no
  AND tel_std_type = @tel_std_type
  and check_no = @check_no
  and check_acct_short = @check_acct_short

          ";

                SqlCommand cmd = conn.CreateCommand();

                cmd.Connection = conn;
                cmd.Transaction = transaction;

                cmd.CommandText = sql;

                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("@aply_no", d.aply_no);
                cmd.Parameters.AddWithValue("@tel_std_type", d.tel_std_type);
                cmd.Parameters.AddWithValue("@check_no", d.check_no);
                cmd.Parameters.AddWithValue("@check_acct_short", d.check_acct_short);
                cmd.Parameters.AddWithValue("@appr_stat", d.appr_stat);
                cmd.Parameters.AddWithValue("@appr_id", d.appr_id);
                cmd.Parameters.AddWithValue("@approve_datetime", d.approve_datetime);

                cmd.ExecuteNonQuery();



            }
            catch (Exception e)
            {
                throw e;
            }
        }



        /// <summary>
        /// 異動覆核結果
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="apprStatus"></param>
        /// <param name="aply_no"></param>
        /// <param name="code_type"></param>
        /// <param name="conn"></param>
        /// <param name="transaction"></param>
        public void updateApprStatus(string userId, string appr_stat, string aply_no, string tel_std_type
            , DateTime now, SqlConnection conn, SqlTransaction transaction)
        {

            try
            {
                string sql = @"UPDATE [FAP_TEL_CHECK_HIS]
SET appr_stat = @appr_stat
  , appr_id = @appr_id
  , approve_datetime = @approve_datetime
WHERE APLY_NO = @APLY_NO
  AND tel_std_type = @tel_std_type

          ";

                SqlCommand cmd = conn.CreateCommand();

                cmd.Connection = conn;
                cmd.Transaction = transaction;

                cmd.CommandText = sql;

                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("@APLY_NO", aply_no);
                cmd.Parameters.AddWithValue("@tel_std_type", tel_std_type);
                cmd.Parameters.AddWithValue("@appr_stat", appr_stat);
                cmd.Parameters.AddWithValue("@appr_id", userId);
                cmd.Parameters.AddWithValue("@approve_datetime", now);

                cmd.ExecuteNonQuery();



            }
            catch (Exception e)
            {
                throw e;
            }
        }


        /// <summary>
        /// 依"支票號碼"為鍵項，設定"第一次電訪人員"
        /// </summary>
        /// <param name="tel_std_type"></param>
        /// <param name="system"></param>
        /// <param name="check_no"></param>
        /// <param name="check_acct_short"></param>
        /// <param name="tel_interview_id"></param>
        /// <param name="usr_id"></param>
        /// <param name="dt"></param>
        /// <param name="conn"></param>
        /// <param name="transaction"></param>
        public void updProdIdByCheckNo(string tel_std_type, string system, string check_no, string check_acct_short, string tel_interview_id
            , string usr_id, DateTime dt, SqlConnection conn, SqlTransaction transaction)
        {
            try
            {

                string sql = @"
UPDATE FAP_TEL_CHECK_HIS
  SET tel_interview_id = @tel_interview_id
     ,update_id = @update_id
     ,update_datetime = @update_datetime
 WHERE aply_no = ''
   AND tel_std_type = @tel_std_type
   and system = @system 
   and check_no = @check_no
   and check_acct_short = @check_acct_short";

                SqlCommand cmd = conn.CreateCommand();

                cmd.Connection = conn;
                cmd.Transaction = transaction;

                cmd.CommandText = sql;

                cmd.Parameters.Clear();

                cmd.Parameters.AddWithValue("@tel_interview_id", StringUtil.toString(tel_interview_id));
                cmd.Parameters.AddWithValue("@update_id", StringUtil.toString(usr_id));
                cmd.Parameters.AddWithValue("@update_datetime", dt);


                cmd.Parameters.AddWithValue("@tel_std_type", StringUtil.toString(tel_std_type));
                cmd.Parameters.AddWithValue("@system", StringUtil.toString(system));
                cmd.Parameters.AddWithValue("@check_no", StringUtil.toString(check_no));
                cmd.Parameters.AddWithValue("@check_acct_short", StringUtil.toString(check_acct_short));

                cmd.ExecuteNonQuery();

            }
            catch (Exception e)
            {
                throw e;
            }
        }




        /// <summary>
        /// OAP0043執行"申請覆核"，將覆核單號補上
        /// </summary>
        /// <param name="type"></param>
        /// <param name="aply_no"></param>
        /// <param name="update_id"></param>
        /// <param name="dt"></param>
        /// <param name="conn"></param>
        /// <param name="transaction"></param>
        public void updateAplyNo(string type, string aply_no, string update_id, DateTime dt, SqlConnection conn, SqlTransaction transaction)
        {
            try
            {

                string sql = @"
UPDATE FAP_TEL_CHECK_HIS
  SET APLY_NO = @aply_no
     ,appr_stat = '1'
     ,update_id = @update_id
     ,update_datetime = @update_datetime
 WHERE aply_no = ''
   AND tel_std_type = @type";

                SqlCommand cmd = conn.CreateCommand();

                cmd.Connection = conn;
                cmd.Transaction = transaction;

                cmd.CommandText = sql;

                cmd.Parameters.Clear();

                cmd.Parameters.AddWithValue("@aply_no", StringUtil.toString(aply_no));
                cmd.Parameters.AddWithValue("@type", StringUtil.toString(type));
                cmd.Parameters.AddWithValue("@update_id", StringUtil.toString(update_id));
                cmd.Parameters.AddWithValue("@update_datetime", dt);

                cmd.ExecuteNonQuery();

            }
            catch (Exception e)
            {
                throw e;
            }
        }



        public void delForOAP0043(FAP_TEL_CHECK_HIS d, SqlConnection conn, SqlTransaction transaction)
        {
            try
            {

                string sql = @"
DELETE FAP_TEL_CHECK_HIS
 WHERE aply_no = @aply_no
   AND tel_std_type = @tel_std_type
   AND fsc_range = @fsc_range
   AND amt_range = @amt_range";

                SqlCommand cmd = conn.CreateCommand();

                cmd.Connection = conn;
                cmd.Transaction = transaction;

                cmd.CommandText = sql;

                cmd.Parameters.Clear();

                cmd.Parameters.AddWithValue("@aply_no", StringUtil.toString(d.aply_no));
                cmd.Parameters.AddWithValue("@tel_std_type", StringUtil.toString(d.tel_std_type));
                cmd.Parameters.AddWithValue("@fsc_range", StringUtil.toString(d.fsc_range));
                cmd.Parameters.AddWithValue("@amt_range", StringUtil.toString(d.amt_range));
                cmd.ExecuteNonQuery();

            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public void delForOAP0042(string tel_std_type, SqlConnection conn, SqlTransaction transaction)
        {
            try
            {

                string sql = @"
DELETE FAP_TEL_CHECK_HIS
 WHERE aply_no = ''
  and tel_std_type = @tel_std_type";

                SqlCommand cmd = conn.CreateCommand();

                cmd.Connection = conn;
                cmd.Transaction = transaction;
                
                cmd.CommandText = sql;

                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("@tel_std_type", StringUtil.toString(tel_std_type));
                cmd.ExecuteNonQuery();

            }
            catch (Exception e)
            {
                throw e;
            }
        }



        public void insertFromFormal(DateTime dt, FAP_TEL_CHECK_HIS d, string _tel_interview_id, string srce_pgm 
            , SqlConnection conn, SqlTransaction transaction)
        {

            try
            {

                string sql = @"
        INSERT INTO FAP_TEL_CHECK_HIS
           (aply_no
           ,system 
           ,check_no
           ,check_acct_short
           ,tel_std_aply_no
           ,tel_std_type
           ,fsc_range
           ,amt_range
           ,tel_proc_no
           ,tel_interview_id
           ,remark
           ,dispatch_date
           ,dispatch_status
           ,sms_date
           ,sms_status
           ,sec_stat
           ,appr_stat
           ,update_id
           ,update_datetime)
select @aply_no
           ,system 
           ,check_no
           ,check_acct_short
           ,tel_std_aply_no
           ,tel_std_type
           ,fsc_range
           ,amt_range
           ,@tel_proc_no
           ,@tel_interview_id
           ,remark
           ,case @srce_pgm
              when '0044' then @dispatch_date
              else dispatch_date
            end 
           ,case @srce_pgm
              when '0044' then '0'
              else dispatch_status
            end 
           ,sms_date
           ,sms_status
           ,sec_stat
           ,@appr_stat
           ,@update_id
           ,@update_datetime
from FAP_TEL_CHECK
where system = @system
  and check_no = @check_no
  and check_acct_short = @check_acct_short
  and tel_std_type = @tel_std_type
  and tel_interview_id = @_tel_interview_id
  and data_flag = 'Y'
";

                SqlCommand cmd = conn.CreateCommand();

                cmd.Connection = conn;
                cmd.Transaction = transaction;

                cmd.CommandText = sql;

                cmd.Parameters.Clear(); 
                cmd.Parameters.AddWithValue("@aply_no", StringUtil.toString(d.aply_no));
                cmd.Parameters.AddWithValue("@system", StringUtil.toString(d.system));
                cmd.Parameters.AddWithValue("@check_no", StringUtil.toString(d.check_no));
                cmd.Parameters.AddWithValue("@check_acct_short", StringUtil.toString(d.check_acct_short));
                cmd.Parameters.AddWithValue("@tel_std_type", StringUtil.toString(d.tel_std_type));
                cmd.Parameters.AddWithValue("@tel_proc_no", StringUtil.toString(d.tel_proc_no));
                cmd.Parameters.AddWithValue("@tel_interview_id", StringUtil.toString(d.tel_interview_id));
                cmd.Parameters.AddWithValue("@_tel_interview_id", StringUtil.toString(_tel_interview_id));
                cmd.Parameters.AddWithValue("@srce_pgm", StringUtil.toString(srce_pgm));
                cmd.Parameters.AddWithValue("@appr_stat", StringUtil.toString(d.appr_stat));
                cmd.Parameters.AddWithValue("@update_id", StringUtil.toString(d.update_id));
                cmd.Parameters.AddWithValue("@update_datetime", dt);

                cmd.Parameters.AddWithValue("@dispatch_date", System.Data.SqlDbType.DateTime).Value = (Object)d.dispatch_date ?? DBNull.Value;

                cmd.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        
        
        

    }
}