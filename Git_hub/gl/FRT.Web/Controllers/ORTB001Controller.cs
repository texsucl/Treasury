using FRT.Web.ActionFilter;
using FRT.Web.BO;
using FRT.Web.Daos;
using FRT.Web.ViewModels;
using System;
using System.Collections.Generic;
using System.Web.Mvc;


/// <summary>
/// 功能說明：判斷快速付款維護作業
/// 初版作者：20180615 Daiyu
/// 修改歷程：20180615 Daiyu
///           需求單號：
///           初版
/// </summary>
///

namespace FRT.Web.Controllers
{
    [Authorize]
    [CheckSessionFilterAttribute]
    public class ORTB001Controller : BaseController
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
                String[] roleInfo = authUtil.chkUserFuncAuth(Session["UserID"].ToString(), "~/ORTB001/");
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
        /// 查詢"FRTBPGM 判斷快速付款維護"資料
        /// </summary>
        /// <param name="sysType"></param>
        /// <param name="srceFrom"></param>
        /// <param name="srceKind"></param>
        /// <param name="srcePgm"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult qryFRTBPGM(string sysType, string srceFrom, string srceKind, string srcePgm)
        {
            logger.Info("qryFRTBPGM begin!!");
            try
            {
                FRTBPGMDao fRTBPGMDao = new FRTBPGMDao();
                List<ORTB001Model> rows = fRTBPGMDao.qryForORTB001(sysType, srceFrom, srceKind, srcePgm);

                var jsonData = new { success = true, dataList = rows };
                return Json(jsonData, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e) {
                return Json(new { success = false, err = "其它錯誤，請洽系統管理員!!" });
            }
            
        }



        /// <summary>
        /// 畫面執行"申請覆核"
        /// </summary>
        /// <param name="sysType"></param>
        /// <param name="srceFrom"></param>
        /// <param name="srceKind"></param>
        /// <param name="srcePgm"></param>
        /// <param name="gridData"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult execSave(string sysType, string srceFrom, string srceKind, string srcePgm, List<ORTB001Model> gridData)
        {
            logger.Info("execSave begin");

            string errStr = "";
            try {
                bool bChg = false;

                FRTBPGMDao fRTBPGMDao = new FRTBPGMDao();
                List<ORTB001Model> dataList = new List<ORTB001Model>();

                
                foreach (ORTB001Model d in gridData)
                {
                    if (!"".Equals(StringUtil.toString(d.status))) {

                        errModel errModel = chkAplyData(d.status, d.sysType, d.srceFrom, d.srceKind, d.srcePgm);
                        if (errModel.chkResult)
                        {
                            bChg = true;
                            d.updId = Session["UserID"].ToString();
                            dataList.Add(d);
                        }
                        else {
                            errStr += "系統別：" + d.sysType + " 資料來源：" + d.srceFrom
                                + " 資料類別：" + d.srceKind + " 來源程式：" + d.srcePgm + "<br/>";
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
                FRTBPGHDao fRTBPGHDao = new FRTBPGHDao();

                string[] curDateTime = DateUtil.getCurChtDateTime(4).Split(' ');
                //取得流水號
                SysSeqDao sysSeqDao = new SysSeqDao();
                String qPreCode = curDateTime[0];
                var cId = sysSeqDao.qrySeqNo("RT", "B001", qPreCode).ToString();


                fRTBPGHDao.insertFRTBPGH0(qPreCode + cId.ToString().PadLeft(3, '0'), dataList);

                return Json(new { success = true, aplyNo = qPreCode + cId.ToString().PadLeft(3, '0'), err = errStr });

            }
            catch (Exception e)
            {
                logger.Error(e.ToString());
                return Json(new { success = false, err = "其它錯誤，請洽系統管理員!!" });

            }
        }



        /// <summary>
        /// 畫面GRID在儲存前，需先檢查可存檔
        /// </summary>
        /// <param name="status"></param>
        /// <param name="sysType"></param>
        /// <param name="srceFrom"></param>
        /// <param name="srceKind"></param>
        /// <param name="srcePgm"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult chkData(string status, string sysType, string srceFrom, string srceKind, string srcePgm)
        {
            errModel errModel = new errModel();
            errModel = chkAplyData(status, sysType, srceFrom, srceKind, srcePgm);

            if(errModel.chkResult)
                return Json(new { success = true });
            else
                return Json(new { success = false, err = errModel.msg });

            //FRTBPGMDao fRTBPGMDao = new FRTBPGMDao();
            //FRTBPGHDao fRTBPGHDao = new FRTBPGHDao();
            //List<ORTB001Model> dataO = new List<ORTB001Model>();

            //if ("A".Equals(status)) {
            //    dataO = fRTBPGMDao.qryForORTB001(sysType, srceFrom, srceKind, srcePgm);

            //    if (dataO.Count > 0) 
            //        return Json(new { success = false, err = "此筆資料已存在「快速付款來源程式資料檔」不可新增!!" });
            //}

            //dataO = fRTBPGHDao.qryForSTAT1(sysType, srceFrom, srceKind, srcePgm);
            //if (dataO.Count > 0)
            //    return Json(new { success = false, err = "資料覆核中，不可修改/刪除此筆資料!!" });


            //return Json(new { success = true });
        }


        private errModel chkAplyData(string status, string sysType, string srceFrom, string srceKind, string srcePgm)
        {
            FRTBPGMDao fRTBPGMDao = new FRTBPGMDao();
            FRTBPGHDao fRTBPGHDao = new FRTBPGHDao();
            List<ORTB001Model> dataO = new List<ORTB001Model>();

            errModel errModel = new errModel();

            if ("A".Equals(status))
            {
                dataO = fRTBPGMDao.qryForORTB001(sysType, srceFrom, srceKind, srcePgm);

                if (dataO.Count > 0) {
                    errModel.chkResult = false;
                    errModel.msg = "此筆資料已存在「快速付款來源程式資料檔」不可新增!!";
                    return errModel;
                }
            }

            dataO = fRTBPGHDao.qryForSTAT1(sysType, srceFrom, srceKind, srcePgm);
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