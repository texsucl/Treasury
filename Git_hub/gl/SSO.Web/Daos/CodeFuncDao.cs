using SSO.Web.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity.SqlServer;
using System.Data.SqlClient;
using System.Linq;
using System.Transactions;
using System.Web;
using SSO.Web.ViewModels;
using SSO.Web.Daos;
using SSO.Web.Utils;
using SSO.Web.BO;

namespace SSO.Web.Daos
{
    public class CodeFuncDao
    {
        /// <summary>
        /// 檢查功能名稱是否已存在
        /// </summary>
        /// <param name="cFunctionName"></param>
        /// <returns></returns>
        public String chkFuncName(string sysCd, string cFunctionId, string cFunctionName)
        {
            String id = "";

            using (new TransactionScope(
                   TransactionScopeOption.Required,
                   new TransactionOptions
                   {
                       IsolationLevel = IsolationLevel.ReadUncommitted
                   }))
            {

                using (dbFGLEntities db = new dbFGLEntities())
                {
                    id = db.CODE_FUNC.Where(x => x.SYS_CD == sysCd
                    && x.FUNC_NAME == cFunctionName.Trim() 
                    && x.FUNC_ID != cFunctionId)
                        .Select(x => x.FUNC_ID).FirstOrDefault();

                    return id;
                }
            }
        }


        /// <summary>
        /// 新增資料時，取得父功能目前最大的子功能編號+1
        /// </summary>
        /// <param name="cParentFunctionID"></param>
        /// <returns></returns>
        public int qrySort(string cParentFunctionID)
        {
            int sort = 0;
            using (new TransactionScope(
                   TransactionScopeOption.Required,
                   new TransactionOptions
                   {
                       IsolationLevel = IsolationLevel.ReadUncommitted　
                   }))
            {
                using (dbFGLEntities db = new dbFGLEntities())
                {
                    var query = db.CODE_FUNC.Where(x => x.PARENT_FUNC_ID == cParentFunctionID);
                    sort = query.Any() ? query.Max(o => o.FUNC_ORDER) : 0;
                    return sort + 1;
                }
            }
        }


        /// <summary>
        /// 新增資料時，取得"功能編號"
        /// </summary>
        /// <returns></returns>
        public String qryFuncId()
        {
            String cFunctionId = "";
            using (new TransactionScope(
                   TransactionScopeOption.Required,
                   new TransactionOptions
                   {
                       IsolationLevel = IsolationLevel.ReadUncommitted
                   }))
            {
                using (dbFGLEntities db = new dbFGLEntities())
                {
                    cFunctionId = db.CODE_FUNC.Max(x => x.FUNC_ID);
                    cFunctionId = (Convert.ToInt32(cFunctionId) + 1).ToString().PadLeft(10, '0');
                    return cFunctionId;
                }
            }
        }


        /// <summary>
        /// 查詢功能清單
        /// </summary>
        /// <returns></returns>
        public List<CODE_FUNC> qryDetail(string sysCd)
        {
            bool bsysCd = StringUtil.isEmpty(sysCd);

            List<CODE_FUNC> rows = new List<CODE_FUNC>();

            using (new TransactionScope(
                   TransactionScopeOption.Required,
                   new TransactionOptions
                   {
                       IsolationLevel = IsolationLevel.ReadUncommitted
                   }))
            {
                using (dbFGLEntities db = new dbFGLEntities())
                {
                    rows = db.CODE_FUNC.Where(x => bsysCd || x.SYS_CD == sysCd ).OrderBy(x => x.FUNC_ORDER).ToList<CODE_FUNC>();


                    return rows;
                }
            }
        }


