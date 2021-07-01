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
/// 功能說明：應付未付每月分析表
/// 初版作者：20190718 Daiyu
/// 修改歷程：20190718 Daiyu
/// 需求單號：
/// 修改歷程：初版
/// ------------------------------------------
/// 修改歷程：20210201 Daiyu
/// 需求單號：202101280283-00
/// 修改內容：查詢畫面增加"給付年月"起訖，有輸入時呈現依給付年月統計的報表
/// </summary>
///

namespace FAP.Web.Controllers
{
    [Authorize]
    [CheckSessionFilterAttribute]
    public class OAP0014Controller : BaseController
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
                String[] roleInfo = authUtil.chkUserFuncAuth(Session["UserID"].ToString(), "~/OAP0014/");
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
        /// <param name="stat_type"></param>
        /// <param name="fsc_range"></param>
        /// <param name="check_ym_b"></param>
        /// <param name="check_ym_e"></param>
        /// <param name="date_b"></param>
        /// <param name="date_e"></param>
        /// <param name="status"></param>
        /// <param name="level_1"></param>
        /// <param name="level_2"></param>
        /// <param name="cnt_type"></param>
        /// <param name="levelSpace"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult execExport(string stat_type, string fsc_range, string check_ym_b, string check_ym_e, string date_b, string date_e, string status, string level_1, string level_2
            , string cnt_type, bool levelSpace, string repaid_ym_b, string repaid_ym_e)
        // public JsonResult execExport( string check_ym_b, string check_ym_e, string date_b, string date_e, string status, string level_1, string level_2
        //, string cnt_type, bool levelSpace)
        {
            logger.Info("execExport begin!!");
            try
            {
                List <OAP0014Model> rows = new List<OAP0014Model>();
                string[] fsc_range_arr = fsc_range.Split('|');
                string[] level_1_arr = level_1.Split('|');
                string[] level_2_arr = level_2.Split('|');

                //查詢【FAP_VE_TRACE 逾期未兌領清理記錄檔】
                FAPVeTraceDao fAPVeTraceDao = new FAPVeTraceDao();

                string strConn = DbUtil.GetDBFglConnStr();
                using (SqlConnection conn = new SqlConnection(strConn))
                {
                    conn.Open();

                    switch (stat_type) {
                        case "check_ym":
                            rows = fAPVeTraceDao.qryForOAP0014CheckYm(check_ym_b, check_ym_e, date_b, date_e, status, level_1_arr, level_2_arr, cnt_type, levelSpace, conn);
                            break;
                        case "repaid_ym":
                            rows = fAPVeTraceDao.qryForOAP0014RepaidYm(repaid_ym_b, repaid_ym_e, date_b, date_e, status, level_1_arr, level_2_arr, cnt_type, levelSpace, conn);
                            break;
                        default:
                            rows = fAPVeTraceDao.qryForOAP0014FscRange(fsc_range_arr, date_e, status, level_1_arr, level_2_arr, cnt_type, levelSpace, conn);
                            break;
                    }
                    //if("check_ym".Equals(stat_type))
                    //    rows = fAPVeTraceDao.qryForOAP0014CheckYm(check_ym_b, check_ym_e, date_b, date_e, status, level_1_arr, level_2_arr, cnt_type, levelSpace, conn);
                    //else
                    //    rows = fAPVeTraceDao.qryForOAP0014FscRange(fsc_range_arr, date_e, status, level_1_arr, level_2_arr, cnt_type, levelSpace, conn);
                }
                    

                string guid = "";
                if (rows.Count > 0) {
                    guid = Guid.NewGuid().ToString();

                    if("repaid_ym".Equals(stat_type))
                        genRepaidRpt(stat_type, guid, rows, date_b, date_e);
                    else
                        genRpt(stat_type, guid, rows, check_ym_b, check_ym_e, date_b, date_e);

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
                logger.Error(e.ToString());
                return Json(new { success = false, err = "其它錯誤，請洽系統管理員!!" });
            }
        }


        public FileContentResult downloadRpt(String id)
        {
            byte[] fileBytes = System.IO.File.ReadAllBytes(Server.MapPath("~/Temp/") + "OAP0014" + "_"+ id + "_" + Session["UserID"].ToString() + ".xlsx");


            string fullPath = Server.MapPath("~/Temp/") + "OAP0014" + "_" + id + "_" + Session["UserID"].ToString() + ".xlsx";
            if (System.IO.File.Exists(fullPath))
                System.IO.File.Delete(fullPath);


            return File(fileBytes, "application/vnd.ms-excel", "應付未付每月分析表.xlsx");
        }


        private void genRpt(string stat_type, string guid, List<OAP0014Model> rows, string check_ym_b, string check_ym_e, string date_b, string date_e)
        {
            string fullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory + "Temp\\"
                            , string.Concat("OAP0014" + "_" + guid + "_" + Session["UserID"].ToString(), ".xlsx"));


            SysCodeDao sysCodeDao = new SysCodeDao();
            //清理狀態
            Dictionary<string, string> clrStatusMap = sysCodeDao.qryByTypeDic("AP", "CLR_STATUS");

            FAPVeCodeDao fAPVeCodeDao = new FAPVeCodeDao();
            //保局範圍
            Dictionary<string, string> fscRangeMap = fAPVeCodeDao.qryByTypeDic("FSC_RANGE");

            List<OAP0014Model> rptStatRow = rows.GroupBy(o => new { o.check_ym })
                .Select(group => new OAP0014Model
                {
                    check_ym = group.Key.check_ym,
                    cnt = group.Sum(x => Convert.ToInt64(x.cnt)).ToString(),
                    amt = group.Sum(x => Convert.ToInt64(x.amt)).ToString(),
                }).OrderBy(x => x.check_ym.Length).ThenBy(x => x.check_ym).ToList<OAP0014Model>();

            using (XLWorkbook workbook = new XLWorkbook())
            {
                var ws = workbook.Worksheets.Add("OAP0014統計結果");

                ws.Cell("A1").Value = "應付未付每月分析表";
                ws.Cell("A2").Value = "資料統計截止日：" + date_e;
                ws.Range(1, 1, 1, 9).Merge();
                ws.Range(2, 1, 2, 9).Merge();

                if("check_ym".Equals(stat_type))
                    ws.Cell("A3").Value = "支票年月";
                else
                    ws.Cell("A3").Value = "保局範圍";

                ws.Cell("B3").Value = "新增支票件數(A1)";
                ws.Cell("C3").Value = "新增支票金額(A2)";
                ws.Cell("D3").Value = "已給付件數(B1)";
                ws.Cell("E3").Value = "已給付金額(B2)";
                ws.Cell("F3").Value = "已清理件數(C1)";
                ws.Cell("G3").Value = "已清理金額(C2)";
                ws.Cell("H3").Value = "件數餘額(A1-B1-C1)";
                ws.Cell("I3").Value = "總餘額(A2-B2-C2)";
                ws.Range(3, 1, 3, 9).Style.Fill.BackgroundColor = XLColor.AirForceBlue;
                ws.Range(3, 1, 3, 9).Style.Font.FontColor = XLColor.White;



                Int64 totBCnt = 0;
                Int64 totBAmt = 0;
                Int64 totCCnt = 0;
                Int64 totCAmt = 0;
                Int64 totDCnt = 0;
                Int64 totDAmt = 0;

                int iCol = 2;
                int iRow = 0;
                iRow = 4;
                foreach (OAP0014Model d in rptStatRow)
                {
                    ws.Cell(iRow, 1).DataType = XLDataType.Text;


                    if ("check_ym".Equals(stat_type)) {
                        string[] check_ym = d.check_ym.Split('/');
                        ws.Cell(iRow, 1).Value = "'" + (Convert.ToInt16(check_ym[0]) - 1911).ToString() + "/" + check_ym[1];
                    }
                    else {
                        if (fscRangeMap.ContainsKey(d.check_ym))
                            ws.Cell(iRow, 1).Value = fscRangeMap[d.check_ym];
                        else
                            ws.Cell(iRow, 1).Value = d.check_ym;
                    }



                    ws.Cell(iRow, 2).Value = String.Format("{0:n0}", Convert.ToInt64(d.cnt));
                    ws.Cell(iRow, 3).Value = String.Format("{0:n0}", Convert.ToInt64(d.amt));

                    Int64 bCnt = 0;
                    Int64 bAmt = 0;
                    Int64 cCnt = 0;
                    Int64 cAmt = 0;
                    Int64 dCnt = 0;
                    Int64 dAmt = 0;




                    if (rows.Where(x => x.check_ym == d.check_ym & x.rpt_status == "1").FirstOrDefault() != null)
                    {
                        bCnt = Convert.ToInt64(rows.Where(x => x.check_ym == d.check_ym & x.rpt_status == "1").FirstOrDefault().cnt);
                        bAmt = Convert.ToInt64(rows.Where(x => x.check_ym == d.check_ym & x.rpt_status == "1").FirstOrDefault().amt);
                        totBCnt += bCnt;
                        totBAmt += bAmt;
                    }
                    ws.Cell(iRow, 4).Value = String.Format("{0:n0}", bCnt);
                    ws.Cell(iRow, 5).Value = String.Format("{0:n0}", bAmt);


                    if (rows.Where(x => x.check_ym == d.check_ym & x.rpt_status == "2").FirstOrDefault() != null)
                    {
                        cCnt = Convert.ToInt64(rows.Where(x => x.check_ym == d.check_ym & x.rpt_status == "2").FirstOrDefault().cnt);
                        cAmt = Convert.ToInt64(rows.Where(x => x.check_ym == d.check_ym & x.rpt_status == "2").FirstOrDefault().amt);
                        totCCnt += cCnt;
                        totCAmt += cAmt;
                    }
                    ws.Cell(iRow, 6).Value = String.Format("{0:n0}", cCnt);
                    ws.Cell(iRow, 7).Value = String.Format("{0:n0}", cAmt);


                    if (rows.Where(x => x.check_ym == d.check_ym & x.rpt_status == "3").FirstOrDefault() != null)
                    {
                        dCnt += Convert.ToInt64(rows.Where(x => x.check_ym == d.check_ym & x.rpt_status == "3").FirstOrDefault().cnt);
                        dAmt += Convert.ToInt64(rows.Where(x => x.check_ym == d.check_ym & x.rpt_status == "3").FirstOrDefault().amt);
                    }

                    if (rows.Where(x => x.check_ym == d.check_ym & x.rpt_status == "4").FirstOrDefault() != null)
                    {
                        dCnt += Convert.ToInt64(rows.Where(x => x.check_ym == d.check_ym & x.rpt_status == "4").FirstOrDefault().cnt);
                        dAmt += Convert.ToInt64(rows.Where(x => x.check_ym == d.check_ym & x.rpt_status == "4").FirstOrDefault().amt);
                        
                    }

                    totDCnt += dCnt;
                    totDAmt += dAmt;

                    ws.Cell(iRow, 8).Value = String.Format("{0:n0}", dCnt);
                    ws.Cell(iRow, 9).Value = String.Format("{0:n0}", dAmt);

                    iRow++;
                }

                ws.Cell(iRow, 1).Value = "合計";
                ws.Cell(iRow, 2).Value = String.Format("{0:n0}", totBCnt + totCCnt + totDCnt);
                ws.Cell(iRow, 3).Value = String.Format("{0:n0}", totBAmt + totCAmt + totDAmt);
                ws.Cell(iRow, 4).Value = String.Format("{0:n0}", totBCnt);
                ws.Cell(iRow, 5).Value = String.Format("{0:n0}", totBAmt);
                ws.Cell(iRow, 6).Value = String.Format("{0:n0}", totCCnt);
                ws.Cell(iRow, 7).Value = String.Format("{0:n0}", totCAmt);
                ws.Cell(iRow, 8).Value = String.Format("{0:n0}", totDCnt);
                ws.Cell(iRow, 9).Value = String.Format("{0:n0}", totDAmt);


                ws.Range(1, 1, iRow, 9).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                ws.Range(4, 2, iRow, 9).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                ws.Range(1, 1, iRow, 9).Style.Border.OutsideBorder = XLBorderStyleValues.Thick;
                ws.Range(1, 1, iRow, 9).Style.Border.InsideBorder = XLBorderStyleValues.Thin;



                ws.Columns().AdjustToContents();  // Adjust column width
                ws.Rows().AdjustToContents();     // Adjust row heights
                workbook.SaveAs(fullPath);


            }
        }

        private void genRepaidRpt(string stat_type, string guid, List<OAP0014Model> rows, string date_b, string date_e)
        {
            string fullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory + "Temp\\"
                            , string.Concat("OAP0014" + "_" + guid + "_" + Session["UserID"].ToString(), ".xlsx"));


            SysCodeDao sysCodeDao = new SysCodeDao();
            //清理狀態
            Dictionary<string, string> clrStatusMap = sysCodeDao.qryByTypeDic("AP", "CLR_STATUS");


            List<OAP0014Model> rptStatRow = rows.GroupBy(o => new { o.check_ym })
                .Select(group => new OAP0014Model
                {
                    check_ym = group.Key.check_ym,
                    cnt = group.Sum(x => Convert.ToInt64(x.cnt)).ToString(),
                    amt = group.Sum(x => Convert.ToInt64(x.amt)).ToString(),
                }).OrderBy(x => x.check_ym.Length).ThenBy(x => x.check_ym).ToList<OAP0014Model>();

            using (XLWorkbook workbook = new XLWorkbook())
            {
                var ws = workbook.Worksheets.Add("OAP0014統計結果");

                ws.Cell("A1").Value = "應付未付每月分析表";
                ws.Cell("A2").Value = "資料統計截止日：" + date_e;
                ws.Range(1, 1, 1, 5).Merge();
                ws.Range(2, 1, 2, 5).Merge();

                ws.Cell("A3").Value = "給付年月";
                ws.Cell("B3").Value = "已給付件數(B1)";
                ws.Cell("C3").Value = "已給付金額(B2)";
                ws.Cell("D3").Value = "已清理件數(C1)";
                ws.Cell("E3").Value = "已清理金額(C2)";

                ws.Range(3, 1, 3, 5).Style.Fill.BackgroundColor = XLColor.AirForceBlue;
                ws.Range(3, 1, 3, 5).Style.Font.FontColor = XLColor.White;


                Int64 totBCnt = 0;
                Int64 totBAmt = 0;
                Int64 totCCnt = 0;
                Int64 totCAmt = 0;


                int iCol = 2;
                int iRow = 0;
                iRow = 4;
                foreach (OAP0014Model d in rptStatRow)
                {
                    ws.Cell(iRow, 1).DataType = XLDataType.Text;

                    string[] check_ym = d.check_ym.Split('/');
                    ws.Cell(iRow, 1).Value = "'" + (Convert.ToInt16(check_ym[0]) - 1911).ToString() + "/" + check_ym[1];

                    Int64 bCnt = 0;
                    Int64 bAmt = 0;
                    Int64 cCnt = 0;
                    Int64 cAmt = 0;


                    if (rows.Where(x => x.check_ym == d.check_ym & x.rpt_status == "1").FirstOrDefault() != null)
                    {
                        bCnt = Convert.ToInt64(rows.Where(x => x.check_ym == d.check_ym & x.rpt_status == "1").FirstOrDefault().cnt);
                        bAmt = Convert.ToInt64(rows.Where(x => x.check_ym == d.check_ym & x.rpt_status == "1").FirstOrDefault().amt);
                        totBCnt += bCnt;
                        totBAmt += bAmt;
                    }
                    ws.Cell(iRow, 2).Value = String.Format("{0:n0}", bCnt);
                    ws.Cell(iRow, 3).Value = String.Format("{0:n0}", bAmt);


                    if (rows.Where(x => x.check_ym == d.check_ym & x.rpt_status == "2").FirstOrDefault() != null)
                    {
                        cCnt = Convert.ToInt64(rows.Where(x => x.check_ym == d.check_ym & x.rpt_status == "2").FirstOrDefault().cnt);
                        cAmt = Convert.ToInt64(rows.Where(x => x.check_ym == d.check_ym & x.rpt_status == "2").FirstOrDefault().amt);
                        totCCnt += cCnt;
                        totCAmt += cAmt;
                    }
                    ws.Cell(iRow, 4).Value = String.Format("{0:n0}", cCnt);
                    ws.Cell(iRow, 5).Value = String.Format("{0:n0}", cAmt);



                    iRow++;
                }

                ws.Cell(iRow, 1).Value = "合計";

                ws.Cell(iRow, 2).Value = String.Format("{0:n0}", totBCnt);
                ws.Cell(iRow, 3).Value = String.Format("{0:n0}", totBAmt);
                ws.Cell(iRow, 4).Value = String.Format("{0:n0}", totCCnt);
                ws.Cell(iRow, 5).Value = String.Format("{0:n0}", totCAmt);


                ws.Range(1, 1, iRow, 5).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                ws.Range(4, 2, iRow, 5).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                ws.Range(1, 1, iRow, 5).Style.Border.OutsideBorder = XLBorderStyleValues.Thick;
                ws.Range(1, 1, iRow, 5).Style.Border.InsideBorder = XLBorderStyleValues.Thin;


                ws.Columns().AdjustToContents();  // Adjust column width
                ws.Rows().AdjustToContents();     // Adjust row heights
                workbook.SaveAs(fullPath);


            }
        }
    }
}