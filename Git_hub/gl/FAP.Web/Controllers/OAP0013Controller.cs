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
using System.Data;
using System.IO;
using ClosedXML.Excel;

/// <summary>
/// 功能說明：壽險公司應支付而未能給付保戶款項調查表
/// 初版作者：20190717 Daiyu
/// 修改歷程：20190717 Daiyu
/// 需求單號：
/// 初版
/// -----------------------------------------------------------------
/// 修改歷程：20200810 Daiyu
/// 需求單號：
/// 修改內容：1.修改報表格式
///           2.列印條件加保局範圍可以全選
/// </summary>
///

namespace FAP.Web.Controllers
{
    [Authorize]
    [CheckSessionFilterAttribute]
    public class OAP0013Controller : BaseController
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
                String[] roleInfo = authUtil.chkUserFuncAuth(Session["UserID"].ToString(), "~/OAP0013/");
                if (roleInfo != null && roleInfo.Length == 2)
                {
                    opScope = roleInfo[0];
                    funcName = roleInfo[1];
                }
            }

            ViewBag.opScope = opScope;
            ViewBag.funcName = funcName;

            OAP0013Model model = new OAP0013Model();

            SysCodeDao sysCodeDao = new SysCodeDao();
            //清理狀態
            ViewBag.clrStatusList = sysCodeDao.loadSelectList("AP", "CLR_STATUS", true);


            FAPVeCodeDao fAPVeCodeDao = new FAPVeCodeDao();
            //保局範圍
            List<FAP_VE_CODE> fscRangeList = fAPVeCodeDao.qryByGrp("FSC_RANGE");
            model.fscRangeList = fscRangeList;

            //清理大類
            List<FAP_VE_CODE> level1List = fAPVeCodeDao.qryByGrp("CLR_LEVEL1");
            model.level1List = level1List;


            ///清理小類
            List<FAP_VE_CODE> level2List = fAPVeCodeDao.qryByGrp("CLR_LEVEL2");
            model.level2List = level2List;


            return View(model);
        }



        /// <summary>
        /// 匯出
        /// </summary>
        /// <param name="fsc_range"></param>
        /// <param name="date_b"></param>
        /// <param name="date_e"></param>
        /// <param name="status"></param>
        /// <param name="level_1"></param>
        /// <param name="level_2"></param>
        /// <param name="cnt_type"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult execExport(string fsc_range, string date_b, string date_e, string status, string level_1, string level_2, string cnt_type
            , bool levelSpace)
        {
            logger.Info("execExport begin!!");
            try
            {
                List<OAP0013Model> rows = new List<OAP0013Model>();

                string[] fsc_range_arr = fsc_range.Split('|');
                string[] level_1_arr = level_1.Split('|');
                string[] level_2_arr = level_2.Split('|');

                //查詢【FAP_VE_TRACE 逾期未兌領清理記錄檔】
                FAPVeTraceDao fAPVeTraceDao = new FAPVeTraceDao();

                string strConn = DbUtil.GetDBFglConnStr();
                using (SqlConnection conn = new SqlConnection(strConn))
                {
                    conn.Open();
                    rows = fAPVeTraceDao.qryForOAP0013(fsc_range_arr, date_b, date_e, status, level_1_arr, level_2_arr, cnt_type, levelSpace, conn);
                 
                }


                string guid = "";
                if (rows.Count > 0) {
                    guid = Guid.NewGuid().ToString();
                    genRpt(guid, rows, fsc_range, date_b, date_e);
                    var jsonData = new { success = true, guid = guid };
                    return Json(jsonData, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    var jsonData = new { success = false, err = "無資料" };
                    return Json(jsonData, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception e) {
                return Json(new { success = false, err = "其它錯誤，請洽系統管理員!!" });
            }
        }


        public FileContentResult downloadRpt(String id)
        {
            byte[] fileBytes = System.IO.File.ReadAllBytes(Server.MapPath("~/Temp/") + "OAP0013" + "_" + id + "_" + Session["UserID"].ToString() + ".xlsx");


            string fullPath = Server.MapPath("~/Temp/") + "OAP0013" + "_" + id + "_" + Session["UserID"].ToString() + ".xlsx";
            if (System.IO.File.Exists(fullPath))
                System.IO.File.Delete(fullPath);


            return File(fileBytes, "application/vnd.ms-excel", "壽險公司應支付而未能給付保戶款項調查表.xlsx");
        }


        private void genRpt(string guid, List<OAP0013Model> rows, string fsc_range, string date_b, string date_e)
        {
            string fullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory + "Temp\\"
                            , string.Concat("OAP0013" + "_" + guid + "_" + Session["UserID"].ToString(), ".xlsx"));


            SysCodeDao sysCodeDao = new SysCodeDao();
            //清理狀態
            Dictionary<string, string> clrStatusMap = sysCodeDao.qryByTypeDic("AP", "CLR_STATUS");


            FAPVeCodeDao fAPVeCodeDao = new FAPVeCodeDao();
            //保局範圍
            Dictionary<string, string> fscRangeMap = fAPVeCodeDao.qryByTypeDic("FSC_RANGE");

            //清理小類
            Dictionary<string, string> level2Map = fAPVeCodeDao.qryByTypeDic("CLR_LEVEL2");


            List<OAP0013Model> rptStatRow = rows.GroupBy(o => new { o.fsc_range })
                .Select(group => new OAP0013Model
                {
                    fsc_range = group.Key.fsc_range,
                    cnt = group.Sum(x => Convert.ToInt64(x.cnt)).ToString(),
                    amt = group.Sum(x => Convert.ToInt64(x.amt)).ToString(),
                }).OrderBy(x => x.fsc_range.Length).ThenBy(x => x.fsc_range).ToList<OAP0013Model>();

            int rangeBCol, totCol = 0;
            totCol = 2 + 5 * rptStatRow.Count + 1;
            rangeBCol = 3;

            using (XLWorkbook workbook = new XLWorkbook())
            {
                var ws = workbook.Worksheets.Add("壽險業-1");

                ws.Cell("A1").Value = "壽險公司應支付而未能給付保戶款項調查表";
                ws.Cell("A2").Value = "資料統計截止日：" + date_e;
                ws.Cell("A3").Value = "金額單位：新臺幣元";
                ws.Cell("A4").Value = "公司";
                ws.Cell("A7").Value = "財務部";
                ws.Cell("A10").Value = "合計";
                ws.Cell(4, totCol).Value = "備註";

                ws.Range(1, 1, 1, totCol).Merge();
                ws.Range(2, 1, 2, totCol).Merge();
                ws.Range(3, 1, 3, totCol).Merge();
                ws.Range(4, 1, 6, 2).Merge();
                ws.Range(7, 1, 7, 2).Merge();
                ws.Range(8, 1, 8, 2).Merge();
                ws.Range(9, 1, 9, 2).Merge();
                ws.Range(10, 1, 10, 2).Merge();
                ws.Range(4, totCol, 6, totCol).Merge();

                ws.Range(1, 1, 10, totCol).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                ws.Range("A3").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                ws.Range("A4").Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                ws.Range(4, totCol, 6, totCol).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                ws.Range(1, 1, 3, totCol).Style.Font.Bold = true;


                int i = 2;
                foreach (OAP0013Model d in rptStatRow)
                {
                    long cnt = rows.Where(x => x.fsc_range == d.fsc_range && x.rpt_status != "1").Sum(x => Convert.ToInt64(x.cnt));
                    long amt = rows.Where(x => x.fsc_range == d.fsc_range && x.rpt_status != "1").Sum(x => Convert.ToInt64(x.amt));



                    ws.Cell(4, rangeBCol).Value = fscRangeMap[d.fsc_range];
                    ws.Cell(5, rangeBCol).Value = "應支付而未支付之款項";
                    ws.Cell(5, rangeBCol + 2).Value = "截止日尚未支付完成";

                    ws.Cell(6, rangeBCol).Value = "總金額";
                    ws.Cell(6, rangeBCol + 1).Value = "件數";
                    ws.Cell(6, rangeBCol + 2).Value = "總金額";
                    ws.Cell(6, rangeBCol + 3).Value = "件數";
                    ws.Cell(6, rangeBCol + 4).Value = "其中為超過1年之保戶\n已領取而未兌領支票金額";

                    
                    

                    ws.Range(6, rangeBCol + 1, 10, rangeBCol + 1).Style.Font.FontColor = XLColor.Red;
                    ws.Range(6, rangeBCol + 3, 10, rangeBCol + 3).Style.Font.FontColor = XLColor.Red;

                    ws.Cell(7, rangeBCol).Value = String.Format("{0:n0}", Convert.ToInt64(d.amt));
                    ws.Cell(7, rangeBCol + 1).Value = String.Format("{0:n0}", Convert.ToInt64(d.cnt));
                    ws.Cell(7, rangeBCol + 2).Value = String.Format("{0:n0}", amt);
                    ws.Cell(7, rangeBCol + 3).Value = String.Format("{0:n0}", cnt);
                    ws.Cell(7, rangeBCol + 4).Value = String.Format("{0:n0}", amt);

                    ws.Cell(10, rangeBCol).Value = String.Format("{0:n0}", Convert.ToInt64(d.amt));
                    ws.Cell(10, rangeBCol + 1).Value = String.Format("{0:n0}", Convert.ToInt64(d.cnt));
                    ws.Cell(10, rangeBCol + 2).Value = String.Format("{0:n0}", amt);
                    ws.Cell(10, rangeBCol + 3).Value = String.Format("{0:n0}", cnt);
                    ws.Cell(10, rangeBCol + 4).Value = String.Format("{0:n0}", amt);

                    ws.Range(4, rangeBCol, 4, rangeBCol + 4).Merge();
                    ws.Range(5, rangeBCol, 5, rangeBCol + 1).Merge();
                    ws.Range(5, rangeBCol + 2, 5, rangeBCol + 4).Merge();

                    //ws.Column(rangeBCol).Width = 25;
                    //ws.Column(rangeBCol + 1).Width = 25;
                    //ws.Column(rangeBCol + 2).AdjustToContents();
                    //ws.Column(rangeBCol + 3).AdjustToContents();
                    //ws.Column(rangeBCol + 4).AdjustToContents();

                    genSubSheet(workbook, "壽險業-" + i.ToString() + "(" + fscRangeMap[d.fsc_range] + ")", d, rows, fscRangeMap[d.fsc_range], date_e);

                    rangeBCol += 5;
                    i++;

                }

                ws.Range(4, 1, 10, totCol).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                ws.Range(4, 1, 10, totCol).Style.Border.InsideBorder = XLBorderStyleValues.Thin;


                ws.Row(6).Style.Alignment.WrapText = true;
                ws.Columns().AdjustToContents();  // Adjust column width
                ws.Rows().AdjustToContents();     // Adjust row heights

                ws.Row(6).Height = 36;

                workbook.SaveAs(fullPath);


            }
        }



        /// <summary>
        /// 各"保局範圍"的應支付而未能給付保戶款項調查表
        /// </summary>
        /// <param name="workbook"></param>
        /// <param name="sheetName"></param>
        /// <param name="d"></param>
        /// <param name="rows"></param>
        /// <param name="fsc_range"></param>
        /// <param name="date_e"></param>
        /// <param name="rate"></param>
        /// <returns></returns>
        private XLWorkbook genSubSheet(XLWorkbook workbook, string sheetName, OAP0013Model d, List<OAP0013Model> rows, string fsc_range, string date_e)
        {
            int totCol = 0;
            totCol = 11;
            
            string[] endDate = date_e.Split('-');
            string endYY = (Convert.ToInt64(endDate[0]) - 1911).ToString();
            string rptDate = endYY + "年" + endDate[1] + "月" + endDate[2] + "日";

            var ws = workbook.Worksheets.Add(sheetName);
            ws.Cell("A1").Value = "壽險公司應支付而未能給付保戶款項調查表";
            ws.Cell("A2").Value = "資料統計截止日：" + date_e;
            ws.Cell("A3").Value = "金額單位：新臺幣元";
            ws.Cell("A4").Value = "公司";
            ws.Cell("A6").Value = "財務部";
            ws.Cell("A9").Value = "合計";

            ws.Cell("C4").Value = fsc_range + "\n清查結果應支付而未支付之款項\n(A)";
            ws.Cell("D4").Value = "至截止日已完成支付\n(B)";
            ws.Cell("E4").Value = "至截止日尚未支付\n(C=A-B)";
            ws.Cell("F4").Value = "設定至" + endYY + "年底完成清理目標";
            ws.Cell("G4").Value = "至" + rptDate + "已完成清理\n(D)";
            ws.Cell("J4").Value = "至截止日尚未清理完成\n(C - D)";

            ws.Cell("K4").Value = "備註";

            ws.Cell("C5").Value = "金額";
            ws.Cell("D5").Value = "金額";
            ws.Cell("E5").Value = "金額";
            ws.Cell("F5").Value = "金額";
            ws.Cell("G5").Value = "金額";
            ws.Cell("H5").Value = "金額清理比率\n((B+D)/A)";
            ws.Cell("I5").Value = "支付進度說明";
            ws.Cell("J5").Value = "金額";
            
            ws.Range(1, 1, 1, totCol).Merge();
            ws.Range(2, 1, 2, totCol).Merge();
            ws.Range(3, 1, 3, totCol).Merge();
            ws.Range(4, 1, 5, 2).Merge();
            ws.Range(6, 1, 6, 2).Merge();
            ws.Range(7, 1, 7, 2).Merge();
            ws.Range(8, 1, 8, 2).Merge();
            ws.Range(9, 1, 9, 2).Merge();
            ws.Range(4, totCol, 5, totCol).Merge();
            ws.Range(4, 7, 4, 9).Merge();


            long iA = Convert.ToInt64(d.amt);
            long iB = Convert.ToInt64(rows.Where(x => x.fsc_range == d.fsc_range && x.rpt_status == "1").Sum(x => Convert.ToInt64(x.amt)));
            long iC = iA - iB;
            long iD = Convert.ToInt64(rows.Where(x => x.fsc_range == d.fsc_range && x.rpt_status == "2").Sum(x => Convert.ToInt64(x.amt)));
            string cleanRate = Convert.ToDouble(((iB + iD) / (double)iA) * 100).ToString("0.00");
            string strDesc = "";
            strDesc += "1.截至" + endYY + "." + endDate[1] + "." + endDate[2] + "已完成給付" + String.Format("{0:n0}", iB) + "\n";



            ws.Cell("C6").Value = String.Format("{0:n0}", iA);
            ws.Cell("D6").Value = String.Format("{0:n0}", iB);
            ws.Cell("E6").Value = String.Format("{0:n0}", iC);
          
            ws.Cell("G6").Value = String.Format("{0:n0}", iD);
            ws.Cell("H6").Value = String.Format("{0:n0}", cleanRate);

            ws.Cell("J6").Value = String.Format("{0:n0}", iA - iB - iD);

            ws.Cell("C9").Value = String.Format("{0:n0}", iA);
            ws.Cell("D9").Value = String.Format("{0:n0}", iB);
            ws.Cell("E9").Value = String.Format("{0:n0}", iC);
            ws.Cell("G9").Value = String.Format("{0:n0}", iD);
            ws.Cell("H9").Value = String.Format("{0:n0}", cleanRate);
            ws.Cell("J9").Value = String.Format("{0:n0}", iA - iB - iD);

            ws.Range(1, 1, 10, totCol).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Range("A3").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
            ws.Range("A4").Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            ws.Range(4, totCol, 6, totCol).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            ws.Range(1, 1, 3, totCol).Style.Font.Bold = true;

            ws.Range("D4", "E4").Style.Fill.BackgroundColor = XLColor.FromArgb(255, 255, 0);
            ws.Range("C4", "C9").Style.Fill.BackgroundColor = XLColor.FromArgb(226, 239, 218);
            ws.Range("E5", "E9").Style.Fill.BackgroundColor = XLColor.FromArgb(226, 239, 218);
            ws.Range("G4", "I9").Style.Fill.BackgroundColor = XLColor.FromArgb(226, 239, 218);


            ws.Range(4, 1, 9, totCol).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            ws.Range(4, 1, 9, totCol).Style.Border.InsideBorder = XLBorderStyleValues.Thin;

            ws.Range("C4", "J5").Style.Alignment.WrapText = true;
            ws.Columns().AdjustToContents();  // Adjust column width
            ws.Rows().AdjustToContents();     // Adjust row heights

            ws.Row(4).Height = 50;
            ws.Row(5).Height = 36;
            //ws.Rows(1, 9).AdjustToContents();


            //workbook.SaveAs(fullPath);
            return workbook;
        }
    }
}