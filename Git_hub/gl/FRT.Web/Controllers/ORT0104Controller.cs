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
using System.Web.Mvc;


/// <summary>
/// 功能說明：ORT0104 退匯個案處理作業
/// 初版作者：20210114 Daiyu
/// 修改歷程：20210114 Daiyu
/// 需求單號：202011050211
/// 修改內容：初版
/// </summary>
///

namespace FRT.Web.Controllers
{
    [Authorize]
    [CheckSessionFilterAttribute]
    public class ORT0104Controller : BaseController
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
                String[] roleInfo = authUtil.chkUserFuncAuth(Session["UserID"].ToString(), "~/ORT0104/");
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
            ViewBag.failCodeList = fPMCODEDao.loadSelectList("FAIL-CODE", "RT", "", true);



            return View();
        }

        /// <summary>
        /// 查詢
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult qryFRTRVMN(ORT0104Model model)
        {
            logger.Info("qryFRTRVMN begin!!");
            

            try
            {
                if ("".Equals(StringUtil.toString(model.currency)) || "".Equals(StringUtil.toString(model.currency))
                    || "".Equals(StringUtil.toString(model.currency)) || "".Equals(StringUtil.toString(model.currency)))
                    return Json(new { success = false, err = "查詢條件輸入錯誤!!" }, JsonRequestBehavior.AllowGet);


                ORT0104Model _model = new ORT0104Model();

                //自AS400查出資料
                FRTRVMNDao fRTRVMNDao = new FRTRVMNDao();
                _model = fRTRVMNDao.qryForORT0104(model);

                //檢核是否已有覆核中的資料
                FRTRvmnHisDao fRTRvmnHisDao = new FRTRvmnHisDao();
                model.corp_no = StringUtil.toString(model.currency) == "NTD" ? "1" : "3";
                List<FRT_RVMN_HIS> _hisList = fRTRvmnHisDao.qryFor400Key(model.corp_no, model.vhr_no1, model.pro_no, model.paid_id, "1");
                string aply_no = "";
                if (_hisList.Count > 0)
                    aply_no = _hisList[0].aply_no;

                int cnt = 0;
                if (!"".Equals(StringUtil.toString(_model.corp_no)))
                    cnt = 1;

                string content = model.currency + "|" + model.vhr_no1 + "|" + model.pro_no + "|" + model.paid_id;
                writePiaLog(content, cnt, model.paid_id, "Q");

                var jsonData = new { success = true, model = _model, aply_no = aply_no };
                return Json(jsonData, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                logger.Error(e.ToString());
                return Json(new { success = false, err = "其它錯誤，請洽系統管理員!!" }, JsonRequestBehavior.AllowGet);
            }
        }


        private void writePiaLog(string content, int affectRows, string piaOwner, string executionType)
        {
            PIA_LOG_MAIN piaLogMain = new PIA_LOG_MAIN();
            piaLogMain.TRACKING_TYPE = "A";
            piaLogMain.ACCESS_ACCOUNT = Session["UserID"].ToString();
            piaLogMain.ACCOUNT_NAME = Session["UserName"].ToString();
            piaLogMain.PROGFUN_NAME = "ORT0104Controller";
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
        /// 執行"申請覆核"
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult execSave(ORT0104Model model, string fail_code_o, string seqn_o)
        {
            logger.Info("execSave begin");

            string errStr = "";


            try
            {
                if ("".Equals(StringUtil.toString(model.currency)) || "".Equals(StringUtil.toString(model.currency))
                   || "".Equals(StringUtil.toString(model.currency)) || "".Equals(StringUtil.toString(model.currency)))
                    return Json(new { success = false, err = "查詢條件輸入錯誤!!" }, JsonRequestBehavior.AllowGet);
                else {
                    model.currency = StringUtil.toString(model.currency);
                    model.vhr_no1 = StringUtil.toString(model.vhr_no1);
                    model.pro_no = StringUtil.toString(model.pro_no);
                    model.paid_id = StringUtil.toString(model.paid_id);
                }

                ORT0104Model _model = new ORT0104Model();

                //自AS400查出資料
                FRTRVMNDao fRTRVMNDao = new FRTRVMNDao();
                FRTRvmnHisDao fRTRvmnHisDao = new FRTRvmnHisDao();
                _model = fRTRVMNDao.qryForORT0104(model);

                if("".Equals(_model.corp_no))
                    return Json(new { success = false, err = "資料不存在AS400!!" }, JsonRequestBehavior.AllowGet);

                //add by daiyu 20210312 【申請覆核】前檢核該處理序號需存於匯款主檔(MAIN)的PRO_NO(處理序號) 中
                if (!StringUtil.toString(seqn_o).Equals(StringUtil.toString(model.seqn))) {
                    FRTMAINDao fRTMAINDao = new FRTMAINDao();
                    string _pro_no = fRTMAINDao.chkForORT0104(StringUtil.toString(model.seqn));

                    if("".Equals(_pro_no))
                        return Json(new { success = false, err = "新處理序號不存在【匯款主檔(MAIN)】!!" }, JsonRequestBehavior.AllowGet);
                }



                if(StringUtil.toString(_model.fail_code).Equals(StringUtil.toString(model.fail_code))  & StringUtil.toString(_model.seqn).Equals(StringUtil.toString(model.seqn)) )
                    return Json(new { success = false, err = "未修改資料，將不進行覆核申請!!" }, JsonRequestBehavior.AllowGet);


                //檢核是否已有覆核中的資料
                model.corp_no = StringUtil.toString(model.currency) == "NTD" ? "1" : "3";

                List<FRT_RVMN_HIS> _hisList = fRTRvmnHisDao.qryFor400Key(model.corp_no, model.vhr_no1, model.pro_no, model.paid_id, "1");
                if(_hisList.Count > 0)
                    return Json(new { success = false, err = "資料已在覆核中，不可再次進行覆核申請!!" }, JsonRequestBehavior.AllowGet);


                /*------------------ DB處理   begin------------------*/

                string[] curDateTime = DateUtil.getCurChtDateTime(3).Split(' ');
                //取得流水號
                SysSeqDao sysSeqDao = new SysSeqDao();
                string qPreCode = "0104" + (DateTime.Now.Year - 1911).ToString();
                var cId = sysSeqDao.qrySeqNo("RT", "0104", qPreCode).ToString();
                string aply_no = qPreCode + cId.ToString().PadLeft(5, '0');

                _model.aply_no = aply_no;
                _model.fail_code_o = _model.fail_code;
                _model.seqn_o = _model.seqn;

                _model.fail_code = model.fail_code;
                _model.seqn = model.seqn;

                _model.update_id = Session["UserID"].ToString();
                _model.update_datetime = DateUtil.getCurDateTime("yyyy/MM/dd HH:mm:ss");
                _model.appr_datetime = null;
                _model.appr_stat = "1";

                FRT_RVMN_HIS _his = new FRT_RVMN_HIS();
                ObjectUtil.CopyPropertiesTo(_model, _his);

                fRTRvmnHisDao.insert(_his);

                return Json(new { success = true, aplyNo = aply_no, err = errStr });

            }
            catch (Exception e)
            {
                logger.Error(e.ToString());
                return Json(new { success = false, err = "其它錯誤，請洽系統管理員!!" }, JsonRequestBehavior.AllowGet);
            }
        }



        internal class ErrorModel
        {
            public string tempId { get; set; }
            public bool chkResult { get; set; }
            public string msg { get; set; }
        }
    }
}