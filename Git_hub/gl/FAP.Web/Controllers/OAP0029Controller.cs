using FAP.Web.BO;
using FAP.Web.Daos;
using FAP.Web.Service.Actual;
using FAP.Web.Service.Interface;
using FAP.Web.Utilitys;
using FAP.Web.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using static FAP.Web.Enum.Ref;
using static FAP.Web.BO.Utility;
using FAP.Web.ActionFilter;

/// <summary>
/// 功能說明：OAP0029 應付票據抽票結果回覆功能
/// 初版作者：20200117 張家華
/// 修改歷程：20200117 張家華 
///           需求單號：
///           初版
/// ==============================================
/// 修改日期/修改人：
/// 需求單號：
/// 修改內容：
/// ==============================================
/// </summary>

namespace FAP.Web.Controllers
{
    [Authorize]
    [CheckSessionFilterAttribute]
    public class OAP0029Controller : CommonController
    {
        private IOAP0029 OAP0029;

        public OAP0029Controller()
        {
            OAP0029 = new OAP0029();
        }

        // GET: OAP0029
        public ActionResult Index()
        {
            UserAuthUtil authUtil = new UserAuthUtil();

            string opScope = "";
            string funcName = "";

            if (Session["UserID"] != null)
            {
                String[] roleInfo = authUtil.chkUserFuncAuth(Session["UserID"].ToString(), "~/OAP0029/");
                if (roleInfo != null && roleInfo.Length == 2)
                {
                    opScope = roleInfo[0];
                    funcName = roleInfo[1];
                }
            }
            ViewBag.opScope = opScope;
            ViewBag.funcName = funcName;
            ViewBag.sendStyle_A = new SelectList(new Service.Actual.Common().GetSysCode("AP", "SEND_STYLE", true), "Value", "Text");
            var _REJ_RSN = new Service.Actual.Common().GetSysCode("AP", "REJ_RSN");
            _REJ_RSN.Insert(0, new SelectOption() { Value = " ", Text = "請選擇" });
            ViewBag.REJ_RSN = new SelectList(_REJ_RSN, "Value", "Text");
            string _Q32 = string.Empty;
            try
            {
                _Q32 = GetLRTCODE("BKMSG_CKQ", "Q32");
            }
            catch (Exception ex)
            {
                NLog.LogManager.GetCurrentClassLogger().Error(ex.ToString());
            }
            ViewBag.Q32 = _Q32;
            return View();
        }

        /// <summary>
        /// 查詢
        /// </summary>
        /// <param name="searchModel"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult qryOAP0029(OAP0029SearchModel searchModel)
        {
            Cache.Invalidate(CacheList.OAP0029SearchData);
            Cache.Set(CacheList.OAP0029SearchData, searchModel);
            MSGReturnModel result = new MSGReturnModel();
            try
            {
                var model = searchOAP0029(searchModel);
                result.RETURN_FLAG = model;
                if (!model)
                    result.DESCRIPTION = MessageType.not_Find_Any.GetDescription();
            }
            catch (Exception ex)
            {
                result.DESCRIPTION = MessageType.sys_Error.GetDescription();
                NLog.LogManager.GetCurrentClassLogger().Error(ex.ToString());
            }        
            return Json(result);
        }

        /// <summary>
        /// 申請 案件成功
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public JsonResult ApplyData()
        {
            MSGReturnModel result = new MSGReturnModel();
            try
            {
                var ViewData = (List<OAP0029ViewModel>)Cache.Get(CacheList.OAP0029ViewData);
                var _ViewData = ViewData.Where(x => x.Ischecked).ToList();
                if (_ViewData.Any())
                {
                    if (_ViewData.Any(x => x.check_no.IsNullOrWhiteSpace()))
                    {
                        result.DESCRIPTION = "核准的資料,支票號碼不能為空值!!";
                    }
                    else
                    {
                        foreach (var item in _ViewData)
                        {
                            item.ce_result_status = "Y";
                            item.rej_rsn = string.Empty;
                        }
                        result = OAP0029.ApplyDeptData(_ViewData, AccountController.CurrentUserId);
                    }
                }
                else
                {
                    result.DESCRIPTION = MessageType.not_Find_Any.GetDescription();
                }
                if (result.RETURN_FLAG)
                    searchOAP0029();
            }
            catch (Exception ex)
            {
                result.DESCRIPTION = MessageType.sys_Error.GetDescription();
                NLog.LogManager.GetCurrentClassLogger().Error(ex.ToString());
            }
            return Json(result);
        }

