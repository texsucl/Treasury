using ClosedXML.Excel;
using FAP.Web.ActionFilter;
using FAP.Web.AS400Models;
using FAP.Web.BO;
using FAP.Web.Daos;
using FAP.Web.Models;
using FAP.Web.ViewModels;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.EasycomClient;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Web.Mvc;

/// <summary>
/// 功能說明：戶政查詢地址匯入
/// 初版作者：20190617 Daiyu
/// 修改歷程：20190617 Daiyu
///           需求單號：
///           初版
/// ------------------------------------------
/// modify by daiyu 20200722
/// 需求單號:202007150155-00
/// 修改歷程：特殊抽件線上問題，修改OAP0009 戶政查詢地址匯入時先刪除AS400的資料
/// ------------------------------------------
/// modify by daiyu 20200805
/// 需求單號:
/// 修改歷程：增加戶政/電訪地址查詢及匯出功能
/// </summary>
///

namespace FAP.Web.Controllers
{
    [Authorize]
    [CheckSessionFilterAttribute]
    public class OAP0009Controller : BaseController
    {
        static private NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();


        /// <summary>
        /// 畫面初始
        /// </summary>
        /// <returns></returns>
        public ActionResult Index()
        {

            UserAuthUtil authUtil = new UserAuthUtil();


            string opScope = "";
            string funcName = "";
            string[] roleInfo = authUtil.chkUserFuncAuth(Session["UserID"].ToString(), "~/OAP0009/");
            if (roleInfo != null && roleInfo.Length == 2)
            {
                opScope = roleInfo[0];
                funcName = roleInfo[1];
            }

            ViewBag.funcName = funcName;
            ViewBag.opScope = opScope;


            SysCodeDao sysCodeDao = new SysCodeDao();


            return View();
        }


