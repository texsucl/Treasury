using FRT.Web.ActionFilter;
using FRT.Web.AS400Models;
using FRT.Web.BO;
using FRT.Web.Daos;
using FRT.Web.Models;
using FRT.Web.ViewModels;
using System;
using System.Collections.Generic;
using System.Transactions;
using System.Web.Mvc;

/// <summary>
/// 功能說明：查詢電文作業
/// 初版作者：20180918 Daiyu
/// 修改歷程：20180918 Daiyu
///           需求單號：
///           初版
/// ==============================================
/// 修改日期/修改人：20190516 daiyu
/// 需求單號：
/// 修改內容：配合金檢議題，稽核軌跡多加寫HOSTNAME
/// ==============================================
/// </summary>
///

namespace FRT.Web.Controllers
{
    [Authorize]
    [CheckSessionFilterAttribute]
    public class ORTB012Controller : BaseController
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
                String[] roleInfo = authUtil.chkUserFuncAuth(Session["UserID"].ToString(), "~/ORTB012/");
                if (roleInfo != null && roleInfo.Length == 2)
                {
                    opScope = roleInfo[0];
                    funcName = roleInfo[1];
                }
            }

            ViewBag.opScope = opScope;
            ViewBag.funcName = funcName;

            SysCodeDao sysCodeDao = new SysCodeDao();
            //查詢電文種類
            var elecTypeList = sysCodeDao.loadSelectList("RT", "ELEC_TYPE", false);
            ViewBag.elecTypeList = elecTypeList;
            ViewBag.elecTypejqList = sysCodeDao.jqGridList("RT", "ELEC_TYPE", false);

            return View();
        }



        /// <summary>
        /// 查詢設定項目
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult LoadData(string elecType, string actDateB, string actDateE, string fastNoB, string fastNoE)
        {
            logger.Info("LoadData begin!!");

            try
            {
                List<ORTB012Model> rows = new List<ORTB012Model>();

                using (new TransactionScope(
                   TransactionScopeOption.Required,
                   new TransactionOptions
                   {
                       IsolationLevel = IsolationLevel.ReadUncommitted
                   }))
                {
                    using (dbFGLEntities db = new dbFGLEntities())
                    {
                        switch (elecType)
                        {
                            case "EACH_T":
                                rows.AddRange(qryEachT(db, actDateB, actDateE, fastNoB, fastNoE));
                                break;
                            case "EACH_R":
                                rows.AddRange(qryEachR(db, actDateB, actDateE, fastNoB, fastNoE));
                                break;
                            case "XM96_T":
                                rows.AddRange(qryXm96T(db, actDateB, actDateE, fastNoB, fastNoE));
                                break;
                            case "XM96_R":
                                rows.AddRange(qryXm96R(db, actDateB, actDateE, fastNoB, fastNoE));
                                break;
                            case "622685":
                                rows.AddRange(qry622685(db, actDateB, actDateE, fastNoB, fastNoE));
                                break;
                            default:
                                rows.AddRange(qryEachT(db, actDateB, actDateE, fastNoB, fastNoE));
                                rows.AddRange(qryEachR(db, actDateB, actDateE, fastNoB, fastNoE));
                                rows.AddRange(qryXm96T(db, actDateB, actDateE, fastNoB, fastNoE));
                                rows.AddRange(qryXm96R(db, actDateB, actDateE, fastNoB, fastNoE));
                                rows.AddRange(qry622685(db, actDateB, actDateE, fastNoB, fastNoE));
                                break;
                        }
                    }
                }

                FRTBERMDao fRTBERMDao = new FRTBERMDao();
                List<FRTBERMModel> errList = fRTBERMDao.qryFRTBERM("", "", "");
                foreach (ORTB012Model d in rows) {
                    if (string.IsNullOrEmpty(d.emsgId))
                    {
                        FRTBERMModel err = errList.Find(x => x.errCode == d.errorCode);
                        if (err != null)
                        {
                            d.errorCode += StringUtil.toString(err.errDesc);
                        }
                    }
                    else {
                        d.errorCode = StringUtil.toString(d.emsgId) + ":" + StringUtil.toString(d.emsgTxt);
                    }

                    string updTime = d.updTime?.Replace("/", "").Replace(":", "");
                    if (!string.IsNullOrWhiteSpace(updTime))
                        d.crtTime = d.updTime;
                }

                /*---add by daiyu 20190516---*/
                PIA_LOG_MAIN piaLogMain = new PIA_LOG_MAIN();
                piaLogMain.TRACKING_TYPE = "A";
                piaLogMain.ACCESS_ACCOUNT = Session["UserID"].ToString();
                piaLogMain.ACCOUNT_NAME = Session["UserName"].ToString();
                piaLogMain.PROGFUN_NAME = "ORTB012";
                piaLogMain.EXECUTION_CONTENT = "elecType:" + elecType + "|" + "actDateB:" + actDateB + "|" 
                    + "actDateE:" + actDateE + "|" + "fastNoB:" + fastNoB + "|" + "fastNoE:" + fastNoE;
                piaLogMain.AFFECT_ROWS = rows.Count;
                piaLogMain.PIA_TYPE = "1000100000";
                piaLogMain.EXECUTION_TYPE = "Q";

                switch (elecType)
                {
                    case "EACH_T":
                        piaLogMain.ACCESSOBJ_NAME = "FRT_XML_T_eACH";
                        break;
                    case "EACH_R":
                        piaLogMain.ACCESSOBJ_NAME = "FRT_XML_R_eACH";
                        break;
                    case "XM96_T":
                        piaLogMain.ACCESSOBJ_NAME = "FRT_XML_T_622823；FRT_XML_T_622821";
                        break;
                    case "XM96_R":
                        piaLogMain.ACCESSOBJ_NAME = "FRT_XML_R_622823；FRT_XML_R_622821";
                        break;
                    case "622685":
                        piaLogMain.ACCESSOBJ_NAME = "FRT_XML_R_622685_NEW；FRT_XML_R_622685；FRT_XML_522657";
                        break;
                    default:
                        piaLogMain.ACCESSOBJ_NAME = "FRT_XML_T_eACH；FRT_XML_R_eACH；FRT_XML_T_622823；FRT_XML_T_622821；FRT_XML_R_622823；FRT_XML_R_622821；FRT_XML_R_622685_NEW；FRT_XML_R_622685；FRT_XML_522657";
                        break;
                }

                
                PiaLogMainDao piaLogMainDao = new PiaLogMainDao();
                piaLogMainDao.Insert(piaLogMain);
                /*---end add 20190516---*/

                var jsonData = new { success = true,  rows };
                return Json(jsonData, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e) {
                logger.Error(e.ToString());
                return Json(new { success = false, err = "其它錯誤，請洽系統管理員!!" }, JsonRequestBehavior.AllowGet);
            }

            
        }


        /// <summary>
        /// 匯出電文-EACH
        /// </summary>
        /// <param name="rows"></param>
        /// <param name="actDateB"></param>
        /// <param name="actDateE"></param>
        /// <param name="fastNoB"></param>
        /// <param name="fastNoE"></param>
        private List<ORTB012Model> qryEachT(dbFGLEntities db, string actDateB, string actDateE, string fastNoB, string fastNoE) {
            List<ORTB012Model> rows = new List<ORTB012Model>();

            FRTXmlTEachDao fRTXmlTEachDao = new FRTXmlTEachDao();

            rows = fRTXmlTEachDao.qryForORTB012(db, actDateB, actDateE, fastNoB, fastNoE);

            return rows;

        }


        /// <summary>
        /// 回饋電文- EACH
        /// </summary>
        /// <param name="rows"></param>
        /// <param name="actDateB"></param>
        /// <param name="actDateE"></param>
        /// <param name="fastNoB"></param>
        /// <param name="fastNoE"></param>
        private List<ORTB012Model> qryEachR(dbFGLEntities db, string actDateB, string actDateE, string fastNoB, string fastNoE)
        {
            List<ORTB012Model> rows = new List<ORTB012Model>();

            FRTXmlREachDao fRTXmlREachDao = new FRTXmlREachDao();

            rows = fRTXmlREachDao.qryForORTB012(db, actDateB, actDateE, fastNoB, fastNoE);

            return rows;
        }


        /// <summary>
        /// 匯出電文-金資(XM96)
        /// </summary>
        /// <param name="rows"></param>
        /// <param name="actDateB"></param>
        /// <param name="actDateE"></param>
        /// <param name="fastNoB"></param>
        /// <param name="fastNoE"></param>
        private List<ORTB012Model> qryXm96T(dbFGLEntities db, string actDateB, string actDateE, string fastNoB, string fastNoE)
        {
            List<ORTB012Model> rows = new List<ORTB012Model>();

            FRTXmlT622821Dao fRTXmlT622821Dao = new FRTXmlT622821Dao();

            rows = fRTXmlT622821Dao.qryForORTB012(db, actDateB, actDateE, fastNoB, fastNoE);
            

            return rows;
        }


        /// <summary>
        /// 回饋電文-金資(XM96)
        /// </summary>
        /// <param name="rows"></param>
        /// <param name="actDateB"></param>
        /// <param name="actDateE"></param>
        /// <param name="fastNoB"></param>
        /// <param name="fastNoE"></param>
        private List<ORTB012Model> qryXm96R(dbFGLEntities db, string actDateB, string actDateE, string fastNoB, string fastNoE)
        {
            List<ORTB012Model> rows = new List<ORTB012Model>();

            FRTXmlR622821Dao fRTXmlR622821Dao = new FRTXmlR622821Dao();

            rows = fRTXmlR622821Dao.qryForORTB012(db, actDateB, actDateE, fastNoB, fastNoE);


            return rows;
        }


        /// <summary>
        /// 回饋電文-622685+522657
        /// </summary>
        /// <param name="rows"></param>
        /// <param name="actDateB"></param>
        /// <param name="actDateE"></param>
        /// <param name="fastNoB"></param>
        /// <param name="fastNoE"></param>
        private List<ORTB012Model> qry622685(dbFGLEntities db, string actDateB, string actDateE, string fastNoB, string fastNoE)
        {
            FRTXmlR622685Dao fRTXmlR622685Dao = new FRTXmlR622685Dao();
            List<ORTB012Model> rows = new List<ORTB012Model>();

            rows = fRTXmlR622685Dao.qryForORTB012(db, actDateB, actDateE, fastNoB, fastNoE);

            return rows;
        }
    }
}