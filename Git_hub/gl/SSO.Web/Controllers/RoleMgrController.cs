using SSO.Web.ActionFilter;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web.Mvc;
using SSO.Web;
using SSO.Web.Daos;
using SSO.Web.Models;
using SSO.Web.ViewModels;
using System.Transactions;
using SSO.Web.Utils;
using SSO.Web.BO;
using System.Reflection;
using System.Data;

/// <summary>
/// 功能說明：角色管理
/// 初版作者：20180430 黃黛鈺
/// 修改歷程：20180430 黃黛鈺 
///           需求單號：201803140070-00
///           初版
/// ==============================================
/// 修改日期/修改人：20191230 B0077 黃黛鈺
/// 需求單號：201911280279-00
/// 修改內容：1.使用者點選『角色維護作業』時，需先以登入者的所屬單位，檢查【可授權程式table檔.管理者所屬部門】 是否有對應資料，若無，則無法作業,需出錯誤訊息”未建管理者可授權程式table檔,請先建檔!”
///           2.可查詢的角色單位屬以登入者的所屬單位帶出對應可處理的程式OWNER單位。
///           3.移除授權角色維護作業程式【允許其他單位授權】之選項。
///           4.可設定的程式為
///            4.1【可授權程式table檔.管理者所屬部門.管理者所屬部門】=登入者單位(依設定判斷至部或科)。
///            4.2【可授權程式table檔.可授權程式owner 單位】=功能的OWNER單位。
///            4.3 不同owner 單位的程式不可以設在同一個角色
/// ==============================================
/// </summary>
/// 

namespace SSO.WebControllers
{
    [Authorize]
    [CheckSessionFilterAttribute]
    public class RoleMgrController : BaseController
    {
        static private NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public ActionResult Index()
        {
            pageIni();
            return View();
        }



        [HttpGet]
        public ActionResult Index(String cRoleID)
        {
            pageIni();
            RoleMgrModel roleMgrModel = new RoleMgrModel();

            if (!"".Equals(StringUtil.toString(cRoleID)))
            {
                CodeRoleDao codeRoleDao = new CodeRoleDao();
                CODE_ROLE codeRole = new CODE_ROLE();
                codeRole = codeRoleDao.qryRoleByKey(cRoleID);

                roleMgrModel.cRoleID = StringUtil.toString(codeRole.ROLE_ID);
                roleMgrModel.cRoleName = StringUtil.toString(codeRole.ROLE_NAME);
                roleMgrModel.isDisabled = StringUtil.toString(codeRole.IS_DISABLED);
                roleMgrModel.vMemo = StringUtil.toString(codeRole.MEMO);
                roleMgrModel.cUpdUserID = StringUtil.toString(codeRole.LAST_UPDATE_UID);

                ViewBag.cRoleID = cRoleID;
                return View(roleMgrModel);
            }
            else
            {
                return View();
            }
        }


        /// <summary>
        /// 畫面初始設定
        /// </summary>
        private void pageIni() {

            UserAuthUtil authUtil = new UserAuthUtil();
        

            string opScope = "";
            string funcName = "";

            String[] roleInfo = authUtil.chkUserFuncAuth(Session["UserID"].ToString(), "~/RoleMgr/");
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

            //程式OWNER單位
            CodeAuthMgrDao codeAuthMgrDao = new CodeAuthMgrDao();
            var AuthUnitList = codeAuthMgrDao.loadSelectList(Session["UserUnit"].ToString(), true);
            ViewBag.AuthUnitList = AuthUnitList;

            ViewBag.authUnitCnt = AuthUnitList.Count();


            //角色名稱
            CodeRoleDao codeRoleDao = new CodeRoleDao();
            //var CodeRoleList = codeRoleDao.loadSelectList(Session["UserUnit"].ToString(), false, "");
            var CodeRoleList = codeRoleDao.loadSelectList(qryAuthUnit(), false, "");   //modify by daiyu 20191230
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

        }



