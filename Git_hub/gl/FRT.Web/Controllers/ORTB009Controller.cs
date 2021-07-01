using FRT.Web.ActionFilter;
using FRT.Web.AS400Models;
using FRT.Web.BO;
using FRT.Web.Daos;
using FRT.Web.Models;
using FRT.Web.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

/// <summary>
/// 功能說明：重新發送電文件業
/// 初版作者：20180830 Daiyu
/// 修改歷程：20180830 Daiyu
///           需求單號：
///           初版
/// 修改歷程：20190514 Mark
///           需求單號：201904100470
/// </summary>
///

namespace FRT.Web.Controllers
{
    [Authorize]
    [CheckSessionFilterAttribute]
    public class ORTB009Controller : BaseController
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
                String[] roleInfo = authUtil.chkUserFuncAuth(Session["UserID"].ToString(), "~/ORTB009/");
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
        /// 查詢"FRTBARM0  快速付款匯款申請檔"
        /// </summary>
        /// <param name="fastNo"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult qryFRTBARM(string fastNo)
        {
            logger.Info("qryFRTBARM begin!!");

            try
            {
                FRTBARHDao fRTBARHDao = new FRTBARHDao();
                List<FRTBARHModel> dataO = new List<FRTBARHModel>();

                dataO = fRTBARHDao.qryForChk(fastNo, "");

                if (dataO.Count > 0)
                {
                    return Json(new { success = false, err = "資料覆核中，不可進行電文重送作業!!" }, JsonRequestBehavior.AllowGet);
                }


                FRTBARMDao fRTBARMDao = new FRTBARMDao();
                List<ORTB009Model> rows = new List<ORTB009Model>();
                ORTB009Model data = fRTBARMDao.qryForORTB009(fastNo);
                if(!"".Equals(data.fastNo))
                    rows.Add(data);

                if (rows.Any())
                {
                    PiaLogMainDao piaLogMainDao = new PiaLogMainDao();
                    PIA_LOG_MAIN piaLogMain = new PIA_LOG_MAIN();
                    piaLogMain.TRACKING_TYPE = "A";
                    piaLogMain.ACCESS_ACCOUNT = Session["UserID"].ToString();
                    piaLogMain.ACCOUNT_NAME = "";
                    piaLogMain.PROGFUN_NAME = "ORTB009Controller";
                    piaLogMain.EXECUTION_CONTENT = $@"fastNo:{fastNo}";
                    piaLogMain.AFFECT_ROWS = rows.Count;
                    piaLogMain.PIA_TYPE = "0000000000";
                    piaLogMain.EXECUTION_TYPE = "Q";
                    piaLogMain.ACCESSOBJ_NAME = "FRTBARM";
                    piaLogMainDao.Insert(piaLogMain);
                }


                var jsonData = new { success = true, dataList = rows };
                return Json(jsonData, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e) {
                logger.Error(e.ToString());
                return Json(new { success = false, err = "其它錯誤，請洽系統管理員!!" }, JsonRequestBehavior.AllowGet);
            }
        }



        /// <summary>
        /// 畫面執行"申請覆核"
        /// </summary>
        /// <param name="gridData"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult execSave(List<ORTB009Model> fastNo)
        {
            logger.Info("execSave begin");

            try {
                List<string> errFastNo = new List<string>();

                FRTBARMDao fRTBARMDao = new FRTBARMDao();
                FRTBARHDao fRTBARHDao = new FRTBARHDao();
                List<FRTBARHModel> dataList = new List<FRTBARHModel>();

                string[] curDateTime = DateUtil.getCurChtDateTime(3).Split(' ');
                //取得流水號
                SysSeqDao sysSeqDao = new SysSeqDao();
                String qPreCode = "4" + curDateTime[0];
                var cId = sysSeqDao.qrySeqNo("RT", "B009", qPreCode).ToString();

                foreach (ORTB009Model d in fastNo)
                {
                    List<FRTBARHModel> dataO = new List<FRTBARHModel>();

                    dataO = fRTBARHDao.qryForChk(d.fastNo, "");

                    if (dataO.Count > 0)
                    {
                        return Json(new { success = false, err = "資料覆核中，不可修改/刪除此筆資料!!" }, JsonRequestBehavior.AllowGet);
                    }


                    FRTBARHModel barh = new FRTBARHModel();
                    barh.applyNo = cId;
                    barh.fastNo = d.fastNo;
                    barh.aplyType = "4";
                    barh.updId = Session["UserID"].ToString();
                    dataList.Add(barh);
                }


                /*------------------ DB處理   begin------------------*/

               


                fRTBARHDao.insertFRTBARH0(qPreCode + cId.ToString().PadLeft(7, '0'), dataList);

                return Json(new { success = true, aplyNo = qPreCode + cId.ToString().PadLeft(7, '0'), errFastNo = errFastNo });



            } catch (Exception e) {
                logger.Error(e.ToString());
                return Json(new { success = false, err = "其它錯誤，請洽系統管理員!!" }, JsonRequestBehavior.AllowGet);

            }

        }


        /// <summary>
        /// 畫面GRID執行"儲存"時，要先檢查是否可以存檔，若有以下狀況，不可儲存
        /// 異動檔中不可存在同樣"快速付款編號"的未覆核資料
        /// </summary>
        /// <param name="fastNo"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult chkData(string fastNo)
        {
            try
            {
                FRTBARHDao fRTBARHDao = new FRTBARHDao();
                List<FRTBARHModel> dataO = new List<FRTBARHModel>();

                dataO = fRTBARHDao.qryForChk(fastNo, "");
                if (dataO.Count > 0)
                    return Json(new { success = false, err = "資料覆核中，不可修改/刪除此筆資料!!" });


                return Json(new { success = true });
            }
            catch (Exception e) {
                logger.Error(e.ToString());
                return Json(new { success = false, err = "其它錯誤，請洽系統管理員!!" }, JsonRequestBehavior.AllowGet);
            }
            
        }
        


    }
}