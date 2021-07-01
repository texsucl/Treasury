using ClosedXML.Excel;
using FAP.Web.ActionFilter;
using FAP.Web.AS400Models;
using FAP.Web.AS400PGM;
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
/// 功能說明：OAP0050 EXCEL上傳清理記錄檔
/// 初版作者：20200922 Daiyu
/// 修改歷程：20200922 Daiyu
/// 需求單號：202008120153-00
/// 修改內容：初版
/// ------------------------------------------
/// 需求單號：
/// 修改歷程：20210125 daiyu
/// 修改內容：修改判斷支票狀態正常否的邏輯
/// ------------------------------------------
/// 需求單號：202103250638
/// 修改歷程：20210330 daiyu
/// 修改內容：修改"案號"、"案號人員別"寫入方式
/// ------------------------------------------
/// 需求單號：202103250638-02
/// 修改歷程：20210517 daiyu
/// 修改內容：調整匯入欄位檢查規則，「原給付性質」欄位原為判斷O_PAID_CD原給付性質TABLE中，改為判斷EXCEL檔欄位上的值，是否存在PMCODE的grp_id=PAID_CDTXT
/// </summary>
///

namespace FAP.Web.Controllers
{
    [Authorize]
    [CheckSessionFilterAttribute]
    public class OAP0050Controller : BaseController
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
            string[] roleInfo = authUtil.chkUserFuncAuth(Session["UserID"].ToString(), "~/OAP0050/");
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