        /// <summary>
        /// 查詢角色的異動紀錄
        /// </summary>
        /// <param name="cRoleID"></param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult roleHis(String cRoleID)
        {

            /*---畫面下拉選單初始值---*/
            SysCodeDao sysCodeDao = new SysCodeDao();


            //覆核狀態
            var apprStatusList = sysCodeDao.loadSelectList("SSO", "APPR_STATUS", false);
            ViewBag.apprStatusList = apprStatusList;


            RoleMgrHisModel roleMgrHisModel = new RoleMgrHisModel();


            if (!"".Equals(StringUtil.toString(cRoleID)))
            {
                CodeRoleDao codeRoleDao = new CodeRoleDao();
                CODE_ROLE codeRole = new CODE_ROLE();
                codeRole = codeRoleDao.qryRoleByKey(cRoleID);


                roleMgrHisModel.cRoleID = StringUtil.toString(codeRole.ROLE_ID);
                roleMgrHisModel.cRoleName = StringUtil.toString(codeRole.ROLE_NAME);


                ViewBag.cRoleID = cRoleID;
                return View(roleMgrHisModel);
            }
            else
            {
                return View();
            }
        }



        /// <summary>
        /// 畫面查詢結果
        /// </summary>
        /// <param name="mgrUnit"></param>
        /// <param name="authUnit"></param>
        /// <param name="codeRole"></param>
        /// <param name="isDIsabled"></param>
        /// <param name="vMemo"></param>
        /// <param name="cUpdUserID"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult LoadData(string mgrUnit, string authUnit, String codeRole, String isDIsabled, String vMemo, String cUpdUserID)
        {
            CodeRoleDao codeRoleDao = new CodeRoleDao();
            OaEmpDao oaEmpDao = new OaEmpDao();

            //modify by daiyu 20191230
            string[] authUnitArr = new string[] { authUnit };

            if ("".Equals(StringUtil.toString(authUnit)))
                authUnitArr = qryAuthUnit();
                
            

            List<RoleMgrModel> rows = new List<RoleMgrModel>();
            rows = codeRoleDao.roleMgrQry(authUnitArr, codeRole, isDIsabled, vMemo, cUpdUserID);

            Dictionary<string, string> userNameMap = new Dictionary<string, string>();
            string userUId = "";
            string userFId = "";


                using (DB_INTRAEntities dbIntra = new DB_INTRAEntities())
                {
                    foreach (RoleMgrModel role in rows)
                    {
                        userUId = StringUtil.toString(role.cUpdUserID);
                        userFId = StringUtil.toString(role.freezeUid);

                        if (!"".Equals(userUId))
                        {
                            if (!userNameMap.ContainsKey(userUId))
                            {
                                userNameMap = oaEmpDao.qryUsrName(userNameMap, userUId, dbIntra);
                            }
                            role.cUpdUserID = userNameMap[userUId];
                        }

                        if (!"".Equals(userFId))
                        {
                            if (!userNameMap.ContainsKey(userFId))
                            {
                                userNameMap = oaEmpDao.qryUsrName(userNameMap, userFId, dbIntra);
                            }
                            role.freezeUid = userNameMap[userFId];
                        }

                    }
                }
            

            var jsonData = new { success = true, rows };
            return Json(jsonData, JsonRequestBehavior.AllowGet);
        }
        

