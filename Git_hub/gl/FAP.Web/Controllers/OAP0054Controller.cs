using FAP.Web.ActionFilter;
using FAP.Web.BO;
using FAP.Web.Daos;
using FAP.Web.Models;
using FAP.Web.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Web.Mvc;
using System.Linq;
using System.IO;
using ClosedXML.Excel;

/// <summary>
/// 功能說明：OAP0054 不同清理狀態查詢
/// 初版作者：20200921 Daiyu
/// 修改歷程：20200921 Daiyu
/// 需求單號：202008120153-01
/// 修改內容：初版
/// ----------------------------------------------------
/// 修改歷程：20210125 daiyu 
/// 需求單號：
/// 修改內容：增加列印功能
/// </summary>
///

namespace FAP.Web.Controllers
{
    [Authorize]
    [CheckSessionFilterAttribute]
    public class OAP0054Controller : BaseController
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
                String[] roleInfo = authUtil.chkUserFuncAuth(Session["UserID"].ToString(), "~/OAP0054/");
                if (roleInfo != null && roleInfo.Length == 2)
                {
                    opScope = roleInfo[0];
                    funcName = roleInfo[1];
                }
            }

            ViewBag.opScope = opScope;
            ViewBag.funcName = funcName;

            SysCodeDao sysCodeDao = new SysCodeDao();

            //清理狀態
            ViewBag.clrStatusList = sysCodeDao.loadSelectList("AP", "CLR_STATUS", true);
            ViewBag.clrStatusjqList = sysCodeDao.jqGridList("AP", "CLR_STATUS", false);

            //給付性質
            FPMCODEDao pPMCODEDao = new FPMCODEDao();
            //ViewBag.oPaidCdjqList = sysCodeDao.jqGridList("AP", "O_PAID_CD", false);
            ViewBag.oPaidCdjqList = pPMCODEDao.jqGridList("PAID_CDTXT", "AP", false);

            return View();
        }



        /// <summary>
        /// 查詢【FAP_VE_TRACE 逾期未兌領清理記錄檔】
        /// </summary>
        /// <param name="status"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult qryCheck( string status, string page, string sidx, string sord, int rows)
        {
            logger.Info("qryProc begin!!");
            try
            {
                string[] status_arr = status.Split('|');
                List<OAP0054Model> checkList = new List<OAP0054Model>();

                //查詢【FAP_VE_TRACE 逾期未兌領清理記錄檔】
                FAPVeTraceDao fAPVeTraceDao = new FAPVeTraceDao();

                string strConn = DbUtil.GetDBFglConnStr();
                using (SqlConnection conn = new SqlConnection(strConn))
                {
                    conn.Open();
                    checkList = fAPVeTraceDao.qryForOAP0054(status_arr, conn);
                }

                int pageIndex = Convert.ToInt32(page) - 1;
                int totalRecords = checkList.Count();
                int totalPages = (int)Math.Ceiling((float)totalRecords / (float)rows);


               // writePiaLog(rows.Count, paid_id, "Q", status);

                var jsonData = new
                {
                    success = true,
                    total = totalPages,
                    page,
                    records = totalRecords,
                    rows = checkList.Skip(pageIndex * rows).Take(rows).ToList()
                };

                return Json(jsonData, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e) {
                return Json(new { success = false, err = "其它錯誤，請洽系統管理員!!" });
            }
        }



        [HttpPost]
        public JsonResult execExport(string status)
        {
            logger.Info("execExport begin!!");
            try
            {
                string[] status_arr = status.Split('|');
                List<OAP0054Model> checkList = new List<OAP0054Model>();

                //查詢【FAP_VE_TRACE 逾期未兌領清理記錄檔】
                FAPVeTraceDao fAPVeTraceDao = new FAPVeTraceDao();

                string strConn = DbUtil.GetDBFglConnStr();
                using (SqlConnection conn = new SqlConnection(strConn))
                {
                    conn.Open();
                    checkList = fAPVeTraceDao.qryForOAP0054(status_arr, conn);
                }


                SysCodeDao sysCodeDao = new SysCodeDao();
                //清理狀態
                Dictionary<string, string> clrStatusMap = sysCodeDao.qryByTypeDic("AP", "CLR_STATUS");

                //給付性質
                FPMCODEDao pPMCODEDao = new FPMCODEDao();
                Dictionary<string, string> oPaidCdMap = pPMCODEDao.qryByTypeDic("PAID_CDTXT", "AP", false); 



                string guid = "";
                if (checkList.Count > 0)
                {

                    guid = Guid.NewGuid().ToString();


                    string fullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory + "Temp\\"
                            , string.Concat("OAP0054" + "_" + guid, ".xlsx"));
                    using (XLWorkbook wb = new XLWorkbook())
                    {
                        var ws = wb.Worksheets.Add("OAP0054");

                        ws.Cell(1, 1).Value = "給付對象ID";
                        ws.Cell(1, 2).Value = "支票號碼";
                        ws.Cell(1, 3).Value = "支票帳號簡稱";
                        ws.Cell(1, 4).Value = "支票到期日";
                        ws.Cell(1, 5).Value = "支票金額";
                        ws.Cell(1, 6).Value = "給付性質";
                        ws.Cell(1, 7).Value = "大系統別";
                        ws.Cell(1, 8).Value = "清理狀態";


                        ws.Range(1, 1, 1, 8).Style.Fill.BackgroundColor = XLColor.AirForceBlue;
                        ws.Range(1, 1, 1, 8).Style.Font.FontColor = XLColor.White;


                        int iRow = 2;

                        foreach (OAP0054Model d in checkList)
                        {
                            ws.Cell(iRow, 1).Value = d.paid_id;
                            ws.Cell(iRow, 2).Value = d.check_no;
                            ws.Cell(iRow, 3).Value = d.check_acct_short;
                            ws.Cell(iRow, 4).Value = "'" + d.check_date;
                            ws.Cell(iRow, 5).Value = d.check_amt;

                            if (oPaidCdMap.ContainsKey(d.o_paid_cd))
                                ws.Cell(iRow, 6).Value = oPaidCdMap[d.o_paid_cd];
                            else
                                ws.Cell(iRow, 6).Value = d.o_paid_cd;

                            ws.Cell(iRow, 7).Value = d.system;

                            if (clrStatusMap.ContainsKey(d.status))
                                ws.Cell(iRow, 8).Value = clrStatusMap[d.status];
                            else
                                ws.Cell(iRow, 8).Value = d.status;

                            iRow++;
                        }

                        ws.Range(2, 5, iRow, 5).Style.NumberFormat.Format = "#,##0";

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
            byte[] fileBytes = System.IO.File.ReadAllBytes(Server.MapPath("~/Temp/") + "OAP0054" + "_" + id + ".xlsx");


            string fullPath = Server.MapPath("~/Temp/") + "OAP0054" + "_" + id + ".xlsx";
            if (System.IO.File.Exists(fullPath))
                System.IO.File.Delete(fullPath);


            return File(fileBytes, "application/vnd.ms-excel", "OAP0054.xlsx");
        }



        private void writePiaLog(int affectRows, string piaOwner, string executionType, string content)
        {
            PIA_LOG_MAIN piaLogMain = new PIA_LOG_MAIN();
            piaLogMain.TRACKING_TYPE = "A";
            piaLogMain.ACCESS_ACCOUNT = Session["UserID"].ToString();
            piaLogMain.ACCOUNT_NAME = Session["UserName"].ToString();
            piaLogMain.PROGFUN_NAME = "OAP0054Controller";
            piaLogMain.EXECUTION_CONTENT = content;
            piaLogMain.AFFECT_ROWS = affectRows;
            piaLogMain.PIA_TYPE = "0100100000";
            piaLogMain.EXECUTION_TYPE = executionType;
            piaLogMain.ACCESSOBJ_NAME = "FAP_VE_TRACE";
            piaLogMain.PIA_OWNER1 = MaskUtil.maskId(piaOwner);
            PiaLogMainDao piaLogMainDao = new PiaLogMainDao();
            piaLogMainDao.Insert(piaLogMain);

        }


    }
}