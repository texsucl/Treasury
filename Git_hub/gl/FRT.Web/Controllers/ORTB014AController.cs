using FRT.Web.ActionFilter;
using FRT.Web.BO;
using FRT.Web.Daos;
using FRT.Web.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.EasycomClient;
using System.Data.SqlClient;
using System.Web.Mvc;


/// <summary>
/// 功能說明：特殊帳號檔維護作業及訊息table覆核作業
/// 初版作者：20181030 Daiyu
/// 修改歷程：20181030 Daiyu
///           需求單號：201808170384-00 雙系統銀行分行整併第二階段
///           初版
/// </summary>
///

namespace FRT.Web.Controllers
{
    [Authorize]
    [CheckSessionFilterAttribute]
    public class ORTB014AController : BaseController
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
                String[] roleInfo = authUtil.chkUserFuncAuth(Session["UserID"].ToString(), "~/ORTB014A/");
                if (roleInfo != null && roleInfo.Length == 2)
                {
                    opScope = roleInfo[0];
                    funcName = roleInfo[1];
                }
            }

            ViewBag.opScope = opScope;
            ViewBag.funcName = funcName;


            return View();
        }


        /// <summary>
        /// 查詢待覆核的資料
        /// </summary>
        /// <param name="groupId"></param>
        /// <param name="srceFrom"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult LoadData(string groupId, string srceFrom)
        {
            FRTRVMYDao fRTRVMYDao = new FRTRVMYDao();

            try {
                List<ORTB014Model> rows = new List<ORTB014Model>();

                rows = fRTRVMYDao.qryForORTB014A();
                var jsonData = new { success = true, rows };
                return Json(jsonData, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                logger.Error(e.ToString);
                var jsonData = new { success = false, err = e.ToString()};
                return Json(jsonData, JsonRequestBehavior.AllowGet);
            }
        }


        /// <summary>
        /// 核可/退回
        /// </summary>
        /// <param name="recData"></param>
        /// <param name="rtnData"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult execSave(List<ORTB014Model> recData, List<ORTB014Model> rtnData)
        {

            List<ErrorModel> errList = new List<ErrorModel>();

            using (EacConnection conn400 = new EacConnection(CommonUtil.GetEasycomConn()))
            {

                conn400.Open();

                FRTRVMYDao fRTRVMYDao = new FRTRVMYDao();

                string rejectPayO = "";
                string filler10O = "";

                //處理"核可"資料
                if (recData.Count > 0)
                {
                    DateTime uDt = DateTime.Now;

                    foreach (ORTB014Model d in recData)
                    {
                        if (!d.updId.Equals(Session["UserID"].ToString()))
                        {
                            try
                            {

                                rejectPayO = d.rejectPay;
                                filler10O = d.filler10;


                                if ("Y".Equals(StringUtil.toString(d.rejectPay)))   //設為"停用"
                                    d.rejectPay = "";
                                else    //設為"拒絕付款"
                                    d.rejectPay = "Y";

                                d.apprDate = (uDt.Year - 1911).ToString() + uDt.Month.ToString().PadLeft(2, '0') + uDt.Day.ToString().PadLeft(2, '0');
                                d.apprTimeN = uDt.ToString("HHmmssff");
                                d.apprId = Session["UserID"].ToString();
                                d.filler10 = "";
                                fRTRVMYDao.UpdForORTB014Appr("2", d, conn400);
                            }
                            catch (Exception e)
                            {
                                ErrorModel errorModel = new ErrorModel();
                                errorModel.bankNo = d.bankNo;
                                errorModel.bankAct = d.bankAct;
                                errorModel.failCode = d.failCode;
                                errorModel.errorMsg = StringUtil.toString(e.Message);

                                errList.Add(errorModel);
                            }
                        }
                        else
                        {
                            ErrorModel errorModel = new ErrorModel();
                            errorModel.bankNo = d.bankNo;
                            errorModel.bankAct = d.bankAct;
                            errorModel.failCode = d.failCode;
                            errorModel.errorMsg = "申請人與覆核人員不可相同!!";

                            errList.Add(errorModel);
                        }
                    }
                }

                //處理"駁回"資料
                if (rtnData.Count > 0)
                {
                    foreach (ORTB014Model d in rtnData)
                    {
                        if (!d.updId.Equals(Session["UserID"].ToString()))
                        {
                            try
                            {
                                d.filler10 = "";
                                fRTRVMYDao.UpdForORTB014Appr("3", d, conn400);
                            }
                            catch (Exception e)
                            {
                                ErrorModel errorModel = new ErrorModel();
                                errorModel.bankNo = d.bankNo;
                                errorModel.bankAct = d.bankAct;
                                errorModel.failCode = d.failCode;
                                errorModel.errorMsg = StringUtil.toString(e.Message);

                                errList.Add(errorModel);
                            }
                        }
                        else {
                            ErrorModel errorModel = new ErrorModel();
                            errorModel.bankNo = d.bankNo;
                            errorModel.bankAct = d.bankAct;
                            errorModel.failCode = d.failCode;
                            errorModel.errorMsg = "申請人與覆核人員不可相同!!";

                            errList.Add(errorModel);
                        }
                    }
                }
            }



            if (errList.Count > 0)
            {
                return Json(new { success = false, err = errList }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { success = true });
            }

        }

        /// <summary>
        /// 錯誤參數model
        /// </summary>
        public partial class ErrorModel
        {

            public string bankNo { get; set; }

            public string bankAct { get; set; }

            public string failCode { get; set; }

            public string errorMsg { get; set; }
        }
    }
}