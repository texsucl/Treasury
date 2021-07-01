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

using System.Web.Mvc;

/// <summary>
/// 功能說明：逾期未兌領支票維護作業
/// 初版作者：20190412 Daiyu
/// 修改歷程：20190412 Daiyu
/// 需求單號：
/// 修改內容：初版
/// ----------------------------------------------------
/// 修改歷程：20191126 Daiyu
/// 需求單號：201910290100-01
/// 修改內容：修改【備用欄位-14 (AML註記)】欄位，要先判斷AML(萊斯)系統客戶編號的"判定狀態"
/// ----------------------------------------------------
/// 修改歷程：20201027 Daiyu
/// 需求單號：202008120153-10
/// 修改內容：增加維護OPEN【清理紀錄檔 FAP_VE_TRACE】
///             1.給付帳務日 re_paid_date
///             2.再給付日期 re_paid_date_n (此需求上線前 "再給付日期 = re_paid_date")
/// </summary>
///

namespace FAP.Web.Controllers
{
    [Authorize]
    [CheckSessionFilterAttribute]
    public class OAP0002Controller : BaseController
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
            string[] roleInfo = authUtil.chkUserFuncAuth(Session["UserID"].ToString(), "~/OAP0002/");
            if (roleInfo != null && roleInfo.Length == 2)
            {
                opScope = roleInfo[0];
                funcName = roleInfo[1];
            }

            ViewBag.funcName = funcName;
            ViewBag.opScope = opScope;

            FPMCODEDao fPMCODEDao = new FPMCODEDao();
            ViewBag.statusList = fPMCODEDao.qryGrpList("PPAASTATUS", "AP");

            SysCodeDao sysCodeDao = new SysCodeDao();
            //再給付方式
            var rPaidTpList = sysCodeDao.loadSelectList("AP", "R_PAID_TP", true);
            ViewBag.rPaidTpList = rPaidTpList;