        /// <summary>
        /// 申請 案件退回
        /// </summary>
        /// <param name="rej_rsn">退件原因</param>
        /// <param name="memo">說明</param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult RejData(string rej_rsn , string memo)
        {
            MSGReturnModel result = new MSGReturnModel();
            if (rej_rsn == "c" && memo.IsNullOrWhiteSpace())
            {
                result.DESCRIPTION = "當原因為其他時,一定要有說明!!";
                return Json(result);
            }
            try
            {
                var ViewData = (List<OAP0029ViewModel>)Cache.Get(CacheList.OAP0029ViewData);
                var _ViewData = ViewData.Where(x => x.Ischecked).ToList();
                var _REJ_RSN = new Service.Actual.Common().GetSysCode("AP", "REJ_RSN");
                var _rej_rsn = rej_rsn == "c" ? memo : _REJ_RSN.FirstOrDefault(x => x.Value == rej_rsn)?.Text;
                if (_ViewData.Any())
                {
                    foreach (var item in _ViewData)
                    {
                        item.ce_result_status = "R";
                        item.rej_rsn = _rej_rsn;
                    }
                    result = OAP0029.ApplyDeptData(_ViewData, AccountController.CurrentUserId);
                }
                else
                {
                    result.DESCRIPTION = MessageType.not_Find_Any.GetDescription();
                }
                if (result.RETURN_FLAG)
                    searchOAP0029();
            }
            catch (Exception ex)
            {
                result.DESCRIPTION = MessageType.sys_Error.GetDescription();
                NLog.LogManager.GetCurrentClassLogger().Error(ex.ToString());
            }
            return Json(result);
        }

        /// <summary>
        /// 勾選觸發事件
        /// </summary>
        /// <param name="apply_no"></param>
        /// <param name="flag"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult CheckChange(string apply_no, bool flag)
        {
            MSGReturnModel<Tuple<bool,string,string,string>> result = new MSGReturnModel<Tuple<bool,string,string,string>>();
            var ViewData = (List<OAP0029ViewModel>)Cache.Get(CacheList.OAP0029ViewData);
            var _ViewData = ViewData.FirstOrDefault(x => x.apply_no == apply_no);
            if (_ViewData != null)
            {
                result.RETURN_FLAG = true;
                _ViewData.Ischecked = flag;
                var _checkDatas = ViewData.Where(x => x.Ischecked).ToList();
                var _REJ_RSNs = _checkDatas.Select(x => x.rej_rsn).Distinct().ToList();
                var str1 = " "; //退件原因
                var str2 = ""; //其他說明
                var str3 = ""; //顯示Q32 訊息
                str3 = _checkDatas.Any(x => x.scre_from == "系統") ? "Y" : "N";
                if (_REJ_RSNs.Count == 1 && !_REJ_RSNs.First().IsNullOrWhiteSpace())
                {
                    var _REJ_RSN = new Service.Actual.Common().GetSysCode("AP", "REJ_RSN");                                      
                    var _value = _REJ_RSNs.First().Trim();
                    var _v = _REJ_RSN.FirstOrDefault(x => x.Text == _value);
                    if (_v != null && _v.Value != "c") //a or b
                    {
                        str1 = _v.Value;                      
                    }
                    else if(_v.Value == "c") //c
                    {
                        str1 = _v.Value; //其他
                        var _memos = _checkDatas.Where(x => !x.memo.IsNullOrWhiteSpace()).Select(x => x.memo).ToList();
                        var _memo = _memos.Distinct().ToList();
                        if (_memos.Count == _checkDatas.Count && _memo.Count == 1)
                        {
                            str2 = _memo.First();
                        }
                    }
                }
                result.Datas = new Tuple<bool, string, string,string>(
                    ViewData.Any(x => x.Ischecked), str1, str2, str3) ;
                Cache.Invalidate(CacheList.OAP0029ViewData);
                Cache.Set(CacheList.OAP0029ViewData, ViewData);
            }
            else
            {
                result.DESCRIPTION = MessageType.parameter_Error.GetDescription();
            }
            return Json(result);
        }

