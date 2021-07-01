using FAP.Web.ActionFilter;
using FAP.Web.AS400Models;
using FAP.Web.BO;
using FAP.Web.Daos;
using FAP.Web.Models;
using FAP.Web.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.EasycomClient;
using System.Data.SqlClient;
using System.Linq;
using System.Transactions;
using System.Web.Mvc;

/// <summary>
/// 功能說明：逾期未兌領支票維護覆核作業
/// 初版作者：20190417 Daiyu
/// 修改歷程：20190417 Daiyu
///           需求單號：
///           初版
/// </summary>
///

namespace FAP.Web.Controllers
{
    [Authorize]
    [CheckSessionFilterAttribute]
    public class OAP0003AController : BaseController
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
           // string funcName = "";
            string[] roleInfo = authUtil.chkUserFuncAuth(Session["UserID"].ToString(), "~/OAP0003A/");
            if (roleInfo != null && roleInfo.Length == 2)
            {
                opScope = roleInfo[0];
                funcName = roleInfo[1];
            }

            ViewBag.funcName = funcName;
            ViewBag.opScope = opScope;


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

                List<APAplyRecModel> rows = new List<APAplyRecModel>();
                //依「信函編號」查是否有覆核中的資料
                using (new TransactionScope(TransactionScopeOption.Required, new TransactionOptions
                {
                    IsolationLevel = System.Transactions.IsolationLevel.ReadUncommitted
                }))
                {
                    using (dbFGLEntities db = new dbFGLEntities())
                    {
                        FAPAplyRecDao fAPAplyRecDao = new FAPAplyRecDao();
                        rows = fAPAplyRecDao.qryAplyType("B", "1", "", db);

                    }
                }


                using (DB_INTRAEntities dbIntra = new DB_INTRAEntities())
                {
                    Dictionary<string, string> userNameMap = new Dictionary<string, string>();
                    OaEmpDao oaEmpDao = new OaEmpDao();
                    string createUid = "";

                    foreach (APAplyRecModel d in rows)
                    {
                        createUid = StringUtil.toString(d.create_id);

                        if (!"".Equals(createUid))
                        {
                            if (!userNameMap.ContainsKey(createUid))
                            {
                                userNameMap = oaEmpDao.qryUsrName(userNameMap, createUid, dbIntra);
                            }
                            d.create_id = createUid + " " + userNameMap[createUid];
                        }
                    }
                }


