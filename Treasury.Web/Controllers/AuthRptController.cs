using Treasury.WebBO;
using Treasury.WebDaos;
using Treasury.WebModels;
using Treasury.WebUtils;
using Treasury.WebViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Treasury.Web;
using Treasury.Web.Daos;
using Treasury.Web.Models;
using Treasury.WebActionFilter;
using Treasury.Web.ViewModels;

/// <summary>
/// 功能說明：權限報表作業
/// 初版作者：20180115 Daiyu
/// 修改歷程：20180115 Daiyu
///           需求單號：201801230413-00
///           初版
/// </summary>
///

namespace Treasury.WebControllers
{
    [Authorize]
    [CheckSessionFilterAttribute]
    public class AuthRptController : BaseController
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
            String[] roleInfo = authUtil.chkUserFuncAuth(Session["UserID"].ToString(), "~/AuthRpt/");
            if (roleInfo != null && roleInfo.Length == 1)
            {
                opScope = roleInfo[0];

            }

            ViewBag.opScope = opScope;


            return View();
        }


        /// <summary>
        /// 畫面執行"查詢"
        /// </summary>
        /// <param name="cRptType"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult loadData(String cRptType)
        {
            CodeUserDao codeUserDao = new CodeUserDao();
            List<UserMgrModel> rows = codeUserDao.qryUserRole();


            var jsonData = new { success = true, rows };
            return Json(jsonData, JsonRequestBehavior.AllowGet);

        }


        /// <summary>
        /// 畫面執行"列印"功能
        /// </summary>
        /// <param name="cRptType"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Print(String cRptType)
        {

            if ("userRole".Equals(cRptType))
                return printUserRole();
            else
                return printRoleAuth();


        }



        /// <summary>
        /// 列印角色授權報表
        /// </summary>
        /// <returns></returns>
        private ActionResult printRoleAuth()
        {

            CodeRoleDao codeRoleDao = new CodeRoleDao();
            List<CODE_ROLE> rptData = codeRoleDao.qryValidRole("");


            //CodeRoleFunctionDao codeRoleFunctionDao = new CodeRoleFunctionDao();
            //List<FuncRoleModel> rptData = codeRoleFunctionDao.qryFuncRole();

            if (rptData.Count == 0)
            {
                return Json(new { success = false, err = "查無資料!!" });
            }
            else
            {
                String guid = Guid.NewGuid().ToString();

                try
                {
                    string htmlText = "";

                    int totCnt = 0;

                    htmlText += "<html><span style='font-size:12px;'>";
                    

                    //處理功能授權
                    List<CODE_ROLE> rptDataF = rptData.Where(x => x.ROLE_AUTH_TYPE == "F").ToList();
                    if (rptDataF != null) {
                        htmlText += "<table border='0' width='100%' style='font-size:16px;'>";
                        htmlText += genRptHeader("funcRole", "", "", "F");

                        htmlText += genRptRoleFunc(rptDataF);
                        totCnt = totCnt + rptDataF.Count;
                    }

                    //處理金庫設備授權
                    List<CODE_ROLE> rptDataE = rptData.Where(x => x.ROLE_AUTH_TYPE == "E").ToList();
                    if (rptDataE != null)
                    {
                        if (totCnt == 0)
                            htmlText += htmlText + "<table border='0' width='100%' style='font-size:16px;'>";

                        else
                            htmlText += "<table border='0' width='100%' style='font-size:16px;page-break-before:always;'>";

                        htmlText += genRptHeader("funcRole", "", "", "E");

                        htmlText += genRptRoleEquip(rptDataE);
                        totCnt = totCnt + rptDataE.Count;
                    }

                    //處理存取項目
                    List<CODE_ROLE> rptDataI = rptData.Where(x => x.ROLE_AUTH_TYPE == "I").ToList();
                    if (rptDataI != null)
                    {
                        if (totCnt == 0)
                            htmlText += htmlText + "<table border='0' width='100%' style='font-size:16px;'>";

                        else
                            htmlText += "<table border='0' width='100%' style='font-size:16px;page-break-before:always;'>";

                        htmlText += genRptHeader("funcRole", "", "", "I");

                        htmlText += genRptRoleItem(rptDataI, "1");
                        totCnt = totCnt + rptDataI.Count;
                    }

                    //處理表單申請
                    List<CODE_ROLE> rptDataA = rptData.Where(x => x.ROLE_AUTH_TYPE == "A").ToList();
                    if (rptDataA != null)
                    {
                        if (totCnt == 0)
                            htmlText += htmlText + "<table border='0' width='100%' style='font-size:16px;'>";

                        else
                            htmlText += "<table border='0' width='100%' style='font-size:16px;page-break-before:always;'>";

                        htmlText += genRptHeader("funcRole", "", "", "A");

                        htmlText += genRptRoleItem(rptDataI, "2");
                        totCnt = totCnt + rptDataA.Count;
                    }


                    htmlText += genRptFooter();
                    htmlText += "</span></html>";


                    ReportUtil reportUtil = new ReportUtil();
                    byte[] pdfFile = reportUtil.ConvertHtmlTextToPDF(htmlText, guid, Server.MapPath("~/Temp/"));

                    string url = Server.MapPath("~/Temp/") + guid + ".pdf";

                    return Json(new { success = true, guid = guid });

                }
                catch (Exception e)
                {
                    logger.Error("[printFuncRole]其它錯誤：" + e.ToString());
                    return Json(new { success = false, err = "其它錯誤，請洽系統管理員!!" });
                }

            }
        }

        /// <summary>
        /// 列印"使用者角色"組織權限報表
        /// </summary>
        /// <returns></returns>
        private ActionResult printUserRole()
        {
            CodeUserDao codeUserDao = new CodeUserDao();
            List<UserMgrModel> rptData = codeUserDao.qryUserRole();

            if (rptData.Count == 0)
            {
                return Json(new { success = false, err = "查無資料!!" });
            }
            else
            {
                using (DB_INTRAEntities db = new DB_INTRAEntities())
                {
                    OaEmpDao oaEmpDao = new OaEmpDao();
                    for (int i = 0; i < rptData.Count; i++) {
                        rptData[i] = oaEmpDao.getUserOaData(rptData[i], db);
                    }
                }


                    String guid = Guid.NewGuid().ToString();

                try
                {
                    String htmlText = "";

                    htmlText = genRptUserRole(rptData);


                    ReportUtil reportUtil = new ReportUtil();
                    byte[] pdfFile = reportUtil.ConvertHtmlTextToPDF(htmlText, guid, Server.MapPath("~/Temp/"));

                    string url = Server.MapPath("~/Temp/") + guid + ".pdf";

                    return Json(new { success = true, guid = guid });

                }
                catch (Exception e)
                {
                    logger.Error("[printUserRole]其它錯誤：" + e.ToString());
                    return Json(new { success = false, err = "其它錯誤，請洽系統管理員!!" });
                }

            }
        }


        /// <summary>
        /// 產生角色功能報表
        /// </summary>
        /// <param name="rptData"></param>
        /// <returns></returns>
        private String genRptRoleFunc(List<CODE_ROLE> rptDataRole)
        {
            CodeRoleFunctionDao codeRoleFunctionDao = new CodeRoleFunctionDao();
            List<FuncRoleModel> rptData = codeRoleFunctionDao.qryFuncRole();

            int totCnt = 0;

            string strTable = "";


            //查出要列印的角色LIST
            List<FuncRoleModel> roleList = rptData.GroupBy(o => new { o.cRoleId, o.cRoleName, o.vMemo })
                .Select(group => new FuncRoleModel
                {
                    cRoleId = group.Key.cRoleId,
                    cRoleName = group.Key.cRoleName,
                    vMemo = group.Key.vMemo
                }).ToList<FuncRoleModel>();


            ////表頭
            //strTable = strTable + "<table border='0' width='100%' style='font-size:16px;'>";

            //strTable += genRptHeader("funcRole", "", "");


            foreach (FuncRoleModel funcRoleModel in roleList)
            {
                totCnt++;

                strTable += "<br/>";

                strTable = strTable + "<table align='center' cellpadding='2' cellspacing='0' width='100%'  style='font-size:12px;border-top:#000000 1px solid;border-left:#000000 1px solid'>";
                strTable = strTable + "<tr>";
                strTable = strTable + "<td colspan='4' style='border-right:#000000 1px solid;' align='left' width='10%'>角色名稱：" + StringUtil.toString(funcRoleModel.cRoleName) + "</td>";
                strTable = strTable + "</tr>";


                strTable = strTable + "<tr>";
                strTable = strTable + "<td colspan='4' style='border-right:#000000 1px solid;' align='left' width='10%'>備註：" + StringUtil.toString(funcRoleModel.vMemo) + "</td>";
                strTable = strTable + "</tr>";

                strTable = strTable + "<tr>";
                strTable = strTable + "<td colspan='4' style='border-top:#000000 1px solid;border-right:#000000 1px solid;' align='left' width='10%'>作業功能：</td>";
                strTable = strTable + "</tr>";

                foreach (FuncRoleModel pFunc in rptData.Where(x => x.iParentFunctionLevel.CompareTo(1) == 0
                                                            && x.cRoleId.Equals(funcRoleModel.cRoleId.Trim()))
                                                            .GroupBy(x => new { x.cParentFunctionID, x.cParentFunctionName })
                                                            .Select(group => new FuncRoleModel
                                                            {
                                                                cParentFunctionID = group.Key.cParentFunctionID
                                                                ,
                                                                cParentFunctionName = group.Key.cParentFunctionName
                                                            }).ToList<FuncRoleModel>()

                                                            )
                {
                    int dCnt = 0;
                    strTable = strTable + "<tr>";
                    strTable = strTable + "<td colspan='4' style='border-right:#000000 1px solid;' align='left' width='10%'>　　　" + StringUtil.toString(pFunc.cParentFunctionName) + "＞＞</td>";
                    strTable = strTable + "</tr>";

                    foreach (FuncRoleModel cFunc in rptData.Where(
                        x => x.cParentFunctionID.Equals(pFunc.cParentFunctionID)
                        && x.cRoleId.Equals(StringUtil.toString(funcRoleModel.cRoleId))
                        ))
                    {
                        if ((dCnt % 3).CompareTo(0) == 0)
                        {
                            strTable = strTable + "<tr>";
                            strTable = strTable + "<td ></td><td >" + StringUtil.toString(cFunc.cFunctionName) + "</td>";
                        }
                        else if ((dCnt % 3).CompareTo(1) == 0)
                        {
                            strTable = strTable + "<td >" + StringUtil.toString(cFunc.cFunctionName) + "</td>";
                        }
                        else
                        {
                            strTable = strTable + "<td style='border-right:#000000 1px solid;'>" + StringUtil.toString(cFunc.cFunctionName) + "</td></tr>";
                        }


                        dCnt++;
                    }

                    if ((dCnt % 3).CompareTo(0) > 0)
                    {
                        strTable = strTable + "<td colspan='" + (3 - (dCnt % 3)) + "' style='border-right:#000000 1px solid;'></td>";
                        strTable = strTable + "</tr>";
                    }

                }

                strTable = strTable + "<tr>";
                strTable = strTable + "<td colspan='4' style='border-top:#000000 1px solid;border-right:#000000 1px solid;' align='left' width='10%'></td>";
                strTable = strTable + "</tr>";
                strTable = strTable + "</table>";

            }
            ////表尾
            //strTable += genRptFooter();

            ////strTable = strTable + "</table>";

            //strTable = strTable + "</span></html>";


            return strTable;

        }


        /// <summary>
        /// 產生角色金庫設備報表
        /// </summary>
        /// <param name="rptData"></param>
        /// <returns></returns>
        private String genRptRoleEquip(List<CODE_ROLE> rptDataEquip)
        {
            CodeRoleTreaItemDao codeRoleTreaItemDao = new CodeRoleTreaItemDao();
            List<CodeRoleEquipModel> rptData = codeRoleTreaItemDao.qryRoleEquip();

            string strTable = "";


            //查出要列印的角色LIST
            List<CodeRoleEquipModel> roleList = rptData.GroupBy(o => new { o.roleId, o.roleName })
                .Select(group => new CodeRoleEquipModel
                {
                    roleId = group.Key.roleId,
                    roleName = group.Key.roleName
                }).ToList<CodeRoleEquipModel>();


            ////表頭
            //strTable = strTable + "<table border='0' width='100%' style='font-size:16px;'>";

            //strTable += genRptHeader("funcRole", "", "");


            foreach (CodeRoleEquipModel codeRoleEquipModel in roleList)
            {

                strTable += "<br/>";

                strTable = strTable + "<table align='center' cellpadding='2' cellspacing='0' width='100%'  style='font-size:12px;border-top:#000000 1px solid;border-left:#000000 1px solid'>";
                strTable = strTable + "<tr>";
                strTable = strTable + "<td colspan='4' style='border-bottom:#000000 1px solid;border-right:#000000 1px solid;' align='left' width='10%'>角色名稱：" + StringUtil.toString(codeRoleEquipModel.roleName) + "</td>";
                strTable = strTable + "</tr>";

                strTable = strTable + "<tr>";
                strTable = strTable + "<td style='border-bottom:#000000 1px solid;border-right:#000000 1px solid;' align='center' width='55%'>設備名稱</td>";
                strTable = strTable + "<td style='border-bottom:#000000 1px solid;border-right:#000000 1px solid;' align='center'  width='15%'>控管模式</td>";
                strTable = strTable + "<td style='border-bottom:#000000 1px solid;border-right:#000000 1px solid;' align='center'  width='15%'>控管方式</td>";
                strTable = strTable + "<td style='border-bottom:#000000 1px solid;border-right:#000000 1px solid;' align='center'  width='15%' >保管順位</td>";
                strTable = strTable + "</tr>";

                foreach (CodeRoleEquipModel d in rptData.Where(
                                        x => x.roleId.Equals(codeRoleEquipModel.roleId)
                                        ))
                {

                    strTable = strTable + "<tr>";
                    strTable = strTable + "<td style='border-bottom:#000000 1px solid;border-right:#000000 1px solid;' align='center'>" + StringUtil.toString(d.equipName) + "</td>";
                    strTable = strTable + "<td style='border-bottom:#000000 1px solid;border-right:#000000 1px solid;' align='center'>" + StringUtil.toString(d.controlModeDesc) + "</td>";
                    strTable = strTable + "<td style='border-bottom:#000000 1px solid;border-right:#000000 1px solid;' align='center'>" + StringUtil.toString(d.custodyModeDesc) + "</td>";
                    strTable = strTable + "<td style='border-bottom:#000000 1px solid;border-right:#000000 1px solid;' align='center'>" + StringUtil.toString(d.custodyOrder) + "</td>";

                    strTable = strTable + "</tr>";



                }

                strTable = strTable + "</table>";

            }
            ////表尾
            //strTable += genRptFooter();

            ////strTable = strTable + "</table>";

            //strTable = strTable + "</span></html>";


            return strTable;

        }


        /// <summary>
        /// 產生角色存取項目報表
        /// </summary>
        /// <param name="rptData"></param>
        /// <returns></returns>
        private String genRptRoleItem(List<CODE_ROLE> rptDataEquip, string roleAuthType)
        {
            CodeRoleItemDao codeRoleItemDao = new CodeRoleItemDao();
            List<CodeRoleItemModel> rptData = codeRoleItemDao.qryRoleForAuthRpt(roleAuthType);

            string strTable = "";


            //查出要列印的角色LIST
            List<CodeRoleItemModel> roleList = rptData.GroupBy(o => new { o.roleId, o.roleName })
                .Select(group => new CodeRoleItemModel
                {
                    roleId = group.Key.roleId,
                    roleName = group.Key.roleName
                }).ToList<CodeRoleItemModel>();




            foreach (CodeRoleItemModel codeRoleItemModel in roleList)
            {

                strTable += "<br/>";

                strTable = strTable + "<table align='center' cellpadding='2' cellspacing='0' width='100%'  style='font-size:12px;border-top:#000000 1px solid;border-left:#000000 1px solid'>";
                strTable = strTable + "<tr>";
                strTable = strTable + "<td colspan='2' style='border-bottom:#000000 1px solid;border-right:#000000 1px solid;' align='left' width='10%'>角色名稱：" + StringUtil.toString(codeRoleItemModel.roleName) + "</td>";
                strTable = strTable + "</tr>";

                strTable = strTable + "<tr>";
                strTable = strTable + "<td style='border-bottom:#000000 1px solid;border-right:#000000 1px solid;' align='center' width='60%'>作業類型</td>";
                strTable = strTable + "<td style='border-bottom:#000000 1px solid;border-right:#000000 1px solid;' align='center'  width='40%'>存取項目</td>";

                strTable = strTable + "</tr>";

                foreach (CodeRoleItemModel d in rptData.Where(
                                        x => x.roleId.Equals(codeRoleItemModel.roleId)
                                        ))
                {

                    strTable = strTable + "<tr>";
                    strTable = strTable + "<td style='border-bottom:#000000 1px solid;border-right:#000000 1px solid;' align='left'>" + StringUtil.toString(d.itemOpTypeDesc) + "</td>";
                    strTable = strTable + "<td style='border-bottom:#000000 1px solid;border-right:#000000 1px solid;' align='center'>" + StringUtil.toString(d.itemDesc) + "</td>";
                   
                    strTable = strTable + "</tr>";



                }

                strTable = strTable + "</table>";

            }
            ////表尾
            //strTable += genRptFooter();

            ////strTable = strTable + "</table>";

            //strTable = strTable + "</span></html>";


            return strTable;

        }


        /// <summary>
        /// 使用者角色權限報表
        /// </summary>
        /// <param name="rptData"></param>
        /// <returns></returns>
        private String genRptUserRole(List<UserMgrModel> rptData)
        {

            int totCnt = 0;

            string strTable = "<html><span style='font-size:12px;'>";


            //查出要列印的單位LIST
            List<UserMgrModel> depList = rptData.GroupBy(o => new { o.cWorkUnitCode })
                .Select(group => new UserMgrModel
                {
                    cWorkUnitCode = group.Key.cWorkUnitCode
                }).ToList<UserMgrModel>();





            //查詢db_intra查單位資料
            OaDeptDao oaDeptDao = new OaDeptDao();
            Dictionary<string, string> pDepDic = new Dictionary<string, string>();


            foreach (UserMgrModel userMgrModel in depList)
            {
                if (!"".Equals(StringUtil.toString(userMgrModel.cWorkUnitCode)))
                {
                    VW_OA_DEPT oaDept = oaDeptDao.qryByDptCd(userMgrModel.cWorkUnitCode.ToUpper());

                    if (oaDept != null)
                    {
                        userMgrModel.cWorkUnitDesc = StringUtil.toString(oaDept.DPT_NAME);
                        userMgrModel.upDptCd = StringUtil.toString(oaDept.UP_DPT_CD);
                        userMgrModel.dptType = StringUtil.toString(oaDept.Dpt_type);


                        if ("04".Equals(userMgrModel.dptType))    //科
                        {
                            if (!pDepDic.ContainsKey(userMgrModel.upDptCd))
                            {
                                VW_OA_DEPT pOaDept = oaDeptDao.qryByDptCd(userMgrModel.upDptCd);
                                if (oaDept != null)
                                {
                                    if (!pDepDic.ContainsKey(StringUtil.toString(pOaDept.DPT_CD)))
                                        pDepDic.Add(StringUtil.toString(pOaDept.DPT_CD), StringUtil.toString(pOaDept.DPT_NAME));
                                }
                                else {
                                    if (!pDepDic.ContainsKey(StringUtil.toString(oaDept.UP_DPT_CD)))
                                        pDepDic.Add(StringUtil.toString(oaDept.UP_DPT_CD), "");
                                }
                                    
                            }
                        }
                        else
                        {  //部以上
                            userMgrModel.upDptCd = StringUtil.toString(oaDept.DPT_CD);

                            if(!pDepDic.ContainsKey(StringUtil.toString(oaDept.DPT_CD)))
                                pDepDic.Add(StringUtil.toString(oaDept.DPT_CD), StringUtil.toString(oaDept.DPT_NAME));
                        }
                    }
                }
            }

            List<UserMgrModel> pDepList = depList.GroupBy(o => new { o.upDptCd })
                .Select(group => new UserMgrModel
                {
                    upDptCd = group.Key.upDptCd
                }).ToList<UserMgrModel>();


            foreach (UserMgrModel pUserMgrModel in pDepList)
            {
                totCnt++;

                //表頭
                //表頭
                if (totCnt == 1)
                    strTable = strTable + "<table border='0' width='100%' style='font-size:16px;'>";
                else
                    strTable = strTable + "<table border='0' width='100%' style='font-size:16px;page-break-before:always;'>";


                string dptCd = StringUtil.toString(pUserMgrModel.upDptCd);
                string dptName = "";
                try
                {
                    dptName = pDepDic[pUserMgrModel.upDptCd];
                }
                catch (Exception e)
                {
                }
                strTable += genRptHeader("userRole", dptCd, dptName, "");



                foreach (UserMgrModel authRptModel in depList.Where(x => x.upDptCd == pUserMgrModel.upDptCd))
                {

                    //處理符合該"單位"的使用者資料
                    List<UserMgrModel> userList = rptData.Where(x => x.cWorkUnitCode == authRptModel.cWorkUnitCode).GroupBy(o => new { o.cUserID })
                    .Select(group => new UserMgrModel
                    {
                        cUserID = group.Key.cUserID
                    }).ToList<UserMgrModel>();

                    //strTable += "<br/>";
                    //strTable = strTable + "<table border='0' width='100%' style='font-size:12px;'>";
                    //strTable = strTable + "<tr>";
                    //strTable = strTable + "<td align='left'>" + "單位：" + StringUtil.toString(authRptModel.cWorkUnitCode)
                    //    + StringUtil.toString(authRptModel.cWorkUnitDesc) + "</td>";
                    //strTable = strTable + "</tr>";
                    //strTable = strTable + "</table>";

                    //strTable = strTable + "<table align='center' cellpadding='6' cellspacing='0' width='100%'  style='font-size:12px;border-top:#000000 1px solid;border-left:#000000 1px solid'>";
                    //strTable = strTable + "<tr>";

                    //strTable = strTable + "<td style='border-bottom:#000000 1px solid;border-right:#000000 1px solid;' align='center' width='10%'>網路帳號</td>";
                    //strTable = strTable + "<td style='border-bottom:#000000 1px solid;border-right:#000000 1px solid;' align='center'  width='10%'>中文姓名</td>";
                    ////strTable = strTable + "<td style='border-bottom:#000000 1px solid;border-right:#000000 1px solid;' align='center'  width='20%'>單位</td>";
                    //strTable = strTable + "<td style='border-bottom:#000000 1px solid;border-right:#000000 1px solid;' align='center'  width='15%' >角色</td>";
                    //strTable = strTable + "<td style='border-bottom:#000000 1px solid;border-right:#000000 1px solid;' align='center'>備註</td>";

                    //strTable = strTable + "</tr>";

                    foreach (UserMgrModel authUser in userList)
                    {
                        List<UserMgrModel> roleList = rptData.Where(x => x.cUserID == authUser.cUserID).ToList<UserMgrModel>();
                        string strRoleF = "";
                        string strRoleE = "";
                        string strRoleI = "";
                        string strRoleA = "";

                        foreach (UserMgrModel role in roleList)
                        {
                            switch (role.roleAuthType) {
                                case "F":
                                    strRoleF += role.roleName + "<br/>";
                                    break;
                                case "E":
                                    strRoleE += role.roleName + "<br/>";
                                    break;
                                case "I":
                                    strRoleI += role.roleName + "<br/>";
                                    break;
                                case "A":
                                    strRoleA += role.roleName + "<br/>";
                                    break;
                            }
                            
                        }


                        strTable += "<br/>";
                        strTable = strTable + "<table border='0' width='100%' style='font-size:12px;'>";
                        strTable = strTable + "<tr>";
                        strTable = strTable + "<td align='left'>" + "單位：" + StringUtil.toString(authRptModel.cWorkUnitCode)
                            + StringUtil.toString(authRptModel.cWorkUnitDesc) + "</td>";

                        strTable = strTable + "<td align='left'>" + "網路帳號：" + StringUtil.toString(authUser.cUserID) + "</td>";

                        strTable = strTable + "<td align='left'>" + "中文姓名：" + StringUtil.toString(roleList[0].cUserName) + "</td>";


                        strTable = strTable + "</tr>";
                        strTable = strTable + "</table>";

                        strTable = strTable + "<table align='center' cellpadding='5' cellspacing='0' width='100%'  style='font-size:12px;border-top:#000000 1px solid;border-left:#000000 1px solid'>";
                        strTable = strTable + "<tr>";

                        strTable = strTable + "<td style='border-bottom:#000000 1px solid;border-right:#000000 1px solid;' align='center'>備註</td>";
                        strTable = strTable + "<td colspan='4' style='border-bottom:#000000 1px solid;border-right:#000000 1px solid;' align='center'></td>";
                        strTable = strTable + "</tr>";

                        strTable = strTable + "<tr>";
                        strTable = strTable + "<td style='border-bottom:#000000 1px solid;border-right:#000000 1px solid;' align='center' width='10%'>角色群組</td>";
                        strTable = strTable + "<td style='border-bottom:#000000 1px solid;border-right:#000000 1px solid;' align='center'  width='20%'>授權功能</td>";
                        strTable = strTable + "<td style='border-bottom:#000000 1px solid;border-right:#000000 1px solid;' align='center'  width='20%'>金庫設備權限</td>";
                        strTable = strTable + "<td style='border-bottom:#000000 1px solid;border-right:#000000 1px solid;' align='center'  width='20%' >存取項目權限</td>";
                        strTable = strTable + "<td style='border-bottom:#000000 1px solid;border-right:#000000 1px solid;' align='center'>表單申請權限</td>";
                        strTable = strTable + "</tr>";



                        strTable = strTable + "<tr>";
                        strTable = strTable + "<td style='border-bottom:#000000 1px solid;border-right:#000000 1px solid;' align='center'>角色名稱</td>";
                        strTable = strTable + "<td style='border-bottom:#000000 1px solid;border-right:#000000 1px solid;' align='center'>" + strRoleF + "</td>";
                        strTable = strTable + "<td style='border-bottom:#000000 1px solid;border-right:#000000 1px solid;' align='center'>" + strRoleE + "</td>";
                        strTable = strTable + "<td style='border-bottom:#000000 1px solid;border-right:#000000 1px solid;' align='center'>" + strRoleI + "</td>";
                        strTable = strTable + "<td style='border-bottom:#000000 1px solid;border-right:#000000 1px solid;' align='center'>" + strRoleA + "</td>";
                        
                        strTable = strTable + "</tr>";




                        strTable = strTable + "</table>";
                    }
                    


                }

                //表尾
                strTable += genRptFooter();


            }


            strTable = strTable + "</span></html>";


            return strTable;

        }


        private String genRptHeader(string rptType, string pDetCd, string pDetName, string roleAuthType)
        {
            String strTable = "";

            strTable = strTable + "<tr rowspan= '2' align='center' >";
            if ("userRole".Equals(rptType))
            {

                strTable = strTable + "<td colspan='2' align='center'>" + "金庫管理系統-使用者角色報表" + "</td>";
                strTable = strTable + "</tr>";
                strTable = strTable + "<tr>";
                strTable = strTable + "<td  colspan='2' align='center'>" + "<br/> 單位：" + pDetCd + pDetName + "</td>";

                strTable = strTable + "</tr>";

            }

            else
            {
                strTable = strTable + "<td colspan='2' align='center'>" + "金庫管理系統-角色功能報表" + "</td>";
                strTable = strTable + "</tr>";
            }


            strTable = strTable + "<tr>";
            strTable = strTable + "<td>" + "<br/> 製表人：" + Session["UserName"].ToString() + "</td>";
            strTable = strTable + "<td  align='right'>" + "<br/> 製表時間：" + DateUtil.formatDateTimeDbToSc(DateTime.Now.ToString("yyyyMMdd HHmmss"), "DT") + "</td>";
            strTable = strTable + "</tr>";

            if (!"userRole".Equals(rptType))
            {
                strTable = strTable + "<tr>";
                switch (roleAuthType) {
                    case "F":
                        strTable = strTable + "<td>" + "<br/>角色群組：功能權限</td>";
                        break;
                    case "E":
                        strTable = strTable + "<td>" + "<br/>角色群組：金庫設備權限</td>";
                        break;
                    case "I":
                        strTable = strTable + "<td>" + "<br/>角色群組：存取項目</td>";
                        break;
                    case "A":
                        strTable = strTable + "<td>" + "<br/>角色群組：表單申請</td>";
                        break;

                }
                
                strTable = strTable + "</tr>";
            }
                

            strTable = strTable + "</table>";



            return strTable;

        }


        /// <summary>
        /// 表尾
        /// </summary>
        /// <returns></returns>
        private String genRptFooter()
        {
            string strTable = "";
            strTable += "<br/>";
            strTable += "<table border='0' width='100%' style='font-size:12px;'>";
            strTable += "<tr>";
            strTable = strTable + "<td >" + "<br/> 管理者主管簽名：</td>";
            strTable = strTable + "<td  width='20%'></td>";
            strTable = strTable + "<td >" + "<br/> 管理者簽名：</td>";
            strTable = strTable + "<td width='20%'></td>";
            strTable += "</tr>";
            strTable += "</table>";


            return strTable;

        }


        public FileContentResult downloadRpt(String id)
        {
            byte[] fileBytes = System.IO.File.ReadAllBytes(Server.MapPath("~/Temp/") + id + ".pdf");


            string fullPath = Server.MapPath("~/Temp/") + id + ".pdf";
            if (System.IO.File.Exists(fullPath))
            {
                System.IO.File.Delete(fullPath);
            }


            return File(fileBytes, "application/pdf", "組織權限報表.pdf");
        }
    }
}