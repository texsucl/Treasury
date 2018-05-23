//using Treasury.WebActionFilter;
//using Treasury.WebBO;
//using Treasury.WebDaos;
//using Treasury.WebModels;
//using Treasury.WebUtils;
//using Treasury.WebViewModels;
//using System;
//using System.Collections.Generic;
//using System.Data.SqlClient;
//using System.Linq;
//using System.Web;
//using System.Web.Mvc;

///// <summary>
///// 功能說明：角色、使用者權限覆核作業
///// 初版作者：20170214 黃黛鈺
///// 修改歷程：20170214 黃黛鈺 
/////           需求單號：201801230413-01 
/////           初版
///// ==============================================
///// 修改日期/修改人：
///// 需求單號：
///// 修改內容：
///// ==============================================
///// </summary>
///// 
//namespace Treasury.WebControllers
//{
//    [Authorize]
//    [CheckSessionFilterAttribute]
//    public class AuthReviewController : BaseController
//    {
//        static private NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();


//        /// <summary>
//        /// 畫面初始
//        /// </summary>
//        /// <returns></returns>
//        public ActionResult Index()
//        {
//            UserAuthUtil authUtil = new UserAuthUtil();

//            String opScope = "";
//            String roleId = "";
//            String funcType = "";
//            String[] roleInfo = authUtil.chkUserFuncAuth(Session["AgentID"].ToString(), "~/AuthReview/");
//            if (roleInfo != null && roleInfo.Length == 3)
//            {
//                opScope = roleInfo[0];
//                roleId = roleInfo[1];
//                funcType = roleInfo[2];
//            }

//            ViewBag.opScope = opScope;


//            /*---畫面下拉選單初始值---*/
//            TypeDefineDao typeDefineDao = new TypeDefineDao();

//            //覆核單種類
//            var ReviewTypeList = typeDefineDao.loadSelectList("reviewType");
//            ViewBag.ReviewTypeList = ReviewTypeList;

//            //覆核狀態
//            var ReviewFlagList = typeDefineDao.loadSelectList("reviewSts");
//            ViewBag.ReviewFlagList = ReviewFlagList;

//            return View();
//        }


//        /// <summary>
//        /// 查詢待覆核的資料
//        /// </summary>
//        /// <param name="cReviewType"></param>
//        /// <returns></returns>
//        [HttpPost]
//        public JsonResult LoadData(String cReviewType)
//        {
//            AuthReviewDao authReviewDao = new AuthReviewDao();
//            CodeRoleDao codeRoleDao = new CodeRoleDao();
//            CodeUserDao codeUserDao = new CodeUserDao();

//            using (DbAccountEntities db = new DbAccountEntities())
//            {
//                try
//                {
//                    List<AuthReviewModel> rows = new List<AuthReviewModel>();

//                    //查出待覆核的資料
//                    rows = authReviewDao.qryAuthReview(cReviewType, "1", db);

//                     foreach (AuthReviewModel d in rows) {
//                        d.cCrtDate = StringUtil.toString(d.cCrtDate) == "" ? "": DateUtil.formatDateTimeDbToSc(d.cCrtDate, "DT");
//                        d.cReviewDate = StringUtil.toString(d.cReviewDate) == "" ? "" : DateUtil.formatDateTimeDbToSc(d.cReviewDate, "DT");


//                        if ("".Equals(StringUtil.toString(d.cMappingKeyDesc))) {
//                            if ("R".Equals(d.cReviewType))
//                            {
//                                CODEROLE codeRole = codeRoleDao.qryRoleByKey(StringUtil.toString(d.cMappingKey));
//                                if (codeRole != null) 
//                                    d.cMappingKeyDesc = StringUtil.toString(codeRole.CROLENAME);
//                            }
//                            else {
//                                CODEUSER codeUser = codeUserDao.qryByAgentId(StringUtil.toString(d.cMappingKey));
//                                if(codeUser != null)
//                                    d.cMappingKeyDesc = StringUtil.toString(codeUser.CUSERNAME);
//                            }
//                        }

//                    }
//                    var jsonData = new { success = true, rows };
//                    return Json(jsonData, JsonRequestBehavior.AllowGet);
//                }
//                catch (Exception e)
//                {
//                    logger.Error("其它錯誤：" + e.ToString());
//                    return Json(new { success = false, err = "其它錯誤，請洽系統管理員!!" });
//                }
//            }
//        }