        /// <summary>
        /// 戶政地址匯入
        /// </summary>
        /// <returns></returns>
        public JsonResult procImport()
        {

            int successCnt = 0;
            int errCnt = 0;
            int fileCnt = 0;
            int dupCnt = 0;

            List<OAP0009Model> fileList = new List<OAP0009Model>();
            List<OAP0009Model> errList = new List<OAP0009Model>();

            string execAction = Request.Form["execAction"];

            try
            {
                string path = "";
                //## 如果有任何檔案類型才做
                if (Request.Files.AllKeys.Any())
                {

                    //## 讀取指定的上傳檔案ID
                    var httpPostedFile = Request.Files["UploadedFile"];

                    //## 真實有檔案，進行處理
                    if (httpPostedFile != null && httpPostedFile.ContentLength != 0)
                    {
                        string fileExtension = Path.GetExtension(httpPostedFile.FileName);
                        if (!(".xls".Equals(fileExtension.ToLower()) || ".xlsx".Equals(fileExtension.ToLower())))
                            return Json(new { success = false, err = "匯入資料必須為excel格式，請重新輸入！" });
                    }
                    else
                        return Json(new { success = false, err = "未上傳檔案" });

                    string execSeq = Session["UserID"].ToString()
                        + "_" + BO.DateUtil.getCurChtDateTime().Replace(" ", "");
                    var fileName = execSeq + "_" + Path.GetFileName(httpPostedFile.FileName); //檔案名稱

                    string projectFile = Server.MapPath("~/FileUploads/OAP0009"); //專案資料夾
                    path = Path.Combine(projectFile, fileName);
                    FileRelated.createFile(projectFile); //檢查是否有FileUploads資料夾,如果沒有就新增

                    //呼叫上傳檔案 function
                    Utility.MSGReturnModel result = FileRelated.FileUpLoadinPath(path, httpPostedFile);

                    if (!result.RETURN_FLAG)
                        return Json(new { success = false, err = result.DESCRIPTION });

                    using (FileStream fs = new FileStream(path, FileMode.Open))
                    {
                        IWorkbook wb = null;  //新建IWorkbook對象 
                        if (fileName.IndexOf(".xlsx") > 0) // 2007版本  
                        {
                            wb = new XSSFWorkbook(fs);  //xlsx數據讀入workbook  
                        }
                        else if (fileName.IndexOf(".xls") > 0) // 2003版本  
                        {
                            wb = new HSSFWorkbook(fs);  //xls數據讀入workbook  
                        }

                        ISheet sheet = wb.GetSheetAt(0);
                        DataTable table = new DataTable();

                        //由第一列取標題做為欄位名稱
                        IRow headerRow = sheet.GetRow(0);
                        int cellCount = headerRow.LastCellNum;


                        try
                        {
                            table.Columns.Add("line");
                            for (int i = headerRow.FirstCellNum; i < cellCount; i++)
                                //以欄位文字為名新增欄位，此處全視為字串型別以求簡化
                                table.Columns.Add(
                                    new DataColumn(headerRow.GetCell(i).StringCellValue.TrimEnd()));

                        }
                        catch (Exception e)
                        {
                            return Json(new { success = false, err = "檔案內容需為文字格式!!" });
                        }


                        //略過前兩列(標題列)，一直處理至最後一列
                        for (int i = (sheet.FirstRowNum + 2); i <= sheet.LastRowNum; i++)
                        {
                            IRow row = sheet.GetRow(i);
                            if (row == null) continue;
                            DataRow dataRow = table.NewRow();
                            dataRow[0] = (i + 1).ToString();
                            //依先前取得的欄位數逐一設定欄位內容
                            for (int j = row.FirstCellNum; j < cellCount; j++)
                            {
                                if (row.GetCell(j) != null)
                                    //如要針對不同型別做個別處理，可善用.CellType判斷型別
                                    //再用.StringCellValue, .DateCellValue, .NumericCellValue...取值
                                    //此處只簡單轉成字串
                                    dataRow[j + 1] = row.GetCell(j).ToString();
                            }

                            table.Rows.Add(dataRow);
                            fileCnt++;
                        }

                        if (fileCnt == 0)
                        {
                            FileRelated.deleteFile(path);
                            return Json(new { success = false, err = "檔案無明細內容!!" });
                        }

                        FAPHouseholdAddrDao addrDao = new FAPHouseholdAddrDao();

                        for (int i = 0; i < fileCnt; i++)
                        {
                            OAP0009Model d = new OAP0009Model();
                            d.linePos = table.Rows[i]["line"]?.ToString();

                            try
                            {
                                d.paid_id = table.Rows[i]["paid_id"]?.ToString();
                                d.paid_name = table.Rows[i]["paid_name"]?.ToString();
                                d.zip_code = table.Rows[i]["zip_code"]?.ToString();
                                d.address = table.Rows[i]["address"]?.ToString();
                                d.addr_type = "HOUSE";

                                checkImpData(d, addrDao);

                                switch (d.chkFlag) {
                                    case "W":
                                        fileList.Add(d);
                                        errList.Add(d);
                                        dupCnt++;
                                        break;
                                    case "E":
                                        errList.Add(d);
                                        errCnt++;
                                        break;
                                    default:
                                        fileList.Add(d);
                                        successCnt++;
                                        break;

                                }
                            }
                            catch (Exception e)
                            {
                                d.msg = e.ToString();
                                errList.Add(d);
                                errCnt++;
                            }
                        }
                    }
                }


                //若excel資料檢核無誤，且執行的功能為"執行匯入"
                if ("I".Equals(execAction) && errCnt == 0) {
                    string msg = procImpDb(fileList);

                    if(!"".Equals(msg))
                        return Json(new { success = false, err = "其它錯誤，請洽系統管理員!!" });
                }
                    

                FileRelated.deleteFile(path);


                if (errCnt > 0)
                    return Json(new
                    {
                        success = false,
                        fileCnt = fileCnt,
                        successCnt = successCnt,
                        errCnt = errCnt,
                        dupCnt = dupCnt,
                        err = "以下資料錯誤",
                        errList = errList
                    });
                else {
                    if (dupCnt > 0)
                    {
                        return Json(new
                        {
                            success = true,
                            fileCnt = fileCnt,
                            successCnt = successCnt,
                            errCnt = errCnt,
                            dupCnt = dupCnt,
                            err = "以下資料為異動",
                            errList = errList
                        });
                    }
                    else
                    {
                        return Json(new
                        {
                            success = true,
                            fileCnt = fileCnt,
                            successCnt = successCnt,
                            errCnt = errCnt,
                            dupCnt = dupCnt
                        });
                    }
                }
                   

            }
            catch (Exception e) {
                return Json(new { success = false, err = "其它錯誤，請洽系統管理員!!" });
            }
        }




