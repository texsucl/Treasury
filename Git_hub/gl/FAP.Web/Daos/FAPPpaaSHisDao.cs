
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using FAP.Web.Models;
using System.Data.Entity.SqlServer;
using System.Transactions;
using FAP.Web.BO;
using FAP.Web.ViewModels;
using FAP.Web.AS400Models;

namespace FAP.Web.Daos
{
    public class FAPPpaaSHisDao
    {
        public List<OAP0002Model> qryCheckByStat(string apprStat, string checkShrt, string checkNo, string aplyNo)
        {
            bool bCheckNo = StringUtil.isEmpty(checkNo);
            bool bCheckShrt = StringUtil.isEmpty(checkShrt);
            bool bAplyNo = StringUtil.isEmpty(aplyNo);

            using (new TransactionScope(
              TransactionScopeOption.Required,
              new TransactionOptions
              {
                  IsolationLevel = IsolationLevel.ReadUncommitted
              }))
            {
                using (dbFGLEntities db = new dbFGLEntities())
                {
                    var his = (from aply in db.FAP_APLY_REC
                               join m in db.FAP_PPAA_S_HIS on aply.aply_no equals m.aply_no 

                               join cSts in db.SYS_CODE.Where(x => x.SYS_CD == "AP" & x.CODE_TYPE == "APPR_STAT") on aply.appr_stat equals cSts.CODE into psCSts
                               from xCSts in psCSts.DefaultIfEmpty()

                               join cAction in db.SYS_CODE.Where(x => x.SYS_CD == "AP" & x.CODE_TYPE == "EXEC_ACTION") on m.exec_action equals cAction.CODE into psAction
                               from xActions in psAction.DefaultIfEmpty()

                               where 1 == 1
                               & aply.aply_type == "A"
                               & aply.appr_stat == apprStat
                               & (bAplyNo || (!bAplyNo & m.aply_no == aplyNo))
                               & (bCheckNo || (!bCheckNo & m.check_no == checkNo))
                               & (bCheckShrt || (!bCheckShrt & m.check_acct_short == checkShrt))

                               select new OAP0002Model
                               {
                                   aplyNo = aply.aply_no,
                                   apprStat = aply.appr_stat,
                                   exec_action = m.exec_action,
                                   execActionDesc = xActions == null ? "" : xActions.CODE_VALUE.Trim(),
                                   check_no = m.check_no,
                                   check_acct_short = m.check_acct_short,
                                   system = m.system,
                                   check_date = m.check_date == null ? "" : SqlFunctions.DatePart("year", m.check_date) + "/" +
                                                 SqlFunctions.DatePart("m", m.check_date).ToString() + "/" +
                                                 SqlFunctions.DateName("day", m.check_date).ToString(),
                                   check_amt = m.check_amt.ToString(),
                                   status = m.status,
                                   filler_10 = m.filler_10,
                                   filler_14 = m.filler_14,
                                   paid_id = m.paid_id,
                                   paid_name = m.paid_name,
                                   re_paid_type = m.re_paid_type,
                                   area = m.area,
                                   srce_from = m.srce_from,
                                   source_kind = m.source_kind,
                                   pay_no = m.pay_no,
                                   pay_seq = m.pay_seq.ToString(),
                                   re_paid_no = m.re_paid_no,
                                   re_paid_seq = m.re_paid_seq.ToString(),
                                   re_paid_check_no = m.re_paid_check_no,
                                   rt_system = m.rt_system,
                                   rt_policy_no = m.rt_policy_no,
                                   rt_policy_seq = m.rt_policy_seq.ToString(),
                                   rt_id_dup = m.rt_id_dup,
                                   re_bank_code = m.re_bank_code,
                                   re_sub_bank = m.re_sub_bank,
                                   re_bank_account = m.re_bank_account,
                                   re_paid_id = m.re_paid_id,
                                   re_paid_date = m.re_paid_date == null ? "" : SqlFunctions.DatePart("year", m.re_paid_date) + "/" + 
                                                 SqlFunctions.DatePart("m", m.re_paid_date).ToString() + "/" + 
                                                 SqlFunctions.DateName("day", m.re_paid_date).ToString(),
                                   re_paid_date_n = m.re_paid_date_n == null ? "" : SqlFunctions.DatePart("year", m.re_paid_date_n) + "/" +
                                                 SqlFunctions.DatePart("m", m.re_paid_date_n).ToString() + "/" +
                                                 SqlFunctions.DateName("day", m.re_paid_date_n).ToString(),
                                   o_paid_cd = m.o_paid_cd,
                                   create_id = aply.create_id,
                                   create_dt = aply.create_dt == null ? "" : SqlFunctions.DateName("year", aply.create_dt) + "/" +
                                                 SqlFunctions.DatePart("m", aply.create_dt) + "/" +
                                                 SqlFunctions.DateName("day", aply.create_dt).Trim() + " " +
                                                 SqlFunctions.DateName("hh", aply.create_dt).Trim() + ":" +
                                                 SqlFunctions.DateName("n", aply.create_dt).Trim() + ":" +
                                                 SqlFunctions.DateName("s", aply.create_dt).Trim()

                               }).Distinct().ToList<OAP0002Model>();

                    return his;
                }
            }

        }




