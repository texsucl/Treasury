using FGL.Web.ActionFilter;
using FGL.Web.BO;
using FGL.Web.Service.Actual;
using FGL.Web.Service.Interface;
using FGL.Web.Utilitys;
using FGL.Web.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using static FGL.Web.BO.Utility;
using static FGL.Web.Enum.Ref;

/// <summary>
/// 功能說明：退費類別維護
/// 初版作者：20200712 Mark
/// 修改歷程：20200712 Mark
///           需求單號：
///           初版
/// </summary>
///

namespace FGL.Web.Controllers
{
    [Authorize]
    [CheckSessionFilterAttribute]
    public class OGL00010Controller : CommonController
    {
        static private NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        private IOGL00010 OGL00010;

        public OGL00010Controller()
        {
            OGL00010 = new OGL00010();
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
                String[] roleInfo = authUtil.chkUserFuncAuth(Session["UserID"].ToString(), "~/OGL00010/");
                if (roleInfo != null && roleInfo.Length == 2)
                {
                    opScope = roleInfo[0];
                    funcName = roleInfo[1];
                }
            }

            ViewBag.opScope = opScope;
            ViewBag.funcName = funcName;

            return View();
        }

        /// <summary>
        /// 退費類別設定明細檔
        /// </summary>
        /// <param name="pk_id">pk_id</param>
        /// <param name="action">執行狀態</param>
        /// <param name="apprFlag">覆核權限</param>
        /// <returns></returns>
        public ActionResult Detail(string pk_id, string action,string apprFlag = "N")
        {
            var data = new OGL00010ViewModel();
            var OGL00010ViewDatas = (List<OGL00010ViewModel>)Cache.Get(CacheList.OGL00010ViewData);
            if (action == "APPR")
            {
                OGL00010ViewDatas = (List<OGL00010ViewModel>)Cache.Get(CacheList.OGL00010AViewData);
            }
            List<OGL00010ViewSubModel> _subDatas = new List<OGL00010ViewSubModel>();
            if (!pk_id.IsNullOrWhiteSpace())
            {
                data = OGL00010ViewDatas.FirstOrDefault(x => x.pk_id == pk_id);
                if (data != null)
                {
                    _subDatas = data.SubDatas;
                }
                data = data ?? new OGL00010ViewModel();
            }

            Cache.Invalidate(CacheList.OGL00010ViewSubData);
            Cache.Set(CacheList.OGL00010ViewSubData, _subDatas);

            var _V = new List<SelectOption>() {
                new SelectOption() { Text = " ", Value = " " },
                new SelectOption() { Text = "V", Value = "V" }
            };
            var _X = new List<SelectOption>() {
                new SelectOption() { Text = " ", Value = " "},
                new SelectOption() { Text = "X", Value = "X"}
            };
            ViewBag.action = action;
            ViewBag.item_yn = new SelectList(_V, "Value", "Text", data.item_yn_n); ; //險種否
            ViewBag.year_yn = new SelectList(_V, "Value", "Text", data.year_yn_n); ; //年次否
            ViewBag.prem_yn = new SelectList(_V, "Value", "Text", data.prem_yn_n); ; //保費類別否
            ViewBag.unit_yn = new SelectList(_V, "Value", "Text", data.unit_yn_n); ; //費用單位否
            ViewBag.recp_yn = new SelectList(_V, "Value", "Text", data.recp_yn_n); ; //送金單否
            ViewBag.cont_yn = new SelectList(_V, "Value", "Text", data.cont_yn_n); ; //合約別否
            ViewBag.corp_yn = new SelectList(_X, "Value", "Text", data.corp_yn_n); ; //帳本否
            List<SelectOption> se = new List<SelectOption>() { new SelectOption() { Text = " ", Value = " " } };
            ViewBag.prem_kind = new SelectList(new Service.Actual.Common().GetSysCode("GL", "PREM_KIND", false, se), "Value", "Text");  //保費類別
            ViewBag.cont_type = new SelectList(new Service.Actual.Common().GetSysCode("GL", "CONT_TYPE", false, se), "Value", "Text"); //合約別
            ViewBag.prod_type = new SelectList(new Service.Actual.Common().GetSysCode("GL", "PROD_TYPE", false, se), "Value", "Text"); //商品別
            ViewBag.corp_no = new SelectList(new Service.Actual.Common().GetSysCode("GL", "CORP_NO", false, se), "Value", "Text"); //帳本別
            ViewBag.actnum_yn = new SelectList(_V, "Value", "Text"); ; //合約別否
            ViewBag.apprFlag = apprFlag;
            return PartialView(data);  
        }

