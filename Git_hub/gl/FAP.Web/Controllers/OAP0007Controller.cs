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
using System.IO;
using System.Linq;
using System.Web.Mvc;

/// <summary>
/// 功能說明：逾期未兌領整批匯入功能
/// 初版作者：20190618 Daiyu
/// 修改歷程：20190618 Daiyu
///           需求單號：
///           初版
/// </summary>
///

namespace FAP.Web.Controllers
{
    [Authorize]
    [CheckSessionFilterAttribute]
    public class OAP0007Controller : BaseController
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
            string[] roleInfo = authUtil.chkUserFuncAuth(Session["UserID"].ToString(), "~/OAP0007/");
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

            int successCnt = 0;
            int errCnt = 0;
            int fileCnt = 0;
            int dupCnt = 0;

            List<OAP0007Model> fileList = new List<OAP0007Model>();
            List<OAP0007Model> errList = new List<OAP0007Model>();

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

                    string projectFile = Server.MapPath("~/FileUploads/OAP0007"); //專案資料夾
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


                        //取得逾期未兌領設定檔相關代碼
                        FAPVeCodeDao fAPVeCodeDao = new FAPVeCodeDao();

                        //CLR_CERT_DOC 証明文件
                        List<FAP_VE_CODE> certList = fAPVeCodeDao.qryByGrp("CLR_CERT_DOC");

                        //CLR_LEVEL1 清理大類
                        List<FAP_VE_CODE> level1List = fAPVeCodeDao.qryByGrp("CLR_LEVEL1");

                        //CLR_LEVEL2 清理小類
                        List<FAP_VE_CODE> level2List = fAPVeCodeDao.qryByGrp("CLR_LEVEL2");

                        //CLR_PRACTICE 踐行程序
                        List<FAP_VE_CODE> practiceList = fAPVeCodeDao.qryByGrp("CLR_PRACTICE");


                        FAPVeTrackProcDao trackProcDao = new FAPVeTrackProcDao();
                        FAPVeTraceDao veTrackDao = new FAPVeTraceDao();

