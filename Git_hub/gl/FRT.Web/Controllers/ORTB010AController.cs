using FRT.Web.ActionFilter;
using FRT.Web.BO;
using FRT.Web.Daos;
using FRT.Web.ViewModels;
using System;
using System.Collections.Generic;
using System.Web.Mvc;
using System.Text;
using System.Linq;
using System.Data.SqlClient;
using FRT.Web.Models;

/// <summary>
/// 功能說明：發查時間設定、水位設定作業
/// 初版作者：20180910 Daiyu
/// 修改歷程：20180910 Daiyu
///           需求單號：201807190487
///           初版
/// </summary>
///

namespace FRT.Web.Controllers
{
    [Authorize]
    [CheckSessionFilterAttribute]
    public class ORTB010AController : BaseController
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
                String[] roleInfo = authUtil.chkUserFuncAuth(Session["UserID"].ToString(), "~/ORTB010A/");
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
        /// <param name="fastNo"></param>
        /// <param name="paidId"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult LoadData()
        {
            try {
                SysParaHisDao sysParaHisDao = new SysParaHisDao();
                List<ORTB010Model> onApprList = sysParaHisDao.qryForORTB010("1");

                List<ORTB010Model> rows = onApprList.GroupBy(x => new { x.aplyNo, x.grpId, x.grpDesc, x.createUid, x.createDt })
                    .Select(group => new ORTB010Model
                    {
                    aplyNo = group.Key.aplyNo,
                    grpId = group.Key.grpId,
                    grpDesc = group.Key.grpDesc,
                    createUid = group.Key.createUid,
                    createDt = group.Key.createDt
                }).ToList<ORTB010Model>();

                foreach (ORTB010Model aplyModel in rows) {
                    string desc = "";

                    foreach (ORTB010Model d in onApprList.Where(x => x.aplyNo == aplyModel.aplyNo))
                    {
                        desc += d.remark + "：" + d.paraValue + "<br/>";
                    }

                    aplyModel.paraValue = desc;
                }


                var jsonData = new { success = true, rows  };
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
        public JsonResult execSave(List<ORTB010Model> recData, List<ORTB010Model> rtnData)
        {

            List<SYS_PARA_HIS> rtnDataList = new List<SYS_PARA_HIS>();
            List<SYS_PARA_HIS> recDataList = new List<SYS_PARA_HIS>();
            List<SYS_PARA> recDataMList = new List<SYS_PARA>();

            string strConn = DbUtil.GetDBFglConnStr();
            using (SqlConnection conn = new SqlConnection(strConn))
            {
                conn.Open();
                SqlTransaction transaction = conn.BeginTransaction("Transaction");

                SysParaHisDao sysParaHisDao = new SysParaHisDao();
                try
                {
                    //處理駁回資料
                    if (rtnData.Count > 0)
                    {
                        foreach (ORTB010Model d in rtnData)
                        {
                            if (d.createUid.Equals(Session["UserID"].ToString()))
                                return Json(new { success = false, err = "申請人與覆核人員不可相同!!" }, JsonRequestBehavior.AllowGet);

                            SYS_PARA_HIS hisD = new SYS_PARA_HIS();
                            hisD.SYS_CD = "RT";
                            hisD.APLY_NO = d.aplyNo;
                            hisD.GRP_ID = d.grpId;

                            rtnDataList.Add(hisD);

                        }
                        sysParaHisDao.updateApprStatus(Session["UserID"].ToString(), "3", rtnDataList, conn, transaction);
                    }


                    //處理核可資料
                    if (recData.Count > 0)
                    {
                        SysParaDao sysParaDao = new SysParaDao();
                        foreach (ORTB010Model d in recData)
                        {
                            if (d.createUid.Equals(Session["UserID"].ToString()))
                                return Json(new { success = false, err = "申請人與覆核人員不可相同!!" }, JsonRequestBehavior.AllowGet);

                            SYS_PARA_HIS hisD = new SYS_PARA_HIS();
                            hisD.SYS_CD = "RT";
                            hisD.APLY_NO = d.aplyNo;
                            hisD.GRP_ID = d.grpId;

                            recDataList.Add(hisD);

                            List<SYS_PARA_HIS> detailH = sysParaHisDao.qryByAplyNo("RT", d.grpId, d.aplyNo);

                            foreach (SYS_PARA_HIS detail in detailH) {
                                SYS_PARA dMain = new SYS_PARA();
                                dMain.SYS_CD = detail.SYS_CD;
                                dMain.GRP_ID = detail.GRP_ID;
                                dMain.PARA_ID = detail.PARA_ID;
                                dMain.PARA_VALUE = detail.PARA_VALUE;

                                if ("FAST_API".Equals(detail.PARA_ID))
                                {
                                    FRTCODEDao FRTCODEDao = new FRTCODEDao();
                                    FRTCODEDao.updateFBApi(detail.PARA_VALUE);
                                }
                                else {
                                    recDataMList.Add(dMain);
                                }

                                  
                                

                            }
                        }
                        sysParaHisDao.updateApprStatus(Session["UserID"].ToString(), "2", recDataList, conn, transaction);
                        
                        sysParaDao.updateParaVale(recDataMList, conn, transaction);
                    }


                    transaction.Commit();
                    return Json(new { success = true });
                }
                catch (Exception e)
                {
                    transaction.Rollback();

                    logger.Error("[execSave]其它錯誤：" + e.ToString());

                    return Json(new { success = false, err = "其它錯誤，請洽系統管理員!!" }, JsonRequestBehavior.AllowGet);

                }
            }
        }
        
    }
}