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
using Microsoft.Reporting.WebForms;
using FAP.Web.AS400PGM;

/// <summary>
/// 功能說明：清理結案申請作業
/// 初版作者：20190703 Daiyu
/// 修改歷程：20190703 Daiyu
/// 需求單號：
/// 修改內容：初版
/// -----------------------------------------
/// 修改歷程：20210118 daiyu
/// 需求單號：
/// 修改內容：1.修改覆核中的資料無法列印結案報表問題
///           2.修正"申請覆核"若遇到錯誤時，再次執行結案報表列印，踐行程序重複問題
///           3."結案日期"改為OAP0011的覆核日期，結案編號改取系統年月
/// </summary>
///

namespace FAP.Web.Controllers
{
    [Authorize]
    [CheckSessionFilterAttribute]
    public class OAP0011Controller : BaseController
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
                String[] roleInfo = authUtil.chkUserFuncAuth(Session["UserID"].ToString(), "~/OAP0011/");
                if (roleInfo != null && roleInfo.Length == 2)
                {
                    opScope = roleInfo[0];
                    funcName = roleInfo[1];
                }
            }

            ViewBag.opScope = opScope;
            ViewBag.funcName = funcName;
            
            SysCodeDao sysCodeDao = new SysCodeDao();
            //資料狀態
            ViewBag.statusjqList = sysCodeDao.jqGridList("AP", "DATA_STATUS", true);


            FAPVeCodeDao fAPVeCodeDao = new FAPVeCodeDao();
            //清理大類
            ViewBag.level1List = fAPVeCodeDao.loadSelectList("CLR_LEVEL1", true);
            ViewBag.level1jqList = fAPVeCodeDao.jqGridList("CLR_LEVEL1", true);

            //清理小類
            ViewBag.level2List = fAPVeCodeDao.loadSelectList("CLR_LEVEL2", true);
            ViewBag.level2jqList = fAPVeCodeDao.jqGridList("CLR_LEVEL2", true);

            //清理狀態  add by daiyu 20200729
            ViewBag.clrStatusList = sysCodeDao.loadSelectList("AP", "CLR_STATUS", true);
            ViewBag.clrStatusjqList = sysCodeDao.jqGridList("AP", "CLR_STATUS", true);

            return View();
        }



        /// <summary>
        /// 查詢清理紀錄
        /// 挑【FAP_VE_TRACE 逾期未兌領清理記錄檔】中清理狀態<>已給付、已清理結案的資料
        /// 1.已給付
        /// 2.已清理結案
        /// 3.已通知尚未給付
        /// 4.尚未通知
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public JsonResult qryProc(string qType, string paid_id, string check_no, string closed_no)
        {
            logger.Info("qryProc begin!!");
            try
            {
                //查詢【FAP_VE_TRACE 逾期未兌領清理記錄檔】
                FAPVeTraceDao fAPVeTraceDao = new FAPVeTraceDao();
                //List<OAP0011Model> rows = fAPVeTraceDao.qryForOAP0011(qType, paid_id, check_no, new string[] { "", "3", "4" });
                List<OAP0011Model> rows = fAPVeTraceDao.qryForOAP0011(qType, paid_id, check_no, closed_no, new string[] { "", "1", "2", "3", "4" });   //modify by daiyu 20200729

                string paid_name = "";
                if (rows.Count > 0)
                {
                    paid_id = rows[0].paid_id;
                    paid_name = rows[0].paid_name;
                }

                //delete by daiyu 20200729
                //else {
                //    List<OAP0011Model> rowsAll = fAPVeTraceDao.qryForOAP0011(qType, paid_id, check_no, new string[] { "", "1", "2", "3", "4" });
                //    if(rowsAll.Count > 0)
                //        return Json(new { success = false, err = "輸入之資料己給付或己清理，不可執行!!" });
                //}
                    

                FAPVeTracePoliDao fAPVeTracePoliDao = new FAPVeTracePoliDao();

                foreach (OAP0011Model d in rows) {
                    FAP_VE_TRACE_POLI poli = fAPVeTracePoliDao.qryForOAP0011(d);
                    d.o_paid_cd = StringUtil.toString(poli.o_paid_cd);

                    if ("".Equals(StringUtil.toString(d.level_1)) & !"".Equals(StringUtil.toString(d.memo))) {
                        string[] memoArr = d.memo.Split('|');
                        d.level_1 = memoArr[3];
                        d.level_2 = memoArr[4];
                    }
                }


                if("q_paid_id".Equals(qType))
                    writePiaLog(rows.Count, paid_id, "Q");

                var jsonData = new { success = true, dataList = rows, paid_id = paid_id, paid_name = paid_name};
                return Json(jsonData, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e) {
                return Json(new { success = false, err = "其它錯誤，請洽系統管理員!!" });
            }
            
        }



        /// <summary>
        /// 用"給付對象ID"查"給付對象姓名"
        /// </summary>
        /// <param name="paid_id"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult qryPaidName(string paid_id)
        {
            string paid_name = "";


            try
            {
                FAPVeTraceDao fAPVeTraceDao = new FAPVeTraceDao();
                FAP_VE_TRACE trace = fAPVeTraceDao.chkPaidIdExist(paid_id);
                paid_name = StringUtil.toString(trace.paid_name);
            }
            catch (Exception e) {
                logger.Error(e.ToString());
                return Json(new { success = false, err = "其它錯誤，請洽系統管理員!!" });
            }
            

            if (!"".Equals(paid_name))
                return Json(new { success = true, paid_name = paid_name });
            else
                return Json(new { success = false, err = "查無對應給付對象姓名" });
        }


        /// <summary>
        /// 檢核同一批要結案的支票，其踐行程序需要一致
        /// </summary>
        /// <param name="gridData"></param>
        /// <returns></returns>
        private bool chkCloseProc(List<OAP0011Model> gridData) {
            bool bPass = true;

            FAPVeTrackProcDao fAPVeTrackProcDao = new FAPVeTrackProcDao();
            FAPVeTraceDao fAPVeTraceDao = new FAPVeTraceDao();

            string strConn = DbUtil.GetDBFglConnStr();
            using (SqlConnection conn = new SqlConnection(strConn))
            {
                conn.Open();

                //add by daiyu 20201217 先判斷有支票有沒有踐行程序才需繼續往下處理
                List<OAP0011Model> noPracCheckList = new List<OAP0011Model>();
                
                foreach (OAP0011Model d in gridData)
                {
                    FAP_VE_TRACE m = fAPVeTraceDao.qryByCheckNo(d.check_no, d.check_acct_short);
                    if (m.exec_date == null) {
                        List<OAP0010DModel> tmpList = fAPVeTrackProcDao.qryByCheckNo(d.check_no, d.check_acct_short);
                        if (!tmpList.Any())
                            bPass = false;
                    }
                }

                if (bPass)
                    return bPass;
                else
                    bPass = true;
                //end add 20201217


                List<OAP0010DModel> procDList = new List<OAP0010DModel>();
                
                foreach (OAP0011Model d in gridData)
                {
                    List<OAP0010DModel> tmpList = fAPVeTrackProcDao.qryByCheckNo(d.check_no, d.check_acct_short);
                    if (procDList.Count == 0 & tmpList.Count > 0)
                    {
                        procDList = tmpList;
                    }
                    else if (procDList.Count > 0 & tmpList.Count > 0) {
                        if (procDList.Count.CompareTo(tmpList.Count) != 0)
                            return false;
                        else {
                            foreach (OAP0010DModel procD in procDList){
                                if(tmpList.Where(x => x.practice == procD.practice & x.cert_doc == procD.cert_doc & x.exec_date == procD.exec_date
                                & x.proc_desc == procD.proc_desc).Count() == 0)
                                    return false;

                            }
                        }
                    }
                }
            }
                

            return bPass;
        }





        /// <summary>
        /// "申請覆核"
        /// </summary>
        /// <param name="level_1"></param>
        /// <param name="level_2"></param>
        /// <param name="qryKey"></param>
        /// <param name="closed_date"></param>
        /// <param name="closed_desc"></param>
        /// <param name="gridData"></param>
        /// <param name="rptPracList"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult execSave(string level_1, string level_2, string qryKey, string closed_date, string closed_desc
            , List<OAP0011Model> gridData, List<VeCleanPracModel> rptPracList)
        {
            logger.Info("execSave begin");

            closed_date = "";   //add by daiyu 20210205

            //delete by daiyu 20201218 取消判斷
            //if (!chkCloseProc(gridData)) {

            //    return Json(new { success = false, err = "選取的支票中，含不一致的踐行程式，請至『OAP0008 異動清理記錄功能維護作業』修正後再申請結案!!" });
            //}

            string errStr = "";
            try
            {
                List<FAP_VE_TRACE_PROC_HIS> dataList = new List<FAP_VE_TRACE_PROC_HIS>();


                /*------------------ DB處理   begin------------------*/
                string[] curDateTime = DateUtil.getCurChtDateTime(3).Split(' ');
                //取得流水號
                SysSeqDao sysSeqDao = new SysSeqDao();
                string qPreCode = curDateTime[0];
                var cId = sysSeqDao.qrySeqNo("AP", "C", qPreCode).ToString();
                int seqLen = 12 - ("C" + qPreCode).Length;
                var aply_no = "C" + qPreCode + cId.ToString().PadLeft(seqLen, '0');

                //取得結案編號
                var closed_no = "";
                if ("".Equals(StringUtil.toString(gridData[0].closed_no)))
                {
                    //string qPreCodeC = closed_date.Substring(0, 5);     
                    string qPreCodeC = curDateTime[0].Substring(0, 5);  //modify by daiyu 20210225
                    var cIdC = sysSeqDao.qrySeqNo("AP", "VECLS", qPreCodeC).ToString();
                    closed_no = qPreCodeC + level_1.Trim() + level_2.Trim() + cIdC.ToString().PadLeft(5, '0');
                }
                else {
                    closed_no = StringUtil.toString(gridData[0].closed_no);
                    FAPVeTraceDao fAPVeTraceDao = new FAPVeTraceDao();
                    gridData = fAPVeTraceDao.qryByClosedNo(StringUtil.toString(closed_no));

                }
                    


                string strConn = DbUtil.GetDBFglConnStr();
                using (SqlConnection conn = new SqlConnection(strConn))
                {
                    conn.Open();

                    SqlTransaction transaction = conn.BeginTransaction("Transaction");
                    try
                    {
                        DateTime dt = DateTime.Now;

                        FAPVeTraceDao fAPVeTraceDao = new FAPVeTraceDao();
                        FAPVeTraceHisDao fAPVeTraceHisDao = new FAPVeTraceHisDao();



                        #region add by daiyu 20201204 清理階段的判斷
                        FAPTelCheckDao fAPTelCheckDao = new FAPTelCheckDao();
                        FAPTelInterviewDao fAPTelInterviewDao = new FAPTelInterviewDao();

                        List<OAP0011Model> checkList = fAPVeTraceHisDao.qryForOAP0011(aply_no);
                        Dictionary<string, FAP_TEL_INTERVIEW> tel_interview_map = new Dictionary<string, FAP_TEL_INTERVIEW>();

                        foreach (OAP0011Model d in gridData)
                        {
                            FAP_TEL_CHECK telM = fAPTelCheckDao.qryByCheckNo(d.check_no, d.check_acct_short, "tel_assign_case");

                            if (telM == null)
                                continue;
                            else
                            {
                                if (!"Y".Equals(StringUtil.toString(telM.data_flag)) || "".Equals(StringUtil.toString(telM.tel_proc_no)))
                                {
                                    continue;
                                }
                                else
                                {
                                    if (!tel_interview_map.ContainsKey(StringUtil.toString(telM.tel_proc_no)))
                                    {
                                        FAP_TEL_INTERVIEW tel_i = fAPTelInterviewDao.qryByTelProcNo(telM.tel_proc_no);
                                        if (tel_i != null)
                                        {
                                            if (!"11".Equals(StringUtil.toString(tel_i.clean_status)))
                                            {
                                                transaction.Rollback();
                                                return Json(new { success = false, err = "支票號碼：" + d.check_no + "  清理階段錯誤，應為11 結案登錄，現為" + StringUtil.toString(tel_i.clean_status) + "，請至『OAP0048』登打清理階段完成日 " }, JsonRequestBehavior.AllowGet);
                                            }
                                            else {
                                                tel_interview_map.Add(telM.tel_proc_no, tel_i);
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        #endregion

                        foreach (OAP0011Model d in gridData) {
                            d.closed_desc = StringUtil.toString(closed_desc);   //add by daiyu 20200730


                            //新增覆核資料【FAP_VE_TRACE_HIS 逾期未兌領清理記錄暫存檔】
                            fAPVeTraceHisDao.insertForOAP0011(dt, Session["UserID"].ToString(), aply_no, closed_no, level_1, level_2
                                , d, conn, transaction);

                            //異動【FAP_VE_TRACE 逾期未兌領清理記錄檔】資料狀態
                            fAPVeTraceDao.updateForOAP0011(dt, Session["UserID"].ToString(), d.check_no, d.check_acct_short, "2", closed_no
                                , closed_desc, conn, transaction);
                        }

        
                        //新增【FAP_VE_CLOSED_PROC_HIS 結案報表踐行程序暫存檔】
                        //add by daiyu 20200804
                        FAPVeClosedProcHisDao fAPVeClosedProcHisDao = new FAPVeClosedProcHisDao();
                        foreach (VeCleanPracModel prac in rptPracList) {
                            FAP_VE_CLOSED_PROC_HIS d = new FAP_VE_CLOSED_PROC_HIS();
                            string[] execDate = prac.exec_date.Split('/');

                            d.aply_no = aply_no;
                            d.closed_no = closed_no;
                            d.practice = prac.practice;
                            d.cert_doc = prac.cert_doc;
                            d.exec_date = Convert.ToDateTime((Convert.ToInt16(execDate[0]) + 1911).ToString() + '-' + execDate[1] + '-' + execDate[2]);

                            //d.exec_date = Convert.ToDateTime(d.exec_date).AddYears(1911);
                            d.proc_desc = StringUtil.toString(prac.proc_desc);
                            d.appr_stat = "1";
                            d.update_id = Session["UserID"].ToString();
                            d.update_datetime = dt;

                            fAPVeClosedProcHisDao.insert(d, conn, transaction);
                        }


                        //新增"覆核資料檔"
                        FAPAplyRecDao fAPAplyRecDao = new FAPAplyRecDao();
                        FAP_APLY_REC aplyRec = new FAP_APLY_REC();
                        aplyRec.aply_type = "C";
                        aplyRec.appr_stat = "1";
                        aplyRec.appr_mapping_key = closed_no; 
                        aplyRec.create_id = Session["UserID"].ToString();
                        aplyRec.memo = qryKey + "|" + level_1 + "|" + level_2 + "|" + closed_date;
                        aply_no = fAPAplyRecDao.insert(aply_no, aplyRec, conn, transaction);

                        transaction.Commit();
                    }
                    catch (Exception e)
                    {
                        logger.Error(e.ToString());
                        transaction.Rollback();

                        return Json(new { success = false, err = "其它錯誤，請洽系統管理員!!" });
                    }
                }

                return Json(new { success = true, aply_no = aply_no, err = errStr, closed_no = closed_no });

            }
            catch (Exception e)
            {
                logger.Error(e.ToString());
                return Json(new { success = false, err = "其它錯誤，請洽系統管理員!!" });

            }
        }


        [HttpPost]
        public JsonResult PrintAply(string paid_id, string paid_name, string level_1, string level_2, string qryKey
            , string closed_date, string closed_desc
            , List<OAP0011Model> gridData, List<VeCleanPracModel> rptPracList)
        {
            FAPVeTraceDao fAPVeTraceDao = new FAPVeTraceDao();
            List<OAP0011Model> dataList = gridData;

            VeCleanUtil veCleanUtil = new VeCleanUtil();
            string guid = veCleanUtil.closedRpt(paid_id, paid_name, level_1, level_2, "", closed_date, dataList
                , Session["UserID"].ToString() , Session["UserName"].ToString(), closed_desc, rptPracList);

            var jsonData = new { success = true, guid = guid };
            return Json(jsonData, JsonRequestBehavior.AllowGet);
        }


        /// <summary>
        /// 刪除結案編號  
        /// -----------------------------
        /// add by daiyu 20200731
        /// </summary>
        /// <param name="closed_no"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult DelClosedNo(string closed_no)
        {
            logger.Info("DelClosedNo begin:" + closed_no);

            try
            {
                if("".Equals(StringUtil.toString(closed_no)))
                    return Json(new { success = false, err = "請輸入結案編號!!" });


                FAPVeTraceDao fAPVeTraceDao = new FAPVeTraceDao();
                List<OAP0011Model> rows = fAPVeTraceDao.qryByClosedNo(closed_no);

                if (rows != null) {
                    if (!"".Equals(rows[0].closed_date)) {
                        return Json(new { success = false, err = "已結案，不可刪除結案編號!!" });
                    }
                }


                string strConn = DbUtil.GetDBFglConnStr();

                using (SqlConnection conn = new SqlConnection(strConn))
                {
                    conn.Open();
                    SqlTransaction transaction = conn.BeginTransaction("Transaction");

                    try
                    {
                        fAPVeTraceDao.delCloseNo(closed_no, conn, transaction);
                        transaction.Commit();
                        return Json(new { success = true });
                    }
                    catch (Exception e) {
                        transaction.Rollback();
                        return Json(new { success = false, err = "其它錯誤，請洽系統管理員!!" });
                    }
                }
            }
            catch (Exception e) {
                logger.Error(e.ToString());
                return Json(new { success = false, err = "其它錯誤，請洽系統管理員!!" });
            }
        }





        /// <summary>
        /// 列印結案報表
        /// </summary>
        /// <param name="paid_id"></param>
        /// <param name="paid_name"></param>
        /// <param name="level_1"></param>
        /// <param name="level_2"></param>
        /// <param name="closed_no"></param>
        /// <param name="closed_date"></param>
        /// <param name="gridData"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Print(string paid_id, string paid_name, string level_1, string level_2, string closed_no, string closed_date, List<OAP0011Model> gridData)
        {

            FAPVeTraceDao fAPVeTraceDao = new FAPVeTraceDao();
            List<OAP0011Model> dataList = new List<OAP0011Model>();

            if ("".Equals(StringUtil.toString(closed_no))) {
                dataList = gridData;

                FAPAplyRecDao fAPAplyRecDao = new FAPAplyRecDao();  //add by daiyu 20201209
                using (dbFGLEntities db = new dbFGLEntities()) {
                    APAplyRecModel fapAplyRec = fAPAplyRecDao.qryAplyType("C", "1", StringUtil.toString(gridData[0].closed_no), db).FirstOrDefault();

                    if (fapAplyRec != null) {
                        closed_no = fapAplyRec.appr_mapping_key;    //add by daiyu 20210118

                        string[] memoArr = fapAplyRec.memo.Split('|');
                        if (memoArr.Length > 5) {
                            if (StringUtil.toString(memoArr[5]).Length >= 7) {
                                closed_date = memoArr[5].Substring(0, 3) + "/" + memoArr[5].Substring(3, 2) + "/" + memoArr[5].Substring(5, 2);
                            } else
                                closed_date = memoArr[5];
                        }
                            


                    }
                }
                    

            }
            else
                dataList = fAPVeTraceDao.qryByClosedNo(StringUtil.toString(gridData[0].closed_no));

            FAPVeTracePoliDao fAPVeTracePoliDao = new FAPVeTracePoliDao();

            foreach (OAP0011Model d in dataList)
            {
                FAP_VE_TRACE_POLI poli = fAPVeTracePoliDao.qryForOAP0011(d);
                d.o_paid_cd = StringUtil.toString(poli.o_paid_cd);

                if ("".Equals(StringUtil.toString(d.level_1)) & !"".Equals(StringUtil.toString(d.memo)))
                {
                    string[] memoArr = d.memo.Split('|');
                    d.level_1 = memoArr[3];
                    d.level_2 = memoArr[4];
                }
            }

            VeCleanUtil veCleanUtil = new VeCleanUtil();

            string guid = veCleanUtil.closedRpt(paid_id, paid_name, level_1, level_2, closed_no, closed_date, dataList
                , Session["UserID"].ToString() , Session["UserName"].ToString(), "", null);

            //SysCodeDao sysCodeDao = new SysCodeDao();
            ////原給付性質
            //Dictionary<string, string> oPaidCdMap = sysCodeDao.qryByTypeDic("AP", "O_PAID_CD");

            //foreach (OAP0011Model d in gridData) {
            //    string o_paid_cd = StringUtil.toString(d.o_paid_cd);
            //    if (oPaidCdMap.ContainsKey(o_paid_cd))
            //        d.o_paid_cd = oPaidCdMap[o_paid_cd];

            //    if ("".Equals(StringUtil.toString(closed_no)))
            //        closed_no = d.closed_no;
            //}



            //FAPVeCodeDao fAPVeCodeDao = new FAPVeCodeDao();

            ////清理大類
            //Dictionary<string, string> level1Map = fAPVeCodeDao.qryByTypeDic("CLR_LEVEL1");
            //if (level1Map.ContainsKey(level_1))
            //    level_1 = level1Map[level_1];

            ////清理小類
            //Dictionary<string, string> level2Map = fAPVeCodeDao.qryByTypeDic("CLR_LEVEL2");
            //if (level2Map.ContainsKey(level_2))
            //    level_2 = level2Map[level_2];

        


            //List<procModel> rptProcList = new List<procModel>();
            //rptModel rptModel = new rptModel();

            //#region 查詢【FAP_VE_TRACE 逾期未兌領清理記錄檔】的踐行程序一~五
            //string[] checkArr = gridData.Select(x => x.check_no).ToArray();
            //FAPVeTraceDao fAPVeTraceDao = new FAPVeTraceDao();
            //List<FAP_VE_TRACE> traceMainList = fAPVeTraceDao.qryForOAP0011Rpt(checkArr);
            //int i = 0;

            //foreach (FAP_VE_TRACE d in traceMainList) {
            //    if (!"".Equals(StringUtil.toString(d.practice_1))) {
            //        i++;
            //        rptModel.practice1 = genRptProcMain(rptModel.practice1, d.practice_1, d.exec_date_1, d.cert_doc_1, d.proc_desc, i.ToString());
            //        rptProcList.Add(rptModel.practice1);
            //    }

            //    if (!"".Equals(StringUtil.toString(d.practice_2)))
            //    {
            //        i++;
            //        rptModel.practice2 = genRptProcMain(rptModel.practice2, d.practice_2, d.exec_date_2, d.cert_doc_2, d.proc_desc, i.ToString());
            //        rptProcList.Add(rptModel.practice2);
            //    }

            //    if (!"".Equals(StringUtil.toString(d.practice_3)))
            //    {
            //        i++;
            //        rptModel.practice3 = genRptProcMain(rptModel.practice3, d.practice_3, d.exec_date_3, d.cert_doc_3, d.proc_desc, i.ToString());
            //        rptProcList.Add(rptModel.practice3);
            //    }

            //    if (!"".Equals(StringUtil.toString(d.practice_4)))
            //    {
            //        i++;
            //        rptModel.practice4 = genRptProcMain(rptModel.practice4, d.practice_4, d.exec_date_4, d.cert_doc_4, d.proc_desc, i.ToString());
            //        rptProcList.Add(rptModel.practice4);
            //    }

            //    if (!"".Equals(StringUtil.toString(d.practice_5)))
            //    {
            //        i++;
            //        rptModel.practice5 = genRptProcMain(rptModel.practice5, d.practice_5, d.exec_date_5, d.cert_doc_5, d.proc_desc, i.ToString());
            //        rptProcList.Add(rptModel.practice5);
            //    }
                
            //}
            

            
            //#endregion

            //#region 查詢【FAP_VE_TRACE_PROC 逾期未兌領清理記錄歷程檔】的踐行程序
            //FAPVeTrackProcDao fAPVeTrackProcDao = new FAPVeTrackProcDao();
            //List<FAP_VE_TRACE_PROC> procList = fAPVeTrackProcDao.qryByCheckNoList(checkArr);
            
            //foreach (FAP_VE_TRACE_PROC d in procList) {
            //    string seq = "";
            //    if (!rptModel.procList.ContainsKey(d.practice))
            //    {
            //        i++;
            //        rptModel.procList.Add(d.practice, new procModel());
            //        seq = i.ToString();
            //    }
            //    else
            //        seq = rptModel.procList[d.practice].seq;


            //    rptModel.procList[d.practice] = genRptProcMain(rptModel.procList[d.practice], d.practice, d.exec_date, d.cert_doc, d.proc_desc, seq);
            //}

            //foreach (KeyValuePair<string, procModel> item in rptModel.procList) {
            //    rptProcList.Add(item.Value);
            //}
            //#endregion

            //rptProcList = rptProcList.OrderBy(x => Convert.ToDateTime(x.exec_date).AddYears(1911))
            //    .ThenBy(x => x.practice.Length).ThenBy(x => x.practice).ToList();
            //rptProcList = genCodeDesc(rptProcList);
            //CommonUtil commonUtil = new CommonUtil();
            //DataTable dtMain = commonUtil.ConvertToDataTable<procModel>(rptProcList);

            //DataTable dtDetail = commonUtil.ConvertToDataTable<OAP0011Model>(gridData);


            //var ReportViewer1 = new ReportViewer();
            ////清除資料來源
            //ReportViewer1.LocalReport.DataSources.Clear();
            ////指定報表檔路徑   
            //ReportViewer1.LocalReport.ReportPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory + "Report\\Rdlc\\OAP0011P.rdlc");
            ////設定資料來源
            //ReportViewer1.LocalReport.DataSources.Add(new ReportDataSource("DataSet1", dtMain));
            //ReportViewer1.LocalReport.DataSources.Add(new ReportDataSource("DataSet2", dtDetail));

            ////報表參數
            //ReportViewer1.LocalReport.SetParameters(new ReportParameter("Title", "逾期未兌領清理結案報表"));
            //ReportViewer1.LocalReport.SetParameters(new ReportParameter("UserId", Session["UserID"].ToString() + Session["UserName"].ToString()));


            //ReportViewer1.LocalReport.SetParameters(new ReportParameter("paid_id", paid_id));
            //ReportViewer1.LocalReport.SetParameters(new ReportParameter("paid_name", paid_name));
            //ReportViewer1.LocalReport.SetParameters(new ReportParameter("level_1", level_1));
            //ReportViewer1.LocalReport.SetParameters(new ReportParameter("level_2", level_2));
            //ReportViewer1.LocalReport.SetParameters(new ReportParameter("closed_no", closed_no));
            //ReportViewer1.LocalReport.SetParameters(new ReportParameter("memo", memo));

            //String guid = Guid.NewGuid().ToString();


            //ReportViewer1.LocalReport.Refresh();

            //Microsoft.Reporting.WebForms.Warning[] tWarnings;
            //string[] tStreamids;
            //string tMimeType;
            //string tEncoding;
            //string tExtension;
            //byte[] tBytes = ReportViewer1.LocalReport.Render("pdf", null, out tMimeType, out tEncoding, out tExtension, out tStreamids, out tWarnings);
            //string fileName = "OAP0011P_" + guid + ".pdf";
            //using (FileStream fs = new FileStream(Path.Combine(AppDomain.CurrentDomain.BaseDirectory + "Temp\\") + fileName, FileMode.Create))
            //{
            //    fs.Write(tBytes, 0, tBytes.Length);
            //}

            ////Session[guid] = rw;



            //writePiaLog(gridData.Count, paid_id, "P");

            var jsonData = new { success = true, guid = guid };
            return Json(jsonData, JsonRequestBehavior.AllowGet);
        }

        public FileContentResult downloadRpt(String id)
        {
            byte[] fileBytes = System.IO.File.ReadAllBytes(Server.MapPath("~/Temp/") + "OAP0011P" + "_" + id + ".pdf");


            string fullPath = Server.MapPath("~/Temp/") + "OAP0011P" + "_" + id + ".pdf";
            if (System.IO.File.Exists(fullPath))
                System.IO.File.Delete(fullPath);


            return File(fileBytes, "application/pdf", "逾期未兌領支票清理結案報表.pdf");
        }


        private void writePiaLog(int affectRows, string piaOwner, string executionType)
        {
            PIA_LOG_MAIN piaLogMain = new PIA_LOG_MAIN();
            piaLogMain.TRACKING_TYPE = "A";
            piaLogMain.ACCESS_ACCOUNT = Session["UserID"].ToString();
            piaLogMain.ACCOUNT_NAME = Session["UserName"].ToString();
            piaLogMain.PROGFUN_NAME = "OAP0011Controller";
            piaLogMain.EXECUTION_CONTENT = MaskUtil.maskId(piaOwner);
            piaLogMain.AFFECT_ROWS = affectRows;
            piaLogMain.PIA_TYPE = "0100100000";
            piaLogMain.EXECUTION_TYPE = executionType;
            piaLogMain.ACCESSOBJ_NAME = "FAP_VE_TRACE";
            piaLogMain.PIA_OWNER1 = MaskUtil.maskId(piaOwner);
            PiaLogMainDao piaLogMainDao = new PiaLogMainDao();
            piaLogMainDao.Insert(piaLogMain);

        }


        private List<procModel> genCodeDesc(List<procModel> rptProcList) {


            FAPVeCodeDao fAPVeCodeDao = new FAPVeCodeDao();
            //踐行程序
            Dictionary<string, string> practiceMap = fAPVeCodeDao.qryByTypeDic("CLR_PRACTICE");

            //証明文件
            Dictionary<string, string> certDacMap = fAPVeCodeDao.qryByTypeDic("CLR_CERT_DOC");

            int i = 0;
            foreach (procModel d in rptProcList.OrderBy(x => Convert.ToDateTime(x.exec_date).AddYears(1911))
                .ThenBy(x => x.practice.Length).ThenBy(x => x.practice).ToList()) {
                i++;
                d.seq = StringUtil.transToChtNumber(i, true);

                if (practiceMap.ContainsKey(d.practice))
                    d.practice_desc = practiceMap[d.practice];
                else
                    d.practice_desc = d.practice;

                if (certDacMap.ContainsKey(d.cert_doc))
                    d.cert_doc_desc = certDacMap[d.cert_doc];
                else
                    d.cert_doc_desc = d.cert_doc;

            }


            return rptProcList;
        }



        private procModel genRptProcMain(procModel model, string practice, DateTime? exec_date, string cert_doc, string proc_desc, string seq) {
            bool bDup = false;
            try
            {
                if (new ValidateUtil().IsNum(seq))
                    model.seq = StringUtil.transToChtNumber(Convert.ToInt16(seq), true);

                if (exec_date != null) {
                    string strDate = exec_date.Value.Year - 1911 + "/" + exec_date.Value.Month + "/" + exec_date.Value.Day;
                    if (model.exec_date == null)
                        bDup = true;
                    else if (strDate.CompareTo(model.exec_date) > 0)
                        bDup = true;


                    if (bDup)
                    {
                        
                        model.practice = practice;
                        model.cert_doc = cert_doc;
                        model.exec_date = strDate;
                        model.proc_desc = proc_desc;
                    }
                }
                

            }
            catch (Exception e) {
                logger.Error(e.ToString());
            }
            

            return model;
        }



        [HttpPost]
        public JsonResult qryPracByClosedNo(string closed_no)
        {
            FAPVeTraceDao fAPVeTraceDao = new FAPVeTraceDao();

            List<OAP0011Model> rows = fAPVeTraceDao.qryByClosedNo(closed_no);

            if (rows == null)
            {
                var jsonData = new { success = true, rows = rows };
                return Json(jsonData, JsonRequestBehavior.AllowGet);
            }
            else
                return qryPractice("", rows);

        }


        [HttpPost]
        public JsonResult qryPractice(string paid_id, List<OAP0011Model> gridData)
        {
            logger.Info("qryPractice begin!!");
            try
            {
                VeCleanUtil veCleanUtil = new VeCleanUtil();
                string[] checkArr = gridData.Select(x => x.check_no).ToArray();
                List<VeCleanPracModel> rows = new List<VeCleanPracModel>();
                rows = veCleanUtil.qryPractice("prac", checkArr, "");

                List<OAP0011Model> dataClosedList = new List<OAP0011Model>();
                FAPVeTraceDao fAPVeTraceDao = new FAPVeTraceDao();
                if (!"".Equals(StringUtil.toString(paid_id))) {
                    List<OAP0011Model> closedRows = fAPVeTraceDao.qryForOAP0011("q_paid_id", paid_id, "", "", new string[] { "1", "2" });  

                    if (closedRows != null) {
                       
                        foreach (OAP0011Model d in closedRows.GroupBy(x => new { x.closed_no, x.closed_date})
                            .Select(group => new OAP0011Model { closed_date = group.Key.closed_date, closed_no = group.Key.closed_no}).ToList<OAP0011Model>())
                        {
                            if (d.closed_date != null)
                                dataClosedList.Add(d);
                        }
                    }
                }

                var jsonData = new { success = true, rows = rows, dataClosedList = dataClosedList };
                return Json(jsonData, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new { success = false, err = "其它錯誤，請洽系統管理員!!" });
            }

        }


        internal class rptModel
        {
            
            public procModel practice1 { get; set; }
            public procModel practice2 { get; set; }
            public procModel practice3 { get; set; }
            public procModel practice4 { get; set; }
            public procModel practice5 { get; set; }

            public Dictionary<string, procModel> procList = new Dictionary<string, procModel>();

            public rptModel()
            {
                
                practice1 = new procModel();
                practice2 = new procModel();
                practice3 = new procModel();
                practice4 = new procModel();
                practice5 = new procModel();
            }
        }

        internal class procModel
        {
            public string seq { get; set; }
            public string practice { get; set; }
            public string exec_date { get; set; }
            public string cert_doc { get; set; }

            public string practice_desc { get; set; }

            public string cert_doc_desc { get; set; }
            public string proc_desc { get; set; }
            

            public procModel()
            {
                seq = "";
                practice = "";
                exec_date = "";
                cert_doc = "";
                practice_desc = "";
                cert_doc_desc = "";
                proc_desc = "";
            }
        }
    }
}