//        /// <summary>
//        /// 開啟使用者修改明細畫面
//        /// </summary>
//        /// <param name="cReviewSeq"></param>
//        /// <returns></returns>
//        public ActionResult detailUser(string cReviewSeq)
//        {
//            try
//            {
//                using (DbAccountEntities db = new DbAccountEntities())
//                {
//                    CodeUserHisDao codeUserHisDao = new CodeUserHisDao();
//                    AuthReviewUserModel userData = codeUserHisDao.qryByNowHis(cReviewSeq, db);

//                    string[] cDateTime = userData.cCrtDateTime.Split(' ');
//                    userData.cCrtDateTime = DateUtil.formatDateTimeDbToSc(cDateTime[0] + " " + cDateTime[1], "DT");

//                    ViewBag.bHaveData = "Y";
//                    ViewBag.cReviewSeq = cReviewSeq;
//                    return View(userData);
//                }
//            }
//            catch (Exception e)
//            {
//                ViewBag.bHaveData = "N";
//                return View();
//            }
//        }


//        /// <summary>
//        /// 開啟角色修改明細畫面
//        /// </summary>
//        /// <param name="cReviewSeq"></param>
//        /// <returns></returns>
//        public ActionResult detailRole(string cReviewSeq)
//        {
//            try
//            {
//                using (DbAccountEntities db = new DbAccountEntities())
//                {


//                    CodeRoleHisDao CodeRoleHisDao = new CodeRoleHisDao();
//                    AuthReviewRoleModel roleData = CodeRoleHisDao.qryByNowHis(cReviewSeq, db);

//                    string[] cDateTime = roleData.cCrtDateTime.Split(' ');
//                    roleData.cCrtDateTime = DateUtil.formatDateTimeDbToSc(cDateTime[0] + " " + cDateTime[1], "DT");
//                    ////查詢CodeRoleHis - 角色資料異動檔

//                    //CodeRoleHis codeRoleHis = CodeRoleHisDao.qryByKey(cReviewSeq);

//                    ////查詢角色資料
//                    //CodeRoleDao codeRoleDao = new CodeRoleDao();
//                    //CODEROLE cODEROLE = codeRoleDao.qryRoleByKey(codeRoleHis.cRoleID.Trim());

//                    ViewBag.bHaveData = "Y";
//                    ViewBag.cReviewSeq = cReviewSeq;
//                    return View(roleData);

//                }
//            }
//            catch (Exception e) {
//                ViewBag.bHaveData = "N";
//                return View();
//            }
//        }


//        /// <summary>
//        /// 查詢角色功能異動明細資料
//        /// </summary>
//        /// <param name="cReviewSeq"></param>
//        /// <returns></returns>
//        [HttpPost]
//        public JsonResult qryRoleFuncHis(String cReviewSeq)
//        {
//            AuthReviewDao authReviewDao = new AuthReviewDao();

//            using (DbAccountEntities db = new DbAccountEntities())
//            {
//                try
//                {
//                    List<RoleFuncHisModel> rows = new List<RoleFuncHisModel>();

//                    rows = authReviewDao.qryRoleFuncHis(cReviewSeq, db);
//                    var jsonData = new { success = true, rows };
//                    return Json(jsonData, JsonRequestBehavior.AllowGet);
//                }
//                catch (Exception e)
//                {
//                    logger.Error("其它錯誤：" + e.ToString());
//                    return Json(new { success = false, err = "其它錯誤，請洽系統管理員!!" });
//                }
//            }
//        }

//        /// <summary>
//        /// 查詢使用者指派單位異動明細資料
//        /// </summary>
//        /// <param name="cReviewSeq"></param>
//        /// <returns></returns>
//        [HttpPost]
//        public JsonResult qryUserUnitHis(String cReviewSeq)
//        {
//            AuthReviewDao authReviewDao = new AuthReviewDao();

//            using (DbAccountEntities db = new DbAccountEntities())
//            {
//                try
//                {
//                    List<CodeUserMUnitHisModel> rows = new List<CodeUserMUnitHisModel>();

//                    rows = authReviewDao.qryUserUnitHis(cReviewSeq, db);
//                    var jsonData = new { success = true, rows };
//                    return Json(jsonData, JsonRequestBehavior.AllowGet);
//                }
//                catch (Exception e)
//                {
//                    logger.Error("其它錯誤：" + e.ToString());
//                    return Json(new { success = false, err = "其它錯誤，請洽系統管理員!!" });
//                }
//            }
//        }
        