        /**
        異動角色/角色功能
        **/
        [HttpPost]
        public ActionResult updateRole(RoleMgrModel roleMgrModel, String authFunc)
        {
            string roleId = StringUtil.toString(roleMgrModel.cRoleID);
            bool bChgRole = false;
            bool bChgFunc = false;
            bool bNewRole = false;

            //比對是否有異動"角色資訊"
            CodeRoleDao codeRoleDao = new CodeRoleDao();
            CODE_ROLE codeRoleO = new CODE_ROLE();
            
            if ("".Equals(roleId))
            {
                bNewRole = true;
                bChgRole = true;
                SysSeqDao sysSeqDao = new SysSeqDao();

                var cId = sysSeqDao.qrySeqNo("SSO", "F1", "").ToString();
                roleId = "F1" + cId.ToString().PadLeft(8, '0');
            }
            else {
                codeRoleO = codeRoleDao.qryRoleByKey(roleId);
                if (!(
                    StringUtil.toString(roleMgrModel.cRoleName).Equals(StringUtil.toString(codeRoleO.ROLE_NAME))
                    && StringUtil.toString(roleMgrModel.freeAuth).Equals(StringUtil.toString(codeRoleO.FREE_AUTH))
                    && StringUtil.toString(roleMgrModel.isDisabled).Equals(StringUtil.toString(codeRoleO.IS_DISABLED))
                    && StringUtil.toString(roleMgrModel.vMemo).Equals(StringUtil.toString(codeRoleO.MEMO))
                    ))
                {
                    bChgRole = true;
                }

            }
           



            //比對是否有異動"授權功能"
            List<FuncRoleModel> funcList = new List<FuncRoleModel>();
            string[] funcData = authFunc.Split('|');

            CodeRoleFunctionDao CodeRoleFunctionDao = new CodeRoleFunctionDao();
            List<FuncRoleModel> roleFuncListO = CodeRoleFunctionDao.qryForRoleMgr(roleId);
            foreach (string item in funcData)
            {
                if (!"".Equals(StringUtil.toString(item))) {
                    FuncRoleModel funcRoleModel = new FuncRoleModel();
                    funcRoleModel.cRoleId = roleId;
                    funcRoleModel.cFunctionID = item;
                    if (roleFuncListO.Exists(x => x.cFunctionID == item))
                    {
                        funcRoleModel.execAction = "";
                    }

                    else {
                        bChgFunc = true;
                        funcRoleModel.execAction = "A";
                    }
                    funcList.Add(funcRoleModel);
                }
            }

            foreach (FuncRoleModel oItem in roleFuncListO) {
                if (!funcList.Exists(x => x.cFunctionID == oItem.cFunctionID))
                {
                    bChgFunc = true;
                    FuncRoleModel funcRoleModel = new FuncRoleModel();
                    funcRoleModel.cRoleId = roleId;
                    funcRoleModel.cFunctionID = oItem.cFunctionID;
                    funcRoleModel.execAction = "D";
                    funcList.Add(funcRoleModel);
                }
            }

            

            if(bChgRole == false && bChgFunc == false)
                return Json(new { success = false, errors = "未異動畫面資料，將不進行修改覆核作業!!" }, JsonRequestBehavior.AllowGet);



            /*------------------ DB處理   begin------------------*/
            string strConn = DbUtil.GetDBFglConnStr();
            using (SqlConnection conn = new SqlConnection(strConn))
            {

                conn.Open();

                SqlTransaction transaction = conn.BeginTransaction("Transaction");
                try
                {
                    roleMgrModel.cRoleID = roleId;

                    AplyRecDao aplyRecDao = new AplyRecDao();
                    SSO_APLY_REC authAppr = new SSO_APLY_REC();
                    authAppr.APLY_TYPE = "R";
                    authAppr.APPR_STATUS = "1";
                    authAppr.APPR_MAPPING_KEY = roleId;
                    authAppr.CREATE_UID = Session["UserID"].ToString();
                    authAppr.CREATE_UNIT = Session["UserUnit"].ToString();

                    //新增"覆核資料檔"
                    string aplyNo = aplyRecDao.insert(authAppr, conn, transaction);


                    //異動"角色資料檔"覆核狀態
                    if (bNewRole == false)
                        updateRole(roleMgrModel, codeRoleO, conn, transaction);


                    //處理角色資料檔的異動
                    if (bChgRole)
                    {
                        //新增"角色資料異動檔"
                        CodeRoleHisDao codeRoleHisDao = new CodeRoleHisDao();
                        CODE_ROLE_HIS codeRoleHis = new CODE_ROLE_HIS();
                        codeRoleHis.APLY_NO = aplyNo;
                        codeRoleHis.ROLE_ID = StringUtil.toString(roleMgrModel.cRoleID);
                        codeRoleHis.AUTH_UNIT = roleMgrModel.authUnit; //Session["UserUnit"].ToString();
                        codeRoleHis.FREE_AUTH = StringUtil.toString(roleMgrModel.freeAuth);
                        codeRoleHis.ROLE_NAME = StringUtil.toString(roleMgrModel.cRoleName);
                        codeRoleHis.IS_DISABLED = StringUtil.toString(roleMgrModel.isDisabled);
                        codeRoleHis.MEMO = StringUtil.toString(roleMgrModel.vMemo);

                        if (codeRoleO != null) {
                            codeRoleHis.ROLE_NAME_B = StringUtil.toString(codeRoleO.ROLE_NAME);
                            codeRoleHis.FREE_AUTH_B = StringUtil.toString(codeRoleO.FREE_AUTH);
                            codeRoleHis.IS_DISABLED_B = StringUtil.toString(codeRoleO.IS_DISABLED);
                            codeRoleHis.MEMO_B = StringUtil.toString(codeRoleO.MEMO);
                        } else {
                            codeRoleHis.ROLE_NAME_B = "";
                            codeRoleHis.FREE_AUTH_B = "";
                            codeRoleHis.IS_DISABLED_B = "";
                            codeRoleHis.MEMO_B = "";

                        }
                        

                        if (bNewRole)
                            codeRoleHis.EXEC_ACTION = "A";
                        else
                            codeRoleHis.EXEC_ACTION = "U";

                        codeRoleHisDao.insert(codeRoleHis, conn, transaction);
                    }

                    

                    //處理功能角色資料檔的異動
                    if (bChgFunc)
                    {
                        CodeRoleFuncHisDao codeRoleFuncHisDao = new CodeRoleFuncHisDao();
                        foreach (FuncRoleModel func in funcList) {
                            if (!"".Equals(func.execAction)) {
                                codeRoleFuncHisDao.insert(aplyNo, func, conn, transaction);
                            }
                        }
                    }



                    transaction.Commit();

                    /*------------------ DB處理   end------------------*/
                    return Json(new { success = true, aplyNo = aplyNo });

                }
                catch (Exception e)
                {
                    transaction.Rollback();
                    logger.Error("[updateRole]其它錯誤：" + e.ToString());

                    return Json(new { success = false, errors = "其它錯誤，請洽系統管理員!!" }, JsonRequestBehavior.AllowGet);
                }
            }


        }


