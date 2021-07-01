
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
/// 功能說明：FAP_TEL_INTERVIEW_HIS 電訪及追踨記錄暫存檔
/// 初版作者：20200908 Daiyu
/// 修改歷程：20200908 Daiyu
/// 需求單號：
/// 修改內容：初版
/// --------------------------------------------------
/// 修改歷程：20210205 daiyu 
/// 需求單號：202101280283-00
/// 修改內容：【OAP0046A 電訪處理結果登錄作業】、【OAP0047A 追蹤處理結果覆核作業】
///           1.畫面增加篩選條件：申請人、電訪編號、給付對象ID，可擇一輸入，若沒選，就SHOW全部。
///           2.畫面呈現改預設SHOW 20筆資料。
/// --------------------------------------------------
/// </summary>
/// 

namespace FAP.Web.Daos
{
    public class FAPTelInterviewHisDao
    {

        public void updVeCleanFinish(string userId, string appr_stat, string tel_proc_no, DateTime now, SqlConnection conn, SqlTransaction transaction)
        {

            try
            {
                string sql = @"UPDATE FAP_TEL_INTERVIEW_HIS
 SET appr_stat = @appr_stat
   , appr_id = @appr_id
   , approve_datetime = @approve_datetime
 WHERE tel_proc_no = @tel_proc_no
  AND appr_stat = '1'
 ";

                SqlCommand cmd = conn.CreateCommand();

                cmd.Connection = conn;
                cmd.Transaction = transaction;

                cmd.CommandText = sql;

                cmd.Parameters.Clear();

                cmd.Parameters.AddWithValue("@tel_proc_no", tel_proc_no);
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


        public void updateApprStatus(string userId, string appr_stat, string aply_no, string tel_proc_no, string data_type,
           DateTime now, SqlConnection conn, SqlTransaction transaction)
        {

            try
            {
                string sql = @"UPDATE [FAP_TEL_INTERVIEW_HIS]
SET appr_stat = @appr_stat
  , appr_id = @appr_id
  , approve_datetime = @approve_datetime
WHERE aply_no = @aply_no
  AND tel_proc_no = @tel_proc_no
  AND data_type = @data_type
          ";

                SqlCommand cmd = conn.CreateCommand();

                cmd.Connection = conn;
                cmd.Transaction = transaction;

                cmd.CommandText = sql;

                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("@aply_no", aply_no);
                cmd.Parameters.AddWithValue("@tel_proc_no", tel_proc_no);
                cmd.Parameters.AddWithValue("@data_type", data_type);
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

        public void updateApprResult(string userId, string appr_stat, OAP0046Model model, string data_type,
            DateTime now, SqlConnection conn, SqlTransaction transaction)
        {

            try
            {
                string sql = "";

                if("3".Equals(appr_stat))
                sql = @"UPDATE [FAP_TEL_INTERVIEW_HIS]
SET appr_stat = @appr_stat
  , appr_id = @appr_id
  , approve_datetime = @approve_datetime
WHERE aply_no = @aply_no
  AND tel_proc_no = @tel_proc_no
  AND data_type = @data_type
          "; 

                if("2".Equals(appr_stat))
                    sql = @"UPDATE [FAP_TEL_INTERVIEW_HIS]
SET appr_stat = @appr_stat
  , tel_appr_result = @tel_appr_result
  , appr_id = @appr_id
  , approve_datetime = @approve_datetime
  , tel_result = @tel_result
  , called_person = @called_person
  , tel_zip_code = @tel_zip_code
  , cust_tel = @cust_tel
  , tel_mail = @tel_mail
  , tel_addr = @tel_addr
  , cust_counter = @cust_counter
  , counter_date = @counter_date
  , reason = @reason
WHERE aply_no = @aply_no
  AND tel_proc_no = @tel_proc_no
  AND data_type = @data_type
          ";

                SqlCommand cmd = conn.CreateCommand();

                cmd.Connection = conn;
                cmd.Transaction = transaction;

                cmd.CommandText = sql;

                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("@aply_no", model.aply_no);
                cmd.Parameters.AddWithValue("@tel_proc_no", model.tel_proc_no);
                cmd.Parameters.AddWithValue("@data_type", data_type);
                cmd.Parameters.AddWithValue("@appr_stat", appr_stat);
                cmd.Parameters.AddWithValue("@appr_id", userId);
                cmd.Parameters.AddWithValue("@approve_datetime", now);

                if ("2".Equals(appr_stat)) {
                    cmd.Parameters.AddWithValue("@tel_appr_result", StringUtil.toString(model.tel_appr_result));
                    cmd.Parameters.AddWithValue("@tel_result", StringUtil.toString(model.tel_result));
                    cmd.Parameters.AddWithValue("@called_person", StringUtil.toString(model.called_person));
                    cmd.Parameters.AddWithValue("@tel_zip_code", StringUtil.toString(model.tel_zip_code));
                    cmd.Parameters.AddWithValue("@cust_tel", StringUtil.toString(model.cust_tel));
                    cmd.Parameters.AddWithValue("@tel_mail", StringUtil.toString(model.tel_mail));
                    cmd.Parameters.AddWithValue("@tel_addr", StringUtil.toString(model.tel_addr));
                    cmd.Parameters.AddWithValue("@cust_counter", StringUtil.toString(model.cust_counter));
                    cmd.Parameters.AddWithValue("@counter_date", System.Data.SqlDbType.DateTime).Value = (Object)model.counter_date ?? DBNull.Value;
                    cmd.Parameters.AddWithValue("@reason", StringUtil.toString(model.reason));

                }


                cmd.ExecuteNonQuery();



            }
            catch (Exception e)
            {
                throw e;
            }
        }


        public List<OAP0048Model> qryForOAP0048A(string data_type, string appr_stat, string aply_no)
        {
            bool bApprStat = true;
            if (!"".Equals(appr_stat))
                bApprStat = false;

            bool bAplyNo = true;
            if (!"".Equals(aply_no))
                bAplyNo = false;

            using (new TransactionScope(TransactionScopeOption.Required, new TransactionOptions
            {
                IsolationLevel = System.Transactions.IsolationLevel.ReadUncommitted
            }))
            {
                using (dbFGLEntities db = new dbFGLEntities())
                {
                    var rows = (from telInterview in db.FAP_TEL_INTERVIEW_HIS
                                join telM in db.FAP_TEL_CHECK.Where(x => x.tel_std_type == "tel_assign_case") on telInterview.tel_proc_no equals telM.tel_proc_no
                                join m in db.FAP_VE_TRACE on new { telM.system, telM.check_acct_short, telM.check_no } equals new { m.system, m.check_acct_short, m.check_no }
                                join veCode in db.FAP_TEL_CODE.Where(x => x.code_type == "tel_clean") on telInterview.clean_status equals veCode.code_id

                                where 1 == 1
                                   & (bApprStat || (!bApprStat & telInterview.appr_stat == appr_stat))
                                   & telInterview.data_type == data_type
                                   & (bAplyNo || (!bAplyNo & telInterview.aply_no == aply_no))
                                   & m.status != "1"
                                select new OAP0048Model
                                {
                                    aply_no = telInterview.aply_no,
                                    tel_proc_no = telInterview.tel_proc_no,
                                    check_no = m.check_no,
                                    update_id = telInterview.update_id,
                                    update_datetime = telInterview.update_datetime == null ? "" : SqlFunctions.DateName("year", telInterview.update_datetime) + "/" +
                                                 SqlFunctions.DatePart("m", telInterview.update_datetime) + "/" +
                                                 SqlFunctions.DateName("day", telInterview.update_datetime).Trim() + " " +
                                                 SqlFunctions.DateName("hh", telInterview.update_datetime).Trim() + ":" +
                                                 SqlFunctions.DateName("n", telInterview.update_datetime).Trim() + ":" +
                                                 SqlFunctions.DateName("s", telInterview.update_datetime).Trim(),
                                    paid_id = m.paid_id,
                                    paid_name = m.paid_name,
                                    std_1 = veCode.std_1 == null ? "" : veCode.std_1.ToString(),
                                    clean_status = telInterview.clean_status,
                                    clean_date = (DateTime)telInterview.clean_date,
                                    clean_f_date = telInterview.clean_f_date == null ? "" : SqlFunctions.DateName("year", telInterview.clean_f_date) + "/" +
                                                 SqlFunctions.DatePart("m", telInterview.clean_f_date) + "/" +
                                                 SqlFunctions.DateName("day", telInterview.clean_f_date).Trim(),
                                    remark = telInterview.remark
                                }).Distinct().ToList<OAP0048Model>();

                    return rows;

                }
            }

        }



        public List<OAP0046Model> qryForOAP0047A(string data_type, string appr_stat, string aply_no, string update_id, string tel_proc_no, string paid_id)
        {
            bool bApprStat = true;
            if (!"".Equals(appr_stat))
                bApprStat = false;

            bool bAplyNo = true;
            if (!"".Equals(aply_no))
                bAplyNo = false;

            bool bUpdateId = true;
            if (!"".Equals(update_id))
                bUpdateId = false;

            bool bTelProcNo = true;
            if (!"".Equals(tel_proc_no))
                bTelProcNo = false;

            bool bPaidId = true;
            if (!"".Equals(paid_id))
                bPaidId = false;

            using (new TransactionScope(TransactionScopeOption.Required, new TransactionOptions
            {
                IsolationLevel = System.Transactions.IsolationLevel.ReadUncommitted
            }))
            {
                using (dbFGLEntities db = new dbFGLEntities())
                {
                    var rows = (from telInterview in db.FAP_TEL_INTERVIEW_HIS
                                join telM in db.FAP_TEL_CHECK.Where(x => x.tel_std_type == "tel_assign_case") on telInterview.tel_proc_no equals telM.tel_proc_no
                                join m in db.FAP_VE_TRACE on new { telM.system, telM.check_acct_short, telM.check_no } equals new { m.system, m.check_acct_short, m.check_no }

                                where 1 == 1
                                   & (bApprStat || (!bApprStat & telInterview.appr_stat == appr_stat))
                                   & telInterview.data_type == data_type
                                   & (bAplyNo || (!bAplyNo & telInterview.aply_no == aply_no))

                                   & (bUpdateId || (!bUpdateId & telInterview.update_id == update_id))
                                   & (bTelProcNo || (!bTelProcNo & telInterview.tel_proc_no == tel_proc_no))
                                   & (bPaidId || (!bPaidId & m.paid_id == paid_id))

                                   & m.status != "1"
                                select new OAP0046Model
                                {
                                    aply_no = telInterview.aply_no,
                                    tel_proc_no = telInterview.tel_proc_no,
                                    tel_interview_id = telM.tel_interview_id,
                                    update_id = telInterview.update_id,
                                    update_datetime = telInterview.update_datetime == null ? "" : SqlFunctions.DateName("year", telInterview.update_datetime) + "/" +
                                                 SqlFunctions.DatePart("m", telInterview.update_datetime) + "/" +
                                                 SqlFunctions.DateName("day", telInterview.update_datetime).Trim() + " " +
                                                 SqlFunctions.DateName("hh", telInterview.update_datetime).Trim() + ":" +
                                                 SqlFunctions.DateName("n", telInterview.update_datetime).Trim() + ":" +
                                                 SqlFunctions.DateName("s", telInterview.update_datetime).Trim(),
                                    paid_id = m.paid_id,
                                    paid_name = m.paid_name,
                                    tel_interview_f_datetime = telInterview.tel_interview_f_datetime.ToString(),
                                    tel_interview_datetime = telInterview.tel_interview_datetime.ToString(),
                                    tel_result = telInterview.tel_result,
                                    record_no = telInterview.record_no,
                                    tel_appr_result = telInterview.tel_appr_result,
                                    called_person = telInterview.called_person,
                                    tel_zip_code = telInterview.tel_zip_code,
                                    tel_addr = telInterview.tel_addr,
                                    cust_tel = telInterview.cust_tel,
                                    tel_mail = telInterview.tel_mail,
                                    level_1 = telInterview.level_1,
                                    level_2 = telInterview.level_2,
                                    cust_counter = telInterview.cust_counter,
                                    counter_date = telInterview.counter_date.ToString(),
                                    reason = telInterview.reason
                                    

                                }).Distinct().ToList<OAP0046Model>();

                    return rows;

                }
            }

        }

        public List<OAP0046Model> qryForOAP0046A(string data_type, string appr_stat, string aply_no, string update_id, string tel_proc_no, string paid_id)
        {
            bool bApprStat = true;
            if (!"".Equals(appr_stat))
                bApprStat = false;

            bool bAplyNo = true;
            if (!"".Equals(aply_no))
                bAplyNo = false;

            bool bUpdateId = true;
            if (!"".Equals(update_id))
                bUpdateId = false;

            bool bTelProcNo = true;
            if (!"".Equals(tel_proc_no))
                bTelProcNo = false;

            bool bPaidId = true;
            if (!"".Equals(paid_id))
                bPaidId = false;

            using (new TransactionScope(TransactionScopeOption.Required, new TransactionOptions
            {
                IsolationLevel = System.Transactions.IsolationLevel.ReadUncommitted
            }))
            {
                using (dbFGLEntities db = new dbFGLEntities())
                {
                    var rows = (from telInterview in db.FAP_TEL_INTERVIEW_HIS
                                join telM in db.FAP_TEL_CHECK_HIS.Where(x => x.tel_std_type == "tel_assign_case") on telInterview.aply_no equals telM.aply_no
                                join m in db.FAP_VE_TRACE on new { telM.system, telM.check_acct_short, telM.check_no } equals new { m.system, m.check_acct_short, m.check_no }
                                
                                where 1 == 1
                                   & (bApprStat || (!bApprStat & telInterview.appr_stat == appr_stat))
                                   & telInterview.data_type == data_type
                                   & (bAplyNo || (!bAplyNo & telInterview.aply_no == aply_no))

                                   & (bUpdateId || (!bUpdateId & telInterview.update_id == update_id))
                                   & (bTelProcNo || (!bTelProcNo & telInterview.tel_proc_no == tel_proc_no))
                                   & (bPaidId || (!bPaidId & m.paid_id == paid_id))

                                   & m.status != "1"
                                select new OAP0046Model
                                {
                                    aply_no = telInterview.aply_no,
                                    tel_proc_no = telInterview.tel_proc_no,
                                    tel_interview_id = telM.tel_interview_id,
                                    update_id = telInterview.update_id,
                                    update_datetime = telInterview.update_datetime == null ? "" : SqlFunctions.DateName("year", telInterview.update_datetime) + "/" +
                                                 SqlFunctions.DatePart("m", telInterview.update_datetime) + "/" +
                                                 SqlFunctions.DateName("day", telInterview.update_datetime).Trim() + " " +
                                                 SqlFunctions.DateName("hh", telInterview.update_datetime).Trim() + ":" +
                                                 SqlFunctions.DateName("n", telInterview.update_datetime).Trim() + ":" +
                                                 SqlFunctions.DateName("s", telInterview.update_datetime).Trim(),
                                    paid_id = m.paid_id,
                                    paid_name = m.paid_name,
                                    tel_interview_datetime = telInterview.tel_interview_f_datetime.ToString(),
                                    tel_result = telInterview.tel_result,
                                    record_no = telInterview.record_no,
                                    tel_appr_result = telInterview.tel_appr_result,
                                    called_person = telInterview.called_person,
                                    tel_zip_code = telInterview.tel_zip_code,
                                    tel_addr = telInterview.tel_addr,
                                    cust_tel = telInterview.cust_tel,
                                    tel_mail = telInterview.tel_mail,
                                    level_1 = telInterview.level_1,
                                    level_2 = telInterview.level_2,
                                    cust_counter = telInterview.cust_counter,
                                    counter_date = telInterview.counter_date.ToString(),
                                    reason = telInterview.reason

                                }).Distinct().ToList<OAP0046Model>();

                    return rows;

                }
            }

        }

        public List<FAP_TEL_INTERVIEW_HIS> qryByDataType(string data_type, string appr_stat)
        {
            bool bDataType = true;
            bool bApprStat = true;

            if (!"".Equals(StringUtil.toString(data_type)))
                bDataType = false;

            if (!"".Equals(StringUtil.toString(appr_stat)))
                bApprStat = false;

            using (dbFGLEntities db = new dbFGLEntities())
            {
                List<FAP_TEL_INTERVIEW_HIS> rows = db.FAP_TEL_INTERVIEW_HIS
                    .Where(x => 1 == 1
                    & (bDataType || (!bDataType & x.data_type == data_type))
                    & (bApprStat || (!bApprStat & x.appr_stat == appr_stat))
                    ).ToList();

                    return rows;
            }
        }

        public List<FAP_TEL_INTERVIEW_HIS> qryByTelProcNo(string tel_proc_no, string[] data_type, string[] appr_stat)
        {
            bool bDataType = true;
            bool bApprStat = true;

            if (data_type.Length > 0)
                bDataType = false;

            if (appr_stat.Length > 0)
                bApprStat = false;

            using (dbFGLEntities db = new dbFGLEntities())
            {
                List<FAP_TEL_INTERVIEW_HIS> rows = db.FAP_TEL_INTERVIEW_HIS
                    .Where(x => x.tel_proc_no == tel_proc_no
                    & (bDataType || (!bDataType & data_type.Contains(x.data_type)))
                    & (bApprStat || (!bApprStat & appr_stat.Contains(x.appr_stat)))
                    ).ToList();

                    return rows;
            }
        }


        public FAP_TEL_INTERVIEW_HIS qryByAplyNo(string aply_no)
        {
            using (dbFGLEntities db = new dbFGLEntities())
            {
                FAP_TEL_INTERVIEW_HIS d = db.FAP_TEL_INTERVIEW_HIS
                    .Where(x => x.aply_no == aply_no
                    ).FirstOrDefault();

                if (d != null)
                    return d;
                else
                    return new FAP_TEL_INTERVIEW_HIS();
            }
        }


        public FAP_TEL_INTERVIEW_HIS qryByTelProcNo(string tel_proc_no, string data_type, string appr_stat)
        {
            bool bDataType = true;
            bool bApprStat = true;

            if (!"".Equals(StringUtil.toString(data_type)))
                bDataType = false;

            if (!"".Equals(StringUtil.toString(appr_stat)))
                bApprStat = false;

            using (dbFGLEntities db = new dbFGLEntities())
            {
                FAP_TEL_INTERVIEW_HIS d = db.FAP_TEL_INTERVIEW_HIS
                    .Where(x => x.tel_proc_no == tel_proc_no
                    & (bDataType || (!bDataType & x.data_type == data_type))
                    & (bApprStat || (!bApprStat & x.appr_stat == appr_stat))
                    ).FirstOrDefault();

                if (d != null)
                    return d;
                else
                    return new FAP_TEL_INTERVIEW_HIS();
            }
        }


        /// <summary>
        /// 新增
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="d"></param>
        /// <param name="conn"></param>
        /// <param name="transaction"></param>
        public void insert(FAP_TEL_INTERVIEW_HIS d, SqlConnection conn, SqlTransaction transaction)
        {

            try
            {

                string sql = @"
        INSERT INTO FAP_TEL_INTERVIEW_HIS (
  aply_no
, tel_proc_no
, data_type
, tel_interview_id
, tel_interview_f_datetime
, tel_interview_datetime
, tel_result
, tel_result_cnt
, tel_appr_result
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
, dispatch_date
, dispatch_status
, clean_status
, clean_date
, clean_f_date
, exec_action
, appr_stat
, update_id
, update_datetime
)  VALUES (
  @aply_no
, @tel_proc_no
, @data_type
, @tel_interview_id
, @tel_interview_f_datetime
, @tel_interview_datetime
, @tel_result
, @tel_result_cnt
, @tel_appr_result
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
, @dispatch_date
, @dispatch_status
, @clean_status
, @clean_date
, @clean_f_date
, @exec_action
, @appr_stat
, @update_id
, @update_datetime)";

                SqlCommand cmd = conn.CreateCommand();

                cmd.Connection = conn;
                cmd.Transaction = transaction;

                cmd.CommandText = sql;

                cmd.Parameters.Clear();

                cmd.Parameters.AddWithValue("@aply_no", StringUtil.toString(d.aply_no));
                cmd.Parameters.AddWithValue("@tel_proc_no", StringUtil.toString(d.tel_proc_no));
                cmd.Parameters.AddWithValue("@data_type", StringUtil.toString(d.data_type));
                cmd.Parameters.AddWithValue("@tel_interview_id", StringUtil.toString(d.tel_interview_id));
                cmd.Parameters.AddWithValue("@tel_interview_f_datetime", System.Data.SqlDbType.DateTime).Value = (Object)d.tel_interview_f_datetime ?? DBNull.Value;
                cmd.Parameters.AddWithValue("@tel_interview_datetime", System.Data.SqlDbType.DateTime).Value = (Object)d.tel_interview_datetime ?? DBNull.Value;
                cmd.Parameters.AddWithValue("@tel_result", StringUtil.toString(d.tel_result));
                cmd.Parameters.AddWithValue("@tel_result_cnt", d.tel_result_cnt);
                cmd.Parameters.AddWithValue("@tel_appr_result", StringUtil.toString(d.tel_appr_result));
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
                cmd.Parameters.AddWithValue("@dispatch_date", System.Data.SqlDbType.DateTime).Value = (Object)d.dispatch_date ?? DBNull.Value;
                cmd.Parameters.AddWithValue("@dispatch_status", StringUtil.toString(d.dispatch_status));
                cmd.Parameters.AddWithValue("@clean_status", StringUtil.toString(d.clean_status));
                cmd.Parameters.AddWithValue("@clean_date", System.Data.SqlDbType.DateTime).Value = (Object)d.clean_date ?? DBNull.Value;
                cmd.Parameters.AddWithValue("@clean_f_date", System.Data.SqlDbType.DateTime).Value = (Object)d.clean_f_date ?? DBNull.Value;
                cmd.Parameters.AddWithValue("@exec_action", "A");
                cmd.Parameters.AddWithValue("@appr_stat", "1");
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