//        /// <summary>
//        /// 查詢使用者角色異動明細資料
//        /// </summary>
//        /// <param name="cReviewSeq"></param>
//        /// <returns></returns>
//        [HttpPost]
//        public JsonResult qryUserRoleHis(String cReviewSeq)
//        {
//            AuthReviewDao authReviewDao = new AuthReviewDao();

//            using (DbAccountEntities db = new DbAccountEntities())
//            {
//                try
//                {
//                    List<UserRoleHisModel> rows = new List<UserRoleHisModel>();

//                    rows = authReviewDao.qryUserRoleHis(cReviewSeq, db);
//                    var jsonData = new { success = true, rows };
//                    return Json(jsonData, JsonRequestBehavior.AllowGet);
//                }
//                catch (Exception e)
//                {
//                    logger.Error("其它錯誤：" + e.ToString());
//                    return Json(new { success = false, err = "其它錯誤，請洽系統管理員!!" });
//                }
//            }
//        }




//        /// <summary>
//        /// 核可/退回(角色覆核)
//        /// </summary>
//        /// <param name="cReviewSeq"></param>
//        /// <param name="cReviewMemo"></param>
//        /// <param name="cReviewFlag"></param>
//        /// <returns></returns>
//        [HttpPost]
//        public JsonResult execReviewR(string cReviewSeq, string cRoleID, string cReviewMemo, string cReviewFlag)
//        {
//            string strConn = DbUtil.GetDBAccountConnStr();
//            using (SqlConnection conn = new SqlConnection(strConn))
//            {
//                conn.Open();
//                SqlTransaction transaction = conn.BeginTransaction("Transaction");

//                try
//                {
                    


//                    //異動角色資料檔
//                    string cExecType = "";
//                    CodeRoleHisDao codeRoleHisDao = new CodeRoleHisDao();
//                    CodeRoleDao codeRoleDao = new CodeRoleDao();
//                    CODEROLE cODEROLEO = new CODEROLE();

//                    CodeRoleHis codeRoleHis = codeRoleHisDao.qryByKey(cReviewSeq);  //本次是否有異動角色資料
//                    if (!"".Equals(codeRoleHis.cReviewSeq)) //有異動角色資料檔
//                    {
//                        cExecType = codeRoleHis.cExecType.Trim();

//                        if ("A".Equals(codeRoleHis.cExecType))  //新增角色
//                        {
                            
//                        }
//                        else {  //異動角色
//                            cODEROLEO = codeRoleDao.qryRoleByKey(cRoleID);
//                        }

//                    }
//                    else {
//                        cODEROLEO = codeRoleDao.qryRoleByKey(cRoleID);  //本次僅異動角色功能資料檔

//                    }


                    
//                    if ("A".Equals(cExecType))
//                    {
//                        if ("2".Equals(cReviewFlag)) {
//                            cODEROLEO.CROLENAME = codeRoleHis.cRoleName;
//                            cODEROLEO.COPERATORAREA = codeRoleHis.cOperatorArea;
//                            cODEROLEO.CSEARCHAREA = codeRoleHis.cSearchArea;
//                            cODEROLEO.CFLAG = "Y";
//                            cODEROLEO.VMEMO = StringUtil.toString(codeRoleHis.vMemo);
//                            cODEROLEO.CREVIEWFLAG = cReviewFlag;
//                            cODEROLEO.CCRTUSERID = Session["UserID"].ToString();
//                            cODEROLEO.CCRTUSERNAME = Session["UserName"].ToString();
//                            int cnt = codeRoleDao.Create(cODEROLEO);

//                            //新增LOG
//                            Log log = new Log();
//                            log.CFUNCTION = "角色管理-新增";
//                            log.CACTION = "A";
//                            log.CCONTENT = codeRoleDao.roleLogContent(cODEROLEO);
//                            LogDao.Insert(log, Session["UserID"].ToString());

//                            cRoleID = cODEROLEO.CROLEID;
//                        }
                            
//                    }
//                    else {
//                        //新增LOG
//                        Log log = new Log();
//                        log.CFUNCTION = "角色管理-修改";
//                        log.CACTION = "U";
//                        log.CCONTENT = codeRoleDao.roleLogContent(cODEROLEO);
//                        LogDao.Insert(log, Session["UserID"].ToString());

//                        cODEROLEO.CREVIEWFLAG = cReviewFlag;
//                        cODEROLEO.CUPDUSERID = Session["UserID"].ToString();
//                        cODEROLEO.CUPDUSERNAME = Session["UserName"].ToString();

