using FGL.Web.ActionFilter;
using FGL.Web.BO;
using FGL.Web.Daos;
using FGL.Web.Models;
using FGL.Web.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Web.Mvc;
using System.Linq;
using System.IO;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using System.Data;
using NPOI.XSSF.UserModel;
using Newtonsoft.Json.Linq;
using Microsoft.Reporting.WebForms;

/// <summary>
/// 功能說明：保險商品編號代碼轉換作業
/// 初版作者：20181115 Daiyu
/// 修改歷程：20181115 Daiyu
///           需求單號：201805080167-00
///           初版
/// </summary>
///

namespace FGL.Web.Controllers
{
    [Authorize]
    [CheckSessionFilterAttribute]
    public class OGL00002Controller : BaseController
    {
        static private NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();


        /// <summary>
        /// 畫面初始
        /// </summary>
        /// <returns></returns>
        public ActionResult Index()
        {
            logger.Info("Index begin");

            UserAuthUtil authUtil = new UserAuthUtil();


            string opScope = "";
            string funcName = "";


            if (Session["UserID"] != null)
            {
                string[] roleInfo = authUtil.chkUserFuncAuth(Session["UserID"].ToString(), "~/OGL00002/");
                if (roleInfo != null && roleInfo.Length == 2)
                {
                    opScope = roleInfo[0];
                    funcName = roleInfo[1];
                }
            }


            ViewBag.funcName = funcName;
            ViewBag.opScope = opScope;


            logger.Info("Index end");
            return View();
        }



        /// <summary>
        /// 將畫面修改的資料寫入暫存檔
        /// </summary>
        /// <param name="model"></param>
        /// <param name="execAction"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult procHisDetail(OGL00002Model model, string execAction)
        {
            errModel errModel = execTmpSave(model, execAction);

            return Json(new { success = errModel.success, err = errModel.err }, JsonRequestBehavior.AllowGet);
        }




