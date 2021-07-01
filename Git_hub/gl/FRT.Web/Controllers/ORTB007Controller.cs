using FRT.Web.ActionFilter;
using FRT.Web.AS400Models;
using FRT.Web.AS400PGM;
using FRT.Web.BO;
using FRT.Web.Daos;
using FRT.Web.Models;
using FRT.Web.ViewModels;
using Microsoft.Reporting.WebForms;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.EasycomClient;
using System.Web.Mvc;

/// <summary>
/// 功能說明：快速付款報表
/// 初版作者：20180808 Daiyu
/// 修改歷程：20180808 Daiyu
///           需求單號：
///           初版
/// 修改歷程：20181128 Daiyu
///           需求單號：
///           增加"快速付款待匯款件明細表(依結案日)"、"快速付款失敗報表-跨天通知"
/// ==============================================
/// 修改日期/修改人：20190516 daiyu
/// 需求單號：
/// 修改內容：配合金檢議題，稽核軌跡多加寫HOSTNAME
/// ==============================================
/// </summary>
///

namespace FRT.Web.Controllers
{
    
    [Authorize]
    [CheckSessionFilterAttribute]
    public class ORTB007Controller : BaseController
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
                String[] roleInfo = authUtil.chkUserFuncAuth(Session["UserID"].ToString(), "~/ORTB007/");
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
        /// 畫面執行"列印"功能
        /// </summary>
        /// <param name="cRptType"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Print(string remitStat, string qDate)
        {
            if (!"1".Equals(remitStat) & !"4".Equals(remitStat) & !"1CloseDt".Equals(remitStat) & !"4RemitDt".Equals(remitStat) & !"ORTB007P5".Equals(remitStat)) {
                var jsonData = new { success = true, err = "報表類型錯誤!!" };
                return Json(jsonData, JsonRequestBehavior.AllowGet);
            }

            try {

                FRTBARMDao fRTBARMDao = new FRTBARMDao();
                List<ORTB007Model> rows = new List<ORTB007Model>();
                rows = fRTBARMDao.qryForORTB007(remitStat, qDate);
           
                  

                Dictionary<string, SRTB0010Model> userMap = new Dictionary<string, SRTB0010Model>();
                SRTB0010Util sRTB0010Util = new SRTB0010Util();

                EacConnection con = new EacConnection();
                EacCommand cmd = new EacCommand();

                con.ConnectionString = CommonUtil.GetEasycomConn();
                con.Open();
                cmd.Connection = con;
                

                foreach (ORTB007Model d in rows)
                {
                    string textRcvdt = StringUtil.toString(d.textRcvdt) == "0" ? "" : d.textRcvdt.PadLeft(8, ' ');
                    string textRcvtm = StringUtil.toString(d.textRcvtm) == "0" ? "" : d.textRcvtm.PadLeft(8, ' ');
                    string closeDate = StringUtil.toString(d.closeDate) == "0" ? "" : d.closeDate.PadLeft(8, ' ');
                    string remitDate = StringUtil.toString(d.remitDate) == "0" ? "" : d.remitDate.PadLeft(8, ' ');

                    //d.policyNo = MaskUtil.maskPolicyNo(d.policyNo);
                    //d.paidId = MaskUtil.maskPolicyNo(d.paidId);
                    //d.bankAct = MaskUtil.maskBankAct(d.bankAct);
                    d.policyNo = d.policyNo;
                    d.paidId = d.paidId;
                    d.bankAct = d.bankAct;
                    d.closeDate = d.closeDate == "" ? "" : DateUtil.formatDateTimeDbToSc(closeDate, "D");
                    d.remitDate = d.remitDate == "" ? "" : DateUtil.formatDateTimeDbToSc(remitDate, "D");
                    d.textRcvdt = d.textRcvdt == "" ? "" : DateUtil.formatDateTimeDbToSc(textRcvdt, "D");
                    d.textRcvtm = (textRcvtm == "" ? "" : DateUtil.formatDateTimeDbToSc(textRcvtm.Substring(0, 6), "T"));

                    if (!userMap.ContainsKey(d.entryId))
                        userMap = sRTB0010Util.callSRTB0010(con, cmd, d.entryId, userMap);

                    string empName = StringUtil.toString(userMap[d.entryId].empName);
                    if ("".Equals(empName))
                        empName = d.entryId;
                   // empName = d.entryId.Length == 10 ? MaskUtil.maskId(d.entryId) : d.entryId;

                    d.entryName = empName;
                    d.entryUnit = StringUtil.toString(userMap[d.entryId].empUnit);

                }


                PIA_LOG_MAIN piaLogMain = new PIA_LOG_MAIN();
                piaLogMain.TRACKING_TYPE = "A";
                piaLogMain.ACCESS_ACCOUNT = Session["UserID"].ToString();
                piaLogMain.ACCOUNT_NAME = Session["UserName"].ToString();
                piaLogMain.PROGFUN_NAME = "ORTB007Controller";
                piaLogMain.EXECUTION_CONTENT = "remitStat:" + remitStat + "|" + "qDate:" + qDate;
                piaLogMain.AFFECT_ROWS = rows.Count;
                piaLogMain.PIA_TYPE = "1000100000"; //modify by daiyu 20190516
                piaLogMain.EXECUTION_TYPE = "R";
                piaLogMain.ACCESSOBJ_NAME = "FRTBARM";
                PiaLogMainDao piaLogMainDao = new PiaLogMainDao();
                piaLogMainDao.Insert(piaLogMain);




                if (rows.Count > 0) {
                    ReportWrapper rw = new ReportWrapper();

                    switch (remitStat) {
                        case "1":   //快速付款待匯款件明細表(累計)
                            rw.ReportId = "ORTB007P1";
                            rw.ReportPath = Server.MapPath($"~/Report/Rdlc/ORTB007P1.rdlc");
                            break;
                        case "4":   //快速付款失敗件明細報表(累計)
                            rw.ReportId = "ORTB007P2";
                            rw.ReportPath = Server.MapPath($"~/Report/Rdlc/ORTB007P2.rdlc");
                            break;
                        case "1CloseDt":    //快速付款待匯款件明細表(依結案日)
                            rw.ReportId = "ORTB007P3";
                            rw.ReportPath = Server.MapPath($"~/Report/Rdlc/ORTB007P3.rdlc");
                            break;
                        case "4RemitDt":    //快速付款失敗報表-跨天通知
                            rw.ReportId = "ORTB007P4";
                            rw.ReportPath = Server.MapPath($"~/Report/Rdlc/ORTB007P4.rdlc");
                            break;
                        case "ORTB007P5":   //快速付款取消件明細表
                            rw.ReportId = remitStat;
                            rw.ReportPath = Server.MapPath($"~/Report/Rdlc/{remitStat}.rdlc");
                            break;
                    }


                    rw.ReportParameters.Add("endDate", DateUtil.formatDateTimeDbToSc(qDate, "D"));

                    CommonUtil commonUtil = new CommonUtil();
                    DataTable dtMain = commonUtil.ConvertToDataTable<ORTB007Model>(rows);

                    rw.ReportDataSources = dtMain;


                    String guid = Guid.NewGuid().ToString();

                    Session[guid] = rw;

                    var jsonData = new { success = true, guid = guid };
                    return Json(jsonData, JsonRequestBehavior.AllowGet);


                    //rows = ScallSAAEMPY(rows);
                } else
                    return Json(new { success = false, err = "查無資料!!" }, JsonRequestBehavior.AllowGet);

                

            } catch (Exception e) {
                logger.Error(e.ToString());
                return Json(new { success = false, err = "其它錯誤，請洽系統管理員!!" }, JsonRequestBehavior.AllowGet);
            }

        }

        

    }
}