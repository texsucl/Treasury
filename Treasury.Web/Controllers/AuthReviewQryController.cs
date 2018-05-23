//using Treasury.WebActionFilter;
//using Treasury.WebBO;
//using Treasury.WebDaos;
//using Treasury.WebModels;
//using Treasury.WebUtils;
//using Treasury.WebViewModels;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Web;
//using System.Web.Mvc;

///// <summary>
///// 功能說明：覆核資料查詢作業
///// 初版作者：20170226 黃黛鈺
///// 修改歷程：20170226 黃黛鈺 
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
//    public class AuthReviewQryController : BaseController
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
//            String[] roleInfo = authUtil.chkUserFuncAuth(Session["AgentID"].ToString(), "~/AuthReviewQry/");
//            if (roleInfo != null && roleInfo.Length == 3)
//            {
//                opScope = roleInfo[0];
//                roleId = roleInfo[1];
//                funcType = roleInfo[2];
//            }

//            ViewBag.opScope = opScope;


//            /*---畫面下拉選單初始值---*/
//            SysCodeDao typeDefineDao = new SysCodeDao();

//            //覆核單種類
//            var ReviewTypeList = typeDefineDao.loadSelectList("reviewType");
//            ViewBag.ReviewTypeList = ReviewTypeList;

//            //覆核狀態
//            var ReviewFlagList = typeDefineDao.loadSelectList("reviewSts");
//            ViewBag.ReviewFlagList = ReviewFlagList;

//            //角色名稱
//            CodeRoleDao codeRoleDao = new CodeRoleDao();
//            var CodeRoleList = codeRoleDao.loadSelectList();
//            ViewBag.CodeRoleList = CodeRoleList;

//            //使用者名稱
//            CodeUserDao codeUserDao = new CodeUserDao();
//            var CodeUserList = codeUserDao.loadSelectList();
//            ViewBag.CodeUserList = CodeUserList;


//            return View();
//        }


//        /// <summary>
//        /// 執行"查詢"功能
//        /// </summary>
//        /// <param name="authReviewQryModel"></param>
//        /// <returns></returns>
//        [HttpPost]
//        public JsonResult LoadData(AuthReviewQryModel authReviewQryModel)
//        {
//            AuthReviewDao authReviewDao = new AuthReviewDao();
//            CodeRoleHisDao codeRoleHisDao = new CodeRoleHisDao();
//            CodeUserHisDao codeUserHisDao = new CodeUserHisDao();

//            using (DbAccountEntities db = new DbAccountEntities())
//            {
//                try
//                {
//                    List<AuthReviewModel> rows = new List<AuthReviewModel>();

//                    rows = authReviewDao.qryAuthReviewQry(authReviewQryModel, db);

//                    foreach (AuthReviewModel d in rows) {
//                        d.cCrtDate = StringUtil.toString(d.cCrtDate) == "" ? "": DateUtil.formatDateTimeDbToSc(d.cCrtDate, "DT");
//                        d.cReviewDate = StringUtil.toString(d.cReviewDate) == "" ? "" : DateUtil.formatDateTimeDbToSc(d.cReviewDate, "DT");


//                        if ("".Equals(StringUtil.toString(d.cMappingKeyDesc))) {
//                            if ("R".Equals(d.cReviewType))
//                            {
//                                CodeRoleHis codeRoleHis = codeRoleHisDao.qryByKey(d.cReviewSeq);
//                                d.cMappingKeyDesc = StringUtil.toString(codeRoleHis.cRoleName);
//                            }
//                            else {
//                                CodeUserHis codeUserHis = codeUserHisDao.qryByKey(d.cReviewSeq);
//                                d.cMappingKeyDesc = StringUtil.toString(codeUserHis.cUserName);
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
//        /// 開啟角色明細畫面
//        /// </summary>
//        /// <param name="cReviewSeq"></param>
//        /// <returns></returns>
//        public ActionResult detailRole(string cReviewSeq)
//        {
//            try
//            {
//                using (DbAccountEntities db = new DbAccountEntities())
//                {
//                    CodeRoleHisDao codeRoleHisDao = new CodeRoleHisDao();
//                    AuthReviewRoleModel roleData = codeRoleHisDao.qryByNowHis(cReviewSeq, db);

