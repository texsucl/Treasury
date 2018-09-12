﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Treasury.Web.Service.Actual;
using Treasury.Web.Service.Interface;
using Treasury.Web.ViewModels;
using Treasury.WebUtility;

/// <summary>
/// 功能說明：金庫進出管理作業-金庫登記簿執行作業-開庫前
/// 初版作者：20180907 侯蔚鑫
/// 修改歷程：20180907 侯蔚鑫 
///           需求單號：
///           初版
/// ==============================================
/// 修改日期/修改人：
/// 需求單號：
/// 修改內容：
/// ==============================================
/// </summary>
/// 
namespace Treasury.Web.Controllers
{
    public class BeforeOpenTreasuryController : CommonController
    {
        // GET: BeforeOpenTreasury
        private IBeforeOpenTreasury BeforeOpenTreasury;
        public BeforeOpenTreasuryController()
        {
            BeforeOpenTreasury = new BeforeOpenTreasury();
        }

        /// <summary>
        /// 畫面初始
        /// </summary>
        /// <returns></returns>	
        public ActionResult Index()
        {
            ViewBag.dOpen_Trea_Type = new SelectList(BeforeOpenTreasury.GetOpenTreaType(), "Value", "Text");
            ViewBag.opScope = GetopScope("~/BeforeOpenTreasury/");
            resetBeforeOpenTreasuryViewModel();

            var RoutineData = (List<BeforeOpenTreasuryViewModel>)Cache.Get(CacheList.BeforeOpenTreasuryRoutine);

            if (RoutineData.Count <= 0)
            {
                ViewBag.ShowRoutine = false;
            }
            else
            {
                ViewBag.ShowRoutine = true;
            }

            return View();
        }

        /// <summary>
        /// 產生工作底稿
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public JsonResult DraftData()
        {
            MSGReturnModel<IEnumerable<ITreaItem>> result = new MSGReturnModel<IEnumerable<ITreaItem>>();

            result = BeforeOpenTreasury.DraftData();

            return Json(result);
        }

        /// <summary>
        /// jqgrid cache data
        /// </summary>
        /// <param name="jdata"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult GetCacheData(jqGridParam jdata, string type)
        {
            switch (type)
            {
                case "Routine":
                    if (Cache.IsSet(CacheList.BeforeOpenTreasuryRoutine))
                        return Json(jdata.modelToJqgridResult(((List<BeforeOpenTreasuryViewModel>)Cache.Get(CacheList.BeforeOpenTreasuryRoutine)).OrderBy(x => x.vItem_Desc).ToList()));
                    break;
                case "Storage":
                    if (Cache.IsSet(CacheList.BeforeOpenTreasuryStorage))
                        return Json(jdata.modelToJqgridResult(((List<BeforeOpenTreasuryViewModel>)Cache.Get(CacheList.BeforeOpenTreasuryStorage)).OrderBy(x => x.vAply_No).ToList()));
                    break;
            }
            return null;
        }

        /// <summary>
        /// 設定開庫前Cache資料
        /// </summary>
        /// <returns></returns>
        private void resetBeforeOpenTreasuryViewModel()
        {
            Cache.Invalidate(CacheList.BeforeOpenTreasuryRoutine);
            Cache.Invalidate(CacheList.BeforeOpenTreasuryStorage);

            //設定資料
            Cache.Set(CacheList.BeforeOpenTreasuryRoutine, BeforeOpenTreasury.GetRoutineList());
            Cache.Set(CacheList.BeforeOpenTreasuryStorage, BeforeOpenTreasury.GetStorageList());

        }
    }
}