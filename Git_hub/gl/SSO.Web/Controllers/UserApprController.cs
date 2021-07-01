
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SSO.Web;
using System.Transactions;
using SSO.Web.ViewModels;
using SSO.Web.Daos;
using SSO.Web.Models;
using SSO.Web.ActionFilter;
using SSO.Web.BO;
using SSO.Web.Utils;
using SSO.WebViewModels;

/// <summary>
/// 功能說明：使用者權限覆核作業
/// 初版作者：20180515 黃黛鈺
/// 修改歷程：20180515 黃黛鈺 
///           需求單號：
///           初版
/// ==============================================
/// 修改日期/修改人：20200121 B0077 黃黛鈺
/// 需求單號：201911280279-00
/// 修改內容：1.使用者點選『使用者權限覆核作業』時，需先以登入者的所屬單位，檢查【可授權程式table檔.管理者所屬部門】 是否有對應資料，若無，則無法作業,需出錯誤訊息”未建管理者可授權程式table檔,請先建檔!”
///           2.畫面「授權單位」改為「管理者單位」：以登入者的所屬單位帶出。
///           3.明細增加「異動角色OWNER單位」。
///           4.可覆核的資料內容
///             4.1 帳號的建立、停用：使用者的單位，屬登入者被授權管理單位對應的程式OWNER單位。
///             4.2 角色的授權：維護的角色OWNER單位為登入者的所屬單位帶出對應可處理的程式OWNER單位。
/// ==============================================
/// </summary>
/// 
namespace SSO.WebControllers
{
    [Authorize]
    [CheckSessionFilterAttribute]
    public class UserApprController : BaseController
    {
        static private NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();


        /// <summary>
        /// 畫面初始
        /// </summary>
        /// <returns></returns>
        public ActionResult Index()
        {
            UserAuthUtil authUtil = new UserAuthUtil();

            string opScope = "";
            string funcName = "";

            String[] roleInfo = authUtil.chkUserFuncAuth(Session["UserID"].ToString(), "~/UserAppr/");
            if (roleInfo != null && roleInfo.Length == 2)
            {
                opScope = roleInfo[0];
                funcName = roleInfo[1];
            }

            ViewBag.opScope = opScope;
            ViewBag.funcName = funcName;

            bool bAdmin = authUtil.chkAdmin("SSO", Session["UserID"].ToString());

            ViewBag.opScope = opScope;

            if (bAdmin)
            {
                ViewBag.mgrUnit = "";
                ViewBag.mgrUnitNm = "";
            }
            else
            {
                ViewBag.mgrUnit = Session["UserUnit"].ToString();
                ViewBag.mgrUnitNm = Session["UserUnitNm"].ToString();
            }


            //程式OWNER單位 add by daiyu 20200110
            CodeAuthMgrDao codeAuthMgrDao = new CodeAuthMgrDao();
            var AuthUnitList = codeAuthMgrDao.loadSelectList(Session["UserUnit"].ToString(), true);
            ViewBag.authUnitCnt = AuthUnitList.Count();

            ///*---畫面下拉選單初始值---*/
            //TypeDefineDao typeDefineDao = new TypeDefineDao();

            ////覆核單種類
            //var ReviewTypeList = typeDefineDao.loadSelectList("reviewType");
            //ViewBag.ReviewTypeList = ReviewTypeList;

            ////覆核狀態
            //var ReviewFlagList = typeDefineDao.loadSelectList("reviewSts");
            //ViewBag.ReviewFlagList = ReviewFlagList;

            return View();
        }


