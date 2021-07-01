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
using System.Linq;
using System.Web.Mvc;


/// <summary>
/// 功能說明：ORT0104 退匯個案處理作業
/// 初版作者：20210115 Daiyu
/// 修改歷程：20210115 Daiyu
/// 需求單號：202011050211
/// 修改內容：初版
/// </summary>
///

namespace FRT.Web.Controllers
{
    [Authorize]
    [CheckSessionFilterAttribute]
    public class ORT0104AController : BaseController
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

            if (Session["UserID"] != null)
            {
                String[] roleInfo = authUtil.chkUserFuncAuth(Session["UserID"].ToString(), "~/ORT0104A/");
                if (roleInfo != null && roleInfo.Length == 2)
                {
                    opScope = roleInfo[0];
                    funcName = roleInfo[1];
                }
            }

            ViewBag.opScope = opScope;
            ViewBag.funcName = funcName;

            FPMCODEDao fPMCODEDao = new FPMCODEDao();
            //退匯原因
            ViewBag.failCodejsList = fPMCODEDao.jqGridList("FAIL-CODE", "RT", "", true);



            return View();
        }


        /// <summary>
        /// 查詢待覆核的資料
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public JsonResult LoadData()
        {
            FRTRvmnHisDao fRTRvmnHisDao = new FRTRvmnHisDao();

            try {

                List<ORT0104Model> rows = fRTRvmnHisDao.qryForORT0104A();

                //查DB_INTRA取得異動人員、覆核人員姓名
                using (DB_INTRAEntities dbIntra = new DB_INTRAEntities())
                {
                    Dictionary<string, string> userNameMap = new Dictionary<string, string>();
                    OaEmpDao oaEmpDao = new OaEmpDao();
                    string update_id = "";

                    foreach (ORT0104Model d in rows)
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
        /// 開啟修改明細畫面
        /// </summary>
        /// <param name="aply_no"></param>
        /// <returns></returns>
        public ActionResult detailAply(string aply_no)
        {
            ViewBag.funcName = funcName;
            ViewBag.aply_no = aply_no;

            ORT0104Model oRT0104Model = new ORT0104Model();

            try
            {
             


                FRTRvmnHisDao fRTRvmnHisDao = new FRTRvmnHisDao();
                FRT_RVMN_HIS _his = fRTRvmnHisDao.qryByAplyNo(aply_no);
                if ("".Equals(StringUtil.toString(_his?.aply_no))) {
                    ViewBag.bHaveData = "N";
                    return View(oRT0104Model);

                }


                //自AS400查出資料
                FRTRVMNDao fRTRVMNDao = new FRTRVMNDao();
                oRT0104Model.currency = _his.currency;
                oRT0104Model.vhr_no1 = _his.vhr_no1;
                oRT0104Model.pro_no = _his.pro_no;
                oRT0104Model.paid_id = _his.paid_id;
                oRT0104Model = fRTRVMNDao.qryForORT0104(oRT0104Model);

                ObjectUtil.CopyPropertiesTo(_his, oRT0104Model);



                FPMCODEDao fPMCODEDao = new FPMCODEDao();
                List<FPMCODEModel> _code_list = fPMCODEDao.qryFPMCODE("FAIL-CODE", "RT", "");
                string fail_code = StringUtil.toString(_his.fail_code);
                if(!"".Equals(fail_code))
                    oRT0104Model.fail_code += StringUtil.toString(_code_list.Where(x => x.refNo == StringUtil.toString(_his.fail_code)).FirstOrDefault()?.text);

                string fail_code_o = StringUtil.toString(_his.fail_code_o);
                if (!"".Equals(fail_code_o))
                    oRT0104Model.fail_code_o += StringUtil.toString(_code_list.Where(x => x.refNo == StringUtil.toString(_his.fail_code_o)).FirstOrDefault()?.text);

                ViewBag.bHaveData = "Y";



                return View(oRT0104Model);



            }
            catch (Exception e)
            {
                ViewBag.bHaveData = "N";
                return View(oRT0104Model);
            }
        }


        /// <summary>
        /// 核可/退回
        /// </summary>
        /// <param name="aply_no"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult execSave(string aply_no, string appr_stat)
        {
            string strConn = DbUtil.GetDBFglConnStr();

            try
            {
                FRTRvmnHisDao fRTRvmnHisDao = new FRTRvmnHisDao();
                FRT_RVMN_HIS _his = fRTRvmnHisDao.qryByAplyNo(aply_no);

                if (_his.update_id.Equals(Session["UserID"].ToString()))
                {
                    var jsonData = new { success = false, err = "申請人與覆核人員不可相同!!" };
                    return Json(jsonData, JsonRequestBehavior.AllowGet);
                }

                using (SqlConnection conn = new SqlConnection(strConn))
                {
                    conn.Open();
                    SqlTransaction transaction = conn.BeginTransaction("Transaction");


                    try
                    {

                        fRTRvmnHisDao.updApprStat(Session["UserID"].ToString(), appr_stat, _his, conn, transaction);

                        //處理核可資料
                        if ("2".Equals(appr_stat))
                        {
                            using (EacConnection conn400 = new EacConnection(CommonUtil.GetEasycomConn()))
                            {
                                conn400.Open();
                                EacTransaction transaction400 = conn400.BeginTransaction();

                                try {
                                    FRTRVMNDao fRTRVMNDao = new FRTRVMNDao();

                                    fRTRVMNDao.updForORT0104A(Session["UserID"].ToString(), _his, conn400, transaction400);

                                    transaction400.Commit();
                                    transaction.Commit();

                                } catch(Exception e400) {
                                    logger.Error(e400.ToString());
                                    transaction.Rollback();
                                    transaction400.Rollback();
                                    throw e400;
                                }
                            }

                            string content = _his.currency + "|" + _his.vhr_no1 + "|" + _his.pro_no + "|" + _his.paid_id;
                            writePiaLog(content, 1, _his.paid_id, "E");

                        } else
                            transaction.Commit();

                    }
                    catch (Exception dbe)
                    {
                        logger.Error(dbe.ToString());
                        transaction.Rollback();
                        throw dbe;
                    }

                }
                return Json(new { success = true});
            }
                    
            catch (Exception e) {
                logger.Error(e.ToString);
                return Json(new { success = false, err = "其它錯誤，請洽系統管理員!!" }, JsonRequestBehavior.AllowGet);
            }
            
        }


        private void writePiaLog(string content, int affectRows, string piaOwner, string executionType)
        {
            PIA_LOG_MAIN piaLogMain = new PIA_LOG_MAIN();
            piaLogMain.TRACKING_TYPE = "A";
            piaLogMain.ACCESS_ACCOUNT = Session["UserID"].ToString();
            piaLogMain.ACCOUNT_NAME = Session["UserName"].ToString();
            piaLogMain.PROGFUN_NAME = "ORT0104AController";
            piaLogMain.EXECUTION_CONTENT = content;
            piaLogMain.AFFECT_ROWS = affectRows;
            piaLogMain.PIA_TYPE = "0100100000";
            piaLogMain.EXECUTION_TYPE = executionType;
            piaLogMain.ACCESSOBJ_NAME = "FRTRVMN0";
            piaLogMain.PIA_OWNER1 = piaOwner;
            PiaLogMainDao piaLogMainDao = new PiaLogMainDao();
            piaLogMainDao.Insert(piaLogMain);

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