using FRT.Web;
using FRT.Web.BO;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using Microsoft.Reporting.WebForms;
using FRT.Web.CacheProvider;
using System.IO;
using FRT.Web.ViewModels;

namespace FRT.Web.Controllers
{
    public class ReportController : BaseController
    {
        internal ICacheProvider Cache { get; set; }

        public ReportController()
        {
            Cache = new DefaultCacheProvider();
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
            List<reportParm> extensionParms = null
            )
        {
            MSGReturnModel<string> result = new MSGReturnModel<string>();
            result.RETURN_FLAG = false;
            try
            {
                string title = "報表名稱";
                if (string.IsNullOrWhiteSpace(data.className))
                {
                    result.DESCRIPTION = "報表錯誤請聯絡IT人員";
                    //result.DESCRIPTION = MessageType.parameter_Error.GetDescription(null, "無呼叫的className");
                    return Json(result);
                }
                if (!string.IsNullOrWhiteSpace(data.title))
                    title = data.title;
                object obj = Activator.CreateInstance(Assembly.Load("FRT.Web").GetType($"FRT.Web.Report.Data.{data.className}"));
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
                ReportWrapperO rw = new ReportWrapperO();
                rw.ReportPath = Server.MapPath($"~/Report/Rdlc/{data.className}.rdlc");
                for (int i = 0; i < ds.Tables.Count; i++)
                {
                    rw.ReportDataSources.Add("DataSet" + (i + 1).ToString(), ds.Tables[i]);
                }
                rw.ReportParameters.Add("Title", title);
                rw.ReportParameters.Add("UserId", Session["UserID"]?.ToString() + Session["UserName"]?.ToString());
                if (extensionParms != null)
                    extensionParms.ForEach(x => {
                        rw.ReportParameters.Add(x.key, x.value);
                    });
                if (eparm.Any())
                    eparm.ForEach(x =>
                    {
                        rw.ReportParameters.Add(x.key, x.value);
                    });
                rw.IsDownloadDirectly = false;
                var g = Guid.NewGuid().ToString();
                Session[g] = rw;
                result.RETURN_FLAG = true;
                result.Datas = g;
            }
            catch (Exception ex)
            {
                result.DESCRIPTION = ex.ToString();
            }
            return Json(result);
        }

