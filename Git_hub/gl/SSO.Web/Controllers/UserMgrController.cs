using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web.Mvc;
using SSO.Web;
using SSO.Web.Daos;
using SSO.Web.ViewModels;
using SSO.Web.Models;
using System.Transactions;
using SSO.Web.ActionFilter;
using SSO.Web.BO;
using SSO.Web.Utils;
using SSO.WebViewModels;

/// <summary>
/// 功能說明：使用者管理
/// 初版作者：20180514 黃黛鈺
/// 修改歷程：20180514 黃黛鈺 
///           需求單號：201803140070-00
///           初版
/// ==============================================
/// 修改日期/修改人：20200110 B0077 黃黛鈺
/// 需求單號：201911280279-00
/// 修改內容：1.使用者點選『使用者維護作業』時，需先以登入者的所屬單位，檢查【可授權程式table檔.管理者所屬部門】 是否有對應資料，若無，則無法作業,需出錯誤訊息”未建管理者可授權程式table檔,請先建檔!”
///           2.「授權單位」改為「管理者單位」，以登入者的所屬單位帶出。
///           3.畫面『使用者資訊』
///            3.1 帳號的建立、停用
///             3.1.1 可維護的條件：使用者的單位，屬登入者被授權管理單位對應的程式OWNER單位。 
///             3.1.2 當使用者設停用時不需判讀人資檔案,停用核准時同時刪除該使用者下所有角色及程式。(於覆核作業核可時，才真正刪除資料)     
///            3.2 角色的授權
///             3.2.1 可維護的角色OWNER單位為登入者的所屬單位帶出對應可處理的程式OWNER單位。
///             3.2.2 次申請的角色對應的管理單位需相同，若不相同，將提示錯誤訊息。
/// ==============================================
/// </summary>

