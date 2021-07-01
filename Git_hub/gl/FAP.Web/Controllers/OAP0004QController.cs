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
using System.Text;

/// <summary>
/// 功能說明：指定保局範圍查詢作業
/// 初版作者：20200805 Daiyu
/// 修改歷程：20200805 Daiyu
/// 需求單號：
/// 修改內容：初版
/// -----------------------------------------
/// 修改歷程：20210205 daiyu 
/// 需求單號：202101280283-00
/// 修改內容：報表增加"派件日期"、"第一次電訪人員"
/// </summary>
///

namespace FAP.Web.Controllers
{
    [Authorize]
    [CheckSessionFilterAttribute]
    public class OAP0004QController : BaseController
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
                String[] roleInfo = authUtil.chkUserFuncAuth(Session["UserID"].ToString(), "~/OAP0004Q/");
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

            //派件狀態
            ViewBag.dispatchList = sysCodeDao.loadSelectList("AP", "dispatch_status", true);

            //清理階段
            ViewBag.stageList = sysCodeDao.loadSelectList("AP", "ve_stage_status", true);



            FAPVeCodeDao fAPVeCodeDao = new FAPVeCodeDao();
            //指定保局範圍
            ViewBag.fscRangeList = fAPVeCodeDao.loadSelectList("FSC_RANGE", true);
            ViewBag.fscRangejqList = fAPVeCodeDao.jqGridList("FSC_RANGE", true);