        /// <summary>
        /// 查詢 退費類別設定明細檔
        /// </summary>
        /// <param name="payclass"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult qryOGL00010(string payclass)
        {
            Cache.Invalidate(CacheList.OGL00010SearchData);
            Cache.Set(CacheList.OGL00010SearchData, payclass);
            MSGReturnModel result = new MSGReturnModel();
            result = searchOGL00010(payclass);
            return Json(result);
        }

        /// <summary>
        /// 新增 刪除 修改 明細資料
        /// </summary>
        /// <param name="exec_action">執行動作</param>
        /// <param name="subModel">執行資料</param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult execUpdateSubData(string exec_action, OGL00010ViewSubModel subModel)
        {
            MSGReturnModel<bool> result = new MSGReturnModel<bool>();
            var OGL00010ViewSubDatas = (List<OGL00010ViewSubModel>)Cache.Get(CacheList.OGL00010ViewSubData);
            var _exec_action = new Service.Actual.Common().GetSysCode("GL", "EXEC_ACTION", false);
            var _prem_kind = new Service.Actual.Common().GetSysCode("GL", "PREM_KIND", false);
            var _cont_type = new Service.Actual.Common().GetSysCode("GL", "CONT_TYPE", false);
            var _prod_type = new Service.Actual.Common().GetSysCode("GL", "PROD_TYPE", false);
            if (((exec_action == "A") || (exec_action == "U") ) && checkSameData(subModel))
            {
                result.DESCRIPTION = $@"有相同Key值的資料!";
                return Json(result);
            }
            else
            {
                switch (exec_action)
                {
                    case "A":
                        subModel.exec_action = exec_action;
                        subModel.exec_action_D = _exec_action.FirstOrDefault(x => x.Value == exec_action)?.Text;
                        subModel.prem_kind_n_D = _prem_kind.FirstOrDefault(x => x.Value == subModel.prem_kind_n)?.Text;
                        subModel.cont_type_n_D = _cont_type.FirstOrDefault(x => x.Value == subModel.cont_type_n)?.Text;
                        subModel.prod_type_n_D = _prod_type.FirstOrDefault(x => x.Value == subModel.prod_type_n)?.Text;
                        OGL00010ViewSubDatas.Add(subModel);
                        break;
                    case "U":
                        var _OGL00010ViewSubData_upt = OGL00010ViewSubDatas.FirstOrDefault(x => x.pk_id == subModel.pk_id);
                        if (_OGL00010ViewSubData_upt != null)
                        {
                            if (_OGL00010ViewSubData_upt.exec_action != "A")
                            {
                                _OGL00010ViewSubData_upt.exec_action = "U";
                                _OGL00010ViewSubData_upt.exec_action_D = _exec_action.FirstOrDefault(x => x.Value == _OGL00010ViewSubData_upt.exec_action)?.Text;
                            }
                            _OGL00010ViewSubData_upt.prem_kind_n = subModel.prem_kind_n; //保費類別
                            _OGL00010ViewSubData_upt.prem_kind_n_D = _prem_kind.FirstOrDefault(x => x.Value == _OGL00010ViewSubData_upt.prem_kind_n)?.Text;
                            _OGL00010ViewSubData_upt.cont_type_n = subModel.cont_type_n; //合約別
                            _OGL00010ViewSubData_upt.cont_type_n_D = _cont_type.FirstOrDefault(x => x.Value == _OGL00010ViewSubData_upt.cont_type_n)?.Text;
                            _OGL00010ViewSubData_upt.prod_type_n = subModel.prod_type_n; //商品別
                            _OGL00010ViewSubData_upt.prod_type_n_D = _prod_type.FirstOrDefault(x => x.Value == _OGL00010ViewSubData_upt.prod_type_n)?.Text;
                            _OGL00010ViewSubData_upt.corp_no_n = subModel.corp_no_n; //帳本別
                            _OGL00010ViewSubData_upt.actnum_yn_n = subModel.actnum_yn_n; //取會科否
                            _OGL00010ViewSubData_upt.acct_code_n = subModel.acct_code_n; //保費收入首年首期
                            _OGL00010ViewSubData_upt.acct_codef_n = subModel.acct_codef_n; //保費收入首年續期
                            _OGL00010ViewSubData_upt.acct_coder_n = subModel.acct_coder_n; //續年度  
                        }
                        else
                        {
                            result.DESCRIPTION = MessageType.parameter_Error.GetDescription();
                            return Json(result);
                        }
                        break;
                    case "D":
                        var _OGL00010ViewSubData_del = OGL00010ViewSubDatas.FirstOrDefault(x => x.pk_id == subModel.pk_id);
                        if (_OGL00010ViewSubData_del != null)
                        {
                            if (_OGL00010ViewSubData_del.exec_action == "A")
                            {
                                OGL00010ViewSubDatas.Remove(_OGL00010ViewSubData_del);
                            }
                            else
                            {
                                _OGL00010ViewSubData_del.exec_action = "D";
                                _OGL00010ViewSubData_del.exec_action_D = _exec_action.FirstOrDefault(x => x.Value == _OGL00010ViewSubData_del.exec_action)?.Text;
                            }
                        }
                        else
                        {
                            result.DESCRIPTION = MessageType.parameter_Error.GetDescription();
                            return Json(result);
                        }
                        break;
                    default:
                        result.DESCRIPTION = MessageType.parameter_Error.GetDescription();
                        return Json(result);
                }
                Cache.Invalidate(CacheList.OGL00010ViewSubData);
                Cache.Set(CacheList.OGL00010ViewSubData, OGL00010ViewSubDatas);
                result.RETURN_FLAG = true;
                result.Datas = OGL00010ViewSubDatas.Any(x => !x.exec_action.IsNullOrWhiteSpace());
            }
            return Json(result);
        }

