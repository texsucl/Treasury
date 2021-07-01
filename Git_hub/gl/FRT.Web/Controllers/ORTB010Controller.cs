using FRT.Web.ActionFilter;
using FRT.Web.AS400Models;
using FRT.Web.BO;
using FRT.Web.Daos;
using FRT.Web.Models;
using FRT.Web.ViewModels;
using System;
using System.Collections.Generic;
using System.Web.Mvc;

/// <summary>
/// 功能說明：發查時間設定、水位設定作業
/// 初版作者：20180906 Daiyu
/// 修改歷程：20180906 Daiyu
///           需求單號：
///           初版
/// </summary>
///

namespace FRT.Web.Controllers
{
    [Authorize]
    [CheckSessionFilterAttribute]
    public class ORTB010Controller : BaseController
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
                String[] roleInfo = authUtil.chkUserFuncAuth(Session["UserID"].ToString(), "~/ORTB010/");
                if (roleInfo != null && roleInfo.Length == 2)
                {
                    opScope = roleInfo[0];
                    funcName = roleInfo[1];
                }
            }

            ViewBag.opScope = opScope;
            ViewBag.funcName = funcName;

            SysCodeDao sysCodeDao = new SysCodeDao();
            //快速付款設定項目
            var fastParaList = sysCodeDao.loadSelectList("RT", "FAST_PARA", false);
            ViewBag.fastParaList = fastParaList;


            return View();
        }



        /// <summary>
        /// 查詢設定項目
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult LoadData(string code)
        {
            logger.Info("LoadData begin!!");

            try
            {
                SysParaDao sysParaDao = new SysParaDao();
                List<ORTB010Model> rows = sysParaDao.qryForORTB010("RT", code, "Y");

                if ("FAST_API".Equals(code) && rows.Count > 0) {
                    FRTCODEDao fRTCODEDao = new FRTCODEDao();

                    rows[0] = fRTCODEDao.qryForORTB010(rows[0]);

                }

                var jsonData = new { success = true, dataList = rows };
                return Json(jsonData, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e) {
                return Json(new { success = false, err = "其它錯誤，請洽系統管理員!!" }, JsonRequestBehavior.AllowGet);
            }

            
        }


        ///// <summary>
        ///// 快速付款通道開啟(啟用)/關閉
        ///// </summary>
        ///// <param name="refNo"></param>
        ///// <returns></returns>
        //public JsonResult updateFBApiStat(string refNo) {
        //    try
        //    {
        //        FRTCODEDao FRTCODEDao = new FRTCODEDao();
        //        FRTCODEDao.updateFBApi(refNo);
        //    }
        //    catch (Exception e) {
        //        logger.Error(e.ToString());
        //        return Json(new { success = false, err = "系統發生錯誤，請洽系統管理員!!" }, JsonRequestBehavior.AllowGet);

        //    }

        //    return Json(new { success = true }, JsonRequestBehavior.AllowGet);
        //}


        /// <summary>
        /// 畫面執行"申請覆核"
        /// </summary>
        /// <param name="code"></param>
        /// <param name="gridData"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult execSave(string code, List<ORTB010Model> gridData)
        {
            logger.Info("execSave begin");

            try
            {
                //查詢是否有待覆核的資料
                SysParaHisDao sysParaHisDao = new SysParaHisDao();
                List<SYS_PARA_HIS> onApprList = sysParaHisDao.qryForGrpId("RT", code, "1");
                if (onApprList.Count > 0)
                    return Json(new { success = false, err = "此設定資料已在覆核中，將不進行修改覆核作業!!" }, JsonRequestBehavior.AllowGet);


                /*------------------ DB處理   begin------------------*/

                string[] curDateTime = DateUtil.getCurChtDateTime(4).Split(' ');
                //取得流水號
                SysSeqDao sysSeqDao = new SysSeqDao();
                String qPreCode = curDateTime[0];
                var cId = sysSeqDao.qrySeqNo("RT", "B010", qPreCode).ToString();

                List<SYS_PARA_HIS> insertData = new List<SYS_PARA_HIS>();
                foreach (ORTB010Model d in gridData) {
                    SYS_PARA_HIS hisD = new SYS_PARA_HIS();
                    hisD.APLY_NO = qPreCode + cId.ToString().PadLeft(3, '0');
                    hisD.SYS_CD = "RT";
                    hisD.GRP_ID = code;
                    hisD.PARA_ID = d.paraId;
                    hisD.PARA_VALUE = d.paraValue;
                    hisD.REMARK = d.remark;
                    hisD.RESERVE1 = d.reserve1;
                    hisD.RESERVE2 = d.reserve2;
                    hisD.RESERVE3 = d.reserve3;
                    hisD.APPR_STATUS = "1";
                    hisD.CREATE_UID = Session["UserID"].ToString();
                    insertData.Add(hisD);
                }

                sysParaHisDao.insert(insertData);

                return Json(new { success = true, aplyNo = qPreCode + cId.ToString().PadLeft(3, '0') });
            }
            catch (Exception e) {
                logger.Error(e.ToString());
                return Json(new { success = false, err = "系統發生錯誤，請洽系統管理員!!" }, JsonRequestBehavior.AllowGet);

            }


        }
        
    }
}