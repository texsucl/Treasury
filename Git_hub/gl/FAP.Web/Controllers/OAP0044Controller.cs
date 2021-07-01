using ClosedXML.Excel;
using FAP.Web.ActionFilter;
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
using System.Data.SqlClient;
using System.DirectoryServices;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

/// <summary>
/// 功能說明：OAP0044 重新派件作業
/// 初版作者：20200914 Daiyu
/// 修改歷程：20200914 Daiyu
/// 需求單號：202008120153-00
/// 修改內容：初版
/// -------------------------------------------------
/// 修改歷程：20201216 Daiyu
/// 需求單號：202008120153-00
/// 修改內容：(淑美需求變更)
///           若派件狀態為0,也可以從此作業功能叫出,直接指定新派件人員
///           若派件日期有值,則派件狀態改為派件中
///           若派件日期無值則派件狀態仍為尚未派件
/// </summary>
///

namespace FAP.Web.Controllers
{
    [Authorize]
    [CheckSessionFilterAttribute]
    public class OAP0044Controller : BaseController
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

            if (Session["UserID"] != null)
            {
                String[] roleInfo = authUtil.chkUserFuncAuth(Session["UserID"].ToString(), "~/OAP0044/");
                if (roleInfo != null && roleInfo.Length == 2)
                {
                    opScope = roleInfo[0];
                    funcName = roleInfo[1];
                }
            }

            ViewBag.opScope = opScope;
            ViewBag.funcName = funcName;


            SysCodeDao sysCodeDao = new SysCodeDao();

            //給付性質
            FPMCODEDao pPMCODEDao = new FPMCODEDao();
            //ViewBag.oPaidCdjqList = sysCodeDao.jqGridList("AP", "O_PAID_CD", false);
            ViewBag.oPaidCdjqList = pPMCODEDao.jqGridList("PAID_CDTXT", "AP", false);


            //資料狀態
            ViewBag.statusjqList = sysCodeDao.jqGridList("AP", "DATA_STATUS", false);


            FAPVeCodeDao fAPVeCodeDao = new FAPVeCodeDao();
            string dispatchList = "";

            //第一次電訪人員
            List<FAP_VE_CODE> dispatchRows = fAPVeCodeDao.qryByGrp("TEL_DISPATCH");

         
            List<ADModel> dataList = new List<ADModel>();
            foreach (var item in dispatchRows)
            {
                ADModel adModel = new ADModel();
                CommonUtil commonUtil = new CommonUtil();
                adModel = commonUtil.qryEmp(item.code_id);
                adModel.name = adModel.user_id + " " + (StringUtil.toString(adModel.name) == "" ? "" : StringUtil.toString(adModel.name));
                dataList.Add(adModel);
            }

            SelectList _dispatchList = new SelectList
             (
             items: dataList,
             dataValueField: "user_id",
             dataTextField: "name"
             );

            ViewBag.dispatchList = _dispatchList;

