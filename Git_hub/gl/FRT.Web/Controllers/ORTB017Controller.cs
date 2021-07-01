using FRT.Web.ActionFilter;
using FRT.Web.BO;
using FRT.Web.CacheProvider;
using FRT.Web.Daos;
using FRT.Web.Infrastructure;
using FRT.Web.Models;
using FRT.Web.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading;
using System.Web.Mvc;

/// <summary>
/// 功能說明：快速付款改FBO匯款作業(BCP-緊急應變措施)
/// 初版作者：20190220 Mark
/// 修改歷程：20190220 Mark
///           需求單號：
///           初版
/// </summary>
///

namespace FRT.Web.Controllers
{
    [Authorize]
    [CheckSessionFilterAttribute]
    public class ORTB017Controller : BaseController
    {
        static private NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        internal ICacheProvider Cache { get; set; }
        public ORTB017Controller()
        {
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
                String[] roleInfo = authUtil.chkUserFuncAuth(Session["UserID"].ToString(), "~/ORTB017/");
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

            return View();
        }

        /// <summary>
        /// 查詢是否有符合FBO的案件
        /// </summary>
        /// <param name="fastNo_S">快速付款起號</param>
        /// <param name="fastNo_E">快速付款迄號</param>
        /// <param name="filler_20">匯款轉檔批號</param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult qryORTB017(string fastNo_S, string fastNo_E, string filler_20)
        {
            MSGReturnModel<List<FBOModel>> result = new MSGReturnModel<List<FBOModel>>();
            logger.Info("qryORTB017 Query!!");
            try
            {
                result = new FRTFBODao().qryForORTB017(fastNo_S, fastNo_E, filler_20);
                if (result.RETURN_FLAG)
                {
                    PIA_LOG_MAIN piaLogMain = new PIA_LOG_MAIN();
                    PiaLogMainDao piaLogMainDao = new PiaLogMainDao();

                    piaLogMain = new PIA_LOG_MAIN();
                    piaLogMain.TRACKING_TYPE = "A";
                    piaLogMain.ACCESS_ACCOUNT = Session["UserID"]?.ToString();
                    piaLogMain.ACCOUNT_NAME = "";
                    piaLogMain.PROGFUN_NAME = "ORTB017Controller";
                    piaLogMain.EXECUTION_CONTENT = $@"fastNo_S:{fastNo_S}|fastNo_E:{fastNo_E}|filler_20:{filler_20}";
                    piaLogMain.AFFECT_ROWS = result.Datas.Count;
                    piaLogMain.PIA_TYPE = "0000000000";
                    piaLogMain.EXECUTION_TYPE = "Q";
                    piaLogMain.ACCESSOBJ_NAME = "FRTBARM";
                    piaLogMainDao.Insert(piaLogMain);

                    Cache.Invalidate("qryORTB017");
                    Cache.Set("qryORTB017", result.Datas);
                }
                else
                {
                    if (!filler_20.IsNullOrWhiteSpace() && result.DESCRIPTION.IndexOf("已有轉檔批號") > -1)
                        setReportParm(filler_20.Trim().Split('*')[0]+"*");                 
                }
                return Json(result);
            }
            catch (Exception e)
            {
                logger.Error(e.ToString());
                result.DESCRIPTION = "其它錯誤，請洽系統管理員!!";
                return Json(result);
            }
        }

