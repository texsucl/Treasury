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
/// 功能說明：OAP0041A 電訪派件設定覆核作業
/// 初版作者：20200714 Daiyu
/// 修改歷程：20200714 Daiyu
///           需求單號：
///           初版
/// </summary>
///

namespace FAP.Web.Controllers
{
    [Authorize]
    [CheckSessionFilterAttribute]
    public class OAP0041AController : BaseController
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
                String[] roleInfo = authUtil.chkUserFuncAuth(Session["UserID"].ToString(), "~/OAP0041A/");
                if (roleInfo != null && roleInfo.Length == 2)
                {
                    opScope = roleInfo[0];
                    funcName = roleInfo[1];
                }
            }

            ViewBag.opScope = opScope;
            ViewBag.funcName = funcName;

            SysCodeDao sysCodeDao = new SysCodeDao();
            ViewBag.codeTypeList = sysCodeDao.loadSelectList("AP", "TEL_CODE_TYPE", false);

            return View();
        }


        /// <summary>
        /// 查詢待覆核的資料
        /// </summary>
        /// <param name="code_type"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult LoadData(string code_type)
        {
            FAPVeCodeHisDao fAPVeCodeHisDao = new FAPVeCodeHisDao();

            try {
                List<VeTraceModel> dataList = fAPVeCodeHisDao.qryInProssByGrp(new string[] { code_type }).OrderBy(o => o.remark).ToList();
                List<OAP0041Model> rows = new List<OAP0041Model>();


                Dictionary<string, ADModel> userNameMap = new Dictionary<string, ADModel>();
                string usr_id = "";
                CommonUtil commonUtil = new CommonUtil();

                foreach (VeTraceModel d in dataList)
                {
                    OAP0041Model oAP0041Model = new OAP0041Model();
                    ObjectUtil.CopyPropertiesTo(d, oAP0041Model);


                    //取得申請人姓名
                    usr_id = StringUtil.toString(d.update_id);

                    if (!"".Equals(usr_id))
                    {
                        if (!userNameMap.ContainsKey(usr_id))
                        {
                            ADModel adModel = new ADModel();
                            adModel = commonUtil.qryEmp(usr_id);
                            userNameMap.Add(usr_id, adModel);
                        }
                        oAP0041Model.update_name = userNameMap[usr_id].name;
                    }


                    //取得電訪、簡訊人員姓名
                    if ("TEL_DISPATCH".Equals(code_type) || "SMS_DISPATCH".Equals(code_type)) { 

                        usr_id = StringUtil.toString(d.code_id);

                        if (!"".Equals(usr_id))
                        {
                            if (!userNameMap.ContainsKey(usr_id))
                            {
                                ADModel adModel = new ADModel();
                                adModel = commonUtil.qryEmp(usr_id);
                                userNameMap.Add(usr_id, adModel);
                            }
                            oAP0041Model.name = userNameMap[usr_id].name;
                            oAP0041Model.e_mail = userNameMap[usr_id].e_mail;
                        }
                    }

                    rows.Add(oAP0041Model);
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
        /// <param name="code_type"></param>
        /// <param name="recData"></param>
        /// <param name="rtnData"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult execSave(string code_type, List<VeTraceModel> recData, List<VeTraceModel> rtnData)
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

                        }

                        if ("TEL_RANGE".Equals(code_type))
                            fAPVeCodeDao.updByType(code_type, "3", rtnData[0], dt, conn, transaction);
                        else
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

                            if ("TEL_RANGE".Equals(code_type))
                                d.exec_action = "A";

                        }

                        if ("TEL_RANGE".Equals(code_type))
                        {
                            fAPVeCodeDao.delByType(code_type, conn, transaction);
                            fAPVeCodeDao.procAppr("2", dt, recData, conn, transaction);
                            fAPVeCodeHisDao.updateApprMk("2", dt, recData, conn, transaction);

                        }
                        else {
                            fAPVeCodeDao.procAppr("2", dt, recData, conn, transaction);
                            fAPVeCodeHisDao.updateApprMk("2", dt, recData, conn, transaction);
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

    }
}