using FAP.Web.ActionFilter;
using FAP.Web.AS400Models;
using FAP.Web.BO;
using FAP.Web.Daos;
using FAP.Web.Models;
using FAP.Web.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.EasycomClient;
using System.Data.SqlClient;
using System.IO;
using System.Web.Mvc;
using System.Linq;

using System.Threading.Tasks;
using FAP.Web.AS400PGM;
using System.Transactions;
using System.Data.Common;
using ClosedXML.Excel;
using Ionic.Zip;

/// <summary>
/// 功能說明：OAP0042 電訪暨簡訊標準設定作業
/// 初版作者：20200813 Daiyu
/// 修改歷程：20200813 Daiyu
/// 需求單號：
/// 修改內容：初版
/// ----------------------------------------------------
/// 修改歷程：20210118 daiyu
/// 需求單號：
/// 修改內容：修改畫面未排除"保局範圍"時，查不到資料問題
/// ----------------------------------------------------
/// 修改歷程：20210208 daiyu
/// 需求單號：202101280283-00
/// 修改內容：設定項目屬"電話訪問"時，增加可篩選"支票號碼"或"給付對象ID"挑錄特定的支票。
/// </summary>
///

namespace FAP.Web.Controllers
{
    [Authorize]
    [CheckSessionFilterAttribute]
    public class OAP0042Controller : BaseController
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
            string[] roleInfo = authUtil.chkUserFuncAuth(Session["UserID"].ToString(), "~/OAP0042/");
            if (roleInfo != null && roleInfo.Length == 2)
            {
                opScope = roleInfo[0];
                funcName = roleInfo[1];
            }

            ViewBag.funcName = funcName;
            ViewBag.opScope = opScope;


           


            #region 畫面下拉選單


            FPMCODEDao fPMCODEDao = new FPMCODEDao();
            SysCodeDao sysCodeDao = new SysCodeDao();
            FAPVeCodeDao fAPVeCodeDao = new FAPVeCodeDao();

            //設定項目
            ViewBag.typeList = sysCodeDao.loadSelectList("AP", "OAP0042_TYPE", false);

            //資料狀態
            ViewBag.dataStatusList = sysCodeDao.loadSelectList("AP", "DATA_STATUS", true);

            //清理狀態
            ViewBag.clrStatusList = sysCodeDao.loadSelectList("AP", "CLR_STATUS", true);

            //主檔狀態
            ViewBag.ppssStatusList = fPMCODEDao.qryGrpList("PPAASTATUS", "AP");

            //原給付性質
            //ViewBag.oPaidCdList = sysCodeDao.loadSelectList("AP", "O_PAID_CD", true);
            ViewBag.oPaidCdList = fPMCODEDao.qryGrpList("PAID_CDTXT", "AP");

            //覆核結果
            ViewBag.telApprCodeList = sysCodeDao.loadSelectList("AP", "TEL_APPR_CODE", true);

            //簡訊狀態
            ViewBag.smsStatusList = sysCodeDao.loadSelectList("AP", "SMS_STATUS", true);

            //保局範圍
            ViewBag.fscRangeList = fAPVeCodeDao.loadSelectList("FSC_RANGE", true);


            DateTime _sms_dt = DateTime.Now.AddMonths(-3);
            ViewBag.smsDate = (_sms_dt.Year - 1911) + (_sms_dt.Month).ToString().PadLeft(2, '0') + (_sms_dt.Day).ToString().PadLeft(2, '0');

            #endregion

