using Treasury.WebActionFilter;
using Treasury.WebBO;
using Treasury.WebDaos;
using Treasury.WebModels;
using Treasury.WebUtils;
using Treasury.WebViewModels;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Treasury.Web;
using Treasury.Web.Daos;
using Treasury.Web.ViewModels;
using Treasury.Web.Models;
using System.Transactions;

/// <summary>
/// 功能說明：使用者管理
/// 初版作者：20180514 黃黛鈺
/// 修改歷程：20180514 黃黛鈺 
///           需求單號：201803140070-00
///           初版
/// ==============================================
/// 修改日期/修改人：
/// 需求單號：
/// 修改內容：
/// ==============================================
/// </summary>

namespace Treasury.WebControllers
{

    [Authorize]
    [CheckSessionFilterAttribute]
    public class UserMgrController : BaseController
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

            String[] roleInfo = authUtil.chkUserFuncAuth(Session["UserID"].ToString(), "~/UserMgr/");
            if (roleInfo != null && roleInfo.Length == 1)
            {
                opScope = "1";
                //roleId = roleInfo[1];
                //funcType = roleInfo[2];
            }


            ViewBag.opScope = opScope;


            /*---畫面下拉選單初始值---*/
            SysCodeDao sysCodeDao = new SysCodeDao();


            //啟用狀態
            var isDisabledList = sysCodeDao.loadSelectList("IS_DISABLED");
            ViewBag.isDisabledList = isDisabledList;

            //是否寄送MAIL
            var isMailList = sysCodeDao.loadSelectList("YN_FLAG");
            ViewBag.isMailList = isMailList;

            //角色群組
            var roleAuthTypeList = sysCodeDao.loadSelectList("ROLE_AUTH_TYPE");
            ViewBag.roleAuthTypeList = roleAuthTypeList;

            //角色名稱
            CodeRoleDao codeRoleDao = new CodeRoleDao();
            var CodeRoleList = codeRoleDao.loadSelectList();
            ViewBag.CodeRoleList = CodeRoleList;

            //異動人員
            CodeUserDao codeUserDao = new CodeUserDao();
            var CodeUserList = codeUserDao.loadSelectList();
            ViewBag.CodeUserList = CodeUserList;

            

