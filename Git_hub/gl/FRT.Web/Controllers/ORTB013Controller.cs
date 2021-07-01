using FRT.Web.ActionFilter;
using FRT.Web.AS400Models;
using FRT.Web.BO;
using FRT.Web.Daos;
using FRT.Web.Models;
using FRT.Web.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

/// <summary>
/// 功能說明：銀行碼基本資料訊息維護作業
/// 初版作者：20181004 Daiyu
/// 修改歷程：20181004 Daiyu
///           需求單號：201808170384-00 雙系統銀行分行整併第二階段
///           初版
/// </summary>
///

namespace FRT.Web.Controllers
{
    [Authorize]
    [CheckSessionFilterAttribute]
    public class ORTB013Controller : BaseController
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
                String[] roleInfo = authUtil.chkUserFuncAuth(Session["UserID"].ToString(), "~/ORTB013/");
                if (roleInfo != null && roleInfo.Length == 2)
                {
                    opScope = roleInfo[0];
                    funcName = roleInfo[1];
                }
            }

            ViewBag.opScope = opScope;
            ViewBag.funcName = funcName;
            
            SysCodeDao sysCodeDao = new SysCodeDao();

            //FRTCODE
            var frtCodeList = sysCodeDao.loadSelectList("RT", "FRTCODE", true);
            ViewBag.frtCodeList = frtCodeList;


            
            //資料狀態
            ViewBag.statusjqList = sysCodeDao.jqGridList("RT", "STATUS", true);

            return View();
        }

        

        /// <summary>
        /// 查詢該組別碼的文字長度及資料來源
        /// </summary>
        /// <param name="groupId"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult qryGroupId(string groupId)
        {
            try {
                string srceFrom = "";
                string textLen = "";
                SysCodeDao SysCodeDao = new SysCodeDao();
                SYS_CODE data = SysCodeDao.qryByKey("RT", "FRTCODE", groupId);
                if (data != null) {
                    srceFrom = StringUtil.toString(data.RESERVE2);
                    textLen = StringUtil.toString(data.RESERVE3);
                }

                return Json(new { success = true, srceFrom = srceFrom, textLen = textLen }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e) {
                logger.Error(e.ToString());
                return Json(new { success = false, err = "其它錯誤，請洽系統管理員!!" }, JsonRequestBehavior.AllowGet);
            }
            
        }

        /// <summary>
        /// 查詢"FRTCODE ＲＴ各類相關代碼檔 "
        /// </summary>
        /// <param name="groupId"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult qryFRTCODE(string groupId, string srceFrom)
        {
            logger.Info("qryFRTCODE begin!!");

            try
            {
                FRTCODEDao fRTCODEDao = new FRTCODEDao();
                FRTCodeHisDao FRTCodeHisDao = new FRTCodeHisDao();

                List<FRTCODEModel> rows = fRTCODEDao.qryRTCode(srceFrom, groupId, "");
                List<ORTB013Model> dataList = new List<ORTB013Model>();

                if (rows.Count > 0) {

                    if (!"BKMSG_OTH".Equals(groupId))
                    {
                        foreach (FRTCODEModel d in rows)
                        {
                            ORTB013Model data = new ORTB013Model();
                            data.tempId = string.IsNullOrWhiteSpace(d.refNo) == true ? "" : d.refNo;
                            data.groupId = groupId;
                            data.srceFrom = srceFrom;
                            data.refNo = string.IsNullOrWhiteSpace(d.refNo) == true ? "" : d.refNo;
                            data.refNoN = string.IsNullOrWhiteSpace(d.refNo) == true ? "" : d.refNo;
                            data.text = string.IsNullOrWhiteSpace(d.text) == true ? "" : d.text;
                            data.entryId = string.IsNullOrWhiteSpace(d.entryId) == true ? "" : d.entryId;
                            data.entryDate = string.IsNullOrWhiteSpace(d.entryDate) == true ? "" : d.entryDate;
                            data.updId = string.IsNullOrWhiteSpace(d.updId) == true ? "" : d.updId;
                            data.updDateTime = string.IsNullOrWhiteSpace(d.updDate) == true ? "" : d.updDate;
                            data.apprId = string.IsNullOrWhiteSpace(d.apprId) == true ? "" : d.apprId;
                            data.apprDate = string.IsNullOrWhiteSpace(d.apprDate) == true ? "" : d.apprDate;

                            dataList.Add(data);
                        }
                    }
                    else {
                        List<FRTCODEModel> bankGrp = rows.GroupBy(o => new { refNo = o.refNo.Substring(0, o.refNo.IndexOf('_')) })
                            .Select(group => new FRTCODEModel
                            {
                                refNo = group.Key.refNo
                            }
                            ).ToList<FRTCODEModel>();


                        foreach (FRTCODEModel d in bankGrp)
                        {
                            FRTCODEModel model = rows.Where(x => x.refNo.StartsWith(d.refNo + "_")).First();

                            ORTB013Model data = new ORTB013Model();
                            data.tempId = string.IsNullOrWhiteSpace(d.refNo) == true ? "" : d.refNo;
                            data.groupId = groupId;
                            data.srceFrom = srceFrom;
                            data.refNo = string.IsNullOrWhiteSpace(d.refNo) == true ? "" : d.refNo;
                            data.refNoN = string.IsNullOrWhiteSpace(d.refNo) == true ? "" : d.refNo;
                            data.entryId = string.IsNullOrWhiteSpace(model.entryId) == true ? "" : model.entryId;
                            data.entryDate = string.IsNullOrWhiteSpace(model.entryDate) == true ? "" : model.entryDate;
                            data.updId = string.IsNullOrWhiteSpace(model.updId) == true ? "" : model.updId;
                            data.updDateTime = string.IsNullOrWhiteSpace(model.updDate) == true ? "" : model.updDate;
                            data.apprId = string.IsNullOrWhiteSpace(model.apprId) == true ? "" : model.apprId;
                            data.apprDate = string.IsNullOrWhiteSpace(model.apprDate) == true ? "" : model.apprDate;

                            foreach (FRTCODEModel detail in rows.Where(x => x.refNo.StartsWith(d.refNo + "_")))
                            {
                                data.text += string.IsNullOrWhiteSpace(detail.text) == true ? "" : detail.text;

                            }

                            dataList.Add(data);
                        }

                    }

                    //查詢在覆核中，且為"新增"的資料，FOR ORTB013查詢時就要看見
                    List<FRT_CODE_HIS> hisStatusA = FRTCodeHisDao.qryByStatus(srceFrom, groupId, "A");
                    if (hisStatusA != null) {
                        foreach (FRT_CODE_HIS hisA in hisStatusA) {
                            string updDate = DateUtil.DatetimeToString(hisA.UPDATE_DATETIME, "yyyyMMdd");
                            updDate = (Convert.ToInt16(updDate.Substring(0, 4)) - 1911).ToString() + updDate.Substring(4);

                            ORTB013Model data = new ORTB013Model();
                            data.tempId = string.IsNullOrWhiteSpace(hisA.REF_NO_N) == true ? "" : hisA.REF_NO_N;
                            data.groupId = groupId;
                            data.srceFrom = srceFrom;
                            data.refNo = string.IsNullOrWhiteSpace(hisA.REF_NO) == true ? "" : hisA.REF_NO;
                            data.refNoN = string.IsNullOrWhiteSpace(hisA.REF_NO_N) == true ? "" : hisA.REF_NO_N;
                            data.text = string.IsNullOrWhiteSpace(hisA.TEXT) == true ? "" : hisA.TEXT;
                            //data.entryId = string.IsNullOrWhiteSpace(hisA.UPDATE_ID) == true ? "" : hisA.UPDATE_ID;
                            //data.entryDate = updDate;
                            data.updId = string.IsNullOrWhiteSpace(hisA.UPDATE_ID) == true ? "" : hisA.UPDATE_ID;
                            data.updDateTime = updDate;
                            data.dataStatus = "覆核中";
                            dataList.Add(data);

                        }
                    }


                    //查DB_INTRA取得異動人員、覆核人員姓名
                    using (DB_INTRAEntities dbIntra = new DB_INTRAEntities())
                    {
                        Dictionary<string, string> userNameMap = new Dictionary<string, string>();
                        OaEmpDao oaEmpDao = new OaEmpDao();
                        string entryId = "";
                        string updId = "";
                        string apprId = "";

                        foreach (ORTB013Model d in dataList)
                        {
                            entryId = StringUtil.toString(d.entryId);
                            updId = StringUtil.toString(d.updId);
                            apprId = StringUtil.toString(d.apprId);
                            d.entryName = entryId;
                            d.updId = updId;
                            d.apprName = apprId;

                            if (!"".Equals(entryId))
                            {
                                if (!userNameMap.ContainsKey(entryId))
                                {
                                    userNameMap = oaEmpDao.qryUsrName(userNameMap, entryId, dbIntra);
                                }
                                d.entryName = userNameMap[entryId];
                            }

                            if (!"".Equals(updId))
                            {
                                if (!userNameMap.ContainsKey(updId))
                                {
                                    userNameMap = oaEmpDao.qryUsrName(userNameMap, updId, dbIntra);
                                }
                                d.updateUName = userNameMap[updId];
                            }

                            if (!"".Equals(apprId))
                            {
                                if (!userNameMap.ContainsKey(apprId))
                                {
                                    userNameMap = oaEmpDao.qryUsrName(userNameMap, apprId, dbIntra);
                                }
                                d.apprName = userNameMap[apprId];
                            }
                        }

                    }

                    foreach (ORTB013Model d in dataList)
                    {
                        FRT_CODE_HIS his = FRTCodeHisDao.chkOnAply(d.srceFrom, d.groupId, d.refNo);

                        if(his != null)
                            d.dataStatus = "覆核中";


                        if ("BKMSG_OTH".Equals(groupId)) {
                            FDCBANKADao fDCBANKADao = new FDCBANKADao();
                            FDCBANKAModel fDCBANKAModel = new FDCBANKAModel();
                            fDCBANKAModel = fDCBANKADao.qryByBankNo(d.refNo);

                            if (!string.IsNullOrEmpty(fDCBANKAModel.bankNo))
                                d.othText = fDCBANKAModel.bankName;
                        }

                    }
                }

                var jsonData = new { success = true, dataList = dataList };
                return Json(jsonData, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e) {
                return Json(new { success = false, err = "其它錯誤，請洽系統管理員!!" }, JsonRequestBehavior.AllowGet);
            }

            
        }



        /// <summary>
        /// 畫面執行"申請覆核"
        /// </summary>
        /// <param name="bankCode"></param>
        /// <param name="bankType"></param>
        /// <param name="gridData"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult execSave(string groupId, string srceFrom, string textLen, List<ORTB013Model> gridData)
        {
            logger.Info("execSave begin");

            string errStr = "";
            bool bChg = false;

            
            List<FRT_CODE_HIS> dataList = new List<FRT_CODE_HIS>();


            foreach (ORTB013Model d in gridData)
            {
                d.refNo = StringUtil.toString(d.refNo);
                d.refNoN = StringUtil.toString(d.refNoN);

                if (!"".Equals(StringUtil.toString(d.status)))
                {
                    errModel errModel = chkAplyData(d.status, groupId, srceFrom, d.refNo, d.refNoN);

                    if (errModel.chkResult)
                    {
                        bChg = true;

                        FRT_CODE_HIS codeHis = new FRT_CODE_HIS();
                        codeHis.GROUP_ID = groupId;
                        codeHis.TEXT_LEN = Convert.ToDecimal(textLen);
                        codeHis.REF_NO = d.refNo;
                        codeHis.TEXT = d.text;
                        codeHis.SRCE_FROM = srceFrom;
                        codeHis.USE_MARK = d.useMark;
                        codeHis.UPDATE_ID = Session["UserID"].ToString();
                        codeHis.REF_NO_N = d.refNoN;
                        codeHis.STATUS = d.status;
                        codeHis.USE_MARK = "";
                        dataList.Add(codeHis);
                    }
                    else
                    {
                        errStr += "參考號碼：" + d.refNoN + "<br/>";
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
            var cId = sysSeqDao.qrySeqNo("RT", "B013", qPreCode).ToString();


            fRTCodeHisDao.insert(qPreCode + cId.ToString().PadLeft(3, '0'), dataList);

            return Json(new { success = true, aplyNo = qPreCode + cId.ToString().PadLeft(3, '0'), err = errStr });

        }


        /// <summary>
        /// 檢核資料是否可異動
        /// </summary>
        /// <param name="status"></param>
        /// <param name="groupId"></param>
        /// <param name="srceFrom"></param>
        /// <param name="refNo"></param>
        /// <param name="refNoN"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult chkData(string status, string groupId, string srceFrom, string refNo, string refNoN)
        {
            errModel errModel = new errModel();
            errModel = chkAplyData(status, groupId, srceFrom, refNo, refNoN);

            if (errModel.chkResult)
                return Json(new { success = true });
            else
                return Json(new { success = false, err = errModel.msg });


        }

        /// <summary>
        /// 預帶"說明"
        /// </summary>
        /// <param name="groupId"></param>
        /// <param name="refNo"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult qryText(string groupId, string refNo)
        {
            string text = "";

            try {

                switch (groupId)
                {
                    case "BKMSG_AR":
                    case "BKMSG_FOG":
                    case "BKMSG_NTD":
                    case "BKMSG_OTH":
                        FDCBANKADao fDCBANKADao = new FDCBANKADao();
                        FDCBANKAModel fDCBANKAModel = new FDCBANKAModel();
                        fDCBANKAModel = fDCBANKADao.qryByBankNo(refNo);
                        if (!string.IsNullOrEmpty(fDCBANKAModel.bankNo))
                            text = fDCBANKAModel.bankName;

                        break;
                    case "BKAC_CDRN":
                        FPMCODEDao fPMCODEDao = new FPMCODEDao();
                        List<FPMCODEModel> d = fPMCODEDao.qryFPMCODE("FAIL-CODE", "RT", refNo);
                        if (d.Count > 0)
                            text = d[0].text;

                        break;

                }


                return Json(new { success = true, text = text });
            } catch (Exception e) {
                logger.Error(e.ToString());
                return Json(new { success = false, err = "其它錯誤，請洽系統管理員!!" });
            }
            
        }


        /// <summary>
        /// 檢查畫面key的資料是否可異動
        /// </summary>
        /// <param name="status"></param>
        /// <param name="groupId"></param>
        /// <param name="srceFrom"></param>
        /// <param name="refNo"></param>
        /// <param name="refNoN"></param>
        /// <returns></returns>
        private errModel chkAplyData(string status, string groupId, string srceFrom, string refNo, string refNoN)
        {


            errModel errModel = new errModel();

            if ("A".Equals(status) || !refNo.Equals(refNoN))
            {
                FRTCODEDao fRTCODEDao = new FRTCODEDao();
                List<FRTCODEModel> dataO = new List<FRTCODEModel>();


                //銀行碼基本資料訊息-其它訊息(長度200中文字) BY銀行代碼建置:前7+ _ + 1/2/3 (最多只到3 ∵200個字)
                if ("BKMSG_OTH".Equals(groupId))
                    refNoN += "_1";

                dataO = fRTCODEDao.qryRTCode(srceFrom, groupId, refNoN);

                if (dataO.Count > 0)
                {
                    errModel.chkResult = false;
                    errModel.msg = "此筆資料已存在「ＲＴ各類相關代碼檔」不可新增!!";
                    return errModel;
                }
            }

            FRTCodeHisDao FRTCodeHisDao = new FRTCodeHisDao();
            List<FrtCodeHisModel> aplyData = new List<FrtCodeHisModel>();

            aplyData = FRTCodeHisDao.qryForSTAT(srceFrom, groupId, refNo, refNoN, "1");
            if (aplyData.Count > 0)
            {
                errModel.chkResult = false;
                errModel.msg = "資料覆核中，不可修改/刪除此筆資料!!";
                return errModel;
            }


            if ("BKMSG_AR".Equals(groupId) && ("A".Equals(status) || "U".Equals(status))) {
                FDCBANKADao fDCBANKADao = new FDCBANKADao();
                FDCBANKAModel fDCBANKAModel = fDCBANKADao.qryByBankNo(refNoN);

                if (string.IsNullOrWhiteSpace(fDCBANKAModel.bankName)) {
                    errModel.chkResult = false;
                    errModel.msg = "查無銀行資料!!";
                    return errModel;
                }
            }



            errModel.chkResult = true;
            errModel.msg = "";
            return errModel;
        }


        internal class errModel
        {
            public bool chkResult { get; set; }
            public string msg { get; set; }
        }

    }
}