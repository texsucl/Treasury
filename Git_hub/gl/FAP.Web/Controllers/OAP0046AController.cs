using FAP.Web.ActionFilter;
using FAP.Web.BO;
using FAP.Web.Daos;
using FAP.Web.ViewModels;
using System;
using System.Collections.Generic;
using System.Web.Mvc;
using System.Text;
using System.Linq;
using System.Data.SqlClient;
using FAP.Web.Models;
using System.Threading.Tasks;
using System.Data.EasycomClient;
using FAP.Web.AS400PGM;
using FAP.Web.AS400Models;

/// <summary>
/// 功能說明：OAP0046 電訪處理結果登錄作業
/// 初版作者：20200909 Daiyu
/// 修改歷程：20200909 Daiyu
/// 需求單號：202008120153-00
/// 修改內容：初版
/// -----------------------------------------
/// 修改歷程：20210205 daiyu 
/// 需求單號：202101280283-00
/// 修改內容：1.畫面增加篩選條件：申請人、電訪編號、給付對象ID，可擇一輸入，若沒選，就SHOW全部。
///           2.畫面呈現改預設SHOW 20筆資料。
/// </summary>
///

namespace FAP.Web.Controllers
{
    [Authorize]
    [CheckSessionFilterAttribute]
    public class OAP0046AController : BaseController
    {
        static private NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        static string funcName = "";

