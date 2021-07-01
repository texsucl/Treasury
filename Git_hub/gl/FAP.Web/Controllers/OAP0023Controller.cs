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

/// <summary>
/// 功能說明：應付票據–轉入支票簽收 (簽收資料個案產出)
/// 初版作者：20191125 Mark
/// 修改歷程：20191125 Mark
///           需求單號：
///           初版
/// </summary>
/// 

namespace FAP.Web.Controllers
{
    public class OAP0023Controller : CommonController
    {
        private IOAP0023 OAP0023;
        public OAP0023Controller()
        {
            OAP0023 = new OAP0023();
        }
        // GET: OAP0023
        public ActionResult Index()
        {
            UserAuthUtil authUtil = new UserAuthUtil();
            string opScope = "";
            string funcName = "";
            if (Session["UserID"] != null)
            {
                String[] roleInfo = authUtil.chkUserFuncAuth(Session["UserID"].ToString(), "~/OAP0023/");
                if (roleInfo != null && roleInfo.Length == 2)
                {
                    opScope = roleInfo[0];
                    funcName = roleInfo[1];
                }
            }
            ViewBag.opScope = opScope;
            ViewBag.funcName = funcName;
            ViewBag.DEPT = OAP0023.GetDepGroup();
            return View();
        }

        /// <summary>
        /// 查詢 應付票據–轉入支票簽收 (簽收資料個案產出)
        /// </summary>
        /// <param name="searchModel"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult qryOAP0023(OAP0023SearchModel searchModel)
        {
            MSGReturnModel<Tuple<bool, string>> result = new MSGReturnModel<Tuple<bool, string>>();
            Cache.Invalidate(CacheList.OAP0023SearchData);
            Cache.Set(CacheList.OAP0023SearchData, searchModel);
            if (searchModel.type == "1")
            {
                var date_s = searchModel.stat_date_s.DPformateTWdate(); //開票日期起
                var date_e = searchModel.stat_date_e.DPformateTWdate(); //開票日期迄
                if (searchModel.pay_class.IsNullOrWhiteSpace())
                {
                    result.DESCRIPTION = "無給付類型!";
                    return Json(result);
                }
                else if (date_s.IsNullOrWhiteSpace() || date_e.IsNullOrWhiteSpace())
                {
                    result.DESCRIPTION = "請設定開庫日起迄!";
                    return Json(result);
                }
                else
                {
                    var _s = searchModel.stat_date_s.Split('/');
                    var _e = searchModel.stat_date_e.Split('/');
                    if (DateTime.Compare((new DateTime(Convert.ToInt32(_s[0]) + 1911, Convert.ToInt32(_s[1]), Convert.ToInt32(_s[2])).AddMonths(1)),
                        new DateTime(Convert.ToInt32(_e[0]) + 1911, Convert.ToInt32(_e[1]), Convert.ToInt32(_e[2]))) < 0)
                    {
                        result.DESCRIPTION = "開庫日起迄不能大於一個月!";
                        return Json(result);
                    }
                }
            }
            else if (searchModel.type == "2")
            {
                if ((searchModel.check_no_1_s.IsNullOrWhiteSpace() || searchModel.check_no_1_e.IsNullOrWhiteSpace()) &&
                    (searchModel.check_no_2_s.IsNullOrWhiteSpace() || searchModel.check_no_2_e.IsNullOrWhiteSpace()) &&
                    (searchModel.check_no_3_s.IsNullOrWhiteSpace() || searchModel.check_no_3_e.IsNullOrWhiteSpace()))
                {
                    result.DESCRIPTION = "至少需要一組支票號碼區段";
                    return Json(result);
                }                 
            }
            try
            {
                var model = searchOAP0023(searchModel);
                result.RETURN_FLAG = model.Any();
                if (result.RETURN_FLAG)
                {
                    result.Datas = new Tuple<bool, string>(model.Any(x => x.checkFlag), model.Count(x => x.checkFlag).ToString());
                    Cache.Invalidate(CacheList.OAP0023ViewData);
                    Cache.Set(CacheList.OAP0023ViewData, model);
                }
                else
                {
                    result.DESCRIPTION = MessageType.not_Find_Any.GetDescription();
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
        /// 執行轉入新增 FAPPYSN0應付票據簽收檔
        /// </summary>
        /// <param name="aptid"></param>
        /// <param name="depid"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult insertFAPPYSN0(string aptid,string depid)
        {
            MSGReturnModel<Tuple<bool,string>> result = new MSGReturnModel<Tuple<bool, string>>();
            var ViewData = (List<OAP0023Model>)Cache.Get(CacheList.OAP0023ViewData);
            var data = ViewData.Where(x => x.checkFlag).ToList();
            if (!data.Any())
            {
                result.DESCRIPTION = MessageType.not_Find_Update_Data.GetDescription();
            }
            else
            {
                try
                {
                    var _result = OAP0023.InsertFAPPYSN0(data, AccountController.CurrentUserId, aptid, depid);
                    result.RETURN_FLAG = _result.RETURN_FLAG;
                    result.DESCRIPTION = _result.DESCRIPTION;
                    if (result.RETURN_FLAG)
                    {
                        var models = searchOAP0023();
                        result.Datas = new Tuple<bool, string>(models.Any(x => x.checkFlag), models.Count(x => x.checkFlag).ToString());
                    }
                }
                catch (Exception ex)
                {
                    result.DESCRIPTION = MessageType.sys_Error.GetDescription();
                    NLog.LogManager.GetCurrentClassLogger().Error(ex.ToString());
                }
            }
            return Json(result);
        }

        private List<OAP0023Model> searchOAP0023(OAP0023SearchModel searchModel = null)
        {
            List<OAP0023Model> result = new List<OAP0023Model>();
            if (searchModel == null)
                searchModel = (OAP0023SearchModel)Cache.Get(CacheList.OAP0023SearchData);
            if (searchModel != null)
            {
                result = OAP0023.Search_OAP0023(searchModel);
                Cache.Invalidate(CacheList.OAP0023ViewData);
                Cache.Set(CacheList.OAP0023ViewData, result);
            }
            return result;
        }

        /// <summary>
        /// 勾選觸發事件
        /// </summary>
        /// <param name="check_no"></param>
        /// <param name="flag"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult CheckChange(string check_no, bool flag)
        {
            MSGReturnModel<Tuple<bool, string>> result = new MSGReturnModel<Tuple<bool, string>>();
            var ViewData = (List<OAP0023Model>)Cache.Get(CacheList.OAP0023ViewData);
            var _ViewData = ViewData.FirstOrDefault(x => x.check_no == check_no);
            if (_ViewData != null)
            {
                result.RETURN_FLAG = true;
                _ViewData.checkFlag = flag;
                result.Datas = new Tuple<bool, string>(ViewData.Any(x => x.checkFlag), ViewData.Count(x => x.checkFlag).ToString());
                Cache.Invalidate(CacheList.OAP0023ViewData);
                Cache.Set(CacheList.OAP0023ViewData, ViewData);
            }
            else
            {
                result.DESCRIPTION = MessageType.parameter_Error.GetDescription();
            }
            return Json(result);
        }


        [HttpPost]
        public JsonResult GetCacheData(jqGridParam jdata, string type)
        {
            switch (type)
            {
                case "OAP0023ViewData":
                    var OAP0023ViewData = (List<OAP0023Model>)Cache.Get(CacheList.OAP0023ViewData);
                    return Json(jdata.modelToJqgridResult(OAP0023ViewData));
            }
            return null;
        }
    }
}