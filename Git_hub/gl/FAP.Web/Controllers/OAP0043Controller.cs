using FAP.Web.ActionFilter;
using FAP.Web.BO;
using FAP.Web.Daos;
using FAP.Web.Models;
using FAP.Web.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Web.Mvc;
using System.Linq;
using ClosedXML.Excel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using NPOI.HSSF.UserModel;
using System.Data;
using System.Threading.Tasks;
using System.Data.EasycomClient;

/// <summary>
/// 功能說明：OAP0043 電訪派件標準設定作業
/// 初版作者：20200826 Daiyu
/// 修改歷程：20200826 Daiyu
/// 需求單號：
/// 修改內容：初版
/// ----------------------------------------------------
/// 修改歷程：20210125 daiyu 
/// 需求單號：
/// 修改內容：1.無法列印明細表(民國年月轉 DATE 問題)
///           2.匯入時，若有多個sheet，有sheet沒有資料時，無法回應作業訊息到畫面上
/// </summary>
///

namespace FAP.Web.Controllers
{
    [Authorize]
    [CheckSessionFilterAttribute]
    public class OAP0043Controller : BaseController
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
            string[] roleInfo = authUtil.chkUserFuncAuth(Session["UserID"].ToString(), "~/OAP0043/");
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

            //保局範圍
            ViewBag.fscRangeList = fAPVeCodeDao.jqGridList("FSC_RANGE", true);




            #endregion

