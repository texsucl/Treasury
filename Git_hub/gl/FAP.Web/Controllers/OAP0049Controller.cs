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
using System.Linq;
using System.Reflection.Emit;
using System.Web.Mvc;

/// <summary>
/// 功能說明：OAP0049 處理追踨及清理階段查詢功能
/// 初版作者：20200915 Daiyu
/// 修改歷程：20200915 Daiyu
/// 需求單號：202008120153-00
/// 修改內容：初版
/// </summary>
///

namespace FAP.Web.Controllers
{
    [Authorize]
    [CheckSessionFilterAttribute]
    public class OAP0049Controller : BaseController
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
                String[] roleInfo = authUtil.chkUserFuncAuth(Session["UserID"].ToString(), "~/OAP0049/");
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
        /// 依"給付對象ID"或"電訪編號"查詢相關電訪紀錄
        /// </summary>
        /// <param name="paid_id"></param>
        /// <param name="tel_proc_no"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult qryTelCheck(string paid_id, string tel_proc_no)
        {
            try
            {
                string content = "paid_id:" + paid_id + "|tel_proc_no:" + tel_proc_no;

                FAPTelCheckDao fAPTelCheckDao = new FAPTelCheckDao();
                List<OAP0049Model> rows = fAPTelCheckDao.qryOAP0049Summary("tel_assign_case", paid_id, tel_proc_no);

                List<OAP0049Model> dataList = rows.GroupBy(o => new { o.tel_proc_no }).Select(group => new OAP0049Model
                {
                    tel_proc_no = group.Key.tel_proc_no
                }).OrderBy(x => x.tel_proc_no).ToList<OAP0049Model>();

                writePiaLog(dataList.Count, paid_id, "Q", content);

                return Json(new { success = true, dataList = dataList });
            }
            catch (Exception e)
            {
                logger.Info(e.ToString());
                return Json(new { success = false, err = "其它錯誤，請洽系統管理員!!" });
            }
        }



        /// <summary>
        /// 查詢電訪編號的處理歷程
        /// </summary>
        /// <param name="tel_proc_no"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult qryProc(string tel_proc_no)
        {
            try
            {

                FAPTelCheckDao fAPTelCheckDao = new FAPTelCheckDao();
                List<OAP0049Model> rows = fAPTelCheckDao.qryOAP0049Summary("tel_assign_case", "", tel_proc_no)
                    .OrderBy(x => x.seq_no.ToString().Length).ThenBy(x => x.seq_no).ToList();

                List<OAP0049Model> dataList = new List<OAP0049Model>();

                CommonUtil commonUtil = new CommonUtil();
                Dictionary<string, ADModel> empMap = new Dictionary<string, ADModel>();

                SysCodeDao sysCodeDao = new SysCodeDao();
                //處理結果
                Dictionary<string, string> telResultMap = sysCodeDao.qryByTypeDic("AP", "tel_call");

                //清理階段
                Dictionary<string, string> cleanStatusMap = sysCodeDao.qryByTypeDic("AP", "tel_clean");

                //電訪覆核結果
                Dictionary<string, string> telApprCodeMap = sysCodeDao.qryByTypeDic("AP", "TEL_APPR_CODE");


                foreach (OAP0049Model d in rows) {
                    switch (d.data_type) {
                        case "1":
                            d.data_type_desc = "電訪";

                            if (telResultMap.ContainsKey(d.proc_status))
                                d.proc_status_desc = d.proc_status + "." + telResultMap[d.proc_status];
                            else
                                d.proc_status_desc = d.proc_status;

                            if (telApprCodeMap.ContainsKey(d.appr_status))
                                d.appr_status_desc = d.appr_status + "." + telApprCodeMap[d.appr_status];
                            else
                                d.appr_status_desc = d.appr_status;


                            break;
                        case "2":
                            d.data_type_desc = "追蹤";

                            if (telResultMap.ContainsKey(d.proc_status))
                                d.proc_status_desc = d.proc_status + "." + telResultMap[d.proc_status];
                            else
                                d.proc_status_desc = d.proc_status;

                            if (telApprCodeMap.ContainsKey(d.appr_status))
                                d.appr_status_desc = d.appr_status + "." + telApprCodeMap[d.appr_status];
                            else
                                d.appr_status_desc = d.appr_status;

                            break;
                        case "3":
                            d.data_type_desc = "清理";

                            if (cleanStatusMap.ContainsKey(d.proc_status))
                                d.proc_status_desc = d.proc_status + "." + cleanStatusMap[d.proc_status];
                            else
                                d.proc_status_desc = d.proc_status;

                            break;
                    }


                    //取得處理人員姓名
                    if (!empMap.ContainsKey(d.proc_id))
                    {
                        ADModel adModel = new ADModel();
                        adModel = commonUtil.qryEmp(d.proc_id);
                        empMap.Add(d.proc_id, adModel);
                    }
                    d.proc_name = StringUtil.toString(empMap[d.proc_id].name) == "" ? d.proc_id : StringUtil.toString(empMap[d.proc_id].name);


                    //取得覆核人員姓名
                    if (!empMap.ContainsKey(d.appr_id))
                    {
                        ADModel adModel = new ADModel();
                        adModel = commonUtil.qryEmp(d.appr_id);
                        empMap.Add(d.appr_id, adModel);
                    }
                    d.appr_name = StringUtil.toString(empMap[d.appr_id].name) == "" ? d.appr_id : StringUtil.toString(empMap[d.appr_id].name);


                    dataList.Add(d);
                }



                return Json(new { success = true, rows = dataList });
            }
            catch (Exception e)
            {
                logger.Info(e.ToString());
                return Json(new { success = false, err = "其它錯誤，請洽系統管理員!!" });
            }
        }



        [HttpPost]
        public JsonResult qryCheck(string tel_proc_no)
        {
            try
            {

                FAPTelCheckDao fAPTelCheckDao = new FAPTelCheckDao();
                List<OAP0049DModel> rows = fAPTelCheckDao.qryOAP0049CheckList("tel_assign_case", tel_proc_no);


                return Json(new { success = true, rows = rows });
            }
            catch (Exception e)
            {
                logger.Info(e.ToString());
                return Json(new { success = false, err = "其它錯誤，請洽系統管理員!!" });
            }
        }



        private void writePiaLog(int affectRows, string piaOwner, string executionType, string content)
        {
            PIA_LOG_MAIN piaLogMain = new PIA_LOG_MAIN();
            piaLogMain.TRACKING_TYPE = "A";
            piaLogMain.ACCESS_ACCOUNT = Session["UserID"].ToString();
            piaLogMain.ACCOUNT_NAME = Session["UserName"].ToString();
            piaLogMain.PROGFUN_NAME = "OAP0049Controller";
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