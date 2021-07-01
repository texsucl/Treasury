using FRT.Web.ActionFilter;
using FRT.Web.AS400Models;
using FRT.Web.BO;
using FRT.Web.Daos;
using FRT.Web.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.EasycomClient;
using System.Data.SqlClient;
using System.Web.Mvc;


/// <summary>
/// 功能說明：銀行碼基本資料訊息維護覆核作業
/// 初版作者：20181008 Daiyu
/// 修改歷程：20181008 Daiyu
///           需求單號：201808170384-00 雙系統銀行分行整併第二階段
///           初版
/// </summary>
///

namespace FRT.Web.Controllers
{
    [Authorize]
    [CheckSessionFilterAttribute]
    public class ORTB013AController : BaseController
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
                String[] roleInfo = authUtil.chkUserFuncAuth(Session["UserID"].ToString(), "~/ORTB013A/");
                if (roleInfo != null && roleInfo.Length == 2)
                {
                    opScope = roleInfo[0];
                    funcName = roleInfo[1];
                }
            }

            ViewBag.opScope = opScope;
            ViewBag.funcName = funcName;

            SysCodeDao sysCodeDao = new SysCodeDao();
            //FRTCODE
            var frtCodeList = sysCodeDao.loadSelectList("RT", "FRTCODE", true);
            ViewBag.frtCodeList = frtCodeList;
            
            //資料狀態
            ViewBag.statusjqList = sysCodeDao.jqGridList("RT", "STATUS", true);

            return View();
        }


        /// <summary>
        /// 查詢待覆核的資料
        /// </summary>
        /// <param name="groupId"></param>
        /// <param name="srceFrom"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult LoadData(string groupId, string srceFrom)
        {
            FRTCodeHisDao fRTCodeHisDao = new FRTCodeHisDao();

            try {
                List<FrtCodeHisModel> rows = new List<FrtCodeHisModel>();

                rows = fRTCodeHisDao.qryForSTAT(srceFrom, groupId, "", "", "1");

                foreach (FrtCodeHisModel d in rows)
                {
                    if ("BKMSG_OTH".Equals(groupId))
                    {
                        FDCBANKADao fDCBANKADao = new FDCBANKADao();
                        FDCBANKAModel fDCBANKAModel = new FDCBANKAModel();
                        fDCBANKAModel = fDCBANKADao.qryByBankNo(d.refNo == "" ? d.refNoN : d.refNo);

                        if (!string.IsNullOrEmpty(fDCBANKAModel.bankNo))
                            d.othText = fDCBANKAModel.bankName;
                    }

                }



                var jsonData = new { success = true, rows };
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
        public JsonResult execSave(string groupId, string srceFrom, string textLen, List<FrtCodeHisModel> recData, List<FrtCodeHisModel> rtnData)
        {

            string strConn = DbUtil.GetDBFglConnStr();
            List<ErrorModel> errList = new List<ErrorModel>();

            using (SqlConnection conn = new SqlConnection(strConn)) { 
                conn.Open();

                FRTCodeHisDao fRTCodeHisDao = new FRTCodeHisDao();

                //處理駁回資料
                if (rtnData.Count > 0)
                {
                    foreach (FrtCodeHisModel d in rtnData)
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
                                d.srceFrom = srceFrom;
                                d.groupId = groupId;
                                d.textLen = textLen;
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

                    FDCBANKADao fDCBANKADao = new FDCBANKADao();

                    using (EacConnection conn400 = new EacConnection(CommonUtil.GetEasycomConn()))
                    {
                        conn400.Open();

                        FRTCODEDao fRTCODEDao = new FRTCODEDao();
                        
                        foreach (FrtCodeHisModel d in recData)
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
                                    d.srceFrom = srceFrom;
                                    d.groupId = groupId;
                                    d.textLen = textLen;
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