        /// <summary>
        /// 申請
        /// </summary>
        /// <param name="mainModel"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult applyData(OGL00010ViewModel mainModel)
        {
            MSGReturnModel result = new MSGReturnModel();
            var OGL00010ViewSubDatas = (List<OGL00010ViewSubModel>)Cache.Get(CacheList.OGL00010ViewSubData);
            if (mainModel.pay_class.IsNullOrWhiteSpace())
            {
                result.DESCRIPTION = $@"請輸入 退費項目類別.";
                return Json(result);
            }
            if (OGL00010.CheckData(mainModel.pay_class,mainModel.exec_action))
            {
                result.DESCRIPTION = $@"有重複的 退費項目類別 在資料中或申請中.";
                return Json(result);          
            }
            var msg = checkData(mainModel, OGL00010ViewSubDatas);
            if (!msg.IsNullOrWhiteSpace())
            {
                result.DESCRIPTION = msg;
                return Json(result);
            }
            var OGL00010ViewDatas = (List<OGL00010ViewModel>)Cache.Get(CacheList.OGL00010ViewData);
            var _OGL00010ViewData = OGL00010ViewDatas.FirstOrDefault(x => x.pk_id == mainModel.pk_id);
            if (_OGL00010ViewData == null)
                _OGL00010ViewData = new OGL00010ViewModel();
            _OGL00010ViewData.exec_action = mainModel.exec_action; //執行動作
            _OGL00010ViewData.apply_status = "1"; //1.表單申請
            _OGL00010ViewData.memo_n = StringUtil.halfToFull(mainModel.memo_n);
            _OGL00010ViewData.pay_class = mainModel.pay_class; //退費項目類別
            _OGL00010ViewData.item_yn_n = mainModel.item_yn_n; //險種否(新)
            _OGL00010ViewData.year_yn_n = mainModel.year_yn_n; //年次否(新)
            _OGL00010ViewData.prem_yn_n = mainModel.prem_yn_n; //保費類別否(新)
            _OGL00010ViewData.unit_yn_n = mainModel.unit_yn_n; //費用單位否(新)  
            _OGL00010ViewData.recp_yn_n = mainModel.recp_yn_n; //送金單否(新)
            _OGL00010ViewData.cont_yn_n = mainModel.cont_yn_n; //合約別否(新)
            _OGL00010ViewData.corp_yn_n = mainModel.corp_yn_n; //帳本否(新)
            _OGL00010ViewData.SubDatas = OGL00010ViewSubDatas;
            result = OGL00010.ApplyData(new List<OGL00010ViewModel>() { _OGL00010ViewData },AccountController.CurrentUserId);
            if (result.RETURN_FLAG)
            {
                searchOGL00010(null,true);
            }
            return Json(result);
        }