        /// <summary>
        /// 取得特定系統別的根節點
        /// </summary>
        /// <param name="sysCd"></param>
        /// <returns></returns>
        public CODE_FUNC qryRootItem(string sysCd)
        {
            bool bsysCd = StringUtil.isEmpty(sysCd);

            CODE_FUNC item = new CODE_FUNC();

            using (new TransactionScope(
                   TransactionScopeOption.Required,
                   new TransactionOptions
                   {
                       IsolationLevel = IsolationLevel.ReadUncommitted
                   }))
            {
                using (dbFGLEntities db = new dbFGLEntities())
                {
                    var iLevel = db.CODE_FUNC.Where(x => x.SYS_CD == sysCd).Min(x => x.FUNC_LEVEL);

                    item = db.CODE_FUNC.Where(x => x.SYS_CD == sysCd & x.FUNC_LEVEL == iLevel).FirstOrDefault();

                    return item;
                }
            }
        }


        /// <summary>
        /// 以功能編號為鍵項，查詢功能詳細資訊
        /// </summary>
        /// <param name="cFuncId"></param>
        /// <returns></returns>
        public FuncMgrDetailModel qryByKey(String cFuncId)
        {
            FuncMgrDetailModel funcMgrDetail = new FuncMgrDetailModel();
            using (new TransactionScope(
                   TransactionScopeOption.Required,
                   new TransactionOptions
                   {
                       IsolationLevel = IsolationLevel.ReadUncommitted
                   }))
            {
                using (dbFGLEntities db = new dbFGLEntities())
                {
                    //codeFunction = db.CODEFUNCTION.Where(x => x.CFUNCTIONID == cFuncId).FirstOrDefault();

                    funcMgrDetail = (from main in db.CODE_FUNC
                                     join parent in db.CODE_FUNC on main.PARENT_FUNC_ID equals parent.FUNC_ID into psParent
                                     from xParent in psParent.DefaultIfEmpty()
                                     where main.FUNC_ID == cFuncId
                                     select new FuncMgrDetailModel
                                     {
                                         sysCd = main.SYS_CD,
                                         authUnit = main.AUTH_UNIT,
                                         cFunctionID = main.FUNC_ID.Trim(),
                                         cParentFunctionID = main.PARENT_FUNC_ID.Trim(),
                                         cParentFunctionName = xParent.FUNC_NAME.Trim(),
                                         cFunctionName = main.FUNC_NAME.Trim(),
                                         cFunctionType = "",
                                         iFunctionLevel = main.FUNC_LEVEL,
                                         vFunctionMemo = main.FUNC_MEMO.Trim(),
                                         vFunctionUrl = main.FUNC_URL.Trim(),
                                         iSortBy = main.FUNC_ORDER,
                                         cFlag = main.IS_DISABLED.Trim(),
                                         cCrtUserID = main.CREATE_UID.Trim(),
                                         cCrtUserName = "",
                                         cCrtDate = main.CREATE_DT == null ? "" : SqlFunctions.DateName("year", main.CREATE_DT) + "/" +
                                                                     SqlFunctions.DatePart("m", main.CREATE_DT) + "/" +
                                                                     SqlFunctions.DateName("day", main.CREATE_DT).Trim() + " " +
                                                                     SqlFunctions.DateName("hh", main.CREATE_DT).Trim() + ":" +
                                                                     SqlFunctions.DateName("n", main.CREATE_DT).Trim() + ":" +
                                                                     SqlFunctions.DateName("s", main.CREATE_DT).Trim()
                                                                     ,
                                         // cCrtTime = main.CCRTTIME.Trim(),
                                         cUpdUserID = main.LAST_UPDATE_UID == null ? "": main.LAST_UPDATE_UID.Trim(),
                                         cUpdUserName = "",
                                         cUpdDate = main.LAST_UPDATE_DT == null ? "" : SqlFunctions.DateName("year", main.LAST_UPDATE_DT) + "/" +
                                                                     SqlFunctions.DatePart("m", main.LAST_UPDATE_DT) + "/" +
                                                                     SqlFunctions.DateName("day", main.LAST_UPDATE_DT).Trim() + " " +
                                                                     SqlFunctions.DateName("hh", main.LAST_UPDATE_DT).Trim() + ":" +
                                                                     SqlFunctions.DateName("n", main.LAST_UPDATE_DT).Trim() + ":" +
                                                                     SqlFunctions.DateName("s", main.LAST_UPDATE_DT).Trim()
                                         //  cUpdTime = main.CUPDTIME.Trim()
                                     }).FirstOrDefault();




                   
                }
                
            }

            //取得新增人員姓名、異動人員姓名
            OaEmpDao oaEmpDao = new OaEmpDao();
            using (DB_INTRAEntities dbIntra = new DB_INTRAEntities())
            {
                try
                {
                    funcMgrDetail.cCrtUserName = StringUtil.toString(oaEmpDao.qryByUsrId(funcMgrDetail.cCrtUserID, dbIntra).EMP_NAME);
                }
                catch (Exception e)
                {

                }

                try
                {
                    funcMgrDetail.cUpdUserName = StringUtil.toString(oaEmpDao.qryByUsrId(funcMgrDetail.cUpdUserID, dbIntra).EMP_NAME);
                }
                catch (Exception e)
                {

                }
            }
              
            


            return funcMgrDetail;
        }




