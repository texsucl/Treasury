using ClosedXML.Excel;
using FAP.Web.ActionFilter;
using FAP.Web.BO;
using FAP.Web.Daos;
using FAP.Web.Models;
using FAP.Web.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.DirectoryServices;
using System.IO;
using System.Reflection.Emit;
using System.Web.Mvc;
using System.Linq;

/// <summary>
/// 功能說明：OAP0048 清理階段完成登錄作業
/// 初版作者：20200911 Daiyu
/// 修改歷程：20200911 Daiyu
/// 需求單號：202008120153-00
/// 修改內容：初版
/// </summary>
///

namespace FAP.Web.Controllers
{
    [Authorize]
    [CheckSessionFilterAttribute]
    public class OAP0048Controller : BaseController
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
                String[] roleInfo = authUtil.chkUserFuncAuth(Session["UserID"].ToString(), "~/OAP0048/");
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

            //資料狀態
            ViewBag.statusjqList = sysCodeDao.jqGridList("AP", "DATA_STATUS", true);

            //執行功能
            ViewBag.execActionjqList = sysCodeDao.jqGridList("AP", "EXEC_ACTION", true);

            return View();
        }



        
        [HttpPost]
        public JsonResult qryTelClean(OAP0048Model model)
        {
            string content = "";
            try
            {
                
                switch (model.qType)
                {
                    case "q_paid_id":
                        content += "q_paid_id >> paid_id:" + MaskUtil.maskId(model.paid_id);
                        model.policy_no = "";
                        model.policy_seq = "";
                        model.id_dup = "";
                        model.paid_name = "";
                        model.check_no = "";
                        model.proc_id = "";
                        break;
                    case "q_check_no":
                        content += "q_check_no >> check_no:" + model.check_no;
                        model.paid_id = "";
                        model.policy_no = "";
                        model.policy_seq = "";
                        model.id_dup = "";
                        model.paid_name = "";
                        model.proc_id = "";
                        break;
                    case "q_policy_no":
                        content = @"q_policy_no >> q_policy_no:"
                                    + MaskUtil.maskId(model.policy_no) + "-" + model.policy_seq + "-" + model.id_dup
                                   + " paid_name:" + MaskUtil.maskName(model.paid_name);

                        model.paid_id = "";
                        model.check_no = "";
                        model.proc_id = "";
                        break;
                    case "q_proc_id":
                        content += "q_proc_id >> proc_id:" + MaskUtil.maskId(model.proc_id);
                        model.paid_id = "";
                        model.policy_no = "";
                        model.policy_seq = "";
                        model.id_dup = "";
                        model.paid_name = "";
                        model.check_no = "";
                        break;
                }

                //查詢電訪紀錄檔，一個電訪編號以一張支票顯示
                FAPTelInterviewDao fAPTelInterviewDao = new FAPTelInterviewDao();
                List<OAP0048Model> rows = fAPTelInterviewDao.qryForOAP0048(model.paid_id
                    , model.policy_no, model.policy_seq, model.id_dup, model.paid_name
                    , model.check_no, model.proc_id);

                List<OAP0048Model> dataList = new List<OAP0048Model>();
                string _tel_proc_no = "";
                foreach (OAP0048Model d in rows)
                {
                    if (!_tel_proc_no.Equals(d.tel_proc_no))
                    {
                        dataList.Add(d);
                        _tel_proc_no = d.tel_proc_no;
                    }
                }

                Dictionary<string, ADModel> empMap = new Dictionary<string, ADModel>();
                string usr_id = "";
                CommonUtil commonUtil = new CommonUtil();
                FGLCALEDao fGLCALEDao = new FGLCALEDao();

                foreach (OAP0048Model d in dataList) {

                    //取得清理人員姓名
                    usr_id = d.proc_id;

                    if (!"".Equals(usr_id)) {
                        if (!empMap.ContainsKey(usr_id))
                        {
                            ADModel adModel = new ADModel();
                            adModel = commonUtil.qryEmp(usr_id);
                            empMap.Add(usr_id, adModel);
                        }
                        d.proc_name = empMap[usr_id].name;
                    } else
                        d.proc_name = d.proc_id;


                    //取得應完成日
                    try
                    {
                        //d.clean_date = DateUtil.ADDateToChtDate(d.clean_date, 3, "");
                        d.clean_shf_date = fGLCALEDao.GetSTDDate(DateUtil.ADDateToChtDate(d.clean_date, 3, ""), Convert.ToInt32(d.std_1));
                        
                    }
                    catch (Exception e) { 

                    
                    }
                }



                //寫稽核軌跡
                if (!"".Equals(StringUtil.toString(model.paid_id)))
                    writePiaLog(dataList.Count, model.paid_id, "Q", content);



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




        /// <summary>
        /// 畫面執行"申請覆核"
        /// </summary>
        /// <param name="gridData"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult execSave(List<OAP0048Model> gridData)
        {
            logger.Info("execSave begin");

            string errStr = "";
            try
            {
                bool bChg = false;
                List<FAP_TEL_INTERVIEW_HIS> dataList = new List<FAP_TEL_INTERVIEW_HIS>();
                FAPTelInterviewDao fAPTelInterviewDao = new FAPTelInterviewDao();
                FAPTelInterviewHisDao fAPTelInterviewHisDao = new FAPTelInterviewHisDao();
                FAPTelProcDao fAPTelProcDao = new FAPTelProcDao();
                FAPTelCheckDao fAPTelCheckDao = new FAPTelCheckDao();

                DateTime now = DateTime.Now;
                string[] curDateTime = BO.DateUtil.getCurChtDateTime(3).Split(' ');

                foreach (OAP0048Model d in gridData)
                {
                    if (!"".Equals(StringUtil.toString(d.exec_action)))
                    {
                        errModel errModel = chkAplyData(d.tel_proc_no, d.clean_f_date, d.proc_id);
                        if (errModel.chkResult)
                        {
                            bChg = true;
                            FAP_TEL_INTERVIEW interview = fAPTelInterviewDao.qryByTelProcNo(d.tel_proc_no);
                            if ("".Equals(interview.tel_proc_no))
                                errStr += "電訪編號：" + d.tel_proc_no + " 錯誤原因：資料不存在!!<br/>";
                            else {
                                FAP_TEL_INTERVIEW_HIS his = new FAP_TEL_INTERVIEW_HIS();
                                ObjectUtil.CopyPropertiesTo(interview, his);


                                his.clean_f_date = Convert.ToDateTime(DateUtil.As400ChtDateToADDate(d.clean_f_date));
                                his.remark = d.remark;
                                his.data_type = "3";
                                his.exec_action = "U";
                                his.appr_stat = "1";
                                his.update_id = Session["UserID"].ToString();
                                his.update_datetime = now;
                                dataList.Add(his);
                            }
                        }
                        else
                        {
                            errStr += "電訪編號：" + d.tel_proc_no + " 錯誤原因：" + errModel.msg + "<br/>";
                        }
                    }
                }



                if (bChg == false)
                {
                    if ("".Equals(errStr))
                        return Json(new { success = false, err = "沒有可以異動的資料，將不進行修改覆核作業!!" }, JsonRequestBehavior.AllowGet);
                    else
                        return Json(new { success = true, err = errStr });
                }


                /*------------------ DB處理   begin------------------*/
                

                //取得流水號
                SysSeqDao sysSeqDao = new SysSeqDao();
                String qPreCode = "0048" + curDateTime[0].Substring(0, 5);
                var cId = sysSeqDao.qrySeqNo("AP", "0048", qPreCode).ToString();
                string aply_no = qPreCode + cId.ToString().PadLeft(3, '0');

                string strConn = DbUtil.GetDBFglConnStr();
                using (SqlConnection conn = new SqlConnection(strConn))
                {
                    conn.Open();

                    SqlTransaction transaction = conn.BeginTransaction("Transaction");
                    try
                    {
                        VeTelUtil veTelUtil = new VeTelUtil();
                        foreach (FAP_TEL_INTERVIEW_HIS d in dataList)
                        {
                            
                            //寫入申請資料
                            veTelUtil.procCleanStageAply(aply_no, d, now, Session["UserID"].ToString(), conn, transaction);
                           

                            //寫入覆核資料(跳下一個清理階段)
                            OAP0048Model _OAP0048Model = fAPTelInterviewHisDao.qryForOAP0048A("3", "1", aply_no).FirstOrDefault();                            
                            veTelUtil.procCleanStageApprove(_OAP0048Model, now, Session["UserID"].ToString(), conn, transaction);

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
        /// 檢核申請的資料是否正確
        /// 1.是否有覆核中的資料
        /// 2.新增的資料是否已存在
        /// </summary>
        /// <param name="code_type"></param>
        /// <param name="exec_action"></param>
        /// <param name="code_id"></param>
        /// <returns></returns>
        private errModel chkAplyData(string tel_proc_no, string clean_f_date, string proc_id)
        {
            ValidateUtil validateUtil = new ValidateUtil();
            errModel errModel = new errModel();
            FAPTelInterviewHisDao fAPTelInterviewHisDao = new FAPTelInterviewHisDao();
            FAP_TEL_INTERVIEW_HIS his = fAPTelInterviewHisDao.qryByTelProcNo(tel_proc_no, "3", "1");

            if (!"".Equals(StringUtil.toString(his.tel_proc_no))) {
                errModel.chkResult = false;
                errModel.msg = "資料覆核中，不可異動此筆資料!!";
                return errModel;
            }

            if (!validateUtil.chkChtDate(clean_f_date))
                errModel.msg += " 「實際完成日期」格式錯誤";


            if (!"".Equals(StringUtil.toString(errModel.msg)))
            {
                errModel.chkResult = false;
            }
            else {
                errModel.chkResult = true;
            }

            return errModel;
        }



        /// <summary>
        /// 查詢使用者資料需存在AD
        /// </summary>
        /// <param name="usr_id"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult qryEmp(string usr_id)
        {
            try
            {
                ADModel adModel = new ADModel();
                CommonUtil commonUtil = new CommonUtil();
                adModel = commonUtil.qryEmp(usr_id);

                return Json(new { success = true, name = adModel.name, e_mail = adModel.e_mail }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                logger.Error(e.ToString());
                return Json(new { success = false, err = "其它錯誤，請洽系統管理員!!" }, JsonRequestBehavior.AllowGet);
            }
        }


        private void writePiaLog(int affectRows, string piaOwner, string executionType, string content)
        {
            PIA_LOG_MAIN piaLogMain = new PIA_LOG_MAIN();
            piaLogMain.TRACKING_TYPE = "A";
            piaLogMain.ACCESS_ACCOUNT = Session["UserID"].ToString();
            piaLogMain.ACCOUNT_NAME = Session["UserName"].ToString();
            piaLogMain.PROGFUN_NAME = "OAP0048Controller";
            piaLogMain.EXECUTION_CONTENT = content;
            piaLogMain.AFFECT_ROWS = affectRows;
            piaLogMain.PIA_TYPE = "0100100000";
            piaLogMain.EXECUTION_TYPE = executionType;
            piaLogMain.ACCESSOBJ_NAME = "FAP_TEL_INTERVIEW";
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