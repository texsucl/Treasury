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
/// 功能說明：OAP0047A 追蹤處理結果覆核作業
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
    public class OAP0047AController : BaseController
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
                String[] roleInfo = authUtil.chkUserFuncAuth(Session["UserID"].ToString(), "~/OAP0047A/");
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


        [HttpPost]
        public JsonResult qryAply(OAP0046Model model)
        {
            try
            {
                FAPTelInterviewHisDao fAPTelInterviewHisDao = new FAPTelInterviewHisDao();
                List<OAP0046Model> dataList = fAPTelInterviewHisDao.qryForOAP0047A("2", "1", ""
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


        ///// <summary>
        ///// 查詢待覆核的資料
        ///// </summary>
        ///// <returns></returns>
        //[HttpPost]
        //public JsonResult LoadData()
        //{
        //    try
        //    {
        //        FAPTelInterviewHisDao fAPTelInterviewHisDao = new FAPTelInterviewHisDao();
        //        List<OAP0046Model> dataList = fAPTelInterviewHisDao.qryForOAP0047A("2", "1", "");
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
            ViewBag.telCalljqList = sysCodeDao.jqGridList("AP", "tel_call", true);

            //電訪覆核結果
            ViewBag.telApprCodeList = sysCodeDao.loadSelectList("AP", "TEL_APPR_CODE", true);
            ViewBag.telApprCodejqList = sysCodeDao.jqGridList("AP", "TEL_APPR_CODE", true);

            //電訪對象
            ViewBag.calledPersonList = sysCodeDao.loadSelectList("AP", "called_person", false);

            //覆核狀態
            ViewBag.apprStatjqList = sysCodeDao.jqGridList("AP", "APPR_STAT", false);

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



            //查詢覆核單資訊
            FAPTelInterviewHisDao fAPTelInterviewHisDao = new FAPTelInterviewHisDao();
            List<OAP0046Model> dataList = fAPTelInterviewHisDao.qryForOAP0047A("2", "1", aply_no, "", "", "");

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



            //取得上次處理結果資訊
            FAPTelInterviewDao fAPTelInterviewDao = new FAPTelInterviewDao();
            FAP_TEL_INTERVIEW _interview = fAPTelInterviewDao.qryByTelProcNo(model.tel_proc_no);
            model.tel_result_o = _interview.tel_result;

            dt = Convert.ToDateTime(_interview.tel_interview_f_datetime);
            model.tel_interview_f_datetime = DateUtil.ADDateToChtDate(dt, 3, "");


            CommonUtil commonUtil = new CommonUtil();
            ADModel adModel = new ADModel();
            adModel = commonUtil.qryEmp(model.update_id);
            model.update_name = adModel.name;

            return View(model);

           
        }


       


        [HttpPost]
        public JsonResult detailRow(string aply_no, string tel_proc_no)
        {
            ViewBag.funcName = funcName;

            List<OAP0046DModel> checkList = new List<OAP0046DModel>();

            FAPTelCheckDao fAPTelCheckDao = new FAPTelCheckDao();
            checkList = fAPTelCheckDao.qryForTelProcRpt(tel_proc_no);


            //查詢電訪處理過程
            CommonUtil commonUtil = new CommonUtil();
            Dictionary<string, ADModel> empMap = new Dictionary<string, ADModel>();

            FAPTelInterviewHisDao fAPTelInterviewHisDao = new FAPTelInterviewHisDao();
            List<FAP_TEL_INTERVIEW_HIS> _proc_list = new List<FAP_TEL_INTERVIEW_HIS>();
            _proc_list = fAPTelInterviewHisDao.qryByTelProcNo(tel_proc_no, new string[] { "1", "2" }, new string[] { });

            List<OAP0047ProcModel> dataList = new List<OAP0047ProcModel>();
            foreach (FAP_TEL_INTERVIEW_HIS _proc in _proc_list)
            {
                OAP0047ProcModel prodModel = new OAP0047ProcModel();
                ObjectUtil.CopyPropertiesTo(_proc, prodModel);

                switch (_proc.data_type)
                {
                    case "1":
                        prodModel.data_type_desc = "第一次電訪結果";
                        break;
                    case "2":
                        prodModel.data_type_desc = "追踨結果登錄";
                        break;

                }

                //取得處理人員姓名
                if (!"".Equals(prodModel.update_id))
                {
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

            var jsonData = new { success = true, checkList = checkList, procList = dataList };
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

                FAPTelInterviewDao fAPTelInterviewDao = new FAPTelInterviewDao();
                FAPTelInterviewHisDao fAPTelInterviewHisDao = new FAPTelInterviewHisDao();
                FAPTelProcDao fAPTelProcDao = new FAPTelProcDao();
                FAPTelCheckHisDao fAPTelCheckHisDao = new FAPTelCheckHisDao();
                FAPTelCheckDao fAPTelCheckDao = new FAPTelCheckDao();

                DateTime now = DateTime.Now;

                FAP_TEL_INTERVIEW_HIS _his = fAPTelInterviewHisDao.qryByAplyNo(model.aply_no);
                if (_his != null)
                    if (!"1".Equals(_his.appr_stat))
                        return Json(new { success = false, err = "此案件已被處理!!" }, JsonRequestBehavior.AllowGet);


                try
                {
                    //新增【FAP_TEL_PROC 電訪及追踨記錄歷程檔】
                    FAP_TEL_INTERVIEW_HIS _his_o = fAPTelInterviewHisDao.qryByTelProcNo(model.tel_proc_no, "2", "1");
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
                        FAP_TEL_CHECK _tel_check = new FAP_TEL_CHECK();
                        _tel_check.tel_proc_no = model.tel_proc_no;
                        _tel_check.tel_std_type = "tel_assign_case";
                        _tel_check.update_id = Session["UserID"].ToString();
                        _tel_check.update_datetime = now;
                        _tel_check.data_status = "1";
                        fAPTelCheckDao.updDataStatusByTelProcNo(_tel_check, conn, transaction);


                        //異動【FAP_TEL_INTERVIEW 電訪及追踨記錄檔】資料狀態
                        FAP_TEL_INTERVIEW _tel_interview = new FAP_TEL_INTERVIEW();
                        _tel_interview.tel_proc_no = model.tel_proc_no;
                        _tel_interview.data_status = "1";
                        _tel_interview.update_datetime = now;
                        _tel_interview.update_id = Session["UserID"].ToString();
                        fAPTelInterviewDao.updDataStatus(_tel_interview, conn, transaction);

                        //異動【FAP_TEL_INTERVIEW_HIS 電訪及追踨記錄暫存檔】.覆核狀態
                        fAPTelInterviewHisDao.updateApprResult(Session["UserID"].ToString()
                            , appr_stat, model, "2", now, conn, transaction);
                    }



                    //處理核可資料
                    if ("2".Equals(StringUtil.toString(appr_stat)))
                    {

                        //異動【FAP_TEL_CHECK 電訪支票檔】.資料狀態
                        FAP_TEL_CHECK _tel_check = new FAP_TEL_CHECK();
                        _tel_check.tel_proc_no = model.tel_proc_no;
                        _tel_check.tel_std_type = "tel_assign_case";
                        _tel_check.update_id = Session["UserID"].ToString();
                        _tel_check.update_datetime = now;
                        _tel_check.data_status = "1";
                        fAPTelCheckDao.updDataStatusByTelProcNo(_tel_check, conn, transaction);

                        model.tel_interview_datetime = DateUtil.As400ChtDateToADDate(model.tel_interview_datetime);

                        try
                        {
                            if (!"".Equals(StringUtil.toString(model.counter_date)))
                                model.counter_date = DateUtil.As400ChtDateToADDate(model.counter_date);
                        }
                        catch (Exception e)
                        {
                            return Json(new { success = false, err = "臨櫃日期輸入錯誤!!" });
                        }


                        //異動【FAP_TEL_INTERVIEW_HIS 電訪及追踨記錄暫存檔】.覆核狀態
                        fAPTelInterviewHisDao.updateApprResult(Session["UserID"].ToString()
                            , appr_stat, model, "2", now, conn, transaction);


                        //異動【FAP_TEL_INTERVIEW 電訪及追踨記錄檔】
                        FAP_TEL_INTERVIEW _tel = new FAP_TEL_INTERVIEW();
                        ObjectUtil.CopyPropertiesTo(_his_o, _tel);

                        

                        //ObjectUtil.CopyPropertiesTo(model, _tel);

                        _tel.tel_result = model.tel_result;
                        _tel.tel_appr_result = model.tel_appr_result;
                        _tel.called_person = model.called_person;
                        _tel.tel_zip_code = model.tel_zip_code;
                        _tel.tel_addr = model.tel_addr;
                        _tel.cust_tel = model.cust_tel;
                        _tel.cust_counter = model.cust_counter;

                        if (!"".Equals(StringUtil.toString(model.counter_date)))
                            _tel.counter_date = Convert.ToDateTime(model.counter_date);
                        else
                            _tel.counter_date = null;


                        _tel.tel_mail = model.tel_mail;
                        _tel.level_1 = model.level_1;
                        _tel.level_2 = model.level_2;
                        _tel.reason = model.reason;

                        _tel.update_datetime = _his_o.update_datetime;
                        _tel.update_id = _his_o.update_id;
                        _tel.tel_appr_datetime = now;
                        _tel.data_status = "1";

                        VeTelUtil veTelUtil = new VeTelUtil();
                        FAPPPAWModel ppaw = veTelUtil.procTelInterview(Session["UserID"].ToString(), "U", _his_o.aply_no, _tel, model.paid_id, model.paid_name, now, conn, transaction);

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





        


        private void writePiaLog(int affectRows, string piaOwner, string executionType)
        {
            PIA_LOG_MAIN piaLogMain = new PIA_LOG_MAIN();
            piaLogMain.TRACKING_TYPE = "A";
            piaLogMain.ACCESS_ACCOUNT = Session["UserID"].ToString();
            piaLogMain.ACCOUNT_NAME = Session["UserName"].ToString();
            piaLogMain.PROGFUN_NAME = "OAP0047AController";
            piaLogMain.EXECUTION_CONTENT = MaskUtil.maskId(StringUtil.toString(piaOwner));
            piaLogMain.AFFECT_ROWS = affectRows;
            piaLogMain.PIA_TYPE = "0100100000";
            piaLogMain.EXECUTION_TYPE = executionType;
            piaLogMain.ACCESSOBJ_NAME = "FAP_TEL_INTERVIEW";
            piaLogMain.PIA_OWNER1 = MaskUtil.maskId(StringUtil.toString(piaOwner));
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