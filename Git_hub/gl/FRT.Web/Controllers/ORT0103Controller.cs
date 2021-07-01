using FRT.Web.ActionFilter;
using FRT.Web.BO;
using FRT.Web.CacheProvider;
using FRT.Web.Daos;
using FRT.Web.Enum;
using FRT.Web.Models;
using FRT.Web.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using static FRT.Web.BO.Extension;

/// <summary>
/// 功能說明：退匯改給付方式原因Table檔
/// 初版作者：20201209 Bianco
/// 修改歷程：20201209 Bianco
///           需求單號：
///           初版
/// </summary>
///

namespace FRT.Web.Controllers
{
    [Authorize]
    [CheckSessionFilterAttribute]
    public class ORT0103Controller : BaseController
    {
        internal ICacheProvider Cache { get; set; }
        static private NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        SysCodeDao sysCodeDao = null;
        FRTCODEDao fRTCODEDao = null;
        FRTGLSIDao fRTGLSIDao = null;
        FRTCodeHisDao FRTCodeHisDao = null;
        public ORT0103Controller()
        {
            sysCodeDao = new SysCodeDao();
            fRTCODEDao = new FRTCODEDao();
            fRTGLSIDao = new FRTGLSIDao();
            FRTCodeHisDao = new FRTCodeHisDao();
            Cache = new DefaultCacheProvider();
        }

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
                String[] roleInfo = authUtil.chkUserFuncAuth(Session["UserID"].ToString(), "~/ORT0103/");
                if (roleInfo != null && roleInfo.Length == 2)
                {
                    opScope = roleInfo[0];
                    funcName = roleInfo[1];
                }
            }

            List<SelectOption> rejectCode = new List<SelectOption>();
            rejectCode.Add(new SelectOption() { Value = "Y", Text = "Y" });
            rejectCode.Add(new SelectOption() { Value = "N", Text = "N" });

            //SysCodeDao sysCodeDao = new SysCodeDao();
            //DATA_STATUS
            var _data_Status = sysCodeDao.qryByTypeDic("RT", "DATA_STATUS");
            ViewBag.dataStatusjqList = _data_Status;
            //資料狀態
            ViewBag.statusjqList = sysCodeDao.qryByTypeDic("RT", "STATUS");

            ViewBag.opScope = opScope;
            ViewBag.funcName = funcName;
            ViewBag.bMaintain = "Y";
            ViewBag.BoolSelect = new SelectList(
               items: rejectCode,
               dataValueField: "Value",
               dataTextField: "Text"
           );