        /// <summary>
        /// 新增功能
        /// </summary>
        /// <param name="func"></param>
        /// <param name="conn"></param>
        /// <param name="transaction"></param>
        /// <returns></returns>
        public CODE_FUNC insert(CODE_FUNC func, SqlConnection conn, SqlTransaction transaction)
        {

            func.FUNC_ID = qryFuncId();
  

            string sql = @"INSERT INTO [CODE_FUNC]
                   ([SYS_CD]
                   ,[AUTH_UNIT]
                   ,[FUNC_ID]
                   ,[PARENT_FUNC_ID]
                   ,[FUNC_NAME]
                   ,[FUNC_LEVEL]
                   ,[FUNC_MEMO]
                   ,[FUNC_URL]
                   ,[FUNC_ORDER]
                   ,[IS_DISABLED]
                   ,[CREATE_UID]
                   ,[CREATE_DT]
                   )
             VALUES
                   (@SYS_CD
                   ,@AUTH_UNIT
                   ,@FUNC_ID
                   ,@PARENT_FUNC_ID
                   ,@FUNC_NAME
                   ,@FUNC_LEVEL
                   ,@FUNC_MEMO
                   ,@FUNC_URL
                   ,@FUNC_ORDER
                   ,@IS_DISABLED
                   ,@CREATE_UID
                   ,GETDATE()
                   )
        ";

            SqlCommand cmd = conn.CreateCommand();

            cmd.Connection = conn;
            cmd.Transaction = transaction;

            try
            {
                cmd.CommandText = sql;
                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("@SYS_CD", StringUtil.toString(func.SYS_CD));
                cmd.Parameters.AddWithValue("@AUTH_UNIT", StringUtil.toString(func.AUTH_UNIT));
                cmd.Parameters.AddWithValue("@FUNC_ID", StringUtil.toString(func.FUNC_ID));
                cmd.Parameters.AddWithValue("@PARENT_FUNC_ID", StringUtil.toString(func.PARENT_FUNC_ID));
                cmd.Parameters.AddWithValue("@FUNC_NAME", StringUtil.toString(func.FUNC_NAME));
                cmd.Parameters.AddWithValue("@FUNC_LEVEL", func.FUNC_LEVEL);
                cmd.Parameters.AddWithValue("@FUNC_MEMO", StringUtil.toString(func.FUNC_MEMO));
                cmd.Parameters.AddWithValue("@FUNC_URL", StringUtil.toString(func.FUNC_URL));
                cmd.Parameters.AddWithValue("@FUNC_ORDER", func.FUNC_ORDER);
                cmd.Parameters.AddWithValue("@IS_DISABLED", StringUtil.toString(func.IS_DISABLED));
                cmd.Parameters.AddWithValue("@CREATE_UID", StringUtil.toString(func.CREATE_UID));

                int cnt = cmd.ExecuteNonQuery();

                return func;
            }
            catch (Exception e)
            {
                throw e;
            }


        }


