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

/// <summary>
/// 功能說明：OAP0046 電訪處理結果登錄作業
/// 初版作者：20200907 Daiyu
/// 修改歷程：20200907 Daiyu
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
    public class OAP0046Controller : BaseController
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
                String[] roleInfo = authUtil.chkUserFuncAuth(Session["UserID"].ToString(), "~/OAP0046/");
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



        [HttpPost]
        public ActionResult Print(string paid_id, string paid_name, string tel_proc_no, string print_srce, string pgm_srce, List<OAP0046DModel> gridData
            , OAP0046Model model)
        {
            List<OAP0046DModel> rptList = new List<OAP0046DModel>();
            List<rptTelProcResultModel> rptProcList = new List<rptTelProcResultModel>();
            OAP0046Model mModel = new OAP0046Model();

            FAP_TEL_INTERVIEW_HIS _his_inProcess = new FAP_TEL_INTERVIEW_HIS();

            SysParaDao sysParaDao = new SysParaDao();
            List<SYS_PARA> paraList = sysParaDao.qryByGrpId("AP", "tel_result");

            string _tel_proc_no = "";
            string _paid_id = ""; 
            string _paid_name = "";
            string _memo = "";
            int proc_seq = 0;
            CommonUtil commonUtil = new CommonUtil();

            SysCodeDao sysCodeDao = new SysCodeDao();

            //電訪對象
            Dictionary<string, string> calledPersonMap = sysCodeDao.qryByTypeDic("AP", "called_person");

            //電訪處理結果
            Dictionary<string, string> telCallMap = sysCodeDao.qryByTypeDic("AP", "tel_call");


            if (gridData.Count == 0)
                return Json(new { success = false, err = "未勾選要列印的資料!!請重新確認!!" });
            

            if ("".Equals(StringUtil.toString(tel_proc_no)))
            {
                if ("table".Equals(print_srce)) //從資料庫帶資料
                {
                    FAPTelCheckHisDao fAPTelCheckHisDao = new FAPTelCheckHisDao();
                    List<OAP0046DModel> tempList = fAPTelCheckHisDao.qryByCheckNo(gridData[0].system, gridData[0].check_acct_short, gridData[0].check_no, "1", "1");
                    if (tempList.Count > 0)
                    {
                        _tel_proc_no = tempList[0].tel_proc_no;
                        FAPTelInterviewHisDao fAPTelInterviewHisDao = new FAPTelInterviewHisDao();
                        FAP_TEL_INTERVIEW_HIS his = fAPTelInterviewHisDao.qryByTelProcNo(_tel_proc_no, "1", "1");
                        ObjectUtil.CopyPropertiesTo(his, mModel);

                        rptList = fAPTelCheckHisDao.qryByTelProcNo(_tel_proc_no, "1", "1");
                        tel_proc_no = _tel_proc_no;
                    }
                }
                else {  //帶畫面的資料
                    FAPTelCheckDao fAPTelCheckDao = new FAPTelCheckDao();

                    

                    ObjectUtil.CopyPropertiesTo(model, mModel);

                    //modify by daiyu 20210226 修改共票時，資料誤帶問題
                    //foreach (OAP0046DModel d in gridData) {
                    foreach (OAP0046DModel d in gridData.GroupBy(o => new { o.check_no })
              .Select(group => new OAP0046DModel
              {
                  check_no = group.Key.check_no
              }).ToList<OAP0046DModel>()
                    ) {

                        List<OAP0046DModel> tmpList = fAPTelCheckDao.qryForOAP0046("", "", "", "", "", d.check_no, "0046");
                        if (tmpList != null) {
                            foreach (OAP0046DModel tmp in tmpList) {
                                tmp.tel_result = model.tel_result;
                                rptList.Add(tmp);
                            }
                            
                        }
                    }
                }
            }
            else {
                FAPTelCheckDao fAPTelCheckDao = new FAPTelCheckDao();
                rptList = fAPTelCheckDao.qryForTelProcRpt(tel_proc_no);

                if ("0047".Equals(pgm_srce))
                    ObjectUtil.CopyPropertiesTo(model, mModel);
                else {
                    FAPTelInterviewDao fAPTelInterviewDao = new FAPTelInterviewDao();
                    FAP_TEL_INTERVIEW d = fAPTelInterviewDao.qryByTelProcNo(tel_proc_no);
                    ObjectUtil.CopyPropertiesTo(d, mModel);
                }


                
            }



            if (!"".Equals(tel_proc_no) || ("".Equals(tel_proc_no) & "table".Equals(print_srce))) {
                //查詢出歷次電訪紀錄
                FAPTelInterviewHisDao fAPTelInterviewHisDao = new FAPTelInterviewHisDao();
                List<FAP_TEL_INTERVIEW_HIS> procList = fAPTelInterviewHisDao.qryByTelProcNo(tel_proc_no, new string[] { "1", "2" }, new string[] { "1", "2" });


                _his_inProcess = procList.Where(x => x.appr_stat == "1").FirstOrDefault();

                foreach (FAP_TEL_INTERVIEW_HIS _proc_his in procList.OrderBy(x => x.update_datetime))
                {
                    proc_seq++;

                    //組合處理結果說明
                    OAP0046Model tmp = new OAP0046Model();
                    ObjectUtil.CopyPropertiesTo(_proc_his, tmp);
                    string _tmp_memo = getMemo(tmp, paraList);

                    rptTelProcResultModel _rptD = new rptTelProcResultModel();
                    ObjectUtil.CopyPropertiesTo(_proc_his, _rptD);

                    _rptD.tel_interview_datetime = DateUtil.ADDateToChtDate(_rptD.tel_interview_datetime, 3, "");
                    _rptD.memo = _tmp_memo;
                    _rptD.seq = proc_seq;

                    if (calledPersonMap.ContainsKey(_proc_his.called_person))
                        _rptD.called_person = calledPersonMap[_proc_his.called_person];

                    if (telCallMap.ContainsKey(_proc_his.tel_result))
                        _rptD.tel_result = telCallMap[_proc_his.tel_result];

                    ADModel adModel = new ADModel();
                    adModel = commonUtil.qryEmp(_proc_his.update_id);
                    if (!"".Equals(StringUtil.toString(adModel.name)))
                        _rptD.update_id = _rptD.update_id + " " + adModel.name;

                    rptProcList.Add(_rptD);
                }

            }


            if (rptList.Count == 0)
                return Json(new { success = false, err = "查無可列印的資料!!請重新確認!!" });
            else {
                _paid_id = rptList[0].paid_id;
                _paid_name = rptList[0].paid_name;
                mModel.tel_interview_id = rptList[0].tel_interview_id;
            }



            //若畫面上有登打處理日期，才要將畫面上的資料印出來
            string _data_type = "";
            if (_his_inProcess != null)
            {
                _data_type = StringUtil.toString(_his_inProcess.data_type);
                if(!"".Equals(_data_type))
                    ObjectUtil.CopyPropertiesTo(_his_inProcess, mModel);
                
            }
            //if ("".Equals(StringUtil.toString(model.tel_interview_datetime)) & _his_inProcess != null) {
            //    ObjectUtil.CopyPropertiesTo(_his_inProcess, mModel);
            //    _data_type = _his_inProcess.data_type;
            //}


            //若沒有覆核中的資料，且畫面上有登打處理日期，才要將畫面上的資料印出來
            if ("".Equals(_data_type) & !"".Equals(StringUtil.toString(model.tel_interview_datetime))) {
                proc_seq++;
                string _tmp_memo = getMemo(mModel, paraList);

                rptTelProcResultModel _rptD = new rptTelProcResultModel();
                ObjectUtil.CopyPropertiesTo(mModel, _rptD);

                if ("0047".Equals(pgm_srce))
                    _rptD.data_type = "2";
                else
                    _rptD.data_type = "1";

                _rptD.memo = _tmp_memo;
                _rptD.seq = proc_seq;

                _rptD.update_id = Session["UserID"].ToString() + " " + Session["UserName"].ToString();


                if (calledPersonMap.ContainsKey(StringUtil.toString(mModel.called_person)))
                    _rptD.called_person = calledPersonMap[mModel.called_person];

                if (telCallMap.ContainsKey(StringUtil.toString(mModel.tel_result)))
                    _rptD.tel_result = telCallMap[mModel.tel_result];

                rptProcList.Add(_rptD);
            }



            DataTable dtMain = commonUtil.ConvertToDataTable<rptTelProcResultModel>(rptProcList);
            DataTable dtDetail = commonUtil.ConvertToDataTable<OAP0046DModel>(rptList);


            var ReportViewer1 = new ReportViewer();
            //清除資料來源
            ReportViewer1.LocalReport.DataSources.Clear();
            //指定報表檔路徑   
            ReportViewer1.LocalReport.ReportPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory + "Report\\Rdlc\\TelProcResult.rdlc");
            //設定資料來源
            ReportViewer1.LocalReport.DataSources.Add(new ReportDataSource("DataSet1", dtMain));
            ReportViewer1.LocalReport.DataSources.Add(new ReportDataSource("DataSet2", dtDetail));


            //報表參數
            ReportViewer1.LocalReport.SetParameters(new ReportParameter("Title", "逾期未兌領支票處理結果"));
            ReportViewer1.LocalReport.SetParameters(new ReportParameter("UserId", Session["UserID"].ToString() + Session["UserName"].ToString()));
            ReportViewer1.LocalReport.SetParameters(new ReportParameter("check_cnt", rptList.GroupBy(o => new { o.check_acct_short, o.check_no }).Count().ToString()));

            ReportViewer1.LocalReport.SetParameters(new ReportParameter("paid_id", _paid_id));
            ReportViewer1.LocalReport.SetParameters(new ReportParameter("paid_name", _paid_name));

            ReportViewer1.LocalReport.SetParameters(new ReportParameter("tel_interview_f_datetime", mModel.tel_interview_f_datetime));

            if ("0046".Equals(pgm_srce))
            {
                ReportViewer1.LocalReport.SetParameters(new ReportParameter("tel_interview_f_datetime", mModel.tel_interview_datetime));
                ReportViewer1.LocalReport.SetParameters(new ReportParameter("tel_interview_datetime", ""));
            }
            else {
                ReportViewer1.LocalReport.SetParameters(new ReportParameter("tel_interview_f_datetime", mModel.tel_interview_f_datetime));
                ReportViewer1.LocalReport.SetParameters(new ReportParameter("tel_interview_datetime", "追蹤處理日期：" + mModel.tel_interview_datetime));
            }

            ReportViewer1.LocalReport.SetParameters(new ReportParameter("tel_proc_no", mModel.tel_proc_no));


            //ReportViewer1.LocalReport.SetParameters(new ReportParameter("record_no", mModel.record_no));


            //if(calledPersonMap.ContainsKey(mModel.called_person))
            //    ReportViewer1.LocalReport.SetParameters(new ReportParameter("called_person", calledPersonMap[mModel.called_person]));
            //else
            //    ReportViewer1.LocalReport.SetParameters(new ReportParameter("called_person", mModel.called_person));

            //ReportViewer1.LocalReport.SetParameters(new ReportParameter("tel_interview_id", mModel.tel_interview_id));

            
            //ReportViewer1.LocalReport.SetParameters(new ReportParameter("tel_interview_name", StringUtil.toString(adModel.name)));

            //if(telCallMap.ContainsKey(mModel.tel_result))
            //    ReportViewer1.LocalReport.SetParameters(new ReportParameter("tel_result", telCallMap[mModel.tel_result]));
            //else
            //    ReportViewer1.LocalReport.SetParameters(new ReportParameter("tel_result", mModel.tel_result));

            //ReportViewer1.LocalReport.SetParameters(new ReportParameter("cust_tel", mModel.cust_tel));


            //ReportViewer1.LocalReport.SetParameters(new ReportParameter("memo", _memo));

            String guid = Guid.NewGuid().ToString();


            ReportViewer1.LocalReport.Refresh();

            Microsoft.Reporting.WebForms.Warning[] tWarnings;
            string[] tStreamids;
            string tMimeType;
            string tEncoding;
            string tExtension;
            byte[] tBytes = ReportViewer1.LocalReport.Render("pdf", null, out tMimeType, out tEncoding, out tExtension, out tStreamids, out tWarnings);
            string fileName = "OAP0046P_" + guid + ".pdf";
            using (FileStream fs = new FileStream(Path.Combine(AppDomain.CurrentDomain.BaseDirectory + "Temp\\") + fileName, FileMode.Create))
            {
                fs.Write(tBytes, 0, tBytes.Length);
            }

            writePiaLog(gridData.Count, paid_id, "P");


            var jsonData = new { success = true, guid = guid };
            return Json(jsonData, JsonRequestBehavior.AllowGet);
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
                
                    
                    
                //查詢電訪紀錄檔
                FAPTelCheckDao fAPTelCheckDao = new FAPTelCheckDao();
                List<OAP0046DModel> dataList = fAPTelCheckDao.qryForOAP0046(model.paid_id
                    , model.policy_no, model.policy_seq, model.id_dup, model.paid_name
                    , model.check_no, "0046");

                Dictionary<string, ADModel> userNameMap = new Dictionary<string, ADModel>();
                string usr_id = "";
                CommonUtil commonUtil = new CommonUtil();

                foreach (OAP0046DModel d in dataList)
                {

                    //取得電訪人員姓名
                    usr_id = StringUtil.toString(d.tel_interview_id);
                    if (!userNameMap.ContainsKey(usr_id))
                    {
                        if (!"".Equals(usr_id))
                        {
                            ADModel adModel = new ADModel();
                            adModel = commonUtil.qryEmp(usr_id);
                            userNameMap.Add(usr_id, adModel);
                        }

                    }

                    d.tel_interview_id = d.tel_interview_id + " " + StringUtil.toString(userNameMap[usr_id].name);
                }


                //寫稽核軌跡
                if (!"".Equals(StringUtil.toString(model.paid_id))) 
                    writePiaLog(dataList.Count, model.paid_id, "Q");


                string _paid_name = "";
                if (dataList.Count > 0) {
                    model.paid_id = StringUtil.toString(dataList[0].paid_id);
                    _paid_name = StringUtil.toString(dataList[0].paid_name);
                }
                    


                return Json(new { success = true, dataList = dataList, paid_id = model.paid_id, paid_name = _paid_name });
            }
            catch (Exception e)
            {
                logger.Info(e.ToString());
                return Json(new { success = false, err = "其它錯誤，請洽系統管理員!!" });
            }

        }



        [HttpPost]
        public JsonResult execSave(OAP0046Model model, List<OAP0046DModel> gridData)
        {
            if (gridData.Count == 0) 
                return Json(new { success = false, err = "未選取要作業的資料!!" });
            

            foreach (OAP0046DModel d in gridData) {
                if(!StringUtil.toString(d.tel_interview_id).StartsWith(Session["UserID"].ToString()))
                    return Json(new { success = false, err = "勾選的資料，含非指定的電訪人員，不可執行本作業!!" });

                if (!"".Equals(StringUtil.toString(d.tel_proc_no)))
                    return Json(new { success = false, err = "勾選的資料，含已登打過的資料，不可執行本作業!!" });
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

                    try
                    {
                        if(!"".Equals(StringUtil.toString(model.counter_date)))
                            model.counter_date = DateUtil.As400ChtDateToADDate(model.counter_date);
                    }
                    catch (Exception e) {
                        return Json(new { success = false, err = "臨櫃日期輸入錯誤!!" });
                    }
                    
                    FAPTelInterviewHisDao fAPTelInterviewHisDao = new FAPTelInterviewHisDao();
                    FAPTelProcDao fAPTelProcDao = new FAPTelProcDao();
                    FAPTelCheckHisDao fAPTelCheckHisDao = new FAPTelCheckHisDao();
                    FAPTelCheckDao fAPTelCheckDao = new FAPTelCheckDao();

                    //取得覆核單號
                    SysSeqDao sysSeqDao = new SysSeqDao();
                    string qPreCode = "0046" + curDateTime[0].Substring(0, 5);
                    var cId = sysSeqDao.qrySeqNo("AP", "0046", qPreCode).ToString();
                    string _aply_no = qPreCode + cId.ToString().PadLeft(3, '0');


                    //取得電訪編號
                    string qTelPreCode = curDateTime[0].Substring(0, 5);
                    string _paid_id = model.paid_id;
                    if ("".Equals(StringUtil.toString(_paid_id)))
                        qTelPreCode += "X";
                    else
                    {
                        if (_paid_id.Length < 5)    //modify by daiyu 20210305
                            _paid_id = _paid_id.PadRight(5, '0');

                        qTelPreCode += _paid_id.Substring(_paid_id.Length - 5);

                    }


                    var cTelId = sysSeqDao.qrySeqNo("AP", "0046", qTelPreCode).ToString();
                    int seqTelLen = 12 - (qTelPreCode).Length;
                    string _tel_proc_no = qTelPreCode + cTelId.ToString().PadLeft(seqTelLen, '0');


                    string _check_acct_short = "";
                    string _check_no = "";

                    foreach (OAP0046DModel d in gridData.OrderBy(x => x.check_acct_short).ThenBy(x => x.check_no))
                    {
                        if (_check_acct_short.Equals(d.check_acct_short) & _check_no.Equals(d.check_no)) {
                            continue;
                        }

                        _check_acct_short = d.check_acct_short;
                        _check_no = d.check_no;

                        //新增【FAP_TEL_CHECK_HIS 電訪支票暫存檔】
                        FAP_TEL_CHECK_HIS _check_his = new FAP_TEL_CHECK_HIS();
                        ObjectUtil.CopyPropertiesTo(d, _check_his);
                        _check_his.aply_no = _aply_no;
                        _check_his.tel_proc_no = _tel_proc_no;
                        _check_his.tel_std_type = "tel_assign_case";
                        _check_his.tel_interview_id = Session["UserID"].ToString();
                        _check_his.update_id = Session["UserID"].ToString();
                        _check_his.update_datetime = now;
                        _check_his.appr_stat = "1";
                        fAPTelCheckHisDao.insertFromFormal(now, _check_his, Session["UserID"].ToString(), "0046", conn, transaction);

                        //異動【FAP_TEL_CHECK 電訪支票檔】資料狀態
                        FAP_TEL_CHECK _tel_check = new FAP_TEL_CHECK();
                        ObjectUtil.CopyPropertiesTo(d, _tel_check);
                        _tel_check.tel_proc_no = "";
                        _tel_check.tel_std_type = "tel_assign_case";
                        _tel_check.update_id = Session["UserID"].ToString();
                        _tel_check.update_datetime = now;
                        _tel_check.data_status = "2";
                        fAPTelCheckDao.updDataStatus(_tel_check, conn, transaction);


                    }

                    //新增【FAP_TEL_INTERVIEW_HIS 電訪及追踨記錄暫存檔】
                    FAP_TEL_INTERVIEW_HIS interview_his = new FAP_TEL_INTERVIEW_HIS();
                    model.tel_interview_f_datetime = model.tel_interview_datetime;
                    ObjectUtil.CopyPropertiesTo(model, interview_his);
                    interview_his.aply_no = _aply_no;
                    interview_his.tel_proc_no = _tel_proc_no;
                    interview_his.data_type = "1";
                    interview_his.tel_interview_id = Session["UserID"].ToString();
                    //interview_his.tel_interview_f_datetime = interview_his.tel_interview_datetime;
                    //interview_his.tel_interview_datetime = now;
                    interview_his.tel_result_cnt = 1;
                    interview_his.dispatch_status = "1";
                    interview_his.exec_action = "A";
                    interview_his.update_datetime = now;
                    interview_his.update_id = Session["UserID"].ToString();
                    interview_his.appr_stat = "1";
                    fAPTelInterviewHisDao.insert(interview_his, conn, transaction);

                    //新增【FAP_TEL_PROC 電訪及追踨記錄歷程檔】
                    FAP_TEL_PROC _tel_proc = new FAP_TEL_PROC();
                    _tel_proc.tel_proc_no = _tel_proc_no;
                    _tel_proc.aply_no = _aply_no;
                    _tel_proc.data_type = "1";
                    _tel_proc.proc_id = Session["UserID"].ToString();
                    _tel_proc.proc_datetime = interview_his.tel_interview_datetime;
                    _tel_proc.proc_status = model.tel_result;
                    _tel_proc.reason = model.reason;
                    _tel_proc.appr_stat = "1";
                    fAPTelProcDao.insert(_tel_proc, conn, transaction);


                    transaction.Commit();

                    if (!"".Equals(StringUtil.toString(model.paid_id)))
                        writePiaLog(gridData.Count, model.paid_id, "E");

                    return Json(new { success = true, aply_no = _aply_no, tel_proc_no = _tel_proc_no , err = ""});
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

        private string getMemo(OAP0046Model mModel, List<SYS_PARA> paraList) {
            string _memo = "";
            SYS_PARA para = paraList.Where(x => x.PARA_ID == mModel.tel_result).FirstOrDefault();
            switch (mModel.tel_result)
            {
                case "1":
                    string _counter_date = "";
                    if (!"".Equals(mModel.counter_date))
                    {
                        try
                        {
                            _counter_date = DateUtil.ADDateToChtDate(mModel.counter_date, 3, "/");
                        }
                        catch (Exception e)
                        {
                            _counter_date = StringUtil.toString(mModel.counter_date);
                        }
                    }


                    _memo = para.PARA_VALUE.Replace("@P1@", StringUtil.toString(mModel.cust_counter) == "" ? "________" : mModel.cust_counter)
                        .Replace("@P2@", StringUtil.toString(mModel.cust_counter) == "" ? "______/____/____" : _counter_date);
                    break;
                case "2":
                    _memo = para.PARA_VALUE.Replace("@P1@", StringUtil.toString(mModel.tel_addr) == "" ? "_____________________________" : (mModel.tel_zip_code + "　" + mModel.tel_addr))
                        .Replace("@P2@", StringUtil.toString(mModel.cust_tel) == "" ? "______________" : mModel.cust_tel);
                    break;
                case "3":
                    _memo = para.PARA_VALUE.Replace("@P1@", StringUtil.toString(mModel.tel_addr) == "" ? "_____________________________" : (mModel.tel_zip_code + "　" + mModel.tel_addr))
                        .Replace("@P2@", StringUtil.toString(mModel.cust_tel) == "" ? "______________" : mModel.cust_tel);
                    break;
                case "4":
                    _memo = para.PARA_VALUE.Replace("@P1@", StringUtil.toString(mModel.tel_mail) == "" ? "_____________________________" : (mModel.tel_mail));
                    break;
                case "5":
                    _memo = para.PARA_VALUE;
                    break;
                case "6":
                    _memo = para.PARA_VALUE;
                    break;
                //case "7":
                //    _memo = para.PARA_VALUE;
                //    break;
                case "8":
                    _memo = para.PARA_VALUE;
                    break;

            }
            SYS_PARA _para_other = paraList.Where(x => x.PARA_ID == " ").FirstOrDefault();
            _memo += "\n\n" + StringUtil.toString(_para_other.PARA_VALUE) + "\n\n" + mModel.reason;

            return _memo;
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


        internal class rptTelProcResultModel
        {
            public int seq { get; set; }
            public string tel_interview_datetime { get; set; }
            public string update_id { get; set; }
            public string tel_result { get; set; }
            public string record_no { get; set; }
            public string called_person { get; set; }
            public string cust_tel { get; set; }
            public string memo { get; set; }
            public string data_type { get; set; }


            public rptTelProcResultModel()
            {
                seq = 0;
                tel_interview_datetime = "";
                update_id = "";
                tel_result = "";
                record_no = "";
                called_person = "";
                cust_tel = "";
                memo = "";
                data_type = "";
            }
        }
    }
}