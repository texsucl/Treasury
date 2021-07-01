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
/// 功能說明：OAP0050A EXCEL上傳清理記錄檔覆核作業
/// 初版作者：20200924 Daiyu
/// 修改歷程：20200924 Daiyu
/// 需求單號：202008120153-01
/// 初版
/// </summary>
///

namespace FAP.Web.Controllers
{
    [Authorize]
    [CheckSessionFilterAttribute]
    public class OAP0050AController : BaseController
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
                String[] roleInfo = authUtil.chkUserFuncAuth(Session["UserID"].ToString(), "~/OAP0050A/");
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
        /// 查詢待覆核的資料
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public JsonResult LoadData()
        {
            FAPVeTraceImpDao fAPVeTraceImpDao = new FAPVeTraceImpDao();

            try {
                List<OAP0050Model> rows = fAPVeTraceImpDao.qryForOAP0050AS("1", "");



                Dictionary<string, ADModel> userNameMap = new Dictionary<string, ADModel>();
                string usr_id = "";
                CommonUtil commonUtil = new CommonUtil();

                foreach (OAP0050Model d in rows)
                {

                    //取得申請人姓名
                    usr_id = StringUtil.toString(d.update_id);

                    if (!userNameMap.ContainsKey(usr_id))
                    {
                        ADModel adModel = new ADModel();
                        adModel = commonUtil.qryEmp(usr_id);
                        userNameMap.Add(usr_id, adModel);
                    }
                    d.update_name = userNameMap[usr_id].name;

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


        [HttpPost]
        public JsonResult qryCheck(string aply_no)
        {
            FAPVeTraceImpDao fAPVeTraceImpDao = new FAPVeTraceImpDao();

            try
            {
                List<OAP0050Model> rows = fAPVeTraceImpDao.qryForOAP0050AD("", aply_no);

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
        /// <param name="code_type"></param>
        /// <param name="recData"></param>
        /// <param name="rtnData"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult execSave(List<OAP0050Model> recData, List<OAP0050Model> rtnData)
        {
            string strConn = DbUtil.GetDBFglConnStr();


            try
            {
                DateTime dt = DateTime.Now;
                FAPVeTraceImpDao fAPVeTraceImpDao = new FAPVeTraceImpDao();


                //處理駁回資料
                if (rtnData.Count > 0)
                {
                    foreach (OAP0050Model d in rtnData)
                    {
                        if (d.update_id.Equals(Session["UserID"].ToString()))
                            return Json(new { success = false, err = "申請人與覆核人員不可相同!!" }, JsonRequestBehavior.AllowGet);

                        FAP_VE_TRACE_IMP imp = new FAP_VE_TRACE_IMP();
                        imp.aply_no = d.aply_no;

                        using (SqlConnection conn = new SqlConnection(strConn))
                        {
                            conn.Open();

                            SqlTransaction transaction = conn.BeginTransaction("Transaction");

                            try
                            {
                                fAPVeTraceImpDao.updateApprStat("3", dt, Session["UserID"].ToString(), imp, conn, transaction);
                                transaction.Commit();
                            }
                            catch (Exception e)
                            {
                                transaction.Rollback();
                                logger.Error(e.ToString());
                                return Json(new { success = false, err = "覆核單號:" + d.aply_no + "執行錯誤，請洽系統管理員!!" }, JsonRequestBehavior.AllowGet);

                            }
                        }
                    }
                }



                //處理核可資料
                if (recData.Count > 0)
                {
                    FAPVeTraceDao fAPVeTraceDao = new FAPVeTraceDao();
                    FAPVeTracePoliDao fAPVeTracePoliDao = new FAPVeTracePoliDao();
                    VeCleanUtil veCleanUtil = new VeCleanUtil();

                    using (EacConnection conn400 = new EacConnection(CommonUtil.GetEasycomConn()))
                    {
                        conn400.Open();
                        try
                        {
                            foreach (OAP0050Model d in recData)
                            {
                                if (d.update_id.Equals(Session["UserID"].ToString()))
                                    return Json(new { success = false, err = "申請人與覆核人員不可相同!!" }, JsonRequestBehavior.AllowGet);

                                d.appr_id = Session["UserID"].ToString();

                                

                                using (SqlConnection conn = new SqlConnection(strConn))
                                {
                                    conn.Open();

                                    SqlTransaction transaction = conn.BeginTransaction("Transaction");

                                    try
                                    {
                                        FAP_VE_TRACE_IMP imp = new FAP_VE_TRACE_IMP();
                                        imp.aply_no = d.aply_no;
                                        


                                        //取得該批覆核單號內的支票
                                        List<OAP0050Model> checkList = fAPVeTraceImpDao.qryForOAP0050AD("1", d.aply_no);
                                        foreach (OAP0050Model check in checkList.GroupBy(o => new { o.check_no, o.check_acct_short, o.system })
                                                        .Select(group => new OAP0050Model
                                                        {
                                                            check_no = group.Key.check_no,
                                                            check_acct_short = group.Key.check_acct_short,
                                                            system = group.Key.system
                                                        }).ToList<OAP0050Model>())
                                        {
                                           

                                            OAP0050Model model = checkList.Where(x => x.system == check.system & x.check_acct_short == check.check_acct_short & x.check_no == check.check_no)
                                                                            .FirstOrDefault();
                                            FAP_VE_TRACE trace = new FAP_VE_TRACE();
                                            trace.system = model.system;
                                            trace.check_no = model.check_no;
                                            trace.check_acct_short = model.check_acct_short;
                                            trace.paid_id = model.paid_id;
                                            trace.paid_name = model.paid_name;
                                            trace.check_amt = Convert.ToDecimal(model.check_amt);
                                            trace.check_date = DateUtil.ChtDateToADDate(model.check_date, '/');
                                            trace.re_paid_date = DateUtil.ChtDateToADDate(model.re_paid_date, '/');
                                            trace.re_paid_date_n = DateUtil.ChtDateToADDate(model.re_paid_date_n, '/');
                                            trace.re_paid_type = model.re_paid_type;
                                            trace.fsc_range = StringUtil.toString(model.fsc_range) == "" ? "0" : model.fsc_range;

                                            if(trace.re_paid_date == null)
                                                trace.status = "4";
                                            else
                                                trace.status = "1";


                                            trace.update_id = Session["UserID"].ToString();
                                            trace.update_datetime = dt;
                                            trace.data_status = "1";
                                            //trace.re_paid_date_n = DateUtil.ChtDateToADDate(model.re_paid_date_n, '/');

                                            List<VeCleanModel> as400List = new List<VeCleanModel>();
                                            foreach (OAP0050Model tmp in checkList.Where(x => x.system == check.system & x.check_acct_short == check.check_acct_short & x.check_no == check.check_no))
                                            {
                                                VeCleanModel veCleanModel = new VeCleanModel();
                                                ObjectUtil.CopyPropertiesTo(tmp, veCleanModel);

                                                as400List.Add(veCleanModel);
                                            }

                                            int i = 0;

                                            //List<VeCleanModel> as400List = veCleanUtil.qryCheckPolicy(check.system, check.check_no, check.check_acct_short, conn400);

                                            //if (as400List.Count == 0)
                                            //{
                                            //    foreach (OAP0050Model tmp in checkList.Where(x => x.system == check.system & x.check_acct_short == check.check_acct_short & x.check_no == check.check_no))
                                            //    {
                                            //        VeCleanModel veCleanModel = new VeCleanModel();
                                            //        ObjectUtil.CopyPropertiesTo(tmp, veCleanModel);

                                            //        as400List.Add(veCleanModel);
                                            //    }
                                            //}
                                            //else
                                            //{
                                            //    trace.paid_name = StringUtil.toString(as400List[0].paid_name);

                                            //    try
                                            //    {
                                            //        trace.check_amt = Convert.ToDecimal(as400List[0].check_amt);
                                            //    }
                                            //    catch (Exception e)
                                            //    {

                                            //    }

                                            //    try
                                            //    {
                                            //        trace.check_date = DateUtil.stringToDatetime(DateUtil.As400ChtDateToADDate(StringUtil.toString(as400List[0].check_date)));
                                            //    }
                                            //    catch (Exception e)
                                            //    {

                                            //    }
                                            //}


                                            //處理【FAP_VE_TRACE 逾期未兌領清理記錄檔】
                                            fAPVeTraceDao.insertForOAP0050(trace, conn, transaction);


                                            //處理【FAP_VE_TRACE_POLI 逾期未兌領清理保單明細檔】
                                            foreach (VeCleanModel poli in as400List) {
                                                FAP_VE_TRACE_POLI poliModel = new FAP_VE_TRACE_POLI();

                                                ObjectUtil.CopyPropertiesTo(poli, poliModel);

                                                if(!"".Equals(StringUtil.toString(poli.policy_no)))
                                                    fAPVeTracePoliDao.insert(poliModel, conn, transaction);
                                            }

                                            //處理【FAP_VE_TRACE_IMP 逾期未兌領清理記錄批次匯入檔】
                                            fAPVeTraceImpDao.updateApprStat("2", dt, Session["UserID"].ToString(), imp, conn, transaction);
                                        }

                                        transaction.Commit();

                                    }
                                    catch (Exception e)
                                    {
                                        logger.Error(e.ToString);
                                        transaction.Rollback();
                                    }
                                }


                                    
                            }
                        }
                        catch (Exception e)
                        {
                            throw e;
                        }
                    }
                }

                return Json(new { success = true });
            }
            catch (Exception e)
            {

                logger.Error("[execReviewR]其它錯誤：" + e.ToString());

                return Json(new { success = false, err = "其它錯誤，請洽系統管理員!!" }, JsonRequestBehavior.AllowGet);
            }

        }

    }
}