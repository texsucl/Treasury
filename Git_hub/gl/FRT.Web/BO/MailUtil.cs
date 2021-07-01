using FRT.Web.Daos;
using FRT.Web.Models;
using FRT.Web.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.EasycomClient;
using System.Data.SqlClient;
using System.IO;
using System.Net;
using System.Net.Mail;

namespace FRT.Web.BO
{
    public class MailUtil
    {
        static private NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        private Dictionary<string, UserBossModel> qryEmp(string usrId, Dictionary<string, UserBossModel> empMap
            , EacConnection con, EacCommand cmd, OaEmpDao oaEmpDao)
        {
            if (!empMap.ContainsKey(usrId))
            {
                UserBossModel userBossModel = oaEmpDao.getEmpBoss(usrId, con, cmd);
                if (userBossModel != null)
                    empMap.Add(usrId, userBossModel);
                else
                    empMap.Add(usrId, null);
            }

            return empMap;

        }

        public List<UserBossModel> getMailGrpId(string groupId) {
            List<UserBossModel> mailTo = new List<UserBossModel>();
            Dictionary<string, UserBossModel> empMap = new Dictionary<string, UserBossModel>();
            List<MailNotifyModel> otherNotify = new List<MailNotifyModel>();
            FRTMailNotifyDao fRTMailNotify = new FRTMailNotifyDao();
            otherNotify = fRTMailNotify.qryNtyUsr(groupId);

            if (otherNotify.Count == 0)
                return mailTo;


            SysCodeDao sysCodeDao = new SysCodeDao();
            OaEmpDao oaEmpDao = new OaEmpDao();

            EacConnection con = new EacConnection();
            EacCommand cmd = new EacCommand();

            con.ConnectionString = CommonUtil.GetEasycomConn();
            con.Open();
            cmd.Connection = con;

 
            foreach (MailNotifyModel dUser in otherNotify)
            {
                //取得群組人員的MAIL相關資料
                empMap = qryEmp(dUser.receiverEmpno, empMap, con, cmd, oaEmpDao);

                UserBossModel usr = empMap[dUser.receiverEmpno];
                if (usr != null) {
                    //MAIL給直屬主管(MAIL的最高層級為部長)
                    if ("Y".Equals(dUser.isNotifyMgr))
                    {
                        if (!usr.usrId.Equals(usr.usrIdMgr)
                                && (("03".Equals(usr.deptType)) || "04".Equals(usr.deptType)))
                        {
                            empMap = qryEmp(usr.usrIdMgr, empMap, con, cmd, oaEmpDao);
                        }
                    }


                    //MAIL給部主管(MAIL的最高層級為部長)
                    if ("Y".Equals(dUser.isNotifyDeptMgr))
                    {
                        if ("04".Equals(usr.deptType)) //承辦人隸屬單位為"科"
                        {
                            empMap = qryEmp(usr.usrIdDeptMgr, empMap, con, cmd, oaEmpDao);
                        }
                        else if ("03".Equals(usr.deptType)) //承辦人隸屬單位為"部"
                        {
                            empMap = qryEmp(usr.usrIdMgr, empMap, con, cmd, oaEmpDao);
                        }
                    }

                }
            }

            foreach (KeyValuePair<string, UserBossModel> item in empMap)
            {
                if(empMap[item.Key] != null)
                    mailTo.Add(empMap[item.Key]);
            }

            return mailTo;


        }


        public bool sendMailMulti(List<UserBossModel> userBoss, string MailSub, string MailBody, bool isBodyHtml
            , string mailAccount, string mailPwd
            , string[] filePaths, bool deleteFileAttachment
            , bool bWriteLog
            , string logKey)
        {
            logger.Info(MailBody);


            string smtpServer = System.Configuration.ConfigurationManager.AppSettings.Get("smtpServer");
            int smtpPort = Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings.Get("smtpPort"));
            string smtpSender = System.Configuration.ConfigurationManager.AppSettings.Get("smtpSender");
            string devRec = System.Configuration.ConfigurationManager.AppSettings.Get("devRec");

