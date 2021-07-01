using FRT.Web.ActionFilter;
using FRT.Web.BO;
using FRT.Web.CacheProvider;
using FRT.Web.Daos;
using FRT.Web.Models;
using FRT.Web.Service.Actual;
using FRT.Web.Service.Interface;
using FRT.Web.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using static FRT.Web.BO.Utility;
using static FRT.Web.Enum.Ref;

/// <summary>
/// 功能說明：跨系統資料庫勾稽銀存銷帳不比對帳號
/// 初版作者：20210518 Mark
/// 修改歷程：20210518 Mark
///           需求單號：
///           初版
/// </summary>
///

namespace FRT.Web.Controllers
{
    [Authorize]
    [CheckSessionFilterAttribute]
    public class ORT0109Controller : BaseController
    {
        static private NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        internal ICacheProvider Cache { get; set; }

        private IORT0109 ORT0109 { get; set; }

        public ORT0109Controller()
        {
            Cache = new DefaultCacheProvider();
            ORT0109 = new ORT0109();
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
                String[] roleInfo = authUtil.chkUserFuncAuth(Session["UserID"].ToString(), "~/ORT0109/");
                if (roleInfo != null && roleInfo.Length == 2)
                {
                    opScope = roleInfo[0];
                    funcName = roleInfo[1];
                }
            }

            ViewBag.opScope = opScope;
            ViewBag.funcName = funcName;

            SysCodeDao sysCodeDao = new SysCodeDao();
            //DATA_STATUS
            var _data_Status = sysCodeDao.qryByTypeDic("RT", "DATA_STATUS");
            ViewBag.dataStatusjqList = _data_Status;

            //資料狀態
            ViewBag.statusjqList = sysCodeDao.qryByTypeDic("RT", "STATUS");


            List<SelectOption> hh = new List<SelectOption>();
            for (var i = 0; i < 24; i++)
            {
                hh.Add(
                    new SelectOption()
                    {
                        Text = i.ToString().PadLeft(2, '0'),
                        Value = i.ToString().PadLeft(2, '0')
                    });
            }

            return View();
        }

        /// <summary>
        /// 查詢 資料
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public JsonResult qryORT0109(string bank_acct_no = null)
        {
            logger.Info("qryORT0109 Query!!");
            MSGReturnModel result = new MSGReturnModel();
            result.DESCRIPTION = "其它錯誤，請洽系統管理員!!";
            try
            {
                return Json(searchORT0109(bank_acct_no));
            }
            catch (Exception e)
            {
                logger.Error(e.ToString());
                return Json(result);
            }
        }

