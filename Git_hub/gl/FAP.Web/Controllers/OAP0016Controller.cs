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
/// 功能說明：基準日帳列應付未付款項之給付情形統計 (含身故及法扣件)
/// 初版作者：20190729 Daiyu
/// 修改歷程：20190729 Daiyu
/// 需求單號：
/// 修改內容：初版
/// ------------------------------------------
/// 需求單號：
/// 修改歷程：20210118 daiyu
/// 修改內容：調整件數計算邏輯
///    已給付為帳務給付日期
///    已清理結案為結案日期
///    在RUN報表當下, 清理狀態為尚未通知的案件,放尚未通知.
///    差額放已通知尚未給付
/// </summary>
///

namespace FAP.Web.Controllers
{
    [Authorize]
    [CheckSessionFilterAttribute]
    public class OAP0016Controller : BaseController
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
                String[] roleInfo = authUtil.chkUserFuncAuth(Session["UserID"].ToString(), "~/OAP0016/");
                if (roleInfo != null && roleInfo.Length == 2)
                {
                    opScope = roleInfo[0];
                    funcName = roleInfo[1];
                }
            }

            ViewBag.opScope = opScope;
            ViewBag.funcName = funcName;

            OAP0016Model model = new OAP0016Model();

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
        public JsonResult execExport(string fsc_range, string date_b, string date_e, string status, string level_1, string level_2, string cnt_type, bool levelSpace)
        {
            logger.Info("execExport begin!!");
            try
            {
                List <OAP0016Model> rows = new List<OAP0016Model>();
                string[] fsc_range_arr = fsc_range.Split('|');
                string[] level_1_arr = level_1.Split('|');
                string[] level_2_arr = level_2.Split('|');

                //查詢【FAP_VE_TRACE 逾期未兌領清理記錄檔】
                FAPVeTraceDao fAPVeTraceDao = new FAPVeTraceDao();

                string strConn = DbUtil.GetDBFglConnStr();
                using (SqlConnection conn = new SqlConnection(strConn))
                {
                    conn.Open();
                    rows = fAPVeTraceDao.qryForOAP0016(fsc_range_arr, date_b, date_e, status, level_1_arr, level_2_arr, cnt_type, levelSpace, conn);    //modify by daiyu 20210118
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
            byte[] fileBytes = System.IO.File.ReadAllBytes(Server.MapPath("~/Temp/") + "OAP0016" + "_"+ id + "_" + Session["UserID"].ToString() + ".xlsx");


            string fullPath = Server.MapPath("~/Temp/") + "OAP0016" + "_" + id + "_" + Session["UserID"].ToString() + ".xlsx";
            if (System.IO.File.Exists(fullPath))
                System.IO.File.Delete(fullPath);


            return File(fileBytes, "application/vnd.ms-excel", "壽險公司應支付而未能給付保戶款項調查表.xlsx");
        }


        private void genRpt(string guid, List<OAP0016Model> rows, string fsc_range, string date_b, string date_e)
        {
            string fullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory + "Temp\\"
                            , string.Concat("OAP0016" + "_" + guid + "_" + Session["UserID"].ToString(), ".xlsx"));


            SysCodeDao sysCodeDao = new SysCodeDao();
            //清理狀態
            Dictionary<string, string> clrStatusMap = sysCodeDao.qryByTypeDic("AP", "CLR_STATUS");


            FAPVeCodeDao fAPVeCodeDao = new FAPVeCodeDao();
            //保局範圍
            Dictionary<string, string> fscRangeMap = fAPVeCodeDao.qryByTypeDic("FSC_RANGE");


            List<OAP0013Model> rptStatRow = rows.GroupBy(o => new { o.rpt_status })
                .Select(group => new OAP0013Model
                {
                    rpt_status = group.Key.rpt_status,
                    cnt = group.Sum(x => Convert.ToInt64(x.cnt)).ToString(),
                    amt = group.Sum(x => Convert.ToInt64(x.amt)).ToString(),
                }).OrderBy(x => x.fsc_range.Length).ThenBy(x => x.fsc_range).ToList<OAP0013Model>();


            Int64 totCnt = 0;
            Int64 totAmt = 0;
            using (XLWorkbook workbook = new XLWorkbook())
            {
                var ws = workbook.Worksheets.Add("OAP0016統計結果");

                ws.Cell("A1").Value = "基準日帳列應付未付款項之給付情形統計";
                ws.Cell("A2").Value = "資料統計截止日：" + date_e;


                ws.Cell("A3").Value = "類型";
                ws.Range("A3", "A4").Merge();
                ws.Cell("A5").Value = "逾期未兌領";

                int iCol = 2;
                int iRow = 0;
                foreach (KeyValuePair<string, string> item in clrStatusMap) {
                    ws.Cell(3, iCol).Value = item.Value;
                    ws.Range(3, iCol, 3, iCol + 1).Merge();

                    ws.Cell(4, iCol).Value = "件數";
                    ws.Cell(4, iCol + 1).Value = "金額";

                    iRow = 5;

                    // ws.Cell(iRow, 1).Value = "合計";

                    OAP0013Model d = rptStatRow.Where(x => x.rpt_status == item.Key).FirstOrDefault();
                    if (d == null)
                    {
                        ws.Cell(iRow, iCol).Value = String.Format("{0:n0}", 0);
                        ws.Cell(iRow, iCol + 1).Value = String.Format("{0:n0}", 0);
                    }
                    else {
                        ws.Cell(iRow, iCol).Value = String.Format("{0:n0}", Convert.ToInt64(d.cnt));
                        ws.Cell(iRow, iCol + 1).Value = String.Format("{0:n0}", Convert.ToInt64(d.amt));


                        totCnt += Convert.ToInt64(d.cnt);
                        totAmt += Convert.ToInt64(d.amt);
                    }
                    iCol = iCol + 2;
                }

                # region 20190822 加合計
                //
                ws.Cell(3, iCol).Value = "合計";
                ws.Range(3, iCol, 3, iCol + 1).Merge();

                ws.Cell(4, iCol).Value = "件數";
                ws.Cell(4, iCol + 1).Value = "金額";

                ws.Cell(iRow, iCol).Value = String.Format("{0:n0}", Convert.ToInt64(totCnt));
                ws.Cell(iRow, iCol + 1).Value = String.Format("{0:n0}", Convert.ToInt64(totAmt));

                iCol = iCol + 2;
                #endregion



                ws.Range(1, 1, 1, iCol - 1).Merge();
                ws.Range(2, 1, 2, iCol - 1).Merge();
    

                ws.Range(1, 1, iRow, iCol - 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                ws.Range(5, 2, iRow, iCol - 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                ws.Range(1, 1, iRow, iCol - 1).Style.Border.OutsideBorder = XLBorderStyleValues.Thick;
                ws.Range(1, 1, iRow, iCol - 1).Style.Border.InsideBorder = XLBorderStyleValues.Thin;



                ws.Columns().AdjustToContents();  // Adjust column width
                ws.Rows().AdjustToContents();     // Adjust row heights
                workbook.SaveAs(fullPath);


            }
        }
    }
}