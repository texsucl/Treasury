
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using FGL.Web.Models;
using System.Data.Entity.SqlServer;
using System.Transactions;
using FGL.Web.BO;
using FGL.Web.ViewModels;

namespace FGL.Web.Daos
{
    public class FGLAplyRecDao
    {
        public FGL_APLY_REC qryByKey(String aplyNo)
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
                    FGL_APLY_REC apprRec = db.FGL_APLY_REC.Where(x => x.aply_no == aplyNo).FirstOrDefault<FGL_APLY_REC>();

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
        public string insert(FGL_APLY_REC aplyRec, SqlConnection conn, SqlTransaction transaction)
        {

            string[] curDateTime = DateUtil.getCurChtDateTime().Split(' ');

            //取得流水號
            SysSeqDao sysSeqDao = new SysSeqDao();
            String qPreCode = curDateTime[0];
            var cId = sysSeqDao.qrySeqNo("FGL", aplyRec.aply_type, qPreCode).ToString();



            try
            {
                aplyRec.aply_no = aplyRec.aply_type + qPreCode + cId.ToString().PadLeft(4, '0');
                aplyRec.create_dt = DateTime.Now;

                string sql = @"
        INSERT INTO [FGL_APLY_REC]
                   ([APLY_NO]
                   ,[APLY_TYPE]
                   ,[APPR_STAT]
                   ,[APPR_MAPPING_KEY]
                   ,[CREATE_ID]
                   ,[CREATE_DT])
             VALUES
                   (@APLY_NO
                   ,@APLY_TYPE
                   ,@APPR_STAT
                   ,@APPR_MAPPING_KEY
                   ,@CREATE_ID
                   ,@CREATE_DT)";

                SqlCommand cmd = conn.CreateCommand();

                cmd.Connection = conn;
                cmd.Transaction = transaction;

                cmd.CommandText = sql;

                cmd.Parameters.AddWithValue("@APLY_NO", StringUtil.toString(aplyRec.aply_no));
                cmd.Parameters.AddWithValue("@APLY_TYPE", StringUtil.toString(aplyRec.aply_type));
                cmd.Parameters.AddWithValue("@APPR_STAT", StringUtil.toString(aplyRec.appr_stat));
                cmd.Parameters.AddWithValue("@APPR_MAPPING_KEY", StringUtil.toString(aplyRec.appr_mapping_key));
                cmd.Parameters.AddWithValue("@CREATE_ID", StringUtil.toString(aplyRec.create_id));
                cmd.Parameters.AddWithValue("@CREATE_DT", aplyRec.create_dt);


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
        public int updateStatus(FGL_APLY_REC aplyRec, SqlConnection conn, SqlTransaction transaction)
        {

            try
            {
                string sql = "";

                if ("1".Equals(aplyRec.appr_stat))
                {
                    sql = @"update [FGL_APLY_REC]
                    set [APPR_STAT] = @APPR_STAT
                   ,[CREATE_ID] = @CREATE_ID
                   ,[CREATE_DT] = @CREATE_DT
             where  APLY_NO = @APLY_NO";
                }
                else {
                    sql = @"update [FGL_APLY_REC]
                    set [APPR_STAT] = @APPR_STAT
                   ,[APPR_ID] = @APPR_ID
                   ,[APPROVE_DATETIME] = @APPROVE_DATETIME
                   ,[UPDATE_ID] = @UPDATE_ID
                   ,[UPDATE_DATETIME] = @UPDATE_DATETIME
             where  APLY_NO = @APLY_NO";
                }
               

                SqlCommand cmd = conn.CreateCommand();

                cmd.Connection = conn;
                cmd.Transaction = transaction;

                cmd.CommandText = sql;
                cmd.Parameters.AddWithValue("@APLY_NO", aplyRec.aply_no.Trim());
                cmd.Parameters.AddWithValue("@APPR_STAT", aplyRec.appr_stat.Trim());
                cmd.Parameters.AddWithValue("@APPR_ID", aplyRec.appr_id.Trim());

                if ("1".Equals(aplyRec.appr_stat))
                {
                    cmd.Parameters.AddWithValue("@CREATE_ID", aplyRec.create_id);
                    cmd.Parameters.AddWithValue("@CREATE_DT", aplyRec.create_dt);
                }
                else {
                    if (aplyRec.approve_datetime == null)
                        cmd.Parameters.AddWithValue("@APPROVE_DATETIME", DBNull.Value);
                    else
                        cmd.Parameters.AddWithValue("@APPROVE_DATETIME", aplyRec.approve_datetime);

                    cmd.Parameters.AddWithValue("@UPDATE_ID", aplyRec.update_id);
                    cmd.Parameters.AddWithValue("@UPDATE_DATETIME", aplyRec.update_datetime);

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
                string sql = @"DELETE FGL_APLY_REC WHERE APLY_NO = @APLY_NO";

                SqlCommand cmd = conn.CreateCommand();

                cmd.Connection = conn;
                cmd.Transaction = transaction;

                cmd.CommandText = sql;
                cmd.Parameters.AddWithValue("@APLY_NO", aplyNo);


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
        public List<GLAplyRecModel> qryAplyType(String aplyType, String apprStat, dbFGLEntities db)
        {
            bool bAplyType = StringUtil.isEmpty(aplyType);
            bool bApprStat = StringUtil.isEmpty(apprStat);

            List<GLAplyRecModel> fglAplyRecList = new List<GLAplyRecModel>();


            fglAplyRecList = (from m in db.FGL_APLY_REC

                                  join cSts in db.SYS_CODE.Where(x => x.SYS_CD == "GL" & x.CODE_TYPE == "APPR_STAT") on m.appr_stat equals cSts.CODE into psCSts
                                  from xCSts in psCSts.DefaultIfEmpty()

                                  where (bAplyType || m.aply_type.Trim() == aplyType)
                                      & (bApprStat || m.appr_stat.Trim() == apprStat)
                                  select new GLAplyRecModel
                                  {

                                      aplyNo = m.aply_no.Trim(),
                                      apprStat = m.appr_stat.Trim(),
                                      apprStatDesc = xCSts == null ? "" : xCSts.CODE_VALUE.Trim(),
                                      createUid = m.create_id,
                                      createDt = m.create_dt == null ? "" : SqlFunctions.DateName("year", m.create_dt) + "/" +
                                                 SqlFunctions.DatePart("m", m.create_dt) + "/" +
                                                 SqlFunctions.DateName("day", m.create_dt).Trim() + " " +
                                                 SqlFunctions.DateName("hh", m.create_dt).Trim() + ":" +
                                                 SqlFunctions.DateName("n", m.create_dt).Trim() + ":" +
                                                 SqlFunctions.DateName("s", m.create_dt).Trim(),
                                      cMappingKey = m.appr_mapping_key.Trim() 

                                  }).ToList();

           


            return fglAplyRecList;
        }
    }
}