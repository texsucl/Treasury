using FRT.Web.ActionFilter;
using FRT.Web.AS400Models;
using FRT.Web.BO;
using FRT.Web.Daos;
using FRT.Web.Models;
using FRT.Web.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.EasycomClient;
using System.Data.SqlClient;
using System.Web.Mvc;


/// <summary>
/// 功能說明：AML檢核TABLE檔維護覆核作業
/// 初版作者：20191212 Daiyu
/// 修改歷程：20181212 Daiyu
///           需求單號：201912030811-01 AML相關需求-第一階段需求
///           初版
/// </summary>
///

namespace FRT.Web.Controllers
{
    [Authorize]
    [CheckSessionFilterAttribute]
    public class ORT0001AController : BaseController
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
                String[] roleInfo = authUtil.chkUserFuncAuth(Session["UserID"].ToString(), "~/ORT0001A/");
                if (roleInfo != null && roleInfo.Length == 2)
                {
                    opScope = roleInfo[0];
                    funcName = roleInfo[1];
                }
            }

            ViewBag.opScope = opScope;
            ViewBag.funcName = funcName;

            FPMCODEDao fPMCODEDao = new FPMCODEDao();
            //繳款原因
            ViewBag.rmResnjsList = fPMCODEDao.jqGridList("RM-RESN", "RT", "", true);

            //繳款類別
            ViewBag.rmTypejsList = fPMCODEDao.jqGridList("RM-TYPE", "RT", "", true);


            SysCodeDao sysCodeDao = new SysCodeDao();
            //給付方式
            ViewBag.remitKindjsList = sysCodeDao.jqGridList("RT", "REMIT_KIND", true);

            //傳入AML系統UNIT代碼
            ViewBag.amlUnitjsList = sysCodeDao.jqGridList("RT", "AML_UNIT", true);

            //執行功能
            ViewBag.statusjqList = sysCodeDao.jqGridList("RT", "STATUS", true);

            //資料狀態
            ViewBag.dataStatusjqList = sysCodeDao.jqGridList("RT", "DATA_STATUS", true);

            //Y、N(For洗錢態樣審查)
            ViewBag.ynjsList = sysCodeDao.jqGridList("SSO", "YN_FLAG", true);

            return View();
        }


        /// <summary>
        /// 查詢待覆核的資料
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public JsonResult LoadData()
        {
            FRTCodeHisDao fRTCodeHisDao = new FRTCodeHisDao();

            try {
                List<FrtCodeHisModel> rows = new List<FrtCodeHisModel>();

                rows = fRTCodeHisDao.qryForSTAT("RT", "6210_AML", "", "", "1");
                List<ORT0001Model> dataList = new List<ORT0001Model>();


                foreach (FrtCodeHisModel d in rows)
                {
                    try
                    {
                        ORT0001Model data = new ORT0001Model();
                        data.aply_No = d.aplyNo;
                        data.tempId = d.refNo;
                        data.rm_resn = d.refNoN.Substring(0, 1);
                        data.rm_type = d.refNoN.Substring(1, 1);
                        data.remit_kind = d.refNoN.Substring(2, 1);
                        data.aml_unit = StringUtil.toString(d.text.Substring(1));
                        data.call_aml = StringUtil.toString(d.text.Substring(0, 1));
                        data.status = d.status;
                        data.update_id = d.updId;
                        data.update_datetime = d.updDateTime;

                        dataList.Add(data);
                    }
                    catch (Exception e)
                    {

                    }
                }


                //查DB_INTRA取得異動人員、覆核人員姓名
                using (DB_INTRAEntities dbIntra = new DB_INTRAEntities())
                {
                    Dictionary<string, string> userNameMap = new Dictionary<string, string>();
                    OaEmpDao oaEmpDao = new OaEmpDao();
                    string update_id = "";

                    foreach (ORT0001Model d in dataList)
                    {
                        update_id = StringUtil.toString(d.update_id);
                        d.update_name = update_id;

                        if (!"".Equals(update_id))
                        {
                            if (!userNameMap.ContainsKey(update_id))
                            {
                                userNameMap = oaEmpDao.qryUsrName(userNameMap, update_id, dbIntra);
                            }
                            d.update_name = userNameMap[update_id];
                        }
                    }
                }

                var jsonData = new { success = true, rows = dataList };
                return Json(jsonData, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                logger.Error(e.ToString);
                var jsonData = new { success = false, err = e.ToString()};
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
        public JsonResult execSave(List<ORT0001Model> recData, List<ORT0001Model> rtnData)
        {

            string strConn = DbUtil.GetDBFglConnStr();
            List<ErrorModel> errList = new List<ErrorModel>();

            using (SqlConnection conn = new SqlConnection(strConn)) { 
                conn.Open();

                FRTCodeHisDao fRTCodeHisDao = new FRTCodeHisDao();

                //處理駁回資料
                if (rtnData.Count > 0)
                {
                    List<FrtCodeHisModel> rtnList = new List<FrtCodeHisModel>();

                    foreach (ORT0001Model d in rtnData) {
                        FrtCodeHisModel model = new FrtCodeHisModel();
                        model.aplyNo = d.aply_No;
                        model.refNo = d.tempId;
                        model.refNoN = d.rm_resn + d.rm_type + d.remit_kind;
                        model.text = d.call_aml + d.aml_unit;
                        model.updId = d.update_id;
                        model.updDateTime = d.update_datetime;
                        model.status = d.status;

                        rtnList.Add(model);
                    }


                    foreach (FrtCodeHisModel d in rtnList)
                    {
                        SqlTransaction transaction = conn.BeginTransaction("Transaction");
                        try
                        {
                            if (d.updId.Equals(Session["UserID"].ToString()))
                            {
                                ErrorModel errorModel = new ErrorModel();
                                errorModel.refNo = d.refNoN;
                                errorModel.errorMsg = "申請人與覆核人員不可相同!!";

                                errList.Add(errorModel);
                                transaction.Rollback();
                            }
                            else
                            {
                                d.srceFrom = "RT";
                                d.groupId = "6210_AML";
                                d.textLen = "8";
                                fRTCodeHisDao.updateFRTCodeHis(Session["UserID"].ToString(), "3", d, conn, transaction);
                                transaction.Commit();
                            }
                        }
                        catch (Exception e) {
                            ErrorModel errorModel = new ErrorModel();
                            errorModel.refNo = d.refNoN;
                            errorModel.errorMsg = StringUtil.toString(e.Message);

                            errList.Add(errorModel);
                            transaction.Rollback();
                        }
                    }
                }

                //處理核可資料
                if (recData.Count > 0)
                {
                    using (EacConnection conn400 = new EacConnection(CommonUtil.GetEasycomConn()))
                    {
                        conn400.Open();

                        FRTCODEDao fRTCODEDao = new FRTCODEDao();

                        List<FrtCodeHisModel> recList = new List<FrtCodeHisModel>();
                        foreach (ORT0001Model d in recData)
                        {
                            FrtCodeHisModel model = new FrtCodeHisModel();
                            model.aplyNo = d.aply_No;
                            model.refNo = d.tempId;
                            model.refNoN = d.rm_resn + d.rm_type + d.remit_kind;
                            model.text = d.call_aml + d.aml_unit;
                            model.updId = d.update_id;
                            model.updDateTime = d.update_datetime;
                            model.status = d.status;

                            recList.Add(model);
                        }

                        foreach (FrtCodeHisModel d in recList)
                        {
                            SqlTransaction transaction = conn.BeginTransaction("Transaction");
                            EacTransaction transaction400 = conn400.BeginTransaction();

                            try
                            {
                                if (d.updId.Equals(Session["UserID"].ToString()))
                                {
                                    ErrorModel errorModel = new ErrorModel();
                                    errorModel.refNo = d.refNoN;
                                    errorModel.errorMsg = "申請人與覆核人員不可相同!!";

                                    errList.Add(errorModel);
                                    transaction.Rollback();
                                    transaction400.Rollback();
                                }
                                else {
                                    d.srceFrom = "RT";
                                    d.groupId = "6210_AML";
                                    d.textLen = "8";
                                    d.apprId = Session["UserID"].ToString();
                                    d.apprvFlg = "Y";
                                    d.apprStat = "Y";

                                    fRTCodeHisDao.updateFRTCodeHis(Session["UserID"].ToString(), "2", d, conn, transaction);
                                    fRTCODEDao.apprFRTCODE0(Session["UserID"].ToString(), d, conn400, transaction400);

                                    transaction.Commit();
                                    transaction400.Commit();
                                }
                            }
                            catch (Exception e) {
                                ErrorModel errorModel = new ErrorModel();
                                errorModel.refNo = d.refNoN;
                                errorModel.errorMsg = StringUtil.toString(e.Message);

                                errList.Add(errorModel);
                                transaction.Rollback();
                                transaction400.Rollback();
                            }
                        }
                    }    
                }
            }


            if (errList.Count > 0)
            {
                return Json(new { success = false, err = errList }, JsonRequestBehavior.AllowGet);
            }
            else {
                return Json(new { success = true });
            }
            
        }


        /// <summary>
        /// 錯誤參數model
        /// </summary>
        public partial class ErrorModel
        {

            public string refNo { get; set; }

            public string errorMsg { get; set; }
        }
    }
}