        /// <summary>
        /// 異動角色檔
        /// </summary>
        /// <param name="roleMgrModel"></param>
        /// <param name="codeRoleO"></param>
        /// <param name="conn"></param>
        /// <param name="transaction"></param>
        public void updateRole(RoleMgrModel roleMgrModel, CODE_ROLE codeRoleO, SqlConnection conn, SqlTransaction transaction)
        {

            try
            {

                CodeRoleDao codeRoleDao = new CodeRoleDao();

                //新增LOG
                Log log = new Log();
                log.CFUNCTION = "角色管理-修改";
                log.CACTION = "U";
                log.CCONTENT = codeRoleDao.roleLogContent(codeRoleO);
                LogDao.Insert(log, Session["UserID"].ToString());

                //異動角色檔
                codeRoleO.DATA_STATUS = "2";
                codeRoleO.LAST_UPDATE_UID = Session["UserID"].ToString();
                codeRoleO.LAST_UPDATE_DT = DateTime.Now;
                codeRoleO.FREEZE_UID = Session["UserID"].ToString();
                codeRoleO.FREEZE_DT = DateTime.Now;

                int cnt = codeRoleDao.Update(codeRoleO, conn, transaction);

            }
            catch (Exception e)
            {
                logger.Error("[updateRole]其它錯誤：" + e.ToString());
                throw e;
                //新增角色檔失敗
                // return Json(new { success = false, errors = e.ToString() }, JsonRequestBehavior.AllowGet);
            }

        }





