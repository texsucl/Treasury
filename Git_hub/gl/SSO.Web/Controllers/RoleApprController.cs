using SSO.Web.ActionFilter;
using SSO.Web.BO;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Web.Mvc;
using SSO.Web;
using SSO.Web.Models;
using System.Transactions;
using SSO.Web.ViewModels;
using SSO.Web.Daos;
using SSO.Web.Utils;
using SSO.WebViewModels;
using System.Linq;

/// <summary>
/// 功能說明：角色權限覆核作業
/// 初版作者：20180511 黃黛鈺
/// 修改歷程：20180511 黃黛鈺 
///           需求單號：
///           初版
/// ==============================================
/// 修改日期/修改人：20190515 daiyu
/// 需求單號：
/// 修改內容：配合金檢議題，稽核軌跡多加寫HOSTNAME
/// ==============================================
/// 修改日期/修改人：20200109 B0077 黃黛鈺
/// 需求單號：201911280279-00
/// 修改內容：1.使用者點選『角色維護覆核作業』時，需先以登入者的所屬單位，檢查【可授權程式table檔.管理者所屬部門】 是否有對應資料，若無，則無法作業,需出錯誤訊息”未建管理者可授權程式table檔,請先建檔!”
///           2.可查詢的角色單位屬以登入者的所屬單位帶出對應可處理的程式OWNER單位。
///           3.移除授權角色維護作業程式【允許其他單位授權】之選項。
/// ==============================================
/// </summary>
/// 
namespace SSO.WebControllers
{
    [Authorize]
    [CheckSessionFilterAttribute]
    public class RoleApprController : BaseController
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

            String[] roleInfo = authUtil.chkUserFuncAuth(Session["UserID"].ToString(), "~/RoleAppr/");
            if (roleInfo != null && roleInfo.Length == 2)
            {
                opScope = "1";
                funcName = roleInfo[1];
            }


            ViewBag.opScope = opScope;
            ViewBag.funcName = funcName;


            //程式OWNER單位
            CodeAuthMgrDao codeAuthMgrDao = new CodeAuthMgrDao();
            var AuthUnitList = codeAuthMgrDao.loadSelectList(Session["UserUnit"].ToString(), true);
            ViewBag.authUnitCnt = AuthUnitList.Count();


            bool bAdmin = authUtil.chkAdmin("SSO", Session["UserID"].ToString());

            if (bAdmin)
            {
                ViewBag.mgrUnit = "";
                ViewBag.mgrUnitNm = "";
            }
            else {
                ViewBag.mgrUnit = Session["UserUnit"].ToString();
                ViewBag.mgrUnitNm = Session["UserUnitNm"].ToString();
            }

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
            //OaDeptDao OaDeptDao = new OaDeptDao();
            //VW_OA_DEPT unit = new VW_OA_DEPT();
            //List<UnitInfoModel> unitList = new List<UnitInfoModel>();
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
                        rows = aplyRecDao.qryAuthReview(authUnitArr, mgrUnit, "R", "1", db);
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
                foreach (AuthReviewModel d in rows.Where(x => x.authUnit == "").ToList()) {

                }


                Dictionary<string, string> deptMap = new Dictionary<string, string>();
                Dictionary<string, string> userNameMap = new Dictionary<string, string>();
                OaEmpDao oaEmpDao = new OaEmpDao();
                OaDeptDao oaDeptDao = new OaDeptDao();
                string createUid = "";
                string authUnit = "";

