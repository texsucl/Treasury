using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web;

namespace Treasury.WebBO
{
    public class MailUtil
    {
        static private NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public bool sendMail(string MailFrom, string[] MailTos, string MailSub, string MailBody, bool isBodyHtml
            , string smtpServer, int smtpPort, string mailAccount, string mailPwd
            , string[] filePaths, bool deleteFileAttachment) {
            try
            {
                string mailToList = "";
                //沒給寄信人mail address
                if (string.IsNullOrEmpty(MailFrom))
                {//※有些公司的Smtp Server會規定寄信人的Domain Name須是該Smtp Server的Domain Name，例如底下的 system.com.tw
                    //MailFrom = "sysadmin@system.com.tw";
                }

               
                //建立MailMessage物件
                MailMessage mms = new MailMessage();
                //指定一位寄信人MailAddress
                mms.From = new MailAddress(MailFrom);
                //信件主旨
                mms.Subject = MailSub;
                //信件內容
                mms.Body = MailBody;
                //信件內容 是否採用Html格式
                mms.IsBodyHtml = isBodyHtml;


                //處理收信人
                if (MailTos != null)
                {
                    for (int i = 0; i < MailTos.Length; i++)
                    {
                        if (!string.IsNullOrEmpty(MailTos[i].Trim()))
                        {

                            if("10.204.241.226".Equals(smtpServer)) //測試機
                                mms.To.Add(new MailAddress("glsisys.life@fbt.com"));
                            else
                                mms.To.Add(new MailAddress(MailTos[i].Trim()));

                            mailToList = mailToList + MailTos[i].Trim();
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

                    return true;//成功
                }


        }
            catch (Exception ex)
            {
                logger.Error(ex.ToString());
                throw ex;
            }
}
    }
}