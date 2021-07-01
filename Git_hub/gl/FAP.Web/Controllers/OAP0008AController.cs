using FAP.Web;
using FAP.Web.ActionFilter;
using FAP.Web.BO;
using FAP.Web.Daos;
using FAP.Web.Models;
using FAP.Web.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.EasycomClient;
using System.Data.SqlClient;
using System.Linq;
using System.Web.Mvc;

/// <summary>
/// 功能說明：刪除清理記錄功能維護覆核作業
/// 初版作者：20190621 Daiyu
/// 修改歷程：20190621 Daiyu
/// 需求單號：
/// 修改內容： 初版
/// ------------------------------------------
/// 修改歷程：20200805 Daiyu
/// 需求單號：
/// 修改內容：可刪除由AS400寫入的踐行程序
/// </summary>
///

namespace FRT.Web.Controllers
{
    [Authorize]
    [CheckSessionFilterAttribute]
    public class OAP0008AController : BaseController
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
                String[] roleInfo = authUtil.chkUserFuncAuth(Session["UserID"].ToString(), "~/OAP0008A/");
                if (roleInfo != null && roleInfo.Length == 2)
                {
                    opScope = roleInfo[0];
                    funcName = roleInfo[1];
                }
            }

            ViewBag.opScope = opScope;
            ViewBag.funcName = funcName;

            SysCodeDao sysCodeDao = new SysCodeDao();
            ViewBag.veClrTypeList = sysCodeDao.loadSelectList("AP", "VE_CLR_TYPE", false);

            //執行功能
            ViewBag.execActionjqList = sysCodeDao.jqGridList("AP", "EXEC_ACTION", true);


            FAPVeCodeDao fAPVeCodeDao = new FAPVeCodeDao();
            //清理大類
            ViewBag.level1jqList = fAPVeCodeDao.jqGridList("CLR_LEVEL1", true);

            //清理小類
            ViewBag.level2jqList = fAPVeCodeDao.jqGridList("CLR_LEVEL2", true);

            //踐行程序
            ViewBag.practicejqList = fAPVeCodeDao.jqGridList("CLR_PRACTICE", true);

            //證明文件
            ViewBag.certDocjqList = fAPVeCodeDao.jqGridList("CLR_CERT_DOC", true);

            return View();
        }


        /// <summary>
        /// 查詢待覆核的資料
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public JsonResult LoadData(string paid_id)
        {

            try {

                FAPVeTrackProcHisDao fAPVeTrackProcHisDao = new FAPVeTrackProcHisDao();
                List<OAP0008Model> rows = fAPVeTrackProcHisDao.qryByForOAP0008A(paid_id, new string[] {"1"});


                using (DB_INTRAEntities dbIntra = new DB_INTRAEntities())
                {
                    Dictionary<string, string> userNameMap = new Dictionary<string, string>();
                    OaEmpDao oaEmpDao = new OaEmpDao();
                    string update_id = "";

                    int i = 0;
                    foreach (OAP0008Model d in rows)
                    {
                        i++;
                        d.temp_id = i.ToString();

                        DateTime exec_date = Convert.ToDateTime(d.exec_date);
                        DateTime exec_date_1 = Convert.ToDateTime(d.exec_date_1);
                        d.exec_date = exec_date.Year - 1911 + exec_date.Month.ToString().PadLeft(2, '0') + exec_date.Day.ToString().PadLeft(2, '0');
                        d.exec_date_1 = exec_date_1.Year - 1911 + exec_date_1.Month.ToString().PadLeft(2, '0') + exec_date_1.Day.ToString().PadLeft(2, '0');

                        update_id = StringUtil.toString(d.update_id);

                        if (!"".Equals(update_id))
                        {
                            if (!userNameMap.ContainsKey(update_id))
                                userNameMap = oaEmpDao.qryUsrName(userNameMap, update_id, dbIntra);

                            d.update_name = userNameMap[update_id];
                        }
                    }
                }

                writePiaLog(MaskUtil.maskId(paid_id), rows.Count, paid_id, "Q");

                var jsonData = new { success = true, rows };
                return Json(jsonData, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                logger.Error(e.ToString);
                var jsonData = new { success = false, err = e.ToString()};
                return Json(jsonData, JsonRequestBehavior.AllowGet);
            }
        }



        /// <summary>
        /// 核可/退回
        /// </summary>
        /// <param name="recData"></param>
        /// <param name="rtnData"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult execSave(List<OAP0008Model> recData, List<OAP0008Model> rtnData)
        {
            string strConn = DbUtil.GetDBFglConnStr();
            using (SqlConnection conn = new SqlConnection(strConn))
            {
                conn.Open();

                SqlTransaction transaction = conn.BeginTransaction("Transaction");

                try
                {
                    DateTime dt = DateTime.Now;
                    FAPVeTrackProcDao fAPVeTrackProcDao = new FAPVeTrackProcDao();
                    FAPVeTrackProcHisDao fAPVeTrackProcHisDao = new FAPVeTrackProcHisDao();


                    //處理駁回資料
                    if (rtnData.Count > 0)
                    {
                        string paid_id = "";

                        foreach (OAP0008Model d in rtnData)
                        {
                            if (d.update_id.Equals(Session["UserID"].ToString()))
                                return Json(new { success = false, err = "申請人與覆核人員不可相同!!" }, JsonRequestBehavior.AllowGet);

                            d.appr_id = Session["UserID"].ToString();
                            d.exec_date = DateUtil.formatDateTimeDbToSc(DateUtil.As400ChtDateToADDate(d.exec_date), "D");
                            d.exec_date_1 = DateUtil.formatDateTimeDbToSc(DateUtil.As400ChtDateToADDate(d.exec_date_1), "D");
                           
                            d.update_datetime = null;
                            paid_id = d.paid_id;

                            //異動【FAP_VE_TRACE_PROC 逾期未兌領清理記錄歷程檔】
                            FAP_VE_TRACE_PROC proc = new FAP_VE_TRACE_PROC();
                            ObjectUtil.CopyPropertiesTo(d, proc);
                            proc.data_status = "1";
                            fAPVeTrackProcDao.updateStatus(dt, Session["UserID"].ToString(), proc, conn, transaction);

                            //異動【FAP_VE_TRACE_PROC_HIS 逾期未兌領清理記錄歷程暫存檔】
                            FAP_VE_TRACE_PROC_HIS his = new FAP_VE_TRACE_PROC_HIS();
                            ObjectUtil.CopyPropertiesTo(d, his);
                            fAPVeTrackProcHisDao.updateApprStat("3", dt, Session["UserID"].ToString(), his, conn, transaction);

                            
                        }
                        writePiaLog("apprStat=3", rtnData.Count, paid_id, "E");
                    }

                    string[] send_cnt_arr = new string[] { "G1", "G2", "G3", "G4", "G10", "G15" };
                    
                    //處理核可資料
                    if (recData.Count > 0)
                    {
                        string paid_id = "";
                        FAPVeTraceDao veTrackDao = new FAPVeTraceDao();

                        foreach (OAP0008Model d in recData)
                        {
                            if (d.update_id.Equals(Session["UserID"].ToString()))
                                return Json(new { success = false, err = "申請人與覆核人員不可相同!!" }, JsonRequestBehavior.AllowGet);


                            d.appr_id = Session["UserID"].ToString();
                            d.exec_date = DateUtil.formatDateTimeDbToSc(DateUtil.As400ChtDateToADDate(d.exec_date), "D");
                            d.exec_date_1 = DateUtil.formatDateTimeDbToSc(DateUtil.As400ChtDateToADDate(d.exec_date_1), "D");
                            d.proc_desc = StringUtil.toString(d.proc_desc);
                            d.update_datetime = null;
                            paid_id = d.paid_id;



                            //異動【FAP_VE_TRACE_PROC_HIS 逾期未兌領清理記錄歷程暫存檔】
                            FAP_VE_TRACE_PROC_HIS his = new FAP_VE_TRACE_PROC_HIS();
                            ObjectUtil.CopyPropertiesTo(d, his);
                            fAPVeTrackProcHisDao.updateApprStat("2", dt, Session["UserID"].ToString(), his, conn, transaction);

                            //異動【FAP_VE_TRACE_PROC 逾期未兌領清理記錄歷程檔】
                            his = fAPVeTrackProcHisDao.qryByKey(his);
                            if ("D".Equals(his.exec_action))
                            {


                                //modify by dayiu 20200805
                                if ("M".Equals(his.srce_from))
                                {
                                    int _as400_send_cnt = 0;

                                    FAP_VE_TRACE main = new FAP_VE_TRACE();
                                    main = veTrackDao.qryByCheckNo(his.check_no, his.check_acct_short);
                                    if (main.as400_send_cnt != null)
                                        _as400_send_cnt = (int)main.as400_send_cnt;

                                    if (send_cnt_arr.Contains(his.practice))
                                        _as400_send_cnt--;

                                    main.as400_send_cnt = _as400_send_cnt < 0 ? 0 : _as400_send_cnt;

                                    if (his.exec_date.CompareTo(main.exec_date_1) == 0)
                                    {
                                        main.exec_date_1 = null;
                                        main.cert_doc_1 = "";
                                        main.practice_1 = "";
                                    }

                                    if (his.exec_date.CompareTo(main.exec_date_2) == 0)
                                    {
                                        main.exec_date_2 = null;
                                        main.cert_doc_2 = "";
                                        main.practice_2 = "";
                                    }

                                    if (his.exec_date.CompareTo(main.exec_date_3) == 0)
                                    {
                                        main.exec_date_3 = null;
                                        main.cert_doc_3 = "";
                                        main.practice_3 = "";
                                    }

                                    if (his.exec_date.CompareTo(main.exec_date_4) == 0)
                                    {
                                        main.exec_date_4 = null;
                                        main.cert_doc_4 = "";
                                        main.practice_4 = "";
                                    }

                                    if (his.exec_date.CompareTo(main.exec_date_5) == 0)
                                    {
                                        main.exec_date_5 = null;
                                        main.cert_doc_5 = "";
                                        main.practice_5 = "";
                                    }


                                    veTrackDao.updaetForOAP0008(main, conn, transaction);
                                }
                                else
                                    fAPVeTrackProcDao.deleteForOAP0008(his, conn, transaction);
                            }
                            else {
                                //modify by daiyu 20201204 AS400的踐行程序可修改"過程說明"

                                if ("M".Equals(his.srce_from)) {
                                    veTrackDao.updateProcDesc(his.check_no, his.check_acct_short, his.proc_desc, conn, transaction);
                                } else
                                    fAPVeTrackProcDao.updateForOAP0008(dt, his, conn, transaction);
                            }

                        }


                        //異動清理大小類【FAP_VE_TRACE 逾期未兌領清理記錄檔】
                        FAPVeTraceHisDao fAPVeTraceHisDao = new FAPVeTraceHisDao();
                        List<OAP0008Model> traceHisList = recData
                            .GroupBy(x => new { x.check_acct_short, x.check_no, x.level_1, x.level_2, x.level_1_1, x.level_2_1 })
                            .Select(g => new OAP0008Model
                            {
                                check_acct_short = g.Key.check_acct_short,
                                check_no = g.Key.check_no,
                                level_1 = g.Key.level_1,
                                level_2 = g.Key.level_2,
                                level_1_1 = g.Key.level_1_1,
                                level_2_1 = g.Key.level_2_1
                            })
                            .ToList<OAP0008Model>();


                        foreach (OAP0008Model d in traceHisList)
                        {
                            bool bChgLevel = false;
                            if (!StringUtil.toString(d.level_1).Equals(StringUtil.toString(d.level_1_1)) 
                                || !StringUtil.toString(d.level_2).Equals(StringUtil.toString(d.level_2_1)))
                                bChgLevel = true;
                            

                            VeCleanUtil veCleanUtil = new VeCleanUtil();
                            DateTime? exec_date = veCleanUtil.qryMaxExecDate(d.check_no, d.check_acct_short);
                            veTrackDao.updateForProc(dt, Session["UserID"].ToString(), d.check_no, d.check_acct_short, exec_date
                                , d.level_1_1, d.level_2_1, bChgLevel
                                , conn, transaction);
                        }

                        writePiaLog("apprStat=2", rtnData.Count, paid_id, "E");
                    }

                    transaction.Commit();

                    return Json(new { success = true });
                }
                catch (Exception e)
                {
                    transaction.Rollback();

                    logger.Error("[execReviewR]其它錯誤：" + e.ToString());

                    return Json(new { success = false, err = "其它錯誤，請洽系統管理員!!" }, JsonRequestBehavior.AllowGet);
                }
            }
        }


        //private DateTime? qryMaxExecDate(string check_no, string check_acct_short) {
        //    FAPVeTraceDao fAPVeTraceDao = new FAPVeTraceDao();

        //    FAP_VE_TRACE d = fAPVeTraceDao.qryByCheckNo(check_no, check_acct_short);
        //    var exec_date = d.exec_date_1;

        //    if (d.exec_date_2 != null)
        //        exec_date = exec_date < d.exec_date_2 ? d.exec_date_2 : exec_date;

        //    if (d.exec_date_3 != null)
        //        exec_date = exec_date < d.exec_date_3 ? d.exec_date_3 : exec_date;

        //    if (d.exec_date_4 != null)
        //        exec_date = exec_date < d.exec_date_4 ? d.exec_date_4 : exec_date;

        //    if (d.exec_date_5 != null)
        //        exec_date = exec_date < d.exec_date_5 ? d.exec_date_5 : exec_date;

        //    FAPVeTrackProcDao fAPVeTrackProcDao = new FAPVeTrackProcDao();
        //    List<OAP0010DModel> procList = fAPVeTrackProcDao.qryByCheckNo(check_no, check_acct_short);
        //    if (procList.Count > 0) {
        //        var procMaxDate = procList.Max(x => Convert.ToDateTime(x.exec_date).AddYears(1911));
        //        if (procMaxDate != null) {
        //            if (exec_date == null)
        //                exec_date = procMaxDate;
        //            else
        //                exec_date = exec_date < procMaxDate ? procMaxDate : exec_date;
        //        }

        //    }
            
                
        //    return exec_date;
        //}


        private void writePiaLog(string content, int affectRows, string piaOwner, string executionType)
        {
            PIA_LOG_MAIN piaLogMain = new PIA_LOG_MAIN();
            piaLogMain.TRACKING_TYPE = "A";
            piaLogMain.ACCESS_ACCOUNT = Session["UserID"].ToString();
            piaLogMain.ACCOUNT_NAME = Session["UserName"].ToString();
            piaLogMain.PROGFUN_NAME = "OAP0008AController";
            piaLogMain.EXECUTION_CONTENT = content;
            piaLogMain.AFFECT_ROWS = affectRows;
            piaLogMain.PIA_TYPE = "0100100000";
            piaLogMain.EXECUTION_TYPE = executionType;
            piaLogMain.ACCESSOBJ_NAME = "FAP_VE_TRACE_PROC_HIS";
            piaLogMain.PIA_OWNER1 = piaOwner;
            PiaLogMainDao piaLogMainDao = new PiaLogMainDao();
            piaLogMainDao.Insert(piaLogMain);

        }


    }
}