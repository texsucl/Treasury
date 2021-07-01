using FGL.Web.ActionFilter;
using FGL.Web.BO;
using FGL.Web.Daos;
using FGL.Web.Models;
using FGL.Web.ViewModels;
using System;
using System.Collections.Generic;
using System.Web.Mvc;

/// <summary>
/// 功能說明：科目設定查詢作業
/// 初版作者：20190320 Daiyu
/// 修改歷程：20190320 Daiyu
///           需求單號：201805080167-00
///           初版
/// </summary>
///

namespace FGL.Web.Controllers
{
    [Authorize]
    [CheckSessionFilterAttribute]
    public class OGL00008Controller : BaseController
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
            string[] roleInfo = authUtil.chkUserFuncAuth(Session["UserID"].ToString(), "~/OGL00008/");
            if (roleInfo != null && roleInfo.Length == 2)
            {
                opScope = roleInfo[0];
                funcName = roleInfo[1];
            }

            ViewBag.funcName = funcName;
            ViewBag.opScope = opScope;

            SysCodeDao sysCodeDao = new SysCodeDao();

            //險種類別
            var productTypeList = sysCodeDao.loadSelectList("GL", "PRODUCT_TYPE", true);
            ViewBag.productTypeList = productTypeList;

            //Y/N
            var ynList = sysCodeDao.loadSelectList("GL", "YN_FLAG", true);
            ViewBag.ynList = ynList;

            SysParaDao SysParaDao = new SysParaDao();

            //會科編碼類別
            var rule56List = SysParaDao.qrySmpRule56List();
            ViewBag.rule56List = rule56List;

            return View();
        }


        /// <summary>
        /// 查詢待輸入的資料
        /// </summary>
        /// <param name="cReviewType"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult LoadData(string isQryTmp, string productType, string rule56, string fuMk
            , string effectDateB, string effectDateE)
        {
            List<OGL00008Model> rows = new List<OGL00008Model>();

            
            FGLItemInfoHisDao fGLItemInfoHisDao = new FGLItemInfoHisDao();
            FGLItemInfoDao fGLItemInfoDao = new FGLItemInfoDao();

            rows.AddRange(fGLItemInfoHisDao.qryForOGL00008(productType, rule56, fuMk, effectDateB, effectDateE));
            rows.AddRange(fGLItemInfoDao.qryForOGL00008(productType, rule56, fuMk, effectDateB, effectDateE));

            //if ("1".Equals(isQryTmp))   //查詢"會計商品資訊檔暫存檔"
            //{
            //    FGLItemInfoHisDao fGLItemInfoHisDao = new FGLItemInfoHisDao();
            //    rows = fGLItemInfoHisDao.qryForOGL00008(productType, rule56, fuMk, effectDateB, effectDateE);
            //}
            //else
            //{  //查詢"會計商品資訊"
            //    FGLItemInfoDao fGLItemInfoDao = new FGLItemInfoDao();
            //    rows = fGLItemInfoDao.qryForOGL00008(productType, rule56, fuMk, effectDateB, effectDateE);
            //}
            


            var jsonData = new { success = true, dataList = rows };
            return Json(jsonData, JsonRequestBehavior.AllowGet);

        }
        

        
    }
}