using FRT.Web.BO;
using FRT.Web.Models;
using FRT.Web.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Data.Entity.SqlServer;

namespace FRT.Web.Daos
{
    public class SysParaHisDao
    {
        public List<ORTB010Model> qryForORTB010(string apprStatus)
        {
            dbFGLEntities context = new dbFGLEntities();

            var rows = (from para in context.SYS_PARA_HIS
                        join code in context.SYS_CODE.Where(x => x.SYS_CD == "RT" & x.CODE_TYPE == "FAST_PARA") on para.GRP_ID equals code.CODE 
                        where para.APPR_STATUS == apprStatus
                        select new ORTB010Model
                        {
                            aplyNo = para.APLY_NO,
                            sysCd = para.SYS_CD,
                            grpId = para.GRP_ID,
                            grpDesc = code.CODE_VALUE,
                            paraId = para.PARA_ID,
                            paraValue = para.PARA_VALUE,
                            remark = para.REMARK,
                            reserve1 = para.RESERVE1,
                            reserve3 = para.RESERVE3,
                            createUid = para.CREATE_UID,
                            createDt = para.CREATE_DT == null ? "" : SqlFunctions.DateName("year", para.CREATE_DT) + "/" +
                                                                     SqlFunctions.DatePart("m", para.CREATE_DT) + "/" +
                                                                     SqlFunctions.DateName("day", para.CREATE_DT).Trim() + " " +
                                                                     SqlFunctions.DateName("hh", para.CREATE_DT).Trim() + ":" +
                                                                     SqlFunctions.DateName("n", para.CREATE_DT).Trim() + ":" +
                                                                     SqlFunctions.DateName("s", para.CREATE_DT).Trim(),
                        }).OrderBy(x => x.reserve3).ToList<ORTB010Model>();

            return rows;

        }


        /// <summary>
        /// 依覆核狀態查詢
        /// </summary>
        /// <param name="sysCd"></param>
        /// <param name="grpId"></param>
        /// <param name="apprStatus"></param>
        /// <returns></returns>
        public List<SYS_PARA_HIS> qryForGrpId(string sysCd, string grpId, string apprStatus)
        {
            dbFGLEntities context = new dbFGLEntities();
            List<SYS_PARA_HIS> rows = context.SYS_PARA_HIS
                .Where(x => x.SYS_CD == sysCd 
                    & x.GRP_ID == grpId 
                    & x.APPR_STATUS == apprStatus).ToList<SYS_PARA_HIS>();

                        
            return rows;

        }



        /// <summary>
        /// 依申請單號查詢
        /// </summary>
        /// <param name="sysCd"></param>
        /// <param name="grpId"></param>
        /// <param name="aplyNo"></param>
        /// <returns></returns>
        public List<SYS_PARA_HIS> qryByAplyNo(string sysCd, string grpId, string aplyNo)
        {
            dbFGLEntities context = new dbFGLEntities();
            List<SYS_PARA_HIS> rows = context.SYS_PARA_HIS
                .Where(x => x.SYS_CD == sysCd
                    & x.GRP_ID == grpId
                    & x.APLY_NO == aplyNo).ToList<SYS_PARA_HIS>();


            return rows;

        }


