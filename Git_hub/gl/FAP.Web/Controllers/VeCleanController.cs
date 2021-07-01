using FAP.Web.ActionFilter;
using FAP.Web.BO;
using FAP.Web.Daos;
using FAP.Web.Models;
using FAP.Web.ViewModels;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Threading;
using System.Web.Http;
using System;
using System.Threading.Tasks;
using System.Data.EasycomClient;
using System.Data.SqlClient;
using System.Linq;
using System.IO;
using ClosedXML.Excel;
using FAP.Web.AS400PGM;
using FAP.Web.AS400Models;
using Ionic.Zip;

/// <summary>
/// 功能說明：AS400 逾期未兌領清理資料轉入OPEN
/// 初版作者：20190614 Daiyu
/// 修改歷程：20190614 Daiyu
///           需求單號：201905310556-01
///           初版
/// ------------------------------------------------
/// 修改歷程：20191025 daiyu 
/// 需求單號：201910240295-00
/// 修改內容：保局範圍以"0"寫入清理計畫檔
/// ------------------------------------------------
/// 修改歷程：20200811 daiyu 
/// 需求單號：202008120153-01(第一階段)
/// 修改內容：修改介接內容
///          1.寫入清理紀錄檔時加判斷若屬無地址...要入檔，但不更新踐行程序
///          2.除了主動通知之外，若原先檔案內沒有對應旳支票資料，亦要新增至清理紀錄檔
/// ---------------------------------------------------------------------------
/// 修改歷程：20201005 daiyu 
/// 需求單號：202008120153-01(第二階段)
/// 修改內容：修改介接內容
///           1.增加一年以下簡訊通知
///           2.VETRACESTATUS
///             2.1 增加傳入"帳務日期"
///             2.2 加判斷"給付細項"
///                a.電訪給付: 若給付日小於電訪處理結果之最後一次電訪日期,則為電訪給付
///                b.追踨給付: 若給付日小於第一次追踨處理結果登錄日,則為追踨給付
///                c.戶政給付: 若已有戶政調閱記錄(清理階段為調閱完成), 則為戶政給付
///                d.正常給付: 完全無電訪日也無戶政調閱記錄者, 即為正常給付
///             2.3 異動【FAP_TEL_INTERVIEW 電訪及追踨記錄檔】、【FAP_TEL_CHECK 電訪支票檔】
///                覆核結果自動改為16給付結案,派件狀態改為”2電訪結束”
/// ------------------------------------------------
/// 修改歷程：20210125 daiyu 
/// 需求單號：
/// 修改內容：AS400所帶入寄信紀錄的過程說明有誤，直接拿掉過程說明資料
/// ------------------------------------------------
/// 修改歷程：20210517 daiyu 
/// 需求單號：202103250638-02
/// 修改內容：回壓給付帳務日時，若檔案上已有給付帳務日，不要再更新。
/// </summary>
/// 
namespace FAP.Web.Controllers
{
    [RoutePrefix("VeClean")]
    public class VeCleanController : ApiController
    {
        static private NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();


        //Bianco Edited
        [Route("SMS_NOTIFY")]
        [ValidateModel]
        public IHttpActionResult SMS_NOTIFY(SmsNotifyModel model)
        {
            string inputKey = "SMS_NOTIFY" + "|" + model.rtnCode + "|" + model.upd_date;
            logger.Info("rtn_code1:" + model.rtnCode);
            logger.Info("upd_date1:" + model.upd_date);

            switch (model.rtnCode)
            {
                case "0":
                case "1":
                    model.rtnCode = "F";
                    break;
                case "2":
                    model.rtnCode = "S";
                    break;
            }
            logger.Info("smsNotifyRtn begin");
            
            try
            {
                //smsNotifyRtn(model, inputKey);
                Task.Run(() => smsNotify(model, inputKey));
            }
            catch (Exception e) {
                logger.Error(e.ToString());
            }
            

            logger.Info("smsNotifyRtn end");
            return Ok(model);
        }


        //private void smsNotifyRtn(SmsNotifyModel model, string inputKey)
        //{
        //    logger.Info("smsNotify begin");

        //    try
        //    {
        //        List<FAP_TEL_SMS_TEMP> dataList = new List<FAP_TEL_SMS_TEMP>();
        //        List<SYS_PARA_HIS> paraHisList = new List<SYS_PARA_HIS>();
        //        CommonUtil commonUtil = new CommonUtil();
        //        string mailContent = string.Empty;

        //        SysParaHisDao sysParaHisDao = new SysParaHisDao();
        //        string upd_id = "";

        //        switch (model.rtnCode)
        //        {
        //            case "F":
        //                mailContent = "呼叫AS400執行異常，請洽系統管理員";
        //                logger.Info("呼叫AS400執行異常，請洽系統管理員");
        //                break;
        //            case "S":
        //                mailContent = "一年以下簡訊通知，資料已轉入OPEN暫存檔，可繼續後續印表及覆核申請";
        //                logger.Info("一年以下簡訊通知，資料已轉入OPEN暫存檔，可繼續後續印表及覆核申請");
        //                FAPPYCTDao fAPPYCTDao = new FAPPYCTDao();
        //                FAPTelPoliDao fAPTelPoliDao = new FAPTelPoliDao();
        //                VeTelUtil veTelUti = new VeTelUtil();

        //                //取得AS400的資料
        //                using (EacConnection conn400 = new EacConnection(CommonUtil.GetEasycomConn()))
        //                {
        //                    conn400.Open();
        //                    dataList = fAPPYCTDao.qryFAPPYCT0(conn400);
        //                    logger.Info("smsNitify dataList count:" + dataList.Count());

        //                    foreach (FAP_TEL_SMS_TEMP d in dataList)
        //                    {

        //                        d.mobile = StringUtil.toString(veTelUti.getSmsMobile(d, conn400));
        //                    }
        //                }

        //                FAPTelSmsTempDao fAPTelSmsTempDao = new FAPTelSmsTempDao();
        //                string strConn = DbUtil.GetDBFglConnStr();
        //                using (SqlConnection conn = new SqlConnection(strConn))
        //                {
        //                    conn.Open();

        //                    fAPTelSmsTempDao.delete(conn);

        //                    SqlTransaction transaction = conn.BeginTransaction("Transaction");

        //                    try
        //                    {

        //                        //寫"FAP_TEL_SMS_TEMP  一年以下簡訊通知暫存檔"
        //                        logger.Info($"smsNitify 寫FAP_TEL_SMS_TEMP  一年以下簡訊通知暫存檔");
        //                        fAPTelSmsTempDao.Insert(dataList.Where(x => x.mobile.Length > 0).ToList(), conn, transaction);
        //                        logger.Info("sysParaHisDao.updForSmsNotify");
        //                        //異動參數檔的保留欄位一，"Y"表示已經將AS400的資料拉回OPEN
        //                        sysParaHisDao.updForSmsNotify(conn, transaction);
        //                        logger.Info("sysParaHisDao.updForSmsNotify end");
        //                        transaction.Commit();
        //                    }
        //                    catch (Exception eData)
        //                    {
        //                        transaction.Rollback();

        //                        logger.Error($"smsNitify 寫FAP_TEL_SMS_TEMP  一年以下簡訊通知暫存檔 Exception: {eData}");
        //                    }
        //                }



        //                paraHisList = sysParaHisDao.qryForGrpId("AP", new string[] { "sms_notify_case" }, "1");

        //                if (paraHisList.Count > 0)
        //                {
        //                    upd_id = StringUtil.toString(paraHisList[0].CREATE_UID);
        //                }


