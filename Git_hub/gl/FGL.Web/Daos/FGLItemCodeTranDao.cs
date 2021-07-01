
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using FGL.Web.Models;
using System.Transactions;
using FGL.Web.ViewModels;
using System.Data.Entity.SqlServer;
using FGL.Web.BO;

namespace FGL.Web.Daos
{
    public class FGLItemCodeTranDao
    {
        public FGL_ITEM_CODE_TRAN qryForWanpie(string productNo)
        {
            productNo = productNo.PadRight(27, ' ');
            string[] p1 = new string[] { " ", productNo .Substring(0, 1)};
            string[] p2 = new string[] { " ", productNo.Substring(1, 1) };
            string[] p3 = new string[] { " ", productNo.Substring(2, 1) };
            string[] p4 = new string[] { " ", productNo.Substring(3, 1) };
            string[] p5 = new string[] { " ", productNo.Substring(4, 1) };
            string[] p6 = new string[] { " ", productNo.Substring(5, 1) };
            string[] p7 = new string[] { " ", productNo.Substring(6, 1) };
            string[] p8 = new string[] { " ", productNo.Substring(7, 1) };
            string[] p9 = new string[] { " ", productNo.Substring(8, 1) };
            string[] p10 = new string[] { " ", productNo.Substring(9, 1) };
            string[] p11 = new string[] { " ", productNo.Substring(10, 1) };
            string[] p12 = new string[] { " ", productNo.Substring(11, 1) };
            string[] p13 = new string[] { " ", productNo.Substring(12, 1) };
            string[] p14 = new string[] { " ", productNo.Substring(13, 1) };
            string[] p15 = new string[] { " ", productNo.Substring(14, 1) };
            string[] p16 = new string[] { " ", productNo.Substring(15, 1) };
            string[] p17 = new string[] { " ", productNo.Substring(16, 1) };
            string[] p18 = new string[] { " ", productNo.Substring(17, 1) };
            string[] p19 = new string[] { " ", productNo.Substring(18, 1) };
            string[] p20 = new string[] { " ", productNo.Substring(19, 1) };
            string[] p21 = new string[] { " ", productNo.Substring(20, 1) };
            string[] p22 = new string[] { " ", productNo.Substring(21, 1) };
            string[] p23 = new string[] { " ", productNo.Substring(22, 1) };
            string[] p24 = new string[] { " ", productNo.Substring(23, 1) };
            string[] p25 = new string[] { " ", productNo.Substring(24, 1) };
            string[] p26 = new string[] { " ", productNo.Substring(25, 1) };
            string[] p27 = new string[] { " ", productNo.Substring(26, 1) };


            FGL_ITEM_CODE_TRAN d = null;

            using (dbFGLEntities db = new dbFGLEntities())
            {
                List<FGL_ITEM_CODE_TRAN> dataList = db.FGL_ITEM_CODE_TRAN
                    .Where(x =>
                        p1.Contains(x.product_no.Substring(0, 1))
                      & p2.Contains(x.product_no.Substring(1, 1))
                      & p3.Contains(x.product_no.Substring(2, 1))
                      & p4.Contains(x.product_no.Substring(3, 1))
                      & p5.Contains(x.product_no.Substring(4, 1))
                      & p6.Contains(x.product_no.Substring(5, 1))
                      & p7.Contains(x.product_no.Substring(6, 1))
                      & p8.Contains(x.product_no.Substring(7, 1))
                      & p9.Contains(x.product_no.Substring(8, 1))
                      & p10.Contains(x.product_no.Substring(9, 1))
                      & p11.Contains(x.product_no.Substring(10, 1))
                      & p12.Contains(x.product_no.Substring(11, 1))
                      & p13.Contains(x.product_no.Substring(12, 1))
                      & p14.Contains(x.product_no.Substring(13, 1))
                      & p15.Contains(x.product_no.Substring(14, 1))
                      & p16.Contains(x.product_no.Substring(15, 1))
                      & p17.Contains(x.product_no.Substring(16, 1))
                      & p18.Contains(x.product_no.Substring(17, 1))
                      & p19.Contains(x.product_no.Substring(18, 1))
                      & p20.Contains(x.product_no.Substring(19, 1))
                      & p21.Contains(x.product_no.Substring(20, 1))
                      & p22.Contains(x.product_no.Substring(21, 1))
                      & p23.Contains(x.product_no.Substring(22, 1))
                      & p24.Contains(x.product_no.Substring(23, 1))
                      & p25.Contains(x.product_no.Substring(24, 1))
                      & p26.Contains(x.product_no.Substring(25, 1))
                      & p27.Contains(x.product_no.Substring(26, 1))
                    ).OrderByDescending(x => x.approve_datetime).ToList();


                
                //20190802 修改為取覆核時間最後的資料(嵐婷)
                if (dataList != null) {
                    if (dataList.Count > 0)
                        d = dataList[0];
                }


                ////比對哪一筆符合的最多，就回傳哪一筆
                //int iMatchMax = 0;
                //foreach (FGL_ITEM_CODE_TRAN temp in dataList)
                //{
                //    int iMatch = 0;
                //    string codeProcNo = temp.product_no.PadRight(27, ' ');
                //    for (int i = 0; i < 27; i++) {
                //        if (codeProcNo.Substring(i, 1).Equals(productNo.Substring(i, 1)))
                //            iMatch++;
                //    }

                //    if (iMatch > iMatchMax) {
                //        iMatchMax = iMatch;
                //        d = temp;
                //    }
                //}
            }


            return d;

        }



