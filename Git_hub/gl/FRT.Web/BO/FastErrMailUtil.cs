using FRT.Web.AS400Models;
using FRT.Web.Daos;
using FRT.Web.Models;
using FRT.Web.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.EasycomClient;
using System.Linq;

/// <summary>
/// 功能說明：寄送"匯款失敗通知報表"MAIL
/// 初版作者：20180807 Daiyu
/// 修改歷程：20180807 Daiyu
///           需求單號：
///           初版
/// 修改歷程：20181212 Daiyu
///           需求單號：201811300566-02
///           1.調整MAIL內容的銀行通知時間
/// ==============================================
/// 修改日期/修改人：20190516 daiyu
/// 需求單號：
/// 修改內容：配合金檢議題，稽核軌跡多加寫HOSTNAME
/// ==============================================
/// </summary>

namespace FRT.Web.BO
{
    public class FastErrMailUtil
    {
        static private NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();



        public Dictionary<string, string> procSendMail(string type, List<FRTBARMModel> recData, string textType,string status = null)
        {
            //List<string> errList = new List<string>();

            Dictionary<string, FRTBERMModel> errMap = new Dictionary<string, FRTBERMModel>();
            Dictionary<string, UserBossModel> empMap = new Dictionary<string, UserBossModel>();
            Dictionary<string, string> failMap = new Dictionary<string, string>();
            List<MailNotifyModel> otherNotify = new List<MailNotifyModel>();

            //FRTBERMDao fRTBERMDao = new FRTBERMDao();
            OaEmpDao oaEmpDao = new OaEmpDao();

            FPMCODEDao fPMCODEDao = new FPMCODEDao();

            EacConnection con = new EacConnection();
            EacCommand cmd = new EacCommand();
            MailUtil mailUtil = new MailUtil();

            List<FPMCODEModel> fPMCODEModel = fPMCODEDao.qryFPMCODE("FAIL-CODE", "RT", "");


            ////匯款失敗原因
            //List<FPMCODEModel> failCode = fPMCODEDao.qryFPMCODE("FAIL-CODE", "RT", "");
            con.ConnectionString = CommonUtil.GetEasycomConn();
            con.Open();
            cmd.Connection = con;

            foreach (FRTBARMModel d in recData)
            {
                string errBelong = "";
                try
                {
                    if ("M".Equals(type))   //人工修改匯款失敗原因作業
                    {
                        errBelong = "1";
                    }
                    else {  //電文回call
                        //判斷錯誤歸屬
                        if ("#".Equals(d.failCode))
                            errBelong = "2";
                        else
                            errBelong = d.errBelong;

                    }

                    logger.Info("errBelong:" + errBelong);



                    //依錯誤歸屬寄送MAIL
                    if ("1".Equals(errBelong))  //錯誤歸屬=1客戶:寄給FRTBARM0.ENTRY_ID及其主管
                    {
                        otherNotify.Clear();

                        try {

                            try
                            {
                                d.errCode = d.failCode + "：" + fPMCODEModel.Where(x => x.refNo == d.failCode).FirstOrDefault().text.ToString();
                            }
                            catch (Exception e) {
                                logger.Error(e.ToString());
                            }

                            //logger.Info("d.errCode:" + d.errCode);
                            //modify by daiyu 20181212 查不到對應人員的，改寄特定群組
                            if (!empMap.ContainsKey(d.entryId))
                            {
                                UserBossModel userBossModel = oaEmpDao.getEmpBoss(d.entryId, con, cmd);
                                empMap.Add(d.entryId, userBossModel);
                            }

                            UserBossModel userBoss = empMap[d.entryId];

                            if (userBoss != null)
                            {
                                List<UserBossModel> notify = new List<UserBossModel>();
                                notify.Add(userBoss);
                                bool bSendSucess = genMail(false, notify, d, true, false, type, textType, errBelong, status);
                                if (!bSendSucess)
                                {
                                    if (!failMap.ContainsKey(d.fastNo)) {
                                        failMap.Add(d.fastNo, "寄送MAIL失敗");
                                        continue;
                                    }
                                }
                            } else {
             
                                SysCodeDao sysCodeDao = new SysCodeDao();
                                string[] mailGrp = new string[] { "REMIT_ERR_BENE", "REMIT_ERR_CL" };
                                SYS_CODE sysCode = new SYS_CODE();

                                if ("A".Equals(d.sysType))
                                {
                                    sysCode = sysCodeDao.qryByReserve("RT", "MAIL_GROUP", mailGrp, d.srceKind, "", "");
                                }
                                else
                                {
                                    sysCode = sysCodeDao.qryByReserve("RT", "MAIL_GROUP", mailGrp, "", d.srceFrom, "");
                                }

                                if ("".Equals(StringUtil.toString(sysCode.CODE)))
                                {
                                    failMap.Add(d.fastNo, "查無建立人員及MAIL群組資訊");
                                    continue;
                                }
                                else
                                {
                                    FRTMailNotifyDao fRTMailNotify = new FRTMailNotifyDao();
                                    List<UserBossModel> notify = mailUtil.getMailGrpId(sysCode.CODE);

                                    if (notify.Count == 0)
                                    {
                                        failMap.Add(d.fastNo, "查無建立人員及MAIL群組資訊");
                                        continue;
                                    }
                                    else {
                                        bool bSendSucess = genMail(true, notify, d, true, false, type, textType, errBelong, status);
                                        if (!bSendSucess)
                                        {
                                            if (!failMap.ContainsKey(d.fastNo))
                                                failMap.Add(d.fastNo, "寄送MAIL失敗");
                                            continue;
                                        }
                                    }
                                }
                            }
                            
                        } catch (Exception e) {
                            logger.Error(e.ToString());
                        }
                        

                    }
                    else
                    {  //錯誤歸屬=2人壽、3銀行:寄給財務部對應窗口

                        //d.errCode += errMap[d.failCode].errDesc;
                        d.errCode = d.errCode + "：" + d.errDesc;


                        FRTMailNotifyDao fRTMailNotify = new FRTMailNotifyDao();
                        List<UserBossModel> notify = mailUtil.getMailGrpId("REMIT_ERR");

                        bool bSendSucess = genMail(true, notify, d, true, false, type, textType, errBelong, status);

                        if (!bSendSucess)
                        {
                            if (!failMap.ContainsKey(d.fastNo))
                                failMap.Add(d.fastNo, "寄送MAIL失敗");
                            continue;
                        }
                    }

                }
                catch (Exception e)
                {
                    logger.Error(e.ToString());
                    

                }
            }
            cmd.Dispose();
            cmd = null;
            con.Close();
            con = null;

            return failMap;
        }




