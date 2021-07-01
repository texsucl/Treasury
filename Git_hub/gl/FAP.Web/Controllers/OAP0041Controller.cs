using ClosedXML.Excel;
using FAP.Web.ActionFilter;
using FAP.Web.BO;
using FAP.Web.Daos;
using FAP.Web.Models;
using FAP.Web.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.EasycomClient;
using System.Data.SqlClient;
using System.DirectoryServices;
using System.IO;
using System.Transactions;
using System.Web.Mvc;
using System.Linq;
using System.Data.Common;

/// <summary>
/// 功能說明：OAP0041 電訪派件設定作業
/// 初版作者：20200709 Daiyu
/// 修改歷程：20200709 Daiyu
///           需求單號：
///           初版
/// </summary>
///

namespace FAP.Web.Controllers
{
    [Authorize]
    [CheckSessionFilterAttribute]
    public class OAP0041Controller : BaseController
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
                String[] roleInfo = authUtil.chkUserFuncAuth(Session["UserID"].ToString(), "~/OAP0041/");
                if (roleInfo != null && roleInfo.Length == 2)
                {
                    opScope = roleInfo[0];
                    funcName = roleInfo[1];
                }
            }

            ViewBag.opScope = opScope;
            ViewBag.funcName = funcName;

            

            SysCodeDao sysCodeDao = new SysCodeDao();
            ViewBag.codeTypeList = sysCodeDao.loadSelectList("AP", "TEL_CODE_TYPE", false);


            //執行功能
            ViewBag.execActionjqList = sysCodeDao.jqGridList("AP", "EXEC_ACTION", true);

            //資料狀態
            ViewBag.statusjqList = sysCodeDao.jqGridList("AP", "DATA_STATUS", true);

            return View();
        }


        /// <summary>
        /// 查詢"FAP_VE_CODE 逾期未兌領代碼設定檔"資料
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public JsonResult qryVeCode(string code_type)
        {
            logger.Info("qryVeCode begin!!");
            try
            {
                FAPVeCodeDao fAPVeCodeDao = new FAPVeCodeDao();
                List<FAP_VE_CODE> rows = fAPVeCodeDao.qryByGrp(code_type);


                if ("TEL_DISPATCH".Equals(code_type) || "SMS_DISPATCH".Equals(code_type))
                {
                    List<OAP0041Model> rowsDispatch = new List<OAP0041Model> ();

                    foreach (FAP_VE_CODE d in rows) {

                        OAP0041Model oAP0041Model = new OAP0041Model();
                        ObjectUtil.CopyPropertiesTo(d, oAP0041Model);

                        ADModel adModel = new ADModel();
                        CommonUtil commonUtil = new CommonUtil();
                        adModel = commonUtil.qryEmp(d.code_id);
                        oAP0041Model.e_mail = adModel.e_mail;
                        oAP0041Model.name = adModel.name;
                        rowsDispatch.Add(oAP0041Model);
                    }

                    var jsonData = new { success = true, dataList = rowsDispatch };
                    return Json(jsonData, JsonRequestBehavior.AllowGet);

                } else {
                    string bInAply = "";
                    if ("TEL_RANGE".Equals(code_type)) {
                        errModel errModel = chkAplyData(code_type, "", "", "");
                        bInAply = errModel.chkResult == false ? "Y" : "";
                    }
                    

                    var jsonData = new { success = true, dataList = rows, bInAply = bInAply };
                    return Json(jsonData, JsonRequestBehavior.AllowGet);

                }


                
            }
            catch (Exception e) {
                return Json(new { success = false, err = "其它錯誤，請洽系統管理員!!" });
            }
            
        }


        /// <summary>
        /// 畫面執行"申請覆核"
        /// </summary>
        /// <param name="code_type"></param>
        /// <param name="gridData"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult execSave(string code_type, List<VeTraceModel> gridData)
        {
            logger.Info("execSave begin");

            string errStr = "";
            try
            {
                bool bChg = false;
                List<VeTraceModel> dataList = new List<VeTraceModel>();

                if ("TEL_RANGE".Equals(code_type))
                {
                    foreach (VeTraceModel d in gridData)
                    {
                        bChg = true;
                        d.code_type = code_type;
                        d.exec_action = "A";
                        d.appr_stat = "1";
                        d.update_id = Session["UserID"].ToString();
                        dataList.Add(d);
                    }
                }
                else
                {
                    foreach (VeTraceModel d in gridData)
                    {
                        d.code_type = code_type;
                        if (!"".Equals(StringUtil.toString(d.exec_action)))
                        {

                            errModel errModel = chkAplyData(d.code_type, d.exec_action, d.code_id, d.code_value);
                            if (errModel.chkResult)
                            {
                                bChg = true;
                                d.code_type = code_type;
                                d.appr_stat = "1";
                                d.update_id = Session["UserID"].ToString();
                                dataList.Add(d);
                            }
                            else
                            {
                                errStr += "代碼：" + d.code_id + " 中文說明：" + d.code_value + " 錯誤原因：" + errModel.msg + "<br/>";
                            }
                        }
                    }
                }
                


                if (bChg == false)
                {
                    if ("".Equals(errStr))
                        return Json(new { success = false, err = "未異動畫面資料，將不進行修改覆核作業!!" }, JsonRequestBehavior.AllowGet);
                    else
                        return Json(new { success = true, err = errStr });
                }


                /*------------------ DB處理   begin------------------*/
                

                string[] curDateTime = DateUtil.getCurChtDateTime(3).Split(' ');
                //取得流水號
                SysSeqDao sysSeqDao = new SysSeqDao();
                String qPreCode = "0041" + curDateTime[0].Substring(0, 5);
                var cId = sysSeqDao.qrySeqNo("AP", "0041", qPreCode).ToString();
                string aply_no = qPreCode + cId.ToString().PadLeft(3, '0');

                string strConn = DbUtil.GetDBFglConnStr();
                using (SqlConnection conn = new SqlConnection(strConn))
                {
                    conn.Open();

                    SqlTransaction transaction = conn.BeginTransaction("Transaction");
                    try
                    {
                        DateTime dt = DateTime.Now;

                        //新增覆核資料至【FAP_VE_CODE_HIS 逾期未兌領代碼設定暫存檔】
                        FAPVeCodeHisDao fAPVeCodeHisDao = new FAPVeCodeHisDao();
                        fAPVeCodeHisDao.insert(aply_no, dt,  dataList, conn, transaction);

                        //將已存在【FAP_VE_CODE 逾期未兌領代碼設定檔】的資料設為"凍結中"
                        FAPVeCodeDao fAPVeCodeDao = new FAPVeCodeDao();

                        if ("TEL_RANGE".Equals(code_type))
                            fAPVeCodeDao.updByType(code_type, "1", dataList[0], dt, conn, transaction);
                        else {
                            foreach (VeTraceModel d in dataList)
                            {
                                if (!"A".Equals(d.exec_action))
                                    fAPVeCodeDao.updateStatus("1", dt, d, conn, transaction);
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

                return Json(new { success = true, aply_no = aply_no, err = errStr });

            }
            catch (Exception e)
            {
                logger.Error(e.ToString());
                return Json(new { success = false, err = "其它錯誤，請洽系統管理員!!" });

            }
        }



        /// <summary>
        /// 畫面GRID在儲存前，需先檢查可存檔
        /// </summary>
        /// <param name="code_type"></param>
        /// <param name="exec_action"></param>
        /// <param name="code_id"></param>
        /// <param name="code_value"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult chkData(string code_type, string exec_action, string code_id, string code_value)
        {
            errModel errModel = new errModel();
            errModel = chkAplyData(code_type, exec_action, code_id, code_value);

            if (errModel.chkResult)
                return Json(new { success = true });
            else
                return Json(new { success = false, err = errModel.msg });

        }



        /// <summary>
        /// 檢核申請的資料是否正確
        /// 1.是否有覆核中的資料
        /// 2.新增的資料是否已存在
        /// </summary>
        /// <param name="code_type"></param>
        /// <param name="exec_action"></param>
        /// <param name="code_id"></param>
        /// <param name="code_value"></param>
        /// <returns></returns>
        private errModel chkAplyData(string code_type, string exec_action, string code_id, string code_value)
        {
            errModel errModel = new errModel();

            if ("A".Equals(exec_action) && !"TEL_RANGE".Equals(code_type))
            {
                FAPVeCodeDao fAPVeCodeDao = new FAPVeCodeDao();
                FAP_VE_CODE formal = fAPVeCodeDao.qryByKey(code_type, code_id);

                if (!"".Equals(StringUtil.toString(formal.code_id))) {
                    errModel.chkResult = false;
                    errModel.msg = "此筆資料已存在，不可新增!!";
                    return errModel;
                }
            }


            FAPVeCodeHisDao fAPVeCodeHisDao = new FAPVeCodeHisDao();
            FAP_VE_CODE_HIS his = fAPVeCodeHisDao.qryInProssById(code_type, code_id, "");
            if (!"".Equals(StringUtil.toString(his.code_id))) {
                errModel.chkResult = false;
                errModel.msg = "資料覆核中，不可異動此筆資料!!";
                return errModel;
            }

            errModel.chkResult = true;
            errModel.msg = "";
            return errModel;
        }



        [HttpPost]
        public JsonResult execExport(string code_type)
        {
            logger.Info("execExport begin!!");
            try
            {
                SysCodeDao sysCodeDao = new SysCodeDao();
                Dictionary<string, string> grpMap = sysCodeDao.qryByTypeDic("AP", "TEL_CODE_TYPE");

                FAPVeCodeDao fAPVeCodeDao = new FAPVeCodeDao();
                List<FAP_VE_CODE> rows = fAPVeCodeDao.qryByGrp(code_type);


                string guid = "";
                if (rows.Count > 0)
                {
                    guid = Guid.NewGuid().ToString();

                    string fullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory + "Temp\\"
                            , string.Concat("OAP0041" + "_" + guid, ".xlsx"));
                    using (XLWorkbook wb = new XLWorkbook())
                    {
                        int iCol = 2;
                        var ws = wb.Worksheets.Add(code_type);

                        ws.Cell(1, 1).Value = grpMap[code_type];

                        switch (code_type) {
                            case "TEL_RANGE":
                                iCol = 2;

                                ws.Cell(2, 1).Value = "下限(含)";
                                ws.Cell(2, 2).Value = "上限";
                                break;
                            case "TEL_DISPATCH":
                                iCol = 4;

                                ws.Cell(2, 1).Value = "電訪人員";
                                ws.Cell(2, 2).Value = "每月最高派件量";
                                ws.Cell(2, 3).Value = "電訪人員姓名";
                                ws.Cell(2, 4).Value = "E_MAIL";
                                break;
                            case "SMS_DISPATCH":
                                iCol = 4;

                                ws.Cell(2, 1).Value = "簡訊人員";
                                ws.Cell(2, 2).Value = "每月最高派件量";
                                ws.Cell(2, 3).Value = "簡訊人員姓名";
                                ws.Cell(2, 4).Value = "E_MAIL";
                                break;
                            case "TEL_SMS":
                                iCol = 2;
                                ws.Cell(2, 1).Value = "簡訊類別";
                                ws.Cell(2, 2).Value = "簡訊內容";

                                break;
                        }
                       

                        int iRow = 2;
                        
                        CommonUtil commonUtil = new CommonUtil();
                        foreach (FAP_VE_CODE d in rows)
                        {
                            iRow++;

                            if ("TEL_SMS".Equals(code_type))
                            {
                                ws.Cell(iRow, 1).Value = d.code_value;
                                ws.Cell(iRow, 2).Value = d.remark;
                            }
                            else {
                                ws.Cell(iRow, 1).Value = d.code_id;
                                ws.Cell(iRow, 2).Value = d.code_value;
                            }
                            
                            if ("TEL_DISPATCH".Equals(code_type) || "SMS_DISPATCH".Equals(code_type))
                            {
                                
                                ADModel adModel = new ADModel();
                                adModel = commonUtil.qryEmp(d.code_id);
                                ws.Cell(iRow, 3).Value = adModel.name;
                                ws.Cell(iRow, 4).Value = adModel.e_mail;
                            }
                         }

                        ws.Range(1, 1, 1, iCol).Merge();

                        ws.Range(1, 1, 1, iCol).Style.Fill.BackgroundColor = XLColor.AirForceBlue;
                        ws.Range(1, 1, 1, iCol).Style.Font.FontColor = XLColor.White;

                        ws.Range(1, 1, iRow, iCol).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        ws.Range(1, 1, iRow, iCol).Style.Border.OutsideBorder = XLBorderStyleValues.Thick;
                        ws.Range(1, 1, iRow, iCol).Style.Border.InsideBorder = XLBorderStyleValues.Thin;

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


        public FileContentResult downloadRpt(String id)
        {
            byte[] fileBytes = System.IO.File.ReadAllBytes(Server.MapPath("~/Temp/") + "OAP0041" + "_" + id + ".xlsx");


            string fullPath = Server.MapPath("~/Temp/") + "OAP0041" + "_" + id + ".xlsx";
            if (System.IO.File.Exists(fullPath))
                System.IO.File.Delete(fullPath);


            return File(fileBytes, "application/vnd.ms-excel", "OAP0041.xlsx");
        }


        /// <summary>
        /// 查詢使用者資料需存在AD
        /// </summary>
        /// <param name="usr_id"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult qryEmp(string usr_id)
        {
            try
            {
                ADModel adModel = new ADModel();
                CommonUtil commonUtil = new CommonUtil();
                adModel = commonUtil.qryEmp(usr_id);

                return Json(new { success = true, name = adModel.name, e_mail = adModel.e_mail }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                logger.Error(e.ToString());
                return Json(new { success = false, err = "其它錯誤，請洽系統管理員!!" }, JsonRequestBehavior.AllowGet);
            }
        }


        internal class errModel
        {
            public bool chkResult { get; set; }
            public string msg { get; set; }
        }
    }
}