using FAP.Web;
using FAP.Web.ActionFilter;
using FAP.Web.BO;
using FAP.Web.Daos;
using FAP.Web.Models;
using FAP.Web.ViewModels;
using NPOI.HSSF.Record.Chart;
using System;
using System.Collections.Generic;
using System.Data.EasycomClient;
using System.Data.SqlClient;
using System.Linq;
using System.Web.Mvc;

/// <summary>
/// 功能說明：OAP0048A 清理階段完成覆核作業
/// 初版作者：20200914 Daiyu
/// 修改歷程：20200914 Daiyu
///           需求單號：
///           初版
/// </summary>
///

namespace FAP.Web.Controllers
{
    [Authorize]
    [CheckSessionFilterAttribute]
    public class OAP0048AController : BaseController
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
                String[] roleInfo = authUtil.chkUserFuncAuth(Session["UserID"].ToString(), "~/OAP0048A/");
                if (roleInfo != null && roleInfo.Length == 2)
                {
                    opScope = roleInfo[0];
                    funcName = roleInfo[1];
                }
            }

            ViewBag.opScope = opScope;
            ViewBag.funcName = funcName;

            SysCodeDao sysCodeDao = new SysCodeDao();

            //清理階段
            ViewBag.telCleanjqList = sysCodeDao.jqGridList("AP", "tel_clean", true);

            return View();
        }


        /// <summary>
        /// 查詢待覆核的資料
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public JsonResult LoadData()
        {
            try
            {
                FAPTelInterviewHisDao fAPTelInterviewHisDao = new FAPTelInterviewHisDao();
                List<OAP0048Model> dataList = fAPTelInterviewHisDao.qryForOAP0048A("3", "1", "");


                List<OAP0048Model> rows = new List<OAP0048Model>();
                string _tel_proc_no = "";
                foreach (OAP0048Model d in dataList)
                {
                    if (!_tel_proc_no.Equals(d.tel_proc_no))
                    {
                        rows.Add(d);
                        _tel_proc_no = d.tel_proc_no;
                    }
                }

                Dictionary<string, ADModel> empMap = new Dictionary<string, ADModel>();
                string usr_id = "";
                CommonUtil commonUtil = new CommonUtil();
                FGLCALEDao fGLCALEDao = new FGLCALEDao();

                foreach (OAP0048Model d in rows)
                {

                    //取得申請人姓名
                    usr_id = d.update_id;

                    if (!"".Equals(usr_id))
                    {
                        if (!empMap.ContainsKey(usr_id))
                        {
                            ADModel adModel = new ADModel();
                            adModel = commonUtil.qryEmp(usr_id);
                            empMap.Add(usr_id, adModel);
                        }
                        d.update_name = empMap[usr_id].name;
                    }
                    else
                        d.update_name = d.update_id;



                    //取得應完成日
                    try
                    {
                        //d.clean_date = DateUtil.ADDateToChtDate(d.clean_date, 3, "");
                        d.clean_shf_date = fGLCALEDao.GetSTDDate(DateUtil.ADDateToChtDate(d.clean_date, 3, ""), Convert.ToInt32(d.std_1));
                    }
                    catch (Exception e)
                    {

                    }

                    d.clean_f_date = DateUtil.ADDateToChtDate(d.clean_f_date, 3, "");

                }


                var jsonData = new { success = true, rows };
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
        /// 核可/退回
        /// </summary>
        /// <param name="recData"></param>
        /// <param name="rtnData"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult execSave(List<OAP0048Model> recData, List<OAP0048Model> rtnData)
        {
            string strConn = DbUtil.GetDBFglConnStr();
            using (SqlConnection conn = new SqlConnection(strConn))
            {
                conn.Open();

                SqlTransaction transaction = conn.BeginTransaction("Transaction");

                try
                {
                    DateTime now = DateTime.Now;
                    FAPTelInterviewDao fAPTelInterviewDao = new FAPTelInterviewDao();
                    FAPTelInterviewHisDao fAPTelInterviewHisDao = new FAPTelInterviewHisDao();
                    FAPTelProcDao fAPTelProcDao = new FAPTelProcDao();
                    FAPTelCheckDao fAPTelCheckDao = new FAPTelCheckDao();


                    //處理駁回資料
                    if (rtnData.Count > 0)
                    {
                        foreach (OAP0048Model d in rtnData)
                        {
                            if (d.update_id.Equals(Session["UserID"].ToString()))
                                return Json(new { success = false, err = "申請人與覆核人員不可相同!!" }, JsonRequestBehavior.AllowGet);

                            d.appr_id = Session["UserID"].ToString();

                            //新增【FAP_TEL_PROC 電訪及追踨記錄歷程檔】
                            FAP_TEL_INTERVIEW_HIS _his_o = fAPTelInterviewHisDao.qryByTelProcNo(d.tel_proc_no, "3", "1");
                            ProcFAPTelProc(d.tel_proc_no, "3", _his_o, fAPTelProcDao, now, conn, transaction);


                            //異動【FAP_TEL_CHECK 電訪支票檔】資料狀態
                            FAP_TEL_CHECK _tel_check = new FAP_TEL_CHECK();
                            ObjectUtil.CopyPropertiesTo(d, _tel_check);
                            _tel_check.tel_proc_no = d.tel_proc_no;
                            procFAPTelCheck(fAPTelCheckDao, _tel_check, now, conn, transaction);



                            //異動【FAP_TEL_INTERVIEW 電訪及追踨記錄檔】資料狀態
                            FAP_TEL_INTERVIEW _tel_interview = new FAP_TEL_INTERVIEW();
                            _tel_interview.tel_proc_no = d.tel_proc_no;
                            ProcFAPTelInterview("3", _his_o, fAPTelInterviewDao, _tel_interview, now, conn, transaction);


                            //異動【FAP_TEL_INTERVIEW_HIS 電訪及追踨記錄暫存檔】.覆核狀態
                            fAPTelInterviewHisDao.updateApprStatus(Session["UserID"].ToString()
                                , "3", d.aply_no, d.tel_proc_no, "3", now, conn, transaction);
                        }
                    }



                    //處理核可資料
                    if (recData.Count > 0)
                    {
                        
                        foreach (OAP0048Model d in recData)
                        {
                            VeTelUtil veTelUtil = new VeTelUtil();
                            veTelUtil.procCleanStageApprove(d, now, Session["UserID"].ToString(), conn, transaction);


                            //if (d.update_id.Equals(Session["UserID"].ToString()))
                            //    return Json(new { success = false, err = "申請人與覆核人員不可相同!!" }, JsonRequestBehavior.AllowGet);

                            //d.appr_id = Session["UserID"].ToString();

                            ////新增【FAP_TEL_PROC 電訪及追踨記錄歷程檔】
                            //FAP_TEL_INTERVIEW_HIS _his_o = fAPTelInterviewHisDao.qryByTelProcNo(d.tel_proc_no, "3", "1");
                            //ProcFAPTelProc(d.tel_proc_no, "2", _his_o, fAPTelProcDao, now, conn, transaction);


                            ////異動【FAP_TEL_CHECK 電訪支票檔】資料狀態
                            //FAP_TEL_CHECK _tel_check = new FAP_TEL_CHECK();
                            //ObjectUtil.CopyPropertiesTo(d, _tel_check);
                            //_tel_check.tel_proc_no = d.tel_proc_no;
                            //int upd_cnt = procFAPTelCheck(fAPTelCheckDao, _tel_check, now, conn, transaction);



                            ////異動【FAP_TEL_INTERVIEW 電訪及追踨記錄檔】資料狀態
                            //FAP_TEL_INTERVIEW _tel_interview = new FAP_TEL_INTERVIEW();
                            //_tel_interview.tel_proc_no = d.tel_proc_no;
                            //ProcFAPTelInterview("2", _his_o, fAPTelInterviewDao, _tel_interview, now, conn, transaction);


                            ////異動【FAP_TEL_INTERVIEW_HIS 電訪及追踨記錄暫存檔】.覆核狀態
                            //fAPTelInterviewHisDao.updateApprStatus(Session["UserID"].ToString()
                            //    , "2", d.aply_no, d.tel_proc_no, "3", now, conn, transaction);


                            ////新增【FAP_VE_TRACE_PROC 逾期未兌領清理記錄歷程檔】
                            //if (new string[] { "5", "8", "10" }.Contains(d.clean_status)) {
                            //    string practice = "";
                            //    string cert_doc = "";

                            //    switch (d.clean_status) {
                            //        case "5":
                            //            practice = "G16";
                            //            cert_doc = "F6";
                            //            break;
                            //        case "8":
                            //            practice = "G8";
                            //            cert_doc = "F4";
                            //            break;
                            //        case "10":
                            //            practice = "G4";
                            //            cert_doc = "F1";
                            //            break;
                            //    }

                            //    FAPVeTrackProcDao fAPVeTrackProcDao = new FAPVeTrackProcDao();
                            //    fAPVeTrackProcDao.insertForTelProc("tel_assign_case", d.tel_proc_no, practice, cert_doc, d.remark, now
                            //        , Session["UserID"].ToString(), now, conn, transaction);
                            //    FAPVeTraceDao faPVeTraceDao = new FAPVeTraceDao();
                            //    faPVeTraceDao.updateForTelCheck("tel_assign_case", d.tel_proc_no, d.remark, now, conn, transaction);

                            //}


                            //writePiaLog(1, d.paid_id, "E", d.tel_proc_no );
                        }
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


        



        //【FAP_TEL_CHECK 電訪支票檔】
        private int procFAPTelCheck(FAPTelCheckDao fAPTelCheckDao, FAP_TEL_CHECK _tel_check, DateTime now, SqlConnection conn, SqlTransaction transaction) {
            
            _tel_check.tel_std_type = "tel_assign_case";
            _tel_check.update_id = Session["UserID"].ToString();
            _tel_check.update_datetime = now;
            _tel_check.data_status = "1";
            return fAPTelCheckDao.updDataStatusByTelProcNo(_tel_check, conn, transaction);

        }


        //【FAP_TEL_INTERVIEW 電訪及追踨記錄檔】
        private void ProcFAPTelInterview(string appr_stat, FAP_TEL_INTERVIEW_HIS _his_o, FAPTelInterviewDao fAPTelInterviewDao
            , FAP_TEL_INTERVIEW _tel_interview, DateTime now, SqlConnection conn, SqlTransaction transaction) {
            _tel_interview.data_status = "1";
            _tel_interview.update_datetime = now;
            _tel_interview.update_id = Session["UserID"].ToString();

            if ("3".Equals(appr_stat))
                fAPTelInterviewDao.updDataStatus(_tel_interview, conn, transaction);
            else
            {
                _tel_interview.remark = _his_o.remark;
                _tel_interview.clean_status = _his_o.clean_status;
                _tel_interview.clean_date = now;
                _tel_interview.remark = _his_o.remark;


                //判斷下一個清理階段
                switch (_his_o.clean_status) {
                    case "1":   //1 檢核寄信記錄
                        _tel_interview.clean_status = "6";

                        if ("A8".Equals(_his_o.level_2))
                            _tel_interview.clean_status = "2";

                        if("B1".Equals(_his_o.level_2) || "B2".Equals(_his_o.level_2) || "B3".Equals(_his_o.level_2))
                            _tel_interview.clean_status = "3";

                        if ("B4".Equals(_his_o.level_2))
                            _tel_interview.clean_status = "4";

                        if ("A7".Equals(_his_o.level_2))
                            _tel_interview.clean_status = "5";

                        break;

                    case "2":   //2 金額小於5000元
                        _tel_interview.clean_status = "9";  //依淑美 2020.11.30 MAIL 改成順序為 1-> 2 -> 9 -> 10 -> 11 -> 12
                        break;

                    case "3":   //3 放棄領取
                        _tel_interview.clean_status = "11";
                        break;

                    case "4":   //4 準備法扣資料
                        _tel_interview.clean_status = "11";
                        break;

                    case "5":   //5 準備法人網站資料
                        _tel_interview.clean_status = "11";
                        break;

                    case "6":   //6 準備調閱資料
                        _tel_interview.clean_status = "7";
                        break;

                    case "7":   //7 用印送件
                        _tel_interview.clean_status = "8";
                        break;

                    case "8":   //8 調閱完成
                        _tel_interview.clean_status = "9";
                        break;

                    case "9":   //9 要保書地址是否相符
                        _tel_interview.clean_status = "10";
                        break;

                    case "10":   //10 再次寄信
                        _tel_interview.clean_status = "11";
                        break;

                    case "11":   //11 結案登錄
                        _tel_interview.clean_status = "12";
                        break;
                    case "12": // 主管覆核
                        _tel_interview.clean_date = _his_o.clean_date;
                        _tel_interview.clean_f_date = _his_o.clean_f_date;
                        break;
                    case "13": // 給付結案
                        _tel_interview.clean_date = _his_o.clean_date;
                        _tel_interview.clean_f_date = _his_o.clean_f_date;
                        break;
                }


                fAPTelInterviewDao.updForOAP0048A(_tel_interview, conn, transaction);
            }
        }


        //【FAP_TEL_PROC 電訪及追踨記錄歷程檔】
        private void ProcFAPTelProc(string tel_proc_no, string appr_stat, FAP_TEL_INTERVIEW_HIS _his_o, FAPTelProcDao fAPTelProcDao
            , DateTime now, SqlConnection conn, SqlTransaction transaction) {

            
            FAP_TEL_PROC _tel_proc = new FAP_TEL_PROC();
            ObjectUtil.CopyPropertiesTo(_his_o, _tel_proc);
            _tel_proc.proc_id = _his_o.update_id;
            _tel_proc.proc_datetime = _his_o.clean_f_date;
            _tel_proc.proc_status = _his_o.clean_status;
            _tel_proc.reason = _his_o.remark;
            _tel_proc.appr_status = "";
            _tel_proc.appr_stat = appr_stat;
            _tel_proc.appr_datetime = now;
            _tel_proc.appr_id = Session["UserID"].ToString();
            fAPTelProcDao.insert(_tel_proc, conn, transaction);

        }



        private void writePiaLog(int affectRows, string piaOwner, string executionType, string content)
        {
            PIA_LOG_MAIN piaLogMain = new PIA_LOG_MAIN();
            piaLogMain.TRACKING_TYPE = "A";
            piaLogMain.ACCESS_ACCOUNT = Session["UserID"].ToString();
            piaLogMain.ACCOUNT_NAME = Session["UserName"].ToString();
            piaLogMain.PROGFUN_NAME = "OAP0048AController";
            piaLogMain.EXECUTION_CONTENT = content;
            piaLogMain.AFFECT_ROWS = affectRows;
            piaLogMain.PIA_TYPE = "0100100000";
            piaLogMain.EXECUTION_TYPE = executionType;
            piaLogMain.ACCESSOBJ_NAME = "FAP_TEL_INTERVIEW";
            piaLogMain.PIA_OWNER1 = MaskUtil.maskId(piaOwner);
            PiaLogMainDao piaLogMainDao = new PiaLogMainDao();
            piaLogMainDao.Insert(piaLogMain);

        }

    }
}