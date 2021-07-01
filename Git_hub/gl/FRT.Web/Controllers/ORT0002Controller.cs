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
/// 功能說明：AML洗錢態樣檢核TABLE檔維護作業
/// 初版作者：20191212 Daiyu
/// 修改歷程：20181212 Daiyu
///           需求單號：201912030811-01 AML相關需求-第一階段需求
///           初版
/// </summary>
///

namespace FRT.Web.Controllers
{
    [Authorize]
    [CheckSessionFilterAttribute]
    public class ORT0002Controller : BaseController
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
                String[] roleInfo = authUtil.chkUserFuncAuth(Session["UserID"].ToString(), "~/ORT0002/");
                if (roleInfo != null && roleInfo.Length == 2)
                {
                    opScope = roleInfo[0];
                    funcName = roleInfo[1];
                }
            }

            ViewBag.opScope = opScope;
            ViewBag.funcName = funcName;
            ViewBag.bMaintain = "Y";
            ViewBag.bRtn = "N";

            getViewDropDownList();

            return View();
        }


        /// <summary>
        /// For只可以查詢
        /// </summary>
        /// <param name="srce_from"></param>
        /// <param name="bRtn"></param>
        /// <returns></returns>
        public ActionResult onlyQry(string srce_from, string bRtn)
        {
            UserAuthUtil authUtil = new UserAuthUtil();

            string srcUrl = "~/" + srce_from + "/";
            string opScope = "";
            string funcName = "";

            if (Session["UserID"] != null)
            {
                String[] roleInfo2 = authUtil.chkUserFuncAuth(Session["UserID"].ToString(), srcUrl);
                if (roleInfo2 != null && roleInfo2.Length == 2)
                {
                    opScope = roleInfo2[0];
                    funcName = roleInfo2[1];
                }
            }

            ViewBag.funcName = funcName;
            ViewBag.opScope = opScope;
            ViewBag.bMaintain = "N";
            ViewBag.bRtn = bRtn;

            getViewDropDownList();
            return View("Index");
        }



        /// <summary>
        /// 取得畫面的下拉選單
        /// </summary>
        private void getViewDropDownList()
        {
            FPMCODEDao fPMCODEDao = new FPMCODEDao();

            //繳款原因
            var rmResnList = fPMCODEDao.loadSelectList("RM-RESN", "RT", "", true);
            ViewBag.rmResnList = rmResnList;
            ViewBag.rmResnjsList = fPMCODEDao.jqGridList("RM-RESN", "RT", "", true);

            SysCodeDao sysCodeDao = new SysCodeDao();
            //執行功能
            ViewBag.statusjqList = sysCodeDao.jqGridList("RT", "STATUS", true);

            //資料狀態
            ViewBag.dataStatusjqList = sysCodeDao.jqGridList("RT", "DATA_STATUS", true);

        }


        /// <summary>
        /// 查詢
        /// </summary>
        /// <param name="rm_resn"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult qryFRTCODE(string rm_resn)
        {
            logger.Info("qryFRTCODE begin!!");

            try
            {
                FRTCODEDao fRTCODEDao = new FRTCODEDao();
                FRTCodeHisDao FRTCodeHisDao = new FRTCodeHisDao();

                List<FRTCODEModel> rows = fRTCODEDao.qryRTCode("RT", "RT_FEC", "");
                List<ORT0002Model> dataList = new List<ORT0002Model>();

                foreach (FRTCODEModel d in rows)
                {
                    try
                    {
                        ORT0002Model data = new ORT0002Model();
                        data.tempId = string.IsNullOrWhiteSpace(d.refNo) == true ? "" : d.refNo;
                        data.rm_resn = d.refNo;
                        data.sys_day = Convert.ToInt16(StringUtil.toString(d.text.Substring(0, 3))).ToString();
                        data.remit_cnt = Convert.ToInt16(StringUtil.toString(d.text.Substring(3,3))).ToString();
                        data.remit_amt = Convert.ToInt64(StringUtil.toString(d.text.Substring(6, 8))).ToString();
                        data.update_id = d.updId;
                        data.update_datetime = d.updDate;
                        data.appr_id = d.apprId;
                        data.appr_datetime = d.apprDate;

                        bool bAdd = true;

                        //繳款原因
                        if (!"".Equals(StringUtil.toString(rm_resn)) & !StringUtil.toString(rm_resn).Equals(d.refNo))
                            bAdd = false;

                        if (bAdd)
                            dataList.Add(data);
                    }
                    catch (Exception e) {

                    }
                }

                //查詢在覆核中，且為"新增"的資料，查詢時就要看見
                List<FRT_CODE_HIS> hisStatusA = FRTCodeHisDao.qryByStatus("RT", "RT_FEC", "A");
                if (hisStatusA != null)
                {
                    foreach (FRT_CODE_HIS hisA in hisStatusA)
                    {
                        try
                        {
                            string updDate = DateUtil.DatetimeToString(hisA.UPDATE_DATETIME, "yyyyMMdd");
                            updDate = (Convert.ToInt16(updDate.Substring(0, 4)) - 1911).ToString() + updDate.Substring(4);

                            ORT0002Model data = new ORT0002Model();
                            data.tempId = string.IsNullOrWhiteSpace(hisA.REF_NO_N) == true ? "" : hisA.REF_NO_N;
                            data.rm_resn = hisA.REF_NO;
                            data.sys_day = Convert.ToInt16(StringUtil.toString(hisA.TEXT.Substring(0, 3))).ToString();
                            data.remit_cnt = Convert.ToInt16(StringUtil.toString(hisA.TEXT.Substring(3, 3))).ToString();
                            data.remit_amt = Convert.ToUInt64(StringUtil.toString(hisA.TEXT.Substring(6, 8))).ToString();
                            data.dataStatus = "2";

                            bool bAdd = true;

                            //繳款原因
                            if (!"".Equals(StringUtil.toString(rm_resn)) & !StringUtil.toString(rm_resn).Equals(hisA.REF_NO))
                                bAdd = false;

                            if (bAdd)
                                dataList.Add(data);

                       
                        }
                        catch (Exception e)
                        {


                        }

                    }
                }


                //查DB_INTRA取得異動人員、覆核人員姓名
                using (DB_INTRAEntities dbIntra = new DB_INTRAEntities())
                {
                    Dictionary<string, string> userNameMap = new Dictionary<string, string>();
                    OaEmpDao oaEmpDao = new OaEmpDao();
                    string update_id = "";
                    string appr_id = "";

                    foreach (ORT0002Model d in dataList)
                    {
                        update_id = StringUtil.toString(d.update_id);
                        appr_id = StringUtil.toString(d.appr_id);
                        d.update_name = update_id;
                        d.appr_name = appr_id;

                        if (!"".Equals(update_id))
                        {
                            if (!userNameMap.ContainsKey(update_id))
                            {
                                userNameMap = oaEmpDao.qryUsrName(userNameMap, update_id, dbIntra);
                            }
                            d.update_name = userNameMap[update_id];
                        }

                        if (!"".Equals(appr_id))
                        {
                            if (!userNameMap.ContainsKey(appr_id))
                            {
                                userNameMap = oaEmpDao.qryUsrName(userNameMap, appr_id, dbIntra);
                            }
                            d.appr_name = userNameMap[appr_id];
                        }
                    }
                }

                foreach (ORT0002Model d in dataList)
                {
                    FRT_CODE_HIS his = FRTCodeHisDao.chkOnAply("RT", "RT_FEC", d.tempId);

                    if (his != null)
                        d.dataStatus = "2";
                    else
                        d.dataStatus = "1";
                }




                var jsonData = new { success = true, dataList = dataList };
                return Json(jsonData, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new { success = false, err = "其它錯誤，請洽系統管理員!!" }, JsonRequestBehavior.AllowGet);
            }
        }


        /// <summary>
        /// 執行"申請覆核"
        /// </summary>
        /// <param name="groupId"></param>
        /// <param name="srceFrom"></param>
        /// <param name="textLen"></param>
        /// <param name="gridData"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult execSave(List<ORT0002Model> gridData)
        {
            logger.Info("execSave begin");

            string errStr = "";
            bool bChg = false;


            List<FRT_CODE_HIS> dataList = new List<FRT_CODE_HIS>();


            foreach (ORT0002Model d in gridData)
            {
                if (!"".Equals(StringUtil.toString(d.status)))
                {
                    ErrorModel errModel = chkAplyData(d.tempId, d.rm_resn, d.status);

                    if (errModel.chkResult)
                    {
                        bChg = true;

                        FRT_CODE_HIS codeHis = new FRT_CODE_HIS();
                        codeHis.GROUP_ID = "RT_FEC";
                        codeHis.TEXT_LEN = 14;
                        codeHis.REF_NO = d.tempId;
                        codeHis.TEXT = d.sys_day.PadLeft(3, '0') + d.remit_cnt.PadLeft(3, '0') + d.remit_amt.PadLeft(8, '0');
                        codeHis.SRCE_FROM = "RT";
                        codeHis.USE_MARK = "";
                        codeHis.UPDATE_ID = Session["UserID"].ToString();
                        codeHis.REF_NO_N = d.rm_resn;
                        codeHis.STATUS = d.status;
                        dataList.Add(codeHis);
                    }
                    else
                    {
                        errStr += "參考號碼：" + d.tempId + "<br/>";
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

            string[] curDateTime = DateUtil.getCurChtDateTime(4).Split(' ');
            //取得流水號
            SysSeqDao sysSeqDao = new SysSeqDao();
            String qPreCode = curDateTime[0];
            var cId = sysSeqDao.qrySeqNo("RT", "0002", qPreCode).ToString();


            fRTCodeHisDao.insert(qPreCode + cId.ToString().PadLeft(3, '0'), dataList);

            return Json(new { success = true, aplyNo = qPreCode + cId.ToString().PadLeft(3, '0'), err = errStr });

        }



        /// <summary>
        /// 檢查要異動的資料是否可以異動
        /// </summary>
        /// <param name="iTempId"></param>
        /// <param name="rm_resn"></param>
        /// <param name="status"></param>
        /// <returns></returns>

        [HttpPost]
        public JsonResult chkData(string iTempId, string rm_resn, string status)
        {
            ErrorModel errModel = new ErrorModel();
            errModel = chkAplyData(iTempId, rm_resn, status);

            if (errModel.chkResult)
                return Json(new { success = true });
            else
                return Json(new { success = false, err = errModel.msg });
        }


        /// <summary>
        /// 檢查資料是否已存在主檔或暫存檔
        /// </summary>
        /// <param name="iTempId"></param>
        /// <param name="rm_resn"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        private ErrorModel chkAplyData(string iTempId, string rm_resn, string status)
        {

            string refNoN = StringUtil.toString(rm_resn);
            ErrorModel errModel = new ErrorModel();

            if ("A".Equals(status) || 
                !StringUtil.toString(iTempId).Equals(refNoN))
            {
                FRTCODEDao fRTCODEDao = new FRTCODEDao();
                List<FRTCODEModel> dataO = new List<FRTCODEModel>();

                dataO = fRTCODEDao.qryRTCode("RT", "RT_FEC", refNoN);

                if (dataO.Count > 0)
                {
                    errModel.chkResult = false;
                    errModel.msg = "此筆資料已存在不可新增!!";
                    return errModel;
                }
            }

            FRTCodeHisDao FRTCodeHisDao = new FRTCodeHisDao();
            List<FrtCodeHisModel> aplyData = new List<FrtCodeHisModel>();

            aplyData = FRTCodeHisDao.qryForSTAT("RT", "RT_FEC", iTempId, refNoN, "1");
            if (aplyData.Count > 0)
            {
                errModel.chkResult = false;
                errModel.msg = "資料覆核中，不可修改/刪除此筆資料!!";
                return errModel;
            }


            errModel.chkResult = true;
            errModel.msg = "";
            return errModel;
        }





        internal class ErrorModel
        {
            public string tempId { get; set; }
            public bool chkResult { get; set; }
            public string msg { get; set; }
        }
    }
}