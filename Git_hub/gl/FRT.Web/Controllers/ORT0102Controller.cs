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
/// 功能說明：ORT0102 非保費類退匯給付方式維護
/// 初版作者：20201211 Daiyu
/// 修改歷程：20201211 Daiyu
/// 需求單號：
/// 修改內容：初版
/// </summary>
///

namespace FRT.Web.Controllers
{
    [Authorize]
    [CheckSessionFilterAttribute]
    public class ORT0102Controller : BaseController
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
                String[] roleInfo = authUtil.chkUserFuncAuth(Session["UserID"].ToString(), "~/ORT0102/");
                if (roleInfo != null && roleInfo.Length == 2)
                {
                    opScope = roleInfo[0];
                    funcName = roleInfo[1];
                }
            }

            ViewBag.opScope = opScope;
            ViewBag.funcName = funcName;

            SysCodeDao sysCodeDao = new SysCodeDao();
            ViewBag.codeTypeList = sysCodeDao.loadSelectList("RT", "ORT0102", false);

            //執行功能
            ViewBag.execActionjqList = sysCodeDao.jqGridList("RT", "STATUS", true);

            //資料狀態
            ViewBag.statusjqList = sysCodeDao.jqGridList("RT", "DATA_STATUS", true);

            return View();
        }

        /// <summary>
        /// 查詢
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult qryFRTCODE(string ptype)
        {
            logger.Info("qryFRTCODE begin!!");

            try
            {
                //自AS400查出資料
                FRTCODEDao fRTCODEDao = new FRTCODEDao();
                List<FRTCODEModel> as400_dataList = new List<FRTCODEModel>();
                as400_dataList = fRTCODEDao.qryRTCode("RT", ptype, "");

                FRTCodeHisDao fRTCodeHisDao = new FRTCodeHisDao();

                List<ORT0102Model> dataList = new List<ORT0102Model>();


                //拆解RT-CODE資料，並判斷是否可維護
                foreach (FRTCODEModel d in as400_dataList) {
                    ORT0102Model model = new ORT0102Model();
                    model.ptype = d.groupId;
                    model.group_id = d.groupId;
                    string _ref_no = d.refNo.PadRight(6, ' ');
                    model.tempId = d.refNo;
                    model.system = _ref_no.Substring(0, 1);
                    model.srce_from = _ref_no.Substring(1, 3);
                    model.srce_kind = _ref_no.Substring(4, 2);

                    FRT_CODE_HIS _his = new FRT_CODE_HIS();
                    _his = fRTCodeHisDao.chkOnAply("RT", model.ptype, d.refNo);

                    if (_his != null)
                        model.data_status = "2";
                    else
                        model.data_status = "1";

                    model.update_id = string.IsNullOrWhiteSpace(d.updId) == true ? "" : d.updId;
                    model.update_datetime = string.IsNullOrWhiteSpace(d.updDate) == true ? "" : d.updDate;
                    model.appr_id = string.IsNullOrWhiteSpace(d.apprId) == true ? "" : d.apprId;
                    model.approve_datetime = string.IsNullOrWhiteSpace(d.apprDate) == true ? "" : d.apprDate;

                    dataList.Add(model);
                }

                var jsonData = new { success = true, dataList = dataList };
                return Json(jsonData, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                logger.Error(e.ToString());
                return Json(new { success = false, err = "其它錯誤，請洽系統管理員!!" }, JsonRequestBehavior.AllowGet);
            }
        }



        [HttpPost]
        public JsonResult chkData(string iTempId, string ptype, string system, string srce_from, string srce_kind, string exec_action)
        {
            ErrorModel errModel = new ErrorModel();
            errModel = chkAplyData(iTempId, ptype, system, srce_from, srce_kind, exec_action);

            if (errModel.chkResult)
                return Json(new { success = true });
            else
                return Json(new { success = false, err = errModel.msg });
        }


        ///檢查資料是否已存在主檔或暫存檔
        /// <returns></returns>
        private ErrorModel chkAplyData(string iTempId, string ptype, string system, string srce_from, string srce_kind, string exec_action)
        {

            string tempIdN = StringUtil.toString(system) + StringUtil.toString(srce_from).PadRight(3, ' ') + StringUtil.toString(srce_kind).PadRight(2, ' ');
            ErrorModel errModel = new ErrorModel();
            FRTCODEDao fRTCODEDao = new FRTCODEDao();
            FRTCodeHisDao fRTCodeHisDao = new FRTCodeHisDao();

            if ("A".Equals(exec_action))
            {
                //
                
                List<FRTCODEModel> as400_data = fRTCODEDao.qryRTCode("RT", ptype, tempIdN);

                if (as400_data.Count > 0) {
                    errModel.chkResult = false;
                    errModel.msg = "此筆資料已存在不可新增!!";
                    return errModel;
                }

            }


            FRT_CODE_HIS _his = fRTCodeHisDao.chkOnAply("RT", ptype, StringUtil.toString(tempIdN));
            if (_his != null)
            {
                errModel.chkResult = false;
                errModel.msg = "資料覆核中，不可異動此筆資料!!";
                return errModel;
            }



            errModel.chkResult = true;
            errModel.msg = "";
            return errModel;
        }



        /// <summary>
        /// 執行"申請覆核"
        /// </summary>
        /// <param name="gridData"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult execSave(List<ORT0102Model> gridData)
        {
            logger.Info("execSave begin");

            string errStr = "";
            bool bChg = false;

            List<ORT0102Model> rptList = new List<ORT0102Model>();

            try
            {

                SysCodeDao sysCodeDao = new SysCodeDao();

                //執行功能
                Dictionary<string, string> execActionMap = sysCodeDao.qryByTypeDic("RT", "EXEC_ACTION");

                List<FRT_CODE_HIS> dataList = new List<FRT_CODE_HIS>();

                if (gridData == null)
                {
                    return Json(new { success = false, err = "未異動畫面資料，將不進行修改覆核作業!!" }, JsonRequestBehavior.AllowGet);
                }

                string now = DateUtil.getCurDateTime("yyyy/MM/dd HH:mm:ss");
                DateTime dt = DateTime.Now;


                foreach (ORT0102Model d in gridData)
                {

                    if (!"".Equals(StringUtil.toString(d.exec_action)))
                    {
                        ErrorModel errModel = chkAplyData(d.tempId, d.group_id, d.system, d.srce_from, d.srce_kind, d.exec_action);

                        if (errModel.chkResult)
                        {
                            bChg = true;
                            d.update_id = Session["UserID"].ToString();
                            d.update_datetime = now;
                            //d.approve_datetime = null;


                            FRT_CODE_HIS his = new FRT_CODE_HIS();
                            his.GROUP_ID = d.group_id;
                            his.TEXT_LEN = 70;
                            his.REF_NO = d.system 
                                + StringUtil.toString(d.srce_from).PadRight(3, ' ')
                                + StringUtil.toString(d.srce_kind).PadRight(2, ' ');


                            switch (d.group_id) {
                                case "5910-PTYPE":
                                    his.TEXT = "PRT5910";
                                    break;
                                case "5920-PTYPE":
                                    his.TEXT = "PRT5920";
                                    break;
                                case "5930-PTYPE":
                                    his.TEXT = "PRT5930";
                                    break;
                                case "5940-PTYPE":
                                    his.TEXT = "PRT5940";
                                    break;
                                case "5950-PTYPE":
                                    his.TEXT = "PRT5950";
                                    break;

                            }

                            his.SRCE_FROM = "RT";
                            his.APPR_STAT = "1";
                            his.STATUS = d.exec_action;
                            his.UPDATE_DATETIME = dt;
                            his.UPDATE_ID = Session["UserID"].ToString();

                            dataList.Add(his);


                        }
                        else
                        {
                            errStr += "系統別：" + d.system + " 資料來源：" + d.srce_from + " 資料類別：" + d.srce_kind + "<br/>";
                        }
                    }
                }

                if (bChg == false)
                {
                    if ("".Equals(errStr))
                        return Json(new { success = false, err = "未異動畫面資料，將不進行修改覆核作業!!" }, JsonRequestBehavior.AllowGet);
                    else
                        return Json(new { success = true, err = errStr });
                }


                /*------------------ DB處理   begin------------------*/
                FRTCodeHisDao fRTCodeHisDao = new FRTCodeHisDao();

                string[] curDateTime = DateUtil.getCurChtDateTime(3).Split(' ');
                //取得流水號
                SysSeqDao sysSeqDao = new SysSeqDao();
                string qPreCode = "0102" + (DateTime.Now.Year - 1911).ToString();
                var cId = sysSeqDao.qrySeqNo("RT", "0102", qPreCode).ToString();
                string aply_no = qPreCode + cId.ToString().PadLeft(5, '0');

                using (dbFGLEntities db = new dbFGLEntities())
                {

                    fRTCodeHisDao.insert(aply_no, dataList);

                    db.SaveChanges();
                }

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