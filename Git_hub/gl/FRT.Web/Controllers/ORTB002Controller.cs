using FRT.Web.ActionFilter;
using FRT.Web.AS400Models;
using FRT.Web.BO;
using FRT.Web.Daos;
using FRT.Web.ViewModels;
using System;
using System.Collections.Generic;
using System.Web.Mvc;

/// <summary>
/// 功能說明：判斷快速付款銀行類型維護作業
/// 初版作者：20180628 Daiyu
/// 修改歷程：2018028 Daiyu
///           需求單號：
///           初版
/// </summary>
///

namespace FRT.Web.Controllers
{
    [Authorize]
    [CheckSessionFilterAttribute]
    public class ORTB002Controller : BaseController
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
                String[] roleInfo = authUtil.chkUserFuncAuth(Session["UserID"].ToString(), "~/ORTB002/");
                if (roleInfo != null && roleInfo.Length == 2)
                {
                    opScope = roleInfo[0];
                    funcName = roleInfo[1];
                }
            }

            ViewBag.opScope = opScope;
            ViewBag.funcName = funcName;



            //資料狀態
            SysCodeDao sysCodeDao = new SysCodeDao();
            ViewBag.statusjqList = sysCodeDao.jqGridList("RT", "STATUS", true);



            return View();
        }



        /// <summary>
        /// 查詢"FRTBBKM 快速付款銀行類型資料檔"
        /// </summary>
        /// <param name="bankCode"></param>
        /// <param name="bankType"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult qryFRTBBKM(string bankCode, string bankType)
        {
            logger.Info("qryFRTBBKM begin!!");

            try
            {
                FRTBBKMDao fRTBBKMDao = new FRTBBKMDao();
                List<ORTB002Model> rows = fRTBBKMDao.qryForORTB002(bankCode, bankType);


                var jsonData = new { success = true, dataList = rows };
                return Json(jsonData, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e) {
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
        public JsonResult execSave(string bankCode, string bankType, List<ORTB002Model> gridData)
        {
            logger.Info("execSave begin");

            string errStr = "";
            bool bChg = false;

            FRTBBKMDao fRTBBKMDao = new FRTBBKMDao();
            List<ORTB002Model> dataList = new List<ORTB002Model>();


            foreach (ORTB002Model d in gridData)
            {
                if (!"".Equals(StringUtil.toString(d.status)))
                {
                    errModel errModel = chkAplyData(d.status, d.bankType, d.bankCode);

                    if (errModel.chkResult)
                    {
                        bChg = true;
                        d.updId = Session["UserID"].ToString();
                        dataList.Add(d);
                    }
                    else
                    {
                        errStr += "銀行類型：" + d.bankType + " 銀行代碼：" + d.bankCode + "<br/>";
                    }

                }
            }

            if (bChg == false) {
                if("".Equals(errStr))
                    return Json(new { success = false, err = "未異動畫面資料，將不進行修改覆核作業!!" }, JsonRequestBehavior.AllowGet);
                else
                    return Json(new { success = true, err = errStr });
            }
                



            /*------------------ DB處理   begin------------------*/
            FRTBBKHDao fRTBBKHDao = new FRTBBKHDao();

            string[] curDateTime = DateUtil.getCurChtDateTime(4).Split(' ');
            //取得流水號
            SysSeqDao sysSeqDao = new SysSeqDao();
            String qPreCode = curDateTime[0];
            var cId = sysSeqDao.qrySeqNo("RT", "B002", qPreCode).ToString();


            fRTBBKHDao.insertFRTBBKH0(qPreCode + cId.ToString().PadLeft(3, '0'), dataList);

            return Json(new { success = true, aplyNo = qPreCode + cId.ToString().PadLeft(3, '0'), err = errStr });

        }



        /// <summary>
        /// 查詢銀行名稱
        /// </summary>
        /// <param name="bankCode"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult qryBankCode(string bankCode)
        {
            try {
                FDCBANKDao fDCBANKDao = new FDCBANKDao();
                List<FDCBANKModel> fDCBANKModel = fDCBANKDao.qryByBankNo(bankCode);

                string bankName = "";
                if (fDCBANKModel.Count > 0)
                    bankName = StringUtil.toString(fDCBANKModel[0].bankName);

                return Json(new { success = true, bankName = bankName });
            }
            catch (Exception e) {
                logger.Error(e.ToString());
                return Json(new { success = false, err = "其它錯誤，請洽系統管理員!!" }, JsonRequestBehavior.AllowGet);
            }
        }


        /// <summary>
        /// 畫面GRID在儲存前，需先檢查可存檔
        /// </summary>
        /// <param name="status"></param>
        /// <param name="bankType"></param>
        /// <param name="bankCode"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult chkData(string status, string bankType, string bankCode)
        {
            errModel errModel = new errModel();
            errModel = chkAplyData(status, bankType, bankCode);

            if (errModel.chkResult)
                return Json(new { success = true });
            else
                return Json(new { success = false, err = errModel.msg });


            //FRTBBKMDao fRTBBKMDao = new FRTBBKMDao();
            //FRTBBKHDao fRTBBKHDao = new FRTBBKHDao();
            //List<ORTB002Model> dataO = new List<ORTB002Model>();

            //if ("A".Equals(status))
            //{
            //    dataO = fRTBBKMDao.qryForORTB002(bankCode, "");

            //    if (dataO.Count > 0)
            //        return Json(new { success = false, err = "此筆資料已存在「快速付款銀行類型資料檔」不可新增!!" });
            //}

            //dataO = fRTBBKHDao.qryForSTAT1(bankCode);
            //if (dataO.Count > 0)
            //    return Json(new { success = false, err = "資料覆核中，不可修改/刪除此筆資料!!" });


            //return Json(new { success = true });
        }


        private errModel chkAplyData(string status, string bankType, string bankCode)
        {
            FRTBBKMDao fRTBBKMDao = new FRTBBKMDao();
            FRTBBKHDao fRTBBKHDao = new FRTBBKHDao();
            List<ORTB002Model> dataO = new List<ORTB002Model>();

            errModel errModel = new errModel();

            if ("A".Equals(status))
            {
                dataO = fRTBBKMDao.qryForORTB002(bankCode, "");

                if (dataO.Count > 0)
                {
                    errModel.chkResult = false;
                    errModel.msg = "此筆資料已存在「快速付款銀行類型資料檔」不可新增!!";
                    return errModel;
                }


            }

            dataO = fRTBBKHDao.qryForSTAT1(bankCode);
            if (dataO.Count > 0)
            {
                errModel.chkResult = false;
                errModel.msg = "資料覆核中，不可修改/刪除此筆資料!!";
                return errModel;
            }


            errModel.chkResult = true;
            errModel.msg = "";
            return errModel;
        }


        internal class errModel
        {
            public bool chkResult { get; set; }
            public string msg { get; set; }
        }

    }
}