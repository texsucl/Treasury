
using SSO.Web.Daos;
using SSO.Web.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web.Mvc;
using SSO.Web.ActionFilter;
using SSO.Web.BO;
using SSO.Web.ViewModels;
using SSO.Web.Utils;

/// <summary>
/// 功能說明：功能管理
/// 初版作者：20180430 黃黛鈺
/// 修改歷程：20170430 黃黛鈺 
///           需求單號：
///           初版
/// </summary>
/// 

namespace SSO.Web.Controllers
{
    [Authorize]
    [CheckSessionFilterAttribute]

    public class FuncMgrController : BaseController
    {
        static private NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        /**
       畫面初始
           **/
        public ActionResult Index()
        {
            UserAuthUtil authUtil = new UserAuthUtil();


            string opScope = "";
            string funcName = "";

            String[] roleInfo = authUtil.chkUserFuncAuth(Session["UserID"].ToString(), "~/FuncMgr/");
            if (roleInfo != null && roleInfo.Length == 2)
            {
                opScope = "1";
                funcName = roleInfo[1];
            }


            ViewBag.opScope = opScope;
            ViewBag.funcName = funcName;

            /*---畫面下拉選單初始值---*/
            SysCodeDao sysCodeDao = new SysCodeDao();
            //停用註記
            var FlagList = sysCodeDao.loadSelectList("SSO", "IS_DISABLED", false);
            ViewBag.FlagList = FlagList;

            //系統別
            CodeSysInfo codeSysInfo = new CodeSysInfo();
            var SysList = codeSysInfo.loadSelectList(true);
            ViewBag.SysList = SysList;

            bool bAdmin = authUtil.chkAdmin("SSO", Session["UserID"].ToString());

            ViewBag.opScope = opScope;

            if (bAdmin)
            {
                ViewBag.authUnit = "";
                ViewBag.authUnitNm = "";
            }
            else
            {
                ViewBag.authUnit = Session["UserUnit"].ToString();
                ViewBag.authUnitNm = Session["UserUnitNm"].ToString();
            }

            return View();
        }



        /// <summary>
        /// 查詢功能明細資訊
        /// </summary>
        /// <param name="cFunctionID"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult qryDetail(String cFunctionID)
        {
            CodeFuncDao codeFunctionDao = new CodeFuncDao();
            FuncMgrDetailModel funcDetail = codeFunctionDao.qryByKey(cFunctionID);



            var jsonData = new { success = true, funcDetail = funcDetail };
            return Json(jsonData, JsonRequestBehavior.AllowGet);
        }