            return View();
        }


      


        /// <summary>
        /// 畫面執行查詢
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult qrySysPara(string type)
        {
            try
            {
                OAP0042Model model = new OAP0042Model();

                //取得設定標準值
                SysParaDao sysParaDao = new SysParaDao();
                List<SYS_PARA> rows = sysParaDao.qryByGrpId("AP", type);


                foreach (SYS_PARA d in rows)
                {
                    switch (d.PARA_ID)
                    {
                        case "appr_code":   //覆核結果
                            model.appr_code = StringUtil.toString(d.PARA_VALUE);
                            break;
                        case "assign_month":    //未派件月份
                            model.assign_month = StringUtil.toString(d.PARA_VALUE);
                            break;
                        case "clr_status":  //清理狀態
                            model.clr_status = StringUtil.toString(d.PARA_VALUE);
                            break;
                        case "fsc_range":   //保局範圍
                            model.fsc_range = StringUtil.toString(d.PARA_VALUE);
                            break;
                        case "o_paid_cd":   //原給付性質
                            model.o_paid_cd = StringUtil.toString(d.PARA_VALUE);
                            break;
                        case "ppaa_status": //主檔狀態
                            model.ppaa_status = StringUtil.toString(d.PARA_VALUE);
                            break;
                        case "rpt_cnt_tp":  //計算條件(P:給付對象ID、C:支票號碼)
                            model.rpt_cnt_tp = StringUtil.toString(d.PARA_VALUE);
                            break;
                        case "stat_amt":    //歸戶金額(起訖)
                            if (!"".Equals(StringUtil.toString(d.PARA_VALUE)))
                            {
                                string[] amtArr = StringUtil.toString(d.PARA_VALUE).Split('|');
                                model.stat_amt_b = amtArr[0];
                                model.stat_amt_e = amtArr[1];
                            }
                            break;
                        case "sms_status":  //簡訊狀態
                            model.sms_status = StringUtil.toString(d.PARA_VALUE);
                            break;
                        case "sms_clear_month": //簡訊清除月份
                            model.sms_clear_month = StringUtil.toString(d.PARA_VALUE);
                            break;
                        case "check_date":  //支票到期日(起訖)
                            if (!"".Equals(StringUtil.toString(d.PARA_VALUE)))
                            {
                                string[] dateArr = StringUtil.toString(d.PARA_VALUE).Split('|');
                                model.check_date_b = dateArr[0];
                                model.check_date_e = dateArr[1];
                            }
                            break;

                        //add by daiyu 20210208
                        case "paid_id": //給付對象ID
                            model.paid_id = StringUtil.toString(d.PARA_VALUE);
                            break;

                        case "check_no": //支票號碼
                            model.check_no = StringUtil.toString(d.PARA_VALUE);
                            break;
                    }
                }

                //判斷是否有資料在覆核中
                SysParaHisDao sysParaHisDao = new SysParaHisDao();
                List<SYS_PARA_HIS> hisList = new List<SYS_PARA_HIS>();
                hisList = sysParaHisDao.qryForGrpId("AP", new string[] { type }, "1");
                if (hisList.Count > 0) {
                    if ("sms_notify_case".Equals(type) & "".Equals(StringUtil.toString(hisList[0].APLY_NO)))
                        model.data_status = "1";
                    else 
                        model.data_status = "2"; 
                }
                else
                    model.data_status = "1";



                return Json(new { success = true, model = model });
            }
            catch (Exception e) {
                logger.Info(e.ToString());
                return Json(new { success = false, err = "其它錯誤，請洽系統管理員!!" });
            }
            
        }


        private bool chk_sms_notify_case(OAP0042Model model) {
            bool inTmp = true;

            SysParaHisDao sysParaHisDao = new SysParaHisDao();
            List<SYS_PARA_HIS> hisList = sysParaHisDao.qryByAplyNo("AP", "sms_notify_case", "");
            if (hisList != null)
            {
                List<SYS_PARA_HIS> paraList = procParaList(model, "", DateTime.Now);

                foreach (SYS_PARA_HIS d in paraList)
                {
                    SYS_PARA_HIS his = hisList.Where(x => x.SYS_CD == d.SYS_CD & x.GRP_ID == d.GRP_ID && x.PARA_ID == d.PARA_ID).FirstOrDefault();

                    if (his == null)
                        inTmp = false;
                    else { 
                        if(!StringUtil.toString(d.PARA_VALUE).Equals(StringUtil.toString(his.PARA_VALUE)))
                            inTmp = false;
                    }
                }
            }
            else
                inTmp = false;

            return inTmp;
        }




        /// <summary>
        /// 匯出報表
        /// </summary>
        /// <param name="model"></param>
        /// <param name="rpt_type"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult execExport(OAP0042Model model, string rpt_type)
        {

            string guid = Guid.NewGuid().ToString();
            string fullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory + "Temp\\"
                            , string.Concat("OAP0042" + "_" + guid + "_" + rpt_type, ".xlsx"));

            logger.Info("rpt_type:" + rpt_type + "  guid:" + guid);
            logger.Info("model.type:" + model.type);
            if ("sms_notify_case".Equals(model.type)) { //一年以下簡訊通知要呼叫AS400取得報表
                if (chk_sms_notify_case(model)) //已經有自AS400撈出來資料
                {
                    SysParaHisDao sysParaHisDao = new SysParaHisDao();
                    List<SYS_PARA_HIS> hisList = sysParaHisDao.qryByAplyNo("AP", "sms_notify_case", "");
                    if (hisList != null)
                    {
                        if ("Y".Equals(StringUtil.toString(hisList[0].RESERVE1)))
                        {
                            VeTelUtil veTelUtil = new VeTelUtil();
                            int cnt = veTelUtil.genSmsNotifyRpt(rpt_type, model, fullPath, "");

                            if (cnt > 0)
                                return Json(new { success = true, guid = guid + "_" + rpt_type });
                            else {
                                var jsonData = new { success = false, err = "無資料!!" };
                                return Json(jsonData, JsonRequestBehavior.AllowGet);
                            }
                               
                        }
                        else {
                            var jsonData = new { success = false, err = "資料尚未自AS400取回，請稍候再試!!" };
                            return Json(jsonData, JsonRequestBehavior.AllowGet);
                        }
                    }

                   
                }
                else {  //需要自AS400撈資料

                    string[] dateArr = StringUtil.toString(model.check_date).Split('|');

                    //將畫面上的資料寫到暫存檔，以供後續使用
                    execSave(model, false);


                    //呼叫AS400 SAPPYCT 產生應付票據一年內未兌現清單
                    //Task.Run(() => procSAPPYCT(dateArr[0], dateArr[1], ""));

                    //回覆畫面待MAIL通知
                    return Json(new { success = true , guid = "" });
                }
            }


            logger.Info("Task.Run" );

            Task.Run(() => genRpt(rpt_type, fullPath, guid + "_" + rpt_type, Session["UserID"].ToString(), Session["UserName"].ToString(), model));
            logger.Info("return ");
            return Json(new { success = true, guid = guid + "_" + rpt_type, bFinish = "N" });

        }




        private async Task genRpt(string rpt_type, string fullPath, string guid, string usr_id, string user_name,  OAP0042Model model)
        {
            logger.Info("genRpt begin");
            await Task.Delay(1);

            try
            {
                bool bRpt = true;

                if ("S".Equals(rpt_type))
                    bRpt = genRptS(fullPath, model);
                else
                    bRpt = genRptD(fullPath, model, usr_id, user_name);


                //if (bRpt)
                //{
                //    var jsonData = new { success = true, guid = guid + "_" + rpt_type };
                //    return Json(jsonData, JsonRequestBehavior.AllowGet);
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
                //return Json(new { success = false, err = "其它錯誤，請洽系統管理員!!" });
            }

        }

            /// <summary>
            /// 呼叫AS400 SAPPYCT 產生應付票據一年內未兌現清單
            /// </summary>
            /// <param name="model"></param>
            /// <returns></returns>
        private async Task procSAPPYCT(string check_date_b, string check_date_e, string aply_no)
        {
            await Task.Delay(1);


            try
            {
                //呼叫AS400 SAPPYCT 產生應付票據一年內未兌現清單
                SAPPYCTUtil sAPPYCTUtil = new SAPPYCTUtil();

                using (EacConnection conn400 = new EacConnection(CommonUtil.GetEasycomConn()))
                {
                    conn400.Open();
                    string rtnCode = sAPPYCTUtil.callSAPPYCT(conn400, check_date_b, check_date_e);
                } 
            }
            catch (Exception e)
            {
                logger.Error(e.ToString());
            }
        }



        /// <summary>
        /// 下載報表
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public FileContentResult downloadRpt(string id)
        {
            byte[] fileBytes = System.IO.File.ReadAllBytes(Server.MapPath("~/Temp/") + "OAP0042" + "_" + id + ".xlsx");


            string fullPath = Server.MapPath("~/Temp/") + "OAP0042" + "_" + id + ".xlsx";
            if (System.IO.File.Exists(fullPath))
                System.IO.File.Delete(fullPath);


            return File(fileBytes, "application/vnd.ms-excel", "OAP0042" + ".xlsx");

        }



        [HttpPost]
        public JsonResult chkRpt(string id)
        {
            try {
                byte[] fileBytes = System.IO.File.ReadAllBytes(Server.MapPath("~/Temp/") + "OAP0042" + "_" + id + ".xlsx");

                string fullPath = Server.MapPath("~/Temp/") + "OAP0042" + "_" + id + ".xlsx";
                if (System.IO.File.Exists(fullPath))
                    return Json(new { bFinish = "Y" });
                else
                    return Json(new { bFinish = "N" });
            }
            catch (Exception e) {
                return Json(new { bFinish = "N" });
            }
        }



        public List<TelDispatchRptModel> getRptList(string stat_type, OAP0042Model model) {
            logger.Info("clr_status:" + model.clr_status);
            logger.Info("o_paid_cd:" + model.o_paid_cd);
            logger.Info("appr_code:" + model.appr_code);
            logger.Info("fsc_range:" + model.fsc_range);
            logger.Info("sms_status:" + model.sms_status);
            logger.Info("ppaa_status:" + model.ppaa_status);
            
            logger.Info("type:" + model.type);

            logger.Info("paid_id:" + model.paid_id);    //add by daiyu20210208
            logger.Info("check_no:" + model.check_no);    //add by daiyu20210208

            string[] status = "".Equals(StringUtil.toString(model.clr_status)) ? null : model.clr_status.Split('|');
            string[] o_paid_cd = "".Equals(StringUtil.toString(model.o_paid_cd)) ? null : model.o_paid_cd.Split('|');
            string[] tel_appr_result = "".Equals(StringUtil.toString(model.appr_code)) ? null : model.appr_code.Split('|');
            string[] fsc_range = "".Equals(StringUtil.toString(model.fsc_range)) ? null : model.fsc_range.Split('|');
            string[] sms_status = "".Equals(StringUtil.toString(model.sms_status)) ? null : model.sms_status.Split('|');

            //查詢清理紀錄檔中符合畫面條件的資料
            FAPVeTraceDao fAPVeTraceDao = new FAPVeTraceDao();
            List<TelDispatchRptModel> rows = fAPVeTraceDao.qryByForOAP0042(status, o_paid_cd, tel_appr_result, sms_status
                , model.rpt_cnt_tp, model.type, model.paid_id, model.check_no); //modify by daiyu 20210208




            logger.Info("rows:" + rows.Count);

            //若有輸入排除條件-"主檔狀態"，需回AS400取得PPAA的資料
            if (!"".Equals(StringUtil.toString(model.ppaa_status)))
            {
                FMNPPAADao fMNPPAADao = new FMNPPAADao();

                using (EacConnection conn400 = new EacConnection(CommonUtil.GetEasycomConn()))
                {
                    conn400.Open();

                    rows = fMNPPAADao.qryForOAP0042(conn400, rows, model.ppaa_status);

                    conn400.Dispose();

                }
            }


            //屬電話訪問時，若有輸入排除條件-"保局範圍"，需加判斷歸戶條件下若有任一筆不在排除的保局範圍的話...其它筆亦不能被排除
            logger.Info("1");
            string[] fscTempIdArr = new string[] { };
            if (!"".Equals(StringUtil.toString(model.fsc_range)))
            {
                if ("P".Equals(model.rpt_cnt_tp))
                {
                    fscTempIdArr = rows.Where(x => !fsc_range.Contains(x.fsc_range)).Select(x => x.temp_id).Distinct().ToArray();
                    //string[] temp_id_arr = rows.Where(x => !fsc_range.Contains(x.fsc_range)).Select(x => x.temp_id).Distinct().ToArray();
                    //rows = rows.Where(x => temp_id_arr.Contains(x.temp_id)).ToList();
                }
                else {
                    fscTempIdArr = rows.Select(x => x.temp_id).Distinct().ToArray();
                    //rows = rows.Where(x => !fsc_range.Contains(x.fsc_range)).ToList();
                }
            } else
                fscTempIdArr = rows.Select(x => x.temp_id).Distinct().ToArray();    //add by daiyu 20210118

            logger.Info("2");
            //判斷歸戶金額
            List<TelDispatchRptModel> tempIdList = rows.GroupBy(o => new { o.temp_id })
              .Select(group => new TelDispatchRptModel
              {
                  temp_id = group.Key.temp_id,
                  cnt = group.Count(),
                  amt = group.Sum(x => Convert.ToInt64(x.main_amt))
              }).OrderBy(x => x.amt).ToList<TelDispatchRptModel>();


            string stat_amt_b = StringUtil.toString(model.stat_amt_b) == "" ? "0" : StringUtil.toString(model.stat_amt_b);
            string stat_amt_e = StringUtil.toString(model.stat_amt_e) == "" ? "999999999999" : StringUtil.toString(model.stat_amt_e);

            //string[] tempIdArr = tempIdList.Where(x => x.amt >= Convert.ToInt64(model.stat_amt_b) & x.amt <= Convert.ToInt64(model.stat_amt_e))
            //    .Select(x => x.temp_id).ToArray();

            //modify by daiyu 20201201
            string[] tempIdArr = tempIdList.Where(x => x.amt >= Convert.ToInt64(model.stat_amt_b) & x.amt <= Convert.ToInt64(model.stat_amt_e)
            & fscTempIdArr.Contains(x.temp_id))
                .Select(x => x.temp_id).ToArray();


            logger.Info("3");

            if ("S".Equals(stat_type))
            {

                foreach (TelDispatchRptModel id in tempIdList.Where(x => tempIdArr.Contains(x.temp_id)))
                    rows.Where(x => x.temp_id == id.temp_id).Select(x => { x.amt = id.amt; return x; }).ToList();


                logger.Info("4");
                return rows.Where(x => tempIdArr.Contains(x.temp_id)).GroupBy(o => new { o.temp_id, o.fsc_range, o.amt }).Select(group => new TelDispatchRptModel
                {
                    temp_id = group.Key.temp_id,
                    fsc_range = group.Key.fsc_range,
                    amt = group.Key.amt,
                    main_amt = group.Sum(x => Convert.ToInt64(x.main_amt))
                }).OrderBy(x => x.temp_id).ToList<TelDispatchRptModel>();
            } else {
                List<string> amt_range_list = new List<string>();
                //查詢級距
                FAPVeCodeDao fAPVeCodeDao = new FAPVeCodeDao();
                List<FAP_VE_CODE> rangeRows = fAPVeCodeDao.qryByGrp("TEL_RANGE");


                List<TelDispatchRptModel> detailList = rows.Where(x => tempIdArr.Contains(x.temp_id)).GroupBy(o => new { o.temp_id })
              .Select(group => new TelDispatchRptModel
              {
                  temp_id = group.Key.temp_id,
                  cnt = group.Count(),
                  amt = group.Sum(x => Convert.ToInt64(x.main_amt))
              }).OrderBy(x => x.amt).ToList<TelDispatchRptModel>();
                logger.Info("4-1");
                foreach (FAP_VE_CODE range in rangeRows.OrderByDescending(x => x.code_id.Length).ThenByDescending(x => x.code_id))
                {
                    decimal amt_range = StringUtil.toString(range.code_id) == "" ? 0 : Convert.ToInt64(range.code_id);
                    string amt_range_desc = (StringUtil.toString(range.code_id) == "" ? "0" : StringUtil.toString(range.code_id)) + " ~ " + StringUtil.toString(range.code_value);
                    amt_range_list.Add(amt_range_desc);
                    long range_l = StringUtil.toString(range.code_id) == "" ? 0 : Convert.ToInt64(range.code_id);
                    long range_h = StringUtil.toString(range.code_value) == "" ? 999999999999 : Convert.ToInt64(range.code_value);

                    detailList.Where(x => x.amt >= range_l & x.amt <= range_h).Select(x => { x.amt_range = amt_range_desc; return x; }).ToList();
                }
                logger.Info("4-2");

                foreach (TelDispatchRptModel d in detailList)
                {
                    rows.Where(x => x.temp_id == d.temp_id).Select(x => { x.amt_range = d.amt_range; return x; }).ToList();
                }

            }
            logger.Info("4-3");
            return rows.Where(x => tempIdArr.Contains(x.temp_id)).ToList();


        }


        /// <summary>
        /// 匯出統計表
        /// </summary>
        /// <param name="fullPath"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        private bool genRptS(string fullPath, OAP0042Model model) {
            bool bRpt = true;

            List<TelDispatchRptModel> grpByIdFscRangeList = getRptList("S", model);

            //if (grpByIdFscRangeList.Count == 0)
            //    return false;

            string[] fsc_range_arr = grpByIdFscRangeList.OrderBy(x => x.fsc_range.Length).ThenBy(x => x.fsc_range).Select(x => x.fsc_range).Distinct().ToArray();



            logger.Info("5");

            //取得該保局範圍的所有統計資料
            FAPVeTraceDao fAPVeTraceDao = new FAPVeTraceDao();
            List<TelDispatchRptModel> fsc_all_stat = fAPVeTraceDao.qryByForOAP0042Fsc(fsc_range_arr, model.rpt_cnt_tp);

            List<string> amt_range_list = new List<string>();
            //查詢級距
            FAPVeCodeDao fAPVeCodeDao = new FAPVeCodeDao();
            List<FAP_VE_CODE> rangeRows = fAPVeCodeDao.qryByGrp("TEL_RANGE");
            foreach (FAP_VE_CODE range in rangeRows.OrderByDescending(x => x.code_id.Length).ThenByDescending(x => x.code_id)) {
                string amt_range = (StringUtil.toString(range.code_id) == "" ? "0" : StringUtil.toString(range.code_id)) + " ~ " + StringUtil.toString(range.code_value);
                amt_range_list.Add(amt_range);
                long range_l = StringUtil.toString(range.code_id) == "" ? 0 : Convert.ToInt64(range.code_id);
                long range_h = StringUtil.toString(range.code_value) == "" ? 999999999999 : Convert.ToInt64(range.code_value);
                grpByIdFscRangeList.Where(x => x.amt >= range_l & x.amt <= range_h).Select(x => { x.amt_range = amt_range; return x; }).ToList();

                logger.Info("===" + amt_range + "===");
                //foreach (TelDispatchRptModel d in grpByIdFscRangeList.Where(x => x.amt >= range_l & x.amt <= range_h).OrderBy(x => x.amt).ToList()) {
                //    logger.Info(d.fsc_range+ "|" + d.temp_id + "|"+d.amt);
                //}
            }


            List<TelDispatchRptModel> rptStatRow = grpByIdFscRangeList.GroupBy(o => new { o.fsc_range, o.amt_range })
           .Select(group => new TelDispatchRptModel
           {
               fsc_range = group.Key.fsc_range,
               amt_range = group.Key.amt_range,
               cnt = group.Count(),
               amt = group.Sum(x => Convert.ToInt64(x.main_amt))
           }).ToList<TelDispatchRptModel>();

            logger.Info("6");


            using (XLWorkbook workbook = new XLWorkbook())
            {
                Dictionary<string, string> fscRangeMap = fAPVeCodeDao.qryByTypeDic("FSC_RANGE");
                var ws = workbook.Worksheets.Add("統計表");
                //string rtp_fsc_range = "";
                //string rpt_amt_range = "";
                int iCol = 0;
                int iRow = 0;
                long fsc_range_tot_cnt = 0;
                long fsc_range_tot_amt = 0;

                ws.Cell(1, 1).Value = "保局範圍";
                ws.Cell(2, 1).Value = "級距";


                if (grpByIdFscRangeList.Count == 0)
                {
                    ws.Cell(3, 1).Value = "資料不存在";
                }
                else {
                    //保局範圍迴圈
                    foreach (string fsc in fsc_range_arr)
                    {
                        fsc_range_tot_cnt = 0;
                        fsc_range_tot_amt = 0;

                        iRow = 3;
                        iCol += 2;
                        if (fscRangeMap.ContainsKey(fsc))
                            ws.Cell(1, iCol).Value = fscRangeMap[fsc];
                        else
                            ws.Cell(1, iCol).Value = fsc;

                        ws.Range(1, iCol, 1, iCol + 1).Merge();


                        ws.Cell(2, iCol).Value = "件數";
                        ws.Cell(2, iCol + 1).Value = "金額";


                        //級距迴圈
                        foreach (string amt_range_desc in amt_range_list)
                        {
                            if (iCol == 2)
                                ws.Cell(iRow, 1).Value = amt_range_desc;


                            TelDispatchRptModel rptItem = rptStatRow.Where(x => x.fsc_range == fsc && x.amt_range == amt_range_desc).FirstOrDefault();



                            if (rptItem == null)
                            {
                                ws.Cell(iRow, iCol).Value = 0;
                                ws.Cell(iRow, iCol + 1).Value = 0;
                            }
                            else
                            {
                                ws.Cell(iRow, iCol).Value = rptItem.cnt;
                                ws.Cell(iRow, iCol + 1).Value = rptItem.amt;

                                fsc_range_tot_cnt += rptItem.cnt;
                                fsc_range_tot_amt += rptItem.amt;
                            }


                            //處理"級距"統計資料
                            ws.Cell(1, iCol + 2).Value = "合計";
                            ws.Range(1, iCol + 2, 1, iCol + 3).Merge();
                            ws.Cell(2, iCol + 2).Value = "件數";
                            ws.Cell(2, iCol + 3).Value = "金額";
                            TelDispatchRptModel rptItem_range_tot = rptStatRow.Where(x => x.amt_range == amt_range_desc).GroupBy(o => new { o.amt_range })
               .Select(group => new TelDispatchRptModel
               {
                   amt_range = group.Key.amt_range,
                   cnt = group.Sum(x => Convert.ToInt64(x.cnt)),
                   amt = group.Sum(x => Convert.ToInt64(x.amt))
               }).FirstOrDefault();

                            if (rptItem_range_tot != null)
                            {
                                ws.Cell(iRow, iCol + 2).Value = rptItem_range_tot.cnt;
                                ws.Cell(iRow, iCol + 3).Value = rptItem_range_tot.amt;
                            }
                            else
                            {
                                ws.Cell(iRow, iCol + 2).Value = 0;
                                ws.Cell(iRow, iCol + 3).Value = 0;
                            }

                            iRow++;

                        }


                        //處理"保局範圍"統計資料
                        if (iCol == 2)
                        {
                            ws.Cell(iRow, 1).Value = "合計";
                            ws.Cell(iRow + 1, 1).Value = "總計";
                            ws.Cell(iRow + 2, 1).Value = "佔比";
                        }


                        //符合畫面條件的保局範圍統計資料
                        TelDispatchRptModel rptItem_fac_tot = rptStatRow.Where(x => x.fsc_range == fsc).GroupBy(o => new { o.fsc_range })
               .Select(group => new TelDispatchRptModel
               {
                   fsc_range = group.Key.fsc_range,
                   cnt = group.Sum(x => Convert.ToInt64(x.cnt)),
                   amt = group.Sum(x => Convert.ToInt64(x.amt))
               }).FirstOrDefault();

                        if (rptItem_fac_tot != null)
                        {
                            ws.Cell(iRow, iCol).Value = rptItem_fac_tot.cnt;
                            ws.Cell(iRow, iCol + 1).Value = rptItem_fac_tot.amt;
                        }
                        else
                        {
                            ws.Cell(iRow, iCol).Value = 0;
                            ws.Cell(iRow, iCol + 1).Value = 0;
                        }



                        //系統內保局範圍的統計資料
                        TelDispatchRptModel fsc_all = fsc_all_stat.Where(x => x.fsc_range == fsc).FirstOrDefault();
                        if (fsc_all == null)
                        {
                            ws.Cell(iRow + 1, iCol).Value = 0;
                            ws.Cell(iRow + 1, iCol + 1).Value = 0;
                        }
                        else
                        {
                            ws.Cell(iRow + 1, iCol).Value = fsc_all.cnt;
                            ws.Cell(iRow + 1, iCol + 1).Value = fsc_all.amt;
                        }


                        if (rptItem_fac_tot == null)
                        {
                            ws.Cell(iRow + 2, iCol).Value = 0;
                            ws.Cell(iRow + 2, iCol + 1).Value = 0;

                        }
                        else
                        {
                            ws.Cell(iRow + 2, iCol).Value = (Math.Round((double)(((double)rptItem_fac_tot.cnt / (double)fsc_all.cnt) * 100), 2)) + "%";
                            ws.Cell(iRow + 2, iCol + 1).Value = (Math.Round((double)(((double)rptItem_fac_tot.amt / (double)fsc_all.amt) * 100), 2)) + "%";

                        }

                    }


                    //處理統計資料
                    ws.Cell(iRow, iCol + 2).Value = rptStatRow.Sum(x => x.cnt);
                    ws.Cell(iRow, iCol + 3).Value = rptStatRow.Sum(x => x.amt);

                    ws.Cell(iRow + 1, iCol + 2).Value = fsc_all_stat.Sum(x => x.cnt);
                    ws.Cell(iRow + 1, iCol + 3).Value = fsc_all_stat.Sum(x => x.amt);


                    ws.Cell(iRow + 2, iCol + 2).Value = (Math.Round((double)(((double)rptStatRow.Sum(x => x.cnt) / (double)fsc_all_stat.Sum(x => x.cnt)) * 100), 2)) + "%";
                    ws.Cell(iRow + 2, iCol + 3).Value = (Math.Round((double)(((double)rptStatRow.Sum(x => x.amt) / (double)fsc_all_stat.Sum(x => x.amt)) * 100), 2)) + "%";


                    ws.Range(2, 1, 2, iCol + 3).Style.Fill.BackgroundColor = XLColor.AirForceBlue;
                    ws.Range(2, 1, 2, iCol + 3).Style.Font.FontColor = XLColor.White;

                    ws.Range(3, 1, iRow + 1, iCol + 3).Style.NumberFormat.Format = "#,##0";
                }

                    
                


                ws.Columns().AdjustToContents();  // Adjust column width
                ws.Rows().AdjustToContents();     // Adjust row heights
                workbook.SaveAs(fullPath);
            }


            logger.Info("7");

            return bRpt;
        }


        /// <summary>
        /// 匯出明細表
        /// </summary>
        /// <param name="fullPath"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        private bool genRptD(string fullPath, OAP0042Model model, string user_id, string user_name)
        {

            logger.Info("genRptD begin");

            bool bRpt = true;

            List<TelDispatchRptModel> grpByIdFscRangeList = getRptList("D", model);

            //if (grpByIdFscRangeList.Count == 0) {
            //    bRpt = false;
            //    return bRpt;
            //}

            //紀錄稽核軌跡

            if (grpByIdFscRangeList.Count == 0)
            {
                writePiaLog(0, "", "X", user_id, user_name);
            }
            else {
                writePiaLog(grpByIdFscRangeList.Count(), grpByIdFscRangeList[0].paid_id, "X", user_id, user_name);
            }

            logger.Info("5");
            try
            {
                using (XLWorkbook workbook = new XLWorkbook())
                {
                    FAPVeCodeDao fAPVeCodeDao = new FAPVeCodeDao();

                    //保局範圍
                    Dictionary<string, string> fscRangeMap = fAPVeCodeDao.qryByTypeDic("FSC_RANGE");

                    //原給付性質
                    //SysCodeDao sysCodeDao = new SysCodeDao();
                    FPMCODEDao fPMCODEDao = new FPMCODEDao();
                    //原給付性質
                    //Dictionary<string, string> oPaidCdMap = sysCodeDao.qryByTypeDic("AP", "O_PAID_CD");
                    Dictionary<string, string> oPaidCdMap = fPMCODEDao.qryByTypeDic("PAID_CDTXT", "AP", true);

                    //金額級距設定範圍
                    List<FAP_VE_CODE> rangeRows = fAPVeCodeDao.qryByGrp("TEL_RANGE");

                    int iRow = 1;

                    var ws = workbook.Worksheets.Add("明細表");

                    ws.Cell(1, 1).Value = "保局範圍";
                    ws.Cell(1, 2).Value = "支票號碼";
                    ws.Cell(1, 3).Value = "支票帳號簡稱";
                    ws.Cell(1, 4).Value = "級距";
                    ws.Cell(1, 5).Value = "回存金額";
                    ws.Cell(1, 6).Value = "支票到期日";
                    ws.Cell(1, 7).Value = "支票金額";
                    ws.Cell(1, 8).Value = "原給付性質";
                    ws.Cell(1, 9).Value = "給付性質";
                    ws.Cell(1, 10).Value = "給付對象 ID";
                    ws.Cell(1, 11).Value = "給付對象姓名";
                    ws.Cell(1, 12).Value = "大系統別";
                    ws.Cell(1, 13).Value = "保單號碼";
                    ws.Cell(1, 14).Value = "保單序號";
                    ws.Cell(1, 15).Value = "重覆碼";
                    ws.Cell(1, 16).Value = "案號";
                    //ws.Cell(1, 16).Value = "要保人 ID";
                    //ws.Cell(1, 17).Value = "要保人姓名";
                    //ws.Cell(1, 18).Value = "被保人 ID";
                    //ws.Cell(1, 19).Value = "被保人姓名";
                    //ws.Cell(1, 20).Value = "通路別";

                    if (grpByIdFscRangeList.Count == 0)
                    {
                        ws.Cell(2, 1).Value = "資料不存在";
                    }
                    else {
                        foreach (TelDispatchRptModel d in grpByIdFscRangeList.OrderBy(x => x.paid_id).ThenBy(x => DateUtil.ChtDateToADDate(x.check_date, '/')))
                        {
                            try
                            {
                                iRow++;

                                if (fscRangeMap.ContainsKey(d.fsc_range))
                                    ws.Cell(iRow, 1).Value = fscRangeMap[d.fsc_range];
                                else
                                    ws.Cell(iRow, 1).Value = d.fsc_range;

                                ws.Cell(iRow, 2).Value = d.check_no;
                                ws.Cell(iRow, 3).Value = d.check_acct_short;

                                FAP_VE_CODE rangeD = rangeRows.Where(x => d.check_amt >= (StringUtil.toString(x.code_id) == "" ? 0 : Convert.ToInt64(x.code_id))
                                & d.check_amt <= (StringUtil.toString(x.code_value) == "" ? 999999999999 : Convert.ToInt64(x.code_value))).FirstOrDefault();


                                // ws.Cell(iRow, 4).Value = rangeD.code_id;
                                //  ws.Cell(iRow, 5).Value = rangeD.code_value;

                                ws.Cell(iRow, 4).Value = d.amt_range;
                                ws.Cell(iRow, 5).Value = d.main_amt;
                                ws.Cell(iRow, 6).Value = "'" + d.check_date;
                                ws.Cell(iRow, 7).Value = d.check_amt;
                                ws.Cell(iRow, 8).Value = d.o_paid_cd;

                                if (oPaidCdMap.ContainsKey(d.o_paid_cd))
                                    ws.Cell(iRow, 9).Value = oPaidCdMap[d.o_paid_cd];


                                ws.Cell(iRow, 10).Value = d.paid_id;
                                ws.Cell(iRow, 11).Value = d.paid_name;
                                ws.Cell(iRow, 12).Value = d.system;
                                ws.Cell(iRow, 13).Value = d.policy_no;
                                ws.Cell(iRow, 14).Value = d.policy_seq;
                                ws.Cell(iRow, 15).Value = d.id_dup;
                                ws.Cell(iRow, 16).Value = d.change_id;
                                //ws.Cell(iRow, 16).Value = d.appl_id;
                                //ws.Cell(iRow, 17).Value = d.appl_name;
                                //ws.Cell(iRow, 18).Value = d.ins_id;
                                //ws.Cell(iRow, 19).Value = d.ins_name;
                                //ws.Cell(iRow, 20).Value = d.sysmark;
                            }
                            catch (Exception ex)
                            {
                                logger.Error(ex.ToString());
                            }
                        }
                    }

                      


                    ws.Range(1, 1, 1, 20).Style.Fill.BackgroundColor = XLColor.AirForceBlue;
                    ws.Range(1, 1, 1, 20).Style.Font.FontColor = XLColor.White;


                    ws.Columns().AdjustToContents();  // Adjust column width
                    ws.Rows().AdjustToContents();     // Adjust row heights


                    workbook.SaveAs(fullPath);

                    
                }

            }
            catch (Exception e)
            {
                logger.Error(e.ToString());
                throw e;
            }


            logger.Info("6");

            return bRpt;
        }





        /// <summary>
        /// 畫面執行申請覆核
        /// </summary>
        /// <param name="model"></param>
        /// <param name="policyNoData"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult execAply(OAP0042Model model, bool getAplyNo)
        {
            try
            {
                string aply_no = execSave(model, getAplyNo);
                return Json(new { success = true, aplyNo = aply_no });
            }
            catch (Exception e) {
                logger.Error("其它錯誤：" + e.ToString());

                return Json(new { success = false, err = "其它錯誤，請洽系統管理員!!" }, JsonRequestBehavior.AllowGet);
            }
        }


        public string execSave(OAP0042Model model, bool getAplyNo)
        {
            bool bSmsNotify = false;
            string aply_no = "";
            string strConn = DbUtil.GetDBFglConnStr();
            using (SqlConnection conn = new SqlConnection(strConn))
            {
                conn.Open();

                SqlTransaction transaction = conn.BeginTransaction("Transaction");
                try
                {
                    DateTime now = DateTime.Now;
                    string[] curDateTime = DateUtil.getCurChtDateTime(3).Split(' ');
                    SysParaHisDao sysParaHisDao = new SysParaHisDao();

                    //一年以下簡訊通知--判斷是否需從AS400重新撈資料
                    if ("sms_notify_case".Equals(model.type) & !chk_sms_notify_case(model))
                        bSmsNotify = true;
                    

                    if ("sms_notify_case".Equals(model.type) & !getAplyNo)
                    {
                        sysParaHisDao.delByAplyNo("AP", "sms_notify_case", "", conn, transaction);
                    }
                    else {
                        if ("sms_notify_case".Equals(model.type) & !bSmsNotify)
                            sysParaHisDao.delByAplyNo("AP", "sms_notify_case", "", conn, transaction);

                        //取得流水號
                        SysSeqDao sysSeqDao = new SysSeqDao();
                        String qPreCode = curDateTime[0].Substring(0, 5);
                        var cId = sysSeqDao.qrySeqNo("AP", "0042", qPreCode).ToString();
                        int seqLen = 12 - ("0042" + qPreCode).Length;
                        aply_no = "0042" + qPreCode + cId.ToString().PadLeft(seqLen, '0');
                    }

                    //資料寫入歷史檔
                    List<SYS_PARA_HIS> paraList = procParaList(model, aply_no, now);


                    //一年以下簡訊通知--多判斷是否需從AS400重撈資料
                    if ("sms_notify_case".Equals(model.type))
                    {
                        if (bSmsNotify)
                        {
                            string[] dateArr = StringUtil.toString(model.check_date).Split('|');
                            Task.Run(() => procSAPPYCT(dateArr[0], dateArr[1], aply_no));
                        }
                        else
                        {
                            foreach (SYS_PARA_HIS para in paraList) //一年以下簡訊通知-->RESERVE1 = Y 表示已從AS400把資料拉回OPEN
                                para.RESERVE1 = "Y";
                        }
                    }
                    
                    sysParaHisDao.insert(paraList, conn, transaction);

                    transaction.Commit();

                    
                }
                catch (Exception e)
                {
                    transaction.Rollback();
                    throw e;
                }
            }

            return aply_no;
        }



        private List<SYS_PARA_HIS> procParaList(OAP0042Model model, string aply_no, DateTime dt) {
            List<SYS_PARA_HIS> paraList = new List<SYS_PARA_HIS>();

            var props = model.GetType().GetProperties();

            foreach (var p in props)
            {
                SYS_PARA_HIS d = new SYS_PARA_HIS();
                d.APLY_NO = aply_no;
                d.SYS_CD = "AP";
                d.GRP_ID = model.type;
                d.APPR_STATUS = "1";
                d.CREATE_UID = Session["UserID"].ToString();
                d.CREATE_DT = dt;

                switch (p.Name)
                {
                    case "rpt_cnt_tp":
                    case "stat_amt":
                    case "assign_month":
                    case "sms_clear_month":
                    case "paid_id": //add by daiyu 20210208
                    case "check_no":    //add by daiyu 20210208
                        d.PARA_ID = p.Name;
                        d.PARA_VALUE = p.GetValue(model, null)?.ToString();
                        paraList.Add(d);
                        break;
                    case "clr_status":
                    case "ppaa_status":
                    case "o_paid_cd":
                    case "appr_code":
                    case "sms_status":
                    case "fsc_range":
                    case "check_date":
                        d.PARA_ID = p.Name;
                        d.PARA_VALUE = p.GetValue(model, null)?.ToString();
                        if (!"".Equals(StringUtil.toString(d.PARA_VALUE)))
                            d.PARA_VALUE = d.PARA_VALUE.Substring(0, d.PARA_VALUE.Length);

                        paraList.Add(d);
                        break;

                }
            }


            return paraList;

        }





        private void writePiaLog(int affectRows, string piaOwner, string executionType, string user_id, string user_name)
        {
            PIA_LOG_MAIN piaLogMain = new PIA_LOG_MAIN();
            piaLogMain.TRACKING_TYPE = "A";
            piaLogMain.ACCESS_ACCOUNT = user_id;
            piaLogMain.ACCOUNT_NAME = user_name;
            piaLogMain.PROGFUN_NAME = "OAP0042Controller";
            piaLogMain.EXECUTION_CONTENT = MaskUtil.maskId(piaOwner);
            piaLogMain.AFFECT_ROWS = affectRows;
            piaLogMain.PIA_TYPE = "0100100000";
            piaLogMain.EXECUTION_TYPE = executionType;
            piaLogMain.ACCESSOBJ_NAME = "FAP_TEL_CHECK";
            piaLogMain.PIA_OWNER1 = piaOwner;
            PiaLogMainDao piaLogMainDao = new PiaLogMainDao();
            piaLogMainDao.Insert(piaLogMain);

        }


    }
}