//                        if ("U".Equals(cExecType) && "2".Equals(cReviewFlag))
//                        {
//                            cODEROLEO.CROLENAME = codeRoleHis.cRoleName.Trim();
//                            cODEROLEO.COPERATORAREA = codeRoleHis.cOperatorArea.Trim();
//                            cODEROLEO.CSEARCHAREA = codeRoleHis.cSearchArea.Trim();
//                            cODEROLEO.CFLAG = codeRoleHis.cFlag.Trim();
//                            cODEROLEO.VMEMO = codeRoleHis.vMemo.Trim();
//                        }
                        

//                        int cnt = codeRoleDao.Update(cODEROLEO, conn, transaction);

//                    }


//                    //異動角色功能(覆核狀態=核可時)
//                    if("2".Equals(cReviewFlag))
//                        procRoleFuncHis(cRoleID, cReviewSeq, conn, transaction);


//                    //異動覆核資料檔
//                    procAuthReview(cRoleID, cReviewSeq, cReviewFlag, cReviewMemo, conn, transaction);

//                    transaction.Commit();
//                    return Json(new { success = true });
//                }
//                catch (Exception e) {
//                    transaction.Rollback();
//                    logger.Error("[execReviewR]其它錯誤：" + e.ToString());

//                    return Json(new { success = false, errors = "其它錯誤，請洽系統管理員!!" }, JsonRequestBehavior.AllowGet);
//                }

//            }

//        }


//        /// <summary>
//        /// 核可/退回(使用者覆核)
//        /// </summary>
//        /// <param name="cReviewSeq"></param>
//        /// <param name="cAgentID"></param>
//        /// <param name="cReviewMemo"></param>
//        /// <param name="cReviewFlag"></param>
//        /// <returns></returns>
//        [HttpPost]
//        public JsonResult execReviewU(string cReviewSeq, string cAgentID, string cReviewMemo, string cReviewFlag)
//        {
//            string strConn = DbUtil.GetDBAccountConnStr();
//            using (SqlConnection conn = new SqlConnection(strConn))
//            {
//                conn.Open();
//                SqlTransaction transaction = conn.BeginTransaction("Transaction");

//                try
//                {

//                    //異動使用者資料檔
//                    bool bSuccess = procCodeUser(cReviewSeq, cAgentID, cReviewMemo, cReviewFlag, conn, transaction);
//                    if(!bSuccess)
//                       return Json(new { success = false, errors = "使用者已不存在員工檔!!" }, JsonRequestBehavior.AllowGet);


//                    //異動使用者角色(覆核狀態=核可時)
//                    if ("2".Equals(cReviewFlag))
//                        procUserRoleHis(cAgentID, cReviewSeq, conn, transaction);

//                    //異動使用者指派單位(覆核狀態=核可時)
//                    if ("2".Equals(cReviewFlag))
//                        procUserUnitHis(cAgentID, cReviewSeq, conn, transaction);


//                    //異動覆核資料檔
//                    procAuthReview(cAgentID, cReviewSeq, cReviewFlag, cReviewMemo, conn, transaction);

//                    transaction.Commit();
//                    return Json(new { success = true });
//                }
//                catch (Exception e)
//                {
//                    transaction.Rollback();
//                    logger.Error("[execReviewU]其它錯誤：" + e.ToString());

//                    return Json(new { success = false, errors = "其它錯誤，請洽系統管理員!!" }, JsonRequestBehavior.AllowGet);
//                }

//            }

//        }


//        /// <summary>
//        /// 異動使用者指派單位
//        /// </summary>
//        /// <param name="cAgentID"></param>
//        /// <param name="cReviewSeq"></param>
//        /// <param name="conn"></param>
//        /// <param name="transaction"></param>
//        private void procUserUnitHis(string cAgentID, string cReviewSeq, SqlConnection conn, SqlTransaction transaction)
//        {
//            CodeUserMaintainUnitHisDao codeUserUnitHisDao = new CodeUserMaintainUnitHisDao();
//            List<CodeUserMaintainUnitHis> userUnitList = codeUserUnitHisDao.qryBySeq(cReviewSeq);

//            if (userUnitList.Where(x => x.cExecType != "T").Count() == 0)
//                return;

//            CodeUserMaintainUnitDao codeUserUnitDao = new CodeUserMaintainUnitDao();

//            //刪除原使用者指派單位資料
//            codeUserUnitDao.delByAgentID(cAgentID, conn, transaction);

