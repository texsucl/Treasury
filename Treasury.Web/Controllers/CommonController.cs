using Treasury.WebBO;
using System;
using Treasury.Web;
using Treasury.Web.Service.Interface;
using Treasury.Web.Service.Actual;
using System.Web.Mvc;
using System.Collections.Generic;
using Treasury.WebUtility;
using Treasury.Web.Enum;
using System.Reflection;
using System.Linq;
using Microsoft.Reporting.WebForms;
using System.Data;
using Treasury.Web.ViewModels;
using Treasury.Web.Controllers;

/// <summary>
/// 功能說明：共用 controller
/// 初版作者：20180604 張家華
/// 修改歷程：20180604 張家華 
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

    public class CommonController : BaseController
    {
        internal ICacheProvider Cache { get; set; }
        internal List<string> Aply_Appr_Type { get; set; }

        protected ITreasuryAccess TreasuryAccess;

        public CommonController()
        {
            Cache = new DefaultCacheProvider();
            Aply_Appr_Type = new List<string>()
            {
                Ref.AccessProjectFormStatus.A01.ToString(),
                Ref.AccessProjectFormStatus.A02.ToString(),
                Ref.AccessProjectFormStatus.A03.ToString(),
                Ref.AccessProjectFormStatus.A04.ToString(),
                Ref.AccessProjectFormStatus.A05.ToString()
            };
            TreasuryAccess = new TreasuryAccess();
        }

        /// <summary>
        /// 判斷 PartialView 是否可以CRUD
        /// </summary>
        /// <param name="type"></param>
        /// <param name="AplyNo"></param>
        /// <returns></returns>
        protected bool GetActType(Ref.OpenPartialViewType type, string AplyNo)
        {
            if (AplyNo.IsNullOrWhiteSpace()) //沒有申請單號表示可以CRUD
                return true;
            switch (type) //哪個畫面呼叫PartialView
            {
                case Ref.OpenPartialViewType.Appr: //金庫物品存取覆核作業
                    //只有檢視功能
                    return false; 

                case Ref.OpenPartialViewType.Index: //金庫物品存取申請作業
                default:
                    //查詢作業 有單號的申請,如果為填表人本人可以修改
                    return TreasuryAccess.GetActType(AplyNo, AccountController.CurrentUserId, Aply_Appr_Type);
            }
        }

        protected string GetopScope(string funcId)
        {
            UserAuthUtil authUtil = new UserAuthUtil();
            String opScope = "";
            String[] roleInfo = authUtil.chkUserFuncAuth(Session["UserID"].ToString(), funcId);
            if (roleInfo != null && roleInfo.Length == 1)
            {
                opScope = roleInfo[0];
            }
            return opScope;
        }

        /// <summary>
        /// 報表
        /// </summary>
        /// <param name="data"></param>
        /// <param name="parms"></param>
        /// <param name="extensionParms"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult GetReport(
            reportModel data,
            List<reportParm> parms,
            List<reportParm> extensionParms  = null
            )
        {
            MSGReturnModel<string> result = new MSGReturnModel<string>();
            result.RETURN_FLAG = false;
            try
            {
                string title = "報表名稱";
                if (data.className.IsNullOrWhiteSpace())
                {
                    result.DESCRIPTION = "報表錯誤請聯絡IT人員";
                    //result.DESCRIPTION = MessageType.parameter_Error.GetDescription(null, "無呼叫的className");
                    return Json(result);
                }
                if (!data.title.IsNullOrWhiteSpace())
                    title = data.title;
                object obj = Activator.CreateInstance(Assembly.Load("Treasury.Web").GetType($"Treasury.Web.Report.Data.{data.className}"));
                MethodInfo[] methods = obj.GetType().GetMethods();
                MethodInfo mi = methods.FirstOrDefault(x => x.Name == "GetData");
                if (mi == null)
                {
                    //檢查是否有實作資料獲取
                    result.DESCRIPTION = "報表錯誤請聯絡IT人員";
                    return Json(result);
                }
                DataSet ds = (DataSet)mi.Invoke(obj, new object[] { parms });
                List<reportParm> eparm = (List<reportParm>)(obj.GetType().GetProperty("extensionParms").GetValue(obj));
                ReportWrapper rw = new ReportWrapper();
                rw.ReportPath = Server.MapPath($"~/Report/Rdlc/{data.className}.rdlc");
                for (int i = 0; i < ds.Tables.Count; i++)
                {
                    rw.ReportDataSources.Add(new ReportDataSource("DataSet" + (i + 1).ToString(), ds.Tables[i]));
                }
                rw.ReportParameters.Add(new ReportParameter("Title", title));
                if (extensionParms != null)
                    rw.ReportParameters.AddRange(extensionParms.Select(x => new ReportParameter(x.key, x.value)));
                if (eparm.Any())
                    rw.ReportParameters.AddRange(eparm.Select(x => new ReportParameter(x.key, x.value)));
                rw.IsDownloadDirectly = false;
                var g = Guid.NewGuid().ToString();
                Session[g] = rw;
                result.RETURN_FLAG = true;
                result.Datas = g;
            }
            catch (Exception ex){
                result.DESCRIPTION = ex.exceptionMessage();
            }
            return Json(result);
        }

        public class reportModel {
            public string title { get; set; }
            public string className { get; set; }
        }
    }
}