        /// <summary>
        /// 資料寫入DB
        /// </summary>
        /// <param name="dataList"></param>
        /// <returns></returns>
        public string procImpDb(List<OAP0009Model> dataList) {
            string msg = "";

            string strConn = DbUtil.GetDBFglConnStr();
            using (SqlConnection conn = new SqlConnection(strConn))
            {
                conn.Open();

                SqlTransaction transaction = conn.BeginTransaction("Transaction");
                try
                {
                    DateTime dt = DateTime.Now;
                    string[] chtDt = BO.DateUtil.getCurChtDateTime(4).Split(' ');

                    List<FAPPPAWModel> iPpawList = new List<FAPPPAWModel>();
                    List<FAPPPAWModel> uPpawList = new List<FAPPPAWModel>();

                    FAPHouseholdAddrDao addrDao = new FAPHouseholdAddrDao();

                    foreach (OAP0009Model d in dataList) {

                        FAPPPAWModel ppaw = new FAPPPAWModel();
                        ppaw.report_tp = "X0002";
                        ppaw.dept_group = "2";
                        
                        ppaw.paid_id = d.paid_id;
                        ppaw.check_no = d.paid_id;
                        ppaw.r_zip_code = d.zip_code;
                        ppaw.r_addr = d.address;
                        ppaw.entry_id = Session["UserID"].ToString();
                        ppaw.entry_date = chtDt[0];
                        ppaw.entry_time = chtDt[1];

                        if ("".Equals(d.chkFlag))
                        {
                            addrDao.insert(dt, Session["UserID"].ToString(), d, conn, transaction);
                            writePiaLog(1, d.paid_id, "A");
                            
                            iPpawList.Add(ppaw);
                        }
                        else {
                            addrDao.update(dt, Session["UserID"].ToString(), d, conn, transaction);
                            writePiaLog(1, d.paid_id, "E");

                            uPpawList.Add(ppaw);
                        }
                    }

                    # region 回寫AS400  FAPPPAW0  逾期未兌領信函歸戶工作檔 

                    using (EacConnection conn400 = new EacConnection(CommonUtil.GetEasycomConn()))
                    {
                        conn400.Open();

                        EacTransaction transaction400 = conn400.BeginTransaction();

                        try
                        {
                            FAPPPAWDao fAPPPAWDao = new FAPPPAWDao();

                            
                            fAPPPAWDao.delete("X0002", "2", conn400, transaction400);   //ADD BY DAIYU 20200722


                            fAPPPAWDao.insert(iPpawList, conn400, transaction400);
                            fAPPPAWDao.insert(uPpawList, conn400, transaction400);  //modify by daiyu 20200722
                            //fAPPPAWDao.updateForOAP0009(uPpawList, conn400, transaction400);

                            transaction400.Commit();
                            transaction.Commit();
                        }
                        catch (Exception e) {
                            msg = "其它錯誤，請洽系統管理員!!";
                            logger.Error(e.ToString());
                            transaction400.Rollback();
                            transaction.Rollback();
                        }
                    }
                    #endregion


                }
                catch (Exception e)
                {
                    transaction.Rollback();
                    msg = "其它錯誤，請洽系統管理員!!";
                    logger.Error("其它錯誤：" + e.ToString());
                }
            }
            return msg;
        }