        /// <summary>
        /// 勾選觸發事件
        /// </summary>
        /// <param name="aply_no"></param>
        /// <param name="flag"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult CheckChange(string pk_id, bool flag)
        {
            MSGReturnModel<bool> result = new MSGReturnModel<bool>();
            var ViewData = (List<OGL00010ViewModel>)Cache.Get(CacheList.OGL00010ViewData);
            var _ViewData = ViewData.FirstOrDefault(x => x.pk_id == pk_id);
            if (_ViewData != null)
            {
                result.RETURN_FLAG = true;
                _ViewData.Ischecked = flag;
                result.Datas = ViewData.Any(x => x.Ischecked);
                Cache.Invalidate(CacheList.OGL00010ViewData);
                Cache.Set(CacheList.OGL00010ViewData, ViewData);
            }
            else
            {
                result.DESCRIPTION = MessageType.parameter_Error.GetDescription();
            }
            return Json(result);
        }

        [HttpPost]
        public JsonResult getReportParmByCheck()
        {
            MSGReturnModel<string> result = new MSGReturnModel<string>();
            var ViewData = (List<OGL00010ViewModel>)Cache.Get(CacheList.OGL00010ViewData);
            var _ViewData = ViewData.Where(x => x.Ischecked).ToList();
            if (_ViewData.Any())
            {
                result.RETURN_FLAG = true;
                result.Datas = string.Join(";", _ViewData.Select(x => x.pk_id));
            }
            else
            {
                result.DESCRIPTION = MessageType.parameter_Error.GetDescription();
            }
            return Json(result);
        }

        /// <summary>
        /// 查詢報表參數
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public JsonResult getReportParm()
        {
            MSGReturnModel<string> result = new MSGReturnModel<string>();
            var searchData = (string)Cache.Get(CacheList.OGL00010SearchData);
            if (searchData != null)
            {
                result.RETURN_FLAG = true;
                result.Datas = searchData?.Trim();
            }
            else
                result.DESCRIPTION = MessageType.login_Time_Out.GetDescription();
            return Json(result);
        }