            return View();
        }



        
        [HttpPost]
        public JsonResult qryTelCheck(string tel_interview_id_o, string check_no, string paid_id)
        {
            try
            {

                FAPTelCheckDao fAPTelCheckDao = new FAPTelCheckDao();
                List<OAP0044Model> rows = fAPTelCheckDao.qryForOAP0044("", tel_interview_id_o, check_no, paid_id);

                rows.Where(x => x.temp_id.Contains('@')).Select(x => { x.temp_id = x.temp_id.Replace('@', '*'); return x; }).ToList();

                List<OAP0044Model> dataList = new List<OAP0044Model>();

                Dictionary<string, ADModel> empMap = new Dictionary<string, ADModel>();
                string usr_id = "";
                CommonUtil commonUtil = new CommonUtil();

                string _temp_id = "";
                foreach (OAP0044Model d in rows.OrderBy(x => x.temp_id)) {
                    if (!_temp_id.Equals(d.temp_id)) {
                        _temp_id = d.temp_id;


                        //取得原派件人員姓名
                        usr_id = d.tel_interview_id;

                        if (!"".Equals(usr_id))
                        {
                            if (!empMap.ContainsKey(usr_id))
                            {
                                ADModel adModel = new ADModel();
                                adModel = commonUtil.qryEmp(usr_id);
                                empMap.Add(usr_id, adModel);
                            }
                            d.tel_interview_name = empMap[usr_id].name == "" ? d.tel_interview_id : empMap[usr_id].name;
                        }
                        else
                            d.tel_interview_name = d.tel_interview_id;

                        dataList.Add(d);
                    }

                }


                string content = "tel_interview_id_o:" + tel_interview_id_o + "; check_no:" + check_no;
                //寫稽核軌跡
                if (dataList.Count > 0)
                    writePiaLog(dataList.Count, StringUtil.toString(dataList[0].paid_id), "Q", content);


                return Json(new { success = true, dataList = dataList });
            }
            catch (Exception e)
            {
                logger.Info(e.ToString());
                return Json(new { success = false, err = "其它錯誤，請洽系統管理員!!" });
            }
        }




        /// <summary>
        /// 畫面執行"申請覆核"
        /// </summary>
        /// <param name="tel_interview_id_n"></param>
        /// <param name="dispatch_date"></param>
        /// <param name="gridData"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult execSave(string tel_interview_id_n, string dispatch_date, List<OAP0044Model> gridData)
        {
            logger.Info("execSave begin");

            try
            {
                DateTime now = DateTime.Now;
                string[] curDateTime = BO.DateUtil.getCurChtDateTime(3).Split(' ');

                if("".Equals(tel_interview_id_n))
                    return Json(new { success = false, err = "新派件人員錯誤!!" }, JsonRequestBehavior.AllowGet);

                if(gridData.Count == 0)
                    return Json(new { success = false, err = "沒有可以異動的資料，將不進行申請覆核作業!!" }, JsonRequestBehavior.AllowGet);



                /*------------------ DB處理   begin------------------*/
                

                //取得流水號
                SysSeqDao sysSeqDao = new SysSeqDao();
                String qPreCode = "0044" + curDateTime[0].Substring(0, 5);
                var cId = sysSeqDao.qrySeqNo("AP", "0044", qPreCode).ToString();
                string aply_no = qPreCode + cId.ToString().PadLeft(3, '0');

                string strConn = DbUtil.GetDBFglConnStr();
                using (SqlConnection conn = new SqlConnection(strConn))
                {
                    conn.Open();

                    SqlTransaction transaction = conn.BeginTransaction("Transaction");
                    try
                    {
                        FAPTelCheckDao fAPTelCheckDao = new FAPTelCheckDao();
                        FAPTelCheckHisDao fAPTelCheckHisDao = new FAPTelCheckHisDao();

                        //查出需重新派件的資料
                        foreach (OAP0044Model temp in gridData)
                        {
                            List<OAP0044Model> rows = fAPTelCheckDao.qryForOAP0044(temp.temp_id.Replace('*', '@'), "", "", "");

                            foreach (OAP0044Model d in rows.ToList()) {
                                d.update_datetime = null;
                                d.dispatch_date = null;

                                //異動【FAP_TEL_CHECK 電訪支票檔】資料狀態
                                FAP_TEL_CHECK _tel_check = new FAP_TEL_CHECK();
                                ObjectUtil.CopyPropertiesTo(d, _tel_check);
                                _tel_check.tel_std_type = "tel_assign_case";
                                _tel_check.update_id = Session["UserID"].ToString();
                                _tel_check.update_datetime = now;
                                _tel_check.data_status = "2";
                                fAPTelCheckDao.updDataStatus(_tel_check, conn, transaction);


                                //新增【FAP_TEL_CHECK_HIS 電訪支票暫存檔】
                                string tel_interview_id_o = d.tel_interview_id;

                                FAP_TEL_CHECK_HIS his = new FAP_TEL_CHECK_HIS();
                                ObjectUtil.CopyPropertiesTo(d, his);
                                his.aply_no = aply_no;
                                his.tel_std_type = "tel_assign_case";
                                his.tel_proc_no = "";
                                his.tel_interview_id = tel_interview_id_n;
                                his.appr_stat = "1";
                                his.update_id = Session["UserID"].ToString();
                                his.update_datetime = now;
                                his.dispatch_date = string.IsNullOrWhiteSpace(dispatch_date) ? (DateTime?)null : DateTime.Parse(dispatch_date);
                                fAPTelCheckHisDao.insertFromFormal(now, his, tel_interview_id_o, "0044", conn, transaction);
                            }
                        }

                        transaction.Commit();
                    }
                    catch (Exception e)
                    {
                        logger.Error(e.ToString());
                        transaction.Rollback();

                        return Json(new { success = false, err = "其它錯誤，請洽系統管理員!!" });
                    }
                }

                return Json(new { success = true, aply_no = aply_no});

            }
            catch (Exception e)
            {
                logger.Error(e.ToString());
                return Json(new { success = false, err = "其它錯誤，請洽系統管理員!!" });

            }
        }



        [HttpPost]
        public async Task<JsonResult> execExport(string tel_interview_id_o, string check_no, string paid_id)
        {
            string guid = "";
            guid = Guid.NewGuid().ToString();
            string fullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory + "Temp\\"
                            , string.Concat("OAP0044" + "_" + guid, ".xlsx"));

            try
            {
                bool bRpt = true;

                FAPTelCheckDao fAPTelCheckDao = new FAPTelCheckDao();


                List<TelDispatchRptModel> dataList = new List<TelDispatchRptModel>();
                dataList = fAPTelCheckDao.qryOAP0044Rpt("tel_assign_case", tel_interview_id_o, check_no, paid_id);

                if (dataList.Count == 0)
                    bRpt = false;


                if (bRpt)
                {
                    VeTelUtil veTelUtil = new VeTelUtil();
                    await veTelUtil.genDispatchRpt("OAP0044Controller", Session["UserID"].ToString(), Session["UserName"].ToString()
                        , "tel_assign_case", fullPath, dataList);

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
                logger.Error(e.ToString());
                return Json(new { success = false, err = "其它錯誤，請洽系統管理員!!" });
            }
        }

        public FileContentResult downloadRpt(string id)
        {
            byte[] fileBytes = System.IO.File.ReadAllBytes(Server.MapPath("~/Temp/") + "OAP0044" + "_" + id + ".xlsx");

            string fullPath = Server.MapPath("~/Temp/") + "OAP0044" + "_" + id + ".xlsx";
            if (System.IO.File.Exists(fullPath))
                System.IO.File.Delete(fullPath);

            return File(fileBytes, "application/vnd.ms-excel", "OAP0044" + ".xlsx");
        }



        /// <summary>
        /// 匯入
        /// </summary>
        /// <returns></returns>
        public JsonResult procImport()
        {


            int fileCnt = 0;
    

            List<impModel> fileList = new List<impModel>();
            List<impModel> errList = new List<impModel>();

            string type = "tel_assign_case";    //Request.Form["type"];

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

                    string projectFile = Server.MapPath("~/FileUploads/OAP0044"); //專案資料夾
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

                        int count = wb.NumberOfSheets;


                        CommonUtil commonUtil = new CommonUtil();
                        Dictionary<string, ADModel> empMap = new Dictionary<string, ADModel>();
                        FAPTelCheckHisDao fAPTelCheckHisDao = new FAPTelCheckHisDao();
                        FAPTelCheckDao fAPTelCheckDao = new FAPTelCheckDao();
                        DateTime now = DateTime.Now;
                        string usr_id = Session["UserID"].ToString();
                        string sheet_name = "";


                        string strConn = DbUtil.GetDBFglConnStr();
                        using (SqlConnection conn = new SqlConnection(strConn))
                        {
                            conn.Open();

                            SqlTransaction transaction = conn.BeginTransaction("Transaction");

                            try
                            {
                                sheet_name = wb.GetSheetAt(0).SheetName;
                                fileCnt = 0;
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
                                    logger.Error(e.ToString());
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



                                //查出需重新派件的資料
                                List<TelDispatchRptModel> rows = fAPTelCheckDao.qryOAP0044Rpt(type, "", "", "");

                                //第一次電訪人員
                                List<FAP_VE_CODE> dispatchRows = new List<FAP_VE_CODE>();

                                FAPVeCodeDao fAPVeCodeDao = new FAPVeCodeDao();
                                if ("tel_assign_case".Equals(type))
                                    dispatchRows = fAPVeCodeDao.qryByGrp("TEL_DISPATCH");
                                else
                                    dispatchRows = fAPVeCodeDao.qryByGrp("SMS_DISPATCH");


                                string[] curDateTime = BO.DateUtil.getCurChtDateTime(3).Split(' ');

                                //取得流水號
                                SysSeqDao sysSeqDao = new SysSeqDao();
                                String qPreCode = "0044" + curDateTime[0].Substring(0, 5);
                                var cId = sysSeqDao.qrySeqNo("AP", "0044", qPreCode).ToString();
                                string aply_no = qPreCode + cId.ToString().PadLeft(3, '0');

                                for (int i = 0; i < fileCnt; i++)
                                {
                                    impModel d = new impModel();
                                    d.linePos = table.Rows[i]["line"]?.ToString();
                                    d.sheet_name = sheet_name;
                                    try
                                    {
                                        d.tel_interview_id = table.Rows[i]["tel_interview_id"]?.ToString();
                                        d.system = table.Rows[i]["system"]?.ToString();
                                        d.check_no = table.Rows[i]["check_no"]?.ToString();
                                        d.check_acct_short = table.Rows[i]["check_acct_short"]?.ToString();
                                        //d.fsc_range = table.Rows[i]["fsc_range"]?.ToString();
                                        d.amt_range = table.Rows[i]["range_l"]?.ToString();

                                        //檢查是否有更新"第一次電訪人員"
                                        TelDispatchRptModel _db_check = rows.Where(x => x.system == d.system & x.check_acct_short == d.check_acct_short & x.check_no == d.check_no).FirstOrDefault();
                                        if (_db_check == null)
                                        {
                                            d.err_msg = "系統內不存在該筆支票的待分派資料";
                                            errList.Add(d);
                                        }
                                        else {
                                            if (StringUtil.toString(d.tel_interview_id).Equals(_db_check.tel_interview_id))
                                                continue;
                                        
                                        }
                                        d.fsc_range = _db_check.fsc_range;

                                        if (!"".Equals(d.tel_interview_id) & dispatchRows.Where(x => x.code_id == d.tel_interview_id).Count() == 0)
                                        {
                                            d.err_msg = "第一次電訪人員錯誤";
                                            errList.Add(d);
                                        }
                                        else
                                        {
                                            if ("2".Equals(_db_check.data_status))
                                            {
                                                d.err_msg = "資料覆核中";
                                                errList.Add(d);
                                            }
                                            else
                                            {
                                                //異動【FAP_TEL_CHECK 電訪支票檔】資料狀態
                                                FAP_TEL_CHECK _tel_check = new FAP_TEL_CHECK();
                                                ObjectUtil.CopyPropertiesTo(d, _tel_check);
                                                _tel_check.tel_std_type = "tel_assign_case";
                                                _tel_check.update_id = Session["UserID"].ToString();
                                                _tel_check.update_datetime = now;
                                                _tel_check.data_status = "2";
                                                fAPTelCheckDao.updDataStatus(_tel_check, conn, transaction);


                                                //新增【FAP_TEL_INTERVIEW_HIS 電訪及追踨記錄暫存檔】
                                                string tel_interview_id_o = d.tel_interview_id;

                                                FAP_TEL_CHECK_HIS his = new FAP_TEL_CHECK_HIS();
                                                ObjectUtil.CopyPropertiesTo(d, his);
                                                his.aply_no = aply_no;
                                                his.tel_std_type = "tel_assign_case";
                                                his.tel_proc_no = "";
                                                //his.tel_interview_id = tel_interview_id_n;
                                                his.appr_stat = "1";
                                                his.update_id = Session["UserID"].ToString();
                                                his.update_datetime = now;
                                                fAPTelCheckHisDao.insertFromFormal(now, his, _db_check.tel_interview_id, "0044", conn, transaction);

                                                fileList.Add(d);

                                            }
                                        }
                                    }
                                    catch (Exception e)
                                    {
                                        d.err_msg = e.ToString();
                                        errList.Add(d);
                                    }
                                }


                                transaction.Commit();


                            }
                            catch (Exception e)
                            {
                                transaction.Rollback();
                                logger.Error(e.ToString());
                                throw e;
                            }
                        }
                    }
                }

                if (fileList.Count() == 0)
                {
                    return Json(new { success = false, err = "本次上傳未更新電訪人員", errList = errList });
                }
                else
                {
                    return Json(new { success = true, msg = "本次上傳更新送至覆核支票筆數:" + fileList.Count(), errList = errList });
                }


            }
            catch (Exception e)
            {
                logger.Error(e.ToString());
                return Json(new { success = false, err = "其它錯誤，請洽系統管理員!!" });
            }
        }

        /// <summary>
        /// 檢核申請的資料是否正確
        /// 1.是否有覆核中的資料
        /// 2.新增的資料是否已存在
        /// </summary>
        /// <param name="code_type"></param>
        /// <param name="exec_action"></param>
        /// <param name="code_id"></param>
        /// <returns></returns>
        private errModel chkAplyData(string tel_proc_no, string clean_f_date, string proc_id)
        {
            ValidateUtil validateUtil = new ValidateUtil();
            errModel errModel = new errModel();
            FAPTelInterviewHisDao fAPTelInterviewHisDao = new FAPTelInterviewHisDao();
            FAP_TEL_INTERVIEW_HIS his = fAPTelInterviewHisDao.qryByTelProcNo(tel_proc_no, "3", "1");

            //if (!"".Equals(StringUtil.toString(his.tel_proc_no))) {
            //    errModel.chkResult = false;
            //    errModel.msg = "資料覆核中，不可異動此筆資料!!";
            //    return errModel;
            //}

            //if (!validateUtil.chkChtDate(clean_f_date))
            //    errModel.msg += " 「實際完成日期」格式錯誤";


            //if (!"".Equals(StringUtil.toString(errModel.msg)))
            //{
            //    errModel.chkResult = false;
            //}
            //else {
            //    errModel.chkResult = true;
            //}

            return errModel;
        }





        private void writePiaLog(int affectRows, string piaOwner, string executionType, string content)
        {
            PIA_LOG_MAIN piaLogMain = new PIA_LOG_MAIN();
            piaLogMain.TRACKING_TYPE = "A";
            piaLogMain.ACCESS_ACCOUNT = Session["UserID"].ToString();
            piaLogMain.ACCOUNT_NAME = Session["UserName"].ToString();
            piaLogMain.PROGFUN_NAME = "OAP0044Controller";
            piaLogMain.EXECUTION_CONTENT = content;
            piaLogMain.AFFECT_ROWS = affectRows;
            piaLogMain.PIA_TYPE = "0100100000";
            piaLogMain.EXECUTION_TYPE = executionType;
            piaLogMain.ACCESSOBJ_NAME = "FAP_TEL_CHECK";
            piaLogMain.PIA_OWNER1 = MaskUtil.maskId(piaOwner);
            PiaLogMainDao piaLogMainDao = new PiaLogMainDao();
            piaLogMainDao.Insert(piaLogMain);

        }

        internal class errModel
        {
            public bool chkResult { get; set; }
            public string msg { get; set; }
        }

        public partial class impModel
        {
            public string linePos { get; set; }

            public string sheet_name { get; set; }

            public string err_msg { get; set; }

            public string system { get; set; }

            public string check_no { get; set; }

            public string check_acct_short { get; set; }

            public string fsc_range { get; set; }

            public string amt_range { get; set; }

            public string tel_interview_id { get; set; }

        }
    }
}