        /// <summary>
        /// 刪除暫存資料
        /// </summary>
        /// <param name="aplyNo"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult delTmpData(string aplyNo)
        {
            string strConn = DbUtil.GetDBFglConnStr();
            using (SqlConnection conn = new SqlConnection(strConn))
            {
                conn.Open();

                SqlTransaction transaction = conn.BeginTransaction("Transaction");

                try
                {
                    FGLAplyRecDao fGLAplyRecDao = new FGLAplyRecDao();
                    fGLAplyRecDao.delByAplyNo(aplyNo, conn, transaction);

                    FGLItemAcctHisDao fGLItemAcctHisDao = new FGLItemAcctHisDao();
                    fGLItemAcctHisDao.delByAplyNo(aplyNo, conn, transaction);

                    transaction.Commit();
                }
                catch (Exception e)
                {
                    transaction.Rollback();
                    logger.Error("其它錯誤：" + e.ToString());

                    return Json(new { success = false, err = "其它錯誤，請洽系統管理員!!" }, JsonRequestBehavior.AllowGet);
                }
            }

            var jsonData = new { success = true };
            return Json(jsonData, JsonRequestBehavior.AllowGet);
        }



        
        private List<OGL00002Model> execQry(OGL00002Model model, bool onlyTmp) {

            List<OGL00002Model> rows = new List<OGL00002Model>();
            List<OGL00002Model> dataList = new List<OGL00002Model>();


            //查詢暫存檔
            if (model.isQryTmp == "1")
            {
                FGLItemCodeTranHisDao fGLItemCodeTranHisDao = new FGLItemCodeTranHisDao();
                if (onlyTmp)
                    rows = fGLItemCodeTranHisDao.qryByTranCodeTmp(model);
                else
                    rows = fGLItemCodeTranHisDao.qryByTranCode(model);
            }
            else
            {
                FGLItemCodeTranDao fGLItemCodeTranDao = new FGLItemCodeTranDao();
                rows = fGLItemCodeTranDao.qryByTranCode(model);
            }


            foreach (OGL00002Model d in rows)
            {
                bool bShow = true;
                string productNo = d.productNo.PadRight(27, ' ');

                OGL00002Model type1 = new OGL00002Model();
                // type1.tempId = d.tempId + "|" + "1";
                type1.tempId = d.tempId;
                //  type1.rowType = "1";
                type1.p01 = productNo.Substring(0, 1);
                type1.p02 = productNo.Substring(1, 1);
                type1.p03 = productNo.Substring(2, 1);
                type1.p04 = productNo.Substring(3, 1);
                type1.p05 = productNo.Substring(4, 1);
                type1.p06 = productNo.Substring(5, 1);
                type1.p07 = productNo.Substring(6, 1);
                type1.p08 = productNo.Substring(7, 1);
                type1.p09 = productNo.Substring(8, 1);
                type1.p10 = productNo.Substring(9, 1);
                type1.p11 = productNo.Substring(10, 1);
                type1.p12 = productNo.Substring(11, 1);
                type1.p13 = productNo.Substring(12, 1);
                type1.p14 = productNo.Substring(13, 1);
                type1.p15 = productNo.Substring(14, 1);
                type1.p16 = productNo.Substring(15, 1);
                type1.p17 = productNo.Substring(16, 1);
                type1.p18 = productNo.Substring(17, 1);
                type1.p19 = productNo.Substring(18, 1);
                type1.p20 = productNo.Substring(19, 1);
                type1.p21 = productNo.Substring(20, 1);
                type1.p22 = productNo.Substring(21, 1);
                type1.p23 = productNo.Substring(22, 1);
                type1.p24 = productNo.Substring(23, 1);
                type1.p25 = productNo.Substring(24, 1);
                type1.p26 = productNo.Substring(25, 1);
                type1.p27 = productNo.Substring(26, 1);

                type1.tranA = d.tranA;
                type1.tranB = d.tranB;
                type1.tranC = d.tranC;
                type1.tranD = d.tranD;
                type1.tranE = d.tranE;
                type1.tranF = d.tranF;
                type1.tranG = d.tranG;
                type1.tranH = d.tranH;
                type1.tranI = d.tranI;
                type1.tranJ = d.tranJ;
                type1.tranK = d.tranK;

                type1.dataStatus = d.dataStatus;
                type1.dataStatusDesc = d.dataStatusDesc;
                type1.execAction = d.execAction;
                type1.execActionDesc = d.execActionDesc;

                type1.updateId = d.updateId;
                type1.updateDatetime = d.updateDatetime;
                type1.apprId = d.apprId;
                type1.apprDt = d.apprDt;


                //20190708 加"查詢條件可以下保險商品編號1~27碼欄位"
                if (!"".Equals(StringUtil.toString(model.p01)) & !StringUtil.toString(type1.p01).Equals((StringUtil.toString(model.p01))))
                    bShow = false;

                if (!"".Equals(StringUtil.toString(model.p02)) & !StringUtil.toString(type1.p02).Equals((StringUtil.toString(model.p02))))
                    bShow = false;

                if (!"".Equals(StringUtil.toString(model.p03)) & !StringUtil.toString(type1.p03).Equals((StringUtil.toString(model.p03))))
                    bShow = false;

                if (!"".Equals(StringUtil.toString(model.p04)) & !StringUtil.toString(type1.p04).Equals((StringUtil.toString(model.p04))))
                    bShow = false;

                if (!"".Equals(StringUtil.toString(model.p05)) & !StringUtil.toString(type1.p05).Equals((StringUtil.toString(model.p05))))
                    bShow = false;

                if (!"".Equals(StringUtil.toString(model.p06)) & !StringUtil.toString(type1.p06).Equals((StringUtil.toString(model.p06))))
                    bShow = false;

                if (!"".Equals(StringUtil.toString(model.p07)) & !StringUtil.toString(type1.p07).Equals((StringUtil.toString(model.p07))))
                    bShow = false;

                if (!"".Equals(StringUtil.toString(model.p08)) & !StringUtil.toString(type1.p08).Equals((StringUtil.toString(model.p08))))
                    bShow = false;

                if (!"".Equals(StringUtil.toString(model.p09)) & !StringUtil.toString(type1.p09).Equals((StringUtil.toString(model.p09))))
                    bShow = false;

                if (!"".Equals(StringUtil.toString(model.p10)) & !StringUtil.toString(type1.p10).Equals((StringUtil.toString(model.p10))))
                    bShow = false;

                if (!"".Equals(StringUtil.toString(model.p11)) & !StringUtil.toString(type1.p11).Equals((StringUtil.toString(model.p11))))
                    bShow = false;

                if (!"".Equals(StringUtil.toString(model.p12)) & !StringUtil.toString(type1.p12).Equals((StringUtil.toString(model.p12))))
                    bShow = false;

                if (!"".Equals(StringUtil.toString(model.p13)) & !StringUtil.toString(type1.p13).Equals((StringUtil.toString(model.p13))))
                    bShow = false;

                if (!"".Equals(StringUtil.toString(model.p14)) & !StringUtil.toString(type1.p14).Equals((StringUtil.toString(model.p14))))
                    bShow = false;

                if (!"".Equals(StringUtil.toString(model.p15)) & !StringUtil.toString(type1.p15).Equals((StringUtil.toString(model.p15))))
                    bShow = false;

                if (!"".Equals(StringUtil.toString(model.p16)) & !StringUtil.toString(type1.p16).Equals((StringUtil.toString(model.p16))))
                    bShow = false;

                if (!"".Equals(StringUtil.toString(model.p17)) & !StringUtil.toString(type1.p17).Equals((StringUtil.toString(model.p17))))
                    bShow = false;

                if (!"".Equals(StringUtil.toString(model.p18)) & !StringUtil.toString(type1.p18).Equals((StringUtil.toString(model.p18))))
                    bShow = false;

                if (!"".Equals(StringUtil.toString(model.p19)) & !StringUtil.toString(type1.p19).Equals((StringUtil.toString(model.p19))))
                    bShow = false;

                if (!"".Equals(StringUtil.toString(model.p20)) & !StringUtil.toString(type1.p20).Equals((StringUtil.toString(model.p20))))
                    bShow = false;

                if (!"".Equals(StringUtil.toString(model.p21)) & !StringUtil.toString(type1.p21).Equals((StringUtil.toString(model.p21))))
                    bShow = false;

                if (!"".Equals(StringUtil.toString(model.p22)) & !StringUtil.toString(type1.p22).Equals((StringUtil.toString(model.p22))))
                    bShow = false;

                if (!"".Equals(StringUtil.toString(model.p23)) & !StringUtil.toString(type1.p23).Equals((StringUtil.toString(model.p23))))
                    bShow = false;

                if (!"".Equals(StringUtil.toString(model.p24)) & !StringUtil.toString(type1.p24).Equals((StringUtil.toString(model.p24))))
                    bShow = false;

                if (!"".Equals(StringUtil.toString(model.p25)) & !StringUtil.toString(type1.p25).Equals((StringUtil.toString(model.p25))))
                    bShow = false;

                if (!"".Equals(StringUtil.toString(model.p26)) & !StringUtil.toString(type1.p26).Equals((StringUtil.toString(model.p26))))
                    bShow = false;

                if (!"".Equals(StringUtil.toString(model.p27)) & !StringUtil.toString(type1.p27).Equals((StringUtil.toString(model.p27))))
                    bShow = false;


                if (bShow)
                    dataList.Add(type1);


            }

            using (DB_INTRAEntities dbIntra = new DB_INTRAEntities())
            {
                Dictionary<string, string> userNameMap = new Dictionary<string, string>();
                OaEmpDao oaEmpDao = new OaEmpDao();
                string uId = "";
                string apprId = "";

                foreach (OGL00002Model d in dataList)
                {
                    uId = StringUtil.toString(d.updateId);
                    if (!"".Equals(uId))
                    {
                        if (!userNameMap.ContainsKey(uId))
                            userNameMap = oaEmpDao.qryUsrName(userNameMap, uId, dbIntra);

                        d.updateId = userNameMap[uId] == "" ? d.updateId : userNameMap[uId];
                    }

                    apprId = StringUtil.toString(d.apprId);
                    if (!"".Equals(apprId))
                    {
                        if (!userNameMap.ContainsKey(apprId))
                            userNameMap = oaEmpDao.qryUsrName(userNameMap, apprId, dbIntra);

                        d.apprId = userNameMap[apprId] == "" ? d.apprId : userNameMap[apprId];
                    }
                }
            }

            return dataList;

        }




