using FRT.Web.ActionFilter;
using FRT.Web.AS400Models;
using FRT.Web.BO;
using FRT.Web.Daos;
using FRT.Web.ViewModels;
using System;
using System.Collections.Generic;
using System.Web.Mvc;

/// <summary>
/// 功能說明：快速付款匯款失敗原因對照TABLE檔維護作業
/// 初版作者：20180706 Daiyu
/// 修改歷程：20180706 Daiyu
///           需求單號：
///           初版
/// </summary>
///

namespace FRT.Web.Controllers
{
    [Authorize]
    [CheckSessionFilterAttribute]
    public class ORTB004Controller : BaseController
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
                String[] roleInfo = authUtil.chkUserFuncAuth(Session["UserID"].ToString(), "~/ORTB004/");
                if (roleInfo != null && roleInfo.Length == 2)
                {
                    opScope = roleInfo[0];
                    funcName = roleInfo[1];
                }
            }

            ViewBag.opScope = opScope;
            ViewBag.funcName = funcName;

            SysCodeDao sysCodeDao = new SysCodeDao();
            //錯誤歸屬
            var errBelongList = sysCodeDao.loadSelectList("RT", "ERR_BELONG", true);
            ViewBag.errBelongList = errBelongList;
            ViewBag.errBelongjqList = sysCodeDao.jqGridList("RT", "ERR_BELONG", true);

            //資料狀態
            ViewBag.statusjqList = sysCodeDao.jqGridList("RT", "STATUS", true);

            return View();
        }



        /// <summary>
        /// 查詢"FRTBERM 快速付款匯款失敗原因對照檔"
        /// </summary>
        /// <param name="errCode"></param>
        /// <param name="errBelong"></param>
        /// <param name="transCode"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult qryFRTBERM(string errCode, string errBelong, string transCode)
        {
            logger.Info("qryFRTBERM begin!!");

            try
            {
                FRTBERMDao fRTBERMDao = new FRTBERMDao();
                List<ORTB004Model> rows = fRTBERMDao.qryForORTB004(errCode, errBelong, transCode);


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
        /// <param name="gridData"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult execSave(List<ORTB004Model> gridData)
        {
            logger.Info("execSave begin");
            string errStr = "";
            bool bChg = false;

            FRTBERMDao fRTBERMDao = new FRTBERMDao();
            List<ORTB004Model> dataList = new List<ORTB004Model>();

            foreach (ORTB004Model d in gridData)
            {
                if (!"".Equals(StringUtil.toString(d.status)))
                {
                    errModel errModel = chkAplyData(d.status, d.errCode);

                    if (errModel.chkResult)
                    {
                        bChg = true;
                        d.updId = Session["UserID"].ToString();
                        dataList.Add(d);
                    }
                    else
                    {
                        errStr += "錯誤代碼：" + d.errCode + "<br/>";
                    }
                }
            }

            if (bChg == false)
            {
                if ("".Equals(errStr))
                    return Json(new { success = false, err = "未異動畫面資料，將不進行修改覆核作業!!" }, JsonRequestBehavior.AllowGet);
                else
                    return Json(new { success = true, err = errStr });
            }


            /*------------------ DB處理   begin------------------*/
            FRTBERHDao fRTBERHDao = new FRTBERHDao();

            string[] curDateTime = DateUtil.getCurChtDateTime(4).Split(' ');
            //取得流水號
            SysSeqDao sysSeqDao = new SysSeqDao();
            String qPreCode = curDateTime[0];
            var cId = sysSeqDao.qrySeqNo("RT", "B004", qPreCode).ToString();


            fRTBERHDao.insertFRTBERH0(qPreCode + cId.ToString().PadLeft(3, '0'), dataList);

            return Json(new { success = true, aplyNo = qPreCode + cId.ToString().PadLeft(3, '0'), err = errStr });

        }


        /// <summary>
        /// 畫面GRID執行"儲存"時，要先檢查是否可以存檔，若有以下狀況，不可儲存
        /// 1.新增資料:已存在主檔
        /// 2.新增、修改、刪除:異動檔中，不可存在同樣errcode的未覆核資料
        /// </summary>
        /// <param name="status"></param>
        /// <param name="errCode"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult chkData(string status, string errCode)
        {
            errModel errModel = new errModel();
            errModel = chkAplyData(status, errCode);

            if (errModel.chkResult)
                return Json(new { success = true });
            else
                return Json(new { success = false, err = errModel.msg });

            //try
            //{
            //    FRTBERMDao fRTBERMDao = new FRTBERMDao();
            //    FRTBERHDao fRTBERHDao = new FRTBERHDao();
            //    List<ORTB004Model> dataO = new List<ORTB004Model>();

            //    if ("A".Equals(status))
            //    {
            //        dataO = fRTBERMDao.qryForORTB004(errCode, "", "");

            //        if (dataO.Count > 0)
            //            return Json(new { success = false, err = "此筆資料已存在「快速付款匯款失敗原因對照檔」不可新增!!" });
            //    }

            //    dataO = fRTBERHDao.qryForSTAT1(errCode);
            //    if (dataO.Count > 0)
            //        return Json(new { success = false, err = "資料覆核中，不可修改/刪除此筆資料!!" });


            //    return Json(new { success = true });
            //}
            //catch (Exception e) {
            //    logger.Error(e.ToString());
            //    return Json(new { success = false, err = "其它錯誤，請洽系統管理員!!" }, JsonRequestBehavior.AllowGet);
            //}
            
        }


        private errModel chkAplyData(string status, string errCode)
        {
            FRTBERMDao fRTBERMDao = new FRTBERMDao();
            FRTBERHDao fRTBERHDao = new FRTBERHDao();
            List<ORTB004Model> dataO = new List<ORTB004Model>();

            errModel errModel = new errModel();

            if ("A".Equals(status))
            {
                dataO = fRTBERMDao.qryForORTB004(errCode, "", "");

                if (dataO.Count > 0)
                {
                    errModel.chkResult = false;
                    errModel.msg = "此筆資料已存在「快速付款匯款失敗原因對照檔」不可新增!!";
                    return errModel;
                }


            }

            dataO = fRTBERHDao.qryForSTAT1(errCode);
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


        /// <summary>
        /// 查詢轉碼代號
        /// </summary>
        /// <param name="transCode"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult qryPMCODE(string transCode)
        {
            try
            {
                FPMCODEDao fPMCODEDao = new FPMCODEDao();
                List<FPMCODEModel> fPMCODEModel = fPMCODEDao.qryFPMCODE("FAIL-CODE", "RT", transCode);

                string transCodeDesc = "";
                if (fPMCODEModel.Count > 0)
                    transCodeDesc = StringUtil.toString(fPMCODEModel[0].text);

                return Json(new { success = true, transCodeDesc = transCodeDesc });
            }
            catch (Exception e)
            {
                logger.Error(e.ToString());
                return Json(new { success = false, err = "其它錯誤，請洽系統管理員!!" }, JsonRequestBehavior.AllowGet);
            }
        }


    }
}