        public List<OGL00002Model> qryByTranCode(OGL00002Model d)
        {
            bool bTranA = StringUtil.isEmpty(d.tranA);
            bool bTranB = StringUtil.isEmpty(d.tranB);
            bool bTranC = StringUtil.isEmpty(d.tranC);
            bool bTranD = StringUtil.isEmpty(d.tranD);
            bool bTranE = StringUtil.isEmpty(d.tranE);
            bool bTranF = StringUtil.isEmpty(d.tranF);
            bool bTranG = StringUtil.isEmpty(d.tranG);
            bool bTranH = StringUtil.isEmpty(d.tranH);
            bool bTranI = StringUtil.isEmpty(d.tranI);
            bool bTranJ = StringUtil.isEmpty(d.tranJ);
            bool bTranK = StringUtil.isEmpty(d.tranK);

            using (new TransactionScope(
              TransactionScopeOption.Required,
              new TransactionOptions
              {
                  IsolationLevel = IsolationLevel.ReadUncommitted
              }))
            {
                using (dbFGLEntities db = new dbFGLEntities())
                {
                    var his = (from  m in db.FGL_ITEM_CODE_TRAN
                               join codeStatus in db.SYS_CODE.Where(x => x.SYS_CD == "GL" & x.CODE_TYPE == "DATA_STATUS") on m.data_status equals codeStatus.CODE into psStatus
                               from xStatus in psStatus.DefaultIfEmpty()

                               //join codeAction in db.SYS_CODE.Where(x => x.SYS_CD == "GL" & x.CODE_TYPE == "EXEC_ACTION") on m.EXEC_ACTION equals codeAction.CODE into psAction
                               //from xAction in psAction.DefaultIfEmpty()
                               where 1 == 1
                               & (bTranA || m.tran_a == d.tranA)
                               & (bTranB || m.tran_b == d.tranB)
                               & (bTranC || m.tran_c == d.tranC)
                               & (bTranD || m.tran_d == d.tranD)
                               & (bTranE || m.tran_e == d.tranE)
                               & (bTranF || m.tran_f == d.tranF)
                               & (bTranG || m.tran_g == d.tranG)
                               & (bTranH || m.tran_h == d.tranH)
                               & (bTranI || m.tran_i == d.tranI)
                               & (bTranJ || m.tran_j == d.tranJ)
                               & (bTranK || m.tran_k == d.tranK)
                               select new OGL00002Model
                               {
                                   tempId = m.product_no,
                                   productNo = m.product_no,
                                   tranA = m.tran_a,
                                   tranB = m.tran_b,
                                   tranC = m.tran_c,
                                   tranD = m.tran_d,
                                   tranE = m.tran_e,
                                   tranF = m.tran_f,
                                   tranG = m.tran_g,
                                   tranH = m.tran_h,
                                   tranI = m.tran_i,
                                   tranJ = m.tran_j,
                                   tranK = m.tran_k,

                                   //execAction = (xHis == null ? String.Empty : xHis.EXEC_ACTION),
                                   //execActionDesc = (xAction == null ? String.Empty : xAction.CODE_VALUE),

                                   dataStatus = m.data_status.Trim(),
                                   dataStatusDesc = (xStatus == null ? String.Empty : xStatus.CODE_VALUE),

                                   apprId = m.appr_id,
                                   apprDt = m.approve_datetime == null ? "" : SqlFunctions.DateName("year", m.approve_datetime) + "/" +
                                                                         SqlFunctions.DatePart("m", m.approve_datetime) + "/" +
                                                                         SqlFunctions.DateName("day", m.approve_datetime).Trim() + " " +
                                                                         SqlFunctions.DateName("hh", m.approve_datetime).Trim() + ":" +
                                                                         SqlFunctions.DateName("n", m.approve_datetime).Trim() + ":" +
                                                                         SqlFunctions.DateName("s", m.approve_datetime).Trim(),

                                   updateId = m.update_id,
                                   updateDatetime = m.update_datetime == null ? "" : SqlFunctions.DateName("year", m.update_datetime) + "/" +
                                                                         SqlFunctions.DatePart("m", m.update_datetime) + "/" +
                                                                         SqlFunctions.DateName("day", m.update_datetime).Trim() + " " +
                                                                         SqlFunctions.DateName("hh", m.update_datetime).Trim() + ":" +
                                                                         SqlFunctions.DateName("n", m.update_datetime).Trim() + ":" +
                                                                         SqlFunctions.DateName("s", m.update_datetime).Trim()
                               }).Distinct().ToList<OGL00002Model>();

                    return his;
                }
            }
        }