        //                //寫稽核軌跡
        //                logger.Info($"smsNotify 寫PIA_LOG_MAIN");
        //                PIA_LOG_MAIN piaLogMain = new PIA_LOG_MAIN();
        //                piaLogMain.EXECUTION_TYPE = "I";
        //                piaLogMain.ACCESS_ACCOUNT = upd_id;
        //                piaLogMain.AFFECT_ROWS = dataList.Where(x => x.mobile.Length > 0).Count();
        //                piaLogMain.EXECUTION_CONTENT = inputKey;
        //                piaLogMain.TRACKING_TYPE = "A";
        //                piaLogMain.PROGFUN_NAME = "VeCleanController";
        //                piaLogMain.PIA_TYPE = "0100100000";
        //                piaLogMain.PIA_OWNER1 = dataList.Where(x => x.mobile.Length > 0).FirstOrDefault()?.paid_id ?? "";
        //                piaLogMain.ACCESSOBJ_NAME = "FAP_TEL_SMS_TEMP";
        //                piaLogMain.ACCOUNT_NAME = commonUtil.qryEmp(upd_id)?.name ?? "";
        //                PiaLogMainDao piaLogMainDao = new PiaLogMainDao();
        //                piaLogMainDao.Insert(piaLogMain);
        //                break;
        //        }

        //        //寄信
        //        if (!string.IsNullOrWhiteSpace(mailContent))
        //        {
        //            FRTMailNotifyDao fRTMailNotify = new FRTMailNotifyDao();
        //            var Users = fRTMailNotify.qryNtyUsr("VE_TEL");

        //            ADModel adModel = new ADModel();
        //            adModel = commonUtil.qryEmp(upd_id);



        //            MailUtil mailUtil = new MailUtil();


        //            bool bSucess = true;

        //            if (!"".Equals(StringUtil.toString(adModel.e_mail)))
        //            {
        //                string[] user_mail = new string[] { adModel.e_mail };   // Users.Select(x => commonUtil.qryEmp(x.receiverEmpno).e_mail).ToArray();

        //                //若資料成功自AS400拉到OPEN，將一年以下簡訊通知報表MAIL給使用者
        //                if ("S".Equals(model.rtnCode) & paraHisList.Count > 0)
        //                {
        //                    string guid = Guid.NewGuid().ToString();
        //                    string fullPathS = Path.Combine(AppDomain.CurrentDomain.BaseDirectory + "Temp\\"
        //                                    , string.Concat("OAP0042" + "_" + guid + "_" + "S", ".xlsx"));

        //                    string fullPathD = Path.Combine(AppDomain.CurrentDomain.BaseDirectory + "Temp\\"
        //                                    , string.Concat("OAP0042" + "_" + guid + "_" + "D", ".xlsx"));



        //                    VeTelUtil veTelUtil = new VeTelUtil();
        //                    OAP0042Model oAP0042Model = new OAP0042Model();

        //                    foreach (SYS_PARA_HIS d in paraHisList)
        //                    {
        //                        switch (d.PARA_ID)
        //                        {

        //                            case "rpt_cnt_tp":  //計算條件(P:給付對象ID、C:支票號碼)
        //                                oAP0042Model.rpt_cnt_tp = StringUtil.toString(d.PARA_VALUE);
        //                                break;
        //                            case "stat_amt":    //歸戶金額(起訖)
        //                                if (!"".Equals(StringUtil.toString(d.PARA_VALUE)))
        //                                {
        //                                    string[] amtArr = StringUtil.toString(d.PARA_VALUE).Split('|');
        //                                    oAP0042Model.stat_amt_b = amtArr[0];
        //                                    oAP0042Model.stat_amt_e = amtArr[1];
        //                                }
        //                                break;
        //                        }
        //                    }



        //                    int cnt = veTelUtil.genSmsNotifyRpt("S", oAP0042Model, fullPathS, "");
        //                    cnt = veTelUtil.genSmsNotifyRpt("D", oAP0042Model, fullPathD, "");

        //                    string chDt = DateUtil.getCurChtDate(3);

        //                    string _fullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory + "Temp\\"
        //                                    , string.Concat("OAP0042" + "_" + guid, ".zip"));

        //                    using (var zip = new ZipFile())
        //                    {
        //                        zip.Password = upd_id + chDt;
        //                        zip.AddFile(fullPathS, "");
        //                        zip.AddFile(fullPathD, "");

        //                        zip.Save(_fullPath);
        //                    }

        //                    File.Delete(fullPathS.Trim());
        //                    File.Delete(fullPathD.Trim());


        //                    bSucess = mailUtil.sendMail(
        //                     user_mail
        //                   , "\"一年以下簡訊通知\"執行結果"
        //                   , mailContent
        //                   , true
        //                   , ""
        //                   , ""
        //                   , new string[] { _fullPath }
        //                   , true);

        //                }
        //                else
        //                {
        //                    bSucess = mailUtil.sendMail(
        //                         user_mail
        //                       , "\"一年以下簡訊通知\"執行結果"
        //                       , mailContent
        //                       , true
        //                       , ""
        //                       , ""
        //                       , null
        //                       , true);
        //                }

        //                logger.Info($"smsNitify rtn_code: {model.rtnCode} 寄信狀態: {bSucess}, 收件者:{string.Join(",", Users.Select(x => x.receiverEmpno))}");
        //            }
        //            else
        //            {
        //                logger.Info($"smsNitify rtn_code: {model.rtnCode} 無寄信,查無收件者");
        //            }
        //        }
        //        else
        //        {
        //            logger.Info($"smsNitify rtn_code: {model.rtnCode} 無寄信,找不到對應的郵件內文");
        //        }

        //    }
        //    catch (Exception e)
        //    {
        //        logger.Error(e.ToString());
        //    }
        //}