        /// <summary>
        /// 新增功能
        /// </summary>
        /// <param name="cParentFunctionID"></param>
        /// <param name="cFunctionName"></param>
        /// <param name="iSortBy"></param>
        /// <param name="cFlag"></param>
        /// <param name="vFunctionUrl"></param>
        /// <param name="vFunctionMemo"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult execInsert(string sysCd, string authUnit,
            string cParentFunctionID, string iFunctionLevelP, string cFunctionName, int iSortBy, string cFlag, string vFunctionUrl, string vFunctionMemo)
        {

            if (!authUnit.Equals(Session["UserUnit"]))
            {
                var jsonData = new { success = false, err = "此功能的授權單位為" + authUnit + "，不可修改!" };
                return Json(jsonData, JsonRequestBehavior.AllowGet);
            }


            CodeFuncDao codeFuncDao = new CodeFuncDao();

            if (codeFuncDao.chkFuncName(sysCd, "", cFunctionName) != null)
            {
                var jsonData = new { success = false, err = "功能名稱：”功能名稱”已存在，請重新輸入!" };
                return Json(jsonData, JsonRequestBehavior.AllowGet);

            }

            CODE_FUNC func = new CODE_FUNC();
            func.SYS_CD = sysCd;
            func.AUTH_UNIT = authUnit.ToUpper();
            func.PARENT_FUNC_ID = cParentFunctionID;
            func.FUNC_NAME = cFunctionName;
            func.FUNC_LEVEL = Convert.ToInt16(iFunctionLevelP) + 1;
            func.FUNC_MEMO = vFunctionMemo;
            func.FUNC_URL = vFunctionUrl;
            func.FUNC_ORDER = iSortBy;
            func.IS_DISABLED = cFlag;
            func.CREATE_UID = Session["UserID"].ToString();

            string strConn = DbUtil.GetDBFglConnStr();
            using (SqlConnection conn = new SqlConnection(strConn))
            {
                conn.Open();

                SqlTransaction transaction = conn.BeginTransaction("Transaction");

                try
                {
                    func = codeFuncDao.insert(func, conn, transaction);

                    Log log = new Log();
                    log.CFUNCTION = "功能維護作業-新增";
                    log.CACTION = "A";
                    log.CCONTENT = codeFuncDao.funcLogContent(func);
                    LogDao.Insert(log, Session["UserID"].ToString());

                    transaction.Commit();

                    var jsonData = new { success = true, id = func.FUNC_ID.Trim() };
                    return Json(jsonData, JsonRequestBehavior.AllowGet);
                }
                catch (Exception e)
                {
                    transaction.Rollback();
                    logger.Error("[execInsert]其它錯誤：" + e.ToString());
                    var jsonData = new { success = false, err = "其它錯誤，請洽系統管理員!!" };
                    return Json(jsonData, JsonRequestBehavior.AllowGet);
                }
            }


        }


        /// <summary>
        /// 取得授權單位中文
        /// </summary>
        /// <param name="authUnit"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult getAuthUnitNm(string authUnit)
        {
            OaDeptDao oaDeptDao = new OaDeptDao();
            VW_OA_DEPT dept = new VW_OA_DEPT();
            dept = oaDeptDao.qryByDptCd(authUnit);
            string authUnitNm = "";
            if(dept != null)
                authUnitNm = StringUtil.toString(dept.DPT_NAME);

            var jsonData = new { success = true, authUnitNm = authUnitNm };
            return Json(jsonData, JsonRequestBehavior.AllowGet);
        }




        /// <summary>
        /// 取得父功能下面所有子功能目前最大的編號
        /// </summary>
        /// <param name="cParentFunctionID"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult qrySort(String cParentFunctionID)
        {
            CodeFuncDao codeFuncDao = new CodeFuncDao();
            // FuncMgrDetailModel funcDetail = codeFunctionDao.qryByKey(cFunctionID);
            int sort = codeFuncDao.qrySort(cParentFunctionID);


            var jsonData = new { success = true, sort = sort };
            return Json(jsonData, JsonRequestBehavior.AllowGet);
        }