        public FGL_ITEM_CODE_TRAN qryByKey(string productNo)
        {
            using (dbFGLEntities db = new dbFGLEntities())
            {
                FGL_ITEM_CODE_TRAN d = db.FGL_ITEM_CODE_TRAN
                    .Where(x => x.product_no == productNo)
                    .FirstOrDefault<FGL_ITEM_CODE_TRAN>();

                return d;
            }
            
        }


        public int updateStatus(string dataStatus, FGL_ITEM_CODE_TRAN d, SqlConnection conn, SqlTransaction transaction)
        {
            string sql = @"update FGL_ITEM_CODE_TRAN
        set DATA_STATUS = @DATA_STATUS
           ,UPDATE_ID = @UPDATE_ID
           ,UPDATE_DATETIME = @UPDATE_DATETIME
           ,APPR_ID = @APPR_ID
           ,APPROVE_DATETIME = @APPROVE_DATETIME
        where 1=1
        and PRODUCT_NO = @PRODUCT_NO
        ";


            SqlCommand command = conn.CreateCommand();


            command.Connection = conn;
            command.Transaction = transaction;

            try
            {
                command.CommandText = sql;
                command.Parameters.AddWithValue("@PRODUCT_NO", d.product_no.PadRight(27, ' '));

                command.Parameters.AddWithValue("@DATA_STATUS", StringUtil.toString(dataStatus));

                command.Parameters.AddWithValue("@UPDATE_ID", StringUtil.toString(d.update_id));
                command.Parameters.Add("@UPDATE_DATETIME", System.Data.SqlDbType.DateTime).Value = (System.Object)d.update_datetime ?? System.DBNull.Value;

                command.Parameters.AddWithValue("@APPR_ID", StringUtil.toString(d.appr_id));
                command.Parameters.Add("@APPROVE_DATETIME", System.Data.SqlDbType.DateTime).Value = (System.Object)d.approve_datetime ?? System.DBNull.Value;



                int cnt = command.ExecuteNonQuery();


                return cnt;
            }
            catch (Exception e)
            {

                throw e;
            }

        }



