using ClosedXML.Excel;
using DocumentFormat.OpenXml.Drawing;
using FAP.Web.ActionFilter;
using FAP.Web.AS400Models;
using FAP.Web.BO;
using FAP.Web.Daos;
using FAP.Web.Models;
using FAP.Web.ViewModels;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;

using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.EasycomClient;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using DateUtil = FAP.Web.BO.DateUtil;

/// <summary>
/// 功能說明：OAP0051 電訪成效統計表
/// 初版作者：20200918 Daiyu
/// 修改歷程：20200918 Daiyu
/// 需求單號：202008120153-01
/// 修改內容：初版
/// ------------------------------------------
/// 修改歷程：20210201 Daiyu
/// 需求單號：202101280283-00
/// 修改內容：修改「電訪人員成效表」格式及公式
/// </summary>
///

namespace FAP.Web.Controllers
{
    [Authorize]
    [CheckSessionFilterAttribute]
    public class OAP0051Controller : BaseController
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
            string[] roleInfo = authUtil.chkUserFuncAuth(Session["UserID"].ToString(), "~/OAP0051/");
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
        public JsonResult execExport(OAP0051Model model)
        {
            logger.Info("execExport begin!!");

            if ("tel_result".Equals(model.rpt_type))
                return rpt_tel_result(model);   //電訪成效統計表
            else
                return rpt_tel_id(model);   //電訪人員成效報表

        }





