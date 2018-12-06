using Treasury.WebActionFilter;
using System;
using Treasury.Web.Properties;
using System.Web.Mvc;
using Treasury.Web.Service.Interface;
using Treasury.Web.Service.Actual;
using System.Collections.Generic;
using Treasury.WebUtility;
using Treasury.Web.Controllers;
using Treasury.Web.ViewModels;
using System.Linq;
using Treasury.Web.Enum;
using static Treasury.Web.Enum.Ref;
using System.IO;

/// <summary>
/// 功能說明：金庫上傳檔案作業
/// 初版作者：20180604 張家華
/// 修改歷程：20180604 張家華 
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
    [Authorize]
    [CheckSessionFilterAttribute]
    public class FileUploadController : CommonController
    {
        IFileService FileService;

        public FileUploadController()
        {
            FileService = new FileService();
        }

        /// <summary>
        /// 申請作業 畫面初始
        /// </summary>
        /// <returns></returns>
        public ActionResult FileUpload()
        {
            var _CustodyFlag = Convert.ToBoolean(Session["CustodyFlag"]);
            ViewBag.CustodyFlag = _CustodyFlag;
            ViewBag.opScope = GetopScope("~/TreasuryAccess/");
            var jqgridBookNoInfo = new FileItemBookEstateModel().TojqGridData();
            ViewBag.jqgridBookNoColNames = jqgridBookNoInfo.colNames;
            ViewBag.jqgridBookNoColModel = jqgridBookNoInfo.colModel;
            var jqgridEstateInfo = new FileEstateModel().TojqGridData(
                new int[]{ 85,85,80,100,180,100,150,110,150,80},
                new string[] { "center", "center", "center", "center" });
            ViewBag.jqgridEstateColNames = jqgridEstateInfo.colNames;
            ViewBag.jqgridEstateColModel = jqgridEstateInfo.colModel;
            return View();
        }

        /// <summary>
        /// 查詢畫面查詢
        /// </summary>
        /// <param name="searchModel"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult Search(string userName)
        {
            MSGReturnModel<List<SelectOption>> result = new MSGReturnModel<List<SelectOption>>();
            result.RETURN_FLAG = false;
            result.DESCRIPTION = Ref.MessageType.not_Find_Any.GetDescription();
            var datas = FileService.SearchUserID(userName);
            if (datas.Any())
            {
                result.DESCRIPTION = $@"查詢到{datas.Count}筆符合資料!";
                datas.Insert(0, new SelectOption() { Text = " ", Value = " " });
                result.RETURN_FLAG = true;
                result.Datas = datas;
            }
            return Json(result);
        }

        /// <summary>
        /// 上傳Excel
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public JsonResult File_Upload()
        {
            MSGReturnModel<string> result = new MSGReturnModel<string>();
            try
            {
                ExcelName excelType = ExcelName.Marginp;

                #region 前端無傳送檔案進來


                if (!Request.Files.AllKeys.Any())
                {
                    result.RETURN_FLAG = false;
                    result.DESCRIPTION = MessageType.upload_Not_Find.GetDescription();
                    return Json(result);
                }

                var FileModel = Request.Files["UploadedFile"];
                string type = Request.Form["type"];

                excelType = EnumUtil.GetValues<ExcelName>().First(y => y.ToString() == type);

                #endregion 前端無傳送檔案進來

                #region 前端檔案大小不符或不為Excel檔案(驗證)

                //ModelState
                if (!ModelState.IsValid)
                {
                    result.RETURN_FLAG = false;
                    result.DESCRIPTION = MessageType.excel_Validate.GetDescription();
                    return Json(result);
                }

                #endregion 前端檔案大小不符或不為Excel檔案(驗證)

                #region 上傳檔案

                string pathType = Path.GetExtension(FileModel.FileName)
                                       .Substring(1); //上傳的檔案類型

                var fileName = string.Format("{0}.{1}",
                    excelType.GetDescription(),
                    pathType); //固定轉成此名稱

                //Cache.Invalidate(CacheList.A59ExcelName); //清除 Cache
                //Cache.Set(CacheList.A59ExcelName, fileName); //把資料存到 Cache

                #region 檢查是否有FileUploads資料夾,如果沒有就新增 並加入 excel 檔案

                string projectFile = Server.MapPath("~/" + SetFile.FileUploads); //專案資料夾
                string path = Path.Combine(projectFile, fileName);

                FileRelated.createFile(projectFile); //檢查是否有FileUploads資料夾,如果沒有就新增

                //呼叫上傳檔案 function
                result = FileRelated.FileUpLoadinPath<string>(path, FileModel);
                if (!result.RETURN_FLAG)
                    return Json(result);

                #endregion 檢查是否有FileUploads資料夾,如果沒有就新增 並加入 excel 檔案

                #region 讀取Excel資料 使用ExcelDataReader 並且組成 json

                var stream = FileModel.InputStream;
                IEnumerable<IFileModel> dataModel = null;
                var Excelresult = FileService.getExcel(pathType, path, excelType);
                if (Excelresult.Item1.IsNullOrWhiteSpace())
                {
                    switch (excelType)
                    {
                        case ExcelName.Marginp:
                            dataModel = Excelresult.Item2.Cast<FileMarginpModel>().ToList();
                            break;
                        case ExcelName.Marging:
                            dataModel = Excelresult.Item2.Cast<FileMargingModel>().ToList();
                            break;
                        case ExcelName.Stock:
                            dataModel = Excelresult.Item2.Cast<FileStockModel>().ToList();
                            break;
                        case ExcelName.Itemimp:
                            dataModel = Excelresult.Item2.Cast<FileItemImpModel>().ToList();
                            break;
                    }                
                }
                else
                {
                    result.RETURN_FLAG = false;
                    result.DESCRIPTION = Excelresult.Item1;
                    return Json(result);
                }
                if (dataModel.Any())
                {
                    result.RETURN_FLAG = true;
                    
                    Cache.Invalidate(CacheList.ExcelfileData); //清除 Cache
                    Cache.Set(CacheList.ExcelfileData, dataModel); //把資料存到 Cache
                }
                else
                {
                    result.RETURN_FLAG = false;
                    result.DESCRIPTION = MessageType.data_Not_Compare.GetDescription();
                }

                #endregion 讀取Excel資料 使用ExcelDataReader 並且組成 json

                #endregion 上傳檔案
            }
            catch (Exception ex)
            {
                result.RETURN_FLAG = false;
                result.DESCRIPTION = ex.Message;
            }
            return Json(result);
        }

        /// <summary>
        /// 轉檔
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult Transfer(string type)
        {
            MSGReturnModel<string> result = new MSGReturnModel<string>();

            ExcelName excelType = ExcelName.Marginp;
            excelType = EnumUtil.GetValues<ExcelName>().First(y => y.ToString() == type);

            IEnumerable<IFileModel> datas = null;

            switch (excelType)
            {
                case ExcelName.Marginp:
                    datas = (List<FileMarginpModel>)Cache.Get(CacheList.ExcelfileData);
                    break;
                case ExcelName.Marging:
                    datas = (List<FileMargingModel>)Cache.Get(CacheList.ExcelfileData);
                    break;
                case ExcelName.Itemimp:
                    datas = (List<FileItemImpModel>)Cache.Get(CacheList.ExcelfileData);
                    break;
                case ExcelName.Stock:
                    datas = (List<FileStockModel>)Cache.Get(CacheList.ExcelfileData);
                    break;
            }

            result = FileService.saveData(datas, excelType);

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
                case "BookNo":
                    return Json(jdata.modelToJqgridResult((List<FileItemBookEstateModel>)Cache.Get(CacheList.ExcelfileData)));
                case "Estate":
                    return Json(jdata.modelToJqgridResult((List<FileEstateModel>)Cache.Get(CacheList.ExcelfileData)));
                case "Marginp":
                    return Json(jdata.modelToJqgridResult((List<FileMarginpModel>)Cache.Get(CacheList.ExcelfileData)));
                case "Marging":
                    return Json(jdata.modelToJqgridResult((List<FileMargingModel>)Cache.Get(CacheList.ExcelfileData)));
                case "Itemimp":
                    return Json(jdata.modelToJqgridResult((List<FileItemImpModel>)Cache.Get(CacheList.ExcelfileData)));
                case "Stock":
                    return Json(jdata.modelToJqgridResult((List<FileStockModel>)Cache.Get(CacheList.ExcelfileData)));
            }           
           return null;
        }
    }
}