        public void insert(FGL_ITEM_CODE_TRAN d, SqlConnection conn, SqlTransaction transaction)
        {

            try
            {

                string sql = @"INSERT INTO FGL_ITEM_CODE_TRAN
                   ([PRODUCT_NO]
                   ,[TRAN_A]
                   ,[TRAN_B]
                   ,[TRAN_C]
                   ,[TRAN_D]
                   ,[TRAN_E]
                   ,[TRAN_F]
                   ,[TRAN_G]
                   ,[TRAN_H]
                   ,[TRAN_I]
                   ,[TRAN_J]
                   ,[TRAN_K]
                   ,[DATA_STATUS]
                   ,[UPDATE_ID]
                   ,[UPDATE_DATETIME]
                   ,[APPR_ID]
                   ,[APPROVE_DATETIME])

             VALUES
                  (@PRODUCT_NO
                   ,@TRAN_A
                   ,@TRAN_B
                   ,@TRAN_C
                   ,@TRAN_D
                   ,@TRAN_E
                   ,@TRAN_F
                   ,@TRAN_G
                   ,@TRAN_H
                   ,@TRAN_I
                   ,@TRAN_J
                   ,@TRAN_K
                   ,@DATA_STATUS
                   ,@UPDATE_ID
                   ,@UPDATE_DATETIME
                   ,@APPR_ID
                   ,@APPROVE_DATETIME)";

                SqlCommand cmd = conn.CreateCommand();

                cmd.Connection = conn;
                cmd.Transaction = transaction;

                cmd.CommandText = sql;


                cmd.Parameters.AddWithValue("@PRODUCT_NO", d.product_no.PadRight(27, ' '));
                cmd.Parameters.AddWithValue("@TRAN_A", StringUtil.toString(d.tran_a));
                cmd.Parameters.AddWithValue("@TRAN_B", StringUtil.toString(d.tran_b));
                cmd.Parameters.AddWithValue("@TRAN_C", StringUtil.toString(d.tran_c));
                cmd.Parameters.AddWithValue("@TRAN_D", StringUtil.toString(d.tran_d));
                cmd.Parameters.AddWithValue("@TRAN_E", StringUtil.toString(d.tran_e));
                cmd.Parameters.AddWithValue("@TRAN_F", StringUtil.toString(d.tran_f));
                cmd.Parameters.AddWithValue("@TRAN_G", StringUtil.toString(d.tran_g));
                cmd.Parameters.AddWithValue("@TRAN_H", StringUtil.toString(d.tran_h));
                cmd.Parameters.AddWithValue("@TRAN_I", StringUtil.toString(d.tran_i));
                cmd.Parameters.AddWithValue("@TRAN_J", StringUtil.toString(d.tran_j));
                cmd.Parameters.AddWithValue("@TRAN_K", StringUtil.toString(d.tran_k));
                cmd.Parameters.AddWithValue("@DATA_STATUS", StringUtil.toString(d.data_status));
                cmd.Parameters.AddWithValue("@UPDATE_ID", StringUtil.toString(d.update_id));
                cmd.Parameters.Add("@UPDATE_DATETIME", System.Data.SqlDbType.DateTime).Value = (System.Object)d.update_datetime ?? System.DBNull.Value;
                cmd.Parameters.AddWithValue("@APPR_ID", StringUtil.toString(d.appr_id));
                cmd.Parameters.Add("@APPROVE_DATETIME", System.Data.SqlDbType.DateTime).Value = (System.Object)d.approve_datetime ?? System.DBNull.Value;


                int cnt = cmd.ExecuteNonQuery();


            }
            catch (Exception e)
            {

                throw e;
            }

        }