        private async Task smsNotify(SmsNotifyModel model, string inputKey)
        {
            logger.Info("smsNotify begin"); 
            await Task.Delay(1);

            try
            {
                List<FAP_TEL_SMS_TEMP> dataList = new List<FAP_TEL_SMS_TEMP>();
                List<SYS_PARA_HIS> paraHisList = new List<SYS_PARA_HIS>();
                CommonUtil commonUtil = new CommonUtil();
                string mailContent = string.Empty;

                SysParaHisDao sysParaHisDao = new SysParaHisDao();
                string upd_id = "";

                switch (model.rtnCode)
                {
                    case "F":
                        mailContent = "呼叫AS400執行異常，請洽系統管理員";
                        logger.Info("呼叫AS400執行異常，請洽系統管理員");
                        break;
                    case "S":
                        mailContent = "一年以下簡訊通知，資料已轉入OPEN暫存檔，可繼續後續印表及覆核申請";
                        logger.Info("一年以下簡訊通知，資料已轉入OPEN暫存檔，可繼續後續印表及覆核申請");
                        FAPPYCTDao fAPPYCTDao = new FAPPYCTDao();
                        FAPTelPoliDao fAPTelPoliDao = new FAPTelPoliDao();
                        VeTelUtil veTelUti = new VeTelUtil();

                        //取得AS400的資料
                        using (EacConnection conn400 = new EacConnection(CommonUtil.GetEasycomConn()))
                        {
                            conn400.Open();
                            dataList = fAPPYCTDao.qryFAPPYCT0(conn400);

                            
                            logger.Info("smsNitify dataList count:" + dataList.Count());

                            foreach (FAP_TEL_SMS_TEMP d in dataList) {

                                d.mobile = StringUtil.toString(veTelUti.getSmsMobile(d, conn400));
                            }
                        }

                        FAPTelSmsTempDao fAPTelSmsTempDao = new FAPTelSmsTempDao();
                        string strConn = DbUtil.GetDBFglConnStr();
                        using (SqlConnection conn = new SqlConnection(strConn))
                        {
                            conn.Open();

                            fAPTelSmsTempDao.delete(conn);

                            SqlTransaction transaction = conn.BeginTransaction("Transaction");
                           
                            try
                            {
                                
                                //寫"FAP_TEL_SMS_TEMP  一年以下簡訊通知暫存檔"
                                logger.Info($"smsNitify 寫FAP_TEL_SMS_TEMP  一年以下簡訊通知暫存檔");
                                fAPTelSmsTempDao.Insert(dataList.Where(x => x.mobile.Length > 0).ToList(), conn, transaction);
                                logger.Info("sysParaHisDao.updForSmsNotify");
                                //異動參數檔的保留欄位一，"Y"表示已經將AS400的資料拉回OPEN
                                sysParaHisDao.updForSmsNotify(conn, transaction);
                                logger.Info("sysParaHisDao.updForSmsNotify end");
                                transaction.Commit();
                            }
                            catch (Exception eData)
                            {
                                transaction.Rollback();
                                
                                logger.Error($"smsNitify 寫FAP_TEL_SMS_TEMP  一年以下簡訊通知暫存檔 Exception: {eData}");
                            }
                        }

                        dataList = dataList.Where(x => x.mobile.Length > 0).ToList();

                        paraHisList = sysParaHisDao.qryForGrpId("AP", new string[] { "sms_notify_case" }, "1");
                        
                        if (paraHisList.Count > 0) {
                            upd_id = StringUtil.toString(paraHisList[0].CREATE_UID);
                        }


                        //寫稽核軌跡
                        logger.Info($"smsNotify 寫PIA_LOG_MAIN");
                        PIA_LOG_MAIN piaLogMain = new PIA_LOG_MAIN();
                        piaLogMain.EXECUTION_TYPE = "I";
                        piaLogMain.ACCESS_ACCOUNT = upd_id;
                        piaLogMain.AFFECT_ROWS = dataList.Count();
                        piaLogMain.EXECUTION_CONTENT = inputKey;
                        piaLogMain.TRACKING_TYPE = "A";
                        piaLogMain.PROGFUN_NAME = "VeCleanController";
                        piaLogMain.PIA_TYPE = "0100100000";
                        piaLogMain.PIA_OWNER1 = dataList.FirstOrDefault()?.paid_id ?? "";
                        piaLogMain.ACCESSOBJ_NAME = "FAP_TEL_SMS_TEMP";
                        piaLogMain.ACCOUNT_NAME = commonUtil.qryEmp(upd_id)?.name ?? "";
                        PiaLogMainDao piaLogMainDao = new PiaLogMainDao();
                        piaLogMainDao.Insert(piaLogMain);
                        break;
                }

                //寄信
                if (!string.IsNullOrWhiteSpace(mailContent))
                {
                    FRTMailNotifyDao fRTMailNotify = new FRTMailNotifyDao();
                    var Users = fRTMailNotify.qryNtyUsr("VE_TEL");

                    ADModel adModel = new ADModel();
                    adModel = commonUtil.qryEmp(upd_id);

                    if (dataList.Count == 0)
                        mailContent = "一年以下簡訊通知，沒有符合條件的資料";

                    MailUtil mailUtil = new MailUtil();
                    

                    bool bSucess = true;

                    if (!"".Equals(StringUtil.toString(adModel.e_mail)))
                    {
                        string[] user_mail = new string[] { adModel.e_mail };   // Users.Select(x => commonUtil.qryEmp(x.receiverEmpno).e_mail).ToArray();

                        //若資料成功自AS400拉到OPEN，將一年以下簡訊通知報表MAIL給使用者
                        if ("S".Equals(model.rtnCode) & paraHisList.Count > 0 & dataList.Count > 0)
                        {
                            string guid = Guid.NewGuid().ToString();
                            string fullPathS = Path.Combine(AppDomain.CurrentDomain.BaseDirectory + "Temp\\"
                                            , string.Concat("OAP0042" + "_" + guid + "_" + "S", ".xlsx"));

                            string fullPathD = Path.Combine(AppDomain.CurrentDomain.BaseDirectory + "Temp\\"
                                            , string.Concat("OAP0042" + "_" + guid + "_" + "D", ".xlsx"));


                           
                            VeTelUtil veTelUtil = new VeTelUtil();
                            OAP0042Model oAP0042Model = new OAP0042Model();

                            foreach (SYS_PARA_HIS d in paraHisList)
                            {
                                switch (d.PARA_ID)
                                {
                                   
                                    case "rpt_cnt_tp":  //計算條件(P:給付對象ID、C:支票號碼)
                                        oAP0042Model.rpt_cnt_tp = StringUtil.toString(d.PARA_VALUE);
                                        break;
                                    case "stat_amt":    //歸戶金額(起訖)
                                        if (!"".Equals(StringUtil.toString(d.PARA_VALUE)))
                                        {
                                            string[] amtArr = StringUtil.toString(d.PARA_VALUE).Split('|');
                                            oAP0042Model.stat_amt_b = amtArr[0];
                                            oAP0042Model.stat_amt_e = amtArr[1];
                                        }
                                        break;
                                }
                            }



                            int cnt = veTelUtil.genSmsNotifyRpt("S", oAP0042Model, fullPathS, "");
                            cnt = veTelUtil.genSmsNotifyRpt("D", oAP0042Model, fullPathD, "");

                            string chDt = DateUtil.getCurChtDate(3);

                            string _fullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory + "Temp\\"
                                            , string.Concat("OAP0042" + "_" + guid, ".zip"));

                            using (var zip = new ZipFile())
                            {
                                zip.Password = upd_id + chDt;
                                zip.AddFile(fullPathS, "");
                                zip.AddFile(fullPathD, "");

                                zip.Save(_fullPath);
                            }

                            File.Delete(fullPathS.Trim());
                            File.Delete(fullPathD.Trim());


                            bSucess = mailUtil.sendMail(
                             user_mail
                           , "\"一年以下簡訊通知\"執行結果"
                           , mailContent
                           , true
                           , ""
                           , ""
                           , new string[] { _fullPath }
                           , true);

                        }
                        else {
                            bSucess = mailUtil.sendMail(
                                 user_mail
                               , "\"一年以下簡訊通知\"執行結果"
                               , mailContent
                               , true
                               , ""
                               , ""
                               , null
                               , true);
                        }

                        logger.Info($"smsNitify rtn_code: {model.rtnCode} 寄信狀態: {bSucess}, 收件者:{string.Join(",", Users.Select(x => x.receiverEmpno))}");
                    }
                    else
                    {
                        logger.Info($"smsNitify rtn_code: {model.rtnCode} 無寄信,查無收件者");
                    }
                }
                else
                {
                    logger.Info($"smsNitify rtn_code: {model.rtnCode} 無寄信,找不到對應的郵件內文");
                }

            }
            catch (Exception e)
            {
                logger.Error(e.ToString());
            }
        }




        /// <summary>
        /// 將AS400的逾期未兌領待清理資料寫入"FAP_VE_TRACE 逾期未兌領清理記錄檔"
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [Route("ProcVeTrace")]
        [ValidateModel]
        public IHttpActionResult ProcVeTrace(ProcVeTraceModel model)
        {
            string inputKey = model.type + "|" + model.exec_date + "|" + model.upd_id + "|" + model.upd_date;
            logger.Info("type:" + model.type);
            logger.Info("exec_date:" + model.exec_date);
            logger.Info("upd_id:" + model.upd_id);
            logger.Info("upd_date:" + model.upd_date);


            switch (StringUtil.toString(model.type))
            {
                case "1":
                case "2":
                case "3":
                case "4":
                case "5":
                    break;
                default:
                    model.rtnCode = "F";
                    logger.Error(inputKey + "==>類別輸入錯誤");
                    break;
            }


            if ("".Equals(model.exec_date) || "".Equals(model.upd_id) || "".Equals(model.upd_date))
            {
                model.rtnCode = "F";
                logger.Error(inputKey + "==>類別輸入錯誤");
            }



            if (!"F".Equals(model.rtnCode))
                Task.Run(() => procVeTrace(model));

            model.rtnCode = "S";


            logger.Info("inputKey:" + inputKey + "==> rtnCode:" + model.rtnCode);

            return Ok(model);
        }