        /// <summary>
        /// 修改功能
        /// </summary>
        /// <param name="func"></param>
        /// <param name="conn"></param>
        /// <param name="transaction"></param>
        /// <returns></returns>
        public CODE_FUNC update(CODE_FUNC func, SqlConnection conn, SqlTransaction transaction)
        {
            string sql = @"update [CODE_FUNC]
           set 
            [FUNC_NAME] = @FUNC_NAME
           ,[AUTH_UNIT] = @AUTH_UNIT
           ,[FUNC_MEMO] = @FUNC_MEMO
           ,[FUNC_URL] = @FUNC_URL
           ,[FUNC_ORDER] = @FUNC_ORDER
           ,[IS_DISABLED] = @IS_DISABLED
           ,[LAST_UPDATE_UID] = @LAST_UPDATE_UID
           ,[LAST_UPDATE_DT] = @LAST_UPDATE_DT
           where FUNC_ID = @FUNC_ID
             and SYS_CD = @SYS_CD
";

            SqlCommand cmd = conn.CreateCommand();

            cmd.Connection = conn;
            cmd.Transaction = transaction;

            try
            {
                cmd.CommandText = sql;
                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("@SYS_CD", StringUtil.toString(func.SYS_CD));
                cmd.Parameters.AddWithValue("@AUTH_UNIT", StringUtil.toString(func.AUTH_UNIT));
                cmd.Parameters.AddWithValue("@FUNC_ID", StringUtil.toString(func.FUNC_ID));


                cmd.Parameters.AddWithValue("@FUNC_NAME", StringUtil.toString(func.FUNC_NAME));
                cmd.Parameters.AddWithValue("@FUNC_MEMO", StringUtil.toString(func.FUNC_MEMO));
                cmd.Parameters.AddWithValue("@FUNC_URL", StringUtil.toString(func.FUNC_URL));
                cmd.Parameters.AddWithValue("@FUNC_ORDER", func.FUNC_ORDER);
                cmd.Parameters.AddWithValue("@IS_DISABLED", StringUtil.toString(func.IS_DISABLED));
                cmd.Parameters.AddWithValue("@LAST_UPDATE_UID", StringUtil.toString(func.LAST_UPDATE_UID));
                cmd.Parameters.AddWithValue("@LAST_UPDATE_DT", func.LAST_UPDATE_DT);


                int cnt = cmd.ExecuteNonQuery();

                return func;
            }
            catch (Exception e)
            {
                throw e;
            }


        }



        public String funcLogContent(CODE_FUNC codeFunction)
        {
            String content = "";
            content += StringUtil.toString(codeFunction.SYS_CD) + "|";
            content += StringUtil.toString(codeFunction.AUTH_UNIT) + "|";
            content += StringUtil.toString(codeFunction.FUNC_ID) + "|";
            content += StringUtil.toString(codeFunction.PARENT_FUNC_ID) + "|";
            content += StringUtil.toString(codeFunction.FUNC_NAME) + "|";
            content += codeFunction.FUNC_LEVEL + "|";
            content += StringUtil.toString(codeFunction.FUNC_MEMO) + "|";
            content += StringUtil.toString(codeFunction.FUNC_URL)  + "|";
            content += codeFunction.FUNC_ORDER + "|";
            content += StringUtil.toString(codeFunction.IS_DISABLED) + "|";
            content += StringUtil.toString(codeFunction.CREATE_UID)  + "|";
            content += DateUtil.DatetimeToString(codeFunction.CREATE_DT, "") + "|";
            content += StringUtil.toString(codeFunction.LAST_UPDATE_UID) + "|";
            content += DateUtil.DatetimeToString(codeFunction.LAST_UPDATE_DT, "") ;


            return content;
        }
    }
}