
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SSO.Web.Models;
using SSO.Web.Utils;
using SSO.Web.ViewModels;

namespace SSO.Web.Daos
{
    public class OaEmpDao
    {
        public List<V_EMPLY2>  qryByUsrIdList(string[] userList)
        {

            using (DB_INTRAEntities db = new DB_INTRAEntities())
            {
                List<V_EMPLY2> oaEmp = db.V_EMPLY2
            .Where(x => userList.Contains(x.USR_ID.TrimEnd())).ToList();

                return oaEmp;
            }
        }

        public List<V_EMPLY2> qryByDptCdList(string[] dptCdList)
        {

            using (DB_INTRAEntities db = new DB_INTRAEntities())
            {
                List<V_EMPLY2> oaEmp = db.V_EMPLY2
            .Where(x => dptCdList.Contains(x.DPT_CD.TrimEnd())).ToList();

                return oaEmp;
            }
        }


        public UserMgrModel getUserOaData(UserMgrModel user, DB_INTRAEntities db)
        {


            V_EMPLY2 oaEmp = db.V_EMPLY2
                        .Where(x => x.USR_ID.TrimEnd() == user.cUserID.TrimEnd()).FirstOrDefault();

            if (oaEmp != null)
            {
                if (!"".Equals(oaEmp.EMP_NO))
                {
                    user.cUserName = StringUtil.toString(oaEmp.EMP_NAME);
                    user.cWorkUnitCode = StringUtil.toString(oaEmp.DPT_CD);
                    user.cWorkUnitDesc = StringUtil.toString(oaEmp.DPT_NAME);
                }
            }

            return user;
        }



        /// <summary>
        /// 以AD帳號查詢使用者相關資訊
        /// </summary>
        /// <param name="USR_ID"></param>
        /// <returns></returns>
        public V_EMPLY2 qryByUsrId(String USR_ID, DB_INTRAEntities db)
        {


            V_EMPLY2 oaEmp = db.V_EMPLY2
                .Where(x => x.USR_ID.TrimEnd() == USR_ID).FirstOrDefault();

            return oaEmp;
        }

        public V_EMPLY2 qryByUsrId(String USR_ID)
        {

            using (DB_INTRAEntities db = new DB_INTRAEntities())
            {
                V_EMPLY2 oaEmp = db.V_EMPLY2
                .Where(x => x.USR_ID.TrimEnd() == USR_ID).FirstOrDefault();

                return oaEmp;
            }
        }


        /// <summary>
        /// 取得人員姓名
        /// </summary>
        /// <param name="userNameMap"></param>
        /// <param name="usrId"></param>
        /// <param name="db"></param>
        /// <returns></returns>
        public Dictionary<string, string> qryUsrName(Dictionary<string, string> userNameMap, string usrId, DB_INTRAEntities db)
        {

            if (!"".Equals(usrId))
            {
                if (!userNameMap.ContainsKey(usrId))
                {
                    V_EMPLY2 oaEmp =  qryByUsrId(usrId, db);
                    if (oaEmp != null)
                    {
                        if (!"".Equals(StringUtil.toString(oaEmp.EMP_NAME)))
                        {
                            userNameMap.Add(usrId, StringUtil.toString(oaEmp.EMP_NAME));
                        }
                        else
                        {
                            userNameMap.Add(usrId, "");
                        }
                    }
                    else {
                        userNameMap.Add(usrId, "");
                    }
                }
            }

            return userNameMap;
        }
    }
}