            return View();
        }


        /// <summary>
        /// 畫面執行查詢
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult qryPPAA(OAP0002Model model)
        {
            if("".Equals(StringUtil.toString(model.check_acct_short)) || "".Equals(StringUtil.toString(model.check_no)))
                Json(new { success = false, err = "請輸入 支票號碼、帳戶簡稱!!" });

            List<OAP0002PoliModel> policyList = new List<OAP0002PoliModel>();
            var haveData = "N";

            //查詢暫存檔
            if (model.isQryTmp == "1")
            {
                FAPPpaaSHisDao fAPPpaaSHisDao = new FAPPpaaSHisDao();
                List<OAP0002Model> ppaaSHisList = new List<OAP0002Model>();
                ppaaSHisList = fAPPpaaSHisDao.qryCheckByStat("1", model.check_acct_short, model.check_no, "");

                if (ppaaSHisList.Count > 0) {
                    haveData = "Y";
                    ObjectUtil.CopyPropertiesTo(ppaaSHisList[0], model);
                    try
                    {
                        model.check_amt = string.Format("{0:N2}", Convert.ToDecimal(model.check_amt));
                    }
                    catch
                    {

                    }

                    FAPPpaaDHisDao fAPPpaaDHisDao = new FAPPpaaDHisDao();
                    policyList = fAPPpaaDHisDao.qryAplyNo(ppaaSHisList[0].aplyNo);
                }

                writePiaLog(model.check_no, policyList.Count, model.paid_id, "Q");
                var jsonData = new { success = true, dataSum = model, policyList = policyList };

                return Json(jsonData, JsonRequestBehavior.AllowGet);
            }
            else
            {  //查詢正式檔
                string hisApprStat = "";
                FAPPpaaSHisDao fAPPpaaSHisDao = new FAPPpaaSHisDao();
                List<OAP0002Model> ppaaSHisList = new List<OAP0002Model>();
                ppaaSHisList = fAPPpaaSHisDao.qryCheckByStat("1", model.check_acct_short, model.check_no, "");
                if (ppaaSHisList.Count > 0) 
                    hisApprStat = StringUtil.toString(ppaaSHisList[0].apprStat);
                


                    using (EacConnection conn400 = new EacConnection(CommonUtil.GetEasycomConn()))
                {
                    conn400.Open();
                    
                    try
                    {
                        FMNPPAADao fMNPPAADao = new FMNPPAADao();
                        List<FMNPPAAModel> ppaaList = fMNPPAADao.qryForOAP0002(conn400, model.check_acct_short, model.check_no);


                        decimal sumCheckAmt = new decimal(0);

                        if (ppaaList.Count > 0) {
                            haveData = "Y";

                            ObjectUtil.CopyPropertiesTo(ppaaList[0], model);
                            try
                            {
                                model.check_amt = string.Format("{0:N2}", Convert.ToDecimal(model.check_amt));
                            }
                            catch
                            {

                            }


                            foreach (FMNPPAAModel d in ppaaList)
                            {
                                OAP0002PoliModel poliD = new OAP0002PoliModel();
                                ObjectUtil.CopyPropertiesTo(d, poliD);
                                poliD.temp_id = d.system + "|"
                                    + d.source_op + "|"
                                    + d.policy_no + "|"
                                    + d.policy_seq + "|"
                                    + d.id_dup + "|"
                                    + d.member_id + "|"
                                    + d.change_id + "|"
                                    + d.paid_id + "|"
                                    + d.check_no;
                                policyList.Add(poliD);

                            }
                        }



                        //add by daiyu 20201027 加查 FAP_VE_TRACE，多維護"給付帳務日"
                        FAPVeTraceDao fAPVeTraceDao = new FAPVeTraceDao();
                        FAP_VE_TRACE trace = fAPVeTraceDao.qryByCheckNo(model.check_no, model.check_acct_short);
                        if (trace != null) {
                            model.re_paid_date = DateUtil.DatetimeToString(trace.re_paid_date, "yyyy/MM/dd");
                        }


                        writePiaLog(model.check_no, policyList.Count, model.paid_id, "Q");
                        var jsonData = new { success = true, haveData = haveData, dataSum = model, policyList = policyList, hisApprStat = hisApprStat };
                        return Json(jsonData, JsonRequestBehavior.AllowGet);

                    }
                    catch (Exception e) {

                        logger.Error(e.ToString());
                        return Json(new { success = false, err = "其它錯誤，請洽系統管理員!!" });
                    }
                }
            }
        }



        private void writePiaLog(string checkNo, int affectRows, string piaOwner, string executionType) {
            PIA_LOG_MAIN piaLogMain = new PIA_LOG_MAIN();
            piaLogMain.TRACKING_TYPE = "A";
            piaLogMain.ACCESS_ACCOUNT = Session["UserID"].ToString();
            piaLogMain.ACCOUNT_NAME = Session["UserName"].ToString();
            piaLogMain.PROGFUN_NAME = "OAP0002Controller";
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
        /// 畫面執行申請覆核
        /// </summary>
        /// <param name="model"></param>
        /// <param name="policyNoData"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult execAply(OAP0002Model model, List<OAP0002PoliModel> policyNoData, string execAction)
        {

            #region   AML檢查  20191126 
            try
            {
                string msg = amlChk(model);

                if (!"".Equals(msg))
                    return Json(new { success = false, err = msg }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e) {
                logger.Error(e.ToString());
                return Json(new { success = false, err = "其它錯誤(AML檢核呼叫失敗)，請洽系統管理員!!" }, JsonRequestBehavior.AllowGet);
            }
            #endregion


            string aply_no = "";
            string strConn = DbUtil.GetDBFglConnStr();
            using (SqlConnection conn = new SqlConnection(strConn))
            {
                conn.Open();

                SqlTransaction transaction = conn.BeginTransaction("Transaction");
                try
                {
                    string[] curDateTime = DateUtil.getCurChtDateTime(4).Split(' ');

                    FAPAplyRecDao fAPAplyRecDao = new FAPAplyRecDao();
                    FAP_APLY_REC aplyRec = new FAP_APLY_REC();
                    aplyRec.aply_type = "A";
                    aplyRec.appr_stat = "1";
                    aplyRec.appr_mapping_key = model.check_no + "|" + model.check_acct_short;
                    aplyRec.create_id = Session["UserID"].ToString();

                    //新增"覆核資料檔"
                    aply_no = fAPAplyRecDao.insert("", aplyRec, conn, transaction);


                    //FAP_PPAA_S_HIS 支票退回回存／外幣無匯款帳號異動主檔
                    FAPPpaaSHisDao fAPPpaaSHisDao = new FAPPpaaSHisDao();
                    FAP_PPAA_S_HIS ppaaSHis = new FAP_PPAA_S_HIS();
                    ObjectUtil.CopyPropertiesTo(model, ppaaSHis);
                    ppaaSHis.aply_no = aply_no;
                    ppaaSHis.exec_action = execAction;
                    fAPPpaaSHisDao.insert(ppaaSHis, conn, transaction);

                    //FAP_PPAA_D_HIS 支票退回回存／外幣無匯款帳號異動明細檔
                    List<FAP_PPAA_D_HIS> ppaaDList = new List<FAP_PPAA_D_HIS>();
                    foreach (OAP0002PoliModel d in policyNoData) {
                        FAP_PPAA_D_HIS ppaaD = new FAP_PPAA_D_HIS();

                        string[] key = d.temp_id.Split('|');
                        ppaaD.aply_no = aply_no;
                        ppaaD.system = key[0];
                        ppaaD.source_op = key[1];
                        ppaaD.policy_no = key[2];
                        ppaaD.policy_seq = (int)(key[3] == null ? (int?)null : Convert.ToInt32(key[3]));
                        ppaaD.id_dup = key[4];
                        ppaaD.member_id = key[5];
                        ppaaD.change_id = key[6];
                        ppaaD.paid_id = key[7];
                        ppaaD.check_no = key[8];
                        ppaaD.check_acct_short = ppaaSHis.check_acct_short;
                        ppaaD.main_amt = (int)(d.main_amt == null ? (int?)null : Convert.ToInt32(d.main_amt));
                        ppaaD.policy_no_aft = d.policy_no;
                        ppaaD.policy_seq_aft = (int)(d.policy_seq == null ? (int?)null : Convert.ToInt32(d.policy_seq));
                        ppaaD.id_dup_aft = d.id_dup;
                        ppaaD.member_id_aft = d.member_id;
                        ppaaD.change_id_aft = d.change_id;

                        ppaaDList.Add(ppaaD);
                    }

                    if (ppaaDList.Count > 0) {
                        FAPPpaaDHisDao fAPPpaaDHisDao = new FAPPpaaDHisDao();

                        fAPPpaaDHisDao.insert(ppaaDList, conn, transaction);
                    }

                    transaction.Commit();

                    return Json(new { success = true, aplyNo = aply_no });
                }
                catch (Exception e)
                {
                    transaction.Rollback();
                    logger.Error("其它錯誤：" + e.ToString());

                    return Json(new { success = false, err = "其它錯誤，請洽系統管理員!!" }, JsonRequestBehavior.AllowGet);
                }
            }

            
        }



        /// <summary>
        /// 制裁名單檢核  add by daiyu 20191126
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        private string amlChk(OAP0002Model model) {
            string msg = "";

            LyodsAmlUtil lyodsAmlUtil = new LyodsAmlUtil();
            string lyodsUrl = lyodsAmlUtil.getUrl();
            if ("".Equals(lyodsUrl)) {
                msg = "制裁名單檢核網址未設定，請洽系統管理員!!";
                return msg;
            }

            

            LyodsAmlFSKWSModel amlModel = new LyodsAmlFSKWSModel();
            amlModel.paid_id = StringUtil.toString(model.paid_id);
            amlModel.paid_name = StringUtil.toString(model.paid_name);
            amlModel.o_paid_cd = StringUtil.toString(model.o_paid_cd);
            amlModel.query_id = Session["UserID"].ToString();
            amlModel.query_name = Session["UserName"].ToString();
            amlModel.unit = "VE30001";
            amlModel.source_id = "VE";
            amlModel.name = StringUtil.toString(model.paid_name);
            amlModel.enName = StringUtil.toString(model.paid_name);

            if ("".Equals(amlModel.paid_id))
                amlModel.cin_no = "VE30001-" + StringUtil.toString(model.check_acct_short) + StringUtil.toString(model.check_no);
            else
                amlModel.cin_no = "VE30001-" + StringUtil.toString(model.paid_id);


            using (EacConnection conn400 = new EacConnection(CommonUtil.GetEasycomConn()))
            {
                conn400.Open();

                amlModel = lyodsAmlUtil.fskws(lyodsUrl, amlModel, conn400);

                if (!"0".Equals(amlModel.rtn_code))
                    msg = "其它錯誤(AML檢核呼叫失敗)，請洽系統管理員!!";
                else
                {
                    FAPPPATDao fAPPPATDao = new FAPPPATDao();

                    switch (amlModel.status)
                    {
                        case "NEW":
                        case "PENDING":
                            msg = "疑似制裁名單，請至AML(萊斯)系統進行名單確認!";
                            break;
                        case "TRUE":
                            if (!("03*".Equals(StringUtil.toString(model.filler_14)) || "03".Equals(StringUtil.toString(model.filler_14))))
                                msg = "已確認是制裁名單!，AML註記欄位值僅能修改為「03」或「03*」!";

                            if ("03*".Equals(StringUtil.toString(model.filler_14)))
                            {
                                bool ppatExist = fAPPPATDao.chkExist(conn400, StringUtil.toString(model.check_no), StringUtil.toString(model.check_acct_short));
                                if (!ppatExist)
                                    msg = "已確認是制裁名單!，AML註記欄位值僅能修改為「03」(支票號碼未執行過PAP7008)!!";
                            }

                            break;

                        case "FALSE":
                            if (!("03**".Equals(StringUtil.toString(model.filler_14)) || "".Equals(StringUtil.toString(model.filler_14))))
                                msg = "已確認非制裁名單!，AML註記欄位值僅能修改為空白或「03**」!";

                            if ("03**".Equals(StringUtil.toString(model.filler_14)))
                            {
                                bool ppatExist = fAPPPATDao.chkExist(conn400, StringUtil.toString(model.check_no), StringUtil.toString(model.check_acct_short));
                                if (!ppatExist)
                                    msg = "已確認非制裁名單!，AML註記欄位值僅能修改為空白(支票號碼未執行過PAP7008)!";
                            }
                            break;
                        default:
                            if (!"".Equals(StringUtil.toString(model.filler_14)))
                                msg = "AML註記檢核不通過，不可異動!";
                            break;
                    }
                }
            }

            return msg;
        }



    }
}