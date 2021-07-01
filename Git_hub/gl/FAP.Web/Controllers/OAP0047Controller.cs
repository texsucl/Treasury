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
using Microsoft.Reporting.WebForms;
using FAP.Web.AS400PGM;
using NLog.Filters;

/// <summary>
/// 功能說明：OAP0047 追蹤處理結果登錄作業
/// 初版作者：20200910 Daiyu
/// 修改歷程：20200910 Daiyu
/// 需求單號：202008120153-00
/// 修改內容：初版
/// -----------------------------------------
/// 修改歷程：
/// 需求單號：
/// 修改內容：
/// </summary>
///

namespace FAP.Web.Controllers
{
    [Authorize]
    [CheckSessionFilterAttribute]
    public class OAP0047Controller : BaseController
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
                String[] roleInfo = authUtil.chkUserFuncAuth(Session["UserID"].ToString(), "~/OAP0047/");
                if (roleInfo != null && roleInfo.Length == 2)
                {
                    opScope = roleInfo[0];
                    funcName = roleInfo[1];
                }
            }

            ViewBag.opScope = opScope;
            ViewBag.funcName = funcName;
            
            SysCodeDao sysCodeDao = new SysCodeDao();

            //處理結果
            ViewBag.telCallList = sysCodeDao.loadSelectList("AP", "tel_call", true);
            ViewBag.telCalljqList = sysCodeDao.jqGridList("AP", "tel_call", false);

            //電訪對象
            ViewBag.calledPersonList = sysCodeDao.loadSelectList("AP", "called_person", false);

            //資料狀態
            ViewBag.statusjqList = sysCodeDao.jqGridList("AP", "DATA_STATUS", false);

            //清理狀態
            ViewBag.clrStatusjqList = sysCodeDao.jqGridList("AP", "CLR_STATUS", false);

            //給付性質
            FPMCODEDao pPMCODEDao = new FPMCODEDao();
            //ViewBag.oPaidCdjqList = sysCodeDao.jqGridList("AP", "O_PAID_CD", false);
            ViewBag.oPaidCdjqList = pPMCODEDao.jqGridList("PAID_CDTXT", "AP", false);

            //派案狀態
            ViewBag.dispatchStatusjqList = sysCodeDao.jqGridList("AP", "dispatch_status", false);

            //覆核狀態
            ViewBag.apprStatjqList = sysCodeDao.jqGridList("AP", "APPR_STAT", false);

            //電訪覆核結果
            ViewBag.telApprCodejqList = sysCodeDao.jqGridList("AP", "TEL_APPR_CODE", false);

            //電訪覆核結果
            ViewBag.telApprCodejqList = sysCodeDao.jqGridList("AP", "TEL_APPR_CODE", false);


            FAPVeCodeDao fAPVeCodeDao = new FAPVeCodeDao();
            //清理大類
            ViewBag.level1List = fAPVeCodeDao.loadSelectList("CLR_LEVEL1", true);

            //清理小類
            ViewBag.level2List = fAPVeCodeDao.loadSelectList("CLR_LEVEL2", true);


            return View();
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
            catch (Exception e)
            {
                logger.Error(e.ToString());
                return Json(new { success = false, err = "其它錯誤，請洽系統管理員!!" });
            }


            if (!"".Equals(paid_name))
                return Json(new { success = true, paid_name = paid_name });
            else
                return Json(new { success = false, err = "查無對應給付對象姓名" });
        }



            /// <summary>
            /// 畫面執行查詢
            /// </summary>
            /// <param name="model"></param>
            /// <returns></returns>
            [HttpPost]
        public JsonResult qryTelInterview(OAP0046Model model)
        {
            try
            {
                switch (model.qType) {
                    case "q_paid_id":
                        model.policy_no = "";
                        model.policy_seq = "";
                        model.id_dup = "";
                        model.paid_name = "";
                        model.check_no = "";
                        break;
                    case "q_check_no":
                        model.paid_id = "";
                        model.policy_seq = "";
                        model.id_dup = "";
                        model.paid_name = "";
                        break;
                    case "q_policy_no":
                        model.paid_id = "";
                        model.check_no = "";
                        break;
                }
                
                    
                    
                //查詢電訪紀錄檔，一個電訪編號以一張支票顯示
                FAPTelCheckDao fAPTelCheckDao = new FAPTelCheckDao();
                List<OAP0046DModel> rows = fAPTelCheckDao.qryForOAP0046(model.paid_id
                    , model.policy_no, model.policy_seq, model.id_dup, model.paid_name
                    , model.check_no, "0047").OrderBy(x => x.tel_proc_no).ToList();


                Dictionary<string, ADModel> userNameMap = new Dictionary<string, ADModel>();
                string usr_id = "";
                CommonUtil commonUtil = new CommonUtil();


                FAPTelCodeDao fAPTelCodeDao = new FAPTelCodeDao();
                List<FAP_TEL_CODE> procList = fAPTelCodeDao.qryByGrp("tel_call");

                List<OAP0046DModel> dataList = new List<OAP0046DModel>();
                string _tel_proc_no = "";
                foreach (OAP0046DModel d in rows) {


                    if (!_tel_proc_no.Equals(d.tel_proc_no)) {
                        dataList.Add(d);
                        _tel_proc_no = d.tel_proc_no;
                    }


                    string id = procList.Where(x => x.code_id == d.tel_result).FirstOrDefault().proc_id;

                    d.update_id = StringUtil.toString(id);

                    //取得申請人姓名
                    usr_id = StringUtil.toString(d.update_id);
                    if (!userNameMap.ContainsKey(usr_id))
                    {
                        if (!"".Equals(usr_id))
                        {
                            ADModel adModel = new ADModel();
                            adModel = commonUtil.qryEmp(usr_id);
                            userNameMap.Add(usr_id, adModel);
                        }

                    }

                    d.update_id = d.update_id + " " + StringUtil.toString(userNameMap[usr_id].name);
                }

                //寫稽核軌跡
                if (!"".Equals(StringUtil.toString(model.paid_id))) 
                    writePiaLog(dataList.Count, model.paid_id, "Q");
                

                
                if (dataList.Count > 0)
                    model.paid_id = StringUtil.toString(dataList[0].paid_id);


                return Json(new { success = true, dataList = dataList, paid_id = model.paid_id });
            }
            catch (Exception e)
            {
                logger.Info(e.ToString());
                return Json(new { success = false, err = "其它錯誤，請洽系統管理員!!" });
            }

        }

        [HttpPost]
        public JsonResult qryTelProc(string tel_proc_no)
        {
            //查詢電訪明細資料
            FAPTelInterviewDao fAPTelInterviewDao = new FAPTelInterviewDao();
            FAP_TEL_INTERVIEW d = fAPTelInterviewDao.qryByTelProcNo(tel_proc_no);
            OAP0046Model model = new OAP0046Model();
            ObjectUtil.CopyPropertiesTo(d, model);

            DateTime dt = Convert.ToDateTime(model.tel_interview_datetime);
            model.tel_interview_datetime = DateUtil.ADDateToChtDate(dt, 3, "");

            dt = Convert.ToDateTime(model.tel_interview_f_datetime);
            model.tel_interview_f_datetime = DateUtil.ADDateToChtDate(dt, 3, "");

            try
            {
                if (!"".Equals(StringUtil.toString(model.counter_date))) {
                    dt = Convert.ToDateTime(model.counter_date);
                    model.counter_date = DateUtil.ADDateToChtDate(dt, 3, "");
                } else
                    model.counter_date = "";
            }
            catch (Exception e)
            {
                model.counter_date = "";
            }

            model.tel_result_o = model.tel_result;
            model.tel_interview_datetime = "";
            model.tel_result = "";


            //查詢電訪處理過程
            CommonUtil commonUtil = new CommonUtil();
            Dictionary<string, ADModel> empMap = new Dictionary<string, ADModel>();

            FAPTelInterviewHisDao fAPTelInterviewHisDao = new FAPTelInterviewHisDao();
            List<FAP_TEL_INTERVIEW_HIS> _proc_list = new List<FAP_TEL_INTERVIEW_HIS>();
            _proc_list = fAPTelInterviewHisDao.qryByTelProcNo(tel_proc_no, new string[] { "1", "2" }, new string[] {  });

            List<OAP0047ProcModel> dataList = new List<OAP0047ProcModel>();
            foreach (FAP_TEL_INTERVIEW_HIS _proc in _proc_list) {
                OAP0047ProcModel prodModel = new OAP0047ProcModel();
                ObjectUtil.CopyPropertiesTo(_proc, prodModel);

                switch (_proc.data_type) {
                    case "1":
                        prodModel.data_type_desc = "第一次電訪結果";
                        break;
                    case "2":
                        prodModel.data_type_desc = "追踨結果登錄";
                        break;

                }

                //取得處理人員姓名
                if (!"".Equals(prodModel.update_id)) {
                    if (!empMap.ContainsKey(prodModel.update_id))
                    {
                        ADModel adModel = new ADModel();
                        adModel = commonUtil.qryEmp(prodModel.update_id);
                        empMap.Add(prodModel.update_id, adModel);
                    }
                    prodModel.update_name = empMap[prodModel.update_id].name;

                }

                //取得覆核人員姓名
                if (!"".Equals(StringUtil.toString(prodModel.appr_id)))
                {
                    if (!empMap.ContainsKey(prodModel.appr_id))
                    {
                        ADModel adModel = new ADModel();
                        adModel = commonUtil.qryEmp(prodModel.appr_id);
                        empMap.Add(prodModel.appr_id, adModel);
                    }
                    prodModel.appr_name = empMap[prodModel.appr_id].name;

                }

                if (_proc.update_datetime != null)
                    prodModel.update_datetime = DateUtil.DatetimeToString(_proc.update_datetime, "");
                else
                    prodModel.update_datetime = "";

                if (_proc.approve_datetime != null)
                    prodModel.approve_datetime = DateUtil.DatetimeToString(_proc.approve_datetime, "");
                else
                    prodModel.approve_datetime = "";

                dataList.Add(prodModel);
            }


            return Json(new { success = true, model = model, dataList = dataList });
        }



        [HttpPost]
        public JsonResult execSave(OAP0046Model model, List<OAP0046DModel> gridData)
        {
            if (gridData.Count == 0) 
                return Json(new { success = false, err = "未選取要作業的資料!!" });


            //檢查追蹤人員
            FAPTelCodeDao fAPTelCodeDao = new FAPTelCodeDao();
            FAP_TEL_CODE _tel_code = fAPTelCodeDao.qryByKey("tel_call", model.tel_result_o);
            if (_tel_code == null)
                return Json(new { success = false, err = "查無對應追蹤人員!!" });
            else { 
                if(!StringUtil.toString(_tel_code.proc_id).Equals(Session["UserID"].ToString()))
                    return Json(new { success = false, err = "非對應的追蹤人員，不可執行本作業!!" });
            }


            DateTime now = DateTime.Now;
            string[] curDateTime = BO.DateUtil.getCurChtDateTime(3).Split(' ');

            string strConn = DbUtil.GetDBFglConnStr();
            using (SqlConnection conn = new SqlConnection(strConn))
            {
                conn.Open();

                SqlTransaction transaction = conn.BeginTransaction("Transaction");

                try
                {

                    model.tel_interview_datetime = DateUtil.As400ChtDateToADDate(model.tel_interview_datetime);
                    model.tel_interview_f_datetime = DateUtil.As400ChtDateToADDate(model.tel_interview_f_datetime);

                    try
                    {
                        if(!"".Equals(StringUtil.toString(model.counter_date)))
                            model.counter_date = DateUtil.As400ChtDateToADDate(model.counter_date);
                    }
                    catch (Exception e) {
                        return Json(new { success = false, err = "臨櫃日期輸入錯誤!!" });
                    }

                    FAPTelInterviewDao fAPTelInterviewDao = new FAPTelInterviewDao();
                    FAPTelInterviewHisDao fAPTelInterviewHisDao = new FAPTelInterviewHisDao();
                    FAPTelProcDao fAPTelProcDao = new FAPTelProcDao();
                    FAPTelCheckHisDao fAPTelCheckHisDao = new FAPTelCheckHisDao();
                    FAPTelCheckDao fAPTelCheckDao = new FAPTelCheckDao();

                    //取得覆核單號
                    SysSeqDao sysSeqDao = new SysSeqDao();
                    string qPreCode = "0047" + curDateTime[0].Substring(0, 5);
                    var cId = sysSeqDao.qrySeqNo("AP", "0047", qPreCode).ToString();
                    string _aply_no = qPreCode + cId.ToString().PadLeft(3, '0');


                    foreach (OAP0046DModel d in gridData)
                    {


                        //異動【FAP_TEL_CHECK 電訪支票檔】資料狀態
                        FAP_TEL_CHECK _tel_check = new FAP_TEL_CHECK();
                        ObjectUtil.CopyPropertiesTo(d, _tel_check);
                        _tel_check.tel_proc_no = model.tel_proc_no;
                        _tel_check.tel_std_type = "tel_assign_case";
                        _tel_check.update_id = Session["UserID"].ToString();
                        _tel_check.update_datetime = now;
                        _tel_check.data_status = "2";
                        fAPTelCheckDao.updDataStatusByTelProcNo(_tel_check, conn, transaction);
                    }


                    FAP_TEL_INTERVIEW _tel_interview_o = fAPTelInterviewDao.qryByTelProcNo(model.tel_proc_no);
                    

                    //新增【FAP_TEL_INTERVIEW_HIS 電訪及追踨記錄暫存檔】
                    FAP_TEL_INTERVIEW_HIS interview_his = new FAP_TEL_INTERVIEW_HIS();
                    ObjectUtil.CopyPropertiesTo(model, interview_his);
                    interview_his.aply_no = _aply_no;
                    interview_his.tel_proc_no = model.tel_proc_no;
                    interview_his.data_type = "2";
                    interview_his.tel_interview_id = Session["UserID"].ToString();
                    interview_his.tel_interview_f_datetime = _tel_interview_o.tel_interview_f_datetime;

                    int _tel_result_cnt = 0;
                    try
                    {
                        _tel_result_cnt = Convert.ToInt32(_tel_interview_o.tel_result_cnt);
                    }
                    catch (Exception e) { 
                    
                    }
                    if (StringUtil.toString(model.tel_result).Equals(StringUtil.toString(_tel_interview_o.tel_result)))
                        _tel_result_cnt++;
                    else
                        _tel_result_cnt = 1;

                    interview_his.tel_result_cnt = _tel_result_cnt;
                    interview_his.dispatch_status = "1";
                    interview_his.exec_action = "A";
                    interview_his.update_datetime = now;
                    interview_his.update_id = Session["UserID"].ToString();
                    interview_his.appr_stat = "1";
                    fAPTelInterviewHisDao.insert(interview_his, conn, transaction);


                    //異動【FAP_TEL_INTERVIEW 電訪及追踨記錄檔】資料狀態
                    FAP_TEL_INTERVIEW _tel_interview = new FAP_TEL_INTERVIEW();
                    _tel_interview.tel_proc_no = model.tel_proc_no;
                    _tel_interview.data_status = "2";
                    _tel_interview.update_datetime = now;
                    _tel_interview.update_id = Session["UserID"].ToString();
                    fAPTelInterviewDao.updDataStatus(_tel_interview, conn, transaction);


                    //新增【FAP_TEL_PROC 電訪及追踨記錄歷程檔】
                    FAP_TEL_PROC _tel_proc = new FAP_TEL_PROC();
                    _tel_proc.tel_proc_no = model.tel_proc_no;
                    _tel_proc.aply_no = _aply_no;
                    _tel_proc.data_type = "2";
                    _tel_proc.proc_id = Session["UserID"].ToString();
                    _tel_proc.proc_datetime = interview_his.tel_interview_datetime;
                    _tel_proc.proc_status = model.tel_result;
                    _tel_proc.reason = model.reason;
                    _tel_proc.appr_stat = "1";
                    fAPTelProcDao.insert(_tel_proc, conn, transaction);


                    transaction.Commit();

                    if (!"".Equals(StringUtil.toString(model.paid_id)))
                        writePiaLog(gridData.Count, model.paid_id, "E");

                    return Json(new { success = true, aply_no = _aply_no, tel_proc_no = model.tel_proc_no , err = ""});
                }
                catch (Exception e)
                {
                    logger.Error(e.ToString());
                    transaction.Rollback();
                    return Json(new { success = false, err = "其它錯誤，請洽系統管理員!!" });
                }
            }
        }



        public FileContentResult downloadRpt(String id)
        {
            byte[] fileBytes = System.IO.File.ReadAllBytes(Server.MapPath("~/Temp/") + "OAP0046P" + "_" + id + ".pdf");


            string fullPath = Server.MapPath("~/Temp/") + "OAP0046P" + "_" + id + ".pdf";
            if (System.IO.File.Exists(fullPath))
                System.IO.File.Delete(fullPath);


            return File(fileBytes, "application/pdf", "逾期未兌領支票處理結果.pdf");
        }


        private void writePiaLog(int affectRows, string piaOwner, string executionType)
        {
            PIA_LOG_MAIN piaLogMain = new PIA_LOG_MAIN();
            piaLogMain.TRACKING_TYPE = "A";
            piaLogMain.ACCESS_ACCOUNT = Session["UserID"].ToString();
            piaLogMain.ACCOUNT_NAME = Session["UserName"].ToString();
            piaLogMain.PROGFUN_NAME = "OAP0046Controller";
            piaLogMain.EXECUTION_CONTENT = MaskUtil.maskId(piaOwner);
            piaLogMain.AFFECT_ROWS = affectRows;
            piaLogMain.PIA_TYPE = "0100100000";
            piaLogMain.EXECUTION_TYPE = executionType;
            piaLogMain.ACCESSOBJ_NAME = "FAP_TEL_INTERVIEW";
            piaLogMain.PIA_OWNER1 = MaskUtil.maskId(piaOwner);
            PiaLogMainDao piaLogMainDao = new PiaLogMainDao();
            piaLogMainDao.Insert(piaLogMain);

        }


        internal class rptModel
        {

            public string check_no { get; set; }
            public string check_date { get; set; }
            public string check_amt { get; set; }
            public string policy_no { get; set; }


            public rptModel()
            {
                check_no = "";
                check_date = "";
                check_amt = "";
                policy_no = "";
            }
        }
    }
}