                var jsonData = new { success = true, rows };
                return Json(jsonData, JsonRequestBehavior.AllowGet);

            }
            catch (Exception e) {

                logger.Error("其它錯誤：" + e.ToString());
                return Json(new { success = false, err = "其它錯誤，請洽系統管理員!!" });
            }




        }


        /// <summary>
        /// 開啟修改明細畫面
        /// </summary>
        /// <param name="aplyNo"></param>
        /// <returns></returns>
        public ActionResult detailAply(string aplyNo)
        {
            ViewBag.funcName = funcName;

            FPMCODEDao fPMCODEDao = new FPMCODEDao();
            //原給付方式(給付項目)
            ViewBag.oPaidCdjqList = fPMCODEDao.jqGridList("PAID_CDTXT", "AP", true);

            //處理階段
            ViewBag.rStatusList = fPMCODEDao.qryGrpList("PPADSTATUS", "AP");

            OAP0003Model oAP0003Model = new OAP0003Model();
            List<OAP0003PoliModel> ppadList = new List<OAP0003PoliModel>();
            try
            {
                FAPAplyRecDao fAPAplyRecDao = new FAPAplyRecDao();
                FAP_APLY_REC aply = fAPAplyRecDao.qryByKey(aplyNo);
                if (aply != null)
                {
                    oAP0003Model.aplyNo = aply.aply_no;
                    oAP0003Model.create_id = aply.create_id;
                    oAP0003Model.create_dt = DateUtil.DatetimeToString(aply.create_dt, "");
                    oAP0003Model.report_no = aply.appr_mapping_key;

                    using (DB_INTRAEntities dbIntra = new DB_INTRAEntities())
                    {
                        Dictionary<string, string> userNameMap = new Dictionary<string, string>();
                        OaEmpDao oaEmpDao = new OaEmpDao();
                        string createUid = "";

                        createUid = StringUtil.toString(oAP0003Model.create_id);

                        if (!"".Equals(createUid))
                        {
                            if (!userNameMap.ContainsKey(createUid))
                            {
                                userNameMap = oaEmpDao.qryUsrName(userNameMap, createUid, dbIntra);
                            }
                            oAP0003Model.create_id = createUid + " " + userNameMap[createUid];
                        }
                    }


                    int cnt = 0;
                    using (EacConnection conn400 = new EacConnection(CommonUtil.GetEasycomConn()))
                    {
                        conn400.Open();

                        FMNPPADDao fMNPPADDao = new FMNPPADDao();
                        ppadList = fMNPPADDao.qryForOAP0003(conn400, oAP0003Model.report_no);

                        if (ppadList.Count > 0)
                        {
                            ObjectUtil.CopyPropertiesTo(ppadList[0], oAP0003Model);
                            cnt = ppadList.Count;
    
                        }
                    }


                    ViewBag.bHaveData = "Y";
                    ViewBag.aplyNo = aplyNo;
                    ViewBag.ppadList = ppadList;

                    writePiaLog(oAP0003Model.report_no, cnt, oAP0003Model.paid_id, "Q", "FMNPPAD0");

                    return View(oAP0003Model);
                }
                else {
                    ViewBag.bHaveData = "N";
                    return View(oAP0003Model);
                }


            }
            catch (Exception e)
            {
                ViewBag.bHaveData = "N";
                return View(oAP0003Model);
            }
        }

        



        /// <summary>
        /// 核可/退回
        /// </summary>
        /// <param name="model"></param>
        /// <param name="apprStat"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult execSave(OAP0003Model model, string apprStat)
        {
            string strConn = DbUtil.GetDBFglConnStr();
            using (SqlConnection conn = new SqlConnection(strConn))
            {
                conn.Open();
                SqlTransaction transaction = conn.BeginTransaction("Transaction");

                try
                {
                    FAPAplyRecDao fAPAplyRecDao = new FAPAplyRecDao();
                    FAP_APLY_REC fapAplyRec = fAPAplyRecDao.qryByKey(model.aplyNo);

                    if (StringUtil.toString(fapAplyRec.create_id).Equals(Session["UserID"].ToString()))
                        return Json(new { success = false, errors = "申請人與覆核人員不可相同!!" }, JsonRequestBehavior.AllowGet);

                    DateTime dt = DateTime.Now;

                    //異動覆核資料檔
                    fapAplyRec.aply_no = model.aplyNo;
                    fapAplyRec.appr_stat = apprStat;
                    fapAplyRec.appr_id = Session["UserID"].ToString();
                    fapAplyRec.approve_datetime = dt;
                    fapAplyRec.update_id = Session["UserID"].ToString();
                    fapAplyRec.update_datetime = dt;

                    

                    //核可
                    if ("2".Equals(apprStat))
                    {
                        using (EacConnection conn400 = new EacConnection(CommonUtil.GetEasycomConn()))
                        {
                            conn400.Open();
                            EacTransaction transaction400 = conn400.BeginTransaction();

                            try
                            {
                                string msg = procAS400DB(model.report_no, model.paid_id, dt, conn400, transaction400);

                                if ("".Equals(msg))
                                {
                                    fAPAplyRecDao.updateStatus(fapAplyRec, conn, transaction);

                                    transaction.Commit();
                                    transaction400.Commit();

                                    return Json(new { success = true });
                                }
                                else
                                {
                                    transaction.Rollback();
                                    transaction400.Rollback();
                                    logger.Error("其它錯誤：" + msg);
                                    return Json(new { success = false, errors = msg }, JsonRequestBehavior.AllowGet);
                                }

                            }
                            catch (Exception e)
                            {
                                transaction.Rollback();
                                transaction400.Rollback();
                                logger.Error("其它錯誤：" + e.ToString());
                                return Json(new { success = false, errors = "其它錯誤，請洽系統管理員!!" }, JsonRequestBehavior.AllowGet);
                            }
                        }
                    }
                    else {
                        fAPAplyRecDao.updateStatus(fapAplyRec, conn, transaction);

                        transaction.Commit();
                        return Json(new { success = true });
                    }

 

                }
                catch (Exception e)
                {
                    transaction.Rollback();
                    logger.Error("其它錯誤：" + e.ToString());

                    return Json(new { success = false, errors = "其它錯誤，請洽系統管理員!!" }, JsonRequestBehavior.AllowGet);
                }
            }
        }



        private string procAS400DB(string report_no, string paid_id, DateTime dt, 
            EacConnection conn400, EacTransaction transaction400) {
            string msg = "";
            string strDt = DateUtil.getCurChtDateTime().Split(' ')[0];


            try
            {
                FMNPPADDao fMNPPADDao = new FMNPPADDao();
                int cnt = fMNPPADDao.delForOAP0003(report_no, conn400, transaction400);
                writePiaLog(report_no, cnt, paid_id, "D", "FMNPPAD0");
                
               
            }
            catch (Exception e) {
                throw e;

            }

            return msg;

        }


        private void writePiaLog(string reportNo, int affectRows, string piaOwner, string executionType, string accessobjName)
        {
            PIA_LOG_MAIN piaLogMain = new PIA_LOG_MAIN();
            piaLogMain.TRACKING_TYPE = "A";
            piaLogMain.ACCESS_ACCOUNT = Session["UserID"].ToString();
            piaLogMain.ACCOUNT_NAME = Session["UserName"].ToString();
            piaLogMain.PROGFUN_NAME = "OAP0003AController";
            piaLogMain.EXECUTION_CONTENT = reportNo;
            piaLogMain.AFFECT_ROWS = affectRows;
            piaLogMain.PIA_TYPE = "0100100000";
            piaLogMain.EXECUTION_TYPE = executionType;
            piaLogMain.ACCESSOBJ_NAME = accessobjName;
            piaLogMain.PIA_OWNER1 = piaOwner;
            PiaLogMainDao piaLogMainDao = new PiaLogMainDao();
            piaLogMainDao.Insert(piaLogMain);

        }


    }
}