        /// <summary>
        /// 檢核 資料是否有符合規則
        /// </summary>
        /// <param name="mainModel"></param>
        /// <returns></returns>
        private string checkData(OGL00010ViewModel mainModel , List<OGL00010ViewSubModel> OGL00010ViewSubDatas)
        {
            string result = string.Empty;
            StringBuilder msg = new StringBuilder();
            //檢核項目
            //1.保費類別否(新) 等於'V'時 且 明細檔的保費類別有空值
            if (mainModel.prem_yn_n == "V" && OGL00010ViewSubDatas.Any(x => x.prem_kind_n.IsNullOrWhiteSpace()))
            {
                msg.AppendLine($@"保費類別否等於'V'時,明細檔的保費類別均需有值.");
            }
            //2.合約別否(新) 等於'V'時 且 明細檔的合約別有空值
            if (mainModel.cont_yn_n == "V" && OGL00010ViewSubDatas.Any(x => x.cont_type_n.IsNullOrWhiteSpace()))
            {
                msg.AppendLine($@"合約別否等於'V'時,明細檔的合約別均需有值.");
            }
            //3.帳本別否(新) 不等於'X'時 且 明細檔的帳本別有空值
            if (mainModel.corp_yn_n != "X" && OGL00010ViewSubDatas.Any(x => x.corp_no_n.IsNullOrWhiteSpace()))
            {
                msg.AppendLine($@"帳本別否不等於'X'時,明細檔的帳本別均需有值.");
            }
            //4.每筆資料 保費收入首年首期 or 保費收入首年續期 or 續年度  (最少其中一個欄位要有值=>會科號碼)
            if (OGL00010ViewSubDatas.Any
                (x => x.acct_code_n.IsNullOrWhiteSpace() &&
                      x.acct_codef_n.IsNullOrWhiteSpace() &&
                      x.acct_coder_n.IsNullOrWhiteSpace()))
            {
                msg.AppendLine($@"每筆資料 保費收入首年首期 or 保費收入首年續期 or 續年度  (最少其中一個欄位要有值=>會科號碼).");
            }
            //5.取會科否 等於'V'時 且 保費收入首年首期 or 保費收入首年續期 or 續年度 有會科長度大於4位數字  
            if (OGL00010ViewSubDatas.Any(
                x => x.actnum_yn_n == "V" &&
                (
                ((x.acct_code_n?.Length ?? 0) > 4) ||
                ((x.acct_codef_n?.Length ?? 0) > 4) ||
                ((x.acct_coder_n?.Length ?? 0) > 4)
                )
                ))
            {
                msg.AppendLine($@"取會科否 等於'V'時 保費收入首年首期 or 保費收入首年續期 or 續年度  資料長度不得大於4位數.");
            }
            //6.檢核是否有明細
            if (!OGL00010ViewSubDatas.Any())
            {
                msg.AppendLine($@"此退費類別無明細.");
            }
            if (msg.Length > 0)
               result = msg.ToString();
            return result;
        }

        /// <summary>
        /// 檢核是否有重複的資料
        /// </summary>
        /// <param name="subModel"></param>
        /// <returns></returns>
        private bool checkSameData(OGL00010ViewSubModel subModel)
        {
            return ((List<OGL00010ViewSubModel>)Cache.Get(CacheList.OGL00010ViewSubData) ?? new List<OGL00010ViewSubModel>())
                .Any(x => x.prem_kind_n == subModel.prem_kind_n &&
                          x.cont_type_n == subModel.cont_type_n &&
                          x.prod_type_n == subModel.prod_type_n &&
                          x.corp_no_n == subModel.corp_no_n &&
                          x.pk_id != subModel.pk_id);
        }

        private MSGReturnModel searchOGL00010(string payclass = null, bool saveFlag = false)
        {
            MSGReturnModel result = new MSGReturnModel();
            if (payclass == null)
                payclass = (string)Cache.Get(CacheList.OGL00010SearchData) ?? string.Empty;
            if (payclass != null)
            {
                var _result = OGL00010.GetSearchData(payclass);
                if (_result.RETURN_FLAG || saveFlag)
                {
                    Cache.Invalidate(CacheList.OGL00010ViewData);
                    Cache.Set(CacheList.OGL00010ViewData, _result.Datas);
                }
                result.RETURN_FLAG = _result.RETURN_FLAG;
                result.DESCRIPTION = _result.DESCRIPTION;
            }        
            return result;
        }

        public JsonResult GetCacheData(jqGridParam jdata, string type)
        {
            switch (type)
            {
                case "OGL00010ViewData":
                    var OGL00010ViewData = (List<OGL00010ViewModel>)Cache.Get(CacheList.OGL00010ViewData);
                    return Json(jdata.modelToJqgridResult(OGL00010ViewData));
                case "OGL00010ViewSubData":
                    var OGL00010ViewSubData = (List<OGL00010ViewSubModel>)Cache.Get(CacheList.OGL00010ViewSubData);
                    return Json(jdata.modelToJqgridResult(OGL00010ViewSubData));
            }
            return null;
        }
    }
}