        /// <summary>
        /// 查詢待覆核的資料
        /// </summary>
        /// <param name="cReviewType"></param>
        /// <param name="mgrUnit"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult LoadData(string cReviewType, string mgrUnit)
        {
            //程式OWNER單位
            CodeAuthMgrDao codeAuthMgrDao = new CodeAuthMgrDao();
            string[] authUnitArr = codeAuthMgrDao.qryAuthUnit(Session["UserUnit"].ToString());


            AplyRecDao aplyRecDao = new AplyRecDao();
            OaDeptDao OaDeptDao = new OaDeptDao();
            VW_OA_DEPT unit = new VW_OA_DEPT();
            List<UnitInfoModel> unitList = new List<UnitInfoModel>();
            //string[] unitArry = new string[] { };
            //unit = OaDeptDao.qryByDptCd(authUnit);
            //if (unit != null)
            //{
            //    if ("03".Equals(StringUtil.toString(unit.Dpt_type)))
            //    {
            //        unitList = OaDeptDao.qryDept(authUnit);
            //        unitArry = unitList.Select(a => a.unitCode.ToString()).ToArray();
            //    }
            //}

            List<AuthReviewModel> rows = new List<AuthReviewModel>();
            List<AuthReviewModel> dataList = new List<AuthReviewModel>();

            using (new TransactionScope(
                  TransactionScopeOption.Required,
                  new TransactionOptions
                  {
                      IsolationLevel = IsolationLevel.ReadUncommitted
                  }))
            {
                using (dbFGLEntities db = new dbFGLEntities())
                {
                    try
                    {
                        //查出待覆核的資料
                        dataList = aplyRecDao.qryAuthReview(authUnitArr, mgrUnit, "U", "1", db);
                    }
                    catch (Exception e)
                    {
                        logger.Error("其它錯誤：" + e.ToString());
                        return Json(new { success = false, err = "其它錯誤，請洽系統管理員!!" });
                    }
                }
            }

            using (DB_INTRAEntities dbIntra = new DB_INTRAEntities())
            {
                Dictionary<string, string> userNameMap = new Dictionary<string, string>();
                OaEmpDao oaEmpDao = new OaEmpDao();
                string createUid = "";
                string userId = "";

                //對屬"使用者"異動(非角色異動)的，查出所屬的單位
                //foreach (AuthReviewModel d in dataList.Where(x => x.authUnit == "").ToList())
                //{
                //    V_EMPLY2 empl = oaEmpDao.qryByUsrId(d.cMappingKey, dbIntra);
                //    d.authUnit = StringUtil.toString(empl.DPT_CD);
                //}


                rows = dataList.Where(x => authUnitArr.Contains(x.authUnit) 
                                        || (x.authUnit == "" & authUnitArr.Contains(x.userUnit))
                                        || (x.authUnit != "" & !authUnitArr.Contains(x.authUnit) & authUnitArr.Contains(x.userUnit) & x.freeAuth == "Y")
                                        
                                        ).ToList();

                foreach (AuthReviewModel d in rows)
                {
                    if ("".Equals(d.authUnit))
                        d.authUnit = d.userUnit;

                    createUid = StringUtil.toString(d.createUid);
                    userId = StringUtil.toString(d.cMappingKey);

                    if (!"".Equals(createUid))
                    {
                        if (!userNameMap.ContainsKey(createUid))
                        {
                            userNameMap = oaEmpDao.qryUsrName(userNameMap, createUid, dbIntra);
                        }
                        d.createUid = createUid + " " + userNameMap[createUid];
                    }

                    if (!"".Equals(userId))
                    {
                        if (!userNameMap.ContainsKey(userId))
                        {
                            userNameMap = oaEmpDao.qryUsrName(userNameMap, userId, dbIntra);
                        }
                        d.cMappingKeyDesc = userId + " " + userNameMap[userId];
                    }

                }

            }
            var jsonData = new { success = true, rows };
            return Json(jsonData, JsonRequestBehavior.AllowGet);
        }

        ///// <summary>
        ///// 開啟使用者修改明細畫面
        ///// </summary>
        ///// <param name="aplyNo"></param>
        ///// <returns></returns>
        //public ActionResult detailUser(string cReviewSeq)
        //{
        //    try
        //    {
        //        using (DbAccountEntities db = new DbAccountEntities())
        //        {
        //            CodeUserHisDao codeUserHisDao = new CodeUserHisDao();
        //            AuthReviewUserModel userData = codeUserHisDao.qryByNowHis(cReviewSeq, db);