        [Route("ProcVeTraceStatus")]
        [ValidateModel]
        public IHttpActionResult ProcVeTraceStatus(ProcVeTraceStatusModel model)
        {
            string inputKey = model.check_acct_short + "|" + model.check_no + "|" + model.re_paid_date + "|" + model.re_paid_type + "|" + model.upd_id + "|" + model.sql_vhrdt;
            logger.Info("check_acct_short:" + model.check_acct_short);
            logger.Info("check_no:" + model.check_no);
            logger.Info("re_paid_date:" + model.re_paid_date);
            logger.Info("re_paid_type:" + model.re_paid_type);
            logger.Info("upd_id:" + model.upd_id);
            logger.Info("sql_vhrdt:" + model.sql_vhrdt);

            ValidateUtil validateUtil = new ValidateUtil();

            if (!validateUtil.chkChtDate(model.re_paid_date)) {
                model.rtnCode = "F";
                logger.Error(inputKey + "==>「再給付日期」輸入錯誤");
            }

            if (!validateUtil.chkChtDate(model.sql_vhrdt))
            {
                model.rtnCode = "F";
                logger.Error(inputKey + "==>「帳務日期」輸入錯誤");
            }


            //modify by daiyu 20190906 再給付方式、異動人員 可不輸入
            if ("".Equals(model.check_acct_short) || "".Equals(model.check_no))
            {
                model.rtnCode = "F";
                logger.Error(inputKey + "==>輸入錯誤");
            }


            //add by daiyu 20210517
            FAPVeTraceDao fAPVeTraceDao = new FAPVeTraceDao();

            FAP_VE_TRACE m = fAPVeTraceDao.qryByCheckNo(model.check_no, model.check_acct_short);
            if (m.re_paid_date != null) {
                model.rtnCode = "F";
                logger.Error(inputKey + "==>已有「帳務日期」不更新");
            }
            //end add 20210517


            if (!"F".Equals(model.rtnCode))
                model = procVeTraceStatus(model);

            // model.rtnCode = "S";


            logger.Info("inputKey:" + inputKey + "==> rtnCode:" + model.rtnCode);

            return Ok(model);
        }





        /// <summary>
        /// 查AS400踐行程序一~五的資料
        /// </summary>
        /// <param name="exec_date"></param>
        /// <param name="upd_id"></param>
        /// <returns></returns>
        private List<VeCleanModel> qryPPAAList(string type, string exec_date, string upd_id)
        {
            List<VeCleanModel> dataList = new List<VeCleanModel>();


            using (EacConnection conn400 = new EacConnection(CommonUtil.GetEasycomConn()))
            {
                conn400.Open();

                //modify by daiyu 20200811
                FAPPPAWDao fAPPPAWDao = new FAPPPAWDao();
                FMNPPAADao fMNPPAADao = new FMNPPAADao();

                switch (type) {
                    case "1":   //主動
                        dataList = fAPPPAWDao.qryForVeClean(conn400, exec_date, upd_id, "X000A");
                        break;
                    case "2":   //比對
                        dataList = fAPPPAWDao.qryForVeClean(conn400, exec_date, upd_id, "X000B");
                        break;
                    case "3":   //特殊-小額抽件
                        dataList = fAPPPAWDao.qryForVeClean(conn400, exec_date, upd_id, "X000C");
                        break;
                    case "4":   //特殊-戶政地址
                        dataList = fAPPPAWDao.qryForVeClean(conn400, exec_date, upd_id, "X000D");
                        break;
                    case "5":   //假扣押
                        dataList = fAPPPAWDao.qryForVeClean(conn400, exec_date, upd_id, "X0001");
                        break;

                }


                return dataList;

                //if ("5".Equals(type))
                //{
                //    FAPPPAWDao fAPPPAWDao = new FAPPPAWDao();
                //    return dataList = fAPPPAWDao.qryForVeClean(conn400, exec_date, upd_id);
                //}
                //else
                //{
                //    FMNPPAADao fMNPPAADao = new FMNPPAADao();
                //    return dataList = fMNPPAADao.qryForVeClean(conn400, exec_date, upd_id, type);
                //}


                //end modify 20200811



            }
        }



        /// <summary>
        /// 支票逾期未兌領通知作業
        /// 處理踐行程序一相關欄位
        /// </summary>
        /// <param name="dataList"></param>
        /// <param name="model"></param>
        /// <param name="user_id"></param>
        /// <returns></returns>
        private List<rptModel> procExecI(List<VeCleanModel> dataList, ProcVeTraceModel model, string user_id) {
            List<rptModel> rptList = new List<rptModel>();
            List<string> idList = new List<string>();

            DateTime dt = DateTime.Now;

            string strConn = DbUtil.GetDBFglConnStr();
            using (SqlConnection conn = new SqlConnection(strConn))
            {
                conn.Open();


                try
                {
                    FAPVeTraceDao fAPVeTraceDao = new FAPVeTraceDao();
                    FAPVeTracePoliDao fAPVeTracePoliDao = new FAPVeTracePoliDao();


                    string status = "";

                    //依"支票簡稱+支票號碼"，逐筆寫入DB
                    foreach (VeCleanModel m in dataList.GroupBy(o => new { o.check_no, o.check_acct_short, o.paid_id })
                .Select(group => new VeCleanModel
                {
                    check_no = group.Key.check_no,
                    check_acct_short = group.Key.check_acct_short,
                    paid_id = group.Key.paid_id
                }).OrderBy(x => x.check_no).ThenBy(x => x.check_acct_short).ToList<VeCleanModel>())
                    {
                        

                        SqlTransaction transaction = conn.BeginTransaction("Transaction");

                        try
                        {
                            FAP_VE_TRACE openTraceData = fAPVeTraceDao.qryByCheckNo(m.check_no, m.check_acct_short);
                            status = StringUtil.toString(openTraceData.status);
                            bool bPass = false;
                            string err_msg = "";
                            rptModel rptModel = new rptModel();
                            switch (status)
                            {
                                case "1":
                                    err_msg = "已給付";
                                    bPass = true;
                                    break;
                                case "2":
                                    err_msg = "已清理";
                                    bPass = true;
                                    break;
                            }

                            if (bPass)
                            {
                                ObjectUtil.CopyPropertiesTo(m, rptModel);
                                rptModel.err_msg = err_msg;
                                rptList.Add(rptModel);
                                transaction.Rollback();
                                continue;
                            }



                            //該支票資料不存在系統
                            if ("".Equals(StringUtil.toString(openTraceData.check_no)))
                            {
                                int i = 0;
                                foreach (VeCleanModel d in dataList.Where(x => x.check_no == m.check_no & x.check_acct_short == m.check_acct_short).ToList())
                                {

                                    //新增"FAP_VE_TRACE 逾期未兌領清理記錄檔"
                                    if (i == 0)
                                    {
                                        FAP_VE_TRACE main = new FAP_VE_TRACE();
                                        ObjectUtil.CopyPropertiesTo(d, main);
                                        main.fsc_range = "0";
                                        main.status = "3";
                                        main.cert_doc_1 = "F1";
                                        main.practice_1 = "G1";
                                        main.exec_date_1 = Convert.ToDateTime(DateUtil.As400ChtDateToADDate(model.exec_date));
                                        main.proc_desc = "主動寄發信函";
                                        main.update_id = user_id;
                                        main.update_datetime = dt;
                                        main.exec_date = Convert.ToDateTime(DateUtil.As400ChtDateToADDate(model.exec_date));

                                        fAPVeTraceDao.insert(main, conn, transaction);
                                    }

                                    //新增"FAP_VE_TRACE_POLI 逾期未兌領清理保單明細檔"
                                    FAP_VE_TRACE_POLI poli = new FAP_VE_TRACE_POLI();
                                    ObjectUtil.CopyPropertiesTo(d, poli);
                                    fAPVeTracePoliDao.insert(poli, conn, transaction);

                                    i++;
                                }

                            }
                            else
                            {
                                VeCleanModel d = dataList.Where(x => x.check_no == m.check_no & x.check_acct_short == m.check_acct_short).FirstOrDefault();
                                FAP_VE_TRACE main = new FAP_VE_TRACE();
                                ObjectUtil.CopyPropertiesTo(d, main);
                                main.status = (status == "" || status == "4") ? "3" : status;
                                main.cert_doc_1 = "F1";
                                main.practice_1 = "G1";
                                main.exec_date_1 = Convert.ToDateTime(DateUtil.As400ChtDateToADDate(model.exec_date));
                                main.proc_desc = "主動寄發信函";
                                main.update_id = user_id;
                                main.update_datetime = dt;
                                main.exec_date = Convert.ToDateTime(DateUtil.As400ChtDateToADDate(model.exec_date));
                                

                                fAPVeTraceDao.updateForVeClean(model.type, main, conn, transaction);
                            }


                            transaction.Commit();


                        }
                        catch (Exception eData)
                        {
                            transaction.Rollback();

                            logger.Error("check_no=" + m.check_no + " check_acct_short=" + m.check_acct_short + "  " + eData.ToString());
                            rptModel rptModel = new rptModel();
                            ObjectUtil.CopyPropertiesTo(m, rptModel);
                            rptModel.err_msg = "其它錯誤";
                            rptList.Add(rptModel);
                        }
                    }
                }
                catch (Exception e)
                {
                    logger.Error(e.ToString());
                }
            }

            return rptList;

        }


