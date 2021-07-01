using ClosedXML.Excel;
using FAP.Web.ActionFilter;
using FAP.Web.BO;
using FAP.Web.Daos;
using FAP.Web.Models;
using FAP.Web.Service.Actual;
using FAP.Web.Service.Interface;
using FAP.Web.ViewModels;
using NPOI.SS.Formula.Functions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using static FAP.Web.BO.Utility;

/// <summary>
/// 功能說明：OAP0052 逾期清理追踨報表
/// 初版作者：20200921 Mark
/// 修改歷程：20200921 Mark
/// 需求單號：202008120153-00
/// 修改內容：初版
/// -----------------------------------------
/// 需求單號：
/// 修改歷程：20210128 daiyu
/// 修改內容：1.「清理暨逾期處理清單」，只印有清理階段完成日的…及現在在哪一個階段的資料  
///           2.「清理處理清單」
///             2.1 拿掉L~N欄
///             2.2 用電訪編號+給付對象ID排序
///             2.3 不依處理人員切SHEET
/// ------------------------------------------
/// </summary>
///

namespace FAP.Web.Controllers
{
    [Authorize]
    [CheckSessionFilterAttribute]
    public class OAP0052Controller : CommonController
    {
        static private NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        private IBAP0004 BAP0004;

        public OAP0052Controller()
        {
            BAP0004 = new BAP0004();
        }


        public ActionResult Index()
        {
            UserAuthUtil authUtil = new UserAuthUtil();

            string opScope = "";
            string funcName = "";
            if (Session["UserID"] != null)
            {
                String[] roleInfo = authUtil.chkUserFuncAuth(Session["UserID"].ToString(), "~/OAP0052/");
                if (roleInfo != null && roleInfo.Length == 2)
                {
                    opScope = roleInfo[0];
                    funcName = roleInfo[1];
                }
            }
            ViewBag.opScope = opScope;
            ViewBag.funcName = funcName;
            return View();
        }

        /// <summary>
        /// 執行匯出
        /// </summary>
        /// <param name="exec"></param>
        /// <param name="type"></param>
        /// <param name="clean_date_b"></param>
        /// <param name="clean_date_e"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult Export(string exec, string type, string clean_date_b, string clean_date_e, string status)
        {
            MSGReturnModel _result = new MSGReturnModel();
            MemoryStream _stream = new MemoryStream();
      
            if (exec == "OAP0052_VE_Clear_Scheduler")
            {
                var result = BAP0004.VE_Clear_Scheduler(AccountController.CurrentUserId, type);
                _result.RETURN_FLAG = result.Item1;
                _result.DESCRIPTION = result.Item2;
                _result.REASON_CODE = result.Item3;
                //_stream = result.Item3;
            }
            else if (exec == "OAP0052_VE_Level_Detail")
            {
                var result = BAP0004.VE_Level_Detail(AccountController.CurrentUserId, type);
                _result.RETURN_FLAG = result.Item1;
                _result.DESCRIPTION = result.Item2;
                _result.REASON_CODE = result.Item3;
                // _stream = result.Item3;
            }
            else if (exec == "OAP0052_CLEAN")
            {
                var result = genCleanRpt(AccountController.CurrentUserId, clean_date_b, clean_date_e, status);
                _result.RETURN_FLAG = result.Item1;
                _result.DESCRIPTION = result.Item2;
                _result.REASON_CODE = result.Item3;
                // _stream = result.Item3;
            }
            //執行成功,無符合資料
            if (_result.RETURN_FLAG && _result.REASON_CODE.Length == 0)
            {
                _result.RETURN_FLAG = false;
            }
            if (_result.RETURN_FLAG && _result.REASON_CODE.Length != 0)
            {
                Cache.Invalidate($@"DL_OAP0052");
                Cache.Set($@"DL_OAP0052", (_result.REASON_CODE));
                try
                {

                    string content = exec + "|" + clean_date_b + "|" + clean_date_e + "|" + status;

                    PIA_LOG_MAIN piaLogMain = new PIA_LOG_MAIN();
                    PiaLogMainDao piaLogMainDao = new PiaLogMainDao();

                    piaLogMain.TRACKING_TYPE = "A";
                    piaLogMain.ACCESS_ACCOUNT = AccountController.CurrentUserId;
                    piaLogMain.ACCOUNT_NAME = "";
                    piaLogMain.PROGFUN_NAME = "OAP0052Controller";
                    piaLogMain.EXECUTION_CONTENT = content;
                    piaLogMain.AFFECT_ROWS = 1;
                    piaLogMain.PIA_TYPE = "1100000000";
                    piaLogMain.EXECUTION_TYPE = "X";
                    piaLogMain.ACCESSOBJ_NAME = "FAP_VE_TRACE";
                    piaLogMainDao.Insert(piaLogMain);
                }
                catch (Exception ex)
                {
                    NLog.LogManager.GetCurrentClassLogger().Error(ex.ToString());
                }
            }
            return Json(_result);
        }

