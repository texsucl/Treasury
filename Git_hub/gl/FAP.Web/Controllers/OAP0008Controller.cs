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

/// <summary>
/// 功能說明：刪除清理記錄功能維護作業
/// 初版作者：20190620 Daiyu
/// 修改歷程：20190620 Daiyu
/// 需求單號：
/// 修改內容：初版
/// ------------------------------------------
/// 修改歷程：20200805 Daiyu
/// 需求單號：
/// 修改內容：可刪除由AS400寫入的踐行程序
/// </summary>
///

namespace FAP.Web.Controllers
{
    [Authorize]
    [CheckSessionFilterAttribute]
    public class OAP0008Controller : BaseController
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
                String[] roleInfo = authUtil.chkUserFuncAuth(Session["UserID"].ToString(), "~/OAP0008/");
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

            //資料狀態
            ViewBag.statusjqList = sysCodeDao.jqGridList("AP", "DATA_STATUS", true);


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
        /// 查詢清理紀錄
        /// 挑【FAP_VE_TRACE 逾期未兌領清理記錄檔】中清理狀態<>已給付、已清理結案的資料
        /// 1.已給付
        /// 2.已清理結案
        /// 3.已通知尚未給付
        /// 4.尚未通知
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public JsonResult qryProc(string paid_id)
        {
            logger.Info("qryProc begin!!");
            try
            {
                List<OAP0008Model> rows = new List<OAP0008Model>();

                //查詢【FAP_VE_TRACE_PROC 逾期未兌領清理記錄歷程檔】
                FAPVeTrackProcDao fAPVeTrackProcDao = new FAPVeTrackProcDao();
                List < OAP0008Model> procList = fAPVeTrackProcDao.qryForOAP0008(paid_id, new string[] { "", "3", "4" });


                //add by daiyu 20190902
                List<OAP0008Model> rowsFinish = fAPVeTrackProcDao.qryForOAP0008(paid_id, new string[] { "1", "2" });
                if (procList.Count == 0 & rowsFinish.Count > 0) 
                    return Json(new { success = false, err = "輸入之資料己給付或己清理，不可執行!!" });
                


                int i = 0;
                foreach (OAP0008Model model in procList) {
                    if (!"".Equals(StringUtil.toString(model.practice))) {
                        i++;
                        model.temp_id = i.ToString();
                        model.srce_from = "D";
                        model.exec_date = DateUtil.ADDateToChtDate(model.exec_date, 3, "");
                        model.exec_date_1 = model.exec_date;
                        model.level_1_1 = model.level_1;
                        model.level_2_1 = model.level_2;

                        rows.Add(model);
                    }
                }

                //查詢【FAP_VE_TRACE 逾期未兌領清理記錄檔】
                FAPVeTraceDao fAPVeTraceDao = new FAPVeTraceDao();
                List<FAP_VE_TRACE> traceList = fAPVeTraceDao.qryByPaidId(paid_id, new string[] { "", "3", "4" });
                string paid_name = "";

                foreach (FAP_VE_TRACE d in traceList) {
                    
                    paid_name = StringUtil.toString(d.paid_name);

                    if (!"".Equals(StringUtil.toString(d.practice_1))) {
                        i++;
                        OAP0008Model model = new OAP0008Model();
                        DbMvToSc(i, model, d, d.practice_1, d.cert_doc_1, Convert.ToDateTime(d.exec_date_1));
                        rows.Add(model);
                    }

                    if (!"".Equals(StringUtil.toString(d.practice_2)))
                    {
                        i++;
                        OAP0008Model model = new OAP0008Model();
                        DbMvToSc(i, model, d, d.practice_2, d.cert_doc_2, Convert.ToDateTime(d.exec_date_2));
                        rows.Add(model);
                    }

                    if (!"".Equals(StringUtil.toString(d.practice_3)))
                    {
                        i++;
                        OAP0008Model model = new OAP0008Model();
                        DbMvToSc(i, model, d, d.practice_3, d.cert_doc_3, Convert.ToDateTime(d.exec_date_3));
                        rows.Add(model);
                    }

                    if (!"".Equals(StringUtil.toString(d.practice_4)))
                    {
                        i++;
                        OAP0008Model model = new OAP0008Model();
                        DbMvToSc(i, model, d, d.practice_4, d.cert_doc_4, Convert.ToDateTime(d.exec_date_4));
                        rows.Add(model);
                    }

                    if (!"".Equals(StringUtil.toString(d.practice_5)))
                    {
                        i++;
                        OAP0008Model model = new OAP0008Model();
                        DbMvToSc(i, model, d, d.practice_5, d.cert_doc_5, Convert.ToDateTime(d.exec_date_5));
                        rows.Add(model);
                    }
                }

                writePiaLog(rows.Count, paid_id, "Q");

                var jsonData = new { success = true, dataList = rows, paid_name = paid_name};
                return Json(jsonData, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e) {
                return Json(new { success = false, err = "其它錯誤，請洽系統管理員!!" });
            }
            
        }


        private OAP0008Model DbMvToSc(int i, OAP0008Model model, FAP_VE_TRACE d, string practice, string cert_doc, DateTime exec_date) {
            model.temp_id = i.ToString();
            model.srce_from = "M";
            model.check_no = d.check_no;
            model.check_acct_short = d.check_acct_short;
            model.level_1 = d.level_1;
            model.level_1_1 = d.level_1;
            model.level_2 = d.level_2;
            model.level_2_1 = d.level_2;

            model.practice = practice;
            model.practice_1 = practice;
            model.cert_doc = cert_doc;
            model.cert_doc_1 = cert_doc;
            model.exec_date = exec_date.Year - 1911 + exec_date.Month.ToString().PadLeft(2, '0') + exec_date.Day.ToString().PadLeft(2, '0');
            model.exec_date_1 = model.exec_date;

            model.proc_desc = d.proc_desc;
            model.data_status = d.data_status;  //add by daiyu 20200807

            return model;
        }


        /// <summary>
        /// 用"給付對象ID"查"給付對象姓名"
        /// </summary>
        /// <param name="paid_id"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult qryPaidName(string paid_id)
        {
            string paid_name = "";


            try
            {
                FAPVeTraceDao fAPVeTraceDao = new FAPVeTraceDao();
                FAP_VE_TRACE trace = fAPVeTraceDao.chkPaidIdExist(paid_id);
                paid_name = StringUtil.toString(trace.paid_name);
            }
            catch (Exception e) {
                logger.Error(e.ToString());
                return Json(new { success = false, err = "其它錯誤，請洽系統管理員!!" });
            }
            

            if (!"".Equals(paid_name))
                return Json(new { success = true, paid_name = paid_name });
            else
                return Json(new { success = false, err = "查無對應給付對象姓名" });
        }


        /// <summary>
        /// 畫面執行"申請覆核"
        /// </summary>
        /// <param name="code_type"></param>
        /// <param name="gridData"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult execSave(List<OAP0008Model> gridData)
        {
            logger.Info("execSave begin");

            string errStr = "";
            try
            {
                bool bChg = false;
                List<FAP_VE_TRACE_PROC_HIS> dataList = new List<FAP_VE_TRACE_PROC_HIS>();


                foreach (OAP0008Model d in gridData)
                {
                    if (!"".Equals(StringUtil.toString(d.exec_action)))
                    {
                        FAP_VE_TRACE_PROC_HIS dbModel = new FAP_VE_TRACE_PROC_HIS();
                        d.exec_date = DateUtil.formatDateTimeDbToSc(DateUtil.As400ChtDateToADDate(d.exec_date), "D");
                        d.exec_date_1 = DateUtil.formatDateTimeDbToSc(DateUtil.As400ChtDateToADDate(d.exec_date_1), "D");
                        d.update_datetime = null;
                        ObjectUtil.CopyPropertiesTo(d, dbModel);
                        dataList.Add(dbModel);
                        bChg = true;
                    }
                }


                if (bChg == false)
                {
                    if ("".Equals(errStr))
                        return Json(new { success = false, err = "未異動畫面資料，將不進行修改覆核作業!!" }, JsonRequestBehavior.AllowGet);
                    else
                        return Json(new { success = true, err = errStr });
                }


                /*------------------ DB處理   begin------------------*/


                string[] curDateTime = DateUtil.getCurChtDateTime(3).Split(' ');
                //取得流水號
                SysSeqDao sysSeqDao = new SysSeqDao();
                String qPreCode = "VEP" + curDateTime[0].Substring(0, 4);
                var cId = sysSeqDao.qrySeqNo("AP", "VEP", qPreCode).ToString();
                string aply_no = qPreCode + cId.ToString().PadLeft(5, '0');

                string strConn = DbUtil.GetDBFglConnStr();
                using (SqlConnection conn = new SqlConnection(strConn))
                {
                    conn.Open();

                    SqlTransaction transaction = conn.BeginTransaction("Transaction");
                    try
                    {
                        DateTime dt = DateTime.Now;
                        
                        FAPVeTrackProcHisDao fAPVeTrackProcHisDao = new FAPVeTrackProcHisDao();
                        FAPVeTrackProcDao fAPVeTrackProcDao = new FAPVeTrackProcDao();

                        foreach (FAP_VE_TRACE_PROC_HIS d in dataList) {
                            //新增覆核資料至【FAP_VE_TRACE_PROC_HIS 逾期未兌領清理記錄歷程暫存檔】
                            fAPVeTrackProcHisDao.insert(aply_no, dt, Session["UserID"].ToString(), d, conn, transaction);

                            //異動【FAP_VE_TRACE_PROC 逾期未兌領清理記錄歷程檔】資料狀態
                            FAP_VE_TRACE_PROC proc = new FAP_VE_TRACE_PROC();
                            ObjectUtil.CopyPropertiesTo(d, proc);
                            proc.data_status = "2";
                            fAPVeTrackProcDao.updateStatus(dt, Session["UserID"].ToString(), proc, conn, transaction);
                        }


                        //新增覆核資料至【FAP_VE_TRACE_HIS 逾期未兌領清理記錄暫存檔】
                        FAPVeTraceHisDao fAPVeTraceHisDao = new FAPVeTraceHisDao();
                        List<OAP0008Model> traceHisList = gridData.GroupBy(x => new { x.check_acct_short, x.check_no, x.level_1_1, x.level_2_1 })
                            .Select(g => new OAP0008Model
                            {
                                check_acct_short = g.Key.check_acct_short,
                                check_no = g.Key.check_no,
                                level_1 = g.Key.level_1_1,
                                level_2 = g.Key.level_2_1
                            })
                            .ToList<OAP0008Model>();

                        foreach (OAP0008Model d in traceHisList) {
                            fAPVeTraceHisDao.insertForOAP0008(dt, Session["UserID"].ToString(), aply_no,
                                 d.check_no, d.check_acct_short, d.level_1, d.level_2, conn, transaction);
                        }


                        
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
        /// 畫面GRID在儲存前，需先檢查可存檔
        /// </summary>
        /// <param name="status"></param>
        /// <param name="sysType"></param>
        /// <param name="srceFrom"></param>
        /// <param name="srceKind"></param>
        /// <param name="srcePgm"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult chkData(string code_type, string exec_action, string code_id, string code_value)
        {
            errModel errModel = new errModel();
            errModel = chkAplyData(code_type, exec_action, code_id, code_value);

            if (errModel.chkResult)
                return Json(new { success = true });
            else
                return Json(new { success = false, err = errModel.msg });

        }


        private errModel chkAplyData(string code_type, string exec_action, string code_id, string code_value)
        {
            errModel errModel = new errModel();

            if ("A".Equals(exec_action))
            {
                FAPVeCodeDao fAPVeCodeDao = new FAPVeCodeDao();
                FAP_VE_CODE formal = fAPVeCodeDao.qryByKey(code_type, code_id);

                if (!"".Equals(StringUtil.toString(formal.code_id))) {
                    errModel.chkResult = false;
                    errModel.msg = "此筆資料已存在「逾期未兌領代碼設定檔」不可新增!!";
                    return errModel;
                }
            }


            FAPVeCodeHisDao fAPVeCodeHisDao = new FAPVeCodeHisDao();
            FAP_VE_CODE_HIS his = fAPVeCodeHisDao.qryInProssById(code_type, code_id, "");
            if (!"".Equals(StringUtil.toString(his.code_id))) {
                errModel.chkResult = false;
                errModel.msg = "資料覆核中，不可異動此筆資料!!";
                return errModel;
            }

            errModel.chkResult = true;
            errModel.msg = "";
            return errModel;
        }


        private void writePiaLog(int affectRows, string piaOwner, string executionType)
        {
            PIA_LOG_MAIN piaLogMain = new PIA_LOG_MAIN();
            piaLogMain.TRACKING_TYPE = "A";
            piaLogMain.ACCESS_ACCOUNT = Session["UserID"].ToString();
            piaLogMain.ACCOUNT_NAME = Session["UserName"].ToString();
            piaLogMain.PROGFUN_NAME = "OAP0008Controller";
            piaLogMain.EXECUTION_CONTENT = MaskUtil.maskId(piaOwner);
            piaLogMain.AFFECT_ROWS = affectRows;
            piaLogMain.PIA_TYPE = "0100100000";
            piaLogMain.EXECUTION_TYPE = executionType;
            piaLogMain.ACCESSOBJ_NAME = "FAP_VE_TRACE_PROC";
            piaLogMain.PIA_OWNER1 = MaskUtil.maskId(piaOwner);
            PiaLogMainDao piaLogMainDao = new PiaLogMainDao();
            piaLogMainDao.Insert(piaLogMain);

        }


        internal class errModel
        {
            public bool chkResult { get; set; }
            public string msg { get; set; }
        }
    }
}