        //            string[] cDateTime = userData.cCrtDateTime.Split(' ');
        //            userData.cCrtDateTime = DateUtil.formatDateTimeDbToSc(cDateTime[0] + " " + cDateTime[1], "DT");

        //            ViewBag.bHaveData = "Y";
        //            ViewBag.cReviewSeq = cReviewSeq;
        //            return View(userData);
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        ViewBag.bHaveData = "N";
        //        return View();
        //    }
        //}


        /// <summary>
        /// 開啟使用者修改明細畫面
        /// </summary>
        /// <param name="aplyNo"></param>
        /// <returns></returns>
        public ActionResult detailUser(string aplyNo, string userId)
        {
            try
            {
                string execAction = "";
                AplyRecDao aplyRecDao = new AplyRecDao();

                SSO_APLY_REC authAppr = new SSO_APLY_REC();


                if (!"".Equals(StringUtil.toString(aplyNo)))
                {
                    authAppr = aplyRecDao.qryByKey(aplyNo);
                    ViewBag.bView = "N";
                }

                else
                {
                    authAppr = aplyRecDao.qryByFreeRole(userId);
                    if (authAppr != null)
                        aplyNo = StringUtil.toString(authAppr.APLY_NO);

                    ViewBag.bView = "Y";
                }




                AuthReviewUserModel userData = new AuthReviewUserModel();
                userData.aplyNo = aplyNo;
                userData.userId = authAppr.APPR_MAPPING_KEY;
                userData.createUid = authAppr.CREATE_UID;

                OaEmpDao oaEmpDao = new OaEmpDao();
                using (DB_INTRAEntities dbIntra = new DB_INTRAEntities())
                {
                    try
                    {
                        userData.createUid = userData.createUid == null ? "" : StringUtil.toString(oaEmpDao.qryByUsrId(userData.createUid, dbIntra).EMP_NAME);
                        userData.userName = userData.userId == null ? "" : StringUtil.toString(oaEmpDao.qryByUsrId(userData.userId, dbIntra).EMP_NAME);
                    }
                    catch (Exception e)
                    {

                    }
                }
                userData.createDt = authAppr.CREATE_DT.ToString();


                SysCodeDao sysCodeDao = new SysCodeDao();
                Dictionary<string, string> dicExecAction = sysCodeDao.qryByTypeDic("SSO", "EXEC_ACTION");
                Dictionary<string, string> dicYNFlag = sysCodeDao.qryByTypeDic("SSO", "YN_FLAG");

                CodeUserHisDao codeUserHisDao = new CodeUserHisDao();
                CODE_USER_HIS codeUserHis = codeUserHisDao.qryByAplyNo(aplyNo);
                if (codeUserHis != null) {
                    execAction = StringUtil.toString(codeUserHis.EXEC_ACTION);
                }

                if ("".Equals(execAction))
                {
                    CodeUserDao codeUserDao = new CodeUserDao();
                    CODE_USER codeUser = new CODE_USER();
                    codeUser = codeUserDao.qryUserByKey(authAppr.APPR_MAPPING_KEY);

                    userData.isMailB = StringUtil.toString(codeUser.IS_MAIL);
                    userData.isDisabledB = StringUtil.toString(codeUser.IS_DISABLED);

                }
                else {
                    if ("A".Equals(execAction))
                    {
                        userData.isMail = StringUtil.toString(codeUserHis.IS_MAIL);
                        userData.isDisabled = StringUtil.toString(codeUserHis.IS_DISABLED);
                    }
                    else {
                        userData.isMail = StringUtil.toString(codeUserHis.IS_MAIL);
                        userData.isDisabled = StringUtil.toString(codeUserHis.IS_DISABLED);

                        userData.isMailB = StringUtil.toString(codeUserHis.IS_MAIL_B);
                        userData.isDisabledB = StringUtil.toString(codeUserHis.IS_DISABLED_B);
                    }
                }


                userData.execAction = execAction;
                userData.execActionDesc = dicExecAction.ContainsKey(userData.execAction) ? dicExecAction[userData.execAction] : userData.execAction;
                userData.isDisabledDesc = dicYNFlag.ContainsKey(userData.isDisabled) ? dicYNFlag[userData.isDisabled] : userData.isDisabled;
                userData.isDisabledDescB = dicYNFlag.ContainsKey(userData.isDisabledB) ? dicYNFlag[userData.isDisabledB] : userData.isDisabledB;
                userData.isMailDesc = dicYNFlag.ContainsKey(userData.isMail) ? dicYNFlag[userData.isMail] : userData.isMail;
                userData.isMailDescB = dicYNFlag.ContainsKey(userData.isMailB) ? dicYNFlag[userData.isMailB] : userData.isMailB;



                ViewBag.bHaveData = "Y";
                ViewBag.aplyNo = aplyNo;
                return View(userData);

            }
            catch (Exception e)
            {
                ViewBag.bHaveData = "N";
                return View();
            }
        }