//            //將使用者指派單位異動檔資料寫入正式檔
//            foreach (CodeUserMaintainUnitHis d in userUnitList)
//            {
//                CODEUSERMAINTAINUNIT unit = new CODEUSERMAINTAINUNIT();
//                unit.CAGENTID = d.cAgentID.Trim();
//                unit.CUNITCODE = d.cUnitCode.Trim();
//                unit.CUNITSEQ = d.cUnitSeq.Trim();
//                unit.CENABLEDATE = d.cEnableDate.Trim();
//                unit.CDISABLEDATE = d.cDisableDate.Trim();
//                unit.CUNITNAME = d.cUnitName.Trim();
//                unit.COPRUSERID = d.cOprUserID.Trim();
//                unit.COPRUSERNAME = d.cOprUserName.Trim();
//                unit.COPRDATE = d.cOprDate.Trim();
//                unit.COPRTIME = d.cOprTime.Trim();

//                if (!"D".Equals(d.cExecType))
//                    codeUserUnitDao.insert(unit, conn, transaction);


//                if (!"T".Equals(d.cExecType))
//                {
//                    //新增LOG
//                    string cContent = writeUserUnitLog(d.cExecType, codeUserUnitDao, unit);

//                    //新增稽核軌跡
//                    writeUserUnitPIALog(d.cExecType, unit.CAGENTID, cContent, 1, conn, transaction);
//                }
//            }
//        }

//        /// <summary>
//        /// 將"使用者指派單位檔"資料紀錄至LOG
//        /// </summary>
//        /// <param name="caction"></param>
//        /// <param name="codeUserMaintainUnitDao"></param>
//        /// <param name="userUnit"></param>
//        /// <returns></returns>
//        private string writeUserUnitLog(string caction, CodeUserMaintainUnitDao codeUserMaintainUnitDao, CODEUSERMAINTAINUNIT userUnit)
//        {
//            Log log = new Log();

//            switch (caction)
//            {
//                case "A":
//                    log.CFUNCTION = "使用者管理-單位-新增";
//                    log.CACTION = "A";
//                    break;
//                case "D":
//                    log.CFUNCTION = "使用者管理-單位-刪除";
//                    log.CACTION = "D";
//                    break;
//                default:
//                    log.CFUNCTION = "使用者管理-單位-修改";
//                    log.CACTION = "U";
//                    break;

//            }

//            log.CCONTENT = codeUserMaintainUnitDao.logContent(userUnit);
//            LogDao.Insert(log, Session["UserID"].ToString());

//            return log.CCONTENT;

//        }


//        /// <summary>
//        /// 將"使用者指派單位檔"資料紀錄至稽核軌跡
//        /// </summary>
//        /// <param name="action"></param>
//        /// <param name="cAgentID"></param>
//        /// <param name="cContent"></param>
//        /// <param name="cnt"></param>
//        /// <param name="conn"></param>
//        /// <param name="transaction"></param>
//        private void writeUserUnitPIALog(string action, string cAgentID, string cContent, int cnt, SqlConnection conn, SqlTransaction transaction)
//        {

//            PiaLogMainDao piaLogMainDao = new PiaLogMainDao();
//            PIA_LOG_MAIN piaLog = new PIA_LOG_MAIN();

//            piaLog.TRACKING_TYPE = "C";
//            piaLog.ACCESS_ACCOUNT = Session["UserID"].ToString();
//            piaLog.ACCOUNT_NAME = Session["UserName"].ToString();
//            piaLog.PROGFUN_NAME = "AuthReview";
//            piaLog.ACCESSOBJ_NAME = "CodeUserMaintainUnit";
//            piaLog.EXECUTION_TYPE = action == "U" ? "E" : action;
//            piaLog.EXECUTION_CONTENT = cContent;
//            piaLog.AFFECT_ROWS = cnt;
//            piaLog.PIA_OWNER1 = cAgentID;
//            piaLog.PIA_OWNER2 = "";
//            piaLog.PIA_TYPE = "0100000000";

//            piaLogMainDao.Insert(piaLog, conn, transaction);

//        }


//        /// <summary>
//        /// 異動使用者角色檔
//        /// </summary>
//        /// <param name="cAgentID"></param>
//        /// <param name="cReviewSeq"></param>
//        /// <param name="conn"></param>
//        /// <param name="transaction"></param>
//        private void procUserRoleHis(string cAgentID, string cReviewSeq, SqlConnection conn, SqlTransaction transaction) {
//            CodeUserRoleHisDao codeUserRoleHisDao = new CodeUserRoleHisDao();
//            List<CodeUserRoleHis> userRoleList = codeUserRoleHisDao.qryBySeq(cReviewSeq);

//            if (userRoleList.Where(x => x.cExecType != "T").Count() == 0)
//                return;