            return View();
        }



        [HttpPost]
        public JsonResult qryVeTrace(string check_date_b, string check_date_e, string check_no, string fsc_range
            ,string dispatch_status, string clean_status, string stage_status
              , string page, string sidx, string sord, int rows)
        {
            logger.Info("qryVeTrace begin!!");
            try
            {
                List<OAP0004DModel> dataList = new List<OAP0004DModel>();

                //查詢【FAP_VE_TRACE 逾期未兌領清理記錄檔】
                FAPVeTraceDao fAPVeTraceDao = new FAPVeTraceDao();
                //dataList = fAPVeTraceDao.qryForOAP0004QExport(check_date_b, check_date_e, check_no, fsc_range, dispatch_status, clean_status);
                dataList = fAPVeTraceDao.qryForOAP0004QQuery(check_date_b, check_date_e, check_no, fsc_range, dispatch_status, clean_status);


                dataList = qryStage(dataList);

                if (!"".Equals(StringUtil.toString(stage_status)))
                {
                    string[] stage_arr = stage_status.Split('|');
                    dataList = dataList.Where(x => stage_arr.Contains(x.stage_status)).ToList();
                }

                var amt = dataList.Sum(y =>y.check_amt); // To just bring down the right values


                ViewBag.amt = amt;
                int pageIndex = Convert.ToInt32(page) - 1;
                int totalRecords = dataList.Count();
                int totalPages = (int)Math.Ceiling((float)totalRecords / (float)rows);

                var jsonData = new
                {
                    success = true,
                    total = totalPages,
                    page,
                    records = totalRecords,
                    rows = dataList.Skip(pageIndex * rows).Take(rows).ToList(),
                    amt = amt
                };
                return Json(jsonData, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new { success = false, err = "其它錯誤，請洽系統管理員!!" });
            }
        }

        [HttpPost]
        public JsonResult qryVeTraceAmt(string check_date_b, string check_date_e, string check_no, string fsc_range
            , string dispatch_status, string clean_status, string stage_status)
        {
            logger.Info("qryVeTrace begin!!");
            try
            {
                List<OAP0004DModel> dataList = new List<OAP0004DModel>();

                //查詢【FAP_VE_TRACE 逾期未兌領清理記錄檔】
                FAPVeTraceDao fAPVeTraceDao = new FAPVeTraceDao();
                dataList = fAPVeTraceDao.qryForOAP0004QExport(check_date_b, check_date_e, check_no, fsc_range, dispatch_status, clean_status);

                dataList = qryStage(dataList);

                if (!"".Equals(StringUtil.toString(stage_status)))
                {
                    string[] stage_arr = stage_status.Split('|');
                    dataList = dataList.Where(x => stage_arr.Contains(x.stage_status)).ToList();
                }

                var amt = dataList.Sum(y => y.check_amt); // To just bring down the right values



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
        /// 匯出
        /// </summary>
        /// <param name="check_date_b"></param>
        /// <param name="check_date_e"></param>
        /// <param name="check_no"></param>
        /// <param name="fsc_range"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult execExport(string check_date_b, string check_date_e, string check_no, string fsc_range
            , string dispatch_status, string clean_status, string stage_status)
        {
            logger.Info("execExport begin!!");
            try
            {
                FAPVeTrackProcDao fAPVeTrackProcDao = new FAPVeTrackProcDao();

                FAPVeCodeDao fAPVeCodeDao = new FAPVeCodeDao();
                //保局範圍
                Dictionary<string, string> fscRangeMap = fAPVeCodeDao.qryByTypeDic("FSC_RANGE");

                SysCodeDao sysCodeDao = new SysCodeDao();

                //清理狀態
                Dictionary<string, string> clrStatusMap = sysCodeDao.qryByTypeDic("AP", "CLR_STATUS");
                //清理階段
                Dictionary<string, string> veStageMap = sysCodeDao.qryByTypeDic("AP", "ve_stage_status");
                //派件狀態
                Dictionary<string, string> dispatchMap = sysCodeDao.qryByTypeDic("AP", "dispatch_status");
                //處理結果
                Dictionary<string, string> telResultMap = sysCodeDao.qryByTypeDic("AP", "tel_call");
                //給付細項
                Dictionary<string, string> paidCodeMap = sysCodeDao.qryByTypeDic("AP", "paid_code");

                List<OAP0004DModel> rows = new List<OAP0004DModel>();
                rows.Clear();


                //查詢【FAP_VE_TRACE 逾期未兌領清理記錄檔】
                FAPVeTraceDao fAPVeTraceDao = new FAPVeTraceDao();

                double totDay = 0;
                if (!"".Equals(StringUtil.toString(check_date_b)) & !"".Equals(StringUtil.toString(check_date_e))) {
                    DateTime date1 = Convert.ToDateTime(check_date_b);
                    DateTime date2 = Convert.ToDateTime(check_date_e);
                    totDay = new TimeSpan(date2.Ticks - date1.Ticks).TotalDays;
                }
                  
                string guid = "";
                //if (rows.Count > 0)
                //{
                    writePiaLog(rows.Count, check_date_b + "|" + check_date_e + "|" + check_no + "|" + "fsc_range");

                    guid = Guid.NewGuid().ToString();

                //string fullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory + "Temp\\"
                //        , string.Concat("OAP0004Q" + "_" + guid, ".xlsx"));

                using (var textWriter = new StreamWriter(
                    new FileStream(AppDomain.CurrentDomain.BaseDirectory + "Temp\\" + "OAP0004Q" + "_" + guid + ".csv", FileMode.OpenOrCreate, FileAccess.ReadWrite),
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

                    writer.WriteField("給付細項");
                    writer.WriteField("寄信次數");
                    writer.WriteField("派件狀態");
                    writer.WriteField("處理結果");

                    writer.WriteField("派件日期");
                    writer.WriteField("第一次電訪人員");

                    writer.WriteField("階段狀態");
                    writer.WriteField("階段1日期");
                    writer.WriteField("階段2日期");
                    writer.WriteField("階段3日期");
                    writer.WriteField("階段4日期");
                    writer.WriteField("階段5日期");
                    writer.WriteField("階段6日期");
                    writer.WriteField("階段7日期");

                    writer.NextRecord();

                    //modify by daiyu 20210205 調整查詢條件太大時，分批查詢
                    if (totDay <= 365)
                    {
                        rows = fAPVeTraceDao.qryForOAP0004QExport(check_date_b, check_date_e, check_no, fsc_range, dispatch_status, clean_status);
                        //    rows = fAPVeTrackProcDao.qryForSendCnt(rows);

                        rows = qryStage(rows);

                        if (!"".Equals(StringUtil.toString(stage_status)))
                        {
                            string[] stage_arr = stage_status.Split('|');
                            rows = rows.Where(x => stage_arr.Contains(x.stage_status)).ToList();
                        }

                        if (rows.Count > 0)
                            wirteCsv(writer, rows);
                    }
                    else
                    {

                        //int iCnt = (int)(totDay / 365) + 1;
                        DateTime dateB = new DateTime();
                        DateTime dateE = new DateTime();

                        int iYear = Convert.ToDateTime(check_date_b).Year;
                        int endYear = Convert.ToDateTime(check_date_e).Year;

                        while (iYear.CompareTo(endYear) <= 0) {
                            rows.Clear();

                            dateB = new DateTime(iYear, 1, 1);
                            dateE = new DateTime(iYear, 12, 31);

                            if (iYear.CompareTo(Convert.ToDateTime(check_date_b).Year) == 0)
                                dateB = Convert.ToDateTime(check_date_b);

                            if (iYear.CompareTo(Convert.ToDateTime(check_date_e).Year) == 0)
                                dateE = Convert.ToDateTime(check_date_e);


                            rows = fAPVeTraceDao.qryForOAP0004QExport(DateUtil.DatetimeToString(dateB, "yyyy-MM-dd"), DateUtil.DatetimeToString(dateE, "yyyy-MM-dd")
                                , check_no, fsc_range, dispatch_status, clean_status);
                            //    rows = fAPVeTrackProcDao.qryForSendCnt(rows);

                            rows = qryStage(rows);

                            if (!"".Equals(StringUtil.toString(stage_status)))
                            {
                                string[] stage_arr = stage_status.Split('|');
                                rows = rows.Where(x => stage_arr.Contains(x.stage_status)).ToList();
                            }

                            if (rows.Count > 0)
                                wirteCsv(writer, rows);

                            iYear++;
                        }
                    }
                }

                    var jsonData = new { success = true, guid = guid };
                    return Json(jsonData, JsonRequestBehavior.AllowGet);
                //}
                //else
                //{
                //    var jsonData = new { success = false, err = "無資料" };
                //    return Json(jsonData, JsonRequestBehavior.AllowGet);
                //}


            }
            catch (Exception e)
            {
                logger.Error(e.ToString());
                return Json(new { success = false, err = "其它錯誤，請洽系統管理員!!" });
            }
        }


        private void wirteCsv(CsvWriter writer, List<OAP0004DModel> rows) {
            FAPVeCodeDao fAPVeCodeDao = new FAPVeCodeDao();
            //保局範圍
            Dictionary<string, string> fscRangeMap = fAPVeCodeDao.qryByTypeDic("FSC_RANGE");

            SysCodeDao sysCodeDao = new SysCodeDao();

            //清理狀態
            Dictionary<string, string> clrStatusMap = sysCodeDao.qryByTypeDic("AP", "CLR_STATUS");
            //清理階段
            Dictionary<string, string> veStageMap = sysCodeDao.qryByTypeDic("AP", "ve_stage_status");
            //派件狀態
            Dictionary<string, string> dispatchMap = sysCodeDao.qryByTypeDic("AP", "dispatch_status");
            //處理結果
            Dictionary<string, string> telResultMap = sysCodeDao.qryByTypeDic("AP", "tel_call");
            //給付細項
            Dictionary<string, string> paidCodeMap = sysCodeDao.qryByTypeDic("AP", "paid_code");

            foreach (var d in rows)
            {

                if (fscRangeMap.ContainsKey(d.fsc_range)) //保局範圍
                    writer.WriteField(fscRangeMap[d.fsc_range]);
                else
                    writer.WriteField(d.fsc_range);

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

                if (paidCodeMap.ContainsKey(StringUtil.toString(d.paid_code)))   //給付細項
                    writer.WriteField(d.paid_code + "." + paidCodeMap[d.paid_code]);
                else
                    writer.WriteField(d.paid_code);

                writer.WriteField(d.send_cnt);  //寄信次數


                if (dispatchMap.ContainsKey(StringUtil.toString(d.dispatch_status)))    //派件狀態
                    writer.WriteField(d.dispatch_status + "." + dispatchMap[d.dispatch_status]);
                else
                    writer.WriteField(d.dispatch_status);

                if ("0".Equals(d.dispatch_status)
                    || (!"".Equals(StringUtil.toString(d.dispatch_status)) && "".Equals(StringUtil.toString(d.tel_result)))) {

                    if("".Equals(StringUtil.toString(d.paid_code)))
                        d.tel_result = "12";
                    else
                        d.tel_result = "11";
                }
                   



                if (telResultMap.ContainsKey(StringUtil.toString(d.tel_result))) //處理結果
                    writer.WriteField(d.tel_result + "." + telResultMap[d.tel_result]);
                else
                    writer.WriteField(d.tel_result);



                writer.WriteField(d.dispatch_date);  //派件日期
                writer.WriteField(d.tel_interview_id);  //第一次電訪人員


                //=====階段狀態=====
                //1 電訪處理：當AS400抽件進入清理記錄檔
                //2 二次追踨 - 電訪：當覆核結果為進入追踨
                //3 清理階段：當覆核結果為進入清理
                //4 戶政調閱：當清理階段代碼"7用印送件"結束
                //5 清理結案：當OAP0011A 清理結案申請覆核作業完成
                //6 給付結案：當清理狀態為"1已給付"
                //7 待處理：當覆核結果為PENDING或轉行政部門
                string _stage_status = "1";
                switch (d.status)
                {
                    case "1":   //清理狀態為"1已給付"
                        _stage_status = "6";
                        break;
                    case "2":   //清理狀態為"2已清理結案"
                        _stage_status = "5";
                        break;
                }


                if ("1".Equals(_stage_status))
                {
                    switch (d.tel_appr_result)
                    {
                        case "11":  //進入追蹤
                            _stage_status = "2";
                            break;
                        case "13":  //進入清理
                            _stage_status = "3";

                            string _clean_status = StringUtil.toString(d.clean_status).PadLeft(2, '0');
                            if (!"".Equals(d.stage_4_date) & d.clean_status.CompareTo("07") >= 0)
                                _stage_status = "4";

                            break;
                        case "12":  //PENDING
                            _stage_status = "7";
                            break;
                        case "15":  //轉行政單位
                            _stage_status = "7";
                            break;
                    }

                }

                if (veStageMap.ContainsKey(_stage_status))
                    writer.WriteField(_stage_status + "." + veStageMap[_stage_status]);
                else
                    writer.WriteField(_stage_status);

                writer.WriteField(d.stage_1_date);
                writer.WriteField(d.stage_2_date);
                writer.WriteField(d.stage_3_date);
                writer.WriteField(d.stage_4_date);
                writer.WriteField(d.stage_5_date);
                writer.WriteField(d.re_paid_date);  //階段6的日期即為給付帳務日
                writer.WriteField(d.stage_7_date);


                writer.NextRecord();
            }

        }


        public FileContentResult downloadRpt(String id)
        {
            byte[] fileBytes = System.IO.File.ReadAllBytes(Server.MapPath("~/Temp/") + "OAP0004Q" + "_" + id + ".csv");


            string fullPath = Server.MapPath("~/Temp/") + "OAP0004Q" + "_" + id + ".csv";
            if (System.IO.File.Exists(fullPath))
                System.IO.File.Delete(fullPath);


            return File(fileBytes, "application/vnd.ms-excel", "OAP0004Q.csv");
        }



        /// <summary>
        /// 判斷支票所屬的階段
        /// </summary>
        /// <param name="dataList"></param>
        /// <returns></returns>
        private List<OAP0004DModel> qryStage(List<OAP0004DModel> dataList) {
            foreach (var d in dataList)
            {


                //=====階段狀態=====
                //1 電訪處理：當AS400抽件進入清理記錄檔
                //2 二次追踨 - 電訪：當覆核結果為進入追踨
                //3 清理階段：當覆核結果為進入清理
                //4 戶政調閱：當清理階段代碼"7用印送件"結束
                //5 清理結案：當OAP0011A 清理結案申請覆核作業完成
                //6 給付結案：當清理狀態為"1已給付"
                //7 待處理：當覆核結果為PENDING或轉行政部門
                string _stage_status = "1";
                switch (d.status)
                {
                    case "1":   //清理狀態為"1已給付"
                        _stage_status = "6";
                        break;
                    case "2":   //清理狀態為"2已清理結案"
                        _stage_status = "5";
                        break;
                }


                if ("1".Equals(_stage_status))
                {
                    switch (d.tel_appr_result)
                    {
                        case "11":  //進入追蹤
                            _stage_status = "2";
                            break;
                        case "13":  //進入清理
                            _stage_status = "3";

                            if (d.clean_status.CompareTo("7") >= 0)
                                _stage_status = "4";

                            break;
                        case "12":  //PENDING
                            _stage_status = "7";
                            break;
                        case "15":  //轉行政單位
                            _stage_status = "7";
                            break;
                    }

                }

                d.stage_status = _stage_status;

                
            }

            return dataList;
        }


        private void writePiaLog(int affectRows, string context)
        {
            PIA_LOG_MAIN piaLogMain = new PIA_LOG_MAIN();
            piaLogMain.TRACKING_TYPE = "A";
            piaLogMain.ACCESS_ACCOUNT = Session["UserID"].ToString();
            piaLogMain.ACCOUNT_NAME = Session["UserName"].ToString();
            piaLogMain.PROGFUN_NAME = "OAP0004QController";
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