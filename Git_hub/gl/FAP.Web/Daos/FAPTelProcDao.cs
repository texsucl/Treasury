
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
/// 功能說明：FAP_TEL_PROC 電訪及追踨記錄歷程檔
/// 初版作者：20200908 Daiyu
/// 修改歷程：20200908 Daiyu
/// 需求單號：
/// 修改內容：初版
/// --------------------------------------------------

/// </summary>
/// 

namespace FAP.Web.Daos
{
    public class FAPTelProcDao
    {

        public FAP_TEL_PROC qryByTelProcNo(string tel_proc_no)
        {

            using (new TransactionScope(
                   TransactionScopeOption.Required,
                   new TransactionOptions
                   {
                       IsolationLevel = IsolationLevel.ReadUncommitted
                   }))
            {

                using (dbFGLEntities db = new dbFGLEntities())
                {
                    FAP_TEL_PROC d = db.FAP_TEL_PROC
                        .Where(x => x.tel_proc_no == tel_proc_no)
                        .OrderByDescending(x => x.seq_no).FirstOrDefault();

                    if (d != null)
                        return d;
                    else
                        return new FAP_TEL_PROC();
                }
            }


            
        }

        public List<FAP_TEL_PROC> qryTelProcNoList(string tel_proc_no, string data_type, string appr_stat)
        {

            bool bDataType = true;
            bool bApprStat = true;

            if (!"".Equals(data_type))
                bDataType = false;

            if (!"".Equals(appr_stat))
                bApprStat = false;

            using (new TransactionScope(
                   TransactionScopeOption.Required,
                   new TransactionOptions
                   {
                       IsolationLevel = IsolationLevel.ReadUncommitted
                   }))
            {

                using (dbFGLEntities db = new dbFGLEntities())
                {
                    return db.FAP_TEL_PROC
                        .Where(x => x.tel_proc_no == tel_proc_no
                        & (bDataType || (!bDataType & x.data_type == data_type))
                        & (bApprStat || (!bApprStat & x.appr_stat == appr_stat))
                        ).ToList();
                }
            }
            
        }


        /// <summary>
        /// 新增
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="d"></param>
        /// <param name="conn"></param>
        /// <param name="transaction"></param>
        public void insert(FAP_TEL_PROC d, SqlConnection conn, SqlTransaction transaction)
        {
            int _seq_no = 0;

            FAP_TEL_PROC proc = new FAP_TEL_PROC();
            proc = qryByTelProcNo(d.tel_proc_no);
            if (!"".Equals(proc.tel_proc_no))
                _seq_no = proc.seq_no + 1;

            try
            {

                string sql = @"
        INSERT INTO FAP_TEL_PROC (
  tel_proc_no
, seq_no
, aply_no
, data_type
, proc_id
, proc_datetime
, expect_datetime
, proc_status
, reason
, appr_id
, appr_datetime
, appr_status
, appr_stat
)  VALUES (
  @tel_proc_no
, @seq_no
, @aply_no
, @data_type
, @proc_id
, @proc_datetime
, @expect_datetime
, @proc_status
, @reason
, @appr_id
, @appr_datetime
, @appr_status
, @appr_stat
)";

                SqlCommand cmd = conn.CreateCommand();

                cmd.Connection = conn;
                cmd.Transaction = transaction;

                cmd.CommandText = sql;

                cmd.Parameters.Clear();

                cmd.Parameters.AddWithValue("@tel_proc_no", StringUtil.toString(d.tel_proc_no));
                cmd.Parameters.AddWithValue("@seq_no", _seq_no);
                cmd.Parameters.AddWithValue("@aply_no", StringUtil.toString(d.aply_no));
                cmd.Parameters.AddWithValue("@data_type", StringUtil.toString(d.data_type));
                cmd.Parameters.AddWithValue("@proc_id", StringUtil.toString(d.proc_id));
                cmd.Parameters.AddWithValue("@proc_datetime", System.Data.SqlDbType.DateTime).Value = (Object)d.proc_datetime ?? DBNull.Value;
                cmd.Parameters.AddWithValue("@expect_datetime", System.Data.SqlDbType.DateTime).Value = (Object)d.expect_datetime ?? DBNull.Value;
                cmd.Parameters.AddWithValue("@proc_status", StringUtil.toString(d.proc_status));
                cmd.Parameters.AddWithValue("@reason", StringUtil.toString(d.reason));
                cmd.Parameters.AddWithValue("@appr_id", StringUtil.toString(d.appr_id));
                cmd.Parameters.AddWithValue("@appr_datetime", System.Data.SqlDbType.DateTime).Value = (Object)d.appr_datetime ?? DBNull.Value;
                cmd.Parameters.AddWithValue("@appr_status", StringUtil.toString(d.appr_status));
                cmd.Parameters.AddWithValue("@appr_stat", StringUtil.toString(d.appr_stat));

                cmd.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        
        
        

    }
}