        /// <summary>
        /// 檢核上傳的每一筆資料內容
        /// </summary>
        /// <param name="d"></param>
        /// <returns></returns>
        private OAP0009Model checkImpData(OAP0009Model d, FAPHouseholdAddrDao addrDao) {
            FAP_HOUSEHOLD_ADDR oAddr = new FAP_HOUSEHOLD_ADDR();
            


            //給付對象ID
            if ("".Equals(StringUtil.toString(d.paid_id)) || StringUtil.toString(d.paid_id).Length > 10)
            {
                d.msg += "「給付對象ID」錯誤，請確認!!" + "<br/>";
                d.chkFlag = d.chkFlag == "" ? "E" : d.chkFlag;
            }
            else {
                //是否已存在"FAP_HOUSEHOLD_ADDR 戶政查詢地址檔"
                oAddr = addrDao.qryByKey(d.paid_id, "HOUSE");

                //是否存在"FAP_VE_TRACE 逾期未兌領清理記錄檔"
                FAPVeTraceDao fAPVeTraceDao = new FAPVeTraceDao();
                FAP_VE_TRACE trace = fAPVeTraceDao.chkPaidIdExist(d.paid_id);

                if ("".Equals(StringUtil.toString(trace.paid_id))) {
                    d.msg += "「給付對象ID」不存在【清理記錄檔】，請確認!!" + "<br/>";
                    d.chkFlag = d.chkFlag == "" ? "E" : d.chkFlag;
                }
            }

            //給付對象姓名
            if ("".Equals(StringUtil.toString(d.paid_name)))
            {
                d.msg += "「給付對象姓名」錯誤，請確認!!" + "<br/>";
                d.chkFlag = d.chkFlag == "" ? "E" : d.chkFlag;
            }

            //戶政查詢郵遞區號
            ValidateUtil validateUtil = new ValidateUtil();
            if ("".Equals(StringUtil.toString(d.zip_code)) || StringUtil.toString(d.zip_code).Length > 5 || !validateUtil.IsNum(d.zip_code))
            {
                d.msg += "「戶政查詢郵遞區號」錯誤，請確認!!" + "<br/>";
                d.chkFlag = d.chkFlag == "" ? "E" : d.chkFlag;
            }

            //戶政查詢地址
            if ("".Equals(StringUtil.toString(d.address)) || StringUtil.toString(d.address).Length > 72)
            {
                d.msg += "「戶政查詢地址」錯誤，請確認!!" + "<br/>";
                d.chkFlag = d.chkFlag == "" ? "E" : d.chkFlag;
            }


            //是否已存在"FAP_HOUSEHOLD_ADDR 戶政查詢地址檔"
            if (!"".Equals(StringUtil.toString(oAddr.paid_id))) {
                d.msg += "已存在「FAP_HOUSEHOLD_ADDR 戶政查詢地址檔」，將覆蓋原有資料!!" + "<br/>";
                d.chkFlag = d.chkFlag == "" ? "W" : d.chkFlag;
            }
            

            return d;
        }

        [HttpPost]
        public JsonResult qryAddr(string paid_id)
        {
            logger.Info("qryAddr begin!!");
            try
            {
                SysCodeDao sysCodeDao = new SysCodeDao();
                //地址類別
                Dictionary<string, string> addrTypeMap = sysCodeDao.qryByTypeDic("AP", "ADDR_TYPE");

                FAPHouseholdAddrDao addrDao = new FAPHouseholdAddrDao();
                List<FAP_HOUSEHOLD_ADDR> addrList = addrDao.qryByPaidId(paid_id, "");

                List<OAP0009Model> rows = new List<OAP0009Model>();

                foreach (FAP_HOUSEHOLD_ADDR addr in addrList) {
                    if (!"".Equals(StringUtil.toString(addr.paid_id)))
                    {
                        OAP0009Model d = new OAP0009Model();
                        d.addr_type = addrTypeMap[addr.addr_type];
                        d.paid_id = addr.paid_id;
                        d.paid_name = addr.paid_name;
                        d.zip_code = addr.zip_code;
                        d.address = addr.address;
                        rows.Add(d);
                    }
                }



                var jsonData = new { success = true, dataList = rows };
                return Json(jsonData, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new { success = false, err = "其它錯誤，請洽系統管理員!!" });
            }

        }

