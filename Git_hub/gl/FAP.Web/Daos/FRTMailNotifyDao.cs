using FAP.Web.BO;
using FAP.Web.Models;
using FAP.Web.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.Entity.SqlServer;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace FAP.Web.Daos
{
    public class FRTMailNotifyDao
    {
        /// <summary>
        /// 依 GROUP_CODE 查詢需MAIL寄送的對象
        /// </summary>
        /// <param name="groupCode"></param>
        /// <returns></returns>
        public List<MailNotifyModel> qryNtyUsr(string groupCode)
        {

            dbFGLEntities db = new dbFGLEntities();

            List<MailNotifyModel> rows = (from main in db.FRT_MAIL_NOTIFY
                        where 1 == 1
                            & main.GROUP_CODE == groupCode
                            & main.EMP_TYPE == "U"

                        select new MailNotifyModel
                        {
                            receiverEmpno = main.RECEIVER_EMPNO,
                            empType = main.EMP_TYPE,
                            isNotifyMgr = main.IS_NOTIFY_MGR,
                            isNotifyDeptMgr = main.IS_NOTIFY_DEPT_MGR
                          
                        }).ToList()
                        .Union(from main in db.FRT_MAIL_NOTIFY
                               join role in db.CODE_ROLE.Where(x => x.IS_DISABLED == "N") on main.RECEIVER_EMPNO equals role.ROLE_ID
                               join rs in db.CODE_USER_ROLE on role.ROLE_ID equals rs.ROLE_ID
                               join usr in db.CODE_USER.Where(x => x.IS_DISABLED == "N") on rs.USER_ID equals usr.USER_ID
                               where 1 == 1
                                   & main.GROUP_CODE == groupCode
                                   & main.EMP_TYPE == "R"

                               select new MailNotifyModel
                               {
                                   receiverEmpno = usr.USER_ID,
                                   empType = "U",
                                   isNotifyMgr = main.IS_NOTIFY_MGR,
                                   isNotifyDeptMgr = main.IS_NOTIFY_DEPT_MGR
                               }).ToList();

            return rows;

        }


        /// <summary>
        /// 依 GROUP_CODE + RESERVE1 + RESERVE2 + RESERVE3 查詢需MAIL寄送的對象
        /// </summary>
        /// <param name="groupCode"></param>
        /// <param name="reserve1"></param>
        /// <param name="reserve2"></param>
        /// <param name="reserve3"></param>
        /// <returns></returns>
        public List<MailNotifyModel> qryNtyUsrByReserve(string[] groupCode, string sysType,
            string reserve1, string reserve2, string reserve3)
        {
            List<MailNotifyModel> data = new List<MailNotifyModel>();

            bool bReserve1 = StringUtil.isEmpty(reserve1);
            bool bReserve2 = StringUtil.isEmpty(reserve2);
            bool bReserve3 = StringUtil.isEmpty(reserve3);

            string code = "";
            SysCodeDao sysCodeDao = new SysCodeDao();
            List<SYS_CODE> codeRows = sysCodeDao.qryByType("RT", "MAIL_GROUP");

            foreach (SYS_CODE d in codeRows) {
                string[] reserve1Arr = StringUtil.toString(d.RESERVE1).Split('|');
                string[] reserve2Arr = StringUtil.toString(d.RESERVE2).Split('|');
                string[] reserve3Arr = StringUtil.toString(d.RESERVE3).Split('|');

                if (groupCode.Contains(d.CODE_VALUE)) {
                    
                    if (!bReserve1 & !reserve1Arr.Contains(reserve1))
                        continue;

                    if (!bReserve2 & !reserve1Arr.Contains(reserve2))
                        continue;

                    if (!bReserve3 & !reserve1Arr.Contains(reserve3))
                        continue;

                    code = d.CODE_VALUE;
                }
            }

            //若沒有查到對應SYS_CODE設定的MAIL群組
            if ("".Equals(code))
                return data;


            dbFGLEntities db = new dbFGLEntities();
            

            List <MailNotifyModel> rows = (from main in db.FRT_MAIL_NOTIFY
                                          join sysCode in db.SYS_CODE.Where(x => x.SYS_CD == "RT" && x.CODE_TYPE == "MAIL_GROUP") on main.GROUP_CODE equals sysCode.CODE_VALUE
                                          where 1 == 1
                                              & main.GROUP_CODE == code
                                              & main.EMP_TYPE == "U"
                                          select new MailNotifyModel
                                          {
                                              receiverEmpno = main.RECEIVER_EMPNO,
                                              empType = main.EMP_TYPE,
                                              isNotifyMgr = main.IS_NOTIFY_MGR,
                                              isNotifyDeptMgr = main.IS_NOTIFY_DEPT_MGR
                                          }).ToList()
                        .Union(from main in db.FRT_MAIL_NOTIFY
                               join sysCode in db.SYS_CODE.Where(x => x.SYS_CD == "RT" && x.CODE_TYPE == "MAIL_GROUP") on main.GROUP_CODE equals sysCode.CODE_VALUE
                               join role in db.CODE_ROLE.Where(x => x.IS_DISABLED == "N") on main.RECEIVER_EMPNO equals role.ROLE_ID
                               join rs in db.CODE_USER_ROLE on role.ROLE_ID equals rs.ROLE_ID
                               join usr in db.CODE_USER.Where(x => x.IS_DISABLED == "N") on rs.USER_ID equals usr.USER_ID
                               where 1 == 1
                                   & main.GROUP_CODE == code
                                   & main.EMP_TYPE == "R"

                               select new MailNotifyModel
                               {
                                   receiverEmpno = usr.USER_ID,
                                   empType = "U",
                                   isNotifyMgr = main.IS_NOTIFY_MGR,
                                   isNotifyDeptMgr = main.IS_NOTIFY_DEPT_MGR
                               }).ToList();


            return data;

        }
        
    }
}