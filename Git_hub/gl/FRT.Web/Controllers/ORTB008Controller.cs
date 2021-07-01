using FRT.Web.ActionFilter;
using FRT.Web.AS400Models;
using FRT.Web.BO;
using FRT.Web.Daos;
using FRT.Web.ViewModels;
using System;
using System.Collections.Generic;
using System.Web.Mvc;
using System.Net;
using System.Net.Sockets;
using System.Text;

/// <summary>
/// 功能說明：MAIL寄送紀錄查詢
/// 初版作者：20180827 Daiyu
/// 修改歷程：20180827 Daiyu
///           需求單號：201807190487
///           初版
/// </summary>
///

namespace FRT.Web.Controllers
{
    [Authorize]
    [CheckSessionFilterAttribute]
    public class ORTB008Controller : BaseController
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
                String[] roleInfo = authUtil.chkUserFuncAuth(Session["UserID"].ToString(), "~/ORTB008/");
                if (roleInfo != null && roleInfo.Length == 2)
                {
                    opScope = roleInfo[0];
                    funcName = roleInfo[1];
                }
            }

            ViewBag.opScope = opScope;
            ViewBag.funcName = funcName;


            //MAIL寄送結果
            SysCodeDao sysCodeDao = new SysCodeDao();
            var mailResultList = sysCodeDao.loadSelectList("RT", "MAIL_RESULT", true);
            ViewBag.mailResultList = mailResultList;
            ViewBag.mailResultjqList = sysCodeDao.jqGridList("RT", "MAIL_RESULT", true);
 
            return View();
        }


/// <summary>
/// 查詢MAIL紀錄
/// </summary>
/// <param name="qDateB"></param>
/// <param name="qDateE"></param>
/// <param name="receiverId"></param>
/// <param name="mailResult"></param>
/// <returns></returns>
        [HttpPost]
        public JsonResult loadData(string qDateB, string qDateE, string receiverEmpno, string mailResult)
        {
            logger.Info("loadData begin!!");

            try
            {
                FRTMailLogDao fRTMailLogDao = new FRTMailLogDao();
                List<ORTB008Model> rows = fRTMailLogDao.qryForORTB008(qDateB, qDateE, receiverEmpno, mailResult);


                var jsonData = new { success = true, dataList = rows };
                return Json(jsonData, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e) {
                logger.Error(e.ToString());
                return Json(new { success = false, err = "其它錯誤，請洽系統管理員!!" }, JsonRequestBehavior.AllowGet);
            }

            
        }

        

    }
}