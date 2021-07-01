
using System;
using System.Linq;
using SSO.Web.Models;
using SSO.Web.Utils;

/// <summary>
/// 功能說明：使用者權限、作業範圍相關
/// 初版作者：20170820 黃黛鈺
/// 修改歷程：20170820 黃黛鈺 
///           需求單號：201707240447-01 
///           初版
/// </summary>
/// 

namespace SSO.Web.Daos
{
    public class UserAuthDao
    {
        public object StringUtilrows { get; private set; }

        /// <summary>
        /// 檢查目前的使用者是否有admin的權限
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public bool chkAdmin(string sysCd, string userId) {

            bool bAdmin = false;

            using (dbFGLEntities db = new dbFGLEntities())
            {
                var rows = (from para in db.SYS_PARA
                            join role in db.CODE_USER_ROLE on para.PARA_VALUE equals role.ROLE_ID

                            where role.USER_ID == userId
                              & para.SYS_CD == sysCd
                              & para.PARA_ID == "admin"

                            select new //AwarkMainDetailModel
                            {

                                roleId = role.ROLE_ID
                            }).FirstOrDefault();

                if (rows != null)
                {
                    if (!"".Equals(StringUtil.toString(rows.roleId)))
                        bAdmin = true;
                }


                return bAdmin;
            }

        }


        /// <summary>
        /// 檢核使用者是否有使用特定功能的權限
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="funcId"></param>
        /// <returns></returns>
        public String[] qryOpScope(string userId, string sysCd,  string funcId)
        {
            String[] result = new string[] { };

            using (dbFGLEntities db = new dbFGLEntities())
            {

                //AwarkMainDetailModel awarkMainDetalModel = new AwarkMainDetailModel();
                var rows = (from userRole in db.CODE_USER_ROLE
                            join role in db.CODE_ROLE on userRole.ROLE_ID equals role.ROLE_ID
                            join roleFunc in db.CODE_ROLE_FUNC on userRole.ROLE_ID.Trim() equals roleFunc.ROLE_ID.Trim()
                            join func in db.CODE_FUNC on roleFunc.FUNC_ID.Trim() equals func.FUNC_ID.Trim()
                            where userRole.USER_ID == userId
                              & func.SYS_CD == sysCd
                              & func.FUNC_URL.StartsWith(funcId)
                              & func.IS_DISABLED.Trim() == "N"
                              & role.IS_DISABLED.Trim() == "N"

                            select new //AwarkMainDetailModel
                            {

                                roleId = role.ROLE_ID,
                                funcName = func.FUNC_NAME
                            }).FirstOrDefault();
                if (rows != null)
                {

                    result = new String[] { rows.roleId.Trim(), rows.funcName.Trim() };
                }


                return result;
            }

        }
    }
}