        private List<rptModel> procExecU(List<VeCleanModel> dataList, ProcVeTraceModel model, string user_id, string user_mail)
        {
            List<rptModel> rptList = new List<rptModel>();
            List<FAP_VE_TRACE> rptExec5List = new List<FAP_VE_TRACE>();
            DateTime dt = DateTime.Now;



            using (dbFGLEntities db = new dbFGLEntities())
            {
                string strConn = DbUtil.GetDBFglConnStr();
                using (SqlConnection conn = new SqlConnection(strConn))
                {
                    conn.Open();


                    try
                    {
                        FAPVeTraceDao fAPVeTraceDao = new FAPVeTraceDao();
                        FAPVeTracePoliDao fAPVeTracePoliDao = new FAPVeTracePoliDao();


                        string status = "";

                        //依"支票簡稱+支票號碼"，逐筆寫入DB
                        foreach (VeCleanModel m in dataList.GroupBy(o => new { o.check_no, o.check_acct_short, o.paid_id, o.data_flag })
                    .Select(group => new VeCleanModel
                    {
                        check_no = group.Key.check_no,
                        check_acct_short = group.Key.check_acct_short,
                        paid_id = group.Key.paid_id,
                        data_flag = group.Key.data_flag
                    }).OrderBy(x => x.check_no).ThenBy(x => x.check_acct_short).ToList<VeCleanModel>())
                        {
                            //logger.Info("check_no;" + m.check_no + " check_acct_short:" + m.check_acct_short + " paid_id:" + m.paid_id);
                            SqlTransaction transaction = conn.BeginTransaction("Transaction");

                            try
                            {
                                FAP_VE_TRACE openTraceData = fAPVeTraceDao.qryByCheckNo(m.check_no, m.check_acct_short, db);
                                status = StringUtil.toString(openTraceData.status);
                                bool bPass = false;
                                string err_msg = "";
                                rptModel rptModel = new rptModel();
                                switch (status)
                                {
                                    case "1":
                                        err_msg = "已給付";
                                        bPass = true;
                                        break;
                                    case "2":
                                        err_msg = "已清理";
                                        bPass = true;
                                        break;
                                }

                                if (bPass)
                                {
                                    ObjectUtil.CopyPropertiesTo(m, rptModel);
                                    rptModel.err_msg = err_msg;
                                    rptList.Add(rptModel);
                                    transaction.Rollback();
                                    continue;
                                }



                                //該支票資料不存在系統
                                if ("".Equals(StringUtil.toString(openTraceData.check_no)))
                                {
                                    //modify by daiyu 20200811
                                    int i = 0;
                                    foreach (VeCleanModel d in dataList.Where(x => x.check_no == m.check_no & x.check_acct_short == m.check_acct_short).ToList())
                                    {

                                        //新增"FAP_VE_TRACE 逾期未兌領清理記錄檔"
                                        if (i == 0)
                                        {
                                            FAP_VE_TRACE main = new FAP_VE_TRACE();
                                            main = proc_practice(main, d, model, user_id, dt, "I", status, 0);

                                            main.proc_desc = "";    //add by daiyu 20210125
                                            fAPVeTraceDao.insert(main, conn, transaction);
                                        }

                                        //新增"FAP_VE_TRACE_POLI 逾期未兌領清理保單明細檔"
                                        FAP_VE_TRACE_POLI poli = new FAP_VE_TRACE_POLI();
                                        ObjectUtil.CopyPropertiesTo(d, poli);
                                        fAPVeTracePoliDao.insert(poli, conn, transaction);

                                        i++;
                                    }

                                    //rptModel.err_msg = "不存在清理紀錄檔";

                                    //rptList.Add(rptModel);
                                    //transaction.Rollback();


                                    //continue;
                                }
                                else
                                {
                                    VeCleanModel d = dataList.Where(x => x.check_no == m.check_no & x.check_acct_short == m.check_acct_short).FirstOrDefault();
                                    FAP_VE_TRACE main = new FAP_VE_TRACE();
                                    int as400_send_cnt = 0;
                                    if (openTraceData.as400_send_cnt != null)
                                        as400_send_cnt = (int)openTraceData.as400_send_cnt;

                                    main = proc_practice(main, d, model, user_id, dt, "U", status, as400_send_cnt);
                                    main.proc_desc = "";    //add by daiyu 20210125



                                    //if ("5".Equals(model.type) & !"Y".Equals(m.data_flag))
                                    //    if (!"".Equals(StringUtil.toString(openTraceData.practice_5)))
                                    //        rptExec5List.Add(openTraceData);

                                    //    ObjectUtil.CopyPropertiesTo(d, main);

                                    //main.status = (status == "" || status == "4") ? "3" : status;
                                    //main.update_id = user_id;
                                    //main.update_datetime = dt;
                                    //main.exec_date = Convert.ToDateTime(DateUtil.As400ChtDateToADDate(model.exec_date));

                                    //    switch (model.type)
                                    //    {
                                    //        case "2":
                                    //            main.cert_doc_2 = "F1";
                                    //            main.practice_2 = "G2";
                                    //            main.exec_date_2 = Convert.ToDateTime(DateUtil.As400ChtDateToADDate(model.exec_date));
                                    //            main.proc_desc = "比對成功信函";
                                    //            break;
                                    //        case "3":
                                    //            main.cert_doc_3 = "F1";
                                    //            main.practice_3 = "G3";
                                    //            main.exec_date_3 = Convert.ToDateTime(DateUtil.As400ChtDateToADDate(model.exec_date));
                                    //            main.proc_desc = "特殊抽件通知信函";
                                    //            break;
                                    //        case "4":
                                    //            main.cert_doc_4 = "F1";
                                    //            main.practice_4 = "G4";
                                    //            main.exec_date_4 = Convert.ToDateTime(DateUtil.As400ChtDateToADDate(model.exec_date));
                                    //            main.proc_desc = "戶政機關查詢後寄信";
                                    //            break;
                                    //        case "5":
                                    //            main.status = (status == "") ? "4" : status;
                                    //            if ("Y".Equals(m.data_flag))
                                    //            {
                                    //                main.cert_doc_5 = "F8";
                                    //                main.practice_5 = "G5";
                                    //                main.exec_date_5 = Convert.ToDateTime(DateUtil.As400ChtDateToADDate(model.exec_date));
                                    //                main.proc_desc = d.filler_10;   //filer_10-->即假扣押編號
                                    //            }
                                    //            else
                                    //            {
                                    //                if (!"".Equals(StringUtil.toString(openTraceData.practice_5)))
                                    //                    rptExec5List.Add(openTraceData);
                                    //            }

                                    //            break;

                                    //    }


                                    if ("5".Equals(model.type)) {
                                        if ("Y".Equals(m.data_flag))
                                            fAPVeTraceDao.updateForVeClean(model.type, main, conn, transaction);
                                        else {
                                            if (!"".Equals(StringUtil.toString(openTraceData.practice_5)))
                                                rptExec5List.Add(openTraceData);
                                        }
                                    }
                                    else {
                                        if(!"0".Equals(d.filler_16))
                                            fAPVeTraceDao.updateForVeClean(model.type, main, conn, transaction);
                                    }


                                        
                                }
                                //end modify 20200811

                                transaction.Commit();

                            }
                            catch (Exception eData)
                            {
                                transaction.Rollback();

                                logger.Error("check_no=" + m.check_no + " check_acct_short=" + m.check_acct_short + "  " + eData.ToString());
                                rptModel rptModel = new rptModel();
                                ObjectUtil.CopyPropertiesTo(m, rptModel);
                                rptModel.err_msg = "其它錯誤";
                                rptList.Add(rptModel);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        logger.Error(e.ToString());
                    }
                }
            }
            //寄送"逾期未兌領-保戶特保檔已下架通知"
            if ("5".Equals(model.type) & rptExec5List.Count > 0)
                genExec5Rpt(rptExec5List, user_mail);



            return rptList;

        }


        private telModel procTelInterview(ProcVeTraceStatusModel model, DateTime dt, SqlConnection conn, SqlTransaction transaction) {
            string paid_code = "4";     //(預設)4 正常給付: 完全無電訪日也無戶政調閱記錄者, 即為正常給付
            string tel_proc_no = "";
            telModel _telModel = new telModel();

            FAPTelCheckDao fAPTelCheckDao = new FAPTelCheckDao();
            FAPTelProcDao fAPTelProcDao = new FAPTelProcDao();
            FAPTelInterviewDao fAPTelInterviewDao = new FAPTelInterviewDao();

            //1.查詢出該張支票所屬電訪編號內的所有支票資料
            FAP_TEL_CHECK tel_check = fAPTelCheckDao.qryByCheckNo(model.check_no, model.check_acct_short, "tel_assign_case");
            if ("Y".Equals(StringUtil.toString(tel_check.data_flag)))
                tel_proc_no = tel_check.tel_proc_no;

            FAP_TEL_INTERVIEW inverview = new FAP_TEL_INTERVIEW();
            List<OAP0046DModel> check_list = new List<OAP0046DModel>();
            List<FAP_TEL_PROC> clean_proc_list = new List<FAP_TEL_PROC>();

            if (!"".Equals(tel_proc_no))
            {
                //異動【FAP_TEL_CHECK 電訪支票檔】--> 派件狀態改為”2電訪結束”(含電訪及簡訊)
                tel_check.update_id = model.upd_id;
                tel_check.update_datetime = dt;
                fAPTelCheckDao.updVeCleanFinish(tel_check, conn, transaction);


                inverview = fAPTelInterviewDao.qryByTelProcNo(tel_proc_no); //查電訪相關資料
                check_list = fAPTelCheckDao.qryForTelProcRpt(tel_proc_no);  //查電訪編號所有的支票
                clean_proc_list = fAPTelProcDao.qryTelProcNoList(tel_proc_no, "3", "2");    //查屬"清理"，且有被核可的歷程
            }
            else {

                if ("Y".Equals(StringUtil.toString(tel_check.data_flag))) {
                    //異動【FAP_TEL_CHECK 電訪支票檔】--> 派件狀態改為”2電訪結束”(含電訪及簡訊)
                    tel_check.update_id = model.upd_id;
                    tel_check.update_datetime = dt;
                    fAPTelCheckDao.updVeCleanFinish(tel_check, conn, transaction);
                }

                _telModel.paid_code = paid_code;    //4 正常給付: 完全無電訪日也無戶政調閱記錄者, 即為正常給付
                _telModel.level_1 = "";
                _telModel.level_2 = "";

                return _telModel; 
            }
                
            


            //2.判斷給付細項
            if (clean_proc_list.Count > 0)
            {
                if (clean_proc_list.Where(x => x.proc_status == "8").Count() > 0)
                    paid_code = "3";    //3 戶政給付: 若已有戶政調閱記錄(清理階段為調閱完成), 則為戶政給付
            }

            if (!"".Equals(inverview.tel_proc_no) & "4".Equals(paid_code))
            {
                try
                {
                    DateTime sql_vhrdt = Convert.ToDateTime(model.sql_vhrdt);
                    if (sql_vhrdt.CompareTo(inverview.tel_interview_datetime) >= 0)
                        paid_code = "2";    //2 追踨給付: 若給付日小於第一次追踨處理結果登錄日,則為追踨給付
                    else if (sql_vhrdt.CompareTo(inverview.tel_interview_f_datetime) >= 0)
                        paid_code = "1";    //1 電訪給付: 若給付日小於電訪處理結果之最後一次電訪日期,則為電訪給付

                }
                catch (Exception e)
                {
                }
            }


            //3.若該批電訪編號的所有支票均已給付
            //  3.1 異動【FAP_TEL_INTERVIEW 電訪及追踨記錄檔】-->覆核結果自動改為16給付結案,派件狀態改為”2電訪結束”
            //  3.2 檢查【FAP_TEL_INTERVIEW_HIS 電訪及追踨記錄暫存檔】，若有覆核中的資料，改成"駁回"，覆核人員以AS400的異動人員帶入
            if (check_list.Count > 0) {
                if (check_list.Where(x => x.status != "1" & !(tel_check.check_acct_short == x.check_acct_short & tel_check.check_no == x.check_no)).Count() == 0) {
                    if ("13".Equals(inverview.tel_appr_result)) {
                        inverview.clean_status = "13";
                        inverview.clean_date = dt;
                        inverview.clean_f_date = dt;
                    }

                    if (!"16".Equals(inverview.tel_appr_result)) {
                        inverview.tel_appr_result = "16";
                        inverview.tel_appr_datetime = dt;
                    }

                    inverview.dispatch_status = "2";
                    inverview.update_id = model.upd_id;
                    inverview.update_datetime = dt;

                    //異動【FAP_TEL_INTERVIEW 電訪及追踨記錄檔】
                    fAPTelInterviewDao.updVeCleanFinish(inverview, conn, transaction);

                    //異動【FAP_TEL_INTERVIEW_HIS 電訪及追踨記錄暫存檔】
                    FAPTelInterviewHisDao fAPTelInterviewHisDao = new FAPTelInterviewHisDao();
                    fAPTelInterviewHisDao.updVeCleanFinish(model.upd_id, "3", tel_proc_no, dt, conn, transaction);
                }
            }

            _telModel.paid_code = paid_code;    //4 正常給付: 完全無電訪日也無戶政調閱記錄者, 即為正常給付
            _telModel.level_1 = StringUtil.toString(inverview.level_1);
            _telModel.level_2 = StringUtil.toString(inverview.level_2);

            return _telModel;
            
        }

        private FAP_VE_TRACE proc_practice(FAP_VE_TRACE main, VeCleanModel d, ProcVeTraceModel model, string user_id, DateTime dt
            , string exec_type, string status, int as400_send_cnt) {
            ObjectUtil.CopyPropertiesTo(d, main);

            if ("I".Equals(exec_type)) {
                main.fsc_range = "0";
                
                if (!"0".Equals(d.filler_16))
                    main.status = "3";  //已通知尚未給付
                else
                    main.status = "4";  //尚未通知

            } else
                main.status = (status == "" || status == "4") ? "3" : status;


            main.update_id = user_id;
            main.update_datetime = dt;


            switch (model.type)
            {
                case "1":
                    if (!"0".Equals(d.filler_16))
                    {
                        main.exec_date = Convert.ToDateTime(DateUtil.As400ChtDateToADDate(model.exec_date));
                        main.cert_doc_1 = "F1";
                        main.practice_1 = "G1";
                        main.exec_date_1 = Convert.ToDateTime(DateUtil.As400ChtDateToADDate(model.exec_date));
                        main.proc_desc = "主動寄發信函";
                        main.as400_send_cnt = (as400_send_cnt + 1);
                    }
                    break;

                case "2":
                    if (!"0".Equals(d.filler_16))
                    {
                        main.exec_date = Convert.ToDateTime(DateUtil.As400ChtDateToADDate(model.exec_date));
                        main.cert_doc_2 = "F1";
                        main.practice_2 = "G2";
                        main.exec_date_2 = Convert.ToDateTime(DateUtil.As400ChtDateToADDate(model.exec_date));
                        main.proc_desc = "比對成功信函";
                        main.as400_send_cnt = (as400_send_cnt + 1);
                    }
                    break;

                case "3":
                    if (!"0".Equals(d.filler_16))
                    {
                        main.exec_date = Convert.ToDateTime(DateUtil.As400ChtDateToADDate(model.exec_date));
                        main.cert_doc_3 = "F1";
                        main.practice_3 = "G3";
                        main.exec_date_3 = Convert.ToDateTime(DateUtil.As400ChtDateToADDate(model.exec_date));
                        main.proc_desc = "特殊抽件通知信函";
                        main.as400_send_cnt = (as400_send_cnt + 1);
                    }
                    break;

                case "4":
                    if (!"0".Equals(d.filler_16))
                    {
                        main.exec_date = Convert.ToDateTime(DateUtil.As400ChtDateToADDate(model.exec_date));
                        main.cert_doc_4 = "F1";
                        main.practice_4 = "G4";
                        main.exec_date_4 = Convert.ToDateTime(DateUtil.As400ChtDateToADDate(model.exec_date));
                        main.proc_desc = "戶政機關查詢後寄信";
                        main.as400_send_cnt = (as400_send_cnt + 1);
                    }
                    break;

                case "5":
                    main.as400_send_cnt = as400_send_cnt;

                    if ("Y".Equals(d.data_flag))
                    {
                        main.status = (status == "") ? "4" : status;
                        main.exec_date = Convert.ToDateTime(DateUtil.As400ChtDateToADDate(model.exec_date));
                        main.cert_doc_5 = "F8";
                        main.practice_5 = "G5";
                        main.exec_date_5 = Convert.ToDateTime(DateUtil.As400ChtDateToADDate(model.exec_date));
                        main.proc_desc = d.filler_10;   //filer_10-->即假扣押編號
                    } else
                        main.status = "4";
                    break;

            }

            return main;
        }


        /// <summary>
        /// 寄送"逾期未兌領-保戶特保檔已下架通知"
        /// </summary>
        /// <param name="rptExec5List"></param>
        /// <param name="user_mail"></param>
        private void genExec5Rpt(List<FAP_VE_TRACE> rptExec5List, string user_mail) {
            MailUtil mailUtil = new MailUtil();

            string guid = Guid.NewGuid().ToString();
            string fullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory + "Temp\\"
                            , string.Concat("VeClean" + "_" + guid , ".xlsx"));

            using (XLWorkbook workbook = new XLWorkbook())
            {
                var ws = workbook.Worksheets.Add("sheet1");

                ws.Cell(1, 1).Value = "給付對象ID";
                ws.Cell(1, 2).Value = "支票號碼";
                ws.Cell(1, 3).Value = "支票帳號簡稱";
                ws.Cell(1, 4).Value = "支票到期日";
                ws.Cell(1, 5).Value = "支票金額";

                int iRow = 1;
                foreach (FAP_VE_TRACE d in rptExec5List) {
                    iRow++;

                    ws.Cell(iRow, 1).Value = d.paid_id;
                    ws.Cell(iRow, 2).Value = d.check_no;
                    ws.Cell(iRow, 3).Value = d.check_acct_short;
                    ws.Cell(iRow, 4).Value = d.check_date;
                    ws.Cell(iRow, 5).Value = String.Format("{0:n0}", d.check_amt);
                }


                ws.Range(1, 1, iRow, 5).Style.Border.OutsideBorder = XLBorderStyleValues.Thick;
                ws.Range(1, 1, iRow, 5).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                ws.Range(1, 5, iRow, 5).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;


                ws.Columns().AdjustToContents();  // Adjust column width
                ws.Rows().AdjustToContents();     // Adjust row heights

                workbook.SaveAs(fullPath);

            }

            string mailContent = "";

            bool bSucess = mailUtil.sendMail(new string[] { user_mail }
                , "逾期未兌領-保戶特保檔已下架通知"
                , mailContent
                , true
               , ""
               , ""
               , new string[] { fullPath }
               , true);
        }



        /// <summary>
        /// AS400資料回寫OPEN TABLE
        /// "FAP_VE_TRACE 逾期未兌領清理記錄檔"
        /// "FAP_VE_TRACE_POLI 逾期未兌領清理保單明細檔"
        /// </summary>
        /// <param name="dataList"></param>
        /// <param name="model"></param>
        /// <param name="user_id"></param>
        /// <returns></returns>
        private List<rptModel> procOpenDb(List<VeCleanModel> dataList, ProcVeTraceModel model, string user_id, string user_mail)
        {
            List<rptModel> rptList = new List<rptModel>();
            rptList = procExecU(dataList, model, user_id, user_mail);   //modify by daiyu 20200811

            //switch (model.type) {
            //    case "1":
            //        rptList = procExecI(dataList, model, user_id);
            //        break;
            //    case "2":
            //    case "3":
            //    case "4":
            //    case "5":
            //        rptList = procExecU(dataList, model, user_id, user_mail);
            //        break;
            //}


            return rptList;
        }


        /// <summary>
        /// 寄送異常報表
        /// </summary>
        /// <param name="rptList"></param>
        /// <param name="user_mail"></param>
        private void procErrRpt(List<rptModel> rptList, string user_mail) {
            MailUtil mailUtil = new MailUtil();

            string guid = Guid.NewGuid().ToString();
            string fullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory + "Temp\\"
                            , string.Concat("VeClean" + "_" + guid, ".xlsx"));

            using (XLWorkbook workbook = new XLWorkbook())
            {
                var ws = workbook.Worksheets.Add("sheet1");

                ws.Cell(1, 1).Value = "未入檔原因";
                ws.Cell(1, 2).Value = "支票號碼";
                ws.Cell(1, 3).Value = "支票帳號簡稱";
                ws.Cell(1, 4).Value = "給付對象ID";

                int iRow = 1;
                foreach (rptModel d in rptList)
                {
                    iRow++;

                    ws.Cell(iRow, 1).Value = d.err_msg;
                    ws.Cell(iRow, 2).Value = d.check_no;
                    ws.Cell(iRow, 3).Value = d.check_acct_short;
                    ws.Cell(iRow, 4).Value = d.paid_id;
                }


                ws.Range(1, 1, iRow, 4).Style.Border.OutsideBorder = XLBorderStyleValues.Thick;
                ws.Range(1, 1, iRow, 4).Style.Border.InsideBorder = XLBorderStyleValues.Thin;



                ws.Columns().AdjustToContents();  // Adjust column width
                ws.Rows().AdjustToContents();     // Adjust row heights

                workbook.SaveAs(fullPath);

            }

            string mailContent = "";

            bool bSucess = mailUtil.sendMail(new string[] { user_mail }
                , "逾期未兌領清理紀錄檔未入檔明細"
                , mailContent
                , true
               , ""
               , ""
               , new string[] { fullPath }
               , true);
        }