        /// <summary>
        /// 檢查查詢資料是否有全部選擇
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public JsonResult CheckORTB017FBO()
        {
            MSGReturnModel<string> result = new MSGReturnModel<string>();
            result.DESCRIPTION = "閒置太久請重新登入!";
            if (Cache.IsSet("qryORTB017"))
            {
                var data = (List<FBOModel>)Cache.Get("qryORTB017");
                if (data.Any(x => x.checkFlag))
                {
                    result.RETURN_FLAG = true;
                    result.DESCRIPTION = string.Empty;
                    var _datas = data.Where(x => !x.checkFlag).ToList();
                    if (_datas.Any())
                    {
                        result.DESCRIPTION = $@"快速付款編號:{string.Join(" & ", _datas
                            .Select(x => x.FAST_NO)
                            .OrderBy(x => x))}未勾選，請確認!
若確認無誤請繼續執行轉檔作業!
(繼續作業請選 確定 ,取消作業請選 取消 )";
                    }
                }
                else
                    result.DESCRIPTION = "無執行資料!";

            }
            return Json(result);
        }

        /// <summary>
        /// 針對點選的快速付款編號執行動作
        /// </summary>
        /// <param name="filler_20">匯款轉檔批號</param>
        /// <param name="downloadType">FBO or 臨櫃</param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult ORTB017FBO(string filler_20,string downloadType)
        {
            MSGReturnModel<string> result = new MSGReturnModel<string>();
            result.DESCRIPTION = "閒置太久請重新登入!";
            if (Cache.IsSet("qryORTB017"))
            {
                var data = (List<FBOModel>)Cache.Get("qryORTB017");
                var _datas = data.Where(x => x.checkFlag).ToList();
                if (_datas.Any())
                {
                    //if (data.Count != _datas.Count)
                    //{
                    //    result.DESCRIPTION = $@"{string.Join(" & ",data
                    //        .Where(x=>!x.checkFlag)
                    //        .Select(x=>x.FAST_NO)
                    //        .OrderBy(x=>x))}未下退匯，請先執行匯款失敗作業!";
                    //}
                    //else
                    //{
                        var userId = Session["UserID"]?.ToString();
                        result = new FRTFBODao().getFiller_20(
                            _datas.Select(x => x.FAST_NO).ToList(),
                            userId,
                            _datas.Sum(x => decimal.Parse(x.REMIT_AMT)),
                            filler_20?.Trim());
                        try
                        {
                            var _filler_20 = result.Datas;
                            setReportParm(result.Datas);
                            var txtBody = new FRTFBODao().getFBOQ11txt(_filler_20, downloadType);

                            PIA_LOG_MAIN piaLogMain = new PIA_LOG_MAIN();
                            PiaLogMainDao piaLogMainDao = new PiaLogMainDao();

                            piaLogMain.TRACKING_TYPE = "A";
                            piaLogMain.ACCESS_ACCOUNT = Session["UserID"]?.ToString();
                            piaLogMain.ACCOUNT_NAME = "";
                            piaLogMain.PROGFUN_NAME = "ORTB017Controller";
                            piaLogMain.EXECUTION_CONTENT = $@"filler_20:{_filler_20}";
                            piaLogMain.AFFECT_ROWS = txtBody.Item2;
                            piaLogMain.PIA_TYPE = "0000000000";
                            piaLogMain.EXECUTION_TYPE = "R"; //報表
                            piaLogMain.ACCESSOBJ_NAME = "FRTBARM";
                            piaLogMainDao.Insert(piaLogMain);

                            piaLogMain.TRACKING_TYPE = "A";
                            piaLogMain.ACCESS_ACCOUNT = Session["UserID"]?.ToString();
                            piaLogMain.ACCOUNT_NAME = "";
                            piaLogMain.PROGFUN_NAME = "ORTB017Controller";
                            piaLogMain.EXECUTION_CONTENT = $@"filler_20:{_filler_20}";
                            piaLogMain.AFFECT_ROWS = txtBody.Item2;
                            piaLogMain.PIA_TYPE = "0000000000";
                            piaLogMain.EXECUTION_TYPE = "X"; //匯出下載
                            piaLogMain.ACCESSOBJ_NAME = "FRTBARM";
                            piaLogMainDao.Insert(piaLogMain);
                            string datFile = Server.MapPath("~/Dat"); //dat資料夾
                            _filler_20 = _filler_20.Split('*')[0];
                            var txtFileName = $@"{downloadType}Q11_{_filler_20}";
                            string path = Path.Combine(datFile, $@"{txtFileName}.txt");
                            System.IO.File.WriteAllText(path, txtBody.Item1, System.Text.Encoding.GetEncoding(950));
                            string pw = "FBOTEST97041";
                            string encFileName = $@"{txtFileName}.txt.enc";
                            ProcessStartInfo processInfo;
                            processInfo = new ProcessStartInfo();
                            //一定要用 bat or cmd 呼叫 加密exe
                            processInfo.FileName = Path.Combine(datFile, "FBO.bat");
                            processInfo.Arguments = $@"{pw} {path}";
                            processInfo.CreateNoWindow = true;
                            processInfo.UseShellExecute = false;
                            // *** Redirect the output ***
                            processInfo.RedirectStandardInput = true;
                            processInfo.RedirectStandardError = true;
                            processInfo.RedirectStandardOutput = true;
                            Process process;
                            process = Process.Start(processInfo);
                            process.WaitForExit();
                        }
                        catch (Exception e)
                        {
                            logger.Error(e.ToString());
                        }
                    //}                 
                }
                else
                    result.DESCRIPTION = "無執行資料!";
            }
            return Json(result);
        }

        /// <summary>
        /// 下載 FBOQ11.zip
        /// </summary>
        /// <param name="filler_20"></param>
        /// <param name="downloadType">FBO or 臨櫃</param>
        /// <returns></returns>
        [HttpGet]
        [DeleteFile]
        public ActionResult DownloadFBOzip(string filler_20,string downloadType)
        {
            try
            {
                var _filler_20 = filler_20.Split('*')[0];
                string datFile = Server.MapPath("~/Dat"); //dat資料夾
                var txtFileName = $@"{downloadType}Q11_{_filler_20}";
                string path = Path.Combine(datFile, $@"{txtFileName}.txt");
                string encFileName = $@"{txtFileName}.txt.enc";
                var _encFileName = Path.Combine(datFile, encFileName);
                string zipFileName = $@"{downloadType}Q11_{_filler_20}.zip";
                var _zipFileName = Path.Combine(datFile, zipFileName);
                if (System.IO.File.Exists(_zipFileName))
                {
                    System.IO.File.Delete(_zipFileName);
                }
                CreateZipFile(_zipFileName, new List<string>() { _encFileName, path });
                System.IO.File.Delete(path);
                System.IO.File.Delete(_encFileName);
                return File(_zipFileName, System.Web.MimeMapping.GetMimeMapping(zipFileName), zipFileName);            
            }
            catch (Exception e)
            {
                logger.Error(e.ToString());
            }
            return new HttpStatusCodeResult(System.Net.HttpStatusCode.Forbidden);
        }

        /// <summary>
        /// Create a ZIP file of the files provided.
        /// </summary>
        /// <param name="fileName">The full path and name to store the ZIP file at.</param>
        /// <param name="files">The list of files to be added.</param>
        public static void CreateZipFile(string fileName, IEnumerable<string> files)
        {
            // Create and open a new ZIP file
            var zip = ZipFile.Open(fileName, ZipArchiveMode.Create);
            foreach (var file in files)
            {
                // Add the entry for each file
                zip.CreateEntryFromFile(file, Path.GetFileName(file), CompressionLevel.Optimal);
            }
            // Dispose of the object when we are done
            zip.Dispose();
        }

        /// <summary>
        /// 變更前端勾選狀態
        /// </summary>
        /// <param name="fastNo">快速付款編號</param>
        /// <param name="flag">勾選狀態</param>
        /// <returns></returns>
        public JsonResult ORTB017Check(string fastNo, bool flag)
        {
            MSGReturnModel<bool> result = new MSGReturnModel<bool>();
            result.DESCRIPTION = "閒置太久請重新登入!";
            if (Cache.IsSet("qryORTB017"))
            {
                var data = (List<FBOModel>)Cache.Get("qryORTB017");
                var _data = data.FirstOrDefault(x => x.FAST_NO == fastNo);
                if (_data != null)
                {
                    _data.checkFlag = flag;
                    Cache.Invalidate("qryORTB017");
                    Cache.Set("qryORTB017", data);
                    var _checked = data.Where(x => x.checkFlag).ToList();
                    result.Datas = _checked.Any();
                    result.RETURN_FLAG = true;
                    result.DESCRIPTION = $@"{_checked.Count().ToString().formateThousand()}/{_checked.Sum(x=> x.REMIT_AMT.stringToDecimal()).ToString().formateThousand()}";
                }
                else
                    result.DESCRIPTION = "其它錯誤，請洽系統管理員!!";
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
            if (Cache.IsSet("qryORTB017"))
            {
                var data = (List<FBOModel>)Cache.Get("qryORTB017");
                return Json(jdata.modelToJqgridResult(data, true, new List<string>() { "BANK_CODE_SUB_BANK", "BANK_ACT" , "checkFlag" }));
            }
            return null;
        }

        private void setReportParm(string filler_20)
        {
            Cache.Invalidate("ORTB017ReportFiller");
            Cache.Set("ORTB017ReportFiller", filler_20);
        }

        [HttpPost]
        public JsonResult GetReportParm()
        {
            return Json((string)Cache.Get("ORTB017ReportFiller"));
        }
    }
}