//            CodeUserRoleDao codeUserRoleDao = new CodeUserRoleDao();

//            //刪除原使用者角色資料
//            codeUserRoleDao.delByAgentID(cAgentID, conn, transaction);

//            //將使用者角色異動檔資料寫入正式檔
//            foreach (CodeUserRoleHis d in userRoleList) {
//                CODEUSERROLE user = new CODEUSERROLE();
//                user.CAGENTID = d.cAgentID.Trim();
//                user.CROLEID = d.cRoleID.Trim();
//                user.CENABLEDATE = d.cEnableDate.Trim();
//                user.CDISABLEDATE = d.cDisableDate.Trim();
//                user.COPRUSERID = d.cOprUserID.Trim();
//                user.COPRUSERNAME = d.cOprUserName.Trim();
//                user.COPRDATE = d.cOprDate.Trim();
//                user.COPRTIME = d.cOprTime.Trim();

//                if (!"D".Equals(d.cExecType))
//                    codeUserRoleDao.insert(user, conn, transaction);


//                if (!"T".Equals(d.cExecType)) {
//                    //新增LOG
//                    string cContent = writeUserRoleLog(d.cExecType, codeUserRoleDao, user);

//                    //新增稽核軌跡
//                    writeUserRolePIALog(d.cExecType, user.CAGENTID, cContent, 1, conn, transaction);
//                }
//            }
//        }


//        /// <summary>
//        /// 將"使用者角色檔"資料紀錄至LOG
//        /// </summary>
//        /// <param name="caction"></param>
//        /// <param name="codeUserRoleDao"></param>
//        /// <param name="userRole"></param>
//        /// <returns></returns>
//        private string writeUserRoleLog(string caction, CodeUserRoleDao codeUserRoleDao, CODEUSERROLE userRole)
//        {
//            Log log = new Log();

//            switch (caction)
//            {
//                case "A":
//                    log.CFUNCTION = "使用者管理-角色-新增";
//                    log.CACTION = "A";
//                    break;
//                case "D":
//                    log.CFUNCTION = "使用者管理-角色-刪除";
//                    log.CACTION = "D";
//                    break;
//                default:
//                    log.CFUNCTION = "使用者管理-角色-修改";
//                    log.CACTION = "U";
//                    break;

//            }

//            log.CCONTENT = codeUserRoleDao.logContent(userRole);
//            LogDao.Insert(log, Session["UserID"].ToString());

//            return log.CCONTENT;

//        }



//        /// <summary>
//        /// 將"使用者角色檔"資料紀錄至稽核軌跡
//        /// </summary>
//        /// <param name="action"></param>
//        /// <param name="cAgentID"></param>
//        /// <param name="cContent"></param>
//        /// <param name="cnt"></param>
//        /// <param name="conn"></param>
//        /// <param name="transaction"></param>
//        private void writeUserRolePIALog(string action, string cAgentID, string cContent, int cnt, SqlConnection conn, SqlTransaction transaction)
//        {

//            PiaLogMainDao piaLogMainDao = new PiaLogMainDao();
//            PIA_LOG_MAIN piaLog = new PIA_LOG_MAIN();

//            piaLog.TRACKING_TYPE = "C";
//            piaLog.ACCESS_ACCOUNT = Session["UserID"].ToString();
//            piaLog.ACCOUNT_NAME = Session["UserName"].ToString();
//            piaLog.PROGFUN_NAME = "AuthReview";
//            piaLog.ACCESSOBJ_NAME = "CodeUserRole";
//            piaLog.EXECUTION_TYPE = action == "U" ? "E" : action;
//            piaLog.EXECUTION_CONTENT = cContent;
//            piaLog.AFFECT_ROWS = cnt;
//            piaLog.PIA_OWNER1 = cAgentID;
//            piaLog.PIA_OWNER2 = "";
//            piaLog.PIA_TYPE = "0100000000";

//            piaLogMainDao.Insert(piaLog, conn, transaction);

//        }


//        /// <summary>
//        /// 異動"使用者資料檔"
//        /// </summary>
//        /// <param name="cReviewSeq"></param>
//        /// <param name="cAgentID"></param>
//        /// <param name="cReviewMemo"></param>
//        /// <param name="cReviewFlag"></param>
//        /// <param name="conn"></param>
//        /// <param name="transaction"></param>
//        /// <returns></returns>
//        private bool procCodeUser(string cReviewSeq, string cAgentID, string cReviewMemo, string cReviewFlag
//            , SqlConnection conn, SqlTransaction transaction) {
//            string cExecType = "";
//            CodeUserHisDao codeUserHisDao = new CodeUserHisDao();
//            CodeUserDao codeUserDao = new CodeUserDao();
//            CODEUSER cODEUSERO = new CODEUSER();