        /// <summary>
        /// 重新給付時要異動【清理記錄檔】的"清理狀態=1.已給付"
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        private  ProcVeTraceStatusModel procVeTraceStatus(ProcVeTraceStatusModel model)
        {
           // await Task.Delay(1);


            try
            {
                string strConn = DbUtil.GetDBFglConnStr();
                using (SqlConnection conn = new SqlConnection(strConn))
                {
                    conn.Open();

                    SqlTransaction transaction = conn.BeginTransaction("Transaction");

                    try
                    {
                        //查詢出異動人員
                        string user_id = "";
                        string user_mail = "";
                        string user_name = "";


                        if (!"".Equals(model.upd_id)) {
                            using (DB_INTRAEntities db = new DB_INTRAEntities())
                            {
                                OaEmpDao OaEmpDao = new OaEmpDao();
                                V_EMPLY2 usr = OaEmpDao.qryByUsrId(model.upd_id, db);
                                if (usr != null)
                                {
                                    user_id = StringUtil.toString(usr.USR_ID);
                                    user_mail = StringUtil.toString(usr.EMAIL);
                                    user_name = StringUtil.toString(usr.EMP_NAME);
                                }
                            }
                        }

                        DateTime dt = DateTime.Now;

                        FAPVeTraceDao fAPVeTraceDao = new FAPVeTraceDao();
                        string re_paid_date = DateUtil.As400ChtDateToADDate(model.re_paid_date);
                        string sql_vhrdt = DateUtil.As400ChtDateToADDate(model.sql_vhrdt);  //add by daiyu 20201005
                        model.sql_vhrdt = re_paid_date;
                        model.sql_vhrdt = sql_vhrdt;
                        model.upd_id = user_id;

                        telModel _telModel = procTelInterview(model, dt, conn, transaction);

                        fAPVeTraceDao.updateForAs400PaidType(user_id, model.check_no, model.check_acct_short, re_paid_date, model.re_paid_type
                            ,model.sql_vhrdt, _telModel.paid_code, _telModel.level_1, _telModel.level_2, conn, transaction);   //modify by daiyu 20201005



                        transaction.Commit();
                    }
                    catch (Exception e) {
                        transaction.Rollback();

                        model.rtnCode = "F";
                        logger.Error(e.ToString());
                    }
                }
            }
            catch (Exception e)
            {
                model.rtnCode = "F";
                logger.Error(e.ToString());
            }

            return model;

        }