        /// <summary>
        /// 匯出
        /// </summary>
        /// <param name="paid_id"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult execExport(string paid_id)
        {
            logger.Info("execExport begin!!");
            try
            {
                FAPHouseholdAddrDao addrDao = new FAPHouseholdAddrDao();
                List<FAP_HOUSEHOLD_ADDR> addrList = addrDao.qryByPaidId(paid_id, "");

                SysCodeDao sysCodeDao = new SysCodeDao();
                //地址類別
                Dictionary<string, string> addrTypeMap = sysCodeDao.qryByTypeDic("AP", "ADDR_TYPE");

                string guid = "";
                if (addrList.Count > 0)
                {
                    writePiaLog(addrList.Count, paid_id, "Q" );

                    guid = Guid.NewGuid().ToString();


                    string fullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory + "Temp\\"
                            , string.Concat("OAP0009" + "_" + guid, ".xlsx"));
                    using (XLWorkbook wb = new XLWorkbook())
                    {
                        var ws = wb.Worksheets.Add("OAP0009");

                        ws.Cell(1, 1).Value = "地址類別";
                        ws.Cell(1, 2).Value = "給付對象 ID";
                        ws.Cell(1, 3).Value = "給付對象姓名";
                        ws.Cell(1, 4).Value = "郵遞區號";
                        ws.Cell(1, 5).Value = "地址";
                        ws.Cell(1, 6).Value = "日期";


                        ws.Range(1, 1, 1, 6).Style.Fill.BackgroundColor = XLColor.AirForceBlue;
                        ws.Range(1, 1, 1, 6).Style.Font.FontColor = XLColor.White;


                        int iRow = 2;

                        foreach (FAP_HOUSEHOLD_ADDR addr in addrList) {
                            ws.Cell(iRow, 1).Value = addrTypeMap[addr.addr_type];
                            ws.Cell(iRow, 2).Value = addr.paid_id;
                            ws.Cell(iRow, 3).Value = addr.paid_name;
                            ws.Cell(iRow, 4).Value = addr.zip_code;
                            ws.Cell(iRow, 5).Value = addr.address;
                            ws.Cell(iRow, 6).Value = BO.DateUtil.DatetimeToString(addr.update_datetime, "yyyy-MM-dd");

                            iRow++;
                        }
                     


                        ws.Columns().AdjustToContents();  // Adjust column width
                        ws.Rows().AdjustToContents();     // Adjust row heights


                        wb.SaveAs(fullPath);
                    }

                    var jsonData = new { success = true, guid = guid };
                    return Json(jsonData, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    var jsonData = new { success = false, err = "無資料" };
                    return Json(jsonData, JsonRequestBehavior.AllowGet);
                }


            }
            catch (Exception e)
            {
                return Json(new { success = false, err = "其它錯誤，請洽系統管理員!!" });
            }
        }


        public FileContentResult downloadRpt(string id)
        {
            byte[] fileBytes = System.IO.File.ReadAllBytes(Server.MapPath("~/Temp/") + "OAP0009" + "_" + id + ".xlsx");


            string fullPath = Server.MapPath("~/Temp/") + "OAP0009" + "_" + id + ".xlsx";
            if (System.IO.File.Exists(fullPath))
                System.IO.File.Delete(fullPath);


            return File(fileBytes, "application/vnd.ms-excel", "OAP0009.xlsx");
        }


        private void writePiaLog(int affectRows, string piaOwner, string executionType)
        {
            PIA_LOG_MAIN piaLogMain = new PIA_LOG_MAIN();
            piaLogMain.TRACKING_TYPE = "A";
            piaLogMain.ACCESS_ACCOUNT = Session["UserID"].ToString();
            piaLogMain.ACCOUNT_NAME = Session["UserName"].ToString();
            piaLogMain.PROGFUN_NAME = "OAP0009Controller";
            piaLogMain.EXECUTION_CONTENT = MaskUtil.maskId(piaOwner);
            piaLogMain.AFFECT_ROWS = affectRows;
            piaLogMain.PIA_TYPE = "0100100000";
            piaLogMain.EXECUTION_TYPE = executionType;
            piaLogMain.ACCESSOBJ_NAME = "FAPHouseholdAddr";
            piaLogMain.PIA_OWNER1 = piaOwner;
            PiaLogMainDao piaLogMainDao = new PiaLogMainDao();
            piaLogMainDao.Insert(piaLogMain);

        }

    }
}