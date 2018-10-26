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
using System.IO;

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
                Ref.AccessProjectFormStatus.A05.ToString(),
                Ref.AccessProjectFormStatus.A06.ToString()
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
                case Ref.OpenPartialViewType.TAAppr: //金庫物品存取覆核作業
                    //只有檢視功能
                    return false; 

                case Ref.OpenPartialViewType.TAIndex: //金庫物品存取申請作業
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

        /// <summary>
        /// 寄送報表
        /// </summary>
        /// <param name="data"></param>
        /// <param name="parms"></param>
        /// <param name="extensionParms"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult SendReport(
            reportModel data,
            List<reportParm> parms,
            List<reportParm> extensionParms = null
            )
        {
            MSGReturnModel<string> result = new MSGReturnModel<string>();
            result.RETURN_FLAG = false;

            FileRelated.createFile(Server.MapPath("~/Temp/"));
            try
            {
                var fileLocation = Server.MapPath("~/Temp/");

                string title = "報表名稱";
                if (data.className.IsNullOrWhiteSpace())
                {
                    result.DESCRIPTION = "寄送報表錯誤請聯絡IT人員";
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
                    result.DESCRIPTION = "寄送報表錯誤請聯絡IT人員";
                    return Json(result);
                }
                DataSet ds = (DataSet)mi.Invoke(obj, new object[] { parms });
                List<reportParm> eparm = (List<reportParm>)(obj.GetType().GetProperty("extensionParms").GetValue(obj));

                var lr = new LocalReport();
                lr.ReportPath = Server.MapPath($"~/Report/Rdlc/{data.className}.rdlc");
                lr.DataSources.Clear();
                List<ReportParameter> _parm = new List<ReportParameter>();
                _parm.Add(new ReportParameter("Title", title));
                if (extensionParms != null)
                    _parm.AddRange(extensionParms.Select(x => new ReportParameter(x.key, x.value)));
                if (eparm.Any())
                    _parm.AddRange(eparm.Select(x => new ReportParameter(x.key, x.value)));
                if(_parm.Any())
                    lr.SetParameters(_parm);
              
                for (int i = 0; i < ds.Tables.Count; i++)
                {
                    lr.DataSources.Add(new ReportDataSource("DataSet" + (i + 1).ToString(), ds.Tables[i]));
                }
                
                string _DisplayName = title;
                if (_DisplayName != null)
                {                   
                    _DisplayName = _DisplayName.Replace("(", "-").Replace(")", "");
                    var _name = _parm.FirstOrDefault(x => x.Name == "vJobProject");
                    if (_name != null)
                        _DisplayName = $"{_DisplayName}({_name.Values[0]})";
                }
                lr.DisplayName = _DisplayName;
                lr.Refresh();

                string mimeType, encoding, extension;

                Warning[] warnings;
                string[] streams;
                var renderedBytes = lr.Render
                    (
                        "PDF",
                        null,
                        out mimeType,
                        out encoding,
                        out extension,
                        out streams,
                        out warnings
                    );

                var saveAs = string.Format("{0}.pdf", Path.Combine(fileLocation, _DisplayName));

                var idx = 0;
                while (Directory.Exists(saveAs))
                {
                    idx++;
                    saveAs = string.Format("{0}.{1}.pdf", Path.Combine(fileLocation, _DisplayName), idx);
                }

                using (var stream = new FileStream(saveAs, FileMode.Create, FileAccess.Write))
                {
                    stream.Write(renderedBytes, 0, renderedBytes.Length);
                    stream.Close();
                }

                lr.Dispose();

                #region 寄信

                #endregion

                System.IO.File.Delete(saveAs);

                //Response.ClearHeaders();
                //Response.ClearContent();
                //Response.Buffer = true;
                //Response.Clear();
                //Response.Charset = "";
                //Response.ContentType = "application/pdf";
                //Response.AddHeader("Content-Disposition", "attachment;filename=\"" + FileName + "\"");
                //Response.WriteFile(Server.MapPath("~/rdlc/Reports/" + FileName));

                //Response.Flush();
                //Response.Close();
                //Response.End();

                result.RETURN_FLAG = true;
                result.DESCRIPTION = "已寄送追蹤報表!";
            }
            catch (Exception ex)
            {
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