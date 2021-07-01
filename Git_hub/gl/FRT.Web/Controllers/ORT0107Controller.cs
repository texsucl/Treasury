using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using static FRT.Web.BO.Utility;
using static FRT.Web.Enum.Ref;
using FRT.Web.Service.Actual;
using FRT.Web.Service.Interface;
using FRT.Web.ViewModels;
using FRT.Web.BO;
using FRT.Web.CacheProvider;
using FRT.Web.Models;

namespace FRT.Web.Controllers
{
    public class ORT0107Controller : Controller
    {
        internal ICacheProvider Cache { get; set; }

        private IORT0106 ORT0106 { get; set; }

        public ORT0107Controller()
        {
            Cache = new DefaultCacheProvider();
            ORT0106 = new ORT0106();
        }

        public ActionResult Index()
        {
            UserAuthUtil authUtil = new UserAuthUtil();

            string opScope = "";
            string funcName = "";

            if (Session["UserID"] != null)
            {
                String[] roleInfo = authUtil.chkUserFuncAuth(Session["UserID"].ToString(), "~/ORT0107/");
                if (roleInfo != null && roleInfo.Length == 2)
                {
                    opScope = roleInfo[0];
                    funcName = roleInfo[1];
                }
            }

            ViewBag.opScope = opScope;
            ViewBag.funcName = funcName;

            ViewBag.cross_system = new SelectList(ORT0106.getCross_System("BD"), "Value", "Text");
            return View();
        }

        [HttpPost]
        public JsonResult GetReport(ORT0106ViewModel model)
        {
            MSGReturnModel<bool> result = new MSGReturnModel<bool>();
            var _userId = AccountController.CurrentUserId;
            IORT0105Report _IORT0105Report = null;
            var parms = model?.id?.Split('_');
            if (parms.Length != 3)
            {
                result.DESCRIPTION = MessageType.parameter_Error.GetDescription();
                return Json(result);
            }
            var _data = ORT0106.getCheck(parms.Last());
            var _Cross_System = ORT0106.getCross_System();
            var _type = $@"{parms[0]}_{parms[1]}";

            //判斷此隻程式現在執行中
            if (model.runFlag != "Y" && _data.RETURN_FLAG && _data.Datas?.runFlag == "Y")
            {
                result.RETURN_FLAG = true;
                result.DESCRIPTION = "SAME";
                return Json(result);
            }           
            switch (_type)
            {
                //case "AP_1":
                //    result.RETURN_FLAG = true;
                //    model.className = "ORT0105AP1";
                //    _IORT0105Report = new ORT0105AP1();
                //    break;
                //case "AP_2":
                //    result.RETURN_FLAG = true;
                //    model.className = "ORT0105AP2";
                //    _IORT0105Report = new ORT0105AP2();
                //    break;
                //case "AP_3":
                //    result.RETURN_FLAG = true;
                //    model.className = "ORT0105AP3";
                //    _IORT0105Report = new ORT0105AP3();
                //    break;
                case "BD_1":
                    result.RETURN_FLAG = true;
                    model.className = "ORT0105BD1";
                    _IORT0105Report = new ORT0105BD1();
                    break;
                case "BD_2":
                    result.RETURN_FLAG = true;
                    model.className = "ORT0105BD2";
                    _IORT0105Report = new ORT0105BD2();
                    break;
                case "NP_1":
                    
                    break;
                case "NP_2":
                   
                    break;
                case "NP_3":
                    
                    break;
                case "NP_4":

                    break;
                case "NP_5":

                    break;
            }
            if (result.RETURN_FLAG)
            {
                var _check = new FRT_CROSS_SYSTEM_CHECK()
                {
                    check_id = parms[2]
                };
                //System.Threading.Tasks.Task.Run(() =>
                //{
                //    _IORT0105Report.check(_check, _userId, ReportType.S, model.date_s, model.date_e, model.deadline);
                //});
                var _report = _IORT0105Report.check(_check, _userId, ReportType.S, model.date_s, model.date_e, model.deadline);
                if (_report.RETURN_FLAG)
                {
                    result.DESCRIPTION = _report.DESCRIPTION;
                    //var _title_date = new GoujiReport().getReportDate(null, model.date_s, model.date_e);
                    //model.title_Date = $@"{_title_date.Item2}~{_title_date.Item3}";
                    //model.title = _Cross_System.FirstOrDefault(x => x.Value.IndexOf(_type) > -1)?.Text;
                    //result.Datas = _report.Datas.Item2;
                    //Cache.Invalidate(CacheList.ORT0106ViewData);
                    //Cache.Set(CacheList.ORT0106ViewData, new Tuple<ORT0106ViewModel, IORT0105ReportModel>(model, _report.Datas.Item1));
                }
                else
                {
                    result.DESCRIPTION = _report.DESCRIPTION;
                }
                //result.DESCRIPTION = "已開始執行";
            }
            else
            {
                result.DESCRIPTION = MessageType.sys_Error.GetDescription();
            }
            return Json(result);
        }

        [HttpPost]
        public JsonResult GetDetail(string check_id)
        {
            var parms = check_id?.Split('_');
            if (parms == null || parms.Length != 3)
            {
                MSGReturnModel result = new MSGReturnModel();
                result.DESCRIPTION = MessageType.parameter_Error.GetDescription();
                return Json(result);
            }
            return Json(ORT0106.getCheck(parms.Last()));
        }


    }
}