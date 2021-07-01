
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using FAP.Web.Models;
using System.Data.Entity.SqlServer;
using System.Transactions;
using FAP.Web.BO;
using FAP.Web.ViewModels;

/// <summary>
/// 功能說明：FAP_VE_CODE_HIS 逾期未兌領代碼設定暫存檔
/// 初版作者：20190612 Daiyu
/// 修改歷程：20190612 Daiyu
///           需求單號：
///           初版
/// </summary>
/// 

namespace FAP.Web.Daos
{
    public class FAPVeCodeHisDao
    {

        /// <summary>
        /// 以"群組代碼 + 代碼" 查覆核中的資料
        /// </summary>
        /// <param name="code_type"></param>
        /// <param name="code_id"></param>
        /// <returns></returns>
        public FAP_VE_CODE_HIS qryInProssById(string code_type, string code_id, string aply_no)
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
                    FAP_VE_CODE_HIS row = db.FAP_VE_CODE_HIS
                        .Where(x => x.code_type == code_type
                                & (code_id == "" || (code_id != "" & x.code_id == code_id))
                                & (aply_no == "" || (aply_no != "" & x.aply_no == aply_no))
                                & x.appr_stat == "1").FirstOrDefault();

                    if (row != null)
                        return row;
                    else
                        return new FAP_VE_CODE_HIS();
                }
            }
        }


        public List<VeTraceModel> qryInProssByGrp(string[] code_type)
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
                    var his = (from m in db.FAP_VE_CODE_HIS
                               join cAction in db.SYS_CODE.Where(x => x.SYS_CD == "AP" & x.CODE_TYPE == "EXEC_ACTION") on m.exec_action equals cAction.CODE into psAction
                               from xActions in psAction.DefaultIfEmpty()

                               where 1 == 1
                               & code_type.Contains(m.code_type)
                               & m.appr_stat == "1"

                               select new VeTraceModel
                               {
                                   aply_no = m.aply_no,
                                   code_type = m.code_type,
                                   code_id = m.code_id,
                                   code_value = m.code_value,
                                   remark = m.remark,
                                   exec_action = xActions == null ? m.exec_action : xActions.CODE_VALUE.Trim(),
                                   update_id = m.update_id,
                                   update_datetime = m.update_datetime == null ? "" : SqlFunctions.DateName("year", m.update_datetime) + "/" +
                                                 SqlFunctions.DatePart("m", m.update_datetime) + "/" +
                                                 SqlFunctions.DateName("day", m.update_datetime).Trim() + " " +
                                                 SqlFunctions.DateName("hh", m.update_datetime).Trim() + ":" +
                                                 SqlFunctions.DateName("n", m.update_datetime).Trim() + ":" +
                                                 SqlFunctions.DateName("s", m.update_datetime).Trim()

                               }).Distinct().ToList<VeTraceModel>();

                    return his;
                }
            }
        }



        /// <summary>
        /// 新增覆核資料
        /// </summary>
        /// <param name="aply_no"></param>
        /// <param name="dataList"></param>
        /// <param name="conn"></param>
        /// <param name="transaction"></param>
        public void insert(string aply_no, DateTime dt, List<VeTraceModel> dataList, SqlConnection conn, SqlTransaction transaction)
        {

            try
            {

                string sql = @"
        INSERT INTO FAP_VE_CODE_HIS
           (aply_no
           ,code_type
           ,code_id
           ,code_value
           ,remark
           ,exec_action
           ,appr_stat
           ,update_id
           ,update_datetime)
             VALUES
           (@aply_no
           ,@code_type
           ,@code_id
           ,@code_value
           ,@remark
           ,@exec_action
           ,@appr_stat
           ,@update_id
           ,@update_datetime)";

                SqlCommand cmd = conn.CreateCommand();

                cmd.Connection = conn;
                cmd.Transaction = transaction;

                cmd.CommandText = sql;


                foreach (VeTraceModel d in dataList)
                {
                    cmd.Parameters.Clear();

                    cmd.Parameters.AddWithValue("@aply_no", StringUtil.toString(aply_no));
                    cmd.Parameters.AddWithValue("@code_type", StringUtil.toString(d.code_type));
                    cmd.Parameters.AddWithValue("@code_id", StringUtil.toString(d.code_id));
                    cmd.Parameters.AddWithValue("@code_value", StringUtil.toString(d.code_value));
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
        /// 異動覆核結果
        /// </summary>
        /// <param name="appr_mk"></param>
        /// <param name="dt"></param>
        /// <param name="d"></param>
        /// <param name="conn"></param>
        /// <param name="transaction"></param>
        public void updateApprMk(string appr_stat, DateTime dt, List<VeTraceModel> dataList, SqlConnection conn, SqlTransaction transaction)
        {

            try
            {

                string sql = @"
UPDATE FAP_VE_CODE_HIS
  SET appr_stat = @appr_stat
     ,appr_id = @appr_id
     ,approve_datetime = @approve_datetime
WHERE aply_no = @aply_no
 AND code_type = @code_type
 AND code_id = @code_id";


                SqlCommand cmd = conn.CreateCommand();

                cmd.Connection = conn;
                cmd.Transaction = transaction;

                cmd.CommandText = sql;

                foreach (VeTraceModel d in dataList)
                {
                    cmd.Parameters.Clear();
                    cmd.Parameters.Clear();

                    cmd.Parameters.AddWithValue("@appr_stat", appr_stat);
                    cmd.Parameters.AddWithValue("@appr_id", d.appr_id);
                    cmd.Parameters.AddWithValue("@approve_datetime", dt);
                    cmd.Parameters.AddWithValue("@aply_no", StringUtil.toString(d.aply_no));
                    cmd.Parameters.AddWithValue("@code_type", StringUtil.toString(d.code_type));
                    cmd.Parameters.AddWithValue("@code_id", StringUtil.toString(d.code_id));



                    cmd.ExecuteNonQuery();
                }

            }
            catch (Exception e)
            {
                throw e;
            }
        }

    }
}