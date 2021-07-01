using FAP.Web.ActionFilter;
using FAP.Web.BO;
using FAP.Web.Daos;
using FAP.Web.ViewModels;
using System;
using System.Collections.Generic;
using System.Web.Mvc;
using System.Text;
using System.Linq;
using System.Data.SqlClient;
using FAP.Web.Models;
using System.Threading.Tasks;
using System.Data.EasycomClient;
using FAP.Web.AS400PGM;
using FAP.Web.AS400Models;

/// <summary>
/// 功能說明：OAP0042A 電訪暨簡訊標準設定作業
/// 初版作者：20200813 Daiyu
/// 修改歷程：20200813 Daiyu
/// 需求單號：
/// 修改內容：初版
/// ----------------------------------------------------
/// 修改歷程：20210208 daiyu
/// 需求單號：202101280283-00
/// 修改內容：設定項目屬"電話訪問"時，增加可篩選"支票號碼"或"給付對象ID"挑錄特定的支票。
/// </summary>
///

namespace FAP.Web.Controllers
{
    [Authorize]
    [CheckSessionFilterAttribute]
    public class OAP0042AController : BaseController
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
            //string funcName = "";

            if (Session["UserID"] != null)
            {
                String[] roleInfo = authUtil.chkUserFuncAuth(Session["UserID"].ToString(), "~/OAP0042A/");
                if (roleInfo != null && roleInfo.Length == 2)
                {
                    opScope = roleInfo[0];
                    funcName = roleInfo[1];
                }
            }

            ViewBag.opScope = opScope;
            ViewBag.funcName = funcName;