namespace SSO.WebControllers
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
            pageIni();
            return View();
        }



        [HttpGet]
        public ActionResult Index(String userId)
        {
            pageIni();

            if (userId != null)
            {
                UserMgrModel userMgrModel = new UserMgrModel();
                userMgrModel.cUserID = userId;
                ViewBag.cUserID = userId;
                return View(userMgrModel);
            }
            else
                return View();
        }


        /// <summary>
        /// 畫面初始相關值
        /// </summary>
        private void pageIni() {
            UserAuthUtil authUtil = new UserAuthUtil();

            string opScope = "";
            string funcName = "";

            String[] roleInfo = authUtil.chkUserFuncAuth(Session["UserID"].ToString(), "~/UserMgr/");
            if (roleInfo != null && roleInfo.Length == 2)
            {
                opScope = "1";
                funcName = roleInfo[1];
            }


            ViewBag.opScope = opScope;
            ViewBag.funcName = funcName;

            /*---畫面下拉選單初始值---*/
            SysCodeDao sysCodeDao = new SysCodeDao();


            //啟用狀態
            var isDisabledList = sysCodeDao.loadSelectList("SSO", "IS_DISABLED", false);
            ViewBag.isDisabledList = isDisabledList;

            //是否寄送MAIL
            var isMailList = sysCodeDao.loadSelectList("SSO", "YN_FLAG", false);
            ViewBag.isMailList = isMailList;

            //角色名稱
            CodeRoleDao codeRoleDao = new CodeRoleDao();
            var CodeRoleList = codeRoleDao.loadSelectList(qryAuthUnit(), true, "Y");   //modify by daiyu 20200110
            ViewBag.CodeRoleList = CodeRoleList;

            //異動人員
            CodeUserDao codeUserDao = new CodeUserDao();
            var CodeUserList = codeUserDao.loadSelectList();
            ViewBag.CodeUserList = CodeUserList;


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

        }

        /// <summary>
        /// 依登入者的單位查詢對應的程式授權OWNER單位
        /// </summary>
        /// <returns></returns>
        private string[] qryAuthUnit()
        {
            CodeAuthMgrDao codeAuthMgrDao = new CodeAuthMgrDao();
            string[] authUnitArr = codeAuthMgrDao.qryAuthUnit(Session["UserUnit"].ToString());

            return authUnitArr;
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

            //判斷使用者單位需與畫面相同
            bool bQryUnit = StringUtil.isEmpty(userMgrModel.qryUnit);
            if (!bQryUnit) {
                OaDeptDao oaDeptDao = new OaDeptDao();
                List<UnitInfoModel> dept = oaDeptDao.qryDept(userMgrModel.qryUnit);
                //可查到的使用者
                //1.與登入者屬同"部"
                //2.使用者的建立單位為登入者的單位(科)
                //20190225修改只可以看到符合查詢單位的資料
                rows = rows.Where(x => userMgrModel.qryUnit.Contains(x.cWorkUnitCode)).ToList();
            }
            


            //bool bAuthUnit = StringUtil.isEmpty(userMgrModel.authUnit);

            //OaDeptDao oaDeptDao = new OaDeptDao();
            //List<UnitInfoModel> dept = oaDeptDao.qryDept(userMgrModel.qryUnit);

            //if (!bAuthUnit)
            //{
            //    //可查到的使用者
            //    //1.與登入者屬同"部"
            //    //2.使用者的建立單位為登入者的單位(科)
            //    rows = rows.Where(x => x.cWorkUnitCode == userMgrModel.authUnit 
            //    || dept.Select(d => d.unitCode).Contains(x.cWorkUnitCode)
            //    || x.cCrtUnit == userMgrModel.authUnit
            //    )
            //    .ToList();
            //}

            //判斷使用者姓名需與畫面相同
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
            string strConn = DbUtil.GetDBFglConnStr();
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
            var isDisabledList = sysCodeDao.loadSelectList("SSO", "IS_DISABLED", false);
            ViewBag.isDisabledList = isDisabledList;

            //是否寄送MAIL
            var isMailList = sysCodeDao.loadSelectList("SSO", "YN_FLAG", false);
            ViewBag.isMailList = isMailList;


            ////查詢使用者資訊
            //CodeUserDao codeUserDao = new CodeUserDao();
            //CODEUSER codeUser = codeUserDao.qryByKey(cUserID);

            //執行功能
            var execActionjqList = sysCodeDao.jqGridList("SSO", "EXEC_ACTION", false);
            ViewBag.execActionjqList = execActionjqList;


            //查詢角色
            CodeRoleDao codeRoleDao = new CodeRoleDao();
            var roleStr = codeRoleDao.jqGridRoleList(qryAuthUnit(), true, "Y");
            ViewBag.roleList = roleStr;

            var roleAllStr = roleStr;
            CodeUserRoleDao CodeUserRoleDao = new CodeUserRoleDao();
            List<CodeUserRoleModel> roles = CodeUserRoleDao.qryByUserID(userId);
            foreach (CodeUserRoleModel d in roles) {
                if(roleStr.IndexOf(d.roleId) < 0)
                    roleAllStr += ";" + StringUtil.toString(d.roleId) + ":" + StringUtil.toString(d.roleName) ;
            }
            ViewBag.roleAllList = roleAllStr;



            //角色所屬單位
            string[] roleItemArr = roleAllStr.Split(';');
            string[] roleList = new string[roleItemArr.Length];
     
            if (roleItemArr != null) {
                for (int i = 0; i < roleItemArr.Length; i++)
                    roleList[i] = StringUtil.toString(roleItemArr[i].Split(':')[0]);
            }
           

            var roleAuthUnitStr = codeRoleDao.jqGridRoleAuthUnitList(roleList);
            ViewBag.roleAuthUnitList = roleAuthUnitStr;




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
                if (userO != null)
                {
                    if (!"".Equals(StringUtil.toString(userO.USER_ID)))
                        return Json(new { success = false, err = "使用者已存在系統，不可新增!!" }, JsonRequestBehavior.AllowGet);

                }
                bUserChg = true;
            }
            else
            {
                if (userO == null)
                    return Json(new { success = false, err = "該使用者不存在系統!!" }, JsonRequestBehavior.AllowGet);
                else
                {
                    if (StringUtil.toString(userMgrModel.isDisabled).Equals(StringUtil.toString(userO.IS_DISABLED))
                        && StringUtil.toString(userMgrModel.isMail).Equals(StringUtil.toString(userO.IS_MAIL)))
                        bUserChg = false;
                    else
                        bUserChg = true;

                }
            }

            UserMgrModel userN = new UserMgrModel();
            using (DB_INTRAEntities db = new DB_INTRAEntities())
            {
                OaEmpDao oaEmpDao = new OaEmpDao();
                userN = oaEmpDao.getUserOaData(userMgrModel, db);
            }


            //比對是否有異動"角色授權"
            CodeUserRoleDao codeUserRoleDao = new CodeUserRoleDao();
            List<CodeUserRoleModel> roleDataO = codeUserRoleDao.qryByUserID(userMgrModel.cUserID);
            List<CodeUserRoleModel> roleList = new List<CodeUserRoleModel>();

            string authUnit = "";
            if (roleData != null)
            {
                string[] authUnitArr = qryAuthUnit();

                //若屬允許其它單位授權的資料(只有授權管理相關功能)，需檢查要異動使用者的單位...是否為登錄者可授權的單位
                bool bPass = false;

                if (!authUnitArr.Contains(roleData[0].roleAuthUnit))
                {
                    if(roleData[0].roleAuthUnit.LastIndexOf("-") >= 0)
                        bPass = true;

                    if ("A".Equals(execAction) & authUnitArr.Contains(userN.cWorkUnitCode))
                        bPass = true;
                    else
                    {
                        if (userO != null) {
                            if(authUnitArr.Contains(userO.USER_UNIT))
                                bPass = true;
                        }

                        if (userN != null)
                        {
                            if (authUnitArr.Contains(userN.cWorkUnitCode))
                                bPass = true;
                        }
                    }
                } else
                    bPass = true;

                if (!bPass)
                    return Json(new { success = false, err = "使用者單位非屬管理者單位所轄!!" }, JsonRequestBehavior.AllowGet);


                foreach (CodeUserRoleModel role in roleData)
                {
                    //modify by daiyu 20200110
                    if (!"".Equals(authUnit) & !authUnit.Equals(role.roleAuthUnit))
                        return Json(new { success = false, err = "本次異動的角色含不同的程式OWNER單位，請重新確認!!" }, JsonRequestBehavior.AllowGet);
                    else
                        authUnit = role.roleAuthUnit;

                   
                    
                    

                    switch (role.execAction) {
                        case "A":
                            bRoleChg = true;
                            roleList.Add(prepareRole(StringUtil.toString(userMgrModel.cUserID), StringUtil.toString(role.roleId), "A"));
                            break;
                        case "D":
                            bRoleChg = true;
                            roleList.Add(prepareRole(StringUtil.toString(userMgrModel.cUserID), StringUtil.toString(role.tempId), "D"));
                            break;
                        case "U":
                            bRoleChg = true;
                            roleList.Add(prepareRole(StringUtil.toString(userMgrModel.cUserID), StringUtil.toString(role.roleId), "A"));
                            roleList.Add(prepareRole(StringUtil.toString(userMgrModel.cUserID), StringUtil.toString(role.tempId), "D"));
                            break;
                    }
                    //if (roleDataO.Exists(x => x.roleId == role.roleId))
                    //{
                    //    codeUserRoleModel.execAction = "";
                    //}
                    //else
                    //{
                    //    bRoleChg = true;
                    //    codeUserRoleModel.execAction = "A";
                    //}
                    //roleList.Add(codeUserRoleModel);
                }
            }

            //bool bDel = false;
            //foreach (CodeUserRoleModel oRole in roleDataO)
            //{
            //    bDel = false;
            //    if (roleList != null)
            //    {
            //        if (!roleList.Exists(x => x.roleId == oRole.roleId))
            //            bDel = true;
            //    }
            //    else
            //        bDel = true;
                    

            //    if (bDel) {
            //        bRoleChg = true;
            //        CodeUserRoleModel codeUserRoleModel = new CodeUserRoleModel();
            //        codeUserRoleModel.userId = StringUtil.toString(userMgrModel.cUserID);
            //        codeUserRoleModel.roleId = StringUtil.toString(oRole.roleId);
            //        codeUserRoleModel.execAction = "D";
            //        roleList.Add(codeUserRoleModel);
            //    }
            //}
            //end modify 20200110


            if (bUserChg == false && bRoleChg == false)
                return Json(new { success = false, err = "未異動畫面資料，將不進行修改覆核作業!!" }, JsonRequestBehavior.AllowGet);


            /*------------------ DB處理   begin------------------*/
            string strConn = DbUtil.GetDBFglConnStr();
            using (SqlConnection conn = new SqlConnection(strConn))
            {

                conn.Open();

                SqlTransaction transaction = conn.BeginTransaction("Transaction");
                try
                {
                    AplyRecDao aplyRecDao = new AplyRecDao();
                    SSO_APLY_REC authAppr = new SSO_APLY_REC();
                    authAppr.APLY_TYPE = "U";
                    authAppr.APPR_STATUS = "1";
                    authAppr.APPR_MAPPING_KEY = userMgrModel.cUserID;
                    authAppr.CREATE_UID = Session["UserID"].ToString();
                    authAppr.CREATE_UNIT = Session["UserUnit"].ToString();

                    //新增"覆核資料檔"
                    string aplyNo = aplyRecDao.insert(authAppr, conn, transaction);


                    // 異動"使用者資料檔"資料狀態
                    if (!"A".Equals(execAction))
                    {
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
                    if (bUserChg)
                    {
                        CodeUserHisDao codeUserHisDao = new CodeUserHisDao();
                        CODE_USER_HIS userHis = new CODE_USER_HIS();
                        userHis.APLY_NO = aplyNo;
                        userHis.USER_ID = userMgrModel.cUserID;
                        userHis.IS_DISABLED = StringUtil.toString(userMgrModel.isDisabled);
                        userHis.IS_MAIL = StringUtil.toString(userMgrModel.isMail);
                        if (!"A".Equals(execAction))
                        {
                            userHis.IS_DISABLED_B = userO.IS_DISABLED;
                            userHis.IS_MAIL_B = userO.IS_MAIL;
                            userHis.EXEC_ACTION = "U";
                        }
                        else
                            userHis.EXEC_ACTION = "A";

                        codeUserHisDao.insert(userHis, conn, transaction);
                    }


                    //處理角色的異動
                    if (bRoleChg)
                    {
                        CodeUserRoleHisDao codeUserRoleHisDao = new CodeUserRoleHisDao();
                        foreach (CodeUserRoleModel role in roleList)
                        {
                            codeUserRoleHisDao.insert(aplyNo, role, conn, transaction);
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


        private CodeUserRoleModel prepareRole(string userId, string roleId, string execAction) {
            CodeUserRoleModel codeUserRoleModel = new CodeUserRoleModel();
            codeUserRoleModel.userId = StringUtil.toString(userId);
            codeUserRoleModel.roleId = StringUtil.toString(roleId);

            codeUserRoleModel.execAction = execAction;
            return codeUserRoleModel;

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
                    d.roleAuthUnitNm = d.roleId;
                    d.tempId = d.roleId;

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
            var apprStatusList = sysCodeDao.loadSelectList("SSO", "APPR_STATUS", false);
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
            Dictionary<string, string> dicExecAction = sysCodeDao.qryByTypeDic("SSO", "EXEC_ACTION");
            Dictionary<string, string> dicYNFlag = sysCodeDao.qryByTypeDic("SSO", "YN_FLAG");
            Dictionary<string, string> dicApprStatus = sysCodeDao.qryByTypeDic("SSO", "APPR_STATUS");
            Dictionary<string, string> dicIsDisabled = sysCodeDao.qryByTypeDic("SSO", "IS_DISABLED");


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

                    using (dbFGLEntities db = new dbFGLEntities())
                    {
                        userHisList = codeUserHisDao.qryForUserMgrHis(db, userId, apprStatus, updDateB, updDateE);

                        userRoleHisList = codeUserRoleHisDao.qryForUserMgrHis(db, userId, apprStatus, updDateB, updDateE);
                    }
                }


                using (DB_INTRAEntities dbIntra = new DB_INTRAEntities())
                {
                    Dictionary<string, string> userNameMap = new Dictionary<string, string>();
                    OaEmpDao oaEmpDao = new OaEmpDao();
                    //string createUid = "";
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


        /// <summary>
        /// 查詢角色所被授權的功能
        /// </summary>
        /// <param name="roleId"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult qryRoleAuth(string roleId)
        {
            List<FuncRoleModel> rows = new List<FuncRoleModel>();
            CodeRoleFunctionDao codeRoleFuncDao = new CodeRoleFunctionDao();
            rows = codeRoleFuncDao.qryForRoleMgr(roleId);


            var jsonData = new { success = true, rows };
            return Json(jsonData, JsonRequestBehavior.AllowGet);



        }


    }


}