            try
            {
                string mailToList = "";
                //沒給寄信人mail address
                if (string.IsNullOrEmpty(smtpSender))
                {//※有些公司的Smtp Server會規定寄信人的Domain Name須是該Smtp Server的Domain Name，例如底下的 system.com.tw
                    //MailFrom = "sysadmin@system.com.tw";
                }


                //建立MailMessage物件
                MailMessage mms = new MailMessage();
                //指定一位寄信人MailAddress
                mms.From = new MailAddress(smtpSender);
                //信件主旨
                mms.Subject = MailSub;
                //信件內容
                mms.Body = MailBody;
                //信件內容 是否採用Html格式
                mms.IsBodyHtml = isBodyHtml;

                string bDev = System.Configuration.ConfigurationManager.AppSettings.Get("bDev");
                //處理收信人
                if (userBoss != null)
                {
                    if ("Y".Equals(bDev)) //測試機
                                          //if("10.204.241.226".Equals(smtpServer)) //測試機
                    { 
                        mms.To.Add(new MailAddress(devRec));

                        logger.Info(devRec);
                    }

                    foreach (UserBossModel d in userBoss) {
                        try
                        {
                            mms.To.Add(new MailAddress(StringUtil.toString(d.empMail)));
                        }
                        catch (Exception e) {
                            logger.Error(e.ToString);
                        }
                        
                        mailToList = mailToList + StringUtil.toString(d.empMail);

                        logger.Info("empMail:" + d.empMail);
                        logger.Info("usrId:" + d.usrId);
                    }
                }


                //處理附加檔案
                if (filePaths != null)
                {
                    for (int i = 0; i < filePaths.Length; i++)
                    {
                        if (!string.IsNullOrEmpty(filePaths[i].Trim()))
                        {
                            Attachment file = new Attachment(filePaths[i].Trim());
                            mms.Attachments.Add(file);
                        }
                    }
                }



                using (SmtpClient client = new SmtpClient(smtpServer, smtpPort))//或公司、客戶的smtp_server
                {
                    if (!string.IsNullOrEmpty(mailAccount) && !string.IsNullOrEmpty(mailPwd))//.config有帳密的話
                        client.Credentials = new NetworkCredential(mailAccount, mailPwd);//寄信帳密

                    logger.Info(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "寄信至 " + mailToList);

                    client.Send(mms);//寄出一封信

                    if (mms.Attachments != null && mms.Attachments.Count > 0)
                    {
                        for (int i = 0; i < mms.Attachments.Count; i++)
                            mms.Attachments[i].Dispose();
                    }

                    if (deleteFileAttachment && filePaths != null && filePaths.Length > 0)
                    {
                        foreach (string filePath in filePaths)
                            File.Delete(filePath.Trim());
                    }

                    logger.Info(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "寄信至 " + mailToList + "完成");

                    if (bWriteLog)
                        procWriteLogMulti("S", userBoss, MailSub, "", logKey);


                    return true;//成功
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex.ToString());

                if (bWriteLog)
                    procWriteLogMulti("F", userBoss, MailSub, ex.Message + " " + ex.InnerException.Message, logKey);
                return false;
            }

        }


        public bool sendMail(UserBossModel userBoss, string MailSub, string MailBody, bool isBodyHtml
            , string mailAccount, string mailPwd
            , string[] filePaths, bool deleteFileAttachment
            , bool bMailMgr
            , bool bMailDeptMgr
            , bool bWriteLog
            , string logKey)
        {

            logger.Info(MailBody);


            string smtpServer = System.Configuration.ConfigurationManager.AppSettings.Get("smtpServer");
            int smtpPort = Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings.Get("smtpPort"));
            string smtpSender = System.Configuration.ConfigurationManager.AppSettings.Get("smtpSender");
            string devRec = System.Configuration.ConfigurationManager.AppSettings.Get("devRec");

