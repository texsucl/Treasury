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
using System.Text;
using Treasury.Web.Models;

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

        /// <summary>
        /// 可以申請覆核的狀態
        /// </summary>
        internal List<string> Aply_Appr_Type { get; set; }

        /// <summary>
        /// 已經結束的狀態
        /// </summary>
        internal List<string> End_Type { get; set; }
        
        /// <summary>
        /// 
        /// </summary>
        internal List<string> Custody_Aply_Appr_Type { get; set; }

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
            End_Type = new List<string>()
            {               
                Ref.AccessProjectFormStatus.E01.ToString(),
                Ref.AccessProjectFormStatus.E02.ToString(),
                Ref.AccessProjectFormStatus.E03.ToString(),
                Ref.AccessProjectFormStatus.E04.ToString()
            };
            Custody_Aply_Appr_Type = new List<string>()
            {
                Ref.AccessProjectFormStatus.B01.ToString(),
                Ref.AccessProjectFormStatus.B02.ToString(),
                Ref.AccessProjectFormStatus.B03.ToString(),
                Ref.AccessProjectFormStatus.B04.ToString()
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
                case Ref.OpenPartialViewType.CustodyAppr: //保管單位覆核作業
                    //只有檢視功能
                    return false;
                case Ref.OpenPartialViewType.CustodyIndex: //保管單位承辦作業
                    return TreasuryAccess.GetActType(AplyNo, AccountController.CurrentUserId, Custody_Aply_Appr_Type );
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

            Treasury.WebUtility.FileRelated.createFile(Server.MapPath("~/Temp/"));
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
                        _DisplayName = $"{_DisplayName}_{_name.Values[0]}";
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

                //using (var stream = new FileStream(saveAs, FileMode.Create, FileAccess.Write))
                //{
                //    stream.Write(renderedBytes, 0, renderedBytes.Length);
                //    stream.Close();
                //}

                lr.Dispose();


                #region 寄信
                //存許項目
                string vitemIdName = extensionParms.FirstOrDefault(x => x.key == "vJobProject")?.value;
                //庫存日期
                string aplyDt = parms.FirstOrDefault(x => x.key == "APLY_DT_From")?.value;

                MAIL_TIME MT = new MAIL_TIME();
                MAIL_CONTENT MC = new MAIL_CONTENT();

                List<Tuple<string, string>> _mailTo = new List<Tuple<string, string>>() { new Tuple<string, string>("glsisys.life@fbt.com", "測試帳號-glsisys") };
                List<Tuple<string, string>> _ccTo = new List<Tuple<string, string>>();
                using (TreasuryDBEntities db = new TreasuryDBEntities())
                {
                    //季追蹤庫存表 抓5
                    MT = db.MAIL_TIME.AsNoTracking().FirstOrDefault(x => x.MAIL_TIME_ID == "5" && x.IS_DISABLED != "Y");
                    var _MAIL_CONTENT_ID = MT?.MAIL_CONTENT_ID;
                    MC = db.MAIL_CONTENT.AsNoTracking().FirstOrDefault(x => x.MAIL_CONTENT_ID == _MAIL_CONTENT_ID && x.IS_DISABLED != "Y");
                    using (DB_INTRAEntities dbINTRA = new DB_INTRAEntities())
                    {
                        //存許項目
                        string vitemId = parms.FirstOrDefault(x => x.key == "vjobProject")?.value;
                        //權責部門
                        string vdept = parms.FirstOrDefault(x => x.key == "CHARGE_DEPT_ID")?.value;
                        //權責科別
                        string vsect = parms.FirstOrDefault(x => x.key == "CHARGE_SECT_ID")?.value;

                        var _VW_OA_DEPT = dbINTRA.VW_OA_DEPT.AsNoTracking();
                        var _V_EMPLY2 = dbINTRA.V_EMPLY2.AsNoTracking();

                        var _ITEM_CHARGE_UNIT = db.ITEM_CHARGE_UNIT.AsNoTracking()
                            .Where(x => x.ITEM_ID == vitemId, vitemId != null)
                            .Where(x => x.CHARGE_DEPT == vdept, vitemId != null)
                            .Where(x => x.CHARGE_SECT == vsect, vsect != null)
                            .Where(x => x.IS_DISABLED == "N")
                            .AsEnumerable()
                            .Select(x => new ITEM_CHARGE_UNIT()
                            {
                                CHARGE_UID = x.CHARGE_UID,
                                IS_MAIL_SECT_MGR = x.IS_MAIL_SECT_MGR,
                                IS_MAIL_DEPT_MGR = x.IS_MAIL_DEPT_MGR
                            }).ToList();
                        _ITEM_CHARGE_UNIT.ForEach(x => {
                            if (x.IS_MAIL_SECT_MGR == "Y")
                            {
                                //科主管員編
                                var _VW_OA_DEPT_DPT_HEAD = _VW_OA_DEPT.FirstOrDefault(y => y.DPT_CD == vsect)?.DPT_HEAD;
                                //人名 EMAIl
                                var _EMP_NAME = _V_EMPLY2.FirstOrDefault(y => y.EMP_NO == _VW_OA_DEPT_DPT_HEAD);
                                if(_EMP_NAME != null)
                                {
                                   _mailTo.Add(new Tuple<string, string>(_EMP_NAME.EMAIL, _EMP_NAME.EMP_NAME));
                                }
                            }
                            if(x.IS_MAIL_DEPT_MGR == "Y")
                            {
                                //部主管員編
                                var _UP_DPT_CD = _VW_OA_DEPT.FirstOrDefault(y => y.DPT_CD == vsect)?.UP_DPT_CD;
                                var _VW_OA_DEPT_DPT_HEAD = _VW_OA_DEPT.FirstOrDefault(y => y.DPT_CD == _UP_DPT_CD)?.DPT_HEAD;
                                //人名 EMAIl
                                var _EMP_NAME = _V_EMPLY2.FirstOrDefault(y => y.EMP_NO == _VW_OA_DEPT_DPT_HEAD);
                                if (_EMP_NAME != null)
                                {
                                    _ccTo.Add(new Tuple<string, string>(_EMP_NAME.EMAIL, _EMP_NAME.EMP_NAME));
                                }
                            }
                        });
                    }
                }
                Dictionary<string, Stream> attachment = new Dictionary<string, Stream>();

                attachment.Add(string.Format("{0}.pdf",_DisplayName), new MemoryStream(renderedBytes));

                string str = MC?.MAIL_CONTENT1 ?? string.Empty;

                str = str.Replace("@_DATE_", aplyDt);
                str = str.Replace("@_ITEM_", vitemIdName);

                StringBuilder sb = new StringBuilder();
                sb.AppendLine(str);
//                    sb.AppendLine(
//$@"請參考附件
//");
                    try
                    {
                        var sms = new SendMail.SendMailSelf();
                        sms.smtpPort = 25;
                        sms.smtpServer = Properties.Settings.Default["smtpServer"]?.ToString();
                        sms.mailAccount = Properties.Settings.Default["mailAccount"]?.ToString();
                        sms.mailPwd = Properties.Settings.Default["mailPwd"]?.ToString();
                        sms.Mail_Send(
                            new Tuple<string, string>("glsisys.life@fbt.com", "測試帳號-glsisys"),
                           _mailTo,
                            _ccTo,
                            MC?.MAIL_SUBJECT ?? "季追蹤庫存表",
                            sb.ToString(),
                            false,
                            attachment
                            );
                    
                    }
                    catch (Exception ex)
                    {
                        result.DESCRIPTION = $"Email 發送失敗請人工通知。";
                        return Json(result);
                }
                
                #endregion

                //System.IO.File.Delete(saveAs);

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