        /// <summary>
        /// 畫面初始
        /// </summary>
        /// <returns></returns>
        public ActionResult Index()
        {

            UserAuthUtil authUtil = new UserAuthUtil();


            string opScope = "";
            //string funcName = "";

            if (Session["UserID"] != null)
            {
                String[] roleInfo = authUtil.chkUserFuncAuth(Session["UserID"].ToString(), "~/OAP0046A/");
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
        /// 查詢待覆核的資料
        /// </summary>
        /// <returns></returns>
        //[HttpPost]
        //public JsonResult LoadData()
        //{
        //    try
        //    {
        //        FAPTelInterviewHisDao fAPTelInterviewHisDao = new FAPTelInterviewHisDao();
        //        List<OAP0046Model> dataList = fAPTelInterviewHisDao.qryForOAP0046A("1", "1", "");
        //        List<summaryModel> rows = new List<summaryModel>();

        //        Dictionary<string, ADModel> userNameMap = new Dictionary<string, ADModel>();
        //        string usr_id = "";
        //        CommonUtil commonUtil = new CommonUtil();

        //        foreach (OAP0046Model d in dataList)
        //        {

        //            //取得申請人姓名
        //            usr_id = StringUtil.toString(d.update_id);
        //            if (!userNameMap.ContainsKey(usr_id))
        //            {
        //                if (!"".Equals(usr_id))
        //                {
        //                    ADModel adModel = new ADModel();
        //                    adModel = commonUtil.qryEmp(usr_id);
        //                    userNameMap.Add(usr_id, adModel);
        //                }
        //                //d.create_id = d.create_id + " " + userNameMap[usr_id].name;

        //            }


        //            summaryModel summaryModel = new summaryModel();
        //            ObjectUtil.CopyPropertiesTo(d, summaryModel);
        //            summaryModel.update_name = userNameMap[usr_id].name;

        //            rows.Add(summaryModel);
        //        }


        //        var jsonData = new { success = true, rows };
        //        return Json(jsonData, JsonRequestBehavior.AllowGet);
        //    }
        //    catch (Exception e)
        //    {
        //        logger.Error(e.ToString);
        //        var jsonData = new { success = false, err = e.ToString() };
        //        return Json(jsonData, JsonRequestBehavior.AllowGet);
        //    }
        //}


        [HttpPost]
        public JsonResult qryAply(OAP0046Model model)
        {
            try
            {
                FAPTelInterviewHisDao fAPTelInterviewHisDao = new FAPTelInterviewHisDao();
                List<OAP0046Model> dataList = fAPTelInterviewHisDao.qryForOAP0046A("1", "1", ""
                    , StringUtil.toString(model.update_id), StringUtil.toString(model.tel_proc_no), StringUtil.toString(model.paid_id));    //modify by daiyu 20210205
                List<summaryModel> rows = new List<summaryModel>();

                Dictionary<string, ADModel> userNameMap = new Dictionary<string, ADModel>();
                string usr_id = "";
                CommonUtil commonUtil = new CommonUtil();

                foreach (OAP0046Model d in dataList)
                {

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
                        //d.create_id = d.create_id + " " + userNameMap[usr_id].name;

                    }


                    summaryModel summaryModel = new summaryModel();
                    ObjectUtil.CopyPropertiesTo(d, summaryModel);
                    summaryModel.update_name = userNameMap[usr_id].name;

                    rows.Add(summaryModel);
                }

                if (rows.Count > 0) //add by daiyu 20210226
                    rows = rows.OrderBy(x => x.update_name).ToList();


                var jsonData = new { success = true, dataList = rows };
                return Json(jsonData, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                logger.Error(e.ToString);
                var jsonData = new { success = false, err = e.ToString() };
                return Json(jsonData, JsonRequestBehavior.AllowGet);
            }
        }




        /// <summary>
        /// 開啟修改明細畫面
        /// </summary>
        /// <param name="tel_proc_no"></param>
        /// <returns></returns>
        public ActionResult detailAply(string aply_no)
        {
            ViewBag.funcName = funcName;

            SysCodeDao sysCodeDao = new SysCodeDao();
            //電訪處理結果
            ViewBag.telCallList = sysCodeDao.loadSelectList("AP", "tel_call", true);

            //電訪覆核結果
            ViewBag.telApprCodeList = sysCodeDao.loadSelectList("AP", "TEL_APPR_CODE", true);

            //電訪對象
            ViewBag.calledPersonList = sysCodeDao.loadSelectList("AP", "called_person", false);

            //電訪對象
            ViewBag.calledPersonList = sysCodeDao.loadSelectList("AP", "called_person", false);


            FAPVeCodeDao fAPVeCodeDao = new FAPVeCodeDao();
            //清理大類
            ViewBag.level1List = fAPVeCodeDao.loadSelectList("CLR_LEVEL1", true);

            //清理小類
            ViewBag.level2List = fAPVeCodeDao.loadSelectList("CLR_LEVEL2", true);


            //清理狀態
            ViewBag.statusjqList = sysCodeDao.jqGridList("AP", "CLR_STATUS", true);

            //給付性質
            FPMCODEDao pPMCODEDao = new FPMCODEDao();
            //ViewBag.oPaidCdjqList = sysCodeDao.jqGridList("AP", "O_PAID_CD", false);
            ViewBag.oPaidCdjqList = pPMCODEDao.jqGridList("PAID_CDTXT", "AP", false);

            

            OAP0046Model model = new OAP0046Model();

        



            FAPTelInterviewHisDao fAPTelInterviewHisDao = new FAPTelInterviewHisDao();
            List<OAP0046Model> dataList = fAPTelInterviewHisDao.qryForOAP0046A("1", "1", aply_no, "", "", "");  //modify by daiyu 20210205

            if (dataList.Count == 0)
            {
                
                ViewBag.bHaveData = "N";
                return View(model);
            }
            else
                model = dataList[0];

            ViewBag.bHaveData = "Y";
            ViewBag.aply_no = model.aply_no;

            writePiaLog(1, StringUtil.toString(model.paid_id), "Q");


            DateTime dt = Convert.ToDateTime(model.tel_interview_datetime);
            model.tel_interview_datetime = DateUtil.ADDateToChtDate(dt, 3, "");

            try
            {
                dt = Convert.ToDateTime(model.counter_date);
                model.counter_date = DateUtil.ADDateToChtDate(dt, 3, "");
            }
            catch (Exception e) {
                model.counter_date = "";
            }

            


            CommonUtil commonUtil = new CommonUtil();
            ADModel adModel = new ADModel();
            adModel = commonUtil.qryEmp(model.update_id);
            model.update_name = adModel.name;

            return View(model);

           
        }



        [HttpPost]
        public JsonResult detailRow(string aply_no)
        {
            ViewBag.funcName = funcName;

            List<TelDispatchRptModel> dataList = new List<TelDispatchRptModel>();
            //List<OAP0046DModel> dataList = new List<OAP0046DModel>();

            FAPTelCheckHisDao fAPTelCheckHisDao = new FAPTelCheckHisDao();


            dataList = fAPTelCheckHisDao.qryByAplyNo("tel_assign_case", aply_no);

            


            var jsonData = new { success = true, rows = dataList };
            return Json(jsonData, JsonRequestBehavior.AllowGet);
        }




        /// <summary>
        /// 核可/退回
        /// </summary>
        /// <param name="model"></param>
        /// <param name="apprStat"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult execSave(OAP0046Model model, string appr_stat)
        {

            if (StringUtil.toString(model.update_id).StartsWith(Session["UserID"].ToString()))
                return Json(new { success = false, err = "申請人與覆核人員不可相同!!" }, JsonRequestBehavior.AllowGet);


            string strConn = DbUtil.GetDBFglConnStr();
            using (SqlConnection conn = new SqlConnection(strConn))
            {
                conn.Open();
                SqlTransaction transaction = conn.BeginTransaction("Transaction");

                
                FAPTelInterviewHisDao fAPTelInterviewHisDao = new FAPTelInterviewHisDao();
                FAPTelProcDao fAPTelProcDao = new FAPTelProcDao();
                FAPTelCheckHisDao fAPTelCheckHisDao = new FAPTelCheckHisDao();
                FAPTelCheckDao fAPTelCheckDao = new FAPTelCheckDao();

                FAP_TEL_INTERVIEW_HIS _his = fAPTelInterviewHisDao.qryByAplyNo(model.aply_no);
                if(_his != null)
                    if(!"1".Equals(_his.appr_stat))
                        return Json(new { success = false, err = "此案件已被處理!!" }, JsonRequestBehavior.AllowGet);


                DateTime now = DateTime.Now;

                try
                {
                    //新增【FAP_TEL_PROC 電訪及追踨記錄歷程檔】
                    FAP_TEL_INTERVIEW_HIS _his_o = fAPTelInterviewHisDao.qryByTelProcNo(model.tel_proc_no, "1", "1");
                    FAP_TEL_PROC _tel_proc = new FAP_TEL_PROC();
                    ObjectUtil.CopyPropertiesTo(_his_o, _tel_proc);
                    _tel_proc.proc_id = _his_o.update_id;
                    _tel_proc.proc_datetime = _his_o.tel_interview_datetime;
                    _tel_proc.proc_status = model.tel_result;
                    _tel_proc.reason = model.reason;
                    _tel_proc.appr_status = model.tel_appr_result;
                    _tel_proc.appr_stat = appr_stat;
                    _tel_proc.appr_datetime = now;
                    _tel_proc.appr_id = Session["UserID"].ToString();
                    fAPTelProcDao.insert(_tel_proc, conn, transaction);


                    //處理駁回資料
                    if ("3".Equals(StringUtil.toString(appr_stat)))
                    {
                        //異動【FAP_TEL_CHECK 電訪支票檔】.資料狀態
                        List<OAP0046DModel> checkList = fAPTelCheckHisDao.qryByTelProcNo(model.tel_proc_no, "1", "1");
                        foreach (OAP0046DModel d in checkList)
                        {
                            FAP_TEL_CHECK check = new FAP_TEL_CHECK();
                            ObjectUtil.CopyPropertiesTo(d, check);
                            check.tel_std_type = "tel_assign_case";
                            check.data_status = "1";
                            check.update_datetime = now;
                            check.update_id = Session["UserID"].ToString();
                            fAPTelCheckDao.updDataStatus(check, conn, transaction);
                        }

                        //異動【FAP_TEL_CHECK_HIS 電訪支票暫存檔】.覆核狀態
                        fAPTelCheckHisDao.updateApprStatus(Session["UserID"].ToString(), appr_stat, model.aply_no, "tel_assign_case", now, conn, transaction);

                        //異動【FAP_TEL_INTERVIEW_HIS 電訪及追踨記錄暫存檔】.覆核狀態
                        fAPTelInterviewHisDao.updateApprStatus(Session["UserID"].ToString(), appr_stat, model.aply_no, model.tel_proc_no, "1", now, conn, transaction);
                    }


                    //處理核可資料
                    if ("2".Equals(StringUtil.toString(appr_stat)))
                    {
                        //異動【FAP_TEL_CHECK 電訪支票檔】
                        List<OAP0046DModel> checkList = fAPTelCheckHisDao.qryByTelProcNo(model.tel_proc_no, "1", "1");
                        foreach (OAP0046DModel d in checkList)
                        {
                            FAP_TEL_CHECK check = new FAP_TEL_CHECK();
                            ObjectUtil.CopyPropertiesTo(d, check);
                            check.tel_std_type = "tel_assign_case";
                            check.tel_proc_no = model.tel_proc_no;
                            check.data_status = "1";
                            check.update_datetime = now;
                            check.update_id = Session["UserID"].ToString();
                            
                            fAPTelCheckDao.updTelProcNo(check, conn, transaction);
                        }

                        model.tel_interview_datetime = DateUtil.As400ChtDateToADDate(model.tel_interview_datetime);
                        model.tel_interview_f_datetime = model.tel_interview_datetime;

                        try
                        {
                            if (!"".Equals(StringUtil.toString(model.counter_date)))
                                model.counter_date = DateUtil.As400ChtDateToADDate(model.counter_date);
                        }
                        catch (Exception e)
                        {
                            return Json(new { success = false, err = "臨櫃日期輸入錯誤!!" });
                        }


                        //異動【FAP_TEL_CHECK_HIS 電訪支票暫存檔】.覆核狀態
                        fAPTelCheckHisDao.updateApprStatus(Session["UserID"].ToString(), appr_stat, model.aply_no, "tel_assign_case", now, conn, transaction);

                        //異動【FAP_TEL_INTERVIEW_HIS 電訪及追踨記錄暫存檔】.覆核狀態
                        fAPTelInterviewHisDao.updateApprResult(Session["UserID"].ToString()
                            , appr_stat, model, "1", now, conn, transaction);


                        //新增【FAP_TEL_INTERVIEW 電訪及追踨記錄檔】
                        FAP_TEL_INTERVIEW _tel = new FAP_TEL_INTERVIEW();
                        ObjectUtil.CopyPropertiesTo(_his_o, _tel);

                        

                        //ObjectUtil.CopyPropertiesTo(model, _tel);

                        _tel.tel_result = model.tel_result;
                        _tel.tel_result_cnt = 1;
                        _tel.tel_appr_result = model.tel_appr_result;
                        _tel.called_person = model.called_person;
                        _tel.tel_zip_code = model.tel_zip_code;
                        _tel.tel_addr = model.tel_addr;
                        _tel.cust_tel = model.cust_tel;
                        _tel.cust_counter = model.cust_counter;
                        _tel.tel_mail = model.tel_mail;
                        _tel.level_1 = model.level_1;
                        _tel.level_2 = model.level_2;
                        _tel.reason = model.reason;

                        _tel.update_datetime = _his_o.update_datetime;
                        _tel.update_id = _his_o.update_id;
                        _tel.tel_appr_datetime = now;
                        _tel.data_status = "1";

                        VeTelUtil veTelUtil = new VeTelUtil();
                        FAPPPAWModel ppaw = veTelUtil.procTelInterview(Session["UserID"].ToString(), "A"
                            , _his_o.aply_no, _tel, model.paid_id, model.paid_name, now, conn, transaction);

                        #region 回寫AS400  FAPPPAW0  逾期未兌領信函歸戶工作檔 
                        if (!"".Equals(StringUtil.toString(ppaw.paid_id))) {
                            List<FAPPPAWModel> ppawList = new List<FAPPPAWModel>();
                            ppawList.Add(ppaw);

                            using (EacConnection conn400 = new EacConnection(CommonUtil.GetEasycomConn()))
                            {
                                conn400.Open();

                                EacTransaction transaction400 = conn400.BeginTransaction();
                                try
                                {
                                    veTelUtil.procAs400TelAddr(ppaw, ppawList, conn400, transaction400);
                                    transaction400.Commit();
                                    //transaction.Commit();
                                }
                                catch (Exception e)
                                {
                                    logger.Error(e.ToString());
                                    transaction400.Rollback();
                                    transaction.Rollback();
                                    return Json(new { success = false, err = "其它錯誤，請洽系統管理員!!" }, JsonRequestBehavior.AllowGet);
                                }
                            }
                        }
                        
                        #endregion


                    }


                    transaction.Commit();

                    writePiaLog(1, StringUtil.toString(model.paid_id), "E");



                    return Json(new { success = true });
                }
                catch (Exception e)
                {
                    transaction.Rollback();

                    logger.Error("[execSave]其它錯誤：" + e.ToString());

                    return Json(new { success = false, err = "其它錯誤，請洽系統管理員!!" }, JsonRequestBehavior.AllowGet);

                }
            }
        }



        /// <summary>
        /// 電訪處理結果="寄信"：若"給付對象ID"<>空白，則將地址轉入清理記錄檔
        /// </summary>
        /// <param name="_paid_id"></param>
        /// <param name="_paid_name"></param>
        /// <param name="_tel"></param>
        /// <param name="now"></param>
        /// <param name="conn"></param>
        /// <param name="transaction"></param>
        /// <returns></returns>
        private FAPPPAWModel procAddr(string _paid_id, string _paid_name, FAP_TEL_INTERVIEW _tel, DateTime now
            , SqlConnection conn, SqlTransaction transaction) {
            FAPPPAWModel _tel_addr = new FAPPPAWModel();

            FAPHouseholdAddrDao addrDao = new FAPHouseholdAddrDao();
            FAP_HOUSEHOLD_ADDR oAddr = addrDao.qryByKey(_paid_id, "TEL");
            OAP0009Model addr = new OAP0009Model();
            addr.paid_id = _paid_id;
            addr.addr_type = "TEL";
            addr.paid_name = _paid_name;
            addr.zip_code = _tel.tel_zip_code;
            addr.address = _tel.tel_addr;

            if (!"".Equals(StringUtil.toString(oAddr.paid_id)))
            {
                addrDao.update(now, Session["UserID"].ToString(), addr, conn, transaction);
            }
            else
            {
                addrDao.insert(now, Session["UserID"].ToString(), addr, conn, transaction);
            }

            string[] chtDt = BO.DateUtil.getCurChtDateTime(4).Split(' ');
            _tel_addr.report_tp = "X0007";
            _tel_addr.dept_group = "2";
            _tel_addr.paid_id = _paid_id;
            _tel_addr.check_no = _paid_id;
            _tel_addr.r_zip_code = _tel.tel_zip_code;
            _tel_addr.r_addr = _tel.tel_addr;
            _tel_addr.entry_id = Session["UserID"].ToString();
            _tel_addr.entry_date = chtDt[0];
            _tel_addr.entry_time = chtDt[1];


            return _tel_addr;

        }


        //private FAPPPAWModel procTelInterview(string exec_action, string aply_no, FAP_TEL_INTERVIEW _tel, string _paid_id, string _paid_name
        //    , DateTime now,  SqlConnection conn, SqlTransaction transaction) {
        //    FAPPPAWModel _tel_addr = new FAPPPAWModel();

        //    FAPTelInterviewDao fAPTelInterviewDao = new FAPTelInterviewDao();

        //    string _dispatch_status = "";
        //    string _clean_status = "";

        //    switch (_tel.tel_appr_result)
        //    {
        //        case "11":  //進入追蹤
        //            _dispatch_status = "1"; //派件中
        //            break;
        //        case "12":  //PENDING
        //            _dispatch_status = "2"; //電訪結束
        //            break;
        //        case "13":  //進入清理
        //            _dispatch_status = "2"; //電訪結束
        //            _clean_status = "1";
        //            _tel.clean_date = now;
        //            break;
        //        case "15":  //轉行政單位
        //            _dispatch_status = "2"; //電訪結束
        //            break;
        //        case "16":  //給付結案
        //            _dispatch_status = "2"; //電訪結束
        //            break;
        //        case "14":  //重新派案
        //            _dispatch_status = "3"; //重新派件
        //            break;
        //    }
        //    _tel.dispatch_status = _dispatch_status;
        //    _tel.clean_status = _clean_status;


        //    if("A".Equals(exec_action))
        //        fAPTelInterviewDao.insert(_tel, conn, transaction);


        //    //新增【FAP_VE_TRACE_PROC 逾期未兌領清理記錄歷程檔】
        //    FAPVeTrackProcDao fAPVeTrackProcDao = new FAPVeTrackProcDao();
        //    fAPVeTrackProcDao.insertTelCheck("tel_assign_case", aply_no, "G6", "F2 ", _tel.reason, (DateTime)_tel.tel_interview_datetime, conn, transaction);
        //    FAPVeTraceDao faPVeTraceDao = new FAPVeTraceDao();
        //    faPVeTraceDao.updateForTelCheck("tel_assign_case", aply_no, _tel.reason, (DateTime)_tel.tel_interview_datetime, conn, transaction);

        //    FAPTelCheckDao fAPTelCheckDao = new FAPTelCheckDao();
        //    FAP_TEL_CHECK _tel_check = new FAP_TEL_CHECK();
        //    _tel_check.dispatch_status = _dispatch_status;
        //    _tel_check.update_id = _tel.update_id;
        //    _tel_check.update_datetime = _tel.update_datetime;
        //    _tel_check.tel_std_type = "tel_assign_case";
        //    _tel_check.tel_proc_no = _tel.tel_proc_no;

        //    if("3".Equals(_dispatch_status))
        //        fAPTelCheckDao.reAssignByTelProcNo(_tel_check, conn, transaction);
        //    else
        //        fAPTelCheckDao.updDispatchStatusByTelProcNo(_tel_check, conn, transaction);
        //    //電訪處理結果="寄信"時，處理地址
        //    //1.異動【FAP_HOUSEHOLD_ADDR 戶政查詢地址檔】
        //    //2.回寫地址到AS400
        //    if ("3".Equals(_tel.tel_result) & !"".Equals(_paid_id))
        //        _tel_addr = procAddr(_paid_id, _paid_name, _tel, now, conn, transaction);

        //    return _tel_addr;

        //}


        private void writePiaLog(int affectRows, string piaOwner, string executionType)
        {
            PIA_LOG_MAIN piaLogMain = new PIA_LOG_MAIN();
            piaLogMain.TRACKING_TYPE = "A";
            piaLogMain.ACCESS_ACCOUNT = Session["UserID"].ToString();
            piaLogMain.ACCOUNT_NAME = Session["UserName"].ToString();
            piaLogMain.PROGFUN_NAME = "OAP0046AController";
            piaLogMain.EXECUTION_CONTENT = MaskUtil.maskId(piaOwner);
            piaLogMain.AFFECT_ROWS = affectRows;
            piaLogMain.PIA_TYPE = "0100100000";
            piaLogMain.EXECUTION_TYPE = executionType;
            piaLogMain.ACCESSOBJ_NAME = "FAP_TEL_INTERVIEW";
            piaLogMain.PIA_OWNER1 = MaskUtil.maskId(piaOwner);
            PiaLogMainDao piaLogMainDao = new PiaLogMainDao();
            piaLogMainDao.Insert(piaLogMain);

        }


        internal class summaryModel
        {
            public string aply_no { get; set; }
            public string tel_proc_no { get; set; }
            public string paid_name { get; set; }
            public string update_id { get; set; }
            public string update_name { get; set; }
            public string update_datetime { get; set; }

            public summaryModel()
            {
                aply_no = "";
                tel_proc_no = "";
                paid_name = "";
                update_id = "";
                update_name = "";
                update_datetime = "";
            }
        }


    }
}