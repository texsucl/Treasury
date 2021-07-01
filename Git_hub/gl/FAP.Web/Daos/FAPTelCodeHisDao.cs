
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
/// 功能說明：FAP_TEL_CODE_HIS 電訪標準設定暫存檔
/// 初版作者：20200827 Daiyu
/// 修改歷程：20200827 Daiyu
///           需求單號：202008120153-00
///           初版
/// --------------------------------------------------
///          20200729
///          需求單號：
/// </summary>
/// 

namespace FAP.Web.Daos
{
    public class FAPTelCodeHisDao
    {
        public void insert(string aply_no, DateTime dt, List<FAP_TEL_CODE_HIS> dataList, SqlConnection conn, SqlTransaction transaction)
        {

            try
            {

                string sql = @"
        INSERT INTO FAP_TEL_CODE_HIS
           (aply_no
           ,code_type
           ,code_id
           ,proc_id
           ,std_1
           ,std_2
           ,std_3
           ,fsc_range
           ,amt_range
           ,remark
           ,exec_action
           ,appr_stat
           ,update_id
           ,update_datetime)
             VALUES
           (@aply_no
           ,@code_type
           ,@code_id
           ,@proc_id
           ,@std_1
           ,@std_2
           ,@std_3
           ,@fsc_range
           ,@amt_range
           ,@remark
           ,@exec_action
           ,@appr_stat
           ,@update_id
           ,@update_datetime)";

                SqlCommand cmd = conn.CreateCommand();

                cmd.Connection = conn;
                cmd.Transaction = transaction;

                cmd.CommandText = sql;


                foreach (FAP_TEL_CODE_HIS d in dataList)
                {
                    cmd.Parameters.Clear();

                    cmd.Parameters.AddWithValue("@aply_no", StringUtil.toString(aply_no));
                    cmd.Parameters.AddWithValue("@code_type", StringUtil.toString(d.code_type));
                    cmd.Parameters.AddWithValue("@code_id", StringUtil.toString(d.code_id));
                    cmd.Parameters.AddWithValue("@proc_id", StringUtil.toString(d.proc_id));
                    cmd.Parameters.AddWithValue("@std_1", System.Data.SqlDbType.Int).Value = (Object)d.std_1 ?? DBNull.Value;
                    cmd.Parameters.AddWithValue("@std_2", System.Data.SqlDbType.Int).Value = (Object)d.std_2 ?? DBNull.Value;
                    cmd.Parameters.AddWithValue("@std_3", System.Data.SqlDbType.Int).Value = (Object)d.std_3 ?? DBNull.Value;
                    cmd.Parameters.AddWithValue("@fsc_range", StringUtil.toString(d.fsc_range));
                    cmd.Parameters.AddWithValue("@amt_range", StringUtil.toString(d.amt_range));
                    cmd.Parameters.AddWithValue("@remark", StringUtil.toString(d.remark));
                    cmd.Parameters.AddWithValue("@exec_action", StringUtil.toString(d.exec_action));
                    cmd.Parameters.AddWithValue("@appr_stat", StringUtil.toString(d.appr_stat));
                    cmd.Parameters.AddWithValue("@update_id", StringUtil.toString(d.update_id));
                    cmd.Parameters.AddWithValue("@update_datetime", dt);



                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception e)
            {
                throw e;
            }
        }




        /// <summary>
        /// 以"群組代碼 + 代碼" 查覆核中的資料
        /// </summary>
        /// <param name="code_type"></param>
        /// <param name="code_id"></param>
        /// <returns></returns>
        public FAP_TEL_CODE_HIS qryByKey(string code_type, string code_id, string aply_no, string appr_stat)
        {

            using (new TransactionScope(
                TransactionScopeOption.Required,
                new TransactionOptions
                {
                    IsolationLevel = System.Transactions.IsolationLevel.ReadUncommitted
                }))
            {
                using (dbFGLEntities db = new dbFGLEntities())
                {
                    FAP_TEL_CODE_HIS row = db.FAP_TEL_CODE_HIS
                        .Where(x => x.code_type == code_type
                                & (code_id == "" || (code_id != "" & x.code_id == code_id))
                                & (aply_no == "" || (aply_no != "" & x.aply_no == aply_no))
                                & (appr_stat == "" || (appr_stat != "" & x.appr_stat == appr_stat))).FirstOrDefault();

                    if (row != null)
                        return row;
                    else
                        return new FAP_TEL_CODE_HIS();
                }
            }
        }

        public List<OAP0043Model> qryByOAP0043(string aply_no, string code_type, string fsc_range, string amt_range)
        {
            using (dbFGLEntities db = new dbFGLEntities())
            {

                var rows = (from m in db.FAP_TEL_CODE_HIS
                            where 1 == 1
                            & m.aply_no == aply_no
                            & m.code_type == code_type
                            & m.fsc_range == fsc_range
                            & m.amt_range == amt_range
                            select new OAP0043Model
                            {
                                key = m.code_id,
                                proc_id = m.proc_id,
                                std_1 = m.std_1.ToString(),
                                std_2 = m.std_2.ToString()
                            }).OrderByDescending(x => x.std_1).ToList<OAP0043Model>();

                return rows;
            }
        }


        public List<OAP0043Model> qryByAplyNo(string type, string aply_no, string appr_stat)
        {
            bool bApprStat = false;
            if ("".Equals(StringUtil.toString(appr_stat)))
                bApprStat = true;

            using (dbFGLEntities db = new dbFGLEntities())
            {

                var rows = (from m in db.FAP_TEL_CODE_HIS
                            where 1 == 1
                            & (bApprStat || (!bApprStat & m.appr_stat == appr_stat))
                            & m.aply_no == aply_no
                            & m.code_type == type
                            select new OAP0043Model
                            {
                                aply_no = m.aply_no,
                                type = m.code_type,
                                key = m.code_id,
                                proc_id = m.proc_id,
                                std_1 = m.std_1 == null ? "" : m.std_1.ToString(),
                                std_2 = m.std_2 == null ? "" : m.std_2.ToString(),
                                fsc_range = m.fsc_range,
                                amt_range = m.amt_range.ToString(),
                                create_id = m.update_id,
                                create_dt = m.update_datetime == null ? "" : SqlFunctions.DateName("year", m.update_datetime) + "/" +
                                                 SqlFunctions.DatePart("m", m.update_datetime) + "/" +
                                                 SqlFunctions.DateName("day", m.update_datetime).Trim() + " " +
                                                 SqlFunctions.DateName("hh", m.update_datetime).Trim() + ":" +
                                                 SqlFunctions.DateName("n", m.update_datetime).Trim() + ":" +
                                                 SqlFunctions.DateName("s", m.update_datetime).Trim()
                            }).Distinct().ToList<OAP0043Model>();

                return rows;
            }
        }



        public List<OAP0043Model> qryByOAP0043A(string[] code_type)
        {
            using (dbFGLEntities db = new dbFGLEntities())
            {

                var rows = (from m in db.FAP_TEL_CODE_HIS
                            where 1 == 1
                            & m.appr_stat == "1"
                            & code_type.Contains(m.code_type)
                            select new OAP0043Model
                            {
                                aply_no = m.aply_no,
                                type = m.code_type,
                                create_id = m.update_id,
                                create_dt = m.update_datetime == null ? "" : SqlFunctions.DateName("year", m.update_datetime) + "/" +
                                                 SqlFunctions.DatePart("m", m.update_datetime) + "/" +
                                                 SqlFunctions.DateName("day", m.update_datetime).Trim() + " " +
                                                 SqlFunctions.DateName("hh", m.update_datetime).Trim() + ":" +
                                                 SqlFunctions.DateName("n", m.update_datetime).Trim() + ":" +
                                                 SqlFunctions.DateName("s", m.update_datetime).Trim()
                            }).Distinct().ToList<OAP0043Model>();

                return rows;
            }
        }


        public List<OAP0045Model> qryByOAP0045A(string[] code_type)
        {
            using (dbFGLEntities db = new dbFGLEntities())
            {

                var rows = (from m in db.FAP_TEL_CODE_HIS

                            join cAction in db.SYS_CODE.Where(x => x.SYS_CD == "AP" & x.CODE_TYPE == "EXEC_ACTION") on m.exec_action equals cAction.CODE into psAction
                            from xActions in psAction.DefaultIfEmpty()

                            join cCodeType in db.SYS_CODE.Where(x => x.SYS_CD == "AP" ) on new { code_type = m.code_type, code_id = m.code_id } equals new { code_type = cCodeType.CODE_TYPE, code_id = cCodeType.CODE } into psCodeType
                            from xCodeType in psCodeType.DefaultIfEmpty()

                            where 1 == 1
                            & m.appr_stat == "1"
                            & code_type.Contains(m.code_type)
                            select new OAP0045Model
                            {
                                aply_no = m.aply_no,
                                code_type = m.code_type,
                                
                                exec_action = xActions.CODE_VALUE,

                                code_id = m.code_id,
                                code_id_desc = xCodeType.CODE_VALUE,
                                proc_id = m.proc_id,
                                std_1 = (int)m.std_1,
                                std_2 = (int)m.std_2,
                                std_3 = (int)m.std_3,
                                update_id = m.update_id,
                                update_datetime = m.update_datetime == null ? "" : SqlFunctions.DateName("year", m.update_datetime) + "/" +
                                                 SqlFunctions.DatePart("m", m.update_datetime) + "/" +
                                                 SqlFunctions.DateName("day", m.update_datetime).Trim() + " " +
                                                 SqlFunctions.DateName("hh", m.update_datetime).Trim() + ":" +
                                                 SqlFunctions.DateName("n", m.update_datetime).Trim() + ":" +
                                                 SqlFunctions.DateName("s", m.update_datetime).Trim()
                            }).Distinct().ToList<OAP0045Model>();

                return rows;
            }
        }


        public void insert(DateTime dt, FAP_TEL_CODE_HIS d, SqlConnection conn, SqlTransaction transaction)
        {

            try
            {

                string sql = @"
        INSERT INTO FAP_TEL_CODE_HIS
           (aply_no
           ,code_type
           ,code_id
           ,proc_id
           ,std_1
           ,std_2
           ,std_3
           ,fsc_range
           ,amt_range
           ,remark
           ,exec_action
           ,appr_stat
           ,update_id
           ,update_datetime)
             VALUES
           (@aply_no
           ,@code_type
           ,@code_id
           ,@proc_id
           ,@std_1
           ,@std_2
           ,@std_3
           ,@fsc_range
           ,@amt_range
           ,@remark
           ,@exec_action
           ,@appr_stat
           ,@update_id
           ,@update_datetime)";

                SqlCommand cmd = conn.CreateCommand();

                cmd.Connection = conn;
                cmd.Transaction = transaction;

                cmd.CommandText = sql;

                cmd.Parameters.Clear();

                cmd.Parameters.AddWithValue("@aply_no", StringUtil.toString(d.aply_no));
                cmd.Parameters.AddWithValue("@code_type", StringUtil.toString(d.code_type));
                cmd.Parameters.AddWithValue("@code_id", StringUtil.toString(d.code_id));
                cmd.Parameters.AddWithValue("@proc_id", StringUtil.toString(d.proc_id));
                cmd.Parameters.AddWithValue("@std_1", System.Data.SqlDbType.Int).Value = (Object)d.std_1 ?? DBNull.Value;
                cmd.Parameters.AddWithValue("@std_2", System.Data.SqlDbType.Int).Value = (Object)d.std_2 ?? DBNull.Value;
                cmd.Parameters.AddWithValue("@std_3", System.Data.SqlDbType.Int).Value = (Object)d.std_3 ?? DBNull.Value;
                cmd.Parameters.AddWithValue("@fsc_range", StringUtil.toString(d.fsc_range));
                cmd.Parameters.AddWithValue("@amt_range", StringUtil.toString(d.amt_range));
                cmd.Parameters.AddWithValue("@remark", StringUtil.toString(d.remark));
                cmd.Parameters.AddWithValue("@exec_action", StringUtil.toString(d.exec_action));
                cmd.Parameters.AddWithValue("@appr_stat", StringUtil.toString(d.appr_stat));
                cmd.Parameters.AddWithValue("@update_id", StringUtil.toString(d.update_id));
                cmd.Parameters.AddWithValue("@update_datetime", d.update_datetime);


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
        /// <param name="appr_stat"></param>
        /// <param name="aply_no"></param>
        /// <param name="code_type"></param>
        /// <param name="code_id"></param>
        /// <param name="conn"></param>
        /// <param name="transaction"></param>
        public void updateApprStatus(string userId, string appr_stat, string aply_no, string code_type, string code_id
            , DateTime now, SqlConnection conn, SqlTransaction transaction)
        {
           
            bool bCodeId = false;
            if ("".Equals(StringUtil.toString(code_id)))
                bCodeId = true;

            try
            {
                string sql = @"UPDATE [FAP_TEL_CODE_HIS]
SET appr_stat = @appr_stat
  , appr_id = @appr_id
  , approve_datetime = @approve_datetime
WHERE APLY_NO = @APLY_NO
  AND CODE_TYPE = @CODE_TYPE

          ";

                if (!bCodeId)
                    sql += " and code_id = @code_id";

                SqlCommand cmd = conn.CreateCommand();

                cmd.Connection = conn;
                cmd.Transaction = transaction;

                cmd.CommandText = sql;

                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("@APLY_NO", aply_no);
                cmd.Parameters.AddWithValue("@CODE_TYPE", code_type);
                cmd.Parameters.AddWithValue("@appr_stat", appr_stat);
                cmd.Parameters.AddWithValue("@appr_id", userId);
                cmd.Parameters.AddWithValue("@approve_datetime", now);
                if (!bCodeId)
                    cmd.Parameters.AddWithValue("@code_id", code_id);

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
        public void updateAplyNo(string code_type, string aply_no, string update_id, DateTime dt, SqlConnection conn, SqlTransaction transaction)
        {
            try
            {

                string sql = @"
UPDATE FAP_TEL_CODE_HIS
  SET APLY_NO = @aply_no
     ,appr_stat = '1'
     ,update_id = @update_id
     ,update_datetime = @update_datetime
 WHERE aply_no = ''
   AND code_type = @code_type";

                SqlCommand cmd = conn.CreateCommand();

                cmd.Connection = conn;
                cmd.Transaction = transaction;

                cmd.CommandText = sql;

                cmd.Parameters.Clear();

                cmd.Parameters.AddWithValue("@aply_no", StringUtil.toString(aply_no));
                cmd.Parameters.AddWithValue("@code_type", StringUtil.toString(code_type));
                cmd.Parameters.AddWithValue("@update_id", StringUtil.toString(update_id));
                cmd.Parameters.AddWithValue("@update_datetime", dt);

                cmd.ExecuteNonQuery();

            }
            catch (Exception e)
            {
                throw e;
            }
        }


        public void delForOAP0043(FAP_TEL_CODE_HIS d, SqlConnection conn, SqlTransaction transaction)
        {
            try
            {

                string sql = @"
DELETE FAP_TEL_CODE_HIS
 WHERE aply_no = @aply_no
   AND code_type = @code_type
   AND fsc_range = @fsc_range
   AND amt_range = @amt_range";

                SqlCommand cmd = conn.CreateCommand();

                cmd.Connection = conn;
                cmd.Transaction = transaction;

                cmd.CommandText = sql;

                cmd.Parameters.Clear();

                cmd.Parameters.AddWithValue("@aply_no", StringUtil.toString(d.aply_no));
                cmd.Parameters.AddWithValue("@code_type", StringUtil.toString(d.code_type));
                cmd.Parameters.AddWithValue("@fsc_range", StringUtil.toString(d.fsc_range));
                cmd.Parameters.AddWithValue("@amt_range", StringUtil.toString(d.amt_range));
                cmd.ExecuteNonQuery();

            }
            catch (Exception e)
            {
                throw e;
            }
        }



    }
}