        /// <summary>
        /// 處理AS400回寫清理計畫主程式段
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        private async Task procVeTrace(ProcVeTraceModel model)
        {
            await Task.Delay(1);


            try
            {
                List<VeCleanModel> dataList = new List<VeCleanModel>();

                //取得AS400的資料
                dataList = qryPPAAList(model.type, model.exec_date, model.upd_id);


                logger.Info("dataList count:" + dataList.Count());

                //查詢出異動人員
                string user_id = "";
                string user_mail = "";
                string user_name = "";

                using (DB_INTRAEntities db = new DB_INTRAEntities())
                {
                    OaEmpDao OaEmpDao = new OaEmpDao();
                    V_EMPLY2 usr = OaEmpDao.qryByUsrId(model.upd_id, db);
                    if (usr != null)
                    {
                        user_id = StringUtil.toString(usr.USR_ID);
                        user_mail = StringUtil.toString(usr.EMAIL);
                        user_name = StringUtil.toString(usr.EMP_NAME);
                    }
                }

                //寫"FAP_VE_TRACE 逾期未兌領清理記錄檔"
                List<rptModel> rptList = procOpenDb(dataList, model, user_id, user_mail);
                logger.Info("rptList count:" + rptList.Count());

                //寄送異常資料報表
                if (rptList.Count > 0)
                    procErrRpt(rptList, user_mail);


                //寫稽核軌跡
                PIA_LOG_MAIN piaLogMain = new PIA_LOG_MAIN();
                piaLogMain.EXECUTION_TYPE = "I";
                piaLogMain.ACCESS_ACCOUNT = user_id;
                piaLogMain.ACCOUNT_NAME = user_name;
                piaLogMain.AFFECT_ROWS = dataList.Count();
                piaLogMain.EXECUTION_CONTENT = model.type + "|" + model.exec_date + "|" + model.upd_id;
                writePiaLog(piaLogMain);


            }
            catch (Exception e) {
                logger.Error(e.ToString());
            }

           
        }