                        for (int i = 0; i < fileCnt; i++)
                        {
                            OAP0007Model d = new OAP0007Model();
                            d.linePos = table.Rows[i]["line"]?.ToString();

                            try
                            {
                                d.paid_id = table.Rows[i]["paid_id"]?.ToString().ToUpper();
                                d.check_no = table.Rows[i]["check_no"]?.ToString().ToUpper();
                                d.check_acct_short = table.Rows[i]["check_acct_short"]?.ToString().ToUpper();
                                d.level_1 = table.Rows[i]["level_1"]?.ToString().ToUpper();
                                d.level_2 = table.Rows[i]["level_2"]?.ToString().ToUpper();
                                d.practice = table.Rows[i]["practice"]?.ToString().ToUpper();
                                d.cert_doc = table.Rows[i]["cert_doc"]?.ToString().ToUpper();
                                d.exec_date = table.Rows[i]["exec_date"]?.ToString();
                                d.proc_desc = table.Rows[i]["proc_desc"]?.ToString();

                                checkImpData(d, trackProcDao, veTrackDao, certList, level1List, level2List, practiceList);


                                if (fileList.Where(x => x.paid_id == d.paid_id & x.check_no == d.check_no & x.check_acct_short == d.check_acct_short
                                  & x.practice == d.practice & x.cert_doc == d.cert_doc & x.exec_date == d.exec_date).Count() > 0)
                                {
                                    d.msg += "上傳檔案內存在相同「給付對象ID + 支票號碼 + 支票帳號簡稱 + 踐行程序 + 証明文件 + 執行日期」的資料，請確認!!" + "<br/>";
                                    d.chkFlag =  "E";
                                }

                                foreach (OAP0007Model model in fileList.Where(x => x.check_no == d.check_no & x.check_acct_short == d.check_acct_short).ToList()){
                                    if (!StringUtil.toString(d.level_1).Equals(StringUtil.toString(model.level_1))
                                        || !StringUtil.toString(d.level_2).Equals(StringUtil.toString(model.level_2))) {
                                        d.msg += "上傳檔案內存在相同「支票號碼 + 支票帳號簡稱」的資料清理大類及小類不一致，請確認!!" + "<br/>";
                                        d.chkFlag = "E";
                                    }
                                }


                                fileList.Add(d);

                                switch (d.chkFlag)
                                {
                                    case "W":
                                        errList.Add(d);
                                        dupCnt++;
                                        break;
                                    case "E":
                                        errList.Add(d);
                                        errCnt++;
                                        break;
                                    default:
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
                if ("I".Equals(execAction) && errCnt == 0)
                {
                    string msg = procImpDb(fileList);

                    if (!"".Equals(msg))
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
                else
                {
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
            catch (Exception e)
            {
                return Json(new { success = false, err = "其它錯誤，請洽系統管理員!!" });
            }
        }




        /// <summary>
        /// 資料寫入DB
        /// </summary>
        /// <param name="dataList"></param>
        /// <returns></returns>
        public string procImpDb(List<OAP0007Model> dataList)
        {
            string msg = "";

            string strConn = DbUtil.GetDBFglConnStr();
            using (SqlConnection conn = new SqlConnection(strConn))
            {
                conn.Open();

                SqlTransaction transaction = conn.BeginTransaction("Transaction");
                try
                {
                    DateTime dt = DateTime.Now;
                    FAPVeTrackProcDao trackProcDao = new FAPVeTrackProcDao();
                    FAPVeTraceDao veTrackDao = new FAPVeTraceDao();

                    foreach (OAP0007Model d in dataList)
                    {
                        FAP_VE_TRACE_PROC proc = new FAP_VE_TRACE_PROC();
                        proc = scmodelToDb(d, proc);


                        //異動【FAP_VE_TRACE_PROC 清理記錄歷程檔】
                        if ("".Equals(d.chkFlag)) {
                            writePiaLog(d.check_no, 1, d.paid_id, "A");
                            trackProcDao.insert(dt, Session["UserID"].ToString(), proc, conn, transaction);
                        } else {
                            writePiaLog(d.check_no, 1, d.paid_id, "E");
                            trackProcDao.update(dt, Session["UserID"].ToString(), proc, conn, transaction);
                        }



                        //異動【FAP_VE_TRACE 清理記錄檔】的清理大小類、執行日期
                        DateTime? maxExecDate = d.last_exec_date == "" ? proc.exec_date : (Convert.ToDateTime(d.last_exec_date) > proc.exec_date ? Convert.ToDateTime(d.last_exec_date) : proc.exec_date);
                        veTrackDao.updateForProc(dt, Session["UserID"].ToString(), d.check_no, d.check_acct_short, maxExecDate, d.level_1, d.level_2, d.chkLevelChg
                            , conn, transaction);

                    }

                    transaction.Commit();
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
        private OAP0007Model checkImpData(OAP0007Model d, FAPVeTrackProcDao trackProcDao, FAPVeTraceDao veTrackDao
            , List<FAP_VE_CODE> certList, List<FAP_VE_CODE> level1List, List<FAP_VE_CODE> level2List, List<FAP_VE_CODE> practiceList)
        {

            FAP_VE_TRACE veTrace = new FAP_VE_TRACE();
            ValidateUtil validateUtil = new ValidateUtil();

            #region 給付對象ID
            if ("".Equals(StringUtil.toString(d.paid_id)) || StringUtil.toString(d.paid_id).Length > 10)
            {
                d.msg += "「給付對象ID」錯誤，請確認!!" + "<br/>";
                d.chkFlag = d.chkFlag == "" ? "E" : d.chkFlag;
            }
            #endregion

            #region 支票號碼
            if ("".Equals(StringUtil.toString(d.check_no)) || StringUtil.toString(d.check_no).Length > 10)
            {
                d.msg += "「支票號碼」錯誤，請確認!!" + "<br/>";
                d.chkFlag = d.chkFlag == "" ? "E" : d.chkFlag;
            }
            else
            {
                veTrace = veTrackDao.qryByCheckNo(d.check_no, StringUtil.toString(d.check_acct_short));
            }
            #endregion

            #region 支票帳號簡稱
            if ("".Equals(StringUtil.toString(d.check_acct_short)) || StringUtil.toString(d.check_acct_short).Length != 2)
            {
                d.msg += "「支票帳號簡稱」錯誤，請確認!!" + "<br/>";
                d.chkFlag = d.chkFlag == "" ? "E" : d.chkFlag;
            }
            #endregion

            #region 清理大類
            if ("".Equals(StringUtil.toString(d.level_1)))
            {
                d.msg += "「清理大類」錯誤，請確認!!" + "<br/>";
                d.chkFlag = d.chkFlag == "" ? "E" : d.chkFlag;
            }
            else
            {
                FAP_VE_CODE code = level1List.Where(x => x.code_id == d.level_1).FirstOrDefault();
                if (code == null)
                {
                    d.msg += "「清理大類」錯誤，請確認!!" + "<br/>";
                    d.chkFlag = d.chkFlag == "" ? "E" : d.chkFlag;
                }
            }
            #endregion

            #region 清理小類
            if ("".Equals(StringUtil.toString(d.level_2)))
            {
                d.msg += "「清理小類」錯誤，請確認!!" + "<br/>";
                d.chkFlag = d.chkFlag == "" ? "E" : d.chkFlag;
            }
            else
            {
                FAP_VE_CODE code = level2List.Where(x => x.code_id == d.level_2).FirstOrDefault();
                if (code == null)
                {
                    d.msg += "「清理小類」錯誤，請確認!!" + "<br/>";
                    d.chkFlag = d.chkFlag == "" ? "E" : d.chkFlag;
                }
            }
            #endregion


            #region 踐行程序
            if ("".Equals(StringUtil.toString(d.practice)))
            {
                d.msg += "「踐行程序」錯誤，請確認!!" + "<br/>";
                d.chkFlag = d.chkFlag == "" ? "E" : d.chkFlag;
            }
            else
            {
                FAP_VE_CODE code = practiceList.Where(x => x.code_id == d.practice).FirstOrDefault();
                if (code == null)
                {
                    d.msg += "「踐行程序」錯誤，請確認!!" + "<br/>";
                    d.chkFlag = d.chkFlag == "" ? "E" : d.chkFlag;
                }
            }
            #endregion


            #region 証明文件
            if ("".Equals(StringUtil.toString(d.cert_doc)))
            {
                d.msg += "「証明文件」錯誤，請確認!!" + "<br/>";
                d.chkFlag = d.chkFlag == "" ? "E" : d.chkFlag;
            }
            else
            {
                FAP_VE_CODE code = certList.Where(x => x.code_id == d.cert_doc).FirstOrDefault();
                if (code == null)
                {
                    d.msg += "「証明文件」錯誤，請確認!!" + "<br/>";
                    d.chkFlag = d.chkFlag == "" ? "E" : d.chkFlag;
                }
            }
            #endregion

            #region 執行日期
            if ("".Equals(StringUtil.toString(d.exec_date)))
            {
                d.msg += "「執行日期」錯誤，請確認!!" + "<br/>";
                d.chkFlag = d.chkFlag == "" ? "E" : d.chkFlag;
            }
            else
            {
                if (!validateUtil.chkChtDate(d.exec_date))
                {
                    d.msg += "「執行日期」錯誤，請確認!!" + "<br/>";
                    d.chkFlag = d.chkFlag == "" ? "E" : d.chkFlag;
                }
                else {
                    //不可大於系統日
                    string input = StringUtil.toString(d.exec_date).PadLeft(8, '0');
                    DateTime exec_date = Convert.ToDateTime((Convert.ToInt16(input.Substring(0, 4)) + 1911).ToString() + "/" + input.Substring(4, 2) + "/" + input.Substring(6, 2));
                    if (exec_date.CompareTo(DateTime.Now) > 0) {
                        d.msg += "「執行日期」錯誤，請確認!!" + "<br/>";
                        d.chkFlag = d.chkFlag == "" ? "E" : d.chkFlag;
                    }
                        
                }
            }
            #endregion

            //#region 過程說明
            //if ("".Equals(StringUtil.toString(d.proc_desc)) || StringUtil.toString(d.proc_desc).Length > 300)
            //{
            //    d.msg += "「過程說明」錯誤，請確認!!" + "<br/>";
            //    d.chkFlag = d.chkFlag == "" ? "E" : d.chkFlag;
            //}
            //#endregion

            #region 檢查是否與【清理記錄檔】一致
            if ("".Equals(StringUtil.toString(veTrace.check_no)))
            {
                
                d.msg += "「支票號碼 + 支票帳號簡稱」不存在【清理記錄檔】，請確認!!" + "<br/>";
                d.chkFlag = d.chkFlag == "" ? "E" : d.chkFlag;
            }
            else
            {
                d.last_exec_date = veTrace.exec_date.ToString();


                //清理狀態=不可以為已清理結案或已給付
                if ("1".Equals(veTrace.status) || "2".Equals(veTrace.status)) {
                    d.msg += "此支票號碼的清理狀態不可以為已清理結案或已給付!!" + "<br/>";
                    d.chkFlag = d.chkFlag == "" ? "E" : d.chkFlag;
                }


                if (!StringUtil.toString(d.paid_id).Equals(StringUtil.toString(veTrace.paid_id)))
                {
                    d.msg += "「給付對象ID」與【清理記錄檔】不一致，請確認!!" + "<br/>";
                    d.chkFlag = d.chkFlag == "" ? "E" : d.chkFlag;
                }

                if (!StringUtil.toString(d.check_acct_short).Equals(StringUtil.toString(veTrace.check_acct_short)))
                {
                    d.msg += "「支票帳號簡稱」與【清理記錄檔】不一致，請確認!!" + "<br/>";
                    d.chkFlag = d.chkFlag == "" ? "E" : d.chkFlag;
                }

                if (!"".EndsWith(StringUtil.toString(veTrace.level_1)))
                {
                    if (!StringUtil.toString(d.level_1).Equals(StringUtil.toString(veTrace.level_1))) {
                        d.msg += "「清理大類」與【清理記錄檔】不一致，請確認!!" + "<br/>";
                        d.chkFlag = d.chkFlag == "" ? "E" : d.chkFlag;
                    }
                } else
                    d.chkLevelChg = true;

                if (!"".EndsWith(StringUtil.toString(veTrace.level_2)) )
                {
                    if (!StringUtil.toString(d.level_2).Equals(StringUtil.toString(veTrace.level_2))) {
                        d.msg += "「清理小類」與【清理記錄檔】不一致，請確認!!" + "<br/>";
                        d.chkFlag = d.chkFlag == "" ? "E" : d.chkFlag;
                    }
                } else
                    d.chkLevelChg = true;
            }
            #endregion


            #region 檢查【清理記錄歷程檔】中已存在對應「給付對象ID + 支票號碼 + 支票帳號簡稱 + 踐行程序 + 証明文件 + 執行日期」
            FAP_VE_TRACE_PROC traceProc = new FAP_VE_TRACE_PROC();
            traceProc = scmodelToDb(d, traceProc);

            traceProc = trackProcDao.qryByKey(traceProc);

            if (!"".Equals(StringUtil.toString(traceProc.check_no)))
            {
                d.msg += "「重覆資料」，請確認!!" + "<br/>";
                d.chkFlag = d.chkFlag == "" ? "W" : d.chkFlag;
            }
            #endregion

            return d;
        }



        public FAP_VE_TRACE_PROC scmodelToDb(OAP0007Model scModel, FAP_VE_TRACE_PROC traceProc) {
            traceProc.paid_id = scModel.paid_id;
            traceProc.check_no = scModel.check_no;
            traceProc.check_acct_short = scModel.check_acct_short;
            traceProc.practice = scModel.practice;
            traceProc.cert_doc = scModel.cert_doc;
            traceProc.exec_date = Convert.ToDateTime(BO.DateUtil.As400ChtDateToADDate(scModel.exec_date));
            traceProc.proc_desc = scModel.proc_desc;

            return traceProc;
        }


        private void writePiaLog(string checkNo, int affectRows, string piaOwner, string executionType)
        {
            PIA_LOG_MAIN piaLogMain = new PIA_LOG_MAIN();
            piaLogMain.TRACKING_TYPE = "A";
            piaLogMain.ACCESS_ACCOUNT = Session["UserID"].ToString();
            piaLogMain.ACCOUNT_NAME = Session["UserName"].ToString();
            piaLogMain.PROGFUN_NAME = "OAP0007Controller";
            piaLogMain.EXECUTION_CONTENT = checkNo;
            piaLogMain.AFFECT_ROWS = affectRows;
            piaLogMain.PIA_TYPE = "0100100000";
            piaLogMain.EXECUTION_TYPE = executionType;
            piaLogMain.ACCESSOBJ_NAME = "FAP_VE_TRACE_PROC";
            piaLogMain.PIA_OWNER1 = piaOwner;
            PiaLogMainDao piaLogMainDao = new PiaLogMainDao();
            piaLogMainDao.Insert(piaLogMain);

        }

    }
}