using FAP.Web.ActionFilter;
using FAP.Web.AS400Models;
using FAP.Web.BO;
using FAP.Web.Daos;
using FAP.Web.Models;
using FAP.Web.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.EasycomClient;
using System.Data.SqlClient;
using System.Linq;
using System.Transactions;
using System.Web.Mvc;

/// <summary>
/// 功能說明：清理結案申請覆核作業
/// 初版作者：20190711 Daiyu
/// 修改歷程：20190711 Daiyu
/// 需求單號：
/// 修改說明：初版
/// ---------------------------------------------------------
/// 修改歷程：20200805 Daiyu
/// 需求單號：
/// 修改說明：處理結案報表踐行程序
/// ---------------------------------------------------------
/// 修改歷程：20210205 daiyu 
/// 需求單號：202101280283-00
/// 修改內容：1.畫面增加篩選條件：申請人、給付對象ID，可擇一輸入，若沒選，就SHOW全部。
///           2.畫面呈現改預設SHOW 20筆資料。
///           3."結案日期"改為OAP0011的覆核日期，結案編號改取系統年月
/// ---------------------------------------------------------
/// 修改歷程：20210331 daiyu 
/// 需求單號：202103250638-00
/// 修改內容：執行覆核作業時，若該批結案編號中，有任一筆的清理狀態為"1.已給付"時，系統提示錯誤，且只能執行"駁回"。
/// </summary>
///

namespace FAP.Web.Controllers
{
    [Authorize]
    [CheckSessionFilterAttribute]
    public class OAP0011AController : BaseController
    {
        static private NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        static string funcName = "";

        /// <summary>
        /// 畫面初始
        /// </summary>
        /// <returns></returns>
        public ActionResult Index()
        {

            UserAuthUtil authUtil = new UserAuthUtil();


            string opScope = "";
           // string funcName = "";
            string[] roleInfo = authUtil.chkUserFuncAuth(Session["UserID"].ToString(), "~/OAP0011A/");
            if (roleInfo != null && roleInfo.Length == 2)
            {
                opScope = roleInfo[0];
                funcName = roleInfo[1];
            }

            ViewBag.funcName = funcName;
            ViewBag.opScope = opScope;


            return View();
        }


