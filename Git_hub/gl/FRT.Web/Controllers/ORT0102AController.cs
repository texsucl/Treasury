using FRT.Web.ActionFilter;
using FRT.Web.AS400Models;
using FRT.Web.BO;
using FRT.Web.Daos;
using FRT.Web.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.EasycomClient;
using System.Data.SqlClient;
using System.Linq;
using System.Web.Mvc;


/// <summary>
/// 功能說明：ORT0102A 非保費類退匯給付方式維護-覆核
/// 初版作者：20201211 Daiyu
/// 修改歷程：20201211 Daiyu
/// 需求單號：
/// 修改內容：初版
/// </summary>
///

namespace FRT.Web.Controllers
{
    [Authorize]
    [CheckSessionFilterAttribute]
    public class ORT0102AController : BaseController
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
                String[] roleInfo = authUtil.chkUserFuncAuth(Session["UserID"].ToString(), "~/ORT0102A/");
                if (roleInfo != null && roleInfo.Length == 2)
                {
                    opScope = roleInfo[0];
                    funcName = roleInfo[1];
                }
            }

            ViewBag.opScope = opScope;
            ViewBag.funcName = funcName;

            SysCodeDao sysCodeDao = new SysCodeDao();
            ViewBag.codeTypeList = sysCodeDao.loadSelectList("RT", "ORT0102", false);

            //執行功能
            ViewBag.execActionjqList = sysCodeDao.jqGridList("RT", "STATUS", true);


            return View();
        }


        /// <summary>
        /// 查詢待覆核的資料
        /// </summary>
        /// <param name="groupId"></param>
        /// <param name="srceFrom"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult LoadData(string groupId)
        {
            FRTCodeHisDao fRTCodeHisDao = new FRTCodeHisDao();

            try {
                List<FrtCodeHisModel> dataList = new List<FrtCodeHisModel>();
                List<ORT0102Model> rows = new List<ORT0102Model>();

                dataList = fRTCodeHisDao.qryForSTAT("RT", groupId, "", "", "1");

                //拆解RT-CODE資料，並判斷是否可維護
                foreach (FrtCodeHisModel d in dataList)
                {
                    ORT0102Model model = new ORT0102Model();
                    model.aply_no = d.aplyNo;
                    model.ptype = d.groupId;
                    model.group_id = d.groupId;
                    model.ref_no = d.refNo;
                    string _ref_no = d.refNo.PadRight(6, ' ');
                    model.system = _ref_no.Substring(0, 1);
                    model.srce_from = _ref_no.Substring(1, 3);
                    model.srce_kind = _ref_no.Substring(4, 2);
                    model.exec_action = d.status;

                    model.update_id = string.IsNullOrWhiteSpace(d.updId) == true ? "" : d.updId;
                    model.update_datetime = d.updDateTime;
                    //model.appr_id = string.IsNullOrWhiteSpace(d.apprId) == true ? "" : d.apprId;
                    //model.approve_datetime = string.IsNullOrWhiteSpace(d.apprDate) == true ? "" : d.apprDate;

                    rows.Add(model);
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
        public JsonResult execSave(string groupId, List<ORT0102Model> recData, List<ORT0102Model> rtnData)
        {

            string strConn = DbUtil.GetDBFglConnStr();
            List<ErrorModel> errList = new List<ErrorModel>();

            using (SqlConnection conn = new SqlConnection(strConn)) { 
                conn.Open();

                FRTCodeHisDao fRTCodeHisDao = new FRTCodeHisDao();

                //處理駁回資料
                if (rtnData.Count > 0)
                {
                    foreach (ORT0102Model d in rtnData)
                    {
                        SqlTransaction transaction = conn.BeginTransaction("Transaction");
                        try
                        {
                            if (d.update_id.Equals(Session["UserID"].ToString()))
                            {
                                ErrorModel errorModel = new ErrorModel();
                                errorModel.refNo = d.ref_no;
                                errorModel.errorMsg = "申請人與覆核人員不可相同!!";

                                errList.Add(errorModel);
                                transaction.Rollback();
                            }
                            else
                            {
                                FrtCodeHisModel _his = new FrtCodeHisModel();
                                _his.aplyNo = d.aply_no;
                                _his.groupId = d.group_id;
                                _his.srceFrom = "RT";
                                _his.refNo = d.ref_no;
                                _his.refNoN = "";
        
                                fRTCodeHisDao.updateFRTCodeHis(Session["UserID"].ToString(), "3", _his, conn, transaction);
                                transaction.Commit();
                            }
                        }
                        catch (Exception e) {
                            ErrorModel errorModel = new ErrorModel();
                            errorModel.refNo = d.ref_no;
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
                        
                        foreach (ORT0102Model d in recData)
                        {
                            SqlTransaction transaction = conn.BeginTransaction("Transaction");
                            EacTransaction transaction400 = conn400.BeginTransaction();

                            try
                            {
                                if (d.update_id.Equals(Session["UserID"].ToString()))
                                {
                                    ErrorModel errorModel = new ErrorModel();
                                    errorModel.refNo = d.ref_no;
                                    errorModel.errorMsg = "申請人與覆核人員不可相同!!";

                                    errList.Add(errorModel);
                                    transaction.Rollback();
                                    transaction400.Rollback();
                                }
                                else {
                                    FrtCodeHisModel _his = fRTCodeHisDao.qryForSTAT("RT", d.group_id, d.ref_no, "", "1").FirstOrDefault() ;
                                    if (_his == null)
                                    {
                                        ErrorModel errorModel = new ErrorModel();
                                        errorModel.refNo = d.ref_no;
                                        errorModel.errorMsg = "此筆資料已被處理!!";

                                        errList.Add(errorModel);
                                        transaction.Rollback();
                                        transaction400.Rollback();

                                    }
                                    else {
                                        fRTCodeHisDao.updateFRTCodeHis(Session["UserID"].ToString(), "2", _his, conn, transaction);

                                        _his.apprId = Session["UserID"].ToString();
                                        _his.refNoN = _his.refNo;

                                        if ("D".Equals(_his.status))
                                            fRTCODEDao.deleteFRTCODE0(Session["UserID"].ToString(), _his, conn400, transaction400);
                                        else
                                            fRTCODEDao.insertFRTCODE0(Session["UserID"].ToString(), _his, conn400, transaction400);

                                        transaction.Commit();
                                        transaction400.Commit();
                                    }
                                }
                            }
                            catch (Exception e) {
                                ErrorModel errorModel = new ErrorModel();
                                errorModel.refNo = d.ref_no;
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