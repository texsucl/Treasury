using FRT.Web.AS400Models;
using FRT.Web.BO;
using FRT.Web.Models;
using FRT.Web.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.Entity.SqlServer;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace FRT.Web.Daos
{
    public class FRTRvmnHisDao
    {
        static private NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();



        public bool updApprStat(string appr_id, string appr_stat, FRT_RVMN_HIS procData, SqlConnection conn, SqlTransaction transaction)
        {
            bool execResult = true;

            try
            {
                string sql = @"
UPDATE FRT_RVMN_HIS
  SET appr_stat = @appr_stat
     ,appr_id = @appr_id 
     ,approve_datetime = @approve_datetime 
 WHERE  1 = 1
  AND aply_no = @aply_no 
  AND corp_no = @corp_no 
  AND vhr_no1 = @vhr_no1
  AND pro_no = @pro_no 
  AND paid_id = @paid_id ";

                SqlCommand cmd = conn.CreateCommand();
                cmd.Connection = conn;
                cmd.Transaction = transaction;
                cmd.CommandText = sql;

                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("@appr_stat", appr_stat);
                cmd.Parameters.AddWithValue("@appr_id", appr_id);
                cmd.Parameters.AddWithValue("@approve_datetime", DateTime.Now);
                cmd.Parameters.AddWithValue("@aply_no", procData.aply_no);
                cmd.Parameters.AddWithValue("@corp_no", procData.corp_no);
                cmd.Parameters.AddWithValue("@vhr_no1", procData.vhr_no1);
                cmd.Parameters.AddWithValue("@pro_no", procData.pro_no);
                cmd.Parameters.AddWithValue("@paid_id", procData.paid_id);

                cmd.ExecuteNonQuery();


            }
            catch (Exception e)
            {
                execResult = false;
                throw e;
            }


            return execResult;
        }


        public List<FRT_RVMN_HIS> qryFor400Key(string corp_no, string vhr_no1, string pro_no, string paid_id, string appr_stat)
        {
            bool bApprStat = StringUtil.isEmpty(appr_stat) & StringUtil.isEmpty(appr_stat);

            using (dbFGLEntities db = new dbFGLEntities())
            {

                List<FRT_RVMN_HIS> his = db.FRT_RVMN_HIS
                    .Where(x => x.corp_no == corp_no 
                     & x.vhr_no1 == vhr_no1 
                     & x.pro_no == pro_no
                     & x.paid_id == paid_id
                     & (bApprStat || (!bApprStat & x.appr_stat == appr_stat)))
                    .ToList<FRT_RVMN_HIS>();

                return his;
            }
        }

        public FRT_RVMN_HIS qryByAplyNo(string aply_no)
        {

            using (dbFGLEntities db = new dbFGLEntities())
            {

                FRT_RVMN_HIS his = db.FRT_RVMN_HIS
                    .Where(x => x.aply_no == aply_no).FirstOrDefault();

                return his;
            }
        }


        public List<ORT0104Model> qryForORT0104A()
        {
            dbFGLEntities db = new dbFGLEntities();

            var rows = (from main in db.FRT_RVMN_HIS
                        where 1 == 1
                            & main.appr_stat == "1"
                        select new ORT0104Model
                        {
                            aply_no = main.aply_no,
                            corp_no = main.corp_no,
                            vhr_no1 = main.vhr_no1,
                            pro_no = main.pro_no,
                            paid_id = main.paid_id,
                            currency = main.currency,
                            fail_code = main.fail_code,
                            seqn = main.seqn,
                            fail_code_o = main.fail_code_o,
                            seqn_o = main.seqn_o,
                            update_id = main.update_id,
                            update_datetime = SqlFunctions.DateName("year", main.update_datetime) + "/" +
                                            SqlFunctions.DatePart("m", main.update_datetime) + "/" +
                                            SqlFunctions.DateName("day", main.update_datetime).Trim() + " " +
                                            SqlFunctions.DateName("hh", main.update_datetime).Trim() + ":" +
                                            SqlFunctions.DateName("n", main.update_datetime).Trim() + ":" +
                                            SqlFunctions.DateName("s", main.update_datetime).Trim()
                        }).ToList<ORT0104Model>();

            return rows;
        }



        public void insert(FRT_RVMN_HIS d)
        {
            d.corp_no = StringUtil.toString(d.currency) == "NTD" ? "1" : "3";

            string strConn = DbUtil.GetDBFglConnStr();
            using (SqlConnection conn = new SqlConnection(strConn))
            {
                conn.Open();
                SqlTransaction transaction = conn.BeginTransaction("Transaction");
                try
                {
                    string sql = @"
INSERT INTO [dbo].[FRT_RVMN_HIS] (  
   aply_no
 , corp_no
 , vhr_no1
 , pro_no
 , paid_id
 , currency
 , fail_code
 , seqn
 , fail_code_o
 , seqn_o
 , appr_stat
 , update_id
 , update_datetime
) VALUES (
   @aply_no
 , @corp_no
 , @vhr_no1
 , @pro_no
 , @paid_id
 , @currency
 , @fail_code
 , @seqn
 , @fail_code_o
 , @seqn_o
 , @appr_stat
 , @update_id
 , @update_datetime
)
";

                    SqlCommand cmd = conn.CreateCommand();

                    cmd.Connection = conn;
                    cmd.Transaction = transaction;

                    cmd.CommandText = sql;

                    cmd.Parameters.Clear();

                    cmd.Parameters.AddWithValue("@aply_no", StringUtil.toString(d.aply_no));
                    cmd.Parameters.AddWithValue("@corp_no", StringUtil.toString(d.corp_no));
                    cmd.Parameters.AddWithValue("@vhr_no1", StringUtil.toString(d.vhr_no1));
                    cmd.Parameters.AddWithValue("@pro_no", StringUtil.toString(d.pro_no));
                    cmd.Parameters.AddWithValue("@paid_id", StringUtil.toString(d.paid_id));
                    cmd.Parameters.AddWithValue("@currency", StringUtil.toString(d.currency));
                    cmd.Parameters.AddWithValue("@fail_code", StringUtil.toString(d.fail_code));
                    cmd.Parameters.AddWithValue("@seqn", StringUtil.toString(d.seqn));
                    cmd.Parameters.AddWithValue("@fail_code_o", StringUtil.toString(d.fail_code_o));
                    cmd.Parameters.AddWithValue("@seqn_o", StringUtil.toString(d.seqn_o));
                    cmd.Parameters.AddWithValue("@appr_stat", StringUtil.toString(d.appr_stat));

                    cmd.Parameters.AddWithValue("@update_id", StringUtil.toString(d.update_id));
                    cmd.Parameters.Add("@update_datetime", System.Data.SqlDbType.DateTime).Value = (System.Object)d.update_datetime ?? System.DBNull.Value;


                   
                    cmd.ExecuteNonQuery();
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