        /// <summary>
        /// 異動"覆核狀態"
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="apprStatus"></param>
        /// <param name="data"></param>
        /// <param name="conn"></param>
        /// <param name="transaction"></param>
        public void updateApprStatus(string userId, string apprStatus, List<SYS_PARA_HIS> data, SqlConnection conn, SqlTransaction transaction)
        {
            DateTime dtNow = DateTime.Now;

            try
            {
                string sql = @"UPDATE [SYS_PARA_HIS]
SET APPR_STATUS = @APPR_STATUS
  , APPR_UID = @APPR_UID
  , APPR_DT = @APPR_DT
WHERE APLY_NO = @APLY_NO
  AND SYS_CD = @SYS_CD
  AND GRP_ID = @GRP_ID

          ";

                SqlCommand cmd = conn.CreateCommand();

                cmd.Connection = conn;
                cmd.Transaction = transaction;

                cmd.CommandText = sql;

                foreach (SYS_PARA_HIS d in data)
                {
                    cmd.Parameters.Clear();
                    cmd.Parameters.AddWithValue("@APLY_NO", d.APLY_NO);
                    cmd.Parameters.AddWithValue("@SYS_CD", d.SYS_CD);
                    cmd.Parameters.AddWithValue("@GRP_ID", d.GRP_ID);
                    cmd.Parameters.AddWithValue("@APPR_STATUS", apprStatus);
                    cmd.Parameters.AddWithValue("@APPR_UID", userId);
                    cmd.Parameters.AddWithValue("@APPR_DT", dtNow);

                    cmd.ExecuteNonQuery();
                }

            }
            catch (Exception e)
            {
                throw e;
            }
        }



        /// <summary>
        /// 新增"系統參數異動檔"
        /// </summary>
        /// <param name="data"></param>
        public void insert(List<SYS_PARA_HIS> data)
        {
            DateTime dtNow = DateTime.Now;

            string strConn = DbUtil.GetDBFglConnStr();
            using (SqlConnection conn = new SqlConnection(strConn))
            {
                conn.Open();
                SqlTransaction transaction = conn.BeginTransaction("Transaction");
                try
                {
                    string sql = @"INSERT INTO [SYS_PARA_HIS]
           ([APLY_NO]
           ,[SYS_CD]
           ,[GRP_ID]
           ,[PARA_ID]
           ,[PARA_VALUE]
           ,[REMARK]
           ,[RESERVE1]
           ,[RESERVE2]
           ,[RESERVE3]
           ,[APPR_STATUS]
           ,[CREATE_UID]
           ,[CREATE_DT])
     VALUES
            (@APLY_NO
           ,@SYS_CD
           ,@GRP_ID
           ,@PARA_ID
           ,@PARA_VALUE
           ,@REMARK
           ,@RESERVE1
           ,@RESERVE2
           ,@RESERVE3
           ,@APPR_STATUS
           ,@CREATE_UID
           ,@CREATE_DT)";

                    SqlCommand cmd = conn.CreateCommand();

                    cmd.Connection = conn;
                    cmd.Transaction = transaction;

                    cmd.CommandText = sql;

                    foreach (SYS_PARA_HIS d in data) {
                        cmd.Parameters.Clear();
                        cmd.Parameters.AddWithValue("@APLY_NO", d.APLY_NO);
                        cmd.Parameters.AddWithValue("@SYS_CD", d.SYS_CD);
                        cmd.Parameters.AddWithValue("@GRP_ID", d.GRP_ID);
                        cmd.Parameters.AddWithValue("@PARA_ID", d.PARA_ID);
                        cmd.Parameters.AddWithValue("@PARA_VALUE", d.PARA_VALUE);
                        cmd.Parameters.AddWithValue("@REMARK", StringUtil.toString(d.REMARK));
                        cmd.Parameters.AddWithValue("@RESERVE1", StringUtil.toString(d.RESERVE1));
                        cmd.Parameters.AddWithValue("@RESERVE2", StringUtil.toString(d.RESERVE2));
                        cmd.Parameters.AddWithValue("@RESERVE3", StringUtil.toString(d.RESERVE3));
                        cmd.Parameters.AddWithValue("@APPR_STATUS", d.APPR_STATUS);
                        cmd.Parameters.AddWithValue("@CREATE_UID", d.CREATE_UID);
                        cmd.Parameters.AddWithValue("@CREATE_DT", dtNow);

                        cmd.ExecuteNonQuery();
                    }

                    transaction.Commit();
                }
                catch (Exception e)
                {
                    transaction.Rollback();
                    throw e;
                }

            }
        }
    }
}