        public JsonResult updateORT0109(ORT0109ViewModel viewModel)
        {
            MSGReturnModel<bool> result = new MSGReturnModel<bool>();
            var ORT0109ViewData = (List<ORT0109ViewModel>)Cache.Get(CacheList.ORT0109ViewData);
            //var _exec_action = new Service.Actual.Common().GetSysCode("RT", "STATUS", false);
            var _sysCodes = new Service.Actual.Common().GetSysCodes("RT", new List<string>() {"STATUS"});
            var _checkData = ORT0109.CheckData(viewModel);
            if (viewModel.exec_action == "A" && 
                (ORT0109ViewData.Any(x => x.bank_acct_no == viewModel.bank_acct_no) ||
                _checkData.Item1))
            {
                if (_checkData.Item1)
                {
                    result.DESCRIPTION = _checkData.Item2;
                }
                else
                {
                    result.DESCRIPTION = $@"欲新增的資料,已有相同'帳號:{viewModel.bank_acct_no}'存在現行的資料或申請的資料!";
                }
              
            }
            else
            {
                try
                {
                    switch (viewModel.exec_action)
                    {
                        case "A":
                            ORT0109ViewData.Add(new ORT0109ViewModel()
                            {
                                pk_id = viewModel.pk_id,
                                exec_action = viewModel.exec_action,
                                exec_action_value = _sysCodes.FirstOrDefault(x => x.CODE_TYPE == "STATUS" &&  x.CODE == viewModel.exec_action)?.CODE_VALUE,
                                bank_acct_no = viewModel.bank_acct_no,
                                bank_acct_make_out = _checkData.Item3
                            });
                            break;
                        case "D":
                            var _ORT0109ViewData_D = ORT0109ViewData.FirstOrDefault(x => x.pk_id == viewModel.pk_id);
                            if (_ORT0109ViewData_D.exec_action == "A")
                            {
                                ORT0109ViewData.Remove(_ORT0109ViewData_D);
                            }
                            else
                            {
                                _ORT0109ViewData_D.exec_action = "D";
                                _ORT0109ViewData_D.exec_action_value = _sysCodes.FirstOrDefault(x => x.CODE_TYPE == "STATUS" && x.CODE == viewModel.exec_action)?.CODE_VALUE;
                            }
                            break;
                        case "U":
                            //var _ORT0109ViewData_U = ORT0109ViewData.FirstOrDefault(x => x.pk_id == viewModel.pk_id);
                            //if (_ORT0109ViewData_U.exec_action != "A")
                            //{
                            //    _ORT0109ViewData_U.exec_action = "U";
                            //    _ORT0109ViewData_U.exec_action_value = _sysCodes.FirstOrDefault(x => x.CODE_TYPE == "STATUS" && x.CODE == viewModel.exec_action)?.CODE_VALUE;
                            //}
                            //_ORT0109ViewData_U.bank_acct_no = viewModel.bank_acct_no;
                            break;
                    }
                    Cache.Invalidate(CacheList.ORT0109ViewData);
                    Cache.Set(CacheList.ORT0109ViewData, ORT0109ViewData);
                    result.RETURN_FLAG = true;
                    result.Datas = ORT0109ViewData.Any(x => !x.exec_action.IsNullOrWhiteSpace());
                }
                catch (Exception ex)
                {
                    result.DESCRIPTION = MessageType.sys_Error.GetDescription();
                    NLog.LogManager.GetCurrentClassLogger().Error(ex.ToString());
                }
            }
            return Json(result);
        }

        public JsonResult ApplyData(string bank_acct_no = null)
        {
            var ORT0109ViewData = (List<ORT0109ViewModel>)Cache.Get(CacheList.ORT0109ViewData);
            if (!ORT0109ViewData.Any(x => !x.exec_action.IsNullOrWhiteSpace()))
            {
                return Json(new MSGReturnModel() { DESCRIPTION = MessageType.not_Find_Audit_Data.GetDescription() });
            }
            MSGReturnModel _result = new MSGReturnModel();
            try
            {
                _result = ORT0109.ApplyDeptData(ORT0109ViewData.Where(x => !x.exec_action.IsNullOrWhiteSpace()), AccountController.CurrentUserId);
                if (_result.RETURN_FLAG)
                    searchORT0109(bank_acct_no);
            }
            catch (Exception ex)
            {
                _result.DESCRIPTION = MessageType.sys_Error.GetDescription();
                NLog.LogManager.GetCurrentClassLogger().Error(ex.ToString());
            }
            return Json(_result);
        }

        private MSGReturnModel searchORT0109(string bank_acct_no = null)
        {
            MSGReturnModel result = new MSGReturnModel();
            var _result = ORT0109.GetSearchData(bank_acct_no);
            if (_result.RETURN_FLAG)
            {
                Cache.Invalidate(CacheList.ORT0109ViewData);
                Cache.Set(CacheList.ORT0109ViewData, _result.Datas);
            }
            result.RETURN_FLAG = _result.RETURN_FLAG;
            result.DESCRIPTION = _result.DESCRIPTION;
            return result;
        }

        [HttpPost]
        public JsonResult GetCacheData(jqGridParam jdata, string type)
        {
            switch (type)
            {
                case "ORT0109Model":
                    return Json(jdata.modelToJqgridResult(((List<ORT0109ViewModel>)Cache.Get(CacheList.ORT0109ViewData))
                        ?.OrderBy(x => x.bank_acct_no).ToList()));
            }
            return null;
        }
    }
}