        public JsonResult procImport()
        {

            int iSuccess = 0;
            int errCnt = 0;
            int fileCnt = 0;
            decimal errAmt = new decimal(0);
            decimal fileCheckAmt = new decimal(0);
            decimal fileCheckCnt = new decimal(0);

            List<OAP0050Model> fileList = new List<OAP0050Model>();
            List<errModel> errList = new List<errModel>();
            //string impType = Request.Form["impType"];
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

                    string projectFile = Server.MapPath("~/FileUploads/OAP0050"); //專案資料夾
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


                        //略過前一列(標題列)，一直處理至最後一列
                        for (int i = (sheet.FirstRowNum + 1); i <= sheet.LastRowNum; i++)
                        {
                            IRow row = sheet.GetRow(i);
                            if (row == null) continue;

                            try
                            {
                                fileCnt++;

                                OAP0050Model d = new OAP0050Model();
                                d.linePos = i.ToString();
                                d.system = StringUtil.toString(row.GetCell(0)?.ToString());
                                d.check_no = StringUtil.toString(row.GetCell(1)?.ToString());
                                d.check_acct_short = StringUtil.toString(row.GetCell(2)?.ToString());

                                if ((!"A".Equals(StringUtil.toString(d.system)) & !"F".Equals(StringUtil.toString(d.system)))
                                    || "".Equals(StringUtil.toString(d.check_no)) || "".Equals(StringUtil.toString(d.check_acct_short)))
                                {
                                    errModel err = new errModel();
                                    err.linePos = i.ToString();

                                    err.check_no = StringUtil.toString(row.GetCell(1)?.ToString());
                                    err.policy_no = StringUtil.toString(row.GetCell(3)?.ToString());
                                    err.policy_seq = StringUtil.toString(row.GetCell(4)?.ToString());
                                    err.id_dup = StringUtil.toString(row.GetCell(5)?.ToString());

                                    

                                    if (!"A".Equals(StringUtil.toString(d.system)) & !"F".Equals(StringUtil.toString(d.system)))
                                    {
                                        err.msg += "「系統別」需為 A或 F<br/>";
                                    }

                                    if ("".Equals(StringUtil.toString(d.check_no)))
                                        err.msg += "「支票號碼」不可為空值<br/>";

                                    if ("".Equals(StringUtil.toString(d.check_acct_short)))
                                        err.msg += "「支票號碼稱」不可為空值<br/>";

                                    errList.Add(err);
                                    continue;
                                }

                                procImpRow(d, fileList, row);    //整理excel匯入的資料
                            }
                            catch (Exception e)
                            {
                                errModel err = new errModel();
                                err.linePos = i.ToString();
                                err.msg = e.ToString();
                                err.check_no = StringUtil.toString(row.GetCell(1)?.ToString());
                                err.policy_no = StringUtil.toString(row.GetCell(3)?.ToString());
                                err.policy_seq = StringUtil.toString(row.GetCell(4)?.ToString());
                                err.id_dup = StringUtil.toString(row.GetCell(5)?.ToString());

                                errList.Add(err);
                            }
                        }

                        if (fileList.Count == 0)
                        {
                            FileRelated.deleteFile(path);
                            return Json(new { success = false, err = "檔案無明細內容!!", errList = errList });
                        }


                        //資料檢核
                        chkData(fileList, errList);

                        string check_acct_short = "";
                        string check_no = "";

                        DateTime dt = DateTime.Now;
                        string[] curDateTime = BO.DateUtil.getCurChtDateTime(3).Split(' ');

                        //取得覆核單號
                        SysSeqDao sysSeqDao = new SysSeqDao();
                        string qPreCode = "0050" + curDateTime[0].Substring(0, 5);
                        var cId = sysSeqDao.qrySeqNo("AP", "0050", qPreCode).ToString();
                        string _aply_no = qPreCode + cId.ToString().PadLeft(3, '0');

                        foreach (OAP0050Model d in fileList.OrderBy(x => x.check_acct_short).ThenBy(x => x.check_no)) {
                            d.update_datetime = BO.DateUtil.DatetimeToString(dt, "");
                            d.update_id = Session["UserID"].ToString();
                            d.aply_no = _aply_no;

                            if (!check_acct_short.Equals(d.check_acct_short) || !check_no.Equals(d.check_no)) {
                                
                                check_acct_short = d.check_acct_short;
                                check_no = d.check_no;

                                if (!"E".Equals(d.chkResult)) {
                                    fileCheckCnt++;
                                    try
                                    {
                                        fileCheckAmt += Convert.ToInt32(d.main_amt);
                                    }
                                    catch (Exception e) { }
                                }
                            }


                            if ("E".Equals(d.chkResult))
                            {
                                errModel err = new errModel();
                                err.linePos = d.linePos;
                                err.msg = d.msg;
                                err.check_no = d.check_no;
                                err.policy_no = d.policy_no;
                                err.policy_seq = d.policy_seq;
                                err.id_dup = d.id_dup;

                                errList.Add(err);

                                errCnt++;
                            }
                            
                        }

                        iSuccess = fileCnt - errCnt;
                    }
                }


                errList = errList.OrderBy(x => x.linePos.Length).ThenBy(x => x.linePos).ToList();

                //若excel資料檢核無誤，且執行的功能為"執行匯入"
                if ("I".Equals(execAction) && errList.Count == 0 && iSuccess > 0)
                {
                    string msg = procDB(fileList.Where(x => !"E".Equals(StringUtil.toString(x.chkResult))).ToList());



                    if (!"".Equals(msg))
                        return Json(new { success = false, errList, err = "其它錯誤，請洽系統管理員!!" });
                }


                FileRelated.deleteFile(path);

                if (errList.Count > 0)
                    return Json(new
                    {
                        success = false,
                        fileCnt = fileCnt,
                        iSuccess = iSuccess,
                        errCnt = errCnt,
                        fileCheckAmt = fileCheckAmt,
                        fileCheckCnt = fileCheckCnt,
                        err = "以下資料錯誤",
                        errList = errList
                    });
                else
                    return Json(new
                    {
                        success = true,
                        fileCnt = fileCnt,
                        iSuccess = iSuccess,
                        errCnt = errCnt,
                        fileCheckAmt = fileCheckAmt,
                        fileCheckCnt = fileCheckCnt
                    });
            }
            catch (Exception e)
            {
                return Json(new { success = false, errList, err = "其它錯誤，請洽系統管理員!!" });
            }
        }


        /// <summary>
        /// 整理excel匯入的資料
        /// </summary>
        /// <param name="d"></param>
        /// <param name="dataList"></param>
        /// <param name="row"></param>
        private void procImpRow(OAP0050Model d, List<OAP0050Model> dataList, IRow row)
        {
            d.policy_no = StringUtil.toString(row.GetCell(3)?.ToString());
            d.policy_seq = StringUtil.toString(row.GetCell(4)?.ToString());
            d.id_dup = StringUtil.toString(row.GetCell(5)?.ToString());
            d.member_id = StringUtil.toString(row.GetCell(6)?.ToString());
            d.change_id = StringUtil.toString(row.GetCell(7)?.ToString());
            d.paid_id = StringUtil.toString(row.GetCell(8)?.ToString());
            d.paid_name = StringUtil.toString(row.GetCell(9)?.ToString());
            d.main_amt = StringUtil.toString(row.GetCell(10)?.ToString());
            d.check_amt = StringUtil.toString(row.GetCell(11)?.ToString());


            string check_date = StringUtil.toString(row.GetCell(12)?.ToString()) == "0" ? "" : StringUtil.toString(row.GetCell(12)?.ToString());

            if (check_date.Length > 0)
            {
                if (check_date.Contains("/"))
                {
                    string[] check_dateArr = check_date.ToString().Split('/');
                    if (check_dateArr.Count() == 3)
                        check_date = (Convert.ToInt16(check_dateArr[0]) + 1911).ToString() + "/" + check_dateArr[1] + "/" + check_dateArr[2];
                }
                else
                {
                    check_date = check_date.PadLeft(7, '0');
                    check_date = check_date == "" ? "" : (Convert.ToInt16(check_date.Substring(0, 3)) + 1911).ToString() + "/" + check_date.Substring(3, 2) + "/" + check_date.Substring(5, 2);
                }
            }
            d.check_date = check_date;

            d.o_paid_cd = StringUtil.toString(row.GetCell(13)?.ToString());

            //帳務日期
            string re_paid_date = StringUtil.toString(row.GetCell(14)?.ToString()) == "0" ? "" : StringUtil.toString(row.GetCell(14)?.ToString());
            if (re_paid_date.Length > 0)
            {
                if (re_paid_date.Contains("/"))
                {
                    string[] re_paid_dateArr = re_paid_date.ToString().Split('/');
                    if (re_paid_dateArr.Count() == 3)
                        re_paid_date = (Convert.ToInt16(re_paid_dateArr[0]) + 1911).ToString() + "/" + re_paid_dateArr[1] + "/" + re_paid_dateArr[2];
                }
                else
                {
                    re_paid_date = re_paid_date.PadLeft(7, '0');
                    re_paid_date = re_paid_date == "" ? "" : (Convert.ToInt16(re_paid_date.Substring(0, 3)) + 1911).ToString() + "/" + re_paid_date.Substring(3, 2) + "/" + re_paid_date.Substring(5, 2);
                }
            }
            d.re_paid_date = re_paid_date;

            d.re_paid_type = StringUtil.toString(row.GetCell(15)?.ToString());
            d.fsc_range = StringUtil.toString(row.GetCell(16)?.ToString());
            d.imp_desc = StringUtil.toString(row.GetCell(17)?.ToString());

            string re_paid_date_n = StringUtil.toString(row.GetCell(18)?.ToString()) == "0" ? "" : StringUtil.toString(row.GetCell(18)?.ToString());
            if (re_paid_date_n.Length > 0)
            {
                if (re_paid_date_n.Contains("/"))
                {
                    string[] re_paid_date_nArr = re_paid_date_n.ToString().Split('/');
                    if (re_paid_date_nArr.Count() == 3)
                        re_paid_date_n = (Convert.ToInt16(re_paid_date_nArr[0]) + 1911).ToString() + "/" + re_paid_date_nArr[1] + "/" + re_paid_date_nArr[2];
                }
                else
                {
                    re_paid_date_n = re_paid_date_n.PadLeft(7, '0');
                    re_paid_date_n = re_paid_date_n == "" ? "" : (Convert.ToInt16(re_paid_date_n.Substring(0, 3)) + 1911).ToString() + "/" + re_paid_date_n.Substring(3, 2) + "/" + re_paid_date_n.Substring(5, 2);
                }
            }
            d.re_paid_date_n = re_paid_date_n;

            dataList.Add(d);


        }



        private string convertPaidTp(string sqlPaidtp) {
            string re_paid_type = "";

            switch (sqlPaidtp) {
                case "A":
                    re_paid_type = "B";
                    break;
                case "B":
                    re_paid_type = "A";
                    break;
                case "C":
                    re_paid_type = "D";
                    break;
                default:
                    re_paid_type = sqlPaidtp;
                    break;

            }


            return re_paid_type;
        }


        /// <summary>
        /// 檢核成功的資料，寫入資料庫
        /// </summary>
        /// <param name="fileList"></param>
        /// <returns></returns>
        private string procDB(List<OAP0050Model> fileList)
        {
            
            string msg = "";

            string strConn = DbUtil.GetDBFglConnStr();
            using (SqlConnection conn = new SqlConnection(strConn))
            {
                conn.Open();

                SqlTransaction transaction = conn.BeginTransaction("Transaction");

                try
                {
                    FAPVeTraceImpDao fAPVeTraceImpDao = new FAPVeTraceImpDao();
                    VeCleanUtil veCleanUtil = new VeCleanUtil();

                    using (EacConnection conn400 = new EacConnection(CommonUtil.GetEasycomConn()))
                    {
                        conn400.Open();
                        try
                        {
                            foreach (OAP0050Model check in fileList.GroupBy(o => new { o.check_no, o.check_acct_short, o.system })
                                                   .Select(group => new OAP0050Model
                                                   {
                                                       check_no = group.Key.check_no,
                                                       check_acct_short = group.Key.check_acct_short,
                                                       system = group.Key.system
                                                   }).ToList<OAP0050Model>())
                            {
                                OAP0050Model model = fileList.Where(x => x.system == check.system & x.check_acct_short == check.check_acct_short & x.check_no == check.check_no)
                                                                                    .FirstOrDefault();

                                //add by daiyu 20201216  
                                //有”應付票據檔”，但對應不到保單資料時
                                //1.系統檢查，EXCEL必要輸入欄位：保單號碼
                                //2.給付對象姓名、支票金額、支票到期日：自應付票據檔帶對應欄位
                                //3.回存金額：以應付票據檔的”支票金額”寫入
                                List<VeCleanModel> as400List = veCleanUtil.qryCheckPolicy(check.system, check.check_no, check.check_acct_short, conn400);
                                if (as400List.Count == 0)
                                {
                                    foreach (OAP0050Model d in fileList.Where(x => x.system == check.system & x.check_acct_short == check.check_acct_short & x.check_no == check.check_no).ToList())
                                    {
                                        //writePiaLog(1, d.paid_id, "A", d.check_no);

                                        if (!"".Equals(StringUtil.toString(d.check_date_pyck))) {

                                            d.paid_name = StringUtil.toString(d.paid_name_pyck);
                                            try
                                            {
                                                d.check_amt = d.check_amt_pyck;
                                                d.main_amt = d.check_amt_pyck;
                                            }
                                            catch (Exception e)
                                            {

                                            }

                                            try
                                            {
                                                d.check_date = BO.DateUtil.As400ChtDateToADDate(StringUtil.toString(d.check_date_pyck));
                                            }
                                            catch (Exception e)
                                            {

                                            }
                                        }

                                        fAPVeTraceImpDao.insert(d, conn, transaction);
                                    }
                                }
                                else {

                                    SAPGETIDUtil sAPGETIDUtil = new SAPGETIDUtil();
                                    string paid_id = sAPGETIDUtil.callSAPGETID(conn400
                                        , check.system, StringUtil.toString(as400List[0].aply_no), StringUtil.toString(as400List[0].aply_seq));

                                    if ("".Equals(paid_id)) {
                                        OAP0050Model excelD = fileList.Where(x => x.system == check.system & x.check_acct_short == check.check_acct_short & x.check_no == check.check_no)
                                                                                        .FirstOrDefault();
                                        paid_id = StringUtil.toString(excelD.paid_id);

                                    }

                                    foreach (VeCleanModel poli in as400List)
                                    {
                                        OAP0050Model poliModel = new OAP0050Model();
                                        ObjectUtil.CopyPropertiesTo(model, poliModel);
                                        //ObjectUtil.CopyPropertiesTo(poli, poliModel);

                                        poliModel.paid_id = paid_id;
                                        poliModel.paid_name = StringUtil.toString(as400List[0].paid_name);

                                        try
                                        {
                                            poliModel.check_amt = as400List[0].check_amt;
                                        }
                                        catch (Exception e)
                                        {

                                        }

                                        try
                                        {
                                            poliModel.check_date = BO.DateUtil.As400ChtDateToADDate(StringUtil.toString(as400List[0].check_date));
                                        }
                                        catch (Exception e)
                                        {

                                        }

                                        poliModel.policy_no = poli.policy_no;
                                        poliModel.policy_seq = poli.policy_seq;
                                        poliModel.id_dup = poli.id_dup;

                                        poliModel.main_amt = poli.main_amt;
                                        //poliModel.o_paid_cd = poli.o_paid_cd;     //delete by daiyu 20210517

                                        if ("F".Equals(poli.system)) { //modify by daiyu 20210331
                                            poliModel.member_id = poli.member_id;
                                            poliModel.change_id = poli.change_id;
                                        }
                                        

                                        fAPVeTraceImpDao.insert(poliModel, conn, transaction);
                                    }
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            logger.Error(e.ToString());
                            throw e;
                        }
                    }


                    transaction.Commit();
                }
                catch (Exception e) {
                    msg = "其它錯誤，請洽系統管理員!!";
                    logger.Error(e.ToString());
                    transaction.Rollback();
                }
            }


            return msg;
        }


        private void writePiaLog(int affectRows, string piaOwner, string executionType, string content)
        {
            PIA_LOG_MAIN piaLogMain = new PIA_LOG_MAIN();
            piaLogMain.TRACKING_TYPE = "A";
            piaLogMain.ACCESS_ACCOUNT = Session["UserID"].ToString();
            piaLogMain.ACCOUNT_NAME = Session["UserName"].ToString();
            piaLogMain.PROGFUN_NAME = "OAP0050Controller";
            piaLogMain.EXECUTION_CONTENT = content;
            piaLogMain.AFFECT_ROWS = affectRows;
            piaLogMain.PIA_TYPE = "0100100000";
            piaLogMain.EXECUTION_TYPE = executionType;
            piaLogMain.ACCESSOBJ_NAME = "FAP_VE_TRACE_IMP";
            piaLogMain.PIA_OWNER1 = MaskUtil.maskId(piaOwner);
            PiaLogMainDao piaLogMainDao = new PiaLogMainDao();
            piaLogMainDao.Insert(piaLogMain);

        }



        /// <summary>
        /// 檢核資料
        /// </summary>
        /// <param name="d"></param>
        /// <param name="err"></param>
        /// <returns></returns>
        private List<OAP0050Model> chkData(List<OAP0050Model> fileList, List<errModel> errList)
        {
            //1.檢核當次匯入資料是否重覆(支票號碼+支票帳號簡稱+保單號碼+序號+重覆別+案號人員別+案號)
            List<OAP0050Model> dupList = fileList.GroupBy(o => new { o.check_no, o.check_acct_short, o.policy_no, o.policy_seq, o.id_dup, o.member_id, o.change_id })
                                                .Select(group => new OAP0050Model
                                                {
                                                    check_no = group.Key.check_no,
                                                    check_acct_short = group.Key.check_acct_short,
                                                    policy_no = group.Key.policy_no,
                                                    policy_seq = group.Key.policy_seq,
                                                    id_dup = group.Key.id_dup,
                                                    member_id = group.Key.member_id,
                                                    change_id = group.Key.change_id,
                                                    cnt = group.Count()
                                                }).Where(x => x.cnt > 1).ToList<OAP0050Model>();

            foreach (OAP0050Model dup in dupList)
            {

                fileList.Where(x => x.check_no == dup.check_no & x.check_acct_short == dup.check_acct_short
                & x.policy_no == dup.policy_no & x.policy_seq == dup.policy_seq
                & x.id_dup == dup.id_dup & x.member_id == dup.member_id & x.change_id == dup.change_id)
                    .Select(x => { x.chkResult = "E"; x.msg += "上傳資料重複<br/>"; return x; }).ToList();

            }


            //2.檢核系統別+支票號碼+支票帳號簡稱是否已存在清理記錄檔
            FAPVeTraceDao fAPVeTraceDao = new FAPVeTraceDao();
            using (dbFGLEntities db = new dbFGLEntities())
            {
                foreach (OAP0050Model d in fileList)
                {
                    FAP_VE_TRACE trace = fAPVeTraceDao.qryByCheckNo(d.check_no, d.check_acct_short, db);
                    if (!"".Equals(StringUtil.toString(trace.check_no)))
                    {
                        d.chkResult = "E";
                        d.msg += "已存在清理紀錄檔；";
                    }
                }
            }


            //3.須檢核支票號碼+支票帳號簡稱是否存在應付票據檔
            using (EacConnection conn400 = new EacConnection(CommonUtil.GetEasycomConn()))
            {
                conn400.Open();
                VeCleanUtil veCleanUtil = new VeCleanUtil();

                foreach (OAP0050Model d in fileList.GroupBy(o => new { o.system, o.check_no, o.check_acct_short, o.re_paid_date, o.re_paid_date_n, o.re_paid_type })
                                                    .Select(group => new OAP0050Model
                                                    {
                                                        system = group.Key.system,
                                                        check_no = group.Key.check_no,
                                                        check_acct_short = group.Key.check_acct_short,
                                                        re_paid_date = group.Key.re_paid_date,
                                                        re_paid_date_n = group.Key.re_paid_date_n,
                                                        re_paid_type = group.Key.re_paid_type
                                                    }).ToList<OAP0050Model>())
                {
                    OAP0050PYCK pyck = new OAP0050PYCK();

                    if ("F".Equals(d.system))   //F系統
                    {
                        FAPPYCKDao fAPPYCKDao = new FAPPYCKDao();
                        pyck = fAPPYCKDao.qryForOAP0050(conn400, d.check_acct_short, d.check_no);

                        if ("".Equals(StringUtil.toString(pyck.checkNo)))
                        {
                            fileList.Where(x => x.system == d.system & x.check_no == d.check_no & x.check_acct_short == d.check_acct_short)
                                        .Select(x => { x.bPyck = false; x.chkResult = "E"; x.msg += "無對應應付票據檔<br/>"; return x; }).ToList();

                            continue;
                        }
                        else
                        {
                            //d.paid_name_pyck = pyck.receiver;
                            //d.check_date_pyck = pyck.checkDate;
                            //d.check_amt_pyck = pyck.checkAmt;

                            fileList.Where(x => x.system == d.system & x.check_no == d.check_no & x.check_acct_short == d.check_acct_short)
                                        .Select(x => { x.paid_name_pyck = pyck.receiver;
                                            x.check_date_pyck = pyck.checkDate; 
                                            x.check_amt_pyck = pyck.checkAmt; return x; }).ToList();

                            List<VeCleanModel> as400List = veCleanUtil.qryCheckPolicy(d.system, d.check_no, d.check_acct_short, conn400);
                            if (as400List.Count == 0)
                            {
                                //fileList.Where(x => x.system == d.system & x.check_no == d.check_no & x.check_acct_short == d.check_acct_short)
                                //        .Select(x => { x.bPyck = false; x.chkResult = "E"; x.msg += "無對應保單資料<br/>"; return x; }).ToList();
                                fileList.Where(x => x.system == d.system & x.check_no == d.check_no & x.check_acct_short == d.check_acct_short)
                                        .Select(x => { x.bPyck = false; return x; }).ToList();
                               // continue;
                            }
                        }

                        
                        //modify by daiyu 20210125 修改支票狀態判斷方式
                        bool statPass = false;

                        /*---正常件---*/
                        //【應付票據檔 FAPPYCK0】：支票狀態 CHECK_STAT = 5轉入雜收
                        //【支票作廢檔 FAPCKER0】：不存在
                        //上傳的EXCEL內容：帳務日期、再給付日期、再給付方式 需為空白
                        if ("5".Equals(StringUtil.toString(pyck.checkStat)) && "".Equals(StringUtil.toString(pyck.checkNo_del))
                              & "".Equals(StringUtil.toString(d.re_paid_date)) & "".Equals(StringUtil.toString(d.re_paid_date_n)) & "".Equals(StringUtil.toString(d.re_paid_type))
                            )
                            statPass = true;

                        /*---DC/DCR件---*/
                        //【應付票據檔 FAPPYCK0】：支票狀態 CHECK_STAT = ３兌現
                        //【支票作廢檔 FAPCKER0】：存在，作廢代碼 DEL_CODE = ９繳保費
                        //上傳的EXCEL內容：帳務日期、再給付日期、再給付方式 需有值
                        if ("3".Equals(StringUtil.toString(pyck.checkStat)) && "9".Equals(StringUtil.toString(pyck.delCode))
                            & !"".Equals(StringUtil.toString(d.re_paid_date)) & !"".Equals(StringUtil.toString(d.re_paid_date_n)) & !"".Equals(StringUtil.toString(d.re_paid_type))
                            )
                            statPass = true;

                        /*---重新給付件---*/
                        //【應付票據檔 FAPPYCK0】：支票狀態 CHECK_STAT = ４作廢
                        //【支票作廢檔 FAPCKER0】：存在，作廢代碼 DEL_CODE = ４重新給付
                        //【給付中介檔 LGLGLPY4】或上傳的EXCEL內容：帳務日期 需有值
                        //上傳的EXCEL內容：再給付日期、再給付方式 需有值
                        if ("4".Equals(StringUtil.toString(pyck.checkStat)) && "4".Equals(StringUtil.toString(pyck.delCode))) {
                            string re_paid_date = StringUtil.toString(d.re_paid_date);
                           // string re_paid_type = StringUtil.toString(d.re_paid_type);

                            if (!"".Equals(StringUtil.toString(pyck.sqlVhrdt)))
                            {
                                try
                                {
                                    re_paid_date = BO.DateUtil.formatDateTimeDbToSc(BO.DateUtil.As400ChtDateToADDate(pyck.sqlVhrdt), "D");
                                    //re_paid_type = convertPaidTp(StringUtil.toString(pyck.sqlPaidtp));

                                    fileList.Where(x => x.system == d.system & x.check_no == d.check_no & x.check_acct_short == d.check_acct_short)
                                        .Select(x =>
                                        {
                                            x.re_paid_date = re_paid_date;
                                            return x;
                                        }).ToList();
                                   // statPass = true;
                                }
                                catch (Exception)
                                {

                                }
                            }

                            if(!"".Equals(re_paid_date) & !"".Equals(d.re_paid_date_n) & !"".Equals(d.re_paid_type)) {
                                statPass = true;
                            }
                        }
                            


                        if(!statPass)
                            fileList.Where(x => x.system == d.system & x.check_no == d.check_no & x.check_acct_short == d.check_acct_short)
                                    .Select(x => { x.chkResult = "E"; x.msg += "支票狀態錯誤<br/>"; return x; }).ToList();

                        ////正常件
                        //if (("0".Equals(pyck.sqlVhrdt) || "".Equals(pyck.sqlVhrdt)) & !"5".Equals(StringUtil.toString(pyck.checkStat)))
                        //{
                        //    fileList.Where(x => x.system == d.system & x.check_no == d.check_no & x.check_acct_short == d.check_acct_short)
                        //            .Select(x => { x.chkResult = "E"; x.msg += "支票狀態錯誤<br/>"; return x; }).ToList();
                        //}

                        ////重新給付件
                        //if ((!"0".Equals(pyck.sqlVhrdt) & !"".Equals(pyck.sqlVhrdt)))
                        //{
                        //    if (("4".Equals(StringUtil.toString(pyck.checkStat)) & "4".Equals(StringUtil.toString(pyck.delCode)))
                        //        ||
                        //        (("3".Equals(StringUtil.toString(pyck.checkStat)) & "9".Equals(StringUtil.toString(pyck.delCode))))
                        //        ||
                        //         (("3".Equals(StringUtil.toString(pyck.checkStat)) & StringUtil.toString(pyck.delCode).IndexOf('*') > 0))
                        //        )
                        //    {
                        //        continue;
                        //    }
                        //    else
                        //    {
                        //        fileList.Where(x => x.system == d.system & x.check_no == d.check_no & x.check_acct_short == d.check_acct_short)
                        //               .Select(x => { x.chkResult = "E"; x.msg += "支票狀態錯誤<br/>"; return x; }).ToList();
                        //    }
                        //}

                        //end modify 20210118
                    }
                    else
                    {   //A系統
                        FFAPYCKDao fFAPYCKDao = new FFAPYCKDao();
                        pyck = fFAPYCKDao.qryForOAP0050(conn400, d.check_acct_short, d.check_no);

                        if ("".Equals(StringUtil.toString(pyck.checkNo)))
                        {
                            fileList.Where(x => x.system == d.system & x.check_no == d.check_no & x.check_acct_short == d.check_acct_short)
                                        .Select(x => { x.bPyck = false; x.chkResult = "E"; x.msg += "無對應應付票據檔<br/>"; return x; }).ToList();

                            continue;
                        }
                        else {
                            //d.paid_name_pyck = pyck.receiver;
                            //d.check_date_pyck = pyck.checkDate;
                            //d.check_amt_pyck = pyck.checkAmt;

                            fileList.Where(x => x.system == d.system & x.check_no == d.check_no & x.check_acct_short == d.check_acct_short)
                                        .Select(x => {
                                            x.paid_name_pyck = pyck.receiver;
                                            x.check_date_pyck = pyck.checkDate;
                                            x.check_amt_pyck = pyck.checkAmt; return x;
                                        }).ToList();

                            List<VeCleanModel> as400List = veCleanUtil.qryCheckPolicy(d.system, d.check_no, d.check_acct_short, conn400);
                            if (as400List.Count == 0)
                            {
                                //fileList.Where(x => x.system == d.system & x.check_no == d.check_no & x.check_acct_short == d.check_acct_short)
                                //        .Select(x => { x.bPyck = false; x.chkResult = "E"; x.msg += "無對應保單資料<br/>"; return x; }).ToList();
                                fileList.Where(x => x.system == d.system & x.check_no == d.check_no & x.check_acct_short == d.check_acct_short)
                                        .Select(x => { x.bPyck = false; return x; }).ToList();
                                //continue;
                            }
                        }


                        //modify by daiyu 20210125 修改支票狀態判斷方式
                        bool statPass = false;

                        /*---正常件---*/
                        //【應付票據檔 FFAPYCK】：支票狀態 CHECK_STAT = 3催收、重開支票狀態 = 空白
                        //上傳的EXCEL內容：帳務日期、再給付日期、再給付方式 需為空白
                        if ("3".Equals(StringUtil.toString(pyck.checkStat)) & "".Equals(StringUtil.toString(pyck.reCkF))
                              & "".Equals(StringUtil.toString(d.re_paid_date)) & "".Equals(StringUtil.toString(d.re_paid_date_n)) & "".Equals(StringUtil.toString(d.re_paid_type))
                            )
                            statPass = true;


                        /*---DC/DCR件---*/
                        //【應付票據檔 FFAPYCK】：支票狀態 CHECK_STAT = ４抵兌、重開支票狀態 = ９繳保費
                        //上傳的EXCEL內容：帳務日期、再給付日期、再給付方式 需有值
                        if ("4".Equals(StringUtil.toString(pyck.checkStat)) & "9".Equals(StringUtil.toString(pyck.reCkF))
                              & !"".Equals(StringUtil.toString(d.re_paid_date)) & !"".Equals(StringUtil.toString(d.re_paid_date_n)) & !"".Equals(StringUtil.toString(d.re_paid_type))
                            )
                            statPass = true;


                        /*---重新給付件---*/
                        //【應付票據檔 FFAPYCK】：支票狀態 CHECK_STAT = ４抵兌、重開支票狀態 = ５重新給付
                        //【給付中介檔 LGLGLPY4】或上傳的EXCEL內容：帳務日期、再給付日期、再給付方式 需有值
                        if ("4".Equals(StringUtil.toString(pyck.checkStat)) & "5".Equals(StringUtil.toString(pyck.reCkF)))
                        {
                            string re_paid_date = StringUtil.toString(d.re_paid_date);
                           // string re_paid_type = StringUtil.toString(d.re_paid_type);

                            if (!"".Equals(StringUtil.toString(pyck.sqlVhrdt)))
                            {
                                try
                                {
                                    re_paid_date = BO.DateUtil.formatDateTimeDbToSc(BO.DateUtil.As400ChtDateToADDate(pyck.sqlVhrdt), "D");
                                    //re_paid_type = convertPaidTp(StringUtil.toString(pyck.sqlPaidtp));

                                    fileList.Where(x => x.system == d.system & x.check_no == d.check_no & x.check_acct_short == d.check_acct_short)
                                        .Select(x =>
                                        {
                                            x.re_paid_date = re_paid_date;
                                            return x;
                                        }).ToList();
                                  //  statPass = true;
                                }
                                catch (Exception)
                                {

                                }
                            }

                            if (!"".Equals(re_paid_date) & !"".Equals(d.re_paid_date_n) & !"".Equals(d.re_paid_type)) {
                                statPass = true;
                            }
                        }


                        if (!statPass)
                            fileList.Where(x => x.system == d.system & x.check_no == d.check_no & x.check_acct_short == d.check_acct_short)
                                    .Select(x => { x.chkResult = "E"; x.msg += "支票狀態錯誤<br/>"; return x; }).ToList();

                        ////正常件
                        //if (("0".Equals(pyck.sqlVhrdt) || "".Equals(pyck.sqlVhrdt)) & !"3".Equals(StringUtil.toString(pyck.checkStat)))
                        //{
                        //    fileList.Where(x => x.system == d.system & x.check_no == d.check_no & x.check_acct_short == d.check_acct_short)
                        //            .Select(x => { x.chkResult = "E"; x.msg += "支票狀態錯誤<br/>"; return x; }).ToList();
                        //}

                        ////重新給付件
                        //if ((!"0".Equals(pyck.sqlVhrdt) & !"".Equals(pyck.sqlVhrdt)))
                        //{
                        //    if (("4".Equals(StringUtil.toString(pyck.checkStat)) & "5".Equals(StringUtil.toString(pyck.reCkF)))
                        //        ||
                        //        (("4".Equals(StringUtil.toString(pyck.checkStat)) & "9".Equals(StringUtil.toString(pyck.reCkF))))
                        //        )
                        //    {
                        //        continue;
                        //    }
                        //    else
                        //    {
                        //        fileList.Where(x => x.system == d.system & x.check_no == d.check_no & x.check_acct_short == d.check_acct_short)
                        //               .Select(x => { x.chkResult = "E"; x.msg += "支票狀態錯誤<br/>"; return x; }).ToList();
                        //    }
                        //}

                        //end modify 20210118
                    }
                }
            }

            ValidateUtil validate = new ValidateUtil();

            FAPVeCodeDao fAPVeCodeDao = new FAPVeCodeDao();
            Dictionary<string, string> fscRangeMap = fAPVeCodeDao.qryByTypeDic("FSC_RANGE");

            //add by daiyu 20210517 
            FPMCODEDao fPMCODEDao = new FPMCODEDao();
            Dictionary<string, string> paidCdMap = fPMCODEDao.qryByTypeDic("PAID_CDTXT", "AP", false);

            foreach (OAP0050Model d in fileList)
            {
                //沒有應付票據檔時，以下欄位不可為空值
                if (!d.bPyck)
                {
                    //20201216 modify by daiyu 取消保單號碼檢核...有些真的會沒有保單
                    //保單號碼
                    //if (d.policy_no.Length == 0 || d.policy_no.Length > 10)
                    //{
                    //    d.chkResult = "E";
                    //    d.msg += "「保單號碼」錯誤" + "<br/>";
                    //}


                    //保單序號
                    if (d.policy_seq.Length == 0)
                        d.policy_seq = "0";

                    if (d.policy_seq.Length > 2 || d.policy_seq.Length == 0 || !validate.IsNum(d.policy_seq))
                    {
                        d.chkResult = "E";
                        d.msg += "「保單序號」錯誤" + "<br/>";
                    }

                    //if ("".Equals(d.policy_no))
                    //{
                    //    if (d.policy_seq.Length > 0 && d.policy_seq != "0")
                    //    {
                    //        d.chkResult = "E";
                    //        d.msg += "「保單序號」錯誤" + "<br/>";
                    //    }
                    //    else
                    //        d.policy_seq = "0";
                    //}
                    //else
                    //{
                    //    if(d.policy_seq.Length == 0)
                    //        d.policy_seq = "0";

                    //    if (d.policy_seq.Length > 2 || d.policy_seq.Length == 0 || !validate.IsNum(d.policy_seq))
                    //    {
                    //        d.chkResult = "E";
                    //        d.msg += "「保單序號」錯誤" + "<br/>";
                    //    }
                    //}

                    //重覆碼
                    if (!"".Equals(d.id_dup) && d.id_dup.Length != 1)
                    {
                        d.chkResult = "E";
                        d.msg += "「重覆碼」錯誤" + "<br/>";
                    }


                    //delete by daiyu 20201216 
                    //如有應付票據檔，給付對象姓名、支票金額、支票到期日 從應付票據檔帶
                    //回存金額以支票金額寫入
                    //    //給付對象姓名
                    //    if ("".Equals(d.paid_name))
                    //    {
                    //        d.chkResult = "E";
                    //        d.msg += "「給付對象姓名」不可空白" + "<br/>";
                    //    }


                    //    //回存金額
                    //    if (d.main_amt.Length == 0 || "0".Equals(d.main_amt) || d.main_amt.Length > 16 || !validate.IsNum(d.main_amt))
                    //    {
                    //        d.chkResult = "E";
                    //        d.msg += "「回存金額」錯誤" + "<br/>";
                    //    }

                    //    //支票金額
                    //    if (d.check_amt.Length == 0 || "0".Equals(d.check_amt) || d.check_amt.Length > 16 || !validate.IsNum(d.check_amt))
                    //    {
                    //        d.chkResult = "E";
                    //        d.msg += "「支票金額」錯誤" + "<br/>";
                    //    }


                    //    //支票到期日
                    //    if (d.check_date.Length == 0)
                    //    {
                    //        d.chkResult = "E";
                    //        d.msg += "「支票到期日」錯誤" + "<br/>";
                    //    }
                    //    else {
                    //        try
                    //        {
                    //            Convert.ToDateTime(d.check_date);
                    //        }
                    //        catch (Exception e)
                    //        {
                    //            d.chkResult = "E";
                    //            d.msg += "「支票到期日」錯誤" + "<br/>";
                    //        }

                    //    }
                    }




                //案號人員別
                if (!"".Equals(d.member_id) && d.member_id.Length > 3)
                {
                    d.chkResult = "E";
                    d.msg += "「案號人員別」錯誤!" + "<br/>";
                }

                //案號
                if (!"".Equals(d.change_id) && d.change_id.Length > 10)
                {
                    d.chkResult = "E";
                    d.msg += "「案號」錯誤" + "<br/>";
                }

                //給付對象 ID 
                if (!"".Equals(d.paid_id) && d.paid_id.Length > 10)
                {
                    d.chkResult = "E";
                    d.msg += "「給付對象 ID」錯誤" + "<br/>";
                }

                //給付帳務日
                if (d.re_paid_date.Length > 0 )
                {
                    try
                    {
                        Convert.ToDateTime(d.re_paid_date);
                    }
                    catch (Exception e) {
                        d.chkResult = "E";
                        d.msg += "「給付帳務日」錯誤" + "<br/>";
                    }
                }


                //保局範圍
                if (!"".Equals(StringUtil.toString(d.fsc_range)) & !fscRangeMap.ContainsKey(d.fsc_range)) {
                    d.chkResult = "E";
                    d.msg += "「保局範圍」錯誤" + "<br/>";
                }


                //再給付日期
                if (d.re_paid_date_n.Length > 0)
                {
                    try
                    {
                        Convert.ToDateTime(d.re_paid_date_n);
                    }
                    catch (Exception e)
                    {
                        d.chkResult = "E";
                        d.msg += "「再給付日期」錯誤" + "<br/>";
                    }
                }


                //原給付性質 add by daiyu 20210517
                if (d.o_paid_cd.Length <= 0)
                {
                    d.chkResult = "E";
                    d.msg += "「原給付性質」必輸" + "<br/>";
                    
                }
                else {
                    try
                    {
                        if (!paidCdMap.ContainsKey(d.o_paid_cd)) {
                            d.chkResult = "E";
                            d.msg += "「原給付性質」錯誤" + "<br/>";
                        }
                    }
                    catch (Exception e)
                    {
                        d.chkResult = "E";
                        d.msg += "「原給付性質」錯誤" + "<br/>";
                    }
                }
            }

            return fileList;
        }



        internal class errModel
        {
            public bool chkResult { get; set; }
            public string linePos { get; set; }
            public string msg { get; set; }

            public string check_no { get; set; }
            public string policy_no { get; set; }
            public string policy_seq { get; set; }
            public string id_dup { get; set; }
        }

    }
}