        private void writePiaLog(PIA_LOG_MAIN piaLogMain)
        {
            piaLogMain.TRACKING_TYPE = "A";
            piaLogMain.PROGFUN_NAME = "VeCleanController";
            piaLogMain.PIA_TYPE = "0100100000";
            piaLogMain.ACCESSOBJ_NAME = "FAP_VE_TRACE";
            PiaLogMainDao piaLogMainDao = new PiaLogMainDao();
            piaLogMainDao.Insert(piaLogMain);

        }



        public partial class rptModel
        {

            public string err_msg { get; set; }

            public string paid_id { get; set; }

            public string check_no { get; set; }

            public string check_acct_short { get; set; }

            public string check_date { get; set; }

            public string check_amt { get; set; }
            public string closed_date { get; set; }
            public string closed_no { get; set; }

            
        }



        /// <summary>
        /// 錯誤參數model
        /// </summary>
        public partial class ProcVeTraceStatusModel
        {
            /// <summary>
            ///支票簡稱
            /// </summary>
            public string check_acct_short { get; set; }

            /// <summary>
            /// 支票號碼
            /// </summary>
            public string check_no { get; set; }

            /// <summary>
            /// 再給付日期
            /// </summary>
            public string re_paid_date { get; set; }

            /// <summary>
            /// 再給付方式
            /// </summary>
            public string re_paid_type { get; set; }

            /// <summary>
            /// 異動人員
            /// </summary>
            public string upd_id { get; set; }

            /// <summary>
            /// 帳務日期
            /// </summary>
            public string sql_vhrdt { get; set; }


            /// <summary>
            /// S:成功;F失敗
            /// </summary>
            public string rtnCode { get; set; }


        }


        /// <summary>
        /// 一年以下簡訊通知
        /// </summary>
        public partial class SmsNotifyModel
        {
            /// <summary>
            ///type
            /// </summary>
            public string rtnCode { get; set; }
            /// <summary>
            /// 執行日期
            /// </summary>
            public string upd_date { get; set; }

        }

        public partial class telModel
        {
            public string paid_code { get; set; }

            public string level_1 { get; set; }

            public string level_2 { get; set; }
        }

            /// <summary>
            /// 錯誤參數model
            /// </summary>
            public partial class ProcVeTraceModel
        {
            /// <summary>
            ///type
            /// </summary>
            public string type { get; set; }

            /// <summary>
            /// 執行日期
            /// </summary>
            public string exec_date { get; set; }

            /// <summary>
            /// 異動人員
            /// </summary>
            public string upd_id { get; set; }

            /// <summary>
            /// 異動日期
            /// </summary>
            public string upd_date { get; set; }

            /// <summary>
            /// S:成功;F失敗
            /// </summary>
            public string rtnCode { get; set; }


        }


    }



}