            return View();
        }

        public ActionResult onlyQry(string srce_from, string btnModify)
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

            List<SelectOption> rejectCode = new List<SelectOption>();
            rejectCode.Add(new SelectOption() { Value = "Y", Text = "Y" });
            rejectCode.Add(new SelectOption() { Value = "N", Text = "N" });

            //SysCodeDao sysCodeDao = new SysCodeDao();
            //DATA_STATUS
            var _data_Status = sysCodeDao.qryByTypeDic("RT", "DATA_STATUS");
            ViewBag.dataStatusjqList = _data_Status;
            //資料狀態
            ViewBag.statusjqList = sysCodeDao.qryByTypeDic("RT", "STATUS");

            ViewBag.funcName = funcName;
            ViewBag.opScope = opScope;
            ViewBag.bMaintain = "N";

            ViewBag.BoolSelect = new SelectList(
               items: rejectCode,
               dataValueField: "Value",
               dataTextField: "Text"
           );

            return View("Index");
        }

        /// <summary>
        /// 查詢資料
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public JsonResult QryData()
        {
            logger.Info("QryData begin!!");
            MSGReturnModel<List<SelectOption>> result = new MSGReturnModel<List<SelectOption>>();
            try
            {
                //SysCodeDao SysCodeDao = new SysCodeDao();
                //FRTCODEDao fRTCODEDao = new FRTCODEDao();
                //FRTGLSIDao fRTGLSIDao = new FRTGLSIDao();

                var rejectedData = fRTGLSIDao.SelectReJectedDatas();

                //FRTCodeHisDao FRTCodeHisDao = new FRTCodeHisDao();
                var datas = fRTCODEDao.qryRTCode("RT", "FAIL-REP", string.Empty);
                var resultDatas = datas.Select(x => new ORT0103Model {
                    rejected_Code = string.IsNullOrWhiteSpace(x.refNo) == true ? "" : x.refNo,
                    rejected_Reason = rejectedData.FirstOrDefault(y => y.Value == x.refNo)?.Text,
                    received_Id = x.text?.Substring(0, 1),
                    received_Account = x.text?.Substring(1, 1),
                    bank_Code = x.text?.Substring(2, 1),
                    bank_Account = x.text?.Substring(3, 1)
                }).ToList();

                //modify by daiyu 未覆核的資料不要看到
                ////查詢在覆核中，且為"新增"的資料，查詢時就要看見
                //List<FRT_CODE_HIS> hisStatusA = FRTCodeHisDao.qryByStatus("RT", "FAIL-REP", "A");
                //if (hisStatusA.Any())
                //{
                //    var dataA = hisStatusA.Select(x => new ORT0103Model {
                //        aplyNo = x.APPLY_NO,
                //        rejected_Code = x.REF_NO,
                //        rejected_Reason = rejectedData.FirstOrDefault(y => y.value == x.REF_NO)?.text,
                //        received_Id = x.TEXT?.Substring(0, 1),
                //        received_Account = x.TEXT?.Substring(1, 1),
                //        bank_Code = x.TEXT?.Substring(2, 1),
                //        bank_Account = x.TEXT?.Substring(3, 1),
                //    });
                    
                //    resultDatas.AddRange(dataA);
                //}

                foreach (ORT0103Model d in resultDatas)
                {
                    FRT_CODE_HIS his = FRTCodeHisDao.chkOnAply("RT", "FAIL-REP", d.rejected_Code);

                    if (his != null)
                    {
                        d.appr_stat = "2";                     
                    }   
                    else
                        d.appr_stat = "1";
                }

                Cache.Invalidate(CacheList.ORT0103ViewData);
                Cache.Set(CacheList.ORT0103ViewData, resultDatas);

                var tempData = resultDatas.Select(y => y.rejected_Code).ToList();

                rejectedData.ToList().ForEach(x => {
                    if (tempData.Contains(x.Value))
                        rejectedData.Remove(x);
                });
                
                result.Datas = rejectedData;
                Cache.Invalidate(CacheList.ORT0103SelectData);
                Cache.Set(CacheList.ORT0103SelectData, rejectedData);


                result.RETURN_FLAG = true;
            }
            catch (Exception e)
            {
                logger.Error($@"qryFRTWord Error : {e.ToString()}");
                result.DESCRIPTION = Ref.MessageType.sys_Error.GetDescription();
                result.RETURN_FLAG = false;
            }
            return Json(result);
        }

        /// <summary>
        /// 畫面執行"申請覆核"
        /// </summary>
        /// <param name="gridData"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult execSave(List<ORTB016Model> gridData)
        {
            logger.Info("ORT0103 execSave begin");
            MSGReturnModel<string> result = new MSGReturnModel<string>();

            try
            {
                result.RETURN_FLAG = false;
                result.DESCRIPTION = Ref.MessageType.login_Time_Out.GetDescription();
                if (Cache.IsSet(CacheList.ORT0103ViewData))
                {
                    var tempData = (List<ORT0103Model>)Cache.Get(CacheList.ORT0103ViewData);
                    tempData = tempData.Where(x => x.status != null).ToList();

                    if (!tempData.Any())    //add by daiyu 20210312
                        result.DESCRIPTION = Ref.MessageType.not_Find_Update_Data.GetDescription();

                    List<FRT_CODE_HIS> dataList = new List<FRT_CODE_HIS>();

                    var datas = tempData.Select(x => new FRT_CODE_HIS 
                    {
                        GROUP_ID = "FAIL-REP",
                        TEXT_LEN = 4,
                        REF_NO = x.rejected_Code,
                        TEXT = $"{x.received_Id?.Trim()}{x.received_Account?.Trim()}{x.bank_Code?.Trim()}{x.bank_Account?.Trim()}",
                        SRCE_FROM = "RT",
                        USE_MARK = "",
                        UPDATE_ID = Session["UserID"].ToString(),
                        REF_NO_N = x.rejected_Code,
                        STATUS = x.status
                    }).ToList();
                    dataList.AddRange(datas);

                    /*------------------ DB處理   begin------------------*/
                    //FRTCodeHisDao fRTCodeHisDao = new FRTCodeHisDao();

                    string[] curDateTime = DateUtil.getCurChtDateTime(4).Split(' ');
                    //取得流水號
                    SysSeqDao sysSeqDao = new SysSeqDao();
                    String qPreCode = curDateTime[0];
                    var cId = sysSeqDao.qrySeqNo("RT", "FAIL", qPreCode).ToString();

                    FRTCodeHisDao.insert(qPreCode + cId.ToString().PadLeft(3, '0'), dataList);
                    result.DESCRIPTION = Ref.MessageType.Apply_Audit_Success.GetDescription();
                    result.RETURN_FLAG = true;

                }
            }
            catch (Exception e)
            {
                logger.Error($@"ORTB016 execSave Error : {e} ");
                result.DESCRIPTION = Ref.MessageType.sys_Error.GetDescription();
            }
            return Json(result);

        }

        /// <summary>
        /// 新增明細資料
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult InsertTempData(ORT0103Model model)
        {
            MSGReturnModel<List<SelectOption>> result = new MSGReturnModel<List<SelectOption>>();
            result.RETURN_FLAG = false;
            result.DESCRIPTION = Ref.MessageType.login_Time_Out.GetDescription();
            if (Cache.IsSet(CacheList.ORT0103ViewData))
            {
                var tempData = (List<ORT0103Model>)Cache.Get(CacheList.ORT0103ViewData);
                model.status = "A";
                tempData.Add(model);
                Cache.Invalidate(CacheList.ORT0103ViewData);
                Cache.Set(CacheList.ORT0103ViewData, tempData);


                if(Cache.IsSet(CacheList.ORT0103SelectData))
                {
                    var rejectedData = (List<SelectOption>)Cache.Get(CacheList.ORT0103SelectData);
                    rejectedData.Remove(rejectedData.FirstOrDefault(x => x.Value == model.rejected_Code));
                    Cache.Invalidate(CacheList.ORT0103SelectData);
                    Cache.Set(CacheList.ORT0103SelectData, rejectedData);

                    result.Datas = rejectedData;
                }

                result.RETURN_FLAG = true;
                result.DESCRIPTION = Ref.MessageType.insert_Success.GetDescription();
            }
            return Json(result);
        }

        /// <summary>
        /// 修改明細資料
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult UpdateTempData(ORT0103Model model)
        {
            MSGReturnModel<List<SelectOption>> result = new MSGReturnModel<List<SelectOption>>();
            result.RETURN_FLAG = false;
            result.DESCRIPTION = Ref.MessageType.login_Time_Out.GetDescription();
            if (Cache.IsSet(CacheList.ORT0103ViewData))
            {
                var tempData = (List<ORT0103Model>)Cache.Get(CacheList.ORT0103ViewData);
                var updateTempData = tempData.FirstOrDefault(x => x.rejected_Code == model.rejected_Code);
                if (updateTempData != null)
                {
                    if (updateTempData.status.IsNullOrWhiteSpace() || (updateTempData.status == "D"))
                        updateTempData.status = "U";

                    updateTempData.received_Id = model.received_Id;
                    updateTempData.received_Account = model.received_Account;
                    updateTempData.bank_Code = model.bank_Code;
                    updateTempData.bank_Account = model.bank_Account;
                    Cache.Invalidate(CacheList.ORT0103ViewData);
                    Cache.Set(CacheList.ORT0103ViewData, tempData);

                    if (Cache.IsSet(CacheList.ORT0103SelectData))
                    {
                        var rejectedData = (List<SelectOption>)Cache.Get(CacheList.ORT0103SelectData);

                        result.Datas = rejectedData;
                    }

                    result.RETURN_FLAG = true;
                    result.DESCRIPTION = Ref.MessageType.update_Success.GetDescription();
                }
                else
                {
                    result.RETURN_FLAG = false;
                    result.DESCRIPTION = Ref.MessageType.update_Fail.GetDescription();
                }
            }
            return Json(result);
        }

        /// <summary>
        /// 刪除明細資料
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult DeleteTempData(ORT0103Model model)
        {
            MSGReturnModel<List<SelectOption>> result = new MSGReturnModel<List<SelectOption>>();
            result.RETURN_FLAG = false;
            result.DESCRIPTION = Ref.MessageType.login_Time_Out.GetDescription();
            if (Cache.IsSet(CacheList.ORT0103ViewData))
            {
                var tempData = (List<ORT0103Model>)Cache.Get(CacheList.ORT0103ViewData);
                var deleteTempData = tempData.FirstOrDefault(x => x.rejected_Code == model.rejected_Code);
                if (deleteTempData != null)
                {
                    if (deleteTempData.status == "A")
                    {
                        tempData.Remove(deleteTempData);

                        if (Cache.IsSet(CacheList.ORT0103SelectData))
                        {
                            var rejectedData = (List<SelectOption>)Cache.Get(CacheList.ORT0103SelectData);
                            rejectedData.Add(new SelectOption() { Value = model.rejected_Code, Text = model.rejected_Reason});
                            rejectedData = rejectedData.OrderBy(x => x.Value).ToList();
                            Cache.Invalidate(CacheList.ORT0103SelectData);
                            Cache.Set(CacheList.ORT0103SelectData, rejectedData);
                            result.Datas = rejectedData;
                        }
                    }
                    else
                        deleteTempData.status = "D";

                    Cache.Invalidate(CacheList.ORT0103ViewData);
                    Cache.Set(CacheList.ORT0103ViewData, tempData);
                    result.RETURN_FLAG = true;
                    result.DESCRIPTION = Ref.MessageType.delete_Success.GetDescription();

                }
                else
                {
                    result.RETURN_FLAG = false;
                    result.DESCRIPTION = Ref.MessageType.delete_Fail.GetDescription();
                }
            }


            return Json(result);
        }

        /// <summary>
        /// 取消
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult ResetInsertTempData(ORT0103Model model)
        {
            MSGReturnModel<List<SelectOption>> result = new MSGReturnModel<List<SelectOption>>();
            if (Cache.IsSet(CacheList.ORT0103SelectData))
            {
                var rejectedData = (List<SelectOption>)Cache.Get(CacheList.ORT0103SelectData);
                result.Datas = rejectedData;
                result.RETURN_FLAG = true;
            }
            else
            {
                result.RETURN_FLAG = false;
                result.DESCRIPTION = "退匯原因選單產生錯誤";
            }
            return Json(result);
        }

        /// <summary>
        /// 清除明細資料
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult ResetTempData()
        {
            MSGReturnModel<bool> result = new MSGReturnModel<bool>();
            if (Cache.IsSet(CacheList.ORT0103ViewData))
            {
                var tempData = (List<ORT0103Model>)Cache.Get(CacheList.ORT0103ViewData);
                tempData = new List<ORT0103Model>();
                Cache.Invalidate(CacheList.ORT0103ViewData);
                Cache.Set(CacheList.ORT0103ViewData, tempData);
            }
            return Json(result);
        }

        /// <summary>
        /// jqgrid cache data
        /// </summary>
        /// <param name="jdata"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult GetCacheData(jqGridParam jdata)
        {
            if (Cache.IsSet(CacheList.ORT0103ViewData))
            {
                var data = (List<ORT0103Model>)Cache.Get(CacheList.ORT0103ViewData);
                ;
                return Json(jdata.modelToJqgridResult(data.OrderByDescending(x => x.status).ThenBy(x => x.appr_stat).ThenBy(x => x.rejected_Code).ToList()));
            }
            return null;
        }
    }
}