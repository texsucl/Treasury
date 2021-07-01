using FRT.Web.ActionFilter;
using FRT.Web.BO;
using FRT.Web.Daos;
using FRT.Web.Models;
using FRT.Web.ViewModels;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.EasycomClient;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace FRT.Web.Controllers
{
    [Authorize]
    [CheckSessionFilterAttribute]
    public class ORT0103AController : BaseController
    {
        static private NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        SysCodeDao sysCodeDao = null;
        FRTCodeHisDao FRTCodeHisDao = null;
        FRTGLSIDao fRTGLSIDao = null;
        public ORT0103AController()
        {
            sysCodeDao = new SysCodeDao();
            FRTCodeHisDao = new FRTCodeHisDao();
            fRTGLSIDao = new FRTGLSIDao();
        }

        // GET: ORT0103A
        public ActionResult Index()
        {
            UserAuthUtil authUtil = new UserAuthUtil();

            string opScope = "";
            string funcName = "";

            if (Session["UserID"] != null)
            {
                String[] roleInfo = authUtil.chkUserFuncAuth(Session["UserID"].ToString(), "~/ORT0103A/");
                if (roleInfo != null && roleInfo.Length == 2)
                {
                    opScope = roleInfo[0];
                    funcName = roleInfo[1];
                }
            }

            ViewBag.opScope = opScope;
            ViewBag.funcName = funcName;

            //SysCodeDao sysCodeDao = new SysCodeDao();
            //資料狀態
            ViewBag.statusjqList = sysCodeDao.qryByTypeDic("RT", "STATUS");

            return View();
        }

        /// <summary>
        /// 查詢待覆核的資料
        /// </summary>
        /// <param name="cReviewType"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult LoadData()
        {
            try
            {
                List<FrtCodeHisModel> datas = FRTCodeHisDao.qryForSTAT("RT", "FAIL-REP", "", "", "1");
                var rejectedData = fRTGLSIDao.SelectReJectedDatas();
                List<ORT0103Model> dataList = datas.Select(x => new ORT0103Model
                {
                    aplyNo = x.aplyNo,
                    status = x.status,
                    rejected_Code = string.IsNullOrWhiteSpace(x.refNo) == true ? "" : x.refNo,
                    rejected_Reason = rejectedData.FirstOrDefault(y => y.Value == x.refNo)?.Text,
                    received_Id = x.text?.Substring(0, 1),
                    received_Account = x.text?.Substring(1, 1),
                    bank_Code = x.text?.Substring(2, 1),
                    bank_Account = x.text.Substring(3, 1),
                    update_Id = x.updId,
                    update_Datetime = x.updDateTime
                }).ToList();

                //查DB_INTRA取得異動人員、覆核人員姓名
                using (DB_INTRAEntities dbIntra = new DB_INTRAEntities())
                {
                    Dictionary<string, string> userNameMap = new Dictionary<string, string>();
                    OaEmpDao oaEmpDao = new OaEmpDao();
                    string update_id = "";

                    foreach (ORT0103Model d in dataList)
                    {
                        update_id = StringUtil.toString(d.update_Id);
                        d.update_Name = update_id;

                        if (!"".Equals(update_id))
                        {
                            if (!userNameMap.ContainsKey(update_id))
                            {
                                userNameMap = oaEmpDao.qryUsrName(userNameMap, update_id, dbIntra);
                            }
                            d.update_Name = userNameMap[update_id];
                        }
                    }
                }

                var jsonData = new { success = true, rows = dataList };
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
        public JsonResult execSave(List<ORT0103Model> recData, List<ORT0103Model> rtnData)
        {
            var strConn = ConfigurationManager.ConnectionStrings["dbFGL"].ConnectionString;
            List<ErrorModel> errList = new List<ErrorModel>();

            using (SqlConnection conn = new SqlConnection(strConn))
            {
                conn.Open();

                //處理駁回資料

                if (rtnData.Count > 0)
                {
                    List<FrtCodeHisModel> rtnList = new List<FrtCodeHisModel>();

                    foreach (ORT0103Model d in rtnData)
                    {
                        FrtCodeHisModel model = new FrtCodeHisModel();
                        model.aplyNo = d.aplyNo;
                        model.refNo = d.rejected_Code;
                        model.refNoN = d.rejected_Code;
                        model.text = $"{d.received_Id?.Trim()}{d.received_Account?.Trim()}{d.bank_Code?.Trim()}{d.bank_Account?.Trim()}";
                        model.updId = d.update_Id;
                        model.updDateTime = d.update_Datetime;
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
                                d.groupId = "FAIL-REP";
                                d.textLen = "4";
                                FRTCodeHisDao.updateFRTCodeHis(Session["UserID"].ToString(), "3", d, conn, transaction);
                                transaction.Commit();
                            }
                        }
                        catch (Exception e)
                        {
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

                        foreach (ORT0103Model d in recData)
                        {
                            FrtCodeHisModel model = new FrtCodeHisModel();
                            model.aplyNo = d.aplyNo;
                            model.refNo = d.rejected_Code;
                            model.refNoN = d.rejected_Code;
                            model.text = $"{d.received_Id?.Trim()}{d.received_Account?.Trim()}{d.bank_Code?.Trim()}{d.bank_Account?.Trim()}";
                            model.updId = d.update_Id;
                            model.updDateTime = d.update_Datetime;
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
                                else
                                {
                                    d.srceFrom = "RT";
                                    d.groupId = "FAIL-REP";
                                    d.textLen = "4";
                                    d.apprId = Session["UserID"].ToString();
                                    d.apprvFlg = "Y";
                                    d.apprStat = "Y";

                                    FRTCodeHisDao.updateFRTCodeHis(Session["UserID"].ToString(), "2", d, conn, transaction);
                                    fRTCODEDao.apprFRTCODE0(Session["UserID"].ToString(), d, conn400, transaction400);

                                    transaction.Commit();
                                    transaction400.Commit();
                                }
                            }
                            catch (Exception e)
                            {
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
            else
            {
                return Json(new { success = true });
            }
        }

        /// <summary>
        /// 錯誤參數model
        /// </summary>
        private partial class ErrorModel
        {

            public string refNo { get; set; }

            public string errorMsg { get; set; }
        }
    }
}