using FAP.Web.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace FAP.Web.Daos
{
    public class SysParaDao
    {
       

        public List<SYS_PARA> qryByGrpId(string sysCd, string grpId)
        {
            dbFGLEntities context = new dbFGLEntities();

            var result = context.SYS_PARA.Where(x => x.SYS_CD == sysCd & x.GRP_ID == grpId).OrderBy(x => x.PARA_ID).ToList();


            return result;
        }


        public SYS_PARA qryByKey(string sysCd, string grpId, string paraId)
        {
            dbFGLEntities context = new dbFGLEntities();

            var result = context.SYS_PARA.Where(x => x.SYS_CD == sysCd & x.GRP_ID == grpId & x.PARA_ID == paraId).FirstOrDefault();


            return result;
        }


        public void updForOAP0042ByHis( string aply_no, string type, SqlConnection conn, SqlTransaction transaction)
        {


            try
            {
                string sql = @"UPDATE [SYS_PARA] 
SET SYS_PARA.PARA_VALUE = h.PARA_VALUE
   , SYS_PARA.RESERVE1 = ''
  FROM SYS_PARA_HIS h
WHERE h.APLY_NO = @APLY_NO
  AND h.SYS_CD = @SYS_CD
  AND h.GRP_ID = @GRP_ID
  AND h.SYS_CD = SYS_PARA.SYS_CD
  and h.grp_id = SYS_PARA.grp_id
  and h.para_id = SYS_PARA.para_id
          ";

                SqlCommand cmd = conn.CreateCommand();

                cmd.Connection = conn;
                cmd.Transaction = transaction;

                cmd.CommandText = sql;
                cmd.Parameters.AddWithValue("@APLY_NO", aply_no);
                cmd.Parameters.AddWithValue("@SYS_CD", "AP");
                cmd.Parameters.AddWithValue("@GRP_ID", type);
                cmd.ExecuteNonQuery();


            }
            catch (Exception e)
            {
                throw e;
            }
        }



        public void updForOAP0042(string type, SqlConnection conn, SqlTransaction transaction)
        {


            try
            {
                string sql = @"UPDATE [SYS_PARA] 
SET RESERVE1 = 'Y'
WHERE SYS_CD = @SYS_CD
  AND GRP_ID = @GRP_ID
          ";

                SqlCommand cmd = conn.CreateCommand();

                cmd.Connection = conn;
                cmd.Transaction = transaction;

                cmd.CommandText = sql;
                cmd.Parameters.AddWithValue("@SYS_CD", "AP");
                cmd.Parameters.AddWithValue("@GRP_ID", type);
                cmd.ExecuteNonQuery();


            }
            catch (Exception e)
            {
                throw e;
            }
        }



    }
}