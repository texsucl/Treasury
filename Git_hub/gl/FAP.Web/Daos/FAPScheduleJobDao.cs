
using FAP.Web.BO;
using FAP.Web.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity.SqlServer;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace FAP.Web.Daos
{
    public class FAPScheduleJobDao
    {
        static private NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();


        public FAP_SCHEDULE_JOB qryByName(string schedName)
        {
            using (dbFGLEntities db = new dbFGLEntities())
            {
                FAP_SCHEDULE_JOB d = db.FAP_SCHEDULE_JOB
                    .Where(x => x.sched_name == schedName).FirstOrDefault();

                return d;
            }
        }


        public string qryNextCode()
        {
            using (dbFGLEntities db = new dbFGLEntities())
            {
                string d = db.FAP_SCHEDULE_JOB.Max(x => x.sched_code);

                if (d == null)
                    d = "0000000000";

                return d;
            }
        }


        public int updateByName(FAP_SCHEDULE_JOB job, string remarkN) {

            int updCnt = 0;

            string strConn = DbUtil.GetDBFglConnStr();
            using (SqlConnection conn = new SqlConnection(strConn))
            {
                conn.Open();

                try
                {
                    string sql = @"
UPDATE FAP_SCHEDULE_JOB
  SET status = @status
     ,remark = @remarkN 
     ,start_exe_time = @start_exe_time 
     ,end_exe_time = @end_exe_time 
 WHERE  1 = 1
  AND sched_name = @sched_name 
  AND remark = @remarkO ";

                    SqlCommand cmd = conn.CreateCommand();
                    cmd.Connection = conn;
                    cmd.CommandText = sql;

                    cmd.Parameters.Clear();
                    cmd.Parameters.AddWithValue("@status", System.Data.SqlDbType.VarChar).Value = (Object)job.status ?? DBNull.Value;
                    cmd.Parameters.AddWithValue("@remarkN", System.Data.SqlDbType.VarChar).Value = (Object)remarkN ?? DBNull.Value;
                    cmd.Parameters.AddWithValue("@start_exe_time", System.Data.SqlDbType.DateTime).Value = (Object)job.start_exe_time ?? DBNull.Value;
                    cmd.Parameters.AddWithValue("@end_exe_time", System.Data.SqlDbType.DateTime).Value = (Object)job.end_exe_time ?? DBNull.Value;

                    cmd.Parameters.AddWithValue("@sched_name", job.sched_name);
                    cmd.Parameters.AddWithValue("@remarkO", System.Data.SqlDbType.VarChar).Value = (Object)job.remark ?? DBNull.Value;


                    updCnt = cmd.ExecuteNonQuery();


                }
                catch (Exception e)
                {
                    throw e;
                }
            }

            return updCnt;
        }



        public int insert(FAP_SCHEDULE_JOB job)
        {
            string nextCode = (Convert.ToInt64(qryNextCode()) + 1).ToString().PadLeft(10, '0');


            int cnt = 0;

            string strConn = DbUtil.GetDBFglConnStr();
            using (SqlConnection conn = new SqlConnection(strConn))
            {
                conn.Open();

                try
                {
                    string sql = @"
insert into FAP_SCHEDULE_JOB
 (sched_code
 ,sched_name
 ,status
 ,remark
 ,start_exe_time
 ,end_exe_time
 ,scan_timec)
     VALUES

(@sched_code
 ,@sched_name
 ,@status
 ,@remark
 ,@start_exe_time
 ,@end_exe_time
 ,@scan_timec
)
";

                    SqlCommand cmd = conn.CreateCommand();
                    cmd.Connection = conn;
                    cmd.CommandText = sql;

                    cmd.Parameters.Clear();
                    cmd.Parameters.AddWithValue("@sched_code", nextCode);
                    cmd.Parameters.AddWithValue("@sched_name", job.sched_name);
                    cmd.Parameters.AddWithValue("@status", System.Data.SqlDbType.VarChar).Value = (Object)job.status ?? DBNull.Value;
                    cmd.Parameters.AddWithValue("@remark", System.Data.SqlDbType.VarChar).Value = (Object)job.remark ?? DBNull.Value;
                    cmd.Parameters.AddWithValue("@start_exe_time", System.Data.SqlDbType.DateTime).Value = (Object)job.start_exe_time ?? DBNull.Value;
                    cmd.Parameters.AddWithValue("@end_exe_time", System.Data.SqlDbType.DateTime).Value = (Object)job.end_exe_time ?? DBNull.Value;
                    cmd.Parameters.AddWithValue("@scan_timec", System.Data.SqlDbType.VarChar).Value = (Object)job.scan_timec ?? DBNull.Value;


                    cnt = cmd.ExecuteNonQuery();


                }
                catch (Exception e)
                {
                    throw e;
                }
            }

            return cnt;
        }
    }

    
}