        private Tuple<bool, string, string> genCleanRpt(string user_id, string clean_date_b, string clean_date_e, string status)
        {
            bool _flag = true; //執行結果
            string _msg = string.Empty; //訊息
            string guid = "";

            FAPTelInterviewHisDao fAPTelInterviewHisDao = new FAPTelInterviewHisDao();
            FGLCALEDao fGLCALEDao = new FGLCALEDao();

            //查出所有的清理階段
            FAPTelCodeDao fAPTelCodeDao = new FAPTelCodeDao();
            List<FAP_TEL_CODE> cleanStdList = fAPTelCodeDao.qryByGrp("tel_clean");

            SysCodeDao sysCodeDao = new SysCodeDao();
            Dictionary<string, string> telCleanMap = sysCodeDao.qryByTypeDic("AP", "tel_clean");
            foreach (FAP_TEL_CODE d in cleanStdList) {
                if (telCleanMap.ContainsKey(d.code_id))
                    d.remark = telCleanMap[d.code_id];
                else
                    d.remark = d.code_id;
            }


            FPMCODEDao fPMCODEDao = new FPMCODEDao();
            //原給付性質
            //Dictionary<string, string> oPaidCdMap = sysCodeDao.qryByTypeDic("AP", "O_PAID_CD");
            Dictionary<string, string> oPaidCdMap = fPMCODEDao.qryByTypeDic("PAID_CDTXT", "AP", true);

            //清理狀態
            Dictionary<string, string> clrStatusMap = sysCodeDao.qryByTypeDic("AP", "CLR_STATUS");
            


            FAPVeCodeDao fAPVeCodeDao = new FAPVeCodeDao();
            //清理大類
            Dictionary<string, string> level1Map = fAPVeCodeDao.qryByTypeDic("CLR_LEVEL1");

            //清理大類
            Dictionary<string, string> level2Map = fAPVeCodeDao.qryByTypeDic("CLR_LEVEL2");


            List<string> prodIdList = cleanStdList.Select(x => x.proc_id).Distinct().ToList();

            //查出仍在清理中的案件
            List<OAP0052Model> dataList = new List<OAP0052Model>();
            FAPTelInterviewDao fAPTelInterviewDao = new FAPTelInterviewDao();

            DateTime dtB = Convert.ToDateTime(DateUtil.As400ChtDateToADDate(clean_date_b));
            DateTime dtE = Convert.ToDateTime(DateUtil.As400ChtDateToADDate(clean_date_e));
            DateTime now = DateTime.Now;
            dataList = fAPTelInterviewDao.qryForOAP0052(dtB, dtE, status);

            if (dataList.Count == 0) {
                _flag = false;
                _msg = "沒有符合查詢條件的資料";

                return new Tuple<bool, string, string>(_flag, _msg, guid);
            }

            guid = Guid.NewGuid().ToString();
            //XLWorkbook Workbook = new XLWorkbook();

            string fullPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory + "Temp\\", string.Concat("BAP0004" + "_" + guid, ".xlsx"));

