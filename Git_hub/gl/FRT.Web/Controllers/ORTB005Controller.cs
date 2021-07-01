using FRT.Web.ActionFilter;
using FRT.Web.AS400Models;
using FRT.Web.AS400PGM;
using FRT.Web.BO;
using FRT.Web.Daos;
using FRT.Web.ViewModels;
using Microsoft.Reporting.WebForms;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.EasycomClient;
using System.Web.Mvc;

/// <summary>
/// 功能說明：櫃匯及快速付款出款資料查詢及列印
/// 初版作者：20180723 Daiyu
/// 修改歷程：20180723 Daiyu
///           需求單號：
///           初版
/// </summary>
///

namespace FRT.Web.Controllers
{
    
    [Authorize]
    [CheckSessionFilterAttribute]
    public class ORTB005Controller : BaseController
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
                String[] roleInfo = authUtil.chkUserFuncAuth(Session["UserID"].ToString(), "~/ORTB005/");
                if (roleInfo != null && roleInfo.Length == 2)
                {
                    opScope = roleInfo[0];
                    funcName = roleInfo[1];
                }
            }

            ViewBag.opScope = opScope;
            ViewBag.funcName = funcName;

            SysCodeDao sysCodeDao = new SysCodeDao();
            //類型
            var paidTypeList = sysCodeDao.loadSelectList("RT", "PAID_TYPE", true);
            ViewBag.paidTypeList = paidTypeList;
            ViewBag.paidTypejqList = sysCodeDao.jqGridList("RT", "PAID_TYPE", true);

            return View();
        }
        



        /// <summary>
        /// 依畫面條件查詢COPL
        /// </summary>
        /// <param name="field081B"></param>
        /// <param name="field081E"></param>
        /// <param name="paidType"></param>
        /// <param name="corpNo"></param>
        /// <param name="currency"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult LoadData(string field081B, string field081E, string paidType, string corpNo, string currency, string sqlNo, string vhrNo1)
        {
            logger.Info("LoadData begin!!");

            try
            {
                FRTCOPLDao fRTCOPLDao = new FRTCOPLDao();
                List<ORTB005Model> rows = fRTCOPLDao.qryForORTB005Summary(field081B, field081E, paidType, corpNo, currency, sqlNo, vhrNo1);

                var jsonData = new { success = true, rows };
                return Json(jsonData, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e) {
                return Json(new { success = false, err = "其它錯誤，請洽系統管理員!!" }, JsonRequestBehavior.AllowGet);
            }

            
        }


        /// <summary>
        /// 畫面執行"列印"功能
        /// </summary>
        /// <param name="cRptType"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Print(string cRptType, string field081B, string field081E, string paidType, string corpNo, string currency, string sqlNo, string vhrNo1)
        {
  
            switch (cRptType)
            {
                case "S":
                    return printS(field081B, field081E, paidType, corpNo, currency, sqlNo, vhrNo1);
                case "D":
                    return printD(field081B, field081E, paidType, corpNo, currency,sqlNo , vhrNo1);
                default:
                    var jsonData = new { success = true, err = "報表類型錯誤!!"};
                    return Json(jsonData, JsonRequestBehavior.AllowGet);
            }

        }


        /// <summary>
        /// 處理彙總表
        /// </summary>
        /// <param name="field081B"></param>
        /// <param name="field081E"></param>
        /// <param name="paidType"></param>
        /// <param name="corpNo"></param>
        /// <param name="currency"></param>
        /// <returns></returns>
        private ActionResult printS(string field081B, string field081E, string paidType, string corpNo, string currency, string sqlNo, string vhrNo1)
        {
            FRTCOPLDao fRTCOPLDao = new FRTCOPLDao();
            List<ORTB005Model> rows = fRTCOPLDao.qryForORTB005Summary(field081B, field081E, paidType, corpNo, currency, sqlNo, vhrNo1);
            rows = ScallSAAEMPY(rows);

            ReportWrapper rw = new ReportWrapper();

            rw.ReportId = "ORTB005P1";
            rw.ReportPath = Server.MapPath($"~/Report/Rdlc/ORTB005P1.rdlc");
            

            CommonUtil commonUtil = new CommonUtil();
            DataTable dtMain = commonUtil.ConvertToDataTable<ORTB005Model>(rows);

            rw.ReportDataSources=dtMain;
            

            String guid = Guid.NewGuid().ToString();

            Session[guid] = rw;

            var jsonData = new { success = true, guid = guid };
            return Json(jsonData, JsonRequestBehavior.AllowGet);

        }


        /// <summary>
        /// 處理明細表
        /// </summary>
        /// <param name="field081B"></param>
        /// <param name="field081E"></param>
        /// <param name="paidType"></param>
        /// <param name="corpNo"></param>
        /// <param name="currency"></param>
        /// <returns></returns>
        private ActionResult printD(string field081B, string field081E, string paidType, string corpNo, string currency,string sqlNo, string vhrNo1)
        {
            FRTCOPLDao fRTCOPLDao = new FRTCOPLDao();
            List<FRTCOPL0Model> rows = fRTCOPLDao.qryForORTB005Detail(field081B, field081E, paidType, corpNo, currency, sqlNo, vhrNo1);
            rows = DcallSAAEMPY(rows);

            ReportWrapper rw = new ReportWrapper();

            rw.ReportId = "ORTB005P2";
            rw.ReportPath = Server.MapPath($"~/Report/Rdlc/ORTB005P2.rdlc");


            CommonUtil commonUtil = new CommonUtil();
            DataTable dtMain = commonUtil.ConvertToDataTable<FRTCOPL0Model>(rows);

            rw.ReportDataSources = dtMain;


            String guid = Guid.NewGuid().ToString();

            Session[guid] = rw;

            var jsonData = new { success = true, guid = guid };
            return Json(jsonData, JsonRequestBehavior.AllowGet);

        }

        private List<ORTB005Model> ScallSAAEMPY(List<ORTB005Model> rows)
        {
            Dictionary<string, SRTB0010Model> userMap = new Dictionary<string, SRTB0010Model>();
            SRTB0010Util sRTB0010Util = new SRTB0010Util();

            EacConnection con = new EacConnection();
            EacCommand cmd = new EacCommand();
            try
            {
                con.ConnectionString = CommonUtil.GetEasycomConn();
                con.Open();
                cmd.Connection = con;

                foreach (ORTB005Model d in rows)
                {
                    if (!userMap.ContainsKey(d.acptId))
                        userMap = sRTB0010Util.callSRTB0010(con, cmd, d.acptId, userMap);

                    string empName = StringUtil.toString(userMap[d.acptId].empName);
                    if ("".Equals(empName))
                        empName = d.acptId.Length == 10 ? MaskUtil.maskId(d.acptId) : d.acptId;

                    d.acptId = empName;
                }



                cmd.Dispose();
                cmd = null;
                con.Close();
                con = null;
            }
            catch (Exception e)
            {
                cmd.Dispose();
                cmd = null;
                con.Close();
                con = null;
                logger.Error("[callEasycom]其它錯誤：" + e.ToString());
                throw e;
            }

            return rows;
        }



        /// <summary>
        /// 呼叫"SRTB0010"取得人員資料
        /// </summary>
        /// <param name="rows"></param>
        /// <returns></returns>
        private List<FRTCOPL0Model>  DcallSAAEMPY(List<FRTCOPL0Model> rows) {
            Dictionary<string, SRTB0010Model> userMap = new Dictionary<string, SRTB0010Model>();
            SRTB0010Util sRTB0010Util = new SRTB0010Util();

            EacConnection con = new EacConnection();
            EacCommand cmd = new EacCommand();
            try
            {
                con.ConnectionString = CommonUtil.GetEasycomConn();
                con.Open();
                cmd.Connection = con;

                foreach (FRTCOPL0Model d in rows)
                {
                    if (!userMap.ContainsKey(d.acptId))
                        userMap = sRTB0010Util.callSRTB0010(con, cmd, d.acptId, userMap);

                    d.acptId = StringUtil.toString(userMap[d.acptId].empName);
                }


                foreach (FRTCOPL0Model d in rows)
                {
                    if (!userMap.ContainsKey(d.genId))
                    {
                        if (!userMap.ContainsKey(d.genId))
                            userMap = sRTB0010Util.callSRTB0010(con, cmd, d.genId, userMap);
                    }
                    d.genUnit = StringUtil.toString(userMap[d.genId].empUnit);
                }


                cmd.Dispose();
                cmd = null;
                con.Close();
                con = null;
            }
            catch (Exception e)
            {
                cmd.Dispose();
                cmd = null;
                con.Close();
                con = null;
                logger.Error("[callEasycom]其它錯誤：" + e.ToString());
                throw e;
            }

            return rows;
        }

    }
}