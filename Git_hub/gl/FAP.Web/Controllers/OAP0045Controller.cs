using ClosedXML.Excel;
using FAP.Web.ActionFilter;
using FAP.Web.BO;
using FAP.Web.Daos;
using FAP.Web.Models;
using FAP.Web.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.DirectoryServices;
using System.IO;
using System.Web.Mvc;

/// <summary>
/// 功能說明：OAP0045 追蹤標準設定作業
/// 初版作者：20200904 Daiyu
/// 修改歷程：20200904 Daiyu
/// 需求單號：202008120153-00
/// 修改內容：初版
/// </summary>
///

namespace FAP.Web.Controllers
{
    [Authorize]
    [CheckSessionFilterAttribute]
    public class OAP0045Controller : BaseController
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
                String[] roleInfo = authUtil.chkUserFuncAuth(Session["UserID"].ToString(), "~/OAP0045/");
                if (roleInfo != null && roleInfo.Length == 2)
                {
                    opScope = roleInfo[0];
                    funcName = roleInfo[1];
                }
            }

            ViewBag.opScope = opScope;
            ViewBag.funcName = funcName;


            
            SysCodeDao sysCodeDao = new SysCodeDao();
            ViewBag.codeTypeList = sysCodeDao.loadSelectList("AP", "OAP0045_TYPE", false);


            //執行功能
            ViewBag.execActionjqList = sysCodeDao.jqGridList("AP", "EXEC_ACTION", true);

            //資料狀態
            ViewBag.statusjqList = sysCodeDao.jqGridList("AP", "DATA_STATUS", true);

            return View();
        }



        
        [HttpPost]
        public JsonResult qryTelCode(string code_type)
        {
            logger.Info("qryTelCode begin!!");
            try
            {
                List<OAP0045Model> dataList = getTelCodeList(code_type);

                var jsonData = new { success = true, dataList = dataList };
                return Json(jsonData, JsonRequestBehavior.AllowGet);



            }
            catch (Exception e) {
                logger.Error(e.ToString());
                return Json(new { success = false, err = "其它錯誤，請洽系統管理員!!" });
            }
            
        }


        private List<OAP0045Model> getTelCodeList(string code_type) {
            CommonUtil commonUtil = new CommonUtil();
            Dictionary<string, ADModel> empMap = new Dictionary<string, ADModel>();

            SysCodeDao sysCodeDao = new SysCodeDao();

            //追蹤標準
            Dictionary<string, string> codeDescMap = sysCodeDao.qryByTypeDic("AP", code_type);


            FAPTelCodeDao fAPTelCodeDao = new FAPTelCodeDao();
            List<FAP_TEL_CODE> rows = fAPTelCodeDao.qryByGrp(code_type);


            List<OAP0045Model> dataList = new List<OAP0045Model>();

            foreach (FAP_TEL_CODE d in rows)
            {

                OAP0045Model oAP0045Model = new OAP0045Model();
                ObjectUtil.CopyPropertiesTo(d, oAP0045Model);

                if (!"".Equals(StringUtil.toString(d.proc_id))) {
                    if (!empMap.ContainsKey(d.proc_id))
                    {
                        ADModel adModel = new ADModel();
                        adModel = commonUtil.qryEmp(d.proc_id);
                        empMap.Add(d.proc_id, adModel);
                    }
                    oAP0045Model.proc_name = empMap[d.proc_id].name;
                }
                

                if (codeDescMap.ContainsKey(oAP0045Model.code_id))
                    oAP0045Model.code_id_desc = codeDescMap[oAP0045Model.code_id];

                dataList.Add(oAP0045Model);
            }

            return dataList;
        }


        /// <summary>
        /// 畫面執行"申請覆核"
        /// </summary>
        /// <param name="code_type"></param>
        /// <param name="gridData"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult execSave(string code_type, List<OAP0045Model> gridData)
        {
            logger.Info("execSave begin");

            string errStr = "";
            try
            {
                bool bChg = false;
                List<FAP_TEL_CODE_HIS> dataList = new List<FAP_TEL_CODE_HIS>();

                foreach (OAP0045Model d in gridData)
                {
                    d.code_type = code_type;
                    if (!"".Equals(StringUtil.toString(d.exec_action)))
                    {

                        errModel errModel = chkAplyData(d.code_type, d.exec_action, d.code_id);
                        if (errModel.chkResult)
                        {
                            bChg = true;
                            FAP_TEL_CODE_HIS his = new FAP_TEL_CODE_HIS();
                            ObjectUtil.CopyPropertiesTo(d, his);
                            his.code_type = code_type;
                            his.appr_stat = "1";
                            his.update_id = Session["UserID"].ToString();
                            dataList.Add(his);
                        }
                        else
                        {
                            errStr += "代碼：" + d.code_id + " 錯誤原因：" + errModel.msg + "<br/>";
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
                String qPreCode = "0045" + curDateTime[0].Substring(0, 5);
                var cId = sysSeqDao.qrySeqNo("AP", "0045", qPreCode).ToString();
                string aply_no = qPreCode + cId.ToString().PadLeft(3, '0');

                string strConn = DbUtil.GetDBFglConnStr();
                using (SqlConnection conn = new SqlConnection(strConn))
                {
                    conn.Open();

                    SqlTransaction transaction = conn.BeginTransaction("Transaction");
                    try
                    {
                        DateTime dt = DateTime.Now;

                        //新增覆核資料至【FAP_TEL_CODE_HIS 電訪標準設定暫存檔】
                        FAPTelCodeHisDao fAPTelCodeHisDao = new FAPTelCodeHisDao();
                        fAPTelCodeHisDao.insert(aply_no, dt,  dataList, conn, transaction);

                        //將已存在【FAP_TEL_CODE 電訪標準設定檔】的資料設為"凍結中"
                        FAPTelCodeDao fAPTelCodeDao = new FAPTelCodeDao();
                        foreach (FAP_TEL_CODE_HIS d in dataList)
                        {
                            FAP_TEL_CODE model = new FAP_TEL_CODE();
                            ObjectUtil.CopyPropertiesTo(d, model);
                            if (!"A".Equals(d.exec_action))
                                fAPTelCodeDao.updateStatus("1", dt, model, conn, transaction);
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
        /// <returns></returns>
        [HttpPost]
        public JsonResult chkData(string code_type, string exec_action, string code_id)
        {
            errModel errModel = new errModel();
            errModel = chkAplyData(code_type, exec_action, code_id);

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
        /// <returns></returns>
        private errModel chkAplyData(string code_type, string exec_action, string code_id)
        {
            errModel errModel = new errModel();

            FAPTelCodeHisDao fAPTelCodeHisDao = new FAPTelCodeHisDao();
            FAP_TEL_CODE_HIS his = fAPTelCodeHisDao.qryByKey(code_type, code_id, "", "1");
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

                FAPTelCodeDao fAPTelCodeDao = new FAPTelCodeDao();
                List<OAP0045Model> rows = getTelCodeList(code_type);


                string guid = "";
                if (rows.Count > 0)
                {
                    guid = Guid.NewGuid().ToString();

                    string fullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory + "Temp\\"
                            , string.Concat("OAP0045" + "_" + guid, ".xlsx"));
                    using (XLWorkbook wb = new XLWorkbook())
                    {
                        var ws = wb.Worksheets.Add(code_type);
                        int iCol = 0;

                        if ("tel_call".Equals(code_type))
                        {
                            iCol = 7;
                            ws.Cell(1, 1).Value = "處理結果代碼";
                            ws.Cell(1, 2).Value = "處理及追蹤結果";
                            ws.Cell(1, 3).Value = "第一次追踨標準";
                            ws.Cell(1, 4).Value = "第二次追踨標準";
                            ws.Cell(1, 5).Value = "第三次追踨標準";
                            ws.Cell(1, 6).Value = "追踨人員帳號";
                            ws.Cell(1, 7).Value = "追踨人員姓名";
                        }
                        else {
                            iCol = 5;
                            ws.Cell(1, 1).Value = "清理階段代碼";
                            ws.Cell(1, 2).Value = "清理階段";
                            ws.Cell(1, 3).Value = "清理標準確(天)";
                            ws.Cell(1, 4).Value = "清理人員帳號";
                            ws.Cell(1, 5).Value = "清理人員姓名";
                        }
                        


                        int iRow = 1;
                        
                        foreach (OAP0045Model d in rows)
                        {
                            iRow++;
                            ws.Cell(iRow, 1).Value = d.code_id;
                            ws.Cell(iRow, 2).Value = d.code_id_desc;
                            ws.Cell(iRow, 3).Value = d.std_1;

                            if ("tel_call".Equals(code_type))
                            {
                                ws.Cell(iRow, 4).Value = d.std_2;
                                ws.Cell(iRow, 5).Value = d.std_3;
                                ws.Cell(iRow, 6).Value = d.proc_id;
                                ws.Cell(iRow, 7).Value = d.proc_name;
                            }
                            else {
                                ws.Cell(iRow, 4).Value = d.proc_id;
                                ws.Cell(iRow, 5).Value = d.proc_name;
                            }
                                
                        }

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
                logger.Error(e.ToString());
                return Json(new { success = false, err = "其它錯誤，請洽系統管理員!!" });
            }
        }


        public FileContentResult downloadRpt(String id)
        {
            byte[] fileBytes = System.IO.File.ReadAllBytes(Server.MapPath("~/Temp/") + "OAP0045" + "_" + id + ".xlsx");


            string fullPath = Server.MapPath("~/Temp/") + "OAP0045" + "_" + id + ".xlsx";
            if (System.IO.File.Exists(fullPath))
                System.IO.File.Delete(fullPath);


            return File(fileBytes, "application/vnd.ms-excel", "OAP0045.xlsx");
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