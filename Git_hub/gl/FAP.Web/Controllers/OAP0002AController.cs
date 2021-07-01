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
/// 功能說明：逾期未兌領支票維護覆核作業
/// 初版作者：20190417 Daiyu
/// 修改歷程：20190417 Daiyu
///           需求單號：
///           初版
/// </summary>
///

namespace FAP.Web.Controllers
{
    [Authorize]
    [CheckSessionFilterAttribute]
    public class OAP0002AController : BaseController
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
            string[] roleInfo = authUtil.chkUserFuncAuth(Session["UserID"].ToString(), "~/OAP0002A/");
            if (roleInfo != null && roleInfo.Length == 2)
            {
                opScope = roleInfo[0];
                funcName = roleInfo[1];
            }

            ViewBag.funcName = funcName;
            ViewBag.opScope = opScope;


            return View();
        }



        /// <summary>
        /// 查詢待覆核的資料
        /// </summary>
        /// <param name="cReviewType"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult LoadData()
        {
            

            FAPPpaaSHisDao fAPPpaaSHisDao = new FAPPpaaSHisDao();

            List<OAP0002Model> rows = new List<OAP0002Model>();
            using (new TransactionScope(
                  TransactionScopeOption.Required,
                  new TransactionOptions
                  {
                      IsolationLevel = IsolationLevel.ReadUncommitted
                  }))
            {
                using (dbFGLEntities db = new dbFGLEntities())
                {
                    try
                    {
                        //查出待覆核的資料
                        rows = fAPPpaaSHisDao.qryCheckByStat("1", "", "", "");
                    }
                    catch (Exception e)
                    {
                        logger.Error("其它錯誤：" + e.ToString());
                        return Json(new { success = false, err = "其它錯誤，請洽系統管理員!!" });
                    }
                }
            }


            using (DB_INTRAEntities dbIntra = new DB_INTRAEntities())
            {
                Dictionary<string, string> userNameMap = new Dictionary<string, string>();
                OaEmpDao oaEmpDao = new OaEmpDao();
                string createUid = "";

                foreach (OAP0002Model d in rows)
                {
                    createUid = StringUtil.toString(d.create_id);

                    if (!"".Equals(createUid))
                    {
                        if (!userNameMap.ContainsKey(createUid))
                        {
                            userNameMap = oaEmpDao.qryUsrName(userNameMap, createUid, dbIntra);
                        }
                        d.create_id = createUid + " " + userNameMap[createUid];
                    }
                }
            }


            var jsonData = new { success = true, rows };
            return Json(jsonData, JsonRequestBehavior.AllowGet);

        }


        /// <summary>
        /// 開啟修改明細畫面
        /// </summary>
        /// <param name="aplyNo"></param>
        /// <returns></returns>
        public ActionResult detailAply(string aplyNo)
        {
            ViewBag.funcName = funcName;

            SysCodeDao sysCodeDao = new SysCodeDao();
            //再給付方式
            var rPaidTpList = sysCodeDao.loadSelectList("AP", "R_PAID_TP", true);
            ViewBag.rPaidTpList = rPaidTpList;

            FPMCODEDao fPMCODEDao = new FPMCODEDao();
            ViewBag.statusList = fPMCODEDao.qryGrpList("PPAASTATUS", "AP");

            OAP0002Model oAP0002Model = new OAP0002Model();

            try
            {

                FAPPpaaSHisDao fAPPpaaSHisDao = new FAPPpaaSHisDao();

                List<OAP0002Model> rows = new List<OAP0002Model>();
                using (new TransactionScope(
                      TransactionScopeOption.Required,
                      new TransactionOptions
                      {
                          IsolationLevel = IsolationLevel.ReadUncommitted
                      }))
                {
                    using (dbFGLEntities db = new dbFGLEntities())
                    {
                        try
                        {
                            //查出待覆核的資料
                            rows = fAPPpaaSHisDao.qryCheckByStat("1", "", "", aplyNo);
                        }
                        catch (Exception e)
                        {
                            logger.Error("其它錯誤：" + e.ToString());
                            return Json(new { success = false, err = "其它錯誤，請洽系統管理員!!" });
                        }
                    }
                }






                if (rows.Count > 0) {
                    oAP0002Model = rows[0];

                    //查詢AS400內正式檔的資料，比對哪些欄位有變動

                    //add by daiyu 20201027 加查 FAP_VE_TRACE，多維護"給付帳務日"
                    FAPVeTraceDao fAPVeTraceDao = new FAPVeTraceDao();
                    FAP_VE_TRACE trace = fAPVeTraceDao.qryByCheckNo(oAP0002Model.check_no, oAP0002Model.check_acct_short);
                    string re_paid_date = "";
                    if (trace != null)
                    {
                        re_paid_date = DateUtil.DatetimeToString(trace.re_paid_date, "yyyy/MM/dd");
                    }

                    procQchange(oAP0002Model, re_paid_date);
                    
                    try
                    {
                        oAP0002Model.check_amt = string.Format("{0:N2}", Convert.ToDecimal(oAP0002Model.check_amt));
                    }
                    catch {

                    }
                    


                    if ("D".Equals(oAP0002Model.exec_action))
                        oAP0002Model.execActionDesc = "※此筆為刪除FMNPPAA0(應付未付主檔資料)";

                    using (DB_INTRAEntities dbIntra = new DB_INTRAEntities())
                    {
                        Dictionary<string, string> userNameMap = new Dictionary<string, string>();
                        OaEmpDao oaEmpDao = new OaEmpDao();
                        string createUid = "";

                        createUid = StringUtil.toString(oAP0002Model.create_id);

                        if (!"".Equals(createUid))
                        {
                            if (!userNameMap.ContainsKey(createUid))
                            {
                                userNameMap = oaEmpDao.qryUsrName(userNameMap, createUid, dbIntra);
                            }
                            oAP0002Model.create_id = createUid + " " + userNameMap[createUid];
                        }
                    }


                    ViewBag.bHaveData = "Y";
                    ViewBag.aplyNo = aplyNo;


                    writePiaLog(oAP0002Model.check_no, rows.Count, oAP0002Model.paid_id, "Q", "FMNPPAA0");

                    return View(oAP0002Model);
                }
                else{

                    ViewBag.bHaveData = "N";
                    return View(oAP0002Model);
                }
                

            }
            catch (Exception e)
            {
                ViewBag.bHaveData = "N";
                return View(oAP0002Model);
            }
        }


        /// <summary>
        /// 查詢AS400內正式檔的資料，比對哪些欄位有變動
        /// </summary>
        /// <param name="d"></param>
        private void procQchange(OAP0002Model d, string re_paid_date) {
            
            using (EacConnection conn400 = new EacConnection(CommonUtil.GetEasycomConn()))
            {
                conn400.Open();

                FMNPPAADao fMNPPAADao = new FMNPPAADao();
                List<FMNPPAAModel> ppaaList = fMNPPAADao.qryForOAP0002(conn400, d.check_acct_short, d.check_no);

                if (ppaaList.Count > 0)
                {
                    FMNPPAAModel m = ppaaList[0];

                    if (!StringUtil.toString(d.status).Equals(StringUtil.toString(m.status)))
                        ViewBag.statusChg = "Y";
                    else
                        ViewBag.statusChg = "N";

                    if (!StringUtil.toString(d.filler_10).Equals(StringUtil.toString(m.filler_10)))
                        ViewBag.filler_10Chg = "Y";
                    else
                        ViewBag.filler_10Chg = "N";

                    if (!StringUtil.toString(d.filler_14).Equals(StringUtil.toString(m.filler_14)))
                        ViewBag.filler_14Chg = "Y";
                    else
                        ViewBag.filler_14Chg = "N";

                    if (!StringUtil.toString(d.paid_id).Equals(StringUtil.toString(m.paid_id)))
                        ViewBag.paid_idChg = "Y";
                    else
                        ViewBag.paid_idChg = "N";

                    if (!StringUtil.toString(d.re_paid_type).Equals(StringUtil.toString(m.re_paid_type)))
                        ViewBag.re_paid_typeChg = "Y";
                    else
                        ViewBag.re_paid_typeChg = "N";

                    if (!StringUtil.toString(d.area).Equals(StringUtil.toString(m.area)))
                        ViewBag.areaChg = "Y";
                    else
                        ViewBag.areaChg = "N";

                    if (!StringUtil.toString(d.srce_from).Equals(StringUtil.toString(m.srce_from)))
                        ViewBag.srce_fromChg = "Y";
                    else
                        ViewBag.srce_fromChg = "N";

                    if (!StringUtil.toString(d.source_kind).Equals(StringUtil.toString(m.source_kind)))
                        ViewBag.source_kindChg = "Y";
                    else
                        ViewBag.source_kindChg = "N";

                    if (!StringUtil.toString(d.pay_no).Equals(StringUtil.toString(m.pay_no)))
                        ViewBag.pay_noChg = "Y";
                    else
                        ViewBag.pay_noChg = "N";

                    if (!StringUtil.toString(d.pay_seq).Equals(StringUtil.toString(m.pay_seq)))
                        ViewBag.pay_seqChg = "Y";
                    else
                        ViewBag.pay_seqChg = "N";

                    if (!StringUtil.toString(d.re_paid_no).Equals(StringUtil.toString(m.re_paid_no)))
                        ViewBag.re_paid_noChg = "Y";
                    else
                        ViewBag.re_paid_noChg = "N";

                    if (!StringUtil.toString(d.re_paid_seq).Equals(StringUtil.toString(m.re_paid_seq)))
                        ViewBag.re_paid_seqChg = "Y";
                    else
                        ViewBag.re_paid_seqChg = "N";

                    if (!StringUtil.toString(d.re_paid_check_no).Equals(StringUtil.toString(m.re_paid_check_no)))
                        ViewBag.re_paid_check_noChg = "Y";
                    else
                        ViewBag.re_paid_check_noChg = "N";

                    if (!StringUtil.toString(d.rt_system).Equals(StringUtil.toString(m.rt_system)))
                        ViewBag.rt_systemChg = "Y";
                    else
                        ViewBag.rt_systemChg = "N";

                    if (!StringUtil.toString(d.rt_policy_no).Equals(StringUtil.toString(m.rt_policy_no)))
                        ViewBag.rt_policy_noChg = "Y";
                    else
                        ViewBag.rt_policy_noChg = "N";

                    if (!StringUtil.toString(d.rt_policy_seq).Equals(StringUtil.toString(m.rt_policy_seq)))
                        ViewBag.rt_policy_seqChg = "Y";
                    else
                        ViewBag.rt_policy_seqChg = "N";

                    if (!StringUtil.toString(d.rt_id_dup).Equals(StringUtil.toString(m.rt_id_dup)))
                        ViewBag.rt_id_dupChg = "Y";
                    else
                        ViewBag.rt_id_dupChg = "N";

                    if (!StringUtil.toString(d.re_bank_code).Equals(StringUtil.toString(m.re_bank_code)))
                        ViewBag.re_bank_codeChg = "Y";
                    else
                        ViewBag.re_bank_codeChg = "N";

                    if (!StringUtil.toString(d.re_sub_bank).Equals(StringUtil.toString(m.re_sub_bank)))
                        ViewBag.re_sub_bankChg = "Y";
                    else
                        ViewBag.re_sub_bankChg = "N";

                    if (!StringUtil.toString(d.re_bank_account).Equals(StringUtil.toString(m.re_bank_account)))
                        ViewBag.re_bank_accountChg = "Y";
                    else
                        ViewBag.re_bank_accountChg = "N";

                    if (!StringUtil.toString(d.re_paid_id).Equals(StringUtil.toString(m.re_paid_id)))
                        ViewBag.re_paid_idChg = "Y";
                    else
                        ViewBag.re_paid_idChg = "N";


                    if (!StringUtil.toString(d.o_paid_cd).Equals(StringUtil.toString(m.o_paid_cd)))
                        ViewBag.o_paid_cdChg = "Y";
                    else
                        ViewBag.o_paid_cdChg = "N";

                    //modify by daiyu 20201027
                    try
                    {
                        DateTime d_re_paid_date_n = Convert.ToDateTime(d.re_paid_date_n); 
                        DateTime m_re_paid_date_n = Convert.ToDateTime(m.re_paid_date_n);

                        if(d_re_paid_date_n.CompareTo(m_re_paid_date_n) == 0)
                            ViewBag.re_paid_date_nChg = "N";
                        else
                            ViewBag.re_paid_date_nChg = "Y";
                    }
                    catch {
                        if (!StringUtil.toString(d.re_paid_date_n).Equals(StringUtil.toString(m.re_paid_date_n)))
                            ViewBag.re_paid_date_nChg = "Y";
                        else
                            ViewBag.re_paid_date_nChg = "N";
                    }


                    try
                    {
                        DateTime d_re_paid_date = Convert.ToDateTime(d.re_paid_date);
                        DateTime m_re_paid_date = Convert.ToDateTime(re_paid_date);

                        if (d_re_paid_date.CompareTo(m_re_paid_date) == 0)
                            ViewBag.re_paid_date_Chg = "N";
                        else
                            ViewBag.re_paid_date_Chg = "Y";
                    }
                    catch
                    {
                        if (!StringUtil.toString(re_paid_date).Equals(StringUtil.toString(d.re_paid_date)))
                            ViewBag.re_paid_date_Chg = "Y";
                        else
                            ViewBag.re_paid_date_Chg = "N";
                    }
                    //end modify 20201027


                }
            }

        }



        /// <summary>
        /// 查詢支票的保單明細
        /// </summary>
        /// <param name="aplyNo"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult qryPoliHis(String aplyNo)
        {
            FAPPpaaDHisDao fAPPpaaDHisDao = new FAPPpaaDHisDao();

            try
            {
                List<OAP0002PoliModel> rows = fAPPpaaDHisDao.qryAplyNo(aplyNo);

                var jsonData = new { success = true, rows };
                return Json(jsonData, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                logger.Error("其它錯誤：" + e.ToString());
                return Json(new { success = false, err = "其它錯誤，請洽系統管理員!!" });
            }


        }




        /// <summary>
        /// 核可/退回
        /// </summary>
        /// <param name="model"></param>
        /// <param name="apprStat"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult execSave(OAP0002Model model, string apprStat)
        {
            string strConn = DbUtil.GetDBFglConnStr();
            using (SqlConnection conn = new SqlConnection(strConn))
            {
                conn.Open();
                SqlTransaction transaction = conn.BeginTransaction("Transaction");

                try
                {
                    FAPAplyRecDao fAPAplyRecDao = new FAPAplyRecDao();
                    FAP_APLY_REC fapAplyRec = fAPAplyRecDao.qryByKey(model.aplyNo);

                    if (StringUtil.toString(fapAplyRec.create_id).Equals(Session["UserID"].ToString()))
                        return Json(new { success = false, errors = "申請人與覆核人員不可相同!!" }, JsonRequestBehavior.AllowGet);

                    DateTime dt = DateTime.Now;

                    //異動覆核資料檔
                    fapAplyRec.aply_no = model.aplyNo;
                    fapAplyRec.appr_stat = apprStat;
                    fapAplyRec.appr_id = Session["UserID"].ToString();
                    fapAplyRec.approve_datetime = dt;
                    fapAplyRec.update_id = Session["UserID"].ToString();
                    fapAplyRec.update_datetime = dt;

                    

                    //核可
                    if ("2".Equals(apprStat))
                    {
                        using (EacConnection conn400 = new EacConnection(CommonUtil.GetEasycomConn()))
                        {
                            conn400.Open();
                            EacTransaction transaction400 = conn400.BeginTransaction();

                            try
                            {
                                string msg = procAS400DB(model.aplyNo, dt, conn400, transaction400, conn, transaction);

                                if ("".Equals(msg))
                                {
                                    fAPAplyRecDao.updateStatus(fapAplyRec, conn, transaction);

                                    transaction.Commit();
                                    transaction400.Commit();

                                    return Json(new { success = true });
                                }
                                else
                                {
                                    transaction.Rollback();
                                    transaction400.Rollback();
                                    logger.Error("其它錯誤：" + msg);
                                    return Json(new { success = false, errors = msg }, JsonRequestBehavior.AllowGet);
                                }

                            }
                            catch (Exception e)
                            {
                                transaction.Rollback();
                                transaction400.Rollback();
                                logger.Error("其它錯誤：" + e.ToString());
                                return Json(new { success = false, errors = "其它錯誤，請洽系統管理員!!" }, JsonRequestBehavior.AllowGet);
                            }
                        }
                    }
                    else {
                        fAPAplyRecDao.updateStatus(fapAplyRec, conn, transaction);

                        transaction.Commit();
                        return Json(new { success = true });
                    }
                }
                catch (Exception e)
                {
                    transaction.Rollback();
                    logger.Error("其它錯誤：" + e.ToString());

                    return Json(new { success = false, errors = "其它錯誤，請洽系統管理員!!" }, JsonRequestBehavior.AllowGet);
                }
            }
        }



        private string procAS400DB(string aplyNo, DateTime dt, EacConnection conn400, EacTransaction transaction400
            , SqlConnection conn, SqlTransaction transaction) {

            string msg = "";
            string strDt = DateUtil.getCurChtDateTime().Split(' ')[0];

            try
            {
                //查詢異動後的資料(smmary)
                FAPPpaaSHisDao fAPPpaaSHisDao = new FAPPpaaSHisDao();
                List<OAP0002Model> rows = fAPPpaaSHisDao.qryCheckByStat("1", "", "", aplyNo);
                OAP0002Model scModel = rows[0];

                //查詢AS400的資料(異動前)
                OAP0002Model AS400model = new OAP0002Model();
                FMNPPAADao fMNPPAADao = new FMNPPAADao();
                List<FMNPPAAModel> ppaaList = fMNPPAADao.qryForOAP0002(conn400, scModel.check_acct_short, scModel.check_no);
                if (ppaaList.Count > 0)
                    ObjectUtil.CopyPropertiesTo(ppaaList[0], AS400model);
                else
                    return "AS400查無對應資料!!";

                FMNPPADDao fMNPPADDao = new FMNPPADDao();
                if ("D".Equals(scModel.exec_action))
                {
                    int cnt = fMNPPAADao.delForOAP0002(AS400model, conn400, transaction400);
                    writePiaLog(scModel.check_no, cnt, scModel.paid_id, "D", "FMNPPAA0");

                    fMNPPADDao.delForOAP0002(ppaaList, conn400, transaction400);
                }
                else {
                    int cnt = 0;



                    //處理異動後的資料(保單明細)
                    FAPPpaaDHisDao fAPPpaaDHisDao = new FAPPpaaDHisDao();
                    List<OAP0002PoliModel> policyRows = fAPPpaaDHisDao.qryAplyNo(aplyNo);
                    int cntD = 0;
                    foreach (OAP0002PoliModel d in policyRows)
                    {
                        FAP_PPAA_D_HIS ppaaD = new FAP_PPAA_D_HIS();
                        string[] key = d.temp_id.Split('|');
                        ppaaD.system = key[0];
                        ppaaD.source_op = key[1];
                        ppaaD.policy_no = key[2];
                        ppaaD.policy_seq = (int)(key[3] == null ? (int?)null : Convert.ToInt32(key[3]));
                        ppaaD.id_dup = key[4];
                        ppaaD.member_id = key[5];
                        ppaaD.change_id = key[6];
                        ppaaD.paid_id = key[7];
                        ppaaD.check_no = key[8];

                        ppaaD.policy_no_aft = d.policy_no;
                        ppaaD.policy_seq_aft = (int)(d.policy_seq == null ? (int?)null : Convert.ToInt32(d.policy_seq));
                        ppaaD.id_dup_aft = d.id_dup;
                        ppaaD.member_id_aft = d.member_id;
                        ppaaD.change_id_aft = d.change_id;

                        if (!StringUtil.toString(ppaaD.policy_no).Equals(StringUtil.toString(ppaaD.policy_no_aft))
                            || ppaaD.policy_seq.CompareTo(ppaaD.policy_seq_aft) != 0
                            || !StringUtil.toString(ppaaD.id_dup).Equals(StringUtil.toString(ppaaD.id_dup_aft))
                            || !StringUtil.toString(ppaaD.member_id).Equals(StringUtil.toString(ppaaD.member_id_aft))
                            || !StringUtil.toString(ppaaD.change_id).Equals(StringUtil.toString(ppaaD.change_id_aft))
                            )
                        {
                            cntD++;
                            fMNPPAADao.updForOAP0002Policy(ppaaD, AS400model.system, AS400model.source_op, AS400model.paid_id, AS400model.check_no
                                , scModel.create_id, strDt, conn400, transaction400);
                        }
                    }



                    //異動PPAA
                    if (!StringUtil.toString(scModel.status).Equals(StringUtil.toString(AS400model.status))
                        || !StringUtil.toString(scModel.filler_10).Equals(StringUtil.toString(AS400model.filler_10))
                        || !StringUtil.toString(scModel.filler_14).Equals(StringUtil.toString(AS400model.filler_14))
                        || !StringUtil.toString(scModel.paid_id).Equals(StringUtil.toString(AS400model.paid_id))
                        || !StringUtil.toString(scModel.re_paid_type).Equals(StringUtil.toString(AS400model.re_paid_type))
                        || !StringUtil.toString(scModel.area).Equals(StringUtil.toString(AS400model.area))
                        || !StringUtil.toString(scModel.srce_from).Equals(StringUtil.toString(AS400model.srce_from))
                        || !StringUtil.toString(scModel.source_kind).Equals(StringUtil.toString(AS400model.source_kind))
                        || !StringUtil.toString(scModel.pay_no).Equals(StringUtil.toString(AS400model.pay_no))
                        || !StringUtil.toString(scModel.pay_seq).Equals(StringUtil.toString(AS400model.pay_seq))
                        || !StringUtil.toString(scModel.re_paid_no).Equals(StringUtil.toString(AS400model.re_paid_no))
                        || !StringUtil.toString(scModel.re_paid_seq).Equals(StringUtil.toString(AS400model.re_paid_seq))
                        || !StringUtil.toString(scModel.re_paid_check_no).Equals(StringUtil.toString(AS400model.re_paid_check_no))
                        || !StringUtil.toString(scModel.rt_system).Equals(StringUtil.toString(AS400model.rt_system))
                        || !StringUtil.toString(scModel.rt_policy_no).Equals(StringUtil.toString(AS400model.rt_policy_no))
                        || !StringUtil.toString(scModel.rt_policy_seq).Equals(StringUtil.toString(AS400model.rt_policy_seq))
                        || !StringUtil.toString(scModel.rt_id_dup).Equals(StringUtil.toString(AS400model.rt_id_dup))
                        || !StringUtil.toString(scModel.re_bank_code).Equals(StringUtil.toString(AS400model.re_bank_code))
                        || !StringUtil.toString(scModel.re_sub_bank).Equals(StringUtil.toString(AS400model.re_sub_bank))
                        || !StringUtil.toString(scModel.re_bank_account).Equals(StringUtil.toString(AS400model.re_bank_account))
                        || !StringUtil.toString(scModel.re_paid_id).Equals(StringUtil.toString(AS400model.re_paid_id))
                        //|| !StringUtil.toString(scModel.re_paid_date).Equals(StringUtil.toString(AS400model.re_paid_date))
                        || !StringUtil.toString(scModel.re_paid_date_n).Equals(StringUtil.toString(AS400model.re_paid_date_n))  //modify by daiyu 20201027
                        || !StringUtil.toString(scModel.o_paid_cd).Equals(StringUtil.toString(AS400model.o_paid_cd))
                        )
                    {
                        cnt = fMNPPAADao.updForOAP0002Summary(scModel, scModel.create_id, strDt, conn400, transaction400);
                    }

                    

                    if (cnt == 0)
                        writePiaLog(scModel.check_no, cntD, scModel.paid_id, "E", "FMNPPAA0");
                    else 
                        writePiaLog(scModel.check_no, cnt, scModel.paid_id, "E", "FMNPPAA0");


                    //MNPPAA0.STATUS 由空白/ 1改為2 / 3 / 4 / 6 要把FMNPPAD0內該張支票的所有FMNPPAD0.R_STATUS都異動為6
                    if (("".EndsWith(StringUtil.toString(AS400model.status)) || "1".EndsWith(StringUtil.toString(AS400model.status)))
                        & ("2".EndsWith(StringUtil.toString(scModel.status)) || "3".EndsWith(StringUtil.toString(scModel.status))
                        || "4".EndsWith(StringUtil.toString(scModel.status)) || "6".EndsWith(StringUtil.toString(scModel.status))
                        ))
                    {
                        fMNPPADDao.updForOAP0002Policy(ppaaList, scModel.create_id, strDt, conn400, transaction400);
                    }

                    //add by daiyu 20201028 若"給付帳務日"有異動，更新OPEN的清理紀錄檔
                    FAPVeTraceDao fAPVeTraceDao = new FAPVeTraceDao();
                    FAP_VE_TRACE trace = fAPVeTraceDao.qryByCheckNo(AS400model.check_no, AS400model.check_acct_short);
                    string re_paid_date = "";
                    if (trace.re_paid_date != null) 
                    {
                        re_paid_date = DateUtil.DatetimeToString(trace.re_paid_date, "yyyy/MM/dd");
                    }


                    //20190828 配合 201905310556-03 逾期未兌領清理計畫加寫清理歷程檔
                    if (!StringUtil.toString(scModel.paid_id).Equals(StringUtil.toString(AS400model.paid_id))
                        || !StringUtil.toString(scModel.re_paid_date).Equals(StringUtil.toString(re_paid_date))
                        || !StringUtil.toString(scModel.re_paid_date_n).Equals(StringUtil.toString(AS400model.re_paid_date_n))
                        )
                        procVeTrace(AS400model.check_acct_short, AS400model.check_no, scModel.paid_id, scModel.re_paid_date, scModel.re_paid_date_n, conn, transaction);

                    //end add 20201028

                }


            }
            catch (Exception e) {
                throw e;

            }

            return msg;

        }


        private void procVeTrace(string check_acct_short, string check_no, string paid_id, string re_paid_date, string re_paid_date_n
            , SqlConnection conn, SqlTransaction transaction) {

            FAPVeTraceDao fAPVeTraceDao = new FAPVeTraceDao();
            FAPVeTrackProcDao fAPVeTrackProcDao = new FAPVeTrackProcDao();

            fAPVeTraceDao.updateForOAP0002(check_acct_short, check_no, paid_id, re_paid_date, re_paid_date_n, conn, transaction);
            fAPVeTrackProcDao.updateForOAP0002(check_acct_short, check_no, paid_id, conn, transaction);
        }


        private void writePiaLog(string checkNo, int affectRows, string piaOwner, string executionType, string accessobjName)
        {
            PIA_LOG_MAIN piaLogMain = new PIA_LOG_MAIN();
            piaLogMain.TRACKING_TYPE = "A";
            piaLogMain.ACCESS_ACCOUNT = Session["UserID"].ToString();
            piaLogMain.ACCOUNT_NAME = Session["UserName"].ToString();
            piaLogMain.PROGFUN_NAME = "OAP0002AController";
            piaLogMain.EXECUTION_CONTENT = checkNo;
            piaLogMain.AFFECT_ROWS = affectRows;
            piaLogMain.PIA_TYPE = "0100100000";
            piaLogMain.EXECUTION_TYPE = executionType;
            piaLogMain.ACCESSOBJ_NAME = accessobjName;
            piaLogMain.PIA_OWNER1 = piaOwner;
            PiaLogMainDao piaLogMainDao = new PiaLogMainDao();
            piaLogMainDao.Insert(piaLogMain);

        }


    }
}