        /// <summary>
        /// 角色資訊(含功能授權)
        /// </summary>
        /// <param name="cRoleId"></param>
        /// <param name="execType"></param>
        /// <returns></returns>
        public ActionResult detailRole(string cRoleId, string execType)
        {

            /*---畫面下拉選單初始值---*/


            SysCodeDao sysCodeDao = new SysCodeDao();

            //停用註記
            var isDisabledList = sysCodeDao.loadSelectList("SSO", "IS_DISABLED", false);
            ViewBag.isDisabledList = isDisabledList;

            //允許其他單位授權
            var freeAuthList = sysCodeDao.loadSelectList("SSO", "YN_FLAG", false);
            ViewBag.freeAuthList = freeAuthList;

            //程式OWNER單位
            CodeAuthMgrDao codeAuthMgrDao = new CodeAuthMgrDao();
            var AuthUnitList = codeAuthMgrDao.loadSelectList(Session["UserUnit"].ToString(), true);
            ViewBag.AuthUnitList = AuthUnitList;


            //覆核狀態  add by daiyu 20180214
            Dictionary<string, string> dicReview = sysCodeDao.qryByTypeDic("SSO", "DATA_STATUS");

            ViewBag.authUnit = Session["UserUnit"].ToString();
            ViewBag.authUnitNm = Session["UserUnitNm"].ToString();

            //查詢角色資訊
            CodeRoleDao codeRoleDao = new CodeRoleDao();
            CODE_ROLE codeRole = new CODE_ROLE();

            if (cRoleId != null)
                codeRole = codeRoleDao.qryRoleByKey(cRoleId);


            //將值搬給畫面欄位
            RoleMgrModel roleMgrModel = new RoleMgrModel();

            roleMgrModel.mgrUnit = Session["UserUnit"].ToString();
            roleMgrModel.mgrUnitNm = Session["UserUnitNm"].ToString();


            qryUserFunc(cRoleId, StringUtil.toString(codeRole.AUTH_UNIT));  //取得已授權、未授權功能清單

            if (!"".Equals(StringUtil.toString(codeRole.ROLE_ID)))
            {
                roleMgrModel.cRoleID = StringUtil.toString(codeRole.ROLE_ID);
                roleMgrModel.cRoleName = StringUtil.toString(codeRole.ROLE_NAME);


                roleMgrModel.authUnit = StringUtil.toString(codeRole.AUTH_UNIT);

                //OaDeptDao oaDeptDao = new OaDeptDao();
                //VW_OA_DEPT dept = new VW_OA_DEPT();
                //dept = oaDeptDao.qryByDptCd(roleMgrModel.authUnit);
                //string authUnitNm = "";
                //if (dept != null)
                //    authUnitNm = StringUtil.toString(dept.DPT_NAME);

                //roleMgrModel.authUnitNm = authUnitNm;


                roleMgrModel.freeAuth = StringUtil.toString(codeRole.FREE_AUTH);

                roleMgrModel.isDisabled = StringUtil.toString(codeRole.IS_DISABLED);
                roleMgrModel.vMemo = StringUtil.toString(codeRole.MEMO);
                roleMgrModel.dataStatus = StringUtil.toString(codeRole.DATA_STATUS) == "" ? "" : codeRole.DATA_STATUS + "." + dicReview[codeRole.DATA_STATUS];

                roleMgrModel.cCrtDateTime = codeRole.CREATE_DT == null ? "" : DateUtil.DatetimeToString(codeRole.CREATE_DT, "");

                roleMgrModel.cUpdDateTime = codeRole.LAST_UPDATE_DT == null ? "" : DateUtil.DatetimeToString(codeRole.LAST_UPDATE_DT, "");

                OaEmpDao oaEmpDao = new OaEmpDao();
                using (DB_INTRAEntities dbIntra = new DB_INTRAEntities())
                {
                    try
                    {
                        roleMgrModel.cCrtUserID = codeRole.CREATE_UID == null ? "" : StringUtil.toString(oaEmpDao.qryByUsrId(codeRole.CREATE_UID, dbIntra).EMP_NAME);
                    }
                    catch (Exception e)
                    {

                    }

                    try
                    {
                        roleMgrModel.cUpdUserID = codeRole.LAST_UPDATE_UID == null ? "" : StringUtil.toString(oaEmpDao.qryByUsrId(codeRole.LAST_UPDATE_UID, dbIntra).EMP_NAME);
                    }
                    catch (Exception e)
                    {

                    }
                }

                ViewBag.authUnit = roleMgrModel.authUnit;
                roleMgrModel.Categories = ViewBag.funcList;

                ViewBag.bHaveData = "Y";
                //return RedirectToAction("Index", "Home");
                return View(roleMgrModel);
            }
            else
            {
                roleMgrModel.authUnit = "";
                roleMgrModel.authUnitNm ="";

                if ("A".Equals(execType))
                {
                    qryUserFunc("", "");  //取得已授權、未授權功能清單

                    //roleMgrModel.cRoleID = "";
                    //roleMgrModel.cRoleName = "";
                    //roleMgrModel.isDisabled = "";
                    //roleMgrModel.vMemo = "";
                    //roleMgrModel.cCrtUserID = "";
                    //roleMgrModel.cCrtDateTime = "";
                    //roleMgrModel.cUpdUserID = "";
                    //roleMgrModel.cUpdDateTime = "";
                    //roleMgrModel.dataStatus = "";

                    roleMgrModel.Categories = ViewBag.funcList;
                    ViewBag.bHaveData = "Y";
                    return View(roleMgrModel);

                }
                else
                {
                    ViewBag.bHaveData = "N";
                    return View("detailRole");
                }


            }
        }


        /// <summary>
        /// 依登入者的單位查詢對應的程式授權OWNER單位
        /// </summary>
        /// <returns></returns>
        private string[] qryAuthUnit() {
            CodeAuthMgrDao codeAuthMgrDao = new CodeAuthMgrDao();
            string[] authUnitArr = codeAuthMgrDao.qryAuthUnit(Session["UserUnit"].ToString());

            return authUnitArr;
        }