//            CodeUserHis codeUserHis = codeUserHisDao.qryByKey(cReviewSeq);  //本次是否有異動使用者資料
//            if (codeUserHis != null) //有異動使用者資料檔
//            {
//                cExecType = codeUserHis.cExecType.Trim();

//                if ("A".Equals(codeUserHis.cExecType))  //新增使用者
//                {

//                }
//                else
//                {  //異動使用者
//                    cODEUSERO = codeUserDao.qryByAgentId(cAgentID);
//                }

//            }
//            else
//            {
//                cODEUSERO = codeUserDao.qryByAgentId(cAgentID);  //本次僅異動使用者角色資料檔或使用者指派單位

//            }



//            if ("A".Equals(cExecType))
//            {
//                if ("2".Equals(cReviewFlag))
//                {
//                    UserAuthUtil userAuthUtil = new UserAuthUtil();
//                    cODEUSERO = userAuthUtil.qryUserUnit(cODEUSERO, codeUserHis.cUserType, codeUserHis.cAgentID);

//                    if (cODEUSERO == null)
//                        return false;

//                    cODEUSERO.CAGENTID = codeUserHis.cAgentID.Trim();
//                    cODEUSERO.CUSERTYPE = codeUserHis.cUserType.Trim();
//                    cODEUSERO.CFLAG = codeUserHis.cFlag.Trim();
//                    cODEUSERO.CUSERID = codeUserHis.cUserID.Trim();
//                    cODEUSERO.CUSERNAME = codeUserHis.cUserName.Trim();
//                    cODEUSERO.VMEMO = StringUtil.toString(codeUserHis.vMemo).Trim();
//                    cODEUSERO.CCRTUSERID = Session["UserID"].ToString();
//                    cODEUSERO.CCRTUSERNAME = Session["UserName"].ToString();
//                    cODEUSERO.CREVIEWFLAG = "2";

//                    cODEUSERO = codeUserDao.Create(cODEUSERO, conn, transaction);


//                    //新增LOG
//                    Log log = new Log();
//                    log.CFUNCTION = "使用者管理-新增";
//                    log.CACTION = "A";
//                    log.CCONTENT = codeUserDao.userLogContent(cODEUSERO);
//                    LogDao.Insert(log, Session["UserID"].ToString());

//                    //新增稽核軌跡
//                    procTrackLog("A", codeUserDao, cODEUSERO, conn, transaction);

//                }


//            }
//            else
//            {
//                //新增LOG
//                Log log = new Log();
//                log.CFUNCTION = "使用者管理-修改";
//                log.CACTION = "U";
//                log.CCONTENT = codeUserDao.userLogContent(cODEUSERO);
//                LogDao.Insert(log, Session["UserID"].ToString());

//                cODEUSERO.CREVIEWFLAG = cReviewFlag;
//                cODEUSERO.CUPDUSERID = Session["UserID"].ToString();
//                cODEUSERO.CUPDUSERNAME = Session["UserName"].ToString();

//                if ("U".Equals(cExecType) && "2".Equals(cReviewFlag))
//                {

//                    cODEUSERO.CUSERNAME = codeUserHis.cUserName.Trim();
//                    cODEUSERO.CUSERTYPE = codeUserHis.cUserType.Trim();
//                    cODEUSERO.CFLAG = codeUserHis.cFlag.Trim();
//                    cODEUSERO.VMEMO = codeUserHis.vMemo.Trim();
//                }

//                cODEUSERO = codeUserDao.updateUserInfo(cODEUSERO, conn, transaction);

//            }

//            return true;
//        }

//        /// <summary>
//        /// 查詢角色功能異動清單
//        /// </summary>
//        /// <param name="cRoleID"></param>
//        /// <param name="cReviewSeq"></param>
//        /// <param name="conn"></param>
//        /// <param name="transaction"></param>
//        private void procRoleFuncHis(string cRoleID, string cReviewSeq, SqlConnection conn, SqlTransaction transaction) {
//            CodeRoleFuncHisDao codeRoleFuncHisDao = new CodeRoleFuncHisDao();
//            List<CodeRoleFunctionHis> cRoleFuncList = codeRoleFuncHisDao.qryBySeq(cReviewSeq);
//            if (cRoleFuncList != null)
//            {
//                if (cRoleFuncList.Count > 0)
//                {
//                    CodeRoleFunctionDao roleFuncDao = new CodeRoleFunctionDao();