        /// <summary>
        /// 查詢支票明細資料
        /// </summary>
        /// <param name="applyNo"></param>
        /// <param name="checkNo"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult GetCheckNo(string applyNo , string checkNo)
        {
            MSGReturnModel<OAP0029ViewModel> result = new MSGReturnModel<OAP0029ViewModel>();
            try
            {
                result = CheckNoExec(applyNo, checkNo);
            }
            catch (Exception ex)
            {
                result.DESCRIPTION = MessageType.sys_Error.GetDescription();
                NLog.LogManager.GetCurrentClassLogger().Error(ex.ToString());
            }
            return Json(result);
        }

        /// <summary>
        /// 更新支票
        /// </summary>
        /// <param name="applyNo"></param>
        /// <param name="checkNo"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult updateOAP0029(string applyNo, string checkNo)
        {
            MSGReturnModel result = new MSGReturnModel();
            try
            {
                if (!checkNo.IsNullOrWhiteSpace())
                {
                    var _checkno = CheckNoExec(applyNo, checkNo);
                    if (!_checkno.RETURN_FLAG) //傳入的支票號碼 查詢不到
                    {
                        result.DESCRIPTION = "輸入的支票號碼,查詢失敗!";
                        return Json(result);
                    }
                }
                var ViewData = (List<OAP0029ViewModel>)Cache.Get(CacheList.OAP0029ViewData);
                List<string> checkeds = ViewData.Where(x => x.Ischecked).Select(x => x.apply_no).ToList();
                var _ViewData = ViewData.FirstOrDefault(x => x.apply_no == applyNo);
                if (_ViewData != null)
                {
                    _ViewData.check_no = checkNo;
                    result = OAP0029.setCheckNo(_ViewData, AccountController.CurrentUserId);
                    if (result.RETURN_FLAG)
                    {
                        searchOAP0029();
                        ViewData = (List<OAP0029ViewModel>)Cache.Get(CacheList.OAP0029ViewData);
                        foreach (var item in checkeds) //勾選狀態復原
                        {
                            var _item = ViewData.FirstOrDefault(x => x.apply_no == item);
                            if (_item != null)
                            {
                                _item.Ischecked = true;
                            }
                        }
                        Cache.Invalidate(CacheList.OAP0029ViewData);
                        Cache.Set(CacheList.OAP0029ViewData, ViewData);
                    }
                }
                else
                {
                    result.DESCRIPTION = MessageType.parameter_Error.GetDescription();
                }
            }
            catch (Exception ex)
            {
                result.DESCRIPTION = MessageType.sys_Error.GetDescription();
                NLog.LogManager.GetCurrentClassLogger().Error(ex.ToString());
            }
            return Json(result);
        }

        /// <summary>
        /// 查詢支票號碼 相關訊息
        /// </summary>
        /// <param name="applyNo"></param>
        /// <param name="checkNo"></param>
        /// <returns></returns>
        private MSGReturnModel<OAP0029ViewModel> CheckNoExec(string applyNo, string checkNo)
        {
            var ViewData = (List<OAP0029ViewModel>)Cache.Get(CacheList.OAP0029ViewData);
            var _ViewData = ViewData.FirstOrDefault(x => x.apply_no == applyNo);
            if (_ViewData != null)
            {
                _ViewData.check_no = checkNo?.Trim();
                return OAP0029.getCheckNo(_ViewData);
            }
            else
            {
                return new MSGReturnModel<OAP0029ViewModel>() { DESCRIPTION = MessageType.parameter_Error.GetDescription() };
            }
        }

        private bool searchOAP0029(OAP0029SearchModel searchModel = null)
        {
            List<OAP0029ViewModel> result = new List<OAP0029ViewModel>();
            if (searchModel == null)
                searchModel = (OAP0029SearchModel)Cache.Get(CacheList.OAP0029SearchData);
            if (searchModel != null)
            {
                result = OAP0029.GetSearchData(searchModel,AccountController.CurrentUserId);
                Cache.Invalidate(CacheList.OAP0029ViewData);
                Cache.Set(CacheList.OAP0029ViewData, result);
            }
            return result.Any();
        }

        [HttpPost]
        public JsonResult GetCacheData(jqGridParam jdata, string type)
        {
            switch (type)
            {
                case "OAP0029ViewData":
                    var OAP0029ViewData = (List<OAP0029ViewModel>)Cache.Get(CacheList.OAP0029ViewData);
                    return Json(jdata.modelToJqgridResult(OAP0029ViewData));
            }
            return null;
        }
    }
}