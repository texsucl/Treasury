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
/// 功能說明：應付未付-逾期未兌票準備金報表
/// 初版作者：20190719 Daiyu
/// 修改歷程：20190719 Daiyu
///           需求單號：
///           初版
/// </summary>
///

namespace FAP.Web.Controllers
{
    [Authorize]
    [CheckSessionFilterAttribute]
    public class OAP0015Controller : BaseController
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
                String[] roleInfo = authUtil.chkUserFuncAuth(Session["UserID"].ToString(), "~/OAP0015/");
                if (roleInfo != null && roleInfo.Length == 2)
                {
                    opScope = roleInfo[0];
                    funcName = roleInfo[1];
                }
            }

            ViewBag.opScope = opScope;
            ViewBag.funcName = funcName;

            OAP0015Model model = new OAP0015Model();


            FAPVeCodeDao fAPVeCodeDao = new FAPVeCodeDao();
            //保局範圍
            List<FAP_VE_CODE> fscRangeList = fAPVeCodeDao.qryByGrp("FSC_RANGE");
            model.fscRangeList = fscRangeList;



            return View(model);
        }



        /// <summary>
        /// 匯出
        /// </summary>
        /// <param name="fsc_range"></param>
        /// <param name="date_e"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult execExport(string fsc_range, string date_e, string status)
        {
            logger.Info("execExport begin!!");
            try
            {
                List <OAP0015Model> rows = new List<OAP0015Model>();
                string[] fsc_range_arr = fsc_range.Split('|');
                string[] status_arr = status.Split('|');


                //查詢【FAP_VE_TRACE 逾期未兌領清理記錄檔】
                FAPVeTraceDao fAPVeTraceDao = new FAPVeTraceDao();

                string strConn = DbUtil.GetDBFglConnStr();
                using (SqlConnection conn = new SqlConnection(strConn))
                {
                    conn.Open();
                    rows = fAPVeTraceDao.qryForOAP0015(fsc_range_arr, date_e, status_arr, conn);
                }
                    

                string guid = "";
                if (rows.Count > 0) {
                    guid = Guid.NewGuid().ToString();
                    genRpt(guid, rows, fsc_range, date_e);
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
            byte[] fileBytes = System.IO.File.ReadAllBytes(Server.MapPath("~/Temp/") + "OAP0015" + "_"+ id + "_" + Session["UserID"].ToString() + ".xlsx");


            string fullPath = Server.MapPath("~/Temp/") + "OAP0015" + "_" + id + "_" + Session["UserID"].ToString() + ".xlsx";
            if (System.IO.File.Exists(fullPath))
                System.IO.File.Delete(fullPath);


            return File(fileBytes, "application/vnd.ms-excel", "應付未付-逾期未兌票準備金報表.xlsx");
        }


        private void genRpt(string guid, List<OAP0015Model> rows, string fsc_range, string date_e)
        {
            string fullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory + "Temp\\"
                            , string.Concat("OAP0015" + "_" + guid + "_" + Session["UserID"].ToString(), ".xlsx"));

            using (XLWorkbook workbook = new XLWorkbook())
            {
                genDetail("A", workbook, rows, date_e);
                workbook.SaveAs(fullPath);

                genDetail("B", workbook, rows, date_e);
                workbook.SaveAs(fullPath);

                genSum( workbook, rows, date_e);
                workbook.SaveAs(fullPath);
            }
        }



        private XLWorkbook genSum(XLWorkbook workbook, List<OAP0015Model> rows, string date_e)
        {
            List<OAP0015Model> rptRow = new List<OAP0015Model>();


            var ws = workbook.Worksheets.Add("OAP0015 逾期未兌領");


            ws.Cell("A1").Value = "應付未付款項-逾期未兌領";
            ws.Cell("A2").Value = "資料統計截止日：" + date_e;
            ws.Range(1, 1, 1, 4).Merge();
            ws.Range(2, 1, 2, 4).Merge();

            ws.Cell(3, 1).Value = "應付未付款";
            ws.Cell(3, 3).Value = "轉列責任準備";
            ws.Range(3, 1, 3, 2).Merge();
            ws.Range(3, 3, 3, 4).Merge();


            ws.Cell(4, 1).Value = "幣別";
            ws.Cell(4, 2).Value = "金額";
            ws.Cell(4, 3).Value = "幣別";
            ws.Cell(4, 4).Value = "金額";


            ws.Range(3, 1, 4, 4).Style.Fill.BackgroundColor = XLColor.AirForceBlue;
            ws.Range(3, 1, 4, 4).Style.Font.FontColor = XLColor.White;

            Int64 totAmt = rows.Sum(x => Convert.ToInt64(x.main_amt));

            //Int64 totAmt  = rows.GroupBy(o => new { o.check_no, o.main_amt })
            //    .Select(group => new OAP0015Model
            //    {
            //        check_no = group.Key.check_no,
            //        main_amt = group.Key.main_amt
            //    }).ToList<OAP0015Model>().Sum(x => Convert.ToInt64(x.main_amt));

            Int64 totAmtOver = rows.Where(x => Convert.ToDateTime(x.check_date).CompareTo(Convert.ToDateTime(date_e).AddYears(-2)) <= 0).ToList()
                .Sum(x => Convert.ToInt64(x.main_amt));

            //Int64 totAmtOver = rows.Where(x => Convert.ToDateTime(x.check_date).CompareTo(Convert.ToDateTime(date_e).AddYears(-2)) < 0).ToList()
            //    .GroupBy(o => new { o.check_no, o.main_amt })
            //    .Select(group => new OAP0015Model
            //    {
            //        check_no = group.Key.check_no,
            //        main_amt = group.Key.main_amt
            //    }).ToList<OAP0015Model>()
            //    .Sum(x => Convert.ToInt64(x.main_amt));

            ws.Cell(5, 1).Value = "NTD";
            //ws.Cell(5, 2).Value = totAmt;
            ws.Cell(5, 2).Value = String.Format("{0:n0}", totAmt);
            ws.Cell(5, 3).Value = "NTD";
            //ws.Cell(5, 4).Value = totAmtOver;
            ws.Cell(5, 4).Value = String.Format("{0:n0}", totAmtOver);

            ws.Range(1, 1, 5, 4).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Range(1, 1, 5, 4).Style.Border.OutsideBorder = XLBorderStyleValues.Thick;
            ws.Range(1, 1, 5, 4).Style.Border.InsideBorder = XLBorderStyleValues.Thin;


            ws.Columns().AdjustToContents();  // Adjust column width
            ws.Rows().AdjustToContents();     // Adjust row heights
            
            return workbook;
        }



        private XLWorkbook genDetail(string type, XLWorkbook workbook, List<OAP0015Model> rows, string date_e) {
            List<OAP0015Model> rptRow = new List<OAP0015Model>();

            string sheetName = "";
            string titleName = "";
            if ("A".Equals(type))
            {
                rptRow = rows;
                sheetName = "OAP0015 逾期未兌票準備金報表(A表)";
                titleName = "A.應付未付-逾期未兌領尚未給付資料(全部)";
            }
            else {
                SysParaDao sysParaDao = new SysParaDao();
                SYS_PARA para = sysParaDao.qryByKey("AP", "OAP0015", "rptBYear");
                string year = "";
                if (para != null)
                    year = para.PARA_VALUE == null? "2" : para.PARA_VALUE;


                rptRow = rows.Where(x => Convert.ToDateTime(x.check_date).CompareTo(Convert.ToDateTime(date_e).AddYears(-2)) <= 0).ToList();

                sheetName = "OAP0015 逾期未兌票準備金報表(B表)";
                titleName = "B.應付未付-逾期未兌領尚未給付資料(" + year +"年以上)";
            }
            



            var ws = workbook.Worksheets.Add(sheetName);


            ws.Cell("A1").Value = "應付未付-逾期未兌票準備金報表";
            ws.Cell("A2").Value = "資料統計截止日：" + date_e;

            ws.Cell("A3").Value = titleName;
            ws.Range(1, 1, 1, 8).Merge();
            ws.Range(2, 1, 2, 8).Merge();
            ws.Range(3, 1, 3, 8).Merge();


            ws.Cell(4, 1).Value = "入帳日/支票到期日";
            ws.Cell(4, 2).Value = "系統別";
            ws.Cell(4, 3).Value = "保單號碼";
            ws.Cell(4, 4).Value = "保單序號";
            ws.Cell(4, 5).Value = "身分證重覆別";
            ws.Cell(4, 6).Value = "支票號碼";
            ws.Cell(4, 7).Value = "幣別";
            ws.Cell(4, 8).Value = "回存金額";

            ws.Range(4, 1, 4, 8).Style.Fill.BackgroundColor = XLColor.AirForceBlue;
            ws.Range(4, 1, 4, 8).Style.Font.FontColor = XLColor.White;

            int iRow = 5;
            int merRow = 0;
            int begRow = 0;
            //string _check_no = "";
            Int64 totAmt = 0;
            foreach (OAP0015Model poli in rptRow)
            {
                //if (!_check_no.Equals(poli.check_no)) {
                //    if (!"".Equals(_check_no) & merRow > 1) {
                //        ws.Range(begRow, 6, begRow + merRow - 1, 6).Merge();
                //        ws.Range(begRow, 7, begRow + merRow - 1, 7).Merge();
                //        ws.Range(begRow, 8, begRow + merRow - 1, 8).Merge();
                //    }

                //    _check_no = poli.check_no;
                //    begRow = iRow;
                //    merRow = 0;
                //}


                DateTime dt = Convert.ToDateTime(poli.check_date);
                ws.Cell(iRow, 1).Value = "'" + (dt.Year - 1911).ToString() + "/" + dt.Month.ToString().PadLeft(2, '0') + "/" + dt.Day.ToString().PadLeft(2, '0');
                ws.Cell(iRow, 2).Value = poli.system;
                ws.Cell(iRow, 3).Value = poli.policy_no;
                ws.Cell(iRow, 4).Value = poli.policy_seq;
                ws.Cell(iRow, 5).Value = poli.id_dup;
                ws.Cell(iRow, 6).Value = poli.check_no;
                ws.Cell(iRow, 7).Value = "NTD";
                ws.Cell(iRow, 8).Value =  Convert.ToInt64(poli.main_amt);
                //ws.Cell(iRow, 8).Value = String.Format("{0:n0}", Convert.ToInt64(poli.main_amt));

                //if(merRow == 0)
                    totAmt += Convert.ToInt64(poli.main_amt);

                //merRow++;
                iRow++;

            }

            //ws.Range(begRow, 6, begRow + merRow - 1, 6).Merge();
            //ws.Range(begRow, 7, begRow + merRow - 1, 7).Merge();
            //ws.Range(begRow, 8, begRow + merRow - 1, 8).Merge();

            ws.Cell(iRow, 1).Value = "合計";
            ws.Cell(iRow, 8).Value = String.Format("{0:n0}", totAmt);
            ws.Range(iRow, 1, iRow, 7).Merge();

            ws.Range(1, 1, 3, 8).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            //ws.Range(1, 1, iRow, 8).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Range(5, 8, iRow, 8).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
            //ws.Range(1, 1, iRow, 8).Style.Border.OutsideBorder = XLBorderStyleValues.Thick;
            //ws.Range(1, 1, iRow, 8).Style.Border.InsideBorder = XLBorderStyleValues.Thin;


            //ws.Columns().AdjustToContents();  // Adjust column width
            //ws.Rows().AdjustToContents();     // Adjust row heights


            return workbook;
        }
    }
}