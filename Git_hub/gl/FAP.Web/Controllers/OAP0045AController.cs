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
/// 功能說明：OAP0045A 追蹤標準設定覆核作業
/// 初版作者：20200904 Daiyu
/// 修改歷程：20200904 Daiyu
/// 需求單號：202008120153-00
/// 初版
/// </summary>
///

namespace FAP.Web.Controllers
{
    [Authorize]
    [CheckSessionFilterAttribute]
    public class OAP0045AController : BaseController
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
                String[] roleInfo = authUtil.chkUserFuncAuth(Session["UserID"].ToString(), "~/OAP0045A/");
                if (roleInfo != null && roleInfo.Length == 2)
                {
                    opScope = roleInfo[0];
                    funcName = roleInfo[1];
                }
            }

            ViewBag.opScope = opScope;
            ViewBag.funcName = funcName;

            SysCodeDao sysCodeDao = new SysCodeDao();
            ViewBag.codeTypeList = sysCodeDao.loadSelectList("AP", "OAP0045_TYPE", false);

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
            FAPTelCodeHisDao fAPTelCodeHisDao = new FAPTelCodeHisDao();

            try {
                List<OAP0045Model> dataList = fAPTelCodeHisDao.qryByOAP0045A(new string[] { code_type }).ToList();
                List<OAP0045Model> rows = new List<OAP0045Model>();


                Dictionary<string, ADModel> userNameMap = new Dictionary<string, ADModel>();
                string usr_id = "";
                CommonUtil commonUtil = new CommonUtil();

                foreach (OAP0045Model d in dataList)
                {
                    OAP0045Model oAP0045Model = new OAP0045Model();
                    ObjectUtil.CopyPropertiesTo(d, oAP0045Model);


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
                        oAP0045Model.update_name = userNameMap[usr_id].name;
                    }


                    //取得處理人員姓名
                    usr_id = StringUtil.toString(d.proc_id);

                    if (!"".Equals(usr_id))
                    {
                        if (!userNameMap.ContainsKey(usr_id))
                        {
                            ADModel adModel = new ADModel();
                            adModel = commonUtil.qryEmp(usr_id);
                            userNameMap.Add(usr_id, adModel);
                        }
                        oAP0045Model.proc_name = userNameMap[usr_id].name;
                    }


                    rows.Add(oAP0045Model);
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
        public JsonResult execSave(string code_type, List<OAP0045Model> recData, List<OAP0045Model> rtnData)
        {
            string strConn = DbUtil.GetDBFglConnStr();
            using (SqlConnection conn = new SqlConnection(strConn))
            {
                conn.Open();

                SqlTransaction transaction = conn.BeginTransaction("Transaction");

                try
                {
                    DateTime dt = DateTime.Now;
                    FAPTelCodeHisDao fAPTelCodeHisDao = new FAPTelCodeHisDao();
                    FAPTelCodeDao fAPTelCodeDao = new FAPTelCodeDao();


                    //處理駁回資料
                    if (rtnData.Count > 0)
                    {
                        foreach (OAP0045Model d in rtnData)
                        {
                            if (d.update_id.Equals(Session["UserID"].ToString()))
                                return Json(new { success = false, err = "申請人與覆核人員不可相同!!" }, JsonRequestBehavior.AllowGet);

                            d.appr_id = Session["UserID"].ToString();
                            
                            //異動歷史檔
                            fAPTelCodeHisDao.updateApprStatus(Session["UserID"].ToString(), "3", d.aply_no, d.code_type, d.code_id, dt, conn, transaction);


                            //將正式檔的資料狀態改為可異動
                            FAP_TEL_CODE m = new FAP_TEL_CODE();
                            m.code_type = d.code_type;
                            m.code_id = d.code_id;
                            m.update_id = d.update_id;
                            m.appr_id = d.appr_id;
                            fAPTelCodeDao.updateStatus("3", dt, m, conn, transaction);
                        }

                    }



                    //處理核可資料
                    if (recData.Count > 0)
                    {
                        
                        foreach (OAP0045Model d in recData)
                        {
                            if (d.update_id.Equals(Session["UserID"].ToString()))
                                return Json(new { success = false, err = "申請人與覆核人員不可相同!!" }, JsonRequestBehavior.AllowGet);

                            d.appr_id = Session["UserID"].ToString();

                            //異動歷史檔
                            fAPTelCodeHisDao.updateApprStatus(Session["UserID"].ToString(), "2", d.aply_no, d.code_type, d.code_id, dt, conn, transaction);

                            //將正式檔的資料狀態改為可異動
                            FAP_TEL_CODE_HIS his = fAPTelCodeHisDao.qryByKey(d.code_type, d.code_id, d.aply_no, "");

                            FAP_TEL_CODE m = new FAP_TEL_CODE();
                            ObjectUtil.CopyPropertiesTo(his, m);
                            fAPTelCodeDao.updateStatus("2", dt, m, conn, transaction);

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