        [HttpPost]
        public JsonResult qryAply(OAP0011Model oAP0011Model)
        {
            try
            {
                List<OAP0011Model> rows = new List<OAP0011Model>();
                List<APAplyRecModel> aplyList = new List<APAplyRecModel>();

                using (new TransactionScope(TransactionScopeOption.Required, new TransactionOptions
                {
                    IsolationLevel = System.Transactions.IsolationLevel.ReadUncommitted
                }))
                {
                    using (dbFGLEntities db = new dbFGLEntities())
                    {
                        FAPAplyRecDao fAPAplyRecDao = new FAPAplyRecDao();
                        aplyList = fAPAplyRecDao.qryAplyType("C", "1", "", db);
                    }
                }


                using (DB_INTRAEntities dbIntra = new DB_INTRAEntities())
                {
                    Dictionary<string, string> userNameMap = new Dictionary<string, string>();
                    OaEmpDao oaEmpDao = new OaEmpDao();
                    string createUid = "";

                    foreach (APAplyRecModel d in aplyList)
                    {
                        OAP0011Model model = new OAP0011Model();
                        model.aply_no = d.aply_no;
                        model.update_id = d.create_id;
                        model.update_datetime = d.create_dt;

                        try
                        {
                            string[] memoArr = d.memo.Split('|');
                            model.paid_id = memoArr[0];
                            model.paid_name = memoArr[1];
                            model.check_no = memoArr[2];


                            if (!"".Equals(model.paid_id) & "".Equals(model.paid_name))
                            {
                                try
                                {
                                    FAPVeTraceDao fAPVeTraceDao = new FAPVeTraceDao();
                                    FAP_VE_TRACE trace = fAPVeTraceDao.chkPaidIdExist(model.paid_id);
                                    model.paid_name = StringUtil.toString(trace.paid_name);
                                }
                                catch (Exception e)
                                {
                                    logger.Error(e.ToString());
                                    //return Json(new { success = false, err = "其它錯誤，請洽系統管理員!!" });
                                }

                            }
                        }
                        catch (Exception e)
                        {
                            logger.Error(e.ToString());
                        }


                        createUid = StringUtil.toString(d.create_id);


                        if (!"".Equals(createUid))
                        {
                            if (!userNameMap.ContainsKey(createUid))
                                userNameMap = oaEmpDao.qryUsrName(userNameMap, createUid, dbIntra);

                            model.update_id = createUid + " " + userNameMap[createUid];
                        }


                        if (!"".Equals(StringUtil.toString(oAP0011Model.update_id))) {
                            if (!StringUtil.toString(oAP0011Model.update_id).Equals(createUid))
                                continue;
                        }

                        if (!"".Equals(StringUtil.toString(oAP0011Model.paid_id)))
                        {
                            if (!StringUtil.toString(oAP0011Model.paid_id).Equals(StringUtil.toString(model.paid_id)))
                                continue;
                        }


                        rows.Add(model);
                    }
                }


                var jsonData = new { success = true, dataList = rows };
                return Json(jsonData, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                logger.Error(e.ToString);
                var jsonData = new { success = false, err = e.ToString() };
                return Json(jsonData, JsonRequestBehavior.AllowGet);
            }
        }


        ///// <summary>
        ///// 查詢待覆核的資料
        ///// </summary>
        ///// <returns></returns>
        //[HttpPost]
        //public JsonResult LoadData()
        //{
        //    try
        //    {
        //        List<OAP0011Model> rows = new List<OAP0011Model>();
        //        List<APAplyRecModel> aplyList = new List<APAplyRecModel>();

        //        using (new TransactionScope(TransactionScopeOption.Required, new TransactionOptions
        //        {
        //            IsolationLevel = System.Transactions.IsolationLevel.ReadUncommitted
        //        }))
        //        {
        //            using (dbFGLEntities db = new dbFGLEntities())
        //            {
        //                FAPAplyRecDao fAPAplyRecDao = new FAPAplyRecDao();
        //                aplyList = fAPAplyRecDao.qryAplyType("C", "1", "", db);
        //            }
        //        }


        //        using (DB_INTRAEntities dbIntra = new DB_INTRAEntities())
        //        {
        //            Dictionary<string, string> userNameMap = new Dictionary<string, string>();
        //            OaEmpDao oaEmpDao = new OaEmpDao();
        //            string createUid = "";

        //            foreach (APAplyRecModel d in aplyList)
        //            {
        //                OAP0011Model model = new OAP0011Model();
        //                model.aply_no = d.aply_no;
        //                model.update_id = d.create_id;
        //                model.update_datetime = d.create_dt;

        //                try
        //                {
        //                    string[] memoArr = d.memo.Split('|');
        //                    model.paid_id = memoArr[0];
        //                    model.paid_name = memoArr[1];
        //                    model.check_no = memoArr[2];


        //                    if (!"".Equals(model.paid_id) & "".Equals(model.paid_name))
        //                    {
        //                        try
        //                        {
        //                            FAPVeTraceDao fAPVeTraceDao = new FAPVeTraceDao();
        //                            FAP_VE_TRACE trace = fAPVeTraceDao.chkPaidIdExist(model.paid_id);
        //                            model.paid_name = StringUtil.toString(trace.paid_name);
        //                        }
        //                        catch (Exception e)
        //                        {
        //                            logger.Error(e.ToString());
        //                            //return Json(new { success = false, err = "其它錯誤，請洽系統管理員!!" });
        //                        }

        //                    }
        //                }
        //                catch (Exception e) {
        //                    logger.Error(e.ToString());
        //                }


        //                createUid = StringUtil.toString(d.create_id);


        //                if (!"".Equals(createUid))
        //                {
        //                    if (!userNameMap.ContainsKey(createUid))
        //                        userNameMap = oaEmpDao.qryUsrName(userNameMap, createUid, dbIntra);

        //                    model.update_id = createUid + " " + userNameMap[createUid];
        //                }

        //                rows.Add(model);
        //            }
        //        }


        //        var jsonData = new { success = true, rows };
        //        return Json(jsonData, JsonRequestBehavior.AllowGet);

        //    }
        //    catch (Exception e) {

        //        logger.Error("其它錯誤：" + e.ToString());
        //        return Json(new { success = false, err = "其它錯誤，請洽系統管理員!!" });
        //    }
        //}



        /// <summary>
        /// 開啟修改明細畫面
        /// </summary>
        /// <param name="aplyNo"></param>
        /// <returns></returns>
        public ActionResult detailAply(string aplyNo)
        {
            ViewBag.funcName = funcName;

            FAPVeCodeDao fAPVeCodeDao = new FAPVeCodeDao();
            //清理大類
            ViewBag.clrLevel1List = fAPVeCodeDao.loadSelectList("CLR_LEVEL1", true);

            //清理小類
            ViewBag.clrLevel2List = fAPVeCodeDao.loadSelectList("CLR_LEVEL2", true);


            OAP0011Model model = new OAP0011Model();

            try
            {
                FAPAplyRecDao fAPAplyRecDao = new FAPAplyRecDao();
                FAP_APLY_REC aply = fAPAplyRecDao.qryByKey(aplyNo);
                if (aply != null)
                {
                    model.aply_no = aply.aply_no;
                    model.closed_no = aply.appr_mapping_key;
                    model.update_id = aply.create_id;
                    model.update_datetime = DateUtil.DatetimeToString(aply.create_dt, "");

                    string[] memoArr = aply.memo.Split('|');

                    model.paid_id = memoArr[0];
                    model.paid_name = memoArr[1];

                    if (!"".Equals(model.paid_id) & "".Equals(model.paid_name)) {
                        try
                        {
                            FAPVeTraceDao fAPVeTraceDao = new FAPVeTraceDao();
                            FAP_VE_TRACE trace = fAPVeTraceDao.chkPaidIdExist(model.paid_id);
                            model.paid_name = StringUtil.toString(trace.paid_name);
                        }
                        catch (Exception e)
                        {
                            logger.Error(e.ToString());
                            //return Json(new { success = false, err = "其它錯誤，請洽系統管理員!!" });
                        }

                    }

                    if(memoArr.Length > 2)
                        model.check_no = memoArr[2];

                    if (memoArr.Length > 3)
                        model.level_1 = memoArr[3];

                    if (memoArr.Length > 4)
                        model.level_2 = memoArr[4];

                    if (memoArr.Length > 5)
                        model.closed_date = memoArr[5];


                    writePiaLog(1, model.paid_id, "Q");

                    using (DB_INTRAEntities dbIntra = new DB_INTRAEntities())
                    {
                        Dictionary<string, string> userNameMap = new Dictionary<string, string>();
                        OaEmpDao oaEmpDao = new OaEmpDao();
                        string createUid = "";

                        createUid = StringUtil.toString(model.update_id);

                        if (!"".Equals(createUid))
                        {
                            if (!userNameMap.ContainsKey(createUid))
                                userNameMap = oaEmpDao.qryUsrName(userNameMap, createUid, dbIntra);

                            model.update_id = createUid + " " + userNameMap[createUid];
                        }
                    }

                    ViewBag.bHaveData = "Y";
                    ViewBag.aply_no = aplyNo;
                    ViewBag.paid_id = aplyNo;

                    return View(model);
                }
                else {
                    ViewBag.bHaveData = "N";
                    return View(model);
                }
            }
            catch (Exception e)
            {
                ViewBag.bHaveData = "N";
                return View(model);
            }
        }


        [HttpPost]
        public JsonResult qryVeTraceHis(string aply_no)
        {
            logger.Info("qryVeTraceHis begin!!");
            try
            {
                List<OAP0011Model> rows = new List<OAP0011Model>();

                //查詢【FAP_VE_TRACE_HIS 逾期未兌領清理記錄暫存檔】
                FAPVeTraceHisDao fAPVeTraceHisDao = new FAPVeTraceHisDao();

                rows = fAPVeTraceHisDao.qryForOAP0011(aply_no);

               

                var jsonData = new { success = true, dataList = rows };
                return Json(jsonData, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new { success = false, err = "其它錯誤，請洽系統管理員!!" });
            }
        }


        /// <summary>
        /// 核可/退回
        /// </summary>
        /// <param name="aply_no"></param>
        /// <param name="apprStat"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult execSave(string aply_no, string apprStat)
        {
            string strConn = DbUtil.GetDBFglConnStr();
            using (SqlConnection conn = new SqlConnection(strConn))
            {
                conn.Open();
                SqlTransaction transaction = conn.BeginTransaction("Transaction");

                try
                {
                    FAPAplyRecDao fAPAplyRecDao = new FAPAplyRecDao();
                    FAP_APLY_REC fapAplyRec = fAPAplyRecDao.qryByKey(aply_no);

                    if (StringUtil.toString(fapAplyRec.create_id).Equals(Session["UserID"].ToString()))
                        return Json(new { success = false, errors = "申請人與覆核人員不可相同!!" }, JsonRequestBehavior.AllowGet);



                    //add by daiyu 20210331 執行覆核作業時，若該批結案編號中，有任一筆的清理狀態為"1.已給付"時，系統提示錯誤，且只能執行"駁回"
                    FAPVeTraceHisDao fAPVeTraceHisDao = new FAPVeTraceHisDao();
                    List<OAP0011Model> checkList = fAPVeTraceHisDao.qryForOAP0011(aply_no);
                    foreach (OAP0011Model main in checkList) {
                        if ("1".Equals(StringUtil.toString(main.status)) & "2".Equals(apprStat)) 
                            return Json(new { success = false, errors = "支票號碼：" + main.check_no + " 已給付，僅能執行駁回!!" }, JsonRequestBehavior.AllowGet);
                    }



                    DateTime dt = DateTime.Now;

                    //異動覆核資料檔
                    fapAplyRec.aply_no = aply_no;
                    fapAplyRec.appr_stat = apprStat;
                    fapAplyRec.appr_id = Session["UserID"].ToString();
                    fapAplyRec.approve_datetime = dt;
                    fapAplyRec.update_id = Session["UserID"].ToString();
                    fapAplyRec.update_datetime = dt;

                    fAPAplyRecDao.updateStatus(fapAplyRec, conn, transaction);

                    string[] memoArr = fapAplyRec.memo.Split('|');

                    //  string closed_date = DateUtil.As400ChtDateToADDate(memoArr[5]);
                    string closed_date = DateUtil.As400ChtDateToADDate(DateUtil.getCurChtDate(3)); //modify by daiyu 20210225
                    FAPVeTraceDao fAPVeTraceDao = new FAPVeTraceDao();
                    int cnt = fAPVeTraceDao.updateForOAP0011A(apprStat, dt, Session["UserID"].ToString(), aply_no
                            , memoArr[3], memoArr[4], closed_date, fapAplyRec.appr_mapping_key, conn, transaction);

                    //同一批結案的支票中，若有踐行程序完全空白者，要以有踐行程序的複制過來
                    copyTracProc(aply_no, fapAplyRec.appr_mapping_key, conn, transaction);



                    //add by daiyu 20200805 處理結案報表踐行程序
                    if ("2".Equals(apprStat))   //核可
                    {
                        FAPVeClosedProcDao fAPVeClosedProcDao = new FAPVeClosedProcDao();
                        fAPVeClosedProcDao.insertFromHis(fapAplyRec.appr_mapping_key, aply_no, conn, transaction);


                        //add by daiyu 20201204 多回寫清理階段
                        //階段11 結案登錄 的資料-> update 完成日=申請日期
                        //階段12 主管覆核 的資料 -> 新增一筆，且完成日=申請覆核日
                        //FAPVeTraceHisDao fAPVeTraceHisDao = new FAPVeTraceHisDao();
                        FAPTelCheckDao fAPTelCheckDao = new FAPTelCheckDao();
                        FAPTelInterviewDao fAPTelInterviewDao = new FAPTelInterviewDao();


                        //List<OAP0011Model> checkList = fAPVeTraceHisDao.qryForOAP0011(aply_no);   //delete by daiyu 20210331
                        Dictionary<string, FAP_TEL_INTERVIEW_HIS> tel_interview_map = new Dictionary<string, FAP_TEL_INTERVIEW_HIS>();

                        foreach (OAP0011Model d in checkList)
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
                                                return Json(new { success = false, errors = "清理階段錯誤，應為11 結案登錄，現為" + StringUtil.toString(tel_i.clean_status) }, JsonRequestBehavior.AllowGet);
                                            }
                                            else
                                            {
                                                FAP_TEL_INTERVIEW_HIS his = new FAP_TEL_INTERVIEW_HIS();
                                                ObjectUtil.CopyPropertiesTo(tel_i, his);


                                                his.clean_f_date = fapAplyRec.create_dt;
                                                his.remark = "";
                                                his.data_type = "3";
                                                his.exec_action = "U";
                                                his.appr_stat = "1";
                                                his.update_id = Session["UserID"].ToString();
                                                his.update_datetime = dt;
                                                tel_interview_map.Add(telM.tel_proc_no, his);
                                            }
                                        }
                                    }
                                }
                            }
                        }