        /// <summary>
        /// 組合MAIL內容資訊
        /// </summary>
        /// <param name="mailTos"></param>
        /// <param name="d"></param>
        /// <returns></returns>
        private bool genMail(bool bMailGrp, List<UserBossModel> userBoss, FRTBARMModel d, bool bMailMgr, bool bMailDeptMgr, string type, string textType, string errBelong, string status = null)
        {
            logger.Info("genMail begin!!");
            bool bSucess = true;
            MailUtil mailUtil = new MailUtil();

            string textRcvdt = StringUtil.toString(d.textRcvdt) == "0" ? "" : d.textRcvdt.PadLeft(8, ' ');
            string textRcvtm = StringUtil.toString(d.textRcvtm) == "0" ? "" : d.textRcvtm.PadLeft(8, ' ');
            string bankAct = StringUtil.toString(d.bankAct);

            string rmtapx = "";

            //modify by daiyu 20181207
            if ("S".Equals(type))
            {
                logger.Info("query 522657 begin");
                FRTXml522657Dao fRTXmlR522657Dao = new FRTXml522657Dao();
                rmtapx = fRTXmlR522657Dao.qryRmtapx(d.fastNo);

                //銀行電文)非622685啟動的失敗通知:維持帶FRTBARM的電文回饋日期時間
                //(銀行電文)由622685啟動的失敗通知:銀行通知日期時間改帶622685的回饋日期時間
                if ("622685".Equals(textType)) {
                    FRTXmlR622685Dao fRTXmlR622685Dao = new FRTXmlR622685Dao();
                    var r622685Data = fRTXmlR622685Dao.qryLstByFastNo(d.fastNo);
                    if (r622685Data.Item3 != 3) {
                        var rcv = r622685Data.Item3 == 1 ? (DateUtil.DatetimeToString((DateTime)(r622685Data.Item1.UPD_TIME == null ? r622685Data.Item1.CRT_TIME : r622685Data.Item1.UPD_TIME),"").Split(' ')) :
                             (DateUtil.DatetimeToString((DateTime)(r622685Data.Item2.UPD_TIME == null ? r622685Data.Item2.CRT_TIME : r622685Data.Item2.UPD_TIME), "").Split(' '));
                        textRcvdt = DateUtil.ADDateToChtDate(rcv[0]);
                        textRcvtm = rcv[1].Replace(":", "");
                    }
                }
            }
            else {
                //經由ORTB006人工修改匯款失敗原因作業:修改為正確的錯誤代碼後，通知前端承辦人時的MAIL，帶ORTB006A覆核時的系統日期時間
                var rcv = DateUtil.getCurChtDateTime(4).Split(' ');
                textRcvdt = rcv[0];
                textRcvtm = rcv[1];

            }
            //end modify 20181207 

            logger.Info("query 522657 end");


            var _status = status?.Trim() ?? string.Empty;

            switch (_status)
            {
                case "0":
                    _status += " (處理中)";
                    break;
                case "1":
                    _status += " (匯出異常)";
                    break;
                case "2":
                    _status += " (匯出被退匯)";
                    break;
                case "3":
                    _status += " (人工退還客戶)";
                    break;
                case "4":
                    _status += " (已匯出)";
                    break;
                case "5":
                    _status += " (人工重匯)";
                    break;
                case "6":
                    _status += " (無資料)";
                    break;
                case "8":
                    _status += " (待OP處理)";
                    break;
                case "9":
                    _status += " (被退匯未處理)";                    
                    break;
                default:
                    break;
            }

            string mailContent = "";
            mailContent = "親愛的同仁您好：<br/>" +
                "   您有快速付款匯款失敗案件，請協助追蹤與查詢，並請儘速完成重新付款。<br/><br/>" +
                "   系統別 = " + d.sysType + "<br/>" +
                "   資料來源 = " + d.srceFrom + "<br/>" +
                "   資料類別 = " + d.srceKind + "<br/>" +
                "   保單號碼 = " + MaskUtil.maskPolicyNo(d.policyNo) + "-" + d.policySeq + "-" + d.idDup + "<br/>" +
                "   人員別 = " + d.memberId + "<br/>" +
                "   案號 = " + d.changeId + " " + d.changeSeq + "<br/>" +
                "   快速付款編號 = " + d.fastNo + "<br/>" +
                "   銀行代號 = " + d.bankCode + d.subBank + "<br/>" +
                "   匯款帳號 = " + MaskUtil.maskBankAct(bankAct) + "<br/>" +
                "   幣別 = " + d.currency + "<br/>" +
                "   匯款金額 = " + String.Format("{0:N0}", d.remitAmt) + "<br/>" +
                "   戶名 = " + MaskUtil.maskName(d.rcvName) + "<br/>" +
                "   銀行通知日期 = " + DateUtil.formatDateTimeDbToSc(textRcvdt, "D") + "<br/>" +
                "   銀行通知時間 = " + (textRcvtm == "" ? "" : DateUtil.formatDateTimeDbToSc(textRcvtm.Substring(0, 6), "T")) + "<br/>" +
                "   匯款失敗原因 = " + d.errCode + "<br/>" +
                "   附言 = " + rmtapx + 
                ( errBelong != "1" ? ("<br/>" + "   STATUS = " + _status) : string.Empty); //只有寄送對象為財務部經辦時才秀此行

            if (bMailGrp)
            {
                bSucess = mailUtil.sendMailMulti(userBoss
               , "快速付款匯款失敗通知表"
               , mailContent
               , true
              , ""
              , ""
              , null
              , true
              , true
              , d.fastNo);
            }
            else {
                bSucess = mailUtil.sendMail(userBoss[0]
               , "快速付款匯款失敗通知表"
               , mailContent
               , true
              , ""
              , ""
              , null
              , true
              , bMailMgr, bMailDeptMgr
              , true
              , d.fastNo);
            }

            /*---add by daiyu 20190516---*/
            PIA_LOG_MAIN piaLogMain = new PIA_LOG_MAIN();
            piaLogMain.TRACKING_TYPE = "A";
            piaLogMain.ACCESS_ACCOUNT = userBoss[0].empNo;
            piaLogMain.ACCOUNT_NAME = userBoss[0].empName;
            piaLogMain.PROGFUN_NAME = "FastErrMail";
            piaLogMain.EXECUTION_CONTENT = "fast_no:" + d.fastNo;
            piaLogMain.AFFECT_ROWS = 1;
            piaLogMain.PIA_TYPE = "1000100000";
            piaLogMain.EXECUTION_TYPE = "Q";
            piaLogMain.ACCESSOBJ_NAME = "FRTBARM";
            PiaLogMainDao piaLogMainDao = new PiaLogMainDao();
            piaLogMainDao.Insert(piaLogMain);
            /*---end add 20190516---*/


            return bSucess;

        }
    }
}