        public void update(FGL_ITEM_CODE_TRAN d, SqlConnection conn, SqlTransaction transaction)
        {

            try
            {

                string sql = @"update FGL_ITEM_CODE_TRAN
                set [TRAN_A] = @TRAN_A
                   ,[TRAN_B] = @TRAN_B
                   ,[TRAN_C] = @TRAN_C
                   ,[TRAN_D] = @TRAN_D
                   ,[TRAN_E] = @TRAN_E
                   ,[TRAN_F] = @TRAN_F
                   ,[TRAN_G] = @TRAN_G
                   ,[TRAN_H] = @TRAN_H
                   ,[TRAN_I] = @TRAN_I
                   ,[TRAN_J] = @TRAN_J
                   ,[TRAN_K] = @TRAN_K
                   ,[DATA_STATUS] = @DATA_STATUS
                   ,[UPDATE_ID] = @UPDATE_ID
                   ,[UPDATE_DATETIME] = @UPDATE_DATETIME
                   ,[APPR_ID] = @APPR_ID
                   ,[APPROVE_DATETIME] = @APPROVE_DATETIME
        where 1=1
        and PRODUCT_NO = @PRODUCT_NO
";

                SqlCommand cmd = conn.CreateCommand();

                cmd.Connection = conn;
                cmd.Transaction = transaction;

                cmd.CommandText = sql;


                cmd.Parameters.AddWithValue("@PRODUCT_NO", d.product_no.PadRight(27, ' '));
                cmd.Parameters.AddWithValue("@TRAN_A", StringUtil.toString(d.tran_a));
                cmd.Parameters.AddWithValue("@TRAN_B", StringUtil.toString(d.tran_b));
                cmd.Parameters.AddWithValue("@TRAN_C", StringUtil.toString(d.tran_c));
                cmd.Parameters.AddWithValue("@TRAN_D", StringUtil.toString(d.tran_d));
                cmd.Parameters.AddWithValue("@TRAN_E", StringUtil.toString(d.tran_e));
                cmd.Parameters.AddWithValue("@TRAN_F", StringUtil.toString(d.tran_f));
                cmd.Parameters.AddWithValue("@TRAN_G", StringUtil.toString(d.tran_g));
                cmd.Parameters.AddWithValue("@TRAN_H", StringUtil.toString(d.tran_h));
                cmd.Parameters.AddWithValue("@TRAN_I", StringUtil.toString(d.tran_i));
                cmd.Parameters.AddWithValue("@TRAN_J", StringUtil.toString(d.tran_j));
                cmd.Parameters.AddWithValue("@TRAN_K", StringUtil.toString(d.tran_k));
                cmd.Parameters.AddWithValue("@DATA_STATUS", StringUtil.toString(d.data_status));
                cmd.Parameters.AddWithValue("@UPDATE_ID", StringUtil.toString(d.update_id));
                cmd.Parameters.Add("@UPDATE_DATETIME", System.Data.SqlDbType.DateTime).Value = (System.Object)d.update_datetime ?? System.DBNull.Value;
                cmd.Parameters.AddWithValue("@APPR_ID", StringUtil.toString(d.appr_id));
                cmd.Parameters.Add("@APPROVE_DATETIME", System.Data.SqlDbType.DateTime).Value = (System.Object)d.approve_datetime ?? System.DBNull.Value;


                int cnt = cmd.ExecuteNonQuery();


            }
            catch (Exception e)
            {

                throw e;
            }

        }



        public void delete(FGL_ITEM_CODE_TRAN d, SqlConnection conn, SqlTransaction transaction)
        {

            try
            {

                string sql = @"delete FGL_ITEM_CODE_TRAN
        where 1=1
        and PRODUCT_NO = @PRODUCT_NO
";

                SqlCommand cmd = conn.CreateCommand();

                cmd.Connection = conn;
                cmd.Transaction = transaction;

                cmd.CommandText = sql;

                cmd.Parameters.AddWithValue("@PRODUCT_NO", d.product_no.PadRight(27, ' '));

                int cnt = cmd.ExecuteNonQuery();


            }
            catch (Exception e)
            {

                throw e;
            }

        }

    }
}