                        if (tel_interview_map.Count > 0)
                        {
                            VeTelUtil veTelUtil = new VeTelUtil();
                            FAPTelInterviewHisDao fAPTelInterviewHisDao = new FAPTelInterviewHisDao();
                            string[] curDateTime = BO.DateUtil.getCurChtDateTime(3).Split(' ');
                            //取得流水號
                            SysSeqDao sysSeqDao = new SysSeqDao();
                            String qPreCode = "0048" + curDateTime[0].Substring(0, 5);
                            var cId = sysSeqDao.qrySeqNo("AP", "0048", qPreCode).ToString();
                            string aply_no_tel = qPreCode + cId.ToString().PadLeft(3, '0');


                            //先處理 階段11 結案登錄 的資料-> update 完成日=申請日期
                            foreach (KeyValuePair<string, FAP_TEL_INTERVIEW_HIS> item in tel_interview_map)
                            {
                                //寫入申請資料
                                veTelUtil.procCleanStageAply(aply_no_tel, item.Value, dt, Session["UserID"].ToString(), conn, transaction);


                                //寫入覆核資料(跳下一個清理階段)
                                OAP0048Model _OAP0048Model = fAPTelInterviewHisDao.qryForOAP0048A("3", "1", aply_no_tel).FirstOrDefault();
                                veTelUtil.procCleanStageApprove(_OAP0048Model, dt, Session["UserID"].ToString(), conn, transaction);

                            }

                            cId = sysSeqDao.qrySeqNo("AP", "0048", qPreCode).ToString();
                            aply_no_tel = qPreCode + cId.ToString().PadLeft(3, '0');

                            //處理 階段12 主管覆核 的資料 -> 新增一筆，且完成日=申請覆核日
                            foreach (KeyValuePair<string, FAP_TEL_INTERVIEW_HIS> item in tel_interview_map)
                            {

                                item.Value.clean_f_date = dt;
                                item.Value.clean_status = "12";
                                //寫入申請資料
                                veTelUtil.procCleanStageAply(aply_no_tel, item.Value, dt, Session["UserID"].ToString(), conn, transaction);


                                //寫入覆核資料(跳下一個清理階段)
                                OAP0048Model _OAP0048Model = fAPTelInterviewHisDao.qryForOAP0048A("3", "1", aply_no_tel).FirstOrDefault();
                                veTelUtil.procCleanStageApprove(_OAP0048Model, dt, Session["UserID"].ToString(), conn, transaction);
                            }
                        }
                    }

                    FAPVeClosedProcHisDao fAPVeClosedProcHisDao = new FAPVeClosedProcHisDao();
                    fAPVeClosedProcHisDao.updateApprStat(apprStat, dt, Session["UserID"].ToString(), aply_no, conn, transaction);
                    //end add 20200805


                    



                    writePiaLog(cnt, memoArr[0], "E");


                    transaction.Commit();
                    return Json(new { success = true });

                }
                catch (Exception e)
                {
                    transaction.Rollback();
                    logger.Error("其它錯誤：" + e.ToString());

                    return Json(new { success = false, errors = "其它錯誤，請洽系統管理員!!" }, JsonRequestBehavior.AllowGet);
                }
            }
        }


        /// <summary>
        /// 同一批結案的支票中，若有踐行程序完全空白者，要以有踐行程序的複制過來
        /// </summary>
        /// <param name="aply_no"></param>
        /// <param name="conn"></param>
        /// <param name="transaction"></param>
        private void copyTracProc(string aply_no, string closed_no, SqlConnection conn, SqlTransaction transaction) {
            List<OAP0011Model> rowsCheckNo = new List<OAP0011Model>();
            FAPVeTraceHisDao fAPVeTraceHisDao = new FAPVeTraceHisDao();
            FAPVeTrackProcDao fAPVeTrackProcDao = new FAPVeTrackProcDao();

            //先找出本次要結案的所有支票
            rowsCheckNo = fAPVeTraceHisDao.qryForOAP0011(aply_no);

            List<OAP0011Model> noProcList = new List<OAP0011Model>();

            string check_no = "";
            string check_acct_short = "";

            //檢查是否有支票的踐行程序是空的...有的話..要拿有踐行程序的支票補上
            foreach (OAP0011Model checkD in rowsCheckNo) {
                List<OAP0010DModel> tmpList = fAPVeTrackProcDao.qryByCheckNo(checkD.check_no, checkD.check_acct_short);

                if (tmpList.Count > 0)
                {
                    check_no = checkD.check_no;
                    check_acct_short = checkD.check_acct_short;
                }
                else 
                    noProcList.Add(checkD);
                
            }


            if (!"".Equals(check_no) & !"".Equals(check_acct_short) & noProcList.Count > 0) {
                foreach (OAP0011Model d in noProcList) {
                    fAPVeTrackProcDao.insertForOAP0011(check_no, check_acct_short, d.check_no, d.check_acct_short, conn, transaction);
                }

            }


        }





        private void writePiaLog(int affectRows, string piaOwner, string executionType)
        {
            PIA_LOG_MAIN piaLogMain = new PIA_LOG_MAIN();
            piaLogMain.TRACKING_TYPE = "A";
            piaLogMain.ACCESS_ACCOUNT = Session["UserID"].ToString();
            piaLogMain.ACCOUNT_NAME = Session["UserName"].ToString();
            piaLogMain.PROGFUN_NAME = "OAP0011AController";
            piaLogMain.EXECUTION_CONTENT = MaskUtil.maskId(piaOwner);
            piaLogMain.AFFECT_ROWS = affectRows;
            piaLogMain.PIA_TYPE = "0100100000";
            piaLogMain.EXECUTION_TYPE = executionType;
            piaLogMain.ACCESSOBJ_NAME = "FAP_VE_TRACE";
            piaLogMain.PIA_OWNER1 = MaskUtil.maskId(piaOwner);
            PiaLogMainDao piaLogMainDao = new PiaLogMainDao();
            piaLogMainDao.Insert(piaLogMain);

        }


    }
}