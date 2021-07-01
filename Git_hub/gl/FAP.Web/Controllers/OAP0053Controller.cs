using ClosedXML.Excel;
using FAP.Web.ActionFilter;
using FAP.Web.BO;
using FAP.Web.Daos;
using FAP.Web.Models;
using FAP.Web.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;


/// <summary>
/// 功能說明：OAP0053 戶政調閱清單
/// 初版作者：20200921 Daiyu
/// 修改歷程：20200921 Daiyu
/// 需求單號：202008120153-01
/// 修改內容：初版
/// ------------------------------------------
/// 修改歷程：20210126 Daiyu
/// 需求單號：
/// 修改內容：1.按調閱完成日+對付對象ID+清理狀態排序
///           2.修改"給付細項"誤帶"原給付性質"問題
///           3.數字靠右(支票金額)
///           4.增加「結案日期」(加在"給付帳務日期"跟"清理狀態"中間)
/// ------------------------------------------
/// </summary>
///

namespace FAP.Web.Controllers
{
    [Authorize]
    [CheckSessionFilterAttribute]
    public class OAP0053Controller : BaseController
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
            string[] roleInfo = authUtil.chkUserFuncAuth(Session["UserID"].ToString(), "~/OAP0053/");
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






        /// <summary>
        /// 匯出
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult execExport(OAP0053Model model)
        {
            logger.Info("execExport begin!!");


            try
            {
                FAPTelCheckDao fAPTelCheckDao = new FAPTelCheckDao();
                List<OAP0053Model> dataList = new List<OAP0053Model>();
                dataList = fAPTelCheckDao.qryOAP0053(model);


                string _content = "date_b:" + model.proc_date_b + "|" + "date_e:" + model.proc_date_e;

                string guid = "";
                if (dataList.Count > 0)
                {
                    writePiaLog(dataList.Count, MaskUtil.maskId(dataList[0].paid_id), "X", _content);
                    guid = Guid.NewGuid().ToString();

                    string fullPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory + "Temp\\"
                            , string.Concat("OAP0053" + "_" + guid, ".xlsx"));
                    using (XLWorkbook wb = new XLWorkbook())
                    {
                        var ws = wb.Worksheets.Add("OAP0053");

                        ws.Cell("A1").Value = "戶政調閱清單";
                        ws.Cell("A2").Value ="資料統計期間：" + StringUtil.toString(model.proc_date_b) + " ~ " + StringUtil.toString(model.proc_date_e);
                        ws.Range(1, 1, 1, 14).Merge();
                        ws.Range(2, 1, 2, 14).Merge();

                        ws.Cell(3, 1).Value = "保局範圍";
                        ws.Cell(3, 2).Value = "給付對象ID";
                        ws.Cell(3, 3).Value = "給付對象姓名";
                        ws.Cell(3, 4).Value = "支票號碼";
                        ws.Cell(3, 5).Value = "支票帳號簡稱";
                        ws.Cell(3, 6).Value = "支票到期日";
                        ws.Cell(3, 7).Value = "支票金額";
                        ws.Cell(3, 8).Value = "原給付性質";
                        ws.Cell(3, 9).Value = "調閱完成日";
                        ws.Cell(3, 10).Value = "給付帳務日期";
                        ws.Cell(3, 11).Value = "結案日期";    //add by daiyu 20200126
                        ws.Cell(3, 12).Value = "清理狀態";
                        ws.Cell(3, 13).Value = "給付細項";
                        ws.Cell(3, 14).Value = "備註說明";


                        int iRow = 3;
                        foreach (OAP0053Model d in dataList.OrderBy(x => DateUtil.ChtDateToADDate(x.proc_date, '/'))    //modify by daiyu 20210126
                            .ThenBy(x => x.paid_id)
                            .ThenBy(x => x.status)) {
                            iRow++;
                            ws.Cell(iRow, 1).Value = d.fsc_range_desc;
                            ws.Cell(iRow, 2).Value = d.paid_id;
                            ws.Cell(iRow, 3).Value = d.paid_name;
                            ws.Cell(iRow, 4).Value = d.check_no;
                            ws.Cell(iRow, 5).Value = d.check_acct_short;
                            ws.Cell(iRow, 6).Value = "'" + d.check_date;
                            ws.Cell(iRow, 7).Value = d.check_amt;   //modify by daiyu 20210126
                            ws.Cell(iRow, 8).Value = d.o_paid_cd_desc;
                            ws.Cell(iRow, 9).Value = "'" + d.proc_date;
                            ws.Cell(iRow, 10).Value = "'" + d.re_paid_date;
                            ws.Cell(iRow, 11).Value = "'" + d.closed_date;
                            ws.Cell(iRow, 12).Value = d.status_desc;
                            ws.Cell(iRow, 13).Value = d.paid_code_desc;
                            ws.Cell(iRow, 14).Value = d.remark;

                            
                        }

                        ws.Range(1, 1, 2, 14).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                        ws.Range(1, 1, iRow, 14).Style.Border.OutsideBorder = XLBorderStyleValues.Thick;
                        ws.Range(1, 1, iRow, 14).Style.Border.InsideBorder = XLBorderStyleValues.Thin;

                        ws.Range(3, 1, 3, 14).Style.Fill.BackgroundColor = XLColor.AirForceBlue;
                        ws.Range(3, 1, 3, 14).Style.Font.FontColor = XLColor.White;

                        ws.Range(4, 7, iRow, 7).Style.NumberFormat.Format = "#,##0";    //add by daiyu 20210126

                        ws.Columns().AdjustToContents();  // Adjust column width
                        ws.Rows().AdjustToContents();     // Adjust row heights


                        wb.SaveAs(fullPath);
                    }

                    var jsonData = new { success = true, guid = guid };
                    return Json(jsonData, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    writePiaLog(dataList.Count, "", "X", _content);
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
            byte[] fileBytes = System.IO.File.ReadAllBytes(Server.MapPath("~/Temp/") + "OAP0053" + "_" + id + ".xlsx");


            string fullPath = Server.MapPath("~/Temp/") + "OAP0053" + "_" + id + ".xlsx";
            if (System.IO.File.Exists(fullPath))
                System.IO.File.Delete(fullPath);


            return File(fileBytes, "application/vnd.ms-excel", "OAP0053.xlsx");
        }


        private void writePiaLog(int affectRows, string piaOwner, string executionType, string content)
        {
            PIA_LOG_MAIN piaLogMain = new PIA_LOG_MAIN();
            piaLogMain.TRACKING_TYPE = "A";
            piaLogMain.ACCESS_ACCOUNT = Session["UserID"].ToString();
            piaLogMain.ACCOUNT_NAME = Session["UserName"].ToString();
            piaLogMain.PROGFUN_NAME = "OAP0053Controller";
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