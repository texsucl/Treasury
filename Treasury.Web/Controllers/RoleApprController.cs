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
/// 功能說明：角色權限覆核作業
/// 初版作者：20180511 黃黛鈺
/// 修改歷程：20180511 黃黛鈺 
///           需求單號：
///           初版
/// ==============================================
/// 修改日期/修改人：
/// 需求單號：
/// 修改內容：
/// ==============================================
/// </summary>
/// 
namespace Treasury.Web.Controllers
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

            String opScope = "";

            String[] roleInfo = authUtil.chkUserFuncAuth(Session["UserID"].ToString(), "~/RoleAppr/");
            if (roleInfo != null && roleInfo.Length == 1)
                opScope = roleInfo[0];


            ViewBag.opScope = opScope;



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
                        rows = authApprDao.qryAuthReview("R", "1", db);
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

                foreach (AuthReviewModel d in rows)
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
                AuthApprDao AuthApprDao = new AuthApprDao();

                AUTH_APPR authAppr = new AUTH_APPR();



                if (!"".Equals(StringUtil.toString(aplyNo))) {
                    authAppr = AuthApprDao.qryByKey(aplyNo);
                    ViewBag.bView = "N";
                }

                else {
                    authAppr = AuthApprDao.qryByFreeRole(roleId);
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
                roleData.roleName = "";
                roleData.isDisabled = "";
                roleData.memo = "";
                roleData.roleNameB = "";
                roleData.isDisabledB = "";
                roleData.memoB = "";


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
                    roleData.roleAuthType = StringUtil.toString(codeRole.ROLE_AUTH_TYPE);
                    roleData.roleNameB = StringUtil.toString(codeRole.ROLE_NAME);
                    roleData.isDisabledB = StringUtil.toString(codeRole.IS_DISABLED);
                    roleData.memoB = StringUtil.toString(codeRole.MEMO);
                }
                else {
                    roleData.roleId = StringUtil.toString(codeRoleHis.ROLE_ID);
                    roleData.roleAuthType = StringUtil.toString(codeRoleHis.ROLE_AUTH_TYPE);

                    if ("A".Equals(execAction))
                    {
                        roleData.roleName = StringUtil.toString(codeRoleHis.ROLE_NAME);
                        roleData.isDisabled = StringUtil.toString(codeRoleHis.IS_DISABLED);
                        roleData.memo = StringUtil.toString(codeRoleHis.MEMO);
                    }
                    else {
                        roleData.roleName = StringUtil.toString(codeRoleHis.ROLE_NAME);
                        roleData.isDisabled = StringUtil.toString(codeRoleHis.IS_DISABLED);
                        roleData.memo = StringUtil.toString(codeRoleHis.MEMO);

                        roleData.roleNameB = StringUtil.toString(codeRoleHis.ROLE_NAME_B);
                        roleData.isDisabledB = StringUtil.toString(codeRoleHis.IS_DISABLED_B);
                        roleData.memoB = StringUtil.toString(codeRoleHis.MEMO_B);
                    }
                }


                SysCodeDao sysCodeDao = new SysCodeDao();
                SYS_CODE sysCode = new SYS_CODE();
                sysCode = sysCodeDao.qryByKey("ROLE_AUTH_TYPE", StringUtil.toString(roleData.roleAuthType));
                if (sysCode != null)
                    roleData.roleAuthTypeDesc = StringUtil.toString(sysCode.CODE_VALUE);

                sysCode = sysCodeDao.qryByKey("IS_DISABLED", StringUtil.toString(roleData.isDisabledB));
                if (sysCode != null)
                    roleData.isDisabledB = StringUtil.toString(sysCode.CODE_VALUE);

                sysCode = sysCodeDao.qryByKey("IS_DISABLED", StringUtil.toString(roleData.isDisabled));
                if (sysCode != null)
                    roleData.isDisabled = StringUtil.toString(sysCode.CODE_VALUE);
                

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
        public JsonResult qryRoleFuncHis(string roleId, string aplyNo)
        {
            CodeRoleFuncHisDao codeRoleFuncHisDao = new CodeRoleFuncHisDao();

            try
            {
                List<RoleFuncHisModel> rows = new List<RoleFuncHisModel>();

                rows = codeRoleFuncHisDao.qryByAplyNo(aplyNo);

                if (rows.Count == 0) {
                    CodeRoleFunctionDao codeRoleFuncDao = new CodeRoleFunctionDao();
                    rows = codeRoleFuncDao.qryForAppr(roleId);

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
        /// 查詢金庫設備異動明細資料
        /// </summary>
        /// <param name="aplyNo"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult qryRoleEquipHis(string roleId, string aplyNo)
        {
            CodeRoleTreaItemHisDao codeRoleTreaItemHisDao = new CodeRoleTreaItemHisDao();

            try
            {
                List<CodeRoleEquipModel> rows = new List<CodeRoleEquipModel>();

                rows = codeRoleTreaItemHisDao.qryByAplyNo(aplyNo);

                if (rows.Count == 0)
                {
                    CodeRoleTreaItemDao codeRoleTreaItemDao = new CodeRoleTreaItemDao();
                    rows = codeRoleTreaItemDao.qryForRoleMgr(roleId);

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
        /// 查詢存取項目/表單申請異動明細資料
        /// </summary>
        /// <param name="aplyNo"></param>
        /// <param name="authType"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult qryRoleItemHis(string roleId,string aplyNo, string authType)
        {
            CodeRoleItemHisDao codeRoleItemHisDao = new CodeRoleItemHisDao();

            try
            {
                List<CodeRoleItemModel> rows = new List<CodeRoleItemModel>();

                rows = codeRoleItemHisDao.qryByAplyNo(aplyNo, authType);

                if (rows.Count == 0)
                {
                    CodeRoleItemDao codeRoleItemDao = new CodeRoleItemDao();
                    rows = codeRoleItemDao.qryForAppr(roleId, authType);

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
        /// 核可/退回(角色覆核)
        /// </summary>
        /// <param name="aplyNo"></param>
        /// <param name="roleId"></param>
        /// <param name="apprStatus"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult execReviewR(string aplyNo, string roleId, string apprStatus)
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

                    if(StringUtil.toString(authAppr.CREATE_UID).Equals(Session["UserID"].ToString()))
                        return Json(new { success = false, errors = "覆核人員與申請人員相同，不可執行覆核作業!!" }, JsonRequestBehavior.AllowGet);



                    //異動角色資料檔
                    string cExecType = "";
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
                            cODEROLEO.ROLE_AUTH_TYPE = StringUtil.toString(codeRoleHis.ROLE_AUTH_TYPE);
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
                            cODEROLEO.ROLE_NAME = StringUtil.toString(codeRoleHis.ROLE_NAME);
                            cODEROLEO.IS_DISABLED = codeRoleHis.IS_DISABLED;
                            cODEROLEO.MEMO = StringUtil.toString(codeRoleHis.MEMO);
                        }


                        int cnt = codeRoleDao.Update(cODEROLEO, conn, transaction);

                    }


                    //覆核狀態=核可時
                    if ("2".Equals(apprStatus)) {
                        procRoleFuncHis(roleId, aplyNo, conn, transaction); //異動角色功能

                        procRoleEquipHis(roleId, aplyNo, conn, transaction); //異動角色設備功能

                        procRoleItemHis(roleId, aplyNo, conn, transaction); //異動存取項目、表單申請權限

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
                        CODE_ROLE_FUNC dFunc = new CODE_ROLE_FUNC();
                        Log log = new Log();
                        switch (d.execAction) {
                            case "A":                                
                                dFunc.ROLE_ID = roleId;
                                dFunc.SYS_CD = "TREASURY";
                                dFunc.FUNC_ID = d.cFunctionID;
                                dFunc.LAST_UPDATE_UID = Session["UserID"].ToString();
                                dFunc.LAST_UPDATE_DT = DateTime.Now;


                                //新增資料
                                roleFuncDao.Insert(dFunc, conn, transaction);


                                //新增LOG
                                log.CFUNCTION = "角色管理(功能授權)-新增";
                                log.CACTION = "A";
                                log.CCONTENT = roleFuncDao.logContent(dFunc);
                                LogDao.Insert(log, Session["UserID"].ToString());

                                break;
                            case "D":
                                dFunc = roleFuncDao.getFuncRoleByKey(roleId, d.cFunctionID);

                                //新增LOG
                                log.CFUNCTION = "角色管理(功能授權)-刪除";
                                log.CACTION = "D";
                                log.CCONTENT = roleFuncDao.logContent(dFunc);
                                LogDao.Insert(log, Session["UserID"].ToString());

                                //刪除資料
                                roleFuncDao.Delete(dFunc, conn, transaction);
                                break;
                            default:
                                break;
                        }


                    }

                }
            }
        }



        /// <summary>
        /// 處理角色設備異動
        /// </summary>
        /// <param name="roleId"></param>
        /// <param name="aplyNO"></param>
        /// <param name="conn"></param>
        /// <param name="transaction"></param>
        private void procRoleEquipHis(string roleId, string aplyNo, SqlConnection conn, SqlTransaction transaction)
        {
            CodeRoleTreaItemHisDao codeRoleTreaItemHisDao = new CodeRoleTreaItemHisDao();
            List<CodeRoleEquipModel> cRoleEquipList = codeRoleTreaItemHisDao.qryByAplyNo(aplyNo);
            if (cRoleEquipList != null)
            {
                if (cRoleEquipList.Count > 0)
                {
                    CodeRoleTreaItemDao codeRoleTreaItemDao = new CodeRoleTreaItemDao();

                    foreach (CodeRoleEquipModel d in cRoleEquipList)
                    {
                        CODE_ROLE_TREA_ITEM dEquip = new CODE_ROLE_TREA_ITEM();
                        Log log = new Log();

                        switch (d.execAction)
                        {
                            case "A":
                                dEquip.ROLE_ID = roleId;
                                dEquip.TREA_EQUIP_ID = StringUtil.toString(d.treaEquipId);
                                dEquip.CUSTODY_MODE = StringUtil.toString(d.custodyMode);
                                dEquip.CUSTODY_ORDER = Convert.ToInt16(d.custodyOrder);
                                dEquip.LAST_UPDATE_UID = Session["UserID"].ToString();
                                dEquip.LAST_UPDATE_DT = DateTime.Now;


                                //新增資料
                                codeRoleTreaItemDao.Insert(dEquip, conn, transaction);


                                //新增LOG

                                log.CFUNCTION = "角色管理(金庫設備)-新增";
                                log.CACTION = "A";
                                log.CCONTENT = codeRoleTreaItemDao.logContent(dEquip);
                                LogDao.Insert(log, Session["UserID"].ToString());

                                break;
                            case "U":
                                dEquip = codeRoleTreaItemDao.getRoleEquipByKey(roleId, d.treaEquipId);

                                //新增LOG
                                log.CCONTENT = codeRoleTreaItemDao.logContent(dEquip);

                                log.CFUNCTION = "角色管理(金庫設備)-修改";
                                log.CACTION = "U";

                                LogDao.Insert(log, Session["UserID"].ToString());

                                dEquip.CUSTODY_MODE = StringUtil.toString(d.custodyMode);
                                dEquip.CUSTODY_ORDER = Convert.ToInt16(d.custodyOrder);
                                dEquip.LAST_UPDATE_UID = Session["UserID"].ToString();
                                dEquip.LAST_UPDATE_DT = DateTime.Now;

                                //修改資料
                                codeRoleTreaItemDao.Update(dEquip, conn, transaction);

                                break;
                            case "D":
                                dEquip = codeRoleTreaItemDao.getRoleEquipByKey(roleId, d.treaEquipId);

                                //新增LOG
                                log.CCONTENT = codeRoleTreaItemDao.logContent(dEquip);

                                log.CFUNCTION = "角色管理(金庫設備)-刪除";
                                log.CACTION = "D";

                                LogDao.Insert(log, Session["UserID"].ToString());

                                //刪除資料
                                codeRoleTreaItemDao.Delete(dEquip, conn, transaction);
                                break;

                            default:
                                break;
                        }
                    }
                }
            }
        }








        /// <summary>
        /// 處理角色存取項目異動
        /// </summary>
        /// <param name="roleId"></param>
        /// <param name="aplyNO"></param>
        /// <param name="conn"></param>
        /// <param name="transaction"></param>
        private void procRoleItemHis(string roleId, string aplyNo, SqlConnection conn, SqlTransaction transaction)
        {
            CodeRoleItemHisDao codeRoleItemHisDao = new CodeRoleItemHisDao();
            List<CodeRoleItemModel> cRoleItemList = codeRoleItemHisDao.qryByAplyNo(aplyNo, "");
            if (cRoleItemList != null)
            {
                if (cRoleItemList.Count > 0)
                {
                    CodeRoleItemDao codeRoleItemDao = new CodeRoleItemDao();

                    foreach (CodeRoleItemModel d in cRoleItemList)
                    {
                        CODE_ROLE_ITEM dItem = new CODE_ROLE_ITEM();
                        Log log = new Log();

                        switch (d.execAction)
                        {
                            case "A":
                                dItem.ROLE_ID = roleId;
                                dItem.ITEM_ID = d.itemId;
                                dItem.AUTH_TYPE = d.authType;
                                dItem.LAST_UPDATE_UID = Session["UserID"].ToString();
                                dItem.LAST_UPDATE_DT = DateTime.Now;


                                //新增資料
                                codeRoleItemDao.Insert(dItem, conn, transaction);


                                //新增LOG
                                log.CFUNCTION = "角色管理(存取項目)-新增";
                                log.CACTION = "A";
                                log.CCONTENT = codeRoleItemDao.logContent(dItem);
                                LogDao.Insert(log, Session["UserID"].ToString());

                                break;
                            case "D":
                                dItem = codeRoleItemDao.getRoleItemByKey(roleId, d.itemId, d.authType);

                                //新增LOG
                                log.CFUNCTION = "角色管理(存取項目)-刪除";
                                log.CACTION = "D";
                                log.CCONTENT = codeRoleItemDao.logContent(dItem);
                                LogDao.Insert(log, Session["UserID"].ToString());

                                //刪除資料
                                codeRoleItemDao.Delete(dItem, conn, transaction);

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
        
    }
}