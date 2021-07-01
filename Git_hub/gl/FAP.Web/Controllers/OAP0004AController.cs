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
/// 功能說明：指定保局範圍覆核作業
/// 初版作者：20190627 Daiyu
/// 修改歷程：20190627 Daiyu
///           需求單號：
///           初版
/// </summary>
///

namespace FAP.Web.Controllers
{
    [Authorize]
    [CheckSessionFilterAttribute]
    public class OAP0004AController : BaseController
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
            string[] roleInfo = authUtil.chkUserFuncAuth(Session["UserID"].ToString(), "~/OAP0004A/");
            if (roleInfo != null && roleInfo.Length == 2)
            {
                opScope = roleInfo[0];
                funcName = roleInfo[1];
            }

            ViewBag.funcName = funcName;
            ViewBag.opScope = opScope;

            FAPVeCodeDao fAPVeCodeDao = new FAPVeCodeDao();
            //指定保局範圍
            ViewBag.fscRangejqList = fAPVeCodeDao.jqGridList("FSC_RANGE", true);

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
                List<OAP0004Model> rows = new List<OAP0004Model>();
                List<APAplyRecModel> aplyList = new List<APAplyRecModel>();

                using (new TransactionScope(TransactionScopeOption.Required, new TransactionOptions
                {
                    IsolationLevel = System.Transactions.IsolationLevel.ReadUncommitted
                }))
                {
                    using (dbFGLEntities db = new dbFGLEntities())
                    {
                        FAPAplyRecDao fAPAplyRecDao = new FAPAplyRecDao();
                        aplyList = fAPAplyRecDao.qryAplyType("VE", "1", "", db);

                    }
                }


