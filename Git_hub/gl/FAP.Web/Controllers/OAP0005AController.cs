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
using System.Web.Mvc;

/// <summary>
/// 功能說明：保局範圍TABLE維護覆核作業
/// 初版作者：20190613 Daiyu
/// 修改歷程：20190613 Daiyu
///           需求單號：
///           初版
/// </summary>
///

namespace FRT.Web.Controllers
{
    [Authorize]
    [CheckSessionFilterAttribute]
    public class OAP0005AController : BaseController
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
                String[] roleInfo = authUtil.chkUserFuncAuth(Session["UserID"].ToString(), "~/OAP0005A/");
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
            FAPVeCodeHisDao fAPVeCodeHisDao = new FAPVeCodeHisDao();

            try {
                List<VeTraceModel> rows = fAPVeCodeHisDao.qryInProssByGrp(new string[] { "FSC_RANGE" });


                using (DB_INTRAEntities dbIntra = new DB_INTRAEntities())
                {
                    Dictionary<string, string> userNameMap = new Dictionary<string, string>();
                    OaEmpDao oaEmpDao = new OaEmpDao();
                    string update_id = "";

                    foreach (VeTraceModel d in rows)
                    {
                        update_id = StringUtil.toString(d.update_id);

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
        /// 核可/退回
        /// </summary>
        /// <param name="recData"></param>
        /// <param name="rtnData"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult execSave(List<VeTraceModel> recData, List<VeTraceModel> rtnData)
        {
            string strConn = DbUtil.GetDBFglConnStr();
            using (SqlConnection conn = new SqlConnection(strConn))
            {
                conn.Open();

                SqlTransaction transaction = conn.BeginTransaction("Transaction");

                try
                {
                    DateTime dt = DateTime.Now;
                    FAPVeCodeHisDao fAPVeCodeHisDao = new FAPVeCodeHisDao();
                    FAPVeCodeDao fAPVeCodeDao = new FAPVeCodeDao();


                    //處理駁回資料
                    if (rtnData.Count > 0)
                    {
                        foreach (VeTraceModel d in rtnData)
                        {
                            if (d.update_id.Equals(Session["UserID"].ToString()))
                                return Json(new { success = false, err = "申請人與覆核人員不可相同!!" }, JsonRequestBehavior.AllowGet);

                            d.appr_id = Session["UserID"].ToString();
                            d.code_type = "FSC_RANGE";

                        }
                        fAPVeCodeDao.procAppr("3", dt, rtnData, conn, transaction);
                        fAPVeCodeHisDao.updateApprMk("3" ,dt, rtnData, conn, transaction);
                        
                    }



                    //處理核可資料
                    if (recData.Count > 0)
                    {
                        
                        foreach (VeTraceModel d in recData)
                        {
                            if (d.update_id.Equals(Session["UserID"].ToString()))
                                return Json(new { success = false, err = "申請人與覆核人員不可相同!!" }, JsonRequestBehavior.AllowGet);

                            d.appr_id = Session["UserID"].ToString();
                            d.code_type = "FSC_RANGE";
                        }
                        fAPVeCodeDao.procAppr("2", dt, recData, conn, transaction);
                        fAPVeCodeHisDao.updateApprMk("2", dt, recData, conn, transaction);
                        
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





    }
}