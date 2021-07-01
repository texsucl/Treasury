
using System;
using System.Collections.Generic;
using System.Linq;
using FAP.Web.Models;
using FAP.Web.ViewModels;
using FAP.Web.BO;
using System.Data.EasycomClient;
using FAP.Web.AS400Models;
using FAP.Web.AS400PGM;

namespace FAP.Web.Daos
{
    public class OaEmpDao
    {

        public UserMgrModel getUserOaData(UserMgrModel user, DB_INTRAEntities db)
        {


            V_EMPLY2 oaEmp = db.V_EMPLY2
                        .Where(x => x.USR_ID == user.cUserID).FirstOrDefault();

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

        public UserBossModel getEmpBoss(string usrId, EacConnection con, EacCommand cmd)
        {
            //若傳入的usrId為身份證字號，需先呼叫SRTB0010取得5碼AD帳號後，再至DB_INTRA撈mail
            if (usrId.Trim().Length == 10)
            {
                SRTB0010Util sRTB0010Util = new SRTB0010Util();
                SRTB0010Model sRTB0010Model = new SRTB0010Model();
                sRTB0010Model = sRTB0010Util.callSRTB0010(con, cmd, usrId);

                usrId = StringUtil.toString(sRTB0010Model.empAd);
            }

            var rows = getEmpBoss(usrId);

            return rows;
        }

        /// <summary>
        /// 以AD帳號查詢使用者相關資訊
        /// </summary>
        /// <param name="USR_ID"></param>
        /// <returns></returns>
        public V_EMPLY2 qryByUsrId(String USR_ID, DB_INTRAEntities db)
        {
            V_EMPLY2 oaEmp = new V_EMPLY2();

            switch (StringUtil.toString(USR_ID).Length) {
                case 5:
                    oaEmp = db.V_EMPLY2.Where(x => x.USR_ID == USR_ID).FirstOrDefault();
                    break;
                case 10:
                    oaEmp = db.V_EMPLY2.Where(x => x.ID_NO == USR_ID).FirstOrDefault();
                    break;
            }

            return oaEmp;
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

        /// <summary>
        /// 查詢員工及其主管的資訊
        /// </summary>
        /// <param name="usrId"></param>
        /// <returns></returns>
        public UserBossModel getEmpBoss(string usrId)
        {
            using (DB_INTRAEntities db = new DB_INTRAEntities())
            {
                var rows = (from emp in db.V_EMPLY2
                            join dept in db.VW_OA_DEPT on emp.DPT_CD equals dept.DPT_CD

                            join empMgr in db.V_EMPLY2 on dept.DPT_HEAD equals empMgr.EMP_NO into psEmpMgr
                            from xEmpMgr in psEmpMgr.DefaultIfEmpty()

                            join deptUp in db.VW_OA_DEPT on dept.UP_DPT_CD equals deptUp.DPT_CD into psDeptUp
                            from xDeptUp in psDeptUp.DefaultIfEmpty()

                            join empDeptMgr in db.V_EMPLY2 on xDeptUp.DPT_HEAD equals empDeptMgr.EMP_NO into psEmpDeptMgr
                            from xEmpDeptMgr in psEmpDeptMgr.DefaultIfEmpty()




                            where emp.USR_ID == usrId

                            select new UserBossModel
                            {
                                usrId = emp.USR_ID,
                                deptType = dept.Dpt_type,
                                empNo = emp.EMP_NO,
                                empName = emp.EMP_NAME,
                                empMail = emp.EMAIL,

                                usrIdMgr = xEmpMgr.USR_ID,
                                empNoMgr = xEmpMgr.EMP_NO,
                                empNameMgr = xEmpMgr.EMP_NAME,
                                empMailMgr = xEmpMgr.EMAIL,

                                usrIdDeptMgr = xEmpDeptMgr.USR_ID,
                                empNoDeptMgr = xEmpDeptMgr.EMP_NO,
                                empNameDeptMgr = xEmpDeptMgr.EMP_NAME,
                                empMailDeptMgr = xEmpDeptMgr.EMAIL
                            }).FirstOrDefault();
                return rows;


            }

        }


        public UserBossModel getEmpBoss(string usrId, EacConnection con)
        {
            EacCommand cmd = new EacCommand();
            cmd.Connection = con;

            //若傳入的usrId為身份證字號，需先呼叫SRTB0010取得5碼AD帳號後，再至DB_INTRA撈mail
            if (usrId.Trim().Length == 10)
            {
                SRTB0010Util sRTB0010Util = new SRTB0010Util();
                SRTB0010Model sRTB0010Model = new SRTB0010Model();
                sRTB0010Model = sRTB0010Util.callSRTB0010(con, cmd, usrId);

                usrId = StringUtil.toString(sRTB0010Model.empAd);
            }

            var rows = getEmpBoss(usrId);

            cmd.Dispose();
            cmd = null;

            return rows;
        }
    }
}