        /// <summary>
        /// 執行匯出
        /// </summary>
        /// <param name="model"></param>
        /// <param name="onlyTmp"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult execExport(OGL00002Model model, bool onlyTmp)
        {
            string fileName = "";

            try
            {
                List<OGL00002Model> dataList = execQry(model, onlyTmp);

                CommonUtil commonUtil = new CommonUtil();
                DataTable dtMain = commonUtil.ConvertToDataTable<OGL00002Model>(dataList);
                var ReportViewer1 = new ReportViewer();

                //清除資料來源
                ReportViewer1.LocalReport.DataSources.Clear();
                //指定報表檔路徑   
                ReportViewer1.LocalReport.ReportPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory + "Report\\Rdlc\\OGL00002P.rdlc");
                //設定資料來源
                ReportViewer1.LocalReport.DataSources.Add(new Microsoft.Reporting.WebForms.ReportDataSource("DataSet1", dtMain));

                //報表參數
                //ReportViewer1.LocalReport.SetParameters(new ReportParameter("Title", "給付介面自動傳送＿批次未接收報表"));

                // ReportViewer1.LocalReport.SubreportProcessing += new SubreportProcessingEventHandler(subReportProcessing);

                ReportViewer1.LocalReport.Refresh();

                Microsoft.Reporting.WebForms.Warning[] tWarnings;
                string[] tStreamids;
                string tMimeType;
                string tEncoding;
                string tExtension;
                fileName = "OGL00002P_" + BO.DateUtil.getCurDateTime("") + ".xls";
                //呼叫ReportViewer.LoadReport的Render function，將資料轉成想要轉換的格式，並產生成Byte資料
                byte[] tBytes = ReportViewer1.LocalReport.Render("EXCEL", null, out tMimeType, out tEncoding, out tExtension, out tStreamids, out tWarnings);

                using (FileStream fs = new FileStream(Path.Combine(AppDomain.CurrentDomain.BaseDirectory + "Temp\\") + fileName, FileMode.Create))
                {
                    fs.Write(tBytes, 0, tBytes.Length);
                }


                return Json(new { success = true, fileName = fileName });


            }
            catch (Exception e)
            {
                logger.Error(e.ToString());

                return Json(new { success = false, err = "其它錯誤，請洽系統管理員!!" }, JsonRequestBehavior.AllowGet);
            }


        }


        /// <summary>
        /// 匯出結果
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public FileContentResult downloadRpt(String fileName)
        {
            byte[] fileBytes = System.IO.File.ReadAllBytes(Server.MapPath("~/Temp/") + fileName);


            string fullPath = Server.MapPath("~/Temp/") + fileName;
            if (System.IO.File.Exists(fullPath))
            {
                System.IO.File.Delete(fullPath);
            }


            return File(fileBytes, "application/vnd.ms-excel", "OGL00002保險商品編號代碼轉換作業.xls");
        }



