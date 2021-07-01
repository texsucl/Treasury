
using FGL.Web.Daos;
using System;

namespace FGL.Web.BO
{


    public class UserAuthUtil
    {

        

        /// <summary>
        /// 查詢使用者使用特定功能的權限
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="funcId"></param>
        /// <returns></returns>
        public String[] chkUserFuncAuth(string userId, string funcId) {
            string sysCd = System.Configuration.ConfigurationManager.AppSettings.Get("SysCd");

            UserAuthDao userAuthDao = new UserAuthDao();


            return userAuthDao.qryOpScope(sysCd, userId, funcId);
        }
        
    }
}