            return View();


        }



        [HttpGet]
        public ActionResult Index(String userId)
        {

            UserAuthUtil authUtil = new UserAuthUtil();

            String opScope = "";

            String[] roleInfo = authUtil.chkUserFuncAuth(Session["UserID"].ToString(), "~/UserMgr/");
            if (roleInfo != null && roleInfo.Length == 1)
            {
                opScope = "1";
                //roleId = roleInfo[1];
                //funcType = roleInfo[2];
            }


            ViewBag.opScope = opScope;

            /*---畫面下拉選單初始值---*/
            SysCodeDao sysCodeDao = new SysCodeDao();


            //啟用狀態
            var isDisabledList = sysCodeDao.loadSelectList("IS_DISABLED");
            ViewBag.isDisabledList = isDisabledList;

            //是否寄送MAIL
            var isMailList = sysCodeDao.loadSelectList("YN_FLAG");
            ViewBag.isMailList = isMailList;

            //角色群組
            var roleAuthTypeList = sysCodeDao.loadSelectList("ROLE_AUTH_TYPE");
            ViewBag.roleAuthTypeList = roleAuthTypeList;

            //角色名稱
            CodeRoleDao codeRoleDao = new CodeRoleDao();
            var CodeRoleList = codeRoleDao.loadSelectList();
            ViewBag.CodeRoleList = CodeRoleList;

            //異動人員
            CodeUserDao codeUserDao = new CodeUserDao();
            var CodeUserList = codeUserDao.loadSelectList();
            ViewBag.CodeUserList = CodeUserList;

            if (userId != null)
            {
                UserMgrModel userMgrModel = new UserMgrModel();
                userMgrModel.cUserID = userId;
                ViewBag.cUserID = userId;
                return View(userMgrModel);
            }
            else
            {
                return View();
            }
        }


        /// <summary>
        /// 主頁面查詢
        /// </summary>
        /// <param name="userMgrModel"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult LoadData(UserMgrModel userMgrModel)
        {

            List<UserMgrModel> rows = qryUserData(userMgrModel);

            CodeUserDao codeUserDao = new CodeUserDao();
            procTrackLog(userMgrModel, codeUserDao, rows.Count);

            var jsonData = new { success = true, rows };
            return Json(jsonData, JsonRequestBehavior.AllowGet);

        }

        private List<UserMgrModel>  qryUserData(UserMgrModel userMgrModel) {
            CodeUserDao codeUserDao = new CodeUserDao();
            List<UserMgrModel> rows = codeUserDao.qryUserMgr(userMgrModel);

            using (DB_INTRAEntities db = new DB_INTRAEntities())
            {
                OaEmpDao oaEmpDao = new OaEmpDao();
                for (int i = 0; i < rows.Count; i++)
                {
                    rows[i] = oaEmpDao.getUserOaData(rows[i], db);

                    Dictionary<string, string> userNameMap = new Dictionary<string, string>();


                    if (!"".Equals(StringUtil.toString(rows[i].cCrtUserID)))
                    {
                        if (!"".Equals(rows[i].cCrtUserID))
                        {
                            if (!userNameMap.ContainsKey(rows[i].cCrtUserID))
                            {
                                userNameMap = oaEmpDao.qryUsrName(userNameMap, rows[i].cCrtUserID, db);
                            }
                            rows[i].cCrtUserID = rows[i].cCrtUserID + " " + userNameMap[rows[i].cCrtUserID];
                        }

                    }


                    if (!"".Equals(StringUtil.toString(rows[i].cUpdUserID)))
                    {
                        if (!"".Equals(rows[i].cUpdUserID))
                        {
                            if (!userNameMap.ContainsKey(rows[i].cUpdUserID))
                            {
                                userNameMap = oaEmpDao.qryUsrName(userNameMap, rows[i].cUpdUserID, db);
                            }
                            rows[i].cUpdUserID = rows[i].cUpdUserID + " " + userNameMap[rows[i].cUpdUserID];
                        }

                    }


                    if (!"".Equals(StringUtil.toString(rows[i].apprUid)))
                    {

                        if (!"".Equals(rows[i].apprUid))
                        {
                            if (!userNameMap.ContainsKey(rows[i].apprUid))
                            {
                                userNameMap = oaEmpDao.qryUsrName(userNameMap, rows[i].apprUid, db);
                            }
                            rows[i].apprUid = rows[i].apprUid + " " + userNameMap[rows[i].apprUid];
                        }
                    }


                    if (!"".Equals(StringUtil.toString(rows[i].frezzeUid)))
                    {
                        if (!"".Equals(rows[i].frezzeUid))
                        {
                            if (!userNameMap.ContainsKey(rows[i].frezzeUid))
                            {
                                userNameMap = oaEmpDao.qryUsrName(userNameMap, rows[i].frezzeUid, db);
                            }
                            rows[i].frezzeUid = rows[i].frezzeUid + " " + userNameMap[rows[i].frezzeUid];
                        }
                    }


                }

            }


            bool bcUserName = StringUtil.isEmpty(userMgrModel.cUserName);
            if (!bcUserName)
            {

                rows = rows.Where(x => x.cUserName == userMgrModel.cUserName).ToList();
            }

            return rows;

        }


        /// <summary>
        /// 主頁面查詢紀錄至稽核軌跡
        /// </summary>
        /// <param name="userMgrModel"></param>
        /// <param name="codeUserDao"></param>
        /// <param name="cnt"></param>
        public void procTrackLog(UserMgrModel userMgrModel, CodeUserDao codeUserDao, int cnt)
        {
            string strConn = DbUtil.GetDBTreasuryConnStr();
            using (SqlConnection conn = new SqlConnection(strConn))
            {
                conn.Open();

                SqlTransaction transaction = conn.BeginTransaction("Transaction");

                try
                {
                    PIA_LOG_MAIN piaLog = new PIA_LOG_MAIN();
                    piaLog.TRACKING_TYPE = "A";
                    piaLog.ACCESS_ACCOUNT = Session["UserID"].ToString();
                    //piaLog.ACCOUNT_NAME = Session["UserName"].ToString();
                    piaLog.PROGFUN_NAME = "UserMgrController";
                    piaLog.ACCESSOBJ_NAME = "CodeUser";
                    piaLog.EXECUTION_TYPE = "Q";
                    piaLog.EXECUTION_CONTENT = codeUserDao.trackLogContent(userMgrModel);
                    piaLog.AFFECT_ROWS = cnt;
                    piaLog.PIA_OWNER1 = "";
                    piaLog.PIA_OWNER2 = "";
                    piaLog.PIA_TYPE = "0100000000";


                    PiaLogMainDao piaLogMainDao = new PiaLogMainDao();
                    piaLogMainDao.Insert(piaLog, conn, transaction);

                    transaction.Commit();
                }
                catch (Exception e)
                {
                    transaction.Rollback();
                    logger.Error("[procTrackLog]其它錯誤：" + e.ToString());
                }
            }
        }

        


        /// <summary>
        /// 使用者資訊codeUser
        /// </summary>
        /// <param name="cUserID"></param>
        /// <returns></returns>
        public ActionResult detailUser(string userId)
        {

            /*---畫面下拉選單初始值---*/
            SysCodeDao sysCodeDao = new SysCodeDao();


            //啟用狀態
            var isDisabledList = sysCodeDao.loadSelectList("IS_DISABLED");
            ViewBag.isDisabledList = isDisabledList;

            //是否寄送MAIL
            var isMailList = sysCodeDao.loadSelectList("YN_FLAG");
            ViewBag.isMailList = isMailList;


            //角色群組
            var roleAuthTypeList = sysCodeDao.jqGridList("ROLE_AUTH_TYPE");
            ViewBag.roleAuthTypeList = roleAuthTypeList;


            ////查詢使用者資訊
            //CodeUserDao codeUserDao = new CodeUserDao();
            //CODEUSER codeUser = codeUserDao.qryByKey(cUserID);


            ////查詢角色
            CodeRoleDao codeRoleDao = new CodeRoleDao();
            var roleStr = codeRoleDao.jqGridRoleList("");
            ViewBag.roleList = roleStr;



            //將值搬給畫面欄位
            UserMgrModel userMgrModel = new UserMgrModel();

            if ("".Equals(StringUtil.toString(userId)))
            {
                ViewBag.bHaveData = false;
                return View(userMgrModel);
            }


            userMgrModel.cUserID = userId;
            List<UserMgrModel> rows = qryUserData(userMgrModel);

            if (rows.Count > 0)
            {
                ViewBag.bHaveData = true;
                
                //return RedirectToAction("Index", "Home");
                return View(rows[0]);
            }
            else
            {
                ViewBag.bHaveData = false;
                return View(userMgrModel);
            }
        }



        /// <summary>
        /// 依"角色群組"取得對應的角色
        /// </summary>
        /// <param name="roleAuthType"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult qryRoleList(string roleAuthType)
        {
            try
            {
                CodeRoleDao codeRoleDao = new CodeRoleDao();
                var roleStr = codeRoleDao.jqGridRoleList(roleAuthType);
           

                return Json(new { success = true, roleList = roleStr });
            }
            catch (Exception e)
            {
                logger.Error("[qryEquip]:" + e.ToString());
                return Json(new { success = false, err = "其它錯誤，請洽系統管理員!!" });
            }
        }



        /// <summary>
        /// 取中文姓名
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public ActionResult getUserName(string userId)
        {
            string userName = "";
            OaEmpDao oaEmpDao = new OaEmpDao();
            V_EMPLY2 emp = new V_EMPLY2();
            using (DB_INTRAEntities dbIntra = new DB_INTRAEntities())
            {
                emp = oaEmpDao.qryByUsrId(userId, dbIntra);
                if (emp != null) {
                    userName = StringUtil.toString(emp.EMP_NAME);
                }

            }

            if ("".Equals(userName))
            {
                return Json(new { success = false, err = "無此帳號資料，不可新增!!" });
            }
            else {
                return Json(new { success = true, userName = userName });
            }

           
        }
        

        /// <summary>
        /// 異動使用者資訊
        /// </summary>
        /// <param name="userMgrModel"></param>
        /// <returns></returns>
        public JsonResult updateUser(UserMgrModel userMgrModel, List<CodeUserRoleModel> roleData, string execAction)
        {
            bool bUserChg = false;
            bool bRoleChg = false;

            

                    CodeUserDao codeUserDao = new CodeUserDao();
                    CODE_USER userO = codeUserDao.qryUserByKey(userMgrModel.cUserID);

                    if ("A".Equals(execAction))
                    {
                        if (userO != null) {
                            if (!"".Equals(StringUtil.toString(userO.USER_ID))) {
                                return Json(new { success = false, err = "使用者已存在系統，不可新增!!" }, JsonRequestBehavior.AllowGet);
                            }
                        }
                        bUserChg = true;
                    }
                    else {
                        if (userO == null)
                            return Json(new { success = false, err = "該使用者不存在系統!!" }, JsonRequestBehavior.AllowGet);
                        else {
                            if (StringUtil.toString(userMgrModel.isDisabled).Equals(StringUtil.toString(userO.IS_DISABLED))
                                && StringUtil.toString(userMgrModel.isMail).Equals(StringUtil.toString(userO.IS_MAIL))
                                && StringUtil.toString(userMgrModel.vMemo).Equals(StringUtil.toString(userO.MEMO))
                                )
                                bUserChg = false;
                            else
                                bUserChg = true;

                        }
                    }


                    //比對是否有異動"角色授權"
                    CodeUserRoleDao codeUserRoleDao = new CodeUserRoleDao();
                    List<CodeUserRoleModel> roleDataO = codeUserRoleDao.qryByUserID(userMgrModel.cUserID);
                    List<CodeUserRoleModel> roleList = new List<CodeUserRoleModel>();
                    if (roleData != null)
                    {
                        foreach (CodeUserRoleModel role in roleData)
                        {
                            CodeUserRoleModel codeUserRoleModel = new CodeUserRoleModel();
                            codeUserRoleModel.userId = StringUtil.toString(userMgrModel.cUserID);
                            codeUserRoleModel.roleId = StringUtil.toString(role.roleId);


                            if (roleDataO.Exists(x => x.roleId == role.roleId))
                            {
                                codeUserRoleModel.execAction = "";
                            }
                            else
                            {
                                bRoleChg = true;
                                codeUserRoleModel.execAction = "A";
                            }
                            roleList.Add(codeUserRoleModel);
                        }
                    }


                    foreach (CodeUserRoleModel oRole in roleDataO)
                    {
                        if (roleList != null)
                        {
                            if (!roleList.Exists(x => x.roleId == oRole.roleId))
                            {
                                bRoleChg = true;
                                CodeUserRoleModel codeUserRoleModel = new CodeUserRoleModel();
                                codeUserRoleModel.userId = StringUtil.toString(userMgrModel.cUserID);
                                codeUserRoleModel.roleId = StringUtil.toString(oRole.roleId);
                                codeUserRoleModel.execAction = "D";
                                roleList.Add(codeUserRoleModel);
                            }
                        }
                        else
                        {
                            bRoleChg = true;
                            CodeUserRoleModel codeUserRoleModel = new CodeUserRoleModel();
                            codeUserRoleModel.userId = StringUtil.toString(oRole.userId);
                            codeUserRoleModel.roleId = StringUtil.toString(oRole.roleId);
                            codeUserRoleModel.execAction = "D";
                            roleList.Add(codeUserRoleModel);
                        }
                    }

                    if (bUserChg == false && bRoleChg == false)
                        return Json(new { success = false, errors = "未異動畫面資料，將不進行修改覆核作業!!" }, JsonRequestBehavior.AllowGet);


            /*------------------ DB處理   begin------------------*/
            string strConn = DbUtil.GetDBTreasuryConnStr();
            using (SqlConnection conn = new SqlConnection(strConn))
            {

                conn.Open();

                SqlTransaction transaction = conn.BeginTransaction("Transaction");
                try
                {
                    AuthApprDao authApprDao = new AuthApprDao();
                    AUTH_APPR authAppr = new AUTH_APPR();
                    authAppr.AUTH_APLY_TYPE = "U";
                    authAppr.APPR_STATUS = "1";
                    authAppr.APPR_MAPPING_KEY = userMgrModel.cUserID;
                    authAppr.CREATE_UID = Session["UserID"].ToString();

                    //新增"覆核資料檔"
                    string aplyNo = authApprDao.insert(authAppr, conn, transaction);


                    // 異動"使用者資料檔"資料狀態
                    if (!"A".Equals(execAction)) {
                        Log log = new Log();
                        log.CFUNCTION = "使用者管理-修改";
                        log.CACTION = "U";
                        log.CCONTENT = codeUserDao.userLogContent(userO);
                        LogDao.Insert(log, Session["UserID"].ToString());


                        userO.DATA_STATUS = "2";
                        userO.LAST_UPDATE_UID = Session["UserID"].ToString();
                        userO.LAST_UPDATE_DT = DateTime.Now;
                        userO.FREEZE_UID = Session["UserID"].ToString();
                        userO.FREEZE_DT = DateTime.Now;

                        int cnt = codeUserDao.Update(userO, conn, transaction);
                    }


                    //處理使用者資料檔的異動
                    if (bUserChg) {
                        CodeUserHisDao codeUserHisDao = new CodeUserHisDao();
                        CODE_USER_HIS userHis = new CODE_USER_HIS();
                        userHis.APLY_NO = aplyNo;
                        userHis.USER_ID = userMgrModel.cUserID;
                        userHis.IS_DISABLED = userMgrModel.isDisabled;
                        userHis.IS_MAIL = userMgrModel.isMail;
                        userHis.MEMO = userMgrModel.vMemo;
                        if (!"A".Equals(execAction))
                        {
                            userHis.IS_DISABLED_B = userO.IS_DISABLED;
                            userHis.IS_MAIL_B = userO.IS_MAIL;
                            userHis.MEMO_B = userO.MEMO;
                            userHis.EXEC_ACTION = "U";
                        }
                        else
                            userHis.EXEC_ACTION = "A";

                        codeUserHisDao.insert(userHis, conn, transaction);
                    }


                    //處理角色金庫設備資料檔的異動
                    if (bRoleChg)
                    {
                        CodeUserRoleHisDao codeUserRoleHisDao = new CodeUserRoleHisDao();
                        foreach (CodeUserRoleModel role in roleList)
                        {
                            codeUserRoleHisDao.insert(aplyNo, role, conn, transaction);
                            //if (!"".Equals(role.execAction))
                            //{
                            //    codeUserRoleHisDao.insert(aplyNo, role, conn, transaction);
                            //}
                        }
                    }

                    transaction.Commit();

                    /*------------------ DB處理   end------------------*/
                    return Json(new { success = true, aplyNo = aplyNo });
                }
                catch (Exception e)
                {
                    transaction.Rollback();
                    logger.Error("[updateUser]其它錯誤：" + e.ToString());

                    return Json(new { success = false, err = "其它錯誤，請洽系統管理員!!" }, JsonRequestBehavior.AllowGet);
                }
            }
        }

        


        /// <summary>
        /// 查詢特定使用者的角色權限codeUserRole
        /// </summary>
        /// <param name="cAgentID"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult qryUserRole(string userId)
        {

            CodeUserRoleDao CodeUserRoleDao = new CodeUserRoleDao();
            List<CodeUserRoleModel> rows = CodeUserRoleDao.qryByUserID(userId);

            using (DB_INTRAEntities dbIntra = new DB_INTRAEntities())
            {
                Dictionary<string, string> userNameMap = new Dictionary<string, string>();
                OaEmpDao oaEmpDao = new OaEmpDao();
                string createUid = "";

                foreach (CodeUserRoleModel d in rows)
                {
                    createUid = StringUtil.toString(d.createUid);

                    if (!"".Equals(createUid))
                    {
                        if (!userNameMap.ContainsKey(createUid))
                        {
                            userNameMap = oaEmpDao.qryUsrName(userNameMap, createUid, dbIntra);
                        }
                        d.createUid = createUid + " " + userNameMap[createUid];
                    }

                }

            }



            var jsonData = new { success = true, roleList = rows };
            return Json(jsonData, JsonRequestBehavior.AllowGet);
        }




        [HttpGet]
        public ActionResult userHis(String cUserID)
        {

            /*---畫面下拉選單初始值---*/
            SysCodeDao sysCodeDao = new SysCodeDao();


            //覆核狀態
            var apprStatusList = sysCodeDao.loadSelectList("APPR_STATUS");
            apprStatusList = new SelectList(apprStatusList
                             .Where(x => x.Value != "4")
                             .ToList(),
                             "Value",
                             "Text");

            ViewBag.apprStatusList = apprStatusList;


            if (!"".Equals(StringUtil.toString(cUserID)))
            {
                UserMgrModel userMgrModel = new UserMgrModel();
                userMgrModel.cUserID = cUserID;
                List<UserMgrModel> rows = qryUserData(userMgrModel);


                ViewBag.cUserID = cUserID;
                return View(rows[0]);
            }
            else
            {
                return View();
            }
        }



        /// <summary>
        /// 查詢歷史異動資料
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="apprStatus"></param>
        /// <param name="updDateB"></param>
        /// <param name="updDateE"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult qryUserHisData(string userId, string apprStatus, string updDateB, string updDateE)
        {

            if ("".Equals(StringUtil.toString(userId)))
                return Json(new { success = false, err = "使用者帳號未輸入!!" });

            SysCodeDao sysCodeDao = new SysCodeDao();
            Dictionary<string, string> dicExecAction = sysCodeDao.qryByTypeDic("EXEC_ACTION");
            Dictionary<string, string> dicYNFlag = sysCodeDao.qryByTypeDic("YN_FLAG");
            Dictionary<string, string> dicApprStatus = sysCodeDao.qryByTypeDic("APPR_STATUS");
            Dictionary<string, string> dicIsDisabled = sysCodeDao.qryByTypeDic("IS_DISABLED");


            List<CodeUserHisModel> userHisList = new List<CodeUserHisModel>();
            List<UserRoleHisModel> userRoleHisList = new List<UserRoleHisModel>();


            CodeUserHisDao codeUserHisDao = new CodeUserHisDao();
            CodeUserRoleHisDao codeUserRoleHisDao = new CodeUserRoleHisDao();


            try
            {
                using (new TransactionScope(
                   TransactionScopeOption.Required,
                   new TransactionOptions
                   {
                       IsolationLevel = System.Transactions.IsolationLevel.ReadUncommitted
                   }))
                {

                    using (dbTreasuryEntities db = new dbTreasuryEntities())
                    {
                        userHisList = codeUserHisDao.qryForUserMgrHis(db, userId, apprStatus, updDateB, updDateE);

                        userRoleHisList = codeUserRoleHisDao.qryForUserMgrHis(db, userId, apprStatus, updDateB, updDateE);
                    }
                }


                using (DB_INTRAEntities dbIntra = new DB_INTRAEntities())
                {
                    Dictionary<string, string> userNameMap = new Dictionary<string, string>();
                    OaEmpDao oaEmpDao = new OaEmpDao();
                    string createUid = "";
                    string updId = "";

                    //處理角色資訊人員&代碼
                    if (userHisList != null)
                    {

                        foreach (CodeUserHisModel d in userHisList)
                        {
                            d.execActionDesc = dicExecAction.ContainsKey(StringUtil.toString(d.execAction)) ? dicExecAction[StringUtil.toString(d.execAction)] : "";
                            d.apprStatusDesc = dicApprStatus.ContainsKey(StringUtil.toString(d.apprStatus)) ? dicApprStatus[StringUtil.toString(d.apprStatus)] : "";

                            d.isDisabledDesc = dicIsDisabled.ContainsKey(StringUtil.toString(d.isDisabled)) ? dicIsDisabled[StringUtil.toString(d.isDisabled)] : "";
                            d.isDisabledDescB = dicIsDisabled.ContainsKey(StringUtil.toString(d.isDisabledB)) ? dicIsDisabled[StringUtil.toString(d.isDisabledB)] : "";

                            d.isMailDesc = dicYNFlag.ContainsKey(StringUtil.toString(d.isMail)) ? dicYNFlag[StringUtil.toString(d.isMail)] : "";
                            d.isMailDescB = dicYNFlag.ContainsKey(StringUtil.toString(d.isMailB)) ? dicYNFlag[StringUtil.toString(d.isMailB)] : "";


                            updId = StringUtil.toString(d.updateUid);
                            if (!"".Equals(updId))
                            {
                                if (!userNameMap.ContainsKey(updId))
                                {
                                    userNameMap = oaEmpDao.qryUsrName(userNameMap, updId, dbIntra);
                                }
                                d.updateUid = userNameMap[updId];
                            }
                        }

                    }


                    //處理使用者角色異動資訊人員&代碼
                    if (userRoleHisList != null)
                    {

                        foreach (UserRoleHisModel d in userRoleHisList)
                        {
                            d.execActionDesc = dicExecAction.ContainsKey(StringUtil.toString(d.execAction)) ? dicExecAction[StringUtil.toString(d.execAction)] : "";
                            d.apprStatusDesc = dicApprStatus.ContainsKey(StringUtil.toString(d.apprStatus)) ? dicApprStatus[StringUtil.toString(d.apprStatus)] : "";

                       

                            updId = StringUtil.toString(d.updateUid);
                            if (!"".Equals(updId))
                            {
                                if (!userNameMap.ContainsKey(updId))
                                {
                                    userNameMap = oaEmpDao.qryUsrName(userNameMap, updId, dbIntra);
                                }
                                d.updateUid = userNameMap[updId];
                            }
                        }

                    }



                }
                return Json(new
                {
                    success = true,
                    userHisList = userHisList,
                    userRoleHisList = userRoleHisList
                });


            }
            catch (Exception e)
            {
                logger.Error("[qryUserHisData]:" + e.ToString());
                return Json(new { success = false, err = "其它錯誤，請洽系統管理員!!" });
            }
        }



        [HttpPost]
        public ActionResult qryRoleAuth(string roleAuthType, string roleId)
        {
            List<authItem> rows = new List<authItem>();


            switch (roleAuthType) {
                case "F":
                    List<RoleFuncHisModel> rowsF = new List<RoleFuncHisModel>();
                    CodeRoleFunctionDao codeRoleFuncDao = new CodeRoleFunctionDao();
                    rowsF = codeRoleFuncDao.qryForAppr(roleId);

                    foreach (RoleFuncHisModel d in rowsF) {
                        authItem authItem = new authItem();
                        authItem.authId = d.cFunctionID;
                        authItem.authDesc = StringUtil.toString(d.cFunctionName);
                        rows.Add(authItem);
                    }
                    break;
                    //var jsonDataF = new { success = true, rows = rowsF };
                    //return Json(jsonDataF, JsonRequestBehavior.AllowGet);

                case "E":
                    List<CodeRoleEquipModel> rowsE = new List<CodeRoleEquipModel>();

                    CodeRoleTreaItemDao codeRoleTreaItemDao = new CodeRoleTreaItemDao();
                    rowsE = codeRoleTreaItemDao.qryForRoleMgr(roleId);

                    foreach (CodeRoleEquipModel d in rowsE)
                    {
                        authItem authItem = new authItem();
                        authItem.authId = d.treaEquipId;
                        authItem.authDesc = StringUtil.toString(d.equipName) + "；" + StringUtil.toString(d.controlModeDesc)
                             + "；" + StringUtil.toString(d.custodyModeDesc) + "；" + StringUtil.toString(d.custodyOrder);
                        rows.Add(authItem);
                    }

                    break;
                    //var jsonDataE = new { success = true, rows = rowsE };
                    //return Json(jsonDataE, JsonRequestBehavior.AllowGet);

                case "I":
                case "A":
                    List<CodeRoleItemModel> rowsI = new List<CodeRoleItemModel>();
                    CodeRoleItemDao codeRoleItemDao = new CodeRoleItemDao();
                    rowsI = codeRoleItemDao.qryForAppr(roleId, roleAuthType == "I" ? "1" : "2");

                    foreach (CodeRoleItemModel d in rowsI)
                    {
                        authItem authItem = new authItem();
                        authItem.authId = d.itemId;
                        authItem.authDesc = StringUtil.toString(d.itemDesc);
                        rows.Add(authItem);
                    }
                    break;

                    //var jsonDataI = new { success = true, rows = rowsI };
                    //return Json(jsonDataI, JsonRequestBehavior.AllowGet);


            }


            var jsonData = new { success = true, rows };
            return Json(jsonData, JsonRequestBehavior.AllowGet);


            return Json(new { success = false, err = "其它錯誤，請洽系統管理員!!" });
        }


        }

    internal class authItem
    {
        public string authId { get; set; }
        public string authDesc { get; set; }
    }
}