        /// <summary>
        /// 畫面執行"查詢"
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult loadData(OGL00002Model model, bool onlyTmp)
        {
            List<OGL00002Model> dataList = execQry(model, onlyTmp);
            var jsonData = new { success = true, dataList = dataList};

            //var jsonData = new { success = true, dataList = dataList.OrderBy(x => x.tempId.TrimEnd().Length).ThenBy(x => x.tempId).ThenBy(x => x.rowType) };
            return Json(jsonData, JsonRequestBehavior.AllowGet);
        }



        public JsonResult procImport()
        {
            List<string> errList = new List<string>();
            int iSuccess = 0;
            int iFail = 0;
            int iRow = 0;

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
                } else
                    return Json(new { success = false, err = "未上傳檔案" });

                string execSeq = Session["UserID"].ToString()
                    + "_" + BO.DateUtil.getCurChtDateTime().Replace(" ", "");
                var fileName = execSeq + "_" + Path.GetFileName(httpPostedFile.FileName); //檔案名稱

                string projectFile = Server.MapPath("~/FileUploads/itemCodeTran"); //專案資料夾
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

                    //由第二列取標題做為欄位名稱
                    IRow headerRow = sheet.GetRow(1);
                    int cellCount = headerRow.LastCellNum;


                    try
                    {
                        for (int i = headerRow.FirstCellNum; i < cellCount; i++)
                            //以欄位文字為名新增欄位，此處全視為字串型別以求簡化
                            table.Columns.Add(
                                new DataColumn(headerRow.GetCell(i).StringCellValue));

                    }
                    catch (Exception e) {
                        return Json(new { success = false, err = "[impitem]檔案內容需為文字格式!!" });
                    }


                   

                    int totCnt = 0;
                    //略過前兩列(標題列)，一直處理至最後一列
                    for (int i = (sheet.FirstRowNum + 2); i <= sheet.LastRowNum; i++)
                    {
                        IRow row = sheet.GetRow(i);
                        if (row == null) continue;
                        DataRow dataRow = table.NewRow();
                        //依先前取得的欄位數逐一設定欄位內容
                        for (int j = row.FirstCellNum; j < cellCount; j++)
                            if (row.GetCell(j) != null)
                                //如要針對不同型別做個別處理，可善用.CellType判斷型別
                                //再用.StringCellValue, .DateCellValue, .NumericCellValue...取值
                                //此處只簡單轉成字串
                                dataRow[j] = row.GetCell(j).ToString();
                        table.Rows.Add(dataRow);
                        totCnt++;

                    }

                    if (totCnt == 0)
                        return Json(new { success = false, err = "[impitem]檔案無明細內容!!" });


                    if(cellCount < 38)
                        return Json(new { success = false, err = "[impitem]檔案內容格式錯誤!!" });