                using (DB_INTRAEntities dbIntra = new DB_INTRAEntities())
                {
                    Dictionary<string, string> userNameMap = new Dictionary<string, string>();
                    OaEmpDao oaEmpDao = new OaEmpDao();
                    string createUid = "";

                    foreach (APAplyRecModel d in aplyList)
                    {
                        OAP0004Model model = new OAP0004Model();
                        model.aply_no = d.aply_no;
                        model.update_id = d.create_id;
                        model.update_datetime = d.create_dt;

                        try
                        {
                            string[] memoArr = d.memo.Split('|');
                            string checkDateB = memoArr[0] == "" ? "" : DateUtil.ADDateToChtDate(memoArr[0], 3, "");
                            string checkDateE = memoArr[1] == "" ? "" : DateUtil.ADDateToChtDate(memoArr[1], 3, "");

                            model.check_date = checkDateB + "~" + checkDateE;
                            model.check_no = memoArr[2];
                            model.fsc_range = memoArr[3];
                            model.fsc_range_n = memoArr[4];
                            model.update_cnt = memoArr[5];
                        }
                        catch (Exception e) {
                            logger.Error(e.ToString());
                        }


                        createUid = StringUtil.toString(d.create_id);


                        if (!"".Equals(createUid))
                        {
                            if (!userNameMap.ContainsKey(createUid))
                            {
                                userNameMap = oaEmpDao.qryUsrName(userNameMap, createUid, dbIntra);
                            }
                            model.update_id = createUid + " " + userNameMap[createUid];
                        }

                        rows.Add(model);
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

            FAPVeCodeDao fAPVeCodeDao = new FAPVeCodeDao();
            //指定保局範圍
            ViewBag.fscRangeList = fAPVeCodeDao.loadSelectList("FSC_RANGE", true);
            ViewBag.fscRangejqList = fAPVeCodeDao.jqGridList("FSC_RANGE", true);

            OAP0004Model oAP0004Model = new OAP0004Model();

            try
            {
                FAPAplyRecDao fAPAplyRecDao = new FAPAplyRecDao();
                FAP_APLY_REC aply = fAPAplyRecDao.qryByKey(aplyNo);
                if (aply != null)
                {
                    oAP0004Model.aply_no = aply.aply_no;
                    oAP0004Model.update_id = aply.create_id;
                    oAP0004Model.update_datetime = DateUtil.DatetimeToString(aply.create_dt, "");

                    string[] memoArr = aply.memo.Split('|');
                    string checkDateB = memoArr[0] == "" ? "" : DateUtil.ADDateToChtDate(memoArr[0], 3, "");
                    string checkDateE = memoArr[1] == "" ? "" : DateUtil.ADDateToChtDate(memoArr[1], 3, "");

                    oAP0004Model.check_date = checkDateB + "~" + checkDateE;
                    oAP0004Model.check_no = memoArr[2];
                    oAP0004Model.fsc_range = memoArr[3];
                    oAP0004Model.fsc_range_n = memoArr[4];
                    oAP0004Model.update_cnt = memoArr[5];


                    using (DB_INTRAEntities dbIntra = new DB_INTRAEntities())
                    {
                        Dictionary<string, string> userNameMap = new Dictionary<string, string>();
                        OaEmpDao oaEmpDao = new OaEmpDao();
                        string createUid = "";

                        createUid = StringUtil.toString(oAP0004Model.update_id);

                        if (!"".Equals(createUid))
                        {
                            if (!userNameMap.ContainsKey(createUid))
                                userNameMap = oaEmpDao.qryUsrName(userNameMap, createUid, dbIntra);

                            oAP0004Model.update_id = createUid + " " + userNameMap[createUid];
                        }
                    }

                    ViewBag.bHaveData = "Y";
                    ViewBag.aply_no = aplyNo;
                    ViewBag.fsc_range_n = oAP0004Model.fsc_range_n;

                    return View(oAP0004Model);
                }
                else {
                    ViewBag.bHaveData = "N";
                    return View(oAP0004Model);
                }
            }
            catch (Exception e)
            {
                ViewBag.bHaveData = "N";
                return View(oAP0004Model);
            }
        }


        [HttpPost]
        public JsonResult qryVeTraceHis(string aply_no
            , string page, string sidx, string sord, int rows)
        {
            logger.Info("qryVeTraceHis begin!!");
            try
            {
                List<OAP0004Model> dataList = new List<OAP0004Model>();

                //查詢【FAP_VE_TRACE_HIS 逾期未兌領清理記錄暫存檔】
                FAPVeTraceHisDao fAPVeTraceHisDao = new FAPVeTraceHisDao();

                dataList = fAPVeTraceHisDao.qryForOAP0004(aply_no);

                int pageIndex = Convert.ToInt32(page) - 1;
                int totalRecords = dataList.Count();
                int totalPages = (int)Math.Ceiling((float)totalRecords / (float)rows);

                var jsonData = new
                {
                    success = true,
                    total = totalPages,
                    page,
                    records = totalRecords,
                    rows = dataList.Skip(pageIndex * rows).Take(rows).ToList()
                };

               // var jsonData = new { success = true, dataList = rows };
                return Json(jsonData, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new { success = false, err = "其它錯誤，請洽系統管理員!!" });
            }
        }


        [HttpPost]
        public JsonResult qryVeTraceHisAmt(string aply_no)
        {
            logger.Info("qryVeTraceHis begin!!");
            try
            {
                List<OAP0004Model> dataList = new List<OAP0004Model>();

                //查詢【FAP_VE_TRACE_HIS 逾期未兌領清理記錄暫存檔】
                FAPVeTraceHisDao fAPVeTraceHisDao = new FAPVeTraceHisDao();

                dataList = fAPVeTraceHisDao.qryForOAP0004(aply_no);

                var amt = dataList.Sum(y => Convert.ToInt64(y.check_amt.Replace(".00", ""))); // To just bring down the right values



                var jsonData = new
                {
                    success = true,
                    amt = amt
                };

                // var jsonData = new { success = true, dataList = rows };
                return Json(jsonData, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new { success = false, err = "其它錯誤，請洽系統管理員!!" });
            }
        }

        /// <summary>
        /// 核可/退回
        /// </summary>
        /// <param name="aply_no"></param>
        /// <param name="apprStat"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult execSave(string aply_no, string apprStat, string fsc_range)
        {
            string strConn = DbUtil.GetDBFglConnStr();
            using (SqlConnection conn = new SqlConnection(strConn))
            {
                conn.Open();
                SqlTransaction transaction = conn.BeginTransaction("Transaction");

                try
                {
                    FAPAplyRecDao fAPAplyRecDao = new FAPAplyRecDao();
                    FAP_APLY_REC fapAplyRec = fAPAplyRecDao.qryByKey(aply_no);

                    if (StringUtil.toString(fapAplyRec.create_id).Equals(Session["UserID"].ToString()))
                        return Json(new { success = false, errors = "申請人與覆核人員不可相同!!" }, JsonRequestBehavior.AllowGet);

                    DateTime dt = DateTime.Now;

                    //異動覆核資料檔
                    fapAplyRec.aply_no = aply_no;
                    fapAplyRec.appr_stat = apprStat;
                    fapAplyRec.appr_id = Session["UserID"].ToString();
                    fapAplyRec.approve_datetime = dt;
                    fapAplyRec.update_id = Session["UserID"].ToString();
                    fapAplyRec.update_datetime = dt;

                    fAPAplyRecDao.updateStatus(fapAplyRec, conn, transaction);


                    //核可
                    if ("2".Equals(apprStat))
                    {
                        FAPVeTraceDao fAPVeTraceDao = new FAPVeTraceDao();
                        fAPVeTraceDao.updateFscRange(dt, Session["UserID"].ToString(), fsc_range, aply_no, conn, transaction);
                    }
   

                    transaction.Commit();
                    return Json(new { success = true });

                }
                catch (Exception e)
                {
                    transaction.Rollback();
                    logger.Error("其它錯誤：" + e.ToString());

                    return Json(new { success = false, errors = "其它錯誤，請洽系統管理員!!" }, JsonRequestBehavior.AllowGet);
                }
            }
        }






    }
}