                foreach (AuthReviewModel d in rows)
                {
                    createUid = StringUtil.toString(d.createUid);
                    authUnit = StringUtil.toString(d.authUnit);

                    if (!"".Equals(createUid))
                    {
                        if (!userNameMap.ContainsKey(createUid))
                        {
                            userNameMap = oaEmpDao.qryUsrName(userNameMap, createUid, dbIntra);
                        }
                        d.createUid = createUid + " " + userNameMap[createUid];
                    }


                    if (!"".EndsWith(authUnit)) {
                        if (!deptMap.ContainsKey(authUnit))
                        {
                            VW_OA_DEPT dept = new VW_OA_DEPT();
                            dept = oaDeptDao.qryByDptCd(authUnit);

                            deptMap.Add(authUnit, dept == null ? "" : StringUtil.toString(dept.DPT_NAME));
                        }
                        d.authUnitNm = authUnit + " " + deptMap[authUnit];

                    }


                }

            }



            var jsonData = new { success = true, rows };
            return Json(jsonData, JsonRequestBehavior.AllowGet);

        }
        

        /// <summary>
        /// 開啟角色修改明細畫面
        /// </summary>
        /// <param name="aplyNo"></param>
        /// <returns></returns>
        public ActionResult detailRole(string aplyNo, string roleId)
        {
            try
            {
                string execAction = "";
                AplyRecDao aplyRecDao = new AplyRecDao();

                SSO_APLY_REC authAppr = new SSO_APLY_REC();



                if (!"".Equals(StringUtil.toString(aplyNo))) {
                    authAppr = aplyRecDao.qryByKey(aplyNo);
                    ViewBag.bView = "N";
                }

                else {
                    authAppr = aplyRecDao.qryByFreeRole(roleId);
                    if(authAppr != null)
                        aplyNo = StringUtil.toString(authAppr.APLY_NO);

                    ViewBag.bView = "Y";
                }
                    


                AuthReviewRoleModel roleData = new AuthReviewRoleModel();
                roleData.aplyNo = aplyNo;
                roleData.createUid = authAppr.CREATE_UID;

                OaEmpDao oaEmpDao = new OaEmpDao();
                using (DB_INTRAEntities dbIntra = new DB_INTRAEntities())
                {
                    try
                    {
                        roleData.createUid = roleData.createUid == null ? "" : StringUtil.toString(oaEmpDao.qryByUsrId(roleData.createUid, dbIntra).EMP_NAME);
                    }
                    catch (Exception e)
                    {

                    }
                }
                roleData.createDt = authAppr.CREATE_DT.ToString();
                //roleData.roleName = "";
                //roleData.isDisabled = "";
                //roleData.memo = "";
                //roleData.roleNameB = "";
                //roleData.isDisabledB = "";
                //roleData.memoB = "";


                CodeRoleHisDao CodeRoleHisDao = new CodeRoleHisDao();
                CODE_ROLE_HIS codeRoleHis = CodeRoleHisDao.qryByAplyNo(aplyNo);
                if (codeRoleHis != null) {
                    execAction = StringUtil.toString(codeRoleHis.EXEC_ACTION);
                }

                if ("".Equals(execAction))
                {
                    CodeRoleDao CodeRoleDao = new CodeRoleDao();
                    CODE_ROLE codeRole = new CODE_ROLE();
                    codeRole = CodeRoleDao.qryRoleByKey(authAppr.APPR_MAPPING_KEY);

                    roleData.roleId = StringUtil.toString(codeRole.ROLE_ID);
                    roleData.authUnit = StringUtil.toString(codeRole.AUTH_UNIT);
                    roleData.freeAuthB = StringUtil.toString(codeRole.FREE_AUTH);
                    roleData.roleNameB = StringUtil.toString(codeRole.ROLE_NAME);
                    roleData.isDisabledB = StringUtil.toString(codeRole.IS_DISABLED);
                    roleData.memoB = StringUtil.toString(codeRole.MEMO);
                }
                else {
                    roleData.roleId = StringUtil.toString(codeRoleHis.ROLE_ID);
                    roleData.authUnit = StringUtil.toString(codeRoleHis.AUTH_UNIT);

                    if ("A".Equals(execAction))
                    {
                        roleData.freeAuth = StringUtil.toString(codeRoleHis.FREE_AUTH);
                        roleData.roleName = StringUtil.toString(codeRoleHis.ROLE_NAME);
                        roleData.isDisabled = StringUtil.toString(codeRoleHis.IS_DISABLED);
                        roleData.memo = StringUtil.toString(codeRoleHis.MEMO);
                        roleData.execActionDesc = "新增";
                    }
                    else {
                        roleData.freeAuth = StringUtil.toString(codeRoleHis.FREE_AUTH);
                        roleData.roleName = StringUtil.toString(codeRoleHis.ROLE_NAME);
                        roleData.isDisabled = StringUtil.toString(codeRoleHis.IS_DISABLED);
                        roleData.memo = StringUtil.toString(codeRoleHis.MEMO);

                        roleData.freeAuthB = StringUtil.toString(codeRoleHis.FREE_AUTH_B);
                        roleData.roleNameB = StringUtil.toString(codeRoleHis.ROLE_NAME_B);
                        roleData.isDisabledB = StringUtil.toString(codeRoleHis.IS_DISABLED_B);
                        roleData.memoB = StringUtil.toString(codeRoleHis.MEMO_B);
                        roleData.execActionDesc = "修改";
                    }
                }


                OaDeptDao oaDeptDao = new OaDeptDao();
                VW_OA_DEPT dept = new VW_OA_DEPT();
                dept = oaDeptDao.qryByDptCd(roleData.authUnit);
                //string authUnitNm = "";
                if (dept != null)
                    roleData.authUnitNm = StringUtil.toString(dept.DPT_NAME);


                ViewBag.bHaveData = "Y";
                ViewBag.aplyNo = aplyNo;
                return View(roleData);

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
        public JsonResult qryRoleFuncHis(String aplyNo)
        {
            CodeRoleFuncHisDao codeRoleFuncHisDao = new CodeRoleFuncHisDao();

            try
            {
                List<RoleFuncHisModel> rows = new List<RoleFuncHisModel>();

                rows = codeRoleFuncHisDao.qryByAplyNo(aplyNo);



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
        /// 核可/退回(角色覆核)
        /// </summary>
        /// <param name="aplyNo"></param>
        /// <param name="roleId"></param>
        /// <param name="apprStatus"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult execReviewR(string aplyNo, string roleId, string apprStatus)
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

                    if(authAppr.CREATE_UID.Equals(Session["UserID"].ToString()))
                        return Json(new { success = false, errors = "申請人與覆核人員不可相同!!" }, JsonRequestBehavior.AllowGet);

                    //異動角色資料檔
                    //string cExecType = "";
                    CodeRoleHisDao codeRoleHisDao = new CodeRoleHisDao();
                    CodeRoleDao codeRoleDao = new CodeRoleDao();
                    CODE_ROLE cODEROLEO = new CODE_ROLE();

                    CODE_ROLE_HIS codeRoleHis = codeRoleHisDao.qryByAplyNo(aplyNo);
                    string execAction = "";
                    if (codeRoleHis != null)
                    {
                        execAction = StringUtil.toString(codeRoleHis.EXEC_ACTION);
                    }

                    if ("A".Equals(execAction))  //新增角色
                    {

                    }
                    else
                    {  //異動角色
                        cODEROLEO = codeRoleDao.qryRoleByKey(roleId);
                    }



                    if ("A".Equals(execAction))
                    {
                        if ("2".Equals(apprStatus))
                        {
                            cODEROLEO.ROLE_ID = StringUtil.toString(codeRoleHis.ROLE_ID);
                            cODEROLEO.ROLE_NAME = StringUtil.toString(codeRoleHis.ROLE_NAME);
                            cODEROLEO.AUTH_UNIT = StringUtil.toString(codeRoleHis.AUTH_UNIT);
                            cODEROLEO.FREE_AUTH = StringUtil.toString(codeRoleHis.FREE_AUTH);
                            cODEROLEO.IS_DISABLED = codeRoleHis.IS_DISABLED;
                            cODEROLEO.MEMO = StringUtil.toString(codeRoleHis.MEMO);
                            cODEROLEO.DATA_STATUS = "1";
                            cODEROLEO.CREATE_UID = authAppr.CREATE_UID;
                            cODEROLEO.CREATE_DT = authAppr.CREATE_DT;
                            cODEROLEO.LAST_UPDATE_UID = StringUtil.toString(authAppr.CREATE_UID);
                            cODEROLEO.LAST_UPDATE_DT = authAppr.CREATE_DT;
                            cODEROLEO.APPR_UID = Session["UserID"].ToString();
                            cODEROLEO.APPR_DT = DateTime.Now;

                            int cnt = codeRoleDao.Create(cODEROLEO, conn, transaction);

                            //新增LOG
                            Log log = new Log();
                            log.CFUNCTION = "角色管理-新增";
                            log.CACTION = "A";
                            log.CCONTENT = codeRoleDao.roleLogContent(cODEROLEO);
                            LogDao.Insert(log, Session["UserID"].ToString());

                            //新增稽核軌跡    20190515
                            procTrackLog("A", log.CCONTENT, cnt, cODEROLEO.ROLE_ID, "codeRole", conn, transaction);
                        }

                    }
                    else
                    {
                        //新增LOG
                        Log log = new Log();
                        log.CFUNCTION = "角色管理-修改";
                        log.CACTION = "U";
                        log.CCONTENT = codeRoleDao.roleLogContent(cODEROLEO);
                        LogDao.Insert(log, Session["UserID"].ToString());

                        cODEROLEO.DATA_STATUS = "1";
                        cODEROLEO.LAST_UPDATE_UID = StringUtil.toString(authAppr.CREATE_UID);
                        cODEROLEO.LAST_UPDATE_DT = authAppr.CREATE_DT;
                        cODEROLEO.APPR_UID = Session["UserID"].ToString();
                        cODEROLEO.APPR_DT = DateTime.Now;
                        cODEROLEO.FREEZE_DT = null;
                        cODEROLEO.FREEZE_UID = "";

                        if ("U".Equals(execAction) && "2".Equals(apprStatus))
                        {
                            cODEROLEO.FREE_AUTH = StringUtil.toString(codeRoleHis.FREE_AUTH);
                            cODEROLEO.ROLE_NAME = StringUtil.toString(codeRoleHis.ROLE_NAME);
                            cODEROLEO.IS_DISABLED = codeRoleHis.IS_DISABLED;
                            cODEROLEO.MEMO = StringUtil.toString(codeRoleHis.MEMO);
                        }


                        int cnt = codeRoleDao.Update(cODEROLEO, conn, transaction);

                        //新增稽核軌跡  add by daiyu 20190515
                        procTrackLog("E", log.CCONTENT, cnt, cODEROLEO.ROLE_ID, "codeRole", conn, transaction);
                    }


                    //覆核狀態=核可時
                    if ("2".Equals(apprStatus)) {
                        procRoleFuncHis(roleId, aplyNo, conn, transaction); //異動角色功能
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

        

        /// <summary>
        /// 處理角色功能異動
        /// </summary>
        /// <param name="roleId"></param>
        /// <param name="aplyNO"></param>
        /// <param name="conn"></param>
        /// <param name="transaction"></param>
        private void procRoleFuncHis(string roleId, string aplyNo, SqlConnection conn, SqlTransaction transaction)
        {
            CodeRoleFuncHisDao codeRoleFuncHisDao = new CodeRoleFuncHisDao();
            List<RoleFuncHisModel> cRoleFuncList = codeRoleFuncHisDao.qryByAplyNo(aplyNo);
            if (cRoleFuncList != null)
            {
                if (cRoleFuncList.Count > 0)
                {
                    CodeRoleFunctionDao roleFuncDao = new CodeRoleFunctionDao();

                    foreach (RoleFuncHisModel d in cRoleFuncList)
                    {
                        if ("A".Equals(d.execAction))
                        {
                            CODE_ROLE_FUNC dFunc = new CODE_ROLE_FUNC();
                            dFunc.ROLE_ID = roleId;
                            dFunc.FUNC_ID = d.cFunctionID;
                            dFunc.LAST_UPDATE_UID = Session["UserID"].ToString();
                            dFunc.LAST_UPDATE_DT = DateTime.Now;


                            //新增資料
                            roleFuncDao.Insert(dFunc, conn, transaction);


                            //新增LOG
                            Log log = new Log();
                            log.CFUNCTION = "角色管理(功能授權)-新增";
                            log.CACTION = "A";
                            log.CCONTENT = roleFuncDao.logContent(dFunc);
                            LogDao.Insert(log, Session["UserID"].ToString());

                            //新增稽核軌跡  add by daiyu 20190515
                            procTrackLog("A", log.CCONTENT, 1, roleId, "codeRoleFunc", conn, transaction);
                        }
                        else
                        {
                            CODE_ROLE_FUNC dFunc = roleFuncDao.getFuncRoleByKey(roleId, d.cFunctionID);


                            //新增LOG
                            Log log = new Log();
                            log.CFUNCTION = "角色管理(功能授權)-刪除";
                            log.CACTION = "D";
                            log.CCONTENT = roleFuncDao.logContent(dFunc);
                            LogDao.Insert(log, Session["UserID"].ToString());

                            //刪除資料
                            roleFuncDao.Delete(dFunc, conn, transaction);

                            //新增稽核軌跡  add by daiyu 20190515
                            procTrackLog("D", log.CCONTENT, 1, roleId, "codeRoleFunc", conn, transaction);

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
        /// 稽核跡軌
        /// </summary>
        /// <param name="type"></param>
        /// <param name="content"></param>
        /// <param name="rowCnt"></param>
        /// <param name="owner"></param>
        /// <param name="conn"></param>
        /// <param name="transaction"></param>
        public void procTrackLog(string type, string content, int rowCnt, string owner, string accessobj, SqlConnection conn, SqlTransaction transaction)
        {

            PIA_LOG_MAIN piaLog = new PIA_LOG_MAIN();
            piaLog.TRACKING_TYPE = "C";
            piaLog.ACCESS_ACCOUNT = Session["UserID"].ToString();
            piaLog.ACCOUNT_NAME = Session["UserName"].ToString();
            piaLog.PROGFUN_NAME = "RoleAppr";
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