        /// <summary>
        /// 電訪成效統計表
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult rpt_tel_result(OAP0051Model model)
        {
            logger.Info("rpt_tel_result begin!!");


            try
            {
                decimal totCnt = 0;
                decimal totAmt = 0;

                FAPTelCheckDao fAPTelCheckDao = new FAPTelCheckDao();
                List<OAP0051Model> dataList = new List<OAP0051Model>();
                dataList = fAPTelCheckDao.qryOAP0051_result(model);

                //處理尚未給付的資料
                List<OAP0051Model> notPaidList = dataList.Where(x => "".Equals(StringUtil.toString(x.paid_code)))
             .GroupBy(o => new { o.temp_id, o.tel_result }).Select(group => new OAP0051Model
             {
                 temp_id = group.Key.temp_id,
                 tel_result = group.Key.tel_result,
                 check_amt = group.Sum(x => Convert.ToInt64(x.check_amt))
             }).ToList<OAP0051Model>();



                foreach (OAP0051Model _d in notPaidList.OrderBy(x => x.tel_result))
                {
                    logger.Info("nopaid-" + "tel_result:" + _d.tel_result  + " temp_id:" + _d.temp_id);
                }


                //List<OAP0051Model> notPaidList = dataList.Where(x => "".Equals(StringUtil.toString(x.paid_code)))
                //.GroupBy(o => new { o.temp_id, o.tel_result, o.amt_range }).Select(group => new OAP0051Model
                //{
                //    temp_id = group.Key.temp_id,
                //    tel_result = group.Key.tel_result,
                //    amt_range = group.Key.amt_range,
                //    check_amt = group.Sum(x => Convert.ToInt64(x.check_amt))
                //}).ToList<OAP0051Model>();

                FAPVeCodeDao fAPVeCodeDao = new FAPVeCodeDao();
                //查詢級距
                List<FAP_VE_CODE> rangeRows = fAPVeCodeDao.qryByGrp("TEL_RANGE");
                foreach (FAP_VE_CODE range in rangeRows.OrderByDescending(x => x.code_id.Length).ThenByDescending(x => x.code_id))
                {
                    string amt_range = StringUtil.toString(range.code_id) == "" ? "0" : range.code_id;
                    string amt_range_desc = (StringUtil.toString(range.code_id) == "" ? "0" : StringUtil.toString(range.code_id)) + " ~ " + StringUtil.toString(range.code_value);

                    //notPaidList.Where(x => x.amt_range == range.code_id).Select(x => { x.amt_range_desc = amt_range_desc; return x; }).ToList();
                    //notPaidList.Where(x => x.amt_range == range.code_id).Select(x => { x.range_flag = "1"; return x; }).ToList();

                    long range_l = StringUtil.toString(range.code_id) == "" ? 0 : Convert.ToInt64(range.code_id);
                    long range_h = StringUtil.toString(range.code_value) == "" ? 999999999999 : Convert.ToInt64(range.code_value);

                    notPaidList.Where(x => x.check_amt >= range_l & x.check_amt <= range_h).Select(x => { x.amt_range_desc = amt_range_desc; return x; }).ToList();
                    notPaidList.Where(x => x.check_amt >= range_l & x.check_amt <= range_h).Select(x => { x.amt_range = amt_range; return x; }).ToList();
                    notPaidList.Where(x => x.check_amt >= range_l & x.check_amt <= range_h).Select(x => { x.range_flag = "1"; return x; }).ToList();
                }
         

                //已給付的資料
                SysCodeDao sysCodeDao = new SysCodeDao();
                //給付細項
                Dictionary<string, string> paidCodeMap = sysCodeDao.qryByTypeDic("AP", "paid_code");
                List<SYS_CODE> paidCodeList = sysCodeDao.qryByType("AP", "paid_code");

                //處理結果
                List<SYS_CODE> telResultList = sysCodeDao.qryByType("AP", "tel_call");


                Dictionary<string, string> telResultMap = sysCodeDao.qryByTypeDic("AP", "tel_call");


                //add by daiyu 20210305
                foreach (OAP0051Model d in dataList.Where(x => !"".Equals(StringUtil.toString(x.paid_code))).ToList()) {
                    if ("".Equals(StringUtil.toString(d.tel_result)))
                        d.tel_result = "11";
                }

                List<OAP0051Model> paidList = dataList.Where(x => !"".Equals(StringUtil.toString(x.paid_code)))
                .GroupBy(o => new { o.temp_id, o.tel_result, o.paid_code }).Select(group => new OAP0051Model
                {
                    temp_id = group.Key.temp_id,
                    tel_result = group.Key.tel_result,
                    amt_range = group.Key.paid_code,
                    check_amt = group.Sum(x => Convert.ToInt64(x.check_amt))
                }).OrderBy(x => x.check_amt).ToList<OAP0051Model>();



                foreach (OAP0051Model _d in paidList.OrderBy(x => x.tel_result))
                {
                    logger.Info("paid-" + "tel_result:" + _d.tel_result + " temp_id:" + _d.temp_id);
                }

                paidList.Select(x => { x.range_flag = "2"; return x; }).ToList();
                

                foreach (KeyValuePair<string, string> item in paidCodeMap)
                {
                    paidList.Where(x => x.paid_code == item.Key).Select(x => { x.amt_range_desc = item.Value; return x; }).ToList();
                }


                List<OAP0051Model> rptList = new List<OAP0051Model>();
                foreach (OAP0051Model d in notPaidList.GroupBy(o => new { o.tel_result, o.amt_range, o.amt_range_desc }).Select(group => new OAP0051Model
                {
                    tel_result = group.Key.tel_result,
                    amt_range = group.Key.amt_range,
                    amt_range_desc = group.Key.amt_range_desc,
                    cnt = group.Count(),
                    check_amt = group.Sum(x => Convert.ToInt64(x.check_amt))
                }).ToList<OAP0051Model>())
                {
                    if ("".Equals(StringUtil.toString(d.tel_result)))
                        d.tel_result = "12";    //尚未聯繫
                    totCnt += d.cnt;
                    totAmt += d.check_amt;
                    rptList.Add(d);
                }

                



                foreach (OAP0051Model d in paidList.GroupBy(o => new { o.tel_result, o.amt_range, o.amt_range_desc }).Select(group => new OAP0051Model
                {
                    tel_result = group.Key.tel_result,
                    amt_range = group.Key.amt_range,
                    amt_range_desc = group.Key.amt_range_desc,
                    cnt = group.Count(),
                    check_amt = group.Sum(x => Convert.ToInt64(x.check_amt))
                }).ToList<OAP0051Model>())
                {
                    if ("".Equals(StringUtil.toString(d.tel_result)) || "11".Equals(StringUtil.toString(d.tel_result))) 
                        d.tel_result = "11";    //聯繫前已給付

                    totCnt += d.cnt;
                    totAmt += d.check_amt;
                    rptList.Add(d);
                }


                string guid = "";
                if (dataList.Count > 0)
                {

                    guid = Guid.NewGuid().ToString();

                    decimal subRangeCnt = 0;
                    decimal subRangeAmt = 0;

                    string fullPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory + "Temp\\"
                            , string.Concat("OAP0051" + "_" + guid, ".xlsx"));
                    using (XLWorkbook wb = new XLWorkbook())
                    {
                        var ws = wb.Worksheets.Add("OAP0051");

                        ws.Cell("A1").Value = "電訪成效統計表";
                        ws.Cell("A2").Value = "電訪標準覆核日：" + StringUtil.toString(model.tel_std_appr_date_b) + " ~ " + StringUtil.toString(model.tel_std_appr_date_e)
                            + "\n電訪日期：" + StringUtil.toString(model.tel_interview_f_datetime_b) + " ~ " + StringUtil.toString(model.tel_interview_f_datetime_e)
                            + "\n派件日期：" + StringUtil.toString(model.dispatch_date_b) + " ~ " + StringUtil.toString(model.dispatch_date_e);

                        int iRow = 3;
                        int iCol = 0;

                        decimal subProcCnt = 0;
                        decimal subProcAmt = 0;

                        foreach (SYS_CODE d in telResultList.OrderBy(x => x.CODE.Length).ThenBy(x => x.CODE))
                        {
                            subProcCnt = 0;
                            subProcAmt = 0;

                            iRow = 3;
                            iCol += 2;

                            ws.Cell(iRow, iCol).Value = d.CODE_VALUE;


                            ws.Range(iRow, iCol, iRow, iCol + 1).Merge();

                            if("P".Equals(model.cnt_type))
                                ws.Cell(iRow + 1, iCol).Value = "ID件數";
                            else
                                ws.Cell(iRow + 1, iCol).Value = "支票件數";

                            ws.Cell(iRow + 1, iCol + 1).Value = "金額";
                            iRow = 5;
                            foreach (FAP_VE_CODE range in rangeRows.OrderByDescending(x => x.code_id.Length).ThenByDescending(x => x.code_id))
                            {
                                OAP0051Model x = rptList.Where(o => o.tel_result == d.CODE & o.amt_range == range.code_id).FirstOrDefault();

                                string amt_range_desc = (StringUtil.toString(range.code_id) == "" ? "0" : StringUtil.toString(range.code_id)) + " ~ " + StringUtil.toString(range.code_value);

                                ws.Cell(iRow, 1).Value = amt_range_desc;

                                //ws.Cell(iRow, 1).Value = range.code_value;
                                if (x != null)
                                {

                                    ws.Cell(iRow, iCol).Value = String.Format("{0:n0}", Convert.ToInt64(x.cnt));
                                    ws.Cell(iRow, iCol + 1).Value = String.Format("{0:n0}", Convert.ToInt64(x.check_amt));

                                    subProcCnt += x.cnt;
                                    subProcAmt += x.check_amt;
                                }

                                iRow++;
                            }


                            foreach (SYS_CODE range in paidCodeList.OrderByDescending(x => x.CODE.Length).ThenByDescending(x => x.CODE))
                            {
                                OAP0051Model x = rptList.Where(o => o.tel_result == d.CODE & o.amt_range == range.CODE).FirstOrDefault();

                                ws.Cell(iRow, 1).Value = range.CODE_VALUE;

                                if (x != null)
                                {
                                    ws.Cell(iRow, iCol).Value = String.Format("{0:n0}", Convert.ToInt64(x.cnt));
                                    ws.Cell(iRow, iCol + 1).Value = String.Format("{0:n0}", Convert.ToInt64(x.check_amt));

                                    subProcCnt += x.cnt;
                                    subProcAmt += x.check_amt;


                                }

                                iRow++;
                            }

                            ws.Cell(iRow, 1).Value = "總計";
                            ws.Cell(iRow + 1, 1).Value = "比率";

                            ws.Cell(iRow, iCol).Value = String.Format("{0:n0}", Convert.ToInt64(subProcCnt));
                            ws.Cell(iRow, iCol + 1).Value = String.Format("{0:n0}", Convert.ToInt64(subProcAmt));

                            ws.Cell(iRow + 1, iCol).Value = Math.Round(((subProcCnt * 100) / totCnt), 2) + "%";
                            ws.Cell(iRow + 1, iCol + 1).Value = Math.Round(((subProcAmt * 100) / totAmt), 2) + "%";


                        }

                        iRow = 3;
                        ws.Cell(iRow, iCol + 2).Value = "合計";
                        ws.Range(iRow, iCol + 2, iRow, iCol + 3).Merge();

                        if ("P".Equals(model.cnt_type))
                            ws.Cell(iRow + 1, iCol + 2).Value = "ID件數";
                        else
                            ws.Cell(iRow + 1, iCol + 2).Value = "支票件數";


                        ws.Cell(iRow + 1, iCol + 3).Value = "金額";

                        iRow = 5;
                        foreach (FAP_VE_CODE range in rangeRows.OrderByDescending(x => x.code_id.Length).ThenByDescending(x => x.code_id))
                        {
                            subRangeCnt = rptList.Where(o => o.amt_range == range.code_id).Sum(o => o.cnt); //modify by daiyu 20210304
                            subRangeAmt = rptList.Where(o => o.amt_range == range.code_id).Sum(o => o.check_amt);
                            ws.Cell(iRow, iCol + 2).Value = String.Format("{0:n0}", Convert.ToInt64(subRangeCnt));
                            ws.Cell(iRow, iCol + 3).Value = String.Format("{0:n0}", Convert.ToInt64(subRangeAmt));
                            iRow++;
                        }

                        foreach (SYS_CODE range in paidCodeList.OrderByDescending(x => x.CODE.Length).ThenByDescending(x => x.CODE))
                        {

                            subRangeCnt = rptList.Where(o => o.amt_range == range.CODE).Sum(o => o.cnt); //modify by daiyu 20210304
                            subRangeAmt = rptList.Where(o => o.amt_range == range.CODE).Sum(o => o.check_amt);
                            ws.Cell(iRow, iCol + 2).Value = String.Format("{0:n0}", Convert.ToInt64(subRangeCnt));
                            ws.Cell(iRow, iCol + 3).Value = String.Format("{0:n0}", Convert.ToInt64(subRangeAmt));

                            iRow++;
                        }

                        ws.Cell(iRow, iCol + 2).Value = String.Format("{0:n0}", Convert.ToInt64(totCnt));
                        ws.Cell(iRow, iCol + 3).Value = String.Format("{0:n0}", Convert.ToInt64(totAmt));

                        ws.Cell(iRow + 1, iCol + 2).Value = "100%";
                        ws.Cell(iRow + 1, iCol + 3).Value = "100%";

                        ws.Range(1, 1, 1, iCol + 3).Merge();
                        ws.Range(2, 1, 2, iCol + 3).Merge();

                        ws.Range(1, 1, 4, iCol + 3).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        

                        ws.Range(1, 1, iRow + 1, iCol + 3).Style.Border.OutsideBorder = XLBorderStyleValues.Thick;
                        ws.Range(1, 1, iRow + 1, iCol + 3).Style.Border.InsideBorder = XLBorderStyleValues.Thin;

                        ws.Range(3, 1, 4, iCol + 3).Style.Fill.BackgroundColor = XLColor.AirForceBlue;
                        ws.Range(3, 1, 4, iCol + 3).Style.Font.FontColor = XLColor.White;

                        ws.Range(5, 2, iRow +1 , iCol + 3).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;  //add by daiyu 20210226

                        ws.Columns().AdjustToContents();  // Adjust column width
                        ws.Rows().AdjustToContents();     // Adjust row heights


                        wb.SaveAs(fullPath);
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


        /// <summary>
        /// 電訪人員成效報表
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult rpt_tel_id(OAP0051Model model)
        {
            logger.Info("rpt_tel_id begin!!");


            try
            {
                decimal totCnt = 0;
                decimal totAmt = 0;

                FAPTelCheckDao fAPTelCheckDao = new FAPTelCheckDao();
                List<OAP0051Model> dataList = new List<OAP0051Model>();
                dataList = fAPTelCheckDao.qryOAP0051_id(model);

                CommonUtil commonUtil = new CommonUtil();
                Dictionary<string, ADModel> empMap = new Dictionary<string, ADModel>();

                foreach (OAP0051Model d in dataList) {
                    if (!empMap.ContainsKey(d.tel_interview_id))
                    {
                        ADModel adModel = new ADModel();
                        adModel = commonUtil.qryEmp(d.tel_interview_id);
                        empMap.Add(d.tel_interview_id, adModel);
                    }

                    if (!"".Equals(StringUtil.toString(d.tel_interview_f_datetime))) {
                        d.tel_interview_ym = DateUtil.ADDateToChtDate(d.tel_interview_f_datetime, 3, "").Substring(0, 5);
                    }

                    if (!"".Equals(StringUtil.toString(d.dispatch_date)))   //add by daiyu 20210201
                    {
                        d.dispatch_ym = DateUtil.ADDateToChtDate(d.dispatch_date, 3, "").Substring(0, 5);
                    }
                }


                //電訪人員清單
                List<OAP0051Model> id_list = dataList
                    .GroupBy(o => new { o.tel_interview_id }).Select(group => new OAP0051Model
                    {
                        tel_interview_id = group.Key.tel_interview_id
                        , cnt = group.Count()
                    }).ToList<OAP0051Model>();


                ////電訪年月清單
                //List<OAP0051Model> interview_ym_list = dataList.Where(x => x.tel_interview_ym != "")
                //    .GroupBy(o => new { o.tel_interview_ym }).Select(group => new OAP0051Model
                //    {
                //        tel_interview_ym = group.Key.tel_interview_ym
                //    }).OrderBy(x => x.tel_interview_ym).ToList<OAP0051Model>();


                //派件年月清單
                List<OAP0051Model> dispatch_ym_list = dataList.Where(x => x.dispatch_ym != "")
                    .GroupBy(o => new { o.dispatch_ym }).Select(group => new OAP0051Model
                    {
                        dispatch_ym = group.Key.dispatch_ym
                    }).OrderBy(x => x.dispatch_ym).ToList<OAP0051Model>();


                string guid = "";
                if (dataList.Count > 0)
                {

                    guid = Guid.NewGuid().ToString();

                    decimal subRangeCnt = 0;
                    decimal subRangeAmt = 0;

                    string fullPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory + "Temp\\"
                            , string.Concat("OAP0051" + "_" + guid, ".xlsx"));
                    using (XLWorkbook wb = new XLWorkbook())
                    {
                        var ws = wb.Worksheets.Add("OAP0051");

                        ws.Cell("A1").Value = "電訪人員成效報表";
                        ws.Cell("A2").Value ="電訪日期：" + StringUtil.toString(model.tel_interview_f_datetime_b) + " ~ " + StringUtil.toString(model.tel_interview_f_datetime_e)
                            + "\n派件日期：" + StringUtil.toString(model.dispatch_date_b) + " ~ " + StringUtil.toString(model.dispatch_date_e);
                        ws.Cell("A4").Value = "第一次電訪人員";
                        //ws.Cell("A5").Value = "電訪日期";
                        ws.Cell("A5").Value = "派件月份";

                        int iRow = 4;
                        int iCol = 2;

                        decimal idYMSumCnt = 0;
                        decimal idYMDSumCnt = 0;


                        foreach (OAP0051Model id in id_list) {
                            iRow = 4;
                            idYMSumCnt = 0;
                            idYMDSumCnt = 0;

                            ws.Cell(iRow, iCol).Value = empMap[id.tel_interview_id].name;
                            ws.Range(iRow, iCol, iRow, iCol + 2).Merge();

                            //ws.Cell(iRow, iCol + 1).Value = "累積件數";
                            //ws.Cell(iRow, iCol + 2).Value = "完成比率";
                            ws.Cell(iRow + 1, iCol).Value = "派件數";
                            ws.Cell(iRow + 1, iCol + 1).Value = "電訪件數";
                            ws.Cell(iRow + 1, iCol + 2).Value = "完成比率";

                            //ws.Cell(iRow + 1, iCol + 1).Value = String.Format("{0:n0}", Convert.ToInt64(id.cnt));

                            //ws.Range(iRow + 1, iCol, iRow + 1, iCol + 1).Style.Fill.BackgroundColor = XLColor.Yellow;

                            iRow = 6;
                            foreach (OAP0051Model ym in dispatch_ym_list) {
                                
                                ws.Cell(iRow, 1).Value = ym.dispatch_ym;

                                OAP0051Model temp = dataList.Where(x => x.tel_interview_id == id.tel_interview_id & x.dispatch_ym == ym.dispatch_ym)
                                    .GroupBy(o => new { o.tel_interview_id, o.dispatch_ym }).Select(group => new OAP0051Model
                                    {
                                        tel_interview_id = group.Key.tel_interview_id,
                                        dispatch_ym = group.Key.dispatch_ym,
                                        cnt = group.Count()
                                    }).FirstOrDefault();

                                if (temp != null)
                                {
                                    idYMSumCnt += temp.cnt;
                                    ws.Cell(iRow, iCol).Value = String.Format("{0:n0}", Convert.ToInt64(temp.cnt));
                                    int iCnt = 0;

                                    try
                                    {
                                        iCnt = dataList.Where(x => x.tel_interview_id == id.tel_interview_id 
                                                                & x.dispatch_ym == ym.dispatch_ym
                                                                & x.tel_interview_f_datetime != "").Count();
                                        idYMDSumCnt += iCnt;
                                    }
                                    catch { 
                                    
                                    }

                                    ws.Cell(iRow, iCol + 1).Value = String.Format("{0:n0}", Convert.ToInt64(iCnt));
                                    ws.Cell(iRow, iCol + 2).Value = Math.Round((iCnt * 100) / temp.cnt, 2) + "%";
                                }
                                else {
                                    ws.Cell(iRow, iCol).Value = 0;
                                    ws.Cell(iRow, iCol + 1).Value = 0;
                                    ws.Cell(iRow, iCol + 2).Value = "0%";
                                }
                               
                                //ws.Cell(iRow, iCol + 1).Value = idYMSumCnt;
                                //ws.Cell(iRow, iCol + 2).Value = Math.Round((idYMSumCnt * 100) / id.cnt, 2) + "%";

                                iRow++;
                            }

                            ws.Cell(iRow, 1).Value = "合計";
                            ws.Cell(iRow, iCol).Value = String.Format("{0:n0}", Convert.ToInt64(idYMSumCnt));
                            ws.Cell(iRow, iCol + 1).Value = String.Format("{0:n0}", Convert.ToInt64(idYMDSumCnt));

                            try
                            {
                                ws.Cell(iRow, iCol + 2).Value = Math.Round((idYMDSumCnt * 100) / idYMSumCnt, 2) + "%";
                            }
                            catch (Exception e) {
                                ws.Cell(iRow, iCol + 2).Value = "0%";
                            }

                            iCol += 3;

                        }

                        iCol -= 1;

                        ws.Range(1, 1, 1, iCol).Merge();
                        ws.Range(2, 1, 2, iCol).Merge();

                        ws.Range(1, 1, 4, iCol).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        ws.Range(1, 1, iRow, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        ws.Range(6, 2, iRow, iCol).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

                        ws.Range(1, 1, iRow, iCol).Style.Border.OutsideBorder = XLBorderStyleValues.Thick;
                        ws.Range(1, 1, iRow, iCol).Style.Border.InsideBorder = XLBorderStyleValues.Thin;

                        ws.Range(4, 1, 4, iCol).Style.Fill.BackgroundColor = XLColor.AirForceBlue;
                        ws.Range(4, 1, 4, iCol).Style.Font.FontColor = XLColor.White;

                        ws.Columns().AdjustToContents();  // Adjust column width
                        ws.Rows().AdjustToContents();     // Adjust row heights


                        wb.SaveAs(fullPath);
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

        public FileContentResult downloadRpt(string id)
        {
            byte[] fileBytes = System.IO.File.ReadAllBytes(Server.MapPath("~/Temp/") + "OAP0051" + "_" + id + ".xlsx");


            string fullPath = Server.MapPath("~/Temp/") + "OAP0051" + "_" + id + ".xlsx";
            if (System.IO.File.Exists(fullPath))
                System.IO.File.Delete(fullPath);


            return File(fileBytes, "application/vnd.ms-excel", "OAP0051.xlsx");
        }


        private void writePiaLog(int affectRows, string piaOwner, string executionType, string content)
        {
            PIA_LOG_MAIN piaLogMain = new PIA_LOG_MAIN();
            piaLogMain.TRACKING_TYPE = "A";
            piaLogMain.ACCESS_ACCOUNT = Session["UserID"].ToString();
            piaLogMain.ACCOUNT_NAME = Session["UserName"].ToString();
            piaLogMain.PROGFUN_NAME = "OAP0051Controller";
            piaLogMain.EXECUTION_CONTENT = MaskUtil.maskId(piaOwner);
            piaLogMain.AFFECT_ROWS = affectRows;
            piaLogMain.PIA_TYPE = "0100100000";
            piaLogMain.EXECUTION_TYPE = executionType;
            piaLogMain.ACCESSOBJ_NAME = "FAP_VE_TRACE";
            piaLogMain.PIA_OWNER1 = piaOwner;
            PiaLogMainDao piaLogMainDao = new PiaLogMainDao();
            piaLogMainDao.Insert(piaLogMain);

        }

    }
}