                    foreach (DataRow dr in table.Rows)
                    {
                        iRow++;
                        OGL00002Model model = new OGL00002Model();


                        model.productNo = dr[0].ToString().ToUpper().PadRight(1, ' ')
                                        + dr[1].ToString().ToUpper().PadRight(1, ' ')
                                        + dr[2].ToString().ToUpper().PadRight(1, ' ')
                                        + dr[3].ToString().ToUpper().PadRight(1, ' ')
                                        + dr[4].ToString().ToUpper().PadRight(1, ' ')
                                        + dr[5].ToString().ToUpper().PadRight(1, ' ')
                                        + dr[6].ToString().ToUpper().PadRight(1, ' ')
                                        + dr[7].ToString().ToUpper().PadRight(1, ' ')
                                        + dr[8].ToString().ToUpper().PadRight(1, ' ')
                                        + dr[9].ToString().ToUpper().PadRight(1, ' ')
                                        + dr[10].ToString().ToUpper().PadRight(1, ' ')
                                        + dr[11].ToString().ToUpper().PadRight(1, ' ')
                                        + dr[12].ToString().ToUpper().PadRight(1, ' ')
                                        + dr[13].ToString().ToUpper().PadRight(1, ' ')
                                        + dr[14].ToString().ToUpper().PadRight(1, ' ')
                                        + dr[15].ToString().ToUpper().PadRight(1, ' ')
                                        + dr[16].ToString().ToUpper().PadRight(1, ' ')
                                        + dr[17].ToString().ToUpper().PadRight(1, ' ')
                                        + dr[18].ToString().ToUpper().PadRight(1, ' ')
                                        + dr[19].ToString().ToUpper().PadRight(1, ' ')
                                        + dr[20].ToString().ToUpper().PadRight(1, ' ')
                                        + dr[21].ToString().ToUpper().PadRight(1, ' ')
                                        + dr[22].ToString().ToUpper().PadRight(1, ' ')
                                        + dr[23].ToString().ToUpper().PadRight(1, ' ')
                                        + dr[24].ToString().ToUpper().PadRight(1, ' ')
                                        + dr[25].ToString().ToUpper().PadRight(1, ' ')
                                        + dr[26].ToString().ToUpper().PadRight(1, ' ');
                        model.tranA = dr[27].ToString().ToUpper();
                        model.tranB = dr[28].ToString().ToUpper();
                        model.tranC = dr[29].ToString().ToUpper();
                        model.tranD = dr[30].ToString().ToUpper();
                        model.tranE = dr[31].ToString().ToUpper();
                        model.tranF = dr[32].ToString().ToUpper();
                        model.tranG = dr[33].ToString().ToUpper();
                        model.tranH = dr[34].ToString().ToUpper();
                        model.tranI = dr[35].ToString().ToUpper();
                        model.tranJ = dr[36].ToString().ToUpper();
                        model.tranK = dr[37].ToString().ToUpper();

                        if (model.productNo.Trim().Length == 0 ) {
                            errList.Add("檔案位置:" + iRow.ToString() + "  " + "「保險商品編號」空白!!");
                            continue;
                        }

                        if (model.productNo.TrimEnd().Length > 27)
                        {
                            errList.Add("檔案位置:" + iRow.ToString() + "  " + "「保險商品編號」長度錯誤!!");
                            continue;
                        }

                        if (model.tranA.TrimEnd().Length > 2)
                        {
                            errList.Add("檔案位置:" + iRow.ToString() + "  " + "「轉換欄位A」長度錯誤!!");
                            continue;
                        }

                        if (model.tranB.TrimEnd().Length > 1)
                        {
                            errList.Add("檔案位置:" + iRow.ToString() + "  " + "「轉換欄位B」長度錯誤!!");
                            continue;
                        }

                        if (model.tranC.TrimEnd().Length > 1)
                        {
                            errList.Add("檔案位置:" + iRow.ToString() + "  " + "「轉換欄位C」長度錯誤!!");
                            continue;
                        }

                        if (model.tranD.TrimEnd().Length > 1)
                        {
                            errList.Add("檔案位置:" + iRow.ToString() + "  " + "「轉換欄位D」長度錯誤!!");
                            continue;
                        }

                        if (model.tranE.TrimEnd().Length > 1)
                        {
                            errList.Add("檔案位置:" + iRow.ToString() + "  " + "「轉換欄位E」長度錯誤!!");
                            continue;
                        }

                        if (model.tranF.TrimEnd().Length > 2)
                        {
                            errList.Add("檔案位置:" + iRow.ToString() + "  " + "「轉換欄位F」長度錯誤!!");
                            continue;
                        }

                        if (model.tranG.TrimEnd().Length > 1)
                        {
                            errList.Add("檔案位置:" + iRow.ToString() + "  " + "「轉換欄位G」長度錯誤!!");
                            continue;
                        }

                        if (model.tranH.TrimEnd().Length > 3)
                        {
                            errList.Add("檔案位置:" + iRow.ToString() + "  " + "「轉換欄位H」長度錯誤!!");
                            continue;
                        }

                        if (model.tranI.TrimEnd().Length > 4)
                        {
                            errList.Add("檔案位置:" + iRow.ToString() + "  " + "「轉換欄位I」長度錯誤!!");
                            continue;
                        }

                        if (model.tranJ.TrimEnd().Length > 1)
                        {
                            errList.Add("檔案位置:" + iRow.ToString() + "  " + "「轉換欄位J」長度錯誤!!");
                            continue;
                        }

                        if (model.tranK.TrimEnd().Length > 1)
                        {
                            errList.Add("檔案位置:" + iRow.ToString() + "  " + "「轉換欄位K」長度錯誤!!");
                            continue;
                        }

                        errModel impResult = execTmpSave(model, "A");
                        if (impResult.success == false) {
                            errList.Add("檔案位置:" + iRow.ToString() + "  " + impResult.err);
                            continue;
                        }

                        iSuccess++;
                    }
                }
            }

            iFail = iRow - iSuccess;

            if(errList.Count > 0)
                return Json(new { success = false, iRow = iRow, iSuccess = iSuccess, iFail = iFail
                    , err = "以下資料錯誤，未匯入暫存檔!!", errList = errList });
            else
                return Json(new { success = true, iRow = iRow, iSuccess = iSuccess, iFail = iFail });

        }


        public class errModel
        {
            public bool success { get; set; }
            public string err { get; set; }
        }



        /// <summary>
        /// 資料寫入暫存檔
        /// </summary>
        /// <param name="model"></param>
        /// <param name="execAction"></param>
        /// <returns></returns>
        private errModel execTmpSave(OGL00002Model model, string execAction)
        {
            errModel errModel = new errModel();

            /*------------------ DB處理   begin------------------*/
            string strConn = DbUtil.GetDBFglConnStr();
            using (SqlConnection conn = new SqlConnection(strConn))
            {
                conn.Open();

                SqlTransaction transaction = conn.BeginTransaction("Transaction");
                try
                {
                    FGLItemCodeTranHisDao fGLItemCodeTranHisDao = new FGLItemCodeTranHisDao();


                    FGL_ITEM_CODE_TRAN_HIS tranHis = new FGL_ITEM_CODE_TRAN_HIS();
                    tranHis.aply_no = "";
                    tranHis.exec_action = execAction;
                    tranHis.appr_stat = "0";
                    tranHis.product_no = model.productNo.TrimEnd();
                    tranHis.tran_a = model.tranA;
                    tranHis.tran_b = model.tranB;
                    tranHis.tran_c = model.tranC;
                    tranHis.tran_d = model.tranD;
                    tranHis.tran_e = model.tranE;
                    tranHis.tran_f = model.tranF;
                    tranHis.tran_g = model.tranG;
                    tranHis.tran_h = model.tranH;
                    tranHis.tran_i = model.tranI;
                    tranHis.tran_j = model.tranJ;
                    tranHis.tran_k = model.tranK;
                    tranHis.update_id = Session["UserID"].ToString();
                    tranHis.update_datetime = DateTime.Now;

                    //查詢正式檔資料
                    FGLItemCodeTranDao fGLItemCodeTranDao = new FGLItemCodeTranDao();
                    FGL_ITEM_CODE_TRAN formal = fGLItemCodeTranDao.qryByKey(model.productNo);


                    //更改單筆資料
                    model.tempId = model.tempId == null ? "" : model.tempId;
                    var temp = model.tempId;

                    //若已存在暫存檔，查詢暫存檔的資料
                    FGL_ITEM_CODE_TRAN_HIS hisO = new FGL_ITEM_CODE_TRAN_HIS();

                    if (!"".Equals(temp.TrimEnd()))
                    {
                        string productNo = temp.TrimEnd();


                        //判斷修改資料的原始「保險商品編號」是否已存在暫存檔
                        hisO = fGLItemCodeTranHisDao.qryByKey("", productNo);

                        if (hisO != null)
                        {
                            if ("D".Equals(execAction))
                            {
                                tranHis.product_no = productNo;
                                tranHis.exec_action = "D";

                                if ("A".Equals(hisO.exec_action))
                                    fGLItemCodeTranHisDao.deleteByKey(tranHis, conn, transaction);
                                else
                                    fGLItemCodeTranHisDao.updateByKey(tranHis, conn, transaction);
                            }
                            else
                            {
                                //若KEY項不相同時，要記兩筆異動檔(新增、刪除)
                                if (hisO.product_no.TrimEnd() != model.productNo.TrimEnd())
                                {
                                    if (formal != null) {
                                        errModel.success = false;
                                        errModel.err = "修改後的「保險商品編號」已存在，不可異動!!";
                                        return errModel;
                                    }
                                        
                                    else
                                    {

                                        tranHis.exec_action = "A";
                                        fGLItemCodeTranHisDao.insert(tranHis, conn, transaction);

                                        tranHis.exec_action = "D";
                                        tranHis.product_no = productNo;

                                        if ("A".Equals(hisO.exec_action))
                                            fGLItemCodeTranHisDao.deleteByKey(tranHis, conn, transaction);
                                        else
                                            fGLItemCodeTranHisDao.updateByKey(tranHis, conn, transaction);
                                    }

                                }
                                else
                                {
                                    tranHis.exec_action = hisO.exec_action == "A" ? "A" : "U";
                                    fGLItemCodeTranHisDao.updateByKey(tranHis, conn, transaction);
                                }

                            }
                        }
                        else
                        {
                            //hisO.PRODUCT_NO = productNo;


                            if ("D".Equals(execAction))
                                fGLItemCodeTranHisDao.insert(tranHis, conn, transaction);
                            else
                            {
                                if ("A".Equals(execAction) & formal != null)
                                {
                                    errModel.success = false;
                                    errModel.err = "修改後的「保險商品編號」已存在，不可異動!!";
                                    return errModel;
                                }

                                //若KEY項不相同時，要記兩筆異動檔(新增、刪除)
                                if (productNo.TrimEnd() != model.productNo.TrimEnd())
                                {
                                    FGL_ITEM_CODE_TRAN_HIS hisTobe = fGLItemCodeTranHisDao.qryByKey("", model.productNo.TrimEnd());
                                    if (hisTobe != null) {
                                        errModel.success = false;
                                        errModel.err = "同樣的「保險商品編號」已存在暫存檔，不可新增!!";
                                        return errModel;
                                    }

                                    if (formal != null)
                                    {
                                        errModel.success = false;
                                        errModel.err = "同樣的「保險商品編號」已存在正式檔，不可新增!!";
                                        return errModel;
                                    }


                                    tranHis.exec_action = "A";
                                    fGLItemCodeTranHisDao.insert(tranHis, conn, transaction);


                                    tranHis.exec_action = "D";
                                    tranHis.product_no = productNo;

                                    fGLItemCodeTranHisDao.insert(tranHis, conn, transaction);

                                }
                                else
                                    fGLItemCodeTranHisDao.insert(tranHis, conn, transaction);
                            }
                        }
                    }
                    else
                    {
                        if (!"D".Equals(execAction))
                        {
                            if ("A".Equals(execAction) & formal != null) {
                                errModel.success = false;
                                errModel.err = "同樣的「保險商品編號」已存在，不可新增!!";
                                return errModel;
                            }
                            else
                            {
                                FGL_ITEM_CODE_TRAN_HIS hisTobe = fGLItemCodeTranHisDao.qryByKey("", tranHis.product_no.TrimEnd());
                                if (hisTobe != null)
                                {
                                    errModel.success = false;
                                    errModel.err = "同樣的「保險商品編號」已存在暫存檔，不可新增!!";
                                    return errModel;
                                }

                                tranHis.exec_action = "A";
                                fGLItemCodeTranHisDao.insert(tranHis, conn, transaction);
                            }
                        }
                    }


                    transaction.Commit();

                    errModel.success = true;
                    errModel.err = "";
                    return errModel;

                }
                catch (Exception e)
                {
                    transaction.Rollback();
                    logger.Error("其它錯誤：" + e.ToString());

                    errModel.success = false;
                    errModel.err = "其它錯誤，請洽系統管理員!!";
                    return errModel;
                }
            }

            /*------------------ DB處理   end------------------*/
        }

        /// <summary>
        /// 畫面執行"申請覆核"
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public JsonResult execSave(List<OGL00002Model> dataList)
        {
            logger.Info("execSave begin");


            FGLItemCodeTranDao fGLItemCodeTranDao = new FGLItemCodeTranDao();


            if (dataList == null)
                return Json(new { success = false, err = "未異動畫面資料，將不進行修改覆核作業!!" }, JsonRequestBehavior.AllowGet);


            /*------------------ DB處理   begin------------------*/
            string strConn = DbUtil.GetDBFglConnStr();
            using (SqlConnection conn = new SqlConnection(strConn))
            {

                conn.Open();

                SqlTransaction transaction = conn.BeginTransaction("Transaction");
                try
                {
                    FGLAplyRecDao fGLAplyRecDao = new FGLAplyRecDao();
                    FGL_APLY_REC aplyRec = new FGL_APLY_REC();
                    aplyRec.aply_type = "D";
                    aplyRec.appr_stat = "1";
                    aplyRec.appr_mapping_key = "";
                    aplyRec.create_id = Session["UserID"].ToString();

                    //新增"覆核資料檔"
                    string aplyNo = fGLAplyRecDao.insert(aplyRec, conn, transaction);

                    FGLItemCodeTranHisDao fGLItemCodeTranHisDao = new FGLItemCodeTranHisDao();


                    foreach (OGL00002Model d in dataList)
                    {
                        FGL_ITEM_CODE_TRAN_HIS his = new FGL_ITEM_CODE_TRAN_HIS();

                        var temp = d.tempId.Split('|');
                        var productNo = temp[0].TrimEnd();
                        his = fGLItemCodeTranHisDao.qryByKey("", productNo);
                        if(his == null)
                            return Json(new { success = false, err = "保險商品編號:" + productNo + "已不存在暫存檔，將不進行修改覆核作業!!" }, JsonRequestBehavior.AllowGet);


                        his.aply_no = aplyNo;
                        his.update_id = Session["UserID"].ToString();
                        his.update_datetime = DateTime.Now;
                        his.appr_stat = "1";

                        //將歷史檔壓上覆核單號
                        fGLItemCodeTranHisDao.updateAplyNo(his, conn, transaction);

                        //將正式檔狀態異動為"凍結"
                        if ("D".Equals(his.exec_action) || "U".Equals(his.exec_action))
                        {
                            FGL_ITEM_CODE_TRAN formal = new FGL_ITEM_CODE_TRAN();

                            formal.product_no = productNo;
                            formal.data_status = "2";

                            formal.update_id = Session["UserID"].ToString();
                            formal.update_datetime = DateTime.Now;
                            formal.appr_id = null;
                            formal.approve_datetime = null;

                            fGLItemCodeTranDao.updateStatus("2", formal, conn, transaction);

                        }
                    }


                    transaction.Commit();

                    return Json(new { success = true, aplyNo = aplyNo });
                }
                catch (Exception e)
                {
                    transaction.Rollback();
                    logger.Error("[updateUser]其它錯誤：" + e.ToString());

                    return Json(new { success = false, err = "其它錯誤，請洽系統管理員!!" }, JsonRequestBehavior.AllowGet);
                }
            }

            /*------------------ DB處理   end------------------*/

        }



        /// <summary>
        /// 依畫面上的查詢條件刪除暫存檔資料
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult delTmp(OGL00002Model model)
        {
            /*------------------ DB處理   begin------------------*/
            string strConn = DbUtil.GetDBFglConnStr();
            using (SqlConnection conn = new SqlConnection(strConn))
            {
                conn.Open();

                SqlTransaction transaction = conn.BeginTransaction("Transaction");
                try
                {
                    FGLItemCodeTranHisDao fGLItemCodeTranHisDao = new FGLItemCodeTranHisDao();

                    fGLItemCodeTranHisDao.delTmpFor00002(model, conn, transaction);

                    transaction.Commit();
                    return Json(new { success = true });
                }
                catch (Exception e)
                {
                    transaction.Rollback();
                    logger.Error("其它錯誤：" + e.ToString());

                    return Json(new { success = false, err = "其它錯誤，請洽系統管理員!!" }, JsonRequestBehavior.AllowGet);
                }
            }

            /*------------------ DB處理   end------------------*/
        }



        /// <summary>
        /// 查詢歷史資料
        /// </summary>
        /// <returns></returns>
        public ActionResult aplyHis()
        {

            UserAuthUtil authUtil = new UserAuthUtil();
            string opScope = "";
            string funcName = "";

            if (Session["UserID"] != null)
            {
                string[] roleInfo = authUtil.chkUserFuncAuth(Session["UserID"].ToString(), "~/OGL00002/");
                if (roleInfo != null && roleInfo.Length == 2)
                {
                    opScope = roleInfo[0];
                    funcName = roleInfo[1];
                }
            }

            ViewBag.funcName = funcName;
            ViewBag.opScope = opScope;



            SysCodeDao sysCodeDao = new SysCodeDao();

            //覆核狀態
            var apprStatList = sysCodeDao.loadSelectList("GL", "APPR_STAT", true);
            ViewBag.apprStatList = apprStatList;


            return View();
        }

        public JsonResult qryApprHis(OGL00002Model model, string apprDateB, string apprDateE, string apprStat)
        {

            List<OGL00002Model> rows = new List<OGL00002Model>();

            List<OGL00002Model> dataList = new List<OGL00002Model>();
            FGLItemCodeTranHisDao fGLItemCodeTranHisDao = new FGLItemCodeTranHisDao();
            rows = fGLItemCodeTranHisDao.qryApprHis(model, apprDateB, apprDateE, apprStat);


            //將資料庫內的一筆資料拆成兩筆，呈現於畫面
            foreach (OGL00002Model d in rows)
            {
                string productNo = d.productNo.PadRight(27, ' ');

                OGL00002Model type1 = new OGL00002Model();
                type1.aplyNo = d.aplyNo;
                type1.tempId = d.tempId;
                //  type1.rowType = "1";
                type1.p01 = productNo.Substring(0, 1);
                type1.p02 = productNo.Substring(1, 1);
                type1.p03 = productNo.Substring(2, 1);
                type1.p04 = productNo.Substring(3, 1);
                type1.p05 = productNo.Substring(4, 1);
                type1.p06 = productNo.Substring(5, 1);
                type1.p07 = productNo.Substring(6, 1);
                type1.p08 = productNo.Substring(7, 1);
                type1.p09 = productNo.Substring(8, 1);
                type1.p10 = productNo.Substring(9, 1);
                type1.p11 = productNo.Substring(10, 1);
                type1.p12 = productNo.Substring(11, 1);
                type1.p13 = productNo.Substring(12, 1);
                type1.p14 = productNo.Substring(13, 1);
                type1.p15 = productNo.Substring(14, 1);
                type1.p16 = productNo.Substring(15, 1);
                type1.p17 = productNo.Substring(16, 1);
                type1.p18 = productNo.Substring(17, 1);
                type1.p19 = productNo.Substring(18, 1);
                type1.p20 = productNo.Substring(19, 1);
                type1.p21 = productNo.Substring(20, 1);
                type1.p22 = productNo.Substring(21, 1);
                type1.p23 = productNo.Substring(22, 1);
                type1.p24 = productNo.Substring(23, 1);
                type1.p25 = productNo.Substring(24, 1);
                type1.p26 = productNo.Substring(25, 1);
                type1.p27 = productNo.Substring(26, 1);

                type1.tranA = d.tranA;
                type1.tranB = d.tranB;
                type1.tranC = d.tranC;
                type1.tranD = d.tranD;
                type1.tranE = d.tranE;
                type1.tranF = d.tranF;
                type1.tranG = d.tranG;
                type1.tranH = d.tranH;
                type1.tranI = d.tranI;
                type1.tranJ = d.tranJ;
                type1.tranK = d.tranK;

                type1.apprStat = d.apprStat;
                type1.dataStatus = d.dataStatus;
                type1.dataStatusDesc = d.dataStatusDesc;
                type1.execAction = d.execAction;
                type1.execActionDesc = d.execActionDesc;

                type1.updateId = d.updateId;
                type1.updateDatetime = d.updateDatetime;
                type1.apprId = d.apprId;
                type1.apprDt = d.apprDt;

                dataList.Add(type1);
            }

            using (DB_INTRAEntities dbIntra = new DB_INTRAEntities())
            {
                Dictionary<string, string> userNameMap = new Dictionary<string, string>();
                OaEmpDao oaEmpDao = new OaEmpDao();
                string uId = "";
                string apprId = "";

                foreach (OGL00002Model d in dataList)
                {
                    uId = StringUtil.toString(d.updateId);
                    if (!"".Equals(uId))
                    {
                        if (!userNameMap.ContainsKey(uId))
                        {
                            userNameMap = oaEmpDao.qryUsrName(userNameMap, uId, dbIntra);
                        }
                        d.updateId = userNameMap[uId] == "" ? d.updateId : userNameMap[uId];
                    }

                    apprId = StringUtil.toString(d.apprId);
                    if (!"".Equals(apprId))
                    {
                        if (!userNameMap.ContainsKey(apprId))
                        {
                            userNameMap = oaEmpDao.qryUsrName(userNameMap, apprId, dbIntra);
                        }
                        d.apprId = userNameMap[apprId] == "" ? d.apprId : userNameMap[apprId];
                    }
                }
            }
            var jsonData = new { success = true, rows = dataList};
            return Json(jsonData, JsonRequestBehavior.AllowGet);
        }
    }
}