            return View();

        }


        /// <summary>
        /// 查詢待覆核的資料
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public JsonResult LoadData()
        {
            try
            {
                SysParaHisDao sysParaHisDao = new SysParaHisDao();
                List<SYS_PARA_HIS> onApprList = sysParaHisDao.qryForGrpId("AP", new string[] { "tel_assign_case", "sms_assign_case", "sms_notify_case" }, "1");

                List<OAP0042Model> rows = onApprList
                    .Where(x => x.APLY_NO != "")
                    .GroupBy(x => new { x.APLY_NO, x.GRP_ID, x.CREATE_UID, x.CREATE_DT })
                    .Select(group => new OAP0042Model
                    {
                        aply_no = group.Key.APLY_NO,
                        type = group.Key.GRP_ID,
                        create_dt = DateUtil.DatetimeToString(group.Key.CREATE_DT, "yyyy-MM-dd HH:mm:ss"),
                        create_id = group.Key.CREATE_UID
                    }).ToList<OAP0042Model>();


                Dictionary<string, ADModel> userNameMap = new Dictionary<string, ADModel>();
                string usr_id = "";
                CommonUtil commonUtil = new CommonUtil();


                SysCodeDao sysCodeDao = new SysCodeDao();
                Dictionary<string, string> typeMap = sysCodeDao.qryByTypeDic("AP", "OAP0042_TYPE");

                foreach (OAP0042Model d in rows)
                {

                    //取得申請人姓名
                    usr_id = StringUtil.toString(d.create_id);
                    if (!"".Equals(usr_id))
                    {
                        if (!userNameMap.ContainsKey(usr_id))
                        {
                            ADModel adModel = new ADModel();
                            adModel = commonUtil.qryEmp(usr_id);
                            userNameMap.Add(usr_id, adModel);
                        }
                        d.create_id = d.create_id + " " + userNameMap[usr_id].name;
                    }

                    if (typeMap.ContainsKey(d.type))
                        d.type_desc = typeMap[d.type];

                }


                var jsonData = new { success = true, rows };
                return Json(jsonData, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                logger.Error(e.ToString);
                var jsonData = new { success = false, err = e.ToString() };
                return Json(jsonData, JsonRequestBehavior.AllowGet);
            }
        }


        /// <summary>
        /// 開啟修改明細畫面
        /// </summary>
        /// <param name="aplyNo"></param>
        /// <returns></returns>
        public ActionResult detailAply(string aply_no, string type)
        {
            ViewBag.funcName = funcName;


            FPMCODEDao fPMCODEDao = new FPMCODEDao();
            SysCodeDao sysCodeDao = new SysCodeDao();
            FAPVeCodeDao fAPVeCodeDao = new FAPVeCodeDao();

            //設定項目
            ViewBag.typeList = sysCodeDao.loadSelectList("AP", "OAP0042_TYPE", false);

            //資料狀態
            ViewBag.dataStatusList = sysCodeDao.loadSelectList("AP", "DATA_STATUS", true);

            //清理狀態
            ViewBag.clrStatusList = sysCodeDao.loadSelectList("AP", "CLR_STATUS", true);

            //主檔狀態
            ViewBag.ppssStatusList = fPMCODEDao.qryGrpList("PPAASTATUS", "AP");

            //原給付性質
            //ViewBag.oPaidCdList = sysCodeDao.loadSelectList("AP", "O_PAID_CD", true);
            ViewBag.oPaidCdList = fPMCODEDao.qryGrpList("PAID_CDTXT", "AP");

            //覆核結果
            ViewBag.telApprCodeList = sysCodeDao.loadSelectList("AP", "TEL_APPR_CODE", true);

            //簡訊狀態
            ViewBag.smsStatusList = sysCodeDao.loadSelectList("AP", "SMS_STATUS", true);

            //保局範圍
            ViewBag.fscRangeList = fAPVeCodeDao.loadSelectList("FSC_RANGE", true);

            OAP0042Model model = new OAP0042Model();

            try
            {

                SysParaHisDao sysParaHisDao = new SysParaHisDao();
                List<SYS_PARA_HIS> hisList = new List<SYS_PARA_HIS>();
                hisList = sysParaHisDao.qryForGrpId("AP", new string[] { type }, "1");

                if (hisList.Count > 0)
                {

                    model.aply_no = aply_no;
                    model.create_dt = DateUtil.DatetimeToString(hisList[0].CREATE_DT, "yyyy-MM-dd HH:mm:ss");

                    CommonUtil commonUtil = new CommonUtil();
                    ADModel adModel = new ADModel();
                    adModel = commonUtil.qryEmp(hisList[0].CREATE_UID);
                    if (adModel != null)
                        model.create_id = hisList[0].CREATE_UID + " " + adModel.name;


                    if ("sms_notify_case".Equals(type))
                    {
                        ViewBag.bAS400F = StringUtil.toString(hisList[0].RESERVE1);
                    }
                }

                

                foreach (SYS_PARA_HIS d in hisList)
                {
                    switch (d.PARA_ID)
                    {
                        case "appr_code":   //覆核結果
                            model.appr_code = StringUtil.toString(d.PARA_VALUE);
                            break;
                        case "assign_month":    //未派件月份
                            model.assign_month = StringUtil.toString(d.PARA_VALUE);
                            break;
                        case "clr_status":  //清理狀態
                            model.clr_status = StringUtil.toString(d.PARA_VALUE);
                            break;
                        case "fsc_range":   //保局範圍
                            model.fsc_range = StringUtil.toString(d.PARA_VALUE);
                            break;
                        case "o_paid_cd":   //原給付性質
                            model.o_paid_cd = StringUtil.toString(d.PARA_VALUE);
                            break;
                        case "ppaa_status": //主檔狀態
                            model.ppaa_status = StringUtil.toString(d.PARA_VALUE);
                            break;
                        case "rpt_cnt_tp":  //計算條件(P:給付對象ID、C:支票號碼)
                            model.rpt_cnt_tp = StringUtil.toString(d.PARA_VALUE);
                            break;
                        case "stat_amt":    //歸戶金額(起訖)
                            if (!"".Equals(StringUtil.toString(d.PARA_VALUE)))
                            {
                                string[] amtArr = StringUtil.toString(d.PARA_VALUE).Split('|');
                                model.stat_amt_b = amtArr[0];
                                model.stat_amt_e = amtArr[1];
                            }
                            break;
                        case "sms_status":  //簡訊狀態
                            model.sms_status = StringUtil.toString(d.PARA_VALUE);
                            break;
                        case "sms_clear_month": //簡訊清除月份
                            model.sms_clear_month = StringUtil.toString(d.PARA_VALUE);
                            break;
                        case "check_date":  //支票到期日(起訖)
                            if (!"".Equals(StringUtil.toString(d.PARA_VALUE)))
                            {
                                string[] dateArr = StringUtil.toString(d.PARA_VALUE).Split('|');
                                model.check_date_b = dateArr[0];
                                model.check_date_e = dateArr[1];
                            }
                            break;
                        case "check_no": //支票號碼     add by daiyu 20210208
                            model.check_no = StringUtil.toString(d.PARA_VALUE);
                            break;
                        case "paid_id": //給付對象ID     add by daiyu 20210208
                            model.paid_id = StringUtil.toString(d.PARA_VALUE);
                            break;
                    }
                }

                ViewBag.aply_no = aply_no;
                ViewBag.create_id = model.create_id;


                ViewBag.bHaveData = "Y";
                return View(model);




            }
            catch (Exception e)
            {
                ViewBag.bHaveData = "N";
                return View(model);
            }
        }



        /// <summary>
        /// 核可/退回
        /// </summary>
        /// <param name="model"></param>
        /// <param name="apprStat"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult execSave(OAP0042Model model, string apprStat)
        {

            if (StringUtil.toString(model.create_id).StartsWith(Session["UserID"].ToString()))
                return Json(new { success = false, err = "申請人與覆核人員不可相同!!" }, JsonRequestBehavior.AllowGet);

            SysParaHisDao sysParaHisDao = new SysParaHisDao();
            List<SYS_PARA_HIS> hisList = new List<SYS_PARA_HIS>();
            hisList = sysParaHisDao.qryForGrpId("AP", new string[] { model.type }, "1");

            if (hisList.Count > 0)
            {
                if (StringUtil.toString(hisList[0].CREATE_UID).EndsWith(Session["UserID"].ToString()))
                    return Json(new { success = false, err = "申請人與覆核人員不可相同!!" }, JsonRequestBehavior.AllowGet);
            }


            string strConn = DbUtil.GetDBFglConnStr();
            using (SqlConnection conn = new SqlConnection(strConn))
            {
                conn.Open();
                SqlTransaction transaction = conn.BeginTransaction("Transaction");

                try
                {
                    List<SYS_PARA_HIS> data = new List<SYS_PARA_HIS>();
                    SYS_PARA_HIS d = new SYS_PARA_HIS();
                    d.APLY_NO = model.aply_no;
                    d.SYS_CD = "AP";
                    d.GRP_ID = model.type;
                    d.APPR_STATUS = apprStat;
                    d.APPR_UID = Session["UserID"].ToString();
                    data.Add(d);


                    //處理駁回資料
                    if ("3".Equals(StringUtil.toString(apprStat)))
                    {
                        sysParaHisDao.updateApprStatus(Session["UserID"].ToString(), apprStat, data, conn, transaction);
                    }



                    //處理核可資料
                    if ("2".Equals(StringUtil.toString(apprStat)))
                    {
                        //異動歷史檔
                        sysParaHisDao.updateApprStatus(Session["UserID"].ToString(), apprStat, data, conn, transaction);

                        //異動正式檔
                        SysParaDao sysParaDao = new SysParaDao();
                        sysParaDao.updForOAP0042ByHis(model.aply_no, model.type, conn, transaction);

                        //將本次框列的資料新增到電訪/簡訊名單
                        if ("sms_notify_case".Equals(model.type)) {
                            Task.Run(() => writeTelSmsNotifyCheck(model.aply_no, model, Session["UserID"].ToString()));
                        }
                        else
                        {
                            Task.Run(() => writeTelCheck(model.aply_no, model, Session["UserID"].ToString()));
                        }
                        
                    }


                    transaction.Commit();




                    return Json(new { success = true });
                }
                catch (Exception e)
                {
                    transaction.Rollback();

                    logger.Error("[execSave]其它錯誤：" + e.ToString());

                    return Json(new { success = false, err = "其它錯誤，請洽系統管理員!!" }, JsonRequestBehavior.AllowGet);

                }
            }
        }


        /// <summary>
        /// 將電訪、簡訊名單寫到table
        /// </summary>
        /// <param name="aply_no"></param>
        /// <param name="model"></param>
        /// <param name="usr_id"></param>
        /// <returns></returns>
        private async Task writeTelCheck(string aply_no, OAP0042Model model, string usr_id)
        {
            await Task.Delay(1);

            try
            {
                DateTime now = DateTime.Now;

                OAP0042Controller oAP0042 = new OAP0042Controller();

                List<TelDispatchRptModel> grpByIdFscRangeList = oAP0042.getRptList("D", model);

                List<TelDispatchRptModel> qryAS400DataList = new List<TelDispatchRptModel>();

                FAPTelCheckDao fAPTelCheckDao = new FAPTelCheckDao();

                List<TelDispatchRptModel> checkList = grpByIdFscRangeList.GroupBy(o => new { o.system, o.check_acct_short, o.check_no, o.sec_stat, o.fsc_range, o.amt_range })
                        .Select(group => new TelDispatchRptModel
                        {
                            system = group.Key.system,
                            check_acct_short = group.Key.check_acct_short,
                            check_no = group.Key.check_no,
                            sec_stat = group.Key.sec_stat,
                            fsc_range = group.Key.fsc_range,
                            amt_range = group.Key.amt_range
                        }).ToList<TelDispatchRptModel>();

                //查詢否已有資料存在【FAP_TEL_CHECK 電訪支票檔】
                List<FAP_TEL_CHECK> o_check_list = new List<FAP_TEL_CHECK>();
                foreach (TelDispatchRptModel d in checkList)
                {
                    FAP_TEL_CHECK tel_check_o = fAPTelCheckDao.qryByCheckNo(d.check_no, d.check_acct_short, model.type);
                    o_check_list.Add(tel_check_o);
                }
                    


                string strConn = DbUtil.GetDBFglConnStr();
                using (SqlConnection conn = new SqlConnection(strConn))
                {
                    conn.Open();
                    SqlTransaction transaction = conn.BeginTransaction("Transaction");

                    string _dispatch_status = "";
                    string _sms_status = "";
                    if ("tel_assign_case".Equals(StringUtil.toString(model.type)))
                        _dispatch_status = "0";
                    else
                        _sms_status = "0";

                    


                    foreach (TelDispatchRptModel d in checkList) {
                        try
                        {
                            FAP_TEL_CHECK tel_check_n = new FAP_TEL_CHECK();
                            tel_check_n.system = d.system;
                            tel_check_n.check_no = d.check_no;
                            tel_check_n.check_acct_short = d.check_acct_short;
                            tel_check_n.tel_std_aply_no = aply_no;
                            tel_check_n.tel_std_type = model.type;
                            tel_check_n.fsc_range = d.fsc_range;
                            tel_check_n.amt_range = StringUtil.toString(d.amt_range.Split('~')[0]);
                            tel_check_n.data_flag = "Y";
                            tel_check_n.dispatch_status = _dispatch_status;
                            tel_check_n.sms_status = _sms_status;
                            tel_check_n.sec_stat = d.sec_stat;
                            tel_check_n.data_status = "1";
                            tel_check_n.update_id = usr_id;
                            tel_check_n.update_datetime = now;

                            string _data_flag = "";
                            FAP_TEL_CHECK tel_check_o = o_check_list.Where(x => x.check_acct_short == d.check_acct_short & x.check_no == d.check_no).FirstOrDefault();
                            if (tel_check_o != null)
                                _data_flag = StringUtil.toString(tel_check_o.data_flag);

                            switch (_data_flag) {
                                case "":    //沒有資料:需要從AS400把相關資料放到OPEN

                                    foreach (TelDispatchRptModel poliD in grpByIdFscRangeList.Where(x => x.system == d.system & x.check_no == d.check_no).ToList())
                                    {
                                        qryAS400DataList.Add(poliD);

                                    }

                                    break;
                                case "Y":   //存在有效資料:要將原檔上的資料設成無效
                                    fAPTelCheckDao.updDataFlag("N", tel_check_o, conn, transaction);

                                    break;
                                case "N":   //存在無效資料:不必再特別處理
                                    break;

                            }


                            //新增【FAP_TEL_CHECK 電訪支票檔】
                            fAPTelCheckDao.insert(now, tel_check_n, conn, transaction);

                            

                        }
                        catch (Exception ex) {
                            logger.Error("check_no:" + d.check_no + " error:" + ex.ToString());

                        }
                    }


                    //刪除【FAP_TEL_CHECK 電訪支票檔】
                    FAPTelCheckHisDao fAPTelCheckHisDao = new FAPTelCheckHisDao();
                    fAPTelCheckHisDao.delForOAP0042(model.type, conn, transaction);



                    

                    transaction.Commit();
                }



                //處理需自AS400查詢出的資料
                await qryAS400PoliDataAsync(qryAS400DataList, model.type);

                logger.Info("writeTelCheck end!!");


            }
            catch (Exception e)
            {
                
            }
        }

        private async Task writeTelSmsNotifyCheck(string aply_no, OAP0042Model model, string usr_id)
        {
            await Task.Delay(1);

            try
            {
                DateTime now = DateTime.Now;

                FAPTelSmsTempDao fAPTelSmsTempDao = new FAPTelSmsTempDao();
                List<TelDispatchRptModel> dataList = fAPTelSmsTempDao.qrySmsNotifyRpt(model.rpt_cnt_tp, "");

                List<TelDispatchRptModel> idCheckList = dataList.GroupBy(o => new { o.temp_id, o.system, o.check_no, o.check_acct_short, o.check_amt, o.fsc_range, o.sec_stat })
            .Select(group => new TelDispatchRptModel
            {
                temp_id = group.Key.temp_id,
                system = group.Key.system,
                check_no = group.Key.check_no,
                check_acct_short = group.Key.check_acct_short,
                fsc_range = group.Key.fsc_range,
                check_amt = group.Key.check_amt,
                sec_stat = group.Key.sec_stat
            }).ToList<TelDispatchRptModel>();


                List<TelDispatchRptModel> idList = idCheckList.GroupBy(o => new { o.temp_id }).Select(group => new TelDispatchRptModel
                {
                    temp_id = group.Key.temp_id,
                    amt = group.Sum(x => Convert.ToInt64(x.check_amt))
                }).ToList<TelDispatchRptModel>();


                string[] idArr = idList.Where(x => x.amt >= Convert.ToInt64(model.stat_amt_b) & x.amt <= Convert.ToInt64(model.stat_amt_e))
                .ToList<TelDispatchRptModel>().Select(x => x.temp_id).ToArray();


                List<string> amt_range_list = new List<string>();
                //查詢級距
                FAPVeCodeDao fAPVeCodeDao = new FAPVeCodeDao();
                List<FAP_VE_CODE> rangeRows = fAPVeCodeDao.qryByGrp("TEL_RANGE");

                foreach (FAP_VE_CODE range in rangeRows.OrderByDescending(x => x.code_id.Length).ThenByDescending(x => x.code_id))
                {
                    decimal amt_range = StringUtil.toString(range.code_id) == "" ? 0 : Convert.ToInt64(range.code_id);
                    string amt_range_desc = (StringUtil.toString(range.code_id) == "" ? "0" : StringUtil.toString(range.code_id)) + " ~ " + StringUtil.toString(range.code_value);
                    amt_range_list.Add(amt_range_desc);
                    long range_l = StringUtil.toString(range.code_id) == "" ? 0 : Convert.ToInt64(range.code_id);
                    long range_h = StringUtil.toString(range.code_value) == "" ? 999999999999 : Convert.ToInt64(range.code_value);

                    idList.Where(x => x.amt >= range_l & x.amt <= range_h).Select(x => { x.amt_range = amt_range.ToString(); return x; }).ToList();
                    idList.Where(x => x.amt >= range_l & x.amt <= range_h).Select(x => { x.amt_range_desc = amt_range_desc; return x; }).ToList();
                }

                foreach (TelDispatchRptModel id in idList)
                {
                    idCheckList.Where(x => x.temp_id == id.temp_id).Select(x => { x.amt_range = id.amt_range.ToString(); return x; }).ToList();
                    idCheckList.Where(x => x.temp_id == id.temp_id).Select(x => { x.amt_range_desc = id.amt_range_desc.ToString(); return x; }).ToList();
                }


               // List<TelDispatchRptModel> qryAS400DataList = new List<TelDispatchRptModel>();

                FAPTelCheckDao fAPTelCheckDao = new FAPTelCheckDao();

                string strConn = DbUtil.GetDBFglConnStr();
                using (SqlConnection conn = new SqlConnection(strConn))
                {
                    conn.Open();
                    SqlTransaction transaction = conn.BeginTransaction("Transaction");

                    string _dispatch_status = "";
                    string _sms_status = "";
                    if ("tel_assign_case".Equals(StringUtil.toString(model.type)))
                        _dispatch_status = "0";
                    else
                        _sms_status = "0";

                    //List<TelDispatchRptModel> checkList = grpByIdFscRangeList.GroupBy(o => new { o.system, o.check_acct_short, o.check_no, o.sec_stat, o.fsc_range, o.amt_range })
                    //    .Select(group => new TelDispatchRptModel
                    //    {
                    //        system = group.Key.system,
                    //        check_acct_short = group.Key.check_acct_short,
                    //        check_no = group.Key.check_no,
                    //        sec_stat = group.Key.sec_stat,
                    //        fsc_range = group.Key.fsc_range,
                    //        amt_range = group.Key.amt_range
                    //    }).ToList<TelDispatchRptModel>();


                    foreach (TelDispatchRptModel d in idCheckList.Where(x => idArr.Contains(x.temp_id)).ToList())
                    {
                        try
                        {

                            FAP_TEL_CHECK tel_check_n = new FAP_TEL_CHECK();
                            tel_check_n.system = d.system;
                            tel_check_n.check_no = d.check_no;
                            tel_check_n.check_acct_short = d.check_acct_short;
                            tel_check_n.tel_std_aply_no = aply_no;
                            tel_check_n.tel_std_type = model.type;
                            tel_check_n.fsc_range = d.fsc_range;
                            tel_check_n.amt_range = StringUtil.toString(d.amt_range.Split('~')[0]);
                            tel_check_n.data_flag = "Y";
                            tel_check_n.dispatch_status = _dispatch_status;
                            tel_check_n.sms_status = _sms_status;
                            tel_check_n.sec_stat = d.sec_stat;
                            tel_check_n.data_status = "1";
                            tel_check_n.update_id = usr_id;
                            tel_check_n.update_datetime = now;


                            //查詢否已有資料存在【FAP_TEL_CHECK 電訪支票檔】
                            FAP_TEL_CHECK tel_check_o = fAPTelCheckDao.qryByCheckNo(d.check_no, d.check_acct_short, model.type);

                            switch (StringUtil.toString(tel_check_o.data_flag))
                            {
                                case "":    //沒有資料:需要從AS400把相關資料放到OPEN

                                    //foreach (TelDispatchRptModel poliD in grpByIdFscRangeList.Where(x => x.system == d.system & x.check_no == d.check_no).ToList())
                                    //{
                                    //    qryAS400DataList.Add(poliD);

                                    //}

                                    break;
                                case "Y":   //存在有效資料:要將原檔上的資料設成無效
                                    fAPTelCheckDao.updDataFlag("N", tel_check_o, conn, transaction);

                                    break;
                                case "N":   //存在無效資料:不必再特別處理
                                    break;

                            }


                            //新增【FAP_TEL_CHECK 電訪支票檔】
                            fAPTelCheckDao.insert(now, tel_check_n, conn, transaction);


                            //異動[FAP_TEL_SMS_TEMP]
                            if ("sms_notify_case".Equals(StringUtil.toString(model.type)))
                                fAPTelSmsTempDao.updateAplyNo(aply_no, d.check_no, d.check_acct_short, conn, transaction);

                        }
                        catch (Exception ex)
                        {
                            logger.Error("check_no:" + d.check_no + " error:" + ex.ToString());

                        }
                    }


                    //刪除【FAP_TEL_CHECK 電訪支票檔】
                    FAPTelCheckHisDao fAPTelCheckHisDao = new FAPTelCheckHisDao();
                    fAPTelCheckHisDao.delForOAP0042(model.type, conn, transaction);

                    //異動參數檔的保留欄位一，"Y"表示已經將AS400的資料拉回OPEN
                    SysParaDao sysParaDao = new SysParaDao();
                    sysParaDao.updForOAP0042(model.type, conn, transaction);

                    transaction.Commit();
                }


                logger.Info("writeTelCheck end!!");


            }
            catch (Exception e)
            {

            }
        }



        private async Task qryAS400PoliDataAsync(List<TelDispatchRptModel> qryAS400DataList, string type)
        {
            logger.Info("qryAS400PoliDataAsync begin!!");

            FMNPPADDao fMNPPADDao = new FMNPPADDao();
            FAPVeTracePoliDao fAPVeTracePoliDao = new FAPVeTracePoliDao();
            FAPTelPoliDao fAPTelPoliDao = new FAPTelPoliDao();
            string strConn = DbUtil.GetDBFglConnStr();

            //foreach (TelDispatchRptModel poliD in qryAS400DataList) {
   
            //    FAP_VE_TRACE_POLI tmp = fAPVeTracePoliDao.qryForOAP0042(poliD.check_no, poliD.check_acct_short, poliD.policy_no, poliD.policy_seq, poliD.id_dup);
            //    if (!"".Equals(StringUtil.toString(tmp.sysmark)))
            //        continue;
            //    else {
            //        EacConnection conn400 = new EacConnection(CommonUtil.GetEasycomConn());
            //        conn400.Open();


                    
            //        using (SqlConnection conn = new SqlConnection(strConn))
            //        {
            //            conn.Open();
            //            SqlTransaction transaction = conn.BeginTransaction("Transaction");

            //            try
            //            {
            //                //自PPAD查詢服務人員ID
            //                TelDispatchRptModel _poliD = fMNPPADDao.qryForOAP0042(conn400, poliD);
            //                _poliD.sysmark = poliD.sysmark;
            //                _poliD.appl_id = poliD.appl_id;
            //                _poliD.appl_name = poliD.appl_name;
            //                _poliD.ins_id = poliD.ins_id;
            //                _poliD.ins_name = poliD.ins_name;


            //                //取得服務人員手機
            //                if (!"".Equals(StringUtil.toString(_poliD.send_id)))
            //                {

            //                    if ("F".Equals(StringUtil.toString(poliD.system)) && "3".Equals(StringUtil.toString(_poliD.sysmark)))
            //                    {
            //                        LydiaUtil lydiaUtil = new LydiaUtil();
            //                        Lydia004Model lydia004Model = new Lydia004Model();
            //                        lydia004Model.agentId = _poliD.send_id;
            //                        lydia004Model = await lydiaUtil.callLydia004Async(lydia004Model);

            //                        _poliD.send_tel = StringUtil.toString(lydia004Model.mobilePhone);
            //                    }
            //                    else
            //                    {
            //                        LydiaUtil lydiaUtil = new LydiaUtil();
            //                        Lydia001Model lydia001Model = new Lydia001Model();
            //                        lydia001Model.agentId = _poliD.send_id;
            //                        lydia001Model = await lydiaUtil.callLydia001Async(lydia001Model);

            //                        _poliD.send_tel = StringUtil.toString(lydia001Model.mobileNo);
            //                    }
            //                }


            //                //異動【FAP_VE_TRACE_POLI 逾期未兌領清理保單明細檔】
            //                fAPVeTracePoliDao.updForOAP0042(_poliD, conn, transaction);


            //                //新增【FAP_TEL_POLI 保單聯絡電話檔】
                            
            //                FAP_TEL_POLI tmp_tel_poli = new FAP_TEL_POLI();
            //                ObjectUtil.CopyPropertiesTo(_poliD, tmp_tel_poli);
            //                if (fAPTelPoliDao.qryByPolicyNo(tmp_tel_poli).Count == 0) {
            //                    SAP7018Util sAP7018Util = new SAP7018Util();
            //                    SAP7018Model sAP7018Model = new SAP7018Model();

            //                    ObjectUtil.CopyPropertiesTo(_poliD, sAP7018Model);

            //                    List<SAP7018TelModel> telList = sAP7018Util.callSAP7018(conn400, sAP7018Model);

            //                    foreach (SAP7018TelModel tel in telList)
            //                    {
            //                        FAP_TEL_POLI tel_poli = new FAP_TEL_POLI();
            //                        ObjectUtil.CopyPropertiesTo(_poliD, tel_poli);

            //                        tel_poli.cust_tel = tel.tel;
            //                        tel_poli.tel_type = tel.tel_type;
            //                        tel_poli.update_datetime = Convert.ToDateTime(DateUtil.formatDateTimeDbToSc(tel.upd_date, "DT"));

            //                        fAPTelPoliDao.insert(tel_poli, conn, transaction);
            //                    }

            //                }

                           


            //                transaction.Commit();
            //            }
            //            catch (Exception ex400)
            //            {
            //                transaction.Rollback();
            //                logger.Error(ex400.ToString());
            //                //throw ex400;
            //            }
            //        }

            //        conn400.Close();
            //        conn400 = null;

            //    }
                
            //}


            //異動參數檔的保留欄位一，"Y"表示已經將AS400的資料拉回OPEN
            using (SqlConnection conn = new SqlConnection(strConn))
            {
                conn.Open();
                SqlTransaction transaction = conn.BeginTransaction("Transaction");

                try
                {
                    SysParaDao sysParaDao = new SysParaDao();
                    sysParaDao.updForOAP0042(type, conn, transaction);
                    transaction.Commit();
                }
                catch (Exception e) {
                    transaction.Rollback();
                } 
                }
                    logger.Info("qryAS400PoliDataAsync end!!");

        }

    }
}