        /// <summary>
        /// 由cache 打開 報表
        /// </summary>
        /// <param name="id"></param>
        /// <param name="type">A => 主表 , B => 明細表</param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult GetReportByCacheData(string id, string type = "A")
        {
            MSGReturnModel<string> result = new MSGReturnModel<string>();
            result.RETURN_FLAG = false;
            try
            {
                if (Cache.IsSet(CacheList.ORT0106ViewData))
                {
                    var _data = (Tuple<ORT0106ViewModel, IORT0105ReportModel>)Cache.Get(CacheList.ORT0106ViewData);
                    var parms = _data.Item1?.id?.Split('_');
                    if (_data.Item1.id == id && parms.Length == 3)
                    {
                        string title = "報表名稱";  
                        title = _data.Item1.title;
                        if (type == "B")
                            title += "差異明細表";
                        //List<reportParm> _parms = new List<reportParm>();
                        //_parms.Add(new reportParm() { key = "Title", value = title });
                        //_parms.Add(new reportParm() { key = "Date", value = _data.Item1.title_Date.dateFormat() });
                        //_parms.Add(new reportParm() { key = "UserId", value = Session["UserID"]?.ToString() + Session["UserName"]?.ToString() });
                        IORT0105ReportModel _model = _data.Item2;
                        DataSet ds = new DataSet();
                        ReportWrapperO rw = new ReportWrapperO();
                        switch ($@"{parms[0]}_{parms[1]}")
                        {
                            case "AP_1":
                                if (type == "A")
                                    ds.Tables.Add(((ORT0105AP1ReportModel)(_model)).model.Item1.ToDataTable());
                                else if (type == "B")
                                    ds.Tables.Add(((ORT0105AP1ReportModel)(_model)).model.Item3.ToDataTable());
                                break;
                            case "AP_2":
                                if (type == "A")
                                    ds.Tables.Add(((ORT0105AP2ReportModel)(_model)).model.Item1.ToDataTable());
                                else if (type == "B")
                                    ds.Tables.Add(((ORT0105AP2ReportModel)(_model)).model.Item3.ToDataTable());
                                break;
                            case "AP_3":
                                if (type == "A")
                                    ds.Tables.Add(((ORT0105AP2ReportModel)(_model)).model.Item1.ToDataTable());
                                else if (type == "B")
                                    ds.Tables.Add(((ORT0105AP2ReportModel)(_model)).model.Item3.ToDataTable());
                                break;
                            case "BD_1":
                                if (type == "A")
                                {
                                    title = @"AS400 及 Wanpie 比對結果差異報表-銀存銷帳-已銷帳";
                                    ds.Tables.Add(((ORT0105BD1ReportModel)(_model)).model.Item2.ToDataTable());
                                    var _BD1pars = ((ORT0105BD1ReportModel)(_model)).model.Item1;
                                    rw.ReportParameters.Add("AS400_AMT", _BD1pars.AS400_AMT);
                                    rw.ReportParameters.Add("Wanpie_AMT", _BD1pars.Wanpie_AMT);
                                    rw.ReportParameters.Add("Diff_AMT", _BD1pars.Diff_AMT);
                                    rw.ReportParameters.Add("Compare_Result", _BD1pars.Compare_Result);
                                    type = string.Empty;
                                }
                                break;
                            case "BD_2":
                                if (type == "A")
                                {
                                    title = @"AS400 及 Wanpie 比對結果差異報表-銀存銷帳-未銷帳";
                                    ds.Tables.Add(((ORT0105BD1ReportModel)(_model)).model.Item2.ToDataTable());
                                    var _BD1pars = ((ORT0105BD1ReportModel)(_model)).model.Item1;
                                    rw.ReportParameters.Add("AS400_AMT", _BD1pars.AS400_AMT);
                                    rw.ReportParameters.Add("AS400_Count", _BD1pars.AS400_Count);
                                    rw.ReportParameters.Add("Wanpie_AMT", _BD1pars.Wanpie_AMT);
                                    rw.ReportParameters.Add("Wanpie_Count", _BD1pars.Wanpie_Count);
                                    rw.ReportParameters.Add("Diff_AMT", _BD1pars.Diff_AMT);
                                    rw.ReportParameters.Add("Diff_Count", _BD1pars.Diff_Count);
                                    rw.ReportParameters.Add("Compare_Result", _BD1pars.Compare_Result);
                                    rw.ReportParameters.Add("Deadline", _BD1pars.Deadline);
                                    type = string.Empty;
                                }
                                break;
                            case "NP_1":

                                break;
                            case "NP_2":

                                break;
                            case "NP_3":
                                if (type == "A")
                                {
                                    ds.Tables.Add(((ORT0105NP1ReportModel)(_model)).model.Item1.ToDataTable());
                                    var _NP3pars = ((ORT0105NP1ReportModel)(_model)).model.Item1.First();
                                    rw.ReportParameters.Add("NO", _NP3pars.NO);
                                    rw.ReportParameters.Add("Kind", _NP3pars.Kind);
                                }
                                else if (type == "B")
                                { 
                                    ds.Tables.Add(((ORT0105NP1ReportModel)(_model)).model.Item3.ToDataTable());
                                    var _NP3pars = ((ORT0105NP1ReportModel)(_model)).model.Item3.First();
                                    rw.ReportParameters.Add("NO", _NP3pars.NO);
                                    rw.ReportParameters.Add("Kind", _NP3pars.Kind);
                                }
                                break;
                            case "NP_4":

                                break;
                            case "NP_5":

                                break;
                        }
                        rw.ReportPath = Server.MapPath($"~/Report/Rdlc/{_data.Item1.className}{type}.rdlc");            
                        for (int i = 0; i < ds.Tables.Count; i++)
                        {
                            rw.ReportDataSources.Add("DataSet" + (i + 1).ToString(), ds.Tables[i]);
                        }
                        rw.ReportParameters.Add("Date", _data.Item1.title_Date.dateFormat());
                        rw.ReportParameters.Add("Title", title);
                        rw.ReportParameters.Add("UserId", Session["UserID"]?.ToString() + Session["UserName"]?.ToString());
                        rw.IsDownloadDirectly = false;
                        var g = Guid.NewGuid().ToString();
                        Session[g] = rw;
                        result.RETURN_FLAG = true;
                        result.Datas = g;
                    }
                }
                if(!result.RETURN_FLAG)
                    result.DESCRIPTION = "報表錯誤請聯絡IT人員";
            }
            catch (Exception ex)
            {
                result.DESCRIPTION = ex.ToString();
            }
            return Json(result);
        }

        public class reportModel
        {
            public string title { get; set; }
            public string className { get; set; }
        }

    }
}