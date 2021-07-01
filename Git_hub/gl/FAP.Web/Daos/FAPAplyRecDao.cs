
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using FAP.Web.Models;
using System.Data.Entity.SqlServer;
using System.Transactions;
using FAP.Web.BO;
using FAP.Web.ViewModels;

namespace FAP.Web.Daos
{
    public class FAPAplyRecDao
    {
        public FAP_APLY_REC qryByKey(String aplyNo)
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
                    FAP_APLY_REC apprRec = db.FAP_APLY_REC.Where(x => x.aply_no == aplyNo).FirstOrDefault<FAP_APLY_REC>();

                    return apprRec;
                }
            }
        }
        

        /// <summary>
        /// 新增覆核資料
        /// </summary>
        /// <param name="authReview"></param>
        /// <param name="conn"></param>
        /// <param name="transaction"></param>
        /// <returns></returns>
        public string insert(string aply_no, FAP_APLY_REC aplyRec, SqlConnection conn, SqlTransaction transaction)
        {

            string[] curDateTime = DateUtil.getCurChtDateTime().Split(' ');

            if ("".Equals(StringUtil.toString(aply_no)))
            {
                //取得流水號
                SysSeqDao sysSeqDao = new SysSeqDao();
                String qPreCode = curDateTime[0];
                var cId = sysSeqDao.qrySeqNo("AP", aplyRec.aply_type, qPreCode).ToString();
                int seqLen = 12 - (aplyRec.aply_type + qPreCode).Length;
                aplyRec.aply_no = aplyRec.aply_type + qPreCode + cId.ToString().PadLeft(seqLen, '0');
            }
            else
                aplyRec.aply_no = aply_no;


            try
            {
                
                aplyRec.create_dt = DateTime.Now;

                string sql = @"
        INSERT INTO [FAP_APLY_REC]
                   ([aply_no]
                   ,[aply_type]
                   ,[appr_stat]
                   ,[appr_mapping_key]
                   ,[memo]
                   ,[create_id]
                   ,[create_dt])
             VALUES
                   (@aply_no
                   ,@aply_type
                   ,@appr_stat
                   ,@appr_mapping_key
                   ,@memo
                   ,@create_id
                   ,@create_dt)";

                SqlCommand cmd = conn.CreateCommand();

                cmd.Connection = conn;
                cmd.Transaction = transaction;

                cmd.CommandText = sql;

                cmd.Parameters.AddWithValue("@aply_no", StringUtil.toString(aplyRec.aply_no));
                cmd.Parameters.AddWithValue("@aply_type", StringUtil.toString(aplyRec.aply_type));
                cmd.Parameters.AddWithValue("@appr_stat", StringUtil.toString(aplyRec.appr_stat));
                cmd.Parameters.AddWithValue("@appr_mapping_key", StringUtil.toString(aplyRec.appr_mapping_key));
                cmd.Parameters.AddWithValue("@memo", StringUtil.toString(aplyRec.memo));
                cmd.Parameters.AddWithValue("@create_id", StringUtil.toString(aplyRec.create_id));
                cmd.Parameters.AddWithValue("@create_dt", aplyRec.create_dt);


                int cnt = cmd.ExecuteNonQuery();


                return aplyRec.aply_no;
            }
            catch (Exception e)
            {

                throw e;
            }


        }


        /// <summary>
        /// 異動覆核狀態
        /// </summary>
        /// <param name="authReview"></param>
        /// <param name="conn"></param>
        /// <param name="transaction"></param>
        /// <returns></returns>
        public int updateStatus(FAP_APLY_REC aplyRec, SqlConnection conn, SqlTransaction transaction)
        {

            try
            {
                string sql = "";

                if ("1".Equals(aplyRec.appr_stat))
                {
                    sql = @"update [FAP_APLY_REC]
                    set [appr_stat] = @appr_stat
                   ,[create_id] = @create_id
                   ,[create_dt] = @create_dt
             where  aply_no = @aply_no";
                }
                else {
                    sql = @"update [FAP_APLY_REC]
                    set [appr_stat] = @appr_stat
                   ,[appr_id] = @appr_id
                   ,[approve_datetime] = @approve_datetime
                   ,[update_id] = @update_id
                   ,[update_datetime] = @update_datetime
             where  aply_no = @aply_no";
                }
               

                SqlCommand cmd = conn.CreateCommand();

                cmd.Connection = conn;
                cmd.Transaction = transaction;

                cmd.CommandText = sql;
                cmd.Parameters.AddWithValue("@aply_no", aplyRec.aply_no.Trim());
                cmd.Parameters.AddWithValue("@appr_stat", aplyRec.appr_stat.Trim());
                cmd.Parameters.AddWithValue("@appr_id", aplyRec.appr_id.Trim());

                if ("1".Equals(aplyRec.appr_stat))
                {
                    cmd.Parameters.AddWithValue("@create_id", aplyRec.create_id);
                    cmd.Parameters.AddWithValue("@create_dt", aplyRec.create_dt);
                }
                else {
                    if (aplyRec.approve_datetime == null)
                        cmd.Parameters.AddWithValue("@approve_datetime", DBNull.Value);
                    else
                        cmd.Parameters.AddWithValue("@approve_datetime", aplyRec.approve_datetime);

                    cmd.Parameters.AddWithValue("@update_id", aplyRec.update_id);
                    cmd.Parameters.AddWithValue("@update_datetime", aplyRec.update_datetime);

                }
                 

                int cnt = cmd.ExecuteNonQuery();

                return cnt;
            }
            catch (Exception e)
            {

                throw e;
            }
        }


        public int delByAplyNo(string aplyNo, SqlConnection conn, SqlTransaction transaction)
        {

            try
            {
                string sql = @"DELETE FAP_APLY_REC WHERE aply_no = @aply_no";

                SqlCommand cmd = conn.CreateCommand();

                cmd.Connection = conn;
                cmd.Transaction = transaction;

                cmd.CommandText = sql;
                cmd.Parameters.AddWithValue("@aply_no", aplyNo);


                int cnt = cmd.ExecuteNonQuery();

                return cnt;
            }
            catch (Exception e)
            {

                throw e;
            }
        }



        /// <summary>
        /// 依覆核單類別查詢特定狀態的資料
        /// </summary>
        /// <param name="aplyType"></param>
        /// <param name="apprStat"></param>
        /// <param name="db"></param>
        /// <returns></returns>
        public List<APAplyRecModel> qryAplyType(string aplyType, string apprStat, string apprMappingKey, dbFGLEntities db)
        {
            bool bAplyType = StringUtil.isEmpty(aplyType);
            bool bApprStat = StringUtil.isEmpty(apprStat);
            bool bApprMappingKey = StringUtil.isEmpty(apprMappingKey);

            List<APAplyRecModel> fapAplyRecList = new List<APAplyRecModel>();


            fapAplyRecList = (from m in db.FAP_APLY_REC
                              join cSts in db.SYS_CODE.Where(x => x.SYS_CD == "AP" & x.CODE_TYPE == "APPR_STAT") on m.appr_stat equals cSts.CODE into psCSts
                              from xCSts in psCSts.DefaultIfEmpty()

                              where (bAplyType || m.aply_type.Trim() == aplyType)
                                      & (bApprStat || m.appr_stat.Trim() == apprStat)
                                      & (bApprMappingKey || m.appr_mapping_key.Trim() == apprMappingKey)
                              select new APAplyRecModel
                                  {

                                      aply_no = m.aply_no.Trim(),
                                      appr_stat = m.appr_stat.Trim(),
                                      appr_stat_desc = xCSts == null ? "" : xCSts.CODE_VALUE.Trim(),
                                      memo = m.memo,
                                      create_id = m.create_id,
                                      create_dt = m.create_dt == null ? "" : SqlFunctions.DateName("year", m.create_dt) + "/" +
                                                 SqlFunctions.DatePart("m", m.create_dt) + "/" +
                                                 SqlFunctions.DateName("day", m.create_dt).Trim() + " " +
                                                 SqlFunctions.DateName("hh", m.create_dt).Trim() + ":" +
                                                 SqlFunctions.DateName("n", m.create_dt).Trim() + ":" +
                                                 SqlFunctions.DateName("s", m.create_dt).Trim(),
                                      appr_mapping_key = m.appr_mapping_key.Trim() 

                                  }).ToList();

           


            return fapAplyRecList;
        }
    }
}