        /// <summary>
        /// 查詢角色已授權的功能
        /// </summary>
        /// <param name="cRoleId"></param>
        public void qryUserFunc(string cRoleId, string auth_unit)
        {

            dbFGLEntities context = new dbFGLEntities();
            string userUnit = Session["UserUnit"].ToString();
            string[] authUnitArr = qryAuthUnit();
            if (!"".Equals(auth_unit))
                authUnitArr = new string[] { auth_unit };
            

            var authData = new
            {
                item = (
                from func in context.CODE_FUNC
                join role in context.CODE_ROLE_FUNC on func.FUNC_ID equals role.FUNC_ID
                where 1 == 1
                & func.IS_DISABLED == "N"
                //& func.CSYSID == "sys006"
                // & func.AUTH_UNIT == userUnit
                 & authUnitArr.Contains(func.AUTH_UNIT)
                & role.ROLE_ID == cRoleId
                orderby (func.FUNC_LEVEL == 1 ? func.FUNC_ID : func.PARENT_FUNC_ID), func.PARENT_FUNC_ID, func.FUNC_ORDER

                select new
                {
                    cFunctionID = func.FUNC_ID,
                    cFunctionName = func.FUNC_NAME,
                    iFunctionLevel = func.FUNC_LEVEL,
                    menuLevel = func.FUNC_LEVEL == 1 ? func.FUNC_ID : func.PARENT_FUNC_ID,
                    funcUrl = func.FUNC_URL.Trim()
                }).ToArray()
            };


            ViewBag.funcList = qryUserFuncList(cRoleId, auth_unit);
            //ViewBag.funcList = result1;
            ViewBag.userAuthFuncList = Json(authData, JsonRequestBehavior.AllowGet);
        }


        [HttpPost]
        public JsonResult qryFuncList(string cRoleId, string auth_unit)
        {
            List<FuncRoleModel> funcList = qryUserFuncList(cRoleId, auth_unit);

            return Json(new { success = true, funcList = funcList }, JsonRequestBehavior.AllowGet);
        }


        public List<FuncRoleModel> qryUserFuncList(string cRoleId, string auth_unit)
        {

            dbFGLEntities context = new dbFGLEntities();
            string userUnit = Session["UserUnit"].ToString();
            string[] authUnitArr = qryAuthUnit();
            if (!"".Equals(auth_unit))
                authUnitArr = new string[] { auth_unit };

            Dictionary<string, MenuModel> pMenu = new Dictionary<string, MenuModel>();

            string strConn = DbUtil.GetDBFglConnStr();


            //modify by daiyu 20190415 begin 功能顯示錯誤
            //modify by daiyu 20191230
            string sql = @"select  distinct func.FUNC_ID MenuID
                              ,func.PARENT_FUNC_ID
                              ,func.FUNC_NAME Title
                              ,func.SYS_CD
                              ,func.FUNC_LEVEL  FUNC_LEVEL
                              ,func.FUNC_URL Link
                              ,func.FUNC_ORDER FUNC_ORDER
                              ,func.AUTH_UNIT
                        from CODE_FUNC func
                        where 1=1
                        and func.IS_DISABLED = 'N'
                        ";


            string sqlP = @"select  distinct func.FUNC_ID MenuID
                              ,func.PARENT_FUNC_ID
                              ,func.FUNC_NAME Title
                              ,func.SYS_CD
                              ,func.FUNC_LEVEL  FUNC_LEVEL
                              ,func.FUNC_URL Link
                              ,func.FUNC_ORDER FUNC_ORDER
                              ,case FUNC_LEVEL when 1 then func.FUNC_ID else func.PARENT_FUNC_ID end  menuLevel
                        from CODE_FUNC func
                        where 1=1
                        and func.FUNC_URL = ''
                        order by menuLevel, func.PARENT_FUNC_ID , func.FUNC_ORDER
                        ";

            List<MenuModel> menuViewModel = new List<MenuModel>();
            using (SqlConnection conn = new SqlConnection(strConn))
            {
                //String menuLevelO = "";

                conn.Open();
                SqlCommand command = conn.CreateCommand();
                command.CommandType = CommandType.Text;
                command.Connection = conn;
                command.CommandTimeout = 0;
                command.CommandText = sql;
                //command.Parameters.AddWithValue("@auth_unit", userUnit);  //delete by daiyu 20191230

                MenuModel menuModel = new MenuModel();

                List<MenuModel> rows = new List<MenuModel>();

                using (SqlDataReader dr = command.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        MenuModel t = new MenuModel();

                        for (int inc = 0; inc < dr.FieldCount; inc++)
                        {
                            Type type = t.GetType();
                            PropertyInfo prop = type.GetProperty(dr.GetName(inc));

                            try
                            {
                                prop.SetValue(t, dr.GetValue(inc), null);
                            }
                            catch (Exception e)
                            {

                            }

                        }

                        if (authUnitArr.Contains(t.AUTH_UNIT))
                            rows.Add(t);
                    }
                }

                //查詢父節點
                command.CommandText = sqlP;

                using (SqlDataReader dr = command.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        MenuModel t = new MenuModel();

                        for (int inc = 0; inc < dr.FieldCount; inc++)
                        {
                            Type type = t.GetType();
                            PropertyInfo prop = type.GetProperty(dr.GetName(inc));

                            try
                            {
                                prop.SetValue(t, dr.GetValue(inc), null);
                            }
                            catch (Exception e)
                            {

                            }
                        }

                        pMenu.Add(t.MenuID, t);
                    }
                }

