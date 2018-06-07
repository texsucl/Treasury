using Treasury.WebActionFilter;
using Treasury.WebBO;
using Treasury.WebDaos;
using Treasury.WebModels;
using Treasury.WebUtils;
using Treasury.WebViewModels;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Treasury.Web;
using Treasury.Web.Models;
using System.Transactions;
using Treasury.Web.ViewModels;
using Treasury.Web.Daos;

/// <summary>
/// 功能說明：使用者權限覆核作業
/// 初版作者：20180515 黃黛鈺
/// 修改歷程：20180515 黃黛鈺 
///           需求單號：
///           初版
/// ==============================================
/// 修改日期/修改人：
/// 需求單號：
/// 修改內容：
/// ==============================================
/// </summary>
/// 
namespace Treasury.WebControllers
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

            String opScope = "";
            String[] roleInfo = authUtil.chkUserFuncAuth(Session["UserID"].ToString(), "~/UserAppr/");
            if (roleInfo != null && roleInfo.Length == 1)
            {
                opScope = roleInfo[0];

            }

            ViewBag.opScope = opScope;


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
        /// <returns></returns>
        [HttpPost]
        public JsonResult LoadData(String cReviewType)
        {
            AuthApprDao authApprDao = new AuthApprDao();

            List<AuthReviewModel> rows = new List<AuthReviewModel>();
            using (new TransactionScope(
                  TransactionScopeOption.Required,
                  new TransactionOptions
                  {
                      IsolationLevel = IsolationLevel.ReadUncommitted
                  }))
            {
                using (dbTreasuryEntities db = new dbTreasuryEntities())
                {
                    try
                    {
                        //查出待覆核的資料
                        rows = authApprDao.qryAuthReview("U", "1", db);
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

                foreach (AuthReviewModel d in rows)
                {
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
                        d.cMappingKeyDesc = userNameMap[userId];
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
                AuthApprDao AuthApprDao = new AuthApprDao();

                AUTH_APPR authAppr = new AUTH_APPR();


                if (!"".Equals(StringUtil.toString(aplyNo)))
                {
                    authAppr = AuthApprDao.qryByKey(aplyNo);
                    ViewBag.bView = "N";
                }

                else
                {
                    authAppr = AuthApprDao.qryByFreeRole(userId);
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
                Dictionary<string, string> dicExecAction = sysCodeDao.qryByTypeDic("EXEC_ACTION");
                Dictionary<string, string> dicYNFlag = sysCodeDao.qryByTypeDic("YN_FLAG");

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
                        userData.isDisabled = StringUtil.toString(codeUserHis.IS_MAIL);

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
        public JsonResult qryUserRoleHis(string userId, string aplyNo)
        {
            CodeUserRoleHisDao codeUserRoleHisDao = new CodeUserRoleHisDao();

            try
            {
                List<CodeUserRoleModel> rows = new List<CodeUserRoleModel>();
                rows = codeUserRoleHisDao.qryByAplyNo(aplyNo);

                if (rows.Count == 0) {
                    CodeUserRoleDao codeUserRoleDao = new CodeUserRoleDao();
                    rows = codeUserRoleDao.qryByUserID(userId);
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
            string strConn = DbUtil.GetDBTreasuryConnStr();
            using (SqlConnection conn = new SqlConnection(strConn))
            {
                conn.Open();
                SqlTransaction transaction = conn.BeginTransaction("Transaction");

                try
                {
                    AuthApprDao AuthApprDao = new AuthApprDao();
                    AUTH_APPR authAppr = AuthApprDao.qryByKey(aplyNo);

                    if (StringUtil.toString(authAppr.CREATE_UID).Equals(Session["UserID"].ToString()))
                        return Json(new { success = false, errors = "覆核人員與申請人員相同，不可執行覆核作業!!" }, JsonRequestBehavior.AllowGet);


                    //異動使用者資料檔
                    string cExecType = "";
                    CodeUserHisDao codeUserHisDao = new CodeUserHisDao();
                    CodeUserDao codeUserDao = new CodeUserDao();
                    CODE_USER cODEUSERO = new CODE_USER();

                    CODE_USER_HIS codeUserHis = codeUserHisDao.qryByAplyNo(aplyNo);
                    string execAction = "";
                    if (codeUserHis != null)
                    {
                        execAction = StringUtil.toString(codeUserHis.EXEC_ACTION);
                    }

                    if ("A".Equals(execAction))  //新增使用者
                    {

                    }
                    else
                    {  //異動角色
                        cODEUSERO = codeUserDao.qryUserByKey(userId);
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

                            //新增稽核軌跡
                            procTrackLog("A", codeUserDao, cODEUSERO, conn, transaction);
                        }
                    }
                    else
                    {
                        //新增LOG
                        Log log = new Log();
                        log.CFUNCTION = "使用者管理-修改";
                        log.CACTION = "U";
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


                        int cnt = codeUserDao.Update(cODEUSERO, conn, transaction);

                    }


                    //覆核狀態=核可時
                    if ("2".Equals(apprStatus))
                        procUserRoleHis(cODEUSERO, aplyNo, conn, transaction); //異動使用者角色




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
                        CODE_USER_ROLE dRole = new CODE_USER_ROLE();
                        Log log = new Log();

                        switch (d.execAction) {
                            case "A":
                                dRole.USER_ID = cODEUSERO.USER_ID;
                                dRole.ROLE_ID = d.roleId;
                                dRole.CREATE_UID = cODEUSERO.LAST_UPDATE_UID;
                                dRole.CREATE_DT = cODEUSERO.LAST_UPDATE_DT;


                                //新增資料
                                codeUserRoleDao.insert(dRole, conn, transaction);


                                //新增LOG
                                log.CFUNCTION = "使用者管理(角色授權)-新增";
                                log.CACTION = "A";
                                log.CCONTENT = codeUserRoleDao.logContent(dRole);
                                LogDao.Insert(log, Session["UserID"].ToString());

                                break;
                            case "D":
                                dRole = codeUserRoleDao.qryByKey(cODEUSERO.USER_ID, d.roleId);

                                //新增LOG

                                log.CFUNCTION = "使用者管理(角色授權)-刪除";
                                log.CACTION = "D";
                                log.CCONTENT = codeUserRoleDao.logContent(dRole);
                                LogDao.Insert(log, Session["UserID"].ToString());

                                //刪除資料
                                codeUserRoleDao.delete(dRole, conn, transaction);
                                break;
                            default:
                                break;

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
            AUTH_APPR authAppr = new AUTH_APPR();
            authAppr.APLY_NO = aplyNo;
            authAppr.APPR_STATUS = appStatus;
            authAppr.APPR_UID = Session["UserID"].ToString();
            authAppr.APPR_DT = DateTime.Now;
            authAppr.LAST_UPDATE_UID = Session["UserID"].ToString();
            authAppr.LAST_UPDATE_DT = DateTime.Now;

            AuthApprDao authApprDao = new AuthApprDao();
            authApprDao.updateStatus(authAppr, conn, transaction);
        }

        /// <summary>
        /// 新增稽核軌跡
        /// </summary>
        /// <param name="codeUserDao"></param>
        /// <param name="codeUser"></param>
        /// <param name="conn"></param>
        /// <param name="transaction"></param>
        public void procTrackLog(string type, CodeUserDao codeUserDao, CODE_USER codeUser, SqlConnection conn, SqlTransaction transaction)
        {

            PIA_LOG_MAIN piaLog = new PIA_LOG_MAIN();
            piaLog.TRACKING_TYPE = "A";
            piaLog.ACCESS_ACCOUNT = Session["UserID"].ToString();
            piaLog.ACCOUNT_NAME = Session["UserName"].ToString();
            piaLog.PROGFUN_NAME = "UserReview";
            piaLog.ACCESSOBJ_NAME = "CodeUser";
            piaLog.EXECUTION_TYPE = type;
            piaLog.EXECUTION_CONTENT = codeUserDao.userLogContent(codeUser);
            piaLog.AFFECT_ROWS = 1;
            piaLog.PIA_OWNER1 = codeUser.USER_ID;
            piaLog.PIA_OWNER2 = "";
            piaLog.PIA_TYPE = "0100000000";


            PiaLogMainDao piaLogMainDao = new PiaLogMainDao();
            piaLogMainDao.Insert(piaLog, conn, transaction);

        }
    }
}