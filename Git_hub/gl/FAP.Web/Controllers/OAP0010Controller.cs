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
/// 功能說明：清理歷程查詢
/// 初版作者：20190702 Daiyu
/// 修改歷程：20190702 Daiyu
/// 需求單號：
/// 修改內容：初版
/// -----------------------------------------
/// 修改歷程：20210128 daiyu
/// 需求單號：202101280283-00
/// 修改內容：尚未結案覆核的資料，不可列印結案報表
/// </summary>
///

namespace FAP.Web.Controllers
{
    [Authorize]
    [CheckSessionFilterAttribute]
    public class OAP0010Controller : BaseController
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
                String[] roleInfo = authUtil.chkUserFuncAuth(Session["UserID"].ToString(), "~/OAP0010/");
                if (roleInfo != null && roleInfo.Length == 2)
                {
                    opScope = roleInfo[0];
                    funcName = roleInfo[1];
                }
            }

            ViewBag.opScope = opScope;
            ViewBag.funcName = funcName;

            SysCodeDao sysCodeDao = new SysCodeDao();

            //清理狀態
            ViewBag.clrStatusList = sysCodeDao.loadSelectList("AP", "CLR_STATUS", true);
            ViewBag.clrStatusjqList = sysCodeDao.jqGridList("AP", "CLR_STATUS", false);

            //再給付方式
            ViewBag.rPaidTpjqList = sysCodeDao.jqGridList("AP", "R_PAID_TP", true);




            FAPVeCodeDao fAPVeCodeDao = new FAPVeCodeDao();
            //保局範圍
            ViewBag.fscRangejqList = fAPVeCodeDao.jqGridList("FSC_RANGE", false);

            //踐行程序
            ViewBag.practicejqList = fAPVeCodeDao.jqGridList("CLR_PRACTICE", false);

            //證明文件
            ViewBag.certDocjqList = fAPVeCodeDao.jqGridList("CLR_CERT_DOC", false);

            //清理大類
            ViewBag.level1jqList = fAPVeCodeDao.jqGridList("CLR_LEVEL1", false);

            //清理小類
            ViewBag.level2jqList = fAPVeCodeDao.jqGridList("CLR_LEVEL2", false);

            return View();
        }



        /// <summary>
        /// 查詢【FAP_VE_TRACE 逾期未兌領清理記錄檔】
        /// </summary>
        /// <param name="paid_id"></param>
        /// <param name="paid_name"></param>
        /// <param name="policy_no"></param>
        /// <param name="policy_seq"></param>
        /// <param name="id_dup"></param>
        /// <param name="check_no"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult qryProc(string paid_id, string paid_name, string policy_no, string policy_seq, string id_dup, string check_no, string status)
        {
            logger.Info("qryProc begin!!");
            try
            {
                List<OAP0010Model> rows = new List<OAP0010Model>();

                //查詢【FAP_VE_TRACE_PROC 逾期未兌領清理記錄歷程檔】
                FAPVeTraceDao fAPVeTraceDao = new FAPVeTraceDao();
                rows = fAPVeTraceDao.qryForOAP0010(paid_id, paid_name, policy_no, policy_seq, id_dup, check_no, status);

                if (!"".Equals(StringUtil.toString(paid_id)) && !"".Equals(StringUtil.toString(paid_name)))
                    writePiaLog(rows.Count, paid_id, "Q");

                 
                if (rows.Count > 0) {
                    paid_id = StringUtil.toString(rows[0].paid_id);
                    paid_name = StringUtil.toString(rows[0].paid_name);
                    using (DB_INTRAEntities dbIntra = new DB_INTRAEntities())
                    {
                        Dictionary<string, string> userNameMap = new Dictionary<string, string>();
                        OaEmpDao oaEmpDao = new OaEmpDao();

                        foreach (OAP0010Model d in rows)
                        {
                            string update_id = StringUtil.toString(d.update_id);

                            if (!"".Equals(update_id))
                            {
                                if (!userNameMap.ContainsKey(update_id))
                                    userNameMap = oaEmpDao.qryUsrName(userNameMap, update_id, dbIntra);

                                d.update_id = update_id + " " + userNameMap[update_id];
                            }
                        }
                    }
                }


                var jsonData = new { success = true, dataList = rows, paid_id = paid_id, paid_name = paid_name};
                return Json(jsonData, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e) {
                return Json(new { success = false, err = "其它錯誤，請洽系統管理員!!" });
            }
        }

        [HttpPost]
        public JsonResult qryTraceProc(string temp_id)
        {
            logger.Info("qryTraceProc begin!!");
            try
            {
                string[] qryKey = temp_id.Split('_');
                string check_no = qryKey[0];
                string check_acct_short = qryKey[1];

                List<OAP0010DModel> rows = new List<OAP0010DModel>();

                //查詢【FAP_VE_TRACE_PROC 逾期未兌領清理記錄歷程檔】
                FAPVeTrackProcDao fAPVeTrackProcDao = new FAPVeTrackProcDao();
                rows = fAPVeTrackProcDao.qryByCheckNo(check_no, check_acct_short);


                //查詢【FAP_VE_TRACE 逾期未兌領清理記錄檔】
                FAPVeTraceDao fAPVeTraceDao = new FAPVeTraceDao();
                FAP_VE_TRACE d = fAPVeTraceDao.qryByCheckNo(check_no, check_acct_short);

                if (!"".Equals(StringUtil.toString(d.practice_1)))
                {
                    OAP0010DModel model = new OAP0010DModel();
                    DbMvToSc(model, d, d.practice_1, d.cert_doc_1, Convert.ToDateTime(d.exec_date_1), d.proc_desc);
                    rows.Add(model);
                }

                if (!"".Equals(StringUtil.toString(d.practice_2)))
                {
                    OAP0010DModel model = new OAP0010DModel();
                    DbMvToSc(model, d, d.practice_2, d.cert_doc_2, Convert.ToDateTime(d.exec_date_2), d.proc_desc);
                    rows.Add(model);
                }

                if (!"".Equals(StringUtil.toString(d.practice_3)))
                {
                    OAP0010DModel model = new OAP0010DModel();
                    DbMvToSc(model, d, d.practice_3, d.cert_doc_3, Convert.ToDateTime(d.exec_date_3), d.proc_desc);
                    rows.Add(model);
                }

                if (!"".Equals(StringUtil.toString(d.practice_4)))
                {
                    OAP0010DModel model = new OAP0010DModel();
                    DbMvToSc(model, d, d.practice_4, d.cert_doc_4, Convert.ToDateTime(d.exec_date_4), d.proc_desc);
                    rows.Add(model);
                }

                if (!"".Equals(StringUtil.toString(d.practice_5)))
                {
                    OAP0010DModel model = new OAP0010DModel();
                    DbMvToSc(model, d, d.practice_5, d.cert_doc_5, Convert.ToDateTime(d.exec_date_5), d.proc_desc);
                    rows.Add(model);
                }



                int i = 0;

                if (rows.Count > 0)
                {
                    using (DB_INTRAEntities dbIntra = new DB_INTRAEntities())
                    {
                        Dictionary<string, string> userNameMap = new Dictionary<string, string>();
                        OaEmpDao oaEmpDao = new OaEmpDao();
                        foreach (OAP0010DModel model in rows.OrderBy(x => x.exec_date).ThenBy(x => x.update_datetime))
                        {
                            i++;
                            model.temp_id = i.ToString();

                            string update_id = StringUtil.toString(model.update_id);

                            if (!"".Equals(update_id))
                            {
                                if (!userNameMap.ContainsKey(update_id))
                                    userNameMap = oaEmpDao.qryUsrName(userNameMap, update_id, dbIntra);

                                model.update_id = update_id + " " + userNameMap[update_id];
                            }


                        }
                    }
                }


                var jsonData = new { success = true, rows = rows.OrderBy(x => Convert.ToDateTime(x.exec_date).AddYears(1911)).ThenBy(x => x.update_datetime).ThenBy(x => x.practice) };
                return Json(jsonData, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new { success = false, err = "其它錯誤，請洽系統管理員!!" });
            }
        }



        private OAP0010DModel DbMvToSc(OAP0010DModel model, FAP_VE_TRACE d, string practice, string cert_doc, DateTime exec_date, string proc_desc) {
            model.practice = practice;
            model.cert_doc = cert_doc;
            model.exec_date = (exec_date.Year - 1911) + "/" + exec_date.Month.ToString() + "/" + exec_date.Day.ToString();
            model.proc_desc = proc_desc;
            return model;
        }


        [HttpPost]
        public ActionResult Print(string paid_id, string paid_name, string level_1, string level_2, string closed_no)
        {
            List<OAP0011Model> gridData = new List<OAP0011Model>();
            FAPVeTraceDao fAPVeTraceDao = new FAPVeTraceDao();
            gridData = fAPVeTraceDao.qryByClosedNo(closed_no);



            FAPVeTracePoliDao fAPVeTracePoliDao = new FAPVeTracePoliDao();
            foreach (OAP0011Model d in gridData)
            {
                FAP_VE_TRACE_POLI poli = fAPVeTracePoliDao.qryForOAP0011(d);
                d.o_paid_cd = StringUtil.toString(poli.o_paid_cd);
            }

            string closed_date = "";
            if (gridData != null)
                closed_date = gridData[0].closed_date;

            //add by daiyu 200210128
            if("".Equals(closed_date))
                return Json(new { success = false, err = "此案件尚未結案覆核，如需列印結案報表，請至『OAP0011 清理結案申請作業』列印!!" });

            VeCleanUtil veCleanUtil = new VeCleanUtil();

            string guid = veCleanUtil.closedRpt(paid_id, paid_name, level_1, level_2, closed_no, closed_date, gridData
                , Session["UserID"].ToString(), Session["UserName"].ToString(), "", null);

            var jsonData = new { success = true, guid = guid };
            return Json(jsonData, JsonRequestBehavior.AllowGet);



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


    }
}