                if (rows.Count > 0)
                {
                    foreach (MenuModel child in rows.GroupBy(x => new { x.PARENT_FUNC_ID }).Select(group => new MenuModel { PARENT_FUNC_ID = group.Key.PARENT_FUNC_ID }).ToList<MenuModel>())
                        rows = qryParent(rows, pMenu, child.PARENT_FUNC_ID);
                }


                menuModel.MenuID = "0";
                menuModel.FUNC_LEVEL = 0;


                menuModel = ChildrenOf(rows, menuModel);

                menuViewModel.Add(menuModel);

            }

            List<FuncRoleModel> funcList = new List<FuncRoleModel>();
            funcList = qryFuncList(menuViewModel[0], funcList);


            return funcList;
 
        }







        public static List<FuncRoleModel> qryFuncList(MenuModel menuViewModel, List<FuncRoleModel> funcList) {
            foreach (MenuModel child in menuViewModel.SubMenu)
            {
                if (child.SubMenu.Count > 0) {
                    FuncRoleModel d = new FuncRoleModel();
                    d.cFunctionID = child.MenuID;
                    d.cFunctionName = child.Title;
                    d.iFunctionLevel = child.FUNC_LEVEL;
                    funcList.Add(d);

                    funcList = qryFuncList(child, funcList);
                }

                if (!"".Equals(child.Link) & child.SubMenu.Count() == 0) {
                    FuncRoleModel d = new FuncRoleModel();
                    d.cFunctionID = child.MenuID;
                    d.cFunctionName = child.Title;
                    d.iFunctionLevel = child.FUNC_LEVEL;
                    d.funcUrl = child.Link;

                    funcList.Add(d);
                }

            }

                return funcList;
        }


        public static MenuModel ChildrenOf(List<MenuModel> rows, MenuModel menu)
        {
            foreach (MenuModel child in rows.Where(x => x.PARENT_FUNC_ID.Trim() == menu.MenuID).OrderBy(x => x.FUNC_ORDER))
            {
                MenuModel item = new MenuModel();
                item.MenuID = child.MenuID;
                item.Title = child.Title;
                item.PARENT_FUNC_ID = child.PARENT_FUNC_ID;
                item.Link = child.Link;
                item.FUNC_LEVEL = child.FUNC_LEVEL;

                menu.SubMenu.Add(ChildrenOf(rows, item));

            }

            return menu;
        }

        public static List<MenuModel> qryParent(List<MenuModel> rows, Dictionary<string, MenuModel> pMenu, string nowPFuncId)
        {
            if (!rows.Exists(x => x.MenuID == nowPFuncId))
            {
                if (pMenu.ContainsKey(nowPFuncId))
                {
                    rows.Add(pMenu[nowPFuncId]);
                    rows = qryParent(rows, pMenu, pMenu[nowPFuncId].PARENT_FUNC_ID);
                }

            }


            return rows;
        }

        /// <summary>
        /// 查詢歷史異動資料
        /// </summary>
        /// <param name="cRoleID"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult qryRoleHisData(string cRoleID, string apprStatus,string updDateB, string updDateE)
        {

            if("".Equals(StringUtil.toString(cRoleID)))
                return Json(new { success = false, err = "角色代號未輸入!!" });

            SysCodeDao sysCodeDao = new SysCodeDao();
            Dictionary<string, string> dicExecAction = sysCodeDao.qryByTypeDic("SSO", "EXEC_ACTION");
            Dictionary<string, string> dicYNFlag = sysCodeDao.qryByTypeDic("SSO", "YN_FLAG");
            Dictionary<string, string> dicApprStatus = sysCodeDao.qryByTypeDic("SSO", "APPR_STATUS");
            Dictionary<string, string> dicIsDisabled = sysCodeDao.qryByTypeDic("SSO", "IS_DISABLED");


            List<CodeRoleModel> roleHisList = new List<CodeRoleModel>();
            List<RoleFuncHisModel> roleFuncHisList = new List<RoleFuncHisModel>();

            List<CodeRoleItemModel> roleItemHisList = new List<CodeRoleItemModel>();
            List<CodeRoleItemModel> roleFormAplyHisList = new List<CodeRoleItemModel>();

            CodeRoleHisDao codeRoleHisDao = new CodeRoleHisDao();
            CodeRoleFuncHisDao codeRoleFuncHisDao = new CodeRoleFuncHisDao();


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
                        roleHisList = codeRoleHisDao.qryForRoleMgrHis(db, cRoleID, apprStatus, updDateB, updDateE);

                        roleFuncHisList = codeRoleFuncHisDao.qryForRoleMgrHis(db, cRoleID, apprStatus, updDateB, updDateE);
                    }

                }


                using (DB_INTRAEntities dbIntra = new DB_INTRAEntities())
                {
                    Dictionary<string, string> userNameMap = new Dictionary<string, string>();
                    OaEmpDao oaEmpDao = new OaEmpDao();
                    //string createUid = "";
                    string userId = "";

                    //處理角色資訊人員&代碼
                    if (roleHisList != null)
                    {
                        
                        foreach (CodeRoleModel role in roleHisList)
                        {
                            role.execActionDesc = dicExecAction.ContainsKey(StringUtil.toString(role.execAction)) ? dicExecAction[StringUtil.toString(role.execAction)]:"";
                            role.apprStatusDesc = dicApprStatus.ContainsKey(StringUtil.toString(role.apprStatus)) ? dicApprStatus[StringUtil.toString(role.apprStatus)]:"";

                            role.freeAuthDesc = dicYNFlag.ContainsKey(StringUtil.toString(role.freeAuth)) ? dicYNFlag[StringUtil.toString(role.freeAuth)] : "";
                            role.freeAuthDescB = dicYNFlag.ContainsKey(StringUtil.toString(role.freeAuthB)) ? dicYNFlag[StringUtil.toString(role.freeAuthB)] : "";



                            role.isDisabledDesc = dicIsDisabled.ContainsKey(StringUtil.toString(role.isDisabled)) ? dicIsDisabled[StringUtil.toString(role.isDisabled)]:"";
                            role.isDisabledDescB = dicIsDisabled.ContainsKey(StringUtil.toString(role.isDisabledB)) ?  dicIsDisabled[StringUtil.toString(role.isDisabledB)]:"";

                            userId = StringUtil.toString(role.updateUid);
                            if (!"".Equals(userId))
                            {
                                if (!userNameMap.ContainsKey(userId))
                                {
                                    userNameMap = oaEmpDao.qryUsrName(userNameMap, userId, dbIntra);
                                }
                                role.updateUid = userNameMap[userId];
                            }
                        }

                    }


                    //處理授權功能人員&代碼
                    if (roleFuncHisList != null)
                    {

                        foreach (RoleFuncHisModel d in roleFuncHisList)
                        {
                            d.execActionDesc = dicExecAction.ContainsKey(StringUtil.toString(d.execAction)) ? dicExecAction[StringUtil.toString(d.execAction)] : "";
                            d.apprStatusDesc = dicApprStatus.ContainsKey(StringUtil.toString(d.apprStatus)) ? dicApprStatus[StringUtil.toString(d.apprStatus)] : "";
                            
                            userId = StringUtil.toString(d.updateUid);
                            if (!"".Equals(userId))
                            {
                                if (!userNameMap.ContainsKey(userId))
                                {
                                    userNameMap = oaEmpDao.qryUsrName(userNameMap, userId, dbIntra);
                                }
                                d.updateUid = userNameMap[userId];
                            }
                        }
                    }

                }
                return Json(new { success = true, roleHisList = roleHisList , roleFuncHisList  = roleFuncHisList });


            }
            catch (Exception e)
            {
                logger.Error("[qryEquip]:" + e.ToString());
                return Json(new { success = false, err = "其它錯誤，請洽系統管理員!!" });
            }
        }




    }

}