            try
            {
                string mailToList = "";
                //沒給寄信人mail address
                if (string.IsNullOrEmpty(smtpSender))
                {//※有些公司的Smtp Server會規定寄信人的Domain Name須是該Smtp Server的Domain Name，例如底下的 system.com.tw
                    //MailFrom = "sysadmin@system.com.tw";
                }


                //建立MailMessage物件
                MailMessage mms = new MailMessage();
                //指定一位寄信人MailAddress
                mms.From = new MailAddress(smtpSender);
                //信件主旨
                mms.Subject = MailSub;
                //信件內容
                mms.Body = MailBody;
                //信件內容 是否採用Html格式
                mms.IsBodyHtml = isBodyHtml;

                string bDev = System.Configuration.ConfigurationManager.AppSettings.Get("bDev");
                //處理收信人
                if (userBoss != null)
                {
                    if ("Y".Equals(bDev)) //測試機
                                          //if("10.204.241.226".Equals(smtpServer)) //測試機
                    {
                        mms.To.Add(new MailAddress(devRec));

                        logger.Info(devRec);
                    }

                    try
                    {
                        mms.To.Add(new MailAddress(StringUtil.toString(userBoss.empMail)));
                    }
                    catch (Exception e) {
                        logger.Error(e.ToString);
                    }
                    mailToList = mailToList + StringUtil.toString(userBoss.empMail);


                    logger.Info("empMail:" + userBoss.empMail);
                    logger.Info("usrId:" + userBoss.usrId);

                    //MAIL給直屬主管(MAIL的最高層級為部長)
                    if (bMailMgr)
                    {
                        if (!userBoss.usrId.Equals(userBoss.usrIdMgr)
                                && (("03".Equals(userBoss.deptType)) || "04".Equals(userBoss.deptType)))
                        {
                            try
                            {
                                mms.To.Add(new MailAddress(StringUtil.toString(userBoss.empMailMgr)));
                            }
                            catch (Exception e) {
                                logger.Error(e.ToString);
                            }
                            
                            mailToList = mailToList + StringUtil.toString(userBoss.empMailMgr);

                            logger.Info("usrIdBoss/empMailBoss:" + userBoss.usrIdMgr + "/" + userBoss.empMailMgr);
                        }  
                    }

                    //MAIL給部主管(MAIL的最高層級為部長)
                    if (bMailDeptMgr)
                    {
                        if ("04".Equals(userBoss.deptType)) //承辦人隸屬單位為"科"
                        {
                            try
                            {
                                mms.To.Add(new MailAddress(StringUtil.toString(userBoss.empMailDeptMgr)));
                            }
                            catch (Exception e) {
                                logger.Error(e.ToString);
                            }
                            
                            mailToList = mailToList + StringUtil.toString(userBoss.empMailDeptMgr);

                            logger.Info("usrIdBoss/empMailBoss:" + userBoss.usrIdDeptMgr + "/" + userBoss.empMailDeptMgr);

                        }
                        else if ("03".Equals(userBoss.deptType)) //承辦人隸屬單位為"部"
                        {
                            try
                            {
                                mms.To.Add(new MailAddress(StringUtil.toString(userBoss.empMailMgr)));
                            }
                            catch (Exception e) {
                                logger.Error(e.ToString);
                            }
                            
                            mailToList = mailToList + StringUtil.toString(userBoss.empMailMgr);

                            logger.Info("usrIdBoss/empMailBoss:" + userBoss.usrIdMgr + "/" + userBoss.empMailMgr);
                        }
                    }
                }


                //處理附加檔案
                if (filePaths != null)
                {
                    for (int i = 0; i < filePaths.Length; i++)
                    {
                        if (!string.IsNullOrEmpty(filePaths[i].Trim()))
                        {
                            Attachment file = new Attachment(filePaths[i].Trim());
                            mms.Attachments.Add(file);
                        }
                    }
                }



                using (SmtpClient client = new SmtpClient(smtpServer, smtpPort))//或公司、客戶的smtp_server
                {
                    if (!string.IsNullOrEmpty(mailAccount) && !string.IsNullOrEmpty(mailPwd))//.config有帳密的話
                        client.Credentials = new NetworkCredential(mailAccount, mailPwd);//寄信帳密

                    logger.Info(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "寄信至 " + mailToList);

                    client.Send(mms);//寄出一封信

                    if (mms.Attachments != null && mms.Attachments.Count > 0)
                    {
                        for (int i = 0; i < mms.Attachments.Count; i++)
                            mms.Attachments[i].Dispose();
                    }

                    if (deleteFileAttachment && filePaths != null && filePaths.Length > 0)
                    {
                        foreach (string filePath in filePaths)
                            File.Delete(filePath.Trim());
                    }

                    logger.Info(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "寄信至 " + mailToList + "完成");

                    if (bWriteLog)
                        procWriteLog("S", userBoss, MailSub, bMailMgr, bMailDeptMgr, "", logKey);


                    return true;//成功
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex.ToString());

                if (bWriteLog)
                    procWriteLog("F", userBoss, MailSub, bMailMgr, bMailDeptMgr, ex.Message + " " + ex.InnerException.Message, logKey);
                return false;
            }
        }


        /// <summary>
        /// 紀錄MAIL結果
        /// </summary>
        /// <param name="mailResult"></param>
        /// <param name="userBoss"></param>
        /// <param name="MailSub"></param>
        /// <param name="resultDesc"></param>
        /// <param name="logKey"></param>
        private void procWriteLog(string mailResult, UserBossModel userBoss, string mailSub, bool bMailMgr, bool bMailDeptMgr
            , string resultDesc, string logKey) {
            try
            {
                DateTime dt = DateTime.Now;
                string [] strDT = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss").Split(' ');

                FRTMailLogDao fRTMailLogDao = new FRTMailLogDao();
                
                string strConn = DbUtil.GetDBFglConnStr();
                using (SqlConnection conn = new SqlConnection(strConn))
                {
                    conn.Open();
                    //SqlTransaction transaction = conn.BeginTransaction("Transaction");

                    try
                    {
                        FRT_MAIL_LOG frtMailLog = new FRT_MAIL_LOG();

                        //取得流水號
                        SysSeqDao sysSeqDao = new SysSeqDao();
                        var cId = sysSeqDao.qrySeqNo("RT", "mail", strDT[0].Replace("-", "")).ToString();
                        frtMailLog.SEQ = Convert.ToInt64(strDT[0].Replace("-", "") + cId.ToString().PadLeft(5, '0'));
                        //frtMailLog.MAIL_DATE = dt.Date;
                        //frtMailLog.MAIL_TIME = new TimeSpan(dt.Ticks);
                        frtMailLog.RECEIVER_EMPNO = userBoss.usrId;
                        frtMailLog.EMAIL = userBoss.empMail;
                        frtMailLog.MAIL_RESULT = mailResult;
                        frtMailLog.RESULT_DESC = logKey + ":" + resultDesc;
                        frtMailLog.MAIL_SUB = mailSub;

                        fRTMailLogDao.Insert(frtMailLog, strDT, conn);

                        //MAIL給直屬主管(MAIL的最高層級為部長)
                        if (bMailMgr) {
                            if (!userBoss.usrId.Equals(userBoss.usrIdMgr)
                                && (("03".Equals(userBoss.deptType)) || "04".Equals(userBoss.deptType)))
                            {
                                cId = sysSeqDao.qrySeqNo("RT", "mail", strDT[0].Replace("-", "")).ToString();
                                frtMailLog.SEQ = Convert.ToInt64(strDT[0].Replace("-", "") + cId.ToString().PadLeft(5, '0'));

                                frtMailLog.RECEIVER_EMPNO = userBoss.usrIdMgr;
                                frtMailLog.EMAIL = userBoss.empMailMgr;
                                doInsertMailLog(fRTMailLogDao, frtMailLog, strDT, conn);
                            }
                        }

                        //MAIL給部主管(MAIL的最高層級為部長)
                        if (bMailDeptMgr) {
                            if ("04".Equals(userBoss.deptType)) //承辦人隸屬單位為"科"
                            {
                                cId = sysSeqDao.qrySeqNo("RT", "mail", strDT[0].Replace("-", "")).ToString();
                                frtMailLog.SEQ = Convert.ToInt64(strDT[0].Replace("-", "") + cId.ToString().PadLeft(5, '0'));

                                frtMailLog.RECEIVER_EMPNO = userBoss.usrIdDeptMgr;
                                frtMailLog.EMAIL = userBoss.empMailDeptMgr;
                                doInsertMailLog(fRTMailLogDao, frtMailLog, strDT, conn);

                            }
                            else if ("03".Equals(userBoss.deptType)) //承辦人隸屬單位為"部"
                            { 
                                cId = sysSeqDao.qrySeqNo("RT", "mail", strDT[0].Replace("-", "")).ToString();
                                frtMailLog.SEQ = Convert.ToInt64(strDT[0].Replace("-", "") + cId.ToString().PadLeft(5, '0'));

                                frtMailLog.RECEIVER_EMPNO = userBoss.usrIdMgr;
                                frtMailLog.EMAIL = userBoss.empMailMgr;
                                doInsertMailLog(fRTMailLogDao, frtMailLog, strDT, conn);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        logger.Error(userBoss.empMail + ":" + e.ToString());
                    }
                }

            }
            catch (Exception e) {
                logger.Error(e.ToString());
            }
        }

        private void procWriteLogMulti(string mailResult, List<UserBossModel> userBoss, string mailSub
            , string resultDesc, string logKey)
        {
            try
            {
                DateTime dt = DateTime.Now;
                string[] strDT = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss").Split(' ');

                FRTMailLogDao fRTMailLogDao = new FRTMailLogDao();

                string strConn = DbUtil.GetDBFglConnStr();
                using (SqlConnection conn = new SqlConnection(strConn))
                {
                    conn.Open();
                    //SqlTransaction transaction = conn.BeginTransaction("Transaction");

                    foreach (UserBossModel d in userBoss) {
                        try
                        {
                            FRT_MAIL_LOG frtMailLog = new FRT_MAIL_LOG();

                            //取得流水號
                            SysSeqDao sysSeqDao = new SysSeqDao();
                            var cId = sysSeqDao.qrySeqNo("RT", "mail", strDT[0].Replace("-", "")).ToString();
                            frtMailLog.SEQ = Convert.ToInt64(strDT[0].Replace("-", "") + cId.ToString().PadLeft(5, '0'));
                            //frtMailLog.MAIL_DATE = dt.Date;
                            //frtMailLog.MAIL_TIME = new TimeSpan(dt.Ticks);
                            frtMailLog.RECEIVER_EMPNO = d.usrId;
                            frtMailLog.EMAIL = d.empMail;
                            frtMailLog.MAIL_RESULT = mailResult;
                            frtMailLog.RESULT_DESC = logKey + ":" + resultDesc;
                            frtMailLog.MAIL_SUB = mailSub;

                            doInsertMailLog(fRTMailLogDao, frtMailLog, strDT, conn);
                        }
                        catch (Exception e) {
                            logger.Error(e.ToString());

                        }

                    }
                }

            }
            catch (Exception e)
            {
                logger.Error(e.ToString());
            }
        }



        private void doInsertMailLog(FRTMailLogDao fRTMailLogDao, FRT_MAIL_LOG frtMailLog, string[] strDT, SqlConnection conn) {
            bool doInsert = true;
            int tryCnt = 0;

            while (doInsert) {

                if (tryCnt > 3)
                    doInsert = false;


                tryCnt++;


                try
                {
                    fRTMailLogDao.Insert(frtMailLog, strDT, conn);
                    doInsert = false;
                }
                catch (SqlException e)
                {
                    if (e.Number == 2627) {
                        SysSeqDao sysSeqDao = new SysSeqDao();
                        var cId = sysSeqDao.qrySeqNo("RT", "mail", strDT[0].Replace("-", "")).ToString();
                        frtMailLog.SEQ = Convert.ToInt64(strDT[0].Replace("-", "") + cId.ToString().PadLeft(5, '0'));
                    } else
                        doInsert = false;

                }
                catch (Exception e)
                {
                    doInsert = false;
                    throw e;

                }
            }
            
            

        }

    }
}