//                    AuthReviewDao authReviewDao = new AuthReviewDao();
//                    AuthReview preAuthReview = authReviewDao.qryPreData(cReviewSeq, StringUtil.toString(roleData.cRoleID));

//                    SysCodeDao typeDefineDao = new SysCodeDao();
//                    Dictionary<string, string> area = typeDefineDao.qryByTypeDic("OprArea");

//                    if (preAuthReview != null)
//                    {
//                        if (!"".Equals(preAuthReview.cMappingKey))
//                        {
//                            CodeRoleHis preRoleData = codeRoleHisDao.qryByKey(preAuthReview.cReviewSeq);
//                            if (preRoleData != null)
//                            {
//                                if (!"".Equals(preRoleData.cRoleID))
//                                {
//                                    roleData.cRoleName = preRoleData.cRoleName.Trim();
//                                    roleData.cSearchArea = area[preRoleData.cSearchArea.Trim()];
//                                    roleData.cOperatorArea = area[preRoleData.cOperatorArea.Trim()];
//                                    roleData.cFlag = preRoleData.cFlag.Trim() == "Y" ? "啟用" : "停用";
//                                    roleData.vMemo = preRoleData.vMemo.Trim();
//                                }
//                            }
//                        }
//                    }
//                    else
//                    {
//                        AuthReview nextAuthReview = authReviewDao.qryNextData(cReviewSeq, roleData.cRoleID.Trim());
//                        if (nextAuthReview != null)
//                        {
//                            roleData.cRoleName = "";
//                            roleData.cSearchArea = "";
//                            roleData.cOperatorArea = "";
//                            roleData.cFlag = "";
//                            roleData.vMemo = "";
//                        }

//                    }



//                    string[] cDateTime = roleData.cCrtDateTime.Split(' ');
//                    roleData.cCrtDateTime = DateUtil.formatDateTimeDbToSc(cDateTime[0] + " " + cDateTime[1], "DT");

//                    string[] cReviewDate = roleData.cReviewDate.Split(' ');
//                    roleData.cReviewDate = DateUtil.formatDateTimeDbToSc(cReviewDate[0] + " " + cReviewDate[1], "DT");

//                    ViewBag.bHaveData = "Y";
//                    ViewBag.cReviewSeq = cReviewSeq;
//                    return View(roleData);
//                }
//            }
//            catch (Exception e)
//            {
//                ViewBag.bHaveData = "N";
//                return View();
//            }
//        }

//        /// <summary>
//        /// 開啟使用者明細畫面
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

//                    AuthReviewDao authReviewDao = new AuthReviewDao();
//                    AuthReview preAuthReview = authReviewDao.qryPreData(cReviewSeq, StringUtil.toString(userData.cAgentID));

//                    if (preAuthReview != null)
//                    {
//                        if (!"".Equals(preAuthReview.cMappingKey))
//                        {
//                            CodeUserHis preUserData = codeUserHisDao.qryByKey(preAuthReview.cReviewSeq);
//                            if (preUserData != null)
//                            {
//                                if (!"".Equals(preUserData.cAgentID))
//                                {
//                                    userData.cUserName = preUserData.cUserName.Trim();
//                                    userData.cUserType = preUserData.cUserType.Trim() == "1" ? "是" : "否";
//                                    userData.cFlag = preUserData.cFlag.Trim() == "Y" ? "啟用" : "停用";
//                                    userData.vMemo = preUserData.vMemo.Trim();
//                                }
//                            }
//                        }
//                    }
//                    else {
//                        AuthReview nextAuthReview = authReviewDao.qryNextData(cReviewSeq, userData.cAgentID.Trim());
//                        if (nextAuthReview != null) {
//                            userData.cUserName = "";
//                            userData.cUserType = "";
//                            userData.cFlag = "";
//                            userData.vMemo = "";
//                        }

//                    }
                            


//                    string[] cDateTime = userData.cCrtDateTime.Split(' ');
//                    userData.cCrtDateTime = DateUtil.formatDateTimeDbToSc(cDateTime[0] + " " + cDateTime[1], "DT");

//                    string[] cReviewDate = userData.cReviewDate.Split(' ');
//                    userData.cReviewDate = DateUtil.formatDateTimeDbToSc(cReviewDate[0] + " " + cReviewDate[1], "DT");

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


//    }
//}