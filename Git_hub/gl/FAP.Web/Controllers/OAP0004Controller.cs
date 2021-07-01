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
using CsvHelper;
using System.Globalization;
using System.Text;

/// <summary>
/// 功能說明：指定保局範圍作業
/// 初版作者：20190627 Daiyu
/// 修改歷程：20190627 Daiyu
///           需求單號：
///           初版
/// </summary>
///

namespace FAP.Web.Controllers
{
    [Authorize]
    [CheckSessionFilterAttribute]
    public class OAP0004Controller : BaseController
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
                String[] roleInfo = authUtil.chkUserFuncAuth(Session["UserID"].ToString(), "~/OAP0004/");
                if (roleInfo != null && roleInfo.Length == 2)
                {
                    opScope = roleInfo[0];
                    funcName = roleInfo[1];
                }
            }

            ViewBag.opScope = opScope;
            ViewBag.funcName = funcName;


            
            SysCodeDao sysCodeDao = new SysCodeDao();

            //執行功能
            ViewBag.execActionjqList = sysCodeDao.jqGridList("AP", "EXEC_ACTION", true);


            FAPVeCodeDao fAPVeCodeDao = new FAPVeCodeDao();
            //指定保局範圍
            ViewBag.fscRangeList = fAPVeCodeDao.loadSelectList("FSC_RANGE", true);
            ViewBag.fscRangejqList = fAPVeCodeDao.jqGridList("FSC_RANGE", true);


            return View();
        }



        [HttpPost]
        public JsonResult qryVeTrace(string check_date_b, string check_date_e, string check_no, string fsc_range
            , string page, string sidx, string sord, int rows)
        {
            logger.Info("qryVeTrace begin!!");
            try
            {
                List<OAP0004Model> dataList = new List<OAP0004Model>();

                //查詢【FAP_VE_TRACE 逾期未兌領清理記錄檔】
                FAPVeTraceDao fAPVeTraceDao = new FAPVeTraceDao();
                dataList = fAPVeTraceDao.qryForOAP0004(check_date_b, check_date_e, check_no, fsc_range);
                var amt = dataList.Sum(y => Convert.ToInt64(y.check_amt.Replace(".00", ""))); // To just bring down the right values
                

                ViewBag.amt = amt;
                int pageIndex = Convert.ToInt32(page) - 1;
                int totalRecords = dataList.Count();
                int totalPages = (int)Math.Ceiling((float)totalRecords / (float)rows);

                var jsonData = new { success = true,
                    total = totalPages,
                    page,
                    records = totalRecords,
                    rows = dataList.Skip(pageIndex * rows).Take(rows).ToList() };
                return Json(jsonData, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e) {
                return Json(new { success = false, err = "其它錯誤，請洽系統管理員!!" });
            }
        }

        [HttpPost]
        public JsonResult qryVeTraceAmt(string check_date_b, string check_date_e, string check_no, string fsc_range)
        {
            logger.Info("qryVeTrace begin!!");
            try
            {
                List<OAP0004Model> dataList = new List<OAP0004Model>();

                //查詢【FAP_VE_TRACE 逾期未兌領清理記錄檔】
                FAPVeTraceDao fAPVeTraceDao = new FAPVeTraceDao();
                dataList = fAPVeTraceDao.qryForOAP0004(check_date_b, check_date_e, check_no, fsc_range);
                var amt = dataList.Sum(y => Convert.ToInt64(y.check_amt.Replace(".00", ""))); // To just bring down the right values



                var jsonData = new
                {
                    success = true,
                    amt = amt
                };
                return Json(jsonData, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new { success = false, err = "其它錯誤，請洽系統管理員!!" });
            }
        }


        /// <summary>
        /// 畫面執行"申請覆核"
        /// </summary>
        /// <param name="code_type"></param>
        /// <param name="gridData"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult execSave(string check_date_b, string check_date_e, string check_no, string fsc_range, string fsc_range_n
            , List<OAP0004Model> gridData)
        {
            logger.Info("execSave begin");

            string errStr = "";
            try
            {
                List<FAP_VE_TRACE_PROC_HIS> dataList = new List<FAP_VE_TRACE_PROC_HIS>();
                string[] passCheckNo = null;
                if(gridData != null)
                    passCheckNo = gridData.Select(x => x.check_no).ToArray();

                /*------------------ DB處理   begin------------------*/

                string[] curDateTime = DateUtil.getCurChtDateTime().Split(' ');


                //取得流水號
                SysSeqDao sysSeqDao = new SysSeqDao();
                String qPreCode = curDateTime[0];
                var cId = sysSeqDao.qrySeqNo("AP", "VE", qPreCode).ToString();
                int seqLen = 12 - ("VE" + qPreCode).Length;
                var aply_no ="VE" + qPreCode + cId.ToString().PadLeft(seqLen, '0');

                string strConn = DbUtil.GetDBFglConnStr();
                using (SqlConnection conn = new SqlConnection(strConn))
                {
                    conn.Open();

                    SqlTransaction transaction = conn.BeginTransaction("Transaction");
                    try
                    {
                        DateTime dt = DateTime.Now;

                        //新增覆核資料至【FAP_VE_TRACE_HIS 逾期未兌領清理記錄暫存檔】
                        FAPVeTraceHisDao fAPVeTraceHisDao = new FAPVeTraceHisDao();
                        int updateCnt = fAPVeTraceHisDao.insertForOAP0004(dt, Session["UserID"].ToString(), aply_no
                            , check_date_b, check_date_e, check_no, fsc_range, fsc_range_n, passCheckNo
                            , conn, transaction);

                        //新增"覆核資料檔"
                        FAPAplyRecDao fAPAplyRecDao = new FAPAplyRecDao();
                        FAP_APLY_REC aplyRec = new FAP_APLY_REC();
                        aplyRec.aply_type = "VE";
                        aplyRec.appr_stat = "1";
                        aplyRec.memo = check_date_b + "|" + check_date_e + "|" + check_no + "|" + fsc_range + "|" + fsc_range_n + "|" + updateCnt;
                        aplyRec.create_id = Session["UserID"].ToString();
                        aply_no = fAPAplyRecDao.insert(aply_no, aplyRec, conn, transaction);
                        

                        transaction.Commit();
                    }
                    catch (Exception e)
                    {
                        logger.Error(e.ToString());
                        transaction.Rollback();

                        return Json(new { success = false, err = "其它錯誤，請洽系統管理員!!" });
                    }
                }

                return Json(new { success = true, aply_no = aply_no, err = errStr });

            }
            catch (Exception e)
            {
                logger.Error(e.ToString());
                return Json(new { success = false, err = "其它錯誤，請洽系統管理員!!" });

            }
        }



        /// <summary>
        /// 匯出
        /// </summary>
        /// <param name="check_date_b"></param>
        /// <param name="check_date_e"></param>
        /// <param name="check_no"></param>
        /// <param name="fsc_range"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult execExport(string check_date_b, string check_date_e, string check_no, string fsc_range)
        {
            logger.Info("execExport begin!!");
            try
            {
                FAPVeTrackProcDao fAPVeTrackProcDao = new FAPVeTrackProcDao();

                SysCodeDao sysCodeDao = new SysCodeDao();

                //清理狀態
                Dictionary<string, string> clrStatusMap = sysCodeDao.qryByTypeDic("AP", "CLR_STATUS");
                //清理階段
                Dictionary<string, string> veStageMap = sysCodeDao.qryByTypeDic("AP", "ve_stage_status");
                //派件狀態
                Dictionary<string, string> dispatchMap = sysCodeDao.qryByTypeDic("AP", "dispatch_status");

                List<OAP0004DModel> rows = new List<OAP0004DModel>();

                //查詢【FAP_VE_TRACE 逾期未兌領清理記錄檔】
                FAPVeTraceDao fAPVeTraceDao = new FAPVeTraceDao();
                rows = fAPVeTraceDao.qryForOAP0004Export(check_date_b, check_date_e, check_no, fsc_range);
                //    rows = fAPVeTrackProcDao.qryForSendCnt(rows);

                string guid = "";
                if (rows.Count > 0)
                {
                    writePiaLog(rows.Count, check_date_b + "|" + check_date_e + "|" + check_no + "|" + "fsc_range");

                    guid = Guid.NewGuid().ToString();


                    string fullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory + "Temp\\"
                            , string.Concat("OAP0004" + "_" + guid, ".xlsx"));

                    using (var textWriter = new StreamWriter(
                        new FileStream(AppDomain.CurrentDomain.BaseDirectory + "Temp\\" + "OAP0004" + "_" + guid + ".csv", FileMode.OpenOrCreate, FileAccess.ReadWrite),
        Encoding.GetEncoding(950)


                        ))
                    {
                        var writer = new CsvWriter(textWriter);
                        writer.Configuration.Delimiter = ",";
                        writer.Configuration.Encoding = Encoding.GetEncoding("GB2312");

                        writer.WriteField("保局範圍");
                        writer.WriteField("大系統別");
                        writer.WriteField("支票號碼／匯費序號");
                        writer.WriteField("支票帳號簡稱");
                        writer.WriteField("給付對象 ID");
                        writer.WriteField("給付對象姓名");
                        writer.WriteField("支票金額");
                        writer.WriteField("支票到期日");
                        writer.WriteField("給付帳務日期");
                        writer.WriteField("再給付方式");
                        writer.WriteField("清理狀態");
                        writer.WriteField("結案日期");
                        writer.WriteField("再給付日期");


                        writer.NextRecord();

                        foreach (var d in rows)
                        {
                            writer.WriteField(d.fsc_range); //保局範圍
                            writer.WriteField(d.system);    //大系統別
                            writer.WriteField(d.check_no);     //支票號碼／匯費序號
                            writer.WriteField(d.check_acct_short);  //支票帳號簡稱
                            writer.WriteField(d.paid_id);   //給付對象 ID
                            writer.WriteField(d.paid_name); //給付對象姓名
                            writer.WriteField(d.check_amt); //支票金額
                            writer.WriteField(d.check_date);    //支票到期日
                            writer.WriteField(d.re_paid_date);  //給付帳務日期
                            writer.WriteField(d.re_paid_type);  //再給付方式

                            if (clrStatusMap.ContainsKey(d.status)) //清理狀態
                                writer.WriteField(d.status + "." + clrStatusMap[d.status]);
                            else
                                writer.WriteField(d.status);

                            writer.WriteField(d.closed_date);//結案日期
                            writer.WriteField(d.re_paid_date_n);    //再給付日期
                            

                            writer.NextRecord();
                        }
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
        //[HttpPost]
        //public JsonResult execExport(string check_date_b, string check_date_e, string check_no, string fsc_range)
        //{
        //    logger.Info("execExport begin!!");
        //    try
        //    {
        //        FAPVeTrackProcDao fAPVeTrackProcDao = new FAPVeTrackProcDao();

        //        SysCodeDao sysCodeDao = new SysCodeDao();

        //        //清理狀態
        //        Dictionary<string, string> clrStatusMap = sysCodeDao.qryByTypeDic("AP", "CLR_STATUS");
        //        //清理階段
        //        Dictionary<string, string> veStageMap = sysCodeDao.qryByTypeDic("AP", "ve_stage_status");
        //        //派件狀態
        //        Dictionary<string, string> dispatchMap = sysCodeDao.qryByTypeDic("AP", "dispatch_status");

        //        List<OAP0004DModel> rows = new List<OAP0004DModel>();

        //        //查詢【FAP_VE_TRACE 逾期未兌領清理記錄檔】
        //        FAPVeTraceDao fAPVeTraceDao = new FAPVeTraceDao();
        //        rows = fAPVeTraceDao.qryForOAP0004Export(check_date_b, check_date_e, check_no, fsc_range);
        //    //    rows = fAPVeTrackProcDao.qryForSendCnt(rows);

        //        string guid = "";
        //        if (rows.Count > 0)
        //        {
        //            writePiaLog(rows.Count, check_date_b + "|" + check_date_e + "|" + check_no + "|" + "fsc_range");

        //            guid = Guid.NewGuid().ToString();


        //            string fullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory + "Temp\\"
        //                    , string.Concat("OAP0004" + "_" + guid, ".xlsx"));
        //            using (XLWorkbook wb = new XLWorkbook())
        //            {
        //                var ws = wb.Worksheets.Add("OAP0004");

        //                ws.Cell(1, 1).Value = "保局範圍";
        //                ws.Cell(1, 2).Value = "大系統別";
        //                ws.Cell(1, 3).Value = "支票號碼／匯費序號";
        //                ws.Cell(1, 4).Value = "支票帳號簡稱";
        //                ws.Cell(1, 5).Value = "給付對象 ID";
        //                ws.Cell(1, 6).Value = "給付對象姓名";
        //                ws.Cell(1, 7).Value = "支票金額";
        //                ws.Cell(1, 8).Value = "支票到期日";
        //                ws.Cell(1, 9).Value = "給付帳務日期";
        //                ws.Cell(1, 10).Value = "再給付方式";
        //                ws.Cell(1, 11).Value = "清理狀態";
        //                ws.Cell(1, 12).Value = "結案日期";
        //                ws.Cell(1, 13).Value = "再給付日期";
        //                ws.Cell(1, 14).Value = "給付細項";
        //                ws.Cell(1, 15).Value = "派件狀態";
        //                ws.Cell(1, 16).Value = "寄信次數";
        //                ws.Cell(1, 17).Value = "階段狀態";
        //                ws.Cell(1, 18).Value = "階段1日期";
        //                ws.Cell(1, 19).Value = "階段2日期";
        //                ws.Cell(1, 20).Value = "階段3日期";
        //                ws.Cell(1, 21).Value = "階段4日期";
        //                ws.Cell(1, 22).Value = "階段5日期";
        //                ws.Cell(1, 23).Value = "階段6日期";
        //                ws.Cell(1, 24).Value = "階段7日期";

        //                ws.Range(1, 1, 1, 24).Style.Fill.BackgroundColor = XLColor.AirForceBlue;
        //                ws.Range(1, 1, 1, 24).Style.Font.FontColor = XLColor.White;


        //                int iRow = 1;
        //                foreach (OAP0004DModel d in rows) {
        //                    iRow++;
        //                    ws.Cell(iRow, 1).Value = d.fsc_range;
        //                    ws.Cell(iRow, 2).Value = d.system;
        //                    ws.Cell(iRow, 3).Value = d.check_no;
        //                    ws.Cell(iRow, 4).Value = d.check_acct_short;
        //                    ws.Cell(iRow, 5).Value = d.paid_id;
        //                    ws.Cell(iRow, 6).Value = d.paid_name;
        //                    ws.Cell(iRow, 7).Value = d.check_amt;   // Convert.ToDecimal(d.check_amt).ToString("#,##0");
        //                    ws.Cell(iRow, 8).Value = "'" + d.check_date;
        //                    ws.Cell(iRow, 9).Value = "'" + d.re_paid_date;
        //                    ws.Cell(iRow, 10).Value = d.re_paid_type;

        //                    if (clrStatusMap.ContainsKey(d.status))
        //                        ws.Cell(iRow, 11).Value = d.status + "." + clrStatusMap[d.status];
        //                    else
        //                        ws.Cell(iRow, 11).Value = d.status;

        //                    ws.Cell(iRow, 12).Value = "'" + d.closed_date;
        //                    ws.Cell(iRow, 13).Value = "'" + d.re_paid_date_n;

        //                    ws.Cell(iRow, 14).Value = d.paid_code;

        //                    if (dispatchMap.ContainsKey(StringUtil.toString(d.dispatch_status)))
        //                        ws.Cell(iRow, 15).Value = d.dispatch_status + "." + dispatchMap[d.dispatch_status];
        //                    else
        //                        ws.Cell(iRow, 15).Value = d.dispatch_status;


        //                    ws.Cell(iRow, 16).Value = d.send_cnt;


        //                    //=====階段狀態=====
        //                    //1 電訪處理：當AS400抽件進入清理記錄檔
        //                    //2 二次追踨 - 電訪：當覆核結果為進入追踨
        //                    //3 清理階段：當覆核結果為進入清理
        //                    //4 戶政調閱：當清理階段代碼"7用印送件"結束
        //                    //5 清理結案：當OAP0011A 清理結案申請覆核作業完成
        //                    //6 給付結案：當清理狀態為"1已給付"
        //                    //7 待處理：當覆核結果為PENDING或轉行政部門
        //                    string stage_status = "1";
        //                    switch (d.status) {
        //                        case "1":   //清理狀態為"1已給付"
        //                            stage_status = "6";
        //                            break;
        //                        case "2":   //清理狀態為"2已清理結案"
        //                            stage_status = "5";
        //                            break; 
        //                    }


        //                    if ("1".Equals(stage_status)) {
        //                        switch (d.tel_appr_result) {
        //                            case "11":  //進入追蹤
        //                                stage_status = "2";
        //                                break;
        //                            case "13":  //進入清理
        //                                stage_status = "3";

        //                                if (d.clean_status.CompareTo("7") >= 0)
        //                                    stage_status = "4";

        //                                break;
        //                            case "12":  //PENDING
        //                                stage_status = "7";
        //                                break;
        //                            case "15":  //轉行政單位
        //                                stage_status = "7";
        //                                break;
        //                        }

        //                    }

        //                    if (veStageMap.ContainsKey(stage_status))
        //                        ws.Cell(iRow, 17).Value = stage_status + "." + veStageMap[stage_status];
        //                    else
        //                        ws.Cell(iRow, 17).Value = stage_status;

        //                    ws.Cell(iRow, 18).Value = "'" + d.stage_1_date;
        //                    ws.Cell(iRow, 19).Value = "'" + d.stage_2_date;
        //                    ws.Cell(iRow, 20).Value = "'" + d.stage_3_date;
        //                    ws.Cell(iRow, 21).Value = "'" + d.stage_4_date;
        //                    ws.Cell(iRow, 22).Value = "'" + d.stage_5_date;
        //                    ws.Cell(iRow, 23).Value = "'" + d.stage_6_date;
        //                    ws.Cell(iRow, 24).Value = "'" + d.stage_7_date;


        //                }


        //                // var ws = wb.Worksheets.Add(dt, "OAP0004");
        //                ws.Column(7).Style.NumberFormat.Format = "#,##0";

        //                //ws.Columns().AdjustToContents();  // Adjust column width
        //                //ws.Rows().AdjustToContents();     // Adjust row heights

        //                wb.SaveAs(fullPath);
        //            }

        //            var jsonData = new { success = true, guid = guid };
        //            return Json(jsonData, JsonRequestBehavior.AllowGet);
        //        }
        //        else
        //        {
        //            var jsonData = new { success = false, err = "無資料" };
        //            return Json(jsonData, JsonRequestBehavior.AllowGet);
        //        }


        //    }
        //    catch (Exception e)
        //    {
        //        return Json(new { success = false, err = "其它錯誤，請洽系統管理員!!" });
        //    }
        //}


        public FileContentResult downloadRpt(String id)
        {
            byte[] fileBytes = System.IO.File.ReadAllBytes(Server.MapPath("~/Temp/") + "OAP0004" + "_" + id + ".csv");


            string fullPath = Server.MapPath("~/Temp/") + "OAP0004" + "_" + id + ".csv";
            if (System.IO.File.Exists(fullPath))
                System.IO.File.Delete(fullPath);


            return File(fileBytes, "application/vnd.ms-excel", "OAP0004.csv");
        }


        private void writePiaLog(int affectRows, string context)
        {
            PIA_LOG_MAIN piaLogMain = new PIA_LOG_MAIN();
            piaLogMain.TRACKING_TYPE = "A";
            piaLogMain.ACCESS_ACCOUNT = Session["UserID"].ToString();
            piaLogMain.ACCOUNT_NAME = Session["UserName"].ToString();
            piaLogMain.PROGFUN_NAME = "OAP0004Controller";
            piaLogMain.EXECUTION_CONTENT = context;
            piaLogMain.AFFECT_ROWS = affectRows;
            piaLogMain.PIA_TYPE = "0100100000";
            piaLogMain.EXECUTION_TYPE = "Q";
            piaLogMain.ACCESSOBJ_NAME = "FAP_VE_TRACE";
            piaLogMain.PIA_OWNER1 = "";
            PiaLogMainDao piaLogMainDao = new PiaLogMainDao();
            piaLogMainDao.Insert(piaLogMain);

        }
    }
}