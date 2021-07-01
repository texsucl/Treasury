using FAP.Web;
using FAP.Web.ActionFilter;
using Microsoft.Reporting.WebForms;
using FAP.Web.Service.Actual;
using FAP.Web.Service.Interface;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using FAP.Web.Utilitys;
using System.IO;
using FAP.Web.ViewModels;
using System.ComponentModel;
using System.Configuration;


/// <summary>
/// 功能說明：共用 controller
/// 初版作者：20190626 李彥賢
/// 修改歷程：20190626 李彥賢 
///           需求單號：
///           初版
/// ==============================================
/// 修改日期/修改人：
/// 需求單號：
/// 修改內容：
/// ==============================================
/// </summary>
/// 

namespace FAP.Web.Controllers
{
    [Authorize]
    [CheckSessionFilterAttribute]
    public class CommonController : BaseController
    {
        internal ICacheProvider Cache { get; set; }

        public CommonController()
        {
            Cache = new DefaultCacheProvider();
        }

        public string GetLRTCODE(string GROUP_ID, string REF_NO)
        {
            return new FAP.Web.Service.Actual.Common().GetMessage(GROUP_ID, REF_NO);
        }

        //protected string[] GetopScope(string funcId)
        //{
        //    UserAuthUtil authUtil = new UserAuthUtil();
        //    string opScope = "";
        //    string funcName = "";
        //    string[] result = new string[2];

        //    string[] roleInfo = authUtil.chkUserFuncAuth(Session["UserID"].ToString(), funcId);
        //    if (roleInfo != null && roleInfo.Length == 2)
        //    {
        //        opScope = roleInfo[0];
        //        funcName = roleInfo[1];
        //        result[0] = opScope;
        //        result[1] = funcName;
        //    }

        //    return result;
        //}

        //////////////////////////////////////



        ///// <summary>
        ///// 報表
        ///// </summary>
        ///// <param name="data"></param>
        ///// <param name="parms"></param>
        ///// <param name="extensionParms"></param>
        ///// <returns></returns>
        //public JsonResult GetReport(reportModel data, List<reportParm> parms, List<reportParm> extensionParms = null)
        //{
        //    MSGReturnModel<string> result = new MSGReturnModel<string>();
        //    result.RETURN_FLAG = false;
        //    try
        //    {
        //        string title = "報表名稱";
        //        if (data.className.IsNullOrWhiteSpace())
        //        {
        //            result.DESCRIPTION = "報表錯誤請聯絡IT人員";
        //            //result.DESCRIPTION = MessageType.parameter_Error.GetDescription(null, "無呼叫的className");
        //            return Json(result);
        //        }
        //        if (!data.title.IsNullOrWhiteSpace())
        //            title = data.title;

        //        object obj = Activator.CreateInstance(Assembly.Load("RCT.Web").GetType($"RCT.Web.Report.Data.{data.className}"));
        //        MethodInfo[] methods = obj.GetType().GetMethods();
        //        MethodInfo mi = methods.FirstOrDefault(x => x.Name == "GetData");
        //        if (mi == null)
        //        {
        //            //檢查是否有實作資料獲取
        //            result.DESCRIPTION = "報表錯誤請聯絡IT人員";
        //            return Json(result);
        //        }
        //        DataSet ds = (DataSet)mi.Invoke(obj, new object[] { parms });
        //        List<reportParm> eparm = (List<reportParm>)(obj.GetType().GetProperty("extensionParms").GetValue(obj));
        //        ReportWrapper rw = new ReportWrapper();
        //        rw.ReportPath = Server.MapPath($"~/Report/Rdlc/{data.className}.rdlc");
        //        for (int i = 0; i < ds.Tables.Count; i++)
        //        {
        //            rw.ReportDataSources.Add(new ReportDataSource("DataSet" + (i + 1).ToString(), ds.Tables[i]));
        //        }
        //        rw.ReportParameters.Add(new ReportParameter("Title", title));
        //        if (extensionParms != null)
        //            rw.ReportParameters.AddRange(extensionParms.Select(x => new ReportParameter(x.key, x.value)));
        //        if (eparm.Any())
        //            rw.ReportParameters.AddRange(eparm.Select(x => new ReportParameter(x.key, x.value)));
        //        rw.IsDownloadDirectly = false;
        //        var g = Guid.NewGuid().ToString();
        //        Session[g] = rw;
        //        result.RETURN_FLAG = true;
        //        result.Datas = g;
        //    }
        //    catch(Exception ex)
        //    {
        //        result.DESCRIPTION = ex.exceptionMessage();
        //    }
        //    return Json(result);
        //}

        //public class reportModel
        //{
        //    public string title { get; set; }
        //    public string className { get; set; }
        //}

        public string GetDeptName(string deptId)
        {
            string deptName = "";
            deptId = deptId?.Trim();
            if (!deptId.IsNullOrWhiteSpace())
            {
                if (deptId.Length == 5)
                {
                    var _fullDepName = new Service.Actual.Common().getFullDepName(new List<string>() { deptId });
                    deptName = _fullDepName.First().Item2;
                }
            }
            deptName = (deptId == deptName ? string.Empty : deptName);
            return deptName;
        }

        public string GetUserName(string userId)
        {
            string userName = string.Empty;

            if (!userId.IsNullOrWhiteSpace())
            {
                userName = new Service.Actual.Common().GetMemoByUserId(new List<string>() { userId.Trim() }).First().Item2;
            }
            return userName;
        }
    }
}