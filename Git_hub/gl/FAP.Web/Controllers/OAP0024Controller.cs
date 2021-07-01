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
/// 功能說明：應付票據簽收資料–維護(尚未簽收)
/// 初版作者：20191125 Mark
/// 修改歷程：20191125 Mark
///           需求單號：
///           初版
/// </summary>
///

namespace FAP.Web.Controllers
{
    public class OAP0024Controller : CommonController
    {
        private IOAP0024 OAP0024;
        public OAP0024Controller()
        {
            OAP0024 = new OAP0024();
        }
        // GET: OAP0023
        public ActionResult Index()
        {
            UserAuthUtil authUtil = new UserAuthUtil();
            string opScope = "";
            string funcName = "";
            if (Session["UserID"] != null)
            {
                String[] roleInfo = authUtil.chkUserFuncAuth(Session["UserID"].ToString(), "~/OAP0024/");
                if (roleInfo != null && roleInfo.Length == 2)
                {
                    opScope = roleInfo[0];
                    funcName = roleInfo[1];
                }
            }
            ViewBag.opScope = opScope;
            ViewBag.funcName = funcName;
            ViewBag.SRCE_FROM = new SysCodeDao().loadSelectList("AP", "SRCE_FROM", true,null,true);//資料來源            
            ViewBag.DEPT = new OAP0023().GetDepGroup();
            return View();
        }

        /// <summary>
        /// 查詢 應付票據簽收資料–維護(尚未簽收)
        /// </summary>
        /// <param name="searchModel"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult qryOAP0024(OAP0024SearchModel searchModel)
        {
            MSGReturnModel result = new MSGReturnModel();
            try
            {
                Cache.Invalidate(CacheList.OAP0024SearchData);
                Cache.Set(CacheList.OAP0024SearchData, searchModel);
                result = searchOAP0024(searchModel);
            }
            catch (Exception ex)
            {
                result.DESCRIPTION = MessageType.sys_Error.GetDescription();
                NLog.LogManager.GetCurrentClassLogger().Error(ex.ToString());
            }
            return Json(result);
        }

        [HttpPost]
        private MSGReturnModel searchOAP0024(OAP0024SearchModel searchModel = null)
        {
            MSGReturnModel result = new MSGReturnModel();
            if (searchModel == null)
                searchModel = (OAP0024SearchModel)Cache.Get(CacheList.OAP0024SearchData);
            if (searchModel != null)
            {
                var _result = OAP0024.Search_OAP0024(searchModel);
                result.RETURN_FLAG = _result.RETURN_FLAG;
                result.DESCRIPTION = _result.DESCRIPTION;
                Cache.Invalidate(CacheList.OAP0024ViewData);
                if(result.RETURN_FLAG && _result.Datas.Any())
                    Cache.Set(CacheList.OAP0024ViewData, _result.Datas);              
            }
            return result;
        }

        /// <summary>
        /// 修改 應付票據簽收資料
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult updateOAP0024(OAP0024Model model)
        {
            MSGReturnModel result = new MSGReturnModel();
            try
            {
                result = OAP0024.updateOAP0024(model);
                if (result.RETURN_FLAG)
                {
                    searchOAP0024();
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
        /// 刪除 應付票據簽收資料
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult deleteOAP0024(OAP0024Model model)
        {
            MSGReturnModel result = new MSGReturnModel();
            try
            {
                result = OAP0024.deleteOAP0024(model);
                if (result.RETURN_FLAG)
                {
                    searchOAP0024();
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
        /// 依部門找尋 應付票據簽收窗口 明細檔
        /// </summary>
        /// <param name="dep_id"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult getDepts(string dep_id)
        {
            var result = OAP0024.getupdateDatas(dep_id);
            return Json(result);
        }

        [HttpPost]
        public JsonResult GetCacheData(jqGridParam jdata, string type)
        {
            switch (type)
            {
                case "OAP0024ViewData":
                    var OAP0024ViewData = (List<OAP0024Model>)Cache.Get(CacheList.OAP0024ViewData);
                    return Json(jdata.modelToJqgridResult(OAP0024ViewData));
            }
            return null;
        }
    }
}