//                    foreach (CodeRoleFunctionHis d in cRoleFuncList)
//                    {
//                        if ("A".Equals(d.cExecType))
//                        {
//                            CODEROLEFUNCTION dFunc = new CODEROLEFUNCTION();
//                            dFunc.CROLEID = cRoleID;
//                            dFunc.CFUNCTIONID = d.cFunctionID;
//                            dFunc.COPRUSERID = Session["UserID"].ToString();
//                            dFunc.COPRUSERNAME = Session["UserName"].ToString();
//                            dFunc.COPRDATE = DateUtil.getCurDate("yyyyMMdd");
//                            dFunc.COPRTIME = DateUtil.getCurDate("HHmmss");

//                            //新增資料
//                            roleFuncDao.Insert(dFunc, conn, transaction);


//                            //新增LOG
//                            Log log = new Log();
//                            log.CFUNCTION = "角色管理(功能授權)-新增";
//                            log.CACTION = "A";
//                            log.CCONTENT = roleFuncDao.logContent(dFunc);
//                            LogDao.Insert(log, Session["UserID"].ToString());
//                        }
//                        else
//                        {
//                            CODEROLEFUNCTION dFunc = roleFuncDao.getFuncRoleByKey(cRoleID, d.cFunctionID);


//                            //新增LOG
//                            Log log = new Log();
//                            log.CFUNCTION = "角色管理(功能授權)-刪除";
//                            log.CACTION = "D";
//                            log.CCONTENT = roleFuncDao.logContent(dFunc);
//                            LogDao.Insert(log, Session["UserID"].ToString());

//                            //刪除資料
//                            roleFuncDao.Delete(dFunc, conn, transaction);

//                        }

//                    }

//                }
//            }
//        }


//        /// <summary>
//        /// 異動覆核資料檔
//        /// </summary>
//        /// <param name="cReviewSeq"></param>
//        /// <param name="cReviewFlag"></param>
//        /// <param name="cReviewMemo"></param>
//        /// <param name="conn"></param>
//        /// <param name="transaction"></param>
//        private void procAuthReview(string cMappingKey, string cReviewSeq, string cReviewFlag, string cReviewMemo, SqlConnection conn, SqlTransaction transaction)
//        {
//            AuthReview authReview = new AuthReview();
//            authReview.cReviewSeq = cReviewSeq;
//            authReview.cReviewFlag = cReviewFlag;
//            authReview.cMappingKey = cMappingKey;
//            authReview.cReviewMemo = StringUtil.toString(cReviewMemo).Trim();
//            authReview.cReviewUserID = Session["UserID"].ToString();
//            authReview.cReviewUserName = Session["UserName"].ToString();
//            authReview.cUpdUserID = Session["UserID"].ToString();
//            authReview.cUpdUserName = Session["UserName"].ToString();

//            AuthReviewDao authReviewDao = new AuthReviewDao();
//            authReviewDao.updateFlag(authReview, conn, transaction);
//        }

//        /// <summary>
//        /// 新增稽核軌跡
//        /// </summary>
//        /// <param name="codeUserDao"></param>
//        /// <param name="codeUser"></param>
//        /// <param name="conn"></param>
//        /// <param name="transaction"></param>
//        public void procTrackLog(string type, CodeUserDao codeUserDao, CODEUSER codeUser, SqlConnection conn, SqlTransaction transaction)
//        {

//            PIA_LOG_MAIN piaLog = new PIA_LOG_MAIN();
//            piaLog.TRACKING_TYPE = "A";
//            piaLog.ACCESS_ACCOUNT = Session["UserID"].ToString();
//            piaLog.ACCOUNT_NAME = Session["UserName"].ToString();
//            piaLog.PROGFUN_NAME = "AuthReview";
//            piaLog.ACCESSOBJ_NAME = "CodeUser";
//            piaLog.EXECUTION_TYPE = type;
//            piaLog.EXECUTION_CONTENT = codeUserDao.userLogContent(codeUser);
//            piaLog.AFFECT_ROWS = 1;
//            piaLog.PIA_OWNER1 = codeUser.CAGENTID;
//            piaLog.PIA_OWNER2 = "";
//            piaLog.PIA_TYPE = "0100000000";


//            PiaLogMainDao piaLogMainDao = new PiaLogMainDao();
//            piaLogMainDao.Insert(piaLog, conn, transaction);

//        }
//    }
//}