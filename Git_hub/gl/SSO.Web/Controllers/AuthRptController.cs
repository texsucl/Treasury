
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using SSO.Web;
using SSO.Web.Daos;
using SSO.Web.Models;
using SSO.Web.BO;
using SSO.Web.ViewModels;
using SSO.Web.Utils;
using SSO.Web.ActionFilter;
using Microsoft.Reporting.WebForms;
using System.Data;
using System.IO;
using SSO.Web.Utility;

/// <summary>
/// 功能說明：權限報表作業
/// 初版作者：20180115 Daiyu
/// 修改歷程：20180115 Daiyu
///           需求單號：201801230413-00
///           初版
/// ==============================================
/// 修改日期/修改人：20200130 B0077 黃黛鈺
/// 需求單號：201911280279-00
/// 修改內容：調整報表條件及格式內容
/// </summary>
///

namespace SSO.WebControllers
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

            string opScope = "";
            string funcName = "";


            String[] roleInfo = authUtil.chkUserFuncAuth(Session["UserID"].ToString(), "~/AuthRpt/");
            if (roleInfo != null && roleInfo.Length == 2)
            {
                opScope = "1";
                funcName = roleInfo[1];
            }

            ViewBag.opScope = opScope;
            ViewBag.funcName = funcName;


            qryDropdownList();

            return View();
        }


        /// <summary>
        /// 查詢畫面各下拉選單值
        /// </summary>
        private void qryDropdownList() {
            CodeAuthMgrDao codeAuthMgrDao = new CodeAuthMgrDao();
            OaDeptDao oaDeptDao = new OaDeptDao();

            //使用者單位列印-使用者單位
            var userUnitList = new List<listModel>();
            string controlStr = "";
            foreach (var item in oaDeptDao.qryDept(Session["UserUnit"].ToString()))
            {
                controlStr = item.unitCode.Trim() + "." + item.unitName.Trim();
                listModel d = new listModel();
                d.code_value = item.unitCode;
                d.code_text = controlStr;

                userUnitList.Add(d);
            }

            var userUnitRptUserList = new SelectList
                (
                items: userUnitList,
                dataValueField: "code_value",
                dataTextField: "code_text",
                selectedValue: (object)null
                );

            ViewBag.userUnitRptUserList = userUnitRptUserList;

            //管理單位列印-使用者單位
            SelectList userUnitRptMgrList = codeAuthMgrDao.qryForAuthRptMgr(Session["UserUnit"].ToString(), "unit");
            ViewBag.userUnitRptMgrList = userUnitRptMgrList;
            ViewBag.userUnitRptMgr = "N";
            if (userUnitRptMgrList.Count() > 0)
                ViewBag.userUnitRptMgr = "Y";


            //程式OWNER部門列印-使用者單位
            SelectList userUnitRptOwnerList = codeAuthMgrDao.qryForAuthRptAuth(Session["UserUnit"].ToString());
            ViewBag.userUnitRptOwnerList = userUnitRptOwnerList;
            ViewBag.userUnitRptOwner = "N";
            if (userUnitRptOwnerList.Count() > 0)
                ViewBag.userUnitRptOwner = "Y";



            //管理單位列印-程式ONWER單位
            ViewBag.ownerUnitList = codeAuthMgrDao.loadSelectList(Session["UserUnit"].ToString(), true);

            //管理單位列印-使用者
            ViewBag.userList = codeAuthMgrDao.qryForAuthRptMgr(Session["UserUnit"].ToString(), "user");
        }

        //200319 Bianco add 變動依角色選項
        /// <summary>
        /// 變動依角色選項
        /// </summary>
        /// <param name="rptType"></param>
        /// <returns></returns>
        public ActionResult changeUserRole(string rptType, List<string> ownerUnit) 
        {
            CodeAuthMgrDao codeAuthMgrDao = new CodeAuthMgrDao();
            //200319 Bianco add 角色
            return Json(new { item = codeAuthMgrDao.qryUserRoleList(Session["UserUnit"].ToString(), rptType, ownerUnit) });

        }

        public class listModel
        {
            public string code_value { get; set; }

            public string code_text { get; set; }


        }


        /// <summary>
        /// 畫面執行"列印"功能
        /// </summary>
        /// <param name="rptType"></param>
        /// <param name="userUnit">依使用者單位</param>
        /// <param name="ownerUnit">依Owner單位</param>
        /// <param name="user">依使用者</param>
        /// <param name="userRole">依角色</param>
        /// <param name="rptFormat">報表格式</param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Print(string rptType, string[] userUnit, string[] ownerUnit, string[] user, string[] userRole, string rptFormat)
        {
            OaDeptDao oaDeptDao = new OaDeptDao();
            OaEmpDao oaEmpDao = new OaEmpDao();

            CodeUserDao codeUserDao = new CodeUserDao();
            CodeAuthMgrDao codeAuthMgrDao = new CodeAuthMgrDao();
            

            //查詢出符合條件的使用者單位，需從DB_INTRA及CODE_USER判斷
            string[] rptUserUnit = new string[] { };
            string[] rptOwnerUnit = new string[] { }; 
            string[] rptUser = new string[] { };
            if (userUnit != null)
                rptUserUnit = userUnit;
            else
            {
                switch (rptType) {
                    case "rptUserUnit":
                        rptUserUnit = oaDeptDao.qryDept(Session["UserUnit"].ToString()).Select(x => x.unitCode.TrimEnd()).ToArray();
                        break;
                    case "rptMgrUnit":
                        rptUserUnit = codeAuthMgrDao.qryForAuthRptMgr(Session["UserUnit"].ToString(), "unit").Select(x => x.Value).ToArray();
                        break;
                    case "rptOwnerUnit":
                        rptUserUnit = codeAuthMgrDao.qryForAuthRptAuth(Session["UserUnit"].ToString()).Select(x => x.Value).ToArray();
                        break;
                    default:
                        return Json(new { success = false, err = "報表種類傳入錯誤!!" });
                }
            }


            //查詢出符合條件的使用者，需從DB_INTRA及CODE_USER判斷
            List<V_EMPLY2> empList = new List<V_EMPLY2>();

            if (user != null) 
                empList = oaEmpDao.qryByUsrIdList(user).Where(x => rptUserUnit.Contains(x.DPT_CD.TrimEnd())).ToList();
            else {
                empList = oaEmpDao.qryByDptCdList(rptUserUnit);
                List<CODE_USER> othUser = codeUserDao.qryForAuthRptOthUser(rptUserUnit, empList.Select(x => x.USR_ID.TrimEnd()).ToArray());

                foreach (var d in othUser) {
                    V_EMPLY2 othEmp = new V_EMPLY2();
                    othEmp = oaEmpDao.qryByUsrId(d.USER_ID);

                    if (othEmp != null)
                    {
                        if (rptUserUnit.Contains(StringUtil.toString(othEmp.DPT_CD)))
                            empList.Add(othEmp);
                    }
                    else {
                        othEmp = new V_EMPLY2();
                        othEmp.USR_ID = d.USER_ID;
                        othEmp.DPT_CD = d.USER_UNIT;

                        VW_OA_DEPT dept = oaDeptDao.qryByDptCd(d.USER_UNIT);
                        if (dept != null)
                            othEmp.DPT_NAME = StringUtil.toString(dept.DPT_NAME);
                        else
                            othEmp.DPT_NAME = d.USER_UNIT;

                        empList.Add(othEmp);
                    }


                   
                }
            }

            if ("rptMgrUnit".Equals(rptType)) {
                if (ownerUnit != null)
                    rptOwnerUnit = ownerUnit;
                else
                    rptOwnerUnit = codeAuthMgrDao.qryAuthUnit(Session["UserUnit"].ToString());
            }

            if ("rptOwnerUnit".Equals(rptType)) {
                rptOwnerUnit = new string[] { Session["UserUnit"].ToString() };
                //rptOwnerUnit = oaDeptDao.qryDept(Session["UserUnit"].ToString()).Select(x => x.unitCode).ToArray();
            }



            //List<AuthRptModel> rptList = codeUserDao.qryForAuthRpt(rptOwnerUnit, empList.Select(x => x.USR_ID.TrimEnd()).ToArray());
            List<AuthRptModel> rptList = codeUserDao.qryForAuthRpt(rptOwnerUnit, empList.Select(x => x.USR_ID.TrimEnd()).ToArray(), userRole, rptFormat, userUnit, user);
            Dictionary<string, string> deptDic = new Dictionary<string, string>();

            foreach (AuthRptModel d in rptList) {
                if(d.user_id == "N/A")
                {
                    d.user_name = "";
                    d.user_unit = "N/A";
                    d.dpt_name = "N/A";
                }
                else
                {
                    V_EMPLY2 emp = empList.Where(x => x.USR_ID == d?.user_id, d?.user_id != "N/A").First();    // user_id == N/A 不查

                    d.user_name = StringUtil.toString(emp.EMP_NAME);
                    d.user_unit = StringUtil.toString(emp.DPT_CD);

                    if (!deptDic.ContainsKey(d.user_unit))
                    {
                        if (!"".Equals(StringUtil.toString(emp.DPT_NAME)))
                            deptDic.Add(d.user_unit, StringUtil.toString(emp.DPT_NAME));
                        else
                        {
                            var dept = oaDeptDao.qryByDptCd(d.user_unit);
                            if (dept != null)
                                deptDic.Add(d.user_unit, StringUtil.toString(dept.DPT_NAME));
                            else
                                deptDic.Add(d.user_unit, d.user_unit);
                        }
                    }

                    d.dpt_name = deptDic[d.user_unit];
                }


                if (!deptDic.ContainsKey(d.owner_unit))
                {
                    var dept = oaDeptDao.qryByDptCd(d.owner_unit);
                    if (dept != null)
                        deptDic.Add(d.owner_unit, StringUtil.toString(dept.DPT_NAME));
                    else
                        deptDic.Add(d.owner_unit, d.owner_unit);
                }

                d.owner_unit_name = deptDic[d.owner_unit];

                d.sys_owner_role = StringUtil.toString(d.grp_id) + "| " + StringUtil.toString(d.owner_unit) + "|" + StringUtil.toString(d.role_id);
            }

            CommonUtil commonUtil = new CommonUtil();
            DataTable dtMain = commonUtil.ConvertToDataTable<AuthRptModel>(rptList);

            string guid = Guid.NewGuid().ToString();

            var ReportViewer1 = new ReportViewer();
            //清除資料來源
            ReportViewer1.LocalReport.DataSources.Clear();
            //指定報表檔路徑   
            //ReportViewer1.LocalReport.ReportPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory + "Report\\Rdlc\\AuthRpt.rdlc"); 200319 Bianco 註解
            //200319 Bianco 判斷報表格式
            switch (rptFormat)
            {
                case "userRoleRpt": //角色功能報表
                    ReportViewer1.LocalReport.ReportPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory + "Report\\Rdlc\\AuthUserRoleRpt.rdlc");
                    break;
                case "userRpt": //使用者角色報表
                    ReportViewer1.LocalReport.ReportPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory + "Report\\Rdlc\\AuthUserRpt.rdlc");
                    break;
                case "allRpt": //程式權限授權報表
                    ReportViewer1.LocalReport.ReportPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory + "Report\\Rdlc\\AuthRpt.rdlc");
                    break;
            }
            //設定資料來源
            ReportViewer1.LocalReport.DataSources.Add(new ReportDataSource("DataSet1", dtMain));
            //報表參數
            ReportViewer1.LocalReport.SetParameters(new ReportParameter("UserId", Session["UserID"].ToString() + Session["UserName"].ToString()));

            string rptTypeDesc = "";
            switch (rptType)
            {
                case "rptUserUnit":
                    rptTypeDesc = "使用者單位列印";
                    break;
                case "rptMgrUnit":
                    rptTypeDesc = "管理者單位列印";
                    break;
                case "rptOwnerUnit":
                    rptTypeDesc = "程式OWNER部門列印";
                    break;
            }

            ReportViewer1.LocalReport.SetParameters(new ReportParameter("rptType", rptTypeDesc));
            
            ReportViewer1.LocalReport.Refresh();

            Microsoft.Reporting.WebForms.Warning[] tWarnings;
            string[] tStreamids;
            string tMimeType;
            string tEncoding;
            string tExtension;
            byte[] tBytes = ReportViewer1.LocalReport.Render("pdf", null, out tMimeType, out tEncoding, out tExtension, out tStreamids, out tWarnings);
            string fileName = "AuthRptP_" + guid + ".pdf";

            bool exists = System.IO.Directory.Exists(Server.MapPath("~/Temp/"));

            if (!exists)
                System.IO.Directory.CreateDirectory(Server.MapPath("~/Temp/"));

            using (FileStream fs = new FileStream(Path.Combine(AppDomain.CurrentDomain.BaseDirectory + "Temp\\") + fileName, FileMode.Create))
            {
                fs.Write(tBytes, 0, tBytes.Length);
            }


            string url = Server.MapPath("~/Temp/") + guid + ".pdf";

            return Json(new { success = true, guid = guid });


            //switch (rptType) {
            //    case "rptUserUnit":
            //        return printUserRole();
            //    case "rptMgrUnit":
            //        return printRoleFunc();
            //    case "rptOwnerUnit":
            //        return printRoleFunc();
            //    default :
            //        return null;
            //}
            //if ("userRole".Equals(cRptType))
            //    return printUserRole();

            //else
            //    return printRoleFunc();


        }
        


        public class Employee
        {
            public string UnitId { get; set; }
            public string UserId { get; set; }
            public string UserName { get; set; }
            public string WorkUnit { get; set; }
            public string WorkUnitNm { get; set; }
            public string RoleName { get; set; }
        }

        




        public FileContentResult downloadRpt(String id)
        {
            byte[] fileBytes = System.IO.File.ReadAllBytes(Server.MapPath("~/Temp/") + "AuthRptP_" + id + ".pdf");


            string fullPath = Server.MapPath("~/Temp/") + id + ".pdf";
            if (System.IO.File.Exists(fullPath))
            {
                System.IO.File.Delete(fullPath);
            }


            return File(fileBytes, "application/pdf", "組織權限報表.pdf");
        }
    }
}