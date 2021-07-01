using FAP.Web.ActionFilter;
using FAP.Web.BO;
using FAP.Web.Daos;
using FAP.Web.Models;
using FAP.Web.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Web.Mvc;
using System.Linq;
using System.Data;
using System.IO;
using ClosedXML.Excel;

/// <summary>
/// 功能說明：踐行程序明細查詢
/// 初版作者：20190729 Daiyu
/// 修改歷程：20190729 Daiyu
/// 需求單號：
/// 修改內容：初版
/// ------------------------------------------------
/// 修改歷程：20200811 Daiyu
/// 需求單號：
/// 修改內容：增加查詢條件"踐行日期"
/// </summary>
///

namespace FAP.Web.Controllers
{
    [Authorize]
    [CheckSessionFilterAttribute]
    public class OAP0017Controller : BaseController
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
                String[] roleInfo = authUtil.chkUserFuncAuth(Session["UserID"].ToString(), "~/OAP0017/");
                if (roleInfo != null && roleInfo.Length == 2)
                {
                    opScope = roleInfo[0];
                    funcName = roleInfo[1];
                }
            }

            ViewBag.opScope = opScope;
            ViewBag.funcName = funcName;

            OAP0017Model model = new OAP0017Model();

            SysCodeDao sysCodeDao = new SysCodeDao();
            //清理狀態
            ViewBag.clrStatusList = sysCodeDao.loadSelectList("AP", "CLR_STATUS", true);

            

            return View(model);
        }



        [HttpPost]
        public JsonResult qryPractice()
        {
            try
            {
                FAPVeCodeDao fAPVeCodeDao = new FAPVeCodeDao();
                List<FAP_VE_CODE> pracList = fAPVeCodeDao.qryByGrp("CLR_PRACTICE");

                return Json(new { success = true, pracList = pracList }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e) {

                logger.Error(e.ToString());
                return Json(new { success = false, err = "其它錯誤，請洽系統管理員!!" }, JsonRequestBehavior.AllowGet);
            }
        }





        /// <summary>
        /// 查詢
        /// </summary>
        /// <param name="status"></param>
        /// <param name="pracList"></param>
        /// <param name="date_b"></param>
        /// <param name="date_e"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult execExport(string status, List<pracModel> pracList, string date_b, string date_e)
        {
            logger.Info("execExport begin");

            try
            {
                FAPVeCodeDao fAPVeCodeDao = new FAPVeCodeDao();
                //踐行程序
                Dictionary<string, string> practiceMap = fAPVeCodeDao.qryByTypeDic("CLR_PRACTICE");

                //証明文件
                Dictionary<string, string> certDacMap = fAPVeCodeDao.qryByTypeDic("CLR_CERT_DOC");


                List<rptMModel> rptList = new List<rptMModel>();

                using (dbFGLEntities db = new dbFGLEntities())
                {
                    FAPVeTraceDao fAPVeTraceDao = new FAPVeTraceDao();
                    FAPVeTrackProcDao fAPVeTrackProcDao = new FAPVeTrackProcDao();


                    //modify by daiyu 20200811
                    string[] pracEQArr = pracList.Where(x => x.value == "EQ").Select(x => x.id).ToArray();
                    string[] pracNQArr = pracList.Where(x => x.value == "NQ").Select(x => x.id).ToArray();

                    List<OAP0017Model> rows = fAPVeTraceDao.qryForOAP0017Proc(status, db, date_b, date_e);

                    List<OAP0017Model> rptStatRow = rows.GroupBy(o => new { o.paid_id, o.check_acct_short, o.check_no, o.level_1, o.level_2 })
                .Select(group => new OAP0017Model
                {
                    paid_id = group.Key.paid_id,
                    check_acct_short = group.Key.check_acct_short,
                    check_no = group.Key.check_no,
                    level_1 = group.Key.level_1,
                    level_2 = group.Key.level_2
                }).OrderBy(x => x.paid_id).ThenBy(x => x.check_acct_short).ToList<OAP0017Model>();


                   // string strPrac = "";

                    logger.Info("rptStatRow:" + rptStatRow.Count);
                    int i = 0;

                    //逐筆檢查每一張支票的踐行程序
                    foreach (OAP0017Model main in rptStatRow)
                    {
                        i++;
                        if (i % 1000 == 0)
                            logger.Info("i:" + i);

                        List<OAP0017Model> procList = rows.Where(x => x.check_no == main.check_no).OrderBy(x => x.seq).ToList();
                        bool bMatch = true;

                        //比對需符合的踐行程序
                        if (pracEQArr.Count() > 0) {
                            if (procList.Where(x => pracEQArr.Contains(x.practice)).ToList().Count == 0) {
                                bMatch = false;
                                continue;
                            }
                        }

                        //比對需不符合的踐行程序
                        if (pracNQArr.Count() > 0)
                        {
                            if (procList.Where(x => pracNQArr.Contains(x.practice)).ToList().Count > 0)
                            {
                                bMatch = false;
                                continue;
                            }
                        }



                        //foreach (OAP0017Model proc in procList)
                        //    strPrac += StringUtil.toString(proc.practice) + "|";


                       

                       

                        //if (pracList != null)
                        //{
                        //    foreach (pracModel prac in pracList)
                        //    {
                        //        if ("EQ".Equals(prac.value))
                        //        {
                        //            if (!strPrac.Contains(prac.id))
                        //            {
                        //                bMatch = false;
                        //                break;
                        //            }
                        //        }
                        //        else if ("NQ".Equals(prac.value))
                        //        {
                        //            if (strPrac.Contains(prac.id))
                        //            {
                        //                bMatch = false;
                        //                break;
                        //            }
                        //        }
                        //    }
                        //}
                        //else
                        //    bMatch = true;


                        if (bMatch)
                        {
                            List<rptDModel> rptDList = new List<rptDModel>();

                            int seq = 5;
                            string strSeq = "";
                            foreach (OAP0017Model proc in procList)
                            {
                                if ("6".Equals(proc.seq))
                                {
                                    seq++;
                                    strSeq = seq.ToString();
                                }
                                else
                                    strSeq = proc.seq;

                                genRptProcList(rptDList, proc.practice, proc.exec_date, proc.cert_doc, proc.proc_desc, strSeq
                                    , practiceMap, certDacMap);

                            }

                            rptMModel rpt = new rptMModel();

                            FAP_VE_TRACE trace = new FAP_VE_TRACE();
                            trace.paid_id = main.paid_id;
                            trace.check_acct_short = main.check_acct_short;
                            trace.check_no = main.check_no;
                            trace.level_1 = main.level_1;
                            trace.level_2 = main.level_2;

                            rpt.trace = trace;
                            rpt.pracList = rptDList;
                            rptList.Add(rpt);
                        }

                    }
                }


                logger.Info("rptList.count:" + rptList.Count);

                string guid = "";
                if (rptList.Count > 0)
                {
                    guid = Guid.NewGuid().ToString();
                    genRpt(guid, rptList);

                    logger.Info("execExport end");

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

        private List<rptDModel> genRptProcList(List<rptDModel> rptDList, string practice, string exec_date, string cert_doc, string proc_desc, string seq
            , Dictionary<string, string> practiceMap, Dictionary<string, string> certDacMap)
        {
            rptDModel model = new rptDModel();

            try
            {
                if (new ValidateUtil().IsNum(seq))
                    model.seq = StringUtil.transToChtNumber(Convert.ToInt16(seq), true);

                if (exec_date != null)
                {
                    //string strDate = "";
                    //if (Convert.ToInt16(seq) <= 5)
                    //    strDate = exec_date.Value.Year - 1911 + "/" + exec_date.Value.Month + "/" + exec_date.Value.Day;
                    //else
                    //    strDate = exec_date.Value.Year + "/" + exec_date.Value.Month + "/" + exec_date.Value.Day;

                    model.practice = practice;
                    model.cert_doc = cert_doc;
                    model.exec_date = exec_date;
                    model.proc_desc = proc_desc;

                    if (practiceMap.ContainsKey(model.practice))
                        model.practice_desc = practiceMap[model.practice];
                    else
                        model.practice_desc = model.practice;

                    if (certDacMap.ContainsKey(model.cert_doc))
                        model.cert_doc_desc = certDacMap[model.cert_doc];
                    else
                        model.cert_doc_desc = model.cert_doc;

                    rptDList.Add(model);
                }
            }
            catch (Exception e)
            {
                logger.Error(e.ToString());
            }

            return rptDList;
        }


        internal class rptMModel
        {
            public FAP_VE_TRACE trace { get; set; }

            public List<rptDModel> pracList { get; set; }
        }

        internal class rptDModel
        {
            public string seq { get; set; }
            public string practice { get; set; }
            public string exec_date { get; set; }
            public string cert_doc { get; set; }

            public string proc_desc { get; set; }

            public string practice_desc { get; set; }

            public string cert_doc_desc { get; set; }

            public rptDModel()
            {
                seq = "";
                practice = "";
                exec_date = "";
                cert_doc = "";
                proc_desc = "";
                practice_desc = "";
                cert_doc_desc = "";
            }
        }


        public FileContentResult downloadRpt(String id)
        {
            byte[] fileBytes = System.IO.File.ReadAllBytes(Server.MapPath("~/Temp/") + "OAP0017" + "_"+ id + "_" + Session["UserID"].ToString() + ".xlsx");


            string fullPath = Server.MapPath("~/Temp/") + "OAP0017" + "_" + id + "_" + Session["UserID"].ToString() + ".xlsx";
            if (System.IO.File.Exists(fullPath))
                System.IO.File.Delete(fullPath);


            return File(fileBytes, "application/vnd.ms-excel", "踐行程序明細.xlsx");
        }


        private void genRpt(string guid, List<rptMModel> rptMList)
        {
            string fullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory + "Temp\\"
                            , string.Concat("OAP0017" + "_" + guid + "_" + Session["UserID"].ToString(), ".xlsx"));


            FAPVeCodeDao fAPVeCodeDao = new FAPVeCodeDao();
            //清理大類
            Dictionary<string, string> level1Map = fAPVeCodeDao.qryByTypeDic("CLR_LEVEL1");

            //清理小類
            Dictionary<string, string> level2Map = fAPVeCodeDao.qryByTypeDic("CLR_LEVEL2");



            using (XLWorkbook workbook = new XLWorkbook())
            {
                var ws = workbook.Worksheets.Add("OAP0017");

                ws.Cell(1, 1).Value = "給付對象ID";
                ws.Cell(1, 2).Value = "支票號碼";
                ws.Cell(1, 3).Value = "支票帳號簡稱";
                ws.Cell(1, 4).Value = "清理大類";
                ws.Cell(1, 5).Value = "清理小類";
                ws.Cell(1, 6).Value = "踐行程序";
                ws.Cell(1, 7).Value = "証明文件";
                ws.Cell(1, 8).Value = "執行日期";
                ws.Cell(1, 9).Value = "過程說明";
                

                int iRow = 1;

                foreach (rptMModel m in rptMList) {
                    foreach (rptDModel d in m.pracList.OrderBy(x => x.seq)) {
                        if (iRow % 1000 == 0)
                            logger.Info("iRow:" + iRow);


                        iRow++;

                        ws.Cell(iRow, 1).Value = m.trace.paid_id;
                        ws.Cell(iRow, 2).Value = m.trace.check_no;
                        ws.Cell(iRow, 3).Value = m.trace.check_acct_short;

                        if(level1Map.ContainsKey(StringUtil.toString(m.trace.level_1)))
                            ws.Cell(iRow, 4).Value = level1Map[m.trace.level_1];
                        else
                            ws.Cell(iRow, 4).Value = m.trace.level_1;

                        if (level2Map.ContainsKey(StringUtil.toString(m.trace.level_2)))
                            ws.Cell(iRow, 5).Value = level2Map[m.trace.level_2];
                        else
                            ws.Cell(iRow, 5).Value = m.trace.level_2;


                        ws.Cell(iRow, 6).Value = d.practice_desc;
                        ws.Cell(iRow, 7).Value = d.cert_doc_desc;
                        ws.Cell(iRow, 8).Value = d.exec_date;
                        ws.Cell(iRow, 9).Value = "'" + d.proc_desc;
                    }
                }

         
                //ws.Range(1, 1, iRow, 9).Style.Border.OutsideBorder = XLBorderStyleValues.Thick;
                //ws.Range(1, 1, iRow, 9).Style.Border.InsideBorder = XLBorderStyleValues.Thin;



                //ws.Columns().AdjustToContents();  // Adjust column width
                //ws.Rows().AdjustToContents();     // Adjust row heights
                workbook.SaveAs(fullPath);


            }
        }



        public class pracModel
        {
            public string id { get; set; }
            public string value { get; set; }



        }
    }
}