            return View();
        }


        public JsonResult execSaveTemp(string type, string fsc_range, string amt_range, List<FAP_TEL_CODE_HIS> gridData)
        {
            FAPTelCodeHisDao fAPTelCodeHisDao = new FAPTelCodeHisDao();
            FAPTelCheckHisDao fAPTelCheckHisDao = new FAPTelCheckHisDao();
            FAPTelCheckDao fAPTelCheckDao = new FAPTelCheckDao();

            try
            {
                string strConn = DbUtil.GetDBFglConnStr();
                using (SqlConnection conn = new SqlConnection(strConn))
                {
                    conn.Open();

                    SqlTransaction transaction = conn.BeginTransaction("Transaction");

                    DateTime now = DateTime.Now;
                    string[] curDateTime = BO.DateUtil.getCurChtDateTime(4).Split(' ');

                    //先將未正式申請覆核的資料刪除
                    //1.FAP_TEL_CODE_HIS 電訪標準設定暫存檔
                    //2.FAP_TEL_CHECK_HIS 電訪支票暫存檔
                    FAP_TEL_CODE_HIS d = new FAP_TEL_CODE_HIS();
                    d.aply_no = "";
                    d.code_type = type;
                    d.fsc_range = fsc_range;
                    d.amt_range = amt_range;
                    fAPTelCodeHisDao.delForOAP0043(d, conn, transaction);

                    FAP_TEL_CHECK_HIS check_d = new FAP_TEL_CHECK_HIS();
                    check_d.aply_no = "";
                    check_d.tel_std_type = type;
                    check_d.fsc_range = fsc_range;
                    check_d.amt_range = amt_range;
                    fAPTelCheckHisDao.delForOAP0043(check_d, conn, transaction);


                    //查詢本次範圍的明細
                    List<OAP0043Model> rows = new List<OAP0043Model>();

                    if ("sms_notify_case".Equals(type))
                        rows = fAPTelCheckDao.qryForOAP0043SmsNotify(type, fsc_range, amt_range);
                    else
                        rows = fAPTelCheckDao.qryForOAP0043(type, fsc_range, amt_range);


                    List<OAP0043Model> rowsGrp = rows.GroupBy(o => new { o.temp_id }).Select(group => new OAP0043Model
                    {
                        temp_id = group.Key.temp_id,
                        check_amt = group.Sum(x => Convert.ToInt64(x.check_amt))
                    }).OrderBy(x => x.check_amt).ToList<OAP0043Model>();


                    //將畫面資料新增至暫存檔(不寫入覆核單號)
                    int i = 0;

                    if (gridData != null) {
                        foreach (FAP_TEL_CODE_HIS m in gridData.OrderByDescending(x => x.std_2))
                        {

                            m.aply_no = d.aply_no;
                            m.code_type = d.code_type;
                            m.code_id = d.fsc_range + "-" + d.amt_range + "-" + m.proc_id;
                            m.fsc_range = d.fsc_range;
                            m.amt_range = d.amt_range;
                            m.update_id = Session["UserID"].ToString();
                            m.update_datetime = now;


                            fAPTelCodeHisDao.insert(now, m, conn, transaction);


                            foreach (OAP0043Model grp in rowsGrp.Where(x => x.data_status == ""))
                            {
                                i++;
                                foreach (OAP0043Model key in rows.Where(x => x.temp_id.Equals(grp.temp_id)).ToList())
                                {
                                    FAP_TEL_CHECK_HIS his = new FAP_TEL_CHECK_HIS();
                                    his.aply_no = "";
                                    his.tel_std_type = d.code_type;
                                    his.system = key.system;
                                    his.check_acct_short = key.check_acct_short;
                                    his.check_no = key.check_no;
                                    his.tel_proc_no = "";
                                    his.tel_interview_id = m.proc_id;
                                    his.update_datetime = now;
                                    his.update_id = Session["UserID"].ToString();
                                    fAPTelCheckHisDao.insertFromFormal(now, his, "", "0043", conn, transaction);
                                    grp.data_status = "1";
                                }

                                if (i == m.std_2)
                                {
                                    i = 0;
                                    break;
                                }

                            }

                        }
                    }
                    



                    transaction.Commit();

                }


                return Json(new { success = true, dataList = gridData });
            }
            catch (Exception e)
            {
                logger.Info(e.ToString());
                return Json(new { success = false, err = "其它錯誤，請洽系統管理員!!" });
            }

        }



        /// <summary>
        /// 以"保局範圍 + 級距"查詢設定項目的派件人員比例設定
        /// </summary>
        /// <param name="type"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult qryDispatch(string type, string key)
        {
            FAPTelCodeHisDao fAPTelCodeHisDao = new FAPTelCodeHisDao();

            string[] keyArr = key.Split('-');

            try
            {
                List<OAP0043Model> rows = fAPTelCodeHisDao.qryByOAP0043("", type, keyArr[0], keyArr[1]);
                CommonUtil commonUtil = new CommonUtil();
                Dictionary<string, ADModel> empMap = new Dictionary<string, ADModel>();

                foreach (OAP0043Model d in rows) {
                    if (!empMap.ContainsKey(d.proc_id)) {
                        ADModel adModel = new ADModel();
                        adModel = commonUtil.qryEmp(d.proc_id);
                        empMap.Add(d.proc_id, adModel);
                    }
                    d.proc_name = empMap[d.proc_id].name;
                }



                return Json(new { success = true, rows = rows });
            }
            catch (Exception e) {
                logger.Info(e.ToString());
                return Json(new { success = false, err = "其它錯誤，請洽系統管理員!!" });
            }

        }


        /// <summary>
        /// 畫面執行查詢
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult qryTelCheck(string type)
        {
            try
            {

                FAPVeCodeDao fAPVeCodeDao = new FAPVeCodeDao();
                string dispatchList = "";


                //查詢SYS_PARA，確定AS400資料已拉到OPEN
                string dataReady = "N";
                SysParaDao sysParaDao = new SysParaDao();
                List<SYS_PARA> paraList = new List<SYS_PARA>();
                paraList = sysParaDao.qryByGrpId("AP", type);

                if (paraList.Count > 0)
                    dataReady = StringUtil.toString(paraList[0].RESERVE1);


                //第一次電訪人員
                List<FAP_VE_CODE> dispatchRows = new List<FAP_VE_CODE>();

                if ("tel_assign_case".Equals(type))
                    dispatchRows = fAPVeCodeDao.qryByGrp("TEL_DISPATCH");
                else
                    dispatchRows = fAPVeCodeDao.qryByGrp("SMS_DISPATCH");


                foreach (var item in dispatchRows)
                {
                    ADModel adModel = new ADModel();
                    CommonUtil commonUtil = new CommonUtil();
                    adModel = commonUtil.qryEmp(item.code_id);

                    dispatchList += item.code_id.Trim() + ":" + (StringUtil.toString(adModel.name) == "" ? item.code_id.Trim() : StringUtil.toString(adModel.name)) + ";";
                }
                dispatchList = dispatchList.Substring(0, dispatchList.Length - 1) + "";



                FAPTelCheckDao fAPTelCheckDao = new FAPTelCheckDao();

                List<OAP0043Model> rows = new List<OAP0043Model>();
                if ("sms_notify_case".Equals(type))
                    rows = fAPTelCheckDao.qryForOAP0043SmsNotify(type, "", "");
                else
                    rows = fAPTelCheckDao.qryForOAP0043(type, "", "");

                List<OAP0043Model> dataList = new List<OAP0043Model>();
                if (rows.Count == 0)
                    return Json(new { success = true, dataList = dataList, dataReady = dataReady });

                //add by daiyu 20201216 若有覆核中的資料，不可以再執行覆核作業
                FAPTelCodeHisDao fAPTelCodeHisDao = new FAPTelCodeHisDao();
                OAP0043Model _his = fAPTelCodeHisDao.qryByOAP0043A(new string[] { type }).FirstOrDefault();
                string _aply_no = "";
                if (_his != null)
                    _aply_no = StringUtil.toString(_his.aply_no);


                dataList = procStat(type, rows);


                return Json(new { success = true, dataReady = dataReady, dataList = dataList, dispatchList = dispatchList, aply_no = _aply_no });
            }
            catch (Exception e) {
                logger.Info(e.ToString());
                return Json(new { success = false, err = "其它錯誤，請洽系統管理員!!" });
            }

        }


        private List<OAP0043Model> procStat(string type, List<OAP0043Model> rows) {
            List<OAP0043Model> dataList = new List<OAP0043Model>();

            FAPVeCodeDao fAPVeCodeDao = new FAPVeCodeDao();



            FAPTelCheckDao fAPTelCheckDao = new FAPTelCheckDao();
            //依保局範圍
            List<OAP0043Model> rowsByFscRange = rows.GroupBy(x => new { x.fsc_range, x.amt_range, x.temp_id })
                .Select(group => new OAP0043Model
                {
                    fsc_range = group.Key.fsc_range,
                    amt_range = group.Key.amt_range,
                    temp_id = group.Key.temp_id,
                    check_amt = group.Sum(o => o.check_amt)
                }).ToList();



            //查詢級距
            List<FAP_VE_CODE> rangeRows = fAPVeCodeDao.qryByGrp("TEL_RANGE");
            foreach (FAP_VE_CODE range in rangeRows.OrderByDescending(x => x.code_id.Length).ThenByDescending(x => x.code_id))
            {
                string amt_range = StringUtil.toString(range.code_id) == "" ? "0" : range.code_id;
                string amt_range_desc = (StringUtil.toString(range.code_id) == "" ? "0" : StringUtil.toString(range.code_id)) + " ~ " + StringUtil.toString(range.code_value);

                //long range_l = StringUtil.toString(range.code_id) == "" ? 0 : Convert.ToInt64(range.code_id);
                //long range_h = StringUtil.toString(range.code_value) == "" ? 999999999999 : Convert.ToInt64(range.code_value);

                //rowsByFscRange.Where(x => x.check_amt >= range_l & x.check_amt <= range_h).Select(x => {  x.amt_range_desc = amt_range_desc; return x; }).ToList();
                //rowsByFscRange.Where(x => x.check_amt >= range_l & x.check_amt <= range_h).Select(x => {  x.amt_range = amt_range ; return x; }).ToList();

                rowsByFscRange.Where(x => x.amt_range == amt_range).Select(x => { x.amt_range_desc = amt_range_desc; return x; }).ToList();
            }


            //依保局範圍+級距
            List<OAP0043Model> rowsByFscRangeAmt = rowsByFscRange.GroupBy(x => new { x.fsc_range, x.amt_range, x.amt_range_desc })
              .Select(group => new OAP0043Model
              {
                  fsc_range = group.Key.fsc_range,
                  amt_range = group.Key.amt_range,
                  amt_range_desc = group.Key.amt_range_desc,
                  cnt = group.Count(),
                  check_amt = group.Sum(o => o.check_amt)
              }).ToList();


            //查詢已分派的資料
            FAPTelCheckHisDao fAPTelCheckHisDao = new FAPTelCheckHisDao();
            List<TelDispatchRptModel> _his_list = fAPTelCheckHisDao.qryOAP0043DRpt(type).Where(x => x.tel_interview_id != "").GroupBy(x => new { x.fsc_range, x.amt_range, x.temp_id })
                     .Select(group => new TelDispatchRptModel
                     {
                         fsc_range = group.Key.fsc_range,
                         amt_range = group.Key.amt_range,
                         temp_id = group.Key.temp_id
                     }).ToList();


            string fsc_range = "";
            foreach (OAP0043Model d in rowsByFscRangeAmt.OrderBy(x => x.fsc_range.Length).ThenBy(x => x.fsc_range)
                .ThenByDescending(x => x.amt_range.Length).ThenByDescending(x => x.amt_range))
            {

                logger.Info("fsc_range:" + d.fsc_range + "-" + d.fsc_range_desc);
                logger.Info("amt_range:" + d.amt_range);

                if (!fsc_range.Equals(d.fsc_range))
                {
                    OAP0043Model fsc_tot = new OAP0043Model();
                    fsc_tot.key = d.fsc_range;
                    fsc_tot.fsc_range = d.fsc_range;
                    fsc_tot.cnt = rowsByFscRange.Where(x => x.fsc_range == d.fsc_range).Count();
                    fsc_tot.cnt_noId = fsc_tot.cnt - _his_list.Where(x => x.fsc_range == d.fsc_range).Count();


                    dataList.Add(fsc_tot);
                    fsc_range = d.fsc_range;
                }

                d.key = d.fsc_range + "-" + d.amt_range;

                d.cnt_noId = d.cnt - _his_list.Where(x => x.fsc_range == d.fsc_range & x.amt_range == d.amt_range).Count();
                dataList.Add(d);

            }


            return dataList;
        }
            


        [HttpPost]
        public async Task<JsonResult> execExport(OAP0043Model model, string rpt_type)
        {
            


            string guid = "";
            guid = Guid.NewGuid().ToString();
            string fullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory + "Temp\\"
                            , string.Concat("OAP0043" + "_" + guid + "_" + rpt_type, ".xlsx"));

            logger.Info("rpt_type:" + rpt_type + " guid:" + guid);

            try
            {
                bool bRpt = true;

                if ("S".Equals(rpt_type))
                    bRpt = genRptS(fullPath, model);
                else {
                    Task.Run(() => genRptDTask(rpt_type, fullPath, guid + "_" + rpt_type, Session["UserID"].ToString(), Session["UserName"].ToString(), model));
                    logger.Info("return ");
                    return Json(new { success = true, guid = guid + "_" + rpt_type, bFinish = "N" });

                }
                    


                if (bRpt)
                {
                    var jsonData = new { success = true, guid = guid + "_" + rpt_type };
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



        private async Task genRptDTask(string rpt_type, string fullPath, string guid, string usr_id, string user_name, OAP0043Model model)
        {
            logger.Info("genRptDTask begin");
            await Task.Delay(1);

            try
            {
                await genRptD(fullPath, model, usr_id, user_name);

            }
            catch (Exception e)
            {
                logger.Error(e.ToString());
                //return Json(new { success = false, err = "其它錯誤，請洽系統管理員!!" });
            }

        }



        [HttpPost]
        public JsonResult chkRpt(string id)
        {
            try
            {
                byte[] fileBytes = System.IO.File.ReadAllBytes(Server.MapPath("~/Temp/") + "OAP0043" + "_" + id + ".xlsx");

                string fullPath = Server.MapPath("~/Temp/") + "OAP0043" + "_" + id + ".xlsx";
                if (System.IO.File.Exists(fullPath))
                    return Json(new { bFinish = "Y" });
                else
                    return Json(new { bFinish = "N" });
            }
            catch (Exception e)
            {
                return Json(new { bFinish = "N" });
            }
        }



        public FileContentResult downloadRpt(string id)
        {
            byte[] fileBytes = System.IO.File.ReadAllBytes(Server.MapPath("~/Temp/") + "OAP0043" + "_" + id + ".xlsx");


            string fullPath = Server.MapPath("~/Temp/") + "OAP0043" + "_" + id + ".xlsx";
            if (System.IO.File.Exists(fullPath))
                System.IO.File.Delete(fullPath);


            return File(fileBytes, "application/vnd.ms-excel", "OAP0043" + ".xlsx");
        }



        //   public List<TelDispatchRptModel>  getRptList(string stat_type, OAP0043Model model) {
        //string[] status = "".Equals(StringUtil.toString(model.clr_status)) ? null : model.clr_status.Split('|');
        //string[] o_paid_cd = "".Equals(StringUtil.toString(model.o_paid_cd)) ? null : model.o_paid_cd.Split('|');
        //string[] tel_appr_result = "".Equals(StringUtil.toString(model.appr_code)) ? null : model.appr_code.Split('|');
        //string[] fsc_range = "".Equals(StringUtil.toString(model.fsc_range)) ? null : model.fsc_range.Split('|');
        //string[] sms_status = "".Equals(StringUtil.toString(model.sms_status)) ? null : model.sms_status.Split('|');

        ////查詢清理紀錄檔中符合畫面條件的資料
        //FAPVeTraceDao fAPVeTraceDao = new FAPVeTraceDao();
        //List<TelDispatchRptModel> rows = fAPVeTraceDao.qryByForOAP0042(status, o_paid_cd, tel_appr_result, sms_status, model.rpt_cnt_tp);

        ////若有輸入排除條件-"主檔狀態"，需回AS400取得PPAA的資料
        //if (!"".Equals(StringUtil.toString(model.ppaa_status)))
        //{
        //    FMNPPAADao fMNPPAADao = new FMNPPAADao();

        //    using (EacConnection conn400 = new EacConnection(CommonUtil.GetEasycomConn()))
        //    {
        //        conn400.Open();

        //        rows = fMNPPAADao.qryForOAP0042(conn400, rows, model.ppaa_status);
        //    }
        //}


        ////屬電話訪問時，若有輸入排除條件-"保局範圍"，需加判斷歸戶條件下若有任一筆不在排除的保局範圍的話...其它筆亦不能被排除
        //logger.Info("1");
        //if (!"".Equals(StringUtil.toString(model.fsc_range)))
        //{
        //    if ("P".Equals(model.rpt_cnt_tp))
        //    {
        //        string[] temp_id_arr = rows.Where(x => !fsc_range.Contains(x.fsc_range)).Select(x => x.temp_id).Distinct().ToArray();
        //        rows = rows.Where(x => temp_id_arr.Contains(x.temp_id)).ToList();
        //    }
        //    else
        //        rows = rows.Where(x => !fsc_range.Contains(x.fsc_range)).ToList();

        //}


        ////判斷歸戶金額
        //List<TelDispatchRptModel> tempIdList = rows.GroupBy(o => new { o.temp_id })
        //  .Select(group => new TelDispatchRptModel
        //  {
        //      temp_id = group.Key.temp_id,
        //      cnt = group.Count(),
        //      amt = group.Sum(x => Convert.ToInt64(x.main_amt))
        //  }).OrderBy(x => x.amt).ToList<TelDispatchRptModel>();


        //string stat_amt_b = StringUtil.toString(model.stat_amt_b) == "" ? "0" : StringUtil.toString(model.stat_amt_b);
        //string stat_amt_e = StringUtil.toString(model.stat_amt_e) == "" ? "999999999999" : StringUtil.toString(model.stat_amt_e);

        //string[] tempIdArr = tempIdList.Where(x => x.amt >= Convert.ToInt64(model.stat_amt_b) & x.amt <= Convert.ToInt64(model.stat_amt_e))
        //    .Select(x => x.temp_id).ToArray();



        //if ("S".Equals(stat_type))
        //    return rows.Where(x => tempIdArr.Contains(x.temp_id)).GroupBy(o => new { o.temp_id, o.fsc_range }).Select(group => new TelDispatchRptModel
        //    {
        //        temp_id = group.Key.temp_id,
        //        fsc_range = group.Key.fsc_range,
        //        amt = group.Sum(x => Convert.ToInt64(x.check_amt))
        //    }).OrderBy(x => x.temp_id).ToList<TelDispatchRptModel>();
        //else
        //    return rows.Where(x => tempIdArr.Contains(x.temp_id)).ToList();



        //   }




        /// <summary>
        /// 匯出統計表
        /// </summary>
        /// <param name="fullPath"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        private bool genRptS(string fullPath, OAP0043Model model) {
            bool bRpt = true;
            OAP0043AController oAP0043AController = new OAP0043AController();

            List<OAP0043Model> dataList = oAP0043AController.getDispatchList(model.type, "", "");

            //查詢本次範圍的明細
            List<OAP0043Model> rowsD = new List<OAP0043Model>();
            FAPTelCheckDao fAPTelCheckDao = new FAPTelCheckDao();
            if ("sms_notify_case".Equals(model.type))
                rowsD = fAPTelCheckDao.qryForOAP0043SmsNotify(model.type, "", "");
            else
                rowsD = fAPTelCheckDao.qryForOAP0043(model.type, "", "");

            FAPVeCodeDao fAPVeCodeDao = new FAPVeCodeDao();
            Dictionary<string, string> fsc_map = fAPVeCodeDao.qryByTypeDic("FSC_RANGE");


            List<OAP0043Model> rowsStat = procStat(model.type, rowsD);

            decimal totAmt = 0;
            decimal totCnt = 0;
            decimal totDispatchCnt = 0;

            using (XLWorkbook workbook = new XLWorkbook())
            {
                var ws = workbook.Worksheets.Add("統計表");

                int iRow = 1;
                if ("sms_notify_case".Equals(model.type))
                    ws.Cell(1, 1).Value = "級距";
                else
                    ws.Cell(1, 1).Value = "保局範圍";

                ws.Cell(1, 2).Value = "可派件數";
                ws.Cell(1, 3).Value = "金額";
                ws.Cell(1, 4).Value = "派件百分比";
                ws.Cell(1, 5).Value = "分派件數";
                ws.Cell(1, 6).Value = "第一次電訪人員";

                string fsc_range = "";
                string amt_range = "";

                foreach (OAP0043Model d in rowsStat) {
                    iRow++;

                    ws.Cell(iRow, 2).Value = d.cnt;

                    if ( "".Equals(StringUtil.toString(d.amt_range_desc)))
                    { //保局範圍的合計列
                        if (fsc_map.ContainsKey(d.fsc_range))
                            ws.Cell(iRow, 1).Value = fsc_map[d.fsc_range];
                        else
                            ws.Cell(iRow, 1).Value = d.fsc_range;

                        decimal amt = rowsD.Where(x => x.fsc_range == d.fsc_range).Sum(x => x.check_amt);
                        ws.Cell(iRow, 3).Value = amt;
                        ws.Cell(iRow, 3).Style.NumberFormat.Format = "#,##0";
                        totAmt += amt;
                        totCnt += d.cnt;

                        //ws.Range(iRow, 1, iRow, 6).Style.Fill.BackgroundColor = XLColor.AirForceBlue;
                        ws.Range(iRow, 1, iRow, 6).Style.Font.FontColor = XLColor.AirForceBlue;
                    }
                    else {
                        int i = 0;
                        ws.Cell(iRow, 1).Value = d.amt_range_desc;

                        decimal amt = rowsD.Where(x => x.fsc_range == d.fsc_range & x.amt_range == d.amt_range).Sum(x => x.check_amt);
                        ws.Cell(iRow, 3).Value = amt;
                        ws.Cell(iRow, 3).Style.NumberFormat.Format = "#,##0";

                        foreach (OAP0043Model proc_id in dataList.Where(x => x.fsc_range == d.fsc_range & x.amt_range == d.amt_range).ToList()) {
                            i++;

                            if (i > 1)
                                iRow++;

                            ws.Cell(iRow, 4).Value = proc_id.std_1;
                            ws.Cell(iRow, 5).Value = proc_id.std_2;
                            ws.Cell(iRow, 6).Value = StringUtil.toString(proc_id.proc_name) == "" ? StringUtil.toString(proc_id.proc_id) : StringUtil.toString(proc_id.proc_name);
                            totDispatchCnt += Convert.ToDecimal(proc_id.std_2);
                        }


                        if ("sms_notify_case".Equals(model.type)) {
                            totAmt += amt;
                            totCnt += d.cnt;
                        }
                    }
                        
                }



                ws.Cell(iRow + 1, 1).Value = "合計";
                ws.Cell(iRow + 1, 2).Value = totCnt;
                ws.Cell(iRow + 1, 3).Value = totAmt;
                //if ("sms_notify_case".Equals(model.type))
                //{
                //    ws.Cell(iRow + 1, 2).Value = rowsStat.Sum(x => x.cnt);
                //    ws.Cell(iRow + 1, 3).Value = rowsStat.Sum(x => x.check_amt);
                //}
                //else {
                //    ws.Cell(iRow + 1, 2).Value = totCnt;
                //    ws.Cell(iRow + 1, 3).Value = totAmt;
                //}

                ws.Cell(iRow + 1, 5).Value = totDispatchCnt;

                ws.Cell(iRow + 1, 2).Style.NumberFormat.Format = "#,##0";
                ws.Cell(iRow + 1, 3).Style.NumberFormat.Format = "#,##0";
                ws.Cell(iRow + 1, 5).Style.NumberFormat.Format = "#,##0";

                ws.Range(iRow + 1, 1, iRow + 1, 6).Style.Font.FontColor = XLColor.MediumVioletRed;

                ws.Range(1, 1, 1, 6).Style.Fill.BackgroundColor = XLColor.AirForceBlue;
                ws.Range(1, 1, 1, 6).Style.Font.FontColor = XLColor.White;


                ws.Columns().AdjustToContents();  // Adjust column width
                ws.Rows().AdjustToContents();     // Adjust row heights
                workbook.SaveAs(fullPath);
            }


            return bRpt;
        }


        /// <summary>
        /// 匯出明細表
        /// </summary>
        /// <param name="fullPath"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        private async Task<bool> genRptD(string fullPath, OAP0043Model model, string user_id, string user_name)
        {
            bool bRpt = true;

            FAPTelCheckHisDao fAPTelCheckHisDao = new FAPTelCheckHisDao();
            

            List<TelDispatchRptModel> dataList = new List<TelDispatchRptModel>();

            
            dataList = fAPTelCheckHisDao.qryOAP0043DRpt(model.type);

            if (dataList.Count == 0) {
                bRpt = false;
                return bRpt;
            }


            VeTelUtil veTelUtil = new VeTelUtil();


            if ("sms_notify_case".Equals(model.type))
            {
                FAPPYCTDao fAPPYCTDao = new FAPPYCTDao();
                
                using (EacConnection conn400 = new EacConnection(CommonUtil.GetEasycomConn()))
                {
                    conn400.Open();

                    foreach (TelDispatchRptModel d in dataList)
                    {
                        FAP_TEL_SMS_TEMP tmp = new FAP_TEL_SMS_TEMP();
                        tmp.policy_no = d.policy_no;
                        tmp.policy_seq = d.policy_seq;
                        tmp.id_dup = d.id_dup;
                        tmp.system = d.system;
                        tmp.paid_name = d.paid_name;
                        tmp.ins_name = d.ins_name;
                        tmp.appl_name = d.appl_name;
                        tmp.mobile = veTelUtil.getSmsMobile(tmp, conn400);

                        d.policy_mobile = tmp.mobile;
                    }
                }

                veTelUtil.genSmsNotifyRptD(dataList, fullPath);
            }
            else {
                await veTelUtil.genDispatchRpt("OAP0043Controller",user_id, user_name, model.type, fullPath, dataList);
            }
            

            return bRpt;
        }



            /// <summary>
            /// 畫面執行申請覆核
            /// </summary>
            /// <param name="type"></param>
            /// <returns></returns>
            [HttpPost]
        public JsonResult execAply(string type)
        {
            string strConn = DbUtil.GetDBFglConnStr();
            using (SqlConnection conn = new SqlConnection(strConn))
            {
                conn.Open();

                SqlTransaction transaction = conn.BeginTransaction("Transaction");
                try
                {
                    DateTime now = DateTime.Now;
                    string[] curDateTime = BO.DateUtil.getCurChtDateTime(3).Split(' ');

                    //取得申請單號
                    SysSeqDao sysSeqDao = new SysSeqDao();
                    String qPreCode = "0043" + curDateTime[0].Substring(0, 5);
                    var cId = sysSeqDao.qrySeqNo("AP", "0043", qPreCode).ToString();
                    string aply_no = qPreCode + cId.ToString().PadLeft(3, '0');


                    //將申請單號填入歷史檔
                    //1.FAP_TEL_CODE_HIS 電訪標準設定暫存檔
                    //2.FAP_TEL_CHECK_HIS 電訪支票暫存檔
                    FAPTelCodeHisDao fAPTelCodeHisDao = new FAPTelCodeHisDao();
                    fAPTelCodeHisDao.updateAplyNo(type, aply_no, Session["UserID"].ToString(), now, conn, transaction);

                    FAPTelCheckHisDao fAPTelCheckHisDao = new FAPTelCheckHisDao();
                    fAPTelCheckHisDao.updateAplyNo(type, aply_no, Session["UserID"].ToString(), now, conn, transaction);


                    transaction.Commit();

                    return Json(new { success = true, aply_no = aply_no });
                }
                catch (Exception e)
                {
                    transaction.Rollback();
                    logger.Error("其它錯誤：" + e.ToString());

                    return Json(new { success = false, err = "其它錯誤，請洽系統管理員!!" }, JsonRequestBehavior.AllowGet);
                }
            }


        }


        /// <summary>
        /// 匯入
        /// </summary>
        /// <returns></returns>
        public JsonResult procImport()
        {


            int sheetCnt = 0;
            int fileCnt = 0;

            List<impModel> fileList = new List<impModel>();
            List<impModel> errList = new List<impModel>();

            string type = Request.Form["type"];

            try
            {
                string path = "";
                //## 如果有任何檔案類型才做
                if (Request.Files.AllKeys.Any())
                {

                    //## 讀取指定的上傳檔案ID
                    var httpPostedFile = Request.Files["UploadedFile"];

                    //## 真實有檔案，進行處理
                    if (httpPostedFile != null && httpPostedFile.ContentLength != 0)
                    {
                        string fileExtension = Path.GetExtension(httpPostedFile.FileName);
                        if (!(".xls".Equals(fileExtension.ToLower()) || ".xlsx".Equals(fileExtension.ToLower())))
                            return Json(new { success = false, err = "匯入資料必須為excel格式，請重新輸入！" });
                    }
                    else
                        return Json(new { success = false, err = "未上傳檔案" });

                    string execSeq = Session["UserID"].ToString()
                        + "_" + BO.DateUtil.getCurChtDateTime().Replace(" ", "");
                    var fileName = execSeq + "_" + Path.GetFileName(httpPostedFile.FileName); //檔案名稱

                    string projectFile = Server.MapPath("~/FileUploads/OAP0043"); //專案資料夾
                    path = Path.Combine(projectFile, fileName);
                    FileRelated.createFile(projectFile); //檢查是否有FileUploads資料夾,如果沒有就新增

                    //呼叫上傳檔案 function
                    Utility.MSGReturnModel result = FileRelated.FileUpLoadinPath(path, httpPostedFile);

                    if (!result.RETURN_FLAG)
                        return Json(new { success = false, err = result.DESCRIPTION });

                    using (FileStream fs = new FileStream(path, FileMode.Open))
                    {
                        IWorkbook wb = null;  //新建IWorkbook對象 
                        if (fileName.IndexOf(".xlsx") > 0) // 2007版本  
                        {
                            wb = new XSSFWorkbook(fs);  //xlsx數據讀入workbook  
                        }
                        else if (fileName.IndexOf(".xls") > 0) // 2003版本  
                        {
                            wb = new HSSFWorkbook(fs);  //xls數據讀入workbook  
                        }

                        int count = wb.NumberOfSheets;


                        CommonUtil commonUtil = new CommonUtil();
                        Dictionary<string, ADModel> empMap = new Dictionary<string, ADModel>();
                        FAPTelCheckHisDao fAPTelCheckHisDao = new FAPTelCheckHisDao();
                        DateTime now = DateTime.Now;
                        string usr_id = Session["UserID"].ToString();
                        string sheet_name = "";


                        string strConn = DbUtil.GetDBFglConnStr();
                        using (SqlConnection conn = new SqlConnection(strConn))
                        {
                            conn.Open();

                            SqlTransaction transaction = conn.BeginTransaction("Transaction");

                            try
                            {

                                for (int iSheet = 0; iSheet < count; iSheet++)
                                {
                                    sheet_name = wb.GetSheetAt(iSheet).SheetName;
                                    sheetCnt = 0;
                                    ISheet sheet = wb.GetSheetAt(iSheet);
                                    DataTable table = new DataTable();

                                    //由第一列取標題做為欄位名稱
                                    IRow headerRow = sheet.GetRow(0);
                                    int cellCount = headerRow.LastCellNum;


                                    try
                                    {
                                        table.Columns.Add("line");
                                        for (int i = headerRow.FirstCellNum; i < cellCount; i++)
                                            //以欄位文字為名新增欄位，此處全視為字串型別以求簡化
                                            table.Columns.Add(
                                                new DataColumn(headerRow.GetCell(i).StringCellValue.TrimEnd()));

                                    }
                                    catch (Exception e)
                                    {
                                        logger.Error(e.ToString());
                                        return Json(new { success = false, err = "檔案內容需為文字格式!!" });
                                    }

                                    //略過前兩列(標題列)，一直處理至最後一列
                                    for (int i = (sheet.FirstRowNum + 2); i <= sheet.LastRowNum; i++)
                                    {

                                        IRow row = sheet.GetRow(i);
                                        if (row == null) continue;
                                        DataRow dataRow = table.NewRow();
                                        dataRow[0] = (i + 1).ToString();
                                        //依先前取得的欄位數逐一設定欄位內容
                                        for (int j = row.FirstCellNum; j < cellCount; j++)
                                        {
                                            if (row.GetCell(j) != null)
                                                //如要針對不同型別做個別處理，可善用.CellType判斷型別
                                                //再用.StringCellValue, .DateCellValue, .NumericCellValue...取值
                                                //此處只簡單轉成字串
                                                dataRow[j + 1] = row.GetCell(j).ToString();
                                        }

                                        table.Rows.Add(dataRow);
                                        sheetCnt++;
                                    }

                                    if (sheetCnt == 0 & fileList.Count == 0)
                                    {
                                        continue;   //modify by daiyu 20210125
                                        //FileRelated.deleteFile(path);
                                        //return Json(new { success = false, err = "檔案無明細內容!!" });
                                    }

                                   

                                    //查出尚未申請覆核，且待指派電訪人員的資料
                                    List<TelDispatchRptModel> rows = fAPTelCheckHisDao.qryOAP0043DRpt(type);

                                    //第一次電訪人員
                                    List<FAP_VE_CODE> dispatchRows = new List<FAP_VE_CODE>();

                                    FAPVeCodeDao fAPVeCodeDao = new FAPVeCodeDao();
                                    if ("tel_assign_case".Equals(type))
                                        dispatchRows = fAPVeCodeDao.qryByGrp("TEL_DISPATCH");
                                    else
                                        dispatchRows = fAPVeCodeDao.qryByGrp("SMS_DISPATCH");


                                    for (int i = 0; i < sheetCnt; i++)
                                    {
                                        impModel d = new impModel();
                                        d.linePos = table.Rows[i]["line"]?.ToString();
                                        d.sheet_name = sheet_name;
                                        try
                                        {
                                            d.tel_interview_id = table.Rows[i]["tel_interview_id"]?.ToString();
                                            d.system = table.Rows[i]["system"]?.ToString();
                                            d.check_no = table.Rows[i]["check_no"]?.ToString();
                                            d.check_acct_short = table.Rows[i]["check_acct_short"]?.ToString();
                                            //d.fsc_range = table.Rows[i]["fsc_range"]?.ToString();

                                            TelDispatchRptModel model = rows.Where(x => x.system == d.system & x.check_no == d.check_no & d.check_acct_short == d.check_acct_short).FirstOrDefault();
                                            if (model != null)
                                            {
                                                d.amt_range = model.amt_range;
                                            }
                                            else {
                                                if(!"sms_notify_case".Equals(type))
                                                    d.amt_range = table.Rows[i]["range_l"]?.ToString();
                                            }

                                            //檢查是否有更新"第一次電訪人員"
                                            TelDispatchRptModel _db_check = rows.Where(x => x.system == d.system & x.check_acct_short == d.check_acct_short & x.check_no == d.check_no).FirstOrDefault();
                                            if (_db_check == null)
                                            {
                                                d.err_msg = "系統內不存在該筆支票的待分派資料";
                                                errList.Add(d);
                                            }
                                            d.fsc_range = _db_check.fsc_range;

                                            if (!_db_check.tel_interview_id.Equals(d.tel_interview_id))
                                            {
                                                if (!"".Equals(d.tel_interview_id) & dispatchRows.Where(x => x.code_id == d.tel_interview_id).Count() == 0)
                                                {
                                                    d.err_msg = "第一次電訪人員錯誤";
                                                    errList.Add(d);
                                                }
                                                else
                                                {
                                                    //異動[FAP_TEL_CHECK_HIS 電訪支票暫存檔]的"第一次電訪人員"
                                                    if ("".Equals(StringUtil.toString(_db_check.tel_interview_id)))
                                                    {
                                                        FAP_TEL_CHECK_HIS _his = new FAP_TEL_CHECK_HIS();
                                                        _his.aply_no = "";
                                                        _his.system = d.system;
                                                        _his.check_no = d.check_no;
                                                        _his.check_acct_short = d.check_acct_short;
                                                        _his.tel_std_type = type;
                                                        _his.tel_proc_no = "";
                                                        _his.tel_interview_id = d.tel_interview_id;
                                                        _his.update_id = usr_id;
                                                        _his.update_datetime = now;
                                                        fAPTelCheckHisDao.insertFromFormal(now, _his, "", "0043", conn, transaction);
                                                    }
                                                    else
                                                        fAPTelCheckHisDao.updProdIdByCheckNo(type, d.system, d.check_no, d.check_acct_short, d.tel_interview_id
                                                            , usr_id, now, conn, transaction);

                                                    fileList.Add(d);

                                                }
                                            }
                                        }
                                        catch (Exception e)
                                        {
                                            d.err_msg = e.ToString();
                                            errList.Add(d);
                                        }
                                    }

                                }

                                

                                //更新[FAP_TEL_CODE_HIS 電訪標準設定暫存檔]
                                if (fileList.Count() > 0) {
                                    procTelCodeHis(type, fileList, usr_id, now, conn, transaction);
                                    fileCnt += fileList.Count;
                                }

                                transaction.Commit();

                            }
                            catch (Exception e)
                            {
                                transaction.Rollback();
                                logger.Error(e.ToString());
                                throw e;
                            }
                        }
                    }
                }



                if (fileCnt == 0) {
                    return Json(new { success = false, err = "本次上傳未更新電訪人員", errList = errList });
                } else {
                    return Json(new { success = true, msg = "本次上傳更新支票筆數:" + fileCnt, errList = errList });
                }


            }
            catch (Exception e)
            {
                logger.Error(e.ToString());
                return Json(new { success = false, err = "其它錯誤，請洽系統管理員!!" });
            }
        }


        private IWorkbook procExcelSheet(IWorkbook wb)
        {

            return wb;
        }



        private void writePiaLog(int affectRows, string piaOwner, string executionType)
        {
            PIA_LOG_MAIN piaLogMain = new PIA_LOG_MAIN();
            piaLogMain.TRACKING_TYPE = "A";
            piaLogMain.ACCESS_ACCOUNT = Session["UserID"].ToString();
            piaLogMain.ACCOUNT_NAME = Session["UserName"].ToString();
            piaLogMain.PROGFUN_NAME = "OAP0043Controller";
            piaLogMain.EXECUTION_CONTENT = MaskUtil.maskId(piaOwner);
            piaLogMain.AFFECT_ROWS = affectRows;
            piaLogMain.PIA_TYPE = "0100100000";
            piaLogMain.EXECUTION_TYPE = executionType;
            piaLogMain.ACCESSOBJ_NAME = "FAP_TEL_CHECK";
            piaLogMain.PIA_OWNER1 = piaOwner;
            PiaLogMainDao piaLogMainDao = new PiaLogMainDao();
            piaLogMainDao.Insert(piaLogMain);

        }


        private void procTelCodeHis(string type, List<impModel> dataList, string usr_id, DateTime dt, SqlConnection conn, SqlTransaction transaction) {
            List<impModel> groupList = dataList.GroupBy(o => new { o.fsc_range, o.amt_range })
              .Select(group => new impModel
              {
                  fsc_range = group.Key.fsc_range,
                  amt_range = group.Key.amt_range
              }).ToList<impModel>();

            FAPTelCodeHisDao fAPTelCodeHisDao = new FAPTelCodeHisDao();
            FAPTelCheckHisDao fAPTelCheckHisDao = new FAPTelCheckHisDao();
            List<TelDispatchRptModel> rows = fAPTelCheckHisDao.qryOAP0043DRpt(type);

            foreach (impModel d in groupList) {
                FAP_TEL_CODE_HIS codeGrp = new FAP_TEL_CODE_HIS();

                //先將[FAP_TEL_CODE_HIS 電訪標準設定暫存檔]原有的資料刪除
                codeGrp.fsc_range = d.fsc_range;
                codeGrp.amt_range = d.amt_range;
                codeGrp.code_type = type;
                codeGrp.aply_no = "";
                fAPTelCodeHisDao.delForOAP0043(codeGrp, conn, transaction);

                int _tot_cnt = rows.Where(x => x.fsc_range == d.fsc_range & x.amt_range == d.amt_range).GroupBy(x => x.temp_id).Count();
                int _cnt = 0;
                int _percent = 0;

                List<string> procIdList = rows.Where(x => x.fsc_range == d.fsc_range & x.amt_range == d.amt_range)
                    .GroupBy(o => new { o.tel_interview_id }).Select(x => x.Key.tel_interview_id).ToList();

                foreach (string id in procIdList) {
                    if (!"".Equals(StringUtil.toString(id))) {
                        FAP_TEL_CODE_HIS his = new FAP_TEL_CODE_HIS();
                        his.aply_no = "";
                        his.code_type = type;
                        his.code_id = d.fsc_range + "-" + d.amt_range + "-" + id;
                        his.proc_id = id;
                        his.std_2 = rows.Where(x => x.fsc_range == d.fsc_range & x.amt_range == d.amt_range & x.tel_interview_id == id).GroupBy(o => new { o.temp_id }).Count();
                        his.std_1 = (his.std_2 * 100) / _tot_cnt ;
                        his.update_id = usr_id;
                        his.update_datetime = dt;
                        his.fsc_range = d.fsc_range;
                        his.amt_range = d.amt_range;
                        his.appr_stat = "";

                        _cnt += (int)his.std_2;

                        if (_cnt == _tot_cnt)
                            his.std_1 = 100 - _percent;
                        else
                            _percent += (int)his.std_1;


                        fAPTelCodeHisDao.insert(dt, his, conn, transaction);



                    }
                }
            }


            
        }




        public partial class impModel
        {
            public string linePos { get; set; }

            public string sheet_name { get; set; }

            public string err_msg { get; set; }

            public string system { get; set; }

            public string check_no { get; set; }

            public string check_acct_short { get; set; }

            public string fsc_range { get; set; }

            public string amt_range { get; set; }

            public string tel_interview_id { get; set; }

        }
    }
}