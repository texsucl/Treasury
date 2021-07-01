
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
    public class FGLGitmHisDao
    {

        /// <summary>
        /// OGL00009 商品年期及躉繳商品設定作業
        /// </summary>
        /// <param name="aply_no"></param>
        /// <param name="appr_stat"></param>
        /// <param name="item_type"></param>
        /// <param name="sys_type"></param>
        /// <param name="item"></param>
        /// <param name="exec_action"></param>
        /// <returns></returns>
        public List<OGL00009Model> qryForOGL00009(string aply_no, string appr_stat, string item_type, string sys_type, string item, string exec_action)
        {
            bool bApprStat = StringUtil.isEmpty(appr_stat);
            bool bAplyNo = StringUtil.isEmpty(aply_no);
            bool bItemType = StringUtil.isEmpty(item_type);
            bool bSysType = StringUtil.isEmpty(sys_type);
            bool bItem = StringUtil.isEmpty(item);
            bool bExecAction = StringUtil.isEmpty(exec_action);

            switch (sys_type)
            {
                case "1":
                    sys_type = "A";
                    break;
                case "2":
                    sys_type = "F";
                    break;
                case "3":
                    sys_type = "";
                    break;
            }


            List<OGL00009Model> rows = new List<OGL00009Model>();
            using (new TransactionScope(
                  TransactionScopeOption.Required,
                  new TransactionOptions
                  {
                      IsolationLevel = IsolationLevel.ReadUncommitted
                  }))
            {
                using (dbFGLEntities db = new dbFGLEntities())
                {
                    rows = (from main in db.FGL_GITM_HIS
                                where 1 == 1
                                    & (bApprStat || (!bApprStat & (main.appr_stat == appr_stat)))
                                    & (bAplyNo || (!bAplyNo & (main.aply_no == aply_no)))
                                    & (bItemType || (!bItemType & (main.item_type == item_type)))
                                    & (bSysType || (!bSysType & (main.sys_type == sys_type)))
                                    & (bItem || (!bItem & (main.item == item)))
                                    & (bExecAction || (!bExecAction & (main.exec_action == exec_action)))
                            select new OGL00009Model
                                {

                                    aply_no = main.aply_no.ToString(),
                                    exec_action = main.exec_action,
                                    //tempId = main.item_type + main.sys_type + main.item,
                                    item_type = main.item_type,
                                    sys_type = main.sys_type,
                                    item = main.item,
                                    year = main.year.ToString(),
                                age = main.age.ToString(),
                                prem_y_tp = main.prem_y_tp,
                                    item_type_n = main.item_type_n,
                                    sys_type_n = main.sys_type_n,
                                    item_n = main.item_n,
                                    update_id = main.update_id,
                                    update_datetime = main.update_datetime == null ? "" : SqlFunctions.DateName("year", main.update_datetime) + "/" +
                                                    SqlFunctions.DatePart("m", main.update_datetime) + "/" +
                                                    SqlFunctions.DateName("day", main.update_datetime).Trim() + " " +
                                                    SqlFunctions.DateName("hh", main.update_datetime).Trim() + ":" +
                                                    SqlFunctions.DateName("n", main.update_datetime).Trim() + ":" +
                                                    SqlFunctions.DateName("s", main.update_datetime).Trim()
                                }).ToList<OGL00009Model>();

                    

                }
            }

            return rows;
        }



        /// <summary>
        /// 新增
        /// </summary>
        /// <param name="his"></param>
        /// <param name="conn"></param>
        /// <param name="transaction"></param>
        public void insert(string aply_no, List<FGL_GITM_HIS> dataList)
        {
            string strConn = DbUtil.GetDBFglConnStr();
            using (SqlConnection conn = new SqlConnection(strConn))
            {
                conn.Open();
                SqlTransaction transaction = conn.BeginTransaction("Transaction");
                try
                {
                    string sql = @"INSERT INTO FGL_GITM_HIS
( [aply_no]
 ,[item_type]
 ,[item]
 ,[sys_type]
 ,[year]
 ,[prem_y_tp]
 ,[age]
 ,[exec_action]
 ,[item_type_n]
 ,[item_n]
 ,[sys_type_n]
 ,[update_id]
 ,[update_datetime]
 ,[appr_stat]
) VALUES (
  @aply_no
 ,@item_type
 ,@item
 ,@sys_type
 ,@year
 ,@prem_y_tp
 ,@age
 ,@exec_action
 ,@item_type_n
 ,@item_n
 ,@sys_type_n
 ,@update_id
 ,@update_datetime
 ,@appr_stat)";

                    SqlCommand cmd = conn.CreateCommand();

                    cmd.Connection = conn;
                    cmd.Transaction = transaction;

                    cmd.CommandText = sql;

                    foreach (FGL_GITM_HIS his in dataList)
                    {
                        var sys_type = "";
                        var sys_type_n = "";
                        switch (his.sys_type)
                        {
                            case "1":
                                sys_type = "A";
                                break;
                            case "2":
                                sys_type = "F";
                                break;
                            case "3":
                                sys_type = "";
                                break;
                            default:
                                sys_type = his.sys_type;
                                break;
                        }

                        switch (his.sys_type_n)
                        {
                            case "1":
                                sys_type_n = "A";
                                break;
                            case "2":
                                sys_type_n = "F";
                                break;
                            case "3":
                                sys_type_n = "";
                                break;
                            default:
                                sys_type_n = his.sys_type_n;
                                break;
                        }


                        cmd.Parameters.Clear();
                        cmd.Parameters.AddWithValue("@aply_no", StringUtil.toString(aply_no));
                        cmd.Parameters.AddWithValue("@item_type", StringUtil.toString(his.item_type));
                        cmd.Parameters.AddWithValue("@item", StringUtil.toString(his.item));
                        cmd.Parameters.AddWithValue("@sys_type", StringUtil.toString(sys_type));
                        cmd.Parameters.AddWithValue("@year", System.Data.SqlDbType.Int).Value = (System.Object)his.year ?? System.DBNull.Value;
                        cmd.Parameters.AddWithValue("@prem_y_tp", StringUtil.toString(his.prem_y_tp));
                        cmd.Parameters.AddWithValue("@age", System.Data.SqlDbType.Int).Value = (System.Object)his.age ?? System.DBNull.Value;
                        cmd.Parameters.AddWithValue("@exec_action", StringUtil.toString(his.exec_action));
                        cmd.Parameters.AddWithValue("@item_type_n", StringUtil.toString(his.item_type_n));
                        cmd.Parameters.AddWithValue("@item_n", StringUtil.toString(his.item_n));
                        cmd.Parameters.AddWithValue("@sys_type_n", StringUtil.toString(sys_type_n));
                        cmd.Parameters.AddWithValue("@update_id", StringUtil.toString(his.update_id));
                        cmd.Parameters.Add("@update_datetime", System.Data.SqlDbType.DateTime).Value = (System.Object)his.update_datetime ?? System.DBNull.Value;
                        cmd.Parameters.AddWithValue("@appr_stat", StringUtil.toString(his.appr_stat));

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




        /// <summary>
        /// 異動覆核狀態
        /// </summary>
        /// <param name="appr_id"></param>
        /// <param name="appr_stat"></param>
        /// <param name="procData"></param>
        /// <param name="conn"></param>
        /// <param name="transaction"></param>
        /// <returns></returns>
        public bool updateHis(string appr_id, string appr_stat, FGL_GITM_HIS procData, SqlConnection conn, SqlTransaction transaction)
        {
            bool execResult = true;
            string[] nowStr = DateUtil.getCurChtDateTime(4).Split(' ');
            try
            {
                string sql = @"
UPDATE FGL_GITM_HIS
  SET APPR_STAT = @APPR_STAT
     ,APPR_ID = @APPR_ID 
     ,APPR_DATETIME = @APPR_DATETIME 
 WHERE  1 = 1
  AND APLY_NO = @APLY_NO 
  AND ITEM_TYPE = @ITEM_TYPE 
  AND SYS_TYPE = @SYS_TYPE
  AND ITEM = @ITEM ";

                SqlCommand cmd = conn.CreateCommand();
                cmd.Connection = conn;
                cmd.Transaction = transaction;
                cmd.CommandText = sql;

                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("@APPR_STAT", appr_stat);
                cmd.Parameters.AddWithValue("@APPR_ID", appr_id);
                cmd.Parameters.AddWithValue("@APPR_DATETIME", DateTime.Now);
                cmd.Parameters.AddWithValue("@APLY_NO", procData.aply_no);
                cmd.Parameters.AddWithValue("@ITEM_TYPE", procData.item_type);
                cmd.Parameters.AddWithValue("@SYS_TYPE", StringUtil.toString(procData.sys_type));
                cmd.Parameters.AddWithValue("@ITEM", StringUtil.toString(procData.item));

                cmd.ExecuteNonQuery();


            }
            catch (Exception e)
            {
                execResult = false;
                throw e;
            }


            return execResult;
        }


    }
}