        /// <summary>
        /// 新增覆核資料
        /// </summary>
        /// <param name="d"></param>
        /// <param name="conn"></param>
        /// <param name="transaction"></param>
        public void insert(FAP_PPAA_S_HIS d, SqlConnection conn, SqlTransaction transaction)
        {
            try
            {

                string sql = @"
        INSERT INTO [FAP_PPAA_S_HIS]
           (aply_no
           ,exec_action
           ,system
           ,source_op
           ,paid_id
           ,paid_name
           ,check_amt
           ,status
           ,check_no
           ,check_acct_short
           ,check_date
           ,area
           ,srce_from
           ,source_kind
           ,pay_no
           ,pay_seq
           ,re_paid_no
           ,re_paid_seq
           ,re_paid_date
           ,re_paid_date_n
           ,re_paid_type
           ,re_paid_id
           ,re_paid_check_no
           ,re_bank_code
           ,re_sub_bank
           ,re_bank_account
           ,rt_system
           ,rt_policy_no
           ,rt_policy_seq
           ,rt_id_dup
           ,o_paid_cd
           ,filler_10
           ,filler_14)
             VALUES
           (@aply_no
           ,@exec_action
           ,@system
           ,@source_op
           ,@paid_id
           ,@paid_name
           ,@check_amt
           ,@status
           ,@check_no
           ,@check_acct_short
           ,@check_date
           ,@area
           ,@srce_from
           ,@source_kind
           ,@pay_no
           ,@pay_seq
           ,@re_paid_no 
           ,@re_paid_seq
           ,@re_paid_date
           ,@re_paid_date_n
           ,@re_paid_type
           ,@re_paid_id
           ,@re_paid_check_no
           ,@re_bank_code
           ,@re_sub_bank
           ,@re_bank_account
           ,@rt_system
           ,@rt_policy_no
           ,@rt_policy_seq
           ,@rt_id_dup
           ,@o_paid_cd
           ,@filler_10
           ,@filler_14)";

                SqlCommand cmd = conn.CreateCommand();

                cmd.Connection = conn;
                cmd.Transaction = transaction;

                cmd.CommandText = sql;

                cmd.Parameters.AddWithValue("@aply_no", StringUtil.toString(d.aply_no));
                cmd.Parameters.AddWithValue("@exec_action", StringUtil.toString(d.exec_action));
                cmd.Parameters.AddWithValue("@system", StringUtil.toString(d.system));
                cmd.Parameters.AddWithValue("@source_op", StringUtil.toString(d.source_op));
                cmd.Parameters.AddWithValue("@paid_id", StringUtil.toString(d.paid_id));
                cmd.Parameters.AddWithValue("@paid_name", StringUtil.toString(d.paid_name));
                cmd.Parameters.AddWithValue("@check_amt", System.Data.SqlDbType.Decimal).Value = (Object)d.check_amt ?? DBNull.Value;
                cmd.Parameters.AddWithValue("@status", StringUtil.toString(d.status));
                cmd.Parameters.AddWithValue("@check_no", StringUtil.toString(d.check_no));
                cmd.Parameters.AddWithValue("@check_acct_short", StringUtil.toString(d.check_acct_short));
                cmd.Parameters.AddWithValue("@check_date", System.Data.SqlDbType.Date).Value = (Object)Convert.ToDateTime(d.check_date) ?? DBNull.Value;
                cmd.Parameters.AddWithValue("@area", StringUtil.toString(d.area));
                cmd.Parameters.AddWithValue("@srce_from", StringUtil.toString(d.srce_from));
                cmd.Parameters.AddWithValue("@source_kind", StringUtil.toString(d.source_kind));
                cmd.Parameters.AddWithValue("@pay_no", StringUtil.toString(d.pay_no));
                cmd.Parameters.AddWithValue("@pay_seq", System.Data.SqlDbType.Int).Value = (Object)d.pay_seq ?? DBNull.Value;
                cmd.Parameters.AddWithValue("@re_paid_no", StringUtil.toString(d.re_paid_no));
                cmd.Parameters.AddWithValue("@re_paid_seq", System.Data.SqlDbType.Int).Value = (Object)d.re_paid_seq ?? DBNull.Value;

                //modify by daiyu 20201028
                if(d.re_paid_date == null)
                    cmd.Parameters.AddWithValue("@re_paid_date", System.Data.SqlDbType.Date).Value = DBNull.Value;
                else
                    cmd.Parameters.AddWithValue("@re_paid_date", System.Data.SqlDbType.Date).Value = (Object)Convert.ToDateTime(d.re_paid_date) ?? DBNull.Value;

                if (d.re_paid_date_n == null)
                    cmd.Parameters.AddWithValue("@re_paid_date_n", System.Data.SqlDbType.Date).Value = DBNull.Value;
                else
                    cmd.Parameters.AddWithValue("@re_paid_date_n", System.Data.SqlDbType.Date).Value = (Object)Convert.ToDateTime(d.re_paid_date_n) ?? DBNull.Value;
                //end modify 20201028

                cmd.Parameters.AddWithValue("@re_paid_type", StringUtil.toString(d.re_paid_type));
                cmd.Parameters.AddWithValue("@re_paid_id", StringUtil.toString(d.re_paid_id));
                cmd.Parameters.AddWithValue("@re_paid_check_no", StringUtil.toString(d.re_paid_check_no));
                cmd.Parameters.AddWithValue("@re_bank_code", StringUtil.toString(d.re_bank_code));
                cmd.Parameters.AddWithValue("@re_sub_bank", StringUtil.toString(d.re_sub_bank));
                cmd.Parameters.AddWithValue("@re_bank_account", StringUtil.toString(d.re_bank_account));
                cmd.Parameters.AddWithValue("@rt_system", StringUtil.toString(d.rt_system));
                cmd.Parameters.AddWithValue("@rt_policy_no", StringUtil.toString(d.rt_policy_no));
                cmd.Parameters.AddWithValue("@rt_policy_seq", System.Data.SqlDbType.Int).Value = (Object)d.rt_policy_seq ?? DBNull.Value;
                cmd.Parameters.AddWithValue("@rt_id_dup", StringUtil.toString(d.rt_id_dup));
                cmd.Parameters.AddWithValue("@o_paid_cd", StringUtil.toString(d.o_paid_cd));
                cmd.Parameters.AddWithValue("@filler_10", StringUtil.toString(d.filler_10));
                cmd.Parameters.AddWithValue("@filler_14", StringUtil.toString(d.filler_14));

                int cnt = cmd.ExecuteNonQuery();

            }
            catch (Exception e)
            {
                throw e;
            }
        }
    }
}