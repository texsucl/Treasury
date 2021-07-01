using FRT.Web.ActionFilter;
using FRT.Web.AS400Models;
using FRT.Web.BO;
using FRT.Web.Daos;
using FRT.Web.Models;
using FRT.Web.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.EasycomClient;
using System.Data.SqlClient;
using System.Web.Mvc;


/// <summary>
/// 功能說明：ORT0102Q 非保費類退匯給付方式查詢
/// 初版作者：20210113 Daiyu
/// 修改歷程：20210113 Daiyu
/// 需求單號：202011050211
/// 修改內容：初版
/// </summary>
///

namespace FRT.Web.Controllers
{
    [Authorize]
    [CheckSessionFilterAttribute]
    public class ORT0102QController : BaseController
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

            if (Session["UserID"] != null)
            {
                String[] roleInfo = authUtil.chkUserFuncAuth(Session["UserID"].ToString(), "~/ORT0102Q/");
                if (roleInfo != null && roleInfo.Length == 2)
                {
                    opScope = roleInfo[0];
                    funcName = roleInfo[1];
                }
            }

            ViewBag.opScope = opScope;
            ViewBag.funcName = funcName;

            SysCodeDao sysCodeDao = new SysCodeDao();
            ViewBag.codeTypeList = sysCodeDao.loadSelectList("RT", "ORT0102", false);


            //資料狀態
            ViewBag.statusjqList = sysCodeDao.jqGridList("RT", "DATA_STATUS", true);

            return View();
        }

        /// <summary>
        /// 查詢
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult qryFRTCODE(string ptype)
        {
            logger.Info("qryFRTCODE begin!!");

            try
            {
                //自AS400查出資料
                FRTCODEDao fRTCODEDao = new FRTCODEDao();
                List<FRTCODEModel> as400_dataList = new List<FRTCODEModel>();
                as400_dataList = fRTCODEDao.qryRTCode("RT", ptype, "");

                FRTCodeHisDao fRTCodeHisDao = new FRTCodeHisDao();

                List<ORT0102Model> dataList = new List<ORT0102Model>();


                //拆解RT-CODE資料，並判斷是否可維護
                foreach (FRTCODEModel d in as400_dataList) {
                    ORT0102Model model = new ORT0102Model();
                    model.ptype = d.groupId;
                    model.group_id = d.groupId;
                    string _ref_no = d.refNo.PadRight(6, ' ');
                    model.tempId = d.refNo;
                    model.system = _ref_no.Substring(0, 1);
                    model.srce_from = _ref_no.Substring(1, 3);
                    model.srce_kind = _ref_no.Substring(4, 2);

                    FRT_CODE_HIS _his = new FRT_CODE_HIS();
                    _his = fRTCodeHisDao.chkOnAply("RT", model.ptype, d.refNo);

                    if (_his != null)
                        model.data_status = "2";
                    else
                        model.data_status = "1";

                    model.update_id = string.IsNullOrWhiteSpace(d.updId) == true ? "" : d.updId;
                    model.update_datetime = string.IsNullOrWhiteSpace(d.updDate) == true ? "" : d.updDate;
                    model.appr_id = string.IsNullOrWhiteSpace(d.apprId) == true ? "" : d.apprId;
                    model.approve_datetime = string.IsNullOrWhiteSpace(d.apprDate) == true ? "" : d.apprDate;

                    dataList.Add(model);
                }

                var jsonData = new { success = true, dataList = dataList };
                return Json(jsonData, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                logger.Error(e.ToString());
                return Json(new { success = false, err = "其它錯誤，請洽系統管理員!!" }, JsonRequestBehavior.AllowGet);
            }
        }





        internal class ErrorModel
        {
            public string tempId { get; set; }
            public bool chkResult { get; set; }
            public string msg { get; set; }
        }
    }
}