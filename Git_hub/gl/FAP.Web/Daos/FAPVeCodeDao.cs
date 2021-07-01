
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
/// 功能說明：FAP_VE_CODE 逾期未兌領代碼設定檔
/// 初版作者：20190612 Daiyu
/// 修改歷程：20190612 Daiyu
///           需求單號：
///           初版
/// --------------------------------------------------
///          20200729
///          需求單號：
/// </summary>
/// 

namespace FAP.Web.Daos
{
    public class FAPVeCodeDao
    {

        public Dictionary<string, string> qryByTypeDic(string cType)
        {
            dbFGLEntities context = new dbFGLEntities();

            var result1 = (from code in context.FAP_VE_CODE
                           where code.code_type == cType
                           select new
                           {
                               CCODE = code.code_id.Trim(),
                               CVALUE = code.code_value.Trim()
                           }
                           ).OrderBy(x => x.CCODE).ToDictionary(x => x.CCODE, x => x.CVALUE);


            return result1;
        }



        public FAP_VE_CODE qryByKey(string group_code, string code_id)
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
                    FAP_VE_CODE row = db.FAP_VE_CODE.Where(x => x.code_type == group_code & x.code_id == code_id).FirstOrDefault();

                    if (row != null)
                        return row;
                    else
                        return new FAP_VE_CODE();
                }
            }
        }


        public List<FAP_VE_CODE> qryByGrp(string code_type)
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
                    List <FAP_VE_CODE> rows = db.FAP_VE_CODE.Where(x => x.code_type == code_type)
                        .OrderBy(x => x.code_id.Length).ThenBy(x => x.code_id).ToList();

                    return rows;
                }
            }
        }


        /// <summary>
        /// 畫面下拉選單
        /// </summary>
        /// <param name="code_type"></param>
        /// <param name="bPreCode"></param>
        /// <returns></returns>
        public SelectList loadSelectList(string code_type, bool bPreCode)
        {
            dbFGLEntities context = new dbFGLEntities();


            var result1 = (from code in context.FAP_VE_CODE
                           where code.code_type == code_type
                           orderby code.code_id.Length, code.code_id
                           select new
                           {
                               CCODE = code.code_id.Trim(),
                               CVALUE = bPreCode ? code.code_id.Trim() + "." + code.code_value.Trim() : code.code_value.Trim()
                           }
                           );

            var items = new SelectList
                (
                items: result1,
                dataValueField: "CCODE",
                dataTextField: "CVALUE",
                selectedValue: (object)null
                );

            return items;
        }


        /// <summary>
        /// for GRID下拉選單
        /// </summary>
        /// <param name="code_type"></param>
        /// <param name="bPreCode"></param>
        /// <returns></returns>
        public string jqGridList(string code_type, bool bPreCode)
        {
            var codeList = loadSelectList(code_type, bPreCode);
            string controlStr = "";
            foreach (var item in codeList)
            {
                controlStr += item.Value.Trim() + ":" + item.Text.Trim() + ";";
            }
            controlStr = controlStr.Substring(0, controlStr.Length - 1) + "";
            return controlStr;
        }



        /// <summary>
        /// 將覆核結果為核可的資料回寫到正式檔
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="dataList"></param>
        /// <param name="conn"></param>
        /// <param name="transaction"></param>
        public void procAppr(string apprMk, DateTime dt, List<VeTraceModel> dataList, SqlConnection conn, SqlTransaction transaction) {
            FAPVeCodeHisDao fAPVeCodeHisDao = new FAPVeCodeHisDao();

            foreach (VeTraceModel d in dataList) {
                if ("3".Equals(apprMk))
                    updateStatus(apprMk, Convert.ToDateTime(d.update_datetime), d, conn, transaction);
                else {
                    FAP_VE_CODE_HIS his = fAPVeCodeHisDao.qryInProssById(d.code_type, d.code_id, d.aply_no);
                    his.appr_id = d.appr_id;

                    if (!"".Equals(his.exec_action)) {
                        switch (his.exec_action)
                        {
                            case "A":
                                insert(dt, his, conn, transaction);
                                break;
                            case "U":
                                update(dt, his, conn, transaction);
                                break;
                            case "D":
                                delete(his, conn, transaction);
                                break;
                        }
                    }
                    
                }
            }
        }


        public void insert(DateTime dt, FAP_VE_CODE_HIS d, SqlConnection conn, SqlTransaction transaction)
        {

            try
            {

                string sql = @"
        INSERT INTO FAP_VE_CODE
           (code_type
           ,code_id
           ,code_value
           ,remark
           ,data_status
           ,update_id
           ,update_datetime
           ,appr_id
           ,approve_datetime)
             VALUES
           (@code_type
           ,@code_id
           ,@code_value
           ,@remark
           ,@data_status
           ,@update_id
           ,@update_datetime
           ,@appr_id
           ,@approve_datetime)";

                SqlCommand cmd = conn.CreateCommand();

                cmd.Connection = conn;
                cmd.Transaction = transaction;

                cmd.CommandText = sql;

                cmd.Parameters.Clear();

                cmd.Parameters.AddWithValue("@code_type", StringUtil.toString(d.code_type));
                cmd.Parameters.AddWithValue("@code_id", StringUtil.toString(d.code_id));
                cmd.Parameters.AddWithValue("@code_value", StringUtil.toString(d.code_value));
                cmd.Parameters.AddWithValue("@remark", StringUtil.toString(d.remark));
                cmd.Parameters.AddWithValue("@data_status", "1");
                cmd.Parameters.AddWithValue("@update_id", StringUtil.toString(d.update_id));
                cmd.Parameters.AddWithValue("@update_datetime", d.update_datetime);
                cmd.Parameters.AddWithValue("@appr_id", d.appr_id);
                cmd.Parameters.AddWithValue("@approve_datetime", dt);

                cmd.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                throw e;
            }
        }



        public void delete (FAP_VE_CODE_HIS d, SqlConnection conn, SqlTransaction transaction)
        {
            try
            {

                string sql = @"
DELETE FAP_VE_CODE
 WHERE code_type = @code_type
   AND code_id = @code_id";

                SqlCommand cmd = conn.CreateCommand();

                cmd.Connection = conn;
                cmd.Transaction = transaction;

                cmd.CommandText = sql;

                cmd.Parameters.Clear();

                cmd.Parameters.AddWithValue("@code_type", StringUtil.toString(d.code_type));
                cmd.Parameters.AddWithValue("@code_id", StringUtil.toString(d.code_id));

                cmd.ExecuteNonQuery();

            }
            catch (Exception e)
            {
                throw e;
            }
        }


        public void update(DateTime dt, FAP_VE_CODE_HIS d, SqlConnection conn, SqlTransaction transaction)
        {

            try
            {

                string sql = @"
UPDATE FAP_VE_CODE
  SET code_value = @code_value
     ,remark = @remark
     ,data_status = @data_status
     ,appr_id = @appr_id
     ,approve_datetime = @approve_datetime
 WHERE code_type = @code_type
   AND code_id = @code_id";

                SqlCommand cmd = conn.CreateCommand();

                cmd.Connection = conn;
                cmd.Transaction = transaction;

                cmd.CommandText = sql;

                cmd.Parameters.Clear();

                cmd.Parameters.AddWithValue("@code_value", d.code_value);
                cmd.Parameters.AddWithValue("@remark", d.remark);
                cmd.Parameters.AddWithValue("@data_status", "1");
                cmd.Parameters.AddWithValue("@appr_id", d.appr_id);
                cmd.Parameters.AddWithValue("@approve_datetime", dt);
                cmd.Parameters.AddWithValue("@code_type", StringUtil.toString(d.code_type));
                cmd.Parameters.AddWithValue("@code_id", StringUtil.toString(d.code_id));

                cmd.ExecuteNonQuery();

            }
            catch (Exception e)
            {
                throw e;
            }
        }



        public void updateStatus(string appr_mk, DateTime dt, VeTraceModel d, SqlConnection conn, SqlTransaction transaction)
        {

            try
            {

                string sql = @"
UPDATE FAP_VE_CODE
  SET data_status = @data_status
     ,update_id = @update_id
     ,update_datetime = @update_datetime";
                sql += appr_mk == "2" ? ",appr_id = @appr_id ,approve_datetime = @approve_datetime" : "";
                sql += @" WHERE code_type = @code_type
                   AND code_id = @code_id";


                SqlCommand cmd = conn.CreateCommand();

                cmd.Connection = conn;
                cmd.Transaction = transaction;

                cmd.CommandText = sql;

                cmd.Parameters.Clear();

                cmd.Parameters.AddWithValue("@data_status", appr_mk == "1" ? "2" : "1");
                cmd.Parameters.AddWithValue("@update_id", d.update_id);
                cmd.Parameters.AddWithValue("@update_datetime", dt);

                if ("2".Equals(appr_mk)) {
                    cmd.Parameters.AddWithValue("@appr_id", d.appr_id);
                    cmd.Parameters.AddWithValue("@approve_datetime", dt);
                }

                cmd.Parameters.AddWithValue("@code_type", StringUtil.toString(d.code_type));
                cmd.Parameters.AddWithValue("@code_id", StringUtil.toString(d.code_id));



                cmd.ExecuteNonQuery();
                
            }
            catch (Exception e)
            {
                throw e;
            }
        }


        /// <summary>
        /// 依群組別異動資料狀態
        /// </summary>
        /// <param name="code_type"></param>
        /// <param name="update_id"></param>
        /// <param name="dt"></param>
        /// <param name="conn"></param>
        /// <param name="transaction"></param>
        public void updByType(string code_type, string  appr_mk, VeTraceModel d, DateTime dt, SqlConnection conn, SqlTransaction transaction)
        {

            try
            {

                string sql = @"
UPDATE FAP_VE_CODE
  SET data_status = @data_status
     ,update_id = @update_id
     ,update_datetime = @update_datetime";
                sql += appr_mk == "2" ? ",appr_id = @appr_id ,approve_datetime = @approve_datetime" : "";
                sql += @" WHERE code_type = @code_type";


                SqlCommand cmd = conn.CreateCommand();

                cmd.Connection = conn;
                cmd.Transaction = transaction;
                cmd.CommandText = sql;
                cmd.Parameters.Clear();

                cmd.Parameters.AddWithValue("@data_status", appr_mk == "1" ? "2" : "1");
                cmd.Parameters.AddWithValue("@update_id", d.update_id);
                cmd.Parameters.AddWithValue("@update_datetime", dt);


                if ("2".Equals(appr_mk))
                {
                    cmd.Parameters.AddWithValue("@appr_id", d.appr_id);
                    cmd.Parameters.AddWithValue("@approve_datetime", dt);
                }

                cmd.Parameters.AddWithValue("@code_type", StringUtil.toString(code_type));


                cmd.ExecuteNonQuery();

            }
            catch (Exception e)
            {
                throw e;
            }
        }



        /// <summary>
        /// 刪除指定群組的資料
        /// </summary>
        /// <param name="code_type"></param>
        /// <param name="conn"></param>
        /// <param name="transaction"></param>
        public void delByType(string code_type, SqlConnection conn, SqlTransaction transaction)
        {

            try
            {

                string sql = @"
DELETE FAP_VE_CODE
 WHERE code_type = @code_type";


                SqlCommand cmd = conn.CreateCommand();

                cmd.Connection = conn;
                cmd.Transaction = transaction;
                cmd.CommandText = sql;
                cmd.Parameters.Clear();

                cmd.Parameters.AddWithValue("@code_type", StringUtil.toString(code_type));

                cmd.ExecuteNonQuery();

            }
            catch (Exception e)
            {
                throw e;
            }
        }


    }
}