        /// <summary>
        /// 執行"修改"
        /// </summary>
        /// <param name="cFunctionName"></param>
        /// <param name="iSortBy"></param>
        /// <param name="cFlag"></param>
        /// <param name="vFunctionUrl"></param>
        /// <param name="vFunctionMemo"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult execModify(string sysCd, string authUnit, string cFunctionId, string cFunctionName, int iSortBy
            , string cFlag, string vFunctionUrl, string vFunctionMemo)
        {

            if (!authUnit.Equals(Session["UserUnit"])) {
                var jsonData = new { success = false, err = "此功能的授權單位為"+ authUnit + "，不可修改!" };
                return Json(jsonData, JsonRequestBehavior.AllowGet);
            }


            CodeFuncDao codeFuncDao = new CodeFuncDao();

            if (codeFuncDao.chkFuncName(sysCd, cFunctionId, cFunctionName) != null)
            {
                var jsonData = new { success = false, err = "功能名稱：”功能名稱”已存在，請重新輸入!" };
                return Json(jsonData, JsonRequestBehavior.AllowGet);
            }

            FuncMgrDetailModel funcO = codeFuncDao.qryByKey(cFunctionId);

            CODE_FUNC func = new CODE_FUNC();
            func.SYS_CD = sysCd;
            func.AUTH_UNIT = authUnit.ToUpper();
            func.FUNC_ID = cFunctionId;
            func.PARENT_FUNC_ID = funcO.cParentFunctionID;
            func.FUNC_NAME = StringUtil.toString(cFunctionName);
            func.FUNC_LEVEL = funcO.iFunctionLevel;
            func.FUNC_MEMO = StringUtil.toString(vFunctionMemo);
            func.FUNC_URL = vFunctionUrl;
            func.FUNC_ORDER = iSortBy;
            func.IS_DISABLED = cFlag;
            func.CREATE_UID = funcO.cCrtUserID;
            func.CREATE_DT = DateUtil.stringToDatetime(funcO.cCrtDate);
            
            func.LAST_UPDATE_UID = Session["UserID"].ToString();
            func.LAST_UPDATE_DT = DateTime.Now;


            string strConn = DbUtil.GetDBFglConnStr();
            using (SqlConnection conn = new SqlConnection(strConn))
            {
                conn.Open();

                SqlTransaction transaction = conn.BeginTransaction("Transaction");

                try
                {
                    func = codeFuncDao.update(func, conn, transaction);


                    Log log = new Log();
                    log.CFUNCTION = "功能維護作業-修改";
                    log.CACTION = "U";
                    log.CCONTENT = codeFuncDao.funcLogContent(func);
                    LogDao.Insert(log, Session["UserID"].ToString());


                    transaction.Commit();

                    var jsonData = new { success = true, id = func.FUNC_ID.Trim() };
                    return Json(jsonData, JsonRequestBehavior.AllowGet);
                }
                catch (Exception e)
                {
                    transaction.Rollback();
                    logger.Error("[execModify]其它錯誤：" + e.ToString());

                    var jsonData = new { success = false, err = "其它錯誤，請洽系統管理員!!" };
                    return Json(jsonData, JsonRequestBehavior.AllowGet);
                }
            }
        }


        [HttpPost]
        public JsonResult GetTreeJson(string sysCd)
        {
            List<FuncMgrModel> funcMenu = new List<FuncMgrModel>();
            CodeFuncDao codeFuncDao = new CodeFuncDao();

            List<CODE_FUNC> funcList = codeFuncDao.qryDetail(sysCd);
            string rootNodeId = "";
            if ("".Equals(sysCd))
                rootNodeId = "0";
            else {
                CODE_FUNC root = new CODE_FUNC();
                root = codeFuncDao.qryRootItem(sysCd);
                if (root != null)
                    rootNodeId = StringUtil.toString(root.PARENT_FUNC_ID);
            }
                

            FuncMgrModel rootItem = new FuncMgrModel();
            rootItem.nodeId = rootNodeId;
            rootItem.text = "根節點";
            funcMenu.Add(rootItem);

            var nodes = ChildrenOf(funcList, rootItem);

            return Json(nodes, JsonRequestBehavior.AllowGet);

        }


        public static FuncMgrModel ChildrenOf(List<CODE_FUNC> funcList, FuncMgrModel func)
        {
            

            foreach (CODE_FUNC child in funcList.Where(x => x.PARENT_FUNC_ID.Trim() == func.nodeId).OrderBy(x => x.FUNC_ORDER))
            {
                FuncMgrModel item = new FuncMgrModel();
                item.Id = child.FUNC_ID;
                item.nodeId =child.FUNC_ID;
                item.parentId = Convert.ToInt32(child.PARENT_FUNC_ID);
                item.text = child.FUNC_NAME.Trim();

                if (StringUtil.toString(child.FUNC_URL).Length > 0)
                    item.nodes = null;

                func.nodes.Add(ChildrenOf(funcList, item));
            }

            return func;
        }

    }
}