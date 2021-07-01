using FGL.Web.ActionFilter;
using FGL.Web.AS400Models;
using FGL.Web.BO;
using FGL.Web.Daos;
using FGL.Web.Models;
using FGL.Web.ViewModels;
using Microsoft.Reporting.WebForms;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.EasycomClient;
using System.Data.SqlClient;
using System.IO;
using System.Web.Mvc;


/// <summary>
/// 功能說明：商品年期及躉繳商品設定作業
/// 初版作者：20191220 Daiyu
/// 修改歷程：20191220 Daiyu
///           需求單號：
///           初版
/// </summary>
///

namespace FGL.Web.Controllers
{
    [Authorize]
    [CheckSessionFilterAttribute]
    public class OGL00009Controller : BaseController
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
                String[] roleInfo = authUtil.chkUserFuncAuth(Session["UserID"].ToString(), "~/OGL00009/");
                if (roleInfo != null && roleInfo.Length == 2)
                {
                    opScope = roleInfo[0];
                    funcName = roleInfo[1];
                }
            }

            ViewBag.opScope = opScope;
            ViewBag.funcName = funcName;

            SysCodeDao sysCodeDao = new SysCodeDao();

            //商品類別
            var itemTermList = sysCodeDao.loadSelectList("GL", "ITEM_TERM", true);
            ViewBag.itemTermList = itemTermList;
            ViewBag.itemTermjsList = sysCodeDao.jqGridList("GL", "ITEM_TERM", true);

            //系統別
            var sysCodeList = sysCodeDao.loadSelectList("GL", "SYS_TYPE", true);
            ViewBag.sysCodeList = sysCodeList;
            ViewBag.sysCodejsList = sysCodeDao.jqGridList("GL", "SYS_TYPE", false);

            //繳費年期類別
            var premYearList = sysCodeDao.loadSelectList("GL", "PREM_YEAR", true);
            ViewBag.premYearList = premYearList;
            ViewBag.premYearjsList = sysCodeDao.jqGridList("GL", "PREM_YEAR", true);

            //執行功能
            ViewBag.execActionjqList = sysCodeDao.jqGridList("GL", "EXEC_ACTION", true);

            //資料狀態
            ViewBag.dataStatusjqList = sysCodeDao.jqGridList("GL", "DATA_STATUS", true);

       

            return View();
        }


        /// <summary>
        /// 查詢
        /// </summary>
        /// <param name="item_type"></param>
        /// <param name="item"></param>
        /// <param name="sys_type"></param>
        /// <param name="prem_y_tp"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult qryFGLGITM(string item_type, string item, string sys_type, string prem_y_tp)
        {
            logger.Info("qryFGLGITM begin!!");

            try
            {
                FGLGITM0Dao fGLGITM0Dao = new FGLGITM0Dao();
                FGLGitmHisDao fGLGitmHisDao = new FGLGitmHisDao();

                List<FGLGITM0Model> rows = fGLGITM0Dao.qryOGL00009(item_type, item, sys_type, prem_y_tp);
                List<OGL00009Model> dataList = new List<OGL00009Model>();

                foreach (FGLGITM0Model d in rows)
                {
                    try
                    {
                        OGL00009Model data = new OGL00009Model();
                        
                        data.item = d.item;
                        data.sys_type = d.sys_type;
                        data.year = d.item_type == "1" ? d.year : "";
                        data.item_type = d.item_type;
                        data.prem_y_tp = d.prem_y_tp;
                        data.age = d.prem_y_tp == "1" ? d.age : "";

                        data.item_n = data.item;
                        data.sys_type_n = data.sys_type;
                        data.item_type_n = data.item_type;

                        data.update_id = d.upd_id;
                        data.update_datetime = d.upd_date == "0" ? "" : DateUtil.As400ChtDateToADDate(d.upd_date);

                        data.tempId = data.item_type + data.sys_type + data.item;

                        dataList.Add(data);
                    }
                    catch (Exception e) {

                    }
                }


                //查詢在覆核中，且為"新增"的資料，查詢時就要看見
                List<OGL00009Model> hisStatusA = fGLGitmHisDao.qryForOGL00009("", "1", item_type, item, sys_type, "A");
                if (hisStatusA != null)
                {
                    foreach (OGL00009Model hisA in hisStatusA)
                    {
                        hisA.tempId = hisA.item_type + hisA.sys_type + hisA.item;
                        hisA.data_status = "2";
                        dataList.Add(hisA);
                    }
                }


                //查DB_INTRA取得異動人員姓名
                using (DB_INTRAEntities dbIntra = new DB_INTRAEntities())
                {
                    Dictionary<string, string> userNameMap = new Dictionary<string, string>();
                    OaEmpDao oaEmpDao = new OaEmpDao();
                    string update_id = "";

                    foreach (OGL00009Model d in dataList)
                    {
                        update_id = StringUtil.toString(d.update_id);

                        if (!"".Equals(update_id))
                        {
                            if (!userNameMap.ContainsKey(update_id))
                                userNameMap = oaEmpDao.qryUsrName(userNameMap, update_id, dbIntra);

                            d.update_id = userNameMap[update_id];
                        }
                    }
                }


                //檢查是否有待覆核的資料
                foreach (OGL00009Model d in dataList)
                {
                    if ("".Equals(StringUtil.toString(d.data_status))) {
                        List<OGL00009Model> his = fGLGitmHisDao.qryForOGL00009("", "1", d.item_type, d.sys_type, d.item, "");

                        if (his.Count > 0)
                            d.data_status = "2";
                        else
                            d.data_status = "1";
                    }
                }


                var jsonData = new { success = true, dataList = dataList };
                return Json(jsonData, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                logger.Error(e.ToString());
                return Json(new { success = false, err = "其它錯誤，請洽系統管理員!!" }, JsonRequestBehavior.AllowGet);
            }
        }


        /// <summary>
        /// 執行"申請覆核"
        /// </summary>
        /// <param name="gridData"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult execSave(List<OGL00009Model> gridData)
        {
            logger.Info("execSave begin");

            string errStr = "";
            bool bChg = false;
            string item_type = "";
            List<OGL00009Model> rptList = new List<OGL00009Model>();
             
            try {

                SysCodeDao sysCodeDao = new SysCodeDao();
                //系統別
                Dictionary<string, string> sysTypeMap = sysCodeDao.qryByTypeDic("GL", "SYS_TYPE_2");

                //繳費年期類別
                Dictionary<string, string> premYearMap = sysCodeDao.qryByTypeDic("GL", "PREM_YEAR");

                //執行功能
                Dictionary<string, string> execActionMap = sysCodeDao.qryByTypeDic("GL", "EXEC_ACTION");

                List<FGL_GITM_HIS> dataList = new List<FGL_GITM_HIS>();

                if (gridData == null)
                {
                    return Json(new { success = false, err = "未異動畫面資料，將不進行修改覆核作業!!" }, JsonRequestBehavior.AllowGet);
                }


                foreach (OGL00009Model d in gridData)
                {

                    if (!"".Equals(StringUtil.toString(d.exec_action)))
                    {
                        ErrorModel errModel = chkAplyData(d.tempId, d.item_type, d.sys_type, d.item, d.item_type_n, d.sys_type_n, d.item_n, d.exec_action);

                        if (errModel.chkResult)
                        {
                            bChg = true;
                            d.update_id = Session["UserID"].ToString();
                            d.update_datetime = DateUtil.getCurDateTime("yyyy/MM/dd HH:mm:ss");
                            d.appr_datetime = null;

                            if ("2".Equals(d.item_type_n) || "3".Equals(d.item_type_n))
                                d.year = null;


                            if (!"1".Equals(d.prem_y_tp))
                                d.age = null;

                            FGL_GITM_HIS his = new FGL_GITM_HIS();
                            ObjectUtil.CopyPropertiesTo(d, his);
                            his.appr_stat = "1";
                            his.sys_type = his.sys_type == "3" ? "" : his.sys_type;
                            his.sys_type_n = his.sys_type_n == "3" ? "" : his.sys_type_n;


                            item_type = d.item_type_n;
                            dataList.Add(his);

                            OGL00009Model data = new OGL00009Model();

                            data.item = d.item_n;

                            switch (d.sys_type_n)
                            {
                                case "1":
                                    d.sys_type = "A";
                                    break;
                                case "2":
                                    d.sys_type = "F";
                                    break;
                                case "3":
                                    d.sys_type = "";
                                    break;
                            }

                            data.year = d.item_type_n == "1" ? d.year : "";
                            data.item_type = d.item_type_n;
                            data.prem_y_tp = d.prem_y_tp;
                            data.age = d.prem_y_tp == "1" ? d.age : "";

                            if (sysTypeMap.ContainsKey(StringUtil.toString(d.sys_type)))
                                data.sys_type = sysTypeMap[d.sys_type];

                            if (premYearMap.ContainsKey(StringUtil.toString(d.prem_y_tp)))
                                data.prem_y_tp = d.prem_y_tp + "." + premYearMap[d.prem_y_tp];

                            if (execActionMap.ContainsKey(StringUtil.toString(d.exec_action)))
                                data.exec_action = execActionMap[d.exec_action];


                            rptList.Add(data);

                        }
                        else
                        {
                            errStr += "商品類別：" + d.item_type_n + " 險種代號：" + d.item_n + " 系統別：" + d.sys_type_n + "<br/>";
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
                FGLGitmHisDao fGLGitmHisDao = new FGLGitmHisDao();

                string[] curDateTime = DateUtil.getCurChtDateTime(4).Split(' ');
                //取得流水號
                SysSeqDao sysSeqDao = new SysSeqDao();
                String qPreCode = curDateTime[0];
                var cId = sysSeqDao.qrySeqNo("GL", "00009", qPreCode).ToString();


                fGLGitmHisDao.insert(qPreCode + cId.ToString().PadLeft(3, '0'), dataList);

                string guid = genRpt(item_type, rptList);



                return Json(new { success = true, aplyNo = qPreCode + cId.ToString().PadLeft(3, '0'), err = errStr, guid = guid });

            } catch (Exception e) {
                logger.Error(e.ToString());
                return Json(new { success = false, err = "其它錯誤，請洽系統管理員!!" }, JsonRequestBehavior.AllowGet);
            }



        }



        /// <summary>
        /// 檢查要異動的資料是否可以異動
        /// </summary>
        /// <param name="iTempId"></param>
        /// <param name="item_type"></param>
        /// <param name="sys_type"></param>
        /// <param name="item"></param>
        /// <param name="item_type_n"></param>
        /// <param name="sys_type_n"></param>
        /// <param name="item_n"></param>
        /// <param name="exec_action"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult chkData(string iTempId, string item_type, string sys_type, string item
            , string item_type_n, string sys_type_n, string item_n, string exec_action)
        {
            ErrorModel errModel = new ErrorModel();
            errModel = chkAplyData(iTempId, item_type, sys_type, item, item_type_n, sys_type_n, item_n, exec_action);

            if (errModel.chkResult)
                return Json(new { success = true });
            else
                return Json(new { success = false, err = errModel.msg });
        }


        /// <summary>
        /// 檢查資料是否已存在主檔或暫存檔
        /// </summary>
        /// <param name="iTempId"></param>
        /// <param name="item_type"></param>
        /// <param name="sys_type"></param>
        /// <param name="item"></param>
        /// <param name="exec_action"></param>
        /// <returns></returns>
        private ErrorModel chkAplyData(string iTempId, string item_type, string sys_type, string item
            , string item_type_n, string sys_type_n, string item_n, string exec_action)
        {

            string tempIdN = StringUtil.toString(item_type) + StringUtil.toString(sys_type) + StringUtil.toString(item);
            ErrorModel errModel = new ErrorModel();

            if ("A".Equals(exec_action) ||
                !StringUtil.toString(iTempId).Equals(tempIdN))
            {
                FGLGITM0Dao fGLGITM0Dao = new FGLGITM0Dao();

                List<FGLGITM0Model> rows = fGLGITM0Dao.qryOGL00009(item_type_n, item_n, sys_type_n, "");

                if (rows.Count > 0)
                {
                    errModel.chkResult = false;
                    errModel.msg = "此筆資料已存在不可新增!!";
                    return errModel;
                }
            }

 

            FGLGitmHisDao fGLGitmHisDao = new FGLGitmHisDao();
            List<OGL00009Model> aplyData = fGLGitmHisDao.qryForOGL00009("", "1", item_type, sys_type, item, "");


            if (aplyData.Count > 0)
            {
                errModel.chkResult = false;
                errModel.msg = "資料覆核中，不可異動此筆資料!!";
                return errModel;
            }


            errModel.chkResult = true;
            errModel.msg = "";
            return errModel;
        }

        /// <summary>
        /// 列印報表
        /// </summary>
        /// <param name="item_type"></param>
        /// <param name="item"></param>
        /// <param name="sys_type"></param>
        /// <param name="prem_y_tp"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Print(string item_type, string item, string sys_type, string prem_y_tp)
        {
            logger.Info("Print begin!!");

            try
            {
                SysCodeDao sysCodeDao = new SysCodeDao();
                ////商品類別
                //Dictionary<string, string> itemTypeMap = sysCodeDao.qryByTypeDic("GL", "ITEM_TERM");

                //系統別
                Dictionary<string, string> sysTypeMap = sysCodeDao.qryByTypeDic("GL", "SYS_TYPE");

                //繳費年期類別
                Dictionary<string, string> premYearMap = sysCodeDao.qryByTypeDic("GL", "PREM_YEAR");


                //var rptItemType = "";
                //if (itemTypeMap.ContainsKey(StringUtil.toString(item_type)))
                //    rptItemType = itemTypeMap[item_type];


                FGLGITM0Dao fGLGITM0Dao = new FGLGITM0Dao();

                List<FGLGITM0Model> rows = fGLGITM0Dao.qryOGL00009(item_type, item, sys_type, prem_y_tp);
                List<OGL00009Model> dataList = new List<OGL00009Model>();
                int rptCnt = rows.Count;


                foreach (FGLGITM0Model d in rows)
                {
                    try
                    {
                        OGL00009Model data = new OGL00009Model();

                        data.item = d.item;
                        data.sys_type = d.sys_type;
                        data.year = d.item_type == "1" ? d.year : "";
                        data.item_type = d.item_type;
                        data.prem_y_tp = d.prem_y_tp;
                        data.age = d.prem_y_tp == "1" ? d.age : "";

                        data.item_n = data.item;
                        data.sys_type_n = data.sys_type;
                        data.item_type_n = data.item_type;

                        data.update_id = d.upd_id;
                        data.update_datetime = d.upd_date == "0" ? "" : DateUtil.As400ChtDateToADDate(d.upd_date);

                        //if (itemTypeMap.ContainsKey(StringUtil.toString(d.item_type)))
                        //    d.item_type = itemTypeMap[item_type];

                        if (sysTypeMap.ContainsKey(StringUtil.toString(d.sys_type)))
                            data.sys_type = sysTypeMap[d.sys_type];

                        if (premYearMap.ContainsKey(StringUtil.toString(d.prem_y_tp)))
                            data.prem_y_tp = d.prem_y_tp + "." + premYearMap[d.prem_y_tp];

                        dataList.Add(data);
                    }
                    catch (Exception e)
                    {

                    }
                }


                string guid = genRpt(item_type, dataList);
                
                var jsonData = new { success = true, guid = guid };
                return Json(jsonData, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new { success = false, err = "其它錯誤，請洽系統管理員!!" }, JsonRequestBehavior.AllowGet);
            }

        }



        public string genRpt(string item_type, List<OGL00009Model> dataList)
        {
            logger.Info("Print begin!!");

            try
            {
                SysCodeDao sysCodeDao = new SysCodeDao();

                //商品類別
                Dictionary<string, string> itemTypeMap = sysCodeDao.qryByTypeDic("GL", "ITEM_TERM");


                var rptItemType = "";
                if (itemTypeMap.ContainsKey(StringUtil.toString(item_type)))
                    rptItemType = itemTypeMap[item_type];



                CommonUtil commonUtil = new CommonUtil();
                DataTable dtMain = commonUtil.ConvertToDataTable<OGL00009Model>(dataList);

                var showYear = "Y";
                var showPremYType = "Y";

                if (!"1".Equals(item_type))
                    showYear = "N";

                if (!"2".Equals(item_type))
                    showPremYType = "N";

                var ReportViewer1 = new ReportViewer();
                //清除資料來源
                ReportViewer1.LocalReport.DataSources.Clear();
                //指定報表檔路徑   
                ReportViewer1.LocalReport.ReportPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory + "Report\\Rdlc\\OGL00009P.rdlc");
                //設定資料來源
                ReportViewer1.LocalReport.DataSources.Add(new ReportDataSource("DataSet1", dtMain));

                //報表參數
                ReportViewer1.LocalReport.SetParameters(new ReportParameter("Title", "商品年期及躉繳商品報表"));
                ReportViewer1.LocalReport.SetParameters(new ReportParameter("UserId", Session["UserID"].ToString() + Session["UserName"].ToString()));
                //ReportViewer1.LocalReport.SetParameters(new ReportParameter("item_type", item_type));
                ReportViewer1.LocalReport.SetParameters(new ReportParameter("item_type_desc", rptItemType));
                ReportViewer1.LocalReport.SetParameters(new ReportParameter("showYear", showYear));
                ReportViewer1.LocalReport.SetParameters(new ReportParameter("showPremYType", showPremYType));
                ReportViewer1.LocalReport.SetParameters(new ReportParameter("rptCnt", dataList.Count.ToString()));
                string guid = Guid.NewGuid().ToString();


                ReportViewer1.LocalReport.Refresh();

                Microsoft.Reporting.WebForms.Warning[] tWarnings;
                string[] tStreamids;
                string tMimeType;
                string tEncoding;
                string tExtension;
                byte[] tBytes = ReportViewer1.LocalReport.Render("pdf", null, out tMimeType, out tEncoding, out tExtension, out tStreamids, out tWarnings);
                string fileName = "OGL00009P_" + guid + ".pdf";
                using (FileStream fs = new FileStream(Path.Combine(AppDomain.CurrentDomain.BaseDirectory + "Temp\\") + fileName, FileMode.Create))
                {
                    fs.Write(tBytes, 0, tBytes.Length);
                }

                return guid;

            }
            catch (Exception e)
            {
                throw e;
            }

        }

        /// <summary>
        /// 報表下載
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public FileContentResult downloadRpt(String id)
        {
            byte[] fileBytes = System.IO.File.ReadAllBytes(Server.MapPath("~/Temp/") + "OGL00009P" + "_" + id + ".pdf");


            string fullPath = Server.MapPath("~/Temp/") + "OGL00009P" + "_" + id + ".pdf";
            if (System.IO.File.Exists(fullPath))
                System.IO.File.Delete(fullPath);


            return File(fileBytes, "application/pdf", "商品年期及躉繳商品報表.pdf");
        }


        internal class ErrorModel
        {
            public string tempId { get; set; }
            public bool chkResult { get; set; }
            public string msg { get; set; }
        }
    }
}