using FRT.Web.ActionFilter;
using FRT.Web.AS400Models;
using FRT.Web.BO;
using FRT.Web.Daos;
using FRT.Web.ViewModels;
using System;
using System.Collections.Generic;
using System.Web.Mvc;

/// <summary>
/// 功能說明：電文類別及時間控制維護作業
/// 初版作者：20180702 Daiyu
/// 修改歷程：20180702 Daiyu
///           需求單號：
///           初版
/// 修改歷程：20181130 Daiyu
///           需求單號：
///           快速付款II 電文類別及時間控制維護作業FRTBTMM0 鍵項修改
/// </summary>
///

namespace FRT.Web.Controllers
{
    [Authorize]
    [CheckSessionFilterAttribute]
    public class ORTB003Controller : BaseController
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
                String[] roleInfo = authUtil.chkUserFuncAuth(Session["UserID"].ToString(), "~/ORTB003/");
                if (roleInfo != null && roleInfo.Length == 2)
                {
                    opScope = roleInfo[0];
                    funcName = roleInfo[1];
                }
            }

            ViewBag.opScope = opScope;
            ViewBag.funcName = funcName;


            SysCodeDao sysCodeDao = new SysCodeDao();
            //電文類型
            var textTypeList = sysCodeDao.loadSelectList("RT", "TEXT_TYPE", true);
            ViewBag.textTypeList = textTypeList;
            ViewBag.textTypejqList = sysCodeDao.jqGridList("RT", "TEXT_TYPE", true);

            //資料狀態
            ViewBag.statusjqList = sysCodeDao.jqGridList("RT", "STATUS", true);

            return View();
        }



        /// <summary>
        /// 查詢"FRTBTMM 電文類別及時間控制檔"
        /// </summary>
        /// <param name="bankType"></param>
        /// <param name="textType"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult qryFRTBTMM(string bankType, string textType)
        {
            logger.Info("qryFRTBTMM begin!!");

            try
            {
                FRTBTMMDao fRTBTMMDao = new FRTBTMMDao();
                List<ORTB003Model> rows = fRTBTMMDao.qryForORTB003(bankType, textType);


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
        public JsonResult execSave(string bankType, string textType, List<ORTB003Model> gridData)
        {
            logger.Info("execSave begin");

            string errStr = "";
            bool bChg = false;

            FRTBTMMDao fRTBTMMDao = new FRTBTMMDao();
            List<ORTB003Model> dataList = new List<ORTB003Model>();


            foreach (ORTB003Model d in gridData)
            {
                if (!"".Equals(StringUtil.toString(d.status)))
                {
                    errModel errModel = chkAplyData(d.tempId, d.status, d.bankType, d.textType, d.strTime, d.endTime);

                    if (errModel.chkResult)
                    {
                        bChg = true;
                        d.updId = Session["UserID"].ToString();
                        dataList.Add(d);
                    }
                    else
                    {
                        errStr += "銀行類型：" + d.bankType 
                            + " 電文類型：" + d.textType
                             + " 開始時間：" + d.strTime
                              + " 結束時間：" + d.endTime 
                              +" 錯誤原因：" + errModel.msg + "<br/>";
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
            FRTBTMHDao fRTBTMHDao = new FRTBTMHDao();

            string[] curDateTime = DateUtil.getCurChtDateTime(4).Split(' ');
            //取得流水號
            SysSeqDao sysSeqDao = new SysSeqDao();
            String qPreCode = curDateTime[0];
            var cId = sysSeqDao.qrySeqNo("RT", "B003", qPreCode).ToString();


            fRTBTMHDao.insertFRTBTMH0(qPreCode + cId.ToString().PadLeft(3, '0'), dataList);

            return Json(new { success = true, aplyNo = qPreCode + cId.ToString().PadLeft(3, '0'), err = errStr });

        }



        [HttpPost]
        public JsonResult chkData(string status, string bankType, string textType, string strTime, string endTime)
        {
            errModel errModel = new errModel();
            errModel = chkAplyData("", status, bankType, textType, strTime, endTime);

            if (errModel.chkResult)
                return Json(new { success = true });
            else
                return Json(new { success = false, err = errModel.msg });


            //FRTBTMMDao fRTBTMMDao = new FRTBTMMDao();
            //FRTBTMHDao fRTBTMHDao = new FRTBTMHDao();
            //List<ORTB003Model> dataO = new List<ORTB003Model>();

            //if ("A".Equals(status))
            //{
            //    dataO = fRTBTMMDao.qryForORTB003(bankType, textType);

            //    if (dataO.Count > 0)
            //        return Json(new { success = false, err = "此筆資料已存在「電文類別及時間控制資料檔」不可新增!!" });
            //}

            //dataO = fRTBTMHDao.qryForSTAT1(bankType, textType);
            //if (dataO.Count > 0)
            //    return Json(new { success = false, err = "資料覆核中，不可修改/刪除此筆資料!!" });


            //return Json(new { success = true });
        }

        private errModel chkAplyData(string tempId, string status, string bankType, string textType, string strTime, string endTime)
        {
            FRTBTMMDao fRTBTMMDao = new FRTBTMMDao();
            FRTBTMHDao fRTBTMHDao = new FRTBTMHDao();
            List<ORTB003Model> dataO = new List<ORTB003Model>();

            errModel errModel = new errModel();
            //modify by daiyu 20181130 begin
            //if ("A".Equals(status) || "U".Equals(status))
            //{
                
            //    //dataO = fRTBTMMDao.qryForORTB003(bankType, textType);
            //    dataO = fRTBTMMDao.qryForORTB003(bankType, "");

            //    foreach (ORTB003Model d in dataO) {
            //        if ((strTime.CompareTo(d.strTime) <= 0 && endTime.CompareTo(d.strTime) >= 0)
            //            || (strTime.CompareTo(d.strTime) >= 0 && strTime.CompareTo(d.endTime) <= 0))
            //        {
            //            if ("A".Equals(status))
            //            {
            //                errModel.chkResult = false;
            //                errModel.msg = "此筆資料與「電文類別及時間控制資料檔」時間區間 " + d.strTime + "~" + d.endTime + " 重疊，不可新增!!";
            //                return errModel;
            //            }
            //            else
            //            {
            //                var temp = StringUtil.toString(tempId).Split('-');
            //                if (temp.Length == 4)
            //                {
            //                    string bankTypeO = temp[0];
            //                    string textTypeO = temp[1];
            //                    string strTimeO = temp[2];
            //                    string endTimeO = temp[3];

            //                    if (!bankTypeO.Equals(d.bankType)
            //                        || !textTypeO.Equals(d.textType)
            //                        || !strTimeO.Equals(d.strTime)
            //                        || !endTimeO.Equals(d.endTime))
            //                    {
            //                        if (!(bankType.Equals(d.bankType) && textType.Equals(d.textType)
            //                && strTime.Equals(d.strTime) && endTime.Equals(d.endTime)))
            //                        {
            //                            errModel.chkResult = false;
            //                            errModel.msg = "此筆資料與「電文類別及時間控制資料檔」時間區間 " + d.strTime + "~" + d.endTime + " 重疊，不可修改!!";
            //                            return errModel;

            //                        }
            //                    }

                                   
            //                }


            //            }

            //        }
            //    }

            //    //if (dataO.Count > 0)
            //    //{
            //    //    errModel.chkResult = false;
            //    //    errModel.msg = "此筆資料已存在「電文類別及時間控制資料檔」不可新增!!";
            //    //    return errModel;
            //    //}


            //}

            //dataO = fRTBTMHDao.qryForSTAT1(bankType, textType);
            dataO = fRTBTMHDao.qryForSTAT1(bankType, "");
            foreach (ORTB003Model d in dataO)
            {
                if ((strTime.CompareTo(d.strTime) <= 0 && endTime.CompareTo(d.strTime) >= 0)
                    || (strTime.CompareTo(d.strTime) >= 0 && strTime.CompareTo(d.endTime) <= 0))
                {
                    errModel.chkResult = false;
                    errModel.msg = "此筆資料與「覆核中資料」時間區間 " + d.strTime + "~" + d.endTime + " 重疊，不可修改/刪除此筆資料";
                    return errModel;
                }
            }
            //if (dataO.Count > 0)
            //{
            //    errModel.chkResult = false;
            //    errModel.msg = "資料覆核中，不可修改/刪除此筆資料!!";
            //    return errModel;
            //}

            //end modify 20181130

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