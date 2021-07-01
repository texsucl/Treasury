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
using System.Transactions;
using System.Web.Mvc;

/// <summary>
/// 功能說明：上傳應付未付主檔/明細檔作業
/// 初版作者：20190326 Daiyu
/// 修改歷程：20190326 Daiyu
///           需求單號：
///           初版
/// ==========================================================
/// 修改歷程：20200114 daiyu
/// 需求單號：201910290100
/// 修改內容：執行上傳時，仍維持現有檢核條件，但需於整批資料回寫到AS400時，每筆資料呼叫AML檢核是否屬制裁名單，若屬制裁名單(回覆狀態為NEW、PENDING、TRUE)，進行下列處理
///         1.回寫AS400時，僅寫FMNPPAA0(PPAA的FILLER_14寫03-AML 禁付)、不寫FMNPPAD。
///         2.將屬制裁名單的資料產製報表，並通知相關承辦人員。
///  ==========================================================
/// 修改歷程：20210401 daiyu
/// 需求單號：202103250638
/// 修改內容：上傳時，先寫REPORT_TP = “XAAAA”的資料到AS400的【FAPPPAW0】
/// ==========================================================
/// </summary>
///

namespace FAP.Web.Controllers
{
    [Authorize]
    [CheckSessionFilterAttribute]
    public class OAP0001Controller : BaseController
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
            string[] roleInfo = authUtil.chkUserFuncAuth(Session["UserID"].ToString(), "~/OAP0001/");
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
            decimal fileAmt = new decimal(0);

            List<OAP0001FileModel> fileList = new List<OAP0001FileModel>();
            List<OAP0001Model> errList = new List<OAP0001Model>();
            string impType = Request.Form["impType"];
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

