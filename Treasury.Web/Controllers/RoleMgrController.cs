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
using System.Web.Mvc;
using Treasury.Web;
using Treasury.Web.Daos;
using Treasury.Web.Models;
using Treasury.Web.ViewModels;
using System.Transactions;

/// <summary>
/// 功能說明：角色管理
/// 初版作者：20180430 黃黛鈺
/// 修改歷程：20180430 黃黛鈺 
///           需求單號：201803140070-00
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
    public class RoleMgrController : BaseController
    {
        static private NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public ActionResult Index()
        {
            UserAuthUtil authUtil = new UserAuthUtil();
            String opScope = "";
            String roleId = "";
            String funcType = "";
            String[] roleInfo = authUtil.chkUserFuncAuth(Session["UserID"].ToString(), "~/RoleMgr/");
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
        public ActionResult Index(String cRoleID)
        {
            UserAuthUtil authUtil = new UserAuthUtil();
            String opScope = "";
            String roleId = "";
            String funcType = "";
            String[] roleInfo = authUtil.chkUserFuncAuth(Session["UserID"].ToString(), "~/RoleMgr/");
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

            RoleMgrModel roleMgrModel = new RoleMgrModel();


            if (!"".Equals(StringUtil.toString(cRoleID)))
            {
                CODE_ROLE codeRole = new CODE_ROLE();
                codeRole = codeRoleDao.qryRoleByKey(cRoleID);


                roleMgrModel.cRoleID = StringUtil.toString(codeRole.ROLE_ID);
                roleMgrModel.cRoleName = StringUtil.toString(codeRole.ROLE_NAME);
                roleMgrModel.roleAuthType = StringUtil.toString(codeRole.ROLE_AUTH_TYPE);
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


        [HttpGet]
        public ActionResult roleHis(String cRoleID)
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

            //角色群組
            var roleAuthTypeList = sysCodeDao.loadSelectList("ROLE_AUTH_TYPE");
            ViewBag.roleAuthTypeList = roleAuthTypeList;


            RoleMgrHisModel roleMgrHisModel = new RoleMgrHisModel();


            if (!"".Equals(StringUtil.toString(cRoleID)))
            {
                CodeRoleDao codeRoleDao = new CodeRoleDao();
                CODE_ROLE codeRole = new CODE_ROLE();
                codeRole = codeRoleDao.qryRoleByKey(cRoleID);


                roleMgrHisModel.cRoleID = StringUtil.toString(codeRole.ROLE_ID);
                roleMgrHisModel.cRoleName = StringUtil.toString(codeRole.ROLE_NAME);
                roleMgrHisModel.roleAuthType = StringUtil.toString(codeRole.ROLE_AUTH_TYPE);

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
        /// <param name="CodeRole"></param>
        /// <param name="cSearchArea"></param>
        /// <param name="cOperatorArea"></param>
        /// <param name="cFlag"></param>
        /// <param name="vMemo"></param>
        /// <param name="cUpdUserID"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult LoadData(string codeRole, string roleAuthType, string isDIsabled, string vMemo, string cUpdUserID)
        {
            CodeRoleDao codeRoleDao = new CodeRoleDao();
            OaEmpDao oaEmpDao = new OaEmpDao();

            List<RoleMgrModel> rows = new List<RoleMgrModel>();
            rows = codeRoleDao.roleMgrQry(codeRole, roleAuthType, isDIsabled, vMemo, cUpdUserID);

            Dictionary<string, string> userNameMap = new Dictionary<string, string>();
            string userUId = "";
            string userFId = "";

            using (new TransactionScope(
                   TransactionScopeOption.Required,
                   new TransactionOptions
                   {
                       IsolationLevel = IsolationLevel.ReadUncommitted
                   }))
            {
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
                            role.cUpdUserID = userUId + " " + userNameMap[userUId];
                        }

                        if (!"".Equals(userFId))
                        {
                            if (!userNameMap.ContainsKey(userFId))
                            {
                                userNameMap = oaEmpDao.qryUsrName(userNameMap, userFId, dbIntra);
                            }
                            role.freezeUid = userFId + " " + userNameMap[userFId];
                        }

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
        public ActionResult updateRole(RoleMgrModel roleMgrModel, String authFunc
            , List<CodeRoleEquipModel> equipData, List<CodeRoleItemModel> itemData, List<CodeRoleItemModel> formAplyData)
        {
            string roleId = StringUtil.toString(roleMgrModel.cRoleID);
            bool bChgRole = false;
            bool bChgFunc = false;
            bool bChgEquip = false;
            bool bChgItem = false;
            bool bChgFormAply = false;
            bool bNewRole = false;

            
            CodeRoleDao codeRoleDao = new CodeRoleDao();
            CODE_ROLE codeRoleO = new CODE_ROLE();

            bool bDupRoleName = codeRoleDao.dupRoleName(StringUtil.toString(roleMgrModel.cRoleID), 
                StringUtil.toString(roleMgrModel.roleAuthType), StringUtil.toString(roleMgrModel.cRoleName));
            if(bDupRoleName)
                return Json(new { success = false, errors = "相同角色群組下,不能建置相同的角色名稱!!" }, JsonRequestBehavior.AllowGet);


            //比對是否有異動"角色資訊"
            if ("".Equals(roleId))
            {
                bNewRole = true;
                bChgRole = true;
                SysSeqDao sysSeqDao = new SysSeqDao();

                var cId = sysSeqDao.qrySeqNo("F1", "").ToString();
                roleId = "F1" + cId.ToString().PadLeft(8, '0');
            }
            else {
                codeRoleO = codeRoleDao.qryRoleByKey(roleId);
                if (!(
                    StringUtil.toString(roleMgrModel.cRoleName).Equals(StringUtil.toString(codeRoleO.ROLE_NAME))
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

            if ("F".Equals(roleMgrModel.roleAuthType)) {
                CodeRoleFunctionDao CodeRoleFunctionDao = new CodeRoleFunctionDao();
                List<FuncRoleModel> roleFuncListO = CodeRoleFunctionDao.qryForRoleMgr(roleId);
                foreach (string item in funcData)
                {
                    if (!"".Equals(StringUtil.toString(item)))
                    {
                        FuncRoleModel funcRoleModel = new FuncRoleModel();
                        funcRoleModel.cRoleId = roleId;
                        funcRoleModel.cFunctionID = item;
                        if (roleFuncListO.Exists(x => x.cFunctionID == item))
                        {
                            funcRoleModel.execAction = "";
                        }

                        else
                        {
                            bChgFunc = true;
                            funcRoleModel.execAction = "A";
                        }
                        funcList.Add(funcRoleModel);
                    }
                }

                foreach (FuncRoleModel oItem in roleFuncListO)
                {
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

            }



            //比對是否有異動"金庫設備權限"
            List<CodeRoleEquipModel> equipList = new List<CodeRoleEquipModel>();
            CodeRoleTreaItemDao codeRoleTreaItemDao = new CodeRoleTreaItemDao();
            if ("E".Equals(roleMgrModel.roleAuthType))
            {
                List<CodeRoleEquipModel> roleEquipListO = codeRoleTreaItemDao.qryForRoleMgr(roleId);

                if (equipData != null)
                {
                    foreach (CodeRoleEquipModel item in equipData)
                    {
                        CodeRoleEquipModel codeRoleEquipModel = new CodeRoleEquipModel();
                        codeRoleEquipModel.roleId = roleId;
                        codeRoleEquipModel.treaEquipId = StringUtil.toString(item.treaEquipId);
                        //codeRoleEquipModel.controlMode = StringUtil.toString(item.controlMode);
                        codeRoleEquipModel.custodyMode = StringUtil.toString(item.custodyMode);
                        codeRoleEquipModel.custodyOrder = StringUtil.toString(item.custodyOrder);
                        codeRoleEquipModel.custodyModeB = "";
                        codeRoleEquipModel.custodyOrderB = "";
                        codeRoleEquipModel.execAction = "";
                        if (roleEquipListO.Exists(x => x.treaEquipId == item.treaEquipId))
                        {
                            CodeRoleEquipModel itemO = roleEquipListO.Find(x => x.treaEquipId == item.treaEquipId);

                            if (!(
                                //item.controlMode.Equals(itemO.controlMode) && 
                                item.custodyMode.Equals(itemO.custodyMode) && item.custodyOrder.Equals(itemO.custodyOrder)))
                            {
                                codeRoleEquipModel.execAction = "U";
                                codeRoleEquipModel.custodyModeB = itemO.custodyMode;
                                codeRoleEquipModel.custodyOrderB = itemO.custodyOrder;
                                bChgEquip = true;
                            }
                        }
                        else
                        {
                            codeRoleEquipModel.execAction = "A";
                            bChgEquip = true;
                        }
                        equipList.Add(codeRoleEquipModel);
                    }
                }


                foreach (CodeRoleEquipModel oItem in roleEquipListO)
                {
                    if (equipList != null)
                    {
                        if (!equipList.Exists(x => x.treaEquipId == oItem.treaEquipId))
                        {
                            bChgEquip = true;
                            CodeRoleEquipModel codeRoleEquipModel = new CodeRoleEquipModel();
                            codeRoleEquipModel.roleId = roleId;
                            codeRoleEquipModel.treaEquipId = StringUtil.toString(oItem.treaEquipId);
                            codeRoleEquipModel.controlMode = StringUtil.toString(oItem.controlMode);
                            codeRoleEquipModel.custodyMode = StringUtil.toString(oItem.custodyMode);
                            codeRoleEquipModel.custodyOrder = StringUtil.toString(oItem.custodyOrder);
                            codeRoleEquipModel.execAction = "D";
                            equipList.Add(codeRoleEquipModel);
                        }
                    }
                    else
                    {
                        bChgEquip = true;
                        CodeRoleEquipModel codeRoleEquipModel = new CodeRoleEquipModel();
                        codeRoleEquipModel.roleId = roleId;
                        codeRoleEquipModel.treaEquipId = StringUtil.toString(oItem.treaEquipId);
                        codeRoleEquipModel.controlMode = StringUtil.toString(oItem.controlMode);
                        codeRoleEquipModel.custodyMode = StringUtil.toString(oItem.custodyMode);
                        codeRoleEquipModel.custodyOrder = StringUtil.toString(oItem.custodyOrder);
                        codeRoleEquipModel.execAction = "D";
                        equipList.Add(codeRoleEquipModel);

                    }

                }
            }


            //比對是否有異動"存取項目權限"
            CodeRoleItemDao codeRoleItemDao = new CodeRoleItemDao();
            List<CodeRoleItemModel> itemList = new List<CodeRoleItemModel>();

            if ("I".Equals(roleMgrModel.roleAuthType)) {
                List<CodeRoleItemModel> itemDataO = codeRoleItemDao.qryForRoleMgr(roleId, "1");

                if (itemData != null)
                {
                    foreach (CodeRoleItemModel item in itemData)
                    {
                        CodeRoleItemModel codeRoleItemModel = new CodeRoleItemModel();
                        codeRoleItemModel.roleId = roleId;
                        codeRoleItemModel.itemId = StringUtil.toString(item.itemId);
                        codeRoleItemModel.authType = "1";

                        if (itemDataO.Exists(x => x.itemId == item.itemId))
                        {
                            codeRoleItemModel.execAction = "";
                        }
                        else
                        {
                            bChgItem = true;
                            codeRoleItemModel.execAction = "A";
                        }
                        itemList.Add(codeRoleItemModel);
                    }
                }


                foreach (CodeRoleItemModel oItem in itemDataO)
                {
                    if (itemList != null)
                    {
                        if (!itemList.Exists(x => x.itemId == oItem.itemId))
                        {
                            bChgItem = true;
                            CodeRoleItemModel codeRoleItemModel = new CodeRoleItemModel();
                            codeRoleItemModel.roleId = roleId;
                            codeRoleItemModel.itemId = StringUtil.toString(oItem.itemId);
                            codeRoleItemModel.authType = "1";
                            codeRoleItemModel.execAction = "D";
                            itemList.Add(codeRoleItemModel);
                        }
                    }
                    else
                    {
                        bChgItem = true;
                        CodeRoleItemModel codeRoleItemModel = new CodeRoleItemModel();
                        codeRoleItemModel.roleId = roleId;
                        codeRoleItemModel.itemId = StringUtil.toString(oItem.itemId);
                        codeRoleItemModel.authType = "1";
                        codeRoleItemModel.execAction = "D";
                        itemList.Add(codeRoleItemModel);
                    }
                }

            }




            //比對是否有異動"表單申請權限"
            
            List<CodeRoleItemModel> formAplyList = new List<CodeRoleItemModel>();

            if ("A".Equals(roleMgrModel.roleAuthType))
            {
                List<CodeRoleItemModel> formAplyDataO = codeRoleItemDao.qryForRoleMgr(roleId, "2");
                if (formAplyData != null)
                {
                    foreach (CodeRoleItemModel item in formAplyData)
                    {
                        CodeRoleItemModel codeRoleItemModel = new CodeRoleItemModel();
                        codeRoleItemModel.roleId = roleId;
                        codeRoleItemModel.itemId = StringUtil.toString(item.itemId);
                        codeRoleItemModel.authType = "2";

                        if (formAplyDataO.Exists(x => x.itemId == item.itemId))
                        {
                            codeRoleItemModel.execAction = "";
                        }
                        else
                        {
                            bChgFormAply = true;
                            codeRoleItemModel.execAction = "A";
                        }
                        formAplyList.Add(codeRoleItemModel);
                    }
                }


                foreach (CodeRoleItemModel oItem in formAplyDataO)
                {
                    if (formAplyList != null)
                    {
                        if (!formAplyList.Exists(x => x.itemId == oItem.itemId))
                        {
                            bChgFormAply = true;
                            CodeRoleItemModel codeRoleItemModel = new CodeRoleItemModel();
                            codeRoleItemModel.roleId = roleId;
                            codeRoleItemModel.itemId = StringUtil.toString(oItem.itemId);
                            codeRoleItemModel.authType = "2";
                            codeRoleItemModel.execAction = "D";
                            formAplyList.Add(codeRoleItemModel);
                        }
                    }
                    else
                    {
                        bChgFormAply = true;
                        CodeRoleItemModel codeRoleItemModel = new CodeRoleItemModel();
                        codeRoleItemModel.roleId = roleId;
                        codeRoleItemModel.itemId = StringUtil.toString(oItem.itemId);
                        codeRoleItemModel.authType = "2";
                        codeRoleItemModel.execAction = "D";
                        formAplyList.Add(codeRoleItemModel);
                    }

                }
            }


                


            if(bChgRole == false && bChgFunc == false && bChgEquip == false && bChgItem == false && bChgFormAply == false)
                return Json(new { success = false, errors = "未異動畫面資料，將不進行修改覆核作業!!" }, JsonRequestBehavior.AllowGet);



            /*------------------ DB處理   begin------------------*/
            string strConn = DbUtil.GetDBTreasuryConnStr();
            using (SqlConnection conn = new SqlConnection(strConn))
            {

                conn.Open();

                SqlTransaction transaction = conn.BeginTransaction("Transaction");
                try
                {
                    roleMgrModel.cRoleID = roleId;

                    AuthApprDao authApprDao = new AuthApprDao();
                    AUTH_APPR authAppr = new AUTH_APPR();
                    authAppr.AUTH_APLY_TYPE = "R";
                    authAppr.APPR_STATUS = "1";
                    authAppr.APPR_MAPPING_KEY = roleId;
                    authAppr.CREATE_UID = Session["UserID"].ToString();


                    //新增"覆核資料檔"
                    string aplyNo = authApprDao.insert(authAppr, conn, transaction);


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
                        codeRoleHis.ROLE_NAME = StringUtil.toString(roleMgrModel.cRoleName);
                        codeRoleHis.ROLE_AUTH_TYPE = StringUtil.toString(roleMgrModel.roleAuthType);
                        codeRoleHis.IS_DISABLED = StringUtil.toString(roleMgrModel.isDisabled);
                        codeRoleHis.MEMO = StringUtil.toString(roleMgrModel.vMemo);

                        if (codeRoleO != null) {
                            codeRoleHis.ROLE_NAME_B = StringUtil.toString(codeRoleO.ROLE_NAME);
                            codeRoleHis.IS_DISABLED_B = StringUtil.toString(codeRoleO.IS_DISABLED);
                            codeRoleHis.MEMO_B = StringUtil.toString(codeRoleO.MEMO);
                        } else {
                            codeRoleHis.ROLE_NAME_B = "";
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
                            codeRoleFuncHisDao.insert(aplyNo, "Treasury", func, conn, transaction);

                            //if (!"".Equals(func.execAction)) {
                            //    codeRoleFuncHisDao.insert(aplyNo, "Treasury", func, conn, transaction);
                            //}
                        }
                    }


                    //處理角色金庫設備資料檔的異動
                    if (bChgEquip)
                    {
                        CodeRoleTreaItemHisDao codeRoleTreaItemHisDao = new CodeRoleTreaItemHisDao();
                        foreach (CodeRoleEquipModel equip in equipList)
                        {
                            codeRoleTreaItemHisDao.insert(aplyNo, equip, conn, transaction);
                            //if (!"".Equals(equip.execAction))
                            //{
                            //    codeRoleTreaItemHisDao.insert(aplyNo, equip, conn, transaction);
                            //}
                        }
                    }


                    //處理角色存取項目資料檔的異動(存取項目)
                    if (bChgItem)
                    {
                        CodeRoleItemHisDao codeRoleItemHisDao = new CodeRoleItemHisDao();
                        foreach (CodeRoleItemModel item in itemList)
                        {
                            codeRoleItemHisDao.insert(aplyNo, item, conn, transaction);
                            //if (!"".Equals(item.execAction))
                            //{
                            //    codeRoleItemHisDao.insert(aplyNo, item, conn, transaction);
                            //}
                        }
                    }


                    //處理角色存取項目資料檔的異動(表單申請)
                    if (bChgFormAply)
                    {
                        CodeRoleItemHisDao codeRoleItemHisDao = new CodeRoleItemHisDao();
                        foreach (CodeRoleItemModel item in formAplyList)
                        {
                            codeRoleItemHisDao.insert(aplyNo, item, conn, transaction);
                            //if (!"".Equals(item.execAction))
                            //{
                            //    codeRoleItemHisDao.insert(aplyNo, item, conn, transaction);
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
                    logger.Error("[updateRole]其它錯誤：" + e.ToString());

                    return Json(new { success = false, errors = "其它錯誤，請洽系統管理員!!" }, JsonRequestBehavior.AllowGet);
                }
            }


        }


        /**
        異動角色檔
        **/
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
        




        /**
        角色資訊(含功能授權)
        **/
        public ActionResult detailRole(string cRoleId, string execType)
        {

            /*---畫面下拉選單初始值---*/

            //金庫設備
            TreaEquipDao treaEquipDao = new TreaEquipDao();
            ViewBag.equipList = treaEquipDao.jqgridSelect();

            //存取項目
            TreaItemDao treaItemDao = new TreaItemDao();
            ViewBag.itemList = treaItemDao.jqgridSelect("");

            SysCodeDao sysCodeDao = new SysCodeDao();

            //停用註記
            var isDisabledList = sysCodeDao.loadSelectList("IS_DISABLED");
            ViewBag.isDisabledList = isDisabledList;

            //角色群組
            var roleAuthTypeList = sysCodeDao.loadSelectList("ROLE_AUTH_TYPE");
            ViewBag.roleAuthTypeList = roleAuthTypeList;

            //控管模式
            var controlList = sysCodeDao.loadSelectList("CONTROL_MODE");
            ViewBag.controlList = controlList;
            //ViewBag.controlList = sysCodeDao.jqGridList("CONTROL_MODE");


            //控管方式
            var custodyList = sysCodeDao.loadSelectList("CUSTODY_MODE");
            ViewBag.custodyList = custodyList;
            //ViewBag.custodyList = sysCodeDao.jqGridList("CUSTODY_MODE");


            //入庫作業類型
            ViewBag.itemOpTypeList = sysCodeDao.jqGridList("ITEM_OP_TYPE");



            //覆核狀態  add by daiyu 20180214
            Dictionary<string, string> dicReview = sysCodeDao.qryByTypeDic("DATA_STATUS");



            //查詢角色資訊
            CodeRoleDao codeRoleDao = new CodeRoleDao();
            CODE_ROLE codeRole = new CODE_ROLE();

            if (cRoleId != null)
                codeRole = codeRoleDao.qryRoleByKey(cRoleId);





            //將值搬給畫面欄位
            RoleMgrModel roleMgrModel = new RoleMgrModel();
            qryUserFunc(cRoleId);  //取得已授權、未授權功能清單

            if (!"".Equals(StringUtil.toString(codeRole.ROLE_ID)))
            {
                

                roleMgrModel.cRoleID = StringUtil.toString(codeRole.ROLE_ID);
                roleMgrModel.cRoleName = StringUtil.toString(codeRole.ROLE_NAME);
                roleMgrModel.roleAuthType = StringUtil.toString(codeRole.ROLE_AUTH_TYPE);
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


                roleMgrModel.Categories = ViewBag.funcList;

                ViewBag.bHaveData = "Y";
                //return RedirectToAction("Index", "Home");
                return View(roleMgrModel);
            }
            else
            {
                if ("A".Equals(execType))
                {
                    qryUserFunc("");  //取得已授權、未授權功能清單

                    roleMgrModel.cRoleID = "";
                    roleMgrModel.cRoleName = "";
                    roleMgrModel.isDisabled = "N";
                    roleMgrModel.vMemo = "";
                    roleMgrModel.cCrtUserID = "";
                    roleMgrModel.cCrtDateTime = "";
                    roleMgrModel.cUpdUserID = "";
                    roleMgrModel.cUpdDateTime = "";
                    roleMgrModel.dataStatus = "";

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
        /// 查詢角色已授權的功能
        /// </summary>
        /// <param name="cRoleId"></param>
        public void qryUserFunc(String cRoleId)
        {

            dbTreasuryEntities context = new dbTreasuryEntities();


            var result1 = (from func in context.CODE_FUNC

                           where 1 == 1
                           & func.IS_DISABLED == "N"
                           //& func.CSYSID == "sys006"

                           orderby (func.FUNC_LEVEL == 1 ? func.FUNC_ID : func.PARENT_FUNC_ID), func.PARENT_FUNC_ID, func.FUNC_ORDER

                           select new FuncRoleModel
                           {
                               cFunctionID = func.FUNC_ID,
                               cFunctionName = func.FUNC_NAME,
                               iFunctionLevel = func.FUNC_LEVEL
                           }
                        ).ToList<FuncRoleModel>();

            var authData = new
            {
                item = (
                from func in context.CODE_FUNC
                join role in context.CODE_ROLE_FUNC on func.FUNC_ID equals role.FUNC_ID
                where 1 == 1
                & func.IS_DISABLED == "N"
                //& func.CSYSID == "sys006"
                & role.ROLE_ID == cRoleId
                orderby (func.FUNC_LEVEL == 1 ? func.FUNC_ID : func.PARENT_FUNC_ID), func.PARENT_FUNC_ID, func.FUNC_ORDER

                select new
                {
                    cFunctionID = func.FUNC_ID,
                    cFunctionName = func.FUNC_NAME,
                    iFunctionLevel = func.FUNC_LEVEL,
                    menuLevel = func.FUNC_LEVEL == 1 ? func.FUNC_ID : func.PARENT_FUNC_ID
                }).ToArray()
            };


            ViewBag.funcList = result1;
            ViewBag.userAuthFuncList = Json(authData, JsonRequestBehavior.AllowGet);
        }



        /// <summary>
        /// 查詢"金庫設備權限"
        /// </summary>
        /// <param name="cRoleID"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult qryEquip(string cRoleID)
        {
            CodeRoleTreaItemDao codeRoleTreaItemDao = new CodeRoleTreaItemDao();

            try
            {
                List<CodeRoleEquipModel> roleEquipList = codeRoleTreaItemDao.qryForRoleMgr(cRoleID);

                Log log = new Log();
                log.CFUNCTION = "角色維護-金庫設備查詢";
                log.CACTION = "Q";
                log.CCONTENT = "cRoleID:" + cRoleID+ "|totCnt:" + roleEquipList.Count;
                LogDao.Insert(log, Session["UserID"].ToString());

                return Json(new { success = true, roleEquipList = roleEquipList });


            }
            catch (Exception e)
            {
                logger.Error("[qryEquip]:" + e.ToString());
                return Json(new { success = false, err = "其它錯誤，請洽系統管理員!!" });
            }
        }


        /// <summary>
        /// 金庫設備權限jqgrid切換"設備名稱"時，要取得控管模式
        /// </summary>
        /// <param name="equipId"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult qryEquipByKey(string equipId)
        {
            TreaEquipDao treaEquipDao = new TreaEquipDao();

            try
            {
                TREA_EQUIP equip = new TREA_EQUIP();
                equip = treaEquipDao.qryByKey(equipId);

                return Json(new { success = true, equip = equip });


            }
            catch (Exception e)
            {
                logger.Error("[qryEquipByKey]:" + e.ToString());
                return Json(new { success = false, err = "其它錯誤，請洽系統管理員!!" });
            }
        }


        /// <summary>
        /// 查詢存取項目
        /// </summary>
        /// <param name="cRoleID"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult qryItem(string cRoleID, string authType)
        {
            CodeRoleItemDao codeRoleItemDao = new CodeRoleItemDao();

            try
            {
                List<CodeRoleItemModel> roleItemList = codeRoleItemDao.qryForRoleMgr(cRoleID, authType);

                Log log = new Log();
                log.CFUNCTION = "角色維護-存取項目查詢";
                log.CACTION = "Q";
                log.CCONTENT = "cRoleID:" + cRoleID + "|authType:1" + "|totCnt:" + roleItemList.Count;
                LogDao.Insert(log, Session["UserID"].ToString());

                return Json(new { success = true, roleItemList = roleItemList });


            }
            catch (Exception e)
            {
                logger.Error("[qryEquip]:" + e.ToString());
                return Json(new { success = false, err = "其它錯誤，請洽系統管理員!!" });
            }
        }


        /// <summary>
        /// 查詢特定"作業類型"對應的"存取項目"
        /// </summary>
        /// <param name="opType"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult qryItemList(string opType)
        {
            try
            {
                TreaItemDao treaItemDao = new TreaItemDao();
                string itemStr  = treaItemDao.jqgridSelect(opType);
              
                return Json(new { success = true, itemList = itemStr });
            }
            catch (Exception e)
            {
                logger.Error("[qryEquip]:" + e.ToString());
                return Json(new { success = false, err = "其它錯誤，請洽系統管理員!!" });
            }
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
            Dictionary<string, string> dicExecAction = sysCodeDao.qryByTypeDic("EXEC_ACTION");
            Dictionary<string, string> dicYNFlag = sysCodeDao.qryByTypeDic("YN_FLAG");
            Dictionary<string, string> dicApprStatus = sysCodeDao.qryByTypeDic("APPR_STATUS");
            Dictionary<string, string> dicIsDisabled = sysCodeDao.qryByTypeDic("IS_DISABLED");


            List<CodeRoleModel> roleHisList = new List<CodeRoleModel>();
            List<RoleFuncHisModel> roleFuncHisList = new List<RoleFuncHisModel>();
            List<CodeRoleEquipModel> roleEquipHisList = new List<CodeRoleEquipModel>();
            List<CodeRoleItemModel> roleRoleItemHisList = new List<CodeRoleItemModel>();

            List<CodeRoleItemModel> roleItemHisList = new List<CodeRoleItemModel>();
            List<CodeRoleItemModel> roleFormAplyHisList = new List<CodeRoleItemModel>();

            CodeRoleHisDao codeRoleHisDao = new CodeRoleHisDao();
            CodeRoleFuncHisDao codeRoleFuncHisDao = new CodeRoleFuncHisDao();
            CodeRoleTreaItemHisDao codeRoleTreaItemHisDao = new CodeRoleTreaItemHisDao();
            CodeRoleItemHisDao codeRoleItemHisDao = new CodeRoleItemHisDao();

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
                        roleHisList = codeRoleHisDao.qryForRoleMgrHis(db, cRoleID, apprStatus, updDateB, updDateE);

                        roleFuncHisList = codeRoleFuncHisDao.qryForRoleMgrHis(db, cRoleID, apprStatus, updDateB, updDateE);

                        roleEquipHisList = codeRoleTreaItemHisDao.qryForRoleMgrHis(db, cRoleID, apprStatus, updDateB, updDateE);

                        roleRoleItemHisList = codeRoleItemHisDao.qryForRoleMgrHis(db, cRoleID, apprStatus, updDateB, updDateE);
                    }

                }


                using (DB_INTRAEntities dbIntra = new DB_INTRAEntities())
                {
                    Dictionary<string, string> userNameMap = new Dictionary<string, string>();
                    OaEmpDao oaEmpDao = new OaEmpDao();
                    string apprUid = "";
                    string userId = "";


                    //處理角色資訊人員&代碼
                    if (roleHisList != null)
                    {
                        
                        foreach (CodeRoleModel role in roleHisList)
                        {
                            role.execActionDesc = dicExecAction.ContainsKey(StringUtil.toString(role.execAction)) ? dicExecAction[StringUtil.toString(role.execAction)]:"";
                            role.apprStatusDesc = dicApprStatus.ContainsKey(StringUtil.toString(role.apprStatus)) ? dicApprStatus[StringUtil.toString(role.apprStatus)]:"";

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

                            apprUid = StringUtil.toString(role.apprUid);
                            if (!"".Equals(apprUid))
                            {
                                if (!userNameMap.ContainsKey(apprUid))
                                {
                                    userNameMap = oaEmpDao.qryUsrName(userNameMap, apprUid, dbIntra);
                                }
                                role.apprUid = userNameMap[apprUid];
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

                            apprUid = StringUtil.toString(d.apprUid);
                            if (!"".Equals(apprUid))
                            {
                                if (!userNameMap.ContainsKey(apprUid))
                                {
                                    userNameMap = oaEmpDao.qryUsrName(userNameMap, apprUid, dbIntra);
                                }
                                d.apprUid = userNameMap[apprUid];
                            }
                        }
                    }


                    //處理金庫設備人員&代碼
                    if (roleEquipHisList != null)
                    {

                        foreach (CodeRoleEquipModel d in roleEquipHisList)
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

                            apprUid = StringUtil.toString(d.apprUid);
                            if (!"".Equals(apprUid))
                            {
                                if (!userNameMap.ContainsKey(apprUid))
                                {
                                    userNameMap = oaEmpDao.qryUsrName(userNameMap, apprUid, dbIntra);
                                }
                                d.apprUid = userNameMap[apprUid];
                            }
                        }
                    }


                    //處理"存取項目/表單申請"人員&代碼
                    if (roleRoleItemHisList != null)
                    {

                        foreach (CodeRoleItemModel d in roleRoleItemHisList)
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

                            apprUid = StringUtil.toString(d.apprUid);
                            if (!"".Equals(apprUid))
                            {
                                if (!userNameMap.ContainsKey(apprUid))
                                {
                                    userNameMap = oaEmpDao.qryUsrName(userNameMap, apprUid, dbIntra);
                                }
                                d.apprUid = userNameMap[apprUid];
                            }
                        }


                        roleItemHisList = roleRoleItemHisList.Where(x => x.authType == "1").ToList();
                        roleFormAplyHisList = roleRoleItemHisList.Where(x => x.authType == "2").ToList();

                    }



                }
                return Json(new { success = true, roleHisList = roleHisList , roleFuncHisList  = roleFuncHisList , roleEquipHisList = roleEquipHisList
                ,
                    roleItemHisList = roleItemHisList,
                    roleFormAplyHisList= roleFormAplyHisList
                });


            }
            catch (Exception e)
            {
                logger.Error("[qryEquip]:" + e.ToString());
                return Json(new { success = false, err = "其它錯誤，請洽系統管理員!!" });
            }
        }

        [HttpPost]
        public JsonResult GetEquip(string contrlMod)
        {
            TreaEquipDao treaEquipDao = new TreaEquipDao();
            var equipList = treaEquipDao.getEquipFun(contrlMod);
            return Json(equipList);
        }


    }

}