        /// <summary>
        /// 查詢角色功能異動明細資料
        /// </summary>
        /// <param name="aplyNo"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult qryUserRoleHis(String aplyNo)
        {
            CodeUserRoleHisDao codeUserRoleHisDao = new CodeUserRoleHisDao();

            try
            {
                List<CodeUserRoleModel> rows = new List<CodeUserRoleModel>();

                rows = codeUserRoleHisDao.qryByAplyNo(aplyNo);
                Dictionary<string, string> unitMap = new Dictionary<string, string>();
                OaDeptDao oaDeptDao = new OaDeptDao();


                foreach (var item in rows)
                {
                    string roleAuthUnit = StringUtil.toString(item.roleAuthUnit);
                    string authUnitNm = "";
                    if (unitMap.ContainsKey(roleAuthUnit))
                        authUnitNm = unitMap[roleAuthUnit];
                    else
                    {
                        VW_OA_DEPT dept = oaDeptDao.qryByDptCd(StringUtil.toString(item.roleAuthUnit));
                        if (dept != null)
                        {
                            authUnitNm = StringUtil.toString(dept.DPT_NAME);
                            unitMap.Add(roleAuthUnit, authUnitNm);

                        }
                        else
                            authUnitNm = StringUtil.toString(item.roleAuthUnit);
                    }

                    item.roleAuthUnitNm = authUnitNm;
                }


                var jsonData = new { success = true, rows };
                return Json(jsonData, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                logger.Error("其它錯誤：" + e.ToString());
                return Json(new { success = false, err = "其它錯誤，請洽系統管理員!!" });
            }


        }




        /// <summary>
        /// 核可/退回(使用者覆核)
        /// </summary>
        /// <param name="aplyNo"></param>
        /// <param name="userId"></param>
        /// <param name="apprStatus"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult execReviewU(string aplyNo, string userId, string apprStatus)
        {
            string strConn = DbUtil.GetDBFglConnStr();
            using (SqlConnection conn = new SqlConnection(strConn))
            {
                conn.Open();
                SqlTransaction transaction = conn.BeginTransaction("Transaction");

                try
                {
                    AplyRecDao aplyRecDao = new AplyRecDao();
                    SSO_APLY_REC authAppr = aplyRecDao.qryByKey(aplyNo);

                    if (authAppr.CREATE_UID.Equals(Session["UserID"].ToString()))
                        return Json(new { success = false, errors = "申請人與覆核人員不可相同!!" }, JsonRequestBehavior.AllowGet);

                    //異動使用者資料檔
                    //string cExecType = "";
                    CodeUserHisDao codeUserHisDao = new CodeUserHisDao();
                    CodeUserDao codeUserDao = new CodeUserDao();
                    CODE_USER cODEUSERO = new CODE_USER();

                    CODE_USER_HIS codeUserHis = codeUserHisDao.qryByAplyNo(aplyNo);
                    string execAction = "";
                    if (codeUserHis != null)
                        execAction = StringUtil.toString(codeUserHis.EXEC_ACTION);


                    OaEmpDao oaEmpDao = new OaEmpDao();
                    V_EMPLY2 emp = new V_EMPLY2();
                    using (DB_INTRAEntities dbIntra = new DB_INTRAEntities())
                    {
                        emp = oaEmpDao.qryByUsrId(cODEUSERO.USER_ID, dbIntra);
                        
                    }

                    if ("A".Equals(execAction))  //新增使用者
                    {
                        if (emp != null)
                            cODEUSERO.USER_UNIT = StringUtil.toString(emp.DPT_CD);
                    }
                    else
                    {  //異動角色
                        cODEUSERO = codeUserDao.qryUserByKey(userId);
                        if (emp != null)
                            cODEUSERO.USER_UNIT = StringUtil.toString(emp.DPT_CD) == "" ? "" : cODEUSERO.USER_UNIT;
                    }



                    if ("A".Equals(execAction))
                    {
                        if ("2".Equals(apprStatus))
                        {
                            cODEUSERO.USER_ID = StringUtil.toString(codeUserHis.USER_ID);
                            cODEUSERO.IS_DISABLED = codeUserHis.IS_DISABLED;
                            cODEUSERO.IS_MAIL = codeUserHis.IS_MAIL;
                            cODEUSERO.DATA_STATUS = "1";
                            cODEUSERO.CREATE_UID = authAppr.CREATE_UID;
                            cODEUSERO.CREATE_DT = authAppr.CREATE_DT;
                            cODEUSERO.CREATE_UNIT = authAppr.CREATE_UNIT;
                            cODEUSERO.LAST_UPDATE_UID = StringUtil.toString(authAppr.CREATE_UID);
                            cODEUSERO.LAST_UPDATE_DT = authAppr.CREATE_DT;
                            cODEUSERO.APPR_UID = Session["UserID"].ToString();
                            cODEUSERO.APPR_DT = DateTime.Now;

                            int cnt = codeUserDao.Create(cODEUSERO, conn, transaction);

                            //新增LOG
                            Log log = new Log();
                            log.CFUNCTION = "使用者管理-新增";
                            log.CACTION = "A";
                            log.CCONTENT = codeUserDao.userLogContent(cODEUSERO);
                            LogDao.Insert(log, Session["UserID"].ToString());

                            //新增稽核軌跡 modify by daiyu 20190515
                            procTrackLog("A", "codeUser", log.CCONTENT, 1, cODEUSERO.USER_ID, conn, transaction);
                        }
                    }
                    else
                    {
                        if (codeUserHis != null)
                        {


                            //新增LOG
                            Log log = new Log();

                            if ("Y".Equals(codeUserHis.IS_DISABLED))
                            {
                                log.CFUNCTION = "使用者管理-修改";
                                log.CACTION = "U";
                            }
                            else
                            {
                                log.CFUNCTION = "使用者管理-刪除";
                                log.CACTION = "D";
                            }

                            log.CCONTENT = codeUserDao.userLogContent(cODEUSERO);
                            LogDao.Insert(log, Session["UserID"].ToString());

                            cODEUSERO.DATA_STATUS = "1";
                            cODEUSERO.LAST_UPDATE_UID = StringUtil.toString(authAppr.CREATE_UID);
                            cODEUSERO.LAST_UPDATE_DT = authAppr.CREATE_DT;
                            cODEUSERO.APPR_UID = Session["UserID"].ToString();
                            cODEUSERO.APPR_DT = DateTime.Now;
                            cODEUSERO.FREEZE_DT = null;
                            cODEUSERO.FREEZE_UID = "";

                            if ("U".Equals(execAction) && "2".Equals(apprStatus))
                            {
                                cODEUSERO.IS_DISABLED = codeUserHis.IS_DISABLED;
                                cODEUSERO.IS_MAIL = StringUtil.toString(codeUserHis.IS_MAIL);
                            }

                            int cnt = 0;
                            if ("Y".Equals(codeUserHis.IS_DISABLED))
                                cnt = codeUserDao.Delete(cODEUSERO, conn, transaction);
                            else
                                cnt = codeUserDao.Update(cODEUSERO, conn, transaction);


                            //新增稽核軌跡 add by daiyu 20190515
                            procTrackLog("E", "codeUser", log.CCONTENT, cnt, cODEUSERO.USER_ID, conn, transaction);
                        }
                        else {
                            cODEUSERO.DATA_STATUS = "1";
                            cODEUSERO.LAST_UPDATE_UID = StringUtil.toString(authAppr.CREATE_UID);
                            cODEUSERO.LAST_UPDATE_DT = authAppr.CREATE_DT;
                            cODEUSERO.APPR_UID = Session["UserID"].ToString();
                            cODEUSERO.APPR_DT = DateTime.Now;
                            cODEUSERO.FREEZE_DT = null;
                            cODEUSERO.FREEZE_UID = "";
                            codeUserDao.Update(cODEUSERO, conn, transaction);
                        }
                            


                    }


                    //覆核狀態=核可時
                    if ("2".Equals(apprStatus)) {
                        if (codeUserHis == null)
                            procUserRoleHis(cODEUSERO, aplyNo, conn, transaction); //異動使用者角色
                        else {
                            if ("Y".Equals(codeUserHis.IS_DISABLED))
                                delUserRoleHis(cODEUSERO, conn, transaction); //刪除使用者角色
                            else
                                procUserRoleHis(cODEUSERO, aplyNo, conn, transaction); //異動使用者角色
                        }
                        
                    }
                        


                    //異動覆核資料檔
                    procAuthAppr(aplyNo, apprStatus, conn, transaction);

                    transaction.Commit();
                    return Json(new { success = true });
                }
                catch (Exception e)
                {
                    transaction.Rollback();
                    logger.Error("[execReviewR]其它錯誤：" + e.ToString());

                    return Json(new { success = false, errors = "其它錯誤，請洽系統管理員!!" }, JsonRequestBehavior.AllowGet);
                }

            }

        }


        private void delUserRoleHis(CODE_USER cODEUSERO, SqlConnection conn, SqlTransaction transaction)
        {
            CodeUserRoleDao codeUserRoleDao = new CodeUserRoleDao();
            List<CodeUserRoleModel> cRoleList = codeUserRoleDao.qryByUserID(cODEUSERO.USER_ID);
            if (cRoleList != null)
            {
                if (cRoleList.Count > 0)
                {

                    foreach (CodeUserRoleModel d in cRoleList)
                    {
                        CODE_USER_ROLE dRole = codeUserRoleDao.qryByKey(cODEUSERO.USER_ID, d.roleId);

                        //新增LOG
                        Log log = new Log();
                        log.CFUNCTION = "使用者管理(角色授權)-刪除";
                        log.CACTION = "D";
                        log.CCONTENT = codeUserRoleDao.logContent(dRole);
                        LogDao.Insert(log, Session["UserID"].ToString());

                        //刪除資料
                        codeUserRoleDao.delete(dRole, conn, transaction);

                        //新增稽核軌跡 add by daiyu 20190515
                        procTrackLog("D", "codeUserRole", log.CCONTENT, 1, cODEUSERO.USER_ID, conn, transaction);

                    }

                }
            }
        }


        /// <summary>
        /// 處理使用者角色異動
        /// </summary>
        /// <param name="roleId"></param>
        /// <param name="aplyNO"></param>
        /// <param name="conn"></param>
        /// <param name="transaction"></param>
        private void procUserRoleHis(CODE_USER cODEUSERO, string aplyNo, SqlConnection conn, SqlTransaction transaction)
        {
            CodeUserRoleHisDao codeUserRoleHisDao = new CodeUserRoleHisDao();
            List<CodeUserRoleModel> cRoleList = codeUserRoleHisDao.qryByAplyNo(aplyNo);
            if (cRoleList != null)
            {
                if (cRoleList.Count > 0)
                {
                    CodeUserRoleDao codeUserRoleDao = new CodeUserRoleDao();

                    foreach (CodeUserRoleModel d in cRoleList)
                    {
                        if ("A".Equals(d.execAction))
                        {
                            CODE_USER_ROLE dRole = new CODE_USER_ROLE();
                            dRole.USER_ID = cODEUSERO.USER_ID;
                            dRole.ROLE_ID = d.roleId;
                            dRole.CREATE_UID = cODEUSERO.LAST_UPDATE_UID;
                            dRole.CREATE_DT = cODEUSERO.LAST_UPDATE_DT;


                            //新增資料
                            codeUserRoleDao.insert(dRole, conn, transaction);


                            //新增LOG
                            Log log = new Log();
                            log.CFUNCTION = "使用者管理(角色授權)-新增";
                            log.CACTION = "A";
                            log.CCONTENT = codeUserRoleDao.logContent(dRole);
                            LogDao.Insert(log, Session["UserID"].ToString());


                            //新增稽核軌跡 add by daiyu 20190515
                            procTrackLog("A", "codeUserRole", log.CCONTENT, 1, cODEUSERO.USER_ID, conn, transaction);
                        }
                        else
                        {
                            CODE_USER_ROLE dRole = codeUserRoleDao.qryByKey(cODEUSERO.USER_ID, d.roleId);


                            //新增LOG
                            Log log = new Log();
                            log.CFUNCTION = "使用者管理(角色授權)-刪除";
                            log.CACTION = "D";
                            log.CCONTENT = codeUserRoleDao.logContent(dRole);
                            LogDao.Insert(log, Session["UserID"].ToString());

                            //刪除資料
                            codeUserRoleDao.delete(dRole, conn, transaction);

                            //新增稽核軌跡 add by daiyu 20190515
                            procTrackLog("D", "codeUserRole", log.CCONTENT, 1, cODEUSERO.USER_ID, conn, transaction);
                        }

                    }

                }
            }
        }


        

        /// <summary>
        /// 異動覆核資料檔
        /// </summary>
        /// <param name="cReviewSeq"></param>
        /// <param name="cReviewFlag"></param>
        /// <param name="conn"></param>
        /// <param name="transaction"></param>
        private void procAuthAppr(string aplyNo, string appStatus, SqlConnection conn, SqlTransaction transaction)
        {
            SSO_APLY_REC authAppr = new SSO_APLY_REC();
            authAppr.APLY_NO = aplyNo;
            authAppr.APPR_STATUS = appStatus;
            authAppr.APPR_UID = Session["UserID"].ToString();
            authAppr.APPR_DT = DateTime.Now;
            authAppr.LAST_UPDATE_UID = Session["UserID"].ToString();
            authAppr.LAST_UPDATE_DT = DateTime.Now;

            AplyRecDao aplyRecDao = new AplyRecDao();
            aplyRecDao.updateStatus(authAppr, conn, transaction);
        }

        /// <summary>
        /// 新增稽核軌跡  
        /// modify by daiyu 20190515
        /// </summary>
        /// <param name="type"></param>
        /// <param name="accessobj"></param>
        /// <param name="content"></param>
        /// <param name="rowCnt"></param>
        /// <param name="owner"></param>
        /// <param name="conn"></param>
        /// <param name="transaction"></param>
        public void procTrackLog(string type, string accessobj, string content, int rowCnt,string owner, SqlConnection conn, SqlTransaction transaction)
        {

            PIA_LOG_MAIN piaLog = new PIA_LOG_MAIN();
            piaLog.TRACKING_TYPE = "C";
            piaLog.ACCESS_ACCOUNT = Session["UserID"].ToString();
            piaLog.ACCOUNT_NAME = Session["UserName"].ToString();
            piaLog.PROGFUN_NAME = "UserAppr";
            piaLog.ACCESSOBJ_NAME = accessobj;
            piaLog.EXECUTION_TYPE = type;
            piaLog.EXECUTION_CONTENT = content;
            piaLog.AFFECT_ROWS = rowCnt;
            piaLog.PIA_OWNER1 = owner;
            piaLog.PIA_OWNER2 = "";
            piaLog.PIA_TYPE = "0100000000";


            PiaLogMainDao piaLogMainDao = new PiaLogMainDao();
            piaLogMainDao.Insert(piaLog, conn, transaction);

        }
    }
}