            using (XLWorkbook workbook = new XLWorkbook())
            {
                //foreach (string prod_id in prodIdList)    //modify by daiyu 20210222
                //{
                   // var ws = workbook.Worksheets.Add(prod_id);
                var ws = workbook.Worksheets.Add("清理處理清單");
                ws.Cell("A1").Value = "清理處理清單";
                    ws.Cell("A2").Value = $@"資料統計截止日：{DateTime.Now.ToString("yyyy-MM-dd")}";
                    //ws.Range(1, 1, 1, 14).Merge();
                    //ws.Range(2, 1, 2, 14).Merge();
                    ws.Range(1, 1, 1, 12).Merge();  //modify by daiyu 20210128
                    ws.Range(2, 1, 2, 12).Merge();

                ws.Cell(3, 1).Value = "處理人員";
                ws.Cell(3, 2).Value = "電訪編號";
                    ws.Cell(3, 3).Value = "給付對象 ID";
                    ws.Cell(3, 4).Value = "給付對象姓名";
                    ws.Cell(3, 5).Value = "支票號碼";
                    ws.Cell(3, 6).Value = "支票號碼簡稱";
                    ws.Cell(3, 7).Value = "支票到期日";
                    ws.Cell(3, 8).Value = "支票金額";
                    ws.Cell(3, 9).Value = "原給付性質";
                    ws.Cell(3, 10).Value = "清理大類";
                    ws.Cell(3, 11).Value = "清理小類";
                    ws.Cell(3, 12).Value = "清理狀態";
                  //  ws.Cell(3, 12).Value = "帳務日期";


                    //delete by daiyu 20210128
                    //ws.Cell(3, 12).Value = "清理階段";
                    //ws.Cell(3, 13).Value = "清理標準";
                    //ws.Cell(3, 14).Value = "清理階段完成日期";


                    //ws.Cell(3, 15).Value = "逾期天數";
                    //ws.Cell(3, 16).Value = "逾期原因";

                    int iRow = 4;
                    //foreach (FAP_TEL_CODE clean_status in cleanStdList.Where(x => x.proc_id == prod_id).OrderBy(x => x.code_id.Length).ThenBy(x => x.code_id))
                    foreach (FAP_TEL_CODE clean_status in cleanStdList.OrderBy(x => x.proc_id).ThenBy(x => x.code_id.Length).ThenBy(x => x.code_id))    //modify by daiyu 20210222
                    {
                        string tel_proc_no = "";
                        foreach (OAP0052Model d in dataList.Where(x => x.clean_status == clean_status.code_id).OrderBy(x => x.tel_proc_no)) {
                            //if (tel_proc_no.Equals(d.tel_proc_no))
                            //    continue;

                            
                            tel_proc_no = d.tel_proc_no;
                        ws.Cell(iRow, 1).Value = clean_status.proc_id;
                        ws.Cell(iRow, 2).Value = "'" + d.tel_proc_no;
                            ws.Cell(iRow, 3).Value = d.paid_id;
                            ws.Cell(iRow, 4).Value = d.paid_name;
                            ws.Cell(iRow, 5).Value = d.check_no;
                            ws.Cell(iRow, 6).Value = d.check_acct_short;
                            ws.Cell(iRow, 7).Value = "'" + d.check_date;
                            ws.Cell(iRow, 8).Value = d.check_amt;

                            if (oPaidCdMap.ContainsKey(d.o_paid_cd))
                                ws.Cell(iRow, 9).Value = oPaidCdMap[d.o_paid_cd];
                            else
                                ws.Cell(iRow, 9).Value = d.o_paid_cd;

                            if (level1Map.ContainsKey(d.level_1))
                                ws.Cell(iRow, 10).Value = level1Map[d.level_1];
                            else
                                ws.Cell(iRow, 10).Value = d.level_1;

                            if (level2Map.ContainsKey(d.level_2))
                                ws.Cell(iRow, 11).Value = level2Map[d.level_2];
                            else
                                ws.Cell(iRow, 11).Value = d.level_2;


                            if(clrStatusMap.ContainsKey(d.status))
                                ws.Cell(iRow, 12).Value = clrStatusMap[d.status];
                            else
                                ws.Cell(iRow, 12).Value = d.status;

                            //ws.Cell(iRow, 12).Value = "'" + d.re_paid_date;


                            //delete by daiyu 20210128
                            //List<FAP_TEL_INTERVIEW_HIS> hisList = fAPTelInterviewHisDao.qryByTelProcNo(d.tel_proc_no, new string[] { "3" }, new string[] { "2" });


                            //foreach (FAP_TEL_CODE clean in cleanStdList.OrderBy(x => x.code_id.Length).ThenBy(x => x.code_id)) {
                            //    ws.Cell(iRow, 12).Value = clean.remark;
                            //    ws.Cell(iRow, 13).Value = clean.std_1;


                            //    FAP_TEL_INTERVIEW_HIS his = hisList.Where(x => x.clean_status == clean.code_id).FirstOrDefault();

                            //    if (his != null) {
                            //        try
                            //        {
                            //            ws.Cell(iRow, 14).Value = "'" + DateUtil.ADDateToChtDate(Convert.ToDateTime(his.clean_f_date), 3, "/");
                            //        }
                            //        catch (Exception e) {
                            //            ws.Cell(iRow, 14).Value = his.clean_f_date;
                            //        }
                            //    }


                            //    if (d.clean_status.Equals(clean.code_id))
                            //    {
                            //        //List<string> dates  =  fGLCALEDao.GetWorkDate(DateUtil.ADDateToChtDate(d.clean_date, 3, ""), DateUtil.ADDateToChtDate(now, 3, ""));

                            //        ////已達追蹤標準 (時間範圍抓到的工作天數大於設定資料的天數
                            //        //var _VE_day = dates.Count - clean.std_1;



                            //        //if (_VE_day > 0) {
                            //        //    ws.Cell(iRow, 15).Value = _VE_day;
                            //        //    ws.Cell(iRow, 16).Value = $@"逾{clean.remark}階段";
                            //        //}

                            //        //ws.Range(iRow, 12, iRow, 16).Style.Fill.BackgroundColor = XLColor.Green;
                            //        //ws.Range(iRow, 12, iRow, 16).Style.Font.FontColor = XLColor.White;

                            //        ws.Range(iRow, 12, iRow, 14).Style.Fill.BackgroundColor = XLColor.Green;
                            //        ws.Range(iRow, 12, iRow, 14).Style.Font.FontColor = XLColor.White;
                            //    }
                            //    iRow++;
                            //}
                            //end delete 20210128
                            iRow++;
                        }


                    }

                    ws.Range(1, 1, 2, 12).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                    ws.Range(1, 1, iRow - 1, 12).Style.Border.OutsideBorder = XLBorderStyleValues.Thick;
                    ws.Range(1, 1, iRow - 1, 12).Style.Border.InsideBorder = XLBorderStyleValues.Thin;

                    ws.Range(3, 1, 3, 12).Style.Fill.BackgroundColor = XLColor.Yellow;
                    // ws.Range(3, 1, 3, 15).Style.Font.FontColor = XLColor.White;

                    //ws.Range(3, 12, 3, 14).Style.Fill.BackgroundColor = XLColor.Green;
                    //ws.Range(3, 12, 3, 14).Style.Font.FontColor = XLColor.White;

                    ws.Columns().AdjustToContents();  // Adjust column width
                    ws.Rows().AdjustToContents();     // Adjust row heights
                //}

                

                workbook.SaveAs(fullPath);

            }




            return new Tuple<bool, string, string>(_flag, _msg, guid);
        }

        public FileContentResult downloadRpt(string id)
        {
            byte[] fileBytes = System.IO.File.ReadAllBytes(Server.MapPath("~/Temp/") + "BAP0004" + "_" + id + ".xlsx");


            string fullPath = Server.MapPath("~/Temp/") + "BAP0004" + "_" + id + ".xlsx";
            if (System.IO.File.Exists(fullPath))
                System.IO.File.Delete(fullPath);


            return File(fileBytes, "application/vnd.ms-excel", "OAP0052.xlsx");
        }
    }
}