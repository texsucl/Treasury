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
/// 功能說明：態樣統計表
/// 初版作者：20190715 Daiyu
/// 修改歷程：20190715 Daiyu
///           需求單號：
///           初版
/// </summary>
///

namespace FAP.Web.Controllers
{
    [Authorize]
    [CheckSessionFilterAttribute]
    public class OAP0012Controller : BaseController
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
                String[] roleInfo = authUtil.chkUserFuncAuth(Session["UserID"].ToString(), "~/OAP0012/");
                if (roleInfo != null && roleInfo.Length == 2)
                {
                    opScope = roleInfo[0];
                    funcName = roleInfo[1];
                }
            }

            ViewBag.opScope = opScope;
            ViewBag.funcName = funcName;

            OAP0012Model model = new OAP0012Model();

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
        /// <param name="level_1"></param>
        /// <param name="level_2"></param>
        /// <param name="cnt_type"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult execExport(string fsc_range, string date_b, string date_e, string level_1, string level_2, string cnt_type)
        {
            logger.Info("execExport begin!!");
            try
            {
                string[] fsc_range_arr = fsc_range.Split('|');
                string[] level_1_arr = level_1.Split('|');
                string[] level_2_arr = level_2.Split('|');

                //查詢【FAP_VE_TRACE 逾期未兌領清理記錄檔】
                FAPVeTraceDao fAPVeTraceDao = new FAPVeTraceDao();
                List<OAP0012Model> rows = fAPVeTraceDao.qryForOAP0012(fsc_range_arr, date_b, date_e, level_1_arr, level_2_arr, cnt_type);

                string guid = "";
                if (rows.Count > 0)
                {
                    guid = Guid.NewGuid().ToString();
                    genRpt(guid, rows, fsc_range, date_b, date_e);
                    var jsonData = new { success = true, guid = guid };
                    return Json(jsonData, JsonRequestBehavior.AllowGet);
                }
                else {
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
            byte[] fileBytes = System.IO.File.ReadAllBytes(Server.MapPath("~/Temp/") + "OAP0012" + "_"+ id + "_" + Session["UserID"].ToString() + ".xlsx");


            string fullPath = Server.MapPath("~/Temp/") + "OAP00012" + "_" + id + "_" + Session["UserID"].ToString() + ".xlsx";
            if (System.IO.File.Exists(fullPath))
                System.IO.File.Delete(fullPath);


            return File(fileBytes, "application/vnd.ms-excel", "態樣統計表.xlsx");
        }


        private void genRpt(string guid, List<OAP0012Model> rows, string fsc_range, string date_b, string date_e)
        {
            string fullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory + "Temp\\"
                            , string.Concat("OAP0012" + "_" + guid + "_" + Session["UserID"].ToString(), ".xlsx"));


            FAPVeCodeDao fAPVeCodeDao = new FAPVeCodeDao();
            //保局範圍
            Dictionary<string, string> fscRangeMap = fAPVeCodeDao.qryByTypeDic("FSC_RANGE");

            //清理大類
            Dictionary<string, string> level1Map = fAPVeCodeDao.qryByTypeDic("CLR_LEVEL1");

            //清理小類
            Dictionary<string, string> level2Map = fAPVeCodeDao.qryByTypeDic("CLR_LEVEL2");

            string[] _fsc_range_arr = fsc_range.Split('|');
            string _fscRange = "";
            foreach (string d in _fsc_range_arr) {
                try
                {
                    _fscRange += fscRangeMap[d] + "/";
                }
                catch (Exception e) {

                }
                
            }
            if (_fscRange.Length > 1)
                _fscRange = _fscRange.Substring(0, _fscRange.Length);

            using (XLWorkbook workbook = new XLWorkbook())
            {
                var ws = workbook.Worksheets.Add("OAP0012統計結果");

                ws.Cell("A1").Value = "清理分類";
                ws.Range("A1", "A3").Merge();
                ws.Cell("A4").Value = "清理大類";
                ws.Cell("A5").Value = "清理小類";

                ws.Cell("B1").Value = "態樣統計表：可列為完成清理之態樣（清理狀態 = 已清理結案）";
                ws.Cell("B2").Value = "資料統計截止日：" + date_b + " ~ " + date_e;
                ws.Cell("B3").Value = "保局範圍：" + _fscRange;
                ws.Cell("B3").Style.Alignment.WrapText = true;
                

                int iCol = 2;

                decimal totCnt = 0;
                decimal totAmt = 0;

                string oLevel1 = "";
                int iLevelo = 0;
                foreach (OAP0012Model d in rows) {
                    if (!oLevel1.Equals(d.level_1)) {
                        if (level1Map.ContainsKey(d.level_1))
                            ws.Cell(4, iCol).Value = level1Map[d.level_1];
                        else
                            ws.Cell(4, iCol).Value = d.level_1;

                        if (!"".Equals(oLevel1))
                            ws.Range(4, iLevelo, 4, iCol - 1).Merge();

                        iLevelo = iCol;
                    }

                    if (level2Map.ContainsKey(d.level_2))
                        ws.Cell(5, iCol).Value = level2Map[d.level_2];
                    else
                        ws.Cell(5, iCol).Value = d.level_2;


                    ws.Range(5, iCol, 5, iCol + 1).Merge();

                    ws.Cell(6, iCol).Value = "筆數";
                    ws.Cell(6, iCol + 1).Value = "金額";
                    ws.Cell(7, iCol).Value = String.Format("{0:n0}", d.cnt);
                    ws.Cell(7, iCol + 1).Value = String.Format("{0:n0}", d.amt);
                    iCol = iCol + 2;
                    oLevel1 = d.level_1;

                    totCnt += Convert.ToDecimal(d.cnt);
                    totAmt += Convert.ToDecimal(d.amt);
                }


                ws.Range(4, iLevelo, 4, iCol - 1).Merge();


                ws.Cell(4, iCol).Value = "合計";
                ws.Range(4, iCol, 5, iCol + 1).Merge();

                ws.Cell(6, iCol).Value = "筆數";
                ws.Cell(6, iCol + 1).Value = "金額";
                ws.Cell(7, iCol).Value = String.Format("{0:n0}", totCnt);
                ws.Cell(7, iCol + 1).Value = String.Format("{0:n0}", totAmt);


                ws.Range(1, 2, 1, iCol + 1).Merge();
                ws.Range(2, 2, 2, iCol + 1).Merge();
                ws.Range(3, 2, 3, iCol + 1).Merge();

                ws.Range(1, 1, 7, iCol + 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                ws.Range(1, 1, 7, iCol + 1).Style.Border.OutsideBorder = XLBorderStyleValues.Thick;
                ws.Range(1, 1, 7, iCol + 1).Style.Border.InsideBorder = XLBorderStyleValues.Thin;

                for (int i = 1; i <= iCol + 1; i++) {
                    ws.Cell(5, i).Style.Alignment.WrapText = true;

                    ws.Cell(4, i).Style.Fill.BackgroundColor = XLColor.AirForceBlue;
                    ws.Cell(5, i).Style.Fill.BackgroundColor = XLColor.AirForceBlue;

                    ws.Cell(4, i).Style.Font.FontColor = XLColor.White;
                    ws.Cell(5, i).Style.Font.FontColor = XLColor.White;
                }


                //ws.Columns().AdjustToContents();  // Adjust column width
                //ws.Rows().AdjustToContents();     // Adjust row heights

                workbook.SaveAs(fullPath);
                // wb.Worksheets.Add(dt1, "人工開立異常檢核報表");

            }
        }
    }
}