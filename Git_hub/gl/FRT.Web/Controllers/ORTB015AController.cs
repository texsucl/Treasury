using FRT.Web.ActionFilter;
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
/// 功能說明：快速付款寄送EMAIL對象的TABLE檔覆核作業
/// 初版作者：20181205 Daiyu
/// 修改歷程：20181205 Daiyu
///           需求單號：201811300566
///           初版
/// </summary>
///

namespace FRT.Web.Controllers
{
    [Authorize]
    [CheckSessionFilterAttribute]
    public class ORTB015AController : BaseController
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
                String[] roleInfo = authUtil.chkUserFuncAuth(Session["UserID"].ToString(), "~/ORTB015A/");
                if (roleInfo != null && roleInfo.Length == 2)
                {
                    opScope = roleInfo[0];
                    funcName = roleInfo[1];
                }
            }

            ViewBag.opScope = opScope;
            ViewBag.funcName = funcName;

            SysCodeDao sysCodeDao = new SysCodeDao();
            //資料狀態
            ViewBag.statusjqList = sysCodeDao.jqGridList("RT", "STATUS", false);

            //可設定的MAIL群組
            var mailGroupList = sysCodeDao.loadSelectList("RT", "MAIL_GROUP", true);
            ViewBag.mailGroupList = mailGroupList;
            ViewBag.mailGroupjqList = sysCodeDao.jqGridList("RT", "MAIL_GROUP", false);

            //設定類型
            ViewBag.empTypejqList = sysCodeDao.jqGridList("RT", "MAIL_EMP_TYPE", false);

            //Y/N
            ViewBag.ynjqList = sysCodeDao.jqGridList("SSO", "YN_FLAG", false);


            return View();
        }


        /// <summary>
        /// 查詢待覆核的資料
        /// </summary>
        /// <param name="cReviewType"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult LoadData(string groupCode)
        {
            FRTMailNotifyHisDao fRTMailNotifyHisDao = new FRTMailNotifyHisDao();

            try {
                List<ORTB015Model> rows = fRTMailNotifyHisDao.qryForSTAT(groupCode, "", "1");


                if (rows.Count > 0)
                {
                    //查DB_INTRA取得mail人員姓名
                    using (DB_INTRAEntities dbIntra = new DB_INTRAEntities())
                    {
                        Dictionary<string, string> userNameMap = new Dictionary<string, string>();
                        OaEmpDao oaEmpDao = new OaEmpDao();
                        string uId = "";

                        string updId = "";
                        string apprId = "";

                        foreach (ORTB015Model d in rows.Where(x => x.empType == "U"))
                        {
                            uId = StringUtil.toString(d.receiverEmpno);

                            if (!"".Equals(uId))
                            {
                                if (!userNameMap.ContainsKey(uId))
                                {
                                    userNameMap = oaEmpDao.qryUsrName(userNameMap, uId, dbIntra);
                                }
                                d.receiverEmpDesc = userNameMap[uId];
                            }
                        }

                        foreach (ORTB015Model d in rows)
                        {
                            updId = StringUtil.toString(d.updId);

                            if (!"".Equals(updId))
                            {
                                if (!userNameMap.ContainsKey(updId))
                                {
                                    userNameMap = oaEmpDao.qryUsrName(userNameMap, updId, dbIntra);
                                }
                                d.updateUName = userNameMap[updId];
                            }
                        }
                    }

                    //取得角色名稱
                    Dictionary<string, string> roleNameMap = new Dictionary<string, string>();
                    CodeRoleDao codeRoleDao = new CodeRoleDao();
                    foreach (ORTB015Model d in rows.Where(x => x.empType == "R"))
                    {
                        if (!roleNameMap.ContainsKey(d.receiverEmpno))
                        {
                            CODE_ROLE role = codeRoleDao.qryRoleByKey(d.receiverEmpno);

                            if (role != null)
                                roleNameMap.Add(d.receiverEmpno, StringUtil.toString(role.ROLE_NAME));
                            else
                                roleNameMap.Add(d.receiverEmpno, "");
                        }

                        d.receiverEmpDesc = roleNameMap[d.receiverEmpno];
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
        public JsonResult execSave(List<ORTB015Model> recData, List<ORTB015Model> rtnData)
        {

            string strConn = DbUtil.GetDBFglConnStr();
            using (SqlConnection conn = new SqlConnection(strConn)) { 
                conn.Open();

                SqlTransaction transaction = conn.BeginTransaction("Transaction");

                try
                {
                    FRTMailNotifyHisDao fRTMailNotifyHisDao = new FRTMailNotifyHisDao();
                    FRTMailNotifyDao fRTMailNotifyDao = new FRTMailNotifyDao();

                    //處理駁回資料
                    if (rtnData.Count > 0) {
                        foreach (ORTB015Model d in rtnData)
                        {
                            if (d.updId.Equals(Session["UserID"].ToString()))
                                return Json(new { success = false, err = "申請人與覆核人員不可相同!!" }, JsonRequestBehavior.AllowGet);
                        }
                        fRTMailNotifyHisDao.updateStat(Session["UserID"].ToString(), "3", rtnData, conn, transaction);

                        List<FRT_MAIL_NOTIFY> dataFormalList = new List<FRT_MAIL_NOTIFY>();
                        foreach (ORTB015Model d in rtnData)
                        {
                            if (!"A".Equals(d.status))
                            {
                                FRT_MAIL_NOTIFY formal = new FRT_MAIL_NOTIFY();
                                formal.GROUP_CODE = d.groupCode;
                                formal.RECEIVER_EMPNO = d.receiverEmpno;
                                formal.DATA_STATUS = "1";
                                //formal.UPDATE_ID = d.updId;
                                //formal.UPDATE_DATETIME = DateUtil.stringToDatetime(d.updDatetime);
                                //formal.UPDATE_ID = Session["UserID"].ToString();
                                //formal.UPDATE_DATETIME = DateTime.Now;
                                dataFormalList.Add(formal);
                            }
                        }

                        fRTMailNotifyDao.updateStatus("1", dataFormalList, conn, transaction);
                    }
                        


                    //處理核可資料
                    if (recData.Count > 0) {
                        
                        foreach (ORTB015Model d in recData)
                        {
                            if (d.updId.Equals(Session["UserID"].ToString()))
                                return Json(new { success = false, err = "申請人與覆核人員不可相同!!" }, JsonRequestBehavior.AllowGet);
                        }
                        fRTMailNotifyHisDao.updateStat(Session["UserID"].ToString(), "2", recData, conn, transaction);
                        fRTMailNotifyDao.appr(Session["UserID"].ToString(), recData, conn, transaction);
                    }

                    transaction.Commit();
                    return Json(new { success = true });
                }
                catch (Exception e) {
                    transaction.Rollback();

                    logger.Error("[execSave]其它錯誤：" + e.ToString());

                    return Json(new { success = false, err = "其它錯誤，請洽系統管理員!!" }, JsonRequestBehavior.AllowGet);
                }

            }
        }
    }
}