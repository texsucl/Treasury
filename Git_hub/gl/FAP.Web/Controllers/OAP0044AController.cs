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
/// 功能說明：OAP0044A 重新派件覆核作業
/// 初版作者：20200914 Daiyu
/// 修改歷程：20200914 Daiyu
/// 需求單號：202008120153-00
/// 修改內容：初版
/// -------------------------------------------------
/// 修改歷程：20201216 Daiyu
/// 需求單號：202008120153-00
/// 修改內容：(淑美需求變更)
///           若派件狀態為0,也可以從此作業功能叫出,直接指定新派件人員
///           若派件日期有值,則派件狀態改為派件中
///           若派件日期無值則派件狀態仍為尚未派件
/// </summary>
///

namespace FAP.Web.Controllers
{
    [Authorize]
    [CheckSessionFilterAttribute]
    public class OAP0044AController : BaseController
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
                String[] roleInfo = authUtil.chkUserFuncAuth(Session["UserID"].ToString(), "~/OAP0044A/");
                if (roleInfo != null && roleInfo.Length == 2)
                {
                    opScope = roleInfo[0];
                    funcName = roleInfo[1];
                }
            }

            ViewBag.opScope = opScope;
            ViewBag.funcName = funcName;

            SysCodeDao sysCodeDao = new SysCodeDao();

            //給付性質
            FPMCODEDao pPMCODEDao = new FPMCODEDao();
            //ViewBag.oPaidCdjqList = sysCodeDao.jqGridList("AP", "O_PAID_CD", false);
            ViewBag.oPaidCdjqList = pPMCODEDao.jqGridList("PAID_CDTXT", "AP", false);

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
                FAPTelCheckHisDao fAPTelCheckHisDao = new FAPTelCheckHisDao();
                List<OAP0044Model> dataList = fAPTelCheckHisDao.qryForOAP0044A().OrderBy(x => x.temp_id).ToList();

                dataList.Where(x => x.temp_id.Contains('@')).Select(x => { x.temp_id = x.temp_id.Replace('@', '*'); return x; }).ToList();

                List<OAP0044Model> rows = new List<OAP0044Model>();
                Dictionary<string, ADModel> empMap = new Dictionary<string, ADModel>();
                string usr_id = "";
                CommonUtil commonUtil = new CommonUtil();

                string _temp_id = "";
                string _paid_id = "";
                foreach (OAP0044Model d in dataList)
                {
                    if (!_temp_id.Equals(d.temp_id))
                    {
                        _temp_id = d.temp_id;
                        _paid_id = d.paid_id;

                        //取得新派件人員姓名
                        usr_id = d.tel_interview_id;

                        if (!"".Equals(usr_id))
                        {
                            if (!empMap.ContainsKey(usr_id))
                            {
                                ADModel adModel = new ADModel();
                                adModel = commonUtil.qryEmp(usr_id);
                                empMap.Add(usr_id, adModel);
                            }
                            d.tel_interview_name = empMap[usr_id].name == "" ? d.tel_interview_id : empMap[usr_id].name;
                        }
                        else
                            d.tel_interview_name = d.tel_interview_id;



                        //取得原派件人員姓名
                        usr_id = d.tel_interview_id_o;

                        if (!"".Equals(usr_id))
                        {
                            if (!empMap.ContainsKey(usr_id))
                            {
                                ADModel adModel = new ADModel();
                                adModel = commonUtil.qryEmp(usr_id);
                                empMap.Add(usr_id, adModel);
                            }
                            d.tel_interview_id_o = d.tel_interview_id_o + " " + (empMap[usr_id].name == "" ? d.tel_interview_id_o : empMap[usr_id].name);
                        }
                        else
                            d.tel_interview_id_o = d.tel_interview_id_o;



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



                        rows.Add(d);
                    }

                }


                writePiaLog(rows.Count, _paid_id, "Q", "");

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
        public JsonResult execSave(List<OAP0044Model> recData, List<OAP0044Model> rtnData)
        {
            string strConn = DbUtil.GetDBFglConnStr();
            using (SqlConnection conn = new SqlConnection(strConn))
            {
                conn.Open();

                SqlTransaction transaction = conn.BeginTransaction("Transaction");

                try
                {
                    DateTime now = DateTime.Now;
                    FAPTelCheckDao fAPTelCheckDao = new FAPTelCheckDao();
                    FAPTelCheckHisDao fAPTelCheckHisDao = new FAPTelCheckHisDao();

                    


                    //處理駁回資料
                    if (rtnData.Count > 0)
                    {
                        foreach (OAP0044Model temp in rtnData)
                        {
                            if (temp.update_id.Equals(Session["UserID"].ToString()))
                                return Json(new { success = false, err = "申請人與覆核人員不可相同!!" }, JsonRequestBehavior.AllowGet);

                            List<OAP0044Model> rows = fAPTelCheckDao.qryForOAP0044(temp.temp_id.Replace('*', '@'), "", "", "");

                            foreach (OAP0044Model d in rows.ToList())
                            {
                                d.update_datetime = null;

                                //異動【FAP_TEL_CHECK 電訪支票檔】資料狀態
                                FAP_TEL_CHECK _tel_check = new FAP_TEL_CHECK();
                                ObjectUtil.CopyPropertiesTo(d, _tel_check);
                                _tel_check.tel_std_type = "tel_assign_case";
                                _tel_check.update_id = Session["UserID"].ToString();
                                _tel_check.update_datetime = now;
                                _tel_check.data_status = "1";
                                fAPTelCheckDao.updDataStatus(_tel_check, conn, transaction);


                                //異動【FAP_TEL_CHECK_HIS 電訪及追踨記錄暫存檔】
                                string tel_interview_id_o = d.tel_interview_id;
                                FAP_TEL_CHECK_HIS his = new FAP_TEL_CHECK_HIS();
                                ObjectUtil.CopyPropertiesTo(d, his);
                                his.aply_no = temp.aply_no;
                                procFAPTelCheckHis(fAPTelCheckHisDao, his, "3", now, conn, transaction);
                            }
                        }
                    }



                    //處理核可資料
                    if (recData.Count > 0)
                    {
                        TelDispatchRptModel _his = fAPTelCheckHisDao.qryByAplyNo("tel_assign_case", recData[0].aply_no).FirstOrDefault();
                        string _dispatch_date = "";
                        if (_his != null)
                            _dispatch_date = _his.dispatch_date;

                        foreach (OAP0044Model temp in recData)
                        {
                            if (temp.update_id.Equals(Session["UserID"].ToString()))
                                return Json(new { success = false, err = "申請人與覆核人員不可相同!!" }, JsonRequestBehavior.AllowGet);

                            List<OAP0044Model> rows = fAPTelCheckDao.qryForOAP0044(temp.temp_id.Replace('*', '@'), "", "", "");

                            foreach (OAP0044Model d in rows.ToList())
                            {
                                d.update_datetime = null;
                                d.dispatch_date = null;

                                //異動【FAP_TEL_CHECK 電訪支票檔】資料狀態
                                FAP_TEL_CHECK _tel_check = new FAP_TEL_CHECK();
                                ObjectUtil.CopyPropertiesTo(d, _tel_check);
                                _tel_check.tel_interview_id = temp.tel_interview_id;
                                _tel_check.tel_std_type = "tel_assign_case";
                                _tel_check.data_status = "1";
                                _tel_check.update_id = Session["UserID"].ToString();
                                _tel_check.update_datetime = now;

                                if (!"".Equals(StringUtil.toString(_dispatch_date)))
                                    _tel_check.dispatch_status = "1";
                                else
                                    _tel_check.dispatch_status = "0";

                                _tel_check.dispatch_date = string.IsNullOrWhiteSpace(_his.dispatch_date) ? (DateTime?)null : DateTime.Parse(_his.dispatch_date);
                                fAPTelCheckDao.reAssignOAP0044A(_tel_check, conn, transaction);


                                //異動【FAP_TEL_CHECK_HIS 電訪及追踨記錄暫存檔】
                                string tel_interview_id_o = d.tel_interview_id;
                                FAP_TEL_CHECK_HIS his = new FAP_TEL_CHECK_HIS();
                                ObjectUtil.CopyPropertiesTo(d, his);
                                his.aply_no = temp.aply_no;
                                procFAPTelCheckHis(fAPTelCheckHisDao, his, "2", now, conn, transaction);
                            }

                            writePiaLog(1, temp.paid_id, "E", MaskUtil.maskId(StringUtil.toString(temp.paid_id)));
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



        //異動【FAP_TEL_CHECK_HIS 電訪及追踨記錄暫存檔】
        private void procFAPTelCheckHis(FAPTelCheckHisDao fAPTelCheckHisDao, FAP_TEL_CHECK_HIS his, string appr_stat, DateTime now, SqlConnection conn, SqlTransaction transaction) {

            his.tel_std_type = "tel_assign_case";
            his.appr_stat = appr_stat;
            his.appr_id = Session["UserID"].ToString();
            his.approve_datetime = now;

            fAPTelCheckHisDao.updateApprOAP0044A(his, conn, transaction);

        }




        private void writePiaLog(int affectRows, string piaOwner, string executionType, string content)
        {
            PIA_LOG_MAIN piaLogMain = new PIA_LOG_MAIN();
            piaLogMain.TRACKING_TYPE = "A";
            piaLogMain.ACCESS_ACCOUNT = Session["UserID"].ToString();
            piaLogMain.ACCOUNT_NAME = Session["UserName"].ToString();
            piaLogMain.PROGFUN_NAME = "OAP0044AController";
            piaLogMain.EXECUTION_CONTENT = content;
            piaLogMain.AFFECT_ROWS = affectRows;
            piaLogMain.PIA_TYPE = "0100100000";
            piaLogMain.EXECUTION_TYPE = executionType;
            piaLogMain.ACCESSOBJ_NAME = "FAP_TEL_CHECK";
            piaLogMain.PIA_OWNER1 = MaskUtil.maskId(piaOwner);
            PiaLogMainDao piaLogMainDao = new PiaLogMainDao();
            piaLogMainDao.Insert(piaLogMain);

        }

    }
}