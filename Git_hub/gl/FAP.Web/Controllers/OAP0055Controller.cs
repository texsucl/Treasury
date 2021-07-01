using ClosedXML.Excel;
using FAP.Web.ActionFilter;
using FAP.Web.AS400Models;
using FAP.Web.BO;
using FAP.Web.Daos;
using FAP.Web.Models;
using FAP.Web.ViewModels;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.EasycomClient;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Web.Mvc;

/// <summary>
/// 功能說明：戶政/電訪地址查詢
/// 初版作者：20200916 Daiyu
/// 修改歷程：20200916 Daiyu
/// 需求單號：202008120153-01
/// 修改內容：初版
/// ------------------------------------------
/// </summary>
///

namespace FAP.Web.Controllers
{
    [Authorize]
    [CheckSessionFilterAttribute]
    public class OAP0055Controller : BaseController
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
            string[] roleInfo = authUtil.chkUserFuncAuth(Session["UserID"].ToString(), "~/OAP0055/");
            if (roleInfo != null && roleInfo.Length == 2)
            {
                opScope = roleInfo[0];
                funcName = roleInfo[1];
            }

            ViewBag.funcName = funcName;
            ViewBag.opScope = opScope;


            SysCodeDao sysCodeDao = new SysCodeDao();


            return View();
        }


        [HttpPost]
        public JsonResult qryAddr(string paid_id)
        {
            logger.Info("qryAddr begin!!");
            try
            {
                SysCodeDao sysCodeDao = new SysCodeDao();
                //地址類別
                Dictionary<string, string> addrTypeMap = sysCodeDao.qryByTypeDic("AP", "ADDR_TYPE");

                FAPHouseholdAddrDao addrDao = new FAPHouseholdAddrDao();
                List<FAP_HOUSEHOLD_ADDR> addrList = addrDao.qryByPaidId(paid_id, "");

                List<OAP0055Model> rows = new List<OAP0055Model>();

                foreach (FAP_HOUSEHOLD_ADDR addr in addrList) {
                    if (!"".Equals(StringUtil.toString(addr.paid_id)))
                    {
                        OAP0055Model d = new OAP0055Model();
                        d.addr_type = addrTypeMap[addr.addr_type];
                        d.paid_id = addr.paid_id;
                        d.paid_name = addr.paid_name;
                        d.zip_code = addr.zip_code;
                        d.address = addr.address;
                        rows.Add(d);
                    }
                }

                writePiaLog(rows.Count, paid_id, "Q");

                var jsonData = new { success = true, dataList = rows };
                return Json(jsonData, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new { success = false, err = "其它錯誤，請洽系統管理員!!" });
            }

        }

        /// <summary>
        /// 匯出
        /// </summary>
        /// <param name="paid_id"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult execExport(string paid_id)
        {
            logger.Info("execExport begin!!");
            try
            {
                FAPHouseholdAddrDao addrDao = new FAPHouseholdAddrDao();
                List<FAP_HOUSEHOLD_ADDR> addrList = addrDao.qryByPaidId(paid_id, "");

                SysCodeDao sysCodeDao = new SysCodeDao();
                //地址類別
                Dictionary<string, string> addrTypeMap = sysCodeDao.qryByTypeDic("AP", "ADDR_TYPE");

                string guid = "";
                if (addrList.Count > 0)
                {
                    writePiaLog(addrList.Count, paid_id, "X" );

                    guid = Guid.NewGuid().ToString();


                    string fullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory + "Temp\\"
                            , string.Concat("OAP0055" + "_" + guid, ".xlsx"));
                    using (XLWorkbook wb = new XLWorkbook())
                    {
                        var ws = wb.Worksheets.Add("OAP0055");

                        ws.Cell(1, 1).Value = "地址類別";
                        ws.Cell(1, 2).Value = "給付對象 ID";
                        ws.Cell(1, 3).Value = "給付對象姓名";
                        ws.Cell(1, 4).Value = "郵遞區號";
                        ws.Cell(1, 5).Value = "地址";
                        ws.Cell(1, 6).Value = "日期";


                        ws.Range(1, 1, 1, 6).Style.Fill.BackgroundColor = XLColor.AirForceBlue;
                        ws.Range(1, 1, 1, 6).Style.Font.FontColor = XLColor.White;


                        int iRow = 2;

                        foreach (FAP_HOUSEHOLD_ADDR addr in addrList) {
                            ws.Cell(iRow, 1).Value = addrTypeMap[addr.addr_type];
                            ws.Cell(iRow, 2).Value = addr.paid_id;
                            ws.Cell(iRow, 3).Value = addr.paid_name;
                            ws.Cell(iRow, 4).Value = addr.zip_code;
                            ws.Cell(iRow, 5).Value = addr.address;
                            ws.Cell(iRow, 6).Value = BO.DateUtil.DatetimeToString(addr.update_datetime, "yyyy-MM-dd");

                            iRow++;
                        }
                     


                        ws.Columns().AdjustToContents();  // Adjust column width
                        ws.Rows().AdjustToContents();     // Adjust row heights


                        wb.SaveAs(fullPath);
                    }

                    var jsonData = new { success = true, guid = guid };
                    return Json(jsonData, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    var jsonData = new { success = false, err = "無資料" };
                    return Json(jsonData, JsonRequestBehavior.AllowGet);
                }


            }
            catch (Exception e)
            {
                return Json(new { success = false, err = "其它錯誤，請洽系統管理員!!" });
            }
        }


        public FileContentResult downloadRpt(string id)
        {
            byte[] fileBytes = System.IO.File.ReadAllBytes(Server.MapPath("~/Temp/") + "OAP0055" + "_" + id + ".xlsx");


            string fullPath = Server.MapPath("~/Temp/") + "OAP0055" + "_" + id + ".xlsx";
            if (System.IO.File.Exists(fullPath))
                System.IO.File.Delete(fullPath);


            return File(fileBytes, "application/vnd.ms-excel", "OAP0055.xlsx");
        }


        private void writePiaLog(int affectRows, string piaOwner, string executionType)
        {
            PIA_LOG_MAIN piaLogMain = new PIA_LOG_MAIN();
            piaLogMain.TRACKING_TYPE = "A";
            piaLogMain.ACCESS_ACCOUNT = Session["UserID"].ToString();
            piaLogMain.ACCOUNT_NAME = Session["UserName"].ToString();
            piaLogMain.PROGFUN_NAME = "OAP0055Controller";
            piaLogMain.EXECUTION_CONTENT = MaskUtil.maskId(piaOwner);
            piaLogMain.AFFECT_ROWS = affectRows;
            piaLogMain.PIA_TYPE = "0100100000";
            piaLogMain.EXECUTION_TYPE = executionType;
            piaLogMain.ACCESSOBJ_NAME = "FAPHouseholdAddr";
            piaLogMain.PIA_OWNER1 = piaOwner;
            PiaLogMainDao piaLogMainDao = new PiaLogMainDao();
            piaLogMainDao.Insert(piaLogMain);

        }

    }
}