using FRT.Web.ActionFilter;
using FRT.Web.AS400Models;
using FRT.Web.BO;
using FRT.Web.Daos;
using FRT.Web.Models;
using FRT.Web.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.EasycomClient;
using System.Linq;
using System.Web.Mvc;

/// <summary>
/// 功能說明：特殊帳號檔維護作業及訊息table維護作業
/// 初版作者：20181029 Daiyu
/// 修改歷程：20181029 Daiyu
///           需求單號：201808170384-00 雙系統銀行分行整併第二階段
///           初版
/// </summary>
///

namespace FRT.Web.Controllers
{
    [Authorize]
    [CheckSessionFilterAttribute]
    public class ORTB014Controller : BaseController
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
                String[] roleInfo = authUtil.chkUserFuncAuth(Session["UserID"].ToString(), "~/ORTB014/");
                if (roleInfo != null && roleInfo.Length == 2)
                {
                    opScope = roleInfo[0];
                    funcName = roleInfo[1];
                }
            }

            ViewBag.opScope = opScope;
            ViewBag.funcName = funcName;

            SysCodeDao sysCodeDao = new SysCodeDao();
            //資料狀態
            ViewBag.statusjqList = sysCodeDao.jqGridList("RT", "DATA_STATUS", false);

            return View();
        }



        /// <summary>
        /// 查詢特殊帳號檔FRTRVMY
        /// </summary>
        /// <param name="bankNo"></param>
        /// <param name="bankAct"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult qryFRTRVMY(string bankNo, string bankAct, string qDateB, string qDateE)
        {
            try {
                FRTRVMYDao fRTRVMYDao = new FRTRVMYDao();
                List<ORTB014Model> rows = fRTRVMYDao.qryForORTB014(bankNo, bankAct, qDateB, qDateE);
               

                return Json(new { success = true, rows}, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e) {
                logger.Error(e.ToString());
                return Json(new { success = false, err = "其它錯誤，請洽系統管理員!!" }, JsonRequestBehavior.AllowGet);
            }
            
        }
        


        /// <summary>
        /// 畫面執行"申請覆核"
        /// </summary>
        /// <param name="bankCode"></param>
        /// <param name="bankType"></param>
        /// <param name="gridData"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult execSave(string bankNo, List<ORTB014Model> recData, List<ORTB014Model> rtnData)
        {
            List<ErrorModel> errList = new List<ErrorModel>();

            using (EacConnection conn400 = new EacConnection(CommonUtil.GetEasycomConn()))
            {
                
                conn400.Open();

                FRTRVMYDao fRTRVMYDao = new FRTRVMYDao();

                string rejectPayO = "";
                string filler10O = "";

                //處理"拒絕付款"資料
                if (recData.Count > 0)
                {

                    foreach (ORTB014Model d in recData.GroupBy(o => new { o.bankNo, o.bankAct, o.rejectPay, o.filler10 })
                        .Select(group => new ORTB014Model
                        {
                            bankNo = group.Key.bankNo,
                            bankAct = group.Key.bankAct,
                            rejectPay = group.Key.rejectPay,
                            filler10 = group.Key.filler10,
                        }).ToList<ORTB014Model>())
                    {
                        try
                        {
                            rejectPayO = StringUtil.toString(d.rejectPay);
                            filler10O = StringUtil.toString(d.filler10);
                            d.updId = Session["UserID"].ToString();
                            d.rejectPay = "";
                            d.filler10 = "Y";
                            fRTRVMYDao.UpdForORTB014(rejectPayO, filler10O, d, conn400);
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
                }

                //處理"停用"資料
                if (rtnData.Count > 0)
                {
                    foreach (ORTB014Model d in rtnData.GroupBy(o => new { o.bankNo, o.bankAct, o.rejectPay, o.filler10 })
                        .Select(group => new ORTB014Model
                        {
                            bankNo = group.Key.bankNo,
                            bankAct = group.Key.bankAct,
                            rejectPay = group.Key.rejectPay,
                            filler10 = group.Key.filler10,
                        }).ToList<ORTB014Model>())
                    {
                        try
                        {
                            rejectPayO = StringUtil.toString(d.rejectPay);
                            filler10O = StringUtil.toString(d.filler10);
                            d.updId = Session["UserID"].ToString();
                            d.rejectPay = "Y";
                            d.filler10 = "X";
                            fRTRVMYDao.UpdForORTB014(rejectPayO, filler10O, d, conn400);
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