                    string projectFile = Server.MapPath("~/FileUploads/OAP0001"); //專案資料夾
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
                                try {
                                    if (row.GetCell(j) != null)
                                        //如要針對不同型別做個別處理，可善用.CellType判斷型別
                                        //再用.StringCellValue, .DateCellValue, .NumericCellValue...取值
                                        //此處只簡單轉成字串
                                        dataRow[j + 1] = row.GetCell(j).ToString();
                                    table.Rows.Add(dataRow);
                                    fileCnt++;
                                } catch(Exception e)
                                {
                                    
                                }
                            }
                        }

                        if (fileCnt == 0)
                        {
                            FileRelated.deleteFile(path);
                            return Json(new { success = false, err = "檔案無明細內容!!" });
                        }


                        for (int i = 0; i < fileCnt; i++)
                        {
                            OAP0001FileModel d = new OAP0001FileModel();
                            d.linePos = table.Rows[i]["line"]?.ToString();

                            try
                            {
                                d.system = StringUtil.toString(table.Rows[i]["SYSTEM"]?.ToString());
                                d.policyNo = StringUtil.toString(table.Rows[i]["POLICY_NO"]?.ToString());
                                d.policySeq = StringUtil.toString(table.Rows[i]["POLICY_SEQ"]?.ToString());
                                d.idDup = StringUtil.toString(table.Rows[i]["ID_DUP"]?.ToString());
                                d.memberId = StringUtil.toString(table.Rows[i]["MEMBER_ID"]?.ToString());
                                d.changeId = StringUtil.toString(table.Rows[i]["CHANGE_ID"]?.ToString());
                                d.paidId = StringUtil.toString(table.Rows[i]["PAID_ID"]?.ToString());
                                d.paidName = StringUtil.toString(table.Rows[i]["PAID_NAME"]?.ToString());
                                d.applId = StringUtil.toString(table.Rows[i]["APPL_ID"]?.ToString());
                                d.applName = StringUtil.toString(table.Rows[i]["APPL_NAME"]?.ToString());
                                d.insId = StringUtil.toString(table.Rows[i]["INS_ID"]?.ToString());
                                d.insName = StringUtil.toString(table.Rows[i]["INS_NAME"]?.ToString());
                                d.oPaidDt = StringUtil.toString(table.Rows[i]["O_PAID_DT"]?.ToString());
                                d.currency = StringUtil.toString(table.Rows[i]["CURRENCY"]?.ToString());
                                d.mainAmt = StringUtil.toString(table.Rows[i]["MAIN_AMT"]?.ToString());
                                d.checkAmt = StringUtil.toString(table.Rows[i]["CHECK_AMT"]?.ToString());
                                d.checkNo = StringUtil.toString(table.Rows[i]["CHECK_NO"]?.ToString());
                                d.checkShrt = StringUtil.toString(table.Rows[i]["CHECK_SHRT"]?.ToString());
                                d.checkDate = StringUtil.toString(table.Rows[i]["CHECK_DATE"]?.ToString());
                                d.oPaidCd = StringUtil.toString(table.Rows[i]["O_PAID_CD"]?.ToString());
                                d.rZipCode = StringUtil.toString(table.Rows[i]["R_ZIP_CODE"]?.ToString());
                                d.rAddr = StringUtil.toString(table.Rows[i]["R_ADDR"]?.ToString());
                                d.sendId = StringUtil.toString(table.Rows[i]["SEND_ID"]?.ToString());
                                d.sendName = StringUtil.toString(table.Rows[i]["SEND_NAME"]?.ToString());
                                d.sendUnit = StringUtil.toString(table.Rows[i]["SEND_UNIT"]?.ToString());
                                d.report = StringUtil.toString(table.Rows[i]["REPORT"]?.ToString());
                                d.delCode = StringUtil.toString(table.Rows[i]["DEL_CODE"]?.ToString());

                                decimal amt = new decimal(0);
                                try {
                                    amt = Convert.ToDecimal(d.mainAmt);
                                    fileAmt += amt;
                                } catch (Exception e) {
                                }

                                OAP0001Model err = checkImpData(impType, d);
                                if ("".Equals(err.linePos))
                                {
                                    fileList.Add(d);
                                    iSuccess++;
                                }
                                else
                                {
                                    errList.Add(err);
                                    errCnt++;
                                    errAmt += amt;
                                }
                            }
                            catch (Exception e)
                            {
                                OAP0001Model err = new OAP0001Model();
                                err.linePos = d.linePos;
                                err.msg = e.ToString();
                                errCnt++;
                            }
                        }
                    }
                }

                //若excel資料檢核無誤，且執行的功能為"執行匯入"
                if ("I".Equals(execAction) && errList.Count == 0) {
                    string msg = procAs400(fileList);

                    if(!"".Equals(msg))
                        return Json(new { success = false, err = "其它錯誤，請洽系統管理員!!" });
                }
                    

                FileRelated.deleteFile(path);

                if (errList.Count > 0)
                    return Json(new
                    {
                        success = false,
                        fileCnt = fileCnt,
                        iSuccess = iSuccess,
                        errCnt = errCnt,
                        fileAmt = string.Format("{0:N2}", fileAmt), 
                        errAmt = string.Format("{0:N2}", errAmt),
                        err = "以下資料錯誤",
                        errList = errList
                    });
                else
                    return Json(new { success = true,
                        fileCnt = fileCnt, iSuccess = iSuccess, errCnt = errCnt,
                        fileAmt = string.Format("{0:N2}", fileAmt), errAmt = string.Format("{0:N2}", errAmt)
                    });
            }
            catch (Exception e) {
                return Json(new { success = false, err = "其它錯誤，請洽系統管理員!!" });
            }
        }


        /// <summary>
        /// 執行AML檢核
        /// </summary>
        /// <param name="lyodsAmlUtil"></param>
        /// <param name="fileList"></param>
        /// <returns></returns>
        private List<OAP0001FileModel> chkAml(LyodsAmlUtil lyodsAmlUtil, List<OAP0001FileModel> fileList) {

            string lyodsUrl = lyodsAmlUtil.getUrl();


            using (EacConnection conn400 = new EacConnection(CommonUtil.GetEasycomConn()))
            {
                conn400.Open();


                foreach (OAP0001FileModel model in fileList)
                {
                    LyodsAmlFSKWSModel amlModel = new LyodsAmlFSKWSModel();
                    amlModel.paid_id = StringUtil.toString(model.paidId);
                    amlModel.paid_name = StringUtil.toString(model.paidName);
                    amlModel.o_paid_cd = StringUtil.toString(model.oPaidCd);
                    amlModel.query_id = Session["UserID"].ToString();
                    amlModel.query_name = Session["UserName"].ToString();
                    amlModel.unit = "VE30001";
                    amlModel.source_id = "VE";
                    amlModel.name = StringUtil.toString(model.paidName);
                    amlModel.enName = StringUtil.toString(model.paidName);

                    if ("".Equals(amlModel.paid_id))
                        amlModel.cin_no = "VE30001-" + StringUtil.toString(model.checkShrt) + StringUtil.toString(model.checkNo);
                    else
                        amlModel.cin_no = "VE30001-" + StringUtil.toString(model.paidId);

                    amlModel = lyodsAmlUtil.fskws(lyodsUrl, amlModel, conn400);

                    if ("0".Equals(amlModel.rtn_code)) {
                        if ("NEW".Equals(amlModel.status) || "PENDING".Equals(amlModel.status) || "TRUE".Equals(amlModel.status)) 
                            model.filler14 = "03";

                        if (!"".Equals(amlModel.status))
                            model.dataFlag = amlModel.status.Substring(0, 1);

                    }
                }
            }


            return fileList;
        }



        /// <summary>
        /// 檢核成功的資料，回寫AS400
        /// </summary>
        /// <param name="fileList"></param>
        /// <returns></returns>
        private string procAs400(List<OAP0001FileModel> fileList) {

            string msg = "";

            //add by daiyu 20200114
            LyodsAmlUtil lyodsAmlUtil = new LyodsAmlUtil();
            string lyodsUrl = lyodsAmlUtil.getUrl();
            if ("".Equals(lyodsUrl))
            {
                msg = "制裁名單檢核網址未設定，請洽系統管理員!!";
                return msg;
            }

            fileList = chkAml(lyodsAmlUtil, fileList);
            //end add 20200114

            using (EacConnection conn400 = new EacConnection(CommonUtil.GetEasycomConn()))
            {
                conn400.Open();

                EacTransaction transaction = conn400.BeginTransaction();

                try
                {
                    FMNPPAADao fMNPPAADao = new FMNPPAADao();
                    fMNPPAADao.insertForOAP0001(Session["UserID"].ToString(), fileList, conn400, transaction);

                    
                    FMNPPADDao fMNPPADDao = new FMNPPADDao();
                    fMNPPADDao.insertForOAP0001(Session["UserID"].ToString(), fileList, conn400, transaction);

                    //modify by daiyu 20210330
                    DateTime dt = DateTime.Now;
                    string[] chtDt = BO.DateUtil.getCurChtDateTime(4).Split(' ');
                    List<FAPPPAWModel> ppawList = new List<FAPPPAWModel>();

                    foreach (OAP0001FileModel d in fileList) {
                        writePiaLog(d.checkNo, 1, d.paidId, "A");


                        FAPPPAWModel ppaw = new FAPPPAWModel();
                        ppaw.report_tp = "XAAAA";
                        ppaw.dept_group = "A";

                        ppaw.system = d.system;
                        ppaw.check_no = d.checkNo;
                        ppaw.check_shrt = d.checkShrt;
                        
                        ppaw.dept_date1 = chtDt[0];
                        ppaw.entry_id = Session["UserID"].ToString();
                        ppaw.entry_date = chtDt[0];
                        ppaw.entry_time = chtDt[1];

                        ppawList.Add(ppaw);
                    }

                    FAPPPAWDao fAPPPAWDao = new FAPPPAWDao();
                    fAPPPAWDao.insertForOAP0001(ppawList, conn400, transaction);



                    transaction.Commit();
                }
                catch (Exception e) {
                    transaction.Rollback();
                    logger.Error("[execReviewR]其它錯誤：" + e.ToString());
                    msg = "其它錯誤，請洽系統管理員!!";
                }
            }


            //add by daiyu 20200114
            if ("".Equals(msg))
                procAMLRpt(fileList);


            return msg;
        }


        private void writePiaLog(string checkNo, int affectRows, string piaOwner, string executionType)
        {
            PIA_LOG_MAIN piaLogMain = new PIA_LOG_MAIN();
            piaLogMain.TRACKING_TYPE = "A";
            piaLogMain.ACCESS_ACCOUNT = Session["UserID"].ToString();
            piaLogMain.ACCOUNT_NAME = Session["UserName"].ToString();
            piaLogMain.PROGFUN_NAME = "OAP0001Controller";
            piaLogMain.EXECUTION_CONTENT = checkNo;
            piaLogMain.AFFECT_ROWS = affectRows;
            piaLogMain.PIA_TYPE = "0100100000";
            piaLogMain.EXECUTION_TYPE = executionType;
            piaLogMain.ACCESSOBJ_NAME = "FMNPPAA0";
            piaLogMain.PIA_OWNER1 = piaOwner;
            PiaLogMainDao piaLogMainDao = new PiaLogMainDao();
            piaLogMainDao.Insert(piaLogMain);

        }

        /// <summary>
        /// 產生"逾期未兌領支票-疑似禁制名單"
        /// </summary>
        /// <param name="fileList"></param>
        private void procAMLRpt(List<OAP0001FileModel> fileList) {
            List<OAP0001FileModel> mailList = fileList.Where(x => x.filler14 == "03")
                .OrderBy(x => x.checkNo).ThenBy(x => x.policyNo).ThenBy(x => x.policySeq).ThenBy(x => x.idDup).ToList();


            if (mailList.Count() > 0) {
                VeAMLController aml = new VeAMLController();
                aml.fromOAP0001(mailList, Session["UserID"].ToString(), Session["UserName"].ToString());
            }
            

        }



        /// <summary>
        /// 檢核上傳的每一筆資料內容
        /// </summary>
        /// <param name="impType"></param>
        /// <param name="d"></param>
        /// <returns></returns>
        private OAP0001Model checkImpData(string impType, OAP0001FileModel d) {

            OAP0001Model err = new OAP0001Model();

            //大系統別
            if (!("A".Equals(d.system) || "F".Equals(d.system))) {
                err.msg += "「系統別」錯誤，請確認!!" + "<br/>";
                err.linePos = d.linePos;
                err.system = d.system;
                err.checkNo = d.checkNo;
                err.checkShrt = d.checkShrt;
                return err;
            }

            //支票號碼／匯費序號
            if ("".Equals(d.checkNo)) {
                err.msg += "「支票號碼／匯費序號」錯誤，請確認!!" + "<br/>";
                err.linePos = d.linePos;
                err.system = d.system;
                err.checkNo = d.checkNo;
                err.checkShrt = d.checkShrt;
                return err;
            }
                

            //支票帳號簡稱
            if (d.checkShrt.Length == 0) {
                err.msg += "「支票帳號簡稱」錯誤，請確認!!" + "<br/>";
                err.linePos = d.linePos;
                err.system = d.system;
                err.checkNo = d.checkNo;
                err.checkShrt = d.checkShrt;
                return err;
            }
                


            OAP0001PYCK pyck = new OAP0001PYCK();

            if (("A".Equals(d.system) || "F".Equals(d.system))
                && !"".Equals(d.checkNo) && !"".Equals(d.checkShrt))
            {



                using (EacConnection conn400 = new EacConnection(CommonUtil.GetEasycomConn()))
                {
                    conn400.Open();


                    if ("F".Equals(d.system))
                    {
                        FAPPYCKDao fAPPYCKDao = new FAPPYCKDao();
                        pyck = fAPPYCKDao.qryForOAP0001(conn400, d.checkShrt, d.checkNo);
                    }
                    else
                    {
                        FFAPYCKDao fFAPYCKDao = new FFAPYCKDao();
                        pyck = fFAPYCKDao.qryForOAP0001(conn400, d.checkShrt, d.checkNo);
                    }

                    //檢核支票號碼+帳戶簡稱是否存在應付票據主檔(不存在屬異常)
                    err = chkAs400PYCK(conn400, d, err, pyck);

                    //檢核支票號碼+帳戶簡稱是否存在PPAA中(存在屬異常)
                    if ("".Equals(err.msg))
                        err = chkAs400PPAA(conn400, d, err);

                    //上傳格式選”2-人工CR/繳款單”時，要用案號檢核是否存在在人工CR/PRT6210檔案(不存在屬異常)
                    if ("2".Equals(impType) & "".Equals(err.msg))
                        err = chkAs400Cr(conn400, d, err);
                }
            }
           



            //string errMsg = "";
            //檢核資料長度、必輸
            if ("".Equals(err.msg))
                err = chkData(d, err, impType, pyck);
            //if (!"".Equals(err.msg))
            //    errMsg = "以下欄位輸入長度錯誤：" + err.msg.Substring(0, err.msg.Length - 1) + "<br/>";

            //err.msg += errMsg;


            if (!"".Equals(err.msg)) {
                err.linePos = d.linePos;
                err.system = d.system;
                err.checkNo = d.checkNo;
                err.checkShrt = d.checkShrt;
            }
                

            return err;
        }



        /// <summary>
        /// 上傳格式選”2-人工CR/繳款單”時，要用案號檢核是否存在在人工CR/PRT6210檔案(不存在屬異常)
        /// </summary>
        /// <param name="conn400"></param>
        /// <param name="d"></param>
        /// <param name="err"></param>
        /// <returns></returns>
        private OAP0001Model chkAs400Cr(EacConnection conn400, OAP0001FileModel d, OAP0001Model err)
        {
            string rtnMsg = "";
            if ("F".Equals(d.system))
            {
                FRTRMREDao fRTRMREDao = new FRTRMREDao();
                rtnMsg = fRTRMREDao.qryForOAP0001(conn400, d.changeId);
            }
            else {
                FFAFAPHDao fFAFAPHDao = new FFAFAPHDao();
                rtnMsg = fFAFAPHDao.qryForOAP0001(conn400, d.changeId);
            }
            

            if (!"".Equals(rtnMsg))
                err.msg += "案號有誤，請確認!" + "</br>";

            return err;
        }




        /// <summary>
        /// 
        /// </summary>
        /// <param name="conn400"></param>
        /// <param name="d"></param>
        /// <param name="err"></param>
        /// <returns></returns>
        private OAP0001Model chkAs400PPAA(EacConnection conn400, OAP0001FileModel d, OAP0001Model err)
        {
            FMNPPAADao fMNPPAADao = new FMNPPAADao();
            string rtnMsg = fMNPPAADao.qryForOAP0001(conn400, d.checkShrt, d.checkNo, d.system);

            if (!"".Equals(rtnMsg))
                err.msg += "應付未付主檔已存在，請確認!" + "</br>";

            return err;
        }




        /// <summary>
        /// 檢核支票號碼+帳戶簡稱是否存在應付票據主檔
        /// 支票狀態A-3催收/6轉收入；F-5轉雜收/重開票原因-空白或3轉列雜收
        /// </summary>
        /// <param name="d"></param>
        /// <param name="err"></param>
        /// <returns></returns>
        private OAP0001Model chkAs400PYCK(EacConnection conn400, OAP0001FileModel d, OAP0001Model err, OAP0001PYCK pyck)
        {
            string rtnMsg = "";


            if ("".Equals(pyck.checkNo))
                rtnMsg = "1";
            else {
                switch (d.system) {
                    case "A":
                        if (("3".Equals(pyck.checkStat) || "6".Equals(pyck.checkStat)) && "".Equals(pyck.reCkF))
                            rtnMsg = "";
                        else
                            rtnMsg = "2";
                        break;
                    case "F":
                        if ("5".Equals(pyck.checkStat))
                        {
                            FAPCKERDao FAPCKERDao = new FAPCKERDao();
                            pyck.delCode = StringUtil.toString(FAPCKERDao.qryForOAP0001(conn400, d.checkShrt, d.checkNo));

                            if (!("".Equals(pyck.delCode) || "3".Equals(pyck.delCode)))
                                rtnMsg = "2";

                        }
                        else
                            rtnMsg = "2";

                        break;
                }
            }
            

            switch (rtnMsg)
            {
                case "1":
                    err.msg += "支票號碼/帳戶簡稱有誤，請確認!" + "<br/>";
                    break;
                case "2":
                    err.msg += "支票狀態有誤，請確認!" + "<br/>";
                    break;
            }


            return err;
        }

        ///// <summary>
        ///// 檢核資料格式(數字、全型)
        ///// </summary>
        ///// <param name="d"></param>
        ///// <param name="err"></param>
        ///// <returns></returns>
        //private OAP0001Model chkDataFormat(OAP0001FileModel d, OAP0001Model err)
        //{
        //    ValidateUtil validate = new ValidateUtil();

        //    //保單序號
        //    if (d.policySeq.Length > 0 & !validate.IsNum(d.policySeq))
        //        err.msg += "「保單序號」、";

        //    //給付對象姓名
        //    if (d.paidName.Length > 0 & !validate.FullWidthWord(d.paidName))
        //        err.msg += "「給付對象姓名」、";

        //    //要保人姓名
        //    if (d.applName.Length > 0 & !validate.FullWidthWord(d.applName))
        //        err.msg += "「要保人姓名」、";

        //    //被保人姓名
        //    if (d.insName.Length > 0 & !validate.FullWidthWord(d.insName))
        //        err.msg += "「被保人姓名」、";

        //    //原給付日
        //    if (d.oPaidDt.Length > 0 & !validate.IsNum(d.oPaidDt))
        //        err.msg += "「原給付日」、";

        //    //回存金額
        //    if (d.mainAmt.Length > 0 & !validate.IsDecimal(d.mainAmt))
        //        err.msg += "「回存金額」、";

        //    //支票金額
        //    if (d.checkAmt.Length > 0 & !validate.IsDecimal(d.checkAmt))
        //        err.msg += "「支票金額」、";

        //    //支票到期日
        //    if (d.checkDate.Length > 0 & !validate.IsNum(d.checkDate))
        //        err.msg += "「支票到期日」、";

        //    //比對到之地址
        //    if (d.checkDate.Length > 0 & !validate.FullWidthWord(d.rAddr))
        //        err.msg += "「比對到之地址」、";

        //    //送件人員姓名
        //    if (d.sendName.Length > 0 & !validate.FullWidthWord(d.sendName))
        //        err.msg += "「送件人員姓名」、";

        //    return err;
        //}



        /// <summary>
        /// 檢核資料
        /// </summary>
        /// <param name="d"></param>
        /// <param name="err"></param>
        /// <returns></returns>
        private OAP0001Model chkData(OAP0001FileModel d, OAP0001Model err, string impType, OAP0001PYCK pyck) {
            ValidateUtil validate = new ValidateUtil();

           

            //保單號碼、給付對象ID不可同時為空白
            if ("".Equals(d.policyNo) && "".Equals(d.paidId) & ("1".Equals(impType) || "3".Equals(impType)))
                err.msg += "上傳資料有誤，請確認!!" + "<br/>";
            else {
                //保單號碼
                switch (impType)
                {
                    case "1":
                    case "3":
                        if (d.policyNo.Length == 0 || d.policyNo.Length > 10)
                            err.msg += "上傳資料有誤，請確認!!" + "<br/>";
                        break;
                    case "2":
                        if (!"".Equals(d.policyNo) && d.policyNo.Length > 10)
                            err.msg += "上傳資料有誤，請確認!!" + "<br/>";
                        break;
                }
            }


            //保單序號
            if ("".Equals(d.policyNo)) {
                if (d.policySeq.Length > 0 && d.policySeq != "0")
                    err.msg += "「保單序號」錯誤，請確認!!" + "<br/>";
                else
                    d.policySeq = "0";
            }
            else
            {
                if (d.policySeq.Length > 2 || d.policySeq.Length == 0 || !validate.IsNum(d.policySeq))
                    err.msg += "「保單序號」錯誤，請確認!!" + "<br/>";
            }

            


            //重覆碼
            if (!"".Equals(d.idDup) && d.idDup.Length != 1)
                err.msg += "「重覆碼」錯誤，請確認!!" + "<br/>";

            //案號人員別
            if (!"".Equals(d.memberId) && d.memberId.Length > 3)
                err.msg += "「案號人員別」錯誤，請確認!!" + "<br/>";

            //案號
            if ("".Equals(d.changeId))
                err.msg += "「案號」不可空白!!" + "<br/>";
            if (!"".Equals(d.changeId) && d.changeId.Length > 10)
                err.msg += "「案號」錯誤，請確認!!" + "<br/>";

            //給付對象 ID 
            if (!"".Equals(d.paidId) && d.paidId.Length > 10)
                err.msg += "「給付對象 ID」錯誤，請確認!!" + "<br/>";

            //給付對象姓名
            if ("".Equals(d.paidName))
                err.msg += "「給付對象姓名」不可空白!!" + "<br/>";

            if (!"".Equals(d.paidName) && (d.paidName.Length > 25 || !validate.FullWidthWord(d.paidName)))
                err.msg += "「給付對象姓名」錯誤，請確認!!" + "<br/>";

            //要保人ID 
            if (!"".Equals(d.applId) && d.applId.Length > 10)
                err.msg += "「要保人ID」錯誤，請確認!!" + "<br/>";

            //要保人姓名
            if (!"".Equals(d.applName) && (d.applName.Length > 25 || !validate.FullWidthWord(d.applName)))
                err.msg += "「要保人姓名」錯誤，請確認!!" + "<br/>";

            //被保人ID 
            if (!"".Equals(d.insId) && d.insId.Length > 10)
                err.msg += "「被保人ID」錯誤，請確認!!" + "<br/>";

            //被保人姓名
            if (!"".Equals(d.insName) && (d.insName.Length > 25 || !validate.FullWidthWord(d.insName)))
                err.msg += "「被保人姓名」錯誤，請確認!!" + "<br/>";

            //原給付日
            if (d.oPaidDt.Length == 0 || "0".Equals(d.oPaidDt) || d.oPaidDt.Length > 8)
                err.msg += "「原給付日」錯誤，請確認!!" + "<br/>";

            //幣別
            if (!"NTD".Equals(d.currency))
                err.msg += "非台幣請確認!!" + "<br/>";

            //回存金額
            if (d.mainAmt.Length == 0 || "0".Equals(d.mainAmt) || d.mainAmt.Length > 16 || !validate.IsNum(d.mainAmt))
                err.msg += "「回存金額」錯誤，請確認!!" + "<br/>";
            else {
                //"回存金額"需與"支票金額"一致
                try
                {
                    if (Convert.ToDecimal(d.mainAmt).CompareTo(Convert.ToDecimal(d.checkAmt)) != 0)
                        err.msg += "「回存金額」與「支票金額」不一致，請確認!!" + "<br/>";
                }
                catch (Exception e)
                {
                    err.msg += "「回存金額」與「支票金額」不一致，請確認!!" + "<br/>";
                }
            }

           


            //支票金額
            if (d.checkAmt.Length == 0 || "0".Equals(d.checkAmt) || d.checkAmt.Length > 16 || !validate.IsNum(d.checkAmt))
                err.msg += "「支票金額」錯誤，請確認!!" + "<br/>";
            else {
                //"支票金額"需與"應付票據檔-金額"一致
                try
                {
                    if (Convert.ToDecimal(d.checkAmt).CompareTo(Convert.ToDecimal(pyck.checkAmt)) != 0)
                        err.msg += "「支票金額」與「應付票據檔-金額」不一致，請確認!!" + "<br/>";
                }
                catch (Exception e)
                {
                    err.msg += "「支票金額」與「應付票據檔-金額」不一致，請確認!!" + "<br/>";
                }
            }

            //支票號碼／匯費序號
            if ("".Equals(d.checkNo) || d.checkNo.Length > 15 || "".Equals(pyck.checkNo))
                err.msg += "「支票號碼／匯費序號」錯誤，請確認!!" + "<br/>";

            //支票帳號簡稱
            if (d.checkShrt.Length == 0 || d.checkShrt.Length > 2 || "".Equals(pyck.checkShrt))
                err.msg += "「支票帳號簡稱」錯誤，請確認!!" + "<br/>";




            //支票到期日
            if (d.checkDate.Length == 0 || "0".Equals(d.checkDate) || d.checkDate.Length > 8)
                err.msg += "「支票到期日」錯誤，請確認!!" + "<br/>";
            else {
                //"支票到期日"需與"應付票據檔-支票到期日"一致
                if (!d.checkDate.PadLeft(8, '0').Equals(pyck.checkDate.PadLeft(8, '0')))
                    err.msg += "「支票到期日」錯誤，請確認!!" + "<br/>";
            }



            //原給付性質
            if ("".Equals(d.oPaidCd) || d.oPaidCd.Length > 10)
                err.msg += "「原給付性質」錯誤，請確認!!" + "<br/>";

            //比對到之郵遞區號
            if (!"".Equals(d.rZipCode) && d.rZipCode.Length > 5)
                err.msg += "「郵遞區號/地址」錯誤，請確認!!" + "<br/>";

            //比對到之地址
            if (!"".Equals(d.rAddr) && (d.rAddr.Length > 35 || !validate.FullWidthWord(d.rAddr)))
                err.msg += "「郵遞區號/地址」錯誤，請確認!!" + "<br/>";

            //"郵遞區號"及"地址"必需同時有值..或沒有值
            if (("".Equals(d.rZipCode) && !"".Equals(d.rAddr)) || (!"".Equals(d.rZipCode) && "".Equals(d.rAddr)))
                err.msg += "「郵遞區號/地址」錯誤，請確認!!" + "<br/>";

            //送件人員
            if (!"".Equals(d.sendId) && d.sendId.Length > 10)
                err.msg += "「送件人員」錯誤，請確認!!" + "<br/>";

            //送件人員姓名
            if (!"".Equals(d.sendName) && (d.sendName.Length > 6 || !validate.FullWidthWord(d.sendName)))
                err.msg += "「送件人員姓名」錯誤，請確認!!" + "<br/>";

            //送件人員單位
            if (!"".Equals(d.sendUnit) && d.sendUnit.Length > 9)
                err.msg += "「送件人員單位」錯誤，請確認!!" + "<br/>";

            //報表代碼
            if (!("A".Equals(d.report) || "B".Equals(d.report)))
                err.msg += "「報表代碼」只可A或B!!" + "<br/>";

            //作廢作碼
            if(!("".Equals(d.delCode) || "*".Equals(d.delCode)))
                err.msg += "「作廢作碼」錯誤，請確認!!" + "<br/>";

            return err;
        }



    }
}