
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
/// 功能說明：FAP_TEL_INTERVIEW 電訪及追踨記錄檔
/// 初版作者：20200905 Daiyu
/// 修改歷程：20200905 Daiyu
/// 需求單號：
/// 修改內容：初版
/// --------------------------------------------------

/// </summary>
/// 

namespace FAP.Web.Daos
{
    public class FAPTelInterviewDao
    {
        static private NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public void updVeCleanFinish(FAP_TEL_INTERVIEW d, SqlConnection conn, SqlTransaction transaction)
        {

            try
            {

                string sql = @"
update FAP_TEL_INTERVIEW
  set tel_appr_result = @tel_appr_result
     ,tel_appr_datetime = @tel_appr_datetime
     ,dispatch_status = @dispatch_status
     ,clean_status = @clean_status
     ,clean_date = @clean_date
     ,clean_f_date = @clean_f_date
     ,update_id = @update_id
     ,update_datetime = @update_datetime
 where tel_proc_no = @tel_proc_no";

                SqlCommand cmd = conn.CreateCommand();

                cmd.Connection = conn;
                cmd.Transaction = transaction;

                cmd.CommandText = sql;

                cmd.Parameters.Clear();

                cmd.Parameters.AddWithValue("@tel_appr_result", StringUtil.toString(d.tel_appr_result));
                cmd.Parameters.AddWithValue("@tel_appr_datetime", System.Data.SqlDbType.DateTime).Value = (Object)d.tel_appr_datetime ?? DBNull.Value;
                cmd.Parameters.AddWithValue("@dispatch_status", StringUtil.toString(d.dispatch_status));
                cmd.Parameters.AddWithValue("@clean_status", StringUtil.toString(d.clean_status));
                cmd.Parameters.AddWithValue("@clean_date", System.Data.SqlDbType.DateTime).Value = (Object)d.clean_date ?? DBNull.Value;
                cmd.Parameters.AddWithValue("@clean_f_date", System.Data.SqlDbType.DateTime).Value = (Object)d.clean_f_date ?? DBNull.Value;
                cmd.Parameters.AddWithValue("@update_id", StringUtil.toString(d.update_id));
                cmd.Parameters.AddWithValue("@update_datetime", System.Data.SqlDbType.DateTime).Value = (Object)d.update_datetime ?? DBNull.Value;
                cmd.Parameters.AddWithValue("@tel_proc_no", StringUtil.toString(d.tel_proc_no));


                cmd.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public List<OAP0052Model> qryForOAP0052(DateTime clean_date_b, DateTime clean_date_e, string status)
        {
            bool bStatus = true;

            if (!"".Equals(StringUtil.toString(status)))
                bStatus = false;

            string[] status_arr = status.Split('|');

            using (new TransactionScope(TransactionScopeOption.Required, new TransactionOptions
            {
                IsolationLevel = System.Transactions.IsolationLevel.ReadUncommitted
            }))
            {
                using (dbFGLEntities db = new dbFGLEntities())
                {
                    var rows = (from telM in db.FAP_TEL_CHECK
                                join m in db.FAP_VE_TRACE on new { telM.system, telM.check_acct_short, telM.check_no } equals new { m.system, m.check_acct_short, m.check_no }
                                join poli in db.FAP_VE_TRACE_POLI on new { m.check_no, m.check_acct_short } equals new { poli.check_no, poli.check_acct_short } into psPoli
                                from xPoli in psPoli.DefaultIfEmpty()
                                join i in db.FAP_TEL_INTERVIEW on telM.tel_proc_no equals i.tel_proc_no
                                join telInterview in db.FAP_TEL_INTERVIEW_HIS.Where(x => (x.data_type == "1" || x.data_type == "2") & x.tel_appr_result == "13" & x.appr_stat == "2") on telM.tel_proc_no equals telInterview.tel_proc_no
                                join veCode in db.FAP_TEL_CODE.Where(x => x.code_type == "tel_clean") on i.clean_status equals veCode.code_id

                                where 1 == 1
                                   & telM.tel_std_type == "tel_assign_case"
                                   & telM.data_flag == "Y"
                                   //& m.status != "1"  //modify by daiyu 20210304
                                   //& telInterview.tel_appr_result == "13"
                                   //& telInterview.clean_f_date == null
                                   //& new string[] { "2" }.Contains(telM.dispatch_status)
                                   & (telInterview.approve_datetime >= clean_date_b & telInterview.approve_datetime <= clean_date_e)
                                   & (bStatus || (!bStatus & status_arr.Contains(m.status)))
                        

                                select new OAP0052Model
                                {
                                    tel_proc_no = telM.tel_proc_no,
                                    paid_id = m.paid_id,
                                    paid_name = m.paid_name,
                                    check_acct_short = m.check_acct_short,
                                    check_no = m.check_no,
                                    check_date = m.check_date == null ? "" : (SqlFunctions.DatePart("year", m.check_date) - 1911) + "/" +
                                                 SqlFunctions.DatePart("m", m.check_date) + "/" +
                                                 SqlFunctions.DateName("day", m.check_date).Trim(),
                                    check_amt = m.check_amt.ToString(),
                                    status = m.status,
                                    o_paid_cd = xPoli == null ? "" : xPoli.o_paid_cd,
                                    re_paid_date = m.re_paid_date == null ? "" : (SqlFunctions.DatePart("year", m.re_paid_date) - 1911) + "/" +
                                                 SqlFunctions.DatePart("m", m.re_paid_date) + "/" +
                                                 SqlFunctions.DateName("day", m.re_paid_date).Trim(),
                                    level_1 = i.level_1,
                                    level_2 = i.level_2,
                                    clean_status = i.clean_status,
                                    clean_date = telInterview.approve_datetime == null ? "" : (SqlFunctions.DatePart("year", telInterview.approve_datetime)) + "/" +
                                                 SqlFunctions.DatePart("m", telInterview.approve_datetime) + "/" +
                                                 SqlFunctions.DateName("day", telInterview.approve_datetime).Trim()
                                }).Distinct().ToList<OAP0052Model>();


                   

                    return rows;
                }
            }
        }


        public List<OAP0048Model> qryForOAP0048(string paid_id, string policy_no, string policy_seq, string id_dup
           , string paid_name, string check_no, string proc_id)
        {
            bool bPaidId = true;
            bool bPolicyNo = true;
            bool bPolicySeq = true;
            bool bIdDup = true;
            bool bPaidName = true;
            bool bCheckNo = true;
            bool bProcId = true;

            if (!"".Equals(StringUtil.toString(paid_id)))
                bPaidId = false;

            if (!"".Equals(StringUtil.toString(policy_no)))
                bPolicyNo = false;

            int _i_policy_seq = 0;
            if (!"".Equals(StringUtil.toString(policy_seq)))
            {
                bPolicySeq = false;
                _i_policy_seq = Convert.ToInt32(policy_seq);
            }


            if (!"".Equals(StringUtil.toString(id_dup)))
                bIdDup = false;

            if (!"".Equals(StringUtil.toString(paid_name)))
                bPaidName = false;

            if (!"".Equals(StringUtil.toString(check_no)))
                bCheckNo = false;

            if (!"".Equals(StringUtil.toString(proc_id)))
                bProcId = false;

            using (new TransactionScope(TransactionScopeOption.Required, new TransactionOptions
            {
                IsolationLevel = System.Transactions.IsolationLevel.ReadUncommitted
            }))
            {
                using (dbFGLEntities db = new dbFGLEntities())
                {
                    var rows = (from telM in db.FAP_TEL_CHECK
                                join m in db.FAP_VE_TRACE on new { telM.system, telM.check_acct_short, telM.check_no } equals new { m.system, m.check_acct_short, m.check_no }
                                join poli in db.FAP_VE_TRACE_POLI on new { m.check_no, m.check_acct_short } equals new { poli.check_no, poli.check_acct_short } into psPoli
                                from xPoli in psPoli.DefaultIfEmpty()
                                join telInterview in db.FAP_TEL_INTERVIEW on telM.tel_proc_no equals telInterview.tel_proc_no

                                join veCode in db.FAP_TEL_CODE.Where(x => x.code_type == "tel_clean") on telInterview.clean_status equals veCode.code_id

                                where 1 == 1
                                   & telM.tel_std_type == "tel_assign_case"
                                   & telM.data_flag == "Y"
                                   & m.status != "1"
                                   & telInterview.tel_appr_result == "13"
                                   & !new string[] { "11", "12", "13" }.Contains(telInterview.clean_status)
                                   & new string[] { "2" }.Contains(telM.dispatch_status)
                                   & (bPaidId || (!bPaidId && m.paid_id == paid_id))
                                   & (bPolicyNo || (!bPolicyNo && xPoli.policy_no == policy_no))
                                   & (bPolicySeq || (!bPolicySeq && _i_policy_seq.CompareTo(xPoli.policy_seq) == 0))
                                   & (bIdDup || (!bIdDup && xPoli.id_dup == id_dup))
                                   & (bPaidName || (!bPaidName && m.paid_name == paid_name))
                                   & (bCheckNo || (!bCheckNo && xPoli.check_no == check_no))
                                   & (bProcId || (!bProcId && veCode.proc_id == proc_id))

                                select new OAP0048Model
                                {
                                    tel_proc_no = telM.tel_proc_no,
                                    proc_id = veCode.proc_id,
                                    paid_id = m.paid_id,
                                    check_no = m.check_no,
                                    clean_status = telInterview.clean_status,
                                    std_1 = veCode.std_1 == null ? "" : veCode.std_1.ToString(),
                                    clean_date = (DateTime)telInterview.clean_date,
                                    data_status = telInterview.data_status
                                }).Distinct().ToList<OAP0048Model>();

                    return rows;
                }
            }
        }



        public void updDataStatus(FAP_TEL_INTERVIEW d, SqlConnection conn, SqlTransaction transaction)
        {

            try
            {

                string sql = @"
UPDATE FAP_TEL_INTERVIEW
  SET data_status = @data_status
    , update_id = @update_id
    , update_datetime = @update_datetime
WHERE tel_proc_no = @tel_proc_no";

                SqlCommand cmd = conn.CreateCommand();

                cmd.Connection = conn;
                cmd.Transaction = transaction;

                cmd.CommandText = sql;

                cmd.Parameters.Clear();

                cmd.Parameters.AddWithValue("@data_status", StringUtil.toString(d.data_status));
                cmd.Parameters.AddWithValue("@tel_proc_no", StringUtil.toString(d.tel_proc_no));
                cmd.Parameters.AddWithValue("@update_id", StringUtil.toString(d.update_id));
                cmd.Parameters.AddWithValue("@update_datetime", System.Data.SqlDbType.DateTime).Value = (Object)d.update_datetime ?? DBNull.Value;

                cmd.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                throw e;
            }
        }



        public FAP_TEL_INTERVIEW qryByTelProcNo(string tel_proc_no)
        {
            using (dbFGLEntities db = new dbFGLEntities())
            {
                FAP_TEL_INTERVIEW d = db.FAP_TEL_INTERVIEW
                    .Where(x => x.tel_proc_no == tel_proc_no).FirstOrDefault();

                if (d != null)
                    return d;
                else
                    return new FAP_TEL_INTERVIEW();
            }
        }

    


        /// <summary>
        /// 新增
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="d"></param>
        /// <param name="conn"></param>
        /// <param name="transaction"></param>
        public void insert(FAP_TEL_INTERVIEW d, SqlConnection conn, SqlTransaction transaction)
        {

            try
            {

                string sql = @"
        INSERT INTO FAP_TEL_INTERVIEW (
  tel_proc_no
, tel_interview_id
, tel_interview_f_datetime
, tel_interview_datetime
, tel_result
, tel_result_cnt
, tel_appr_result
, tel_appr_datetime
, called_person
, record_no
, tel_addr
, tel_zip_code
, tel_mail
, cust_tel
, cust_counter
, counter_date
, level_1
, level_2
, reason
, remark
, dispatch_status
, clean_status
, clean_date
, clean_f_date
, data_status
, update_id
, update_datetime
)  VALUES (
  @tel_proc_no
, @tel_interview_id
, @tel_interview_f_datetime
, @tel_interview_datetime
, @tel_result
, @tel_result_cnt
, @tel_appr_result
, @tel_appr_datetime
, @called_person
, @record_no
, @tel_addr
, @tel_zip_code
, @tel_mail
, @cust_tel
, @cust_counter
, @counter_date
, @level_1
, @level_2
, @reason
, @remark
, @dispatch_status
, @clean_status
, @clean_date
, @clean_f_date
, @data_status
, @update_id
, @update_datetime)";

                SqlCommand cmd = conn.CreateCommand();

                cmd.Connection = conn;
                cmd.Transaction = transaction;

                cmd.CommandText = sql;

                cmd.Parameters.Clear();

                cmd.Parameters.AddWithValue("@tel_proc_no", StringUtil.toString(d.tel_proc_no));
                cmd.Parameters.AddWithValue("@tel_interview_id", StringUtil.toString(d.tel_interview_id));
                cmd.Parameters.AddWithValue("@tel_interview_f_datetime", System.Data.SqlDbType.DateTime).Value = (Object)d.tel_interview_f_datetime ?? DBNull.Value;
                cmd.Parameters.AddWithValue("@tel_interview_datetime", System.Data.SqlDbType.DateTime).Value = (Object)d.tel_interview_datetime ?? DBNull.Value;
                cmd.Parameters.AddWithValue("@tel_result", StringUtil.toString(d.tel_result));
                cmd.Parameters.AddWithValue("@tel_result_cnt", d.tel_result_cnt);
                cmd.Parameters.AddWithValue("@tel_appr_result", StringUtil.toString(d.tel_appr_result));
                cmd.Parameters.AddWithValue("@tel_appr_datetime", System.Data.SqlDbType.DateTime).Value = (Object)d.tel_appr_datetime ?? DBNull.Value;
                cmd.Parameters.AddWithValue("@called_person", StringUtil.toString(d.called_person));
                cmd.Parameters.AddWithValue("@record_no", StringUtil.toString(d.record_no));
                cmd.Parameters.AddWithValue("@tel_addr", StringUtil.toString(d.tel_addr));
                cmd.Parameters.AddWithValue("@tel_zip_code", StringUtil.toString(d.tel_zip_code));
                cmd.Parameters.AddWithValue("@tel_mail", StringUtil.toString(d.tel_mail));
                cmd.Parameters.AddWithValue("@cust_tel", StringUtil.toString(d.cust_tel));
                cmd.Parameters.AddWithValue("@cust_counter", StringUtil.toString(d.cust_counter));
                cmd.Parameters.AddWithValue("@counter_date", System.Data.SqlDbType.DateTime).Value = (Object)d.counter_date ?? DBNull.Value;
                cmd.Parameters.AddWithValue("@level_1", StringUtil.toString(d.level_1));
                cmd.Parameters.AddWithValue("@level_2", StringUtil.toString(d.level_2));
                cmd.Parameters.AddWithValue("@reason", StringUtil.toString(d.reason));
                cmd.Parameters.AddWithValue("@remark", StringUtil.toString(d.remark));
                cmd.Parameters.AddWithValue("@dispatch_status", StringUtil.toString(d.dispatch_status));
                cmd.Parameters.AddWithValue("@clean_status", StringUtil.toString(d.clean_status));
                cmd.Parameters.AddWithValue("@clean_date", System.Data.SqlDbType.DateTime).Value = (Object)d.clean_date ?? DBNull.Value;
                cmd.Parameters.AddWithValue("@clean_f_date", System.Data.SqlDbType.DateTime).Value = (Object)d.clean_f_date ?? DBNull.Value;
                cmd.Parameters.AddWithValue("@data_status", "1");
                cmd.Parameters.AddWithValue("@update_id", StringUtil.toString(d.update_id));
                cmd.Parameters.AddWithValue("@update_datetime", System.Data.SqlDbType.DateTime).Value = (Object)d.update_datetime ?? DBNull.Value;


                cmd.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                throw e;
            }
        }



        public void updForOAP0048A(FAP_TEL_INTERVIEW d, SqlConnection conn, SqlTransaction transaction)
        {

            try
            {

                string sql = @"
        UPDATE FAP_TEL_INTERVIEW set
  remark = @remark
, clean_status = @clean_status
, clean_date = @clean_date
, clean_f_date = @clean_f_date
, data_status = @data_status
, update_id = @update_id
, update_datetime = @update_datetime
 where tel_proc_no = @tel_proc_no";


                SqlCommand cmd = conn.CreateCommand();

                cmd.Connection = conn;
                cmd.Transaction = transaction;

                cmd.CommandText = sql;

                cmd.Parameters.Clear();

                cmd.Parameters.AddWithValue("@tel_proc_no", StringUtil.toString(d.tel_proc_no));
                cmd.Parameters.AddWithValue("@remark", StringUtil.toString(d.remark));
                cmd.Parameters.AddWithValue("@clean_status", StringUtil.toString(d.clean_status));
                cmd.Parameters.AddWithValue("@clean_date", System.Data.SqlDbType.DateTime).Value = (Object)d.clean_date ?? DBNull.Value;
                cmd.Parameters.AddWithValue("@clean_f_date", System.Data.SqlDbType.DateTime).Value = (Object)d.clean_f_date ?? DBNull.Value;
                cmd.Parameters.AddWithValue("@data_status", "1");
                cmd.Parameters.AddWithValue("@update_id", StringUtil.toString(d.update_id));
                cmd.Parameters.AddWithValue("@update_datetime", System.Data.SqlDbType.DateTime).Value = (Object)d.update_datetime ?? DBNull.Value;


                cmd.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                throw e;
            }
        }


        public void updForOAP0047A(FAP_TEL_INTERVIEW d, SqlConnection conn, SqlTransaction transaction)
        {

            try
            {

                string sql = @"
        UPDATE FAP_TEL_INTERVIEW set
  tel_interview_datetime = @tel_interview_datetime
, record_no = @record_no
, tel_result = @tel_result
, tel_result_cnt = @tel_result_cnt
, tel_appr_result = @tel_appr_result
, tel_appr_datetime = @tel_appr_datetime
, called_person = @called_person
, tel_addr = @tel_addr
, tel_zip_code = @tel_zip_code
, tel_mail = @tel_mail
, cust_tel = @cust_tel
, cust_counter = @cust_counter
, counter_date = @counter_date
, level_1 = @level_1
, level_2 = @level_2
, reason = @reason
, remark = @remark
, dispatch_status = @dispatch_status
, clean_status = @clean_status
, clean_date = @clean_date
, data_status = @data_status
, update_id = @update_id
, update_datetime = @update_datetime
 where tel_proc_no = @tel_proc_no";


                SqlCommand cmd = conn.CreateCommand();

                cmd.Connection = conn;
                cmd.Transaction = transaction;

                cmd.CommandText = sql;

                cmd.Parameters.Clear();

                cmd.Parameters.AddWithValue("@tel_proc_no", StringUtil.toString(d.tel_proc_no));
                cmd.Parameters.AddWithValue("@tel_interview_datetime", System.Data.SqlDbType.DateTime).Value = (Object)d.tel_interview_datetime ?? DBNull.Value;
                cmd.Parameters.AddWithValue("@tel_result", StringUtil.toString(d.tel_result));
                cmd.Parameters.AddWithValue("@record_no", StringUtil.toString(d.record_no));
                cmd.Parameters.AddWithValue("@tel_result_cnt", d.tel_result_cnt);
                cmd.Parameters.AddWithValue("@tel_appr_result", StringUtil.toString(d.tel_appr_result));
                cmd.Parameters.AddWithValue("@tel_appr_datetime", System.Data.SqlDbType.DateTime).Value = (Object)d.tel_appr_datetime ?? DBNull.Value;
                cmd.Parameters.AddWithValue("@called_person", StringUtil.toString(d.called_person));
                cmd.Parameters.AddWithValue("@tel_addr", StringUtil.toString(d.tel_addr));
                cmd.Parameters.AddWithValue("@tel_zip_code", StringUtil.toString(d.tel_zip_code));
                cmd.Parameters.AddWithValue("@tel_mail", StringUtil.toString(d.tel_mail));
                cmd.Parameters.AddWithValue("@cust_tel", StringUtil.toString(d.cust_tel));
                cmd.Parameters.AddWithValue("@cust_counter", StringUtil.toString(d.cust_counter));
                cmd.Parameters.AddWithValue("@counter_date", System.Data.SqlDbType.DateTime).Value = (Object)d.counter_date ?? DBNull.Value;
                cmd.Parameters.AddWithValue("@level_1", StringUtil.toString(d.level_1));
                cmd.Parameters.AddWithValue("@level_2", StringUtil.toString(d.level_2));
                cmd.Parameters.AddWithValue("@reason", StringUtil.toString(d.reason));
                cmd.Parameters.AddWithValue("@remark", StringUtil.toString(d.remark));
                cmd.Parameters.AddWithValue("@dispatch_status", StringUtil.toString(d.dispatch_status));
                cmd.Parameters.AddWithValue("@clean_status", StringUtil.toString(d.clean_status));
                cmd.Parameters.AddWithValue("@clean_date", System.Data.SqlDbType.DateTime).Value = (Object)d.clean_date ?? DBNull.Value;
                cmd.Parameters.AddWithValue("@data_status", "1");
                cmd.Parameters.AddWithValue("@update_id", StringUtil.toString(d.update_id));
                cmd.Parameters.AddWithValue("@update_datetime", System.Data.SqlDbType.DateTime).Value = (Object)d.update_datetime ?? DBNull.Value;


                cmd.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                throw e;
            }
        }


    }
}