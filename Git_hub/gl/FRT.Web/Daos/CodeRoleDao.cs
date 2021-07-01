
using FRT.Web.Models;
using FRT.Web.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Entity.SqlServer;
using System.Transactions;
using FRT.Web.BO;

/// <summary>
/// 功能說明：
/// 初版作者：20181205 黃黛鈺
/// 修改歷程：20181205 黃黛鈺 
///           需求單號：201811300566-02
///           初版
/// ==============================================
/// 修改日期/修改人：
/// 需求單號：
/// 修改內容：
/// ==============================================
/// </summary>
/// 
namespace FRT.Web.Daos
{
    public class CodeRoleDao 
    {
        

        /// <summary>
        /// 以鍵項查詢角色資料
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public CODE_ROLE qryRoleByKey(String roleId)
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
                    CODE_ROLE codeRole = db.CODE_ROLE.Where(x => x.ROLE_ID == roleId).FirstOrDefault<CODE_ROLE>();

                    return codeRole;
                }
            }
        }
        
    }
}
