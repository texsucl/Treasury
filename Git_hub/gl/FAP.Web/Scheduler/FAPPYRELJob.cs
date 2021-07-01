using FAP.Web.BO;
using FAP.Web.Models;
using FAP.Web.Utilitys;
using Quartz;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data.Common;
using System.Data.EasycomClient;
using System.Linq;
using System.Text;
using System.Web;


namespace FAP.WebScheduler
{
    public class FAPPYRELJob : IJob
    {
        static private NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        public void Execute(IJobExecutionContext context)
        {
            logger.Info("[Execute]執行開始!!");
            logger.Info("SendMail_FAPPYREL0:" + DateTime.Now.ToString("HH:mm"));
            SendMail();
        }

        public void SendMail()
        {
            List<FAPPYREL0> mailDatas = new List<FAPPYREL0>();
            try
            {
                List<FAPPYREL0> datas = new List<FAPPYREL0>();
                using (EacConnection conn = new EacConnection(CommonUtil.GetEasycomConn()))
                {
                    conn.Open();
                    using (EacCommand com = new EacCommand(conn))
                    {
                        string sql = $@"
                        select  APLY_NO, APLY_SEQ, SRCE_FROM, NEW_CK, OLD_CK, AMOUNT_N , AMOUNT_O, UPD_ID , UPD_DATE   from FAPPYREL0 
                        where EMAIL_FLAG <> 'Y' ; ";
                        com.CommandText = sql;
                        com.Prepare();
                        DbDataReader dbDate = com.ExecuteReader();
                        while (dbDate.Read())
                        {
                            datas.Add(new FAPPYREL0()
                            {
                                APLY_NO = dbDate["APLY_NO"]?.ToString()?.Trim(),
                                APLY_SEQ = dbDate["APLY_SEQ"]?.ToString()?.Trim(),
                                SRCE_FROM = dbDate["SRCE_FROM"]?.ToString()?.Trim(),
                                NEW_CK = dbDate["NEW_CK"]?.ToString()?.Trim(),
                                OLD_CK = dbDate["OLD_CK"]?.ToString()?.Trim(),
                                AMOUNT_N = dbDate["AMOUNT_N"]?.ToString()?.Trim(),
                                AMOUNT_O = dbDate["AMOUNT_O"]?.ToString()?.Trim(),
                                UPD_ID = dbDate["UPD_ID"]?.ToString()?.Trim(),
                                UPD_DATE = dbDate["UPD_DATE"]?.ToString()?.Trim()
                            });
                        }
                        com.Dispose();
                    }
                    conn.Dispose();
                    conn.Close();
                }
                if (datas.Any())
                {
                    foreach (var item in datas)
                    {
                        var remindList = new FAP.Web.Service.Actual.Common().GetMemoByUserId(new List<string>() { item.UPD_ID });
                        var _UATmailAccount = ConfigurationManager.AppSettings["UATmailAccount"] ?? string.Empty;
                        List<Tuple<string, string>> _mailTo = new List<Tuple<string, string>>() { };
                        if (_UATmailAccount == "Y")
                        {
                            _mailTo.Add(new Tuple<string, string>("Ex2016ap@fbt.com", "測試帳號-Ex2016ap"));
                        }
                        _mailTo.AddRange(remindList.Select(x => new Tuple<string, string>(x.Item5, x.Item2)));
                        var sms = new SendMail.SendMailSelf();
                        sms.smtpPort = 25;
                        sms.smtpServer = ConfigurationManager.AppSettings["smtpServer"];
                        sms.mailAccount = ConfigurationManager.AppSettings["mailAccount"];
                        sms.mailPwd = ConfigurationManager.AppSettings["mailPwd"];
                        var msg = sms.Mail_Send(
                           new Tuple<string, string>(sms.mailAccount, "應付票據 MAIL 通知"),
                           _mailTo,
                           null,
                           "B06 應付票據比對不符",
                           $@"APLY_NO：{item.APLY_NO}，APLY_SEQ:{item.APLY_SEQ}，比對不符。",
                           false,
                           null
                           );
                        if (string.IsNullOrWhiteSpace(msg))
                        {
                            mailDatas.Add(item);
                        }
                        else
                        {
                            logger.Error($@"{msg}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error($@"{ex.ToString()}");
            }
            finally {
                if (mailDatas.Any())
                {
                    try
                    {
                        using (EacConnection conn = new EacConnection(CommonUtil.GetEasycomConn()))
                        {
                            conn.Open();
                            using (EacCommand com = new EacCommand(conn))
                            {
                                string sql = $@"
                            update FAPPYREL0
                            set EMAIL_FLAG = 'Y'  
                            where 1 = 1 and ( ";
                                var i = 0;
                                var c = string.Empty;
                                foreach (var item in mailDatas)
                                {
                                    sql += $@" {c} ( APLY_NO = :APLY_NO_{i} and APLY_SEQ = :APLY_SEQ_{i} ) ";
                                    com.Parameters.Add($@"APLY_NO_{i}", item.APLY_NO.strto400DB());
                                    com.Parameters.Add($@"APLY_SEQ_{i}", item.APLY_SEQ.strto400DB());
                                    c = "OR";
                                    i += 1;
                                }
                                sql += " ) ;";
                                com.CommandText = sql;
                                com.Prepare();
                                var updateNum = com.ExecuteNonQuery();
                                com.Dispose();
                            }
                            conn.Dispose();
                            conn.Close();
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.Error($@"{ex.ToString()}");
                    }
                }
            }
        }

        public class FAPPYREL0
        {
            public string APLY_NO { get; set; }
            public string APLY_SEQ { get; set; }
            public string SRCE_FROM { get; set; }
            public string NEW_CK { get; set; }
            public string OLD_CK { get; set; }
            public string AMOUNT_N { get; set; }
            public string AMOUNT_O { get; set; }
            public string UPD_ID { get; set; }
            public string UPD_DATE { get; set; }
        }
    }
}