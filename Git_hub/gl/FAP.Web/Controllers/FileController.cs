using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.SS.Util;
using NPOI.XSSF.UserModel;
using static FAP.Web.Enum.Ref;
using static FAP.Web.BO.Utility;
using FAP.Web.Utilitys;
using System.IO;
using FAP.Web.ViewModels;
using FAP.Web.Service.Actual;
using FAP.Web.Service.Interface;
using System.Reflection;
using System.ComponentModel;
using System.Configuration;

namespace FAP.Web.Controllers
{
    public class FileController : CommonController
    {
        private IFileService FileService;
        public FileController()
        {
            FileService = new FileService();
        }

        // GET: File
        public ActionResult Index()
        {
            return View();
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

                #region 前端無傳送檔案進來

                if (!Request.Files.AllKeys.Any())
                {
                    result.RETURN_FLAG = false;
                    result.DESCRIPTION = MessageType.upload_Not_Find.GetDescription();
                    return Json(result);
                }

                var FileModel = Request.Files["UploadedFile"];
                string excelType = Request.Form["Type"];


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
       
                #region 讀取Excel資料 使用ExcelDataReader 並且組成 json

                var stream = FileModel.InputStream;
                IEnumerable<IFileUpLoadModel> dataModel = null;
                var Excelresult = FileService.getExcel(stream , pathType, excelType);
                if (Excelresult.Item1.IsNullOrWhiteSpace())
                {
                    switch (excelType)
                    {
                        case "OAP0031":
                            dataModel = Excelresult.Item2.Cast<OAP0031ViewModel>().ToList();
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
                    Cache.Invalidate($@"UL_{excelType}"); //清除 Cache
                    Cache.Set($@"UL_{excelType}", dataModel); //把資料存到 Cache
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
                NLog.LogManager.GetCurrentClassLogger().Error(ex.exceptionMessage());
            }
            return Json(result);
        }

        [HttpGet]
        public ActionResult DownloadExcel(string type)
        {
            try
            {
                var pNameFlag = true;
                List<string> otherKeys = new List<string>();
                switch (type)
                {
                    case "OAP0031":
                        otherKeys = new List<string>()
                        {
                            "checkFlag", //選項按鈕
                            "pkid", //PKID
                            //"send_style", //寄送方式
                            //"send_style_D", //寄送方式(中文)
                            //"zip_code", //郵遞區號
                            //"apply_name", //行政單位
                            //"apply_id", //申請人員
                            //"create_date", //新增日期
                            //"rece_id", //接收人員
                        };
                        break;
                }
                var str = $@"Excel_Data_{type}.xls";
                var data = (List<IFileDownLoadModel>)Cache.Get($@"DL_{type}");
                return File(IFileDownLoadModelToExcel(data, type, pNameFlag, otherKeys), System.Web.MimeMapping.GetMimeMapping(str), str);
            }
            catch (Exception ex)
            {
                NLog.LogManager.GetCurrentClassLogger().Error(ex.exceptionMessage());
            }
            return new HttpStatusCodeResult(System.Net.HttpStatusCode.Forbidden);
        }

        /// <summary>
        /// 下載資料 轉換至 Memory
        /// </summary>
        /// <param name="models"></param>
        /// <param name="type"></param>
        /// <param name="pNameFlag"></param>
        /// <param name="otherKeys"></param>
        /// <returns></returns>
        public static MemoryStream IFileDownLoadModelToExcel(
            IEnumerable<IFileDownLoadModel> models,
            string type,
            bool pNameFlag = true,
            List<string> otherKeys = null)
        {
            try
            {
                ISheet ws;
                string version = "2003"; //default 2003
                IWorkbook wb = null;
                string configVersion = ConfigurationManager.AppSettings["ExcelVersion"];
                if (!configVersion.IsNullOrWhiteSpace())
                    version = configVersion;

                //建立Excel 2003檔案
                if ("2003".Equals(version))
                    wb = new HSSFWorkbook();

                if ("2007".Equals(version))
                    wb = new XSSFWorkbook();

                ws = wb.CreateSheet($@"Data");
                ExcelSetValue(ws, models, type, pNameFlag, otherKeys);
                MemoryStream stream = new MemoryStream();
                wb.Write(stream);
                stream.Flush();
                stream.Position = 0;
                return stream;
            }
            catch (Exception ex)
            {
                NLog.LogManager.GetCurrentClassLogger().Error(ex.ToString());
                return null;
            }
            finally
            {
                //關閉文件
                //oXL.Quit();
            }
        }

        private static void ExcelSetValue(ISheet ws, IEnumerable<IFileDownLoadModel> models, string type, bool pNameFlag = true, List<string> otherKeys = null)
        {

            List<Tuple<string, string>> colums = new List<Tuple<string, string>>();
            PropertyInfo[] pros = null;
            switch (type)
            {
                case "OAP0031":
                    pros = new OAP0031ViewModel().GetType().GetProperties();
                    break;
            }
            if (pros != null)
            {
                colums = pros.Where(x => !otherKeys.Contains(x.Name), otherKeys != null && otherKeys.Any())
                    .Select(
                        x =>
                        new Tuple<string, string>
                        (
                        x.Name,
                        (x.GetCustomAttributes(typeof(DescriptionAttribute), false)).Length > 0 ?
                        ((DescriptionAttribute)((x.GetCustomAttributes(typeof(DescriptionAttribute), false))[0])).Description :
                        string.Empty
                        )).ToList();
            }
            if (colums.Any())
            {
                var l = 0; //預設至第幾行
                if (pNameFlag) //第一行為欄位中文,第二行為欄位名稱
                {
                    ws.CreateRow(0);//第一行為欄位中文
                    ws.CreateRow(1);//第二行為欄位名稱
                    for (int i = 0; i < colums.Count; i++)
                    {
                        ws.GetRow(0).CreateCell(i).SetCellValue(colums[i].Item2); //欄位中文
                        ws.GetRow(1).CreateCell(i).SetCellValue(colums[i].Item1); //欄位名稱
                    }
                    l = 2; //預設至第二行
                }
                else //第一行為欄位
                {
                    ws.CreateRow(0);//第一行為欄位名稱
                    for (int i = 0; i < colums.Count; i++)
                    {
                        ws.GetRow(0).CreateCell(i).SetCellValue(colums[i].Item1); //欄位名稱
                    }
                    l = 1; //預設至第一行
                }
                var k = 0;
                foreach (var item in models)
                {
                    ws.CreateRow(k + l);
                    for (int j = 0; j < colums.Count; j++)
                    {
                        var p = pros.FirstOrDefault(y => y.Name.ToUpper() == colums[j].Item1.ToUpper());
                        var _data = p.GetValue(item);
                        if (_data != null)
                            ws.GetRow(k + l).CreateCell(j).SetCellValue(_data.ToString());
                    }
                    k += 1;
                }
                for (int i = 0; i < colums.Count; i++)
                {
                    ws.AutoSizeColumn(i);
                }
            }
        }
    }
}